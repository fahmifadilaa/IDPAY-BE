using System.Collections.Generic;

namespace Ekr.Core.Entities.Auth
{
    public class MatchingLoginFinger
    {
        public int? id { get; set; }
        public string text { get; set; }
        public string file { get; set; }
        public string iso { get; set; }
    }
    public class ListMatchingLoginFinger
    {
        public List<MatchingLoginFinger> ListData { get; set; }
    }
}
