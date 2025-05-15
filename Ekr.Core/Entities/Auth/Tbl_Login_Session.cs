using System;

namespace Ekr.Core.Entities.Auth
{
    public class Tbl_Login_Session
    {
        public int Id { get; set; }
        public string npp { get; set; }
        public string IpAddress { get; set; }
        public DateTime? LastActive { get; set; }
        public int Attempt { get; set; }
        public DateTime? LastAttempt { get; set; }
    }

    
}
