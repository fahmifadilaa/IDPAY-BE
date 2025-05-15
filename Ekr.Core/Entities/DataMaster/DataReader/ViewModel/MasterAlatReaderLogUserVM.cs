using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ekr.Core.Entities.DataMaster.DataReader.ViewModel
{
    public class MasterAlatReaderLogUserVM
    {
        public Int64 Number { get; set; }
        public int Id { get; set; }
        public string UID { get; set; }
        public string Nama { get; set; }
        public DateTime? LastActive { get; set; }
    }

    public class MasterAlatReaderLogUserVM2
    {
        public Int64 Number { get; set; }
        public int Id { get; set; }
        public string UID { get; set; }
        public string Npp { get; set; }
        public string Nama { get; set; }
        public string UnitCode { get; set; }
        public string UnitName { get; set; }
        public string LastActive { get; set; }
    }
}
