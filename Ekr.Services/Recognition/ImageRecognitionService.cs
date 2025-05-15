using Ekr.Core.Constant;
using Ekr.Core.Entities.Auth;
using Ekr.Core.Entities.Recognition;
using Ekr.Core.Services;
using Ekr.Services.Contracts.Recognition;
using System.Threading.Tasks;

namespace Ekr.Services.Recognition
{
    public class ImageRecognitionService : IImageRecognitionService
    {
        private readonly IHttpRequestService _httpRequestService;

        public ImageRecognitionService(IHttpRequestService httpRequestService)
        {
            _httpRequestService = httpRequestService;
        }

        public Task<ServiceResponse<MatchImageRes>> MatchImageBase64ToBase64(Base64ToBase64Req req, UrlRequestRecognition UrlReq)
        {
            return _httpRequestService.SendPostRequestAsync<ServiceResponse<MatchImageRes>, Base64ToBase64Req>(
                UrlReq.EndPoint,
                SendMethodByContentType.RAW,
                UrlReq.BaseUrl,
                req
                );
        }

        public Task<ServiceResponse<MatchingFingerRes>> MatchUrlImagesToBase64Json(UrlToBase64Req req, UrlRequestRecognition UrlReq)
        {
            return _httpRequestService.SendPostRequestAsync<ServiceResponse<MatchingFingerRes>, UrlToBase64Req>(
                UrlReq.EndPoint,
                SendMethodByContentType.RAW,
                UrlReq.BaseUrl,
                req
                );
        }
    }
}
