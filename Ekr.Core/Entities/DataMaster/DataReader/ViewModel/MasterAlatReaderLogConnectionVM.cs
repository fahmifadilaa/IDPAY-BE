using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ekr.Core.Entities.DataMaster.DataReader.ViewModel
{
    public class MasterAlatReaderLogConnectionVM
    {
        public Int64 Number { get; set; }
        public int Id { get; set; }
        public string UID { get; set; }
        public string IP { get; set; }
        public string Status { get; set; }
        public DateTime? StartTimePing { get; set; }
        public DateTime? EndTimePing { get; set; }
    }
}
