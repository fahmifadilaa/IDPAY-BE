using Ekr.Core.Constant;
using Ekr.Core.Entities;
using Ekr.Core.Entities.DataEnrollment;
using Ekr.Repository.Contracts.DataEnrollment;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Ekr.Auth;
using System.IO;
using Ekr.Core.Securities.Symmetric;
using Ekr.Api.EnrollmentMonitoring.Filters;
using Ekr.Core.Entities.DataMaster.Utility.Entity;
using Ekr.Repository.Contracts.DataMaster.Utility;
using Newtonsoft.Json;

namespace Ekr.Api.EnrollmentMonitoring.Controllers
{
	[Route("enrollTemp")]
    [ApiController]
    public class EnrollmentTempController : ControllerBase
    {
        private readonly IEnrollTempRepository _enrollTempRepository;
        private readonly IUtilityRepository _utilityRepository;
        public EnrollmentTempController(IEnrollTempRepository enrollTempRepository, IUtilityRepository utilityRepository)
        {
            _enrollTempRepository = enrollTempRepository;
            _utilityRepository = utilityRepository;
        }

        [HttpPost("temp-grid")]
        [ProducesResponseType(typeof(ServiceResponse<GridResponse<MonitoringEnroll>>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Enrollment Monitoring")]
        public async Task<ServiceResponse<GridResponse<MonitoringEnroll>>> GetDataTemp(DataEnrollTempFilter filter)
        {
            if (filter?.PageSize == null)
            {
                return new ServiceResponse<GridResponse<MonitoringEnroll>>
                {
                    Message = nameof(ServiceResponseStatus.EMPTY_PARAMETER),
                    Status = (int)ServiceResponseStatus.EMPTY_PARAMETER
                };
            }

            string authorization = HttpContext.Request.Headers["Authorization"];

            if (string.IsNullOrWhiteSpace(authorization))
            {
                filter.LoginPegawaiId = 20;
                filter.LoginRoleId = 1;
                filter.LoginUnitId = 1;
            }
            else
            {
                var token = authorization.Split(" ")[1];

                var claims = TokenManager.GetPrincipal(token);

                filter.LoginPegawaiId = int.Parse(claims.PegawaiId);
                filter.LoginRoleId = int.Parse(claims.RoleUnitId == "x" ? claims.RoleId : claims.RoleUnitId);
                filter.LoginUnitId = int.Parse(claims.UnitId == "x" ? "1" : claims.UnitId);
            }

            var res = await _enrollTempRepository.GetDataEnroolTemp(filter);

            var data = new List<MonitoringEnroll>();

            foreach (var i in res.Data)
            {
                data.Add(new MonitoringEnroll
                {
                    AlamatLengkap = i.AlamatLengkap,
                    CreatedTime = i.CreatedTime,
                    File = ConvertUrlToB64(i.File),
                    Id = i.Id,
                    JenisKelamin = i.JenisKelamin,
                    Nama = i.Nama,
                    NIK = i.NIK,
                    Number = i.Number,
                    PathFile = ConvertUrlToB64(i.PathFile),
                    TanggalLahir = i.TanggalLahir,
                    TempatLahir = i.TempatLahir,
                    EnrollBy = i.EnrollBy,
                    StatusData = i.StatusData
                });
            }

            res.Data = data;

            return new ServiceResponse<GridResponse<MonitoringEnroll>>
            {
                Data = res,
                Message = nameof(ServiceResponseStatus.SUKSES),
                Status = (int)ServiceResponseStatus.SUKSES
            };
        }

        [HttpPost("temp-grid-finger-enc-only")]
        [ProducesResponseType(typeof(ServiceResponse<GridResponse<MonitoringEnroll>>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Enrollment Monitoring")]
        public async Task<ServiceResponse<GridResponse<MonitoringEnroll>>> GetDataTempFingerEncOnly(DataEnrollTemp2Filter filter)
        {
            if (filter?.PageSize == null)
            {
                return new ServiceResponse<GridResponse<MonitoringEnroll>>
                {
                    Message = nameof(ServiceResponseStatus.EMPTY_PARAMETER),
                    Status = (int)ServiceResponseStatus.EMPTY_PARAMETER
                };
            }

            string authorization = HttpContext.Request.Headers["Authorization"];
            var Npp = "";

            if (!string.IsNullOrWhiteSpace(authorization))
            {
                var token = authorization.Split(" ")[1];

                var claims = TokenManager.GetPrincipal(token);

                filter.LoginPegawaiId = int.Parse(claims.PegawaiId);
                filter.LoginRoleId = int.Parse(claims.RoleUnitId == "x" ? claims.RoleId : claims.RoleUnitId);
                Npp = claims.NIK;
            }

            if(filter.NIK != "" || filter.NIK != null)
			{
                var _filter = new Tbl_LogNIKInquiry
				{
                    Nik = filter.NIK,
                    Npp = Npp,
                    Action = "Search",
                    SearchParam = JsonConvert.SerializeObject(filter),
                    Browser = filter.Browser == null ? "" : filter.Browser,
                    IpAddress = filter.IpAddress == null ? "" : filter.IpAddress,
                    Url = filter.Url == null ? "" : filter.Url,
                    CreatedTime = DateTime.Now
                };

                var _ = _utilityRepository.InsertLogNIKInquiry(_filter);
            }

            var res = await _enrollTempRepository.GetDataEnrool2Temp(filter);

            var data = new List<MonitoringEnroll>();

            foreach (var i in res.Data)
            {
                data.Add(new MonitoringEnroll
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
                    StatusData = i.StatusData
                });
            }

            res.Data = data;

            return new ServiceResponse<GridResponse<MonitoringEnroll>>
            {
                Data = res,
                Message = nameof(ServiceResponseStatus.SUKSES),
                Status = (int)ServiceResponseStatus.SUKSES
            };
        }

        [HttpPost("export-temp-grid-finger-enc-only")]
        [ProducesResponseType(typeof(ServiceResponse<GridResponse<ExportMonitoringEnroll>>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Enrollment Monitoring")]
        public async Task<ServiceResponses<ExportMonitoringEnroll>> ExportDataTempFingerEncOnly(ExportDataEnrollTemp2Filter filter)
        {
            string authorization = HttpContext.Request.Headers["Authorization"];

            if (string.IsNullOrWhiteSpace(authorization))
            {
                filter.LoginPegawaiId = 20;
                filter.LoginRoleId = 1;
            }
            else
            {
                var token = authorization.Split(" ")[1];

                var claims = TokenManager.GetPrincipal(token);

                filter.LoginPegawaiId = int.Parse(claims.PegawaiId);
                filter.LoginRoleId = int.Parse(claims.RoleUnitId == "x" ? claims.RoleId : claims.RoleUnitId);
            }

            var res = await _enrollTempRepository.ExportDataEnrool2Temp(filter);

            return new ServiceResponses<ExportMonitoringEnroll>
            {
                Data = res,
                Message = nameof(ServiceResponseStatus.SUKSES),
                Status = (int)ServiceResponseStatus.SUKSES
            };
        }

        private static string ConvertUrlToB64(string path)
        {
            try
            {
                using WebClient webClient = new();

                var b64 = webClient.DownloadData(path ?? "");

                var b64String = "";

                using (var r = new StreamReader(new MemoryStream(b64)))
                {
                    var text = r.ReadToEnd();
                    b64String = text.Decrypt(Phrase.FileEncryption);
                }

                return b64String;
            }
            catch (Exception)
            {
                return "";
            }
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
