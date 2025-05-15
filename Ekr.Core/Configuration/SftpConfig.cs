namespace Ekr.Core.Configuration
{
    public class SftpConfig
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Url { get; set; }
        public string RootDirectory { get; set; }
        public bool IsActive { get; set; }
    }
}