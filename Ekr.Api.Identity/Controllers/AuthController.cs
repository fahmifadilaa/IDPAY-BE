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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NPOI.OpenXmlFormats.Wordprocessing;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UdpClient.Services;
using Microsoft.AspNetCore.Mvc.Controllers;
using Org.BouncyCastle.Ocsp;
using static NPOI.HSSF.Util.HSSFColor;
using ServiceStack.Text;

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
        private readonly UdpLogService _udpLogService;

        public AuthController(ILogger<AuthController> logger,
            IUserManager userManager,
            IRefreshToken refreshToken,
            IRTokenRepository iRTokenRepository,
            IConfiguration config,
            UdpLogService udpLogService)
        {
            _logger = logger;
            _userManager = userManager;
            _refreshToken = refreshToken;
            _iRTokenRepository = iRTokenRepository;
            _config = config;
            _udpLogService = udpLogService;
        }

        [HttpPost("agent")]
        [ProducesResponseType(typeof(ServiceResponse<AuthAgentRes>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Login Agent")]
        public async Task<ServiceResponse<AuthAgentRes>> LoginAgent([FromBody] AuthAgentReq req)
        {
            var uuid = Guid.NewGuid();
            var startTime = DateTime.Now;

            var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var clientPort = HttpContext.Connection.RemotePort.ToString();

            var host = _config.GetValue<string>("UdpLogging:Host");
            var port = _config.GetValue<int>("UdpLogging:Port");

            var maskedPayload = UdpLogService.MaskSensitiveFields(req, new[] { "Password" });

            var _actionDescriptor = ControllerContext.ActionDescriptor as ControllerActionDescriptor;
            var serviceName = _actionDescriptor?.ControllerTypeInfo?.Namespace ?? "UnknownService";
            var serviceNameWithoutControllers = serviceName.Replace(".Controllers", "");
            var methodName = ControllerContext.ActionDescriptor.RouteValues["action"] ?? "UnknownMethod";

            ServiceResponse<AuthAgentRes> response;
            string logMessage = "";
            var endTime = DateTime.Now;

            var tokenRepo = await _userManager.CheckExistingLoginAgent(
                req.Name, req.ClientId, req.IpAddress, req.Token,
                int.Parse(_config.GetValue<string>("AppSettings:TokenLifetime")));

            if (tokenRepo.error == "-1")
            {
                logMessage = "91507 || Username atau password salah";
                response = new ServiceResponse<AuthAgentRes>
                {
                    Message = logMessage,
                    Status = (int)ServiceResponseStatus.ERROR
                };
            }
            else if (!string.IsNullOrEmpty(tokenRepo.jwt))
            {
                logMessage = nameof(ServiceResponseStatus.SUKSES);
                response = new ServiceResponse<AuthAgentRes>
                {
                    Status = (int)ServiceResponseStatus.SUKSES,
                    Message = logMessage,
                    Data = new AuthAgentRes
                    {
                        JwtToken = tokenRepo.jwt,
                        RefreshToken = tokenRepo.refreshToken
                    }
                };
            }
            else
            {
                var (jwt, error, refreshToken) = await _userManager.AuthenticateAgent(req.Name, req.ClientId, req.IpAddress, req.Token);

                if (string.IsNullOrWhiteSpace(jwt))
                {
                    logMessage = error;
                    response = new ServiceResponse<AuthAgentRes>
                    {
                        Message = error,
                        Status = (int)ServiceResponseStatus.ERROR
                    };
                }
                else
                {
                    logMessage = nameof(ServiceResponseStatus.SUKSES);
                    response = new ServiceResponse<AuthAgentRes>
                    {
                        Status = (int)ServiceResponseStatus.SUKSES,
                        Message = logMessage,
                        Data = new AuthAgentRes
                        {
                            JwtToken = jwt,
                            RefreshToken = refreshToken
                        }
                    };
                }
            }

            HttpContext.Response.StatusCode = response.Status == (int)ServiceResponseStatus.SUKSES
                ? StatusCodes.Status200OK
                : StatusCodes.Status500InternalServerError;

            endTime = DateTime.Now;

            var logObject = _udpLogService.CreateLogObject(
                uuid,
                startTime,
                endTime,
                clientIp,
                clientPort,
                serviceNameWithoutControllers,
                methodName,
                "Login",
                maskedPayload,
                HttpContext.Response.StatusCode,
                logMessage);

            string logJson = JsonConvert.SerializeObject(logObject);
            await _udpLogService.SendAsync(logJson, host, port);

            return response;
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

            var uuid = Guid.NewGuid();
            var startTime = DateTime.Now;

            var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var clientPort = HttpContext.Connection.RemotePort.ToString();

            var host = _config.GetValue<string>("UdpLogging:Host");
            var port = _config.GetValue<int>("UdpLogging:Port");

            // Masking sensitive fields dynamically (like "Password")
            var maskedPayload = UdpLogService.MaskSensitiveFields(req, new[] { "Password" });
            var endTime = DateTime.Now;

            var _actionDescriptor = ControllerContext.ActionDescriptor as ControllerActionDescriptor;
            var serviceName = _actionDescriptor?.ControllerTypeInfo?.Namespace ?? "UnknownService";
            var serviceNameWithoutControllers = serviceName.Replace(".Controllers", "");
            var methodName = ControllerContext.ActionDescriptor.RouteValues["action"] ?? "UnknownMethod";

            if (isLimited)
            {
                var (jwt, error, refreshToken) = await _userManager.AuthenticateUserLimited(
                req.Nik, req.ClientId, req.IpAddress, req.Password, req.LoginType, req.Finger, BaseUrl, MatchImageBase64ToBase64, isEncrypt);

                bool isSuccess = !string.IsNullOrWhiteSpace(jwt);
                HttpContext.Response.StatusCode = isSuccess ? StatusCodes.Status200OK : StatusCodes.Status500InternalServerError;

                var logObject = _udpLogService.CreateLogObject(
               uuid,
               startTime,
               endTime,
               clientIp,
               clientPort,
               serviceNameWithoutControllers,
               methodName,
               "Login",
               maskedPayload,
               HttpContext.Response.StatusCode,
               isSuccess ? nameof(ServiceResponseStatus.SUKSES) : error);

                string logJson = JsonConvert.SerializeObject(logObject);

                await _udpLogService.SendAsync(logJson, host, port);

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

                bool isSuccess = !string.IsNullOrWhiteSpace(jwt);
                HttpContext.Response.StatusCode = isSuccess ? StatusCodes.Status200OK : StatusCodes.Status500InternalServerError;

                var logObject = _udpLogService.CreateLogObject(
               uuid,
               startTime,
               endTime,
               clientIp,
               clientPort,
               serviceNameWithoutControllers,
               methodName,
               "Login",
               maskedPayload,
               HttpContext.Response.StatusCode,
               isSuccess ? nameof(ServiceResponseStatus.SUKSES) : error);

                string logJson = JsonConvert.SerializeObject(logObject);

                await _udpLogService.SendAsync(logJson, host, port);

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

            var uuid = Guid.NewGuid();
            var startTime = DateTime.Now;

            var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var clientPort = HttpContext.Connection.RemotePort.ToString();

            var host = _config.GetValue<string>("UdpLogging:Host");
            var port = _config.GetValue<int>("UdpLogging:Port");

            // Masking sensitive fields dynamically (like "Password")
            var maskedPayload = UdpLogService.MaskSensitiveFields(req, new[] { "Password" });
            var endTime = DateTime.Now;

            var _actionDescriptor = ControllerContext.ActionDescriptor as ControllerActionDescriptor;
            var serviceName = _actionDescriptor?.ControllerTypeInfo?.Namespace ?? "UnknownService";
            var serviceNameWithoutControllers = serviceName.Replace(".Controllers", "");
            var methodName = ControllerContext.ActionDescriptor.RouteValues["action"] ?? "UnknownMethod";

            if (isLimited)
            {
                var (jwt, error, refreshToken) = await _userManager.AuthenticateUserLimited(
                req.Nik, req.ClientId, req.IpAddress, req.Password, req.LoginType, req.Finger, BaseUrl, MatchImageBase64ToBase64, isEncrypt);

                bool isSuccess = !string.IsNullOrWhiteSpace(jwt);
                HttpContext.Response.StatusCode = isSuccess ? StatusCodes.Status200OK : StatusCodes.Status500InternalServerError;

                var logObject = _udpLogService.CreateLogObject(
               uuid,
               startTime,
               endTime,
               clientIp,
               clientPort,
               serviceNameWithoutControllers,
               methodName,
               "Login",
               maskedPayload,
               HttpContext.Response.StatusCode,
               isSuccess ? nameof(ServiceResponseStatus.SUKSES) : error);

                string logJson = JsonConvert.SerializeObject(logObject);

                await _udpLogService.SendAsync(logJson, host, port);

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

                bool isSuccess = !string.IsNullOrWhiteSpace(jwt);
                HttpContext.Response.StatusCode = isSuccess ? StatusCodes.Status200OK : StatusCodes.Status500InternalServerError;

                var logObject = _udpLogService.CreateLogObject(
               uuid,
               startTime,
               endTime,
               clientIp,
               clientPort,
               serviceNameWithoutControllers,
               methodName,
               "Login",
               maskedPayload,
               HttpContext.Response.StatusCode,
               isSuccess ? nameof(ServiceResponseStatus.SUKSES) : error);

                string logJson = JsonConvert.SerializeObject(logObject);

                await _udpLogService.SendAsync(logJson, host, port);

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

            var uuid = Guid.NewGuid();
            var startTime = DateTime.Now;

            var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var clientPort = HttpContext.Connection.RemotePort.ToString();

            var host = _config.GetValue<string>("UdpLogging:Host");
            var port = _config.GetValue<int>("UdpLogging:Port");

            // Masking sensitive fields dynamically (like "Password")
            var maskedPayload = UdpLogService.MaskSensitiveFields(Req, new[] { "Password" });
            var endTime = DateTime.Now;

            var _actionDescriptor = ControllerContext.ActionDescriptor as ControllerActionDescriptor;
            var serviceName = _actionDescriptor?.ControllerTypeInfo?.Namespace ?? "UnknownService";
            var serviceNameWithoutControllers = serviceName.Replace(".Controllers", "");
            var methodName = ControllerContext.ActionDescriptor.RouteValues["action"] ?? "UnknownMethod";

            bool isSuccess = !string.IsNullOrWhiteSpace(token);
            HttpContext.Response.StatusCode = isSuccess ? StatusCodes.Status200OK : StatusCodes.Status500InternalServerError;

            var logObject = _udpLogService.CreateLogObject(
           uuid,
           startTime,
           endTime,
           clientIp,
           clientPort,
           serviceNameWithoutControllers,
           methodName,
           "Login",
           maskedPayload,
           HttpContext.Response.StatusCode,
           isSuccess ? nameof(ServiceResponseStatus.SUKSES) : error);

            string logJson = JsonConvert.SerializeObject(logObject);

            _ = _udpLogService.SendAsync(logJson, host, port).ConfigureAwait(false);

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

            var uuid = Guid.NewGuid();
            var startTime = DateTime.Now;

            var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var clientPort = HttpContext.Connection.RemotePort.ToString();

            var host = _config.GetValue<string>("UdpLogging:Host");
            var port = _config.GetValue<int>("UdpLogging:Port");

            // Masking sensitive fields dynamically (like "Password")
            var maskedPayload = UdpLogService.MaskSensitiveFields(Req, new[] { "Password" });
            var endTime = DateTime.Now;

            var _actionDescriptor = ControllerContext.ActionDescriptor as ControllerActionDescriptor;
            var serviceName = _actionDescriptor?.ControllerTypeInfo?.Namespace ?? "UnknownService";
            var serviceNameWithoutControllers = serviceName.Replace(".Controllers", "");
            var methodName = ControllerContext.ActionDescriptor.RouteValues["action"] ?? "UnknownMethod";

            bool isSuccess = !string.IsNullOrWhiteSpace(token);
            HttpContext.Response.StatusCode = isSuccess ? StatusCodes.Status200OK : StatusCodes.Status500InternalServerError;

            var logObject = _udpLogService.CreateLogObject(
           uuid,
           startTime,
           endTime,
           clientIp,
           clientPort,
           serviceNameWithoutControllers,
           methodName,
           "Login",
           maskedPayload,
           HttpContext.Response.StatusCode,
           isSuccess ? nameof(ServiceResponseStatus.SUKSES) : error);

            string logJson = JsonConvert.SerializeObject(logObject);

            _ = _udpLogService.SendAsync(logJson, host, port).ConfigureAwait(false);

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

            var uuid = Guid.NewGuid();
            var startTime = DateTime.Now;

            var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var clientPort = HttpContext.Connection.RemotePort.ToString();

            var host = _config.GetValue<string>("UdpLogging:Host");
            var port = _config.GetValue<int>("UdpLogging:Port");

            // Masking sensitive fields dynamically (like "Password")
            var maskedPayload = UdpLogService.MaskSensitiveFields(Req, new[] { "Password" });
            var endTime = DateTime.Now;

            var _actionDescriptor = ControllerContext.ActionDescriptor as ControllerActionDescriptor;
            var serviceName = _actionDescriptor?.ControllerTypeInfo?.Namespace ?? "UnknownService";
            var serviceNameWithoutControllers = serviceName.Replace(".Controllers", "");
            var methodName = ControllerContext.ActionDescriptor.RouteValues["action"] ?? "UnknownMethod";

            bool isSuccess = !string.IsNullOrWhiteSpace(Req.RefreshTokenn);
            HttpContext.Response.StatusCode = isSuccess ? StatusCodes.Status200OK : StatusCodes.Status500InternalServerError;

            var logObject = _udpLogService.CreateLogObject(
           uuid,
           startTime,
           endTime,
           clientIp,
           clientPort,
           serviceNameWithoutControllers,
           methodName,
           "Login",
           maskedPayload,
           HttpContext.Response.StatusCode,
           isSuccess ? nameof(ServiceResponseStatus.SUKSES) : nameof(ServiceResponseStatus.ERROR));

            string logJson = JsonConvert.SerializeObject(logObject);

            _ = _udpLogService.SendAsync(logJson, host, port).ConfigureAwait(false);

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

            var uuid = Guid.NewGuid();
            var startTime = DateTime.Now;

            var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            var clientPort = HttpContext.Connection.RemotePort.ToString();

            var host = _config.GetValue<string>("UdpLogging:Host");
            var port = _config.GetValue<int>("UdpLogging:Port");

            // Masking sensitive fields dynamically (like "Password")
            var payload = new { Npp, password };
            var maskedPayload = UdpLogService.MaskSensitiveFields(payload, new[] { "password" });
            var endTime = DateTime.Now;

            var _actionDescriptor = ControllerContext.ActionDescriptor as ControllerActionDescriptor;
            var serviceName = _actionDescriptor?.ControllerTypeInfo?.Namespace ?? "UnknownService";
            var serviceNameWithoutControllers = serviceName.Replace(".Controllers", "");
            var methodName = ControllerContext.ActionDescriptor.RouteValues["action"] ?? "UnknownMethod";

            var ldap = new LDAPConfig
            {
                Url = LdapUrl,
                LdapHierarchy = LdapHir,
                IbsRoleLdapHierarchy = IbsLdapHir
            };

            bool isSuccess = !string.IsNullOrWhiteSpace(Npp) && !string.IsNullOrWhiteSpace(password);
            HttpContext.Response.StatusCode = isSuccess ? StatusCodes.Status200OK : StatusCodes.Status500InternalServerError;

            var logObject = _udpLogService.CreateLogObject(
            uuid,
            startTime,
            endTime,
            clientIp,
            clientPort,
            serviceNameWithoutControllers,
            methodName,
            "Login",
            maskedPayload,
            HttpContext.Response.StatusCode,
            isSuccess ? nameof(ServiceResponseStatus.SUKSES) : nameof(ServiceResponseStatus.EMPTY_PARAMETER));

            var logJson = JsonConvert.SerializeObject(logObject);
            _ = _udpLogService.SendAsync(logJson, host, port).ConfigureAwait(false);


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
