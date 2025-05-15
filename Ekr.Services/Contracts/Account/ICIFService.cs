using Ekr.Core.Entities;
using Ekr.Core.Entities.Account;
using Ekr.Core.Entities.ThirdParty;
using System.Threading.Tasks;

namespace Ekr.Services.Contracts.Account
{
    public interface ICIFService
    {
        Task<Core.Entities.Account.ServiceResponse<CekCIFDto>> GetCIF(NikDto req, NikDtoUrl url);
        //Task<Core.Entities.ServiceResponse<string>> GetSOAByCif(ApiSOA req, string UrlCIf);
        Task<ApiSOAResponse> GetSOAByCif(ApiSOA req);
    }
}
