using Ekr.Api.DataEnrollment.Filters;
using Ekr.Auth;
using Ekr.Core.Constant;
using Ekr.Core.Entities;
using Ekr.Core.Entities.DataEnrollment;
using Ekr.Core.Entities.DataEnrollment.ViewModel;
using Ekr.Core.Entities.DataMaster.AgeSegmentation.Entity;
using Ekr.Core.Entities.DataMaster.AgeSegmentation.ViewModel;
using Ekr.Core.Securities.Symmetric;
using Ekr.Repository.Contracts.DataEnrollment;
using Ekr.Repository.Contracts.DataMaster.AgeSegmentation;
using Ekr.Repository.Contracts.Enrollment;
using Ekr.Repository.DataEnrollment;
using Ekr.Repository.Enrollment;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Ekr.Api.MonitoringDataNoMatching.Controllers
{
    [Route("dashboard/data_enrollment_no_matching")]
    [ApiController]
    public class DashboardDataEnrollNoMatchingController : ControllerBase
    {
        private readonly IDashboardDataEnrollNoMatchingRepository _dashboardDataEnrollNoMatchingRepository;
        private readonly IEnrollmentKTPRepository _enrollmentKTPRepository;

        public DashboardDataEnrollNoMatchingController(IDashboardDataEnrollNoMatchingRepository dashboardDataEnrollNoMatchingRepository, IEnrollmentKTPRepository enrollmentKTPRepository)
        {
            _dashboardDataEnrollNoMatchingRepository = dashboardDataEnrollNoMatchingRepository;
            _enrollmentKTPRepository = enrollmentKTPRepository;
        }

        /// <summary>
        /// Untuk load chart pekerjaan
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        [HttpPost("job_chart_no_matching")]
        [ProducesResponseType(typeof(ServiceResponses<JobChartDataVM>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Untuk load chart pekerjaan")]
        public async Task<ServiceResponses<JobChartDataVM>> GetJobChartNoMatching(UnitIdsFilterVM req)
        {
            var res = await _dashboardDataEnrollNoMatchingRepository.GetJobChart(req);

            return new ServiceResponses<JobChartDataVM>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = res
            };
        }

        /// <summary>
        /// Untuk load chart Data Type Enrollment
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        [HttpPost("type_enrollment_chart_no_matching")]
        [ProducesResponseType(typeof(ServiceResponses<TypeEnrollmentVM>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Untuk load chart data type enrollment")]
        public async Task<ServiceResponses<TypeEnrollmentVM>> GettypeEnrollmentChartNoMatching(UnitIdsFilterVM req)
        {
            var res = await _dashboardDataEnrollNoMatchingRepository.GetTypeEnrollmentChart(req);

            return new ServiceResponses<TypeEnrollmentVM>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = res
            };
        }

        /// <summary>
        /// Untuk load chart Data Enrollment By Channel
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        [HttpPost("channel_enrollment_chart_no_matching")]
        [ProducesResponseType(typeof(ServiceResponses<TypeEnrollmentVM>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Untuk load chart channel enrollment")]
        public async Task<ServiceResponses<ChannelEnrollmentVM>> GetChannelEnrollmentChartNoMatching(UnitIdsFilterVM req)
        {
            var res = await _dashboardDataEnrollNoMatchingRepository.GetChannelEnrollmentChart(req);

            return new ServiceResponses<ChannelEnrollmentVM>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = res
            };
        }

        /// <summary>
        /// Untuk load chart Data Enrollment By Status
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        [HttpPost("status_enrollment_chart_no_matching")]
        [ProducesResponseType(typeof(ServiceResponses<StatusEnrollmentVM>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Untuk load chart status enrollment")]
        public async Task<ServiceResponses<StatusEnrollmentVM>> GetStatusEnrollmentChartNoMatching(UnitIdsFilterVM req)
        {
            var res = await _dashboardDataEnrollNoMatchingRepository.GetStatusEnrollmentChart(req);

            return new ServiceResponses<StatusEnrollmentVM>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = res
            };
        }

        /// <summary>
        /// Untuk load chart agama
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        [HttpPost("religion_chart_no_matching")]
        [ProducesResponseType(typeof(ServiceResponses<ReligionChartDataVM>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Untuk load chart agama")]
        public async Task<ServiceResponses<ReligionChartDataVM>> GetReligionChartNoMatching(UnitIdsFilterVM req)
        {
            var res = await _dashboardDataEnrollNoMatchingRepository.GetReligionChart(req);

            return new ServiceResponses<ReligionChartDataVM>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = res
            };
        }

        /// <summary>
        /// Untuk load chart generasi kelahiran
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        [HttpPost("born_generation_chart_no_matching")]
        [ProducesResponseType(typeof(ServiceResponses<BornGenerationChartDataVM>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Untuk load chart generasi kelahiran")]
        public async Task<ServiceResponses<BornGenerationChartDataVM>> GetBornGenerationChartNoMatching(UnitIdsFilterVM req)
        {
            var res = await _dashboardDataEnrollNoMatchingRepository.GetBornGenerationChart(req);

            return new ServiceResponses<BornGenerationChartDataVM>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = res
            };
        }

        /// <summary>
        /// Untuk load chart segmentasi usia
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        [HttpPost("age_segmentation_chart_no_matching")]
        [ProducesResponseType(typeof(ServiceResponses<AgeSegmentationChartDataVM>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Untuk load chart segmentasi usia")]
        public async Task<ServiceResponses<AgeSegmentationChartDataVM>> GetAgeSegmentationChartNoMatching(UnitIdsFilterVM req)
        {
            var res = await _dashboardDataEnrollNoMatchingRepository.GetAgeSegmentationChart(req);

            return new ServiceResponses<AgeSegmentationChartDataVM>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = res
            };
        }

        /// <summary>
        /// Untuk load enroll per unit
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        [HttpPost("enroll_per_unit_no_matching")]
        [ProducesResponseType(typeof(ServiceResponse<GridResponse<EnrollPerUnitVM>>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Untuk load enroll per unit")]
        public async Task<ServiceResponse<GridResponse<EnrollPerUnitVM>>> GetEnrollPerUnitNoMatching([FromBody] EnrollPerUnitFilterVM req)
        {
            var res = await _dashboardDataEnrollNoMatchingRepository.GetEnrollPerUnit(req);

            return new ServiceResponse<GridResponse<EnrollPerUnitVM>>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = res
            };
        }

        /// <summary>
        /// Untuk load detail enroll per unit
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        [HttpPost("enroll_per_unit_list_no_matching")]
        [ProducesResponseType(typeof(ServiceResponse<GridResponse<EnrollPerUnitVM>>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Untuk load detail enroll per unit")]
        public async Task<ServiceResponse<GridResponse<EnrollPerUnitVM>>> GetEnrollPerUnitListNoMatching([FromBody] EnrollPerUnitFilterVM req)
        {
            var res = await _dashboardDataEnrollNoMatchingRepository.GetEnrollPerUnit(req);

            return new ServiceResponse<GridResponse<EnrollPerUnitVM>>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = res
            };
        }

        /// <summary>
        /// To get detail data by nik
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        [HttpGet("detail_data_no_matching")]
        [ProducesResponseType(typeof(ServiceResponse<EnrollKTPVM>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "To get detail data by nik")]
        public async Task<ServiceResponse<EnrollKTPVM>> DetailDataNoMatching([FromQuery] EnrollKTPFIlterVM req)
        {
            var res = await _dashboardDataEnrollNoMatchingRepository.DetailData(req);

            return new ServiceResponse<EnrollKTPVM>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = res
            };
        }

        /// <summary>
        /// Untuk load dashboard enroll
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        [HttpPost("dashboard-enroll-list_no_matching")]
        [ProducesResponseType(typeof(ServiceResponse<GridResponse<DahboardEnrollmentPG>>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Untuk load dashboard enroll")]
        public async Task<ServiceResponse<GridResponse<DahboardEnrollmentPG>>> GetDashboardEnrollNoMatching([FromBody] DahboardEnrollmentPGFilterVM req)
        {
            string authorization = HttpContext.Request.Headers["Authorization"];

            if (!string.IsNullOrWhiteSpace(authorization))
            {
                var token = authorization.Split(" ")[1];

                var claims = TokenManager.GetPrincipal(token);

                req.UnitCode = claims.KodeUnit;
                req.Role = claims.RoleUnitId == "x" ? claims.RoleId : claims.RoleUnitId;
            }

            var res = await _dashboardDataEnrollNoMatchingRepository.DashboardEnrollList(req);

            var data = new List<DahboardEnrollmentPG>();

            foreach (var i in res.Data)
            {
                data.Add(new DahboardEnrollmentPG
                {
                    Alamat = i.Alamat,
                    CreatedTime = i.CreatedTime,
                    File = ConvertUrlToB64(i.File),
                    Id = i.Id,
                    JenisKelamin = i.JenisKelamin,
                    Nama = i.Nama,
                    NIK = i.NIK,
                    Provinsi = i.Provinsi,
                    PathFile = ConvertUrlToB64(i.PathFile),
                    TanggalLahir = i.TanggalLahir,
                    TempatLahir = i.TempatLahir,
                    EnrollBy = i.EnrollBy,
                    StatusData = i.StatusData
                });
            }

            res.Data = data;

            return new ServiceResponse<GridResponse<DahboardEnrollmentPG>>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = res
            };
        }

        /// <summary>
        /// Untuk Load Data all enroll
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        [HttpPost("grid-finger-enc-only_no_matching")]
        [ProducesResponseType(typeof(ServiceResponse<GridResponse<MonitoringEnroll>>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Dashboard data enroll")]
        public async Task<ServiceResponse<GridResponse<MonitoringEnroll>>> GetDBEnrollNoMatching(DataEnrollTempFilter filter)
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

            //var res = await _dashboardDataEnrollRepository.GetDBEnroll(filter);
            //var res = await _enrollmentKTPRepository.GetDBEnrollV2(filter);
            var res = await _dashboardDataEnrollNoMatchingRepository.GetDBEnrollNoMatchingFR(filter);

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
                    CIF = i.CIF,
                    status = i.status
                });
            }
            res.Data = data;

            //var Initiate = new GridResponse<MonitoringEnroll>();

            //Initiate.Data = res;
            //Initiate.Count = res.Count();

            return new ServiceResponse<GridResponse<MonitoringEnroll>>
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
            catch (Exception ex)
            {
                return ex.Message.ToString();
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
