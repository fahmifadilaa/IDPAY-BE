using Ekr.Business.Contracts.DataMaster;
using Ekr.Core.Configuration;
using Ekr.Core.Entities.DataMaster.ProfileSetting.ViewModel;
using Ekr.Core.Entities.DataMaster.User;
using Ekr.Core.Entities.DataMaster.Utility.ViewModel;
using Ekr.Repository.Contracts.DataMaster.User;
using Ekr.Repository.Contracts.DataMaster.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace Ekr.Business.DataMaster
{
    public class ProfileSettingService : IProfileSettingService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUtilityRepository _utilityRepository;
        private readonly ErrorMessageConfig _ErrorMessageConfig;
        private readonly successMessageConfig _SuccessMessageConfig;

        public ProfileSettingService(IUserRepository userRepository, IUtilityRepository utilityRepository,IOptions<ErrorMessageConfig> options2,
            IOptions<successMessageConfig> options3)
        {
            _userRepository = userRepository;
            _utilityRepository = utilityRepository;
            _ErrorMessageConfig = options2.Value;
            _SuccessMessageConfig = options3.Value;
        }
        public async Task<bool> SubmitSettingProfile(SubmitProfileVM submitProfileSettingVM)
        {
            var pegawai = await _userRepository.UpdateProfilePegawai(submitProfileSettingVM);
            if (pegawai == true)
            {
                var user = await _userRepository.UpdateProfileUser(submitProfileSettingVM);
                return user;
            }

            return false;
        }
        public async Task<(bool, string)> ChangeUserPassword(ChangeUserPasswordVM changeUserPasswordVM)
        {
            if (changeUserPasswordVM.PasswordLama != changeUserPasswordVM.Password)
            {
                return (false, _ErrorMessageConfig.ChangePasswordSalah);
            }

            if (changeUserPasswordVM.PasswordBaru != changeUserPasswordVM.ConfirmPasswordBaru)
            {
                return (false, _ErrorMessageConfig.ConformPasswordSalah);
            }

            var changepw = await _userRepository.ChangeUserPassword(changeUserPasswordVM);

            return (changepw, changepw==true?"success":"failed");
        }
        public async Task<(bool, string)> ChangeUserPhoto(IFormFile FilePhoto, string PegawaiId)
        {
            try
            {
                //Get allowed extention
                SystemParameterVM systemParameter = await _utilityRepository.SelectSystemParameter(new SystemParameterFilterVM { KataKunci = "AllowedFileUploadImages" });

                string AllowedFileUploadType = systemParameter.Value;

                string Ext = Path.GetExtension(FilePhoto.FileName);

                //Validate Upload
                if (!AllowedFileUploadType.Contains(Ext))
                {
                    return (false, _ErrorMessageConfig.ImageTidakSesuai);
                }

                var SubPathFolder = "Photo";
                var Pathfolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\" + SubPathFolder);

                if (!Directory.Exists(Pathfolder))
                {
                    Directory.CreateDirectory(Pathfolder);
                }

                var fileNameReplaceSpace = FilePhoto.FileName.Replace(" ", "_");
                var path = Path.Combine(Pathfolder, fileNameReplaceSpace);
                using (System.IO.Stream stream = new FileStream(path, FileMode.Create))
                {
                    FilePhoto.CopyTo(stream);
                }

                var updatePhoto = await _userRepository.UpdatePhotoTblPegawai(new TblPegawai {
                    Id = int.Parse(PegawaiId),
                    Images = path
                });
                return (updatePhoto, updatePhoto == true ? "success" : "failed");

            }
            catch (Exception err)
            {
                return (false, _ErrorMessageConfig.InternalError);
            }
        }
    }
}
