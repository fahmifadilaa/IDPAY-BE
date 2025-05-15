using Ekr.Api.EnrollmentNoMatching.Filters;
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
using Ekr.Repository.DataKTP;
using Ekr.Repository.DataMaster.Utility;
using Ekr.Services.Contracts.Account;
using Ekr.Services.Contracts.Recognition;
using FluentFTP.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using NPOI.POIFS.Crypt.Dsig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Ekr.Api.EnrollmentNoMatching.Controllers.DataEnrollment
{
    [Route("enrollment-encrypt-NoMatching")]
    [ApiController]
    public class EnrollmentEncryptNoMatchingController : ControllerBase
    {
        private readonly IEnrollmentNoMatchingService _enrollmentService;
        private readonly IEnrollmentNoMatchingRepository _enrollmentKTPRepository;
        private readonly ICIFService _cifService;
        private readonly IConfiguration _configuration;
        private readonly IUtilityRepository _utilityRepository;
        private readonly IProfileService _profileService;
        private readonly IProfileRepository _profileRepo;
        private readonly IImageRecognitionService _imageRecognitionService;
        private readonly IProfileRepository _profileRepository;

        private readonly string HostSoa = "";
        private readonly string systemId = "";
        private readonly string idType = "";
        private readonly string teller = "";
        private readonly string branch = "";
        private readonly string BaseUrlNonSoa = "";
        private readonly string EndPointNonSoa = "";
        private readonly bool IsHitSOA = false;
        private readonly string ResponseSukses = "";
        private readonly string ResponseErrorEnrollExistingFR = "";
        private readonly string ResponseErrorEnrollDalamProses = "";
        private readonly string ResponseParameterKosong = "";
        private readonly string ResponseDataKosong = "";
        private readonly string ResponseBelumEnroll = "";
        private readonly string ResponseGagalFR = "";

        public EnrollmentEncryptNoMatchingController(IProfileRepository profileRepository, IEnrollmentNoMatchingService enrollmentService,
            IEnrollmentNoMatchingRepository enrollmentKTPRepository,
            ICIFService cifService,
            IConfiguration configuration, IUtilityRepository utilityRepository, IProfileService profileService, IProfileRepository profileRepo, IImageRecognitionService imageRecognitionService)
        {
            _enrollmentService = enrollmentService;
            _profileRepository = profileRepository;
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
            ResponseErrorEnrollExistingFR = _configuration.GetValue<string>("Response:ErrorPernahEnroll");
            ResponseErrorEnrollDalamProses = _configuration.GetValue<string>("Response:ErrorEnrollDalamProses");
            ResponseParameterKosong = _configuration.GetValue<string>("Response:ErrorParameterKosong");
            ResponseDataKosong = _configuration.GetValue<string>("Response:DataKosong");
            ResponseBelumEnroll = _configuration.GetValue<string>("Response:DataBelumEnroll");
            ResponseGagalFR = _configuration.GetValue<string>("Response:ResponseGagalFR");
            _imageRecognitionService = imageRecognitionService;
        }

        /// <summary>
        /// To Create KTP data non employee encrypted
        /// </summary>
        /// <param name="enroll"></param>
        /// <returns></returns>
        [HttpPost("submit-finger-enc-only-nomatching")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse<EnrollKTP>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "submit-finger-enc-only-nomatching")]
        public async Task<Core.Entities.ServiceResponse<EnrollKTP>> SubmitEnrollFingerEncOnlyISO(EnrollKTPNoMatching enroll)
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
                var (msg, code, cif) = await _enrollmentService.SubmitEnrollmentFingerEncryptedOnlyNoMatching(IsHitSOA, reqSOA, enroll, 24, "tes", "0001", 1, 3);
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

            var res = await _enrollmentService.SubmitEnrollmentFingerEncryptedOnlyNoMatching(IsHitSOA, reqSOA, enroll, int.Parse(claims.PegawaiId),
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
        /// To Updates KTP data non employee encrypted 
        /// </summary>
        /// <param name="enroll"></param>
        /// <returns></returns>
        [HttpPost("resubmit-finger-enc-only-nomatching")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse<EnrollKTP>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "To Updates KTP data non employee encrypted no matching")]
        public async Task<Core.Entities.ServiceResponse<EnrollKTP>> ReSubmitEnrollFingerEncOnlyISO(EnrollKTPNoMatching enroll)
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
                var (msg, code, cif) = await _enrollmentService.ReSubmitEnrollmentFingerEncryptedOnlyNoMatching(IsHitSOA, reqSOA, enroll, 24, "tes", "0001", 1);
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

            var res = await _enrollmentService.ReSubmitEnrollmentFingerEncryptedOnlyNoMatching(IsHitSOA, reqSOA, enroll, int.Parse(claims.PegawaiId),
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

        /// <summary>
        /// To Create KTP data non employee encrypted
        /// </summary>
        /// <param name="enroll"></param>
        /// <returns></returns>
        [HttpPost("submit-finger-enc-only-nomatching-v2")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse<EnrollKTP>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "submit-finger-enc-only-nomatching-v2")]
        public async Task<Core.Entities.ServiceResponse<EnrollKTP>> SubmitEnrollFingerEncOnlyISOv2(EnrollKTPNoMatchingv2 enroll)
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
                var (msg, code, cif) = await _enrollmentService.SubmitEnrollmentFingerEncryptedOnlyNoMatchingv2(IsHitSOA, reqSOA, enroll, 24, "tes", "0001", 1, 3);
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

            var res = await _enrollmentService.SubmitEnrollmentFingerEncryptedOnlyNoMatchingv2(IsHitSOA, reqSOA, enroll, int.Parse(claims.PegawaiId),
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
        /// To Updates KTP data non employee encrypted 
        /// </summary>
        /// <param name="enroll"></param>
        /// <returns></returns>
        [HttpPost("resubmit-finger-enc-only-nomatching-v2")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse<EnrollKTP>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "To Updates KTP data non employee encrypted no matching v2")]
        public async Task<Core.Entities.ServiceResponse<EnrollKTP>> ReSubmitEnrollFingerEncOnlyISOv2(EnrollKTPNoMatchingv2 enroll)
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
                var (msg, code, cif) = await _enrollmentService.ReSubmitEnrollmentFingerEncryptedOnlyNoMatchingv2(IsHitSOA, reqSOA, enroll, 24, "tes", "0001", 1);
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

            var res = await _enrollmentService.ReSubmitEnrollmentFingerEncryptedOnlyNoMatchingv2(IsHitSOA, reqSOA, enroll, int.Parse(claims.PegawaiId),
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

        [HttpPost("enroll-nomatching-list")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse<GridResponse<SettingThresholdData>>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Dashboard Enrollment No Matching List")]
        public async Task<Core.Entities.ServiceResponse<GridResponse<EnrollNoMatchingData>>> GetEnrollNoMatchingListAsync(EnrollNoMatchingFilter filter)
        {
            if (filter?.PageSize == null)
            {
                return new Core.Entities.ServiceResponse<GridResponse<EnrollNoMatchingData>>
                {
                    Message = ResponseParameterKosong,
                    Status = (int)ServiceResponseStatus.EMPTY_PARAMETER
                };
            }

            string authorization = HttpContext.Request.Headers["Authorization"];

            if (!string.IsNullOrWhiteSpace(authorization))
            {
                var token = authorization.Split(" ")[1];

                var claims = TokenManager.GetPrincipal(token);

                //filter.LoginRoleId = int.Parse(claims.RoleUnitId == "x" ? claims.RoleId : claims.RoleUnitId);
                //filter.LoginUnitId = int.Parse(claims.UnitId == "x" ? "1" : claims.UnitId);
            }

            GridResponse<EnrollNoMatchingData> res = await _enrollmentKTPRepository.GetPengajuanNoMatchingList(filter, 0);

            return new Core.Entities.ServiceResponse<GridResponse<EnrollNoMatchingData>>
            {
                Data = res,
                Message = ResponseSukses,
                Status = (int)ServiceResponseStatus.SUKSES
            };
        }

        /// <summary>
        /// Setting threshold for user
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        [HttpPost("enroll-nomatching-list-by-user")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse<GridResponse<EnrollNoMatchingData>>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Dashboard Enrollment No Matching List By User")]
        public async Task<Core.Entities.ServiceResponse<GridResponse<EnrollNoMatchingData>>> GetEnrollNoMatchingListByUserAsync(EnrollNoMatchingFilter filter)
        {
            if (filter?.PageSize == null)
            {
                return new Core.Entities.ServiceResponse<GridResponse<EnrollNoMatchingData>>
                {
                    Message = ResponseParameterKosong,
                    Status = (int)ServiceResponseStatus.EMPTY_PARAMETER
                };
            }

            string authorization = HttpContext.Request.Headers["Authorization"];
            var currentUserId = 0;

            if (!string.IsNullOrWhiteSpace(authorization))
            {
                var token = authorization.Split(" ")[1];

                var claims = TokenManager.GetPrincipal(token);

                currentUserId = int.Parse(claims.PegawaiId);
            }

            var res = await _enrollmentKTPRepository.GetPengajuanNoMatchingList(filter, currentUserId);

            var data = new List<EnrollNoMatchingData>();


            foreach (var i in res.Data)
            {
                data.Add(new EnrollNoMatchingData
                {
                    Number = i.Number,
                    Id = i.Id,
                    NoPengajuan = i.NoPengajuan,
                    Nama = i.Nama,
                    NIK = i.NIK,
                    CIF = i.CIF,
                    TempatLahir = i.TempatLahir,
                    TanggalLahir = i.TanggalLahir,
                    JenisKelamin = i.JenisKelamin,
                    AlamatLengkap = i.AlamatLengkap,
                    PathFile = ConvertUrlToB64FingerEncOnly(i.PathFile),
                    File = ConvertUrlToB64FingerEncOnly(i.File),
                    CreatedTime = i.CreatedTime,
                    EnrollBy = i.EnrollBy,
                    StatusPengajuan = i.StatusPengajuan,
                    StatusData = i.StatusData,
                    CreatedBy = i.EnrollBy,
                    PenyeliaName = i.PenyeliaName,
                    PemimpinName = i.PemimpinName,
                    UnitName = i.UnitName
                });
            }

            res.Data = data;

            return new Core.Entities.ServiceResponse<GridResponse<EnrollNoMatchingData>>
            {
                Data = res,
                Message = ResponseSukses,
                Status = (int)ServiceResponseStatus.SUKSES
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

            var obj = await _profileService.GetKTPDataFingerEncOnlyNoMatching(nik);

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
        /// Get data profile by NIK
        /// </summary>
        /// <param name="nik"></param>
        /// <returns></returns>
        [HttpGet("get-profile-by-nopengajuan-finger-enc-only")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse<ProfileByNik>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Get data profile by No Pengajuan")]
        public async Task<Core.Entities.ServiceResponse<ProfileByNik>> GetProfileFingerByNoPengajuanEncOnly(string noPengajuan, string? IpAddress, string? Browser, string? Url)
        {
            if (noPengajuan == "")
            {
                return new Core.Entities.ServiceResponse<ProfileByNik>
                {
                    Message = nameof(ServiceResponseStatus.EMPTY_PARAMETER),
                    Status = (int)ServiceResponseStatus.EMPTY_PARAMETER
                };
            }

            var nik = await _profileRepo.GetNikNoMatchingByNoPengajuan(noPengajuan);

            if (nik == null || nik == "")
            {
                return new Core.Entities.ServiceResponse<ProfileByNik>
                {
                    Message = nameof(ServiceResponseStatus.Data_Empty),
                    Status = (int)ServiceResponseStatus.Data_Empty
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

            var obj = await _profileService.GetKTPDataFingerEncOnlyNoMatching(nik);

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

        [HttpGet("get-profile-by-idpengajuan-finger-enc-only")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse<ProfileByNik>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Get data profile by Id Pengajuan")]
        public async Task<Core.Entities.ServiceResponse<ProfileByNik>> GetProfileFingerByIdPengajuanEncOnly(int id, string? IpAddress, string? Browser, string? Url)
        {
            if (id == 0)
            {
                return new Core.Entities.ServiceResponse<ProfileByNik>
                {
                    Message = nameof(ServiceResponseStatus.EMPTY_PARAMETER),
                    Status = (int)ServiceResponseStatus.EMPTY_PARAMETER
                };
            }

            var nik = await _profileRepo.GetNikNoMatchingByIdPengajuan(id);

            if (nik == null || nik == "")
            {
                return new Core.Entities.ServiceResponse<ProfileByNik>
                {
                    Message = nameof(ServiceResponseStatus.Data_Empty),
                    Status = (int)ServiceResponseStatus.Data_Empty
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

            var obj = await _profileService.GetKTPDataFingerEncOnlyNoMatchingNew(nik,id);

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

        [HttpPost("update-status")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse<string>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Enrollment No Matching Update Status")]
        public async Task<Core.Entities.ServiceResponse<string>> UpdateNoMatchingStatus([FromBody] EnrollNoMatchingStatusRequest req)
        {
            string authorization = HttpContext.Request.Headers["Authorization"];

            if (string.IsNullOrWhiteSpace(authorization))
            {
                await _enrollmentService.UpdateEnrollNoMatchingStatusAsync(req, 24, "tes", "0001", 1, 3);
                
                return new Core.Entities.ServiceResponse<string>
                {
                    Message = ResponseSukses,
                    Status = (int)ServiceResponseStatus.SUKSES
                };
            }

            var token = authorization.Split(" ")[1];
            var claims = TokenManager.GetPrincipal(token);

            //var NIK = await _profileRepository.GetNikNoMatchingByIdPengajuan(req.Id).ConfigureAwait(false);
            //bool res = await _enrollmentKTPRepository.GetEnrollwithourFRAsync(NIK);
            //if (res)
            //{
            //    return new Core.Entities.ServiceResponse<string>
            //    {
            //        Status = (int)ServiceResponseStatus.ERROR,
            //        Message = ResponseErrorEnrollExistingFR
            //    };
            //}


            await _enrollmentService.UpdateEnrollNoMatchingStatusAsync(req, int.Parse(claims.PegawaiId), claims.NIK, claims.KodeUnit, int.Parse(claims.UnitId), int.Parse(claims.RoleId));

            return new Core.Entities.ServiceResponse<string>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = ResponseSukses
            };
        }

        [HttpPost("get-probability-division")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse<string>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Enrollment No Matching Update Status")]
        public async Task<Core.Entities.ServiceResponse<string>> GetProbabilityDivision(string nik)
        {
            int pegawaiId = 100;
            string authorization = HttpContext.Request.Headers["Authorization"];
            var data = "";

            if (string.IsNullOrWhiteSpace(authorization))
            {
                data = await _enrollmentKTPRepository.GetProbabilityDivision(nik);

                return new Core.Entities.ServiceResponse<string>
                {
                    Message = ResponseSukses,
                    Status = (int)ServiceResponseStatus.SUKSES
                };
            }

            var token = authorization.Split(" ")[1];
            var claims = TokenManager.GetPrincipal(token);

            data = await _enrollmentKTPRepository.GetProbabilityDivision(nik);


            return new Core.Entities.ServiceResponse<string>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = ResponseSukses,
                Data = data

            };
        }

        [HttpPost("get-probability-division-v2")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse<string>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Enrollment No Matching Get Probability Division")]
        public async Task<Core.Entities.ServiceResponse<string>> GetProbabilityDivisionv2(string tipeTreshold)
        {
            int pegawaiId = 100;
            string authorization = HttpContext.Request.Headers["Authorization"];
            var data = "";

            if (string.IsNullOrWhiteSpace(authorization))
            {
                data = await _enrollmentKTPRepository.GetProbabilityDivisionV2(tipeTreshold);

                return new Core.Entities.ServiceResponse<string>
                {
                    Message = ResponseSukses,
                    Status = (int)ServiceResponseStatus.SUKSES,
                    Data = data
                };
            }

            var token = authorization.Split(" ")[1];
            var claims = TokenManager.GetPrincipal(token);

            data = await _enrollmentKTPRepository.GetProbabilityDivisionV2(tipeTreshold);


            return new Core.Entities.ServiceResponse<string>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = ResponseSukses,
                Data = data

            };
        }

        [HttpPost("matching-face-api")]
        [ProducesResponseType(typeof(ServiceResponseFR<FaceRecogResponse>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "matching face for enrollment no matching")]
        public async Task<ServiceResponseFR<FaceRecogResponse>> MatchingFace(FaceRecogRequest req)
        {
            int pegawaiId = 100;
            string authorization = HttpContext.Request.Headers["Authorization"];
            var data = new FaceRecogResponse();

            var env = _configuration.GetValue<bool>("UrlFaceRecognition:IsProd");

            var BaseUrl = _configuration.GetValue<string>("UrlFaceRecognition:BaseUrl");
            var EndPoint = _configuration.GetValue<string>("UrlFaceRecognition:FaceRecogBase64");



            if (string.IsNullOrWhiteSpace(authorization))
            {
                data = await _enrollmentService.MatchImageBase64ToBase64(req, new UrlRequestRecognitionFR
                {
                    BaseUrl = BaseUrl,
                    EndPoint = EndPoint,
                    Env = env
                });

                return new ServiceResponseFR<FaceRecogResponse>
                {
                    Message = ResponseSukses,
                    Status = (int)ServiceResponseStatus.SUKSES,
                    Data = data
                };
            }

            var token = authorization.Split(" ")[1];
            var claims = TokenManager.GetPrincipal(token);

            data = await _enrollmentService.MatchImageBase64ToBase64(req, new UrlRequestRecognitionFR
            {
                BaseUrl = BaseUrl,
                EndPoint = EndPoint,
                Env = env
            });


            return new ServiceResponseFR<FaceRecogResponse>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = ResponseSukses,
                Data = data

            };
        }

        [HttpPost("matching-face-api-v2")]
        [ProducesResponseType(typeof(ServiceResponseFR<FaceRecogResponseV2>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "matching face for enrollment no matching")]
        public async Task<ServiceResponseFR<FaceRecogResponseV2>> MatchingFaceV2(FaceRecogRequestV2 req)
        {
            int pegawaiId = 100;
            string authorization = HttpContext.Request.Headers["Authorization"];
            var data = new FaceRecogResponseV2();

            var env = _configuration.GetValue<bool>("UrlFaceRecognition:IsProd");

            var BaseUrl = _configuration.GetValue<string>("UrlFaceRecognition:BaseUrl");
            var EndPoint = _configuration.GetValue<string>("UrlFaceRecognition:FaceRecogBase64");

            req.channel = _configuration.GetValue<string>("UrlFaceRecognition:Channel");

            var treshold = _configuration.GetValue<decimal>("UrlFaceRecognition:ThresholdValue");


            if (string.IsNullOrWhiteSpace(authorization))
            {
                data = await _enrollmentService.MatchImageBase64ToBase64FRV2(req, new UrlRequestRecognitionFR
                {
                    BaseUrl = BaseUrl,
                    EndPoint = EndPoint,
                    Env = env
                });

                return new ServiceResponseFR<FaceRecogResponseV2>
                {
                    Message = ResponseSukses,
                    Status = (int)ServiceResponseStatus.SUKSES,
                    Data = data
                };
            }

            var token = authorization.Split(" ")[1];
            var claims = TokenManager.GetPrincipal(token);

            data = await _enrollmentService.MatchImageBase64ToBase64FRV2(req, new UrlRequestRecognitionFR
            {
                BaseUrl = BaseUrl,
                EndPoint = EndPoint,
                Env = env
            });

            if (data.status.ToLower().Contains("true") && decimal.Parse(data.selfie_photo.Replace(".", ",")) >= treshold)
            {
                return new ServiceResponseFR<FaceRecogResponseV2>
                {
                    Status = (int)ServiceResponseStatus.SUKSES,
                    Message = ResponseSukses,
                    Data = data

                };
            }

            data.status = "False";
            return new ServiceResponseFR<FaceRecogResponseV2>
            {
                Status = (int)ServiceResponseStatus.ERROR,
                Message = ResponseGagalFR,
                Data = data

            };
        }

        [HttpGet("get-pemimpin")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse<GridResponse<DataDropdownServerSide>>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Enroll No Matching Get List Pemimpin")]
        public async Task<Core.Entities.ServiceResponse<GridResponse<DataDropdownServerSide>>> GetListPemimpin([FromQuery] int unitId)
        {
            GridResponse<DataDropdownServerSide> res = await _enrollmentKTPRepository.GetListPemimpin(unitId);

            return new Core.Entities.ServiceResponse<GridResponse<DataDropdownServerSide>>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = ResponseSukses,
                Data = res
            };
        }

        /// <summary>
        /// Get list penyelia
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet("get-penyelia")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse<GridResponse<DataDropdownServerSide>>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Enroll No Matching Get List Penyelia")]
        public async Task<Core.Entities.ServiceResponse<GridResponse<DataDropdownServerSide>>> GetListPenyelia([FromQuery] int unitId)
        {
            GridResponse<DataDropdownServerSide> res = await _enrollmentKTPRepository.GetListPenyelia(unitId);

            return new Core.Entities.ServiceResponse<GridResponse<DataDropdownServerSide>>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = ResponseSukses,
                Data = res
            };
        }

        [HttpPost("matching-finger")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse<bool>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Matching Finger Encrypt")]
        public async Task<Core.Entities.ServiceResponse<bool>> MatchFinger([FromBody] MatchingFingerReq req)
        {
            var RequestTime = DateTime.Now;
            var BaseUrl = "";
            var EndPoint = "";
            int? match = 0;

            if (req.isIso)
            {
                BaseUrl = _configuration.GetValue<string>("UrlImageRecognition:BaseUrl");
                EndPoint = _configuration.GetValue<string>("UrlImageRecognition:MatchImageBase64ISOToBase64ISO");
            }
            else {
                BaseUrl = _configuration.GetValue<string>("UrlImageRecognition:BaseUrl");
                EndPoint = _configuration.GetValue<string>("UrlImageRecognition:MatchImageBase64ToBase64");
            }


            var matchingRes = await _imageRecognitionService.MatchImageBase64ToBase64(
                            new Base64ToBase64Req
                            {
                                Base64Images1 = req.BaseImg64,
                                Base64Images2 = req.BaseImg642
                            },
                            new UrlRequestRecognition
                            {
                                BaseUrl = BaseUrl,
                                EndPoint = EndPoint
                            })
                            .ConfigureAwait(false);

            if (matchingRes?.Data?.IsFoundOrSuccess == true) match = 1;

            return new Core.Entities.ServiceResponse<bool>
            {
                Data = match == 1? true : false,
                Message = match == 1 ? "Finger Match!" : "Finger Not Match!",
                Status = match == 1 ? (int)ServiceResponseStatus.ERROR : (int)ServiceResponseStatus.SUKSES,
            };
        }

        /// <summary>
        /// Untuk Load Data finger enc only with Nik Mandatory
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        [HttpPost("temp-grid-finger-enc-only")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse<GridResponse<InboxEnrollNoMatchingData>>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Enrollment No Matching Inbox")]
        public async Task<Core.Entities.ServiceResponse<GridResponse<InboxEnrollNoMatchingData>>> GetDataTempEncOnly(DataEnrollTempFilter filter)
        {
            if (filter?.PageSize == null)
            {
                return new Core.Entities.ServiceResponse<GridResponse<InboxEnrollNoMatchingData>>
                {
                    Message = nameof(ServiceResponseStatus.EMPTY_PARAMETER),
                    Status = (int)ServiceResponseStatus.EMPTY_PARAMETER
                };
            }

            string authorization = HttpContext.Request.Headers["Authorization"];

            if (!string.IsNullOrWhiteSpace(authorization))
            {
                var token = authorization.Split(" ")[1];

                var claims = TokenManager.GetPrincipal(token);

                filter.UnitIds = claims.UnitId == "x" ? "1" : claims.UnitId;
            }

            var res = await _enrollmentKTPRepository.GetDBEnroll(filter);

            var data = new List<InboxEnrollNoMatchingData>();

            foreach (var i in res.Data)
            {
                data.Add(new InboxEnrollNoMatchingData
                {
                    AlamatLengkap = i.AlamatLengkap,
                    CreatedTime = i.CreatedTime,
                    File = ConvertUrlToB64FingerEncOnly(i.File),
                    Id = i.Id,
                    JenisKelamin = i.JenisKelamin,
                    Nama = i.Nama,
                    NIK = i.NIK,
                    Number = i.Number,
                    PathFile = ConvertUrlToB64FingerEncOnly(i.PathFile),
                    TanggalLahir = i.TanggalLahir,
                    TempatLahir = i.TempatLahir,
                    EnrollBy = i.EnrollBy,
                    StatusData = i.StatusData,
                    CIF = i.CIF
                });
            }

            res.Data = data;

            return new Core.Entities.ServiceResponse<GridResponse<InboxEnrollNoMatchingData>>
            {
                Data = res,
                Message = nameof(ServiceResponseStatus.SUKSES),
                Status = (int)ServiceResponseStatus.SUKSES
            };
        }

        /// <summary>
        /// Get list pemimpin
        /// </summary>
        /// <param name="thresho">Threshold Id</param>
        /// <returns></returns>
        [HttpGet("get-Enrollment-No-Matching-log")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse<GridResponse<TblEnrollNoMatchingLogVM>>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Enroll No Matching Log")]
        public async Task<Core.Entities.ServiceResponse<GridResponse<TblEnrollNoMatchingLogVM>>> GetEnrollNoMatchingLogAsync(int id)
        {
            GridResponse<TblEnrollNoMatchingLogVM> res = await _enrollmentKTPRepository.GetEnrollNoMatchingLogAsync(id);

            return new Core.Entities.ServiceResponse<GridResponse<TblEnrollNoMatchingLogVM>>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = ResponseSukses,
                Data = res
            };
        }

        [HttpGet("validasi-existing-enroll-fr")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Validasi existing enroll yang tidak melalui FR")]
        public async Task<Core.Entities.ServiceResponse> GetEnrollWithoutFRAsync(string nik)
        {
            #region check if already enrolled
            //bool conclusion = new bool();
            //bool res = await _enrollmentKTPRepository.GetEnrollwithourFRAsync(nik);
            //if (res)
            //{
            //    conclusion = false;
            //    return new Core.Entities.ServiceResponse
            //    {
            //        Status = Convert.ToInt32(conclusion),
            //        Message = ResponseErrorEnrollExistingFR
            //    };
            //}

            bool resProsesFR = await _enrollmentKTPRepository.GetEnrollStatusFRAsync(nik);

            return new Core.Entities.ServiceResponse
            {
                Status = Convert.ToInt32(resProsesFR),
                Message = Convert.ToInt32(resProsesFR) == 1 ? ResponseSukses : ResponseErrorEnrollDalamProses
            };
            #endregion
                       
        }

        //[HttpGet("validasi-status-enroll-fr")]
        //[ProducesResponseType(typeof(Core.Entities.ServiceResponse), 200)]
        //[ProducesResponseType(500)]
        //[LogActivity(Keterangan = "Validasi Exisiting Enroll FR yang sedang Proses")]
        //public async Task<Core.Entities.ServiceResponse> GetEnrollStatusFRAsync(string nik)
        //{
        //    bool res = await _enrollmentKTPRepository.GetEnrollStatusFRAsync(nik);

        //    return new Core.Entities.ServiceResponse
        //    {
        //        Status = Convert.ToInt32(res),
        //        Message = ResponseSukses
        //    };
        //}

        [HttpPost("crawling-nik")]
        [ProducesResponseType(typeof(CrawlingSubContent), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Crawling Dukcapil")]
        public async Task<CrawlingSubContent> CrawlingNik(CrawlingRequest req)
        {
            int pegawaiId = 100;
            string authorization = HttpContext.Request.Headers["Authorization"];
            var data = new CrawlingSubContent();

            var env = _configuration.GetValue<bool>("UrlCrawlingDukcapil:IsProd");

            var BaseUrl = _configuration.GetValue<string>("UrlCrawlingDukcapil:BaseUrl");
            var EndPoint = _configuration.GetValue<string>("UrlCrawlingDukcapil:Crawling");

            var BaseUrlDEV = _configuration.GetValue<string>("UrlCrawlingDukcapil:BaseUrlDEV");
            var EndPointDEV = _configuration.GetValue<string>("UrlCrawlingDukcapil:CrawlingDEV");

            var Header = _configuration.GetValue<string>("UrlCrawlingDukcapil:HeaderName");
            var HeaderValue = _configuration.GetValue<string>("UrlCrawlingDukcapil:HeaderValue");
            var HeaderValueDev = _configuration.GetValue<string>("UrlCrawlingDukcapil:HeaderValueDev");

            if (string.IsNullOrWhiteSpace(authorization))
            {
                data = await _enrollmentService.CrawlingDukcapilHIT(req, new UrlRequestCrawlingDukcapil
                {
                    BaseUrl = BaseUrl,
                    EndPoint = EndPoint,
                    BaseUrlDEV = BaseUrlDEV,
                    EndPointDEV= EndPointDEV,
                    HeaderName = Header,
                    HeaderValue = HeaderValue,
                    HeaderValueDev = HeaderValueDev,
                    Env = env
                });

                return data;
            }

            var token = authorization.Split(" ")[1];
            var claims = TokenManager.GetPrincipal(token);

            data = await _enrollmentService.CrawlingDukcapilHIT(req, new UrlRequestCrawlingDukcapil
            {
                BaseUrl = BaseUrl,
                EndPoint = EndPoint,
                BaseUrlDEV = BaseUrlDEV,
                EndPointDEV = EndPointDEV,
                HeaderName = Header,
                HeaderValue = HeaderValue,
                HeaderValueDev = HeaderValueDev,
                Env = env
            });


            return data;
        }

        [HttpPost("scan-qr-ikd")]
        [ProducesResponseType(typeof(ServiceResponseFR<ScanResponse>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "scan qr ikd for enrollment no matching")]
        public async Task<ScanResponse> ScanQRIKD(ScanQRIKDV2Req req)
        {
            string authorization = HttpContext.Request.Headers["Authorization"];
            var data = new ScanResponse();

            var env = _configuration.GetValue<bool>("UrlScanIKD:IsProd");

            var BaseUrl = _configuration.GetValue<string>("UrlScanIKD:BaseUrl");
            var EndPoint = _configuration.GetValue<string>("UrlScanIKD:EndPoint");

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
            }

            var token = authorization.Split(" ")[1];
            var claims = TokenManager.GetPrincipal(token);

            data = await _enrollmentService.ScanQRIKD(req, new UrlRequestRecognitionFR
            {
                BaseUrl = BaseUrl,
                EndPoint = EndPoint,
                Env = env
            });


            return data;
        }

        [HttpPost("scan-qr-ikd-xml")]
        [ProducesResponseType(typeof(ServiceResponseFR<FaceRecogResponse>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "scan qr ikd XML for enrollment no matching")]
        public async Task<ScanResponse> ScanQRIKDXML(ScanQRIKDReq req)
        {
            string authorization = HttpContext.Request.Headers["Authorization"];
            var data = new ScanResponse();

            var env = _configuration.GetValue<bool>("UrlScanIKD:IsProd");

            var BaseUrl = _configuration.GetValue<string>("UrlScanIKDXML:BaseUrl");
            var EndPoint = _configuration.GetValue<string>("UrlScanIKDXML:EndPoint");


            var APIKEY = _configuration.GetValue<string>("UrlScanIKDXML:api_key");
            var CLIENTKEy = _configuration.GetValue<string>("UrlScanIKDXML:client_key");
            var channel = _configuration.GetValue<string>("UrlScanIKDXML:channel");

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

        private static string ConvertUrlToB64FingerEncOnly(string path)
        {
            try
            {
                using WebClient webClient = new();

                var b64 = webClient.DownloadData(path ?? "");

                return Convert.ToBase64String(b64);
            }
            catch (Exception)
            {
                return "";
            }
        }
    }
}
