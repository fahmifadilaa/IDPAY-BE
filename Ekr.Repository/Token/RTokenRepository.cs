using Dapper;
using Ekr.Core.Configuration;
using Ekr.Core.Entities.Token;
using Ekr.Dapper.Connection.Base;
using Ekr.Dapper.Connection.Contracts.Base;
using Ekr.Dapper.Connection.Contracts.Sql;
using Ekr.Repository.Contracts.Token;
using Microsoft.Extensions.Options;
using System;
using System.Reflection.Metadata.Ecma335;

namespace Ekr.Repository.Token
{
    public class RTokenRepository : BaseRepository, IRTokenRepository
    {
        private readonly IBaseConnection _baseConnection;

        public RTokenRepository(IEKtpReaderBackendDb con,
             Microsoft.Extensions.Options.IOptions<ConnectionStringConfig> options, Microsoft.Extensions.Options.IOptions<ErrorMessageConfig> options2
            ) : base(con)
        {
            _baseConnection = new SqlServerConnection(options.Value.dbConnection1, options2);
        }

        public bool AddToken(Tbl_Jwt_Repository token)
        {
            const string query = "INSERT INTO Tbl_Jwt_Repository (UserCode, ClientId, ClientIp, RefreshToken, IsExpired, " +
                "TokenId, Token, CreatedTime, ChannelTokenId)  " +
                "VALUES " +
                "(@UserCode, @ClientId, @ClientIp, @RefreshToken, @IsExpired, @TokenId, @Token, @CreatedTime, @ChannelTokenId)";

            return Db.WithConnection(db => db.Execute(query, new
            {
                RefreshToken = new DbString { Value = token.RefreshToken, Length = 350 },
                Token = new DbString { Value = token.Token, Length = 1500 },
                TokenId = new DbString { Value = token.TokenId, Length = 350 },
                UserCode = new DbString { Value = token.UserCode, Length = 150 },
                ClientId = new DbString { Value = token.ClientId, Length = 150 },
                ClientIp = new DbString { Value = token.ClientIp, Length = 150 },
                token.CreatedTime,
                token.ChannelTokenId,
                token.IsExpired
            })) > 0;
        }

        public bool AddTokenThirdParty(Tbl_Jwt_Repository_ThirdParty token)
        {
            const string query = "INSERT INTO Tbl_Jwt_Repository_ThirdParty (UserCode, ClientId, ClientIp, RefreshToken, IsExpired, " +
                "TokenId, Token, CreatedTime, ChannelTokenId)  " +
                "VALUES " +
                "(@UserCode, @ClientId, @ClientIp, @RefreshToken, @IsExpired, @TokenId, @Token, @CreatedTime, @ChannelTokenId)";

            return Db.WithConnection(db => db.Execute(query, new
            {
                RefreshToken = new DbString { Value = token.RefreshToken, Length = 350 },
                Token = new DbString { Value = token.Token, Length = 1500 },
                TokenId = new DbString { Value = token.TokenId, Length = 350 },
                UserCode = new DbString { Value = token.UserCode, Length = 150 },
                ClientId = new DbString { Value = token.ClientId, Length = 150 },
                ClientIp = new DbString { Value = token.ClientIp, Length = 150 },
                token.CreatedTime,
                token.ChannelTokenId,
                token.IsExpired
            })) > 0;
        }

        public bool AddTokenLog(Tbl_Jwt_Repository token)
        {
            const string query = "INSERT INTO Tbl_Jwt_Repository_Log (UserCode, ClientId, ClientIp, RefreshToken, IsExpired, " +
                "TokenId, Token, CreatedTime, ChannelTokenId, UpdatedTime)  " +
                "VALUES " +
                "(@UserCode, @ClientId, @ClientIp, @RefreshToken, @IsExpired, @TokenId, @Token, @CreatedTime, @ChannelTokenId, @UpdatedTime)";

            return _baseConnection.WithConnection(db => db.Execute(query, new
            {
                RefreshToken = new DbString { Value = token.RefreshToken, Length = 350 },
                Token = new DbString { Value = token.Token, Length = 1500 },
                TokenId = new DbString { Value = token.TokenId, Length = 350 },
                UserCode = new DbString { Value = token.UserCode, Length = 150 },
                ClientId = new DbString { Value = token.ClientId, Length = 150 },
                ClientIp = new DbString { Value = token.ClientIp, Length = 150 },
                token.CreatedTime,
                token.ChannelTokenId,
                token.IsExpired,
                token.UpdatedTime
            })) > 0;
        }

        public bool AddTokenLogThirdParty(Tbl_Jwt_Repository_ThirdParty token)
        {
            const string query = "INSERT INTO Tbl_Jwt_Repository_ThirdParty_Log (UserCode, ClientId, ClientIp, RefreshToken, IsExpired, " +
                "TokenId, Token, CreatedTime, ChannelTokenId, UpdatedTime)  " +
                "VALUES " +
                "(@UserCode, @ClientId, @ClientIp, @RefreshToken, @IsExpired, @TokenId, @Token, @CreatedTime, @ChannelTokenId, @UpdatedTime)";

            return _baseConnection.WithConnection(db => db.Execute(query, new
            {
                RefreshToken = new DbString { Value = token.RefreshToken, Length = 350 },
                Token = new DbString { Value = token.Token, Length = 1500 },
                TokenId = new DbString { Value = token.TokenId, Length = 350 },
                UserCode = new DbString { Value = token.UserCode, Length = 150 },
                ClientId = new DbString { Value = token.ClientId, Length = 150 },
                ClientIp = new DbString { Value = token.ClientIp, Length = 150 },
                token.CreatedTime,
                token.ChannelTokenId,
                token.IsExpired,
                token.UpdatedTime
            })) > 0;
        }

        public bool ExpireToken(Tbl_Jwt_Repository token)
        {
            const string query = "update Tbl_Jwt_Repository " +
                "SET " +
                "IsExpired = 1, " +
                "UpdatedTime = @UpdatedTime " +
                "WHERE Id = @Id";

            return Db.WithConnection(db => db.Execute(query, new
            {
                token.Id,
                token.UpdatedTime
            })) > 0;
        }
        public bool ExpireTokenThirdParty(Tbl_Jwt_Repository_ThirdParty token)
        {
            const string query = "update Tbl_Jwt_Repository_ThirdParty " +
                "SET " +
                "IsExpired = 1, " +
                "UpdatedTime = @UpdatedTime " +
                "WHERE Id = @Id";

            return Db.WithConnection(db => db.Execute(query, new
            {
                token.Id,
                token.UpdatedTime
            })) > 0;
        }

        public bool DeleteToken(Tbl_Jwt_Repository token)
        {
            const string query = "DELETE Tbl_Jwt_Repository " +
                "WHERE Id = @Id";

            return Db.WithConnection(db => db.Execute(query, new
            {
                token.Id
            })) > 0;
        }

        public bool DeleteTokenThirdParty(Tbl_Jwt_Repository_ThirdParty token)
        {
            const string query = "DELETE Tbl_Jwt_Repository_ThirdParty " +
                "WHERE Id = @Id";

            return Db.WithConnection(db => db.Execute(query, new
            {
                token.Id
            })) > 0;
        }

        public bool UpdateToken(Tbl_Jwt_Repository token)
        {
            const string query = "update Tbl_Jwt_Repository " +
                "SET " +
                "IsExpired = 1, " +
                "ClientId = @ClientId, " +
                "ClientIp = @ClientIp, " +
                "RefreshToken = @RefreshToken, " +
                "Token = @Token, " +
                "CreatedTime = @CreatedTime, " +
                "UpdatedTime = @UpdatedTime " +
                "WHERE Id = @Id";

            return Db.WithConnection(db => db.Execute(query, new
            {
                token.Id,
                token.UpdatedTime,
                RefreshToken = new DbString { Value = token.RefreshToken, Length = 350 },
                Token = new DbString { Value = token.Token, Length = 1500 },
                ClientId = new DbString { Value = token.ClientId, Length = 150 },
                ClientIp = new DbString { Value = token.ClientIp, Length = 150 },
                token.CreatedTime,
            })) > 0;
        }
       
        public Tbl_Jwt_Repository GetToken(string refreshToken, string clientId, string userCode, string ipAddress)
        {
            //const string query = "SELECT * FROM Tbl_Jwt_Repository WHERE " +
            //    "UserCode = @userCode and ClientId = @clientId and ClientIp = @ipAddress AND " +
            //    "RefreshToken = @refreshToken";

            const string query = "SELECT " +
                
                "RefreshToken, " +
                "ClientId," +
                "UserCode," +
                "ClientIp, " +
                "TokenId, " +
                "Token, " +
                "ChannelTokenId, " +
                "CreatedTime " +
                "FROM Tbl_Jwt_Repository WHERE " +
                "UserCode = @userCode " +
                "and ClientId = @clientId " +
                "and ClientIp = @ipAddress " +
                "AND RefreshToken = @refreshToken";

            return Db.WithConnection(c => c.QueryFirstOrDefault<Tbl_Jwt_Repository>(query, new
            {
                refreshToken = new DbString { Value = refreshToken, Length = 250 },
                clientId = new DbString { Value = clientId, Length = 150 },
                userCode = new DbString { Value = userCode, Length = 150 },
                ipAddress = new DbString { Value = ipAddress, Length = 150 }
            }));
        }

        public Tbl_Jwt_Repository_ThirdParty GetTokenThirdParty(string refreshToken, string clientId, string userCode, string ipAddress)
        {
            const string query = "SELECT * FROM Tbl_Jwt_Repository_ThirdParty WHERE " +
                "UserCode = @userCode and ClientId = @clientId and ClientIp = @ipAddress AND " +
                "RefreshToken = @refreshToken";

            return Db.WithConnection(c => c.QueryFirstOrDefault<Tbl_Jwt_Repository_ThirdParty>(query, new
            {
                refreshToken = new DbString { Value = refreshToken, Length = 250 },
                clientId = new DbString { Value = clientId, Length = 150 },
                userCode = new DbString { Value = userCode, Length = 150 },
                ipAddress = new DbString { Value = ipAddress, Length = 150 }
            }));
        }

        public Tbl_Jwt_Repository GetActiveToken(string clientId, string userCode, string ipAddress, int tokenLifetime)
        {
            const string query = "SELECT Token, RefreshToken FROM Tbl_Jwt_Repository WHERE " +
                "UserCode = @userCode and ClientId = @clientId and ClientIp = @ipAddress AND " +
                "DATEADD(mi, @increase, CreatedTime) > @now";

            return Db.WithConnection(c => c.QueryFirstOrDefault<Tbl_Jwt_Repository>(query, new
            {
                userCode = new DbString { Value = userCode, Length = 150 },
                clientId = new DbString { Value = clientId, Length = 150 },
                ipAddress = new DbString { Value = ipAddress, Length = 150 },
                increase = tokenLifetime,
                now = DateTime.Now
            }, commandTimeout: 600));
        }

        public void ExpireDuplicateRefreshToken(string userCode, string clientId)
        {
            const string query = "SELECT [Id],[UserCode],[ClientId],[ClientIp],[RefreshToken],[IsExpired],[TokenId],[Token],[CreatedTime]," +
                "[ChannelTokenId] FROM Tbl_Jwt_Repository WHERE " +
                "UserCode = @userCode and ClientId = @clientId";

            var tokens = Db.WithConnection(c => c.Query<Tbl_Jwt_Repository>(query, new
            {
                clientId = new DbString { Value = clientId, Length = 150 },
                userCode = new DbString { Value = userCode, Length = 150 }
            }, commandTimeout: 6000).AsList());

            // move to log and delete
            foreach (var t in tokens)
            {
                t.UpdatedTime = DateTime.Now;
                t.IsExpired = true;

                AddTokenLog(t);
                DeleteToken(t);
            }
        }

        public void ExpireDuplicateRefreshTokenAgent(string userCode, string clientId, string clientIp)
        {
            const string query = "SELECT [Id],[UserCode],[ClientId],[ClientIp],[RefreshToken],[IsExpired],[TokenId],[Token],[CreatedTime]," +
                "[ChannelTokenId] FROM Tbl_Jwt_Repository WHERE " +
                "UserCode = @userCode and ClientId = @clientId and ClientIp = @clientIp";

            var tokens = Db.WithConnection(c => c.Query<Tbl_Jwt_Repository>(query, new
            {
                clientId = new DbString { Value = clientId, Length = 150 },
                userCode = new DbString { Value = userCode, Length = 150 },
                clientIp = new DbString { Value = clientIp, Length = 150 }
            }, commandTimeout: 6000).AsList());

            // move to log and delete
            foreach (var t in tokens)
            {
                t.UpdatedTime = DateTime.Now;
                t.IsExpired = true;

                AddTokenLog(t);
                DeleteToken(t);
            }
        }

        public void ExpireDuplicateRefreshTokenThirdParty(string userCode, string clientId, string clientIp)
        {
            const string query = "SELECT [Id],[UserCode],[ClientId],[ClientIp],[RefreshToken],[IsExpired],[TokenId],[Token],[CreatedTime]," +
                "[ChannelTokenId] FROM Tbl_Jwt_Repository_ThirdParty WHERE " +
                "UserCode = @userCode and ClientId = @clientId and ClientIp = @clientIp";

            var tokens = Db.WithConnection(c => c.Query<Tbl_Jwt_Repository_ThirdParty>(query, new
            {
                clientId = new DbString { Value = clientId, Length = 150 },
                userCode = new DbString { Value = userCode, Length = 150 },
                clientIp = new DbString { Value = clientIp, Length = 150 }
            }, commandTimeout: 6000).AsList());

            // move to log and delete
            foreach (var t in tokens)
            {
                t.UpdatedTime = DateTime.Now;
                t.IsExpired = true;

                AddTokenLogThirdParty(t);
                DeleteTokenThirdParty(t);
            }
        }
    }
}
