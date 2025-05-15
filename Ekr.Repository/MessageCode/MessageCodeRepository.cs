using Ekr.Core.Entities;
using Ekr.Core.Entities.MessageCode;
using Ekr.Dapper.Connection.Contracts.Sql;
using Ekr.Repository.Contracts.MessageCode;
using ServiceStack.OrmLite.Dapper;
using System;
using System.Data;
using System.Threading.Tasks;

namespace Ekr.Repository.MessageCode
{
    public class MessageCodeRepository : BaseRepository, IMessageCodeRepository
    {
        public MessageCodeRepository(IEKtpReaderBackendDb con) : base(con) { }

        #region Load Data
        public async Task<GridResponse<MessageCodeVM>> LoadData(MessageCodeFilter req)
        {
            const string sp = "[ProcMessageCode]";
            var values = new
            {
                req.Code,
                req.Message,
                SColumn = req.SortColumn,
                SColumnValue = req.SortColumnDir,
                Page = req.PageNumber,
                Rows = req.PageSize
            };

            var data = Db.WithConnectionAsync(db =>
            db.QueryAsync<MessageCodeVM>(sp, values, commandType: CommandType.StoredProcedure));

            const string spCount = "[ProcMessageCodeNum]";
            var valuesCount = new
            {
                req.Code,
                req.Message,
            };

            var count = Db.WithConnectionAsync(db =>
            db.ExecuteScalarAsync<int>(spCount, valuesCount, commandType: CommandType.StoredProcedure));

            await Task.WhenAll(data, count)
                .ConfigureAwait(false);

            return new GridResponse<MessageCodeVM>
            {
                Count = count.Result,
                Data = data.Result
            };
        }
        #endregion

        #region Get
        public async Task<Tbl_Master_MessageCode> GetById(MessageCodeByIdVM req)
        {
            const string query = "select * from [dbo].[Tbl_Master_MessageCode] where Id = @Id";

            return await Db.WithConnectionAsync(db => db.QueryFirstOrDefaultAsync<Tbl_Master_MessageCode>(query, new { req.Id }));
        }
        #endregion

        #region Insert
        public async Task<Tbl_Master_MessageCode> InsertMessageCode(Tbl_Master_MessageCode req)
        {
            req.IsActive = true;
            req.IsDeleted = false;
            const string query = "Insert Into Tbl_Master_MessageCode (" +
                    "Code, " +
                    "Message, " +
                    "Description, " +
                    "Mitigation, " +
                    "CreatedTime, " +
                    "CreatedBy_Id, " +
                    "IsDeleted, " +
                    "IsActive) " +
                "values(" +
                    "@Code, " +
                    "@Message, " +
                    "@Description, " +
                    "@Mitigation, " +
                    "@CreatedTime, " +
                    "@CreatedBy_Id, " +
                    "@IsDeleted, " +
                    "@IsActive) ";

            await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
            {
                req.Code,
                req.Message,
                req.Description,
                req.Mitigation,
                CreatedTime = DateTime.Now,
                req.CreatedBy_Id,
                IsDeleted = false,
                IsActive = true,
            }));

            return req;
        }
        #endregion

        #region Update
        public async Task<Tbl_Master_MessageCode> UpdateMessageCode(Tbl_Master_MessageCode req)
        {
            req.UpdatedTime = DateTime.Now;
            const string query = "Update Tbl_Master_MessageCode set " +
                        "Code = @Code, " +
                        "Message = @Message, " +
                        "Description = @Description, " +
                        "Mitigation = @Mitigation, " +
                        "UpdatedTime = @UpdatedTime, " +
                        "UpdatedBy_Id = @UpdatedBy_Id, " +
                        "IsActive = @IsActive " +
                    "Where ID = @Id";

            await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
            {
                req.Code,
                req.Message,
                req.Description,
                req.Mitigation,
                req.UpdatedTime,
                req.UpdatedBy_Id,
                req.IsActive,
                req.Id
            }));

            return req;
        }
        #endregion

        #region Delete
        public async Task DeleteMessageCode(MessageCodeByIdVM req, int PegawaiId)
        {
            const string query = "Update Tbl_Master_MessageCode set " +
                        "IsDeleted = @IsDeleted, " +
                        "DeletedTime = @DeletedTime, " +
                        "DeletedBy_Id = @DeletedBy_Id " +
                    "Where ID = @Id";

            await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
            {
                IsDeleted = true,
                DeletedTime = DateTime.Now,
                DeletedBy_Id = PegawaiId,
                req.Id
            }));
        }
        #endregion

        #region create

        #endregion
    }
}
