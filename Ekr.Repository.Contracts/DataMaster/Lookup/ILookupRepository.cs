using Ekr.Core.Entities;
using Ekr.Core.Entities.DataMaster.Lookup;
using System.Threading.Tasks;

namespace Ekr.Repository.Contracts.DataMaster.Lookup
{
    public interface ILookupRepository
    {
        Task<GridResponse<LookupVM>> LoadData(LookupFilter req);
        Task<TblLookup> GetById(LookupByIdVM req);
        Task<TblLookup> GetByType(string Type);
        Task<TblLookup> InsertLookup(TblLookup req);
        Task<TblLookup> UpdateLookup(TblLookup req);
        Task DeleteLookup(LookupByIdVM req, int PegawaiId);
    }
}
