using Ekr.Core.Entities.SettingThreshold;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ekr.Business.Contracts.SettingThreshold
{
    public interface ISettingThresholdService
    {
        Task<TblSettingThresholdVM> InsertSettingThresholdAsync(Tbl_Setting_Threshold req);
        Task UpdateSettingThresholdStatusAsync(SettingThresholdStatusRequest req, int updatedById);
    }
}
