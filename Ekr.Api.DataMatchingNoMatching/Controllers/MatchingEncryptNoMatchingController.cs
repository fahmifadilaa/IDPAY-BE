using Ekr.Api.DataMatchingNoMatching.Filters;
using Ekr.Auth;
using Ekr.Business.Contracts.DataKTP;
using Ekr.Business.Contracts.Enrollment;
using Ekr.Business.Contracts.EnrollmentNoMatching;
using Ekr.Business.DataKTP;
using Ekr.Business.Recognition;
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
using NPOI.HSSF.Record.CF;
using NPOI.POIFS.Crypt.Dsig;
using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Ekr.Api.DataMatchingNoMatching.Controllers
{
    [Route("matching-encrypt-NoMatching")]
    [ApiController]
    public class MatchingEncryptNoMatchingController : ControllerBase
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
        private readonly string ResponseNppNotFound = "";
        private readonly string ResponseGagalFR = "";

        private readonly string ResponseDataNotFound = "";
        private readonly DateTime RequestTime = DateTime.Now;

        public MatchingEncryptNoMatchingController(IEnrollmentNoMatchingService enrollmentService,
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
            ResponseNppNotFound = _configuration.GetValue<string>("Response:NppNotFound");
            ResponseGagalFR = _configuration.GetValue<string>("Response:ResponseGagalFR");
            _imageRecognitionService = imageRecognitionService;


            ResponseDataNotFound = _configuration.GetValue<string>("Response:DataNotFound");
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
            var threshold = _configuration.GetValue<string>("UrlFaceRecognition:ThresholdValue");

            if (req.photoThresholdDukcapil == null) { 
                req.photoThresholdDukcapil = threshold;
            }

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

        [HttpPost("finger-loop-bynpp-bool-iso")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse<bool>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Matching Finger Encrypt ISO")]
        public async Task<Core.Entities.ServiceResponse<object>> MatchFingerLoopNppBoolISO([FromBody] ProfileLoopNppReq req)
        {
            var RequestTime = DateTime.Now;
            var BaseUrl = _configuration.GetValue<string>("UrlImageRecognition:BaseUrl");
            var EndPoint = _configuration.GetValue<string>("UrlImageRecognition:MatchImageBase64ISOToBase64ISO");

            var mapping = await _enrollmentKTPRepository.MappingNppNik(req.Npp);
            if (mapping == null)
            {
                return new Core.Entities.ServiceResponse<object>
                {
                    Message = ResponseNppNotFound,
                    Status = (int)ServiceResponseStatus.SUKSES,
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

            var obj = await _profileService.IsFingerNppIsoMatch(_req, BaseUrl, EndPoint, RequestTime, req.Npp, req.UnitCode);

            return new Core.Entities.ServiceResponse<object>
            {
                Data = obj.Status.Equals("sukses") ? true : false,
                Message = obj.Message,
                Status = obj.Status.Equals("error") ? (int)ServiceResponseStatus.ERROR : obj.Status.Equals("Empty") ? (int)ServiceResponseStatus.EMPTY_PARAMETER : (int)ServiceResponseStatus.SUKSES,
            };
        }

        [HttpPost("finger-loop-bynpp-bool-iso-db")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse<bool>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Matching Finger Encrypt ISO DB Vers")]
        public async Task<Core.Entities.ServiceResponse<object>> MatchFingerLoopNppBoolISODB([FromBody] ProfileLoopNppReq req)
        {
            var RequestTime = DateTime.Now;
            var BaseUrl = _configuration.GetValue<string>("UrlImageRecognition:BaseUrl");
            var EndPoint = _configuration.GetValue<string>("UrlImageRecognition:MatchImageBase64ISOToBase64ISO");

            var mapping = await _enrollmentKTPRepository.MappingNppNik(req.Npp);
            if (mapping == null)
            {
                return new Core.Entities.ServiceResponse<object>
                {
                    Message = ResponseNppNotFound,
                    Status = (int)ServiceResponseStatus.SUKSES,
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

            var obj = await _profileService.IsFingerNppIsoMatchDB(_req, BaseUrl, EndPoint, RequestTime, req.Npp, req.UnitCode);

            return new Core.Entities.ServiceResponse<object>
            {
                Data = obj.Status.Equals("sukses") ? true : false,
                Message = obj.Message,
                Status = obj.Status.Equals("error") ? (int)ServiceResponseStatus.ERROR : obj.Status.Equals("Empty") ? (int)ServiceResponseStatus.EMPTY_PARAMETER : (int)ServiceResponseStatus.SUKSES,
            };
        }

        [HttpPost("profile-bycif-data")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse<ProfileByNikOnlyImg>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Matching Finger Get Data By CIF")]
        public async Task<Core.Entities.ServiceResponse<ProfileByNikOnlyImg>> GetProfileByCifImgOnly([FromBody] ProfileLoopByCifReq req)
        {
            var requestTime = DateTime.Now;

            var BaseUrl = _configuration.GetValue<string>("UrlImageRecognition:BaseUrl");
            var EndPoint = _configuration.GetValue<string>("UrlImageRecognition:MatchImageBase64ISOToBase64ISO");

            var obj = await _profileService.GetAuthKTPDataByCifFingerEncOnlyIso(req, BaseUrl, EndPoint, requestTime);

            if (obj == null)
            {
                return new Core.Entities.ServiceResponse<ProfileByNikOnlyImg>
                {
                    Message = obj.Message,
                    Status = obj.Status.Equals("error") ? (int)ServiceResponseStatus.ERROR : (int)ServiceResponseStatus.SUKSES,
                    Data = null
                };
            }

            var resp = new ProfileByNikOnlyImg
            {
                Id = obj.Data.Id,
                ktp_NIK = obj.Data.ktp_NIK,
                ktp_CIF = obj.Data.ktp_CIF,
                ktp_Nama = obj.Data.ktp_Nama,
                ktp_TempatLahir = obj.Data?.ktp_TempatLahir,
                ktp_TanggalLahir = obj.Data?.ktp_TanggalLahir,
                ktp_TTL = obj.Data?.ktp_TTL,
                ktp_JanisKelamin = obj.Data?.ktp_JanisKelamin,
                ktp_GolonganDarah = obj.Data?.ktp_GolonganDarah,
                ktp_Alamat = obj.Data?.ktp_Alamat,
                ktp_RT = obj.Data?.ktp_RT,
                ktp_RW = obj.Data?.ktp_RW,
                ktp_RTRW = obj.Data?.ktp_RTRW,
                ktp_Kelurahan = obj.Data?.ktp_Kelurahan,
                Desa = obj.Data?.Desa,
                ktp_Kecamatan = obj.Data?.ktp_Kecamatan,
                ktp_Kota = obj.Data?.ktp_Kota,
                ktp_Provinsi = obj.Data?.ktp_Provinsi,
                ktp_Agama = obj.Data?.ktp_Agama,
                ktp_KodePos = obj.Data?.ktp_KodePos,
                ktp_Latitude = obj.Data?.ktp_Latitude,
                ktp_Longitude = obj.Data?.ktp_Longitude,
                ktp_StatusPerkawinan = obj.Data?.ktp_StatusPerkawinan,
                ktp_Pekerjaan = obj.Data?.ktp_Pekerjaan,
                ktp_Kewarganegaraan = obj.Data?.ktp_Kewarganegaraan,
                ktp_MasaBerlaku = obj.Data?.ktp_MasaBerlaku,
                ktp_AlamatConvertLengkap = obj.Data?.ktp_AlamatConvertLengkap,
                ktp_AlamatConvertLatlong = obj.Data?.ktp_AlamatConvertLatlong,
                ktp_PhotoCam = obj.Data?.ktp_PhotoCam,
                ktp_Signature = obj.Data?.ktp_Signature,
                ktp_PhotoKTP = obj.Data?.ktp_PhotoKTP,
                RequestedImg = obj.Data?.RequestedImg,
                ErrorMsg = obj.Data?.ErrorMsg
            };

            return new Core.Entities.ServiceResponse<ProfileByNikOnlyImg>
            {
                Message = obj.Message,
                Status = obj.Status.Equals("error") ? (int)ServiceResponseStatus.ERROR : obj.Status.Equals("Empty") ? (int)ServiceResponseStatus.EMPTY_PARAMETER : (int)ServiceResponseStatus.SUKSES,
                Data = resp
            };
        }

        [HttpPost("profile-bycif-data-db")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse<ProfileByNikOnlyImg>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Matching Finger Get Data By CIF")]
        public async Task<Core.Entities.ServiceResponse<ProfileByNikOnlyImg>> GetProfileByCifImgOnlyDB([FromBody] ProfileLoopByCifReq req)
        {
            var requestTime = DateTime.Now;

            var BaseUrl = _configuration.GetValue<string>("UrlImageRecognition:BaseUrl");
            var EndPoint = _configuration.GetValue<string>("UrlImageRecognition:MatchImageBase64ISOToBase64ISO");

            var obj = await _profileService.GetAuthKTPDataByCifFingerEncOnlyIsoDb(req, BaseUrl, EndPoint, requestTime);

            if (obj == null)
            {
                return new Core.Entities.ServiceResponse<ProfileByNikOnlyImg>
                {
                    Message = obj.Message,
                    Status = obj.Status.Equals("error") ? (int)ServiceResponseStatus.ERROR : (int)ServiceResponseStatus.SUKSES,
                    Data = null
                };
            }

            var resp = new ProfileByNikOnlyImg
            {
                Id = obj.Data.Id,
                ktp_NIK = obj.Data.ktp_NIK,
                ktp_CIF = obj.Data.ktp_CIF,
                ktp_Nama = obj.Data.ktp_Nama,
                ktp_TempatLahir = obj.Data?.ktp_TempatLahir,
                ktp_TanggalLahir = obj.Data?.ktp_TanggalLahir,
                ktp_TTL = obj.Data?.ktp_TTL,
                ktp_JanisKelamin = obj.Data?.ktp_JanisKelamin,
                ktp_GolonganDarah = obj.Data?.ktp_GolonganDarah,
                ktp_Alamat = obj.Data?.ktp_Alamat,
                ktp_RT = obj.Data?.ktp_RT,
                ktp_RW = obj.Data?.ktp_RW,
                ktp_RTRW = obj.Data?.ktp_RTRW,
                ktp_Kelurahan = obj.Data?.ktp_Kelurahan,
                Desa = obj.Data?.Desa,
                ktp_Kecamatan = obj.Data?.ktp_Kecamatan,
                ktp_Kota = obj.Data?.ktp_Kota,
                ktp_Provinsi = obj.Data?.ktp_Provinsi,
                ktp_Agama = obj.Data?.ktp_Agama,
                ktp_KodePos = obj.Data?.ktp_KodePos,
                ktp_Latitude = obj.Data?.ktp_Latitude,
                ktp_Longitude = obj.Data?.ktp_Longitude,
                ktp_StatusPerkawinan = obj.Data?.ktp_StatusPerkawinan,
                ktp_Pekerjaan = obj.Data?.ktp_Pekerjaan,
                ktp_Kewarganegaraan = obj.Data?.ktp_Kewarganegaraan,
                ktp_MasaBerlaku = obj.Data?.ktp_MasaBerlaku,
                ktp_AlamatConvertLengkap = obj.Data?.ktp_AlamatConvertLengkap,
                ktp_AlamatConvertLatlong = obj.Data?.ktp_AlamatConvertLatlong,
                ktp_PhotoCam = obj.Data?.ktp_PhotoCam,
                ktp_Signature = obj.Data?.ktp_Signature,
                ktp_PhotoKTP = obj.Data?.ktp_PhotoKTP,
                RequestedImg = obj.Data?.RequestedImg,
                ErrorMsg = obj.Data?.ErrorMsg
            };

            return new Core.Entities.ServiceResponse<ProfileByNikOnlyImg>
            {
                Message = obj.Message,
                Status = obj.Status.Equals("error") ? (int)ServiceResponseStatus.ERROR : obj.Status.Equals("Empty") ? (int)ServiceResponseStatus.EMPTY_PARAMETER : (int)ServiceResponseStatus.SUKSES,
                Data = resp
            };
        }

        [HttpPost("profile-bynik-data")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse<ProfileByNikOnlyImg>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Matching Finger Get Data By CIF")]
        public async Task<Core.Entities.ServiceResponse<ProfileByNikOnlyImg>> GetProfileImgOnly([FromBody] ProfileLoopReq req)
        {
            var BaseUrl = _configuration.GetValue<string>("UrlImageRecognition:BaseUrl");
            var EndPoint = _configuration.GetValue<string>("UrlImageRecognition:MatchImageBase64ISOToBase64ISO");

            var obj = await _profileService.GetAuthKTPDataFingerEncOnlyISOFileWData(req, BaseUrl, EndPoint, RequestTime, "", "");

            if (obj == null)
            {
                return new Core.Entities.ServiceResponse<ProfileByNikOnlyImg>
                {
                    Message = obj.Message,
                    Status = obj.Status.Equals("error") ? (int)ServiceResponseStatus.ERROR : (int)ServiceResponseStatus.SUKSES,
                    Data = null
                };
            }

            var resp = new ProfileByNikOnlyImg
            {
                Id = obj.Data.Id,
                ktp_NIK = obj.Data.ktp_NIK,
                ktp_CIF = obj.Data.ktp_CIF,
                ktp_Nama = obj.Data.ktp_Nama,
                ktp_TempatLahir = obj.Data?.ktp_TempatLahir,
                ktp_TanggalLahir = obj.Data?.ktp_TanggalLahir,
                ktp_TTL = obj.Data?.ktp_TTL,
                ktp_JanisKelamin = obj.Data?.ktp_JanisKelamin,
                ktp_GolonganDarah = obj.Data?.ktp_GolonganDarah,
                ktp_Alamat = obj.Data?.ktp_Alamat,
                ktp_RT = obj.Data?.ktp_RT,
                ktp_RW = obj.Data?.ktp_RW,
                ktp_RTRW = obj.Data?.ktp_RTRW,
                ktp_Kelurahan = obj.Data?.ktp_Kelurahan,
                Desa = obj.Data?.Desa,
                ktp_Kecamatan = obj.Data?.ktp_Kecamatan,
                ktp_Kota = obj.Data?.ktp_Kota,
                ktp_Provinsi = obj.Data?.ktp_Provinsi,
                ktp_Agama = obj.Data?.ktp_Agama,
                ktp_KodePos = obj.Data?.ktp_KodePos,
                ktp_Latitude = obj.Data?.ktp_Latitude,
                ktp_Longitude = obj.Data?.ktp_Longitude,
                ktp_StatusPerkawinan = obj.Data?.ktp_StatusPerkawinan,
                ktp_Pekerjaan = obj.Data?.ktp_Pekerjaan,
                ktp_Kewarganegaraan = obj.Data?.ktp_Kewarganegaraan,
                ktp_MasaBerlaku = obj.Data?.ktp_MasaBerlaku,
                ktp_AlamatConvertLengkap = obj.Data?.ktp_AlamatConvertLengkap,
                ktp_AlamatConvertLatlong = obj.Data?.ktp_AlamatConvertLatlong,
                ktp_PhotoCam = obj.Data?.ktp_PhotoCam,
                ktp_Signature = obj.Data?.ktp_Signature,
                ktp_PhotoKTP = obj.Data?.ktp_PhotoKTP,
                RequestedImg = obj.Data?.RequestedImg,
                ErrorMsg = obj.Data?.ErrorMsg
            };

            return new Core.Entities.ServiceResponse<ProfileByNikOnlyImg>
            {
                Message = obj.Message,
                Status = obj.Status.Equals("error") ? (int)ServiceResponseStatus.ERROR : (int)ServiceResponseStatus.SUKSES,
                Data = resp
            };
        }

        [HttpPost("profile-bynik-data-db")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse<ProfileByNikOnlyImg>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Matching Finger Get Data By CIF")]
        public async Task<Core.Entities.ServiceResponse<ProfileByNikOnlyImg>> GetProfileImgOnlyDB([FromBody] ProfileLoopReq req)
        {
            var BaseUrl = _configuration.GetValue<string>("UrlImageRecognition:BaseUrl");
            var EndPoint = _configuration.GetValue<string>("UrlImageRecognition:MatchImageBase64ISOToBase64ISO");

            string authorization = HttpContext.Request.Headers["Authorization"];

            //if (!string.IsNullOrWhiteSpace(authorization))
            //{
            //	var token = authorization.Split(" ")[1];

            //	var claims = TokenManager.GetPrincipalAgent(token);

            //	req.UnitCode = claims.KodeUnit;
            //	req = claims.RoleUnitId == "x" ? claims.RoleId : claims.RoleUnitId;
            //}

            var obj = await _profileService.GetAuthKTPDataFingerEncOnlyISOWData(req, BaseUrl, EndPoint, RequestTime, "", "");

            if (obj == null)
            {
                return new Core.Entities.ServiceResponse<ProfileByNikOnlyImg>
                {
                    Message = obj.Message,
                    Status = obj.Status.Equals("error") ? (int)ServiceResponseStatus.ERROR : (int)ServiceResponseStatus.SUKSES,
                    Data = null
                };
            }

            var resp = new ProfileByNikOnlyImg
            {
                Id = obj.Data.Id,
                ktp_NIK = obj.Data.ktp_NIK,
                ktp_CIF = obj.Data.ktp_CIF,
                ktp_Nama = obj.Data.ktp_Nama,
                ktp_TempatLahir = obj.Data?.ktp_TempatLahir,
                ktp_TanggalLahir = obj.Data?.ktp_TanggalLahir,
                ktp_TTL = obj.Data?.ktp_TTL,
                ktp_JanisKelamin = obj.Data?.ktp_JanisKelamin,
                ktp_GolonganDarah = obj.Data?.ktp_GolonganDarah,
                ktp_Alamat = obj.Data?.ktp_Alamat,
                ktp_RT = obj.Data?.ktp_RT,
                ktp_RW = obj.Data?.ktp_RW,
                ktp_RTRW = obj.Data?.ktp_RTRW,
                ktp_Kelurahan = obj.Data?.ktp_Kelurahan,
                Desa = obj.Data?.Desa,
                ktp_Kecamatan = obj.Data?.ktp_Kecamatan,
                ktp_Kota = obj.Data?.ktp_Kota,
                ktp_Provinsi = obj.Data?.ktp_Provinsi,
                ktp_Agama = obj.Data?.ktp_Agama,
                ktp_KodePos = obj.Data?.ktp_KodePos,
                ktp_Latitude = obj.Data?.ktp_Latitude,
                ktp_Longitude = obj.Data?.ktp_Longitude,
                ktp_StatusPerkawinan = obj.Data?.ktp_StatusPerkawinan,
                ktp_Pekerjaan = obj.Data?.ktp_Pekerjaan,
                ktp_Kewarganegaraan = obj.Data?.ktp_Kewarganegaraan,
                ktp_MasaBerlaku = obj.Data?.ktp_MasaBerlaku,
                ktp_AlamatConvertLengkap = obj.Data?.ktp_AlamatConvertLengkap,
                ktp_AlamatConvertLatlong = obj.Data?.ktp_AlamatConvertLatlong,
                ktp_PhotoCam = obj.Data?.ktp_PhotoCam,
                ktp_Signature = obj.Data?.ktp_Signature,
                ktp_PhotoKTP = obj.Data?.ktp_PhotoKTP,
                RequestedImg = obj.Data?.RequestedImg,
                ErrorMsg = obj.Data?.ErrorMsg
            };

            return new Core.Entities.ServiceResponse<ProfileByNikOnlyImg>
            {
                Message = obj.Message,
                Status = obj.Status.Equals("error") ? (int)ServiceResponseStatus.ERROR : (int)ServiceResponseStatus.SUKSES,
                Data = resp
            };
        }

        [HttpPost("profile-bynpp-data")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse<ProfileByNikOnlyImg>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Matching Finger Get Data By NPP")]
        public async Task<Core.Entities.ServiceResponse<ProfileByNikOnlyImg>> GetProfileByNppImgOnly([FromBody] ProfileLoopNppReq req)
        {
            var BaseUrl = _configuration.GetValue<string>("UrlImageRecognition:BaseUrl");
            var EndPoint = _configuration.GetValue<string>("UrlImageRecognition:MatchImageBase64ISOToBase64ISO");

            var mapping = await _enrollmentKTPRepository.MappingNppNik(req.Npp);
            if (mapping == null)
            {
                return new Core.Entities.ServiceResponse<ProfileByNikOnlyImg>
                {
                    Message = ResponseNppNotFound,
                    Status = (int)ServiceResponseStatus.SUKSES,
                    Data = null,
                    Code = 0
                };
            }
            var reqs = new ProfileLoopReq
            {
                Npp = req.Npp,
                Base64Img = req.Base64Img,
                EndPoint = EndPoint,
                UnitCode = req.UnitCode,
                Branch = req.Branch,
                ClientApps = req.ClientApps,
                LvTeller = req.LvTeller,
                Nik = mapping.Nik,
                SubBranch = req.SubBranch
            };

            var obj = await _profileService.GetAuthKTPDataFingerEncOnlyISOFileWData(reqs, BaseUrl, EndPoint, RequestTime, "", "");

            if (obj == null)
            {
                return new Core.Entities.ServiceResponse<ProfileByNikOnlyImg>
                {
                    Message = obj.Message,
                    Status = obj.Status.Equals("error") ? (int)ServiceResponseStatus.ERROR : (int)ServiceResponseStatus.SUKSES,
                    Data = null
                };
            }

            var resp = new ProfileByNikOnlyImg
            {
                Id = obj.Data.Id,
                ktp_NIK = obj.Data.ktp_NIK,
                ktp_CIF = obj.Data.ktp_CIF,
                ktp_Nama = obj.Data.ktp_Nama,
                ktp_TempatLahir = obj.Data?.ktp_TempatLahir,
                ktp_TanggalLahir = obj.Data?.ktp_TanggalLahir,
                ktp_TTL = obj.Data?.ktp_TTL,
                ktp_JanisKelamin = obj.Data?.ktp_JanisKelamin,
                ktp_GolonganDarah = obj.Data?.ktp_GolonganDarah,
                ktp_Alamat = obj.Data?.ktp_Alamat,
                ktp_RT = obj.Data?.ktp_RT,
                ktp_RW = obj.Data?.ktp_RW,
                ktp_RTRW = obj.Data?.ktp_RTRW,
                ktp_Kelurahan = obj.Data?.ktp_Kelurahan,
                Desa = obj.Data?.Desa,
                ktp_Kecamatan = obj.Data?.ktp_Kecamatan,
                ktp_Kota = obj.Data?.ktp_Kota,
                ktp_Provinsi = obj.Data?.ktp_Provinsi,
                ktp_Agama = obj.Data?.ktp_Agama,
                ktp_KodePos = obj.Data?.ktp_KodePos,
                ktp_Latitude = obj.Data?.ktp_Latitude,
                ktp_Longitude = obj.Data?.ktp_Longitude,
                ktp_StatusPerkawinan = obj.Data?.ktp_StatusPerkawinan,
                ktp_Pekerjaan = obj.Data?.ktp_Pekerjaan,
                ktp_Kewarganegaraan = obj.Data?.ktp_Kewarganegaraan,
                ktp_MasaBerlaku = obj.Data?.ktp_MasaBerlaku,
                ktp_AlamatConvertLengkap = obj.Data?.ktp_AlamatConvertLengkap,
                ktp_AlamatConvertLatlong = obj.Data?.ktp_AlamatConvertLatlong,
                ktp_PhotoCam = obj.Data?.ktp_PhotoCam,
                ktp_Signature = obj.Data?.ktp_Signature,
                ktp_PhotoKTP = obj.Data?.ktp_PhotoKTP,
                RequestedImg = obj.Data?.RequestedImg,
                ErrorMsg = obj.Data?.ErrorMsg
            };

            return new Core.Entities.ServiceResponse<ProfileByNikOnlyImg>
            {
                Message = obj.Message,
                Status = obj.Status.Equals("error") ? (int)ServiceResponseStatus.ERROR : (int)ServiceResponseStatus.SUKSES,
                Data = resp
            };
        }

        [HttpPost("profile-bynpp-data-db")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse<ProfileByNikOnlyImg>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Matching Finger Get Data By NPP")]
        public async Task<Core.Entities.ServiceResponse<ProfileByNikOnlyImg>> GetProfileByNppImgOnlyDB([FromBody] ProfileByNppReq req)
        {
            var BaseUrl = _configuration.GetValue<string>("UrlImageRecognition:BaseUrl");
            var EndPoint = _configuration.GetValue<string>("UrlImageRecognition:MatchImageBase64ISOToBase64ISO");

            var mapping = await _enrollmentKTPRepository.MappingNppNik(req.Npp);
            if (mapping == null)
            {
                return new Core.Entities.ServiceResponse<ProfileByNikOnlyImg>
                {
                    Message = ResponseNppNotFound,
                    Status = (int)ServiceResponseStatus.SUKSES,
                    Data = null,
                    Code = 0
                };
            }

            var reqs = new ProfileLoopReq
            {
                Npp = req.Npp,
                Base64Img = req.Base64Img,
                EndPoint = EndPoint,
                UnitCode = req.UnitCode,
                Branch = req.Branch,
                ClientApps = req.ClientApps,
                LvTeller = req.LvTeller,
                Nik = mapping.Nik,
                SubBranch = req.SubBranch
            };

            var obj = await _profileService.GetAuthKTPDataFingerEncOnlyISOWData(reqs, BaseUrl, EndPoint, RequestTime, "", "");

            if (obj == null)
            {
                return new Core.Entities.ServiceResponse<ProfileByNikOnlyImg>
                {
                    Message = obj.Message,
                    Status = obj.Status.Equals("error") ? (int)ServiceResponseStatus.ERROR : (int)ServiceResponseStatus.SUKSES,
                    Data = null
                };
            }

            var resp = new ProfileByNikOnlyImg
            {
                Id = obj.Data.Id,
                ktp_NIK = obj.Data.ktp_NIK,
                ktp_CIF = obj.Data.ktp_CIF,
                ktp_Nama = obj.Data.ktp_Nama,
                ktp_TempatLahir = obj.Data?.ktp_TempatLahir,
                ktp_TanggalLahir = obj.Data?.ktp_TanggalLahir,
                ktp_TTL = obj.Data?.ktp_TTL,
                ktp_JanisKelamin = obj.Data?.ktp_JanisKelamin,
                ktp_GolonganDarah = obj.Data?.ktp_GolonganDarah,
                ktp_Alamat = obj.Data?.ktp_Alamat,
                ktp_RT = obj.Data?.ktp_RT,
                ktp_RW = obj.Data?.ktp_RW,
                ktp_RTRW = obj.Data?.ktp_RTRW,
                ktp_Kelurahan = obj.Data?.ktp_Kelurahan,
                Desa = obj.Data?.Desa,
                ktp_Kecamatan = obj.Data?.ktp_Kecamatan,
                ktp_Kota = obj.Data?.ktp_Kota,
                ktp_Provinsi = obj.Data?.ktp_Provinsi,
                ktp_Agama = obj.Data?.ktp_Agama,
                ktp_KodePos = obj.Data?.ktp_KodePos,
                ktp_Latitude = obj.Data?.ktp_Latitude,
                ktp_Longitude = obj.Data?.ktp_Longitude,
                ktp_StatusPerkawinan = obj.Data?.ktp_StatusPerkawinan,
                ktp_Pekerjaan = obj.Data?.ktp_Pekerjaan,
                ktp_Kewarganegaraan = obj.Data?.ktp_Kewarganegaraan,
                ktp_MasaBerlaku = obj.Data?.ktp_MasaBerlaku,
                ktp_AlamatConvertLengkap = obj.Data?.ktp_AlamatConvertLengkap,
                ktp_AlamatConvertLatlong = obj.Data?.ktp_AlamatConvertLatlong,
                ktp_PhotoCam = obj.Data?.ktp_PhotoCam,
                ktp_Signature = obj.Data?.ktp_Signature,
                ktp_PhotoKTP = obj.Data?.ktp_PhotoKTP,
                RequestedImg = obj.Data?.RequestedImg,
                ErrorMsg = obj.Data?.ErrorMsg
            };

            return new Core.Entities.ServiceResponse<ProfileByNikOnlyImg>
            {
                Message = obj.Message,
                Status = obj.Status.Equals("error") ? (int)ServiceResponseStatus.ERROR : (int)ServiceResponseStatus.SUKSES,
                Data = resp
            };
        }

        /// <summary>
        /// Get data profile if finger match without finger type
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("get-profile-loop-finger-enc-only-fr")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse<ProfileByNik>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Get data profile for Enrollment FR")]
        public async Task<Core.Entities.ServiceResponse<ProfileByNik>> GetProfileAuthLoopFingerEncOnlyFR([FromBody] ProfileFRReq req)
        {
            var RequestTime = DateTime.Now;

            var obj = await _profileService.GetAuthKTPDataFingerEncOnlyFR(req, RequestTime, req.Npp, req.UnitCode);

            return new Core.Entities.ServiceResponse<ProfileByNik>
            {
                Message = obj.Message,
                Status = obj.Status.Equals("error") ? (int)ServiceResponseStatus.ERROR : (int)ServiceResponseStatus.SUKSES,
                Data = obj.Data
            };
        }


    }
}
