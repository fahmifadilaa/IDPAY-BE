using Ekr.Api.DataMaster.Filters;
using Ekr.Auth;
using Ekr.Core.Constant;
using Ekr.Core.Entities;
using Ekr.Core.Entities.DataMaster.Unit;
using Ekr.Repository.Contracts.DataMaster.Unit;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Ekr.Api.DataMaster.Controllers
{
    [Route("dm/unit")]
    [ApiController]
    public class DataUnitController : ControllerBase
    {
        private readonly IUnitRepository _unitRepository;
        public DataUnitController(IUnitRepository unitRepository)
        {
            _unitRepository = unitRepository;
        }
        /// <summary>
        /// Untuk Get All Data Unit
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("getall")]
        [ProducesResponseType(typeof(GridResponse<UnitVM>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public Task<GridResponse<UnitVM>> GetAll([FromBody] UnitFilter req)
        {
            return _unitRepository.GridGetAll(req);
        }

        /// <summary>
        /// Untuk Get All Data Department
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("getAllDepartment")]
        [ProducesResponseType(typeof(GridResponse<DepartmentVM>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public Task<GridResponse<DepartmentVM>> GetAllDepartment([FromBody] DepartmentFilter req)
        {
            return _unitRepository.GridGetAllDepartment(req);
        }

        /// <summary>
        /// Untuk Get Data By Id
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpGet("getById")]
        [ProducesResponseType(typeof(ServiceResponse<TblUnitVM>), 201)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<TblUnitVM>> Get([FromQuery] int Id)
        {
            var res = await _unitRepository.Get(Id);

            return new ServiceResponse<TblUnitVM>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = res
            };
        }

        /// <summary>
        /// Untuk Create Data Department
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("create")]
        [ProducesResponseType(typeof(ServiceResponse<TblUnitVM>), 201)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<TblUnitVM>> Create([FromBody] TblUnitVM req)
        {
            string authorization = HttpContext.Request.Headers["Authorization"];
            if (authorization != null)
            {
                var token = authorization.Split(" ")[1];
                var claims = TokenManager.GetPrincipal(token);
                req.CreatedById = int.Parse(claims.PegawaiId);
            }

            var res = await _unitRepository.Create(req);

            return new ServiceResponse<TblUnitVM>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = res
            };
        }

        /// <summary>
        /// Untuk Update Data Department
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPut("update")]
        [ProducesResponseType(typeof(ServiceResponse<TblUnitVM>), 201)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<TblUnitVM>> Update([FromBody] TblUnitVM req)
        {
            string authorization = HttpContext.Request.Headers["Authorization"];
            if (authorization != null)
            {
                var token = authorization.Split(" ")[1];
                var claims = TokenManager.GetPrincipal(token);
                req.UpdatedById = int.Parse(claims.PegawaiId);
            }

            var res = await _unitRepository.Update(req);

            return new ServiceResponse<TblUnitVM>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = res
            };
        }

        /// <summary>
        /// Untuk Delete Data Department
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [HttpDelete("delete")]
        [ProducesResponseType(typeof(ServiceResponse), 201)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse> Delete([FromQuery] string ids)
        {
            int PegawaiId = 100;
            string authorization = HttpContext.Request.Headers["Authorization"];
            if (authorization != null)
            {
                var token = authorization.Split(" ")[1];
                var claims = TokenManager.GetPrincipal(token);
                PegawaiId = int.Parse(claims.PegawaiId);
            }

            _ = await _unitRepository.Delete(ids, PegawaiId);

            return new ServiceResponse
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success"
            };
        }
    }
}
