using Dapper;
using Ekr.Core.Configuration;
using Ekr.Core.Entities;
using Ekr.Core.Entities.DataMaster.AlatReader;
using Ekr.Core.Entities.DataMaster.DataReader.Entity;
using Ekr.Core.Entities.DataMaster.Utility.ViewModel;
using Ekr.Core.Entities.Logging;
using Ekr.Dapper.Connection.Base;
using Ekr.Dapper.Connection.Contracts.Base;
using Ekr.Dapper.Connection.Contracts.Sql;
using Ekr.Repository.Contracts.DataMaster.AlatReader;
using Microsoft.Extensions.Options;
using ServiceStack.Logging;
using System;
using System.Data;
using System.Threading.Tasks;

namespace Ekr.Repository.DataMaster.AlatReaders
{
    public class AlatReaderRepository : BaseRepository, IAlatReaderRepository
    {
        private readonly IBaseConnection _baseConnection;
        public AlatReaderRepository(IEKtpReaderBackendDb con, IOptions<ConnectionStringConfig> options,
            IOptions<ErrorMessageConfig> options2) : base(con)
        {
            _baseConnection = new SqlServerConnection(options.Value.dbConnection1, options2);
        }

        #region GET
        public async Task<GridResponse<TblMasterAlatReaderVM>> GridGetAll(MasterAlatReaderFilter req)
        {
            const string sp = "[ProcMasterAlatReaderData]";
            var values = new
            {
                SerialNumber = req.SerialNumber,
                UID = req.UID,
                SColumn = req.SortColumn,
                SColumnValue = req.SortColumnDir,
                Page = req.PageNumber,
                Rows = req.PageSize
            };

            var data = Db.WithConnectionAsync(db =>
            db.QueryAsync<TblMasterAlatReaderVM>(sp, values, commandType: CommandType.StoredProcedure));

            const string spCount = "[ProcMasterAlatReaderTotal]";
            var valuesCount = new
            {
                SerialNumber = req.SerialNumber,
                UID = req.UID
            };

            var count = Db.WithConnectionAsync(db =>
            db.ExecuteScalarAsync<int>(spCount, valuesCount, commandType: CommandType.StoredProcedure));

            await Task.WhenAll(data, count)
                .ConfigureAwait(false);

            return new GridResponse<TblMasterAlatReaderVM>
            {
                Count = count.Result,
                Data = data.Result
            };
        }

        public async Task<GridResponse<Tbl_VersionAgentVM>> GridGetAllVersionApps(AppsVersionRequestFilter req)
        {
            const string sp = "[ProcVersionApps]";
            var values = new
            {
                req.Version,
                SColumn = req.SortColumn,
                SColumnValue = req.SortColumnDir,
                Page = req.PageNumber,
                Rows = req.PageSize
            };

            var data = Db.WithConnectionAsync(db =>
            db.QueryAsync<Tbl_VersionAgentVM>(sp, values, commandType: CommandType.StoredProcedure));

            const string spCount = "[ProcVersionAppsCount]";
            var valuesCount = new
            {
                req.Version
            };

            var count = Db.WithConnectionAsync(db =>
            db.ExecuteScalarAsync<int>(spCount, valuesCount, commandType: CommandType.StoredProcedure));

            await Task.WhenAll(data, count)
                .ConfigureAwait(false);

            return new GridResponse<Tbl_VersionAgentVM>
            {
                Count = count.Result,
                Data = data.Result
            };
        }

        public Task<Tbl_VersionAgent> GetVersionById(int Id)
        {
            const string query = "select * from [dbo].[Tbl_VersionAgent] where Id = @Id";

            return Db.WithConnectionAsync(db => db.QueryFirstOrDefaultAsync<Tbl_VersionAgent>(query, new { Id }));
        }

        public Task<TblMasterAlatReaderVM> GetDataByUID(string uid)
        {
            const string query = "select TMA.*, TU.Type as TypeUnitId, TL.Name as TypeNamaUnit " +
                "from [dbo].[Tbl_MasterAlatReader] TMA " +
                "left join [dbo].[Tbl_Unit] TU on TMA.Unit_Id = TU.Id " +
                "left join [dbo].[Tbl_Lookup] TL on TU.Type = TL.Value and TL.Type = 'TipeUnit' " +
                "where TMA.UID = @uid";

            return Db.WithConnectionAsync(db => db.QueryFirstOrDefaultAsync<TblMasterAlatReaderVM>(query, new { uid }));
        }

        public Task<TblMasterAlatReaderUser> GetAlatMasterReaderUserByUID(string uid)
        {
            const string query = "select * from [dbo].[Tbl_MasterAlatReaderUser] where uid = @uid";

            return Db.WithConnectionAsync(db => db.QueryFirstOrDefaultAsync<TblMasterAlatReaderUser>(query, new { uid }));
        }

        public Task<TblMasterAlatReaderUser> GetAlatMasterReaderUserByUIDPegawaiId(string uid, string npp)
        {
            const string query = "select * from [dbo].[Tbl_MasterAlatReaderUser] where uid = @uid and Npp = @npp";

            return Db.WithConnectionAsync(db => db.QueryFirstOrDefaultAsync<TblMasterAlatReaderUser>(query, new { uid, npp }));
        }

        public Task<TblMasterAlatReaderUser> GetAlatMasterReaderUserByUIDPegawaiIdMax(string uid, string npp)
        {
            const string query = "select max (Id) from [dbo].[Tbl_MasterAlatReaderUser] where uid = @uid and Npp = @npp";

            return Db.WithConnectionAsync(db => db.QueryFirstOrDefaultAsync<TblMasterAlatReaderUser>(query, new { uid, npp }));
        }
        #endregion

        #region create
        public async Task<ReqCreateMasterAlatReader> CreateAlatMasterReader(ReqCreateMasterAlatReader req)
        {
            int isactive = 0;
            int isdelete = 0;
            //if (req.isActive == true)
            //{
            //    isactive = 1;
            //}
            if (req.isDelete == true)
            {
                isdelete = 1;
            }
            const string query = "Insert Into Tbl_MasterAlatReader (" +
            "[Kode]," +
            "[Nama]," +
            "[Unit_Id]," +
            "[SN_Unit]," +
            "[No_Perso_SAM]," +
            "[No_Kartu]," +
            "[PCID]," +
            "[Confiq]," +
            "[UID]," +
            "[Status]," +
            "[LastIP]," +
            "[LastPingIP]," +
            "[LastActive]," +
            "[Latitude]," +
            "[Longitude]," +
            "[LastPegawaiId]," +
            "[LastNpp]," +
            "[LastUnitCode]," +
            "[LastUnitId]," +
            "[LastUsed]," +
            "[IsActive]," +
            "[IsDeleted]," +
            "[CreatedTime]," +
            "[CreatedBy_Id])" +
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
            "@LastPegawaiId," +
            "@LastNpp," +
            "@LastUnitCode," +
            "@LastUnitId," +
            "@LastUsed," +
            "@IsActive," +
            "@IsDeleted," +
            "@CreatedTime," +
            "@CreatedBy_Id)";

            _ = await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
            {
                Kode = req.kode,
                Nama = req.nama,
                Unit_Id = req.unitId,
                SN_Unit = req.snUnit,
                No_Perso_SAM = req.noPersoSam,
                No_Kartu = req.noKartu,
                PCID = req.pcid,
                Confiq = req.confiq,
                UID = req.uid,
                Status = req.status,
                LastIP = req.lastIp,
                LastPingIP = DateTime.Parse(req.lastPingIp),
                LastActive = DateTime.Now,
                Latitude = req.latitude,
                Longitude = req.longitude,
                LastPegawaiId = req.lastPegawaiId,
                LastNpp = req.lastNpp,
                LastUnitCode = req.lastUnitCode,
                LastUnitId = req.LastUnitId,
                LastUsed = DateTime.Now,
                IsActive = isactive,
                IsDeleted = isdelete,
                CreatedTime = DateTime.Now,
                CreatedBy_Id = req.lastPegawaiId
            }));

            return req;
        }

        public async Task<ReqCreateLogActivity> CreateLogActivity(ReqCreateLogActivity req)
        {
            const string query = "Insert Into Tbl_MasterAlatReaderLogActvity (" +
                "[UID]," +
                "[Type]," +
                "[NIK]," +
                "[LastIP]," +
                "[PegawaiId]," +
                "[NppPegawai]," +
                "[KodeUnit]," +
                "[UnitId]," +
                "[SessionId]," +
                "[ReqCode]," +
                "[ResultSocket]," +
                "[IsActive]," +
                "[IsDeleted]," +
                "[CreatedTime]," +
                "[CreatedBy_Id]," +
                "[UpdatedTime]," +
                "[UpdatedBy_Id])" +
            "values(" +
                "@UID," +
                "@Type," +
                "@NIK," +
                "@LastIP," +
                "@PegawaiId," +
                "@NppPegawai," +
                "@KodeUnit," +
                "@UnitId," +
                "@SessionId," +
                "@ReqCode," +
                "@ResultSocket," +
                "@IsActive," +
                "@IsDeleted," +
                "@CreatedTime," +
                "@CreatedBy_Id," +
                "@UpdatedTime," +
                "@UpdatedBy_Id)";

            await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
            {
                req.UID,
                req.Type,
                req.NIK,
                req.LastIP,
                req.PegawaiId,
                req.NppPegawai,
                req.KodeUnit,
                req.UnitId,
                req.SessionId,
                req.ReqCode,
                req.ResultSocket,
                IsActive = 1,
                IsDeleted = 0,
                CreatedTime = DateTime.Now,
                CreatedBy_Id = req.PegawaiId,
                UpdatedTime = DateTime.Now,
                UpdatedBy_Id = req.PegawaiId
            }));

            return req;
        }

        public async Task<ReqCreateMasterAlatReaderLogError> CreateLogError(ReqCreateMasterAlatReaderLogError req)
        {
            const string query = "Insert Into Tbl_MasterAlatReaderLogError (" +
                "[UID]," +
                "[LastIP]," +
                "[NppPegawai]," +
                "[KodeUnit]," +
                "[SessionId]," +
                "[ReqCode]," +
                "[ResultSocket]," +
                "[CreatedTime]," +
                "[CreatedByNpp])" +
            "values(" +
                "@UID," +
                "@LastIp," +
                "@NppPegawai," +
                "@KodeUnit," +
                "@SessionId," +
                "@ReqCode," +
                "@ResultSocket," +
                "@CreatedTime," +
                "@CreatedByNpp)";

            await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
            {
                req.UID,
                req.LastIp,
                req.NppPegawai,
                req.KodeUnit,
                req.SessionId,
                req.ReqCode,
                req.ResultSocket,
                CreatedTime = DateTime.Now,
                CreatedByNpp = req.NppPegawai
            }));

            return req;
        }

        public async Task<Tbl_MasterAlatReaderLogActvity> CreateLogActivity2(Tbl_MasterAlatReaderLogActvity req)
        {
            try
            {

                const string query = "Insert Into Tbl_MasterAlatReaderLogActvity (" +
                    "[UID]," +
                    "[Type]," +
                    "[NIK]," +
                    "[LastIP]," +
                    "[PegawaiId]," +
                    "[NppPegawai]," +
                    "[KodeUnit]," +
                    "[UnitId]," +
                    "[SessionId]," +
                    "[ReqCode]," +
                    "[ResultSocket]," +
                    "[IsActive]," +
                    "[IsDeleted]," +
                    "[CreatedTime]," +
                    "[CreatedBy_Id]," +
                    "[UpdatedTime]," +
                    "[UpdatedBy_Id])" +
                "values(" +
                    "@UID," +
                    "@Type," +
                    "@NIK," +
                    "@LastIP," +
                    "@PegawaiId," +
                    "@NppPegawai," +
                    "@KodeUnit," +
                    "@UnitId," +
                    "@SessionId," +
                    "@ReqCode," +
                    "@ResultSocket," +
                    "@IsActive," +
                    "@IsDeleted," +
                    "@CreatedTime," +
                    "@CreatedBy_Id," +
                    "@UpdatedTime," +
                    "@UpdatedBy_Id)";

                // DB LOG
                await _baseConnection.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
                {
                    UID = new DbString { Value = req.UID, Length = 100, IsAnsi = false },
                    Type = new DbString { Value = req.Type, Length = 100, IsAnsi = false },
                    NIK = new DbString { Value = req.NIK, Length = 50, IsAnsi = false },
                    LastIP = new DbString { Value = req.LastIP, Length = 100, IsAnsi = false },
                    req.PegawaiId,
                    NppPegawai = new DbString { Value = req.NppPegawai, Length = 50, IsAnsi = false },
                    KodeUnit = new DbString { Value = req.KodeUnit, Length = 50, IsAnsi = false },
                    req.UnitId,
                    SessionId = new DbString { Value = req.SessionId, Length = 250, IsAnsi = false },
                    ReqCode = new DbString { Value = req.ReqCode, Length = 250, IsAnsi = false },
                    req.ResultSocket,
                    IsActive = 1,
                    IsDeleted = 0,
                    CreatedTime = DateTime.Now,
                    CreatedBy_Id = req.CreatedBy_Id,
                    UpdatedTime = DateTime.Now,
                    UpdatedBy_Id = req.CreatedBy_Id
                }));

                // MAIN DB
                //await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
                //{
                //    req.UID,
                //    req.Type,
                //    req.NIK,
                //    req.LastIP,
                //    req.PegawaiId,
                //    req.NppPegawai,
                //    req.KodeUnit,
                //    req.UnitId,
                //    req.SessionId,
                //    req.ReqCode,
                //    req.ResultSocket,
                //    IsActive = 1,
                //    IsDeleted = 0,
                //    CreatedTime = DateTime.Now,
                //    CreatedBy_Id = req.CreatedBy_Id,
                //    UpdatedTime = DateTime.Now,
                //    UpdatedBy_Id = req.CreatedBy_Id
                //}));

                return req;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public Task<int> InsertLogEnrollThirdParty(Tbl_Enrollment_ThirdParty_Log logActivity)
        {
            try
            {
                //const string query = "Insert Into [Tbl_Enrollment_ThirdParty_Log] (" +
                //    "NIK, " +
                //    "AppsChannel, " +
                //    "SubmitDate) " +
                //"values(" +
                //    "@NIK, " +
                //    "@AppsChannel, " +
                //    "@SubmitDate) ";

                const string query = "Insert Into [Tbl_Enrollment_ThirdParty_Log] (" +
                    "NIK, " +
                    "AppsChannel, " +
                    "SubmitDate, " +
                    "JournalId) " +
                "values(" +
                    "@NIK, " +
                    "@AppsChannel, " +
                    "@SubmitDate, " +
                    "@JournalId) ";

                var insert = _baseConnection.WithConnection(c => c.ExecuteScalar<int>(query, new
                {
                    logActivity.NIK,
                    logActivity.AppsChannel,
                    logActivity.SubmitDate,
                    logActivity.JournalID
                }));
                return Task.FromResult(_baseConnection.WithConnection(c => c.ExecuteScalar<int>(query, new
                {
                    logActivity.NIK,
                    logActivity.AppsChannel,
                    logActivity.SubmitDate
                })));


            }
            catch (Exception Ex)
            {
                return Task.FromResult(1);
            }


        }

        public long CreateAlatReaderLogActivity(Tbl_MasterAlatReaderLogActvity log)
        {
            //var lastId = Db.WithConnection(c => c.ExecuteScalar<int>("select max(Id) from dbo.Tbl_MasterAlatReaderLogActvity"));

            //log.Id = lastId + 1;

            return InsertIncrement(log);
        }

        public long CreateAlatReaderLogActivityNew(Tbl_MasterAlatReaderLogActvity logActivity)
        {
            return InsertIncrement(logActivity);
        }

        public async Task<Tbl_MasterAlatReaderLog> CreateAlatReaderLog(Tbl_MasterAlatReaderLog log)
        {
            const string query = "Insert Into Tbl_MasterAlatReaderLog (" +
            "[UID]," +
            "[Serial_Number]," +
            "[Type]," +
            "[NIK]," +
            "[PegawaiId]," +
            "[UnitId]," +
            "[IsActive]," +
            "[IsDeleted]," +
            "[CreatedTime]," +
            "[CreatedBy_Id])" +
        "values(" +
            "@UID," +
            "@Serial_Number," +
            "@Type," +
            "@Nik," +
            "@PegawaiId," +
            "@UnitId," +
            "@IsActive," +
            "@IsDeleted," +
            "@CreatedTime," +
            "@CreatedBy_Id)";

            await _baseConnection.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
            {
                UID = new DbString { Value = log.Uid , Length = 100, IsAnsi = false},
                Serial_Number = new DbString { Value = log.Serial_Number, Length = 100, IsAnsi = false },
                Type = new DbString { Value = log.Type, Length = 100, IsAnsi = false },
                Nik = new DbString { Value = log.Nik, Length = 50, IsAnsi = false },
                log.PegawaiId,
                log.UnitId,
                log.IsActive,
                log.IsDeleted,
                CreatedTime = DateTime.Now,
                log.CreatedBy_Id
            }));
            return log;
        }

        public async Task<ReqCreateLogConnection> CreateAlatMasterReaderLogConnection(ReqCreateLogConnection req)
        {
            const string query = "Insert Into [Tbl_MasterAlatReaderLogConnections] (" +
            "[UID]," +
            "[IP]," +
            "[Status]," +
            "[RFID]," +
            "[CreatedTime]," +
            "[StartTimePing]," +
            "[EndTimePing])" +
        "values(" +
            "@UID," +
            "@IP," +
            "@Status," +
            "@RFID," +
            "@CreatedTime," +
            "@StartTimePing," +
            "@EndTimePing)";

            await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
            {
                UID = req.UID,
                IP = req.ip,
                Status = req.status,
                RFID = req.rfid,
                CreatedTime = DateTime.Now,
                StartTimePing = req.startTimePing,
                EndTimePing = req.EndTimePing
            }));
            return req;
        }

        public async Task<ReqMasterAlatReaderGetByUid> CreateAlatMasterReaderUser(ReqMasterAlatReaderGetByUid req)
        {
            const string query = "Insert Into Tbl_MasterAlatReaderUser (" +
            "[UID]," +
            "[PegawaiId]," +
            "[UnitId]," +
            "[Npp]," +
            "[UnitCode]," +
            "[LastActive])" +
        "values(" +
            "@UID," +
            "@PegawaiId," +
            "@UnitId," +
            "@Npp," +
            "@UnitCode," +
            "@LastActive)";

            await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
            {
                req.UID,
                PegawaiId = req.pegawaiId,
                UnitId = req.unitId,
                Npp = req.NppPegawai,
                UnitCode = req.KodeUnit,
                LastActive = DateTime.Now
            }));
            return req;
        }
        #endregion

        #region Update
        //public async Task<bool> UpdateAlatMasterReaderUserByUID(int id)
        public async Task<bool> UpdateAlatMasterReaderUserByUID(string uid)
        {
            try
            {
                const string query = "Update Tbl_MasterAlatReaderUser set " +
                                "[LastActive] = @LastActive" +
                                " where uid  = @uid ";

                //const string query = "Update Tbl_MasterAlatReaderUser set " +
                //                "[LastActive] = @LastActive" +
                //                " where Id  = @id ";

                await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
                {
                    LastActive = DateTime.Now,
                    uid
                    //id
                }));
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> UpdateAlatMasterReaderUserByID(int id)
        {
            try
            {
                //const string query = "Update Tbl_MasterAlatReaderUser set " +
                //                "[LastActive] = @LastActive" +
                //                " where uid  = @uid ";

                const string query = "Update Tbl_MasterAlatReaderUser set " +
                                "[LastActive] = @LastActive" +
                                " where Id  = @id ";

                await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
                {
                    LastActive = DateTime.Now,
                    id
                    //id
                }));
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<ReqCreateMasterAlatReader> UpdateAlatMasterReader(ReqCreateMasterAlatReader req)
        {
            int isactive = 0;
            int isdelete = 0;
            var uids = req.uid;
            if (req.isActive == true)
            {
                isactive = 1;
            }
            if (req.isDelete == true)
            {
                isdelete = 1;
            }
            const string query = "Update [Tbl_MasterAlatReader] set " +
                            "[Kode] = @Kode," +
                            "[Nama] = @Nama," +
                            "[Unit_Id] = @Unit_Id," +
                            "[SN_Unit] = @SN_Unit," +
                            "[No_Perso_SAM] = @No_Perso_SAM," +
                            "[No_Kartu] = @No_Kartu," +
                            "[PCID] = @PCID," +
                            "[Confiq] = @Confiq," +
                            "[UID] = @UID," +
                            "[Status] = @Status," +
                            "[LastIP] = @LastIP," +
                            "[LastPingIP] = @LastPingIP," +
                            "[LastActive] = @LastActive," +
                            "[Latitude] = @Latitude," +
                            "[Longitude] = @Longitude," +
                            "[LastPegawaiId] = @LastPegawaiId," +
                            "[LastUnitId] = @LastUnitId," +
                            "[LastUsed] = @LastUsed," +
                            "[IsActive] = @IsActive," +
                            "[IsDeleted] = @IsDeleted," +
                            "[UpdatedTime] = @UpdatedTime," +
                            "[UpdatedBy_Id] = @UpdatedBy_Id" +
                            " where UID  = @UIDS ";

            await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
            {
                Kode = req.kode,
                Nama = req.nama,
                Unit_Id = req.unitId,
                SN_Unit = req.snUnit,
                No_Perso_SAM = req.noPersoSam,
                No_Kartu = req.noKartu,
                PCID = req.pcid,
                Confiq = req.confiq,
                UID = req.uid,
                Status = req.status,
                LastIP = req.lastIp,
                LastPingIP = DateTime.Parse(req.lastPingIp),
                LastActive = DateTime.Now,
                Latitude = req.latitude,
                Longitude = req.longitude,
                LastPegawaiId = req.lastPegawaiId,
                LastUnitId = req.LastUnitId,
                LastUsed = DateTime.Now,
                IsActive = isactive,
                IsDeleted = isdelete,
                UpdatedTime = DateTime.Now,
                UpdatedBy_Id = req.lastPegawaiId,
                UIDS = uids
            }));
            return req;
        }

        public async Task<ReqCreateMasterAlatReader> UpdateAlatMasterReaderFromLogActivity(ReqCreateMasterAlatReader req)
        {
            const string query = "Update [Tbl_MasterAlatReader] set " +
                            "[LastIP] = @LastIP," +
                            "[LastActive] = @LastActive," +
                            "[LastPegawaiId] = @LastPegawaiId," +
                            "[LastNpp] = @LastNpp," +
                            "[LastUnitCode] = @LastUnitCode," +
                            "[LastUnitId] = @LastUnitId," +
                            "[LastUsed] = @LastUsed," +
                            "[UpdatedTime] = @UpdatedTime," +
                            "[UpdatedBy_Id] = @UpdatedBy_Id" +
                            " where UID  = @UIDS ";

            await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
            {
                LastIP = req.lastIp,
                LastActive = DateTime.Now,
                LastPegawaiId = req.lastPegawaiId,
                LastNpp = req.lastNpp,
                LastUnitCode = req.lastUnitCode,
                LastUnitId = req.unitId,
                LastUsed = DateTime.Now,
                UpdatedTime = DateTime.Now,
                UpdatedBy_Id = req.lastPegawaiId,
                UIDS = req.uid
            }));
            return req;
        }

        public async Task<ReqUpdateMasterAlatReader> UpdateStatusAlatMasterReader(ReqUpdateMasterAlatReader req)
        {
            var active = DateTime.ParseExact(req.lastActive, "yyyy-MM-dd HH:mm:ss,fff",
                                   System.Globalization.CultureInfo.InvariantCulture);
            var used = DateTime.ParseExact(req.lastUsed, "yyyy-MM-dd HH:mm:ss,fff",
                                   System.Globalization.CultureInfo.InvariantCulture);

            const string query = "Update [Tbl_MasterAlatReader] set " +
                            "[Status] = @Status," +
                            "[LastIP] = @LastIP," +
                            "[LastUsed] = @LastUsed," +
                            "[LastActive] = @LastActive," +
                            "[UpdatedTime] = @UpdatedTime," +
                            "[UpdatedBy_Id] = @UpdatedBy_Id" +
                            " where UID  = @UIDS ";

            await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
            {
                Status = req.status,
                LastIP = req.lastIp,
                LastUsed = used,
                LastActive = active,
                UpdatedTime = DateTime.Now,
                UpdatedBy_Id = req.UpdatedBy_Id,
                UIDS = req.uid
            }));
            return req;
        }

        public async Task<UpdateStatusManifestAlatReader> UpdateStatusManifestAlatReader(UpdateStatusManifestAlatReader req, int idPegawai)
        {
            const string query = "Update [Tbl_MasterAlatReader] set " +
                            "[IsErrorManifest] = @IsErrorManifest," +
                            "[UpdatedTime] = @UpdatedTime," +
                            "[UpdatedBy_Id] = @UpdatedBy_Id" +
                            " where UID  = @UIDS ";

            await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
            {
                IsErrorManifest = req.IsManifestError,
                UpdatedTime = DateTime.Now,
                UpdatedBy_Id = idPegawai,
                UIDS = req.uid
            }));
            return req;
        }
        #endregion

        #region delete
        public async Task<string> DeleteApps(int Id, string updateBy)
        {
            const string query = "Update [Tbl_VersionAgent] set " +
                            "[UpdatedById] = @UpdatedById," +
                            "[UpdatedTime] = @UpdatedTime," +
                            "[IsActive] = @IsActive," +
                            "[IsDeleted] = @IsDeleted" +
                            " where Id  = @Id";

            await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
            {
                UpdatedById = updateBy,
                UpdatedTime = DateTime.Now,
                IsActive = 0,
                IsDeleted = 1,
                Id,
            }));

            return "";
        }
        #endregion
    }
}
