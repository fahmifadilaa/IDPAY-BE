using Ekr.Api.EnrollmentIKD.Filters;
using Ekr.Auth;
using Ekr.Business.Contracts.DataKTP;
using Ekr.Business.Contracts.Enrollment;
using Ekr.Business.Contracts.EnrollmentNoMatching;
using Ekr.Business.DataKTP;
using Ekr.Core.Constant;
using Ekr.Core.Entities;
using Ekr.Core.Entities.DataEnrollment;
using Ekr.Core.Entities.DataKTP;
using Ekr.Core.Entities.DataMaster.Utility;
using Ekr.Core.Entities.DataMaster.Utility.Entity;
using Ekr.Core.Entities.Enrollment;
using Ekr.Core.Entities.Recognition;
using Ekr.Core.Entities.SettingThreshold;
using Ekr.Core.Entities.ThirdParty;
using Ekr.Repository.Contracts.DataKTP;
using Ekr.Repository.Contracts.DataMaster.Utility;
using Ekr.Repository.Contracts.Enrollment;
using Ekr.Repository.Contracts.EnrollmentNoMatching;
using Ekr.Repository.DataMaster.Utility;
using Ekr.Services.Contracts.Account;
using Ekr.Services.Contracts.Recognition;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using NPOI.POIFS.Crypt.Dsig;
using ServiceStack.Text;
using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Ekr.Api.EnrollmentIKD.Controllers.DataEnrollment
{
    [Route("enrollment-encrypt-IKD")]
    [ApiController]
    public class EnrollmentEncryptIKDController : ControllerBase
    {
        private readonly IEnrollmentNoMatchingService _enrollmentService;
        private readonly IEnrollmentNoMatchingRepository _enrollmentKTPRepository;

        private readonly ICIFService _cifService;
        private readonly IConfiguration _configuration;
        private readonly IUtilityRepository _utilityRepository;
        private readonly IProfileService _profileService;
        private readonly IProfileRepository _profileRepo;
        private readonly IImageRecognitionService _imageRecognitionService;

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
        private readonly string ResponseBelumEnroll = "";

        public EnrollmentEncryptIKDController(IEnrollmentNoMatchingService enrollmentService,
           IEnrollmentNoMatchingRepository enrollmentKTPRepository,
           ICIFService cifService,
           IConfiguration configuration, IUtilityRepository utilityRepository, IProfileService profileService, IProfileRepository profileRepo, IImageRecognitionService imageRecognitionService)
        {
            _enrollmentService = enrollmentService;
            _enrollmentKTPRepository = enrollmentKTPRepository;
            _cifService = cifService;
            _configuration = configuration;
            _utilityRepository = utilityRepository;
            _profileService = profileService;
            _profileRepo = profileRepo;
            _imageRecognitionService = imageRecognitionService;

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
            ResponseBelumEnroll = _configuration.GetValue<string>("Response:DataBelumEnroll");
            _imageRecognitionService = imageRecognitionService;
        }

        /// <summary>
        /// To Create KTP data non employee encrypted ISO Version
        /// </summary>
        /// <param name="enroll"></param>
        /// <returns></returns>
        [HttpPost("submit")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse<EnrollKTP>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "submit-enroll-IKD")]
        public async Task<Core.Entities.ServiceResponse<EnrollKTP>> SubmitEnrollIKD(EnrollKTPNoMatching enroll)
        {
            string authorization = HttpContext.Request.Headers["Authorization"];

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

            if (string.IsNullOrWhiteSpace(authorization))
            {
                var (msg, code, cif) = await _enrollmentService.SubmitEnrollmentFingerEncryptedOnlyIKD(IsHitSOA, reqSOA, enroll, 24, "tes", "0001", 1, 3);
                enroll.KtpCif = cif;
                return new Core.Entities.ServiceResponse<EnrollKTP>
                {
                    Message = ResponseSukses,
                    Status = (int)ServiceResponseStatus.SUKSES,
                    Code = code
                };
            }

            var token = authorization.Split(" ")[1];

            var claims = TokenManager.GetPrincipal(token);

            var res = await _enrollmentService.SubmitEnrollmentFingerEncryptedOnlyIKD(IsHitSOA, reqSOA, enroll, int.Parse(claims.PegawaiId),
                claims.NIK, claims.KodeUnit, int.Parse(claims.UnitId), int.Parse(claims.RoleId));

            enroll.KtpCif = res.cif;

            return new Core.Entities.ServiceResponse<EnrollKTP>
            {
                Message = ResponseSukses,
                Status = (int)ServiceResponseStatus.SUKSES,
                Code = res.code
            };
        }

        /// <summary>
        /// To Updates KTP data non employee encrypted ISO Version
        /// </summary>
        /// <param name="enroll"></param>
        /// <returns></returns>
        [HttpPost("resubmit")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse<EnrollKTP>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Resubmit-enroll-IKD")]
        public async Task<Core.Entities.ServiceResponse<EnrollKTP>> ReSubmitEnrollIKD(EnrollKTPNoMatching enroll)
        {
            string authorization = HttpContext.Request.Headers["Authorization"];

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

            if (string.IsNullOrWhiteSpace(authorization))
            {
                var (msg, code, cif) = await _enrollmentService.ReSubmitEnrollmentFingerEncryptedOnlyNoMatchingIKD(IsHitSOA, reqSOA, enroll, 24, "tes", "0001", 1);
                enroll.KtpCif = cif;
                if (code == 0)
                {
                    return new Core.Entities.ServiceResponse<EnrollKTP>
                    {
                        Message = ResponseSukses,
                        Status = (int)ServiceResponseStatus.SUKSES,
                        Code = code
                    };
                }
                else
                {
                    return new Core.Entities.ServiceResponse<EnrollKTP>
                    {
                        Message = msg,
                        Status = (int)ServiceResponseStatus.ERROR,
                        Code = code
                    };
                }
            }

            var token = authorization.Split(" ")[1];

            var claims = TokenManager.GetPrincipal(token);

            var res = await _enrollmentService.ReSubmitEnrollmentFingerEncryptedOnlyNoMatchingIKD(IsHitSOA, reqSOA, enroll, int.Parse(claims.PegawaiId),
                claims.NIK, claims.KodeUnit, int.Parse(claims.UnitId));

            enroll.KtpCif = res.cif;

            if (res.code == 0)
            {
                return new Core.Entities.ServiceResponse<EnrollKTP>
                {
                    Message = ResponseSukses,
                    Status = (int)ServiceResponseStatus.SUKSES,
                    Code = res.code
                };
            }
            else
            {
                return new Core.Entities.ServiceResponse<EnrollKTP>
                {
                    Message = res.msg,
                    Status = (int)ServiceResponseStatus.ERROR,
                    Code = res.code
                };
            }
        }

        #region old scan-qr-ikd
        //[HttpPost("scan-qr-ikd")]
        //[ProducesResponseType(typeof(ServiceResponseFR<ScanResponse>), 200)]
        //[ProducesResponseType(500)]
        //[LogActivity(Keterangan = "scan qr ikd for enrollment no matching")]
        //public async Task<ScanResponse> ScanQRIKD(ScanQRIKDV2Req req)
        //{
        //    string authorization = HttpContext.Request.Headers["Authorization"];
        //    var data = new ScanResponse();

        //    var env = _configuration.GetValue<bool>("UrlScanIKD:IsProd");

        //    var BaseUrl = _configuration.GetValue<string>("UrlScanIKD:BaseUrl");
        //    var EndPoint = _configuration.GetValue<string>("UrlScanIKD:EndPoint");

        //    req.channel = _configuration.GetValue<string>("UrlScanIKD:channel");



        //    if (string.IsNullOrWhiteSpace(authorization))
        //    {
        //        data = await _enrollmentService.ScanQRIKD(req, new UrlRequestRecognitionFR
        //        {
        //            BaseUrl = BaseUrl,
        //            EndPoint = EndPoint,
        //            Env = env
        //        });

        //        return data;
        //    }

        //    var token = authorization.Split(" ")[1];
        //    var claims = TokenManager.GetPrincipal(token);

        //    data = await _enrollmentService.ScanQRIKD(req, new UrlRequestRecognitionFR
        //    {
        //        BaseUrl = BaseUrl,
        //        EndPoint = EndPoint,
        //        Env = env
        //    });


        //    return data;
        //}
        #endregion

        #region scan-qr-ikd new (20240102)

        #region scan qr ikd encrypted
        [HttpPost("scan-qr-ikd")]
        [ProducesResponseType(typeof(ServiceResponseFR<ScanResponse>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "scan qr ikd for enrollment no matching")]
        public async Task<ScanResponseEncrypt> ScanQRIKD(ScanQRIKDV2Req req)
        {
            string authorization = HttpContext.Request.Headers["Authorization"];
            //var data = new ScanResponse();
            var data = new ScanResponseEncrypt();

            var env = _configuration.GetValue<bool>("UrlScanIKD:IsProd");

            var BaseUrl = _configuration.GetValue<string>("UrlScanIKD:BaseUrl");
            var EndPoint = _configuration.GetValue<string>("UrlScanIKD:EndPoint");
            var isLimit = _configuration.GetValue<bool>("UrlScanIKD:isLimitAttempts");
            int maxLimit = _configuration.GetValue<int>("UrlScanIKD:max_attempts");
            int timeLimit = _configuration.GetValue<int>("UrlScanIKD:time_limit");
            var aesKey = _configuration.GetValue<string>("AesKey");

            req.channel = _configuration.GetValue<string>("UrlScanIKD:channel");

            if (string.IsNullOrWhiteSpace(authorization))
            {
                //data = await _enrollmentService.ScanQRIKD(req, new UrlRequestRecognitionFR
                //{
                //    BaseUrl = BaseUrl,
                //    EndPoint = EndPoint,
                //    Env = env
                //});

                //return data;

                data = await _enrollmentService.ScanQRIKDEncrypt(req, new UrlRequestRecognitionFR
                {
                    BaseUrl = BaseUrl,
                    EndPoint = EndPoint,
                    Env = env
                }, aesKey);

                return data;
            }

            var token = authorization.Split(" ")[1];
            var claims = TokenManager.GetPrincipal(token);

            if (isLimit)
            {
                data = await _enrollmentService.ScanQRIKDLimit(req, new UrlRequestRecognitionFR
                {
                    BaseUrl = BaseUrl,
                    EndPoint = EndPoint,
                    Env = env
                }, claims.NIK, maxLimit, timeLimit, Convert.ToInt32(claims.UserId), Convert.ToInt32(claims.RoleId), Convert.ToInt32(claims.UnitId), aesKey);

                //return data;
                return data;
            }
            else
            {
                //data = await _enrollmentService.ScanQRIKD(req, new UrlRequestRecognitionFR
                //{
                //    BaseUrl = BaseUrl,
                //    EndPoint = EndPoint,
                //    Env = env
                //});

                //return data;

                data = await _enrollmentService.ScanQRIKDEncrypt(req, new UrlRequestRecognitionFR
                {
                    BaseUrl = BaseUrl,
                    EndPoint = EndPoint,
                    Env = env
                }, aesKey);

                return data;

            }


        }
        #endregion

        #region scan qr ikd not encrypted
        [HttpPost("scan-qr-ikd-v3")]
        [ProducesResponseType(typeof(ServiceResponseFR<ScanResponse>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "scan qr ikd for enrollment no matching")]
        public async Task<ScanResponse> ScanQRIKDNotEncrypted(ScanQRIKDV2Req req)
        {
            string authorization = HttpContext.Request.Headers["Authorization"];
            var data = new ScanResponse();
            //var data = new ScanResponseEncrypt();

            var env = _configuration.GetValue<bool>("UrlScanIKD:IsProd");

            var BaseUrl = _configuration.GetValue<string>("UrlScanIKD:BaseUrl");
            var EndPoint = _configuration.GetValue<string>("UrlScanIKD:EndPoint");
            var isLimit = _configuration.GetValue<bool>("UrlScanIKD:isLimitAttempts");
            int maxLimit = _configuration.GetValue<int>("UrlScanIKD:max_attempts");
            int timeLimit = _configuration.GetValue<int>("UrlScanIKD:time_limit");
            var aesKey = _configuration.GetValue<string>("AppSettings:AesKey");

            req.channel = _configuration.GetValue<string>("UrlScanIKD:channel");

            if (string.IsNullOrWhiteSpace(authorization))
            {
                data = await _enrollmentService.ScanQRIKD(req, new UrlRequestRecognitionFR
                {
                    BaseUrl = BaseUrl,
                    EndPoint = EndPoint,
                    Env = env
                });

                return data;

                //data = await _enrollmentService.ScanQRIKDEncrypt(req, new UrlRequestRecognitionFR
                //{
                //    BaseUrl = BaseUrl,
                //    EndPoint = EndPoint,
                //    Env = env
                //}, aesKey);

                //return data;
            }

            var token = authorization.Split(" ")[1];
            var claims = TokenManager.GetPrincipal(token);

            if (isLimit)
            {
                data = await _enrollmentService.ScanQRIKDLimitNotEncrypted(req, new UrlRequestRecognitionFR
                {
                    BaseUrl = BaseUrl,
                    EndPoint = EndPoint,
                    Env = env
                }, claims.NIK, maxLimit, timeLimit, Convert.ToInt32(claims.UserId), Convert.ToInt32(claims.RoleId), Convert.ToInt32(claims.UnitId));

                return data;
            }
            else
            {
                data = await _enrollmentService.ScanQRIKD(req, new UrlRequestRecognitionFR
                {
                    BaseUrl = BaseUrl,
                    EndPoint = EndPoint,
                    Env = env
                });

                return data;

                //data = await _enrollmentService.ScanQRIKDEncrypt(req, new UrlRequestRecognitionFR
                //{
                //    BaseUrl = BaseUrl,
                //    EndPoint = EndPoint,
                //    Env = env
                //}, aesKey);

                //return data;

            }
        }
        #endregion

        #endregion

        [HttpPost("scan-qr-ikd-xml")]
        [ProducesResponseType(typeof(ServiceResponseFR<ScanResponse>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "scan qr ikd XML for enrollment no matching")]
        public async Task<ScanResponse> ScanQRIKDXML(ScanQRIKDReq req)
        {
            string authorization = HttpContext.Request.Headers["Authorization"];
            var data = new ScanResponse();

            var env = _configuration.GetValue<bool>("UrlScanIKD:IsProd");

            var BaseUrl = _configuration.GetValue<string>("UrlScanIKD:BaseUrl");
            var EndPoint = _configuration.GetValue<string>("UrlScanIKD:EndPoint");


            var APIKEY = _configuration.GetValue<string>("UrlScanIKD:api_key");
            var CLIENTKEy = _configuration.GetValue<string>("UrlScanIKD:client_key");
            var channel = _configuration.GetValue<string>("UrlScanIKD:channel");

            req.api_key = APIKEY;
            req.client_key = CLIENTKEy;
            req.channel = channel;

            if (string.IsNullOrWhiteSpace(authorization))
            {
                data = await _enrollmentService.ScanQRIKDXML(req, new UrlRequestRecognitionFR
                {
                    BaseUrl = BaseUrl,
                    EndPoint = EndPoint,
                    Env = env
                });

                return data;
            }

            var token = authorization.Split(" ")[1];
            var claims = TokenManager.GetPrincipal(token);

            data = await _enrollmentService.ScanQRIKDXML(req, new UrlRequestRecognitionFR
            {
                BaseUrl = BaseUrl,
                EndPoint = EndPoint,
                Env = env
            });


            return data;
        }

        [HttpPost("scan-qr-ikd-v2")]
        [ProducesResponseType(typeof(ServiceResponseFR<ScanResponse>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "scan qr ikd V2 for enrollment no matching")]
        public async Task<ScanResponse> ScanQRIKDV2(ScanQRIKDV2Req req)
        {
            string authorization = HttpContext.Request.Headers["Authorization"];
            var data = new ScanResponse();

            var env = _configuration.GetValue<bool>("UrlScanIKD:IsProd");

            var BaseUrl = _configuration.GetValue<string>("UrlScanIKD:BaseUrl");
            var EndPoint = _configuration.GetValue<string>("UrlScanIKD:EndPoint");
            req.channel = _configuration.GetValue<string>("UrlScanIKD:channel");

            if (string.IsNullOrWhiteSpace(authorization))
            {
                data = await _enrollmentService.ScanQRIKDV2(req, new UrlRequestRecognitionFR
                {
                    BaseUrl = BaseUrl,
                    EndPoint = EndPoint,
                    Env = env
                });

                return data;
            }

            var token = authorization.Split(" ")[1];
            var claims = TokenManager.GetPrincipal(token);

            data = await _enrollmentService.ScanQRIKDV2(req, new UrlRequestRecognitionFR
            {
                BaseUrl = BaseUrl,
                EndPoint = EndPoint,
                Env = env
            });


            return data;
        }


        [HttpPost("scan-qr-ikd-get-consent")]
        [ProducesResponseType(typeof(ServiceResponseFR<IKDConsentResponse>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Request consent IKD using QR")]
        public async Task<IActionResult> GetConsentIKD(ScanQRIKDConsentReq req)
        {
            string authorization = HttpContext.Request.Headers["Authorization"];

            var env = _configuration.GetValue<bool>("UrlScanIKD:IsProd");
            var BaseUrl = _configuration.GetValue<string>("UrlScanIKD:BaseUrl");
            var EndPoint = _configuration.GetValue<string>("UrlScanIKD:EndPointConsent");

            req.channel = _configuration.GetValue<string>("UrlScanIKD:channel");

            var urlRequest = new UrlRequestRecognitionFR
            {
                BaseUrl = BaseUrl,
                EndPoint = EndPoint,
                Env = env
            };

            try
            {
                var response = await _enrollmentService.GetConsentIKDAsync(req, urlRequest);
                return Ok(response);
            }
            catch (Exception)
            {
                return StatusCode(500, new ScanResponseEncrypt
                {
                    err_code = 500,
                    err_msg = "INTERNAL SERVER ERROR"
                });
            }
        }

        [HttpPost("scan-qr-ikd-get-data")]
        [ProducesResponseType(typeof(ServiceResponseFR<IKDDataResponse>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "get data ikd from trxId")]
        public async Task<IActionResult> GetDataIKD(ScanIKDGetData req)
        {
            string authorization = HttpContext.Request.Headers["Authorization"];

            var env = _configuration.GetValue<bool>("UrlScanIKD:IsProd");
            var baseUrl = _configuration.GetValue<string>("UrlScanIKD:BaseUrl");
            var EndPoint = _configuration.GetValue<string>("UrlScanIKD:EndPointData");
            var isLimit = _configuration.GetValue<bool>("UrlScanIKD:isLimitAttempts");
            int maxLimit = _configuration.GetValue<int>("UrlScanIKD:max_attempts");
            int timeLimit = _configuration.GetValue<int>("UrlScanIKD:time_limit");
            var aesKey = _configuration.GetValue<string>("AesKey");

            req.channel = _configuration.GetValue<string>("UrlScanIKD:channel");

            var urlRequest = new UrlRequestRecognitionFR
            {
                BaseUrl = baseUrl,
                EndPoint = EndPoint,
                Env = env
            };

            try
            {
                var result = await _enrollmentService.GetDataIKDEncrypt(req, urlRequest, aesKey);

                return Ok(new ServiceResponseFR<IKDDataResponse>
                {
                    Message = result.Message,
                    Status = result.Status,
                    Code = result.Code,
                    Data = result.Data
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ServiceResponseFR<IKDDataResponse>
                {
                    Message = "INTERNAL SERVER ERROR",
                    Status = 0,
                    Code = 500,
                    Data = null
                });
            }
        }

    }
}
