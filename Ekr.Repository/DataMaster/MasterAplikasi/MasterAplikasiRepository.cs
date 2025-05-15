using Dapper;
using Ekr.Core.Entities.DataMaster.MasterAplikasi;
using Ekr.Dapper.Connection.Contracts.Sql;
using Ekr.Repository.Contracts.DataMaster.MasterAplikasi;
using ServiceStack.OrmLite;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ekr.Repository.DataMaster.MasterAplikasi
{
    public class MasterAplikasiRepository : BaseRepository, IMasterAplikasiRepository
    {
        public MasterAplikasiRepository(IEKtpReaderBackendDb con) : base(con) { }

        #region Get
        public Task<TblMasterAplikasi> GetMasterAplikasiById(int Id)
        {
            const string query = "select * from [dbo].[Tbl_Master_Aplikasi] where id = @Id and isdeleted = false and Isactive = true";

            return Db.WithConnectionAsync(db => db.QueryFirstOrDefaultAsync<TblMasterAplikasi>(query, new { Id }));
        }

        public List<string> GetMasterAplikasiByIds(List<string> Ids)
        {
            return Db.WithConnection(
                c =>
                {
                    var a = c.From<Tbl_Master_Aplikasi>().Where(x => Ids.Contains(x.Id.ToString()) &&
                x.IsActive == true && x.IsDeleted != true).Select(x => x.Nama);

                return c.Column<string>(a);
                }
                  );
        }
        #endregion
    }
}
