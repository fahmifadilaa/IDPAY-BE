using Ekr.Api.DataMatching.Filters;
using Ekr.Auth;
using Ekr.Business.Contracts.DataKTP;
using Ekr.Business.Contracts.Recognition;
using Ekr.Core.Constant;
using Ekr.Core.Entities.DataKTP;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace Ekr.Api.DataMatching.Controllers
{
    [Route("matching")]
    [ApiController]
    public class MatchingFingerController : ControllerBase
    {
        private readonly IProfileService _profileService;
        private readonly IMatchingFingerService _matchingFingerService;
        private readonly IConfiguration _configuration;

        public MatchingFingerController(IProfileService profileService,
            IMatchingFingerService matchingFingerService, IConfiguration configuration)
        {
            _profileService = profileService;
            _matchingFingerService = matchingFingerService;
            _configuration = configuration;
        }

        [HttpPost("profile")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse<ProfileByNik>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<Core.Entities.ServiceResponse<ProfileByNik>> GetProfile([FromBody] ProfileReq req)
        {

            var BaseUrl = _configuration.GetValue<string>("UrlImageRecognition:BaseUrl");
            var EndPoint = _configuration.GetValue<string>("UrlImageRecognition:MatchImageBase64ToBase64");

            var obj = await _profileService.GetAuthKTPData(req.Base64Img, req.Nik, req.FingerType, BaseUrl, EndPoint);

            return new Core.Entities.ServiceResponse<ProfileByNik>
            {
                Message = obj.Message,
                Status = obj.Status.Equals("error") ? (int)ServiceResponseStatus.ERROR : (int)ServiceResponseStatus.SUKSES,
                Data = obj.Data
            };
        }

        [HttpPost("finger-loop")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse<string>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<Core.Entities.ServiceResponse<string>> MatchFingerLoop([FromBody] ProfileLoopReq req)
        {
            var RequestTime = DateTime.Now;
            var BaseUrl = _configuration.GetValue<string>("UrlImageRecognition:BaseUrl");
            var EndPoint = _configuration.GetValue<string>("UrlImageRecognition:MatchImageBase64ToBase64");

            var obj = await _matchingFingerService.MatchFinger(req, BaseUrl, EndPoint, RequestTime, req.Npp, req.UnitCode);

            return new Core.Entities.ServiceResponse<string>
            {
                Message = obj.msg,
                Status = (int)ServiceResponseStatus.SUKSES,
                Data = obj.msg,
                Code = obj.status.Equals("error") ? 1 : 0
            };
        }

        [HttpPost("finger-type")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse<string>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<Core.Entities.ServiceResponse<string>> MatchFingerType([FromBody] ProfileReq req)
        {
            var BaseUrl = _configuration.GetValue<string>("UrlImageRecognition:BaseUrl");
            var EndPoint = _configuration.GetValue<string>("UrlImageRecognition:MatchUrlImagesToBase64Json");

            var obj = await _matchingFingerService.MatchFingerType(req.Base64Img, req.Nik, req.FingerType, BaseUrl, EndPoint);

            return new Core.Entities.ServiceResponse<string>
            {
                Message = obj.msg,
                Status = (int)ServiceResponseStatus.SUKSES,
                Data = obj.msg,
                Code = obj.status.Equals("error") ? 1 : 0
            };
        }
    }
}
