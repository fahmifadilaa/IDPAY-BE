using Dapper;
using Ekr.Core.Entities;
using Ekr.Core.Entities.DataMaster.SystemParameter;
using Ekr.Dapper.Connection.Contracts.Sql;
using Ekr.Repository.Contracts.DataMaster.SystemParameters;
using System.Threading.Tasks;

namespace Ekr.Repository.DataMaster.SystemParameter
{
    public class SysParameterRepository : BaseRepository, ISysParameterRepository
    {
        public SysParameterRepository(IEKtpReaderBackendDb con) : base(con) { }

        public Task<TblSystemParameter> GetPathFolder(string KataKunci)
        {
            const string query = "select * from [dbo].[Tbl_SystemParameter] where KataKunci = @KataKunci";

            return Db.WithConnectionAsync(db => db.QueryFirstOrDefaultAsync<TblSystemParameter>(query, new { KataKunci }));
        }
    }
}
