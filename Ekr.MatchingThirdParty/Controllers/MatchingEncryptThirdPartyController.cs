using Ekr.Business.Contracts.DataKTP;
using Ekr.Repository.Contracts.DataMaster.Utility;
using Ekr.Repository.Contracts.Recognition;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Ekr.Core.Constant;
using Ekr.Core.Entities.DataKTP;
using System.Threading.Tasks;
using System;
using Ekr.MatchingThirdParty.Filters;
using Ekr.Core.Entities.Recognition;
using System.Collections.Generic;
using Ekr.Repository.Contracts.Enrollment;
using Ekr.Auth;
using NPOI.POIFS.Crypt.Dsig;

namespace Ekr.MatchingThirdParty.Controllers
{
    [Route("matching-thirdparty")]
    [ApiController]
    public class MatchingEncryptThirdPartyController : ControllerBase
    {

        private readonly ILogger<MatchingEncryptThirdPartyController> _logger;
        private readonly IProfileService _profileService;
        private readonly IFingerRepository _fingerRepository;
        private readonly IConfiguration _configuration;
        private readonly IUtilityRepository _utilityRepository;
        private readonly IEnrollmentKTPRepository _enrollmentRepository;
        private readonly string BaseUrl = "";
        private readonly string EndPoint = "";
        private readonly string EndPointIso = "";
        private readonly string ResponseUnauthorize = "";

        public MatchingEncryptThirdPartyController(ILogger<MatchingEncryptThirdPartyController> logger, IProfileService profileService,
            IFingerRepository fingerRepository, IConfiguration configuration, IUtilityRepository utilityRepository, IEnrollmentKTPRepository enrollmentRepository)
        {
            _logger = logger;
            _profileService = profileService;
            _fingerRepository = fingerRepository;
            _utilityRepository = utilityRepository;
            _configuration = configuration;
            _enrollmentRepository = enrollmentRepository;

            BaseUrl = _configuration.GetValue<string>("UrlImageRecognition:BaseUrl");
            EndPoint = _configuration.GetValue<string>("UrlImageRecognition:MatchImageBase64ToBase64");
            EndPointIso = _configuration.GetValue<string>("UrlImageRecognition:MatchImageBase64ISOToBase64ISO");
            ResponseUnauthorize = _configuration.GetValue<string>("Response:Unauthorize");
        }

        #region Get full profile by nik iso
        /// <summary>
        /// Get data profile if finger match without finger type
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("get-profile-finger-enc-nik-iso")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse<ProfileByNik>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Get data profile if finger match without finger type Iso File Server NIK Vers For Third Party")]
        public async Task<IActionResult> GetProfileAuthLoopFingerEncOnlyNIKISO([FromBody] ProfileLoopNIKThirdPartyReq req)
        {
            string authorization = HttpContext.Request.Headers["Authorization"];

            var TypeApps = "";
            var AppsChannel = "";

            if (HttpContext.Request.Headers.TryGetValue("Type-Aplikasi", out var headerValues))
            {
                TypeApps = headerValues.ToString();
            }
            var token = authorization.Split(" ")[1];

            var claims = TokenManager.GetPrincipalThirdParty(token);

            if (HttpContext.Request.Headers.TryGetValue("Apps-Agent", out var headerValues2))
            {
                AppsChannel = headerValues2.ToString();
            }

            if (AppsChannel != claims.Name)
            {
                return StatusCode(StatusCodes.Status401Unauthorized, new Core.Entities.ServiceResponse<ProfileByNik>
                {
                    Message = ResponseUnauthorize,
                    Status = (int)ServiceResponseStatus.ERROR_PARAMETER,
                    Data = null
                });
            }

            var RequestTime = DateTime.Now;

            var _req = new ProfileLoopReq
            {
                Branch = req.Branch,
                Base64Img = req.Base64Img,
                ClientApps = req.ClientApps,
                //EndPoint = req.EndPoint,
                LvTeller = req.LvTeller,
                Nik = req.Nik,
                Npp = null,
                SubBranch = req.SubBranch,
                UnitCode = req.UnitCode
            };

            var obj = await _profileService.GetAuthKTPDataFingerEncOnlyISOThirdParty(_req, BaseUrl, EndPointIso, RequestTime, req.UnitCode);

            return StatusCode(StatusCodes.Status200OK, new Core.Entities.ServiceResponse<ProfileByNik>
            {
                Message = obj.Message,
                Status = obj.Status.Equals("error") ? (int)ServiceResponseStatus.ERROR : (int)ServiceResponseStatus.SUKSES,
                Data = obj.Data
            });
        }
        #endregion

        #region Get full profile by cif iso
        /// <summary>
        /// Get data profile if finger match without finger type
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("get-profile-finger-enc-cif-iso")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse<ProfileByNik>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Get data profile if finger match without finger type Iso File Server CIF Vers For Third Party")]
        public async Task<IActionResult> GetProfileAuthLoopFingerEncOnlyCIFISO([FromBody] ProfileLoopCifThirdPartyReq req)
        {
            string authorization = HttpContext.Request.Headers["Authorization"];

            var TypeApps = "";
            var AppsChannel = "";

            if (HttpContext.Request.Headers.TryGetValue("Type-Aplikasi", out var headerValues))
            {
                TypeApps = headerValues.ToString();
            }
            var token = authorization.Split(" ")[1];

            var claims = TokenManager.GetPrincipalThirdParty(token);

            if (HttpContext.Request.Headers.TryGetValue("Apps-Agent", out var headerValues2))
            {
                AppsChannel = headerValues2.ToString();
            }

            if (AppsChannel != claims.Name)
            {
                return StatusCode(StatusCodes.Status401Unauthorized, new Core.Entities.ServiceResponse<ProfileByNik>
                {
                    Message = ResponseUnauthorize,
                    Status = (int)ServiceResponseStatus.ERROR_PARAMETER,
                    Data = null
                });
            }

            string Nik = await _enrollmentRepository.GetNikByCif(req.Cif);
            if(Nik == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new Core.Entities.ServiceResponse<ProfileByNik>
                {
                    Message = "Data Not Found",
                    Status = (int)ServiceResponseStatus.Data_Empty,
                    Data = null
                });
            }

            var isEmployee = await _enrollmentRepository.IsEmployee(Nik);

            var _req = new ProfileLoopReq
            {
                Branch = req.Branch,
                Base64Img = req.Base64Img,
                ClientApps = req.ClientApps,
                //EndPoint = req.EndPoint,
                LvTeller = req.LvTeller,
                Nik = Nik,
                Npp = isEmployee?.Npp,
                SubBranch = req.SubBranch,
                UnitCode = req.UnitCode
            };

            var RequestTime = DateTime.Now;

            var obj = await _profileService.GetAuthKTPDataFingerEncOnlyISOThirdParty(_req, BaseUrl, EndPoint, RequestTime, req.UnitCode);

            return StatusCode(StatusCodes.Status200OK, new Core.Entities.ServiceResponse<ProfileByNik>
            {
                Message = obj.Message,
                Status = obj.Status.Equals("error") ? (int)ServiceResponseStatus.ERROR : (int)ServiceResponseStatus.SUKSES,
                Data = obj.Data
            });
        }
        #endregion

        #region Get full profile by npp iso
        /// <summary>
        /// Get data profile if finger match without finger type
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("get-profile-finger-enc-npp-iso")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse<ProfileByNik>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Get data profile if finger match without finger type Iso File Server Npp Vers For Third Party")]
        public async Task<IActionResult> GetProfileAuthLoopFingerEncOnlyNppISO([FromBody] ProfileLoopNppThirdPartyReq req)
        {
            string authorization = HttpContext.Request.Headers["Authorization"];

            var TypeApps = "";
            var AppsChannel = "";

            if (HttpContext.Request.Headers.TryGetValue("Type-Aplikasi", out var headerValues))
            {
                TypeApps = headerValues.ToString();
            }
            var token = authorization.Split(" ")[1];

            var claims = TokenManager.GetPrincipalThirdParty(token);

            if (HttpContext.Request.Headers.TryGetValue("Apps-Agent", out var headerValues2))
            {
                AppsChannel = headerValues2.ToString();
            }

            if (AppsChannel != claims.Name)
            {
                return StatusCode(StatusCodes.Status401Unauthorized, new Core.Entities.ServiceResponse<ProfileByNik>
                {
                    Message = ResponseUnauthorize,
                    Status = (int)ServiceResponseStatus.ERROR_PARAMETER,
                    Data = null
                });
            }

            var _data = await _enrollmentRepository.MappingNppNik(req.Npp);
            if (_data == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new Core.Entities.ServiceResponse<ProfileByNik>
                {
                    Message = "Data Not Found",
                    Status = (int)ServiceResponseStatus.Data_Empty,
                    Data = null
                });
            }

            var _req = new ProfileLoopReq
            {
                Branch = req.Branch,
                Base64Img = req.Base64Img,
                ClientApps = req.ClientApps,
                //EndPoint = req.EndPoint,
                LvTeller = req.LvTeller,
                Nik = _data.Nik,
                Npp = req.Npp,
                SubBranch = req.SubBranch,
                UnitCode = req.UnitCode
            };

            var RequestTime = DateTime.Now;

            var obj = await _profileService.GetAuthKTPDataFingerEncOnlyISOThirdParty(_req, BaseUrl, EndPoint, RequestTime, req.UnitCode);

            return StatusCode(StatusCodes.Status200OK, new Core.Entities.ServiceResponse<ProfileByNik>
            {
                Message = obj.Message,
                Status = obj.Status.Equals("error") ? (int)ServiceResponseStatus.ERROR : (int)ServiceResponseStatus.SUKSES,
                Data = obj.Data
            });
        }
        #endregion

        #region get boolean by nik iso
        /// <summary>
        /// Get boolean result type if finger match without finger type
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("get-profile-finger-enc-nik-iso-bool")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse<bool>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Get boolean result type finger match without finger type Iso File Server NIK Vers For Third Party")]
        public async Task<IActionResult> GetProfileAuthLoopFingerEncOnlyNIKISOBool([FromBody] ProfileLoopNIKThirdPartyReq req)
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

            var TypeApps = "";
            var AppsChannel = "";

            if (HttpContext.Request.Headers.TryGetValue("Type-Aplikasi", out var headerValues))
            {
                TypeApps = headerValues.ToString();
            }
            var token = authorization.Split(" ")[1];

            var claims = TokenManager.GetPrincipalThirdParty(token);

            if (HttpContext.Request.Headers.TryGetValue("Apps-Agent", out var headerValues2))
            {
                AppsChannel = headerValues2.ToString();
            }

            if (AppsChannel != claims.Name)
            {
                return StatusCode(StatusCodes.Status401Unauthorized, new Core.Entities.ServiceResponse<ProfileByNik>
                {
                    Message = ResponseUnauthorize,
                    Status = (int)ServiceResponseStatus.ERROR_PARAMETER,
                    Data = null
                });
            }

            var RequestTime = DateTime.Now;

            var _req = new ProfileLoopReq
            {
                Branch = req.Branch,
                Base64Img = req.Base64Img,
                ClientApps = req.ClientApps,
                //EndPoint = req.EndPoint,
                LvTeller = req.LvTeller,
                Nik = req.Nik,
                Npp = null,
                SubBranch = req.SubBranch,
                UnitCode = req.UnitCode
            };
            
            //var obj = await _profileService.GetAuthKTPDataFingerEncOnlyISOThirdParty(_req, BaseUrl, EndPoint, RequestTime, req.UnitCode);
            var obj = await _profileService.GetAuthKTPDataFingerEncOnlyISOThirdParty(_req, BaseUrl, EndPointIso, RequestTime, req.UnitCode);

            if (obj != null && obj.Status == "sukses")
            {
                return StatusCode(StatusCodes.Status200OK, new Core.Entities.ServiceResponseResult<bool>
                {
                    Message = obj.Message,
                    Status = (int)ServiceResponseStatus.SUKSES,
                    Result = true
                });
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Core.Entities.ServiceResponseResult<bool>
                {
                    Message = obj.Message,
                    Status = (int)ServiceResponseStatus.ERROR,
                    Result = false
                });
            }
        }
        #endregion

        #region get boolean by cif iso
        /// <summary>
        /// Get boolean result type if finger match without finger type
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("get-profile-finger-enc-cif-iso-bool")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse<ProfileByNik>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Get boolean result type finger match without finger type Iso File Server CIF Vers For Third Party")]
        public async Task<IActionResult> GetProfileAuthLoopFingerEncOnlyCIFISOBool([FromBody] ProfileLoopCifThirdPartyReq req)
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

            var TypeApps = "";
            var AppsChannel = "";

            if (HttpContext.Request.Headers.TryGetValue("Type-Aplikasi", out var headerValues))
            {
                TypeApps = headerValues.ToString();
            }
            var token = authorization.Split(" ")[1];

            var claims = TokenManager.GetPrincipalThirdParty(token);

            if (HttpContext.Request.Headers.TryGetValue("Apps-Agent", out var headerValues2))
            {
                AppsChannel = headerValues2.ToString();
            }

            if (AppsChannel != claims.Name)
            {
                return StatusCode(StatusCodes.Status401Unauthorized, new Core.Entities.ServiceResponse<ProfileByNik>
                {
                    Message = ResponseUnauthorize,
                    Status = (int)ServiceResponseStatus.ERROR_PARAMETER,
                    Data = null
                });
            }

            string Nik = await _enrollmentRepository.GetNikByCif(req.Cif);
            if (Nik == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new Core.Entities.ServiceResponse<bool>
                {
                    Message = "Data Not Found",
                    Status = (int)ServiceResponseStatus.Data_Empty,
                    Data = false
                });
            }

            var isEmployee = await _enrollmentRepository.IsEmployee(Nik);

            var _req = new ProfileLoopReq
            {
                Branch = req.Branch,
                Base64Img = req.Base64Img,
                ClientApps = req.ClientApps,
                //EndPoint = req.EndPoint,
                LvTeller = req.LvTeller,
                Nik = Nik,
                Npp = isEmployee?.Npp,
                SubBranch = req.SubBranch,
                UnitCode = req.UnitCode
            };

            var RequestTime = DateTime.Now;

            //var obj = await _profileService.GetAuthKTPDataFingerEncOnlyISOThirdParty(_req, BaseUrl, EndPoint, RequestTime, req.UnitCode);
            var obj = await _profileService.GetAuthKTPDataFingerEncOnlyISOThirdParty(_req, BaseUrl, EndPointIso, RequestTime, req.UnitCode);

            if (obj != null && obj.Status == "sukses")
            {
                return StatusCode(StatusCodes.Status200OK, new Core.Entities.ServiceResponseResult<bool>
                {
                    Message = obj.Message,
                    Status = (int)ServiceResponseStatus.SUKSES,
                    Result = true
                });
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Core.Entities.ServiceResponseResult<bool>
                {
                    Message = obj.Message,
                    Status = (int)ServiceResponseStatus.ERROR,
                    Result = false
                });
            }
        }
        #endregion

        #region get boolean by npp iso
        /// <summary>
        /// Get boolean result type if finger match without finger type
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("get-profile-finger-enc-npp-iso-bool")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse<ProfileByNik>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Get boolean result type finger match without finger type Iso File Server Npp Vers For Third Party")]
        public async Task<IActionResult> GetProfileAuthLoopFingerEncOnlyNppISOBool([FromBody] ProfileLoopNppThirdPartyReq req)
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

            var TypeApps = "";
            var AppsChannel = "";

            if (HttpContext.Request.Headers.TryGetValue("Type-Aplikasi", out var headerValues))
            {
                TypeApps = headerValues.ToString();
            }
            var token = authorization.Split(" ")[1];

            var claims = TokenManager.GetPrincipalThirdParty(token);

            if (HttpContext.Request.Headers.TryGetValue("Apps-Agent", out var headerValues2))
            {
                AppsChannel = headerValues2.ToString();
            }

            if (AppsChannel != claims.Name)
            {
                return StatusCode(StatusCodes.Status401Unauthorized, new Core.Entities.ServiceResponse<ProfileByNik>
                {
                    Message = ResponseUnauthorize,
                    Status = (int)ServiceResponseStatus.ERROR_PARAMETER,
                    Data = null
                });
            }

            var _data = await _enrollmentRepository.MappingNppNik(req.Npp);
            if (_data == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new Core.Entities.ServiceResponse<ProfileByNik>
                {
                    Message = "Data Not Found",
                    Status = (int)ServiceResponseStatus.Data_Empty,
                    Data = null
                });
            }

            var _req = new ProfileLoopReq
            {
                Branch = req.Branch,
                Base64Img = req.Base64Img,
                ClientApps = req.ClientApps,
                //EndPoint = req.EndPoint,
                LvTeller = req.LvTeller,
                Nik = _data.Nik,
                Npp = req.Npp,
                SubBranch = req.SubBranch,
                UnitCode = req.UnitCode
            };

            var RequestTime = DateTime.Now;

            //var obj = await _profileService.GetAuthKTPDataFingerEncOnlyISOThirdParty(_req, BaseUrl, EndPoint, RequestTime, req.UnitCode);
            var obj = await _profileService.GetAuthKTPDataFingerEncOnlyISOThirdParty(_req, BaseUrl, EndPointIso, RequestTime, req.UnitCode);

            if (obj != null && obj.Status == "sukses")
            {
                return StatusCode(StatusCodes.Status200OK, new Core.Entities.ServiceResponseResult<bool>
                {
                    Message = obj.Message,
                    Status = (int)ServiceResponseStatus.SUKSES,
                    Result = true
                });
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new Core.Entities.ServiceResponseResult<bool>
                {
                    Message = obj.Message,
                    Status = (int)ServiceResponseStatus.ERROR,
                    Result = false
                });
            }
        }
        #endregion

        #region Get Demographic Profile by NIK ISO
        /// <summary>
        /// Get data profile if finger match without finger type
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("get-profile-finger-enc-nik-iso-demo")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse<ProfileByNik>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Get data profile if finger match without finger type Iso File Server NIK Vers For Third Party")]
        public async Task<IActionResult> GetProfileAuthLoopFingerEncOnlyNIKISODemo([FromBody] ProfileLoopNIKThirdPartyReq req)
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

            var TypeApps = "";
            var AppsChannel = "";

            if (HttpContext.Request.Headers.TryGetValue("Type-Aplikasi", out var headerValues))
            {
                TypeApps = headerValues.ToString();
            }
            var token = authorization.Split(" ")[1];

            var claims = TokenManager.GetPrincipalThirdParty(token);

            if (HttpContext.Request.Headers.TryGetValue("Apps-Agent", out var headerValues2))
            {
                AppsChannel = headerValues2.ToString();
            }

            if (AppsChannel != claims.Name)
            {
                return StatusCode(StatusCodes.Status401Unauthorized, new Core.Entities.ServiceResponse<ProfileByNik>
                {
                    Message = ResponseUnauthorize,
                    Status = (int)ServiceResponseStatus.ERROR_PARAMETER,
                    Data = null
                });
            }

            var RequestTime = DateTime.Now;

            var _req = new ProfileLoopReq
            {
                Branch = req.Branch,
                Base64Img = req.Base64Img,
                ClientApps = req.ClientApps,
                //EndPoint = req.EndPoint,
                LvTeller = req.LvTeller,
                Nik = req.Nik,
                Npp = null,
                SubBranch = req.SubBranch,
                UnitCode = req.UnitCode
            };

            var obj = await _profileService.GetAuthKTPDataFingerEncOnlyISOThirdPartyDemo(_req, BaseUrl, EndPointIso, RequestTime, req.UnitCode);

            return StatusCode(StatusCodes.Status200OK, new Core.Entities.ServiceResponse<ProfileByNikOnlyImg>
            {
                Message = obj.Message,
                Status = obj.Status.Equals("error") ? (int)ServiceResponseStatus.ERROR : (int)ServiceResponseStatus.SUKSES,
                Data = obj.Data
            });
        }
        #endregion

        #region Get Demographic Profile by CIF ISO
        /// <summary>
        /// Get data profile if finger match without finger type
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("get-profile-finger-enc-cif-iso-demo")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse<ProfileByNik>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Get data profile if finger match without finger type Iso File Server CIF Vers For Third Party")]
        public async Task<IActionResult> GetProfileAuthLoopFingerEncOnlyCIFISODemo([FromBody] ProfileLoopCifThirdPartyReq req)
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

            var TypeApps = "";
            var AppsChannel = "";

            if (HttpContext.Request.Headers.TryGetValue("Type-Aplikasi", out var headerValues))
            {
                TypeApps = headerValues.ToString();
            }
            var token = authorization.Split(" ")[1];

            var claims = TokenManager.GetPrincipalThirdParty(token);

            if (HttpContext.Request.Headers.TryGetValue("Apps-Agent", out var headerValues2))
            {
                AppsChannel = headerValues2.ToString();
            }

            if (AppsChannel != claims.Name)
            {
                return StatusCode(StatusCodes.Status401Unauthorized, new Core.Entities.ServiceResponse<ProfileByNik>
                {
                    Message = ResponseUnauthorize,
                    Status = (int)ServiceResponseStatus.ERROR_PARAMETER,
                    Data = null
                });
            }

            string Nik = await _enrollmentRepository.GetNikByCif(req.Cif);
            if (Nik == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new Core.Entities.ServiceResponse<ProfileByNik>
                {
                    Message = "Data Not Found",
                    Status = (int)ServiceResponseStatus.Data_Empty,
                    Data = null
                });
            }

            var isEmployee = await _enrollmentRepository.IsEmployee(Nik);

            var _req = new ProfileLoopReq
            {
                Branch = req.Branch,
                Base64Img = req.Base64Img,
                ClientApps = req.ClientApps,
                //EndPoint = req.EndPoint,
                LvTeller = req.LvTeller,
                Nik = Nik,
                Npp = isEmployee?.Npp,
                SubBranch = req.SubBranch,
                UnitCode = req.UnitCode
            };

            var RequestTime = DateTime.Now;

            var obj = await _profileService.GetAuthKTPDataFingerEncOnlyISOThirdPartyDemo(_req, BaseUrl, EndPointIso, RequestTime, req.UnitCode);

            return StatusCode(StatusCodes.Status200OK, new Core.Entities.ServiceResponse<ProfileByNikOnlyImg>
            {
                Message = obj.Message,
                Status = obj.Status.Equals("error") ? (int)ServiceResponseStatus.ERROR : (int)ServiceResponseStatus.SUKSES,
                Data = obj.Data
            });
        }
        #endregion

        #region Get Demographic Profile by NPP ISO
        /// <summary>
        /// Get data profile if finger match without finger type
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("get-profile-finger-enc-npp-iso-demo")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse<ProfileByNik>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Get data profile if finger match without finger type Iso File Server Npp Vers For Third Party")]
        public async Task<IActionResult> GetProfileAuthLoopFingerEncOnlyNppISODemo([FromBody] ProfileLoopNppThirdPartyReq req)
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

            var TypeApps = "";
            var AppsChannel = "";

            if (HttpContext.Request.Headers.TryGetValue("Type-Aplikasi", out var headerValues))
            {
                TypeApps = headerValues.ToString();
            }
            var token = authorization.Split(" ")[1];

            var claims = TokenManager.GetPrincipalThirdParty(token);

            if (HttpContext.Request.Headers.TryGetValue("Apps-Agent", out var headerValues2))
            {
                AppsChannel = headerValues2.ToString();
            }

            if (AppsChannel != claims.Name)
            {
                return StatusCode(StatusCodes.Status401Unauthorized, new Core.Entities.ServiceResponse<ProfileByNik>
                {
                    Message = ResponseUnauthorize,
                    Status = (int)ServiceResponseStatus.ERROR_PARAMETER,
                    Data = null
                });
            }

            var _data = await _enrollmentRepository.MappingNppNik(req.Npp);
            if (_data == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new Core.Entities.ServiceResponse<ProfileByNik>
                {
                    Message = "Data Not Found",
                    Status = (int)ServiceResponseStatus.Data_Empty,
                    Data = null
                });
            }

            var _req = new ProfileLoopReq
            {
                Branch = req.Branch,
                Base64Img = req.Base64Img,
                ClientApps = req.ClientApps,
                //EndPoint = req.EndPoint,
                LvTeller = req.LvTeller,
                Nik = _data.Nik,
                Npp = req.Npp,
                SubBranch = req.SubBranch,
                UnitCode = req.UnitCode
            };

            var RequestTime = DateTime.Now;

            var obj = await _profileService.GetAuthKTPDataFingerEncOnlyISOThirdPartyDemo(_req, BaseUrl, EndPointIso, RequestTime, req.UnitCode);

            return StatusCode(StatusCodes.Status200OK, new Core.Entities.ServiceResponse<ProfileByNikOnlyImg>
            {
                Message = obj.Message,
                Status = obj.Status.Equals("error") ? (int)ServiceResponseStatus.ERROR : (int)ServiceResponseStatus.SUKSES,
                Data = obj.Data
            });
        }
        #endregion

        #region Get Biometric Profile By NIK ISO
        /// <summary>
        /// Get data profile if finger match without finger type
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("get-profile-finger-enc-nik-iso-bio")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse<ProfileByNik>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Get data profile if finger match without finger type Iso File Server NIK Vers For Third Party")]
        public async Task<IActionResult> GetProfileAuthLoopFingerEncOnlyNIKISOBio([FromBody] ProfileLoopNIKThirdPartyReq req)
        {
            string authorization = HttpContext.Request.Headers["Authorization"];

            if (string.IsNullOrWhiteSpace(authorization))
            {
                return StatusCode(StatusCodes.Status401Unauthorized, new Core.Entities.ServiceResponse<ProfileByNikOnlyFinger>
                {
                    Message = ResponseUnauthorize,
                    Status = (int)ServiceResponseStatus.ERROR_PARAMETER,
                    Data = null
                });
            }

            var TypeApps = "";
            var AppsChannel = "";

            if (HttpContext.Request.Headers.TryGetValue("Type-Aplikasi", out var headerValues))
            {
                TypeApps = headerValues.ToString();
            }
            var token = authorization.Split(" ")[1];

            var claims = TokenManager.GetPrincipalThirdParty(token);

            if (HttpContext.Request.Headers.TryGetValue("Apps-Agent", out var headerValues2))
            {
                AppsChannel = headerValues2.ToString();
            }

            if (AppsChannel != claims.Name)
            {
                return StatusCode(StatusCodes.Status401Unauthorized, new Core.Entities.ServiceResponse<ProfileByNikOnlyFinger>
                {
                    Message = ResponseUnauthorize,
                    Status = (int)ServiceResponseStatus.ERROR_PARAMETER,
                    Data = null
                });
            }

            var RequestTime = DateTime.Now;

            var _req = new ProfileLoopReq
            {
                Branch = req.Branch,
                Base64Img = req.Base64Img,
                ClientApps = req.ClientApps,
                //EndPoint = req.EndPoint,
                LvTeller = req.LvTeller,
                Nik = req.Nik,
                Npp = null,
                SubBranch = req.SubBranch,
                UnitCode = req.UnitCode
            };

            var obj = await _profileService.GetAuthKTPDataFingerEncOnlyISOThirdPartyBio(_req, BaseUrl, EndPointIso, RequestTime, req.UnitCode);

            return StatusCode(StatusCodes.Status200OK, new Core.Entities.ServiceResponse<ProfileByNikOnlyFinger>
            {
                Message = obj.Message,
                Status = obj.Status.Equals("error") ? (int)ServiceResponseStatus.ERROR : (int)ServiceResponseStatus.SUKSES,
                Data = obj.Data
            });
        }
        #endregion

        #region Get Biometric Profile By CIF ISO
        /// <summary>
        /// Get data profile if finger match without finger type
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("get-profile-finger-enc-cif-iso-bio")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse<ProfileByNik>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Get data profile if finger match without finger type Iso File Server CIF Vers For Third Party")]
        public async Task<IActionResult> GetProfileAuthLoopFingerEncOnlyCIFISOBio([FromBody] ProfileLoopCifThirdPartyReq req)
        {
            string authorization = HttpContext.Request.Headers["Authorization"];

            if (string.IsNullOrWhiteSpace(authorization))
            {
                return StatusCode(StatusCodes.Status401Unauthorized, new Core.Entities.ServiceResponse<ProfileByNikOnlyFinger>
                {
                    Message = ResponseUnauthorize,
                    Status = (int)ServiceResponseStatus.ERROR_PARAMETER,
                    Data = null
                });
            }

            var TypeApps = "";
            var AppsChannel = "";

            if (HttpContext.Request.Headers.TryGetValue("Type-Aplikasi", out var headerValues))
            {
                TypeApps = headerValues.ToString();
            }
            var token = authorization.Split(" ")[1];

            var claims = TokenManager.GetPrincipalThirdParty(token);

            if (HttpContext.Request.Headers.TryGetValue("Apps-Agent", out var headerValues2))
            {
                AppsChannel = headerValues2.ToString();
            }

            if (AppsChannel != claims.Name)
            {
                return StatusCode(StatusCodes.Status401Unauthorized, new Core.Entities.ServiceResponse<ProfileByNikOnlyFinger>
                {
                    Message = ResponseUnauthorize,
                    Status = (int)ServiceResponseStatus.ERROR_PARAMETER,
                    Data = null
                });
            }

            string Nik = await _enrollmentRepository.GetNikByCif(req.Cif);
            if (Nik == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new Core.Entities.ServiceResponse<ProfileByNikOnlyFinger>
                {
                    Message = "Data Not Found",
                    Status = (int)ServiceResponseStatus.Data_Empty,
                    Data = null
                });
            }

            var isEmployee = await _enrollmentRepository.IsEmployee(Nik);

            var _req = new ProfileLoopReq
            {
                Branch = req.Branch,
                Base64Img = req.Base64Img,
                ClientApps = req.ClientApps,
                //EndPoint = req.EndPoint,
                LvTeller = req.LvTeller,
                Nik = Nik,
                Npp = isEmployee?.Npp,
                SubBranch = req.SubBranch,
                UnitCode = req.UnitCode
            };

            var RequestTime = DateTime.Now;

            var obj = await _profileService.GetAuthKTPDataFingerEncOnlyISOThirdPartyBio(_req, BaseUrl, EndPointIso, RequestTime, req.UnitCode);

            return StatusCode(StatusCodes.Status200OK, new Core.Entities.ServiceResponse<ProfileByNikOnlyFinger>
            {
                Message = obj.Message,
                Status = obj.Status.Equals("error") ? (int)ServiceResponseStatus.ERROR : (int)ServiceResponseStatus.SUKSES,
                Data = obj.Data
            });
        }
        #endregion

        #region Get Biometric Profile By NPP ISO
        /// <summary>
        /// Get data profile if finger match without finger type
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("get-profile-finger-enc-npp-iso-bio")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse<ProfileByNik>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Get data profile if finger match without finger type Iso File Server Npp Vers For Third Party")]
        public async Task<IActionResult> GetProfileAuthLoopFingerEncOnlyNppISOBio([FromBody] ProfileLoopNppThirdPartyReq req)
        {
            string authorization = HttpContext.Request.Headers["Authorization"];

            if (string.IsNullOrWhiteSpace(authorization))
            {
                return StatusCode(StatusCodes.Status401Unauthorized, new Core.Entities.ServiceResponse<ProfileByNikOnlyFinger>
                {
                    Message = ResponseUnauthorize,
                    Status = (int)ServiceResponseStatus.ERROR_PARAMETER,
                    Data = null
                });
            }

            var TypeApps = "";
            var AppsChannel = "";

            if (HttpContext.Request.Headers.TryGetValue("Type-Aplikasi", out var headerValues))
            {
                TypeApps = headerValues.ToString();
            }
            var token = authorization.Split(" ")[1];

            var claims = TokenManager.GetPrincipalThirdParty(token);

            if (HttpContext.Request.Headers.TryGetValue("Apps-Agent", out var headerValues2))
            {
                AppsChannel = headerValues2.ToString();
            }

            if (AppsChannel != claims.Name)
            {
                return StatusCode(StatusCodes.Status401Unauthorized, new Core.Entities.ServiceResponse<ProfileByNikOnlyFinger>
                {
                    Message = ResponseUnauthorize,
                    Status = (int)ServiceResponseStatus.ERROR_PARAMETER,
                    Data = null
                });
            }

            var _data = await _enrollmentRepository.MappingNppNik(req.Npp);
            if (_data == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest, new Core.Entities.ServiceResponse<ProfileByNikOnlyFinger>
                {
                    Message = "Data Not Found",
                    Status = (int)ServiceResponseStatus.Data_Empty,
                    Data = null
                });
            }

            var _req = new ProfileLoopReq
            {
                Branch = req.Branch,
                Base64Img = req.Base64Img,
                ClientApps = req.ClientApps,
                //EndPoint = req.EndPoint,
                LvTeller = req.LvTeller,
                Nik = _data.Nik,
                Npp = req.Npp,
                SubBranch = req.SubBranch,
                UnitCode = req.UnitCode
            };

            var RequestTime = DateTime.Now;

            var obj = await _profileService.GetAuthKTPDataFingerEncOnlyISOThirdPartyBio(_req, BaseUrl, EndPointIso, RequestTime, req.UnitCode);

            return StatusCode(StatusCodes.Status200OK, new Core.Entities.ServiceResponse<ProfileByNikOnlyFinger>
            {
                Message = obj.Message,
                Status = obj.Status.Equals("error") ? (int)ServiceResponseStatus.ERROR : (int)ServiceResponseStatus.SUKSES,
                Data = obj.Data
            });
        }
        #endregion
    }
}
