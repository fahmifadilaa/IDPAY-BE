using Ekr.Core.Entities;
using Ekr.Core.Entities.DataMaster.Unit;
using System.Threading.Tasks;

namespace Ekr.Repository.Contracts.DataMaster.Unit
{
    public interface IUnitRepository
    {
        Task<GridResponse<UnitVM>> GridGetAll(UnitFilter req);
        Task<GridResponse<DepartmentVM>> GridGetAllDepartment(DepartmentFilter req);
        Task<TblUnitVM> Get(int Id);
        Task<TblUnitVM> GetByKodeOutlet(string KodeOutlet);
        Task<TblUnitVM> Create(TblUnitVM req);
        Task<TblUnitVM> Update(TblUnitVM req);
        Task<bool> Delete(string ids, int PegawaiId);
    }
}
