using Dapper;
using Ekr.Core.Entities;
using Ekr.Core.Entities.DataMaster.Lookup;
using Ekr.Core.Entities.DataMaster.MasterTreshold;
using Ekr.Dapper.Connection.Contracts.Sql;
using Ekr.Repository.Contracts.DataMaster.MasterTreshold;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ekr.Repository.DataMaster.MasterTreshold
{
    public class TresholdRepository : BaseRepository, ITresholdRepository
    {
        public TresholdRepository(IEKtpReaderBackendDb con) : base(con) { }

        public async Task DeleteTreshold(TresholdByIdVM req, int PegawaiId)
        {
            const string query = "Update Tbl_Master_Treshold set " +
                        "isDelete = @IsDeleted, " +
                        "deletedTime = @DeletedTime, " +
                        "deletedById = @DeletedBy_Id " +
                    "Where ID = @Id";

            await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
            {
                IsDeleted = true,
                DeletedTime = DateTime.Now,
                DeletedBy_Id = PegawaiId,
                req.Id
            }));
        }

        public async Task<TblMasterTreshold> GetById(TresholdByIdVM req)
        {
            const string query = "select * from [dbo].[Tbl_Master_Treshold] where Id = @Id";

            return await Db.WithConnectionAsync(db => db.QueryFirstOrDefaultAsync<TblMasterTreshold>(query, new { req.Id }));
        }

        public async Task<TblMasterTreshold> InsertTreshold(TblMasterTreshold req)
        {
            req.isActive = true;
            req.isDelete = false;
            const string query = "Insert Into Tbl_Master_Treshold (" +
                    "tipeid, " +
                    "value, " +
                    "isActive, " +
                    "isDelete, " +
                    "createdTime, " +
                    "createdById) " +
                "values(" +
                    "@tipeid, " +
                    "@value, " +
                    "@isActive, " +
                    "@isDelete, " +
                    "@createdTime, " +
                    "@createdById) ";

            await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
            {
                req.tipeid,
                req.value,
                req.isActive,
                req.isDelete,
                createdTime = DateTime.Now,
                req.createdById
            }));

            return req;
        }

        public async Task<GridResponse<TresholdVM>> LoadData(TresholdFilter req)
        {
            const string sp = "[ProcTreshold]";
            var values = new
            {
                Name = req.Filter,
                SColumn = req.SortColumn,
                SColumnValue = req.SortColumnDir,
                Page = req.PageNumber,
                Rows = req.PageSize
            };

            var data = Db.WithConnectionAsync(db =>
            db.QueryAsync<TresholdVM>(sp, values, commandType: CommandType.StoredProcedure));

            const string spCount = "[ProcTresholdNum]";
            var valuesCount = new
            {
                Name = req.Filter
            };

            var count = Db.WithConnectionAsync(db =>
            db.ExecuteScalarAsync<int>(spCount, valuesCount, commandType: CommandType.StoredProcedure));

            await Task.WhenAll(data, count)
                .ConfigureAwait(false);

            return new GridResponse<TresholdVM>
            {
                Count = count.Result,
                Data = data.Result
            };
        }

        public async Task<TblMasterTreshold> UpdateTreshold(TblMasterTreshold req)
        {
            req.updatedTime = DateTime.Now;
            const string query = "Update Tbl_Master_Treshold set " +
                        "tipeid = @tipeid, " +
                        "value = @value, " +
                        "updatedTime = @updatedTime, " +
                        "updatedById = @updatedById, " +
                        "isActive = @isActive " +
                    "Where id = @Id";

            await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
            {
                req.tipeid,
                req.value,
                req.updatedTime,
                req.updatedById,
                req.isActive,
                req.id
            }));

            return req;
        }
    }
}
