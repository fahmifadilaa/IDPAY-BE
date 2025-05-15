using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ekr.Core.Entities.DataMaster.ProfileSetting.ViewModel
{
    public class DataProfileFilterVM
    {
        public int PegawaiId { get; set; }
    }

    public class DataProfileVM
    {
        public int PegawaiId { get; set; }

        public string Pegawai_Id { get; set; }
        public string NIK { get; set; }
        public string Nama_Pegawai { get; set; }
        public string Unit_Id { get; set; }

        public string Nama_Unit { get; set; }

        public string Jenis_Kelamin { get; set; }
        public string Role_Id { get; set; }
        public string Role_Unit_Id { get; set; }

        public string Role_Nama_Unit { get; set; }

        public string User_Id { get; set; }
        public string Nama_Role { get; set; }
        public string Status_Role { get; set; }
        public string User_Role_Id { get; set; }

        public string Images_User { get; set; }
        public string Email { get; set; }

        public string Password { get; set; }
        public bool? IsActive { get; set; }
        public bool? LDAPLogin { get; set; }
    }
}
