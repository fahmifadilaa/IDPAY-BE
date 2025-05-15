using Ekr.Core.Entities;
using Ekr.Core.Entities.DataKTP;
using Ekr.Core.Entities.DataMaster.ProfileSetting.ViewModel;
using Ekr.Core.Entities.DataMaster.User;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ekr.Repository.Contracts.DataMaster.User
{
    public interface IUserRepository
    {
        #region Get
        Task<GridResponse<UserResponseVM>> GridGetAll(UserFilter req, int unitId, int roleId);
        Task<GridResponse<UserResponseVM>> GetDataById(int Id);
        Task<TblPegawai> GetDataPegawai(string nik);
        Task<TblPegawai> GetDataPegawaiById(int id);
        Task<TblPegawaiMutasiLog> GetDataPegawaiMutasi(int PegawaiId);
        Task<TblUser> GetDataUser(int PegawaiId);
        Task<TblRolePegawai> GetDataRole(int PegawaiId);
        Task<GridResponse<UserFingerVM>> GetDataUserFinger(int UserId);
        Task<List<RoleUserVM>> GridGetAllRole(int UserId, string Date);
        Task<GridResponse<Tbl_DataKTP_Demografis>> GetDataPegawaiDemografi(PegawaiDemografi req);
        #endregion

        #region Create
        Task<TblPegawai> CreateTblPegawai(UserVM req);
        Task<TblUser> CreateTblUser(UserVM req, int pegawaiId, bool isEncrypt);
        Task<TblRolePegawai> CreateTblRolePegawai(UserVM req, int Id_Pegawai);
        Task<TblPegawaiMutasiLog> CreateMutationUser(UserMutateVM req, int CreateBy);
        #endregion

        #region Update
        Task<TblPegawai> UpdateTblPegawai(UserVM req);
        Task<TblUser> UpdateTblUser(UserVM req, int PegawaiId, bool isEncrypt);
        Task<TblRolePegawai> UpdateTblRolePegawai(UserVM req, int PegawaiId);
        Task<List<TblRolePegawai>> UpdateRolePegawai(RoleUserUpdateReqVM req, string[] appItems);
        Task<TblPegawaiMutasiLog> UpdateMutationUser(UserMutateVM req, int UpdateBy);
        Task<bool> UpdateProfilePegawai(SubmitProfileVM req);
        Task<bool> UpdateProfileUser(SubmitProfileVM req);
        Task<bool> ChangeUserPassword(ChangeUserPasswordVM req);
        Task<bool> UpdatePhotoTblPegawai(TblPegawai tblPegawai);
        Task<DataProfileVM> GetProfile(DataProfileFilterVM dataProfileFilterVM);
        Task<bool> UpdateOnlyRolePegawai(int Role, int Id);
        Task<bool> UpdateOnlyUnitPegawai(int UnitId, int Id);
        #endregion

        #region Delete
        Task<bool> Delete(int Id);
        Task<bool> DeleteRole(int Id);
        #endregion
    }
}
