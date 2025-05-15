using ServiceStack.DataAnnotations;
using System;

namespace Ekr.Core.Entities.DataKTP
{
    public class Tbl_ScanIKD_Session
    {
        [AutoIncrement]
        public int Id { get; set; }
        public int UserId { get; set; }
        public string npp { get; set; }
        public int RoleId { get; set; }
        public int UnitId { get; set; }
        public DateTime? LastActive { get; set; }
        public int Attempt { get; set; }
        public DateTime? LastAttempt { get; set; }
        
    }
}
