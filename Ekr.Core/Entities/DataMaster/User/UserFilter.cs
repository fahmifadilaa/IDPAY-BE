using Ekr.Core.Entities.Base;

namespace Ekr.Core.Entities.DataMaster.User
{
    public class UserFilter : BaseSqlGridFilter
    {
        public string NikSearchParam { get; set; } = null;
        public string NamaSearchParam { get; set; } = null;
        public string DivisiSearchParam { get; set; } = null;
        public string BidangSearchParam { get; set; } = null;
    }

    public class PegawaiDemografi : BaseSqlGridFilter
    {
        public string Npp { get; set; } = null;
    }
}
