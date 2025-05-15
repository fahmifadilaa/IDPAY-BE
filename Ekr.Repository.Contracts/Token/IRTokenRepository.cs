using Ekr.Core.Entities.Token;

namespace Ekr.Repository.Contracts.Token
{
    public interface IRTokenRepository
    {
        bool AddToken(Tbl_Jwt_Repository token);
        bool AddTokenThirdParty(Tbl_Jwt_Repository_ThirdParty token);
        bool ExpireToken(Tbl_Jwt_Repository token);
        bool ExpireTokenThirdParty(Tbl_Jwt_Repository_ThirdParty token);
        Tbl_Jwt_Repository GetToken(string refreshToken, string clientId, string userCode, string ipAddress);
        Tbl_Jwt_Repository_ThirdParty GetTokenThirdParty(string refreshToken, string clientId, string userCode, string ipAddress);
        void ExpireDuplicateRefreshToken(string userCode, string clientId);
        void ExpireDuplicateRefreshTokenThirdParty(string userCode, string clientId, string clientIp);
        Tbl_Jwt_Repository GetActiveToken(string clientId, string userCode, string ipAddress, int tokenLifetime);
        bool UpdateToken(Tbl_Jwt_Repository token);
        bool DeleteToken(Tbl_Jwt_Repository token);
        bool AddTokenLog(Tbl_Jwt_Repository token);
        void ExpireDuplicateRefreshTokenAgent(string userCode, string clientId, string clientIp);
    }
}
