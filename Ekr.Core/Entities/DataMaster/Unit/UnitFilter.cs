using Ekr.Core.Entities.Base;

namespace Ekr.Core.Entities.DataMaster.Unit
{
    public class UnitFilter : BaseSqlGridFilter
    {
        public string TypeUnitSearchParam { get; set; } = null;
        public string KodeUnitSearchParam { get; set; } = null;
        public string NamaUnitSearchParam { get; set; } = null;
    }
}
