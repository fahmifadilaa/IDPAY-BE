namespace Ekr.Auth.Contracts
{
    public interface IRefreshToken
    {
        (string token, string refreshToken, string error) DoRefreshTokenAgent(string refreshTokenn, string clientId,
            string userCode, string jwtToken, string ipAddress);
        (string token, string refreshToken, string error) DoRefreshTokenUser(string refreshTokenn, string clientId,
            string userCode, string jwtToken, string ipAddress);
        (string token, string refreshToken, string error) DoRefreshTokenThirdParty(string refreshTokenn, string clientId,
            string userCode, string jwtToken, string ipAddress);
    }
}
