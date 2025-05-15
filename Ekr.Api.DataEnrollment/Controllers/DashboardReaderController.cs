using Ekr.Api.DataEnrollment.Filters;
using Ekr.Core.Constant;
using Ekr.Core.Entities;
using Ekr.Core.Entities.DataEnrollment.ViewModel;
using Ekr.Repository.Contracts.DataEnrollment;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ekr.Api.DataEnrollment.Controllers
{
    /// <summary>
    /// api dashboard reader
    /// </summary>
    [Route("dashboard/reader")]
    [ApiController]
    public class DashboardReaderController : ControllerBase
    {
        private readonly IDashboardReaderRepository _dashboardReaderRepository;

        public DashboardReaderController(IDashboardReaderRepository dashboardReaderRepository)
        {
            _dashboardReaderRepository = dashboardReaderRepository;
        }

        /// <summary>
        /// Untuk load monitoring alat
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        [HttpGet("monitoring_reader")]
        [ProducesResponseType(typeof(ServiceResponses<MonitoringReaderDataVM>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Untuk load monitoring alat")]
        public async Task<ServiceResponses<MonitoringReaderDataVM>> GetMonitoringReaderChart([FromQuery] string UnitIds)
        {
            var res = await _dashboardReaderRepository.GetMonitoringReaderChart(UnitIds);

            return new ServiceResponses<MonitoringReaderDataVM>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = res
            };
        }

        /// <summary>
        /// Untuk load presentase alat digunakan
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        [HttpGet("presentase_alat_digunakan")]
        [ProducesResponseType(typeof(ServiceResponses<PresentaseReaderDigunakanVM>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Untuk load presentase alat digunakan")]
        public async Task<ServiceResponses<PresentaseReaderDigunakanVM>> GetPresentaseAlatDigunakanChart([FromQuery] string UnitIds)
        {
            var res = await _dashboardReaderRepository.GetPresentaseAlatDigunakanChart(UnitIds);

            return new ServiceResponses<PresentaseReaderDigunakanVM>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = res
            };
        }
    }
}
