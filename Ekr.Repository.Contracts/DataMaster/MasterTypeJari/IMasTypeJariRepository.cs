using Ekr.Core.Entities.DataMaster.MasterTypeJari;
using System.Threading.Tasks;

namespace Ekr.Repository.Contracts.DataMaster.MasterTypeJari
{
    public interface IMasTypeJariRepository
    {
        Task<TblMasterTypeJari> GetTypeJari(int? Id);
    }
}
