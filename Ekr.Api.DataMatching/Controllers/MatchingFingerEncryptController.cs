using Ekr.Api.DataMatching.Filters;
using Ekr.Auth;
using Ekr.Business.Contracts.DataKTP;
using Ekr.Business.Contracts.Recognition;
using Ekr.Core.Constant;
using Ekr.Core.Entities.DataKTP;
using Ekr.Core.Securities.Symmetric;
using Ekr.Repository.Contracts.Enrollment;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace Ekr.Api.DataMatching.Controllers
{
    [Route("matching-encrypt")]
    [ApiController]
    public class MatchingFingerEncryptController : ControllerBase
    {
        private readonly IProfileService _profileService;
        private readonly IMatchingFingerService _matchingFingerService;
        private readonly IConfiguration _configuration;
        private readonly IEnrollmentKTPRepository _enrollmentKTPRepository;

        public MatchingFingerEncryptController(IProfileService profileService,
            IMatchingFingerService matchingFingerService, IConfiguration configuration,
            IEnrollmentKTPRepository enrollmentKTPRepository)
        {
            _profileService = profileService;
            _matchingFingerService = matchingFingerService;
            _configuration = configuration;
            _enrollmentKTPRepository = enrollmentKTPRepository;
        }

        [HttpPost("profile")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse<ProfileByNik>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<Core.Entities.ServiceResponse<ProfileByNik>> GetProfile([FromBody] ProfileReq req)
        {
            var BaseUrl = _configuration.GetValue<string>("UrlImageRecognition:BaseUrl");
            var EndPoint = _configuration.GetValue<string>("UrlImageRecognition:MatchImageBase64ToBase64");

            var obj = await _profileService.GetAuthKTPDataFingerEncOnly(req.Base64Img, req.Nik, req.FingerType, BaseUrl, EndPoint);

            return new Core.Entities.ServiceResponse<ProfileByNik>
            {
                Message = obj.Message,
                Status = obj.Status.Equals("error") ? (int)ServiceResponseStatus.ERROR : (int)ServiceResponseStatus.SUKSES,
                Data = obj.Data
            };
        }

        [HttpPost("profile-imgOnly")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse<ProfileByNikOnlyImg>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<Core.Entities.ServiceResponse<ProfileByNikOnlyImg>> GetProfileImgOnly([FromBody] ProfileReq req)
        {
            var BaseUrl = _configuration.GetValue<string>("UrlImageRecognition:BaseUrl");
            var EndPoint = _configuration.GetValue<string>("UrlImageRecognition:MatchImageBase64ToBase64");

            var obj = await _profileService.GetAuthKTPDataFingerEncOnly(req.Base64Img, req.Nik, req.FingerType, BaseUrl, EndPoint);

            if(obj == null)
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
                ktp_TanggalLahir = obj.Data.ktp_TanggalLahir,
                ktp_TTL = obj.Data?.ktp_TTL,
                ktp_JanisKelamin = obj.Data.ktp_JanisKelamin,
                ktp_GolonganDarah = obj.Data.ktp_GolonganDarah,
                ktp_Alamat = obj.Data.ktp_Alamat,
                ktp_RT = obj.Data.ktp_RT,
                ktp_RW = obj.Data.ktp_RW,
                ktp_RTRW = obj.Data.ktp_RTRW,
                ktp_Kelurahan = obj.Data.ktp_Kelurahan,
                Desa = obj.Data.Desa,
                ktp_Kecamatan = obj.Data.ktp_Kecamatan,
                ktp_Kota = obj.Data.ktp_Kota,
                ktp_Provinsi = obj.Data.ktp_Provinsi,
                ktp_Agama = obj.Data.ktp_Agama,
                ktp_KodePos = obj.Data.ktp_KodePos,
                ktp_Latitude = obj.Data.ktp_Latitude,
                ktp_Longitude = obj.Data.ktp_Longitude,
                ktp_StatusPerkawinan = obj.Data.ktp_StatusPerkawinan,
                ktp_Pekerjaan = obj.Data.ktp_Pekerjaan,
                ktp_Kewarganegaraan = obj.Data.ktp_Kewarganegaraan,
                ktp_MasaBerlaku = obj.Data.ktp_MasaBerlaku,
                ktp_AlamatConvertLengkap = obj.Data.ktp_AlamatConvertLengkap,
                ktp_AlamatConvertLatlong = obj.Data.ktp_AlamatConvertLatlong,
                ktp_PhotoCam = obj.Data.ktp_PhotoCam,
                ktp_Signature = obj.Data.ktp_Signature,
                ktp_PhotoKTP = obj.Data.ktp_PhotoKTP,
                RequestedImg = obj.Data.RequestedImg,
                ErrorMsg = obj.Data.ErrorMsg
            };

            return new Core.Entities.ServiceResponse<ProfileByNikOnlyImg>
            {
                Message = obj.Message,
                Status = obj.Status.Equals("error") ? (int)ServiceResponseStatus.ERROR : (int)ServiceResponseStatus.SUKSES,
                Data = resp
            };
        }

        [HttpPost("profile-bynpp")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse<ProfileByNik>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<Core.Entities.ServiceResponse<ProfileByNik>> GetProfileByNpp([FromBody] ProfileByNppReq req)
        {
            var requestTime = DateTime.Now;
            var BaseUrl = _configuration.GetValue<string>("UrlImageRecognition:BaseUrl");
            var EndPoint = _configuration.GetValue<string>("UrlImageRecognition:MatchImageBase64ToBase64");

            var mapping = await _enrollmentKTPRepository.MappingNppNik(req.Npp);
            if (mapping == null)
            {
                return new Core.Entities.ServiceResponse<ProfileByNik>
                {
                    Message = "Npp Tidak Ditemukan",
                    Status = (int)ServiceResponseStatus.SUKSES,
                    Data = null,
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

            var obj = await _profileService.GetAuthKTPDataFingerEmpEncOnly(_req, BaseUrl, EndPoint, requestTime, req.NppRequester, req.UnitCode);

            return new Core.Entities.ServiceResponse<ProfileByNik>
            {
                Message = obj.Message,
                Status = obj.Status.Equals("error") ? (int)ServiceResponseStatus.ERROR : (int)ServiceResponseStatus.SUKSES,
                Data = obj.Data
            };
        }

        [HttpPost("profile-byCif-imgOnly")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse<ProfileByNikOnlyImg>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<Core.Entities.ServiceResponse<ProfileByNikOnlyImg>> GetProfileByCifImgOnly([FromBody] ProfileLoopByCifReq req)
        {
            var requestTime = DateTime.Now;

            var BaseUrl = _configuration.GetValue<string>("UrlImageRecognition:BaseUrl");
            var EndPoint = _configuration.GetValue<string>("UrlImageRecognition:MatchImageBase64ToBase64");

            var obj = await _profileService.GetAuthKTPDataByCifFingerEncOnly(req, BaseUrl, EndPoint, requestTime);

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
                ktp_TanggalLahir = obj.Data.ktp_TanggalLahir,
                ktp_TTL = obj.Data?.ktp_TTL,
                ktp_JanisKelamin = obj.Data.ktp_JanisKelamin,
                ktp_GolonganDarah = obj.Data.ktp_GolonganDarah,
                ktp_Alamat = obj.Data.ktp_Alamat,
                ktp_RT = obj.Data.ktp_RT,
                ktp_RW = obj.Data.ktp_RW,
                ktp_RTRW = obj.Data.ktp_RTRW,
                ktp_Kelurahan = obj.Data.ktp_Kelurahan,
                Desa = obj.Data.Desa,
                ktp_Kecamatan = obj.Data.ktp_Kecamatan,
                ktp_Kota = obj.Data.ktp_Kota,
                ktp_Provinsi = obj.Data.ktp_Provinsi,
                ktp_Agama = obj.Data.ktp_Agama,
                ktp_KodePos = obj.Data.ktp_KodePos,
                ktp_Latitude = obj.Data.ktp_Latitude,
                ktp_Longitude = obj.Data.ktp_Longitude,
                ktp_StatusPerkawinan = obj.Data.ktp_StatusPerkawinan,
                ktp_Pekerjaan = obj.Data.ktp_Pekerjaan,
                ktp_Kewarganegaraan = obj.Data.ktp_Kewarganegaraan,
                ktp_MasaBerlaku = obj.Data.ktp_MasaBerlaku,
                ktp_AlamatConvertLengkap = obj.Data.ktp_AlamatConvertLengkap,
                ktp_AlamatConvertLatlong = obj.Data.ktp_AlamatConvertLatlong,
                ktp_PhotoCam = obj.Data.ktp_PhotoCam,
                ktp_Signature = obj.Data.ktp_Signature,
                ktp_PhotoKTP = obj.Data.ktp_PhotoKTP,
                RequestedImg = obj.Data.RequestedImg,
                ErrorMsg = obj.Data.ErrorMsg
            };

            return new Core.Entities.ServiceResponse<ProfileByNikOnlyImg>
            {
                Message = obj.Message,
                Status = obj.Status.Equals("error") ? (int)ServiceResponseStatus.ERROR : obj.Status.Equals("Empty") ? (int)ServiceResponseStatus.EMPTY_PARAMETER : (int)ServiceResponseStatus.SUKSES,
                Data = resp

            };
        }

        [HttpPost("profile-byCif-bool")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse<ProfileByNikOnlyImg>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<Core.Entities.ServiceResponse<bool>> GetProfileByCifBoolOnly([FromBody] ProfileLoopByCifReq req)
        {
            var requestTime = DateTime.Now;

            var BaseUrl = _configuration.GetValue<string>("UrlImageRecognition:BaseUrl");
            var EndPoint = _configuration.GetValue<string>("UrlImageRecognition:MatchImageBase64ToBase64");

            var obj = await _profileService.IsFingerByCifMatch(req, BaseUrl, EndPoint, requestTime);

            return new Core.Entities.ServiceResponse<bool>
            {
                Data = obj.Status.Equals("sukses") ? true : false,
                Message = obj.Message,
                Status = obj.Status.Equals("error") ? (int)ServiceResponseStatus.ERROR : obj.Status.Equals("Empty") ? (int)ServiceResponseStatus.EMPTY_PARAMETER : (int)ServiceResponseStatus.SUKSES,
            };
        }

        [HttpPost("profile-bynik-employee")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse<ProfileByNik>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<Core.Entities.ServiceResponse<ProfileByNik>> GetProfileByNikEmp([FromBody] ProfileReq req)
        {
            var BaseUrl = _configuration.GetValue<string>("UrlImageRecognition:BaseUrl");
            var EndPoint = _configuration.GetValue<string>("UrlImageRecognition:MatchImageBase64ToBase64");

            var obj = await _profileService.GetAuthKTPDataFingerEncOnly(req.Base64Img, req.Nik, req.FingerType, BaseUrl, EndPoint);

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
            var requestTime = DateTime.Now;
            var BaseUrl = _configuration.GetValue<string>("UrlImageRecognition:BaseUrl");
            var EndPoint = _configuration.GetValue<string>("UrlImageRecognition:MatchImageBase64ToBase64");

            var obj = await _matchingFingerService.MatchFinger(req, BaseUrl, EndPoint, requestTime, req.Npp, req.UnitCode);

            return new Core.Entities.ServiceResponse<string>
            {
                Message = obj.msg,
                Status = (int)ServiceResponseStatus.SUKSES,
                Data = obj.msg,
                Code = obj.status.Equals("error") ? 1 : 0
            };
        }

        [HttpPost("finger-loop-bool")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse<bool>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<Core.Entities.ServiceResponse<bool>> MatchFingerLoopBool([FromBody] ProfileLoopReq req)
        {
            var RequestTime = DateTime.Now;
            var BaseUrl = _configuration.GetValue<string>("UrlImageRecognition:BaseUrl");
            var EndPoint = _configuration.GetValue<string>("UrlImageRecognition:MatchImageBase64ToBase64");

            var obj = await _matchingFingerService.IsMatchFinger(req, BaseUrl, EndPoint, RequestTime, req.Npp, req.UnitCode);

            return new Core.Entities.ServiceResponse<bool>
            {
                Message = nameof(ServiceResponseStatus.SUKSES),
                Status = (int)ServiceResponseStatus.SUKSES,
                Data = obj,
                Code = obj ? 1 : 0
            };
        }

        [HttpPost("finger-loop-bynik-employee")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse<string>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<Core.Entities.ServiceResponse<string>> MatchFingerLoopEmp([FromBody] ProfileLoopReq req)
        {
            var RequestTime = DateTime.Now;
            var BaseUrl = _configuration.GetValue<string>("UrlImageRecognition:BaseUrl");
            var EndPoint = _configuration.GetValue<string>("UrlImageRecognition:MatchImageBase64ToBase64");

            var _req = new ProfileLoopNppReq
            {
                Base64Img = req.Base64Img,
                Branch = req.Branch,
                ClientApps = req.ClientApps,
                LvTeller = req.LvTeller,
                SubBranch = req.SubBranch,
            };

            var obj = await _matchingFingerService.MatchFingerEmp(_req, req.Nik, BaseUrl, EndPoint, RequestTime, req.Npp, req.UnitCode);

            return new Core.Entities.ServiceResponse<string>
            {
                Message = obj.msg,
                Status = (int)ServiceResponseStatus.SUKSES,
                Data = obj.msg,
                Code = obj.status.Equals("error") ? 1 : 0
            };
        }

        [HttpPost("finger-loop-bynik-employee-bool")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse<bool>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<Core.Entities.ServiceResponse<bool>> MatchFingerLoopEmpBool([FromBody] ProfileLoopReq req)
        {
            var RequestTime = DateTime.Now;
            var BaseUrl = _configuration.GetValue<string>("UrlImageRecognition:BaseUrl");
            var EndPoint = _configuration.GetValue<string>("UrlImageRecognition:MatchImageBase64ToBase64");

            var obj = await _matchingFingerService.IsMatchFingerEmp(req, BaseUrl, EndPoint, RequestTime, req.Npp, req.UnitCode);

            return new Core.Entities.ServiceResponse<bool>
            {
                Message = nameof(ServiceResponseStatus.SUKSES),
                Status = (int)ServiceResponseStatus.SUKSES,
                Data = obj,
                Code = obj ? 1 : 0
            };
        }

        [HttpPost("finger-loop-bynpp")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse<ProfileByNik>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<Core.Entities.ServiceResponse<ProfileByNik>> MatchFingerLoopNpp([FromBody] ProfileLoopNppReq req)
        {
            var requestTime = DateTime.Now;
            var BaseUrl = _configuration.GetValue<string>("UrlImageRecognition:BaseUrl");
            var EndPoint = _configuration.GetValue<string>("UrlImageRecognition:MatchImageBase64ToBase64");

            var mapping = await _enrollmentKTPRepository.MappingNppNik(req.Npp);
            if (mapping == null)
            {
                return new Core.Entities.ServiceResponse<ProfileByNik>
                {
                    Message = "Npp Tidak Ditemukan",
                    Status = (int)ServiceResponseStatus.SUKSES,
                    Data = null,
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
                Npp = req.Npp,
                UnitCode = req.UnitCode
            };

            var obj = await _profileService.GetAuthKTPDataFingerEmpEncOnly(_req, BaseUrl, EndPoint, requestTime, req.NppRequester, req.UnitCode);

            return new Core.Entities.ServiceResponse<ProfileByNik>
            {
                Message = obj.Message,
                Status = (int)ServiceResponseStatus.SUKSES,
                Data = obj.Data,
                Code = obj.Status.Equals("error") ? 1 : 0
            };
        }

        [HttpPost("finger-loop-bynpp-bool")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse<bool>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<Core.Entities.ServiceResponse<bool>> MatchFingerLoopNppBool([FromBody] ProfileLoopNppReq req)
        {
            var RequestTime = DateTime.Now;
            var BaseUrl = _configuration.GetValue<string>("UrlImageRecognition:BaseUrl");
            var EndPoint = _configuration.GetValue<string>("UrlImageRecognition:MatchImageBase64ToBase64");

            var mapping = await _enrollmentKTPRepository.MappingNppNik(req.Npp);
            if (mapping == null)
            {
                return new Core.Entities.ServiceResponse<bool>
                {
                    Message = "Npp Tidak Ditemukan",
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
                Npp =req.NppRequester,
                UnitCode = req.UnitCode
            };

            var obj = await _profileService.IsFingerLoopMatch(_req, BaseUrl, EndPoint, RequestTime, req.Npp, req.UnitCode);

            return new Core.Entities.ServiceResponse<bool>
            {
                Data = obj.Status.Equals("sukses") ? true : false,
                Message = obj.Message,
                Status = obj.Status.Equals("error") ? (int)ServiceResponseStatus.ERROR : obj.Status.Equals("Empty") ? (int)ServiceResponseStatus.EMPTY_PARAMETER : (int)ServiceResponseStatus.SUKSES,
            };
        }

        [HttpPost("finger-loop-bynpp-bool-iso")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse<bool>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
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
                    Message = "Npp Tidak Ditemukan",
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

        [HttpPost("finger-loop-bynpp-imgOnly")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse<ProfileByNikOnlyImg>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<Core.Entities.ServiceResponse<ProfileByNikOnlyImg>> MatchFingerLoopNppImgOnly([FromBody] ProfileLoopNppReq req)
        {
            var requestTime = DateTime.Now;
            var BaseUrl = _configuration.GetValue<string>("UrlImageRecognition:BaseUrl");
            var EndPoint = _configuration.GetValue<string>("UrlImageRecognition:MatchImageBase64ToBase64");

            var mapping = await _enrollmentKTPRepository.MappingNppNik(req.Npp);

            if(mapping == null)
			{
                return new Core.Entities.ServiceResponse<ProfileByNikOnlyImg>
                {
                    Message = "Npp Tidak Ditemukan",
                    Status = (int)ServiceResponseStatus.SUKSES,
                    Data = null,
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
                Npp = req.Npp,
                UnitCode = req.UnitCode
            };

            var obj = await _profileService.GetAuthKTPDataFingerEmpEncOnly(_req, BaseUrl, EndPoint, requestTime, req.NppRequester, req.UnitCode);

            if(obj == null)
			{
                return new Core.Entities.ServiceResponse<ProfileByNikOnlyImg>
                {
                    Message = "Data Tidak Ditemukan",
                    Status = (int)ServiceResponseStatus.SUKSES,
                    Data = null,
                    Code = 0
                };
            }

            var resp = new ProfileByNikOnlyImg
            {
                Id = obj.Data.Id,
                ktp_NIK = obj.Data.ktp_NIK,
                ktp_CIF = obj.Data.ktp_CIF,
                ktp_Nama = obj.Data.ktp_Nama,
                ktp_TempatLahir = obj.Data?.ktp_TempatLahir,
                ktp_TanggalLahir = obj.Data.ktp_TanggalLahir,
                ktp_TTL = obj.Data?.ktp_TTL,
                ktp_JanisKelamin = obj.Data.ktp_JanisKelamin,
                ktp_GolonganDarah = obj.Data.ktp_GolonganDarah,
                ktp_Alamat = obj.Data.ktp_Alamat,
                ktp_RT = obj.Data.ktp_RT,
                ktp_RW = obj.Data.ktp_RW,
                ktp_RTRW = obj.Data.ktp_RTRW,
                ktp_Kelurahan = obj.Data.ktp_Kelurahan,
                Desa = obj.Data.Desa,
                ktp_Kecamatan = obj.Data.ktp_Kecamatan,
                ktp_Kota = obj.Data.ktp_Kota,
                ktp_Provinsi = obj.Data.ktp_Provinsi,
                ktp_Agama = obj.Data.ktp_Agama,
                ktp_KodePos = obj.Data.ktp_KodePos,
                ktp_Latitude = obj.Data.ktp_Latitude,
                ktp_Longitude = obj.Data.ktp_Longitude,
                ktp_StatusPerkawinan = obj.Data.ktp_StatusPerkawinan,
                ktp_Pekerjaan = obj.Data.ktp_Pekerjaan,
                ktp_Kewarganegaraan = obj.Data.ktp_Kewarganegaraan,
                ktp_MasaBerlaku = obj.Data.ktp_MasaBerlaku,
                ktp_AlamatConvertLengkap = obj.Data.ktp_AlamatConvertLengkap,
                ktp_AlamatConvertLatlong = obj.Data.ktp_AlamatConvertLatlong,
                ktp_PhotoCam = obj.Data.ktp_PhotoCam,
                ktp_Signature = obj.Data.ktp_Signature,
                ktp_PhotoKTP = obj.Data.ktp_PhotoKTP,
                RequestedImg = obj.Data.RequestedImg,
                ErrorMsg = obj.Data.ErrorMsg
            };

            return new Core.Entities.ServiceResponse<ProfileByNikOnlyImg>
            {
                Message = obj.Message,
                Status = (int)ServiceResponseStatus.SUKSES,
                Data = resp,
                Code = obj.Status.Equals("error") ? 1 : 0
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

        [HttpPost("finger-type-bool")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse<bool>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<Core.Entities.ServiceResponse<bool>> MatchFingerTypeBool([FromBody] ProfileReq req)
        {
            var BaseUrl = _configuration.GetValue<string>("UrlImageRecognition:BaseUrl");
            var EndPoint = _configuration.GetValue<string>("UrlImageRecognition:MatchUrlImagesToBase64Json");

            var obj = await _matchingFingerService.IsMatchFingerType(req.Base64Img, req.Nik, req.FingerType, BaseUrl, EndPoint);

            return new Core.Entities.ServiceResponse<bool>
            {
                Message = nameof(ServiceResponseStatus.SUKSES),
                Status = (int)ServiceResponseStatus.SUKSES,
                Data = obj,
                Code = obj? 1 : 0
            };
        }

        [HttpPost("finger-type-bynik-employee")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse<string>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<Core.Entities.ServiceResponse<string>> MatchFingerTypeEmp([FromBody] ProfileReq req)
        {
            var BaseUrl = _configuration.GetValue<string>("UrlImageRecognition:BaseUrl");
            var EndPoint = _configuration.GetValue<string>("UrlImageRecognition:MatchUrlImagesToBase64Json");

            var obj = await _matchingFingerService.MatchFingerTypeEmp(req.Base64Img, req.Nik, req.FingerType, BaseUrl, EndPoint);

            return new Core.Entities.ServiceResponse<string>
            {
                Message = obj.msg,
                Status = (int)ServiceResponseStatus.SUKSES,
                Data = obj.msg,
                Code = obj.status.Equals("error") ? 1 : 0
            };
        }

        [HttpPost("finger-type-bynik-employee-bool")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse<bool>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<Core.Entities.ServiceResponse<bool>> MatchFingerTypeEmpBool([FromBody] ProfileReq req)
        {
            var BaseUrl = _configuration.GetValue<string>("UrlImageRecognition:BaseUrl");
            var EndPoint = _configuration.GetValue<string>("UrlImageRecognition:MatchUrlImagesToBase64Json");

            var obj = await _matchingFingerService.MatchFingerTypeEmpBool(req.Base64Img, req.Nik, req.FingerType, BaseUrl, EndPoint);

            return new Core.Entities.ServiceResponse<bool>
            {
                Message = nameof(ServiceResponseStatus.SUKSES),
                Status = (int)ServiceResponseStatus.SUKSES,
                Data = obj,
                Code = obj ? 1 : 0
            };
        }

        [HttpPost("finger-type-npp")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse<string>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<Core.Entities.ServiceResponse<string>> MatchFingerTypeNpp([FromBody] ProfileByNppReq req)
        {
            var BaseUrl = _configuration.GetValue<string>("UrlImageRecognition:BaseUrl");
            var EndPoint = _configuration.GetValue<string>("UrlImageRecognition:MatchUrlImagesToBase64Json");

            var mapping = await _enrollmentKTPRepository.MappingNppNik(req.Npp);
            if (mapping == null)
            {
                return new Core.Entities.ServiceResponse<string>
                {
                    Message = "Npp Tidak Ditemukan",
                    Status = (int)ServiceResponseStatus.SUKSES,
                    Data = null,
                    Code = 0
                };
            }

            var obj = await _matchingFingerService.MatchFingerTypeEmp(req.Base64Img, mapping.Nik, req.FingerType, BaseUrl, EndPoint);

            return new Core.Entities.ServiceResponse<string>
            {
                Message = obj.msg,
                Status = (int)ServiceResponseStatus.SUKSES,
                Data = obj.msg,
                Code = obj.status.Equals("error") ? 1 : 0
            };
        }

        [HttpPost("finger-type-npp-bool")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse<string>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<Core.Entities.ServiceResponse<bool>> MatchFingerTypeNppBool([FromBody] ProfileByNppReq req)
        {
            var BaseUrl = _configuration.GetValue<string>("UrlImageRecognition:BaseUrl");
            var EndPoint = _configuration.GetValue<string>("UrlImageRecognition:MatchUrlImagesToBase64Json");

            var mapping = await _enrollmentKTPRepository.MappingNppNik(req.Npp);
            if (mapping == null)
            {
                return new Core.Entities.ServiceResponse<bool>
                {
                    Message = "Npp Tidak Ditemukan",
                    Status = (int)ServiceResponseStatus.SUKSES,
                    Data = false,
                    Code = 0
                };
            }

            var obj = await _matchingFingerService.MatchFingerTypeEmpBool(req.Base64Img, mapping.Nik, req.FingerType, BaseUrl, EndPoint);

            return new Core.Entities.ServiceResponse<bool>
            {
                Message = nameof(ServiceResponseStatus.SUKSES),
                Status = (int)ServiceResponseStatus.SUKSES,
                Data = obj,
                Code = obj ? 1 : 0
            };
        }

        [HttpPost("test-decrypt")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse<TestDecryptRes>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public Core.Entities.ServiceResponse<TestDecryptRes> TestDecrypt([FromBody] TestDecrypt req)
        {
            var dec = req.EncryptedText.Decrypt(Phrase.FileEncryption);

            return new Core.Entities.ServiceResponse<TestDecryptRes>
            {
                Message = "",
                Status = (int)ServiceResponseStatus.SUKSES,
                Data = new TestDecryptRes
                {
                    IsMatch = (dec == req.WantedResult),
                    TextResult = dec
                },
                Code = 0
            };
        }

        [HttpPost("test-encrypt")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse<TestDecryptRes>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public Core.Entities.ServiceResponse<TestDecryptRes> TestEncrypt([FromBody] TestDecrypt req)
        {
            var dec = req.EncryptedText.Encrypt(Phrase.FileEncryption);

            return new Core.Entities.ServiceResponse<TestDecryptRes>
            {
                Message = "",
                Status = (int)ServiceResponseStatus.SUKSES,
                Data = new TestDecryptRes
                {
                    IsMatch = (dec == req.WantedResult),
                    TextResult = dec
                },
                Code = 0
            };
        }
    }
}
