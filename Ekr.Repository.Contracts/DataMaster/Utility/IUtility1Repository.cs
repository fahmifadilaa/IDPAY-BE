using Ekr.Core.Entities.Logging;

namespace Ekr.Repository.Contracts.DataMaster.Utility
{
    public interface IUtility1Repository
    {
        bool InsertLogActivity(Core.Entities.DataMaster.Utility.Entity.TblLogActivity logActivity);
        bool InsertLogEnrollThirdParty(Tbl_Enrollment_ThirdParty_Log logActivity);
    }
}
