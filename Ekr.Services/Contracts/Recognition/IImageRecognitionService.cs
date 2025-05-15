using Ekr.Core.Entities.Auth;
using Ekr.Core.Entities.Recognition;
using System.Threading.Tasks;

namespace Ekr.Services.Contracts.Recognition
{
    public interface IImageRecognitionService
    {
        Task<ServiceResponse<MatchImageRes>> MatchImageBase64ToBase64(Base64ToBase64Req req, UrlRequestRecognition UrlReq);
        Task<ServiceResponse<MatchingFingerRes>> MatchUrlImagesToBase64Json(UrlToBase64Req req, UrlRequestRecognition UrlReq);
    }
}
