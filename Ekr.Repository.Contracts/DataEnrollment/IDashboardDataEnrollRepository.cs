using Ekr.Core.Entities;
using Ekr.Core.Entities.DataEnrollment;
using Ekr.Core.Entities.DataEnrollment.ViewModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ekr.Repository.Contracts.DataEnrollment
{
    public interface IDashboardDataEnrollRepository
    {
        Task<IEnumerable<JobChartDataVM>> GetJobChart(UnitIdsFilterVM req);
        Task<IEnumerable<JobChartDataVM>> GetJobChart2(UnitIdsFilterVM2 req);
        Task<IEnumerable<TypeEnrollmentVM>> GetTypeEnrollmentChart(UnitIdsFilterVM req);
        Task<IEnumerable<TypeEnrollmentVM>> GetTypeEnrollmentChart2(UnitIdsFilterVM2 req);
        Task<IEnumerable<ChannelEnrollmentVM>> GetChannelEnrollmentChart(UnitIdsFilterVM req);
        Task<IEnumerable<ChannelEnrollmentVM>> GetChannelEnrollmentChart2(UnitIdsFilterVM2 req);
        Task<IEnumerable<StatusEnrollmentVM>> GetStatusEnrollmentChart(UnitIdsFilterVM req);
        Task<IEnumerable<StatusEnrollmentVM>> GetStatusEnrollmentChart2(UnitIdsFilterVM2 req);
        Task<IEnumerable<ReligionChartDataVM>> GetReligionChart(UnitIdsFilterVM req);
        Task<IEnumerable<ReligionChartDataVM>> GetReligionChart2(UnitIdsFilterVM2 req);
        Task<IEnumerable<BornGenerationChartDataVM>> GetBornGenerationChart(UnitIdsFilterVM req);
        Task<IEnumerable<BornGenerationChartDataVM>> GetBornGenerationChart2(UnitIdsFilterVM2 req);
        Task<IEnumerable<AgeSegmentationChartDataVM>> GetAgeSegmentationChart(UnitIdsFilterVM req);
        Task<IEnumerable<AgeSegmentationChartDataVM>> GetAgeSegmentationChart2(UnitIdsFilterVM2 req);
        Task<GridResponse<EnrollPerUnitVM>> GetEnrollPerUnit(EnrollPerUnitFilterVM req);
        Task<EnrollKTPVM> DetailData(EnrollKTPFIlterVM req);
        Task<GridResponse<DahboardEnrollmentPG>> DashboardEnrollList(DahboardEnrollmentPGFilterVM req);
        Task<GridResponse<MonitoringEnroll>> GetDBEnroll(DataEnrollTempFilter filter);
    }
}
