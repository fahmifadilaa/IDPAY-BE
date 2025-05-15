using Ekr.Api.DataFingerIso.Filters;
using Ekr.Business.Contracts.DataKTP;
using Ekr.Business.Contracts.Enrollment;
using Ekr.Core.Constant;
using Ekr.Core.Entities.Account;
using Ekr.Core.Entities.DataKTP;
using Ekr.Core.Entities.Recognition;
using Ekr.Repository.Contracts.DataMaster.Utility;
using Ekr.Repository.Contracts.Enrollment;
using Ekr.Repository.Contracts.Recognition;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ekr.Api.DataFingerIso.Controllers
{
    [Route("profile-encrypt-iso")]
	[ApiController]
	public class ProfileEncryptISOController : ControllerBase
	{
        private readonly ILogger<ProfileEncryptISOController> _logger;
        private readonly IProfileService _profileService;
        private readonly IFingerRepository _fingerRepository;
        private readonly IConfiguration _configuration;
        private readonly IUtilityRepository _utilityRepository;
        private readonly IEnrollmentService _enrollmentService;
        private readonly IEnrollmentKTPRepository _enrollmentKTPRepository;
        private readonly string BaseUrl = "";
        private readonly string EndPoint = "";
        private readonly string ResponseSukses = "";
        private readonly string ResponseParameterKosong = "";
        private readonly string ResponseDataKosong = "";
        private readonly string ResponseCIFNotFOUND = "";

        private readonly string ResponseBelumEnroll = "";

        public ProfileEncryptISOController(ILogger<ProfileEncryptISOController> logger, IProfileService profileService,
            IFingerRepository fingerRepository, IConfiguration configuration, IUtilityRepository utilityRepository, IEnrollmentService enrollmentService, IEnrollmentKTPRepository enrollmentKTPRepository)
        {
            _logger = logger;
            _profileService = profileService;
            _fingerRepository = fingerRepository;
            _utilityRepository = utilityRepository;
            _enrollmentKTPRepository = enrollmentKTPRepository;
            _enrollmentService = enrollmentService;
            _configuration = configuration;

            BaseUrl = _configuration.GetValue<string>("UrlImageRecognition:BaseUrl");
            EndPoint = _configuration.GetValue<string>("UrlImageRecognition:MatchImageBase64ISOToBase64ISO");
            ResponseSukses = _configuration.GetValue<string>("Response:Sukses");
            ResponseParameterKosong = _configuration.GetValue<string>("Response:ErrorParameterKosong");
            ResponseDataKosong = _configuration.GetValue<string>("Response:DataKosong");
            ResponseCIFNotFOUND = _configuration.GetValue<string>("Response:CIFNotFound");
            ResponseBelumEnroll = _configuration.GetValue<string>("Response:DataBelumEnroll");
        }

        /// <summary>
        /// Get data finger by Npp
        /// </summary>
        /// <param name="npp"></param>
        /// <returns></returns>
        [HttpGet("type-finger-npp-iso-db")]
        [ProducesResponseType(typeof(IEnumerable<FingerByNik>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Get Type Finger By NPP New")]
        public async Task<Core.Entities.ServiceResponse<IEnumerable<FingerByNik>>> GetTypeFingerByNpp(string npp)
        {
            var obj = await _fingerRepository.GetFingersEnrolledByNppIsoDB(npp);

            return new Core.Entities.ServiceResponse<IEnumerable<FingerByNik>>
            {
                Message = ResponseSukses,
                Status = (int)ServiceResponseStatus.SUKSES,
                Data = obj
            };
        }

        /// <summary>
        /// Get data profile if finger match without finger type
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("get-profile-loop-finger-enc-only-iso-file")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse<ProfileByNik>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Get data profile if finger match without finger type Iso File Server Vers")]
        public async Task<Core.Entities.ServiceResponse<ProfileByNik>> GetProfileAuthLoopFingerEncOnlyISO([FromBody] ProfileLoopReq req)
        {
            var RequestTime = DateTime.Now;

            var obj = await _profileService.GetAuthKTPDataFingerEncOnlyISO(req, BaseUrl, EndPoint, RequestTime, req.Npp, req.UnitCode);

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
        [HttpPost("get-profile-loop-finger-enc-only-iso-db")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse<ProfileByNik>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Get data profile if finger match without finger type Iso Db Vers")]
        public async Task<Core.Entities.ServiceResponse<ProfileByNik>> GetProfileAuthLoopFingerEncOnlyISODB([FromBody] ProfileLoopReq req)
        {
            var RequestTime = DateTime.Now;

            var obj = await _profileService.GetAuthKTPDataFingerEncOnlyISODB(req, BaseUrl, EndPoint, RequestTime, req.Npp, req.UnitCode);

            return new Core.Entities.ServiceResponse<ProfileByNik>
            {
                Message = obj.Message,
                Status = obj.Status.Equals("error") ? (int)ServiceResponseStatus.ERROR : (int)ServiceResponseStatus.SUKSES,
                Data = obj.Data
            };
        }

        /// <summary>
        /// Get data finger by Npp
        /// </summary>
        /// <param name="npp"></param>
        /// <returns></returns>
        [HttpGet("type-finger-npp-iso-file")]
        [ProducesResponseType(typeof(IEnumerable<FingerByNik>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Get Type Finger By NPP New")]
        public async Task<Core.Entities.ServiceResponse<IEnumerable<FingerByNik>>> GetTypeFingerByNppFile(string npp)
        {
            var obj = await _fingerRepository.GetFingersEnrolledByNppIsoFile(npp);

            return new Core.Entities.ServiceResponse<IEnumerable<FingerByNik>>
            {
                Message = ResponseSukses,
                Status = (int)ServiceResponseStatus.SUKSES,
                Data = obj
            };
        }

        /// <summary>
        /// Get Data Type Finger By CIF NEW
        /// </summary>
        /// <param name="cif"></param>
        /// <returns></returns>
        [HttpGet("type-finger-cif-iso-db")]
        [ProducesResponseType(typeof(IEnumerable<FingerByNik>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Get Type Finger By CIF New")]
        public async Task<Core.Entities.ServiceResponse<IEnumerable<FingerByNik>>> GetTypeFingerByCif(string cif)
        {
            var obj = await _fingerRepository.GetFingersEnrolledByCIFIsoDB(cif);

            return new Core.Entities.ServiceResponse<IEnumerable<FingerByNik>>
            {
                Message = ResponseSukses,
                Status = (int)ServiceResponseStatus.SUKSES,
                Data = obj
            };
        }

        /// <summary>
        /// Get Data Type Finger By CIF NEW
        /// </summary>
        /// <param name="cif"></param>
        /// <returns></returns>
        [HttpGet("type-finger-cif-iso-file")]
        [ProducesResponseType(typeof(IEnumerable<FingerByNik>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Get Type Finger By CIF New")]
        public async Task<Core.Entities.ServiceResponse<IEnumerable<FingerByNik>>> GetTypeFingerByCifFile(string cif)
        {
            var obj = await _fingerRepository.GetFingersEnrolledByCIFIsoFile(cif);

            return new Core.Entities.ServiceResponse<IEnumerable<FingerByNik>>
            {
                Message = ResponseSukses,
                Status = (int)ServiceResponseStatus.SUKSES,
                Data = obj
            };
        }

        /// <summary>
        /// Get Data Type Finger By NIK New
        /// </summary>
        /// <param name="nik"></param>
        /// <returns></returns>
        [HttpGet("type-finger-nik-iso-db")]
        [ProducesResponseType(typeof(IEnumerable<FingerByNik>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Get Type Finger By NIK New")]
        public async Task<Core.Entities.ServiceResponse<IEnumerable<FingerByNik>>> GetTypeFingerNik(string nik)
        {
            var obj = await _fingerRepository.GetFingersEnrolledByNikIsoDB(nik);

            return new Core.Entities.ServiceResponse<IEnumerable<FingerByNik>>
            {
                Message = ResponseSukses,
                Status = (int)ServiceResponseStatus.SUKSES,
                Data = obj
            };
        }

        /// <summary>
        /// Get Data Type Finger By NIK New
        /// </summary>
        /// <param name="nik"></param>
        /// <returns></returns>
        [HttpGet("type-finger-nik-iso-file")]
        [ProducesResponseType(typeof(IEnumerable<FingerByNik>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Get Type Finger By NIK New")]
        public async Task<Core.Entities.ServiceResponse<IEnumerable<FingerByNik>>> GetTypeFingerNikIsoFile(string nik)
        {
            var obj = await _fingerRepository.GetFingersEnrolledByNikIsoFile(nik);

            return new Core.Entities.ServiceResponse<IEnumerable<FingerByNik>>
            {
                Message = ResponseSukses,
                Status = (int)ServiceResponseStatus.SUKSES,
                Data = obj
            };
        }

        /// <summary>
        /// Get Data Type Finger By CIF ISO NEW with true/false resp only
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("finger-cif-iso-db-bit")]
        [ProducesResponseType(typeof(IEnumerable<bool>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Get Matching Finger By CIF ISO DB with true/false response only")]
        public async Task<Core.Entities.ServiceResponse<bool>> GetMatchingFingerByCifBitIso([FromBody] ProfileLoopByCifReq req)
        {
            var RequestTime = DateTime.Now;

            var obj = await _profileService.GetAuthKTPDataByCifFingerEncOnlyNew(req, BaseUrl, EndPoint, RequestTime);

            return new Core.Entities.ServiceResponse<bool>
            {
                Data = obj.Status.Equals("sukses") ? true : false,
                Message = obj.Message,
                //Status = obj.Status.Equals("error") ? (int)ServiceResponseStatus.ERROR : obj.Status.Equals("Empty") ? (int)ServiceResponseStatus.EMPTY_PARAMETER : (int)ServiceResponseStatus.SUKSES,
                Status = obj.Status.Equals("sukses") ? (int)ServiceResponseStatus.SUKSES : obj.Status.Equals("Empty") ? (int)ServiceResponseStatus.EMPTY_PARAMETER : (int)ServiceResponseStatus.ERROR,
            };
        }

        /// <summary>
        /// Get data finger by NIk Bit Response Only
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("finger-nik-iso-db-bit")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse<bool>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Get Matching Finger By NIK ISO DB with true/false response only")]
        public async Task<Core.Entities.ServiceResponse<bool>> GetMatchingFingerByNikBItIso([FromBody] ProfileLoopReq req)
        {
            var RequestTime = DateTime.Now;

            var obj = await _profileService.GetAuthKTPDataFingerEncOnlyNew(req, BaseUrl, EndPoint, RequestTime, req.Npp, req.UnitCode);

            return new Core.Entities.ServiceResponse<bool>
            {
                Data = obj.Status.Equals("sukses") ? true : false,
                Message = obj.Message,
                Status = obj.Status.Equals("sukses") ? (int)ServiceResponseStatus.SUKSES : obj.Status.Equals("Empty") ? (int)ServiceResponseStatus.EMPTY_PARAMETER : (int)ServiceResponseStatus.ERROR,
            };
        }

        /// <summary>
        /// Get data finger by NIk Bit Response Only
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("finger-npp-iso-db-bit")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse<bool>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Get Matching Finger By NPP ISO DB with true/false response only")]
        public async Task<Core.Entities.ServiceResponse<bool>> GetMatchingFingerByNppBItIso([FromBody] ProfileLoopNppReq req)
        {
            var RequestTime = DateTime.Now;
            var BaseUrl = _configuration.GetValue<string>("UrlImageRecognition:BaseUrl");
            var EndPoint = _configuration.GetValue<string>("UrlImageRecognition:MatchImageBase64ISOToBase64ISO");

            var mapping = await _enrollmentKTPRepository.MappingNppNik(req.Npp);
            if (mapping == null)
            {
                return new Core.Entities.ServiceResponse<bool>
                {
                    Message = ResponseSukses,
                    Status = (int)ServiceResponseStatus.Data_Empty,
                    Data = false,
                    Code = 0
                };
            }

            var _req = new ProfileLoopReq
            {
                Base64Img = req.Base64Img,
                Nik = mapping.Nik,
                Branch = req.Branch,
                ClientApps = req.ClientApps,
                SubBranch = req.SubBranch,
                LvTeller = req.LvTeller,
                EndPoint = req.EndPoint,
                Npp = req.NppRequester,
                UnitCode = req.UnitCode
            };

            var obj = await _profileService.IsFingerLoopMatchNew(_req, BaseUrl, EndPoint, RequestTime, req.Npp, req.UnitCode);

            return new Core.Entities.ServiceResponse<bool>
            {
                Data = obj.Status.Equals("sukses") ? true : false,
                Message = obj.Message,
                Status = obj.Status.Equals("sukses") ? (int)ServiceResponseStatus.SUKSES : obj.Status.Equals("Empty") ? (int)ServiceResponseStatus.EMPTY_PARAMETER : (int)ServiceResponseStatus.ERROR,
            };
        }

        /// <summary>
        /// Get Data Type Finger By CIF ISO NEW with true/false resp only
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("finger-cif-iso-file-bit")]
        [ProducesResponseType(typeof(IEnumerable<bool>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Get Matching Finger By CIF ISO File with true/false response only")]
        public async Task<Core.Entities.ServiceResponse<bool>> GetMatchingFingerByCifBitIsoFile([FromBody] ProfileLoopByCifReq req)
        {
            var RequestTime = DateTime.Now;

            var obj = await _profileService.GetAuthKTPDataByCifFingerEncOnlyNewFile(req, BaseUrl, EndPoint, RequestTime);

            return new Core.Entities.ServiceResponse<bool>
            {
                Data = obj.Status.Equals("sukses") ? true : false,
                Message = obj.Message,
                Status = obj.Status.Equals("sukses") ? (int)ServiceResponseStatus.SUKSES : obj.Status.Equals("Empty") ? (int)ServiceResponseStatus.EMPTY_PARAMETER : (int)ServiceResponseStatus.ERROR,
            };
        }

        /// <summary>
        /// Get data finger by NIk Bit Response Only
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("finger-nik-iso-file-bit")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse<bool>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Get Matching Finger By NIK ISO File with true/false response only")]
        public async Task<Core.Entities.ServiceResponse<bool>> GetTypeFingerByNikBItIsoFile([FromBody] ProfileLoopReq req)
        {
            var RequestTime = DateTime.Now;

            var obj = await _profileService.GetAuthKTPDataFingerEncOnlyNewFile(req, BaseUrl, EndPoint, RequestTime, req.Npp, req.UnitCode);

            return new Core.Entities.ServiceResponse<bool>
            {
                Data = obj.Status.Equals("sukses") ? true : false,
                Message = obj.Message,
                Status = obj.Status.Equals("sukses") ? (int)ServiceResponseStatus.SUKSES : obj.Status.Equals("Empty") ? (int)ServiceResponseStatus.EMPTY_PARAMETER : (int)ServiceResponseStatus.ERROR,
            };
        }

        /// <summary>
        /// Get data finger by NIk Bit Response Only
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("finger-npp-iso-file-bit")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse<bool>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Get Matching Finger By NPP ISO File with true/false response only")]
        public async Task<Core.Entities.ServiceResponse<bool>> GetMatchingFingerByNppBItIsoFile([FromBody] ProfileLoopNppReq req)
        {
            var RequestTime = DateTime.Now;
            var BaseUrl = _configuration.GetValue<string>("UrlImageRecognition:BaseUrl");
            var EndPoint = _configuration.GetValue<string>("UrlImageRecognition:MatchImageBase64ISOToBase64ISO");

            var mapping = await _enrollmentKTPRepository.MappingNppNik(req.Npp);
            if (mapping == null)
            {
                return new Core.Entities.ServiceResponse<bool>
                {
                    Message = ResponseSukses,
                    Status = (int)ServiceResponseStatus.Data_Empty,
                    Data = false,
                    Code = 0
                };
            }

            var _req = new ProfileLoopReq
            {
                Base64Img = req.Base64Img,
                Nik = mapping.Nik,
                Branch = req.Branch,
                ClientApps = req.ClientApps,
                SubBranch = req.SubBranch,
                LvTeller = req.LvTeller,
                EndPoint = req.EndPoint,
                Npp = req.NppRequester,
                UnitCode = req.UnitCode
            };

            var obj = await _profileService.IsFingerLoopMatchNewIsoFile(_req, BaseUrl, EndPoint, RequestTime, req.Npp, req.UnitCode);

            return new Core.Entities.ServiceResponse<bool>
            {
                Data = obj.Status.Equals("sukses") ? true : false,
                Message = obj.Message,
                //Status = obj.Status.Equals("error") ? (int)ServiceResponseStatus.ERROR : obj.Status.Equals("Empty") ? (int)ServiceResponseStatus.EMPTY_PARAMETER : (int)ServiceResponseStatus.SUKSES,
                Status = obj.Status.Equals("sukses") ? (int)ServiceResponseStatus.SUKSES : obj.Status.Equals("Empty") ? (int)ServiceResponseStatus.EMPTY_PARAMETER : (int)ServiceResponseStatus.ERROR,
            };
        }
        /// <summary>
        /// Get data finger by Npp
        /// </summary>
        /// <param name="npp"></param>
        /// <returns></returns>
        [HttpGet("finger-npp-iso")]
        [ProducesResponseType(typeof(IEnumerable<FingerByNik>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Get Finger")]
        public async Task<Core.Entities.ServiceResponse<IEnumerable<FingerByNik>>> GetFingerNppISO(string npp)
        {
            var obj = await _fingerRepository.GetFingersEnrolledNpp(npp);

            return new Core.Entities.ServiceResponse<IEnumerable<FingerByNik>>
            {
                Message = ResponseSukses,
                Status = (int)ServiceResponseStatus.SUKSES,
                Data = obj
            };
        }


        /// <summary>
        /// Get data finger ISO by NIK
        /// </summary>
        /// <param name="nik"></param>
        /// <returns></returns>
        [HttpGet("finger-nik-iso")]
        [ProducesResponseType(typeof(IEnumerable<FingerByNik>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Get Finger ISO By NIK")]
        public async Task<Core.Entities.ServiceResponse<IEnumerable<FingerISOByNik>>> GetFingerISOByNIK(string nik)
        {
            var obj = await _fingerRepository.GetFingersISOEnrolledNik(nik);

            if (obj == null)
            {
                return new Core.Entities.ServiceResponse<IEnumerable<FingerISOByNik>>
                {
                    Message = "NIK Belum Enroll ISO",
                    Status = (int)ServiceResponseStatus.Data_Empty,
                    Data = null
                };
            }

            return new Core.Entities.ServiceResponse<IEnumerable<FingerISOByNik>>
            {
                Message = ResponseSukses,
                Status = (int)ServiceResponseStatus.SUKSES,
                Data = obj
            };
        }

        /// <summary>
        /// To check whether iso have been inserted or not
        /// </summary>
        /// <param name="nik"></param>
        /// <returns></returns>
        [HttpGet("get-iso")]
        [ProducesResponseType(typeof(Core.Entities.Account.ServiceResponse<CekCIFDto>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "To check whether iso have been inserted or not")]
        public async Task<Core.Entities.Account.ServiceResponse<object>> GetIso(string nik)
        {

            if (string.IsNullOrEmpty(nik))
            {
                return new Core.Entities.Account.ServiceResponse<object>
                {
                    Code = (int)ServiceResponseStatus.EMPTY_PARAMETER,
                    Msg = nameof(ServiceResponseStatus.EMPTY_PARAMETER)
                };
            }

            var resp = await _enrollmentService.GetISO(nik);
            if (resp == null)
            {
                return new Core.Entities.Account.ServiceResponse<object>
                {
                    Data = resp,
                    Msg = ResponseBelumEnroll,
                    Code = (int)ServiceResponseStatus.Data_Empty,
                };
            }

            return new Core.Entities.Account.ServiceResponse<object>
            {
                Data = resp,
                Msg = ResponseSukses,
                Code = (int)ServiceResponseStatus.SUKSES,
            };
        }

        /// <summary>
        /// To check whether iso have been inserted or not
        /// </summary>
        /// <param name="npp"></param>
        /// <returns></returns>
        [HttpGet("get-iso-byNpp")]
        [ProducesResponseType(typeof(Core.Entities.Account.ServiceResponse<CekCIFDto>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "To check whether iso have been inserted or not")]
        public async Task<Core.Entities.Account.ServiceResponse<object>> GetIsoByNpp(string npp)
        {

            if (string.IsNullOrEmpty(npp))
            {
                return new Core.Entities.Account.ServiceResponse<object>
                {
                    Code = (int)ServiceResponseStatus.EMPTY_PARAMETER,
                    Msg = ResponseParameterKosong
                };
            }

            var _data = await _enrollmentKTPRepository.MappingNppNik(npp);
            if (_data == null)
            {
                return new Core.Entities.Account.ServiceResponse<object>
                {
                    Code = (int)ServiceResponseStatus.EMPTY_PARAMETER,
                    Msg = ResponseParameterKosong
                };
            }

            var resp = await _enrollmentService.GetISO(_data.Nik);
            if (resp == null)
            {
                return new Core.Entities.Account.ServiceResponse<object>
                {
                    Data = resp,
                    Msg = ResponseBelumEnroll,
                    Code = (int)ServiceResponseStatus.Data_Empty,
                };
            }

            return new Core.Entities.Account.ServiceResponse<object>
            {
                Data = resp,
                Msg = ResponseSukses,
                Code = (int)ServiceResponseStatus.SUKSES,
            };
        }
    }
}
