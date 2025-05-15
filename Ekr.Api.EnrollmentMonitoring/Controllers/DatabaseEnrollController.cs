using Ekr.Api.EnrollmentMonitoring.Filters;
using Ekr.Auth;
using Ekr.Core.Constant;
using Ekr.Core.Entities;
using Ekr.Core.Entities.DataEnrollment;
using Ekr.Core.Entities.DataMaster.Utility.Entity;
using Ekr.Core.Securities.Symmetric;
using Ekr.Repository.Contracts.DataMaster.Utility;
using Ekr.Repository.Contracts.Enrollment;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Ekr.Api.EnrollmentMonitoring.Controllers
{
    /// <summary>
    /// api dbenroll
    /// </summary>
    [Route("dbenroll")]
    [ApiController]
    public class DatabaseEnrollController : ControllerBase
    {
        private readonly IEnrollmentKTPRepository _enrollmentKTPRepository;
        private readonly IUtilityRepository _utilityRepository;

        public DatabaseEnrollController(IEnrollmentKTPRepository enrollmentKTPRepository, IUtilityRepository utilityRepository)
        {
            _enrollmentKTPRepository = enrollmentKTPRepository;
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
                filter.LoginRoleId = int.Parse(claims.RoleUnitId=="x"?claims.RoleId:claims.RoleUnitId);
                filter.LoginUnitId = int.Parse(claims.UnitId == "x"? "1": claims.UnitId);
            }

            var res = await _enrollmentKTPRepository.GetDBEnroll(filter);

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
                    StatusData = i.StatusData,
                    CIF = i.CIF,
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


        /// <summary>
        /// Untuk Load Data finger enc only with Nik Mandatory
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        [HttpPost("temp-grid-finger-enc-only")]
        [ProducesResponseType(typeof(ServiceResponse<GridResponse<MonitoringEnroll>>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Enrollment Monitoring")]
        public async Task<ServiceResponse<GridResponse<MonitoringEnroll>>> GetDataTempFingerEncoNLY(DataEnrollTempFilter filter)
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
                filter.LoginUnitId = int.Parse(claims.UnitId == "x" ? "1" : claims.UnitId);
                filter.UnitIds = claims.UnitId == "x" ? "1" : claims.UnitId;
                Npp = claims.NIK;
            }

            if (filter.NIK != "" || filter.NIK != null)
            {
                var _filter = new Tbl_LogNIKInquiry
                {
                    Nik = filter.NIK,
                    Npp = Npp,
                    Action = "Search",
                    SearchParam = JsonConvert.SerializeObject(filter),
                    Browser = filter.Browser == null ? "" : filter.Browser,
                    IpAddress = filter.IpAddress == null ? "" : filter.IpAddress,
                    CreatedTime = DateTime.Now,
                    Url = filter.Url == null ? "" : filter.Url
                };

                var _ = _utilityRepository.InsertLogNIKInquiry(_filter);
            }

            var res = await _enrollmentKTPRepository.GetDBEnroll(filter);

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
                    StatusData = i.StatusData,
                    CIF = i.CIF
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

        /// <summary>
        /// Untuk Load Data finger enc only with Nik Not Mandatory
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        [HttpPost("temp-grid-finger-enc-only-sec")]
        [ProducesResponseType(typeof(ServiceResponse<GridResponse<MonitoringEnroll>>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Enrollment Monitoring")]
        public async Task<ServiceResponse<GridResponse<MonitoringEnroll>>> GetDataTempFingerEncOnlySec(DataEnrollTempFilter filter)
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

            if (!string.IsNullOrWhiteSpace(authorization))
            {
                var token = authorization.Split(" ")[1];

                var claims = TokenManager.GetPrincipal(token);

                filter.LoginPegawaiId = int.Parse(claims.PegawaiId);
                filter.LoginRoleId = int.Parse(claims.RoleUnitId == "x" ? claims.RoleId : claims.RoleUnitId);
                filter.LoginUnitId = int.Parse(claims.UnitId == "x" ? "1" : claims.UnitId);
            }

            var res = await _enrollmentKTPRepository.GetDBEnrollSec(filter);

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
                    StatusData = i.StatusData,
                    CIF=i.CIF,
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
        [ProducesResponseType(typeof(ServiceResponses<ExportMonitoringEnrollNew>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Enrollment Monitoring")]
        //public async Task<ServiceResponses<ExportMonitoringEnrollNew>> ExportDataTempFingerEncoNLY(ExportDataEnrollTempFilter filter)
        public async Task<ServiceResponses<ExportMonitoringEnrollNew>> ExportDataTempFingerEncoNLY(ExportDataEnrollTempFilterV5 filter)
        {
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

            //var res = await _enrollmentKTPRepository.ExportDBEnroll(filter);
            //var res = await _enrollmentKTPRepository.ExportDBEnrollNew(filter);
            var res = await _enrollmentKTPRepository.ExportDBEnrollNew2(filter);

            return new ServiceResponses<ExportMonitoringEnrollNew>
            {
                Data = res,
                Message = nameof(ServiceResponseStatus.SUKSES),
                Status = (int)ServiceResponseStatus.SUKSES
            };
        }

        [HttpPost("export-temp-grid-finger-enc-only_no_matching")]
        [ProducesResponseType(typeof(ServiceResponses<ExportMonitoringEnroll>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Enrollment Monitoring")]
        public async Task<ServiceResponses<ExportMonitoringEnroll>> ExportDataTempFingerEncoNLYFR(ExportDataEnrollTempFilter filter)
        {
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

            var res = await _enrollmentKTPRepository.ExportDBEnrollFR(filter);

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
