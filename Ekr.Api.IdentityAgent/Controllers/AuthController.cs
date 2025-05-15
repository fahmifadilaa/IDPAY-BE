using Ekr.Api.IdentityAgent.Filters;
using Ekr.Auth.Contracts;
using Ekr.Core.Configuration;
using Ekr.Core.Constant;
using Ekr.Core.Entities;
using Ekr.Core.Entities.Auth;
using Ekr.Core.Entities.Token;
using Ekr.Repository.Contracts.Token;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Ekr.Api.IdentityAgent.Controllers
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

        

        
    }
}
