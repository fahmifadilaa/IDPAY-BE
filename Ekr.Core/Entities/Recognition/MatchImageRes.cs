using Newtonsoft.Json;

namespace Ekr.Core.Entities.Recognition
{
    public class MatchImageRes
    {
        [JsonProperty("isFoundOrSuccess")]
        public bool IsFoundOrSuccess { get; set; }

        [JsonProperty("score")]
        public int Score { get; set; }

        [JsonProperty("userId")]
        public string UserId { get; set; }

        [JsonProperty("fingerprintID")]
        public string FingerprintID { get; set; }

        [JsonProperty("fingerprintName")]
        public string FingerprintName { get; set; }
    }
}
