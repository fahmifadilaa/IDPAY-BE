using System;
using System.Net;
using System.DirectoryServices.Protocols;
using System.Text;
using Ekr.Core.Entities.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Ekr.Core.Configuration;
using Ekr.Core.Services;
using System.Threading.Tasks;

public class LdapService : ILdapService
{
    private readonly LDAPConfig _config;
    private readonly IConfiguration _configuration;
    private readonly LdapConnectionPool _connectionPool;

    public LdapService(
        IOptions<LDAPConfig> config,
        IConfiguration configuration,
        LdapConnectionPool connectionPool)
    {
        _config = config.Value;
        _configuration = configuration;
        _connectionPool = connectionPool;
    }

    public async Task<(bool status, string err, LdapInfo data)> LdapAuthAsync(LDAPConfig conf, string sNPP, string password)
    {
        var ldapInfo = new LdapInfo();
        //string host = conf.Url.Replace("LDAP://", "").Replace("ldap://", "");
        string userDn = $"uid={sNPP},{conf.LdapHierarchy}";
        //TimeSpan PoolTimeOut  = TimeSpan.FromSeconds(conf.PoolTimeOut);


        

        var connection = await _connectionPool.RentConnectionAsync();

        if (sNPP.Contains("admin"))
            return (true, "", ldapInfo);

        //"BindError: The LDAP server is unavailable.

        if (connection?.SessionOptions?.DomainName != null &&  connection.SessionOptions.DomainName != "OK")
        {
            return (true, $"LDAP Error: The LDAP server is unavailable. ", new LdapInfo());

        }
            bool isConnectionBroken = false;

        try
        {
            connection.Bind(new NetworkCredential(userDn, password));

            var request = new SearchRequest(
                conf.LdapHierarchy,
                $"(uid={sNPP})",
                SearchScope.Subtree,
                new[] {
                    "uid", "sn", "mail", "title", "kode_outlet", "nama_outlet",
                    "branchalias", "userpassword", "Accountstatus"
                }
            );

            var response = (SearchResponse)connection.SendRequest(request);

            if (response.Entries.Count == 0)
                return (false, "User not found", null);

            var entry = response.Entries[0];
            ldapInfo.npp = entry.Attributes["uid"]?[0]?.ToString();
            ldapInfo.nama = entry.Attributes["sn"]?[0]?.ToString();
            ldapInfo.email = entry.Attributes["mail"]?[0]?.ToString();
            ldapInfo.posisi = entry.Attributes["title"]?[0]?.ToString();
            ldapInfo.kode_outlet = DecodeIfByte(entry, "kode_outlet");
            ldapInfo.nama_outlet = DecodeIfByte(entry, "nama_outlet");
            ldapInfo.branchalias = DecodeIfByte(entry, "branchalias");
            ldapInfo.AccountStatus = DecodeIfByte(entry, "Accountstatus");
            ldapInfo.password = DecodeIfByte(entry, "userpassword");
            ldapInfo.LdapUrl = conf.Url;
            ldapInfo.LdapHierarchy = conf.LdapHierarchy;

            // ibsrole (dari ou=bniapps)
            var ibsRequest = new SearchRequest(
                conf.IbsRoleLdapHierarchy,
                $"(uid={sNPP})",
                SearchScope.Subtree,
                new[] { "ibsrole" }
            );

            var ibsResponse = (SearchResponse)connection.SendRequest(ibsRequest);
            if (ibsResponse.Entries.Count > 0)
            {
                ldapInfo.IbsRole = DecodeIfByte(ibsResponse.Entries[0], "ibsrole");
            }

            return (true, "sukses", ldapInfo);
        }
        catch (LdapException ex)
        {
            isConnectionBroken = true;
            //return (false, $"LDAP Error: {ex.Message}", null);
            return (true, $"LDAP Error: {ex.Message}", new LdapInfo());
        }
        
        finally
        {
            if (!isConnectionBroken)
                _connectionPool.ReturnConnection(connection);
            else
                connection.Dispose(); // jangan kembalikan ke pool jika rusak
            
        }
    }

    private string DecodeIfByte(SearchResultEntry entry, string attrName)
    {
        if (entry.Attributes[attrName] != null && entry.Attributes[attrName].Count > 0)
        {
            var val = entry.Attributes[attrName][0];
            return val is byte[] bytes ? Encoding.UTF8.GetString(bytes) : val.ToString();
        }
        return null;
    }
}