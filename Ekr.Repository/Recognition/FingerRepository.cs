using Dapper;
using Ekr.Core.Entities.Recognition;
using Ekr.Dapper.Connection.Contracts.Sql;
using Ekr.Repository.Contracts.Recognition;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Ekr.Repository.Recognition
{
    public class FingerRepository : BaseRepository, IFingerRepository
    {
        public FingerRepository(IEKtpReaderBackendDb con) : base(con) { }

        #region GET
        public Task<IEnumerable<FingerByNik>> GetFingersEnrolled(string nik)
        {
            const string proc = "[ProcAPIGetDataFingerNIK]";

            return Db.WithConnectionAsync(c => c.QueryAsync<FingerByNik>(proc, new { NIK = nik },
                commandType: CommandType.StoredProcedure));
        }

        public Task<IEnumerable<FingerByNik>> GetFingersEnrolledByNikIsoDB(string nik)
        {
            const string proc = "[ProcAPIGetDataFingerNIKIsoDB]";

            return Db.WithConnectionAsync(c => c.QueryAsync<FingerByNik>(proc, new { NIK = new DbString { Value = nik, Length = 50 } },
                commandType: CommandType.StoredProcedure));
        }

        public Task<IEnumerable<FingerByNik>> GetFingersEnrolledByNikIsoFile(string nik)
        {
            const string proc = "[ProcAPIGetDataFingerNIKIsoFile]";

            return Db.WithConnectionAsync(c => c.QueryAsync<FingerByNik>(proc, new { NIK = new DbString { Value = nik, Length = 50 } },
                commandType: CommandType.StoredProcedure));
        }

        public Task<IEnumerable<FingerByNik>> GetFingersEnrolledNpp(string npp)
        {
            const string proc = "[ProcAPIGetDataFingerNpp]";

            return Db.WithConnectionAsync(c => c.QueryAsync<FingerByNik>(proc, new { Npp = npp },
                commandType: CommandType.StoredProcedure));
        }

        public Task<IEnumerable<FingerByNik>> GetFingersEnrolledByNppIsoFile(string npp)
        {
            const string proc = "[ProcAPIGetDataFingerNppIsoFile]";

            return Db.WithConnectionAsync(c => c.QueryAsync<FingerByNik>(proc, new { Npp = new DbString { Value = npp, Length = 50} },
                commandType: CommandType.StoredProcedure));
        }

        public Task<IEnumerable<FingerByNik>> GetFingersEnrolledByNppIsoDB(string npp)
        {
            const string proc = "[ProcAPIGetDataFingerNppIsoDB]";

            return Db.WithConnectionAsync(c => c.QueryAsync<FingerByNik>(proc,
                new { Npp = new DbString { Value = npp, Length = 10 } },
                commandType: CommandType.StoredProcedure));
        }

        public Task<IEnumerable<FingerISOByNik>> GetFingersISOEnrolledNik(string nik)
        {
            const string proc = "[ProcAPIGetDataFingerISONik]";

            return Db.WithConnectionAsync(c => c.QueryAsync<FingerISOByNik>(proc, new { Nik = nik },
                commandType: CommandType.StoredProcedure));
        }

        public Task<IEnumerable<FingerByNik>> GetFingersEmpEnrolled(string nik)
        {
            const string proc = "[ProcAPIGetDataFingerEmpNIK]";

            return Db.WithConnectionAsync(c => c.QueryAsync<FingerByNik>(proc, new { NIK = nik },
                commandType: CommandType.StoredProcedure));
        }
        public Task<IEnumerable<FingerByNik>> GetFingersEnrolledByCIF(string cif)
        {
            const string proc = "[ProcAPIGetDataFingerByCIF]";

            return Db.WithConnectionAsync(c => c.QueryAsync<FingerByNik>(proc, new { cif = cif },
                commandType: CommandType.StoredProcedure));
        }

        public Task<IEnumerable<FingerByNik>> GetFingersEnrolledByCIFIsoFile(string cif)
        {
            const string proc = "[ProcAPIGetDataFingerByCIFIsoFile]";

            return Db.WithConnectionAsync(c => c.QueryAsync<FingerByNik>(proc, new { cif = new DbString { Value = cif, Length = 50 } },
                commandType: CommandType.StoredProcedure));
        }

        public Task<IEnumerable<FingerByNik>> GetFingersEnrolledByCIFIsoDB(string cif)
        {
            const string proc = "[ProcAPIGetDataFingerByCIFIsoDB]";

            return Db.WithConnectionAsync(c => c.QueryAsync<FingerByNik>(proc, new { cif = new DbString { Value = cif, Length = 50 } },
                commandType: CommandType.StoredProcedure));
        }

        public Task<FingerByType> GetFingerByType(string nik, string type)
        {
            const string proc = "[ProcUtilityGetDataFinger]";

            return Db.WithConnectionAsync(c => c.QueryFirstOrDefaultAsync<FingerByType>(proc, new { NIK = nik, TypeFinger = type },
                commandType: CommandType.StoredProcedure));
        }

        public Task<FingerByType> GetFingerByTypeEmp(string nik, string type)
        {
            const string query = "DECLARE " +
                "@Query varchar(max)," +
                "@VirtualPath nvarchar(max)," +
                "@PathFinger nvarchar(max) " +
                "SELECT @VirtualPath = [Value] from Tbl_SystemParameter where KataKunci = 'VirtualPath' " +
                "SELECT @PathFinger = [Value] from Tbl_SystemParameter where KataKunci = 'FolderFinger' " +
                "SELECT [Id]      " +
                ",[NIK]      " +
                ",[TypeFinger]      " +
                ",CONCAT(@VirtualPath,'/',@PathFinger,'/',Nik,'/',[FileName]) as [Url]      " +
                ",[FileJari]  " +
                "FROM [dbo].[Tbl_DataKTP_Finger_Employee]  " +
                "Where NIK = @Nik and [TypeFinger] = @TypeFinger";

            return Db.WithConnectionAsync(c => c.QueryFirstOrDefaultAsync<FingerByType>(query, new { Nik = nik, TypeFinger = type }));
        }

        public Task<FingerByType> GetFingerByTypeEmpIso(string nik, string type)
        {
            const string query = "DECLARE " +
                "@Query varchar(max)," +
                "@VirtualPath nvarchar(max)," +
                "@PathFinger nvarchar(max), " +
                "@Npp nvarchar(50) " +
                "select @Npp = Npp from Tbl_MappingNIK_Pegawai where NIK = @Nik; " +
                "SELECT @VirtualPath = [Value] from Tbl_SystemParameter where KataKunci = 'VirtualPath' " +
                "SELECT @PathFinger = [Value] from Tbl_SystemParameter where KataKunci = 'FolderFinger' " +
                "if @Npp != '' " +
                "BEGIN " +
                "SELECT [Id]      " +
                ",[NIK]      " +
                ",[TypeFinger]      " +
                ",CONCAT(@VirtualPath,'/',@PathFinger,'/',Nik,'/',[FileNameISO]) as [Url]      " +
                ",[FileJariISO] as FileJari " +
                "FROM [dbo].[Tbl_DataKTP_Finger_Employee]  " +
                "Where NIK = @Nik and [TypeFinger] = @TypeFinger and FileJariISO is not null " +
                "END " +
                "ELSE " +
                "BEGIN " +
                "SELECT [Id] " +
                ",[NIK] " +
                ",[TypeFinger] " +
                ",CONCAT(@VirtualPath,'/',@PathFinger,'/',Nik,'/',FileNameISO) as [Url] " +
                ",[FileJariISO] as FileJari " +
                "FROM [dbo].[Tbl_DataKTP_Finger_Employee] " +
                "Where NIK = @Nik and [TypeFinger] = @TypeFinger and FileJariISO is not null " +
                "END";

            return Db.WithConnectionAsync(c => c.QueryFirstOrDefaultAsync<FingerByType>(query, new { Nik = nik, TypeFinger = type }));
        }

        public Task<FingerByTypeISO> GetFingerByTypeEmpISO(string nik, string type)
        {
            const string proc = "[ProcUtilityGetDataFingerISO]";

            return Db.WithConnectionAsync(c => c.QueryFirstOrDefaultAsync<FingerByTypeISO>(proc, new { NIK = nik, TypeFinger = type },
                commandType: CommandType.StoredProcedure));
        }
        #endregion
    }
}
