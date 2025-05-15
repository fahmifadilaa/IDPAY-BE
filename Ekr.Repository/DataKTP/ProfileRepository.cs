using Dapper;
using Ekr.Core.Configuration;
using Ekr.Core.Entities.Auth;
using Ekr.Core.Entities.DataEnrollment.Entity;
using Ekr.Core.Entities.DataKTP;
using Ekr.Core.Entities.SettingThreshold;
using Ekr.Dapper.Connection.Base;
using Ekr.Dapper.Connection.Contracts.Base;
using Ekr.Dapper.Connection.Contracts.Sql;
using Ekr.Repository.Contracts.DataKTP;
using Microsoft.Extensions.Options;
using ServiceStack.OrmLite;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Ekr.Repository.DataKTP
{
    public class ProfileRepository : BaseRepository, IProfileRepository
    {
        private readonly IBaseConnection _baseConnection;
        public ProfileRepository(IEKtpReaderBackendDb con,
            IOptions<ConnectionStringConfig> options, IOptions<ErrorMessageConfig> options2) : base(con)
        {
            _baseConnection = new SqlServerConnection(options.Value.dbConnection1, options2);
        }

        #region GET
        public Task<ProfileByNik> GetProfileByNik(string nik)
        {
            const string proc = "[ProcViewDataGetDataByNIK]";

            return Db.WithConnectionAsync(c => c.QueryFirstOrDefaultAsync<ProfileByNik>(proc, new { NIK = new DbString { Value = nik, Length = 50 } },
                commandType: CommandType.StoredProcedure));
        }

        public Task<ProfileByNik> GetProfileByNikNoMatching(string nik)
        {
            const string proc = "[ProcViewDataGetDataByNIKISOTemp]";

            return Db.WithConnectionAsync(c => c.QueryFirstOrDefaultAsync<ProfileByNik>(proc, new { NIK = new DbString { Value = nik, Length = 50 } },
                commandType: CommandType.StoredProcedure));
        }

        public Task<ProfileByNik> GetProfileByNikNoMatchingNew(string nik,int id)
        {
            const string proc = "[ProcViewDataGetDataByNIKISOTempNew]";

            return Db.WithConnectionAsync(c => c.QueryFirstOrDefaultAsync<ProfileByNik>(proc, new { NIK = new DbString { Value = nik, Length = 50 }, id = id },
                commandType: CommandType.StoredProcedure));
        }

        public async Task<string> GetNikNoMatchingByNoPengajuan(string NoPengajuan)
        {
            const string query = "select B.NIK from [dbo].[Tbl_Inbox_Enrollment_Temp] A LEFT JOIN [dbo].[Tbl_DataKTP_Demografis_Temp] B ON A.DemografisTempId = B.Id where A.NoPengajuan = @NoPengajuan";

            return await Db.WithConnectionAsync(db => db.QueryFirstOrDefaultAsync<string>(query, new { NoPengajuan }));
        }

        public async Task<string> GetNikNoMatchingByIdPengajuan(int id)
        {
            const string query = "select B.NIK from [dbo].[Tbl_Inbox_Enrollment_Temp] A LEFT JOIN [dbo].[Tbl_DataKTP_Demografis_Temp] B ON A.DemografisTempId = B.Id where A.Id = @id";

            return await Db.WithConnectionAsync(db => db.QueryFirstOrDefaultAsync<string>(query, new { id }));
        }

        public Task<ProfileByNik> GetProfileByNikISO(string nik)
        {
            const string proc = "[ProcViewDataGetDataByNIKISO]";

            return Db.WithConnectionAsync(c => c.QueryFirstOrDefaultAsync<ProfileByNik>(proc, new { NIK = new DbString { Value = nik, Length = 50 } },
                commandType: CommandType.StoredProcedure));
        }

        public Task<ProfileByNikOnlyFinger> GetProfileByNikISOBio(string nik)
        {
            const string proc = "[ProcViewDataGetDataByNIKISOBIO]";

            return Db.WithConnectionAsync(c => c.QueryFirstOrDefaultAsync<ProfileByNikOnlyFinger>(proc, new { NIK = new DbString { Value = nik, Length = 50 } },
                commandType: CommandType.StoredProcedure));
        }

        public Task<ProfileByNik> GetProfileByNikEmp(string nik)
        {
            const string proc = "[ProcViewDataGetDataByNIKEmp]";

            return Db.WithConnectionAsync(c => c.QueryFirstOrDefaultAsync<ProfileByNik>(proc, new { NIK = nik },
                commandType: CommandType.StoredProcedure));
        }

        public Task<ProfileByNik> GetProfileByCIF(string cif)
        {
            const string proc = "[ProcViewDataGetDataByCIF]";

            return Db.WithConnectionAsync(c => c.QueryFirstOrDefaultAsync<ProfileByNik>(proc, new { CIF = new DbString { Value = cif, Length = 70 } },
                commandType: CommandType.StoredProcedure));
        }

        public Task<ProfileByNik> GetProfileEmpByCIF(string cif)
        {
            const string proc = "[ProcViewDataGetDataEmpByCIF]";

            return Db.WithConnectionAsync(c => c.QueryFirstOrDefaultAsync<ProfileByNik>(proc, new { CIF = cif },
                commandType: CommandType.StoredProcedure));
        }

        public Task<Tbl_DataKTP_Demografis> GetDataDemografis(string nik)
        {
            return Db.WithConnectionAsync(c => c.SingleAsync<Tbl_DataKTP_Demografis>(x => x.NIK == nik && x.IsActive.Value));
        }

        public Task<DetailLoginThirdParty2> GetCS()
        {
            const string proc = "[ProcGetDataCS]";

            return Db.WithConnectionAsync(c => c.QueryFirstOrDefaultAsync<DetailLoginThirdParty2>(proc, new
            {
                NIK = new DbString { Value = "00000", Length = 50 },
            }, commandType: CommandType.StoredProcedure, commandTimeout: 6000));
        }

        public Task<DetailLogin> GetPenyelia(string npp, int unitId)
        {
            const string proc = "[ProcGetDataPenyelia]";

            return Db.WithConnectionAsync(c => c.QueryFirstOrDefaultAsync<DetailLogin>(proc, new
            {
                NIK = new DbString { Value = npp, Length = 50 },
                UnitId = unitId,
            }, commandType: CommandType.StoredProcedure, commandTimeout: 6000));
        }
        public Task<DetailLogin> GetPemimpin(string npp, int unitId)
        {
            const string proc = "[ProcGetDataPemimpin]";

            return Db.WithConnectionAsync(c => c.QueryFirstOrDefaultAsync<DetailLogin>(proc, new
            {
                NIK = new DbString { Value = npp, Length = 50 },
                UnitId = unitId,
            }, commandType: CommandType.StoredProcedure, commandTimeout: 6000));
        }

        public Task<Tbl_DataKTP_Demografis_Temp> GetDataDemografisTemp(string nik)
        {
            return Db.WithConnectionAsync(c => c.SingleAsync<Tbl_DataKTP_Demografis_Temp>(x => x.NIK == nik && x.IsActive == false && x.IsDeleted == false));
        }

        public Task<Tbl_DataKTP_Demografis_Temp> GetDataDemografisTempOnProgress(string npp)
        {
            const string proc = "[ProcGetDataDemoOnProgress]";

            return Db.WithConnectionAsync(c => c.QueryFirstOrDefaultAsync<Tbl_DataKTP_Demografis_Temp>(proc, new
            {
                NIK = new DbString { Value = npp, Length = 50 },
            }, commandType: CommandType.StoredProcedure, commandTimeout: 6000));
        }

        public Task<Tbl_DataKTP_Photo> GetPhotoKtp(string nik)
        {
            return Db.WithConnectionAsync(c => c.SingleAsync<Tbl_DataKTP_Photo>(x => x.Nik == nik && x.IsActive.Value && !x.IsDeleted.Value));
        }
        public Task<Tbl_DataKTP_Photo_Temp> GetPhotoKtpTemp(string nik)
        {
            return Db.WithConnectionAsync(c => c.SingleAsync<Tbl_DataKTP_Photo_Temp>(x => x.Nik == nik && x.IsActive == false && x.IsDeleted == false));
        }

        public Task<Tbl_DataKTP_Signature> GetPhotoSignature(string nik)
        {
            return Db.WithConnectionAsync(c => c.SingleAsync<Tbl_DataKTP_Signature>(x => x.Nik == nik && x.IsActive.Value && !x.IsDeleted.Value));
        }

        public Task<Tbl_DataKTP_Signature_Temp> GetPhotoSignatureTemp(string nik)
        {
            return Db.WithConnectionAsync(c => c.SingleAsync<Tbl_DataKTP_Signature_Temp>(x => x.Nik == nik && x.IsActive == false && x.IsDeleted == false));
        }

        public Task<Tbl_DataKTP_PhotoCam> GetPhotoCam(string nik)
        {
            return Db.WithConnectionAsync(c => c.SingleAsync<Tbl_DataKTP_PhotoCam>(x => x.Nik == nik && x.IsActive.Value && !x.IsDeleted.Value));
        }

        public Task<Tbl_DataKTP_PhotoCam_Temp> GetPhotoCamTemp(string nik)
        {
            return Db.WithConnectionAsync(c => c.SingleAsync<Tbl_DataKTP_PhotoCam_Temp>(x => x.Nik == nik && x.IsActive == false && x.IsDeleted == false));
        }

        public Task<Tbl_DataKTP_Finger> GetPhotoFinger(string nik, string fingerType)
        {
            return Db.WithConnectionAsync(c => c.SingleAsync<Tbl_DataKTP_Finger>(x => x.Nik == nik && x.IsActive.Value && !x.IsDeleted.Value
            && x.TypeFinger == fingerType));
        }

        public Task<List<Tbl_DataKTP_Finger>> GetPhotoFingerExisting(string nik)
        {
            return Db.WithConnectionAsync(c => c.SelectAsync<Tbl_DataKTP_Finger>(x => x.Nik == nik && x.IsActive.Value && !x.IsDeleted.Value));
        }

        public Task<Tbl_DataKTP_Finger_Temp> GetPhotoFingerTemp(string nik, string fingerType)
        {
            return Db.WithConnectionAsync(c => c.SingleAsync<Tbl_DataKTP_Finger_Temp>(x => x.Nik == nik && x.IsActive == false && x.IsDeleted == false
            && x.TypeFinger == fingerType));
        }

        public Task<Tbl_DataKTP_Finger_Employee> GetPhotoFingerEmployee(string nik, string fingerType)
        {
            return Db.WithConnectionAsync(c => c.SingleAsync<Tbl_DataKTP_Finger_Employee>(x => x.Nik == nik && x.IsActive.Value && !x.IsDeleted.Value
            && x.TypeFinger == fingerType));
        }

        public Task<List<Tbl_DataKTP_Finger_Employee>> GetPhotoFingerEmployeeExisting(string nik)
        {
            return Db.WithConnectionAsync(c => c.SelectAsync<Tbl_DataKTP_Finger_Employee>(x => x.Nik == nik && x.IsActive.Value && !x.IsDeleted.Value));
        }

        public Task<Tbl_DataKTP_Finger_Employee_Temp> GetPhotoFingerEmployeeTemp(string nik, string fingerType)
        {
            return Db.WithConnectionAsync(c => c.SingleAsync<Tbl_DataKTP_Finger_Employee_Temp>(x => x.Nik == nik && x.IsActive == false && x.IsDeleted == false
            && x.TypeFinger == fingerType));
        }

        public Task<Tbl_DataKTP_Finger> GetPhotoFingerLike(string nik, string fingerType)
        {
            //return Db.WithConnectionAsync(c => c.SingleAsync<Tbl_DataKTP_Finger>(x => x.Nik == nik && x.IsActive.Value && !x.IsDeleted.Value
            //&& x.TypeFinger == fingerType));

            const string query = "select * from [dbo].[Tbl_DataKTP_Finger] where NIK = @nik AND IsActive = 1 AND IsDeleted = 0 AND TypeFinger LIKE '%'+@fingerType+'%'";

            return Db.WithConnectionAsync(db => db.QueryFirstOrDefaultAsync<Tbl_DataKTP_Finger>(query, new { nik, fingerType }));
        }

        public Task<Tbl_DataKTP_Finger_Employee> GetPhotoFingerEmployeeLike(string nik, string fingerType)
        {
            //return Db.WithConnectionAsync(c => c.SingleAsync<Tbl_DataKTP_Finger>(x => x.Nik == nik && x.IsActive.Value && !x.IsDeleted.Value
            //&& x.TypeFinger == fingerType));

            const string query = "select * from [dbo].[Tbl_DataKTP_Finger_Employee] where NIK = @nik AND IsActive = 1 AND IsDeleted = 0 AND TypeFinger LIKE '%'+@fingerType+'%'";

            return Db.WithConnectionAsync(db => db.QueryFirstOrDefaultAsync<Tbl_DataKTP_Finger_Employee>(query, new { nik, fingerType }));
        }
        #endregion

        #region UPDATE
        public Task UpdateDataDemografis(Tbl_DataKTP_Demografis data)
        {
            return UpdateAsync(data);
        }

        public Task UpdateDataFinger(Tbl_DataKTP_Finger data)
        {
            return UpdateAsync(data);
        }
        public Task UpdateDataFingerEmployee(Tbl_DataKTP_Finger_Employee data)
        {
            return UpdateAsync(data);
        }

        public Task UpdateDataPhoto(Tbl_DataKTP_Photo data)
        {
            return UpdateAsync(data);
        }

        public Task UpdateDataPhotoCam(Tbl_DataKTP_PhotoCam data)
        {
            return UpdateAsync(data);
        }

        public Task UpdateDataSignature(Tbl_DataKTP_Signature data)
        {
            return UpdateAsync(data);
        }
        #endregion

        #region INSERT
        public long InsertDemografiLog(Tbl_DataKTP_Demografis_Log log)
        {
            //return _baseConnection.WithConnection(db => db.Insert(log, true));
            return InsertIncrement(log);
        }
        public long InsertFingerLog(Tbl_DataKTP_Finger_Log log)
        {
            return InsertIncrement(log);
        }
        public long InsertFingerEmployeeLog(Tbl_DataKTP_Finger_Employee_Log log)
        {
            return InsertIncrement(log);
        }
        public long InsertPhotoLog(Tbl_DataKTP_Photo_Log log)
        {
            return InsertIncrement(log);
        }
        public long InsertPhotoCamLog(Tbl_DataKTP_PhotoCam_Log log)
        {
            return InsertIncrement(log);
        }
        public long InsertSignatureLog(Tbl_DataKTP_Signature_Log log)
        {
            return InsertIncrement(log);
        }
        public long InsertCIFLog(Tbl_DataKTP_CIF log)
        {
            return InsertIncrement(log);
        }
        public long InsertHistoryPengajuan(Tbl_LogHistoryPengajuan tbl_LogHistoryPengajuan)
        {
            return InsertIncrement(tbl_LogHistoryPengajuan);
        }
        #endregion
    }
}
