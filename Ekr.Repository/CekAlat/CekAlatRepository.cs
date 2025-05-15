using Dapper;
using Ekr.Core.Entities;
using Ekr.Core.Entities.CekAlat;
using Ekr.Dapper.Connection.Contracts.Sql;
using Ekr.Repository.Contracts.DataEnrollment;
using System;
using System.Data;
using System.Threading.Tasks;

namespace Ekr.Repository.CekAlat
{
    public class CekAlatRepository : BaseRepository, ICekAlatRepository
    {
        public CekAlatRepository(IEKtpReaderBackendDb con) : base(con) { }

        #region Get
        public async Task<GridResponse<DataDashboard1_ViewModels>> GridGetDashboard1(CekAlatFilter req)
        {
            const string sp = "[ProcDashboardReaderInfoDetailsBySN]";
            var values = new
            {
                Serial_Number = req.SerialNumber
            };

            var data = Db.WithConnectionAsync(db =>
            db.QueryAsync<DataDashboard1_ViewModels>(sp, values, commandType: CommandType.StoredProcedure));

            await Task.WhenAll(data)
                .ConfigureAwait(false);

            return new GridResponse<DataDashboard1_ViewModels>
            {
                Count = 1,
                Data = data.Result
            };
        }

        public async Task<GridResponse<DataDashboard2_ViewModels>> GridGetDashboard2(CekAlatFilter req)
        {
            const string sp = "[ProcDashboardReaderInfoDetailsAlatBySN]";
            var values = new
            {
                Serial_Number = req.SerialNumber
            };

            var data = Db.WithConnectionAsync(db =>
            db.QueryAsync<DataDashboard2_ViewModels>(sp, values, commandType: CommandType.StoredProcedure));

            await Task.WhenAll(data)
                .ConfigureAwait(false);

            return new GridResponse<DataDashboard2_ViewModels>
            {
                Count = 1,
                Data = data.Result
            };
        }

        public async Task<GridResponse<DataDashboard3_ViewModels>> ChartWeekly(CekAlatFilter req)
        {
            const string sp = "[ProcDashboardReaderGetAllLogReader]";
            var values = new
            {
                Serial_Number = req.SerialNumber,
                Date = DateTime.Now.ToString("yyyy-MM-dd")
            };

            var data = Db.WithConnectionAsync(db =>
            db.QueryAsync<DataDashboard3_ViewModels>(sp, values, commandType: CommandType.StoredProcedure));

            await Task.WhenAll(data)
                .ConfigureAwait(false);

            return new GridResponse<DataDashboard3_ViewModels>
            {
                Count = 1,
                Data = data.Result
            };
        }
        #endregion
    }
}
