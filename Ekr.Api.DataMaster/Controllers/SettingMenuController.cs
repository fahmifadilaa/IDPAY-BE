using Ekr.Api.DataMaster.Filters;
using Ekr.Auth;
using Ekr.Core.Constant;
using Ekr.Core.Entities;
using Ekr.Core.Entities.DataMaster.Menu.ViewModel;
using Ekr.Repository.Contracts.DataMaster.Menu;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ekr.Api.DataMaster.Controllers
{
    /// <summary>
    /// api Setting Menu
    /// </summary>
    [Route("dm/setting_menu")]
    [ApiController]
    public class SettingMenuController : ControllerBase
    {
        private readonly IMenuRepository _menuRepository;

        public SettingMenuController(IMenuRepository menuRepository)
        {
            _menuRepository = menuRepository;
        }

        /// <summary>
        /// To load all data menu
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("menus")]
        [ProducesResponseType(typeof(ServiceResponse<GridResponse<ManageMenuVM>>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<GridResponse<ManageMenuVM>>> LoadData([FromBody] ManageMenuFilterVM req)
        {
            var res = await _menuRepository.LoadManageData(req);

            return new ServiceResponse<GridResponse<ManageMenuVM>>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = res
            };
        }

        /// <summary>
        /// To get data menu
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet("menu")]
        [ProducesResponseType(typeof(ServiceResponse<SettingMenuVM>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<SettingMenuVM>> GetMenu([FromQuery] SettingMenuViewFilterVM req)
        {
            var res = await _menuRepository.GetSettingMenu(req);

            return new ServiceResponse<SettingMenuVM>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = res
            };
        }

        /// <summary>
        /// To create setting menu
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("menu")]
        [ProducesResponseType(typeof(ServiceResponse<SettingMenuReqVM>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<SettingMenuReqVM>> InsertSettingMenu([FromBody] SettingMenuReqVM req)
        {
            if(req.Roles == null)
            {
                return new ServiceResponse<SettingMenuReqVM>
                {
                    Status = (int)ServiceResponseStatus.EMPTY_PARAMETER,
                    Message = "Roles is Required"
                };
            }
            if (req.AppsVal == null)
            {
                return new ServiceResponse<SettingMenuReqVM>
                {
                    Status = (int)ServiceResponseStatus.EMPTY_PARAMETER,
                    Message = "Apps is Required"
                };
            }

            string authorization = HttpContext.Request.Headers["Authorization"];
            if (authorization != null)
            {
                var token = authorization.Split(" ")[1];
                var claims = TokenManager.GetPrincipal(token);
                req.CreatedBy_Id = int.Parse(claims.PegawaiId);
            }

            var res = await _menuRepository.InsertSettingMenu(req);

            return new ServiceResponse<SettingMenuReqVM>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = res
            };
        }

        /// <summary>
        /// To update setting menu
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPut("menu")]
        [ProducesResponseType(typeof(ServiceResponse<SettingMenuReqVM>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<SettingMenuReqVM>> UpdateSettingMenu([FromBody] SettingMenuReqVM req)
        {
            if (req.Roles == null)
            {
                return new ServiceResponse<SettingMenuReqVM>
                {
                    Status = (int)ServiceResponseStatus.EMPTY_PARAMETER,
                    Message = "Roles is Required"
                };
            }
            if (req.AppsVal == null)
            {
                return new ServiceResponse<SettingMenuReqVM>
                {
                    Status = (int)ServiceResponseStatus.EMPTY_PARAMETER,
                    Message = "Apps is Required"
                };
            }

            req.UpdatedBy_Id = 100;
            string authorization = HttpContext.Request.Headers["Authorization"];
            if (authorization != null)
            {
                var token = authorization.Split(" ")[1];
                var claims = TokenManager.GetPrincipal(token);
                req.UpdatedBy_Id = int.Parse(claims.PegawaiId);
            }

            var res = await _menuRepository.UpdateSettingMenu(req);

            return new ServiceResponse<SettingMenuReqVM>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = res
            };
        }

        /// <summary>
        /// To delete setting menu
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpDelete("menu")]
        [ProducesResponseType(typeof(ServiceResponse<string>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<string>> DeleteMenu([FromQuery] SettingMenuViewFilterVM req)
        {
            string authorization = HttpContext.Request.Headers["Authorization"];
            int PegawaiId = 100;
            if (authorization != null)
            {
                var token = authorization.Split(" ")[1];
                var claims = TokenManager.GetPrincipal(token);
                PegawaiId = int.Parse(claims.PegawaiId);
            }

            await _menuRepository.DeleteSettingMenu(req, PegawaiId);

            return new ServiceResponse<string>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success"
            };
        }


    }
}
