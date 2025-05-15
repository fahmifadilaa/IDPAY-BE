using Ekr.Core.Entities.DataMaster.UserFinger;
using System.Threading.Tasks;

namespace Ekr.Repository.Contracts.DataMaster.UserFinger
{
    public interface IUserFingerRepository
    {
        Task<TblPegawaiFinger> GetDataFinger(int PegawaiId, int TypeFingerId);
        Task<bool> Update(int Id);
        Task<TblPegawaiFinger> Create(TblPegawaiFinger req);
    }
}
