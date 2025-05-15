using Ekr.Api.DataEnrollment.Filters;
//using Ekr.Api.DataEnrollment.Filters;
using Ekr.Auth;
using Ekr.Business.Contracts.Enrollment;
using Ekr.Core.Constant;
using Ekr.Core.Entities;
using Ekr.Core.Entities.Account;
using Ekr.Core.Entities.DataEnrollment.ViewModel;
using Ekr.Core.Entities.DataKTP;
using Ekr.Core.Entities.Enrollment;
using Ekr.Core.Entities.ThirdParty;
using Ekr.Repository.Contracts.Enrollment;
using Ekr.Services.Contracts.Account;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ekr.Api.DataEnrollment.Controllers
{
	[Route("enrollment-encrypt")]
    [ApiController]
    public class EnrollmentEncryptController : ControllerBase
    {
        private readonly IEnrollmentService _enrollmentService;
        private readonly IEnrollmentKTPRepository _enrollmentKTPRepository;
        private readonly ICIFService _cifService;
        private readonly IConfiguration _configuration;
        private readonly string HostSoa = "";
        private readonly string systemId = "";
        private readonly string idType = "";
        private readonly string teller = "";
        private readonly string branch = "";
        private readonly string BaseUrlNonSoa = "";
        private readonly string EndPointNonSoa = "";
        private readonly bool IsHitSOA = false;

        public EnrollmentEncryptController(IEnrollmentService enrollmentService,
            IEnrollmentKTPRepository enrollmentKTPRepository,
            ICIFService cifService,
            IConfiguration configuration)
        {
            _enrollmentService = enrollmentService;
            _enrollmentKTPRepository = enrollmentKTPRepository;
            _cifService = cifService;
            _configuration = configuration;

            HostSoa = _configuration.GetValue<string>("SOAApi:Host");
            systemId = _configuration.GetValue<string>("SOAApi:systemId");
            idType = _configuration.GetValue<string>("SOAApi:idType");
            teller = _configuration.GetValue<string>("SOAApi:teller");
            branch = _configuration.GetValue<string>("SOAApi:branch");
            IsHitSOA = _configuration.GetValue<bool>("isHitSOA");
            BaseUrlNonSoa = _configuration.GetValue<string>("NonSOAApi:base_url");
            EndPointNonSoa = _configuration.GetValue<string>("NonSOAApi:endpoint");
        }

        /// <summary>
        /// To enroll KTP data
        /// </summary>
        /// <param name="enroll"></param>
        /// <returns></returns>
        [HttpPost("submit")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse<EnrollKTP>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "To enroll KTP data")]
        public async Task<Core.Entities.ServiceResponse<EnrollKTP>> SubmitEnroll(EnrollKTP enroll)
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
                var (msg, code, cif) = await _enrollmentService.SubmitEnrollment(IsHitSOA, reqSOA, enroll, 24, "tes", "0001", 1);

                enroll.KtpCif = cif;

                return new Core.Entities.ServiceResponse<EnrollKTP>
                {
                    Message = nameof(ServiceResponseStatus.SUKSES),
                    Status = (int)ServiceResponseStatus.SUKSES,
                    Data = enroll,
                    Code = code
                };
            }

            var token = authorization.Split(" ")[1];

            var claims = TokenManager.GetPrincipal(token);

            var res = await _enrollmentService.SubmitEnrollment(IsHitSOA, reqSOA, enroll, int.Parse(claims.PegawaiId),
                claims.NIK, claims.KodeUnit, int.Parse(claims.UnitId));

            enroll.KtpCif = res.cif;

            return new Core.Entities.ServiceResponse<EnrollKTP>
            {
                Data = enroll,
                Message = nameof(ServiceResponseStatus.SUKSES),
                Status = (int)ServiceResponseStatus.SUKSES,
                Code = res.code
            };
        }


        /// <summary>
        /// To resubmit and update KTP Data
        /// </summary>
        /// <param name="enroll"></param>
        /// <returns></returns>
        [HttpPost("resubmit")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse<EnrollKTP>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "To Update Enroll KTP data")]
        public async Task<Core.Entities.ServiceResponse<EnrollKTP>> ReSubmitEnroll(EnrollKTP enroll)
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
                var (msg, code, cif) = await _enrollmentService.ReSubmitEnrollment(IsHitSOA, reqSOA, enroll, 24, "tes", "0001", 1);

                enroll.KtpCif = cif;

                return new Core.Entities.ServiceResponse<EnrollKTP>
                {
                    Message = nameof(ServiceResponseStatus.SUKSES),
                    Status = (int)ServiceResponseStatus.SUKSES,
                    Data = enroll,
                    Code = code
                };
            }

            var token = authorization.Split(" ")[1];

            var claims = TokenManager.GetPrincipal(token);

            var res = await _enrollmentService.ReSubmitEnrollment(IsHitSOA, reqSOA, enroll, int.Parse(claims.PegawaiId),
                claims.NIK, claims.KodeUnit, int.Parse(claims.UnitId));

            enroll.KtpCif = res.cif;

            return new Core.Entities.ServiceResponse<EnrollKTP>
            {
                Data = enroll,
                Message = nameof(ServiceResponseStatus.SUKSES),
                Status = (int)ServiceResponseStatus.SUKSES,
                Code = res.code
            };
        }

        /// <summary>
        /// To Updates Photo Cam
        /// </summary>
        /// <param name="enroll"></param>
        /// <returns></returns>
        [HttpPost("update-photocam")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse<EnrollKTP>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "To Updates Photo Cam Data")]
        public async Task<Core.Entities.ServiceResponse<EnrollKTP>> UpdatesPhotoCam(EnrollKTP enroll)
        {
            string authorization = HttpContext.Request.Headers["Authorization"];

            if (string.IsNullOrWhiteSpace(authorization))
            {
                var (msg, code, cif) = await _enrollmentService.UpdatesPhotoCam(enroll, 24, "tes", "0001", 1);

                enroll.KtpCif = cif;

                return new Core.Entities.ServiceResponse<EnrollKTP>
                {
                    Message = nameof(ServiceResponseStatus.SUKSES),
                    Status = (int)ServiceResponseStatus.SUKSES,
                    Data = enroll,
                    Code = code
                };
            }

            var token = authorization.Split(" ")[1];

            var claims = TokenManager.GetPrincipal(token);

            var res = await _enrollmentService.UpdatesPhotoCam(enroll, int.Parse(claims.PegawaiId),
                claims.NIK, claims.KodeUnit, int.Parse(claims.UnitId));

            enroll.KtpCif = res.cif;

            return new Core.Entities.ServiceResponse<EnrollKTP>
            {
                Data = enroll,
                Message = nameof(ServiceResponseStatus.SUKSES),
                Status = (int)ServiceResponseStatus.SUKSES,
                Code = res.code
            };
        }

        [HttpPost("submit-finger-enc-only")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse<EnrollKTP>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "submit-finger-enc-only")]
        public async Task<Core.Entities.ServiceResponse<EnrollKTP>> SubmitEnrollFingerEncOnly(EnrollKTP enroll)
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
                var (msg, code, cif) = await _enrollmentService.SubmitEnrollmentFingerEncryptedOnly(IsHitSOA, reqSOA, enroll, 24, "tes", "0001", 1);
                enroll.KtpCif = cif;
                return new Core.Entities.ServiceResponse<EnrollKTP>
                {
                    Message = nameof(ServiceResponseStatus.SUKSES),
                    Status = (int)ServiceResponseStatus.SUKSES,
                    Data = enroll,
                    Code = code
                };
            }

            var token = authorization.Split(" ")[1];

            var claims = TokenManager.GetPrincipal(token);

            var res = await _enrollmentService.SubmitEnrollmentFingerEncryptedOnly(IsHitSOA, reqSOA, enroll, int.Parse(claims.PegawaiId),
                claims.NIK, claims.KodeUnit, int.Parse(claims.UnitId));

            enroll.KtpCif = res.cif;

            return new Core.Entities.ServiceResponse<EnrollKTP>
            {
                Data = enroll,
                Message = nameof(ServiceResponseStatus.SUKSES),
                Status = (int)ServiceResponseStatus.SUKSES,
                Code = res.code
            };
        }

        /// <summary>
        /// To Updates KTP data non employee encrypted
        /// </summary>
        /// <param name="enroll"></param>
        /// <returns></returns>
        [HttpPost("resubmit-finger-enc-only")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse<EnrollKTP>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "To Updates KTP data non employee encrypted")]
        public async Task<Core.Entities.ServiceResponse<EnrollKTP>> ReSubmitEnrollFingerEncOnly(EnrollKTP enroll)
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
                var (msg, code, cif) = await _enrollmentService.ReSubmitEnrollmentFingerEncryptedOnly(IsHitSOA, reqSOA, enroll, 24, "tes", "0001", 1);
                enroll.KtpCif = cif;
                if(code == 0)
				{
                    return new Core.Entities.ServiceResponse<EnrollKTP>
                    {
                        Message = nameof(ServiceResponseStatus.SUKSES),
                        Status = (int)ServiceResponseStatus.SUKSES,
                        Data = enroll,
                        Code = code
                    };
				}
				else
				{
                    return new Core.Entities.ServiceResponse<EnrollKTP>
                    {
                        Message = msg,
                        Status = (int)ServiceResponseStatus.ERROR,
                        Data = null,
                        Code = code
                    };
                }
            }

            var token = authorization.Split(" ")[1];

            var claims = TokenManager.GetPrincipal(token);

            var res = await _enrollmentService.ReSubmitEnrollmentFingerEncryptedOnly(IsHitSOA, reqSOA, enroll, int.Parse(claims.PegawaiId),
                claims.NIK, claims.KodeUnit, int.Parse(claims.UnitId));

            enroll.KtpCif = res.cif;

            if (res.code == 0)
            {
                return new Core.Entities.ServiceResponse<EnrollKTP>
                {
                    Message = nameof(ServiceResponseStatus.SUKSES),
                    Status = (int)ServiceResponseStatus.SUKSES,
                    Data = enroll,
                    Code = res.code
                };
            }
            else
            {
                return new Core.Entities.ServiceResponse<EnrollKTP>
                {
                    Message = res.msg,
                    Status = (int)ServiceResponseStatus.ERROR,
                    Data = null,
                    Code = res.code
                };
            }
        }

        /// <summary>
        /// To enroll KTP data employee
        /// </summary>
        /// <param name="enroll"></param>
        /// <returns></returns>
        [HttpPost("submit-employee")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse<EnrollKTP>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "To enroll KTP data employee")]
        public async Task<Core.Entities.ServiceResponse<EnrollKTP>> SubmitEnrollPegawai(EnrollKTP enroll)
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
                var res1 = await _enrollmentService.SubmitEnrollment(IsHitSOA, reqSOA, enroll, 24, "tes", "0001", 1);
                enroll.KtpCif = res1.cif;
                return new Core.Entities.ServiceResponse<EnrollKTP>
                {
                    Message = nameof(ServiceResponseStatus.SUKSES),
                    Status = (int)ServiceResponseStatus.SUKSES,
                    Data = enroll,
                    Code = res1.code
                };
            }

            var token = authorization.Split(" ")[1];

            var claims = TokenManager.GetPrincipal(token);

            var (msg, code, cif) = await _enrollmentService.SubmitEnrollment(IsHitSOA, reqSOA, enroll, int.Parse(claims.PegawaiId),
                claims.NIK, claims.KodeUnit, int.Parse(claims.UnitId));
            enroll.KtpCif = cif;

            return new Core.Entities.ServiceResponse<EnrollKTP>
            {
                Data = enroll,
                Message = nameof(ServiceResponseStatus.SUKSES),
                Status = (int)ServiceResponseStatus.SUKSES,
                Code = code
            };
        }

        /// <summary>
        /// To Updates enroll KTP data employee
        /// </summary>
        /// <param name="enroll"></param>
        /// <returns></returns>
        [HttpPost("resubmit-employee")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse<EnrollKTP>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "To Updates KTP data employee")]
        public async Task<Core.Entities.ServiceResponse<EnrollKTP>> ReSubmitEnrollPegawai(EnrollKTP enroll)
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
                var res1 = await _enrollmentService.ReSubmitEnrollment(IsHitSOA, reqSOA, enroll, 24, "tes", "0001", 1);
                enroll.KtpCif = res1.cif;
                return new Core.Entities.ServiceResponse<EnrollKTP>
                {
                    Message = nameof(ServiceResponseStatus.SUKSES),
                    Status = (int)ServiceResponseStatus.SUKSES,
                    Data = enroll,
                    Code = res1.code
                };
            }

            var token = authorization.Split(" ")[1];

            var claims = TokenManager.GetPrincipal(token);

            var (msg, code, cif) = await _enrollmentService.ReSubmitEnrollment(IsHitSOA, reqSOA, enroll, int.Parse(claims.PegawaiId),
                claims.NIK, claims.KodeUnit, int.Parse(claims.UnitId));
            enroll.KtpCif = cif;

            return new Core.Entities.ServiceResponse<EnrollKTP>
            {
                Data = enroll,
                Message = nameof(ServiceResponseStatus.SUKSES),
                Status = (int)ServiceResponseStatus.SUKSES,
                Code = code
            };
        }

        /// <summary>
        /// To enroll KTP data employee Finger Encrypted Only
        /// </summary>
        /// <param name="enroll"></param>
        /// <returns></returns>
        [HttpPost("submit-employee-finger-enc-only")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse<string>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "To enroll KTP data employee Finger Encrypted Only")]
        public async Task<Core.Entities.ServiceResponse<EnrollKTP>> SubmitEnrollPegawaiFingerEncOnly(EnrollKTP enroll)
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
                var res1 = await _enrollmentService.SubmitEnrollmentFingerEncryptedOnly(IsHitSOA, reqSOA, enroll, 24, "tes", "0001", 1);
                enroll.KtpCif = res1.cif;
                return new Core.Entities.ServiceResponse<EnrollKTP>
                {
                    Message = nameof(ServiceResponseStatus.SUKSES),
                    Status = (int)ServiceResponseStatus.SUKSES,
                    Data = enroll,
                    Code = res1.code
                };
            }

            var token = authorization.Split(" ")[1];

            var claims = TokenManager.GetPrincipal(token);

            var (msg, code, cif) = await _enrollmentService.SubmitEnrollmentFingerEncryptedOnly(IsHitSOA, reqSOA, enroll, int.Parse(claims.PegawaiId),
                claims.NIK, claims.KodeUnit, int.Parse(claims.UnitId));
            enroll.KtpCif = cif;

            return new Core.Entities.ServiceResponse<EnrollKTP>
            {
                Data = enroll,
                Message = nameof(ServiceResponseStatus.SUKSES),
                Status = (int)ServiceResponseStatus.SUKSES,
                Code = code
            };
        }

        /// <summary>
        /// To Updates KTP data employee Finger Encrypted Only
        /// </summary>
        /// <param name="enroll"></param>
        /// <returns></returns>
        [HttpPost("resubmit-employee-finger-enc-only")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse<string>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "To Updates KTP data employee Finger Encrypted Only")]
        public async Task<Core.Entities.ServiceResponse<EnrollKTP>> ReSubmitEnrollPegawaiFingerEncOnly(EnrollKTP enroll)
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
                var res1 = await _enrollmentService.ReSubmitEnrollmentFingerEncryptedOnly(IsHitSOA, reqSOA, enroll, 24, "tes", "0001", 1);
                enroll.KtpCif = res1.cif;
                return new Core.Entities.ServiceResponse<EnrollKTP>
                {
                    Message = nameof(ServiceResponseStatus.SUKSES),
                    Status = (int)ServiceResponseStatus.SUKSES,
                    Data = enroll,
                    Code = res1.code
                };
            }

            var token = authorization.Split(" ")[1];

            var claims = TokenManager.GetPrincipal(token);

            var (msg, code, cif) = await _enrollmentService.ReSubmitEnrollmentFingerEncryptedOnly(IsHitSOA, reqSOA, enroll, int.Parse(claims.PegawaiId),
                claims.NIK, claims.KodeUnit, int.Parse(claims.UnitId));
            enroll.KtpCif = cif;

            return new Core.Entities.ServiceResponse<EnrollKTP>
            {
                Data = enroll,
                Message = nameof(ServiceResponseStatus.SUKSES),
                Status = (int)ServiceResponseStatus.SUKSES,
                Code = code
            };
        }

        /// <summary>
        /// To check whether NPP have been enrolled or not
        /// </summary>
        /// <param name="npp"></param>
        /// <returns></returns>
        [HttpGet("is-npp-enrolled")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse<bool>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "To check whether NPP have been enrolled or not")]
        public async Task<Core.Entities.ServiceResponse<bool>> CheckExistingNpp(string npp)
        {
            if (string.IsNullOrEmpty(npp))
            {
                return new Core.Entities.ServiceResponse<bool>
                {
                    Status = (int)ServiceResponseStatus.EMPTY_PARAMETER,
                    Message = nameof(ServiceResponseStatus.EMPTY_PARAMETER)
                };
            }

            var res = await _enrollmentKTPRepository.IsNppEnrolled(npp);

            return new Core.Entities.ServiceResponse<bool>
            {
                Data = res,
                Message = nameof(ServiceResponseStatus.SUKSES),
                Status = (int)ServiceResponseStatus.SUKSES,
            };
        }

        /// <summary>
        /// To check whether NPP have been enrolled or not
        /// </summary>
        /// <param name="nik"></param>
        /// <returns></returns>
        [HttpGet("get-cif")]
        [ProducesResponseType(typeof(Core.Entities.Account.ServiceResponse<CekCIFDto>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "To check whether NPP have been enrolled or not")]
        public async Task<Core.Entities.Account.ServiceResponse<CekCIFDto>> GetCIF(string nik)
        {
            var res = new CekCIFDto();

            if (string.IsNullOrEmpty(nik))
            {
                return new Core.Entities.Account.ServiceResponse<CekCIFDto>
                {
                    Code = (int)ServiceResponseStatus.EMPTY_PARAMETER,
                    Msg = nameof(ServiceResponseStatus.EMPTY_PARAMETER)
                };
            }

            if (IsHitSOA)
            {
                var reqSOA = new ApiSOA
                {
                    systemId = systemId,
                    numId = nik,
                    idType = idType,
                    host = HostSoa,
                    teller = teller,
                    branch = branch,
                    baseUrlNonSoa = BaseUrlNonSoa,
                    UrlEndPointNonSoa = EndPointNonSoa
                };

                #region HIT SOA And Logging it
                var cif = await _cifService.GetSOAByCif(reqSOA);

                var status = 0;
                if (cif.cif != null)
                {
                    status = 1;
                }

                var _log = new Tbl_ThirdPartyLog
                {
                    FeatureName = "GetCIF",
                    HostUrl = HostSoa,
                    Request = nik,
                    Status = status,
                    Response = Newtonsoft.Json.JsonConvert.SerializeObject(cif),
                    CreatedDate = System.DateTime.Now,
                    CreatedBy = "24"
                };

                _ = _enrollmentKTPRepository.CreateThirdPartyLog(_log);
                #endregion

                res.Nik = nik;
                res.Cif = cif.cif;
            }
            else
            {
                var CifNonSoa = await _cifService.GetCIF(
                    new NikDto { Nik = nik },
                    new NikDtoUrl { baseUrl = BaseUrlNonSoa, endpoint = EndPointNonSoa });
                if(CifNonSoa.Data == null)
                {
                    res.Cif = null;
                }
                else
                {
                    res.Cif = CifNonSoa.Data.Cif;
                }
                res.Nik = nik;
            }

            return new Core.Entities.Account.ServiceResponse<CekCIFDto>
            {
                Data = res,
                Msg = nameof(ServiceResponseStatus.SUKSES),
                Code = (int)ServiceResponseStatus.SUKSES,
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
            #region old code
            //var _emp = await _enrollmentKTPRepository.IsEmployee(nik);

            //         if(_emp == null)
            //{
            //             var res = await _enrollmentKTPRepository.GetISO(nik);

            //             if (res == null)
            //	{
            //                 return new Core.Entities.Account.ServiceResponse<object>
            //                 {
            //                     Code = (int)ServiceResponseStatus.Data_Empty,
            //                     Msg = "Data Belum di Enroll"
            //                 };
            //             }

            //	foreach (var item in res)
            //	{
            //                 if(item.FileNameISO == null || item.PathFileISO == null)
            //		{
            //                     return new Core.Entities.Account.ServiceResponse<object>
            //                     {
            //                         Code = (int)ServiceResponseStatus.Data_Empty,
            //                         Msg = "Data Belum di Enroll"
            //                     };
            //                 }
            //	}

            //             return new Core.Entities.Account.ServiceResponse<object>
            //             {
            //                 Data = res,
            //                 Msg = nameof(ServiceResponseStatus.SUKSES),
            //                 Code = (int)ServiceResponseStatus.SUKSES,
            //             };
            //         }
            //else
            //{
            //             var res = await _enrollmentKTPRepository.GetISOEmp(nik);
            //             if (res == null)
            //             {
            //                 return new Core.Entities.Account.ServiceResponse<object>
            //                 {
            //                     Code = (int)ServiceResponseStatus.Data_Empty,
            //                     Msg = "Data Belum di Enroll"
            //                 };
            //             }

            //             foreach (var item in res)
            //             {
            //                 if (item.FileNameISO == null || item.PathFileISO == null)
            //                 {
            //                     return new Core.Entities.Account.ServiceResponse<object>
            //                     {
            //                         Code = (int)ServiceResponseStatus.Data_Empty,
            //                         Msg = "Data Belum di Enroll"
            //                     };
            //                 }
            //             }

            //             return new Core.Entities.Account.ServiceResponse<object>
            //             {
            //                 Data = res,
            //                 Msg = nameof(ServiceResponseStatus.SUKSES),
            //                 Code = (int)ServiceResponseStatus.SUKSES,
            //             };
            //         }
            #endregion

            var resp = await _enrollmentService.GetISO(nik);
            if(resp == null)
			{
                return new Core.Entities.Account.ServiceResponse<object>
                {
                    Data = resp,
                    Msg = "Data Belum di Enroll",
                    Code = (int)ServiceResponseStatus.Data_Empty,
                };
            }

            return new Core.Entities.Account.ServiceResponse<object>
			{
				Data = resp,
				Msg = nameof(ServiceResponseStatus.SUKSES),
				Code = (int)ServiceResponseStatus.SUKSES,
			};
		}

        /// <summary>
        /// To check whether iso have been inserted or not
        /// </summary>
        /// <param name="nik"></param>
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
                    Msg = nameof(ServiceResponseStatus.EMPTY_PARAMETER)
                };
            }

            var _data = await _enrollmentKTPRepository.MappingNppNik(npp);
            if (_data == null)
            {
                return new Core.Entities.Account.ServiceResponse<object>
                {
                    Code = (int)ServiceResponseStatus.EMPTY_PARAMETER,
                    Msg = nameof(ServiceResponseStatus.EMPTY_PARAMETER)
                };
            }

            var resp = await _enrollmentService.GetISO(_data.Nik);
            if (resp == null)
            {
                return new Core.Entities.Account.ServiceResponse<object>
                {
                    Data = resp,
                    Msg = "Data Belum di Enroll",
                    Code = (int)ServiceResponseStatus.Data_Empty,
                };
            }

            return new Core.Entities.Account.ServiceResponse<object>
            {
                Data = resp,
                Msg = nameof(ServiceResponseStatus.SUKSES),
                Code = (int)ServiceResponseStatus.SUKSES,
            };
        }

        /// <summary>
        /// Get NIK by NPP
        /// </summary>
        /// <param name="npp"></param>
        /// <returns>NIK</returns>
        [HttpGet("nik")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse<string>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Get NIK by NPP")]
        public async Task<Core.Entities.ServiceResponse<string>> GetNIK(string npp)
        {
            var res = await _enrollmentKTPRepository.GetNIK(npp);

            return new Core.Entities.ServiceResponse<string>
            {
                Message = string.IsNullOrWhiteSpace(res) ? nameof(ServiceResponseStatus.Data_Empty) : nameof(ServiceResponseStatus.SUKSES),
                Status = string.IsNullOrWhiteSpace(res) ? (int)ServiceResponseStatus.Data_Empty : (int)ServiceResponseStatus.SUKSES,
                Data = res
            };
        }

        /// <summary>
        /// To verify enrollment
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("verify")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse<string>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "To verify enrollment")]
        public async Task<Core.Entities.ServiceResponse<string>> VerifyEnrollment([FromBody] VerifyEnrollReq req)
        {
            string authorization = HttpContext.Request.Headers["Authorization"];

            if (string.IsNullOrWhiteSpace(authorization))
            {
                var res1 = await _enrollmentService.VerifyEnrollment(req.Nik, "tes", req.Comment);
                return new Core.Entities.ServiceResponse<string>
                {
                    Message = string.IsNullOrWhiteSpace(res1) ? nameof(ServiceResponseStatus.SUKSES) : nameof(ServiceResponseStatus.Data_Empty),
                    Status = string.IsNullOrWhiteSpace(res1) ? (int)ServiceResponseStatus.SUKSES : (int)ServiceResponseStatus.Data_Empty,
                    Data = res1
                };
            }

            var token = authorization.Split(" ")[1];

            var claims = TokenManager.GetPrincipal(token);

            var res = await _enrollmentService.VerifyEnrollment(req.Nik, claims.NIK, req.Comment);

            return new Core.Entities.ServiceResponse<string>
            {
                Data = res,
                Message = string.IsNullOrWhiteSpace(res) ? nameof(ServiceResponseStatus.SUKSES) : nameof(ServiceResponseStatus.Data_Empty),
                Status = string.IsNullOrWhiteSpace(res) ? (int)ServiceResponseStatus.SUKSES : (int)ServiceResponseStatus.Data_Empty
            };
        }

        /// <summary>
        /// To load data inbox
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("inbox")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse<GridResponse<InboxDataEnrollVM>>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "To load data inbox")]
        public async Task<Core.Entities.ServiceResponse<GridResponse<InboxDataEnrollVM>>> LoadDataInboxEnroll([FromBody] InboxDataEnrollFilterVM req)
        {
            var res = await _enrollmentKTPRepository.LoadDataInboxEnroll(req);

            return new Core.Entities.ServiceResponse<GridResponse<InboxDataEnrollVM>>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = nameof(ServiceResponseStatus.SUKSES),
                Data = res
            };
        }

        /// <summary>
        /// To confirm data enroll (verified/not)
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("confirm_submission")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse<string>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "To confirm data enroll (verified/not)")]
        public async Task<Core.Entities.ServiceResponse<string>> ConfirmEnrollSubmission([FromBody] ConfirmEnrollSubmissionVM req)
        {
            string authorization = HttpContext.Request.Headers["Authorization"];
            if (string.IsNullOrEmpty(req.NIK) || (string.IsNullOrEmpty(req.VerifiedByNpp) && (authorization == null)))
            {
                return new Core.Entities.ServiceResponse<string>
                {
                    Status = (int)ServiceResponseStatus.EMPTY_PARAMETER,
                    Message = nameof(ServiceResponseStatus.EMPTY_PARAMETER),
                    Data = string.IsNullOrEmpty(req.VerifiedByNpp) ? "verified by npp is required" : "nik is required"
                };
            }

            if (authorization != null)
            {
                var token = authorization.Split(" ")[1];
                var claims = TokenManager.GetPrincipal(token);
                req.UpdatedById = int.Parse(claims.PegawaiId);
                req.VerifiedByNpp = claims.NIK;
            }

            var res = await _enrollmentService.ConfirmSubmission(req);

            return new Core.Entities.ServiceResponse<string>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = nameof(ServiceResponseStatus.SUKSES),
                Data = res
            };
        }

        /// <summary>
        /// To load data submissions log history (comment)
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet("submission_history")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponses<Tbl_LogHistoryPengajuanVM>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "To load data submissions log history (comment)")]
        public async Task<Core.Entities.ServiceResponses<Tbl_LogHistoryPengajuanVM>> LoadDataHistoryPengajuan([FromQuery] HistorySubmissionFilterVM req)
        {
            if (req.DataKTPId == 0)
            {
                return new Core.Entities.ServiceResponses<Tbl_LogHistoryPengajuanVM>
                {
                    Status = (int)ServiceResponseStatus.EMPTY_PARAMETER,
                    Message = nameof(ServiceResponseStatus.EMPTY_PARAMETER)
                };
            }

            var res = await _enrollmentKTPRepository.LoadDataHistoryPengajuan(req);

            return new Core.Entities.ServiceResponses<Tbl_LogHistoryPengajuanVM>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = nameof(ServiceResponseStatus.SUKSES),
                Data = res
            };
        }
    }
}
