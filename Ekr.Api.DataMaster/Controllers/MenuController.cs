using Ekr.Api.DataMaster.Filters;
using Ekr.Auth;
using Ekr.Core.Constant;
using Ekr.Core.Entities;
using Ekr.Core.Entities.DataMaster.Menu.Entity;
using Ekr.Core.Entities.DataMaster.Menu.ViewModel;
using Ekr.Repository.Contracts.DataMaster.Menu;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Ekr.Api.DataMaster.Controllers
{
    /// <summary>
    /// api Menu
    /// </summary>
    [Route("dm/menu")]
    [ApiController]
    public class MenuController : ControllerBase
    {
        private readonly IMenuRepository _menuRepository;

        public MenuController(IMenuRepository menuRepository)
        {
            _menuRepository = menuRepository;
        }

        /// <summary>
        /// Untuk load all data menu
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("menus")]
        [ProducesResponseType(typeof(ServiceResponse<GridResponse<MenuVM>>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<GridResponse<MenuVM>>> LoadData([FromBody] MenuFilterVM req)
        {
            var res = await _menuRepository.LoadData(req);

            return new ServiceResponse<GridResponse<MenuVM>>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = res
            };
        }

        /// <summary>
        /// Untuk get data menu
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet("menu")]
        [ProducesResponseType(typeof(ServiceResponse<TblMenu>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<TblMenu>> GetMenu([FromQuery] MenuViewFilterVM req)
        {
            var res = await _menuRepository.GetMenu(req);

            return new ServiceResponse<TblMenu>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = res
            };
        }

        /// <summary>
        /// Untuk create menu
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("menu")]
        [ProducesResponseType(typeof(ServiceResponse<TblMenu>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<TblMenu>> InsertMenu([FromBody] TblMenu req)
        {
            string authorization = HttpContext.Request.Headers["Authorization"];
            if (authorization != null)
            {
                var token = authorization.Split(" ")[1];
                var claims = TokenManager.GetPrincipal(token);
                req.CreatedBy_Id = int.Parse(claims.PegawaiId);
            }

            var res = await _menuRepository.InsertMenu(req);

            return new ServiceResponse<TblMenu>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = res
            };
        }

        /// <summary>
        /// Untuk update menu
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPut("menu")]
        [ProducesResponseType(typeof(ServiceResponse<TblMenu>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<TblMenu>> UpdateMenu([FromBody] TblMenu req)
        {
            string authorization = HttpContext.Request.Headers["Authorization"];
            if (authorization != null)
            {
                var token = authorization.Split(" ")[1];
                var claims = TokenManager.GetPrincipal(token);
                req.UpdatedBy_Id = int.Parse(claims.PegawaiId);
            }

            var res = await _menuRepository.UpdateMenu(req);

            return new ServiceResponse<TblMenu>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = res
            };
        }

        /// <summary>
        /// Untuk delete menu
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpDelete("menu")]
        [ProducesResponseType(typeof(ServiceResponse<string>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<string>> DeleteMenu([FromQuery] MenuViewFilterVM req)
        {
            int PegawaiId = 100;
            string authorization = HttpContext.Request.Headers["Authorization"];
            if (authorization != null)
            {
                var token = authorization.Split(" ")[1];
                var claims = TokenManager.GetPrincipal(token);
                PegawaiId = int.Parse(claims.PegawaiId);
            }

            await _menuRepository.DeleteMenu(req, PegawaiId);

            return new ServiceResponse<string>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success"
            };
        }

    }
}
