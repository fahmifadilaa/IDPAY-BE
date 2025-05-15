using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ekr.Core.Entities.DataMaster.Utility.ViewModel
{
    public class LogActivityVM
    {
        public Int64 Number { get; set; }
        public int Id { get; set; }
        public string Npp { get; set; }
        public string NamaPegawai { get; set; }
        public string Url { get; set; }
        public string ActionTime { get; set; }
        public string Browser { get; set; }
        public string IP { get; set; }
        public string OS { get; set; }
        public string ClientInfo { get; set; }
        public string Keterangan { get; set; }
    }
}
