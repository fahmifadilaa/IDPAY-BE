using Ekr.Core.Configuration;
using Ekr.Core.Entities.Auth;
using System.Threading.Tasks;

namespace Ekr.Core.Services
{
    public interface ILdapService
    {
        //(bool status, string err, LdapInfo data) LdapAuth(LDAPConfig conf, string sNPP, string password);

        Task<(bool status, string err, LdapInfo data)> LdapAuthAsync(LDAPConfig conf, string sNPP, string password);
    }
}
