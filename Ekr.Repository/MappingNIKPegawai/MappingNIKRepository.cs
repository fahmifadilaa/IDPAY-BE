using Ekr.Core.Entities.DataMaster.Lookup;
using Ekr.Core.Entities.MappingNIKPegawai;
using Ekr.Core.Entities;
using Ekr.Dapper.Connection.Contracts.Sql;
using Ekr.Repository.Contracts.MappingNIK;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace Ekr.Repository.MappingNIKPegawai
{
    public class MappingNIKRepository : BaseRepository, IMappingNIKRepository
    {
        public MappingNIKRepository(IEKtpReaderBackendDb con) : base(con)
        {
        }

        public async Task DeleteData(LookupByIdVM req, Tbl_MappingNIK_Pegawai_log log)
        {
            const string query = "DELETE FROM [dbo].[Tbl_MappingNIK_Pegawai] WHERE Id = @Id ";

            const string logquery = "Insert Into [dbo].[Tbl_MappingNIK_Pegawai_Log] (" +
                   "[Npp], " +
                   "[Nama], " +
                   "[NIK], " +
                   "[CreatedDate]," +
                   "[CreateById]," +
                   "[Keterangan]) " +
               "values(" +
                   "@Npp, " +
                   "@Nama, " +
                   "@NIK, " +
                   "@CreatedDate," +
                   "@CreateById," +
                   "@Keterangan) ";

            await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(logquery, log));


            await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
            {
                Id = req.Id
            }));

        }

        public async Task<Tbl_MappingNIK_Pegawai> GetById(LookupByIdVM req)
        {
            const string query = "select Id, Npp, Nama, NIK, InsertedDate from [dbo].[Tbl_MappingNIK_Pegawai] where Id = @Id";

            return await Db.WithConnectionAsync(db => db.QueryFirstOrDefaultAsync<Tbl_MappingNIK_Pegawai>(query, new { Id = req.Id }));
        }

        public async Task<int> GetExistingDataUpdate(requestExist req)
        {
            const string query = "select COUNT(Id) from [dbo].[Tbl_MappingNIK_Pegawai] where (NIK = @NIK OR Npp = @Npp) AND Id NOT IN ( @Id )";

            return await Db.WithConnectionAsync(db => db.QuerySingleAsync<int>(query, new { NIK = req.NIK, Npp = req.Npp, Id = req.Id }));
        }

        public async Task<int> GetExistingDataInsert(requestExist req)
        {
            const string query = "select COUNT(Id) from [dbo].[Tbl_MappingNIK_Pegawai] where (NIK = @NIK OR Npp = @Npp) ";

            return await Db.WithConnectionAsync(db => db.QuerySingleAsync<int>(query, new { NIK = req.NIK, Npp = req.Npp }));
        }

        public async Task<Tbl_MappingNIK_Pegawai> InsertData(Tbl_MappingNIK_Pegawai req)
        {
            req.InsertedDate = DateTime.Now;
            const string query = "Insert Into [dbo].[Tbl_MappingNIK_Pegawai] (" +
                    "[Npp], " +
                    "[Nama], " +
                    "[NIK], " +
                    "[InsertedDate]," +
                    "[InsertById]) " +
                "values(" +
                    "@Npp, " +
                    "@Nama, " +
                    "@NIK, " +
                    "@InsertedDate," +
                    "@InsertById) ";

            await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, req));

            return req;
        }

        public async Task<GridResponse<MappingNIKVM>> LoadData(mappingGrid req)
        {
            const string sp = "[ProcMappingNIKPegawai]";
            var values = new
            {
                NIK = req.filterNIK,
                NPP = req.filterNPP,
                SColumn = req.SortColumn,
                SColumnValue = req.SortColumnDir,
                Page = req.PageNumber,
                Rows = req.PageSize
            };

            var data = Db.WithConnectionAsync(db =>
            db.QueryAsync<MappingNIKVM>(sp, values, commandType: CommandType.StoredProcedure));

            const string spCount = "[ProcMappingNIKPegawaiCount]";
            var valuesCount = new
            {
                NIK = req.filterNIK,
                NPP = req.filterNPP,
            };

            var count = Db.WithConnectionAsync(db =>
            db.ExecuteScalarAsync<int>(spCount, valuesCount, commandType: CommandType.StoredProcedure));

            await Task.WhenAll(data, count)
                .ConfigureAwait(false);

            return new GridResponse<MappingNIKVM>
            {
                Count = count.Result,
                Data = data.Result
            };
        }

        public async Task<Tbl_MappingNIK_Pegawai> UpdateData(Tbl_MappingNIK_Pegawai req, Tbl_MappingNIK_Pegawai_log log)
        {
            const string query = "Update [Tbl_MappingNIK_Pegawai] set " +
                        "[Npp] = @Npp, " +
                        "[Nama] = @Nama, " +
                        "[NIK] = @NIK, " +
                        "[UpdatedDate] = @UpdatedDate, " +
                        "[UpdateById] = @UpdateById " +
                    "Where Id = @Id";

            const string logquery = "Insert Into [dbo].[Tbl_MappingNIK_Pegawai_Log] (" +
                    "[Npp], " +
                    "[Nama], " +
                    "[NIK], " +
                    "[CreatedDate]," +
                    "[CreateById]," +
                    "[Keterangan]) " +
                "values(" +
                    "@Npp, " +
                    "@Nama, " +
                    "@NIK, " +
                    "@CreatedDate," +
                    "@CreateById," +
                    "@Keterangan) ";

            await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(logquery, log));

            await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, req));

            return req;
        }
    }
}
