using Ekr.Api.DataFingerIso.Filters;
using Ekr.Auth;
using Ekr.Business.Contracts.DataKTP;
using Ekr.Business.Contracts.Recognition;
using Ekr.Core.Constant;
using Ekr.Core.Entities.DataKTP;
using Ekr.Repository.Contracts.Enrollment;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace Ekr.Api.DataFingerIso.Controllers
{
	[Route("matching-encrypt-iso")]
	[ApiController]
	public class MatchingFingerEncryptISOController : ControllerBase
	{
		private readonly IProfileService _profileService;
		private readonly IMatchingFingerService _matchingFingerService;
		private readonly IConfiguration _configuration;
		private readonly IEnrollmentKTPRepository _enrollmentKTPRepository;

		private readonly string ResponseSukses = "";
		private readonly string ResponseNppNotFound = "";
		private readonly string ResponseDataNotFound = "";
		private readonly DateTime RequestTime= DateTime.Now;
		public MatchingFingerEncryptISOController(IProfileService profileService,
			IMatchingFingerService matchingFingerService, IConfiguration configuration,
			IEnrollmentKTPRepository enrollmentKTPRepository)
		{
			_profileService = profileService;
			_matchingFingerService = matchingFingerService;
			_configuration = configuration;
			_enrollmentKTPRepository = enrollmentKTPRepository;

			ResponseSukses = _configuration.GetValue<string>("Response:Sukses");
			ResponseNppNotFound = _configuration.GetValue<string>("Response:NppNotFound");
			ResponseDataNotFound = _configuration.GetValue<string>("Response:DataNotFound");
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

			var obj = await _profileService.GetAuthKTPDataFingerEncOnlyISOWData(req, BaseUrl, EndPoint, RequestTime,"", "");

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
    }
}
