using Ekr.Core.Entities.DataMaster.MasterAplikasi;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ekr.Repository.Contracts.DataMaster.MasterAplikasi
{
    public interface IMasterAplikasiRepository
    {
        Task<TblMasterAplikasi> GetMasterAplikasiById(int Id);
        List<string> GetMasterAplikasiByIds(List<string> Ids);
    }
}
