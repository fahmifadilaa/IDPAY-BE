using Ekr.Api.DataMaster.Filters;
using Ekr.Auth;
using Ekr.Core.Constant;
using Ekr.Core.Entities;
using Ekr.Core.Entities.DataMaster.Lookup;
using Ekr.Repository.Contracts.DataMaster.Lookup;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Ekr.Api.DataMaster.Controllers
{
    /// <summary>
    /// api Lookup
    /// </summary>
    [Route("dm/lookup")]
    [ApiController]
    public class LookupController : ControllerBase
    {
        private readonly ILookupRepository _lookupRepository;

        public LookupController(ILookupRepository lookupRepository)
        {
            _lookupRepository = lookupRepository;
        }

        /// <summary>
        /// Untuk load data lookup
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("lookups")]
        [ProducesResponseType(typeof(ServiceResponse<GridResponse<LookupVM>>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<GridResponse<LookupVM>>> LoadData([FromBody] LookupFilter req)
        {
            var res = await _lookupRepository.LoadData(req);

            return new ServiceResponse<GridResponse<LookupVM>>
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
        [HttpGet("lookup")]
        [ProducesResponseType(typeof(ServiceResponse<TblLookup>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<TblLookup>> GetById([FromQuery] LookupByIdVM req)
        {
            var res = await _lookupRepository.GetById(req);

            return new ServiceResponse<TblLookup>
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
        [HttpPost("lookup")]
        [ProducesResponseType(typeof(ServiceResponse<TblLookup>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<TblLookup>> InsertLookup([FromBody] TblLookup req)
        {
            string authorization = HttpContext.Request.Headers["Authorization"];
            if (authorization != null)
            {
                var token = authorization.Split(" ")[1];
                var claims = TokenManager.GetPrincipal(token);
                req.CreatedBy_Id = int.Parse(claims.PegawaiId);
            }

            var res = await _lookupRepository.InsertLookup(req);

            return new ServiceResponse<TblLookup>
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
        [HttpPut("lookup")]
        [ProducesResponseType(typeof(ServiceResponse<TblLookup>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<TblLookup>> UpdateLookup([FromBody] TblLookup req)
        {
            string authorization = HttpContext.Request.Headers["Authorization"];
            if (authorization != null)
            {
                var token = authorization.Split(" ")[1];
                var claims = TokenManager.GetPrincipal(token);
                req.UpdatedBy_Id = int.Parse(claims.PegawaiId);
            }

            var res = await _lookupRepository.UpdateLookup(req);

            return new ServiceResponse<TblLookup>
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
        [HttpDelete("lookup")]
        [ProducesResponseType(typeof(ServiceResponse<string>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<string>> DeleteLookup([FromQuery] LookupByIdVM req)
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
