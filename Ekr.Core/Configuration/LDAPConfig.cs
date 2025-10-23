using System;

namespace Ekr.Core.Configuration
{
    public class LDAPConfig
    {
        public string Url { get; set; }

        public int Port { get; set; }
        public string LdapHierarchy { get; set; }
        public string IbsRoleLdapHierarchy { get; set; }
        public bool IsActive { get; set; }
        public int MaxPoolSize { get; set; } = 10; // Default ke 10 kalau tidak di-set
        public int ConnTimeOut { get; set; } = 10;  // Default ke 10 Detik kalau tidak di-set -> Time Out untuk Koneksi keitka hit LDAP
        public int PoolTimeOut { get; set; } = 10;  // Default ke 10 Detik kalau tidak di-set ->  Time Out untuk Login Pooling
        public int IdleTimeout { get; set; } = 10;  // Default ke 10 Detik kalau tidak di-set ->  
        public string AdminDn { get; set; }
        //public string AdminPassword { get; set; }
        public string AdminPassword => Environment.GetEnvironmentVariable("LDAP_AdminPassword");

    }

    public class LDAPRespon
    {
        public string ErrMessage { get; set; }
    }

    public class LDAPSummary
    {
        public string Npp { get; set; }
        public LDAPConfig Request { get; set; }
        public LDAPRespon Response { get; set;}
    }
}
