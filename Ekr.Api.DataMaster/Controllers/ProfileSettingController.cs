using Ekr.Api.DataMaster.Filters;
using Ekr.Auth;
using Ekr.Business.Contracts.DataMaster;
using Ekr.Core.Constant;
using Ekr.Core.Entities;
using Ekr.Core.Entities.DataMaster.ProfileSetting.ViewModel;
using Ekr.Repository.Contracts.DataMaster.User;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ekr.Api.DataMaster.Controllers
{
    /// <summary>
    /// api Profile Setting
    /// </summary>
    [Route("dm/profile_setting")]
    [ApiController]
    public class ProfileSettingController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IProfileSettingService _profileSettingService;

        public ProfileSettingController(IProfileSettingService profileSettingService, IUserRepository userRepository)
        {
            _userRepository = userRepository;
            _profileSettingService = profileSettingService;
        }

        /// <summary>
        /// To get profile
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet("profile")]
        [ProducesResponseType(typeof(ServiceResponse<DataProfileVM>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<DataProfileVM>> GetProfile([FromQuery] DataProfileFilterVM req)
        {
            string authorization = HttpContext.Request.Headers["Authorization"];
            if (authorization != null)
            {
                var token = authorization.Split(" ")[1];
                var claims = TokenManager.GetPrincipal(token);
                req.PegawaiId = int.Parse(claims.PegawaiId);
            }

            var res = await _userRepository.GetProfile(req);

            return new ServiceResponse<DataProfileVM>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = res
            };
        }

        /// <summary>
        /// Untuk submit profile
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("profile")]
        [ProducesResponseType(typeof(ServiceResponse<bool>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<bool>> SubmitProfil([FromBody] SubmitProfileVM req)
        {
            string authorization = HttpContext.Request.Headers["Authorization"];
            if (authorization != null)
            {
                var token = authorization.Split(" ")[1];
                var claims = TokenManager.GetPrincipal(token);
                req.UpdatedByPegawaiId = claims.PegawaiId;
            }

            var res = await _profileSettingService.SubmitSettingProfile(req);

            return new ServiceResponse<bool>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = res
            };
        }

        /// <summary>
        /// Untuk submit change password
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("password")]
        [ProducesResponseType(typeof(ServiceResponse<bool>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<bool>> SubmitPassword([FromBody] ChangeUserPasswordVM req)
        {
            string authorization = HttpContext.Request.Headers["Authorization"];
            if (authorization != null)
            {
                var token = authorization.Split(" ")[1];
                var claims = TokenManager.GetPrincipal(token);
                req.UpdatedByPegawaiId = claims.PegawaiId;
            }

            var (res, info) = await _profileSettingService.ChangeUserPassword(req);

            return new ServiceResponse<bool>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = info,
                Data = res
            };
        }

        /// <summary>
        /// Untuk submit change photo
        /// </summary>
        /// <param name="FilePhoto"></param>
        /// <returns></returns>
        [HttpPost("photo")]
        [ProducesResponseType(typeof(ServiceResponse<bool>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<bool>> SubmitPhoto(IFormFile FilePhoto)
        {
            if(FilePhoto != null && FilePhoto.Length > 0)
            {
                return new ServiceResponse<bool>
                {
                    Status = (int)ServiceResponseStatus.EMPTY_PARAMETER,
                    Message = "Harap pilih file images terlebih dahulu!",
                    Data = false
                };
            }

            string authorization = HttpContext.Request.Headers["Authorization"];
            if (authorization == null)
            {
                return new ServiceResponse<bool>
                {
                    Status = (int)ServiceResponseStatus.ERROR,
                    Message = "authorization is required",
                    Data = false
                };
            }
            var token = authorization.Split(" ")[1];
            var claims = TokenManager.GetPrincipal(token);
            string PegawaiId = claims.PegawaiId;
            var (res, info) = await _profileSettingService.ChangeUserPhoto(FilePhoto, PegawaiId);


            return new ServiceResponse<bool>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = info,
                Data = res
            };
        }
    }
}
