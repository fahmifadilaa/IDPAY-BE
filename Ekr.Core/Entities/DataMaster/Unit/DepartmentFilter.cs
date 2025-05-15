using Ekr.Core.Entities.Base;

namespace Ekr.Core.Entities.DataMaster.Unit
{
    public class DepartmentFilter : BaseSqlGridFilter
    {
        public string TypeUnitSearchParam { get; set; } = null;
        public string NameSearchParam { get; set; } = null;
    }
}
