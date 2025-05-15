using System;

namespace Ekr.Core.Entities.DataMaster.Utility.Entity
{
    public class Tbl_LogNIKInquiry
    {
        public int Id { get; set; }
        public string Npp { get; set; }
        public string Url { get; set; }
        public string Nik { get; set; }
        public string SearchParam { get; set; }
        public string Action { get; set; }
        public string IpAddress { get; set; }
        public string Browser { get; set; }
        public DateTime CreatedTime { get; set; }
    }
}
