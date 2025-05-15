using Ekr.Api.IdentityThirdParty.Filters;
using Ekr.Auth.Contracts;
using Ekr.Core.Constant;
using Ekr.Core.Entities;
using Ekr.Core.Entities.Auth;
using Ekr.Repository.Contracts.Token;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Ekr.Api.IdentityThirdParty.Controllers
{
    [ApiController]
    [Route("auth-thirdparty")]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly IUserManager _userManager;
        private readonly IRefreshToken _refreshToken;
        private readonly IRTokenRepository _iRTokenRepository;
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthController(ILogger<AuthController> logger,
            IUserManager userManager,
            IRefreshToken refreshToken,
            IRTokenRepository iRTokenRepository,
             IConfiguration config,
             IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _userManager = userManager;
            _refreshToken = refreshToken;
            _iRTokenRepository = iRTokenRepository;
            _config = config;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpPost("get-token")]
        [ProducesResponseType(typeof(ServiceResponse<AuthAgentRes>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Login Third Party")]
        public async Task<ServiceResponse<AuthAgentRes>> LoginThirdParty([FromBody] AuthThirdPartyReq req)
        {
            var remoteIpAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();

            var (jwt, error, refreshToken) = await _userManager.AuthenticateThirdParty(req.Name, req.ClientId, remoteIpAddress, req.Password);

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

        [HttpPost("refresh-token")]
        [ProducesResponseType(typeof(ServiceResponse<AuthAgentRes>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Refresh Token Third Party")]
        public ServiceResponse<AuthAgentRes> RefreshTokenAgent([FromBody] RefreshTokenReq Req)
        {
            var (token, refreshToken, error) = _refreshToken.DoRefreshTokenThirdParty(Req.RefreshTokenn, Req.ClientId,
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
