using Ekr.Api.DataEnrollment.Filters;
using Ekr.Auth;
using Ekr.Business.Contracts.DataKTP;
using Ekr.Core.Constant;
using Ekr.Core.Entities.DataKTP;
using Ekr.Core.Entities.Recognition;
using Ekr.Repository.Contracts.Recognition;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ekr.Api.DataEnrollment.Controllers
{
    [ApiController]
    [Route("profile")]
    public class ProfileController : ControllerBase
    {
        private readonly ILogger<ProfileController> _logger;
        private readonly IProfileService _profileService;
        private readonly IFingerRepository _fingerRepository;
        private readonly IConfiguration _configuration;

        public ProfileController(ILogger<ProfileController> logger, IProfileService profileService,
            IFingerRepository fingerRepository, IConfiguration configuration)
        {
            _logger = logger;
            _profileService = profileService;
            _fingerRepository = fingerRepository;
            _configuration = configuration;
        }

        /// <summary>
        /// Get data profile if finger match
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("get-profile")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse<ProfileByNik>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Get data profile if finger match")]
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

        /// <summary>
        /// Get data profile if finger match without finger type
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("get-profile-loop")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse<ProfileByNik>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Get data profile if finger match without finger type")]
        public async Task<Core.Entities.ServiceResponse<ProfileByNik>> GetProfileAuthLoop([FromBody] ProfileLoopReq req)
        {
            var BaseUrl = _configuration.GetValue<string>("UrlImageRecognition:BaseUrl");
            var EndPoint = _configuration.GetValue<string>("UrlImageRecognition:MatchImageBase64ToBase64");

            var obj = await _profileService.GetAuthKTPData(req.Base64Img, req.Nik, BaseUrl, EndPoint);

            return new Core.Entities.ServiceResponse<ProfileByNik>
            {
                Message = obj.Message,
                Status = obj.Status.Equals("error") ? (int)ServiceResponseStatus.ERROR : (int)ServiceResponseStatus.SUKSES,
                Data = obj.Data
            };
        }

        [HttpGet("finger")]
        [ProducesResponseType(typeof(IEnumerable<FingerByNik>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "finger")]
        public async Task<Core.Entities.ServiceResponse<IEnumerable<FingerByNik>>> GetFinger(string nik)
        {
            //var decryptedNik = StringCipher.Decrypt(nik, Phrase.ParameterSecurity);

            var obj = await _fingerRepository.GetFingersEnrolled(nik);

            return new Core.Entities.ServiceResponse<IEnumerable<FingerByNik>>
            {
                Message = nameof(ServiceResponseStatus.SUKSES),
                Status = (int)ServiceResponseStatus.SUKSES,
                Data = obj
            };
        }

        /// <summary>
        /// Get data profile by NIK
        /// </summary>
        /// <param name="nik"></param>
        /// <returns></returns>
        [HttpGet("get-profile-bynik")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse<ProfileByNik>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Get data profile by NIK")]
        public async Task<Core.Entities.ServiceResponse<ProfileByNik>> GetProfile(string nik)
        {
            var obj = await _profileService.GetKTPData(nik);

            return new Core.Entities.ServiceResponse<ProfileByNik>
            {
                Message = obj.Message,
                Status = obj.Status.Equals("error") ? (int)ServiceResponseStatus.ERROR : (int)ServiceResponseStatus.SUKSES,
                Data = obj.Data
            };
        }

        /// <summary>
        /// Update CIF by NIK
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("change-cif")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse<string>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Update CIF by NIK")]
        public async Task<Core.Entities.ServiceResponse<string>> ChangeCIF([FromBody] ChangeCIFReq req)
        {
            var res = await _profileService.UpdateCIF(req.Nik, req.Cif, req.Source, req.Npp, req.Username, req.UnitCode);

            if (res == null)
            {
                return new Core.Entities.ServiceResponse<string>
                {
                    Message = "Cif Tidak Ditemukan",
                    Status = (int)ServiceResponseStatus.Data_Empty,
                    Data = null
                };
            }

            return new Core.Entities.ServiceResponse<string>
            {
                Message = nameof(ServiceResponseStatus.SUKSES),
                Status = (int)ServiceResponseStatus.SUKSES,
                Data = ""
            };
        }
    }
}
