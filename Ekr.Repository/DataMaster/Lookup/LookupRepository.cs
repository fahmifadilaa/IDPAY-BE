using Dapper;
using Ekr.Core.Entities;
using Ekr.Core.Entities.DataMaster.Lookup;
using Ekr.Dapper.Connection.Contracts.Sql;
using Ekr.Repository.Contracts.DataMaster.Lookup;
using System;
using System.Data;
using System.Threading.Tasks;

namespace Ekr.Repository.DataMaster.Lookup
{
    public class LookupRepository : BaseRepository, ILookupRepository
    {
        public LookupRepository(IEKtpReaderBackendDb con) : base(con) { }

        #region Load Data
        public async Task<GridResponse<LookupVM>> LoadData(LookupFilter req)
        {
            const string sp = "[ProcLookup]";
            var values = new
            {
                req.Type,
                req.Name,
                App_Id = req.AppId,
                SColumn = req.SortColumn,
                SColumnValue = req.SortColumnDir,
                Page = req.PageNumber,
                Rows = req.PageSize
            };

            var data = Db.WithConnectionAsync(db =>
            db.QueryAsync<LookupVM>(sp, values, commandType: CommandType.StoredProcedure));

            const string spCount = "[ProcLookupNum]";
            var valuesCount = new
            {
                req.Type,
                req.Name,
                App_Id = req.AppId
            };

            var count = Db.WithConnectionAsync(db =>
            db.ExecuteScalarAsync<int>(spCount, valuesCount, commandType: CommandType.StoredProcedure));

            await Task.WhenAll(data, count)
                .ConfigureAwait(false);

            return new GridResponse<LookupVM>
            {
                Count = count.Result,
                Data = data.Result
            };
        }
        #endregion

        #region Get
        public async Task<TblLookup> GetById(LookupByIdVM req)
        {
            const string query = "select * from [dbo].[Tbl_Lookup] where Id = @Id";

            return await Db.WithConnectionAsync(db => db.QueryFirstOrDefaultAsync<TblLookup>(query, new { req.Id }));
        }
        public async Task<TblLookup> GetByType(string Type)
        {
            var types = Type.ToUpper();
            const string query = "select * from [dbo].[Tbl_Lookup] where Type = 'TypeRole' and upper(Name) = @types";

            return await Db.WithConnectionAsync(db => db.QueryFirstOrDefaultAsync<TblLookup>(query, new { types }));
        }
        #endregion

        #region Insert
        public async Task<TblLookup> InsertLookup(TblLookup req)
        {
            req.IsActive = true;
            req.IsDeleted = false;
            const string query = "Insert Into Tbl_Lookup (" +
                    "Aplikasi_Id, " +
                    "Type, " +
                    "Name, " +
                    "Value, " +
                    "Order_By, " +
                    "CreatedTime, " +
                    "CreatedBy_Id, " +
                    "IsDeleted, " +
                    "IsActive) " +
                "values(" +
                    "@Aplikasi_Id, " +
                    "@Type, " +
                    "@Name, " +
                    "@Value, " +
                    "@Order_By, " +
                    "@CreatedTime, " +
                    "@CreatedBy_Id, " +
                    "@IsDeleted, " +
                    "@IsActive) ";

            await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
            {
                req.Aplikasi_Id,
                req.Type,
                req.Name,
                req.Value,
                req.Order_By,
                CreatedTime = DateTime.Now,
                req.CreatedBy_Id,
                IsDeleted = false,
                IsActive = true,
            }));

            return req;
        }
        #endregion

        #region Update
        public async Task<TblLookup> UpdateLookup(TblLookup req)
        {
            req.UpdatedTime = DateTime.Now;
            const string query = "Update Tbl_Lookup set " +
                        "Aplikasi_Id = @Aplikasi_Id, " +
                        "Type = @Type, " +
                        "Name = @Name, " +
                        "Value = @Value, " +
                        "Order_By = @Order_By, " +
                        "UpdatedTime = @UpdatedTime, " +
                        "UpdatedBy_Id = @UpdatedBy_Id, " +
                        "IsActive = @IsActive " +
                    "Where ID = @Id";

            await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
            {
                req.Aplikasi_Id,
                req.Type,
                req.Name,
                req.Value,
                req.Order_By,
                req.UpdatedTime,
                req.UpdatedBy_Id,
                req.IsActive,
                req.Id
            }));

            return req;
        }
        #endregion

        #region Delete
        public async Task DeleteLookup(LookupByIdVM req, int PegawaiId)
        {
            const string query = "Update Tbl_Lookup set " +
                        "IsDeleted = @IsDeleted, " +
                        "DeletedTime = @DeletedTime, " +
                        "DeletedBy_Id = @DeletedBy_Id " +
                    "Where ID = @Id";

            await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
            {
                IsDeleted = true,
                DeletedTime = DateTime.Now,
                DeletedBy_Id = PegawaiId,
                req.Id
            }));
        }
        #endregion

        #region create

        #endregion
    }
}
