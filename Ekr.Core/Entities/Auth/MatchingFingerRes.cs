using Newtonsoft.Json;

namespace Ekr.Core.Entities.Auth
{
    public class MatchingFingerRes
    {
        [JsonProperty("isFoundOrSuccess")]
        public bool IsFoundOrSuccess { get; set; }

        [JsonProperty("score")]
        public int Score { get; set; }
    }
}