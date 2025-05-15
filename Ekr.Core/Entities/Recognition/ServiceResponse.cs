using Newtonsoft.Json;

namespace Ekr.Core.Entities.Recognition
{
    public class ServiceResponse<T>
    {
        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("data")]
        public T Data { get; set; }
    }

 
}
