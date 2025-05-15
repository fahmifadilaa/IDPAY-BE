using Ekr.Api.DataEnrollment.Filters;
using Ekr.Auth;
using Ekr.Business.Contracts.DataKTP;
using Ekr.Core.Constant;
using Ekr.Core.Entities.DataKTP;
using Ekr.Core.Entities.DataMaster.Utility.Entity;
using Ekr.Core.Entities.Recognition;
using Ekr.Repository.Contracts.DataMaster.Utility;
using Ekr.Repository.Contracts.Recognition;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ekr.Api.DataEnrollment.Controllers
{
	[Route("profile-encrypt")]
    [ApiController]
    public class ProfileEncryptController : ControllerBase
    {
        private readonly ILogger<ProfileEncryptController> _logger;
        private readonly IProfileService _profileService;
        private readonly IFingerRepository _fingerRepository;
        private readonly IConfiguration _configuration;
        private readonly IUtilityRepository _utilityRepository;
        private readonly string BaseUrl = "";
        private readonly string EndPoint = "";

        public ProfileEncryptController(ILogger<ProfileEncryptController> logger, IProfileService profileService,
            IFingerRepository fingerRepository, IConfiguration configuration, IUtilityRepository utilityRepository)
        {
            _logger = logger;
            _profileService = profileService;
            _fingerRepository = fingerRepository;
            _utilityRepository = utilityRepository;
            _configuration = configuration;

            BaseUrl = _configuration.GetValue<string>("UrlImageRecognition:BaseUrl");
            EndPoint = _configuration.GetValue<string>("UrlImageRecognition:MatchImageBase64ToBase64");
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
            var obj = await _profileService.GetAuthKTPData(req.Base64Img, req.Nik, req.FingerType, BaseUrl, EndPoint);

            return new Core.Entities.ServiceResponse<ProfileByNik>
            {
                Message = obj.Message,
                Status = obj.Status.Equals("error") ? (int)ServiceResponseStatus.ERROR : (int)ServiceResponseStatus.SUKSES,
                Data = obj.Data
            };
        }

        /// <summary>
        /// Get data profile if finger match
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("get-profile-finger-enc-only")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse<ProfileByNik>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Get data profile if finger match")]
        public async Task<Core.Entities.ServiceResponse<ProfileByNik>> GetProfileFingerEncOnly([FromBody] ProfileReq req)
        {
            var obj = await _profileService.GetAuthKTPDataFingerEncOnly(req.Base64Img, req.Nik, req.FingerType, BaseUrl, EndPoint);

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
            var obj = await _profileService.GetAuthKTPData(req.Base64Img, req.Nik, BaseUrl, EndPoint);

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
        [HttpPost("get-profile-loop-finger-enc-only")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse<ProfileByNik>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Get data profile if finger match without finger type")]
        public async Task<Core.Entities.ServiceResponse<ProfileByNik>> GetProfileAuthLoopFingerEncOnly([FromBody] ProfileLoopReq req)
        {
            var RequestTime = DateTime.Now;

			var obj = await _profileService.GetAuthKTPDataFingerEncOnly(req, BaseUrl, EndPoint, RequestTime, req.Npp, req.UnitCode);

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
        [HttpPost("get-profile-loop-finger-enc-only-compressed")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse<ProfileByNik>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Get data profile if finger match without finger type")]
        public async Task<Core.Entities.ServiceResponse<ProfileByNik>> GetProfileAuthLoopFingerEncOnlyCompressed([FromBody] ProfileLoopReq req)
        {
            var RequestTime = DateTime.Now;

            var obj = await _profileService.GetAuthKTPDataFingerEncOnlyCompressed(req, BaseUrl, EndPoint, RequestTime, req.Npp, req.UnitCode);

            return new Core.Entities.ServiceResponse<ProfileByNik>
            {
                Message = obj.Message,
                Status = obj.Status.Equals("error") ? (int)ServiceResponseStatus.ERROR : (int)ServiceResponseStatus.SUKSES,
                Data = obj.Data
            };
        }


        /// <summary>
        /// Get data profile by cif if finger match without finger type
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("get-profile-by-cif-loop-finger-enc-only")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse<ProfileByNik>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Get data profile by cif if finger match without finger type")]
        public async Task<Core.Entities.ServiceResponse<ProfileByNik>> GetProfileByCifAuthLoopFingerEncOnly([FromBody] ProfileLoopByCifReq req)
        {
            var requestTime = DateTime.Now;

            var obj = await _profileService.GetAuthKTPDataByCifFingerEncOnly(req, BaseUrl, EndPoint, requestTime);

            return new Core.Entities.ServiceResponse<ProfileByNik>
            {
                Message = obj.Message,
                Status = obj.Status.Equals("error") ? (int)ServiceResponseStatus.ERROR : obj.Status.Equals("Empty") ? (int)ServiceResponseStatus.EMPTY_PARAMETER : (int)ServiceResponseStatus.SUKSES,
                //Data = obj.Data
            };
        }

        /// <summary>
        /// Get data finger by NIk
        /// </summary>
        /// <param name="nik"></param>
        /// <returns></returns>
        [HttpGet("finger")]
        [ProducesResponseType(typeof(IEnumerable<FingerByNik>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Get Finger")]
        public async Task<Core.Entities.ServiceResponse<IEnumerable<FingerByNik>>> GetFinger(string nik)
        {
            var obj = await _fingerRepository.GetFingersEnrolled(nik);

            return new Core.Entities.ServiceResponse<IEnumerable<FingerByNik>>
            {
                Message = nameof(ServiceResponseStatus.SUKSES),
                Status = (int)ServiceResponseStatus.SUKSES,
                Data = obj
            };
        }

        /// <summary>
        /// Get data finger by Npp
        /// </summary>
        /// <param name="npp"></param>
        /// <returns></returns>
        [HttpGet("finger-npp")]
        [ProducesResponseType(typeof(IEnumerable<FingerByNik>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Get Finger")]
        public async Task<Core.Entities.ServiceResponse<IEnumerable<FingerByNik>>> GetFingerNpp(string npp)
        {
            var obj = await _fingerRepository.GetFingersEnrolledNpp(npp);

            return new Core.Entities.ServiceResponse<IEnumerable<FingerByNik>>
            {
                Message = nameof(ServiceResponseStatus.SUKSES),
                Status = (int)ServiceResponseStatus.SUKSES,
                Data = obj
            };
        }

        /// <summary>
        /// Get data finger by NIk
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("finger-nik-emp-bit")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse<bool>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Get Finger employee bool response")]
        public async Task<Core.Entities.ServiceResponse<bool>> GetFinger([FromBody] ProfileLoopReq req)
        {
            var RequestTime = DateTime.Now;

            string authorization = HttpContext.Request.Headers["Authorization"];

            var obj = await _profileService.GetAuthKTPDataFingerEncOnly(req, BaseUrl, EndPoint, RequestTime, req.Npp, req.UnitCode);

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
        [HttpPost("finger-nik-bit")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse<bool>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Get Finger By NIk with true/false response only")]
        public async Task<Core.Entities.ServiceResponse<bool>> GetFingerByNikBItRespOnly([FromBody] ProfileLoopReq req)
        {
            var RequestTime = DateTime.Now;

            var obj = await _profileService.GetAuthKTPDataFingerEncOnly(req, BaseUrl, EndPoint, RequestTime, req.Npp, req.UnitCode);

            return new Core.Entities.ServiceResponse<bool>
            {
                Data = obj.Status.Equals("sukses") ? true : false,
                Message = obj.Message,
                Status = obj.Status.Equals("sukses") ? (int)ServiceResponseStatus.SUKSES : obj.Status.Equals("Empty") ? (int)ServiceResponseStatus.EMPTY_PARAMETER : (int)ServiceResponseStatus.ERROR,
            };
        }

        /// <summary>
        /// Get data finger by Cif
        /// </summary>
        /// <param name="cif"></param>
        /// <returns></returns>
        [HttpGet("finger-cif")]
        [ProducesResponseType(typeof(IEnumerable<FingerByNik>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Get Finger By Cif")]
        public async Task<Core.Entities.ServiceResponse<IEnumerable<FingerByNik>>> GetFingerByCif(string cif)
        {
            //var decryptedNik = StringCipher.Decrypt(nik, Phrase.ParameterSecurity);

            var obj = await _fingerRepository.GetFingersEnrolledByCIF(cif);

            return new Core.Entities.ServiceResponse<IEnumerable<FingerByNik>>
            {
                Message = nameof(ServiceResponseStatus.SUKSES),
                Status = (int)ServiceResponseStatus.SUKSES,
                Data = obj
            };
        }


        /// <summary>
        /// Get data finger by Cif with true/false resp only
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("finger-cif-bit")]
        [ProducesResponseType(typeof(IEnumerable<bool>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Get Finger By Cif with true/false response only")]
        public async Task<Core.Entities.ServiceResponse<bool>> GetFingerByCifBitRespOnly([FromBody] ProfileLoopByCifReq req)
        {
            var RequestTime = DateTime.Now;

            var obj = await _profileService.GetAuthKTPDataByCifFingerEncOnly(req, BaseUrl, EndPoint, RequestTime);

            return new Core.Entities.ServiceResponse<bool>
            {
                Data = obj.Status.Equals("sukses")? true : false,
                Message = obj.Message,
                Status = obj.Status.Equals("sukses") ? (int)ServiceResponseStatus.SUKSES : obj.Status.Equals("Empty") ? (int)ServiceResponseStatus.EMPTY_PARAMETER : (int)ServiceResponseStatus.ERROR,
            };
        }

        /// <summary>
        /// Get data finger by Cif with true/false resp only
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("finger-cif-emp-bit")]
        [ProducesResponseType(typeof(IEnumerable<bool>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Get Finger Employee By Cif with true/false response only")]
        public async Task<Core.Entities.ServiceResponse<bool>> GetFingerEmpByCifBitRespOnly([FromBody] ProfileLoopByCifReq req)
        {
            var obj = await _profileService.GetEmpAuthKTPDataByCifFingerEncOnly(req.Base64Img, req.Cif, BaseUrl, EndPoint);

            return new Core.Entities.ServiceResponse<bool>
            {
                Data = obj.Status.Equals("sukses") ? true : false,
                Message = obj.Message,
                Status = obj.Status.Equals("sukses") ? (int)ServiceResponseStatus.SUKSES : obj.Status.Equals("Empty") ? (int)ServiceResponseStatus.EMPTY_PARAMETER : (int)ServiceResponseStatus.ERROR,
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
        /// Get data profile by NIK
        /// </summary>
        /// <param name="nik"></param>
        /// <returns></returns>
        [HttpGet("get-profile-bynik-finger-enc-only")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse<ProfileByNik>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Get data profile by NIK")]
        public async Task<Core.Entities.ServiceResponse<ProfileByNik>> GetProfileFingerEncOnly(string nik, string? IpAddress, string? Browser, string? Url)
        {
            if (nik == null)
            {
                return new Core.Entities.ServiceResponse<ProfileByNik>
                {
                    Message = nameof(ServiceResponseStatus.EMPTY_PARAMETER),
                    Status = (int)ServiceResponseStatus.EMPTY_PARAMETER
                };
            }
            var Npp = "";
            try
			{
                string authorization = HttpContext.Request.Headers["Authorization"];
               
                if (!string.IsNullOrWhiteSpace(authorization))
                {
                    var token = authorization.Split(" ")[1];

                    var claims = TokenManager.GetPrincipal(token);

                    Npp = claims.NIK;
                }
            }
			catch (Exception)
			{
                Npp = "";
            }

            var _filter = new Tbl_LogNIKInquiry
            {
                Nik = nik,
                Npp = Npp,
                Action = "Search",
                SearchParam = JsonConvert.SerializeObject(nik),
                Browser = Browser == null ? "" : Browser,
                IpAddress = IpAddress == null ? "" : IpAddress,
                Url = Url == null ? "" : Url,
                CreatedTime = DateTime.Now
            };

            var _ = _utilityRepository.InsertLogNIKInquiry(_filter);

            var obj = await _profileService.GetKTPDataFingerEncOnly(nik);

            if (obj.Data == null)
            {
                return new Core.Entities.ServiceResponse<ProfileByNik>
                {
                    Message = "Tidak Ada Data",
                    Status = (int)ServiceResponseStatus.Data_Empty,
                    Data = null
                };
            }

            return new Core.Entities.ServiceResponse<ProfileByNik>
            {
                Message = obj.Message,
                Status = obj.Status.Equals("error") ? (int)ServiceResponseStatus.ERROR : (int)ServiceResponseStatus.SUKSES,
                Data = obj.Data
            };
        }

        /// <summary>
        /// Get data profile by CIF
        /// </summary>
        /// <param name="cif"></param>
        /// <returns></returns>
        [HttpGet("get-profile-by-cif-finger-enc-only")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse<ProfileByNik>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Get data profile by cif")]
        public async Task<Core.Entities.ServiceResponse<ProfileByNik>> GetProfileByCifFingerEncOnly(string cif)
        {
            var obj = await _profileService.GetKTPDataByCifFingerEncOnly(cif);
            
            if(obj.Data == null)
            {
                return new Core.Entities.ServiceResponse<ProfileByNik>
                {
                    Message = "Tidak Ada Data",
                    Status = (int)ServiceResponseStatus.Data_Empty,
                    Data = null
                };
            }

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
