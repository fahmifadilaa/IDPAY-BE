using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ekr.Core.Entities.DataMaster.User
{
    public class UserVM
    {
        [RegularExpression("^[0-9-]*$", ErrorMessage = "Bad Request")]
        public Int64 Number { get; set; }
        [RegularExpression("^[0-9-]*$", ErrorMessage = "Bad Request")]
        public int Id { get; set; }
        [RegularExpression("^[0-9-]*$", ErrorMessage = "Bad Request")]
        public int? UnitId { get; set; }
        [RegularExpression("^[0-9-]*$", ErrorMessage = "Bad Request")]
        public int ApplicationId { get; set; }
        [RegularExpression("^[0-9-]*$", ErrorMessage = "Bad Request")]
        public int StatusRole { get; set; }
        [RegularExpression("^[-a-zA-Z0-9,/'. ]*$", ErrorMessage = "Bad Request")]
        public string UnitName { get; set; }
        [RegularExpression("^[-a-zA-Z0-9,/'._ ]*$", ErrorMessage = "Bad Request")]
        public string? Password { get; set; }
        [RegularExpression("^[0-9-]*$", ErrorMessage = "Bad Request")]
        public int? RoleId { get; set; }
        [RegularExpression("^[0-9-]*$", ErrorMessage = "Bad Request")]
        public int? IdJenisKelamin { get; set; }
        [RegularExpression("^[0-9]*$", ErrorMessage = "Bad Request")]
        public string Nik { get; set; }
        [RegularExpression("^[-a-zA-Z,/'. ]*$", ErrorMessage = "Bad Request")]
        public string Nama { get; set; }
        [RegularExpression("^[-a-zA-Z0-9,/'.: ]*$", ErrorMessage = "Bad Request")]
        public string Alamat { get; set; }
        [RegularExpression("^[-a-zA-Z0-9,/'. ]*$", ErrorMessage = "Bad Request")]
        public string Kode_Anggota { get; set; }
        [RegularExpression("^[-a-zA-Z0-9,/'. ]*$", ErrorMessage = "Bad Request")]
        public string Kode_Divisi { get; set; }
        [RegularExpression("^[-a-zA-Z0-9,/'. ]*$", ErrorMessage = "Bad Request")]
        public string Roles { get; set; }
        [RegularExpression("^[-a-zA-Z0-9,/'. ]*$", ErrorMessage = "Bad Request")]
        public string LastLoginConvert { get; set; }
        [RegularExpression("^[a-zA-Z0-9@._]*$", ErrorMessage = "Bad Request")]
        public string Email { get; set; }
        [RegularExpression("^[-a-zA-Z0-9,/'. ]*$", ErrorMessage = "Bad Request")]
        public string Status { get; set; }
        [RegularExpression("^[-a-zA-Z,/'. ]*$", ErrorMessage = "Bad Request")]
        public string Jenis_Kelamin { get; set; }
        //[RegularExpression("^[0-9,:. ]*$", ErrorMessage = "Bad Request")]
        public DateTime? Lastlogin { get; set; }
        [RegularExpression("^[-a-zA-Z0-9,/'.+=_ ]*$", ErrorMessage = "Bad Request")]
        public string Images { get; set; }
        [RegularExpression("^[-0-9/, ]*$", ErrorMessage = "Bad Request")]
        public string TanggalLahir { get; set; }
        [RegularExpression("^[-0-9,+ ]*$", ErrorMessage = "Bad Request")]
        public string NoHp { get; set; }
        [RegularExpression("^[-a-zA-Z0-9,/'.+=_: ]*$", ErrorMessage = "Bad Request")]
        public string Created_Time { get; set; }
        [RegularExpression("^[a-zA-Z0-9,/'.+=_: ]*$", ErrorMessage = "Bad Request")]
        public string Created_By { get; set; }
        [RegularExpression("^[-a-zA-Z0-9,/'.+=_: ]*$", ErrorMessage = "Bad Request")]
        public string Updated_Time { get; set; }
        [RegularExpression("^[a-zA-Z0-9,/'.+=_: ]*$", ErrorMessage = "Bad Request")]
        public string Updated_By { get; set; }
        [RegularExpression("^[a-zA-Z0-9,/'.+=_: ]*$", ErrorMessage = "Bad Request")]
        public string Deleted_By { get; set; }
        [RegularExpression("^[-a-zA-Z0-9,/'.+=_: ]*$", ErrorMessage = "Bad Request")]
        public string Deleted_Time { get; set; }
        [RegularExpression("^[a-zA-Z0-9]*$", ErrorMessage = "Bad Request")]
        public bool? IsActive { get; set; }
        [RegularExpression("^[a-zA-Z0-9]*$", ErrorMessage = "Bad Request")]
        public bool? Ldaplogin { get; set; }
        [RegularExpression("^[a-zA-Z0-9,/'.+=_: ]*$", ErrorMessage = "Bad Request")]
        public string State { get; set; }
        [RegularExpression("^[a-zA-Z0-9,/'.+=_: ]*$", ErrorMessage = "Bad Request")]
        public string ListDataRole { get; set; }
    }

    public class UserPasswordVM
    {
        public int Id { get; set; }
        public string? OldPassword { get; set; }
        public string? NewPassword { get; set; }
    }

    public class UserResponseVM
    {
        public Int64 Number { get; set; }
        public int Id { get; set; }
        public int? UnitId { get; set; }
        public int ApplicationId { get; set; }
        public string ApplicationName { get; set; }
        public int StatusRole { get; set; }
        public string StatusRoleName { get; set; }
        public string UnitName { get; set; }
        public string Password { get; set; }
        public int? RoleId { get; set; }
        public int? IdJenisKelamin { get; set; }
        public string Nik { get; set; }
        public string Nama { get; set; }
        public string Alamat { get; set; }
        public string Kode_Anggota { get; set; }
        public string Kode_Divisi { get; set; }
        public string Roles { get; set; }
        public string LastLoginConvert { get; set; }
        public string Email { get; set; }
        public string Status { get; set; }
        public string Jenis_Kelamin { get; set; }
        public DateTime? Lastlogin { get; set; }
        public string Images { get; set; }
        public string TanggalLahir { get; set; }
        public string NoHp { get; set; }
        public string Created_Time { get; set; }
        public string Created_By { get; set; }
        public string Updated_Time { get; set; }
        public string Updated_By { get; set; }
        public string Deleted_By { get; set; }
        public string Deleted_Time { get; set; }
        public bool? IsActive { get; set; }
        public bool? Ldaplogin { get; set; }
        public string State { get; set; }
        public string ListDataRole { get; set; }
        public int? TypeUnitId { get; set; }
        public string TypeNamaUnit { get; set; }
    }

    public partial class TblPegawai
    {
        public TblPegawai()
        {
            TblRolePegawai = new HashSet<TblRolePegawai>();
            TblUser = new HashSet<TblUser>();
        }

        public int Id { get; set; }
        public int? Unit_Id { get; set; }
        public int? Role_Id { get; set; }
        public int? Id_JenisKelamin { get; set; }
        public int? Agama_Id { get; set; }
        public int? PendidikanAkhir_Id { get; set; }
        public string Nik { get; set; }
        public string Nama { get; set; }
        public string Tempat_Lahir { get; set; }
        public DateTime? Tanggal_Lahir { get; set; }
        public string Alamat { get; set; }
        public string Email { get; set; }
        public DateTime? Lastlogin { get; set; }
        public string Images { get; set; }
        public string No_HP { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? Created_Date { get; set; }
        public int? CreatedBy_Id { get; set; }
        public DateTime? Updated_Date { get; set; }
        public int? UpdatedBy_Id { get; set; }
        public DateTime? Delete_Date { get; set; }
        public int? DeleteBy_Id { get; set; }
        public bool? IsDeleted { get; set; }
        public bool? Ldaplogin { get; set; }
        public bool? IsIe { get; set; }

        public virtual ICollection<TblRolePegawai> TblRolePegawai { get; set; }
        public virtual ICollection<TblUser> TblUser { get; set; }
    }

    public partial class TblRolePegawai
    {
        public int Id { get; set; }
        public int? Id_Pegawai { get; set; }
        public int? Role_Id { get; set; }
        public int? Unit_Id { get; set; }
        public int? Application_Id { get; set; }
        public int? CreatedBy_Id { get; set; }
        public DateTime? CreatedTime { get; set; }
        public int? StatusRole { get; set; }
        public DateTime? DateStart { get; set; }
        public DateTime? DateEnd { get; set; }
        public int? UpdatedBy_Id { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public bool? IsDeleted { get; set; }
        public virtual TblPegawai IdPegawaiNavigation { get; set; }
    }

    public partial class TblPegawaiMutasiLog
    {
        public int Id { get; set; }
        public int? Pegawai_Id { get; set; }
        public int? Unit_Id { get; set; }
        public int? Role_Id { get; set; }
        public string NIK { get; set; }
        public bool? IsLast { get; set; }
        public DateTime? Created_Date { get; set; }
        public string Created_By { get; set; }
        public DateTime? Update_Date { get; set; }
        public string Updated_By { get; set; }
    }

    public partial class TblUser
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public int Pegawai_Id { get; set; }
        public DateTime? CreatedTime { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public int? CreatedBy_Id { get; set; }
        public int? UpdatedBy_Id { get; set; }
        public DateTime? LastLogin { get; set; }

        public virtual TblPegawai Pegawai { get; set; }
    }

    public class UserFingerVM
    {
        public int? PegawaiId { get; set; }
        public int? TipeTanganId { get; set; }
        public int? Order_By { get; set; }
        public int? TypeFingerId { get; set; }
        public string Nama { get; set; }
        public string FileName { get; set; }
        public string file { get; set; }
    }
    public class UserMutateVM
    {
        public int PegawaiId { get; set; }
        public int UnitId { get; set; }
    }
    public class UserFingerReqVM
    {
        public int? PegawaiId { get; set; }
        public int? TypeFingerId { get; set; }
        public string File { get; set; }
    }

    public class RoleUserVM
    {
        public int Id { get; set; }
        public int RoleDivisiId { get; set; }
        public string Unit_Name { get; set; }
        public int Role_Id { get; set; }
        public string Role_Name { get; set; }
        public int Status_Role { get; set; }
        public string Status_Role_Name { get; set; }
        public string Role { get; set; }
        public string Tanggal { get; set; }
        public string Tanggal_Awal { get; set; }
        public string Tanggal_Akhir { get; set; }
        public string AppName { get; set; }

        public RoleUserVM()
        {
            //baru
            Status_Role = 2;
        }
    }

    public class RoleUserReqVM
    {
        public string ListDataRole { get; set; }
        public string State { get; set; }
        public int UserId { get; set; }
    }
    public class RoleUserUpdateReqVM
    {
        public RoleUserVM Model { get; set; }
        public string Apps { get; set; }
        public int StatusRole { get; set; }
        public int UserId { get; set; }
    }
}
