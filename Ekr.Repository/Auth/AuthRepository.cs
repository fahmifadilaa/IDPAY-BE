using Dapper;
using Ekr.Core.Configuration;
using Ekr.Core.Constant;
using Ekr.Core.Entities.Auth;
using Ekr.Core.Entities.DataKTP;
using Ekr.Core.Entities.Logging;
using Ekr.Core.Helper;
using Ekr.Dapper.Connection.Base;
using Ekr.Dapper.Connection.Contracts.Base;
using Ekr.Dapper.Connection.Contracts.Sql;
using Ekr.Repository.Contracts.Auth;
using Ekr.Repository.Enrollment;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using ServiceStack;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Ekr.Repository.Auth
{
    public class AuthRepository : BaseRepository, IAuthRepository
    {
        private readonly IBaseConnection _baseConnection;
        private readonly ILogger<AuthRepository> _logger;

        public AuthRepository(IEKtpReaderBackendDb con,
            Microsoft.Extensions.Options.IOptions<ConnectionStringConfig> options, Microsoft.Extensions.Options.IOptions<ErrorMessageConfig> options2, ILogger<AuthRepository> logger
            ) : base(con)
        {
            _baseConnection = new SqlServerConnection(options.Value.dbConnection1, options2);
            _logger = logger;

        }

        public async Task<bool> LoginAgent(string name, string token)
        {
            const string query = "Select Count(Id) FROM Tbl_Master_Apps " +
                "WHERE Nama = @name and Token = @token and IsActive = 1";

            return await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
            {
                name = new DbString { Value = name, Length = 500 },
                token = new DbString { Value = token, Length = 250 }
            })) > 0;
        }

        public Task<DetailLogin> LoginUser(string nik)
        {
            const string proc = "[ProcLoginGetData]";

            return Db.WithConnectionAsync(c => c.QueryFirstOrDefaultAsync<DetailLogin>(proc, new
            {
                NIK = new DbString { Value = nik, Length = 50 },
            }, commandType: CommandType.StoredProcedure, commandTimeout: 6000));
        }

        public Task<DetailLoginThirdParty> LoginThirdParty(string username)
        {
            const string proc = "[ProcLoginGetDataThirdParty]";

            return Db.WithConnectionAsync(c => c.QueryFirstOrDefaultAsync<DetailLoginThirdParty>(proc, new
            {
                username = new DbString { Value = username, Length = 50 },
            }, commandType: CommandType.StoredProcedure, commandTimeout: 6000));
        }

        public Task<IEnumerable<MatchingLoginFinger>> GetDataFinger(string npp)
        {
            const string sp = "[ProcMatchingLoginFingerByNpp]";
            var values = new
            {
                Npp = npp
            };

            return Db.WithConnectionAsync(db =>
                db.QueryAsync<MatchingLoginFinger>(sp, values, commandType: CommandType.StoredProcedure)
            );
        }

        public Task<IEnumerable<MatchingLoginFinger>> GetDataFingerGeneral(string nik)
        {
            const string sp = "[ProcMatchingLoginFinger]";
            var values = new
            {
                NIK = nik
            };

            return Db.WithConnectionAsync(db =>
                db.QueryAsync<MatchingLoginFinger>(sp, values, commandType: CommandType.StoredProcedure)
            );
        }

        public Task<IEnumerable<MatchingLoginFinger>> GetDataFingerGeneralISO(string nik)
        {
            const string sp = "[ProcMatchingLoginFingerISO]";
            var values = new
            {
                NIK = new DbString { Value = nik, Length = 50 }
            };

            return Db.WithConnectionAsync(db =>
                db.QueryAsync<MatchingLoginFinger>(sp, values, commandType: CommandType.StoredProcedure)
            );
        }

        public Task<IEnumerable<MatchingLoginFinger>> GetDataFingerGeneralNew(string nik)
        {
            const string sp = "[ProcMatchingLoginFinger]";
            var values = new
            {
                NIK = new DbString { Value = nik, Length = 50 }
            };

            return Db.WithConnectionAsync(db =>
                db.QueryAsync<MatchingLoginFinger>(sp, values, commandType: CommandType.StoredProcedure)
            );
        }

        public Task<IEnumerable<MatchingLoginFinger>> GetDataFingerGeneralNewFile(string nik)
        {
            const string sp = "[ProcMatchingLoginFingerFileIso]";
            var values = new
            {
                NIK = new DbString { Value = nik, Length = 50 }
            };

            return Db.WithConnectionAsync(db =>
                db.QueryAsync<MatchingLoginFinger>(sp, values, commandType: CommandType.StoredProcedure)
            );
        }

        public Task<IEnumerable<MatchingLoginFinger>> GetDataFingerGeneralEmp(string nik)
        {
            const string sp = "[ProcMatchingLoginFingerEmp]";
            var values = new
            {
                NIK = new DbString { Value = nik, Length = 50 }
            };

            return Db.WithConnectionAsync(db =>
                db.QueryAsync<MatchingLoginFinger>(sp, values, commandType: CommandType.StoredProcedure)
            );
        }

        public Task<IEnumerable<MatchingLoginFinger>> GetDataFingerGeneralEmpNew(string nik)
        {
            const string sp = "[ProcMatchingLoginFingerEmp]";
            var values = new
            {
                NIK = new DbString { Value = nik, Length = 50 }
            };

            return Db.WithConnectionAsync(db =>
                db.QueryAsync<MatchingLoginFinger>(sp, values, commandType: CommandType.StoredProcedure)
            );
        }

        public Task<IEnumerable<MatchingLoginFinger>> GetDataFingerGeneralEmpNewFileIso(string nik)
        {
            const string sp = "[ProcMatchingLoginFingerEmpIso]";
            var values = new
            {
                NIK = new DbString { Value = nik, Length = 50 }
            };

            return Db.WithConnectionAsync(db =>
                db.QueryAsync<MatchingLoginFinger>(sp, values, commandType: CommandType.StoredProcedure)
            );
        }

        public Task<IEnumerable<MatchingLoginFinger>> GetDataFingerGeneralEmpIso(string nik)
        {
            const string sp = "[ProcMatchingLoginFingerEmpIso]";
            var values = new
            {
                NIK = nik
            };

            return Db.WithConnectionAsync(db =>
                db.QueryAsync<MatchingLoginFinger>(sp, values, commandType: CommandType.StoredProcedure)
            );
        }

        public Task<IEnumerable<MatchingLoginFinger>> GetDataFingerGeneralByCif(string cif)
        {
            const string sp = "[ProcMatchingLoginFingerByCif]";
            var values = new
            {
                CIF = cif
            };

            return Db.WithConnectionAsync(db =>
                db.QueryAsync<MatchingLoginFinger>(sp, values, commandType: CommandType.StoredProcedure)
            );
        }

        public Task<IEnumerable<MatchingLoginFinger>> GetDataFingerGeneralByCifIso(string cif)
        {
            const string sp = "[ProcMatchingLoginFingerByCifIso]";
            var values = new
            {
                CIF = new DbString { Value = cif, Length = 50 }
            };

            return Db.WithConnectionAsync(db =>
                db.QueryAsync<MatchingLoginFinger>(sp, values, commandType: CommandType.StoredProcedure)
            );
        }

        public Task<IEnumerable<MatchingLoginFinger>> GetDataFingerGeneralByCifNew(string cif)
        {
            const string sp = "[ProcMatchingLoginFingerByCifISODB]";
            var values = new
            {
                CIF = new DbString { Value = cif, Length = 70 }
            };

            return Db.WithConnectionAsync(db =>
                db.QueryAsync<MatchingLoginFinger>(sp, values, commandType: CommandType.StoredProcedure)
            );
        }

        public Task<IEnumerable<MatchingLoginFinger>> GetDataFingerGeneralByCifNewFile(string cif)
        {
            const string sp = "[ProcMatchingLoginFingerByCifFileIso]";
            var values = new
            {
                CIF = new DbString { Value = cif, Length = 70 }
            };

            return Db.WithConnectionAsync(db =>
                db.QueryAsync<MatchingLoginFinger>(sp, values, commandType: CommandType.StoredProcedure)
            );
        }

        public Task<IEnumerable<MatchingLoginFinger>> GetDataFingerEmpGeneralByCif(string cif)
        {
            const string sp = "[ProcMatchingLoginEmpFingerByCif]";
            var values = new
            {
                CIF = cif
            };

            return Db.WithConnectionAsync(db =>
                db.QueryAsync<MatchingLoginFinger>(sp, values, commandType: CommandType.StoredProcedure)
            );
        }

        #region get
        public Task<Tbl_Login_Session> CheckSessionLogin(string npp, string ipAddress)
        {
            const string query = 
                "select * from Tbl_Login_Session " +
                "where Npp = @npp " +
                "and IpAddress = @ipAddress"; 

            return Db.WithConnectionAsync(c => c.QueryFirstOrDefaultAsync<Tbl_Login_Session>(query, new { npp = new DbString { Value = npp, Length = 50 },
                ipAddress = new DbString { Value = ipAddress, Length = 50 }
            }));
        }
        #endregion

        #region Insert
        public async Task<Tbl_LogClientApps> InsertLogClientApps(Tbl_LogClientApps req)
        {
            try
            {
                const string query = "Insert Into Tbl_LogClientApps (" +
                    "Param, " +
                    "LvTeller, " +
                    "Branch, " +
                    "SubBranch, " +
                    "ClientApps, " +
                    "EndPoint, " +
                    "RequestTime, " +
                    "ResponseTime, " +
                    "ErrorMessage, " +
                    "CreatedTime, " +
                    "CreatedByNpp, " +
                    "CreatedByUnitCode) " +
                "values(" +
                    "@Param, " +
                    "@LvTeller, " +
                    "@Branch, " +
                    "@SubBranch, " +
                    "@ClientApps, " +
                    "@EndPoint, " +
                    "@RequestTime, " +
                    "@ResponseTime, " +
                    "@ErrorMessage, " +
                    "@CreatedTime, " +
                    "@CreatedByNpp, " +
                    "@CreatedByUnitCode) ";

                _baseConnection.WithConnection(c => c.ExecuteScalar<int>(query, new
                {
                    req.Param,
                    LvTeller = new DbString { Value = req.LvTeller, Length = 200, IsAnsi = true },
                    Branch = new DbString { Value = req.Branch, Length = 250, IsAnsi = true },
                    SubBranch = new DbString { Value = req.SubBranch, Length = 250, IsAnsi = true },
                    ClientApps = new DbString { Value = req.ClientApps, Length = 500, IsAnsi = true },
                    EndPoint = new DbString { Value = req.EndPoint, Length = 500, IsAnsi = true },
                    req.RequestTime,
                    req.ResponseTime,
                    req.ErrorMessage,
                    CreatedTime = DateTime.Now,
                    CreatedByNpp = new DbString { Value = req.CreatedByNpp, Length = 80, IsAnsi = true },
                    CreatedByUnitCode = new DbString { Value = req.CreatedByUnitCode, Length = 20, IsAnsi = true }
                }));
            }
            catch (Exception ex) {

                _logger.LogError("[ErrorAuthRepository] : " + ex.Message, ex);
            };


            return req;
        }

        public async Task<Tbl_Login_Session> InsertSessionLogin(Tbl_Login_Session req)
        {
            const string query =
                "INSERT INTO Tbl_Login_Session (" +
                "[npp]," +
                "[IpAddress]," +
                "[LastActive]," +
                "[Attempt]," +
                "[LastAttempt]" +
                ") " +
                "VALUES " +
                "(" +
                "@npp," +
                "@IpAddress," +
                "@LastActive," +
                "@Attempt," +
                "@LastAttempt" +
                ")";

            await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
            {
                req.npp,
                req.IpAddress,
                req.LastActive,
                req.Attempt,
                req.LastAttempt
            }));

            return req;
        }
        #endregion

        #region update
        public async Task<Tbl_Login_Session> UpdateSessionLogin(Tbl_Login_Session req)
        {
            const string query =
                "UPDATE Tbl_Login_Session SET " +
                "[npp] = @npp, " +
                "[IpAddress] = @IpAddress, " +
                "[LastActive] = @LastActive, " +
                "[Attempt] = @Attempt, " +
                "[LastAttempt] = @LastAttempt " +
                "WHERE " +
                "[npp] = @npp "+
                "and [IpAddress] = @IpAddress "
                ;

            await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
            {
                req.npp,
                req.IpAddress,
                req.LastActive,
                req.Attempt,
                req.LastAttempt,
                req.Id
            }));

            return req;
        }
        #endregion
    }
}
