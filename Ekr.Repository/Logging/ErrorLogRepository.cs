using Ekr.Core.Entities.Logging;
using Ekr.Dapper.Connection.Contracts.Sql;
using Ekr.Repository.Contracts.Logging;

namespace Ekr.Repository.Logging
{
    public class ErrorLogRepository : BaseRepository, IErrorLogRepository
    {
        public ErrorLogRepository(IEKtpReaderBackendDb2 con) : base(con) { }

        #region CREATE
        public long CreateErrorLog(Tbl_LogError log)
        {
            return InsertIncrement(log);
        }
        #endregion
    }
}
