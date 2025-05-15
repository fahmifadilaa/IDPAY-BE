using Ekr.Core.Entities.DataMaster.ProfileSetting.ViewModel;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ekr.Business.Contracts.DataMaster
{
    public interface IProfileSettingService
    {
        Task<bool> SubmitSettingProfile(SubmitProfileVM submitProfileSettingVM);
        Task<(bool, string)> ChangeUserPassword(ChangeUserPasswordVM changeUserPasswordVM);
        Task<(bool, string)> ChangeUserPhoto(IFormFile FilePhoto, string PegawaiId);
    }
}
