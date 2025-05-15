using Ekr.Core.Configuration;
using Ekr.Core.Entities.Auth;
using System.Threading.Tasks;

namespace Ekr.Auth.Contracts
{
    public interface IUserManager
    {
        Task<(string jwt, string error, string refreshToken)> AuthenticateAgent(string name,
            string clientId, string ipAddress, string token);
        Task<(string jwt, string error, string refreshToken)> AuthenticateUser(string nik,
            string clientId, string ipAddress, string password, string loginType, string finger, string baseUrl, string targetURL, bool isEncrypt = false);
        Task<(string jwt, string error, string refreshToken)> AuthenticateUserLimited(string nik,
            string clientId, string ipAddress, string password, string loginType, string finger, string baseUrl, string targetURL, bool isEncrypt = false);
        Task<(string jwt, string error, string refreshToken)> AuthenticateThirdParty(string username,
            string clientId, string ipAddress, string password);
        LdapInfo GetLdap(LDAPConfig req, string npp, string password);
        Task<(string jwt, string error, string refreshToken)> CheckExistingLoginAgent(string name,
            string clientId, string ipAddress, string token, int tokenLifetime);
    }
}