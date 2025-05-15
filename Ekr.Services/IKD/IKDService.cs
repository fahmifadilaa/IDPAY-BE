using Ekr.Core.Constant;
using Ekr.Core.Entities.Account;
using Ekr.Core.Entities.ThirdParty;
using Ekr.Core.Entities;
using Ekr.Core.Services;
using Ekr.Services.Contracts.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Ekr.Services.Contracts.IKD;
using Ekr.Core.Entities.Enrollment;

namespace Ekr.Services.IKD
{
    public class IKDService : IIKDServices
    {
        private readonly IHttpRequestService _httpRequestService;

        public IKDService(IHttpRequestService httpRequestService)
        {
            _httpRequestService = httpRequestService;
        }

        public async Task<ScanResponse> ScanQRIKD(ScanQRIKDReq req, UrlRequestRecognitionFR url)
        {
            var res = new ScanResponse();

            var soapString = "<scanQRRequest>" +
                                "<Header></Header>" +
                                "<Body>" +
                                    "<channel>" + req.channel +"</channel>" +
                                    "<qrCode>" + req.qrCode + "</qrCode>" +
                                "</Body>" +
                            "</scanQRRequest>";
            XDocument xDoc = XDocument.Load(new System.IO.StringReader
                    (await (await PostXmlRequestAsync(url.BaseUrl + url.EndPoint, soapString))
                    .Content.ReadAsStringAsync()));


            if (xDoc.Descendants("data").FirstOrDefault() == null)
            {
                res.err_code = int.Parse( xDoc.Descendants("responseCode").FirstOrDefault().Value);
                res.err_msg = xDoc.Descendants("responseDesc").FirstOrDefault().Value;
                res.obj = null;
            }
            else
            {
                res.err_code = int.Parse(xDoc.Descendants("responseCode").FirstOrDefault().Value);
                res.err_msg = xDoc.Descendants("responseDesc").FirstOrDefault().Value;


                var data = new IKDObject();
                data.nik = xDoc.Descendants("nik").FirstOrDefault().Value;
                data.foto = xDoc.Descendants("foto").FirstOrDefault().Value;
                data.kk = xDoc.Descendants("kk").FirstOrDefault().Value;
                data.nama = xDoc.Descendants("nama").FirstOrDefault().Value;
                data.tempat_lahir = xDoc.Descendants("tempat_lahir").FirstOrDefault().Value;
                data.tanggal_lahir = xDoc.Descendants("tanggal_lahir").FirstOrDefault().Value;
                data.alamat = xDoc.Descendants("alamat").FirstOrDefault().Value;
                data.rt = xDoc.Descendants("rt").FirstOrDefault().Value;
                data.rw = xDoc.Descendants("rw").FirstOrDefault().Value;
                data.kel_Desa = xDoc.Descendants("kel_Desa").FirstOrDefault().Value;
                data.kecamatan = xDoc.Descendants("kecamatan").FirstOrDefault().Value;
                data.kabupaten_kota = xDoc.Descendants("kabupaten_kota").FirstOrDefault().Value;
                data.provinsi = xDoc.Descendants("provinsi").FirstOrDefault().Value;
                data.kode_pos = xDoc.Descendants("kode_pos").FirstOrDefault().Value;
                data.golongan_darah = xDoc.Descendants("golongan_darah").FirstOrDefault().Value;
                data.status_pernikahan = xDoc.Descendants("status_pernikahan").FirstOrDefault().Value;
                data.pekerjaan = xDoc.Descendants("pekerjaan").FirstOrDefault().Value;
                data.agama = xDoc.Descendants("agama").FirstOrDefault().Value;
                data.jenis_kelamin = xDoc.Descendants("jenis_kelamin").FirstOrDefault().Value;
                data.pendidikan = xDoc.Descendants("pendidikan").FirstOrDefault().Value;
                data.status_hubungan_keluarga = xDoc.Descendants("status_hubungan_keluarga").FirstOrDefault().Value;
                data.nama_ibu = xDoc.Descendants("nama_ibu").FirstOrDefault().Value;
                data.nik_ibu = xDoc.Descendants("nik_ibu").FirstOrDefault().Value;
                data.nik_ayah = xDoc.Descendants("nik_ayah").FirstOrDefault().Value;
                data.nama_ayah = xDoc.Descendants("nik_ibu").FirstOrDefault().Value;
                res.obj = data;
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
