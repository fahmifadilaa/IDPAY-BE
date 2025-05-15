using Ekr.Api.Identity.Filters;
using Ekr.Auth.Contracts;
using Ekr.Core.Configuration;
using Ekr.Core.Constant;
using Ekr.Core.Entities;
using Ekr.Core.Entities.Auth;
using Ekr.Core.Entities.Enrollment;
using Ekr.Core.Entities.Token;
using Ekr.Core.Securities.Symmetric;
using Ekr.Repository.Contracts.Token;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NPOI.OpenXmlFormats.Wordprocessing;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Ekr.Api.Identity.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {

        private readonly ILogger<AuthController> _logger;
        private readonly IUserManager _userManager;
        private readonly IRefreshToken _refreshToken;
        private readonly IRTokenRepository _iRTokenRepository;
        private readonly IConfiguration _config;

        public AuthController(ILogger<AuthController> logger,
            IUserManager userManager,
            IRefreshToken refreshToken,
            IRTokenRepository iRTokenRepository,
             IConfiguration config)
        {
            _logger = logger;
            _userManager = userManager;
            _refreshToken = refreshToken;
            _iRTokenRepository = iRTokenRepository;
            _config = config;
        }

        [HttpPost("agent")]
        [ProducesResponseType(typeof(ServiceResponse<AuthAgentRes>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Login Agent")]
        public async Task<ServiceResponse<AuthAgentRes>> LoginAgent([FromBody] AuthAgentReq req)
        {
            // check is active

            var tokenRepo = await _userManager.CheckExistingLoginAgent(req.Name, req.ClientId, req.IpAddress, req.Token,
                int.Parse(_config.GetValue<string>("AppSettings:TokenLifetime")));

            if (tokenRepo.error == "-1")
            {
                return new ServiceResponse<AuthAgentRes> { Message = "91507 || Username atau password salah", Status = (int)ServiceResponseStatus.ERROR };
            };

            if (!string.IsNullOrEmpty(tokenRepo.jwt))
            {
                return new ServiceResponse<AuthAgentRes>
                {
                    Status = (int)ServiceResponseStatus.SUKSES,
                    Message = nameof(ServiceResponseStatus.SUKSES),
                    Data = new AuthAgentRes
                    {
                        JwtToken = tokenRepo.jwt,
                        RefreshToken = tokenRepo.refreshToken
                    }
                };
            }

            var (jwt, error, refreshToken) = await _userManager.AuthenticateAgent(req.Name, req.ClientId, req.IpAddress, req.Token);

            if (string.IsNullOrWhiteSpace(jwt))
            {
                return new ServiceResponse<AuthAgentRes> { Message = error, Status = (int)ServiceResponseStatus.ERROR };
            }

            return new ServiceResponse<AuthAgentRes>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = nameof(ServiceResponseStatus.SUKSES),
                Data = new AuthAgentRes
                {
                    JwtToken = jwt,
                    RefreshToken = refreshToken
                }
            };
        }

        #region (old before 2024 pentest)
        //[HttpPost("user")]
        //[ProducesResponseType(typeof(ServiceResponse<AuthAgentRes>), 200)]
        //[ProducesResponseType(500)]
        //[LogActivity(Keterangan = "Login User")]
        //public async Task<ServiceResponse<AuthAgentRes>> LoginUser([FromBody] AuthUserReq req)
        //{
        //    var isEncrypt = _config.GetValue<bool>("isEncrypt");
        //    var BaseUrl = _config.GetValue<string>("UrlImageRecognition:BaseUrl");
        //    var MatchImageBase64ToBase64 = _config.GetValue<string>("UrlImageRecognition:MatchImageBase64ToBase64");
        //    var (jwt, error, refreshToken) = await _userManager.AuthenticateUser(
        //        req.Nik, req.ClientId, req.IpAddress, req.Password, req.LoginType, req.Finger, BaseUrl, MatchImageBase64ToBase64, isEncrypt);

        //    if (string.IsNullOrWhiteSpace(jwt))
        //    {
        //        return new ServiceResponse<AuthAgentRes> { Message = error, Status = (int)ServiceResponseStatus.ERROR };
        //    }

        //    return new ServiceResponse<AuthAgentRes>
        //    {
        //        Status = (int)ServiceResponseStatus.SUKSES,
        //        Message = nameof(ServiceResponseStatus.SUKSES),
        //        Data = new AuthAgentRes
        //        {
        //            JwtToken = jwt,
        //            RefreshToken = refreshToken
        //        }
        //    };
        //}
        #endregion

        #region (new after 2024 pentest)
            #region unencrypted login
        [HttpPost("user")]
        [ProducesResponseType(typeof(ServiceResponse<AuthAgentRes>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Login User")]
        public async Task<ServiceResponse<AuthAgentRes>> LoginUser([FromBody] AuthUserReq req)
        {
            var isEncrypt = _config.GetValue<bool>("isEncrypt");
            var isLimited = _config.GetValue<bool>("isLimited");
            var BaseUrl = _config.GetValue<string>("UrlImageRecognition:BaseUrl");
            var MatchImageBase64ToBase64 = _config.GetValue<string>("UrlImageRecognition:MatchImageBase64ToBase64");
            
            if (isLimited)
            {
                var (jwt, error, refreshToken) = await _userManager.AuthenticateUserLimited(
                req.Nik, req.ClientId, req.IpAddress, req.Password, req.LoginType, req.Finger, BaseUrl, MatchImageBase64ToBase64, isEncrypt);

                if (string.IsNullOrWhiteSpace(jwt))
                {
                    return new ServiceResponse<AuthAgentRes> { Message = error, Status = (int)ServiceResponseStatus.ERROR };
                }

                return new ServiceResponse<AuthAgentRes>
                {
                    Status = (int)ServiceResponseStatus.SUKSES,
                    Message = nameof(ServiceResponseStatus.SUKSES),
                    Data = new AuthAgentRes
                    {
                        JwtToken = jwt,
                        RefreshToken = refreshToken
                    }
                };

            }
            else
            {
                var (jwt, error, refreshToken) = await _userManager.AuthenticateUser(
                req.Nik, req.ClientId, req.IpAddress, req.Password, req.LoginType, req.Finger, BaseUrl, MatchImageBase64ToBase64, isEncrypt);

                if (string.IsNullOrWhiteSpace(jwt))
                {
                    return new ServiceResponse<AuthAgentRes> { Message = error, Status = (int)ServiceResponseStatus.ERROR };
                }

                return new ServiceResponse<AuthAgentRes>
                {
                    Status = (int)ServiceResponseStatus.SUKSES,
                    Message = nameof(ServiceResponseStatus.SUKSES),
                    Data = new AuthAgentRes
                    {
                        JwtToken = jwt,
                        RefreshToken = refreshToken
                    }
                };
            }
        }
        #endregion


            #region encrypted login
        [HttpPost("user-encrypted")]
        [ProducesResponseType(typeof(ServiceResponse<AuthAgentRes>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Login User")]
        public async Task<ServiceResponse<AuthAgentRes>> LoginUserEncrypted( EncryptNikPassword obj)
        {
            var isEncrypt = _config.GetValue<bool>("isEncrypt");
            var isLimited = _config.GetValue<bool>("isLimited");
            var BaseUrl = _config.GetValue<string>("UrlImageRecognition:BaseUrl");
            var MatchImageBase64ToBase64 = _config.GetValue<string>("UrlImageRecognition:MatchImageBase64ToBase64");
            var aesKey = _config.GetValue<string>("key");

            var objDecrypt = Aes256Encryption.Decrypt(obj.stringEncrypt, aesKey);
            //var repData = objDecrypt.Replace("=", ":");
            var resData = JsonConvert.DeserializeObject<AuthUserReq>(objDecrypt);
            //var resData = JsonConvert.DeserializeObject<AuthUserReq>(repData);

            var req = new AuthUserReq {
                Nik = resData.Nik,
                ClientId = resData.ClientId,
                IpAddress = resData.IpAddress,
                Password = resData.Password,
                LoginType = resData.LoginType,
                Finger = resData.Finger
            };

            if (isLimited)
            {
                var (jwt, error, refreshToken) = await _userManager.AuthenticateUserLimited(
                req.Nik, req.ClientId, req.IpAddress, req.Password, req.LoginType, req.Finger, BaseUrl, MatchImageBase64ToBase64, isEncrypt);

                if (string.IsNullOrWhiteSpace(jwt))
                {
                    return new ServiceResponse<AuthAgentRes> { Message = error, Status = (int)ServiceResponseStatus.ERROR };
                }

                return new ServiceResponse<AuthAgentRes>
                {
                    Status = (int)ServiceResponseStatus.SUKSES,
                    Message = nameof(ServiceResponseStatus.SUKSES),
                    Data = new AuthAgentRes
                    {
                        JwtToken = jwt,
                        RefreshToken = refreshToken
                    }
                };

            }
            else
            {
                var (jwt, error, refreshToken) = await _userManager.AuthenticateUser(
                req.Nik, req.ClientId, req.IpAddress, req.Password, req.LoginType, req.Finger, BaseUrl, MatchImageBase64ToBase64, isEncrypt);

                if (string.IsNullOrWhiteSpace(jwt))
                {
                    return new ServiceResponse<AuthAgentRes> { Message = error, Status = (int)ServiceResponseStatus.ERROR };
                }

                return new ServiceResponse<AuthAgentRes>
                {
                    Status = (int)ServiceResponseStatus.SUKSES,
                    Message = nameof(ServiceResponseStatus.SUKSES),
                    Data = new AuthAgentRes
                    {
                        JwtToken = jwt,
                        RefreshToken = refreshToken
                    }
                };
            }
        }
        #endregion
        #endregion

        [HttpPost("refresh-agent")]
        [ProducesResponseType(typeof(ServiceResponse<AuthAgentRes>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Refresh Agent")]
        public ServiceResponse<AuthAgentRes> RefreshTokenAgent([FromBody] RefreshTokenReq Req)
        {
            var (token, refreshToken, error) = _refreshToken.DoRefreshTokenAgent(Req.RefreshTokenn, Req.ClientId,
                Req.UserCode, Req.JwtToken, Req.IpAddress);

            if (string.IsNullOrWhiteSpace(token))
            {
                return new ServiceResponse<AuthAgentRes> { Message = error, Status = (int)ServiceResponseStatus.ERROR };
            }

            return new ServiceResponse<AuthAgentRes>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = nameof(ServiceResponseStatus.SUKSES),
                Data = new AuthAgentRes
                {
                    JwtToken = token,
                    RefreshToken = refreshToken
                }
            };
        }

        [HttpPost("refresh-user")]
        [ProducesResponseType(typeof(ServiceResponse<AuthAgentRes>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Refresh User")]
        public ServiceResponse<AuthAgentRes> RefreshTokenUser([FromBody] RefreshTokenReq Req)
        {
            var (token, refreshToken, error) = _refreshToken.DoRefreshTokenUser(Req.RefreshTokenn, Req.ClientId,
                Req.UserCode, Req.JwtToken, Req.IpAddress);

            if (string.IsNullOrWhiteSpace(token))
            {
                return new ServiceResponse<AuthAgentRes> { Message = error, Status = (int)ServiceResponseStatus.ERROR };
            }

            return new ServiceResponse<AuthAgentRes>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = nameof(ServiceResponseStatus.SUKSES),
                Data = new AuthAgentRes
                {
                    JwtToken = token,
                    RefreshToken = refreshToken
                }
            };
        }

        [HttpPost("is-alive")]
        [ProducesResponseType(typeof(ServiceResponse<Tbl_Jwt_Repository>), 200)]
        [ProducesResponseType(500)]
        public ServiceResponse<Tbl_Jwt_Repository> CheckSession([FromBody] RefreshTokenReq Req)
        {
            if (string.IsNullOrWhiteSpace(Req.RefreshTokenn))
            {
                return new ServiceResponse<Tbl_Jwt_Repository> { Message = "parameter null", Status = (int)ServiceResponseStatus.ERROR };
            }

            var token = _iRTokenRepository.GetToken(Req.RefreshTokenn, Req.ClientId, Req.UserCode, Req.IpAddress);

            return new ServiceResponse<Tbl_Jwt_Repository>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = nameof(ServiceResponseStatus.SUKSES),
                Data = token
            };
        }

        /// <summary>
        /// Get data Ldap Info
        /// </summary>
        /// <param name="Npp"></param>
        /// <returns></returns>
        [HttpPost("GetLdapInfo")]
        [ProducesResponseType(typeof(ServiceResponse<LdapInfo>), 200)]
        [ProducesResponseType(500)]
        public ServiceResponse<LdapInfo> GetLdapInfo([FromQuery] string Npp, string password)
        {
            var LdapUrl = _config.GetValue<string>("LDAPConfig:Url");
            var LdapHir = _config.GetValue<string>("LDAPConfig:LdapHierarchy");
            var IbsLdapHir = _config.GetValue<string>("LDAPConfig:IbsRoleLdapHierarchy");
            var ldap = new LDAPConfig
            {
                Url = LdapUrl,
                LdapHierarchy = LdapHir,
                IbsRoleLdapHierarchy = IbsLdapHir
            };
            if (string.IsNullOrEmpty(Npp) || string.IsNullOrEmpty(password))
            {
                return new ServiceResponse<LdapInfo>
                {
                    Status = (int)ServiceResponseStatus.EMPTY_PARAMETER,
                    Message = nameof(ServiceResponseStatus.EMPTY_PARAMETER),
                    Data = null
                };
            }

            var data = _userManager.GetLdap(ldap, Npp, password);

            return new ServiceResponse<LdapInfo>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = nameof(ServiceResponseStatus.SUKSES),
                Data = data
            };
        }

    }
}
