using Dapper;
using Ekr.Core.Entities;
using Ekr.Core.Entities.DataMaster.Pegawai;
using Ekr.Core.Entities.Enrollment;
using Ekr.Dapper.Connection.Contracts.Sql;
using Ekr.Repository.Contracts.DataMaster.Pegawai;
using System;
using System.Data;
using System.Threading.Tasks;

namespace Ekr.Repository.DataMaster.Pegawai
{
	public class PegawaiRepository : BaseRepository, IPegawaiRepository
	{
		public PegawaiRepository(IEKtpReaderBackendDb con) : base(con) { }

        #region load data
        public async Task<GridResponse<Tbl_MappingNIK_Pegawai>> LoadManageData(PegawaiVM req)
        {
			try
			{
                const string sp = "[ProcNIKPegawaiData]";
                var values = new
                {
                    Nik = req.NIK,
                    Npp = req.Npp,
                    SColumn = req.SortColumn,
                    SColumnValue = req.SortColumnDir,
                    Page = req.PageNumber,
                    Rows = req.PageSize
                };
                var data = Db.WithConnectionAsync(db => db.QueryAsync<Tbl_MappingNIK_Pegawai>(sp, values, commandType: CommandType.StoredProcedure));

                const string spCount = "[ProcNIKPegawaiTotal]";
                var valuesCount = new
                {
                    Nik = req.NIK,
                    Npp = req.Npp,
                };
                var count = Db.WithConnectionAsync(db => db.ExecuteScalarAsync<int>(spCount, valuesCount, commandType: CommandType.StoredProcedure));

                await Task.WhenAll(data, count).ConfigureAwait(false);

                return new GridResponse<Tbl_MappingNIK_Pegawai>
                {
                    Count = count.Result,
                    Data = data.Result
                };
            }
            catch (Exception ex)
			{
                return new GridResponse<Tbl_MappingNIK_Pegawai>
                {
                    Count = 0,
                    Data = null
                };
            }
        }

        public Task<Tbl_MappingNIK_Pegawai> Get(int Id)
        {
            const string query = "select [Id] ,[Npp], [Nama], [NIK] ,[InsertedDate] " +
                " from [dbo].[Tbl_MappingNIK_Pegawai] where Id = @Id";

            return Db.WithConnectionAsync(db => db.QueryFirstOrDefaultAsync<Tbl_MappingNIK_Pegawai>(query, new { Id }));
        }

        public Task<Tbl_MappingNIK_Pegawai> GetByNpp(string Npp)
        {
            const string query = "select [Id] ,[Npp], [Nama], [NIK] ,[InsertedDate] " +
                " from [dbo].[Tbl_MappingNIK_Pegawai] where Npp = @Npp";

            return Db.WithConnectionAsync(db => db.QueryFirstOrDefaultAsync<Tbl_MappingNIK_Pegawai>(query, new { Npp }));
        }
        public Task<Tbl_MappingNIK_Pegawai> GetByNIk(string Nik)
        {
            const string query = "select [Id] ,[Npp], [Nama], [NIK] ,[InsertedDate] " +
                " from [dbo].[Tbl_MappingNIK_Pegawai] where NIK = @Nik";

            return Db.WithConnectionAsync(db => db.QueryFirstOrDefaultAsync<Tbl_MappingNIK_Pegawai>(query, new { Nik }));
        }
        #endregion

        public async Task<Tbl_MappingNIK_Pegawai> Create(Tbl_MappingNIK_Pegawai req)
        {

            const string query = "Insert Into Tbl_MappingNIK_Pegawai (" +
                "[Npp]," +
                "[Nama]," +
                "[NIK]," +
                "[InsertedDate])" +
            "values(" +
                "@Npp," +
                "@Nama," +
                "@NIK," +
                "@InsertedDate)";

            await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
            {
                req.Npp,
                req.Nama,
                req.NIK,
                InsertedDate = DateTime.Now
            }));

            return req;
        }

        public async Task<Tbl_MappingNIK_Pegawai> Update(Tbl_MappingNIK_Pegawai req)
        {
			try
			{
                const string query = "Update Tbl_MappingNIK_Pegawai set " +
                        "Npp = @Npp, " +
                        "Nama = @Nama, " +
                        "NIK = @NIK " +
                    "Where Id = @Id";

                await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
                {
                    req.Npp,
                    req.Nama,
                    req.NIK,
                    req.Id
                }));

                return req;
            }
			catch (Exception)
			{

                return null;
			}
        }

        public async Task<int> Delete(int Id)
        {
            const string query = "delete from Tbl_MappingNIK_Pegawai " +
                    "Where Id = @Id";

            await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
            {
                Id
            }));

            return Id;
        }
    }
}
