using Ekr.Core.Entities;
using Ekr.Core.Entities.MessageCode;
using System.Threading.Tasks;

namespace Ekr.Repository.Contracts.MessageCode
{
    public interface IMessageCodeRepository
    {
        Task<GridResponse<MessageCodeVM>> LoadData(MessageCodeFilter req);
        Task<Tbl_Master_MessageCode> GetById(MessageCodeByIdVM req);
        Task<Tbl_Master_MessageCode> InsertMessageCode(Tbl_Master_MessageCode req);
        Task<Tbl_Master_MessageCode> UpdateMessageCode(Tbl_Master_MessageCode req);
        Task DeleteMessageCode(MessageCodeByIdVM req, int PegawaiId);
    }
}
