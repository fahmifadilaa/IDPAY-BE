using Ekr.Auth;
using Ekr.Core.Constant;
using Ekr.Core.Entities.DataMaster.Lookup;
using Ekr.Core.Entities;
using Ekr.Repository.Contracts.DataMaster.Lookup;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Ekr.Api.DataMaster.Filters;
using Ekr.Core.Entities.DataMaster.MasterTreshold;
using Ekr.Repository.Contracts.DataMaster.MasterTreshold;

namespace Ekr.Api.DataMaster.Controllers
{
    [Route("dm/treshold")]
    [ApiController]
    public class TresholdController : ControllerBase
    {
        private readonly ITresholdRepository _TresholdRepository;

        public TresholdController(ITresholdRepository TresholdRepository)
        {
            _TresholdRepository = TresholdRepository;
        }

        /// <summary>
        /// Untuk load data lookup
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("tresholds")]
        [ProducesResponseType(typeof(ServiceResponse<GridResponse<TresholdVM>>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master Treshold Load data")]
        public async Task<ServiceResponse<GridResponse<TresholdVM>>> LoadData([FromBody] TresholdFilter req)
        {
            var res = await _TresholdRepository.LoadData(req);

            return new ServiceResponse<GridResponse<TresholdVM>>
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
        [HttpGet("treshold")]
        [ProducesResponseType(typeof(ServiceResponse<TblMasterTreshold>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master Treshold Get By Id")]
        public async Task<ServiceResponse<TblMasterTreshold>> GetById([FromQuery] TresholdByIdVM req)
        {
            var res = await _TresholdRepository.GetById(req);

            return new ServiceResponse<TblMasterTreshold>
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
        [HttpPost("treshold")]
        [ProducesResponseType(typeof(ServiceResponse<TblMasterTreshold>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<TblMasterTreshold>> InsertLookup([FromBody] TblMasterTreshold req)
        {
            string authorization = HttpContext.Request.Headers["Authorization"];
            if (authorization != null)
            {
                var token = authorization.Split(" ")[1];
                var claims = TokenManager.GetPrincipal(token);
                req.createdById = int.Parse(claims.PegawaiId);
            }

            var res = await _TresholdRepository.InsertTreshold(req);

            return new ServiceResponse<TblMasterTreshold>
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
        [HttpPut("treshold")]
        [ProducesResponseType(typeof(ServiceResponse<TblMasterTreshold>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<TblMasterTreshold>> UpdateLookup([FromBody] TblMasterTreshold req)
        {
            string authorization = HttpContext.Request.Headers["Authorization"];
            if (authorization != null)
            {
                var token = authorization.Split(" ")[1];
                var claims = TokenManager.GetPrincipal(token);
                req.updatedById = int.Parse(claims.PegawaiId);
            }

            var res = await _TresholdRepository.UpdateTreshold(req);

            return new ServiceResponse<TblMasterTreshold>
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
        [HttpDelete("treshold")]
        [ProducesResponseType(typeof(ServiceResponse<string>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<string>> DeleteLookup([FromQuery] TresholdByIdVM req)
        {
            int PegawaiId = 100;
            string authorization = HttpContext.Request.Headers["Authorization"];
            if (authorization != null)
            {
                var token = authorization.Split(" ")[1];
                var claims = TokenManager.GetPrincipal(token);
                PegawaiId = int.Parse(claims.PegawaiId);
            }

            await _TresholdRepository.DeleteTreshold(req, PegawaiId);

            return new ServiceResponse<string>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success"
            };
        }
    }
}
