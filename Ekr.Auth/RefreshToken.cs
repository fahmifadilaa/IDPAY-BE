using Ekr.Auth.Contracts;
using Ekr.Core.Configuration;
using Ekr.Core.Entities.Token;
using Ekr.Repository.Contracts.Token;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Ekr.Auth
{
    public class RefreshToken : IRefreshToken
    {
        private readonly CredConfig _appSettings;
        private readonly IRTokenRepository _repo;
        private readonly ErrorMessageConfig _errorMessageConfig;
        private readonly successMessageConfig _successMessageConfig;
        public RefreshToken(IRTokenRepository repo, IOptions<CredConfig> appSettings, IOptions<ErrorMessageConfig> errorMessageConfig,
            IOptions<successMessageConfig> successMessageConfig
            )
        {
            _repo = repo;
            _appSettings = appSettings.Value;
            _errorMessageConfig = errorMessageConfig.Value;
            _successMessageConfig = successMessageConfig.Value;
        }

        public (string token, string refreshToken, string error) DoRefreshTokenAgent(string refreshTokenn, string clientId,
            string userCode, string jwtToken, string ipAddress)
        {
            var tokenRepo = _repo.GetToken(refreshTokenn, clientId, userCode, ipAddress);

            if (tokenRepo == null)
            {
                return ("", "", _errorMessageConfig.RefreshTokenFailed);
            }

            if (tokenRepo.IsExpired)
            {
                return ("", "", _errorMessageConfig.RefreshTokenInvalid);
            }

            var refresh_token = Guid.NewGuid().ToString().Replace("-", "");

            // expire the old refresh_token
            var updateFlag = _repo.ExpireToken(tokenRepo);

            // expire duplicate refresh token which is not be use anymore
            _repo.ExpireDuplicateRefreshToken(userCode, clientId);

            // add a new refresh_token
            var addFlag = _repo.AddToken(new Tbl_Jwt_Repository
            {
                UserCode = userCode,
                ClientId = clientId,
                ClientIp = ipAddress,
                RefreshToken = refresh_token,
                TokenId = Guid.NewGuid().ToString(),
                IsExpired = false
            });

            if (updateFlag && addFlag)
            {
                // get claims from existing token
                var principals = TokenManager.GetPrincipalAgent(jwtToken);

                // generate jwt token claims
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                    new Claim("Name", principals.Name ?? "x")
                    }),
                    Expires = DateTime.UtcNow.AddMinutes(30),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                    Issuer = _appSettings.Issuer
                };
                var token = tokenHandler.CreateToken(tokenDescriptor);
                var jwt = tokenHandler.WriteToken(token);

                return (jwt, refresh_token, "");
            }
            else
            {
                return ("", "", _errorMessageConfig.ErrorCreteTokenFromRefreshToken);
            }
        }

        public (string token, string refreshToken, string error) DoRefreshTokenThirdParty(string refreshTokenn, string clientId,
            string userCode, string jwtToken, string ipAddress)
        {
            var tokenRepo = _repo.GetTokenThirdParty(refreshTokenn, clientId, userCode, ipAddress);

            if (tokenRepo == null)
            {
                return ("", "", _errorMessageConfig.RefreshTokenFailed);
            }

            if (tokenRepo.IsExpired)
            {
                return ("", "", _errorMessageConfig.RefreshTokenInvalid);
            }

            var refresh_token = Guid.NewGuid().ToString().Replace("-", "");

            // expire the old refresh_token
            var updateFlag = _repo.ExpireTokenThirdParty(tokenRepo);

            // expire duplicate refresh token which is not be use anymore
            _repo.ExpireDuplicateRefreshTokenThirdParty(userCode, clientId, ipAddress);

            // add a new refresh_token
            var addFlag = _repo.AddTokenThirdParty(new Tbl_Jwt_Repository_ThirdParty
            {
                UserCode = userCode,
                ClientId = clientId,
                ClientIp = ipAddress,
                RefreshToken = refresh_token,
                TokenId = Guid.NewGuid().ToString(),
                IsExpired = false
            });

            if (updateFlag && addFlag)
            {
                // get claims from existing token
                var principals = TokenManager.GetPrincipalThirdParty(jwtToken);

                // generate jwt token claims
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                    new Claim("Name", principals.Name ?? "x")
                    }),
                    Expires = DateTime.UtcNow.AddMinutes(30),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                    Issuer = _appSettings.Issuer
                };
                var token = tokenHandler.CreateToken(tokenDescriptor);
                var jwt = tokenHandler.WriteToken(token);

                return (jwt, refresh_token, "");
            }
            else
            {
                return ("", "", _errorMessageConfig.ErrorCreteTokenFromRefreshToken);
            }
        }

        public (string token, string refreshToken, string error) DoRefreshTokenUser(string refreshTokenn, string clientId,
            string userCode, string jwtToken, string ipAddress)
        {
            var tokenRepo = _repo.GetToken(refreshTokenn, clientId, userCode, ipAddress);

            if (tokenRepo == null)
            {
                return ("", "", _errorMessageConfig.RefreshTokenFailed);
            }

            if (tokenRepo.IsExpired)
            {
                return ("", "", _errorMessageConfig.RefreshTokenInvalid);
            }

            var refresh_token = Guid.NewGuid().ToString().Replace("-", "");

            // expire the old refresh_token
            var updateFlag = _repo.ExpireToken(tokenRepo);

            // expire duplicate refresh token which is not be use anymore
            _repo.ExpireDuplicateRefreshToken(userCode, clientId);

            // add a new refresh_token
            var addFlag = _repo.AddToken(new Tbl_Jwt_Repository
            {
                UserCode = userCode,
                ClientId = clientId,
                ClientIp = ipAddress,
                RefreshToken = refresh_token,
                TokenId = Guid.NewGuid().ToString(),
                IsExpired = false
            });

            if (updateFlag && addFlag)
            {
                // get claims from existing token
                var principals = TokenManager.GetPrincipal(jwtToken);

                // generate jwt token claims
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                    new Claim("NamaPegawai", principals.NamaPegawai??"x"),
                    new Claim("NIK", principals.NIK??"x"),
                    new Claim("PegawaiId", principals.PegawaiId??"x"),
                    new Claim("UnitId", principals.UnitId??"x"),
                    new Claim("NamaUnit", principals.NamaUnit??"x"),
                    new Claim("UserId", principals.UserId??"x"),
                    new Claim("RoleId", principals.RoleId??"x"),
                    new Claim("RoleUnitId", principals.RoleUnitId??"x"),
                    new Claim("RoleNamaUnit", principals.RoleNamaUnit??"x"),
                    new Claim("NamaRole", principals.NamaRole??"x"),
                    new Claim("ImagesUser", principals.ImagesUser??"x"),
                    new Claim("StatusRole", principals.StatusRole??"x"),
                    new Claim("UserRoleId", principals.UserRoleId??"x"),
                    new Claim("KodeUnit", principals.KodeUnit??"x"),
                    new Claim("ApplicationId", "1")
                    }),
                    Expires = DateTime.UtcNow.AddMinutes(30),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                    Issuer = _appSettings.Issuer
                };
                var token = tokenHandler.CreateToken(tokenDescriptor);
                var jwt = tokenHandler.WriteToken(token);

                return (jwt, refresh_token, "");
            }
            else
            {
                return ("", "", _errorMessageConfig.ErrorCreteTokenFromRefreshToken);
            }
        }
    }
}
