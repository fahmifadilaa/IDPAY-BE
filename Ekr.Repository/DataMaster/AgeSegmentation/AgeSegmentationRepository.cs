using Dapper;
using Ekr.Core.Entities;
using Ekr.Core.Entities.DataMaster.AgeSegmentation.Entity;
using Ekr.Core.Entities.DataMaster.AgeSegmentation.ViewModel;
using Ekr.Dapper.Connection.Contracts.Sql;
using Ekr.Repository.Contracts.DataMaster.AgeSegmentation;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ekr.Repository.DataMaster.Segmentation
{
    public class AgeSegmentationRepository : BaseRepository, IAgeSegmentationRepository
    {
        public AgeSegmentationRepository(IEKtpReaderBackendDb con) : base(con) { }

        #region Load Data
        public async Task<GridResponse<AgeSegmentationVM>> LoadData(AgeSegmentationFilterVM req)
        {
            const string sp = "[ProcMasterSegmentasiUsia]";
            var values = new
            {
                req.Nama,
                SColumn = req.SortColumn,
                SColumnValue = req.SortColumnDir,
                Page = req.PageNumber,
                Rows = req.PageSize
            };
            var data = Db.WithConnectionAsync(db => db.QueryAsync<AgeSegmentationVM>(sp, values, commandType: CommandType.StoredProcedure));

            const string spCount = "[ProcMasterSegmentasiUsiaNum]";
            var valuesCount = new
            {
                req.Nama,
            };
            var count = Db.WithConnectionAsync(db => db.ExecuteScalarAsync<int>(spCount, valuesCount, commandType: CommandType.StoredProcedure));

            await Task.WhenAll(data, count).ConfigureAwait(false);

            return new GridResponse<AgeSegmentationVM>
            {
                Count = count.Result,
                Data = data.Result
            };
        }
        #endregion

        #region View
        public async Task<TblMasterSegmentasiUsia> GetAgeSegmentation(AgeSegmentationViewFilterVM req)
        {
            const string query = "Select * FROM Tbl_Master_SegmentasiUsia " +
                "WHERE Id = @Id";

            return await Db.WithConnectionAsync(c => c.QueryFirstOrDefaultAsync<TblMasterSegmentasiUsia>(query, new
            {
                req.Id
            }));
        }
        #endregion

        #region Insert
        public async Task<TblMasterSegmentasiUsia> InsertAgeSegmentation(TblMasterSegmentasiUsia req)
        {
            req.CreatedTime = DateTime.Now;
            const string query = "Insert Into Tbl_Master_SegmentasiUsia (" +
                    "Nama, " +
                    "UsiaAwal, " +
                    "UsiaAkhir, " +
                    "CreatedTime, " +
                    "CreatedBy_Id, " +
                    "IsDeleted, " +
                    "IsActive) " +
                "values(" +
                    "@Nama, " +
                    "@UsiaAwal, " +
                    "@UsiaAkhir, " +
                    "@CreatedTime, " +
                    "@CreatedBy_Id, " +
                    "@IsDeleted, " +
                    "@IsActive) ";

            await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
            {
                req.Nama,
                req.UsiaAwal,
                req.UsiaAkhir,
                req.CreatedTime,
                req.CreatedBy_Id,
                IsActive = true,
                IsDeleted = false
            }));

            return req;
        }
        #endregion

        #region Update
        public async Task<TblMasterSegmentasiUsia> UpdateAgeSegmentation(TblMasterSegmentasiUsia req)
        {
            req.UpdatedTime = DateTime.Now;
            const string query = "Update Tbl_Master_SegmentasiUsia set " +
                        "Nama = @Nama, " +
                        "UsiaAwal = @UsiaAwal, " +
                        "UsiaAkhir = @UsiaAkhir, " +
                        "UpdatedTime = @UpdatedTime, " +
                        "UpdatedBy_Id = @UpdatedBy_Id, " +
                        "IsActive = @IsActive " +
                    "Where Id = @Id";

            await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
            {
                req.Nama,
                req.UsiaAwal,
                req.UsiaAkhir,
                req.UpdatedTime,
                req.UpdatedBy_Id,
                req.IsActive,
                req.Id
            }));

            return req;
        }
        #endregion

        #region Delete
        public async Task DeleteAgeSegmentation(AgeSegmentationViewFilterVM req, int PegawaiId)
        {
            const string query = "Update Tbl_Master_SegmentasiUsia set " +
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
