using Ekr.Business.Contracts.SettingThreshold;
using Ekr.Core.Entities.SettingThreshold;
using Ekr.Core.Enums;
using Ekr.Repository.Contracts.Setting;
using System.Threading.Tasks;

namespace Ekr.Business.SettingThreshold
{
    public class SettingThresholService : ISettingThresholdService
    {
        private readonly ISettingThresholdRepository _settingThresholdRepository;

        public SettingThresholService(ISettingThresholdRepository settingThresholdRepository)
        {
            _settingThresholdRepository = settingThresholdRepository;
        }

        public async Task<TblSettingThresholdVM> InsertSettingThresholdAsync(Tbl_Setting_Threshold req)
        {
            var resp = await _settingThresholdRepository.InsertSettingThresholdAsync(req);
            await _settingThresholdRepository.InsertSettingThresholdLogAsync(new Tbl_Setting_Threshold_Log
            {
                Alasan = req.Keterangan,
                Threshold_Id = resp.Id,
                CreatedBy_Id = req.CreatedBy_Id,
                Status = (int)SettingThresholdApprovalStatus.MenungguApprovalPenyelia
            });

            return resp;
        }

        public async Task UpdateSettingThresholdStatusAsync(SettingThresholdStatusRequest req, int updatedById)
        {
            await _settingThresholdRepository.UpdateSettingTresholdStatusAsync(req, updatedById);
            await _settingThresholdRepository.InsertSettingThresholdLogAsync(new Tbl_Setting_Threshold_Log
            {
                Alasan = req.Alasan,
                Threshold_Id = req.Id,
                CreatedBy_Id = updatedById,
                Status = req.Status
            });
        }

    }
}
