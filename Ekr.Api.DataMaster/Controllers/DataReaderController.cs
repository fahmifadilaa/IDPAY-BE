using Ekr.Api.DataMaster.Filters;
using Ekr.Auth;
using Ekr.Business.Contracts.DataMaster;
using Ekr.Core.Constant;
using Ekr.Core.Entities;
using Ekr.Core.Entities.DataMaster.AlatReader;
using Ekr.Core.Entities.DataMaster.DataReader.Entity;
using Ekr.Core.Entities.DataMaster.DataReader.ViewModel;
using Ekr.Repository.Contracts.DataMaster.DataReader;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Ekr.Api.DataMaster.Controllers
{
    /// <summary>
    /// api Data Reader
    /// </summary>
    [Route("dm/datareader")]
    [ApiController]
    public class DataReaderController : ControllerBase
    {
        private readonly IDataReaderRepository _dataReaderRepository;
        private readonly IDataReaderService _dataReaderService;

        public DataReaderController(IDataReaderRepository dataReaderRepository, IDataReaderService dataReaderService)
        {
            _dataReaderRepository = dataReaderRepository;
            _dataReaderService = dataReaderService;
        }

        /// <summary>
        /// Untuk load all data reader
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("data_readers")]
        [ProducesResponseType(typeof(ServiceResponse<GridResponse<MasterAlatReaderVM>>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<GridResponse<MasterAlatReaderVM>>> LoadData([FromBody] DataReaderFilterVM req)
        {
            string authorization = HttpContext.Request.Headers["Authorization"];

            if (string.IsNullOrWhiteSpace(authorization))
            {
                var res1 = await _dataReaderRepository.LoadData(req, 1, 1);
                return new ServiceResponse<GridResponse<MasterAlatReaderVM>>
                {
                    Status = (int)ServiceResponseStatus.SUKSES,
                    Message = "Success",
                    Data = res1
                };
            }

            var token = authorization.Split(" ")[1];

            var claims = TokenManager.GetPrincipal(token);

            var res = await _dataReaderRepository.LoadData(req, int.Parse(claims.UnitId),
                int.Parse(claims.RoleUnitId == "x" ? claims.RoleId : claims.RoleUnitId));

            return new ServiceResponse<GridResponse<MasterAlatReaderVM>>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = res
            };
        }

        /// <summary>
        /// Untuk load all data reader dengan kondisi aktif/tidak aktif
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("data_readers_with_condition")]
        [ProducesResponseType(typeof(ServiceResponse<GridResponse<MasterAlatReaderVM>>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<GridResponse<MasterAlatReaderVM>>> LoadDataWithCondition([FromBody] DataReaderConditionFilterVM req)
        {
            string authorization = HttpContext.Request.Headers["Authorization"];

            if (string.IsNullOrWhiteSpace(authorization))
            {
                var res1 = await _dataReaderRepository.LoadDataWithCondition(req, 1, 1);

                return new ServiceResponse<GridResponse<MasterAlatReaderVM>>
                {
                    Status = (int)ServiceResponseStatus.SUKSES,
                    Message = "Success",
                    Data = res1
                };
            }

            var token = authorization.Split(" ")[1];

            var claims = TokenManager.GetPrincipal(token);

            var res = await _dataReaderRepository.LoadDataWithCondition(req, int.Parse(claims.UnitId),
                int.Parse(claims.RoleUnitId == "x" ? claims.RoleId : claims.RoleUnitId));

            return new ServiceResponse<GridResponse<MasterAlatReaderVM>>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = res
            };
        }

        /// <summary>
        /// Untuk get data reader by Id
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet("data_reader")]
        [ProducesResponseType(typeof(ServiceResponse<TblMasterAlatReaderVM>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<TblMasterAlatReaderVM>> GetDataReader([FromQuery] DataReaderViewFilterVM req)
        {
            var res = await _dataReaderRepository.GetDataReader(req);

            return new ServiceResponse<TblMasterAlatReaderVM>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = res
            };
        }

        /// <summary>
        /// Untuk get detail data reader dengan serial number
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet("detail_data_reader")]
        [ProducesResponseType(typeof(ServiceResponse<DashboardDetailReaderVM>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<DashboardDetailReaderVM>> GetDetailDataReader([FromQuery] DataReaderDetailFilter req)
        {
            var res = await _dataReaderRepository.GetDetailDataReader(req);

            return new ServiceResponse<DashboardDetailReaderVM>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = res
            };
        }

        /// <summary>
        /// Untuk get detail data reader dengan uid
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        [HttpGet("detail_data_reader_byUID")]
        [ProducesResponseType(typeof(ServiceResponse<DashboardDetailReaderVM>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<DashboardDetailReaderVM>> GetDetailDataReaderByUID([FromQuery] string uid)
        {
            var res = await _dataReaderRepository.GetDetailDataReaderByUID(uid);

            return new ServiceResponse<DashboardDetailReaderVM>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = res
            };
        }

        /// <summary>
        /// Untuk get detail alat dengan UID
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet("detail_alat_data_reader")]
        [ProducesResponseType(typeof(ServiceResponse<MonitoringReaderExcelVM>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<MonitoringReaderExcelVM>> GetDetailAlatDataReader([FromQuery] DataReaderDetailAlatFilter req)
        {
            var res = await _dataReaderRepository.GetDetailAlatDataReader(req);
            if(res== null)
			{
                return new ServiceResponse<MonitoringReaderExcelVM>
                {
                    Status = (int)ServiceResponseStatus.Data_Empty,
                    Message = "Alat Reader Tidak Ditemukan!",
                    Data = null
                };
            }

            return new ServiceResponse<MonitoringReaderExcelVM>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = res
            };
        }

        /// <summary>
        /// Untuk create data reader
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("data_reader")]
        [ProducesResponseType(typeof(ServiceResponse<Tbl_MasterAlatReader>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<Tbl_MasterAlatReader>> InsertDataReader([FromBody] Tbl_MasterAlatReader req)
        {
            string authorization = HttpContext.Request.Headers["Authorization"];
            if (authorization != null)
            {
                var token = authorization.Split(" ")[1];
                var claims = TokenManager.GetPrincipal(token);
                req.CreatedBy_Id = int.Parse(claims.PegawaiId);
            }

            //check the uid
            var uid = await _dataReaderRepository.GetDatareaderUid(req.UID);
            if(uid != null)
            {
                return new ServiceResponse<Tbl_MasterAlatReader>
                {
                    Status = (int)ServiceResponseStatus.ERROR,
                    Message = "Uid Sudah Ada",
                    Data = null
                };
            }

            //check SN
            var sn = await _dataReaderRepository.GetDatareaderBySN(req.SN_Unit);

            if (sn != null)
            {
                return new ServiceResponse<Tbl_MasterAlatReader>
                {
                    Status = (int)ServiceResponseStatus.ERROR,
                    Message = "Serial Number Sudah Ada pada UID: " + sn.UID,
                    Data = null
                };
            }

            var res = await _dataReaderRepository.InsertDataReader(req);

            return new ServiceResponse<Tbl_MasterAlatReader>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = res
            };
        }

        /// <summary>
        /// Untuk update data reader
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPut("data_reader")]
        [ProducesResponseType(typeof(ServiceResponse<Tbl_MasterAlatReader>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<Tbl_MasterAlatReader>> UpdateDataReader([FromBody] Tbl_MasterAlatReader req)
        {
            string authorization = HttpContext.Request.Headers["Authorization"];
            if (authorization != null)
            {
                var token = authorization.Split(" ")[1];
                var claims = TokenManager.GetPrincipal(token);
                req.UpdatedBy_Id = int.Parse(claims.PegawaiId);
            }

            //check the uid
            var uid = await _dataReaderRepository.GetDatareaderUid(req.UID);
            if (uid != null && uid.Id != req.Id)
            {
                return new ServiceResponse<Tbl_MasterAlatReader>
                {
                    Status = (int)ServiceResponseStatus.ERROR,
                    Message = "Uid Sudah Ada pada ID:" + uid.Id,
                    Data = null
                };
            }

            //check SN
            var sn = await _dataReaderRepository.GetDatareaderBySN(req.SN_Unit);

            if (sn != null && uid.Id != req.Id)
            {
                return new ServiceResponse<Tbl_MasterAlatReader>
                {
                    Status = (int)ServiceResponseStatus.ERROR,
                    Message = "Serial Number Sudah Ada pada ID: " + sn.Id,
                    Data = null
                };
            }

            req.LastIP = uid.LastIP;
            req.LastActive = uid.LastActive;
            req.LastUsed = uid.LastUsed;

            var res = await _dataReaderRepository.UpdateDataReader(req);

            return new ServiceResponse<Tbl_MasterAlatReader>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = res
            };
        }

        /// <summary>
        /// Untuk delete data reader
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpDelete("data_reader")]
        [ProducesResponseType(typeof(ServiceResponse<string>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<string>> DeleteDataReader([FromQuery] DataReaderViewFilterVM req)
        {
            int PegawaiId = 100;
            string authorization = HttpContext.Request.Headers["Authorization"];
            if (authorization != null)
            {
                var token = authorization.Split(" ")[1];
                var claims = TokenManager.GetPrincipal(token);
                PegawaiId = int.Parse(claims.PegawaiId);
            }

            await _dataReaderRepository.DeleteDataReader(req, PegawaiId);

            return new ServiceResponse<string>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success"
            };
        }

        /// <summary>
        /// Untuk load log activity data reader
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("log_activity")]
        [ProducesResponseType(typeof(ServiceResponse<GridResponse<MasterAlatReaderLogActivityVM>>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<GridResponse<MasterAlatReaderLogActivityVM>>> LoadDataLogActivity([FromBody] LogActivityDataReaderFilterVM req)
        {
            var res = await _dataReaderRepository.LoadDataLogActivity(req);

            return new ServiceResponse<GridResponse<MasterAlatReaderLogActivityVM>>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = res
            };
        }

        /// <summary>
        /// Untuk load log connection data reader
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("log_connection")]
        [ProducesResponseType(typeof(ServiceResponse<GridResponse<MasterAlatReaderLogConnectionVM>>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<GridResponse<MasterAlatReaderLogConnectionVM>>> LoadDataLogConnection([FromBody] LogConnectionDataReaderFilterVM req)
        {
            var res = await _dataReaderRepository.LoadDataLogConnection(req);

            return new ServiceResponse<GridResponse<MasterAlatReaderLogConnectionVM>>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = res
            };
        }

        /// <summary>
        /// Untuk load log user data reader
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("log_user")]
        [ProducesResponseType(typeof(ServiceResponse<GridResponse<MasterAlatReaderLogUserVM2>>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<GridResponse<MasterAlatReaderLogUserVM2>>> LoadDataLogUser([FromBody] LogUserDataReaderFilterVM req)
        {
            var res = await _dataReaderRepository.LoadDataLogUser(req);

            return new ServiceResponse<GridResponse<MasterAlatReaderLogUserVM2>>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = res
            };
        }

        /// <summary>
        /// To insert Data Reader by excel
        /// </summary>
        /// <param name="formFile"></param>
        /// <returns></returns>
        [HttpPost("data_reader_by_excel")]
        [ProducesResponseType(typeof(ServiceResponse<string>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<string>> ExcelBulkInsert([FromForm] IFormFileDto formFile)
        {
            if(formFile.file.Length <= 0)
            {
                return new ServiceResponse<string>
                {
                    Message = nameof(ServiceResponseStatus.EMPTY_PARAMETER),
                    Status = (int)ServiceResponseStatus.EMPTY_PARAMETER,
                    Data = "File tidak ada"
                };
            }

            int PegawaiId = 0;
            string authorization = HttpContext.Request.Headers["Authorization"];
            if (authorization != null)
            {
                var token = authorization.Split(" ")[1];
                var claims = TokenManager.GetPrincipal(token);
                PegawaiId = int.Parse(claims.PegawaiId);
            }

            var res = await _dataReaderService.ExcelBulkInsert(formFile.file, PegawaiId);

            return new ServiceResponse<string>
            {
                Message = res.status == true? nameof(ServiceResponseStatus.SUKSES): nameof(ServiceResponseStatus.ERROR),
                Status = res.status == true ? (int)ServiceResponseStatus.SUKSES : (int)ServiceResponseStatus.ERROR,
                Data = res.msg
            };
        }

        /// <summary>
        /// Get Count Data Reader
        /// </summary>
        /// <param name="UnitIds"></param>
        /// <returns></returns>
        [HttpGet("data_reader_total")]
        [ProducesResponseType(typeof(ServiceResponse<int>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<int>> GetCountDataEnroll([FromQuery] string UnitIds)
        {
            var res = await _dataReaderRepository.GetCountJumlahDataReader(UnitIds);

            return new ServiceResponse<int>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = res
            };
        }
    }
}
