using Ekr.Core.Entities.Base;

namespace Ekr.Core.Entities.DataMaster.Lookup
{
    public class LookupFilter : BaseSqlGridFilter
    {
        public string Type { get; set; } = null;
        public string Name { get; set; } = null;
        public int? AppId { get; set; } = null;
    }

    public class LookupByIdVM
    {
        public int Id { get; set; }
    }
}
