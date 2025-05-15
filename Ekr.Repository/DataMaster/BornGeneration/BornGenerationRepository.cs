using Dapper;
using Ekr.Core.Entities;
using Ekr.Core.Entities.DataMaster.BornGeneration.Entity;
using Ekr.Core.Entities.DataMaster.BornGeneration.ViewModel;
using Ekr.Dapper.Connection.Contracts.Sql;
using Ekr.Repository.Contracts.DataMaster.BornGeneration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ekr.Repository.DataMaster.BornGeneration
{
    public class BornGenerationRepository : BaseRepository, IBornGenerationRepository
    {
        public BornGenerationRepository(IEKtpReaderBackendDb con) : base(con) { }

        #region Load Data
        public async Task<GridResponse<BornGenerationVM>> LoadData(BornGenerationFilterVM req)
        {
            const string sp = "[ProcMasterGenerasiLahir]";
            var values = new
            {
                req.Nama,
                SColumn = req.SortColumn,
                SColumnValue = req.SortColumnDir,
                Page = req.PageNumber,
                Rows = req.PageSize
            };
            var data = Db.WithConnectionAsync(db => db.QueryAsync<BornGenerationVM>(sp, values, commandType: CommandType.StoredProcedure));

            const string spCount = "[ProcMasterGenerasiLahirNum]";
            var valuesCount = new
            {
                req.Nama,
            };
            var count = Db.WithConnectionAsync(db => db.ExecuteScalarAsync<int>(spCount, valuesCount, commandType: CommandType.StoredProcedure));

            await Task.WhenAll(data, count).ConfigureAwait(false);

            return new GridResponse<BornGenerationVM>
            {
                Count = count.Result,
                Data = data.Result
            };
        }
        #endregion

        #region View
        public async Task<TblMasterGenerasiLahir> GetBornGeneration(BornGenerationViewFilterVM req)
        {
            const string query = "Select * FROM Tbl_Master_GenerasiLahir " +
                "WHERE Id = @Id";

            return await Db.WithConnectionAsync(c => c.QueryFirstOrDefaultAsync<TblMasterGenerasiLahir>(query, new
            {
                req.Id
            }));
        }
        #endregion

        #region Insert
        public async Task<TblMasterGenerasiLahir> InsertBornGeneration(TblMasterGenerasiLahir req)
        {
            req.CreatedTime = DateTime.Now;
            req.IsActive = true;
            req.IsDeleted = false;
            const string query = "Insert Into Tbl_Master_GenerasiLahir (" +
                    "Nama, " +
                    "TahunLahirAwal, " +
                    "TahunLahirAkhir, " +
                    "CreatedTime, " +
                    "CreatedBy_Id, " +
                    "IsDeleted, " +
                    "IsActive) " +
                "values(" +
                    "@Nama, " +
                    "@TahunLahirAwal, " +
                    "@TahunLahirAkhir, " +
                    "@CreatedTime, " +
                    "@CreatedBy_Id, " +
                    "@IsDeleted, " +
                    "@IsActive)";

            await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
            {
                req.Nama,
                req.TahunLahirAwal,
                req.TahunLahirAkhir,
                req.CreatedTime,
                req.CreatedBy_Id,
                IsDeleted = false,
                IsActive = true
            }));

            return req;
        }
        #endregion

        #region Update
        public async Task<TblMasterGenerasiLahir> UpdateBornGeneration(TblMasterGenerasiLahir req)
        {
            req.UpdatedTime = DateTime.Now;
            const string query = "Update Tbl_Master_GenerasiLahir set " +
                        "Nama = @Nama, " +
                        "TahunLahirAwal = @TahunLahirAwal, " +
                        "TahunLahirAkhir = @TahunLahirAkhir, " +
                        "UpdatedTime = @UpdatedTime, " +
                        "UpdatedBy_Id = @UpdatedBy_Id, " +
                        "IsActive = @IsActive " +
                    "Where Id = @Id";

            await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
            {
                req.Nama,
                req.TahunLahirAwal,
                req.TahunLahirAkhir,
                req.UpdatedTime,
                req.UpdatedBy_Id,
                req.IsActive,
                req.Id
            }));

            return req;
        }
        #endregion

        #region Delete
        public async Task DeleteBornGeneration(BornGenerationViewFilterVM req, int PegawaiId)
        {
            const string query = "Update Tbl_Master_GenerasiLahir set " +
                        "IsDeleted = @IsDeleted, " +
                        "DeletedTime = @DeletedTime, " +
                        "DeletedBy_Id = @DeletedBy_Id " +
                    "Where Id = @Id";

            await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
            {
                IsDeleted = true,
                DeletedTime = DateTime.Now,
                DeletedBy_Id = PegawaiId,
                req.Id
            }));
        }
        #endregion

    }
}
