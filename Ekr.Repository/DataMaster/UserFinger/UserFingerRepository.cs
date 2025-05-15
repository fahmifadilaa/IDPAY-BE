using Dapper;
using Ekr.Core.Entities.DataMaster.UserFinger;
using Ekr.Dapper.Connection.Contracts.Sql;
using Ekr.Repository.Contracts.DataMaster.UserFinger;
using System.Threading.Tasks;

namespace Ekr.Repository.DataMaster.UserFinger
{
    public class UserFingerRepository : BaseRepository, IUserFingerRepository
    {
        public UserFingerRepository(IEKtpReaderBackendDb con) : base(con) { }

        public Task<TblPegawaiFinger> GetDataFinger(int PegawaiId, int TypeFingerId)
        {
            const string query = "select * from [dbo].[Tbl_Pegawai_Finger] where PegawaiId = @PegawaiId and TypeFingerId = @TypeFingerId";

            return Db.WithConnectionAsync(db => db.QueryFirstOrDefaultAsync<TblPegawaiFinger>(query, new { PegawaiId, TypeFingerId }));
        }

        public async Task<bool> Update(int Id)
        {
            const string query = "Update Tbl_Pegawai_Finger set " +
            "[IsActive] = false," +
            "where Id  = @id ";

            await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new{ Id }));

            return true;
        }

        public async Task<TblPegawaiFinger> Create(TblPegawaiFinger req)
        {
            const string query = "Insert Into Tbl_Pegawai_Finger (" +
            "[PegawaiId]," +
            "[TypeFingerId]," +
            "[FileName]," +
            "[Path]," +
            "[CreatedTime]," +
            "[CreatedBy_Id]," +
            "[IsDeleted]," +
            "[IsActive]," +
        "values(" +
            "@PegawaiId," +
            "@TypeFingerId," +
            "@Path," +
            "@CreatedTime," +
            "@CreatedBy_Id," +
            "@IsDeleted," +
            "@IsActive";

            await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
            {
                req.PegawaiId,
                req.TypeFingerId,
                req.FileName,
                req.Path,
                req.CreatedTime,
                req.CreatedById,
                req.IsDeleted,
                req.IsActive
            }));


            return req;
        }
    }
}
