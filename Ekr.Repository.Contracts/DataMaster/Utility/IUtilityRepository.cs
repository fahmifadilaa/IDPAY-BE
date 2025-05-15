using Ekr.Core.Entities;
using Ekr.Core.Entities.DataMaster.Lookup;
using Ekr.Core.Entities.DataMaster.MasterAplikasi;
using Ekr.Core.Entities.DataMaster.Menu.Entity;
using Ekr.Core.Entities.DataMaster.Menu.ViewModel;
using Ekr.Core.Entities.DataMaster.User;
using Ekr.Core.Entities.DataMaster.Utility;
using Ekr.Core.Entities.DataMaster.Utility.ViewModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ekr.Repository.Contracts.DataMaster.Utility
{
    public interface IUtilityRepository
    {
        Task<List<GetMenuVM>> GetMenu(GetMenuFilterVM req);
        Task<TblMenu> GetMenuById(GetMenuByIdFilterVM req);
        Task<List<TblLookup>> SelectLookup(LookupFilterVM req);
        Task<List<TblLookup>> SelectLookupTypeByUnitId(string RoleId, string UnitId);
        Task<GridResponse<DataDropdownServerSide>> GetUnitByTypeAndUnitId(GetDataUnitByTypeAndUnitIdViewModel req);
        Task<GridResponse<DataDropdownServerSide>> DropdownPegawaiHirarki(DropdownMenuFilterVM req, string UnitId, string RoleId);
        Task<SystemParameterVM> SelectSystemParameter(SystemParameterFilterVM req);
        Task<GridResponse<DataDropdownServerSide>> GetRole(Utility2VM req);
        Task<GridResponse<DataDropdownServerSide>> GetUnit(Utility2VM req);
        Task<List<EligibleAppsVM>> GetEligibleApps(EligibleAppsFilterVM req);
        Task<GridResponse<LogActivityVM>> GetLogActivity(LogActivityFilterVM req);
        Task<GridResponse<LogNikInquiryVM>> GetLogNikInquiry(LogNikInquiryFilterVM req);
        Task<GridResponse<DataDropdownServerSide>> DropdownMenu(DropdownMenuFilterVM req);
        Task<bool> IsGetAccess(CheckAccessMenuFilterVM req);
        Task<List<DataDropdownServerSide>> DropdownAplikasi();
        bool UploadAppsVersion(Tbl_VersionAgent versionApps);
        Tbl_VersionAgent GetLatestAppsVersion();
        Tbl_VersionAgent GetAppsVersionById(int Id);
        Task<TblMasterAplikasi> GetMasterAplikasi(GetByIdVM req);
        Task<MenuByChangeAppsVM> ChangeApplication(GetByIdAppVM req);
        Task<GridResponse<DataDropdownServerSide>> GetAllDataMasterTypeFinger(UtilityVM req);
        Task<GridResponse<DataDropdownServerSide>> GetAllRolePegawai(int PegawaiId, int appId);
        Task<GridResponse<DataDropdownServerSide>> GetDropdownRolesByMenuId(int Roles);
        Task<GridResponse<DataDropdownServerSide>> GetDropdownAppsById(string apps);
        Task<GridResponse<Jumlah_Inbox>> GetCountDataEnroll(string uid, string PegawaiId, string unitId);
        Task<GridResponse<Jumlah_Inbox>> GetCountDataEnrollByUnitId(string UnitId);
        Task<GridResponse<Jumlah_Inbox>> GetCountDataEnrollByUnitIdJenis(string UnitId, string Jenis);
        Task<GridResponse<Jumlah_Inbox>> GetCountDataEnrollTemp(DataEnrollFilter req);
        Task<GridResponse<DataMaps_ViewModels>> GetDataMapsEnroll(string uid);
        Task<GridResponse<CekUsia_ViewModels>> CekUsia(string TglLahir);
        Task<GridResponse<DataDropdownServerSide>> SelectDataTypeFingerLogin(string username);
        Task<GridResponse<DataDropdownServerSide>> GetUnitById(string id);
        Task<int> GetCountDataReader(string UnitIds);
        Task<List<DataMapsVM>> GetMapsDataReader();
        Core.Entities.DataMaster.Utility.Entity.TblUserSession GetUserSession(int userId);
        bool UpdateUserSession(Core.Entities.DataMaster.Utility.Entity.TblUserSession userSession);
        bool InsertLogNIKInquiry(Core.Entities.DataMaster.Utility.Entity.Tbl_LogNIKInquiry req);
        bool InsertUserSession(Core.Entities.DataMaster.Utility.Entity.TblUserSession userSession);
        Task<List<MonitoringReaderVM>> GetMonitoringReader(string UnitIds);
    }
}
