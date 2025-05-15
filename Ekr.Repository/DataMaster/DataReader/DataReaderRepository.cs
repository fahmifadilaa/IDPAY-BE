using Dapper;
using Ekr.Core.Entities;
using Ekr.Core.Entities.DataMaster.AlatReader;
using Ekr.Core.Entities.DataMaster.DataReader.Entity;
using Ekr.Core.Entities.DataMaster.DataReader.ViewModel;
using Ekr.Dapper.Connection.Contracts.Sql;
using Ekr.Repository.Contracts.DataMaster.DataReader;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace Ekr.Repository.DataMaster.DataReader
{
    public class DataReaderRepository : BaseRepository, IDataReaderRepository
    {
        public DataReaderRepository(IEKtpReaderBackendDb con) : base(con) { }

        #region Load Data Reader
        public async Task<GridResponse<MasterAlatReaderVM>> LoadData(DataReaderFilterVM req, int unitId, int roleId)
        {
            const string sp = "[ProcMasterAlatReader]";
            var values = new
            {
                req.SerialNumber,
                req.UID,
                SColumn = req.SortColumn,
                SColumnValue = req.SortColumnDir,
                req.UnitIds,
                UnitId = unitId,
                RoleId = roleId,
                Page = req.PageNumber,
                Rows = req.PageSize
            };
            var data = Db.WithConnectionAsync(db => db.QueryAsync<MasterAlatReaderVM>(sp, values, commandType: CommandType.StoredProcedure));

            const string spCount = "[ProcMasterAlatReaderNum]";
            var valuesCount = new
            {
                req.SerialNumber,
                req.UnitIds,
                req.UID,
                UnitId = unitId,
                RoleId = roleId,
            };
            var count = Db.WithConnectionAsync(db => db.ExecuteScalarAsync<int>(spCount, valuesCount, commandType: CommandType.StoredProcedure));

            await Task.WhenAll(data, count).ConfigureAwait(false);

            return new GridResponse<MasterAlatReaderVM>
            {
                Count = count.Result,
                Data = data.Result
            };
        }

        public async Task<GridResponse<MasterAlatReaderVM>> LoadDataWithCondition(DataReaderConditionFilterVM req, int unitId, int roleId)
        {
            const string sp = "[ProcMasterAlatReaderIncondition]";
            var values = new
            {
                req.SerialNumber,
                req.UID,
                req.Status,
                SColumn = req.SortColumn,
                SColumnValue = req.SortColumnDir,
                Page = req.PageNumber,
                Rows = req.PageSize,
                UnitId = unitId,
                RoleId = roleId
            };
            var data = Db.WithConnectionAsync(db => db.QueryAsync<MasterAlatReaderVM>(sp, values, commandType: CommandType.StoredProcedure));

            const string spCount = "[ProcMasterAlatReaderInconditionNum]";
            var valuesCount = new
            {
                req.SerialNumber,
                req.UID,
                req.Status,
                UnitId = unitId,
                RoleId = roleId
            };
            var count = Db.WithConnectionAsync(db => db.ExecuteScalarAsync<int>(spCount, valuesCount, commandType: CommandType.StoredProcedure));

            await Task.WhenAll(data, count).ConfigureAwait(false);

            return new GridResponse<MasterAlatReaderVM>
            {
                Count = count.Result,
                Data = data.Result
            };
        }

        public async Task<int> GetCountJumlahDataReader(string UnitIds)
        {
            const string sp = "[ProcGetAllMasterAlatReader]";
            var val = new
            {
                UnitIds = UnitIds
            };

            var data = await Db.WithConnectionAsync(db => db.QueryAsync<MasterAlatReaderVM>(sp, val, commandType: CommandType.StoredProcedure)) ?? new List<MasterAlatReaderVM>();

            return data.Count();
        }
        #endregion

        #region View
        public async Task<TblMasterAlatReaderVM> GetDataReader(DataReaderViewFilterVM req)
        {
            //const string query = "Select * FROM Tbl_MasterAlatReader " +
            //    "WHERE Id = @Id";

            //return await Db.WithConnectionAsync(c => c.QueryFirstOrDefaultAsync<Tbl_MasterAlatReader>(query, new
            //{
            //    req.Id
            //}));

            const string query = "select TMA.*, TU.Type as TypeUnitId, TL.Name as TypeNamaUnit " +
                "from [dbo].[Tbl_MasterAlatReader] TMA " +
                "left join [dbo].[Tbl_Unit] TU on TMA.Unit_Id = TU.Id " +
                "left join [dbo].[Tbl_Lookup] TL on TU.Type = TL.Value and TL.Type = 'TipeUnit' " +
                "where TMA.Id = @Id";

            return await Db.WithConnectionAsync(db => db.QueryFirstOrDefaultAsync<TblMasterAlatReaderVM>(query, new { req.Id }));
        }

        public async Task<DashboardDetailReaderVM> GetDetailDataReader(DataReaderDetailFilter req)
        {
            const string spDs1 = "[ProcDashboardReaderInfoDetailsBySN2]";
            var valuesDs1 = new
            {
                Serial_Number = req.SerialNumber
            };
            var dataDs1 = Db.WithConnectionAsync(db => db.QueryAsync<DataDashboard1VM>(spDs1, valuesDs1, commandType: CommandType.StoredProcedure));

            const string spDs2 = "[ProcDashboardReaderInfoDetailsAlatBySN]";
            var valuesDs2 = new
            {
                Serial_Number = req.SerialNumber
            };
            var dataDs2 = Db.WithConnectionAsync(db => db.QueryAsync<DataDashboard2VM>(spDs2, valuesDs2, commandType: CommandType.StoredProcedure));

            const string spDs3 = "[ProcDashboardReaderGetAllLogReader]";
            var valuesDs3 = new
            {
                Serial_Number = req.SerialNumber,
                Date = DateTime.Now.ToString("yyyy-MM-dd")
            };
            var dataDs3 = Db.WithConnectionAsync(db => db.QueryAsync<DataDashboard3VM>(spDs3, valuesDs3, commandType: CommandType.StoredProcedure));

            await Task.WhenAll(dataDs1, dataDs2, dataDs3);

            return new DashboardDetailReaderVM
            {
                DataDashboard1 = dataDs1.Result.ToList() ?? new List<DataDashboard1VM>(),
                DataDashboard2 = dataDs2.Result.ToList() ?? new List<DataDashboard2VM>(),
                DataDashboard3 = dataDs3.Result.ToList() ?? new List<DataDashboard3VM>()
            };
        }

        public async Task<DashboardDetailReaderVM> GetDetailDataReaderByUID(string UID)
        {
            const string spDs1 = "[ProcDashboardReaderInfoDetailsByUID]";
            var valuesDs1 = new
            {
                uid = UID
            };
            var dataDs1 = Db.WithConnectionAsync(db => db.QueryAsync<DataDashboard1VM>(spDs1, valuesDs1, commandType: CommandType.StoredProcedure));

            const string spDs2 = "[ProcDashboardReaderInfoDetailsAlatByUID]";
            var valuesDs2 = new
            {
                uid = UID
            };
            var dataDs2 = Db.WithConnectionAsync(db => db.QueryAsync<DataDashboard2VM>(spDs2, valuesDs2, commandType: CommandType.StoredProcedure));

            await Task.WhenAll(dataDs1, dataDs2);

            return new DashboardDetailReaderVM
            {
                DataDashboard1 = dataDs1.Result.ToList() ?? new List<DataDashboard1VM>(),
                DataDashboard2 = dataDs2.Result.ToList() ?? new List<DataDashboard2VM>()
            };
        }

        public async Task<MonitoringReaderExcelVM> GetDetailAlatDataReader(DataReaderDetailAlatFilter req)
        {
            const string sp = "[ProcDashboardReaderInfoDetailsAlat]";
            var values = new
            {
                req.UID
            };

            var result = await Db.WithConnectionAsync(db => db.QueryFirstOrDefaultAsync<MonitoringReaderExcelVM>(
                sp,
                values,
                commandType: CommandType.StoredProcedure)
            );
            if(result == null)
			{
                return null;
			}
            return result;
        }

        public Task<Tbl_MasterAlatReader> GetDatareaderUid(string uid)
        {
            const string query = "Select * FROM Tbl_MasterAlatReader " +
                "WHERE UID = @uid and isdeleted = 0";

            return Db.WithConnectionAsync(c => c.QueryFirstOrDefaultAsync<Tbl_MasterAlatReader>(query, new { uid }));
        }

        public Task<Tbl_MasterAlatReader> GetDatareaderBySN(string sn)
        {
            const string query = "Select * FROM Tbl_MasterAlatReader " +
                "WHERE SN_Unit = @sn and isdeleted = 0";

            return Db.WithConnectionAsync(c => c.QueryFirstOrDefaultAsync<Tbl_MasterAlatReader>(query, new { sn }));
        }
        #endregion

        #region Insert
        public async Task<Tbl_MasterAlatReader> InsertDataReader(Tbl_MasterAlatReader req)
        {
            req.CreatedTime = DateTime.Now;
            req.IsActive = true;
            req.IsDeleted = false;
            const string query = "Insert Into Tbl_MasterAlatReader (" +
                    "Kode," +
                    "Nama," +
                    "Unit_Id," +
                    "SN_Unit," +
                    "No_Perso_SAM," +
                    "No_Kartu," +
                    "PCID," +
                    "Confiq," +
                    "UID," +
                    "Status," +
                    "LastIP," +
                    "LastPingIP," +
                    "LastActive," +
                    "Latitude," +
                    "Longitude," +
                    "LastUserId," +
                    "LastPegawaiId," +
                    "LastUnitId," +
                    "LastUsed," +
                    "IsActive," +
                    "IsDeleted," +
                    "CreatedTime," +
                    "CreatedBy_Id) " +
                "values(" +
                    "@Kode," +
                    "@Nama," +
                    "@Unit_Id," +
                    "@SN_Unit," +
                    "@No_Perso_SAM," +
                    "@No_Kartu," +
                    "@PCID," +
                    "@Confiq," +
                    "@UID," +
                    "@Status," +
                    "@LastIP," +
                    "@LastPingIP," +
                    "@LastActive," +
                    "@Latitude," +
                    "@Longitude," +
                    "@LastUserId," +
                    "@LastPegawaiId," +
                    "@LastUnitId," +
                    "@LastUsed," +
                    "@IsActive," +
                    "@IsDeleted," +
                    "@CreatedTime," +
                    "@CreatedBy_Id)";

            await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
            {
                req.Kode,
                req.Nama,
                req.Unit_Id,
                req.SN_Unit,
                req.No_Perso_SAM,
                req.No_Kartu,
                req.PCID,
                req.Confiq,
                req.UID,
                req.Status,
                req.LastIP,
                req.LastPingIP,
                req.LastActive,
                req.Latitude,
                req.Longitude,
                req.LastUserId,
                req.LastPegawaiId,
                req.LastUnitId,
                req.LastUsed,
                IsActive = true,
                IsDeleted = false,
                req.CreatedTime,
                req.CreatedBy_Id
            }));


            //diganti keini ketika nama table udh diganti tanpa underscore
            //await Db.WithConnectionAsync(c => c.InsertAsync(req, true));
            return req;
        }
        public bool ExcelBulkInsert(List<Tbl_MasterAlatReader> req)
        {
            try
            {
                BulkInsert(req);
            }catch(Exception err)
            {
                return false;
            }
            return true;
        }
        #endregion

        #region Update
        public async Task<Tbl_MasterAlatReader> UpdateDataReader(Tbl_MasterAlatReader req)
        {
            req.UpdatedTime = DateTime.Now;
            const string query = "Update Tbl_MasterAlatReader set " +
                        "Kode = @Kode, " +
                        "Nama = @Nama, " +
                        "Unit_Id = @Unit_Id, " +
                        "SN_Unit = @SN_Unit, " +
                        "No_Perso_SAM = @No_Perso_SAM, " +
                        "No_Kartu = @No_Kartu, " +
                        "PCID = @PCID, " +
                        "Confiq = @Confiq, " +
                        "UID = @UID, " +
                        "Status = @Status, " +
                        "LastIP = @LastIP, " +
                        "LastPingIP = @LastPingIP, " +
                        "LastActive = @LastActive, " +
                        "Latitude = @Latitude, " +
                        "Longitude = @Longitude, " +
                        "LastUserId = @LastUserId, " +
                        "LastPegawaiId = @LastPegawaiId, " +
                        "LastUnitId = @LastUnitId, " +
                        "LastUsed = @LastUsed, " +
                        "IsActive = @IsActive, " +
                        "UpdatedTime = @UpdatedTime, " +
                        "UpdatedBy_Id = @UpdatedBy_Id " +
                    "Where Id = @Id";

            await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
            {
                req.Kode,
                req.Nama,
                req.Unit_Id,
                req.SN_Unit,
                req.No_Perso_SAM,
                req.No_Kartu,
                req.PCID,
                req.Confiq,
                req.UID,
                req.Status,
                req.LastIP,
                req.LastPingIP,
                req.LastActive,
                req.Latitude,
                req.Longitude,
                req.LastUserId,
                req.LastPegawaiId,
                req.LastUnitId,
                req.LastUsed,
                req.IsActive,
                req.UpdatedTime,
                req.UpdatedBy_Id,
                req.Id
            }));


            //diganti keini ketika nama table udh diganti tanpa underscore
            //await Db.WithConnectionAsync(c => c.UpdateAsync(req));
            return req;
        }
        #endregion

        #region Delete
        public async Task DeleteDataReader(DataReaderViewFilterVM req, int PegawaiId)
        {
            const string query = "Update Tbl_MasterAlatReader set " +
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

        #region Load Data Log
        public async Task<GridResponse<MasterAlatReaderLogActivityVM>> LoadDataLogActivity(LogActivityDataReaderFilterVM req)
        {
            const string sp = "[ProcMasterAlatReaderLogActvity]";
            var values = new
            {
                req.UID,
                req.Type,
                req.NIK,
                req.LastIp,
                SColumn = req.SortColumn,
                SColumnValue = req.SortColumnDir,
                Page = req.PageNumber,
                Rows = req.PageSize
            };
            var data = Db.WithConnectionAsync(db => db.QueryAsync<MasterAlatReaderLogActivityVM>(sp, values, commandType: CommandType.StoredProcedure));

            const string spCount = "[ProcMasterAlatReaderLogActvityNum]";
            var valuesCount = new
            {
                req.UID,
                req.Type,
                req.NIK,
                req.LastIp
            };
            var count = Db.WithConnectionAsync(db => db.ExecuteScalarAsync<int>(spCount, valuesCount, commandType: CommandType.StoredProcedure));

            await Task.WhenAll(data, count).ConfigureAwait(false);

            return new GridResponse<MasterAlatReaderLogActivityVM>
            {
                Count = count.Result,
                Data = data.Result
            };
        }

        public async Task<GridResponse<MasterAlatReaderLogConnectionVM>> LoadDataLogConnection(LogConnectionDataReaderFilterVM req)
        {
            const string sp = "[ProcMasterAlatReaderLogConnection]";
            var values = new
            {
                req.UID,
                req.IP,
                req.Status,
                SColumn = req.SortColumn,
                SColumnValue = req.SortColumnDir,
                Page = req.PageNumber,
                Rows = req.PageSize
            };
            var data = Db.WithConnectionAsync(db => db.QueryAsync<MasterAlatReaderLogConnectionVM>(sp, values, commandType: CommandType.StoredProcedure));

            const string spCount = "[ProcMasterAlatReaderLogConnectionNum]";
            var valuesCount = new
            {
                req.UID,
                req.IP,
                req.Status
            };
            var count = Db.WithConnectionAsync(db => db.ExecuteScalarAsync<int>(spCount, valuesCount, commandType: CommandType.StoredProcedure));

            await Task.WhenAll(data, count).ConfigureAwait(false);

            return new GridResponse<MasterAlatReaderLogConnectionVM>
            {
                Count = count.Result,
                Data = data.Result
            };
        }

        public async Task<GridResponse<MasterAlatReaderLogUserVM2>> LoadDataLogUser(LogUserDataReaderFilterVM req)
        {
            const string sp = "[ProcMasterAlatReaderUser]";
            var values = new
            {
                req.UID,
                req.Nama,
                SColumn = req.SortColumn,
                SColumnValue = req.SortColumnDir,
                Page = req.PageNumber,
                Rows = req.PageSize
            };
            var data = Db.WithConnectionAsync(db => db.QueryAsync<MasterAlatReaderLogUserVM2>(sp, values, commandType: CommandType.StoredProcedure));

            const string spCount = "[ProcMasterAlatReaderUserNum]";
            var valuesCount = new
            {
                req.UID,
                req.Nama
            };
            var count = Db.WithConnectionAsync(db => db.ExecuteScalarAsync<int>(spCount, valuesCount, commandType: CommandType.StoredProcedure));

            await Task.WhenAll(data, count).ConfigureAwait(false);

            return new GridResponse<MasterAlatReaderLogUserVM2>
            {
                Count = count.Result,
                Data = data.Result
            };
        }
        #endregion
    }
}
