using Ekr.Core.Entities.Logging;
using Ekr.Dapper.Connection.Contracts.Sql;
using Ekr.Repository.Contracts.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ekr.Repository.Logging
{
    public class ActivityLogRepository : BaseRepository, IActivityLogRepository
    {
        public ActivityLogRepository(IEKtpReaderBackendDb con) : base(con) { }

        #region CREATE
        public long CreateActivityLog(Tbl_LogActivity log)
        {
            return InsertIncrement(log);
        }
        #endregion
    }
}
