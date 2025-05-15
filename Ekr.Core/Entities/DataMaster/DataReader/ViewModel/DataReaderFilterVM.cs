using Ekr.Core.Entities.Base;

namespace Ekr.Core.Entities.DataMaster.DataReader.ViewModel
{
    public class DataReaderFilterVM : BaseSqlGridFilter
    {
        public string SerialNumber { get; set; } = null;
        public string UID { get; set; } = null;
        public string UnitIds { get; set; } = null;
    }
}
