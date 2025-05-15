using Dapper;
using Ekr.Core.Entities;
using Ekr.Core.Entities.DataEnrollment;
using Ekr.Core.Entities.DataEnrollment.ViewModel;
using Ekr.Dapper.Connection.Contracts.Sql;
using Ekr.Repository.Contracts.DataEnrollment;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Ekr.Repository.DataEnrollment
{
    public class DashboardDataEnrollRepository : BaseRepository, IDashboardDataEnrollRepository
    {
        public DashboardDataEnrollRepository(IEKtpReaderBackendDb con) : base(con) { }

        public async Task<IEnumerable<JobChartDataVM>> GetJobChart(UnitIdsFilterVM req)
        {
            const string sp = "[ProcJobChartNew]";

            //var data = await Db.WithConnectionAsync(db => db.QueryAsync<JobChartDataVM>(sp, commandType: CommandType.StoredProcedure));
            var data = await Db.WithConnectionAsync(db => db.QueryAsync<JobChartDataVM>(sp, new
            {
                UnitIds = req.UnitIds,
                Jenis = req.Jenis
            }, commandType: CommandType.StoredProcedure, commandTimeout: 2000000));

            return data;
        }
        public async Task<IEnumerable<JobChartDataVM>> GetJobChart2(UnitIdsFilterVM2 req)
        {
            const string sp = "[ProcJobChartNew]";

            //var data = await Db.WithConnectionAsync(db => db.QueryAsync<JobChartDataVM>(sp, commandType: CommandType.StoredProcedure));
            var data = await Db.WithConnectionAsync(db => db.QueryAsync<JobChartDataVM>(sp, new
            {
                UnitIds = req.UnitIds,
                Jenis = req.Jenis,
                Tipe = req.Tipe
            }, commandType: CommandType.StoredProcedure, commandTimeout: 12000));

            return data;
        }
        public async Task<IEnumerable<TypeEnrollmentVM>> GetTypeEnrollmentChart(UnitIdsFilterVM req)
        {
            const string sp = "[sp_EnrollmentMenuStatistics]";

            //var data = await Db.WithConnectionAsync(db => db.QueryAsync<JobChartDataVM>(sp, commandType: CommandType.StoredProcedure));
            var data = await Db.WithConnectionAsync(db => db.QueryAsync<TypeEnrollmentVM>(sp, new
            {
                UnitIds = req.UnitIds
            }, commandType: CommandType.StoredProcedure, commandTimeout: 2000000));

            return data;
        }
        public async Task<IEnumerable<TypeEnrollmentVM>> GetTypeEnrollmentChart2(UnitIdsFilterVM2 req)
        {
            const string sp = "[sp_EnrollmentMenuStatistics]";

            //var data = await Db.WithConnectionAsync(db => db.QueryAsync<JobChartDataVM>(sp, commandType: CommandType.StoredProcedure));
            var data = await Db.WithConnectionAsync(db => db.QueryAsync<TypeEnrollmentVM>(sp, new
            {
                UnitIds = req.UnitIds,
                Jenis = req.Jenis,
                Tipe = req.Tipe
            }, commandType: CommandType.StoredProcedure, commandTimeout: 12000));

            return data;
        }

        public async Task<IEnumerable<ChannelEnrollmentVM>> GetChannelEnrollmentChart(UnitIdsFilterVM req)
        {
            const string sp = "[sp_EnrollmentMenuStatisticsChannelNew]";

            //var data = await Db.WithConnectionAsync(db => db.QueryAsync<JobChartDataVM>(sp, commandType: CommandType.StoredProcedure));
            var data = await Db.WithConnectionAsync(db => db.QueryAsync<ChannelEnrollmentVM>(sp, new
            {
                UnitIds = req.UnitIds,
                Jenis = req.Jenis
            }, commandType: CommandType.StoredProcedure, commandTimeout: 2000000));

            return data;
        }

        public async Task<IEnumerable<ChannelEnrollmentVM>> GetChannelEnrollmentChart2(UnitIdsFilterVM2 req)
        {
            const string sp = "[sp_EnrollmentMenuStatisticsChannelNew]";

            //var data = await Db.WithConnectionAsync(db => db.QueryAsync<JobChartDataVM>(sp, commandType: CommandType.StoredProcedure));
            var data = await Db.WithConnectionAsync(db => db.QueryAsync<ChannelEnrollmentVM>(sp, new
            {
                UnitIds = req.UnitIds,
                Jenis = req.Jenis,
                Tipe = req.Tipe
            }, commandType: CommandType.StoredProcedure, commandTimeout: 12000));

            return data;
        }

        public async Task<IEnumerable<StatusEnrollmentVM>> GetStatusEnrollmentChart(UnitIdsFilterVM req)
        {
            const string sp = "[sp_EnrollmentMenuStatisticsStatusNew]";

            //var data = await Db.WithConnectionAsync(db => db.QueryAsync<JobChartDataVM>(sp, commandType: CommandType.StoredProcedure));
            var data = await Db.WithConnectionAsync(db => db.QueryAsync<StatusEnrollmentVM>(sp, new
            {
                UnitIds = req.UnitIds,
                Jenis = req.Jenis
            }, commandType: CommandType.StoredProcedure, commandTimeout: 2000000));

            return data;
        }

        public async Task<IEnumerable<StatusEnrollmentVM>> GetStatusEnrollmentChart2(UnitIdsFilterVM2 req)
        {
            const string sp = "[sp_EnrollmentMenuStatisticsStatusNew]";

            //var data = await Db.WithConnectionAsync(db => db.QueryAsync<JobChartDataVM>(sp, commandType: CommandType.StoredProcedure));
            var data = await Db.WithConnectionAsync(db => db.QueryAsync<StatusEnrollmentVM>(sp, new
            {
                UnitIds = req.UnitIds,
                Jenis = req.Jenis,
                Tipe = req.Tipe
            }, commandType: CommandType.StoredProcedure, commandTimeout: 12000));

            return data;
        }

        public async Task<IEnumerable<ReligionChartDataVM>> GetReligionChart(UnitIdsFilterVM req)
        {
            const string sp = "[ProcReligionChartNew]";

            //var data = await Db.WithConnectionAsync(db => db.QueryAsync<ReligionChartDataVM>(sp, commandType: CommandType.StoredProcedure));
            var data = await Db.WithConnectionAsync(db => db.QueryAsync<ReligionChartDataVM>(sp, new
            {
                UnitIds = req.UnitIds,
                Jenis = req.Jenis
            }, commandType: CommandType.StoredProcedure, commandTimeout: 2000000));

            return data;
        }

        public async Task<IEnumerable<ReligionChartDataVM>> GetReligionChart2(UnitIdsFilterVM2 req)
        {
            const string sp = "[ProcReligionChartNew]";

            //var data = await Db.WithConnectionAsync(db => db.QueryAsync<ReligionChartDataVM>(sp, commandType: CommandType.StoredProcedure));
            var data = await Db.WithConnectionAsync(db => db.QueryAsync<ReligionChartDataVM>(sp, new
            {
                UnitIds = req.UnitIds,
                Jenis = req.Jenis,
                Tipe = req.Tipe
            }, commandType: CommandType.StoredProcedure, commandTimeout: 12000));

            return data;
        }

        public async Task<IEnumerable<BornGenerationChartDataVM>> GetBornGenerationChart(UnitIdsFilterVM req)
        {
            const string sp = "[ProcBornGenerationChartNew]";

            //var data = await Db.WithConnectionAsync(db => db.QueryAsync<BornGenerationChartDataVM>(sp, commandType: CommandType.StoredProcedure));
            var data = await Db.WithConnectionAsync(db => db.QueryAsync<BornGenerationChartDataVM>(sp, new
            {
                UnitIds = req.UnitIds,
                Jenis = req.Jenis
            }, commandType: CommandType.StoredProcedure, commandTimeout: 12000));

            return data;
        }

        public async Task<IEnumerable<BornGenerationChartDataVM>> GetBornGenerationChart2(UnitIdsFilterVM2 req)
        {
            const string sp = "[ProcBornGenerationChartNew]";

            //var data = await Db.WithConnectionAsync(db => db.QueryAsync<BornGenerationChartDataVM>(sp, commandType: CommandType.StoredProcedure));
            var data = await Db.WithConnectionAsync(db => db.QueryAsync<BornGenerationChartDataVM>(sp, new
            {
                UnitIds = req.UnitIds,
                Jenis = req.Jenis,
                Tipe = req.Tipe
            }, commandType: CommandType.StoredProcedure, commandTimeout: 2000000));

            return data;
        }

        public async Task<IEnumerable<AgeSegmentationChartDataVM>> GetAgeSegmentationChart(UnitIdsFilterVM req)
        {
            const string sp = "[ProcAgeSegmentationChartNew]";

            //var data = await Db.WithConnectionAsync(db => db.QueryAsync<AgeSegmentationChartDataVM>(sp, commandType: CommandType.StoredProcedure));
            var data = await Db.WithConnectionAsync(db => db.QueryAsync<AgeSegmentationChartDataVM>(sp, new
            {
                UnitIds = req.UnitIds,
                Jenis = req.Jenis
                //}, commandType: CommandType.StoredProcedure, commandTimeout: 2200000));
            }, commandType: CommandType.StoredProcedure, commandTimeout: 2200000));

            return data;
        }

        public async Task<IEnumerable<AgeSegmentationChartDataVM>> GetAgeSegmentationChart2(UnitIdsFilterVM2 req)
        {
            const string sp = "[ProcAgeSegmentationChartNew]";

            //var data = await Db.WithConnectionAsync(db => db.QueryAsync<AgeSegmentationChartDataVM>(sp, commandType: CommandType.StoredProcedure));
            var data = await Db.WithConnectionAsync(db => db.QueryAsync<AgeSegmentationChartDataVM>(sp, new
            {
                UnitIds = req.UnitIds,
                Jenis = req.Jenis,
                Tipe = req.Tipe
            }, commandType: CommandType.StoredProcedure, commandTimeout: 12000));

            return data;
        }

        public async Task<GridResponse<MonitoringEnroll>> GetDBEnroll(DataEnrollTempFilter filter)
        {
            const string proc = "[ProcMonitoringEnrollDataAllNewV2]";

            var val = new
            {
                SColumn = string.IsNullOrWhiteSpace(filter.SortColumn) ? "Id" : filter.SortColumn,
                SColumnValue = string.IsNullOrWhiteSpace(filter.SortColumnDir) ? "desc" : filter.SortColumnDir,
                Nama = string.IsNullOrWhiteSpace(filter.Nama) ? "" : filter.Nama,
                NIK = string.IsNullOrWhiteSpace(filter.NIK) ? "" : filter.NIK,
                Page = filter.PageNumber,
                Rows = filter.PageSize,
                filter.LoginPegawaiId,
                filter.LoginRoleId,
                filter.LoginUnitId,
                filter.UnitIds,
                Jenis = filter.Jenis

            };

            var res = await Db.WithConnectionAsync(c => c.QueryAsync<MonitoringEnroll>(proc, val, commandType: CommandType.StoredProcedure))
                .ConfigureAwait(false);

            const string procCount = "[ProcMonitoringEnrollTotalAllNewV2]";

            var count = await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(procCount, new
            {
                Nama = string.IsNullOrWhiteSpace(filter.Nama) ? "" : filter.Nama,
                NIK = string.IsNullOrWhiteSpace(filter.NIK) ? "" : filter.NIK,
                filter.LoginPegawaiId,
                filter.LoginRoleId,
                filter.LoginUnitId,
                filter.UnitIds, 
                Jenis = filter.Jenis
            }, commandType: CommandType.StoredProcedure, commandTimeout: 2000000))
                .ConfigureAwait(false);

            return new GridResponse<MonitoringEnroll>
            {
                Count = count,
                Data = res
            };
        }

        public async Task<GridResponse<EnrollPerUnitVM>> GetEnrollPerUnit(EnrollPerUnitFilterVM req)
        {
            const string sp = "ProcEnrollPerUnit";
            var values = new
            {
                req.Name,
                req.Type,
                SColumn = req.SortColumn,
                SColumnValue = req.SortColumnDir,
                Page =  req.PageNumber,
                Rows = req.PageSize
            };
            var data = Db.WithConnectionAsync(db => db.QueryAsync<EnrollPerUnitVM>(sp, values, commandType: CommandType.StoredProcedure));

            const string spCount = "[ProcEnrollPerUnitNum]";
            var valuesCount = new
            {
                req.Name,
                req.Type
            };
            var count = Db.WithConnectionAsync(db => db.ExecuteScalarAsync<int>(spCount, valuesCount, commandType: CommandType.StoredProcedure));

            await Task.WhenAll(data, count).ConfigureAwait(false);

            return new GridResponse<EnrollPerUnitVM>
            {
                Count = count.Result,
                Data = data.Result
            };
        }

        public async Task<EnrollKTPVM> DetailData(EnrollKTPFIlterVM req)
        {
            const string sp = "[ProcViewDataGetDataByNIK]";

            var data = await Db.WithConnectionAsync(db => db.QueryFirstOrDefaultAsync<EnrollKTPVM>(sp, new {
                NIK = req.nik
            } ,commandType: CommandType.StoredProcedure));

            return data;
        }

        public async Task<GridResponse<DahboardEnrollmentPG>> DashboardEnrollList(DahboardEnrollmentPGFilterVM req)
        {
            const string sp = "ProcDashboardEnrollList";
            var values = new
            {
                Name = req.Nama,
                req.Provinsi,
                req.UnitCode,
                req.Role,
                SColumn = req.SortColumn,
                SColumnValue = req.SortColumnDir,
                Page = req.PageNumber,
                Rows = req.PageSize
            };
            var data = Db.WithConnectionAsync(db => db.QueryAsync<DahboardEnrollmentPG>(sp, values, commandType: CommandType.StoredProcedure));

            const string spCount = "[ProcDashboardEnrollListCount]";
            var valuesCount = new
            {
                Name = req.Nama,
                req.Provinsi,
                req.UnitCode,
                req.Role
            };
            var count = Db.WithConnectionAsync(db => db.ExecuteScalarAsync<int>(spCount, valuesCount, commandType: CommandType.StoredProcedure));

            await Task.WhenAll(data, count).ConfigureAwait(false);

            return new GridResponse<DahboardEnrollmentPG>
            {
                Count = count.Result,
                Data = data.Result
            };
        }

    }
}
