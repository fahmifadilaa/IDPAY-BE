using Dapper;
using Ekr.Core.Entities;
using Ekr.Core.Entities.DataMaster.Unit;
using Ekr.Dapper.Connection.Contracts.Sql;
using Ekr.Repository.Contracts.DataMaster.Unit;
using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Ekr.Repository.DataMaster.Unit
{
    public class UnitRepository : BaseRepository, IUnitRepository
    {
        public UnitRepository(IEKtpReaderBackendDb con) : base(con) { }

        #region GET
        public async Task<GridResponse<UnitVM>> GridGetAll(UnitFilter req)
        {
            const string sp = "[ProcManageUnitData]";
            var values = new
            {
                TypeUnit = req.TypeUnitSearchParam,
                KodeUnit = req.KodeUnitSearchParam,
                NamaUnit = req.NamaUnitSearchParam,
                SColumn = req.SortColumn,
                SColumnValue = req.SortColumnDir,
                Page = req.PageNumber,
                Rows = req.PageSize
            };

            var data = Db.WithConnectionAsync(db =>
            db.QueryAsync<UnitVM>(sp, values, commandType: CommandType.StoredProcedure));

            const string spCount = "[ProcManageUnitTotal]";
            var valuesCount = new
            {
                TypeUnit = req.TypeUnitSearchParam,
                KodeUnit = req.KodeUnitSearchParam,
                NamaUnit = req.NamaUnitSearchParam
            };

            var count = Db.WithConnectionAsync(db =>
            db.ExecuteScalarAsync<int>(spCount, valuesCount, commandType: CommandType.StoredProcedure));

            await Task.WhenAll(data, count)
                .ConfigureAwait(false);

            return new GridResponse<UnitVM>
            {
                Count = count.Result,
                Data = data.Result
            };
        }

        public async Task<GridResponse<DepartmentVM>> GridGetAllDepartment(DepartmentFilter req)
        {
            const string sp = "[ProcMasterDepartementData]";
            var values = new
            {
                Type = req.TypeUnitSearchParam,
                Name = req.NameSearchParam,
                SColumn = req.SortColumn,
                SColumnValue = req.SortColumnDir,
                Page = req.PageNumber,
                Rows = req.PageSize
            };

            var data = Db.WithConnectionAsync(db =>
            db.QueryAsync<DepartmentVM>(sp, values, commandType: CommandType.StoredProcedure));

            const string spCount = "[ProcMasterDepartementTotal]";
            var valuesCount = new
            {
                Type = req.TypeUnitSearchParam,
                Name = req.NameSearchParam
            };

            var count = Db.WithConnectionAsync(db =>
            db.ExecuteScalarAsync<int>(spCount, valuesCount, commandType: CommandType.StoredProcedure));

            await Task.WhenAll(data, count)
                .ConfigureAwait(false);

            return new GridResponse<DepartmentVM>
            {
                Count = count.Result,
                Data = data.Result
            };
        }

        public Task<TblUnitVM> Get(int Id)
        {
            const string query = "select [Id], [Parent_Id] as ParentId, [Type], [KodeWilayah], " +
                "[Code],[FullCode],[ShortName],[Name],[Address],[Email],[Telepon],[StatusOutlet],[Latitude]," +
                "[Longitude],[IsActive],[CreatedTime],[UpdatedTime],[CreatedBy_Id] as CreatedById,[UpdatedBy_Id] as UpdatedById,[DeletedTime],[DeletedBy_Id] as DeletedById" +
                ",[IsDelete] from[dbo].[Tbl_Unit] where Id = @Id";

            return Db.WithConnectionAsync(db => db.QueryFirstOrDefaultAsync<TblUnitVM>(query, new { Id }));
        }

        public Task<TblUnitVM> GetByKodeOutlet(string KodeOutlet)
        {
            const string query = "select [Id], [Parent_Id] as ParentId, [Type], [KodeWilayah], " +
                "[Code],[FullCode],[ShortName],[Name],[Address],[Email],[Telepon],[StatusOutlet],[Latitude]," +
                "[Longitude],[IsActive],[CreatedTime],[UpdatedTime],[CreatedBy_Id] as CreatedById,[UpdatedBy_Id] as UpdatedById,[DeletedTime],[DeletedBy_Id] as DeletedById" +
                ",[IsDelete] from[dbo].[Tbl_Unit] where FullCode = @KodeOutlet";

            return Db.WithConnectionAsync(db => db.QueryFirstOrDefaultAsync<TblUnitVM>(query, new { KodeOutlet }));
        }
        #endregion

        #region create
        public async Task<TblUnitVM> Create(TblUnitVM req)
        {
            const string query = "Insert Into Tbl_Unit (" +
                "[Parent_Id]," +
                "[Type]," +
                "[Code]," +
                "[FullCode]," +
                "[ShortName]," +
                "[Name]," +
                "[Address]," +
                "[Email]," +
                "[Telepon]," +
                "[Latitude]," +
                "[Longitude]," +
                "[IsActive]," +
                "[CreatedTime]," +
                "[CreatedBy_Id])" +
            "values(" +
                "@ParentId," +
                "@Type," +
                "@Code," +
                "@FullCode," +
                "@ShortName," +
                "@Name," +
                "@Address," +
                "@Email," +
                "@Telepon," +
                "@Latitude," +
                "@Longitude," +
                "@IsActive," +
                "@CreatedTime," +
                "@CreatedById)";

            await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
            {
                req.ParentId,
                req.Type,
                req.Code,
                req.FullCode,
                req.ShortName,
                req.Name,
                req.Address,
                req.Email,
                req.Telepon,
                req.Latitude,
                req.Longitude,
                req.IsActive,
                req.CreatedTime,
                req.CreatedById
            }));

            return req;
        }
        #endregion
        #region Update
        public async Task<TblUnitVM> Update(TblUnitVM req)
        {
            req.UpdatedTime = DateTime.Now;
            const string query = "Update Tbl_Unit set " +
                        "Parent_Id = @ParentId, " +
                        "Type = @Type, " +
                        "Code = @Code, " +
                        "FullCode = @FullCode, " +
                        "ShortName = @ShortName, " +
                        "Name = @Name, " +
                        "Address = @Address, " +
                        "Email = @Email, " +
                        "Telepon = @Telepon, " +
                        "Latitude = @Latitude, " +
                        "Longitude = @Longitude, " +
                        "IsActive = @IsActive, " +
                        "UpdatedTime = @UpdatedTime, " +
                        "UpdatedBy_Id = @UpdatedById " +
                    "Where Id = @Id";

            await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
            {
                req.ParentId,
                req.Type,
                req.Code,
                req.FullCode,
                req.ShortName,
                req.Name,
                req.Address,
                req.Email,
                req.Telepon,
                req.Latitude,
                req.Longitude,
                req.IsActive,
                req.UpdatedTime,
                req.UpdatedById,
                req.Id
            }));

            return req;
        }
        #endregion

        #region Delete
        public async Task<bool> Delete(string ids, int PegawaiId)
        {
            int[] confirmedDeleteId = ids.Split(',').Select(int.Parse).ToArray();
           
            foreach (var item in confirmedDeleteId)
            {
                var values = new
                {
                    DeletedTime = DateTime.Now,
                    IsDelete = true,
                    Id = item
                };
                const string query = "Update Tbl_Unit set " +
                           "DeletedTime = @DeletedTime, " +
                           "IsDelete = @IsDelete " +
                       "Where Id = @Id";

                await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, values));
            }

            return true;
        }
        #endregion
    }
}
