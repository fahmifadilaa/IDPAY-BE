using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ekr.Core.Entities.DataMaster.Utility.ViewModel
{
    public class DataMapsVM
    {
        public int? id { get; set; }
        public string label { get; set; }
        public string lat { get; set; }
        public string lng { get; set; }
        public string file { get; set; }
        public string nik { get; set; }
        public string alamatlengkap { get; set; }
        public string uid { get; set; }
        public string sn_alat { get; set; }
        public string status { get; set; }
    }
}
