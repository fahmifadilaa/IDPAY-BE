using Ekr.Core.Entities.DataMaster.Utility;
using Ekr.Core.Entities.SettingThreshold;
using Ekr.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ekr.Core.Entities.DataMaster.Utility.ViewModel;

namespace Ekr.Repository.Contracts.Setting
{
    public interface ISettingThresholdRepository
    {
        Task<TblSettingThresholdVM> InsertSettingThresholdAsync(Tbl_Setting_Threshold req);
        Task<string> GetProbabilityDivision(string nik);
        Task<GridResponse<SettingThresholdData>> GetSettingThresholdList(SettingThresholdFilter filter, int currentUserId);
        Task<TblSettingThresholdVM> GetById(SettingThresholdRequest req);
        Task<TblSettingThresholdVM> UpdateSettingTreshold(Tbl_Setting_Threshold req);
        Task DeleteSettingTreshold(SettingThresholdRequest req, int PegawaiId);
        Task<GridResponse<DataDropdownServerSide>> GetListPenyelia(int unitId);
        Task<GridResponse<DataDropdownServerSide>> GetListPenyelia2(int unitId, string npp);
        Task<GridResponse<DataDropdownServerSide>> GetListPemimpin(int unitId);
        Task<GridResponse<DataDropdownServerSide>> GetListPemimpin2(int unitId, string npp);
        Task<GridResponse<DataDropdownServerSide>> GetDropdownTreshold(DropdownLookupFilterVM request);
        Task<TblSettingThresholdLogVM> InsertSettingThresholdLogAsync(Tbl_Setting_Threshold_Log req);
        Task UpdateSettingTresholdStatusAsync(SettingThresholdStatusRequest req, int updatedById);
        Task<GridResponse<TblSettingThresholdLogVM>> GetSettingTresholdLogAsync(int thresholdId);
    }
}
