using Ekr.Core.Entities;
using Ekr.Core.Entities.DataEnrollment;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ekr.Repository.Contracts.DataEnrollment
{
    public interface IEnrollTempRepository
    {
        Task<GridResponse<MonitoringEnroll>> GetDataEnroolTemp(DataEnrollTempFilter filter);
        Task<GridResponse<MonitoringEnroll>> GetDataEnrool2Temp(DataEnrollTemp2Filter filter);
        Task<IEnumerable<ExportMonitoringEnroll>> ExportDataEnrool2Temp(ExportDataEnrollTemp2Filter filter);
    }
}
