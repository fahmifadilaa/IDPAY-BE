using Ekr.Core.Constant;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace Ekr.Core.Services
{
    public class HttpRequestService : IHttpRequestService
    {
        private readonly JsonSerializerSettings _jsonSerializer = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        };

        public async Task<TResponse> SendRequestAsync<TResponse, TRequest>(string endpoint, Method method,
            Dictionary<string, string> headers, TRequest body, SendMethodByContentType type, string baseUrl)
        {
            bool isDownload = false;

            var cts = new CancellationTokenSource();

            var client = new RestClient(baseUrl);

            var request = new RestRequest(endpoint, method);

            // add request header from dictionary
            foreach (var key in headers.Keys)
            {
                if (!String.IsNullOrEmpty(headers[key]))
                    request.AddHeader(key, headers[key]);

                if (headers[key].Contains(ContentType.OCTET_STREAM))
                    isDownload = true;
            }

            foreach (var accepCharset in request.Parameters.Where(x => x.Name.Equals("Accept-Charset")))
            {
                request.Parameters.Remove(accepCharset);
            }

            // add request body
            if (body != null)
                switch (type)
                {
                    case SendMethodByContentType.RAW:
                        request.AddJsonBody(JsonConvert.SerializeObject(body, _jsonSerializer));
                        break;
                    case SendMethodByContentType.URL_ENCODED:
                        var dictBody = (Dictionary<string, string>)Convert.ChangeType(body, typeof(Dictionary<string, string>));
                        var reqBody = new StringBuilder();
                        foreach (var key in dictBody.Keys)
                        {
                            reqBody.Append(key).Append('=').Append(dictBody[key]).Append('&');
                        }
                        string reqBodyString = reqBody.Remove(reqBody.Length - 1, 1).ToString();
                        request.AddParameter("application/x-www-form-urlencoded", reqBodyString, ParameterType.RequestBody);
                        break;
                }

            if (!isDownload)
            {
                var response = await client.ExecuteAsync<TResponse>(request, cts.Token)
                    .ConfigureAwait(false);

                if (response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == (HttpStatusCode)422)
                    return response.Data;

                if (!response.IsSuccessful)
                    throw new Exception();

                return response.Data;
            }
            else
            {
                var res = client.DownloadData(request);

                return (TResponse)Convert.ChangeType(res, typeof(TResponse));
            }
        }

        public async Task<TResponse> SendRequestIgnoreSSLAsync<TResponse, TRequest>(string endpoint, Method method,
            Dictionary<string, string> headers, TRequest body, SendMethodByContentType type, string baseUrl)
        {
            bool isDownload = false;

            var cts = new CancellationTokenSource();

            var client = new RestClient(baseUrl);
            //client.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;

            //ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;

            var request = new RestRequest(endpoint, method);

            // add request header from dictionary
            foreach (var key in headers.Keys)
            {
                if (!String.IsNullOrEmpty(headers[key]))
                    request.AddHeader(key, headers[key]);

                if (headers[key].Contains(ContentType.OCTET_STREAM))
                    isDownload = true;
            }

            foreach (var accepCharset in request.Parameters.Where(x => x.Name.Equals("Accept-Charset")))
            {
                request.Parameters.Remove(accepCharset);
            }

            // add request body
            if (body != null)
                switch (type)
                {
                    case SendMethodByContentType.RAW:
                        request.AddJsonBody(JsonConvert.SerializeObject(body, _jsonSerializer));
                        break;
                    case SendMethodByContentType.URL_ENCODED:
                        var dictBody = (Dictionary<string, string>)Convert.ChangeType(body, typeof(Dictionary<string, string>));
                        var reqBody = new StringBuilder();
                        foreach (var key in dictBody.Keys)
                        {
                            reqBody.Append(key).Append('=').Append(dictBody[key]).Append('&');
                        }
                        string reqBodyString = reqBody.Remove(reqBody.Length - 1, 1).ToString();
                        request.AddParameter("application/x-www-form-urlencoded", reqBodyString, ParameterType.RequestBody);
                        break;
                }

            if (!isDownload)
            {
                var response = await client.ExecuteAsync<TResponse>(request, cts.Token)
                    .ConfigureAwait(false);

                if (response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == (HttpStatusCode)422)
                    return response.Data;

                if (!response.IsSuccessful)
                    throw new Exception();

                return response.Data;
            }
            else
            {
                var res = client.DownloadData(request);

                return (TResponse)Convert.ChangeType(res, typeof(TResponse));
            }
        }

        public async Task<TResponse> SendGetRequestAsync<TResponse>(string endpoint,
            SendMethodByContentType type, string baseUrl)
        {

            var defaultHeaders = new Dictionary<string, string>
                {
                    {"Content-Type", ContentType.JSON }
                };

            return await SendRequestAsync<TResponse, object>(
                endpoint,
                Method.GET,
                defaultHeaders,
                null,
                type,
                baseUrl
                ).ConfigureAwait(false);
        }

        public async Task<TResponse> SendPostRequestCrawlingAsync<TResponse, TRequest>(string endpoint,
            SendMethodByContentType type, string baseUrl, TRequest request, string headername, string headervalue)
        {

            var defaultHeaders = new Dictionary<string, string>
                {
                    {"Content-Type", ContentType.JSON },
                    {headername, headervalue }
                };

            return await SendRequestAsync<TResponse, TRequest>(
                endpoint,
                Method.POST,
                defaultHeaders,
                request,
                type,
                baseUrl
                ).ConfigureAwait(false);
        }
        public async Task<TResponse> SendPostRequestAsync<TResponse, TRequest>(string endpoint,
            SendMethodByContentType type, string baseUrl, TRequest request)
        {

            var defaultHeaders = new Dictionary<string, string>
                {
                    {"Content-Type", ContentType.JSON },
                };

            return await SendRequestAsync<TResponse, TRequest>(
                endpoint,
                Method.POST,
                defaultHeaders,
                request,
                type,
                baseUrl
                ).ConfigureAwait(false);
        }
        
        public async Task<TResponse> SendPostRequestIgnoreSSLAsync<TResponse, TRequest>(string endpoint,
            SendMethodByContentType type, string baseUrl, TRequest request)
        {

            var defaultHeaders = new Dictionary<string, string>
                {
                    {"Content-Type", ContentType.JSON },
                };

            return await SendRequestIgnoreSSLAsync<TResponse, TRequest>(
                endpoint,
                Method.POST,
                defaultHeaders,
                request,
                type,
                baseUrl
                ).ConfigureAwait(false);
        }

        public async Task<TResponse> SendPostRequestAsyncFormData<TResponse, TRequest>(string endpoint,
            SendMethodByContentType type, string baseUrl, TRequest request)
        {

            var defaultHeaders = new Dictionary<string, string>
                {
                    {"Content-Type", ContentType.JSON },
                };

            return await SendRequestAsyncFormData<TResponse, TRequest>(
                endpoint,
                Method.POST,
                defaultHeaders,
                request,
                type,
                baseUrl
                ).ConfigureAwait(false);
        }

        public async Task<TResponse> SendRequestAsyncFormData<TResponse, TRequest>(string endpoint, Method method,
            Dictionary<string, string> headers, TRequest body, SendMethodByContentType type, string baseUrl)
        {
            bool isDownload = false;

            var cts = new CancellationTokenSource();

            var client = new RestClient(baseUrl);

            var request = new RestRequest(endpoint, method);
            var formData = new MultipartFormDataContent();

            // add request header from dictionary
            foreach (var key in headers.Keys)
            {
                if (!String.IsNullOrEmpty(headers[key]))
                    request.AddHeader(key, headers[key]);

                if (headers[key].Contains(ContentType.OCTET_STREAM))
                    isDownload = true;
            }

            foreach (var accepCharset in request.Parameters.Where(x => x.Name.Equals("Accept-Charset")))
            {
                request.Parameters.Remove(accepCharset);
            }

            // add request body
            if (body != null)
                switch (type)
                {
                    case SendMethodByContentType.RAW:
                        request.AddJsonBody(JsonConvert.SerializeObject(body, _jsonSerializer));
                        break;
                    case SendMethodByContentType.URL_ENCODED:
                        var dictBody = (Dictionary<string, string>)Convert.ChangeType(body, typeof(Dictionary<string, string>));
                        var reqBody = new StringBuilder();
                        foreach (var key in dictBody.Keys)
                        {
                            reqBody.Append(key).Append('=').Append(dictBody[key]).Append('&');
                        }
                        string reqBodyString = reqBody.Remove(reqBody.Length - 1, 1).ToString();
                        request.AddParameter("application/x-www-form-urlencoded", reqBodyString, ParameterType.RequestBody);
                        break;
                    case SendMethodByContentType.FORM_DATA:
                        
                        foreach (var prop in body.GetType().GetProperties())
                        {
                            string name = prop.Name;

                            if (prop.PropertyType == typeof(IFormFile))
                            {
                                IFormFile file = ((IFormFile)prop.GetValue(body, null));
                                if (file != null)
                                    formData.Add(new StreamContent(file.OpenReadStream()), prop.Name, file.FileName);
                            }
                            else if (prop.PropertyType == typeof(IFormFile[]))
                            {
                                IFormFile[] files = ((IFormFile[])prop.GetValue(body, null));
                                if (files != null)
                                {
                                    foreach (var file in files)
                                    {
                                        if (file != null)
                                        {
                                            formData.Add(new StreamContent(file.OpenReadStream()), prop.Name, file.FileName);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                var value = (prop.GetValue(body, null) ?? string.Empty).ToString();
                                formData.Add(new StringContent(value), prop.Name);
                            }
                        }
                        break;

                }

            string JsonString = "";
            bool? resultApi = null;

            using (HttpClient httpClient = new HttpClient())
            {

                httpClient.DefaultRequestHeaders.Accept.Clear();

                var result = httpClient.PostAsync(baseUrl + endpoint, formData).Result;

                if (result.IsSuccessStatusCode)
                {
                    resultApi = true;
                    JsonString = result.Content.ReadAsStringAsync().Result;

                    var data = JsonConvert.DeserializeObject<TResponse>(JsonString);

                    return data;
                }
                else
                {
                    JsonString = result.Content.ReadAsStringAsync().Result;
                    var data = JsonConvert.DeserializeObject<TResponse>(JsonString);

                    return data;
                }
            }
        }
    }
}
