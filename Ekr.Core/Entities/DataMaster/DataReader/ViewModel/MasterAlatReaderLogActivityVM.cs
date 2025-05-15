using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ekr.Core.Entities.DataMaster.DataReader.ViewModel
{
    public class MasterAlatReaderLogActivityVM
    {
        public Int64 Number { get; set; }
        public int Id { get; set; }
        public string UID { get; set; }
        public string Type { get; set; }
        public string NIK { get; set; }
        public string LastIp { get; set; }
        public int? PegawaiId { get; set; }
        public int? UnitId { get; set; }
        public string UnitCode { get; set; }
        public string UnitName { get; set; }
        public string Npp { get; set; }
        public string PegawaiName { get; set; }
        public string CreatedTime { get; set; }
    }
}
