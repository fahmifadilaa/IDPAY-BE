using Ekr.Core.Entities;
using Ekr.Core.Entities.CekAlat;
using System.Threading.Tasks;

namespace Ekr.Repository.Contracts.DataEnrollment
{
    public interface ICekAlatRepository
    {
        Task<GridResponse<DataDashboard1_ViewModels>> GridGetDashboard1(CekAlatFilter req);
        Task<GridResponse<DataDashboard2_ViewModels>> GridGetDashboard2(CekAlatFilter req);
        Task<GridResponse<DataDashboard3_ViewModels>> ChartWeekly(CekAlatFilter req);
    }
}
