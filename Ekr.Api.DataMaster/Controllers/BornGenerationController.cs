using Ekr.Api.DataMaster.Filters;
using Ekr.Auth;
using Ekr.Core.Constant;
using Ekr.Core.Entities;
using Ekr.Core.Entities.DataMaster.BornGeneration.Entity;
using Ekr.Core.Entities.DataMaster.BornGeneration.ViewModel;
using Ekr.Repository.Contracts.DataMaster.BornGeneration;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Ekr.Api.DataMaster.Controllers
{
    /// <summary>
    /// api Generasi Kelahiran
    /// </summary>
    [Route("dm/borngeneration")]
    [ApiController]
    public class BornGenerationController : ControllerBase
    {
        private readonly IBornGenerationRepository _bornGenerationRepository;

        public BornGenerationController(IBornGenerationRepository bornGenerationRepository)
        {
            _bornGenerationRepository = bornGenerationRepository;
        }

        /// <summary>
        /// Untuk load all data generasi kelahiran
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("born_generations")]
        [ProducesResponseType(typeof(ServiceResponse<GridResponse<BornGenerationVM>>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<GridResponse<BornGenerationVM>>> LoadData([FromBody] BornGenerationFilterVM req)
        {
            var res = await _bornGenerationRepository.LoadData(req);

            return new ServiceResponse<GridResponse<BornGenerationVM>>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = res
            };
        }

        /// <summary>
        /// Untuk get data generasi kelahiran by Id
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet("born_generation")]
        [ProducesResponseType(typeof(ServiceResponse<TblMasterGenerasiLahir>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<TblMasterGenerasiLahir>> GetBornGeneration([FromQuery] BornGenerationViewFilterVM req)
        {
            var res = await _bornGenerationRepository.GetBornGeneration(req);

            return new ServiceResponse<TblMasterGenerasiLahir>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = res
            };
        }

        /// <summary>
        /// Untuk create generasi kelahiran
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("born_generation")]
        [ProducesResponseType(typeof(ServiceResponse<TblMasterGenerasiLahir>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<TblMasterGenerasiLahir>> InsertBornGeneration([FromBody] TblMasterGenerasiLahir req)
        {
            string authorization = HttpContext.Request.Headers["Authorization"];
            if (authorization != null)
            {
                var token = authorization.Split(" ")[1];
                var claims = TokenManager.GetPrincipal(token);
                req.CreatedBy_Id = int.Parse(claims.PegawaiId);
            }

            var res = await _bornGenerationRepository.InsertBornGeneration(req);

            return new ServiceResponse<TblMasterGenerasiLahir>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = res
            };
        }

        /// <summary>
        /// Untuk update generasi lahir
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPut("born_generation")]
        [ProducesResponseType(typeof(ServiceResponse<TblMasterGenerasiLahir>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<TblMasterGenerasiLahir>> UpdateBornGeneration([FromBody] TblMasterGenerasiLahir req)
        {
            string authorization = HttpContext.Request.Headers["Authorization"];
            if (authorization != null)
            {
                var token = authorization.Split(" ")[1];
                var claims = TokenManager.GetPrincipal(token);
                req.UpdatedBy_Id = int.Parse(claims.PegawaiId);
            }

            var res = await _bornGenerationRepository.UpdateBornGeneration(req);

            return new ServiceResponse<TblMasterGenerasiLahir>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = res
            };
        }

        /// <summary>
        /// Untuk delete generasi kelahiran
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpDelete("born_generation")]
        [ProducesResponseType(typeof(ServiceResponse<string>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<string>> DeleteBornGeneration([FromQuery] BornGenerationViewFilterVM req)
        {
            int PegawaiId = 100;
            string authorization = HttpContext.Request.Headers["Authorization"];
            if (authorization != null)
            {
                var token = authorization.Split(" ")[1];
                var claims = TokenManager.GetPrincipal(token);
                PegawaiId = int.Parse(claims.PegawaiId);
            }
            await _bornGenerationRepository.DeleteBornGeneration(req, PegawaiId);

            return new ServiceResponse<string>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success"
            };
        }
    }
}
