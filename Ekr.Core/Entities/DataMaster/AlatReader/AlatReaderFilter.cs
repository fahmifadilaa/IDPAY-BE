using Ekr.Core.Entities.Base;
using System;

namespace Ekr.Core.Entities.DataMaster.AlatReader
{
    public class MasterAlatReaderFilter : BaseSqlGridFilter
    {
        public string SerialNumber { get; set; }
        public string UID { get; set; }
    }
}
