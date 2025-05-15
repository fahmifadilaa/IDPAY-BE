using Newtonsoft.Json;

namespace Ekr.Core.Entities.Account
{
    public class ServiceResponse<T>
    {
        [JsonProperty("code")]
        public int? Code { get; set; }
        [JsonProperty("msg")]
        public string Msg { get; set; }
        [JsonProperty("data")]
        public T Data { get; set; }
    }
}