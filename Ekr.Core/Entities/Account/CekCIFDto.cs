using Newtonsoft.Json;

namespace Ekr.Core.Entities.Account
{
    public class CekCIFDto
    {
        [JsonProperty("nik")]
        public string Nik { get; set; }
        [JsonProperty("cif")]
        public string Cif { get; set; }
        public string ErrDescription { get; set; }
    }
}