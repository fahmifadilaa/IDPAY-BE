using Dapper;
using Ekr.Core.Configuration;
using Ekr.Core.Entities;
using Ekr.Core.Entities.DataMaster.Lookup;
using Ekr.Core.Entities.DataMaster.MasterAplikasi;
using Ekr.Core.Entities.DataMaster.Menu.Entity;
using Ekr.Core.Entities.DataMaster.Menu.ViewModel;
using Ekr.Core.Entities.DataMaster.User;
using Ekr.Core.Entities.DataMaster.Utility;
using Ekr.Core.Entities.DataMaster.Utility.ViewModel;
using Ekr.Dapper.Connection.Base;
using Ekr.Dapper.Connection.Contracts.Base;
using Ekr.Dapper.Connection.Contracts.Sql;
using Ekr.Repository.Contracts.DataMaster.Utility;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ekr.Repository.DataMaster.Utility
{
    public class UtilityRepository : BaseRepository, IUtilityRepository
    {
        private readonly IBaseConnection _baseConnection;

        public UtilityRepository(IEKtpReaderBackendDb con,
           Microsoft.Extensions.Options.IOptions<ConnectionStringConfig> options, Microsoft.Extensions.Options.IOptions<ErrorMessageConfig> options2
            ) : base(con)
        {
            _baseConnection = new SqlServerConnection(options.Value.dbConnection1, options2);
        }

        public async Task<List<GetMenuVM>> GetMenu(GetMenuFilterVM req)
        {
            const string sp = "[ProcGetMenu]";
            var values = new
            {
                req.RoleId,
                req.AppId
            };
            var data = await Db.WithConnectionAsync(db => db.QueryAsync<GetMenuVM>(sp, values, commandType: CommandType.StoredProcedure)) ?? new List<GetMenuVM>();

            return data.ToList();
        }

        public async Task<TblMenu> GetMenuById(GetMenuByIdFilterVM req)
        {
            const string query = "Select * FROM [Navigation] " +
                "WHERE [Id] = @Id";

            var data = await Db.WithConnectionAsync(c => c.QueryFirstOrDefaultAsync<TblMenu>(query, new
            {
                req.Id
            })) ?? new TblMenu();

            return data;
        }
        public async Task<List<TblLookup>> SelectLookup(LookupFilterVM req)
        {
            const string query = "Select * FROM Tbl_Lookup " +
                "WHERE IsActive=1 and IsDeleted=0 and Type = @Type";

            var data = await Db.WithConnectionAsync(c => c.QueryAsync<TblLookup>(query, new
            {
                req.Type
            })) ?? new List<TblLookup>();

            return data.ToList();
        }

        public async Task<List<TblLookup>> SelectLookupTypeByUnitId(string RoleId, string UnitId)
        {
            const string sp = "[ProcGetTypeUnitByUnitId]";
            var values = new
            {
                RoleId = RoleId,
                UnitId = UnitId
            };
            var data = await Db.WithConnectionAsync(db => db.QueryAsync<TblLookup>(sp, values, commandType: CommandType.StoredProcedure)) ?? new List<TblLookup>();


            return data.ToList();
        }
        public async Task<SystemParameterVM> SelectSystemParameter(SystemParameterFilterVM req)
        {
            const string query = "Select * FROM Tbl_SystemParameter " +
                "WHERE IsActive=1 and IsDelete=0 and KataKunci = @KataKunci";

            var data = await Db.WithConnectionAsync(c => c.QueryFirstOrDefaultAsync<SystemParameterVM>(query, new
            {
                req.KataKunci
            })) ?? new SystemParameterVM();

            return data;
        }
        public async Task<List<EligibleAppsVM>> GetEligibleApps(EligibleAppsFilterVM req)
        {
            const string sp = "[ProcGetEligibleApps]";
            var values = new
            {
                Pegawai_Id = req.PegawaiId
            };
            var data = await Db.WithConnectionAsync(db => db.QueryAsync<EligibleAppsVM>(sp, values, commandType: CommandType.StoredProcedure)) ?? new List<EligibleAppsVM>();

            return data.ToList();
        }
        public async Task<GridResponse<LogActivityVM>> GetLogActivity(LogActivityFilterVM req)
        {
            const string sp = "[ProcLogActivity]";
            var values = new
            {
                req.Nama,
                req.Nik,
                SColumn = req.SortColumn,
                SColumnValue = req.SortColumnDir,
                Page = req.PageNumber,
                Rows = req.PageSize
            };
            var data = await Db.WithConnectionAsync(db => db.QueryAsync<LogActivityVM>(sp, values, commandType: CommandType.StoredProcedure));

            const string spCount = "[ProcLogActivityNum]";
            var valuesCount = new
            {
                req.Nama,
                req.Nik,
            };
            var count = await Db.WithConnectionAsync(db => db.ExecuteScalarAsync<int>(spCount, valuesCount, commandType: CommandType.StoredProcedure));

            return new GridResponse<LogActivityVM>
            {
                Count = count,
                Data = data
            };
        }

        public async Task<GridResponse<LogNikInquiryVM>> GetLogNikInquiry(LogNikInquiryFilterVM req)
        {
            const string sp = "[ProcLogNIKInquiry]";
            var values = new
            {
                req.Npp,
                req.Nik,
                SColumn = req.SortColumn,
                SColumnValue = req.SortColumnDir,
                Page = req.PageNumber,
                Rows = req.PageSize
            };
            var data = await Db.WithConnectionAsync(db => db.QueryAsync<LogNikInquiryVM>(sp, values, commandType: CommandType.StoredProcedure));

            const string spCount = "[ProcLogNIKInquiryNum]";
            var valuesCount = new
            {
                req.Npp,
                req.Nik
            };
            var count = await Db.WithConnectionAsync(db => db.ExecuteScalarAsync<int>(spCount, valuesCount, commandType: CommandType.StoredProcedure));

            return new GridResponse<LogNikInquiryVM>
            {
                Count = count,
                Data = data
            };
        }
        public async Task<GridResponse<DataDropdownServerSide>> DropdownMenu(DropdownMenuFilterVM req)
        {
            const string sp = "[ProcDropdownMenu]";
            var values = new
            {
                req.Parameter,
                Page = req.PageNumber,
                Rows = req.PageSize
            };
            var data = await Db.WithConnectionAsync(db => db.QueryAsync<DataDropdownServerSide>(sp, values, commandType: CommandType.StoredProcedure));

            const string spCount = "[ProcDropdownMenuNUm]";
            var valuesCount = new
            {
                req.Parameter,
            };
            var count = await Db.WithConnectionAsync(db => db.ExecuteScalarAsync<int>(spCount, valuesCount, commandType: CommandType.StoredProcedure));

            return new GridResponse<DataDropdownServerSide>
            {
                Count = count,
                Data = data
            };
        }

        public async Task<GridResponse<DataDropdownServerSide>> DropdownPegawaiHirarki(DropdownMenuFilterVM req, string UnitId, string RoleId)
        {
            const string sp = "[ProcPegawaiHirarki]";
            var values = new
            {
                Param = req.Parameter,
                SColumn = "Id",
                SColumnValue = "desc",
                Page = req.PageNumber,
                Rows = req.PageSize,
                UnitId,
                RoleId
            };
            var data = await Db.WithConnectionAsync(db => db.QueryAsync<DataDropdownServerSide>(sp, values, commandType: CommandType.StoredProcedure));

            const string spCount = "[ProcPegawaiHirarkiNum]";
            var valuesCount = new
            {
                Param = req.Parameter,
                UnitId,
                RoleId
            };
            var count = await Db.WithConnectionAsync(db => db.ExecuteScalarAsync<int>(spCount, valuesCount, commandType: CommandType.StoredProcedure));

            return new GridResponse<DataDropdownServerSide>
            {
                Count = count,
                Data = data
            };
        }

        public async Task<bool> IsGetAccess(CheckAccessMenuFilterVM req)
        {
            const string sp = "[ProcCheckAccessMenu]";
            var values = new
            {
                Role_Id = req.RoleId,
                req.Url
            };
            var data = await Db.WithConnectionAsync(db => db.ExecuteScalarAsync<bool>(sp, values, commandType: CommandType.StoredProcedure));

            return data;
        }
        public async Task<List<DataDropdownServerSide>> DropdownAplikasi()
        {
            const string query = "Select " +
                "Id id, " +
                "Nama nama_text, " +
                "Nama text " +
                " FROM Tbl_Master_Aplikasi " +
                "WHERE IsActive=1 and IsDeleted=0";

            var data = await Db.WithConnectionAsync(c => c.QueryAsync<DataDropdownServerSide>(query)) ?? new List<DataDropdownServerSide>();

            return data.ToList();
        }

        public async Task<GridResponse<DataDropdownServerSide>> GetUnit(Utility2VM req)
        {
            const string sp = "[ProcDropdownUnit]";
            var values = new
            {
                req.Parameter,
                Page = req.PageNumber,
                Rows = req.PageSize
            };

            var data = await Db.WithConnectionAsync(db =>
            db.QueryAsync<DataDropdownServerSide>(sp, values, commandType: CommandType.StoredProcedure));

            const string spCount = "[ProcDropdownUnitNum]";
            var valuesCount = new
            {
                req.Parameter
            };

            var count = await Db.WithConnectionAsync(db =>
            db.ExecuteScalarAsync<int>(spCount, valuesCount, commandType: CommandType.StoredProcedure));

            return new GridResponse<DataDropdownServerSide>
            {
                Count = count,
                Data = data
            };
        }

        public async Task<GridResponse<DataDropdownServerSide>> GetUnitByTypeAndUnitId(GetDataUnitByTypeAndUnitIdViewModel req)
        {
            const string sp = "[ProcDropdownUnitByTypeAndUnitId]";
            var values = new
            {
                req.Parameter,
                TypeId = req.TypeId,
                UnitId = req.UnitId,
                RoleId = req.RoleId,
                Page = req.Page,
                Rows = req.Rows
            };

            var data = await Db.WithConnectionAsync(db =>
            db.QueryAsync<DataDropdownServerSide>(sp, values, commandType: CommandType.StoredProcedure));

            const string spCount = "[ProcDropdownUnitByTypeAndUnitIdTotal]";
            var valuesCount = new
            {
                req.Parameter,
                TypeId = req.TypeId,
                UnitId = req.UnitId,
                RoleId = req.RoleId
            };

            var count = await Db.WithConnectionAsync(db =>
            db.ExecuteScalarAsync<int>(spCount, valuesCount, commandType: CommandType.StoredProcedure));

            return new GridResponse<DataDropdownServerSide>
            {
                Count = count,
                Data = data
            };
        }

        public async Task<GridResponse<DataDropdownServerSide>> GetUnitById(string id)
        {
            const string sp = "[ProcDropdownUnitById]";
            var values = new
            {
                Id = id
            };

            var data = await Db.WithConnectionAsync(db =>
            db.QueryAsync<DataDropdownServerSide>(sp, values, commandType: CommandType.StoredProcedure));

            int Count = data.Count();

            return new GridResponse<DataDropdownServerSide>
            {
                Count = Count,
                Data = data
            };
        }

        public async Task<GridResponse<DataDropdownServerSide>> GetAllDataMasterTypeFinger(UtilityVM req)
        {
            const string sp = "[ProcDropdownMasterTypeJari]";
            var values = new
            {
                Parameter = req.q,
                Page = req.page,
                Rows = req.rowPerPage
            };

            var data = await Db.WithConnectionAsync(db =>
            db.QueryAsync<DataDropdownServerSide>(sp, values, commandType: CommandType.StoredProcedure));

            const string spCount = "[ProcDropdownMasterTypeJariTotal]";
            var valuesCount = new
            {
                Parameter = req.q
            };

            var count = await Db.WithConnectionAsync(db =>
            db.ExecuteScalarAsync<int>(spCount, valuesCount, commandType: CommandType.StoredProcedure));

            return new GridResponse<DataDropdownServerSide>
            {
                Count = count,
                Data = data
            };
        }

        public async Task<GridResponse<DataDropdownServerSide>> GetAllRolePegawai(int PegawaiId, int appId)
        {
            string Date = DateTime.Now.ToString("yyyy-MM-dd");
            const string sp = "[ProcLoginGetDataRolePegawai]";
            var values = new
            {
                Pegawai_id = PegawaiId,
                Date = Date,
                App_Id = appId
            };

            var data = await Db.WithConnectionAsync(db =>
            db.QueryAsync<DataDropdownServerSide>(sp, values, commandType: CommandType.StoredProcedure));

            int Count = data.Count();

            return new GridResponse<DataDropdownServerSide>
            {
                Count = Count,
                Data = data
            };
        }

        public async Task<GridResponse<DataDropdownServerSide>> GetDropdownRolesByMenuId(int Roles)
        {
            const string sp = "[ProcDropdownMenuGetDataRolesById]";
            var values = new
            {
                Id = Roles
            };

            var data = await Db.WithConnectionAsync(db =>
            db.QueryAsync<DataDropdownServerSide>(sp, values, commandType: CommandType.StoredProcedure));

            int Count = data.Count();

            return new GridResponse<DataDropdownServerSide>
            {
                Count = Count,
                Data = data
            };
        }

        public async Task<GridResponse<DataDropdownServerSide>> GetDropdownAppsById(string apps)
        {
            const string sp = "[ProcDropdownMenuGetDataAppsById]";
            var values = new
            {
                Id = apps
            };

            var data = await Db.WithConnectionAsync(db =>
            db.QueryAsync<DataDropdownServerSide>(sp, values, commandType: CommandType.StoredProcedure));

            int Count = data.Count();

            return new GridResponse<DataDropdownServerSide>
            {
                Count = Count,
                Data = data
            };
        }

        public async Task<GridResponse<Jumlah_Inbox>> GetCountDataEnroll(string uid, string PegawaiId, string unitId)
        {
            const string sp = "[ProcUtilityJumlahEnroll]";
            var values = new
            {
                UID = uid,
                UnitId = unitId,
                PegawaiId = PegawaiId
            };

            var data = await Db.WithConnectionAsync(db =>
            db.QueryAsync<Jumlah_Inbox>(sp, values, commandType: CommandType.StoredProcedure));

            int Count = data.Count();

            return new GridResponse<Jumlah_Inbox>
            {
                Count = Count,
                Data = data
            };
        }

        public async Task<GridResponse<Jumlah_Inbox>> GetCountDataEnrollByUnitId(string UnitId)
        {
            const string sp = "[ProcUtilityJumlahEnroll]";
            var values = new
            {
                UnitId = UnitId
            };

            var data = await Db.WithConnectionAsync(db =>
            db.QueryAsync<Jumlah_Inbox>(sp, values, commandType: CommandType.StoredProcedure));

            int Count = data.Count();

            return new GridResponse<Jumlah_Inbox>
            {
                Count = Count,
                Data = data
            };
        }

        public async Task<GridResponse<Jumlah_Inbox>> GetCountDataEnrollByUnitIdJenis(string UnitId, string Jenis)
        {
            const string sp = "[ProcUtilityJumlahEnrollNew]";
            var values = new
            {
                UnitId = UnitId,
                Jenis = Jenis
            };

            var data = await Db.WithConnectionAsync(db =>
            db.QueryAsync<Jumlah_Inbox>(sp, values, commandType: CommandType.StoredProcedure));

            int Count = data.Count();

            return new GridResponse<Jumlah_Inbox>
            {
                Count = Count,
                Data = data
            };
        }

        public async Task<GridResponse<Jumlah_Inbox>> GetCountDataEnrollTemp(DataEnrollFilter req)
        {
            const string sp = "[ProcUtilityJumlahEnrollTemp]";
            var values = new
            {
                UID = req.uid,
                Nama = req.nama,
                Nik = req.nik,
                UnitId = req.UnitId,
                PegawaiId = req.PegawaiId
            };

            var data = await Db.WithConnectionAsync(db =>
            db.QueryAsync<Jumlah_Inbox>(sp, values, commandType: CommandType.StoredProcedure));

            int Count = data.Count();

            return new GridResponse<Jumlah_Inbox>
            {
                Count = Count,
                Data = data
            };
        }

        public async Task<GridResponse<DataMaps_ViewModels>> GetDataMapsEnroll(string uid)
        {
            const string sp = "[ProcUtilityMapsGetAllDataEnroll]";
            var values = new
            {
                UID = uid
            };

            var data = await Db.WithConnectionAsync(db =>
            db.QueryAsync<DataMaps_ViewModels>(sp, values, commandType: CommandType.StoredProcedure));

            int Count = data.Count();

            return new GridResponse<DataMaps_ViewModels>
            {
                Count = Count,
                Data = data
            };
        }

        public async Task<GridResponse<CekUsia_ViewModels>> CekUsia(string TglLahir)
        {
            const string sp = "[ProcUtilityCekUsia]";
            var values = new
            {
                TglLahir = TglLahir
            };

            var data = await Db.WithConnectionAsync(db =>
            db.QueryAsync<CekUsia_ViewModels>(sp, values, commandType: CommandType.StoredProcedure));

            int Count = data.Count();

            return new GridResponse<CekUsia_ViewModels>
            {
                Count = Count,
                Data = data
            };
        }

        public async Task<GridResponse<DataDropdownServerSide>> SelectDataTypeFingerLogin(string username)
        {
            const string sp = "[ProcDropdownFingerUser]";
            var values = new
            {
                Username = username
            };

            var data = await Db.WithConnectionAsync(db =>
            db.QueryAsync<DataDropdownServerSide>(sp, values, commandType: CommandType.StoredProcedure));

            int Count = data.Count();

            return new GridResponse<DataDropdownServerSide>
            {
                Count = Count,
                Data = data
            };
        }

        public async Task<GridResponse<DataDropdownServerSide>> GetRole(Utility2VM req)
        {
            const string sp = "[ProcDropdownRolePegawai]";
            var values = new
            {
                req.Parameter,
                Page = req.PageNumber,
                Rows = req.PageSize
            };

            var data = Db.WithConnectionAsync(db =>
            db.QueryAsync<DataDropdownServerSide>(sp, values, commandType: CommandType.StoredProcedure));

            const string spCount = "[ProcDropdownRolePegawaiNum]";
            var valuesCount = new
            {
                req.Parameter
            };

            var count = Db.WithConnectionAsync(db =>
            db.ExecuteScalarAsync<int>(spCount, valuesCount, commandType: CommandType.StoredProcedure));

            await Task.WhenAll(data, count)
                .ConfigureAwait(false);

            return new GridResponse<DataDropdownServerSide>
            {
                Count = count.Result,
                Data = data.Result
            };
        }

        public bool UploadAppsVersion(Tbl_VersionAgent versionApps)
        {
            return InsertIncrement(versionApps) > 0;
        }
        public Tbl_VersionAgent GetLatestAppsVersion()
        {
            const string query = "Select top(1) * FROM Tbl_VersionAgent where isdeleted = 0 order by Version desc";

            var data = Db.WithConnection(c => c.QueryFirstOrDefault<Tbl_VersionAgent>(query)) ?? new Tbl_VersionAgent();

            return data;
        }
        public Tbl_VersionAgent GetAppsVersionById(int Id)
        {
            const string query = "Select * FROM Tbl_VersionAgent where [Id] = @Id";

            var data = Db.WithConnection(c => c.QueryFirstOrDefault<Tbl_VersionAgent>(query, new
            {
                Id
            })) ?? new Tbl_VersionAgent();

            return data;
        }
        public async Task<TblMasterAplikasi> GetMasterAplikasi(GetByIdVM req)
        {
            const string query = "Select * FROM [Tbl_Master_Aplikasi] " +
                "WHERE [Id] = @Id";

            var data = await Db.WithConnectionAsync(c => c.QueryFirstOrDefaultAsync<TblMasterAplikasi>(query, new
            {
                req.Id
            })) ?? new TblMasterAplikasi();

            return data;
        }
        public async Task<MenuByChangeAppsVM> ChangeApplication(GetByIdAppVM req)
        {
            var ListMenu = GetMenu(new GetMenuFilterVM
            {
                RoleId = req.RoleId,
                AppId = req.AppId
            });

            var Apps = GetMasterAplikasi(new GetByIdVM
            {
                Id = req.AppId
            });

            await Task.WhenAll(ListMenu, Apps).ConfigureAwait(false);


            return new MenuByChangeAppsVM
            {
                MasterApps = Apps.Result,
                ListMenu = ListMenu.Result
            };
        }

        public async Task<int> GetCountDataReader(string UnitIds)
        {
            const string sp = "[ProcUtilityMapsGetAllDataReader]";
            var val = new
            {
                UnitIds = UnitIds
            };

            var data = await Db.WithConnectionAsync(db => db.QueryAsync<DataMapsVM>(sp, val, commandType: CommandType.StoredProcedure)) ?? new List<DataMapsVM>();

            return data.Count();
        }

        public async Task<List<DataMapsVM>> GetMapsDataReader()
        {
            const string sp = "[ProcUtilityMapsGetAllDataReader]";

            var data = await Db.WithConnectionAsync(db => db.QueryAsync<DataMapsVM>(sp, commandType: CommandType.StoredProcedure)) ?? new List<DataMapsVM>();

            return data.ToList();
        }
        public Core.Entities.DataMaster.Utility.Entity.TblUserSession GetUserSession(int userId)
        {
            const string query = "Select top(1) * FROM Tbl_UserSession " +
                "WHERE [User_Id] = @userId";

            var data = Db.WithConnection(c => c.QueryFirstOrDefault<Core.Entities.DataMaster.Utility.Entity.TblUserSession>(query, new
            {
                userId
            })) ?? new Core.Entities.DataMaster.Utility.Entity.TblUserSession();

            return data;
        }

        public bool UpdateUserSession(Core.Entities.DataMaster.Utility.Entity.TblUserSession userSession)
        {
            const string query = "Update [Tbl_UserSession] set " +
                        "SessionID = @SessionId, " +
                        "LastActive = @LastActive, " +
                        "Info = @Info, " +
                        "Role_Id = @RoleId, " +
                        "Unit_Id = @UnitId " +
                    "Where User_Id = @UserId";

            var update = Db.WithConnection(c => c.ExecuteScalar<int>(query, new
            {
                userSession.SessionId,
                userSession.LastActive,
                userSession.Info,
                userSession.RoleId,
                userSession.UnitId,
                userSession.UserId
            }));

            return (update == 0);
        }

        public bool InsertUserSession(Core.Entities.DataMaster.Utility.Entity.TblUserSession userSession)
        {
            const string query = "Insert Into Tbl_UserSession (" +
                    "User_Id, " +
                    "SessionID, " +
                    "LastActive, " +
                    "Info, " +
                    "Role_Id, " +
                    "Unit_Id) " +
                "values(" +
                    "@UserId, " +
                    "@SessionId, " +
                    "@LastActive, " +
                    "@Info, " +
                    "@RoleId, " +
                    "@UnitId) ";

            var insert = Db.WithConnection(c => c.ExecuteScalar<int>(query, new
            {
                userSession.UserId,
                userSession.SessionId,
                userSession.LastActive,
                userSession.Info,
                userSession.RoleId,
                userSession.UnitId
            }));

            return (insert == 0);
        }

        public bool InsertLogNIKInquiry(Core.Entities.DataMaster.Utility.Entity.Tbl_LogNIKInquiry req)
        {
            const string query = "Insert Into Tbl_LogNIKInquiry (" +
                    "Npp, " +
                    "Url, " +
                    "Nik, " +
                    "SearchParam, " +
                    "Action, " +
                    "IpAddress, " +
                    "Browser, " +
                    "CreatedTime) " +
                "values(" +
                    "@Npp, " +
                    "@Url, " +
                    "@Nik, " +
                    "@SearchParam, " +
                    "@Action, " +
                    "@IpAddress, " +
                    "@Browser, " +
                    "@CreatedTime) ";

            var insert = _baseConnection.WithConnection(c => c.ExecuteScalar<int>(query, new
            {
                Npp = new DbString { Value = req.Npp, Length = 50 },
                Url = new DbString { Value = req.Url, Length = 300 },
                Nik = new DbString { Value = req.Nik, Length = 50 },
                SearchParam = new DbString { Value = req.SearchParam, Length = 500 },
                Action = new DbString { Value = req.Action, Length = 250 },
                IpAddress = new DbString { Value = req.IpAddress, Length = 50 },
                Browser = new DbString { Value = req.Browser, Length = 350 },
                CreatedTime = DateTime.Now
            }));

            return (insert == 0);
        }

        public async Task<List<MonitoringReaderVM>> GetMonitoringReader(string UnitIds)
        {
            const string sp = "[ProcEksportExcelDataReader]";
            var val = new
            {
                UnitIds = UnitIds
            };
            var data = await Db.WithConnectionAsync(db => db.QueryAsync<MonitoringReaderVM>(sp, val, commandType: CommandType.StoredProcedure)) ?? new List<MonitoringReaderVM>();

            return data.ToList();
        }
    }
}
