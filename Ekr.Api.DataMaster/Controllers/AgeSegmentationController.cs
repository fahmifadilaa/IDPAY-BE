using Ekr.Api.DataMaster.Filters;
using Ekr.Auth;
using Ekr.Core.Constant;
using Ekr.Core.Entities;
using Ekr.Core.Entities.DataMaster.AgeSegmentation.Entity;
using Ekr.Core.Entities.DataMaster.AgeSegmentation.ViewModel;
using Ekr.Repository.Contracts.DataMaster.AgeSegmentation;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Ekr.Api.DataMaster.Controllers
{
    /// <summary>
    /// api Segmentasi Usia
    /// </summary>
    [Route("dm/agesegmentation")]
    [ApiController]
    public class AgeSegmentationController : ControllerBase
    {
        private readonly IAgeSegmentationRepository _ageSegmentationRepository;

        public AgeSegmentationController(IAgeSegmentationRepository ageSegmentationRepository)
        {
            _ageSegmentationRepository = ageSegmentationRepository;
        }

        /// <summary>
        /// Untuk load all data segmentasi usia
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("age_segmentations")]
        [ProducesResponseType(typeof(ServiceResponse<GridResponse<AgeSegmentationVM>>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<GridResponse<AgeSegmentationVM>>> LoadData([FromBody] AgeSegmentationFilterVM req)
        {
            var res = await _ageSegmentationRepository.LoadData(req);

            return new ServiceResponse<GridResponse<AgeSegmentationVM>>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = res
            };
        }
        //public async Task<ServiceResponseEncrypted<ServiceResponse<GridResponse<AgeSegmentationVM>>>> LoadData([FromBody] AgeSegmentationFilterVM req)
        //{
        //    var res = await _ageSegmentationRepository.LoadData(req);

        //    return new ServiceResponseEncrypted<ServiceResponse<GridResponse<AgeSegmentationVM>>>
        //    {
        //        Data = new ServiceResponse<GridResponse<AgeSegmentationVM>>
        //        {
        //            Status = (int)ServiceResponseStatus.SUKSES,
        //            Message = "Success",
        //            Data = res
        //        }
        //    };
        //}


        /// <summary>
        /// Untuk get data segmentasi usia by Id
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet("age_segmentation")]
        [ProducesResponseType(typeof(ServiceResponse<TblMasterSegmentasiUsia>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<TblMasterSegmentasiUsia>> GetAgeSegmentation([FromQuery] AgeSegmentationViewFilterVM req)
        {
            var res = await _ageSegmentationRepository.GetAgeSegmentation(req);

            return new ServiceResponse<TblMasterSegmentasiUsia>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = res
            };
        }

        /// <summary>
        /// Untuk create segmentasi usia
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("age_segmentation")]
        [ProducesResponseType(typeof(ServiceResponse<TblMasterSegmentasiUsia>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<TblMasterSegmentasiUsia>> InsertAgeSegmentation([FromBody] TblMasterSegmentasiUsia req)
        {
            string authorization = HttpContext.Request.Headers["Authorization"];
            if (authorization != null)
            {
                var token = authorization.Split(" ")[1];
                var claims = TokenManager.GetPrincipal(token);
                req.CreatedBy_Id = int.Parse(claims.PegawaiId);
            }

            var res = await _ageSegmentationRepository.InsertAgeSegmentation(req);

            return new ServiceResponse<TblMasterSegmentasiUsia>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = res
            };
        }

        /// <summary>
        /// Untuk update segmentasi usia
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPut("age_segmentation")]
        [ProducesResponseType(typeof(ServiceResponse<TblMasterSegmentasiUsia>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<TblMasterSegmentasiUsia>> UpdateAgeSegmentation([FromBody] TblMasterSegmentasiUsia req)
        {
            string authorization = HttpContext.Request.Headers["Authorization"];
            if (authorization != null)
            {
                var token = authorization.Split(" ")[1];
                var claims = TokenManager.GetPrincipal(token);
                req.UpdatedBy_Id = int.Parse(claims.PegawaiId);
            }

            var res = await _ageSegmentationRepository.UpdateAgeSegmentation(req);

            return new ServiceResponse<TblMasterSegmentasiUsia>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = res
            };
        }

        /// <summary>
        /// Untuk delete segmentasi usia
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpDelete("age_segmentation")]
        [ProducesResponseType(typeof(ServiceResponse<string>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<string>> DeleteAgeSegmentation([FromQuery] AgeSegmentationViewFilterVM req)
        {
            int PegawaiId = 100;
            string authorization = HttpContext.Request.Headers["Authorization"];
            if (authorization != null)
            {
                var token = authorization.Split(" ")[1];
                var claims = TokenManager.GetPrincipal(token);
                PegawaiId = int.Parse(claims.PegawaiId);
            }

            await _ageSegmentationRepository.DeleteAgeSegmentation(req, PegawaiId);

            return new ServiceResponse<string>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success"
            };
        }

    }
}
