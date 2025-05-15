using Dapper;
using Ekr.Core.Entities;
using Ekr.Core.Entities.DataEnrollment;
using Ekr.Dapper.Connection.Contracts.Sql;
using Ekr.Repository.Contracts.DataEnrollment;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Ekr.Repository.DataEnrollment
{
    public class EnrollTempRepository : BaseRepository, IEnrollTempRepository
    {
        public EnrollTempRepository(IEKtpReaderBackendDb con) : base(con) { }

        #region GET
        public async Task<GridResponse<MonitoringEnroll>> GetDataEnroolTemp(DataEnrollTempFilter filter)
        {
            const string proc = "[ProcDataEnrollTempData]";

            var res = await Db.WithConnectionAsync(c => c.QueryAsync<MonitoringEnroll>(proc, new
            {
                SColumn = string.IsNullOrWhiteSpace(filter.SortColumn)?"Id": filter.SortColumn,
                SColumnValue = string.IsNullOrWhiteSpace(filter.SortColumnDir)?"desc": filter.SortColumnDir,
                filter.Nama,
                filter.NIK,
                Page = filter.PageNumber,
                Rows = filter.PageSize,
                filter.LoginPegawaiId,
                filter.LoginRoleId,
                filter.LoginUnitId
            }, commandType: CommandType.StoredProcedure))
                .ConfigureAwait(false);

            const string procCount = "[ProcDataEnrollTempTotal]";

            var count = await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(procCount, new
            {
                filter.Nama,
                filter.NIK,
                filter.LoginPegawaiId,
                filter.LoginRoleId,
                filter.LoginUnitId
            }, commandType: CommandType.StoredProcedure))
                .ConfigureAwait(false);

            return new GridResponse<MonitoringEnroll>
            {
                Count = count,
                Data = res
            };
        }

        public async Task<GridResponse<MonitoringEnroll>> GetDataEnrool2Temp(DataEnrollTemp2Filter filter)
        {
            const string proc = "[ProcDataEnrollTempData]";

            var res = await Db.WithConnectionAsync(c => c.QueryAsync<MonitoringEnroll>(proc, new
            {
                SColumn = string.IsNullOrWhiteSpace(filter.SortColumn) ? "Id" : filter.SortColumn,
                SColumnValue = string.IsNullOrWhiteSpace(filter.SortColumnDir) ? "desc" : filter.SortColumnDir,
                filter.Nama,
                filter.NIK,
                CIF = filter.CIF,
                Page = filter.PageNumber,
                Rows = filter.PageSize,
                filter.LoginPegawaiId,
                filter.LoginRoleId,
                filter.LoginUnitId
            }, commandType: CommandType.StoredProcedure))
                .ConfigureAwait(false);

            const string procCount = "[ProcDataEnrollTempTotal]";

            var count = await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(procCount, new
            {
                filter.Nama,
                filter.NIK,
                CIF = filter.CIF,
                filter.LoginPegawaiId,
                filter.LoginRoleId,
                filter.LoginUnitId
            }, commandType: CommandType.StoredProcedure))
                .ConfigureAwait(false);

            return new GridResponse<MonitoringEnroll>
            {
                Count = count,
                Data = res
            };
        }

        public async Task<IEnumerable<ExportMonitoringEnroll>> ExportDataEnrool2Temp(ExportDataEnrollTemp2Filter filter)
        {
            const string proc = "[ProcExportDataEnrollTempData]";

            var res = await Db.WithConnectionAsync(c => c.QueryAsync<ExportMonitoringEnroll>(proc, new
            {
                SColumn = string.IsNullOrWhiteSpace(filter.SortColumn) ? "Id" : filter.SortColumn,
                SColumnValue = string.IsNullOrWhiteSpace(filter.SortColumnDir) ? "desc" : filter.SortColumnDir,
                filter.Nama,
                filter.NIK,
                filter.LoginPegawaiId,
                filter.LoginRoleId,
                filter.LoginUnitId
            }, commandType: CommandType.StoredProcedure))
                .ConfigureAwait(false);

            return res;
        }
        #endregion

    }
}
