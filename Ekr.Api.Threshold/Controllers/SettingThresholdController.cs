using Ekr.Api.Threshold.Filters;
using Ekr.Auth;
using Ekr.Business.Contracts.SettingThreshold;
using Ekr.Core.Constant;
using Ekr.Core.Entities;
using Ekr.Core.Entities.DataMaster.Utility;
using Ekr.Core.Entities.DataMaster.Utility.ViewModel;
using Ekr.Core.Entities.SettingThreshold;
using Ekr.Core.Securities.Symmetric;
using Ekr.Repository.Contracts.Setting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Ekr.Api.Threshold.Controllers
{
    /// <summary>
    /// api dashboard setting threshold
    /// </summary>
    [Route("setting_threshold")]
    [Authorize]
    [ApiController]
    public class SettingThresholdController : ControllerBase
    {
        private readonly ISettingThresholdRepository _settingThresholdRepository;
        private readonly ISettingThresholdService _settingThresholdService;
        private readonly IConfiguration _configuration;
        private readonly string ResponseSukses = "";
        private readonly string ResponseParameterKosong = "";

        public SettingThresholdController(ISettingThresholdRepository settingThresholdRepository, IConfiguration configuration
            , ISettingThresholdService settingThresholdService)
        {
            _settingThresholdRepository = settingThresholdRepository;
            _configuration = configuration;
            _settingThresholdService = settingThresholdService;

            ResponseSukses = _configuration.GetValue<string>("Response:Sukses");
            ResponseParameterKosong = _configuration.GetValue<string>("Response:ErrorParameterKosong");
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
                data = await _settingThresholdRepository.GetProbabilityDivision(nik);

                return new Core.Entities.ServiceResponse<string>
                {
                    Message = ResponseSukses,
                    Status = (int)ServiceResponseStatus.SUKSES
                };
            }

            var token = authorization.Split(" ")[1];
            var claims = TokenManager.GetPrincipal(token);

            data = await _settingThresholdRepository.GetProbabilityDivision(nik);


            return new Core.Entities.ServiceResponse<string>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = ResponseSukses,
                Data = data

            };
        }

        /// <summary>
        /// Untuk Load Data all enroll
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        [HttpPost("setting_threshold_list")]
        [ProducesResponseType(typeof(ServiceResponse<GridResponse<SettingThresholdData>>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Dashboard setting threshold")]
        public async Task<Core.Entities.ServiceResponse<GridResponse<SettingThresholdData>>> GetSettingThresholdListAsync(SettingThresholdFilter filter)
        {
            if (filter?.PageSize == null)
            {
                return new ServiceResponse<GridResponse<SettingThresholdData>>
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

            GridResponse<SettingThresholdData> res = await _settingThresholdRepository.GetSettingThresholdList(filter, 0);

            return new ServiceResponse<GridResponse<SettingThresholdData>>
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
        [HttpPost("setting_threshold_list_by_user")]
        [ProducesResponseType(typeof(ServiceResponse<GridResponse<SettingThresholdData>>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Dashboard setting threshold")]
        public async Task<ServiceResponse<GridResponse<SettingThresholdData>>> GetSettingThresholdListByUserAsync(SettingThresholdFilter filter)
        {
            if (filter?.PageSize == null)
            {
                return new ServiceResponse<GridResponse<SettingThresholdData>>
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

            GridResponse<SettingThresholdData> res = await _settingThresholdRepository.GetSettingThresholdList(filter, currentUserId);

            return new ServiceResponse<GridResponse<SettingThresholdData>>
            {
                Data = res,
                Message = ResponseSukses,
                Status = (int)ServiceResponseStatus.SUKSES
            };
        }

        /// <summary>
        /// To create Setting Threshold
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("insert")]
        [ProducesResponseType(typeof(ServiceResponse<TblSettingThresholdVM>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Setting Threshold")]
        public async Task<ServiceResponse<TblSettingThresholdVM>> InsertSettingThreshold([FromBody] Tbl_Setting_Threshold req)
        {
            string authorization = HttpContext.Request.Headers["Authorization"];
            if (authorization != null)
            {
                var token = authorization.Split(" ")[1];
                var claims = TokenManager.GetPrincipal(token);
                req.CreatedBy_Id = int.Parse(claims.PegawaiId);
            }

            TblSettingThresholdVM res = await _settingThresholdService.InsertSettingThresholdAsync(req);

            return new ServiceResponse<TblSettingThresholdVM>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = ResponseSukses,
                Data = res
            };
        }

        /// <summary>
        /// To get data Setting Threshold by id
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet("get_by_id")]
        [ProducesResponseType(typeof(ServiceResponse<TblSettingThresholdVM>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Setting Threshold")]
        public async Task<ServiceResponse<TblSettingThresholdVM>> GetById([FromQuery] SettingThresholdRequest req)
        {
            var res = await _settingThresholdRepository.GetById(req);

            return new ServiceResponse<TblSettingThresholdVM>
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
        [HttpGet("get_penyelia")]
        [ProducesResponseType(typeof(ServiceResponse<GridResponse<DataDropdownServerSide>>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Setting Threshold")]
        public async Task<ServiceResponse<GridResponse<DataDropdownServerSide>>> GetListPenyelia([FromQuery] int unitId)
        {
            GridResponse<DataDropdownServerSide> res = await _settingThresholdRepository.GetListPenyelia(unitId);

            return new ServiceResponse<GridResponse<DataDropdownServerSide>>
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
        [HttpGet("get_penyelia_new")]
        [ProducesResponseType(typeof(ServiceResponse<GridResponse<DataDropdownServerSide>>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Setting Threshold")]
        public async Task<ServiceResponse<GridResponse<DataDropdownServerSide>>> GetListPenyeliaNew([FromQuery] int unitId, string npp)
        {
            GridResponse<DataDropdownServerSide> res = await _settingThresholdRepository.GetListPenyelia2(unitId, npp);

            return new ServiceResponse<GridResponse<DataDropdownServerSide>>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = ResponseSukses,
                Data = res
            };
        }

        /// <summary>
        /// Get list pemimpin
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet("get_pemimpin")]
        [ProducesResponseType(typeof(ServiceResponse<GridResponse<DataDropdownServerSide>>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Setting Threshold")]
        public async Task<ServiceResponse<GridResponse<DataDropdownServerSide>>> GetListPemimpin([FromQuery] int unitId)
        {
            GridResponse<DataDropdownServerSide> res = await _settingThresholdRepository.GetListPemimpin(unitId);

            return new ServiceResponse<GridResponse<DataDropdownServerSide>>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = ResponseSukses,
                Data = res
            };
        }

        /// <summary>
        /// Get list pemimpin
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet("get_pemimpin_new")]
        [ProducesResponseType(typeof(ServiceResponse<GridResponse<DataDropdownServerSide>>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Setting Threshold")]
        public async Task<ServiceResponse<GridResponse<DataDropdownServerSide>>> GetListPemimpinNew([FromQuery] int unitId, string npp)
        {
            GridResponse<DataDropdownServerSide> res = await _settingThresholdRepository.GetListPemimpin2(unitId, npp);

            return new ServiceResponse<GridResponse<DataDropdownServerSide>>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = ResponseSukses,
                Data = res
            };
        }

        /// <summary>
        /// Get list pemimpin
        /// </summary>
        /// <param name="thresholdId">Threshold Id</param>
        /// <returns></returns>
        [HttpGet("get_setting_threshold_log")]
        [ProducesResponseType(typeof(ServiceResponse<GridResponse<TblSettingThresholdLogVM>>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Setting Threshold")]
        public async Task<ServiceResponse<GridResponse<TblSettingThresholdLogVM>>> GetSettingTresholdLogAsync(int thresholdId)
        {
            GridResponse<TblSettingThresholdLogVM> res = await _settingThresholdRepository.GetSettingTresholdLogAsync(thresholdId);

            return new ServiceResponse<GridResponse<TblSettingThresholdLogVM>>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = ResponseSukses,
                Data = res
            };
        }



        /// <summary>
        /// To update Setting Threshold
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPut("update")]
        [ProducesResponseType(typeof(ServiceResponse<TblSettingThresholdVM>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Setting Threshold")]
        public async Task<ServiceResponse<TblSettingThresholdVM>> UpdateSettingTreshold([FromBody] Tbl_Setting_Threshold req)
        {
            string authorization = HttpContext.Request.Headers["Authorization"];
            if (authorization != null)
            {
                var token = authorization.Split(" ")[1];
                var claims = TokenManager.GetPrincipal(token);
                req.UpdatedBy_Id = int.Parse(claims.PegawaiId);
            }

            var res = await _settingThresholdRepository.UpdateSettingTreshold(req);

            return new ServiceResponse<TblSettingThresholdVM>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = ResponseSukses,
                Data = res
            };
        }

        /// <summary>
        /// To update Setting Threshold status
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("update_status")]
        [ProducesResponseType(typeof(ServiceResponse<string>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<string>> UpdateSettingThresholdStatus([FromBody] SettingThresholdStatusRequest req)
        {
            int pegawaiId = 100;
            string authorization = HttpContext.Request.Headers["Authorization"];
            if (authorization != null)
            {
                var token = authorization.Split(" ")[1];
                var claims = TokenManager.GetPrincipal(token);
                pegawaiId = int.Parse(claims.PegawaiId);
            }

            await _settingThresholdService.UpdateSettingThresholdStatusAsync(req, pegawaiId);

            return new ServiceResponse<string>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = ResponseSukses
            };
        }

        /// <summary>
        /// To delete Setting Threshold
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpDelete("delete")]
        [ProducesResponseType(typeof(ServiceResponse<string>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<string>> DeleteSettingThreshold([FromQuery] SettingThresholdRequest req)
        {
            int PegawaiId = 100;
            string authorization = HttpContext.Request.Headers["Authorization"];
            if (authorization != null)
            {
                var token = authorization.Split(" ")[1];
                var claims = TokenManager.GetPrincipal(token);
                PegawaiId = int.Parse(claims.PegawaiId);
            }

            await _settingThresholdRepository.DeleteSettingTreshold(req, PegawaiId);

            return new ServiceResponse<string>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = ResponseSukses
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

        [HttpGet("dropdown-treshold-value")]
        [ProducesResponseType(typeof(ServiceResponse<GridResponse<DataDropdownServerSide>>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<GridResponse<DataDropdownServerSide>>> DropdownLookup([FromQuery] DropdownLookupFilterVM req)
        {
            var res = await _settingThresholdRepository.GetDropdownTreshold(req);

            return new ServiceResponse<GridResponse<DataDropdownServerSide>>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = ResponseSukses,
                Data = res
            };
        }

    }
}
