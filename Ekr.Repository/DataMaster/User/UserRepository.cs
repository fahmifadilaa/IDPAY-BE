using Dapper;
using Ekr.Core.Entities;
using Ekr.Core.Entities.DataKTP;
using Ekr.Core.Entities.DataMaster.ProfileSetting.ViewModel;
using Ekr.Core.Entities.DataMaster.User;
using Ekr.Dapper.Connection.Contracts.Sql;
using Ekr.Repository.Contracts.DataMaster.User;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Threading.Tasks;

namespace Ekr.Repository.DataMaster.User
{
    public class UserRepository : BaseRepository, IUserRepository
    {
        public UserRepository(IEKtpReaderBackendDb con) : base(con) { }

        #region Get
        public async Task<GridResponse<UserResponseVM>> GridGetAll(UserFilter req, int unitId, int roleId)
        {
            const string sp = "[ProcManageUserData]";
            var values = new
            {
                Nik = req.NikSearchParam,
                Nama = req.NamaSearchParam,
                UnitId = unitId,
                RoleId = roleId,
                SColumn = req.SortColumn,
                SColumnValue = req.SortColumnDir,
                Page = req.PageNumber,
                Rows = req.PageSize
            };

            var data = Db.WithConnectionAsync(db =>
            db.QueryAsync<UserResponseVM>(sp, values, commandType: CommandType.StoredProcedure));

            const string spCount = "[ProcManageUserTotal]";
            var valuesCount = new
            {
                Nik = req.NikSearchParam,
                Nama = req.NamaSearchParam,
                UnitId = unitId,
                RoleId = roleId,
            };

            var count = Db.WithConnectionAsync(db =>
            db.ExecuteScalarAsync<int>(spCount, valuesCount, commandType: CommandType.StoredProcedure));

            await Task.WhenAll(data, count)
                .ConfigureAwait(false);

            return new GridResponse<UserResponseVM>
            {
                Count = count.Result,
                Data = data.Result
            };
        }

        public async Task<GridResponse<UserResponseVM>> GetDataById(int Id)
        {
            const string query = "[ProcManageUserGetDataById]";
            var values = new
            {
                Id
            };
            var data = Db.WithConnectionAsync(db =>
            db.QueryAsync<UserResponseVM>(query, values, commandType: CommandType.StoredProcedure));
            return new GridResponse<UserResponseVM>
            {
                Count = 1,
                Data = data.Result
            };
        }

        public async Task<DataProfileVM> GetProfile(DataProfileFilterVM dataProfileFilterVM)
        {
            const string query = "[ProcProfileGetData]";
            var values = new
            {
                dataProfileFilterVM.PegawaiId
            };
            var data =  await Db.WithConnectionAsync(db => db.QueryFirstOrDefaultAsync<DataProfileVM>(query, values, commandType: CommandType.StoredProcedure));

            return data;
        }

        public async Task<GridResponse<Tbl_DataKTP_Demografis>> GetDataPegawaiDemografi(PegawaiDemografi req)
        {
            const string sp = "[ProcDataPegawaiDemografi]";
            var values = new
            {
                req.Npp,
                SColumn = req.SortColumn,
                SColumnValue = req.SortColumnDir,
                Page = req.PageNumber,
                Rows = req.PageSize
            };
            var data = await Db.WithConnectionAsync(db => db.QueryAsync<Tbl_DataKTP_Demografis>(sp, values, commandType: CommandType.StoredProcedure));

            const string spCount = "[ProcDataPegawaiDemografiNum]";
            var valuesCount = new
            {
                req.Npp
            };
            var count = await Db.WithConnectionAsync(db => db.ExecuteScalarAsync<int>(spCount, valuesCount, commandType: CommandType.StoredProcedure));

            return new GridResponse<Tbl_DataKTP_Demografis>
            {
                Count = count,
                Data = data
            };
        }
        #endregion

        #region Get Data User Finger
        public async Task<GridResponse<UserFingerVM>> GetDataUserFinger(int UserId)
        {
            const string query = "[ProcDataPenggunaListFinger]";
            var values = new
            {
                UserId
            };
            var data = Db.WithConnectionAsync(db =>
            db.QueryAsync<UserFingerVM>(query, values, commandType: CommandType.StoredProcedure));

            return new GridResponse<UserFingerVM>
            {
                Count = 1,
                Data = data.Result
            };
        }
        #endregion

        #region Get Data Pegawai
        public Task<TblPegawai> GetDataPegawai(string nik)
        {
            const string query = "select * from [dbo].[Tbl_Pegawai] where nik = @Nik and isdeleted = 0";

            return Db.WithConnectionAsync(db => db.QueryFirstOrDefaultAsync<TblPegawai>(query, new { nik }));
        }

        public Task<TblPegawaiMutasiLog> GetDataPegawaiMutasi(int PegawaiId)
        {
            const string query = "select * from [dbo].[Tbl_PegawaiMutasi_Log] where [Pegawai_Id] = @PegawaiId and [IsLast] = 1";

            return Db.WithConnectionAsync(db => db.QueryFirstOrDefaultAsync<TblPegawaiMutasiLog>(query, new { PegawaiId }));
        }

        public Task<TblPegawai> GetDataPegawaiById(int id)
        {
            const string query = "select * from [dbo].[Tbl_Pegawai] where Id = @id and isdeleted = 0";

            return Db.WithConnectionAsync(db => db.QueryFirstOrDefaultAsync<TblPegawai>(query, new { id }));
        }
        #endregion

        #region Get Data User
        public Task<TblUser> GetDataUser(int PegawaiId)
        {
            const string query = "select * from [dbo].[Tbl_User] where Pegawai_Id = @PegawaiId";

            return Db.WithConnectionAsync(db => db.QueryFirstOrDefaultAsync<TblUser>(query, new { PegawaiId }));
        }
        #endregion

        #region Get Data User Role
        public async Task<List<RoleUserVM>> GridGetAllRole(int UserId, string Date)
        {
            const string sp = "[ProcManageUserGetDataRoleByPegawaiId]";
            var values = new
            {
                Pegawai_Id = UserId,
                Date = Date
            };

            var data = Db.WithConnectionAsync(db => db.QueryAsync<List<RoleUserVM>>(sp, values, commandType: CommandType.StoredProcedure));

            await Task.WhenAll(data).ConfigureAwait(false);

            return (List<RoleUserVM>)data.Result;
        }
        public Task<TblRolePegawai> GetDataRole(int PegawaiId)
        {
            const string query = "select * from [dbo].[Tbl_Role_Pegawai] where id_pegawai = @PegawaiId and isdeleted = 0 and StatusRole = 1 and Application_Id = 1";

            return Db.WithConnectionAsync(db => db.QueryFirstOrDefaultAsync<TblRolePegawai>(query, new { PegawaiId }));
        }
        #endregion

        #region Create Tbl Pegawai
        public async Task<TblPegawai> CreateTblPegawai(UserVM req)
        {
            int IsActive = 0;
            int? LDAPLogin = 1;
            if (req.IsActive == true)
            {
                IsActive = 1;
            }
            else if(req.Ldaplogin != null)
            {
                if(req.Ldaplogin == true)
                {
                    LDAPLogin = 1;
                }
                else
                {
                    LDAPLogin = 0;
                }
            }
            else
            {
                IsActive = 0;
            }
            var Created_Time = DateTime.Now;
            const string query = "Insert Into Tbl_Pegawai (" +
            "[Unit_Id]," +
            "[Role_Id]," +
            "[Id_JenisKelamin]," +
            "[NIK]," +
            "[Nama]," +
            "[Alamat]," +
            "[Email]," +
            "[Images]," +
            "[No_HP]," +
            "[IsActive]," +
            "[LDAPLogin]," +
            "[CreatedBy_Id]," +
            "[Created_Date]) " +
            "OUTPUT INSERTED.Id " +
        "values(" +
            "@UnitId," +
            "@RoleId," +
            "@IdJenisKelamin," +
            "@Nik," +
            "@Nama," +
            "@Alamat," +
            "@Email," +
            "@Images," +
            "@NoHp," +
            "@IsActive," +
            "@LDAPLogin," +
            "@Created_By," +
            "@Created_Time)";

            await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
            {
                req.UnitId,
                req.RoleId,
                req.IdJenisKelamin,
                Nik = new DbString { Value= req.Nik, Length = 80 },
                Nama = new DbString { Value = req.Nama, Length = 300 },
                ALamat = new DbString { Value = req.Alamat, Length = 150 },
                Email = new DbString { Value = req.Email, Length = 50 },
                Images = new DbString { Value = req.Images, Length = 2000 },
                NoHp = new DbString { Value = req.NoHp, Length = 25 },
                IsActive,
                LDAPLogin = 1,
                req.Created_By,
                Created_Time
            }));

            var _pegawai = await GetDataPegawai(req.Nik);

            return _pegawai;
        }
        #endregion

        #region create Pegawai Mutasi
        public async Task<TblPegawaiMutasiLog> CreateMutationUser(UserMutateVM req, int CreateBy)
        {
            var Created_Time = DateTime.Now;
            const string query = "Insert Into [Tbl_PegawaiMutasi_Log] (" +
            "[Pegawai_Id]," +
            "[Unit_Id]," +
            "[IsLast]," +
            "[Created_By]," +
            "[Created_Date])" +
        "values(" +
            "@PegawaiId," +
            "@UnitId," +
            "@IsLast," +
            "@CreateBy," +
            "@Created_Time)";

            await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
            {
                req.PegawaiId,
                req.UnitId,
                IsLast = 1,
                CreateBy,
                Created_Time
            }));

            var _pegawai = await GetDataPegawaiMutasi(req.PegawaiId);

            return _pegawai;
        }
        #endregion

        #region Create Tbl User
        public async Task<TblUser> CreateTblUser(UserVM req, int pegawaiId, bool isEncrypt=false)
        {
            var pass = req.Password;
            if (isEncrypt)
            {
                pass = BCrypt.Net.BCrypt.HashPassword(req.Password);
            }
            const string query = "Insert Into Tbl_User (" +
            "[Pegawai_Id]," +
            "[Username]," +
            "[Password]," +
            "[CreatedTime]," +
            "[CreatedBy_Id]) " +
            "OUTPUT INSERTED.Id " +
        "values(" +
            "@pegawaiId," +
            "@Nik," +
            "@Password," +
            "@CreatedTime," +
            "@Created_By)";

            var id = await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
            {
                pegawaiId,
                Nik = new DbString { Value= req.Nik, Length=250 },
                Password = pass,
                CreatedTime = DateTime.Now,
                req.Created_By
            }));

            var _user = new TblUser
            {
                Id = id,
                Pegawai_Id = pegawaiId,
                Password = pass,
                CreatedBy_Id = string.IsNullOrWhiteSpace(req.Created_By) ? null : int.Parse(req.Created_By),
                CreatedTime = DateTime.Now,
            };

            //var _user = await GetDataUser(pegawaiId);

            return _user;
        }
        #endregion

        #region Create Tbl Role Pegawai
        public async Task<TblRolePegawai> CreateTblRolePegawai(UserVM req, int Id_Pegawai)
        {
            var Created_Time = DateTime.Now;
            const string query = "Insert Into Tbl_Role_Pegawai (" +
            "[Id_Pegawai]," +
            "[Role_Id]," +
            "[Unit_Id]," +
            "[Application_Id]," +
            "[StatusRole]," +
            "[CreatedTime]," +
            "[CreatedBy_Id]) " +
            "OUTPUT INSERTED.Id " +
        "values(" +
            "@Id_Pegawai," +
            "@RoleId," +
            "@UnitId," +
            "@appId," +
            "@stats," +
            "@Created_Time," +
            "@Created_By)";

            var id = await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
            {
                Id_Pegawai,
                req.RoleId,
                req.UnitId,
                appId = req.ApplicationId,
                stats = req.StatusRole,
                Created_Time,
                req.Created_By
            }));

            var _role = new TblRolePegawai
            {
                Id_Pegawai = Id_Pegawai,
                Application_Id = req.ApplicationId,
                CreatedBy_Id = string.IsNullOrWhiteSpace(req.Created_By) ? null : int.Parse(req.Created_By),
                CreatedTime = Created_Time,
                Role_Id = req.RoleId,
                Unit_Id = req.UnitId,
                Id= id,
            };

            //var _role = await GetDataRole(Id_Pegawai);

            return _role;
        }
        #endregion

        #region Update
        public async Task<TblPegawai> UpdateTblPegawai(UserVM req)
        {
            int IsActive = 0;
            int LDAPLogin = 1;
            if (req.IsActive == true)
            {
                IsActive = 1;
                if (req.Ldaplogin == true)
                {
                    LDAPLogin = 1;
                }
                else
                {
                    LDAPLogin = 0;
                }
            }
            else
            {
                IsActive = 0;
            }
            var Updated_Time = DateTime.Now;
            const string query = "Update Tbl_Pegawai set " +
            "[Unit_Id] = @UnitId," +
            "[Role_Id] = @RoleId," +
            "[Id_JenisKelamin] = @IdJenisKelamin," +
            "[NIK] = @Nik ," +
            "[Nama] = @Nama," +
            "[Alamat] = @Alamat," +
            "[Email] = @Email," +
            "[Images] = @Images," +
            "[No_HP] = @NoHp," +
            "[IsActive] = @IsActive," +
            "[LDAPLogin] = @LDAPLogin," +
            "[UpdatedBy_Id] = @Updated_By," +
            "[Updated_Date] = @Updated_Time" +
            " where Id  = @Id ";

            await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
            {
                req.UnitId,
                req.RoleId,
                req.IdJenisKelamin,
                req.Nik,
                req.Nama,
                req.Alamat,
                req.Email,
                req.Images,
                req.NoHp,
                IsActive,
                LDAPLogin = 1,
                req.Updated_By,
                Updated_Time,
                req.Id
            }));

            var _data = new TblPegawai
            {
                Id = req.Id,
                Unit_Id = req.UnitId,
                Role_Id = req.RoleId,
                Id_JenisKelamin = req.IdJenisKelamin,
                Nik = req.Nik,
                Nama = req.Nama,
                Alamat = req.Alamat,
                Email = req.Email,
                Images = req.Images,
                No_HP = req.NoHp,
                IsActive = req.IsActive,
                Ldaplogin = req.Ldaplogin,
                UpdatedBy_Id = string.IsNullOrWhiteSpace(req.Updated_By) ? null : int.Parse(req.Updated_By),
                Updated_Date = Updated_Time,
            };

            //var _data = await GetDataPegawaiById(req.Id);

            return _data;
        }

        public async Task<TblPegawaiMutasiLog> UpdateMutationUser(UserMutateVM req, int UpdateBy)
        {
            const string query = "Update [Tbl_PegawaiMutasi_Log] set " +
            "[IsLast] = @IsLast," +
            "[Update_Date] = @UpdateDate," +
            "[Updated_By] = @UpdateBy" +
            " where Pegawai_Id = @PegawaiId and IsLast = 1";

            await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
            {
                IsLast = 0,
                UpdateDate = DateTime.Now,
                UpdateBy,
                req.PegawaiId
            }));

            var _data = await GetDataPegawaiMutasi(req.PegawaiId);

            return _data;
        }

        public async Task<TblRolePegawai> UpdateTblRolePegawai(UserVM req, int PegawaiId)
        {
            var Updated_Time = DateTime.Now;
            var IsDeleted = 1;
            const string query = "Update Tbl_Role_Pegawai set " +
            "[IsDeleted] = @IsDeleted," +
            "[UpdatedBy_Id] = @Updated_By," +
            "[UpdatedTime] = @Updated_Time" +
            " where Id_Pegawai = @PegawaiId and IsDeleted = 0";

            var id = await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
            {
                IsDeleted,
                req.Updated_By,
                Updated_Time,
                PegawaiId,
                req.RoleId
            }));

            //var _data = await GetDataRole(PegawaiId);

            var _data = new TblRolePegawai
            {
                Id = id,
                IsDeleted = true,
                UpdatedBy_Id = string.IsNullOrWhiteSpace(req.Updated_By) ? null : int.Parse(req.Updated_By),
                Id_Pegawai = PegawaiId,
                UpdatedTime = Updated_Time,
            };

            return _data;
        }

        public async Task<TblUser> UpdateTblUser(UserVM req, int PegawaiId, bool isEncrypt = false)
        {
            var pass = req.Password;

            if (req.Password == "" || req.Password == null)
            {
                var Updated_Time = DateTime.Now;
                const string query = "Update Tbl_User set " +
                "[Username] = @nik," +
                "[UpdatedBy_Id] = @Updated_By," +
                "[UpdatedTime] = @Updated_Time" +
                " where Pegawai_Id = @PegawaiId";

                await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
                {
                    req.Nik,
                    req.Updated_By,
                    Updated_Time,
                    PegawaiId
                }));

                //var _data = await GetDataUser(PegawaiId);
            }
            else
            {
                if (isEncrypt)
                {
                    pass = BCrypt.Net.BCrypt.HashPassword(req.Password);
                }
                var Updated_Time = DateTime.Now;
                const string query = "Update Tbl_User set " +
                "[Username] = @nik," +
                "[Password] = @Password," +
                "[UpdatedBy_Id] = @Updated_By," +
                "[UpdatedTime] = @Updated_Time" +
                " where Pegawai_Id = @PegawaiId";

                await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
                {
                    req.Nik,
                    Password = pass,
                    req.Updated_By,
                    Updated_Time,
                    PegawaiId
                }));
            }

            var _data = new TblUser
            {
                Id = req.Id,
                Username = req.Nik,
                Password = pass,
                UpdatedBy_Id = string.IsNullOrWhiteSpace(req.Updated_By) ? null : int.Parse(req.Updated_By),
                UpdatedTime = DateTime.Now,
                Pegawai_Id = PegawaiId
            };

            return _data;
        }

        public async Task<List<TblRolePegawai>> UpdateRolePegawai(RoleUserUpdateReqVM req, string[] appItems)
        {
            var datas = new List<TblRolePegawai>();
            var CreatedTime = DateTime.Now;
            foreach (var i in appItems)
            {
                if (req.Model.Tanggal != null && req.Model.Tanggal != "")
                {
                    var splitTanggal = req.Model.Tanggal.Replace(" ", "").Split("-");
                    var DateStart = DateTime.ParseExact(splitTanggal[0], "dd/MM/yyyy", CultureInfo.InvariantCulture);
                    var DateEnd = DateTime.ParseExact(splitTanggal[1], "dd/MM/yyyy", CultureInfo.InvariantCulture);
                }
                const string query = "Insert Into Tbl_Role_Pegawai (" +
                    "[Id_Pegawai]," +
                    "[Role_Id]," +
                    "[Application_Id]," +
                    "[StatusRole]," +
                    "[Created_Date]," +
                    "[CreatedBy_Id])" +
                "values(" +
                    "@UserId," +
                    "@Role_Id," +
                    "@appItems," +
                    "@Status_Role," +
                    "@CreatedTime," +
                    "@CreatedBy_Id)";

                await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
                {
                    req.UserId,
                    req.Model.Role_Id,
                    appItems,
                    req.Model.Status_Role,
                    CreatedTime,
                    CreatedBy_Id = req.UserId
                }));

                var data = await GetDataRole(req.UserId);
                datas.Add(data);
            }

            return datas;
        }

        public async Task<bool> UpdateProfilePegawai(SubmitProfileVM req)
        {
            const string query = "Update [Tbl_Pegawai] set " +
                        "Nama = @Nama_Pegawai, " +
                        "NIK = @NIK, " +
                        "Email = @Email, " +
                        "Updated_Date = @UpdatedTime, " +
                        "UpdatedBy_Id = @UpdatedByPegawaiId " +
                    "Where Id = @PegawaiId";

            var res = await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
            {
                req.Nama_Pegawai,
                req.NIK,
                req.Email,
                UpdatedTime = DateTime.Now,
                req.UpdatedByPegawaiId,
                req.PegawaiId
            }));

            return res>0;
        }

        public async Task<bool> UpdateProfileUser(SubmitProfileVM req)
        {
            const string query = "Update [Tbl_User] set " +
                        "Username = @NIK, " +
                        "UpdatedTime = @UpdatedTime, " +
                        "UpdatedBy_Id = @UpdatedByPegawaiId " +
                    "Where Pegawai_Id = @PegawaiId";

            var res = await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
            {
                req.NIK,
                UpdatedTime = DateTime.Now,
                req.UpdatedByPegawaiId,
                req.PegawaiId
            }));

            return res > 0;
        }

        public async Task<bool> ChangeUserPassword(ChangeUserPasswordVM req)
        {
            const string query = "Update [Tbl_User] set " +
                        "Password = @PasswordBaru, " +
                        "UpdatedTime = @UpdatedTime, " +
                        "UpdatedBy_Id = @UpdatedByPegawaiId " +
                    "Where Pegawai_Id = @PegawaiId";

            var res = await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
            {
                req.PasswordBaru,
                UpdatedTime = DateTime.Now,
                req.UpdatedByPegawaiId,
                req.PegawaiId
            }));

            return res > 0;
        }

        public async Task<bool> UpdatePhotoTblPegawai(TblPegawai tblPegawai)
        {
            const string query = "Update [Tbl_Pegawai] set " +
                        "Images = @Images, " +
                        "Updated_Date = @Updated_Date, " +
                        "UpdatedBy_Id = @UpdatedBy_Id " +
                    "Where Id = @Id";

            var res = await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
            {
                tblPegawai.Images,
                Updated_Date = DateTime.Now,
                UpdatedBy_Id = tblPegawai.Id,
                tblPegawai.Id
            }));

            return res > 0;
        }

        public async Task<bool> UpdateOnlyRolePegawai(int Role, int Id)
        {
            const string query = "Update [Tbl_Pegawai] set " +
                        "Role_Id = @Role, " +
                        "Updated_Date = @Updated_Date, " +
                        "UpdatedBy_Id = @UpdatedBy_Id " +
                    "Where Id = @Id";

            var res = await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
            {
                Role = Role,
                Updated_Date = DateTime.Now,
                UpdatedBy_Id = Id,
                Id
            }));

            const string query2 = "Update [Tbl_Role_Pegawai] set " +
                        "Role_Id = @Role, " +
                        "UpdatedTime = @Updated_Date, " +
                        "UpdatedBy_Id = @UpdatedBy_Id " +
                    "Where Id_Pegawai = @Id";

            var res2 = await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query2, new
            {
                Role = Role,
                Updated_Date = DateTime.Now,
                UpdatedBy_Id = Id,
                Id
            }));

            return res > 0;
        }

        public async Task<bool> UpdateOnlyUnitPegawai(int UnitId, int Id)
        {
            const string query = "Update [Tbl_Pegawai] set " +
                        "Unit_Id = @UnitId, " +
                        "Updated_Date = @Updated_Date, " +
                        "UpdatedBy_Id = @UpdatedBy_Id " +
                    "Where Id = @Id";

            var res = await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
            {
				UnitId,
                Updated_Date = DateTime.Now,
                UpdatedBy_Id = Id,
                Id
            }));

            const string query2 = "Update [Tbl_Role_Pegawai] set " +
                        "Unit_Id = @UnitId, " +
                        "UpdatedTime = @Updated_Date, " +
                        "UpdatedBy_Id = @UpdatedBy_Id " +
                    "Where Id_Pegawai = @Id";

            var res2 = await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query2, new
            {
				UnitId,
                Updated_Date = DateTime.Now,
                UpdatedBy_Id = Id,
                Id
            }));

            return res > 0;
        }
        #endregion

        #region Delete
        public async Task<bool> Delete(int Id)
        {
            var Delete_Date = DateTime.Now;
            const string query = "Update Tbl_Pegawai set " +
            "[IsDeleted] = 1," +
            "[Delete_Date] = @Delete_Date" +
            " where Id = @Id";

            await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
            {
                Delete_Date,
                Id
            }));

            return true;
        }

        public async Task<bool> DeleteRole(int Id)
        {
            var Delete_Date = DateTime.Now;
            const string query = "Update Tbl_Role_Pegawai set " +
            "[IsDeleted] = 1," +
            "[UpdatedTime] = @Delete_Date" +
            " where Id = @Id";

            await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
            {
                Delete_Date,
                Id
            }));

            return true;
        }
        #endregion
    }
}
