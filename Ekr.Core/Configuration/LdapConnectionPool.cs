using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Net;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Ekr.Core.Configuration;
using System.IO;

public class LdapConnectionPool : IAsyncDisposable
{
    private readonly Channel<LdapConnection> _channel;
    private readonly TimeSpan _idleTimeout;
    private readonly TimeSpan _connectionTimeout;
    private readonly TimeSpan _PoolTimeOut;
    private readonly Timer _evictionTimer;
    private readonly string _host;
    private readonly int _port;
    private readonly int _maxSize;

    // Tambahkan flag InUse
    private readonly List<(LdapConnection Conn, DateTime LastUsed, bool InUse)> _allConnections = new();

    public LdapConnectionPool(IOptions<LDAPConfig> config)
    {
        var ldapConfig = config.Value;

        _host = new Uri(ldapConfig.Url).Host;
        _port = ldapConfig.Port;
        _maxSize = ldapConfig.MaxPoolSize > 0 ? ldapConfig.MaxPoolSize : 10;
        _connectionTimeout = TimeSpan.FromSeconds(ldapConfig.ConnTimeOut);
        _PoolTimeOut = TimeSpan.FromSeconds(ldapConfig.PoolTimeOut);
        _idleTimeout = TimeSpan.FromSeconds(ldapConfig.IdleTimeout);

        _channel = Channel.CreateBounded<LdapConnection>(new BoundedChannelOptions(_maxSize)
        {
            SingleWriter = false,
            SingleReader = false
        });

        _evictionTimer = new Timer(EvictIdleConnections, null, _idleTimeout, _idleTimeout);
    }

    public async Task<LdapConnection?> RentConnectionAsync(TimeSpan? timeout = null)
    {
        if (_channel.Reader.TryRead(out var conn))
        {
            if (IsConnectionAlive(conn))
            {
                UpdateLastUsed(conn);
                MarkInUse(conn, true);
                Console.WriteLine($"[LDAP POOL] Reuse connection at {DateTime.UtcNow}");
                LogPoolMetrics("Rent Success (reuse)");
                return conn;
            }
            else
            {
                Console.WriteLine("[LDAP POOL] Disposing dead connection.");
                LogPoolMetrics("Rent - disposed");
                conn.Dispose();
                RemoveFromAll(conn);
                return null;
            }
        }

        lock (_allConnections)
        {
            if (_allConnections.Count < _maxSize)
            {
                var connection = CreateConnection();
                if (connection != null && IsConnectionAlive(connection))
                {
                    _allConnections.Add((connection, DateTime.UtcNow, true)); // langsung tandai in use
                    Console.WriteLine($"[LDAP POOL] Created new connection at {DateTime.UtcNow}");
                    LogPoolMetrics("Rent success (new)");
                    return connection;
                }
                else
                {
                    connection?.Dispose();
                    return null;
                }
            }
        }

        try
        {
            timeout ??= _PoolTimeOut;
            Console.WriteLine($"[LDAP POOL] Waiting for connection (Timeout: {timeout?.TotalSeconds ?? 10}s)...");
            using var cts = new CancellationTokenSource(timeout.Value);
            var rentedConn = await _channel.Reader.ReadAsync(cts.Token);
            if (IsConnectionAlive(rentedConn))
            {
                UpdateLastUsed(rentedConn);
                MarkInUse(rentedConn, true);
                return rentedConn;
            }
            else
            {
                rentedConn.Dispose();
                RemoveFromAll(rentedConn);
                return null;
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("[LDAP POOL] Timed out waiting for available connection.");
            return null;
        }
    }

    public void ReturnConnection(LdapConnection conn)
    {
        if (!IsConnectionAlive(conn))
        {
            Console.WriteLine("[LDAP POOL] Disposing broken connection on return.");
            LogPoolMetrics("Return - disposed");
            conn.Dispose();
            RemoveFromAll(conn);
            return;
        }

        UpdateLastUsed(conn);
        MarkInUse(conn, false);

        if (!_channel.Writer.TryWrite(conn))
        {
            Console.WriteLine("[LDAP POOL] Channel full, disposing connection.");
            LogPoolMetrics("Return - channel full");
            conn.Dispose();
            RemoveFromAll(conn);
        }
        else
        {
            Console.WriteLine($"[LDAP POOL] Connection returned at {DateTime.UtcNow}");
            LogPoolMetrics("Return success");
        }
    }

    private LdapConnection CreateConnection()
    {
        var identifier = new LdapDirectoryIdentifier(_host, _port);
        var connection = new LdapConnection(identifier)
        {
            Timeout = _connectionTimeout,
            AuthType = AuthType.Basic
        };

        string ldapAdminPassword = Environment.GetEnvironmentVariable("LDAP_AdminPassword");
        string adminDn = Environment.GetEnvironmentVariable("LDAP_AdminDn");

        if (string.IsNullOrEmpty(ldapAdminPassword) || string.IsNullOrEmpty(adminDn))
        {
            return null;
        }

        connection.SessionOptions.ProtocolVersion = 3;

        try
        {
            connection.Bind(new NetworkCredential(adminDn, ldapAdminPassword));
            LogPoolMetrics("CreateConnection success");
        }
        catch (LdapException ex)
        {
            LogPoolMetrics($"CreateConnection fail - {ex.Message}");
            connection.Dispose();
            return null;
        }

        return connection;
    }

    private void EvictIdleConnections(object state)
    {
        lock (_allConnections)
        {
            var now = DateTime.UtcNow;
            _allConnections.RemoveAll(tuple =>
            {
                if (!tuple.InUse && now - tuple.LastUsed > _idleTimeout)
                {
                    try
                    {
                        Console.WriteLine("[LDAP POOL] Evicting idle connection.");
                        LogPoolMetrics("EvictIdleConnections - disposed");
                        tuple.Conn.Dispose();
                    }
                    catch { }
                    return true;
                }
                return false;
            });
        }
    }

    public async ValueTask DisposeAsync()
    {
        _evictionTimer.Dispose();
        while (await _channel.Reader.WaitToReadAsync())
        {
            while (_channel.Reader.TryRead(out var conn))
            {
                conn.Dispose();
            }
        }

        lock (_allConnections)
        {
            foreach (var (conn, _, _) in _allConnections)
            {
                conn.Dispose();
            }
            _allConnections.Clear();
        }
    }

    private bool IsConnectionAlive(LdapConnection conn)
    {
        try
        {
            var request = new SearchRequest("", "(objectClass=*)", SearchScope.Base);
            conn.SendRequest(request);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public void RemoveFromAll(LdapConnection conn)
    {
        lock (_allConnections)
        {
            _allConnections.RemoveAll(t => t.Conn == conn);
        }
    }

    private void UpdateLastUsed(LdapConnection conn)
    {
        lock (_allConnections)
        {
            for (int i = 0; i < _allConnections.Count; i++)
            {
                if (_allConnections[i].Conn == conn)
                {
                    _allConnections[i] = (_allConnections[i].Conn, DateTime.UtcNow, _allConnections[i].InUse);
                    break;
                }
            }
        }
    }

    private void MarkInUse(LdapConnection conn, bool inUse)
    {
        lock (_allConnections)
        {
            for (int i = 0; i < _allConnections.Count; i++)
            {
                if (_allConnections[i].Conn == conn)
                {
                    _allConnections[i] = (_allConnections[i].Conn, _allConnections[i].LastUsed, inUse);
                    break;
                }
            }
        }
    }

    #region Monitor cmd
    private string MetricsFile
    {
        get
        {
            string datePart = DateTime.UtcNow.ToString("yyyy-MM-dd");
            string fileName = $"ldap_pool_metrics_{datePart}.csv";
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", fileName);
        }
    }

    private void LogPoolMetrics(string action)
    {
        lock (_allConnections)
        {
            int totalConnections = _allConnections.Count;
            int channelCount = _channel.Reader.Count;
            string logLine = $"{DateTime.UtcNow:O},{action},{totalConnections},{channelCount}";

            try
            {
                string metricsFile = MetricsFile;
                Directory.CreateDirectory(Path.GetDirectoryName(metricsFile));

                if (!File.Exists(metricsFile))
                {
                    File.AppendAllText(metricsFile, "Timestamp,Action,TotalConnections,ChannelCount" + Environment.NewLine);
                }

                File.AppendAllText(metricsFile, logLine + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LDAP POOL METRIC ERROR] {ex.Message}");
            }

            Console.WriteLine($"[LDAP POOL METRIC] {logLine}");
        }
    }
    #endregion
}
