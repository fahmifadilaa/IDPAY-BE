using Ekr.Core.Entities.DataMaster.Lookup;
using Ekr.Core.Entities;
using Ekr.Dapper.Connection.Contracts.Sql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ekr.Repository.Contracts.DataMaster.MasterApps;
using Ekr.Core.Entities.DataMaster.MasterApps;
using Dapper;
using System.Data;

namespace Ekr.Repository.DataMaster.MasterApps
{
    public class MasterAppsRepository : BaseRepository, IMasterAppsRepository
    {
        public MasterAppsRepository(IEKtpReaderBackendDb con) : base(con) { }

        #region Load Data
        public async Task<GridResponse<MasterAppsVM>> LoadData(MasterAppsFilter req)
        {
            const string sp = "[ProcMasterApps]";
            var values = new
            {
                req.Name,
                SColumn = req.SortColumn,
                SColumnValue = req.SortColumnDir,
                Page = req.PageNumber,
                Rows = req.PageSize
            };

            var data = Db.WithConnectionAsync(db =>
            db.QueryAsync<MasterAppsVM>(sp, values, commandType: CommandType.StoredProcedure));

            const string spCount = "[ProcMasterAppsNum]";
            var valuesCount = new
            {
                req.Name,
            };

            var count = Db.WithConnectionAsync(db =>
            db.ExecuteScalarAsync<int>(spCount, valuesCount, commandType: CommandType.StoredProcedure));

            await Task.WhenAll(data, count)
                .ConfigureAwait(false);

            return new GridResponse<MasterAppsVM>
            {
                Count = count.Result,
                Data = data.Result
            };
        }
        #endregion

        #region Get
        public async Task<Tbl_Master_Apps> GetById(MasterAppsByIdVM req)
        {
            const string query = "select Id, Nama, Token, IsActive, IsDeleted from [dbo].[Tbl_Master_Apps] where Id = @Id";

            return await Db.WithConnectionAsync(db => db.QueryFirstOrDefaultAsync<Tbl_Master_Apps>(query, new { req.Id }));
        }
        #endregion

        #region Insert
        public async Task<Tbl_Master_Apps> InsertLookup(Tbl_Master_Apps req)
        {
            req.IsActive = true;
            req.IsDeleted = false;
            const string query = "Insert Into Tbl_Master_Apps (" +
                    "Nama, " +
                    "Token, " +
                    "CreatedTime, " +
                    "CreatedByNpp, " +
                    "IsDeleted, " +
                    "IsActive) " +
                "values(" +
                    "@Nama, " +
                    "@Token, " +
                    "@CreatedTime, " +
                    "@CreatedByNpp, " +
                    "@IsDeleted, " +
                    "@IsActive) ";

            await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
            {
                req.Nama,
                req.Token,
                CreatedTime = DateTime.Now,
                req.CreatedByNpp,
                IsDeleted = false,
                IsActive = true,
            }));

            return req;
        }
        #endregion

        #region Update
        public async Task<Tbl_Master_Apps> UpdateLookup(Tbl_Master_Apps req)
        {
            req.UpdatedTime = DateTime.Now;
            const string query = "Update Tbl_Master_Apps set " +
                        "Nama = @Nama, " +
                        "Token = @Token, " +
                        "UpdatedTime = @UpdatedTime, " +
                        "UpdatedByNpp = @UpdatedByNpp, " +
                        "IsActive = @IsActive " +
                    "Where ID = @Id";

            await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
            {
                req.Nama,
                req.Token,
                UpdatedTime = DateTime.Now,
                req.UpdatedByNpp,
                req.IsActive,
                req.Id
            }));

            return req;
        }
        #endregion

        #region Delete
        public async Task DeleteLookup(MasterAppsByIdVM req, int PegawaiId)
        {
            const string query = "Update Tbl_Master_Apps set " +
                        "IsDeleted = @IsDeleted, " +
                        "DeletedTime = @DeletedTime, " +
                        "DeletedByNpp = @DeletedByNpp " +
                    "Where ID = @Id";

            await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
            {
                IsDeleted = true,
                DeletedTime = DateTime.Now,
                DeletedByNpp = PegawaiId,
                req.Id
            }));
        }
        #endregion

        #region create

        #endregion
    }
}
