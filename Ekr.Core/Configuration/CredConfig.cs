namespace Ekr.Core.Configuration
{
    public class CredConfig
    {
        public string Secret { get; set; }
        public string Issuer { get; set; }
        public string TokenLifetime { get; set; }
    }
}
