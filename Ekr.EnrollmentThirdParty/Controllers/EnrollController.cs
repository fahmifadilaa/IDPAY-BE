using Ekr.Auth;
using Ekr.Business.Contracts.DataKTP;
using Ekr.Business.Contracts.Enrollment;
using Ekr.Business.Contracts.EnrollmentNoMatching;
using Ekr.Core.Constant;
using Ekr.Core.Entities.DataKTP;
using Ekr.Core.Entities.Enrollment;
using Ekr.Core.Entities.ThirdParty;
using Ekr.EnrollmentThirdParty.Filters;
using Ekr.Repository.Contracts.DataKTP;
using Ekr.Repository.Contracts.DataMaster.Utility;
using Ekr.Repository.Contracts.EnrollmentNoMatching;
using Ekr.Services.Contracts.Account;
using Ekr.Services.Contracts.Recognition;
using Elastic.Apm.Api;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Ekr.EnrollmentThirdParty.Controllers
{
    [Route("enrollment-thirdparty")]
    [ApiController]
    public class EnrollController : ControllerBase
    {
        private readonly IEnrollmentNoMatchingService _enrollmentNoMatchingService;
        private readonly IEnrollmentService _enrollmentService;
        private readonly IEnrollmentNoMatchingRepository _enrollmentKTPRepository;
        private readonly ICIFService _cifService;
        private readonly IConfiguration _configuration;
        private readonly IUtilityRepository _utilityRepository;
        private readonly IProfileService _profileService;
        private readonly IProfileRepository _profileRepo;
        private readonly IImageRecognitionService _imageRecognitionService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private readonly string HostSoa = "";
        private readonly string systemId = "";
        private readonly string idType = "";
        private readonly string teller = "";
        private readonly string branch = "";
        private readonly string BaseUrlNonSoa = "";
        private readonly string EndPointNonSoa = "";
        private readonly bool IsHitSOA = false;
        private readonly string ResponseSukses = "";
        private readonly string ResponseParameterKosong = "";
        private readonly string ResponseDataKosong = "";
        private readonly string ResponseUnauthorize = "";
        private readonly string ResponseBelumEnroll = "";
        public EnrollController(IEnrollmentNoMatchingService enrollmentNoMatchingService, IEnrollmentService enrollmentService,
            IEnrollmentNoMatchingRepository enrollmentKTPRepository,
            ICIFService cifService,
            IConfiguration configuration, IUtilityRepository utilityRepository, IProfileService profileService, IProfileRepository profileRepo, IImageRecognitionService imageRecognitionService,
            IHttpContextAccessor httpContextAccessor)
        {
            _enrollmentNoMatchingService = enrollmentNoMatchingService;
            _enrollmentService = enrollmentService;
            _enrollmentKTPRepository = enrollmentKTPRepository;
            _cifService = cifService;
            _configuration = configuration;
            _utilityRepository = utilityRepository;
            _profileService = profileService;
            _profileRepo = profileRepo;
            _imageRecognitionService = imageRecognitionService;
            _httpContextAccessor = httpContextAccessor;

            HostSoa = _configuration.GetValue<string>("SOAApi:Host");
            systemId = _configuration.GetValue<string>("SOAApi:systemId");
            idType = _configuration.GetValue<string>("SOAApi:idType");
            teller = _configuration.GetValue<string>("SOAApi:teller");
            branch = _configuration.GetValue<string>("SOAApi:branch");
            IsHitSOA = _configuration.GetValue<bool>("isHitSOA");
            BaseUrlNonSoa = _configuration.GetValue<string>("NonSOAApi:base_url");
            EndPointNonSoa = _configuration.GetValue<string>("NonSOAApi:endpoint");
            ResponseSukses = _configuration.GetValue<string>("Response:Sukses");
            ResponseParameterKosong = _configuration.GetValue<string>("Response:ErrorParameterKosong");
            ResponseDataKosong = _configuration.GetValue<string>("Response:DataKosong");
            ResponseUnauthorize = _configuration.GetValue<string>("Response:Unauthorize");
            ResponseBelumEnroll = _configuration.GetValue<string>("Response:DataBelumEnroll");
            _imageRecognitionService = imageRecognitionService;
        }

        /// <summary>
        /// To Create KTP data non employee encrypted ISO Version
        /// </summary>
        /// <param name="enroll"></param>
        /// <returns></returns>
        [HttpPost("submit-finger-enc-only-iso-thirdparty")]
        [ProducesResponseType(500)]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse), 200)]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse), 204)]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse), 400)]
        [LogActivity(Keterangan = "submit-finger-enc-only-thirdparty for thirdparty")]
        public async Task<IActionResult> SubmitEnrollFingerEncOnlyFRThirdParty(EnrollKTPThirdParty2VM enroll)
        {
            string authorization = HttpContext.Request.Headers["Authorization"];

            if (string.IsNullOrWhiteSpace(authorization))
            {
                return StatusCode(StatusCodes.Status401Unauthorized, new Core.Entities.ServiceResponse<ProfileByNik>
                {
                    Message = ResponseUnauthorize,
                    Status = (int)ServiceResponseStatus.ERROR_PARAMETER,
                    Data = null
                });
            }

            var remoteIpAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();

            var TypeApps = "";
            var AppsChannel = "";

            if (HttpContext.Request.Headers.TryGetValue("Type-Aplikasi", out var headerValues))
            {
                TypeApps = headerValues.ToString();
            }
           

            var reqSOA = new ApiSOA
            {
                systemId = systemId,
                numId = enroll.KtpNIK,
                idType = idType,
                host = HostSoa,
                teller = teller,
                branch = branch,
                baseUrlNonSoa = BaseUrlNonSoa,
                UrlEndPointNonSoa = EndPointNonSoa
            };

            var token = authorization.Split(" ")[1];

            var claims = TokenManager.GetPrincipalThirdParty(token);

            if (HttpContext.Request.Headers.TryGetValue("Apps-Agent", out var headerValues2))
            {
                AppsChannel = headerValues2.ToString();
            }

            if (AppsChannel != claims.Name) {
                return StatusCode(StatusCodes.Status401Unauthorized, new Core.Entities.ServiceResponse
                {
                    Message = ResponseUnauthorize,
                    Status = (int)ServiceResponseStatus.ERROR_PARAMETER
                }); 
            }

            if (DateTime.Now > claims.ExpTime)
            {
                return StatusCode(StatusCodes.Status401Unauthorized, new Core.Entities.ServiceResponse
                {
                    Message = ResponseUnauthorize,
                    Status = (int)ServiceResponseStatus.ERROR_PARAMETER
                });
            }

            var res = await _enrollmentService.SubmitEnrollmentFingerEncryptedOnlyISOThirdParty(AppsChannel,IsHitSOA, reqSOA, enroll, remoteIpAddress);

            if (res.code == (int)EnrollStatus.Inputan_tidak_lengkap)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new Core.Entities.ServiceResponse
                {
                    Message = res.msg,
                    Status = (int)ServiceResponseStatus.ERROR_PARAMETER
                });
            }
            else {
                return StatusCode(StatusCodes.Status200OK, new Core.Entities.ServiceResponse
                {
                    Message = res.msg,
                    Status = (int)ServiceResponseStatus.SUKSES
                });
            }
        
        }
    }
}
