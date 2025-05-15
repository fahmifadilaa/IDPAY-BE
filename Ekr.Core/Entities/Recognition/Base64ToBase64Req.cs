using Newtonsoft.Json;

namespace Ekr.Core.Entities.Recognition
{
    public class Base64ToBase64Req
    {
        [JsonProperty("base64_images1")]
        public string Base64Images1 { get; set; }

        [JsonProperty("base64_images2")]
        public string Base64Images2 { get; set; }
    }

    public class UrlRequestRecognition
    {
        public string BaseUrl { get; set; }
        public string EndPoint { get; set; }
    }
}
