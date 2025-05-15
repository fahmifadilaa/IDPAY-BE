namespace Ekr.Core.Entities.Auth
{
    public class AuthReq
    {
    }

    public class AuthAgentReq
    {
        public string Name { get; set; }
        public string ClientId { get; set; }
        public string IpAddress { get; set; }
        public string Token { get; set; }
    }

    public class AuthThirdPartyReq
    {
        public string Name { get; set; }
        public string ClientId { get; set; }
        public string Password { get; set; }
    }

    public class AuthUserReq
    {
        public string Nik { get; set; }
        public string ClientId { get; set; }
        public string IpAddress { get; set; }
        public string Password { get; set; }
        public string LoginType { get; set; }
        public string Finger { get; set; }
    }

    public class AuthAgentRes
    {
        public string JwtToken { get; set; }
        public string RefreshToken { get; set; }
    }

    public class RefreshTokenReq
    {
        public string RefreshTokenn { get; set; }
        public string ClientId { get; set; }
        public string UserCode { get; set; }
        public string JwtToken { get; set; }
        public string IpAddress { get; set; }
    }

    public class LdapInfo
    {
        public string nama { get; set; }
        public string npp { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public string posisi { get; set; }
        public string kode_outlet { get; set; }
        public string nama_outlet { get; set; }
        public string branchalias { get; set; }
        public string nik { get; set; }
        public string LdapUrl { get; set; }
        public string LdapHierarchy { get; set; }
        public string IbsRole { get; set; }
        public string AccountStatus { get; set; }
    }

    public class EncryptNikPassword
    {
        public string stringEncrypt { get; set; }
    }
}
