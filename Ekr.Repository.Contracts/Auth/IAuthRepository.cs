using Ekr.Core.Entities.Auth;
using Ekr.Core.Entities.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ekr.Repository.Contracts.Auth
{
    public interface IAuthRepository
    {
        Task<bool> LoginAgent(string name, string token);
        Task<DetailLogin> LoginUser(string nik);
        Task<DetailLoginThirdParty> LoginThirdParty(string username);
        Task<IEnumerable<MatchingLoginFinger>> GetDataFinger(string npp);
        Task<IEnumerable<MatchingLoginFinger>> GetDataFingerGeneral(string npp);
        Task<IEnumerable<MatchingLoginFinger>> GetDataFingerGeneralNew(string npp);
        Task<IEnumerable<MatchingLoginFinger>> GetDataFingerGeneralNewFile(string npp);
        Task<IEnumerable<MatchingLoginFinger>> GetDataFingerGeneralEmp(string nik);
        Task<IEnumerable<MatchingLoginFinger>> GetDataFingerGeneralEmpNew(string nik);
        Task<IEnumerable<MatchingLoginFinger>> GetDataFingerGeneralEmpNewFileIso(string nik);
        Task<IEnumerable<MatchingLoginFinger>> GetDataFingerGeneralEmpIso(string nik);
        Task<IEnumerable<MatchingLoginFinger>> GetDataFingerGeneralByCif(string cif);
        Task<IEnumerable<MatchingLoginFinger>> GetDataFingerGeneralByCifNew(string cif);
        Task<IEnumerable<MatchingLoginFinger>> GetDataFingerGeneralByCifNewFile(string cif);
        Task<IEnumerable<MatchingLoginFinger>> GetDataFingerEmpGeneralByCif(string cif);
        Task<IEnumerable<MatchingLoginFinger>> GetDataFingerGeneralISO(string nik);
        Task<Tbl_LogClientApps> InsertLogClientApps(Tbl_LogClientApps req);
        Task<IEnumerable<MatchingLoginFinger>> GetDataFingerGeneralByCifIso(string cif);
        Task<Tbl_Login_Session> CheckSessionLogin(string nik, string ipAddress);
        Task<Tbl_Login_Session> InsertSessionLogin(Tbl_Login_Session req);
        Task<Tbl_Login_Session> UpdateSessionLogin(Tbl_Login_Session req);
    }
}
