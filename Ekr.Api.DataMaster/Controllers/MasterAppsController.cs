using Ekr.Auth;
using Ekr.Core.Constant;
using Ekr.Core.Entities.DataMaster.Lookup;
using Ekr.Core.Entities;
using Ekr.Repository.Contracts.DataMaster.Lookup;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Ekr.Api.DataMaster.Filters;
using Ekr.Core.Entities.DataMaster.MasterApps;
using Ekr.Repository.Contracts.DataMaster.MasterApps;

namespace Ekr.Api.DataMaster.Controllers
{
    [Route("dm/masterapps")]
    [ApiController]
    public class MasterAppsController : ControllerBase
    {
        private readonly IMasterAppsRepository _lookupRepository;

        public MasterAppsController(IMasterAppsRepository lookupRepository)
        {
            _lookupRepository = lookupRepository;
        }

        /// <summary>
        /// Untuk load data lookup
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("masterapps")]
        [ProducesResponseType(typeof(ServiceResponse<GridResponse<MasterAppsVM>>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<GridResponse<MasterAppsVM>>> LoadData([FromBody] MasterAppsFilter req)
        {
            var res = await _lookupRepository.LoadData(req);

            return new ServiceResponse<GridResponse<MasterAppsVM>>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = res
            };
        }

        /// <summary>
        /// To get data lookup by id
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet("masterapp")]
        [ProducesResponseType(typeof(ServiceResponse<Tbl_Master_Apps>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<Tbl_Master_Apps>> GetById([FromQuery] MasterAppsByIdVM req)
        {
            var res = await _lookupRepository.GetById(req);

            return new ServiceResponse<Tbl_Master_Apps>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = res
            };
        }

        /// <summary>
        /// To create lookup
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("masterapp")]
        [ProducesResponseType(typeof(ServiceResponse<Tbl_Master_Apps>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<Tbl_Master_Apps>> InsertMasterApps([FromBody] Tbl_Master_Apps req)
        {
            string authorization = HttpContext.Request.Headers["Authorization"];
            if (authorization != null)
            {
                var token = authorization.Split(" ")[1];
                var claims = TokenManager.GetPrincipal(token);
                req.CreatedByNpp = claims.NIK;
            }

            var res = await _lookupRepository.InsertLookup(req);

            return new ServiceResponse<Tbl_Master_Apps>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = res
            };
        }

        /// <summary>
        /// To update lookup
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPut("masterapp")]
        [ProducesResponseType(typeof(ServiceResponse<Tbl_Master_Apps>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<Tbl_Master_Apps>> UpdateMasterApps([FromBody] Tbl_Master_Apps req)
        {
            string authorization = HttpContext.Request.Headers["Authorization"];
            if (authorization != null)
            {
                var token = authorization.Split(" ")[1];
                var claims = TokenManager.GetPrincipal(token);
                req.UpdatedByNpp = claims.NIK;
            }

            var res = await _lookupRepository.UpdateLookup(req);

            return new ServiceResponse<Tbl_Master_Apps>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = res
            };
        }

        /// <summary>
        /// To delete lookup
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpDelete("masterapp")]
        [ProducesResponseType(typeof(ServiceResponse<string>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<string>> DeleteMasterApps([FromQuery] MasterAppsByIdVM req)
        {
            int PegawaiId = 100;
            string authorization = HttpContext.Request.Headers["Authorization"];
            if (authorization != null)
            {
                var token = authorization.Split(" ")[1];
                var claims = TokenManager.GetPrincipal(token);
                PegawaiId = int.Parse(claims.PegawaiId);
            }

            await _lookupRepository.DeleteLookup(req, PegawaiId);

            return new ServiceResponse<string>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success"
            };
        }
    }
}
