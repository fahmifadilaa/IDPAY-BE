using Ekr.Core.Entities.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ekr.Core.Entities.DataMaster.DataReader.ViewModel
{
    public class LogDataReaderFilterVM
    {
    }
    public class LogActivityDataReaderFilterVM : BaseSqlGridFilter
    {
        public string UID { get; set; }
        public string Type { get; set; }
        public string NIK { get; set; }
        public string LastIp { get; set; }
    }
    public class LogConnectionDataReaderFilterVM : BaseSqlGridFilter
    {
        public string UID { get; set; }
        public string IP { get; set; }
        public string Status { get; set; }
    }
    public class LogUserDataReaderFilterVM : BaseSqlGridFilter
    {
        public string UID { get; set; }
        public string Nama { get; set; }
    }
}
