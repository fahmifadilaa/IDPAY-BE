using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Ekr.Core.Entities.DataMaster.AlatReader
{
    public class UploadAppsReq
    {
        public decimal Version { get; set; }
        public IFormFile File { get; set; }
        [RegularExpression("^[a-zA-Z0-9 ]*$", ErrorMessage = "Bad Request")]
        public string Keterangan { get; set; }
    }
}
