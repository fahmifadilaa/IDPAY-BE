using Dapper;
using Ekr.Core.Entities;
using Ekr.Core.Entities.DataMaster.Menu.Entity;
using Ekr.Core.Entities.DataMaster.Menu.ViewModel;
using Ekr.Dapper.Connection.Contracts.Sql;
using Ekr.Repository.Contracts.DataMaster.Menu;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ekr.Repository.DataMaster.Menu
{
    public class MenuRepository : BaseRepository, IMenuRepository
    {
        public MenuRepository(IEKtpReaderBackendDb con) : base(con) { }

        #region Load Data
        public async Task<GridResponse<ManageMenuVM>> LoadManageData(ManageMenuFilterVM req)
        {
            const string sp = "[ProcManageMenu]";
            var values = new
            {
                req.Name,
                req.Type,
                SColumn = req.SortColumn,
                SColumnValue = req.SortColumnDir,
                Page = req.PageNumber,
                Rows = req.PageSize
            };
            var data = Db.WithConnectionAsync(db => db.QueryAsync<ManageMenuVM>(sp, values, commandType: CommandType.StoredProcedure));

            const string spCount = "[ProcManageMenuNum]";
            var valuesCount = new
            {
                req.Name,
                req.Type,
            };
            var count = Db.WithConnectionAsync(db => db.ExecuteScalarAsync<int>(spCount, valuesCount, commandType: CommandType.StoredProcedure));

            await Task.WhenAll(data, count).ConfigureAwait(false);

            return new GridResponse<ManageMenuVM>
            {
                Count = count.Result,
                Data = data.Result
            };
        }
        public async Task<GridResponse<MenuVM>> LoadData(MenuFilterVM req)
        {
            const string sp = "[ProcMenu]";
            var values = new
            {
                req.Name,
                req.Type,
                SColumn = req.SortColumn,
                SColumnValue = req.SortColumnDir,
                Page = req.PageNumber,
                Rows = req.PageSize
            };
            var data = Db.WithConnectionAsync(db => db.QueryAsync<MenuVM>(sp, values, commandType: CommandType.StoredProcedure));

            const string spCount = "[ProcMenuNum]";
            var valuesCount = new
            {
                req.Name,
                req.Type,
            };
            var count = Db.WithConnectionAsync(db => db.ExecuteScalarAsync<int>(spCount, valuesCount, commandType: CommandType.StoredProcedure));

            await Task.WhenAll(data, count).ConfigureAwait(false);

            return new GridResponse<MenuVM>
            {
                Count = count.Result,
                Data = data.Result
            };
        }
        #endregion

        #region View
        public async Task<SettingMenuVM> GetSettingMenu(SettingMenuViewFilterVM req)
        {
            const string query = "Select * FROM Navigation " +
                "WHERE Id = @Id";
            var Menu = await Db.WithConnectionAsync(c => c.QueryFirstOrDefaultAsync<SettingMenuVM>(query, new
            {
                req.Id
            })); 

            const string queryNavAssign = "Select [Role_Id] FROM [NavigationAssignment] " +
                 "WHERE [IsActive] = 1 and [Navigation_Id] = @NavId";
            var NavAssign = Db.WithConnectionAsync(c => c.QueryAsync<int>(queryNavAssign, new
            {
                NavId = Menu.Id
            }));

            const string queryNavApps = "Select [Application_Id] FROM [NavigationAplikasi_Mapping] " +
                 "WHERE [IsActive] = 1 and [Navigation_Id] = @NavId";
            var NavApps = Db.WithConnectionAsync(c => c.QueryAsync<int>(queryNavApps, new
            {
                NavId = Menu.Id
            }));

            await Task.WhenAll(NavAssign, NavApps).ConfigureAwait(false);

            if (NavAssign.Result != null)
            {
                Menu.Roles = string.Join(",", NavAssign.Result);
            }

            if (NavApps.Result != null)
            {
                Menu.AppsVal = string.Join(",", NavApps.Result);
            }


            return Menu;
        }
        public async Task<TblMenu> GetMenu(MenuViewFilterVM req)
        {
            const string query = "Select * FROM Navigation " +
                "WHERE Id = @Id";

            return await Db.WithConnectionAsync(c => c.QueryFirstOrDefaultAsync<TblMenu>(query, new
            {
                req.Id
            }));
        }
        #endregion

        #region Insert
        public async Task<SettingMenuReqVM> InsertSettingMenu(SettingMenuReqVM req)
        {
            #region Insert Menu
            req.Visible = 1;
            const string sp = "ProcInsertSettingMenu";
            var values = new
            {
                req.Type,
                req.Name,
                req.Route,
                req.Order,
                req.Visible,
                req.ParentNavigation_Id,
                CreatedTime = DateTime.Now,
                req.CreatedBy_Id,
                req.IconClass
            };
            var NavId = await Db.WithConnectionAsync(db => db.ExecuteScalarAsync<int>(sp, values, commandType: CommandType.StoredProcedure));
            #endregion

            #region insert menu assign
            string[] ArrayRoles = req.Roles.Split(',');
            foreach(var role in ArrayRoles)
            {
                const string queryrole = "Insert Into [NavigationAssignment] (" +
                    "[Navigation_Id], " +
                    "[Role_Id], " +
                    "[CreatedTime], " +
                    "[CreatedBy_Id], " +
                    "[IsActive], " +
                    "[IsDelete]) " +
                "values(" +
                    "@Navigation_Id, " +
                    "@Role_Id, " +
                    "@CreatedTime, " +
                    "@CreatedBy_Id, " +
                    "@IsActive, " +
                    "@IsDelete) ";

                await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(queryrole, new
                {
                    Navigation_Id = NavId,
                    Role_Id = int.Parse(role),
                    CreatedTime = DateTime.Now,
                    CreatedBy_Id = req.CreatedBy_Id,
                    IsActive = true,
                    IsDelete = false
                }));
            }
            #endregion

            #region insert aplikasi mapping
            string[] ArrayApps = req.AppsVal.Split(',');
            foreach (var app in ArrayApps)
            {
                const string queryapp = "Insert Into [NavigationAplikasi_Mapping] (" +
                    "[Application_Id], " +
                    "[Navigation_Id], " +
                    "[CreatedBy], " +
                    "[CreatedDate], " +
                    "[IsActive], " +
                    "[IsDeleted]) " +
                "values(" +
                    "@Application_Id, " +
                    "@Navigation_Id, " +
                    "@CreatedBy, " +
                    "@CreatedDate, " +
                    "@IsActive, " +
                    "@IsDeleted) ";

                await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(queryapp, new
                {
                    Application_Id = int.Parse(app),
                    Navigation_Id = NavId,
                    CreatedBy = req.CreatedBy_Id,
                    CreatedDate = DateTime.Now,
                    IsActive = true,
                    IsDeleted = false
                }));
            }
            #endregion

            return req;
        }
        public async Task<TblMenu> InsertMenu(TblMenu req)
        {
            req.CreatedTime = DateTime.Now;
            req.Visible = 1;
            req.IsDeleted = false;
            const string query = "Insert Into Navigation (" +
                    "[Type], " +
                    "[Name], " +
                    "Route, " +
                    "[Order], " +
                    "Visible, " +
                    "ParentNavigation_Id, " +
                    "CreatedTime, " +
                    "CreatedBy_Id, " +
                    "IconClass, " +
                    "IsDeleted) " +
                "values(" +
                    "@Type, " +
                    "@Name, " +
                    "@Route, " +
                    "@Order, " +
                    "@Visible, " +
                    "@ParentNavigation_Id, " +
                    "@CreatedTime, " +
                    "@CreatedBy_Id, " +
                    "@IconClass, " +
                    "@IsDeleted) ";

            await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
            {
                req.Type,
                req.Name,
                req.Route,
                req.Order,
                req.Visible,
                req.ParentNavigation_Id,
                req.CreatedTime,
                req.CreatedBy_Id,
                req.IconClass,
                req.IsDeleted
            }));

            return req;
        }
        #endregion

        #region Update
        public async Task<SettingMenuReqVM> UpdateSettingMenu(SettingMenuReqVM req)
        {
            #region Update menu
            const string query = "Update Navigation set " +
                        "[Type] = @Type, " +
                        "[Name] = @Name, " +
                        "Route = @Route, " +
                        "[Order] = @Order, " +
                        "Visible = @Visible, " +
                        "ParentNavigation_Id = @ParentNavigation_Id, " +
                        "UpdatedTime = @UpdatedTime, " +
                        "UpdatedBy_Id = @UpdatedBy_Id, " +
                        "IconClass = @IconClass " +
                    "Where Id = @Id";

            await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
            {
                req.Type,
                req.Name,
                req.Route,
                req.Order,
                req.Visible,
                req.ParentNavigation_Id,
                UpdatedTime = DateTime.Now,
                req.UpdatedBy_Id,
                req.IconClass,
                req.Id
            }));
            #endregion

            #region Update Menu Assign
            const string deleteassign = "Delete from NavigationAssignment " +
                    "Where Navigation_Id = @Navigation_Id";
            await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(deleteassign, new
            {
                Navigation_Id = req.Id
            }));

            string[] ArrayRoles = req.Roles.Split(',');
            foreach (var role in ArrayRoles)
            {
                const string queryrole = "Insert Into [NavigationAssignment] (" +
                    "[Navigation_Id], " +
                    "[Role_Id], " +
                    "[CreatedTime], " +
                    "[CreatedBy_Id], " +
                    "[IsActive], " +
                    "[IsDelete]) " +
                "values(" +
                    "@Navigation_Id, " +
                    "@Role_Id, " +
                    "@CreatedTime, " +
                    "@CreatedBy_Id, " +
                    "@IsActive, " +
                    "@IsDelete) ";

                await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(queryrole, new
                {
                    Navigation_Id = req.Id,
                    Role_Id = int.Parse(role),
                    CreatedTime = DateTime.Now,
                    CreatedBy_Id = req.CreatedBy_Id,
                    IsActive = true,
                    IsDelete = false
                }));
            }
            #endregion

            #region Update Apps
            const string deleteapps = "Delete from [NavigationAplikasi_Mapping] " +
                    "Where Navigation_Id = @Navigation_Id";
            await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(deleteapps, new
            {
                Navigation_Id = req.Id
            }));

            string[] ArrayApps = req.AppsVal.Split(',');
            foreach (var app in ArrayApps)
            {
                const string queryapp = "Insert Into [NavigationAplikasi_Mapping] (" +
                    "[Application_Id], " +
                    "[Navigation_Id], " +
                    "[CreatedBy], " +
                    "[CreatedDate], " +
                    "[IsActive], " +
                    "[IsDeleted]) " +
                "values(" +
                    "@Application_Id, " +
                    "@Navigation_Id, " +
                    "@CreatedBy, " +
                    "@CreatedDate, " +
                    "@IsActive, " +
                    "@IsDeleted) ";

                await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(queryapp, new
                {
                    Application_Id = int.Parse(app),
                    Navigation_Id = req.Id,
                    CreatedBy = req.CreatedBy_Id,
                    CreatedDate = DateTime.Now,
                    IsActive = true,
                    IsDeleted = false
                }));
            }
            #endregion

            return req;
        }
        public async Task<TblMenu> UpdateMenu(TblMenu req)
        {
            req.UpdatedTime = DateTime.Now;
            const string query = "Update Navigation set " +
                        "[Type] = @Type, " +
                        "[Name] = @Name, " +
                        "Route = @Route, " +
                        "[Order] = @Order, " +
                        "Visible = @Visible, " +
                        "ParentNavigation_Id = @ParentNavigation_Id, " +
                        "UpdatedTime = @UpdatedTime, " +
                        "UpdatedBy_Id = @UpdatedBy_Id, " +
                        "IconClass = @IconClass " +
                    "Where Id = @Id";

            await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
            {
                req.Type,
                req.Name,
                req.Route,
                req.Order,
                req.Visible,
                req.ParentNavigation_Id,
                req.UpdatedTime,
                req.UpdatedBy_Id,
                req.IconClass,
                req.Id
            }));

            return req;
        }
        #endregion

        #region Delete
        public async Task DeleteSettingMenu(SettingMenuViewFilterVM req, int PegawaiId)
        {
            const string query = "Update Navigation set " +
                        "IsDeleted = @IsDeleted, " +
                        "DeletedTime = @DeletedTime, " +
                        "DeletedById = @DeletedById " +
                    "Where Id = @Id";

            await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
            {
                IsDeleted = true,
                DeletedTime = DateTime.Now,
                DeletedById = PegawaiId,
                req.Id
            }));
        }
        public async Task DeleteMenu(MenuViewFilterVM req, int PegawaiId)
        {
            const string query = "Update Navigation set " +
                        "IsDeleted = @IsDeleted, " +
                        "DeletedTime = @DeletedTime, " +
                        "DeletedById = @DeletedById " +
                    "Where Id = @Id";

            await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
            {
                IsDeleted = true,
                DeletedTime = DateTime.Now,
                DeletedById = PegawaiId,
                req.Id
            }));
        }
        #endregion

    }
}
