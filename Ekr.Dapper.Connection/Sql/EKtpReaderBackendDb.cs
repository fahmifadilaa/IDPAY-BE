using Ekr.Core.Configuration;
using Ekr.Dapper.Connection.Base;
using Ekr.Dapper.Connection.Contracts.Sql;
using Microsoft.Extensions.Options;

namespace Ekr.Dapper.Connection.Sql
{
    public class EKtpReaderBackendDb : SqlServerConnection, IEKtpReaderBackendDb
    {
        public EKtpReaderBackendDb(IOptions<ConnectionStringConfig> options, IOptions<ErrorMessageConfig> options2) : base(options.Value.dbConnection, options2)
        {
        }
    }
    public class EKtpReaderBackendDb2 : SqlServerConnection, IEKtpReaderBackendDb2
    {
        public EKtpReaderBackendDb2(IOptions<ConnectionStringConfig> options, IOptions<ErrorMessageConfig> options2) : base(options.Value.dbConnection1, options2)
        {
        }
    }
}
