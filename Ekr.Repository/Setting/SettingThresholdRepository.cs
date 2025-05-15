using Ekr.Core.Entities.DataMaster.Utility.ViewModel;
using Ekr.Core.Entities.DataMaster.Utility;
using Ekr.Core.Entities.SettingThreshold;
using Ekr.Core.Entities;
using Ekr.Dapper.Connection.Contracts.Sql;
using Ekr.Repository.Contracts.Setting;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Ekr.Core.Helper;

namespace Ekr.Repository.Setting
{
    public class SettingThresholdRepository : BaseRepository, ISettingThresholdRepository
    {
        public SettingThresholdRepository(IEKtpReaderBackendDb con) : base(con)
        {
        }

        public async Task<string> GetProbabilityDivision(string nik)
        {
            const string query = "select B.[Name] from [dbo].[Tbl_Setting_Threshold] A LEFT JOIN [dbo].[Tbl_Lookup] B ON A.Probability_Division = B.Value AND B.Type = 'TresholdValue' where A.NIK = @nik";

            return await Db.WithConnectionAsync(db => db.QueryFirstOrDefaultAsync<string>(query, new { nik }));
        }

        #region Insert
        public async Task<TblSettingThresholdVM> InsertSettingThresholdAsync(Tbl_Setting_Threshold req)
        {
            string query = "";
            int insertedId = 0;
            if (req.IsTemp == 1)
            {
                query = "INSERT INTO [dbo].[Tbl_Setting_Threshold] " +
                    "([NIK]" +
                    ",[Start_Date] " +
                    ",[End_Date]" +
                    ",[IsTemp]" +
                    ",[Probability_Division]" +
                    ",[Keterangan]" +
                    ",[IsActive]" +
                    ",[IsDeleted]" +
                    ",[CreatedTime]" +
                    ",[CreatedBy_Id]" +
                    ",[ApproverByEmployeeId]" +
                    ",[ApproverByEmployeeId2]" +
                    ",[UnitId]" +
                    ",[StatusPengajuan])" +
                "OUTPUT INSERTED.[Id]" +
                "values(" +
                    "@NIK" +
                    ",@Start_Date " +
                    ",@End_Date" +
                    ",@IsTemp" +
                    ",@Probability_Division" +
                    ",@Keterangan" +
                    ",@IsActive" +
                    ",@IsDeleted" +
                    ",@CreatedTime" +
                    ",@CreatedBy_Id" +
                    ",@ApproverByEmployeeId" +
                    ",@ApproverByEmployeeId2" +
                    ",@UnitId" +
                    ",@StatusPengajuan)";

                insertedId = await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
                {
                    NIK = new DbString { Value = req.NIK, Length = 50 },
                    Start_Date = req.Start_Date.GetValueOrDefault(),
                    End_Date = req.Start_Date.GetValueOrDefault(),
                    IsTemp = req.IsTemp.GetValueOrDefault(),
                    Probability_Division = req.Probability_Division.GetValueOrDefault(),
                    Keterangan = new DbString { Value = req.Keterangan },
                    IsActive = true,
                    IsDeleted = false,
                    CreatedTime = DateTime.Now,
                    CreatedBy_Id = req.CreatedBy_Id.GetValueOrDefault(),
                    ApproverByEmployeeId = req.ApproverByEmployeeId.GetValueOrDefault(),
                    ApproverByEmployeeId2 = req.ApproverByEmployeeId2.GetValueOrDefault(),
                    UnitId = req.UnitId.GetValueOrDefault(),
                    StatusPengajuan = req.StatusPengajuan.GetValueOrDefault()
                }));
            }
            else {
                query = "INSERT INTO [dbo].[Tbl_Setting_Threshold] " +
                        "([NIK]" +
                        ",[IsTemp]" +
                        ",[Probability_Division]" +
                        ",[Keterangan]" +
                        ",[IsActive]" +
                        ",[IsDeleted]" +
                        ",[CreatedTime]" +
                        ",[CreatedBy_Id]" +
                        ",[ApproverByEmployeeId]" +
                        ",[ApproverByEmployeeId2]" +
                        ",[UnitId]" +
                        ",[StatusPengajuan])" +
                    "OUTPUT INSERTED.[Id]" +
                    "values(" +
                        "@NIK" +
                        ",@IsTemp" +
                        ",@Probability_Division" +
                        ",@Keterangan" +
                        ",@IsActive" +
                        ",@IsDeleted" +
                        ",@CreatedTime" +
                        ",@CreatedBy_Id" +
                        ",@ApproverByEmployeeId" +
                        ",@ApproverByEmployeeId2" +
                        ",@UnitId" +
                        ",@StatusPengajuan)";

                insertedId = await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
                {
                    NIK = new DbString { Value = req.NIK, Length = 50 },
                    IsTemp = req.IsTemp.GetValueOrDefault(),
                    Probability_Division = req.Probability_Division.GetValueOrDefault(),
                    Keterangan = new DbString { Value = req.Keterangan },
                    IsActive = true,
                    IsDeleted = false,
                    CreatedTime = DateTime.Now,
                    CreatedBy_Id = req.CreatedBy_Id.GetValueOrDefault(),
                    ApproverByEmployeeId = req.ApproverByEmployeeId.GetValueOrDefault(),
                    ApproverByEmployeeId2 = req.ApproverByEmployeeId2.GetValueOrDefault(),
                    UnitId = req.UnitId.GetValueOrDefault(),
                    StatusPengajuan = req.StatusPengajuan.GetValueOrDefault()
                }));
            }
            

            

            req.Id = insertedId;
            return CommonConverter.ConvertToDerived<TblSettingThresholdVM>(req);
        }

        public async Task<TblSettingThresholdLogVM> InsertSettingThresholdLogAsync(Tbl_Setting_Threshold_Log req)
        {
            const string query = "INSERT INTO [dbo].[Tbl_Setting_Threshold_Log] " +
                    "([Threshold_Id]" +
                    ",[Alasan] " +
                    ",[Status]" +
                    ",[CreatedBy_Id]" +
                    ",[Created_Date])" +
                "values(" +
                    "@Threshold_Id" +
                    ",@Alasan " +
                    ",@Status" +
                    ",@CreatedBy_Id" +
                    ",@Created_Date)";

            int insert = await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
            {
                req.Threshold_Id,
                req.Alasan,
                req.Status,
                req.CreatedBy_Id,
                Created_Date = req.CreatedTime = DateTime.Now
            }));

            return CommonConverter.ConvertToDerived<TblSettingThresholdLogVM>(req);
        }

        public async Task UpdateSettingTresholdStatusAsync(SettingThresholdStatusRequest req, int updatedById)
        {
            const string query = "Update Tbl_Setting_Threshold set " +
                        "UpdatedTime = @UpdatedTime, " +
                        "UpdatedBy_Id = @UpdatedBy_Id, " +
                        "Keterangan = @Keterangan, " +
                        "StatusPengajuan = @StatusPengajuan " +
                        "Where ID = @Id";

            await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
            {
                UpdatedTime = DateTime.Now,
                Keterangan = req.Alasan,
                UpdatedBy_Id = updatedById,
                ApproverByEmployeeId = req.ApproverId,
                StatusPengajuan = req.Status,
                req.Id
            }));
        }
        #endregion

        #region GET
        public async Task<GridResponse<SettingThresholdData>> GetSettingThresholdList(SettingThresholdFilter filter, int currentUserId)
        {
            const string proc = "[ProcSettingThresholdData]";

            var val = new
            {
                SColumn = new DbString { Value = string.IsNullOrWhiteSpace(filter.SortColumn) ? "Id" : filter.SortColumn, Length = 100 },
                SColumnValue = new DbString { Value = string.IsNullOrWhiteSpace(filter.SortColumnDir) ? "desc" : filter.SortColumnDir, Length = 10 },
                StatusPengajuan = filter.StatusPengajuan != null && filter.StatusPengajuan.Count > 0 ? string.Join(',', filter.StatusPengajuan) : "",
                ID = filter.ID.GetValueOrDefault(),
                NIK = new DbString { Value = string.IsNullOrWhiteSpace(filter.NIK) ? "" : filter.NIK, Length = 50 },
                Page = filter.PageNumber,
                Rows = filter.PageSize,
                CurrentUserId = currentUserId,
            };

            var res = await Db.WithConnectionAsync(c => c.QueryAsync<SettingThresholdData>(proc, val, commandType: CommandType.StoredProcedure))
                .ConfigureAwait(false);

            const string procCount = "[ProcSettingThresholdDataTotal]";

            var count = await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(procCount, new
            {
                StatusPengajuan = filter.StatusPengajuan != null && filter.StatusPengajuan.Count > 0 ? string.Join(',', filter.StatusPengajuan) : "",
                ID = filter.ID.GetValueOrDefault(),
                NIK = new DbString { Value = string.IsNullOrWhiteSpace(filter.NIK) ? "" : filter.NIK, Length = 50 },
                CurrentUserId = currentUserId,
            }, commandType: CommandType.StoredProcedure))
                .ConfigureAwait(false);

            return new GridResponse<SettingThresholdData>
            {
                Count = count,
                Data = res
            };
        }

        public async Task<TblSettingThresholdVM> GetById(SettingThresholdRequest req)
        {
            const string query = "select * from [dbo].[Tbl_Setting_Threshold] where Id = @Id";

            return await Db.WithConnectionAsync(db => db.QueryFirstOrDefaultAsync<TblSettingThresholdVM>(query, new { req.Id }));
        }

        public async Task<GridResponse<DataDropdownServerSide>> GetListPenyelia(int unitId)
        {
            const string proc = "[ProcSettingThrehsoldPenyelia]";

            IEnumerable<Pegawai> queryResponseList = await Db.WithConnectionAsync(db => db.QueryAsync<Pegawai>(proc, new { UnitId = unitId }, commandType: CommandType.StoredProcedure));

            var dataDropdown = new List<DataDropdownServerSide>();
            foreach (var queryResponse in queryResponseList)
            {
                dataDropdown.Add(new DataDropdownServerSide
                {
                    id = queryResponse.Id,
                    text = $"{queryResponse.NIK} - {queryResponse.Nama}",
                });
            }

            return new GridResponse<DataDropdownServerSide>
            {
                Count = dataDropdown != null ? dataDropdown.Count() : 0,
                Data = dataDropdown
            };
        }

        public async Task<GridResponse<DataDropdownServerSide>> GetListPenyelia2(int unitId, string npp)
        {
            const string proc = "[ProcSettingThrehsoldPenyeliaNIK]";

            IEnumerable<Pegawai> queryResponseList = await Db.WithConnectionAsync(db => db.QueryAsync<Pegawai>(proc, new { UnitId = unitId, NPP = npp}, commandType: CommandType.StoredProcedure));

            var dataDropdown = new List<DataDropdownServerSide>();
            foreach (var queryResponse in queryResponseList)
            {
                dataDropdown.Add(new DataDropdownServerSide
                {
                    id = queryResponse.Id,
                    text = $"{queryResponse.NIK} - {queryResponse.Nama}",
                });
            }

            return new GridResponse<DataDropdownServerSide>
            {
                Count = dataDropdown != null ? dataDropdown.Count() : 0,
                Data = dataDropdown
            };
        }

        public async Task<GridResponse<DataDropdownServerSide>> GetListPemimpin(int unitId)
        {
            const string proc = "[ProcSettingThrehsoldPemimpin]";

            IEnumerable<Pegawai> queryResponseList = await Db.WithConnectionAsync(db => db.QueryAsync<Pegawai>(proc, new { UnitId = unitId }, commandType: CommandType.StoredProcedure));


            var dataDropdown = new List<DataDropdownServerSide>();
            foreach (var queryResponse in queryResponseList)
            {
                dataDropdown.Add(new DataDropdownServerSide
                {
                    id = queryResponse.Id,
                    text = $"{queryResponse.NIK} - {queryResponse.Nama}",
                });
            }

            return new GridResponse<DataDropdownServerSide>
            {
                Count = dataDropdown != null ? dataDropdown.Count() : 0,
                Data = dataDropdown
            };
        }

        public async Task<GridResponse<DataDropdownServerSide>> GetListPemimpin2(int unitId, string npp)
        {
            const string proc = "[ProcSettingThrehsoldPemimpinNIK]";

            IEnumerable<Pegawai> queryResponseList = await Db.WithConnectionAsync(db => db.QueryAsync<Pegawai>(proc, new { UnitId = unitId, NPP = npp }, commandType: CommandType.StoredProcedure));


            var dataDropdown = new List<DataDropdownServerSide>();
            foreach (var queryResponse in queryResponseList)
            {
                dataDropdown.Add(new DataDropdownServerSide
                {
                    id = queryResponse.Id,
                    text = $"{queryResponse.NIK} - {queryResponse.Nama}",
                });
            }

            return new GridResponse<DataDropdownServerSide>
            {
                Count = dataDropdown != null ? dataDropdown.Count() : 0,
                Data = dataDropdown
            };
        }

        public async Task<GridResponse<TblSettingThresholdLogVM>> GetSettingTresholdLogAsync(int thresholdId)
        {
            const string query = "SELECT STL.*, TL.Name as StatusName, TP.Nama as CreatedByName, " +
                "dbo.[uf_ShortIndonesianDateTime](STL.Created_Date) as CreatedTimeString " +
                "FROM Tbl_Setting_Threshold_Log STL " +
                "LEFT JOIN Tbl_Pegawai TP on TP.Id = STL.CreatedBy_Id " +
                "LEFT JOIN Tbl_Lookup TL on TL.Type = 'PengajuanThreshold' AND TL.Value = STL.Status " +
                "WHERE Threshold_Id = @Threshold_Id";

            IEnumerable<TblSettingThresholdLogVM> queryResponse = await Db.WithConnectionAsync(db => db.QueryAsync<TblSettingThresholdLogVM>(query, new { Threshold_Id = thresholdId }));

            return new GridResponse<TblSettingThresholdLogVM>
            {
                Count = queryResponse != null ? queryResponse.Count() : 0,
                Data = queryResponse
            };
        }

        public async Task<GridResponse<DataDropdownServerSide>> GetDropdownTreshold(DropdownLookupFilterVM request)
        {
            const string proc = "[ProcDropdownLookup]";

            IEnumerable<DataDropdownServerSide> queryResponseList = await Db.WithConnectionAsync(db => db.QueryAsync<DataDropdownServerSide>(proc, new { Type = request.Type, Parameter = request.Parameter, Page = request.PageNumber, Rows = request.PageSize }, commandType: CommandType.StoredProcedure));

            const string procCount = "[ProcDropdownLookupCount]";

            int queryCount = await Db.WithConnectionAsync(db => db.QuerySingleAsync<int>(procCount, new { Type = request.Type, Parameter = request.Parameter }, commandType: CommandType.StoredProcedure));

            return new GridResponse<DataDropdownServerSide>
            {
                Count = queryResponseList != null ? queryCount : 0,
                Data = queryResponseList
            };
        }


        #endregion

        #region Update
        public async Task<TblSettingThresholdVM> UpdateSettingTreshold(Tbl_Setting_Threshold req)
        {
            req.UpdatedTime = DateTime.Now;

            string query = "";

            if (req.IsTemp == 1)
            {
                query = "Update Tbl_Setting_Threshold set " +
                        "NIK = @NIK, " +
                        "Start_Date = @Start_Date, " +
                        "End_Date = @End_Date, " +
                        "IsTemp = @IsTemp, " +
                        "Probability_Division = @Probability_Division, " +
                        "UpdatedTime = @UpdatedTime, " +
                        "UpdatedBy_Id = @UpdatedBy_Id, " +
                        "ApproverByEmployeeId = @ApproverByEmployeeId, " +
                        "ApproverByEmployeeId2 = @ApproverByEmployeeId2, " +
                        "Keterangan = @Keterangan, " +
                        "StatusPengajuan = @StatusPengajuan, " +
                        "IsActive = @IsActive " +
                    "Where ID = @Id";

                await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
                {
                    req.NIK,
                    req.Start_Date,
                    req.End_Date,
                    req.IsTemp,
                    req.Probability_Division,
                    req.Keterangan,
                    req.IsActive,
                    req.UpdatedTime,
                    req.UpdatedBy_Id,
                    req.ApproverByEmployeeId,
                    req.ApproverByEmployeeId2,
                    req.UnitId,
                    req.StatusPengajuan,
                    req.Id
                }));

            }
            else {
                query = "Update Tbl_Setting_Threshold set " +
                            "NIK = @NIK, " +
                            "IsTemp = @IsTemp, " +
                            "Probability_Division = @Probability_Division, " +
                            "UpdatedTime = @UpdatedTime, " +
                            "UpdatedBy_Id = @UpdatedBy_Id, " +
                            "ApproverByEmployeeId = @ApproverByEmployeeId, " +
                            "ApproverByEmployeeId2 = @ApproverByEmployeeId2, " +
                            "Keterangan = @Keterangan, " +
                            "StatusPengajuan = @StatusPengajuan, " +
                            "IsActive = @IsActive " +
                        "Where ID = @Id";

                await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
                {
                    req.NIK,
                    req.IsTemp,
                    req.Probability_Division,
                    req.Keterangan,
                    req.IsActive,
                    req.UpdatedTime,
                    req.UpdatedBy_Id,
                    req.ApproverByEmployeeId,
                    req.ApproverByEmployeeId2,
                    req.UnitId,
                    req.StatusPengajuan,
                    req.Id
                }));

            }


           

            return CommonConverter.ConvertToDerived<TblSettingThresholdVM>(req);
        }
        #endregion

        #region Delete
        public async Task DeleteSettingTreshold(SettingThresholdRequest req, int pegawaiId)
        {
            const string query = "Update Tbl_Setting_Threshold set " +
                        "IsDeleted = @IsDeleted, " +
                        "DeletedTime = @DeletedTime, " +
                        "DeletedBy_Id = @DeletedBy_Id " +
                    "Where ID = @Id";

            await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
            {
                IsDeleted = true,
                DeletedTime = DateTime.Now,
                DeletedBy_Id = pegawaiId,
                req.Id
            }));
        }


        #endregion
    }
}
