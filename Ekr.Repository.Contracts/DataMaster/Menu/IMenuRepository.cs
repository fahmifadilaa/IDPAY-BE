using Ekr.Core.Entities;
using Ekr.Core.Entities.DataMaster.Menu.Entity;
using Ekr.Core.Entities.DataMaster.Menu.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ekr.Repository.Contracts.DataMaster.Menu
{
    public interface IMenuRepository
    {
        Task<GridResponse<MenuVM>> LoadData(MenuFilterVM req);
        Task<TblMenu> GetMenu(MenuViewFilterVM req);
        Task<TblMenu> InsertMenu(TblMenu req);
        Task<TblMenu> UpdateMenu(TblMenu req);
        Task DeleteMenu(MenuViewFilterVM req, int PegawaiId);
        Task<GridResponse<ManageMenuVM>> LoadManageData(ManageMenuFilterVM req);
        Task<SettingMenuVM> GetSettingMenu(SettingMenuViewFilterVM req);
        Task<SettingMenuReqVM> InsertSettingMenu(SettingMenuReqVM req);
        Task<SettingMenuReqVM> UpdateSettingMenu(SettingMenuReqVM req);
        Task DeleteSettingMenu(SettingMenuViewFilterVM req, int PegawaiId);
    }
}
