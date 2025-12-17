using Dapper;
using Ekr.Core.Configuration;
using Ekr.Core.Entities;
using Ekr.Dapper.Connection.Base;
using Ekr.Dapper.Connection.Contracts.Base;
using Ekr.Dapper.Connection.Contracts.Sql;
using Ekr.Dapper.Connection.Sql;
using Ekr.Repository.Contracts.BancslinkCifUpdate;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Ekr.Core.Constant;
using Ekr.Core.Entities.Logging;
using Ekr.Core.Helper;
using Ekr.Repository.Enrollment;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using ServiceStack;
using System.Collections;
using System.Xml.Linq;

namespace Ekr.Repository.BancslinkCifUpdate
{
    public class BancslinkCifUpdateRepository : BaseRepository,IBancslinkCifUpdateRepository
    {

        private readonly IBaseConnection _baseConnection;
        private readonly ILogger<BancslinkCifUpdateRepository> _logger;

        public BancslinkCifUpdateRepository(IEKtpReaderBackendDb con,
            Microsoft.Extensions.Options.IOptions<ConnectionStringConfig> options, Microsoft.Extensions.Options.IOptions<ErrorMessageConfig> options2, ILogger<BancslinkCifUpdateRepository> logger
            ) : base(con)
        {
            _baseConnection = new SqlServerConnection(options.Value.dbConnection1, options2);
            _logger = logger;

        }

        public Task<BancslinkCifNikUpdateResult> ExecuteBancslinkCifNikUpdateAsync(BancslinkCifNikUpdateRequest request)
        {
            const string proc = "[sp_BancslinkCifUpdate]";

            return Db.WithConnectionAsync(async c =>
            {
                var parameters = new DynamicParameters();
                parameters.Add("@SpvID", request.SpvID);
                parameters.Add("@TellerID", request.TellerID);
                parameters.Add("@BranchID", request.BranchID);
                parameters.Add("@CIF", request.CIF);
                parameters.Add("@NIK", request.NIK);
                parameters.Add("@ResultMessage", dbType: DbType.String, size: 255, direction: ParameterDirection.Output);

                var result = await c.QueryFirstOrDefaultAsync<BancslinkCifNikUpdateResult>(
                    proc,
                    parameters,
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: 6000
                );
                result ??= new BancslinkCifNikUpdateResult();

                result.Message = parameters.Get<string>("@ResultMessage");
                result.Success = true; 

                return result;
            });
        }

        //public async Task<BancslinkCifNikUpdateResult> ExecuteBancslinkCifNikUpdateAsync(BancslinkCifNikUpdateRequest request)
        //{
        //    try
        //    {
        //        await using var conn = new SqlConnection(_connectionString);
        //        await using var cmd = new SqlCommand("sp_BancslinkCifUpdate", conn)
        //        {
        //            CommandType = CommandType.StoredProcedure
        //        };

        //        cmd.Parameters.AddWithValue("@SupervisorID", (object)request.SupervisorID ?? DBNull.Value);
        //        cmd.Parameters.AddWithValue("@APIKey", (object)request.APIKey ?? DBNull.Value);
        //        cmd.Parameters.AddWithValue("@createdByNPP", (object)request.CreatedByNPP ?? DBNull.Value);
        //        cmd.Parameters.AddWithValue("@ApprovalNpp", (object)request.ApprovalNpp ?? DBNull.Value);
        //        cmd.Parameters.AddWithValue("@createdByUnitCode", (object)request.CreatedByUnitCode ?? DBNull.Value);
        //        cmd.Parameters.AddWithValue("@CIF", (object)request.CIF ?? DBNull.Value);
        //        cmd.Parameters.AddWithValue("@NIK", (object)request.NIK ?? DBNull.Value);

        //        var outParam = new SqlParameter("@ResultMessage", SqlDbType.NVarChar, 500)
        //        {
        //            Direction = ParameterDirection.Output
        //        };
        //        cmd.Parameters.Add(outParam);

        //        await conn.OpenAsync();
        //        await cmd.ExecuteNonQueryAsync();

        //        var message = outParam.Value?.ToString() ?? "Process Success";

        //        return new BancslinkCifNikUpdateResult
        //        {
        //            Success = true,
        //            Message = message
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        // boleh log ke logger atau rethrow sesuai kebijakan
        //        return new BancslinkCifNikUpdateResult
        //        {
        //            Success = false,
        //            Message = ex.Message,
        //            ErrorCode = "ERR_DB"
        //        };
        //    }
        //}
    }
}

