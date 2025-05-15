using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ekr.Core.Entities.DataMaster.ProfileSetting.ViewModel
{
    public class SubmitProfileVM
    {
        public string Nama_Pegawai { get; set; }
        public string NIK { get; set; }
        public string Email { get; set; }
        public int PegawaiId { get; set; }

        public string UpdatedByPegawaiId { get; set; }
    }
    public class ChangeUserPasswordVM
    {
        public int PegawaiId { get; set; }
        public string Password { get; set; }
        public string PasswordLama { get; set; }
        public string PasswordBaru { get; set; }
        public string ConfirmPasswordBaru { get; set; }

        public string UpdatedByPegawaiId { get; set; }
    }

    public class UpdateBaseLDAPPegawai
    {
        public int PegawaiId { get; set; }
        public int Unit_Id { get; set; }
        public int Role_Id { get; set; }
    }
}
