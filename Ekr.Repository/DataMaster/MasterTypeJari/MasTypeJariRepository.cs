using Dapper;
using Ekr.Core.Entities.DataMaster.MasterTypeJari;
using Ekr.Dapper.Connection.Contracts.Sql;
using Ekr.Repository.Contracts.DataMaster.MasterTypeJari;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ekr.Repository.DataMaster.MasterTypeJari
{
    public class MasTypeJariRepository : BaseRepository, IMasTypeJariRepository
    {
        public MasTypeJariRepository(IEKtpReaderBackendDb con) : base(con) { }

        public Task<TblMasterTypeJari> GetTypeJari(int? Id)
        {
            const string query = "select * from [dbo].[Tbl_Master_TypeJari] where Id = @Id";

            return Db.WithConnectionAsync(db => db.QueryFirstOrDefaultAsync<TblMasterTypeJari>(query, new { Id }));
        }
    }
}
