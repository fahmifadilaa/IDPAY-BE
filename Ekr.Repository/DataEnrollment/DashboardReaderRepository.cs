using Dapper;
using Ekr.Core.Entities.DataEnrollment.ViewModel;
using Ekr.Dapper.Connection.Contracts.Sql;
using Ekr.Repository.Contracts.DataEnrollment;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ekr.Repository.DataEnrollment
{
    public class DashboardReaderRepository : BaseRepository, IDashboardReaderRepository
    {
        public DashboardReaderRepository(IEKtpReaderBackendDb con) : base(con) { }

        public async Task<IEnumerable<MonitoringReaderDataVM>> GetMonitoringReaderChart(string UnitIds)
        {
            const string sp = "[ProcMonitoringAlatChart]";
            var val = new
            {
                UnitIds = UnitIds
            };

            var data = await Db.WithConnectionAsync(db => db.QueryAsync<MonitoringReaderDataVM>(sp, val, commandType: CommandType.StoredProcedure));

            return data;
        }
        public async Task<IEnumerable<PresentaseReaderDigunakanVM>> GetPresentaseAlatDigunakanChart(string UnitIds)
        {
            const string sp = "[ProcPresentaseAlatChart]";
            var val = new
            {
                UnitIds = UnitIds
            };

            var data = await Db.WithConnectionAsync(db => db.QueryAsync<PresentaseReaderDigunakanVM>(sp, val, commandType: CommandType.StoredProcedure));

            return data;
        }
    }
}
