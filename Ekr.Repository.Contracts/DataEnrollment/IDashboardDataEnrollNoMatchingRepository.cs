using Ekr.Core.Entities;
using Ekr.Core.Entities.DataEnrollment;
using Ekr.Core.Entities.DataEnrollment.ViewModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ekr.Repository.Contracts.DataEnrollment
{
    public interface IDashboardDataEnrollNoMatchingRepository
    {
        Task<IEnumerable<JobChartDataVM>> GetJobChart(UnitIdsFilterVM req);
        Task<IEnumerable<TypeEnrollmentVM>> GetTypeEnrollmentChart(UnitIdsFilterVM req);
        Task<IEnumerable<ChannelEnrollmentVM>> GetChannelEnrollmentChart(UnitIdsFilterVM req);
        Task<IEnumerable<StatusEnrollmentVM>> GetStatusEnrollmentChart(UnitIdsFilterVM req);
        Task<IEnumerable<ReligionChartDataVM>> GetReligionChart(UnitIdsFilterVM req);
        Task<IEnumerable<BornGenerationChartDataVM>> GetBornGenerationChart(UnitIdsFilterVM req);
        Task<IEnumerable<AgeSegmentationChartDataVM>> GetAgeSegmentationChart(UnitIdsFilterVM req);
        Task<GridResponse<EnrollPerUnitVM>> GetEnrollPerUnit(EnrollPerUnitFilterVM req);
        Task<EnrollKTPVM> DetailData(EnrollKTPFIlterVM req);
        Task<GridResponse<DahboardEnrollmentPG>> DashboardEnrollList(DahboardEnrollmentPGFilterVM req);
        Task<GridResponse<MonitoringEnroll>> GetDBEnrollNoMatchingFR(DataEnrollTempFilter filter);
    }
}
