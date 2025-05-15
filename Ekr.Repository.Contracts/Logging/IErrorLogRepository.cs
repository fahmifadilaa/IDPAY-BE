using Ekr.Core.Entities.Logging;

namespace Ekr.Repository.Contracts.Logging
{
    public interface IErrorLogRepository
    {
        long CreateErrorLog(Tbl_LogError log);
    }
}
