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
    public class CLientAppsLogRepository : BaseRepository, IClientAppsLogRepository
    {
        public CLientAppsLogRepository(IEKtpReaderBackendDb con) : base(con) { }

        #region CREATE
        public long CreateClientAppsLog(Tbl_LogClientApps log)
        {
            return InsertIncrement(log);
        }
        #endregion
    }
}
