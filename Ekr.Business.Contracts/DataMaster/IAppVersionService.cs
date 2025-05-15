using Ekr.Core.Entities.DataMaster.AlatReader;
using System.Threading.Tasks;

namespace Ekr.Business.Contracts.DataMaster
{
    public interface IAppVersionService
    {
        Task UploadApps(UploadAppsReq uploadAppsReq, int Id, string npp, string unitCode);
        Task<(byte[] fileByte, string appsVersion)> CheckVersion(CheckAppsVersionRequest checkAppsVersionRequest);
        Task<(string fileBase64, string filePath)> CheckVersionV2(CheckAppsVersionRequest checkAppsVersionRequest);
        Task<(string fileBase64, string filePath)> GetVersionById(int Id);
    }
}
