using Ekr.Core.Constant;
using RestSharp;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ekr.Core.Services
{
    public interface IHttpRequestService
    {
        Task<TResponse> SendRequestAsync<TResponse, TRequest>(string endpoint, Method method,
            Dictionary<string, string> headers, TRequest body, SendMethodByContentType type, string baseUrl);
        Task<TResponse> SendRequestAsyncFormData<TResponse, TRequest>(string endpoint, Method method,
            Dictionary<string, string> headers, TRequest body, SendMethodByContentType type, string baseUrl);
        Task<TResponse> SendGetRequestAsync<TResponse>(string endpoint,
            SendMethodByContentType type, string baseUrl);
        Task<TResponse> SendPostRequestAsync<TResponse, TRequest>(string endpoint,
            SendMethodByContentType type, string baseUrl, TRequest request);

        Task<TResponse> SendPostRequestIgnoreSSLAsync<TResponse, TRequest>(string endpoint,
           SendMethodByContentType type, string baseUrl, TRequest request);

        Task<TResponse> SendPostRequestAsyncFormData<TResponse, TRequest>(string endpoint,
            SendMethodByContentType type, string baseUrl, TRequest request);

        Task<TResponse> SendPostRequestCrawlingAsync<TResponse, TRequest>(string endpoint,
            SendMethodByContentType type, string baseUrl, TRequest request, string headername, string headervalue);


    }
}
