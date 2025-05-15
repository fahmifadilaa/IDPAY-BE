using System;

namespace Ekr.Core.Entities.DataMaster.Utility.ViewModel
{
    public class LogNikInquiryVM
    {
        public Int64 Number { get; set; }
        public int Id { get; set; }
        public string Npp { get; set; }
        public string NamaPegawai { get; set; }
        public string Url { get; set; }
        public string Nik { get; set; }
        public string SearchParam { get; set; }
        public string Action { get; set; }
        public string CreatedTime { get; set; }
        public string IpAddress { get; set; }
        public string Browser { get; set; }
    }
}
