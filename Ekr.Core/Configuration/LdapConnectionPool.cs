using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Net;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Ekr.Core.Configuration;

public class LdapConnectionPool : IAsyncDisposable
{
    private readonly Channel<LdapConnection> _channel;
    private readonly TimeSpan _idleTimeout = TimeSpan.FromMinutes(5);
    private readonly TimeSpan _connectionTimeout;
    private readonly TimeSpan _PoolTimeOut;
    private readonly Timer _evictionTimer;
    private readonly string _host;
    private readonly int _port;
    private readonly int _maxSize;
    private readonly List<(LdapConnection, DateTime)> _allConnections = new();

    public LdapConnectionPool(IOptions<LDAPConfig> config)
    {
        var ldapConfig = config.Value;

        _host = new Uri(ldapConfig.Url).Host;
        _port = ldapConfig.Port;
        _maxSize = ldapConfig.MaxPoolSize > 0 ? ldapConfig.MaxPoolSize : 10;
        _connectionTimeout = TimeSpan.FromSeconds(ldapConfig.ConnTimeOut);
        _PoolTimeOut = TimeSpan.FromSeconds(ldapConfig.PoolTimeOut);

        _channel = Channel.CreateBounded<LdapConnection>(new BoundedChannelOptions(_maxSize)
        {
            SingleWriter = false,
            SingleReader = false
        });

        _evictionTimer = new Timer(EvictIdleConnections, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
    }

    public async Task<LdapConnection?> RentConnectionAsync(TimeSpan? timeout = null)
    {

        if (_channel.Reader.TryRead(out var conn))
        {
            Console.WriteLine($"[LDAP POOL] Reuse connection at {DateTime.UtcNow}");
            return conn;
        }

        lock (_allConnections)
        {
            if (_allConnections.Count < _maxSize)
            {
                var connection = CreateConnection();
                if (connection != null)
                {
                    _allConnections.Add((connection, DateTime.UtcNow));
                    Console.WriteLine($"[LDAP POOL] Created new connection at {DateTime.UtcNow}");
                    return connection;
                }
                else 
                {
                     return connection;

                }
            }
        }

        try
        {
            timeout = _PoolTimeOut;
            Console.WriteLine($"[LDAP POOL] Waiting for connection (Timeout: {timeout?.TotalSeconds ?? 10}s)...");
            using var cts = new CancellationTokenSource(timeout ?? TimeSpan.FromSeconds(10));
            var rentedConn = await _channel.Reader.ReadAsync(cts.Token);
            return rentedConn;
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("[LDAP POOL] Timed out waiting for available connection.");
            return null;
        }
    }

    public void ReturnConnection(LdapConnection conn)
    {
        if (!_channel.Writer.TryWrite(conn))
        {
            Console.WriteLine("[LDAP POOL] Channel full, disposing connection.");
            conn.Dispose();
        }
        else
        {
            Console.WriteLine($"[LDAP POOL] Connection returned at {DateTime.UtcNow}");
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

        if (string.IsNullOrEmpty(ldapAdminPassword))
        {
            connection.SessionOptions.DomainName = "Missing LDAP_AdminPassword";
            return connection;
        }

        if (string.IsNullOrEmpty(adminDn))
        {
            connection.SessionOptions.DomainName = "Missing LDAP_AdminDn";
            return connection;
        }

        connection.SessionOptions.ProtocolVersion = 3;

        try
        {
            connection.Bind(new NetworkCredential(adminDn, ldapAdminPassword));
            connection.SessionOptions.DomainName = "OK"; // ✅ tanda sukses
        }
        catch (LdapException ex)
        {

            connection.SessionOptions.DomainName = $"LDAP Error: {ex.Message}";
            // Jangan lempar, tetap kembalikan connection
            //Console.WriteLine($"[LDAP POOL] Failed to bind: {ex.Message}");
            //connection.Dispose(); // ❗ Penting
            //return null;          // ❗ Return null agar tidak masuk pool
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
                var (conn, lastUsed) = tuple;
                if (now - lastUsed > _idleTimeout)
                {
                    try
                    {
                        Console.WriteLine("[LDAP POOL] Evicting idle connection.");
                        conn.Dispose();
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
            foreach (var (conn, _) in _allConnections)
            {
                conn.Dispose();
            }
            _allConnections.Clear();
        }
    }
}
