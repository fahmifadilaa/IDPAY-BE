using Ekr.Api.EnrollmentMonitoring.Filters;
using Ekr.Core.Entities;
using Ekr.Core.Entities.CekAlat;
using Ekr.Repository.Contracts.DataEnrollment;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Ekr.Api.EnrollmentMonitoring.Controllers
{
    [Route("CekAlat")]
    [ApiController]
    public class CekAlatController :ControllerBase
    {
        private readonly ICekAlatRepository _cekAlatRepository;

        public CekAlatController(ICekAlatRepository cekAlatRepository)
        {
            _cekAlatRepository = cekAlatRepository;
        }

        #region GET
        [HttpGet("GridGetDashboard1")]
        [ProducesResponseType(typeof(GridResponse<DataDashboard1_ViewModels>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Enrollment Monitoring")]
        public async Task<GridResponse<DataDashboard1_ViewModels>> GridGetDashboard1([FromQuery] CekAlatFilter req)
        {
            return await _cekAlatRepository.GridGetDashboard1(req);
        }

        [HttpGet("GridGetDashboard2")]
        [ProducesResponseType(typeof(GridResponse<DataDashboard2_ViewModels>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Enrollment Monitoring")]
        public async Task<GridResponse<DataDashboard2_ViewModels>> GridGetDashboard2([FromQuery] CekAlatFilter req)
        {
            return await _cekAlatRepository.GridGetDashboard2(req);
        }

        [HttpGet("ChartWeekly")]
        [ProducesResponseType(typeof(GridResponse<DataDashboard3_ViewModels>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Enrollment Monitoring")]
        public async Task<GridResponse<DataDashboard3_ViewModels>> ChartWeekly([FromQuery] CekAlatFilter req)
        {
            return await _cekAlatRepository.ChartWeekly(req);
        }
        #endregion
    }
}
