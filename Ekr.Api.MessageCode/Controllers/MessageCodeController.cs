using Ekr.Api.MessageCode.Filters;
using Ekr.Auth;
using Ekr.Core.Constant;
using Ekr.Core.Entities;
using Ekr.Core.Entities.MessageCode;
using Ekr.Repository.Contracts.MessageCode;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace Ekr.Api.MessageCode.Controllers
{
    /// <summary>
    /// api MessageCode
    /// </summary>
    [Route("mc")]
    [Authorize]
    [ApiController]
    public class MessageCodeController : ControllerBase
    {
        private readonly IMessageCodeRepository _messageCodeRepository;
        private readonly IConfiguration _config;

        private readonly string ResponseSukses = "";

        public MessageCodeController(IMessageCodeRepository messageCodeRepository, IConfiguration config)
        {
            _messageCodeRepository = messageCodeRepository;

            _config = config;

            ResponseSukses = _config.GetValue<string>("Response:Sukses");
        }

        /// <summary>
        /// Untuk load data message code
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("list")]
        [ProducesResponseType(typeof(ServiceResponse<GridResponse<MessageCodeVM>>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Message Code")]
        public async Task<ServiceResponse<GridResponse<MessageCodeVM>>> LoadData([FromBody] MessageCodeFilter req)
        {
            var res = await _messageCodeRepository.LoadData(req);

            return new ServiceResponse<GridResponse<MessageCodeVM>>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = ResponseSukses,
                Data = res
            };
        }

        /// <summary>
        /// To get data message code by id
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet("get_by_id")]
        [ProducesResponseType(typeof(ServiceResponse<Tbl_Master_MessageCode>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Message Code")]
        public async Task<ServiceResponse<Tbl_Master_MessageCode>> GetById([FromQuery] MessageCodeByIdVM req)
        {
            var res = await _messageCodeRepository.GetById(req);

            return new ServiceResponse<Tbl_Master_MessageCode>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = ResponseSukses,
                Data = res
            };
        }

        /// <summary>
        /// To create message code
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("insert")]
        [ProducesResponseType(typeof(ServiceResponse<Tbl_Master_MessageCode>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Message Code")]
        public async Task<ServiceResponse<Tbl_Master_MessageCode>> InsertMessageCode([FromBody] Tbl_Master_MessageCode req)
        {
            string authorization = HttpContext.Request.Headers["Authorization"];
            if (authorization != null)
            {
                var token = authorization.Split(" ")[1];
                var claims = TokenManager.GetPrincipal(token);
                req.CreatedBy_Id = int.Parse(claims.PegawaiId);
            }

            var res = await _messageCodeRepository.InsertMessageCode(req);

            return new ServiceResponse<Tbl_Master_MessageCode>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = ResponseSukses,
                Data = res
            };
        }

        /// <summary>
        /// To update Message Code
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPut("update")]
        [ProducesResponseType(typeof(ServiceResponse<Tbl_Master_MessageCode>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Message Code")]
        public async Task<ServiceResponse<Tbl_Master_MessageCode>> UpdateMessageCode([FromBody] Tbl_Master_MessageCode req)
        {
            string authorization = HttpContext.Request.Headers["Authorization"];
            if (authorization != null)
            {
                var token = authorization.Split(" ")[1];
                var claims = TokenManager.GetPrincipal(token);
                req.UpdatedBy_Id = int.Parse(claims.PegawaiId);
            }

            var res = await _messageCodeRepository.UpdateMessageCode(req);

            return new ServiceResponse<Tbl_Master_MessageCode>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = ResponseSukses,
                Data = res
            };
        }

        /// <summary>
        /// To delete Message Code
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpDelete("delete")]
        [ProducesResponseType(typeof(ServiceResponse<string>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Message Code")]
        public async Task<ServiceResponse<string>> DeleteMessageCode([FromQuery] MessageCodeByIdVM req)
        {
            int PegawaiId = 100;
            string authorization = HttpContext.Request.Headers["Authorization"];
            if (authorization != null)
            {
                var token = authorization.Split(" ")[1];
                var claims = TokenManager.GetPrincipal(token);
                PegawaiId = int.Parse(claims.PegawaiId);
            }

            await _messageCodeRepository.DeleteMessageCode(req, PegawaiId);

            return new ServiceResponse<string>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = ResponseSukses
            };
        }
    }
}
