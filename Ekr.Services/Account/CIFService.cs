using Ekr.Core.Constant;
using Ekr.Core.Entities;
using Ekr.Core.Entities.Account;
using Ekr.Core.Entities.ThirdParty;
using Ekr.Core.Services;
using Ekr.Services.Contracts.Account;
using Newtonsoft.Json;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Ekr.Services.Account
{
    public class CIFService : ICIFService
    {
        private readonly IHttpRequestService _httpRequestService;

        public CIFService(IHttpRequestService httpRequestService)
        {
            _httpRequestService = httpRequestService;
        }

        public Task<Core.Entities.Account.ServiceResponse<CekCIFDto>> GetCIF(NikDto req, NikDtoUrl url)
        {
            //const string baseUrl = "http://147.139.192.2/ServiceCekCIF";
            //const string endpoint = "/api/CekCIF/CekDataCIF";

            return _httpRequestService.SendPostRequestAsync<Core.Entities.Account.ServiceResponse<CekCIFDto>, NikDto>(
                url.endpoint,
                SendMethodByContentType.RAW,
                url.baseUrl,
                req
                );
        }

        //public async Task<Core.Entities.ServiceResponse<string>> GetSOAByCif(ApiSOA req, string UrlCIf)
        //{
        //    var data = new Core.Entities.ServiceResponse<string>();

        //    using (var httpClient = new HttpClient())
        //    {
        //        StringContent content = new(JsonConvert.SerializeObject(req), Encoding.UTF8, "application/json");

        //        using var response = await httpClient.PostAsync(req.host, content);

        //        string apiResponse = await response.Content.ReadAsStringAsync();

        //        data = JsonConvert.DeserializeObject<Core.Entities.ServiceResponse<string>>(apiResponse);
        //    }

        //    return data;
        //}

        public async Task<ApiSOAResponse> GetSOAByCif(ApiSOA req)
        {
            var res = new ApiSOAResponse();

            var soapString = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                    "<soapenv:Envelope xmlns:soapenv='http://schemas.xmlsoap.org/soap/envelope/'>" +
                       "<soapenv:Body>" +
                           "<core:transaction xmlns:core='http://service.bni.co.id/core'>" +
                               "<request>" +
                                  "<systemId>" + req.systemId + "</systemId>" +
                                  "<customHeader>" +
                                    "<branch>" + req.branch + "</branch>" +
                                    "<terminal>001</terminal>" +
                                    "<teller>" + req.teller + "</teller>" +
                                    "<overrideFlag>I</overrideFlag>" +
                                "</customHeader>" +
                                  "<content xsi:type=\"bo:CustomerSearchByIdReq\" xmlns:bo='http://service.bni.co.id/core/bo' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'>" +
                                             "<numId>" + req.numId + "</numId>" +
                                             "<idType>" + req.idType + "</idType>" +
                                          "</content>" +
                                       "</request>" +
                                    "</core:transaction>" +
                                  "</soapenv:Body>" +
                                "</soapenv:Envelope>";
            XDocument xDoc = XDocument.Load(new System.IO.StringReader
                    (await (await PostXmlRequestAsync(req.host, soapString))
                    .Content.ReadAsStringAsync()));


            if (xDoc.Descendants("coreJournal").FirstOrDefault() == null || xDoc.Descendants("errorNum").FirstOrDefault() != null)
            {
                res.errorNum = xDoc.Descendants("errorNum").FirstOrDefault().Value;
                res.errorDescription = xDoc.Descendants("errorDescription").FirstOrDefault().Value;
                res.coreJournal = null;
                res.cif = null;
            }
            else
            {
                res.errorNum = null;
                res.errorDescription = null;
                res.coreJournal = xDoc.Descendants("coreJournal").FirstOrDefault().Value; ;
                res.cif = xDoc.Descendants("cif_number").FirstOrDefault().Value; ;
            }

            return res;
        }

        public static async Task<HttpResponseMessage> PostXmlRequestAsync(string baseUrl, string xmlString)
        {
            using HttpClient httpClient = new();
            StringContent httpContent = new(xmlString, Encoding.UTF8, "text/xml");
            return await httpClient.PostAsync(baseUrl, httpContent);
        }
    }
}
