using Ekr.Business.Contracts.MappingNIKPegawai;
using Ekr.Core.Constant;
using Ekr.Core.Entities.DataMaster.Lookup;
using Ekr.Core.Entities;
using Ekr.Repository.Contracts.MappingNIK;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using Ekr.Api.MappingNik.Filters;
using Ekr.Core.Entities.MappingNIKPegawai;
using Ekr.Auth;
using System;
using System.Security.Claims;

namespace Ekr.Api.MappingNik.Controllers
{
    [Route("mapping-nik")]
    [Authorize]
    [ApiController]
    public class MappingNIKController : ControllerBase
    {

        private readonly IMappingNIKRepository _MappingRepository;
        private readonly IMappingNIKPegawaiService _MappingService;
        private readonly IConfiguration _configuration;
        private readonly string ResponseSukses = "";
        private readonly string ResponseParameterKosong = "";
        private readonly string ResponseErrorUpdateValidasi = "";

        public MappingNIKController(IMappingNIKRepository mappingRepository, IConfiguration configuration
          , IMappingNIKPegawaiService mappingService)
        {
            _MappingRepository = mappingRepository;
            _configuration = configuration;
            _MappingService = mappingService;

            ResponseSukses = _configuration.GetValue<string>("Response:Sukses");
            ResponseParameterKosong = _configuration.GetValue<string>("Response:ErrorParameterKosong");
            ResponseErrorUpdateValidasi = _configuration.GetValue<string>("ErrorMessageSettings:MappingNIKErrorUpdate");
        }

        [HttpPost("load-data")]
        [ProducesResponseType(typeof(ServiceResponse<GridResponse<MappingNIKVM>>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<GridResponse<MappingNIKVM>>> LoadData([FromBody] mappingGrid req)
        {
            var res = await _MappingRepository.LoadData(req);

            return new ServiceResponse<GridResponse<MappingNIKVM>>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = ResponseSukses,
                Data = res
            };
        }

        [HttpGet("get-mapping")]
        [ProducesResponseType(typeof(ServiceResponse<Tbl_MappingNIK_Pegawai>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<Tbl_MappingNIK_Pegawai>> GetById([FromQuery] LookupByIdVM req)
        {
            var res = await _MappingRepository.GetById(req);

            return new ServiceResponse<Tbl_MappingNIK_Pegawai>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = ResponseSukses,
                Data = res
            };
        }

        [HttpPost("insert-mapping")]
        [ProducesResponseType(typeof(ServiceResponse<Tbl_MappingNIK_Pegawai>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<Tbl_MappingNIK_Pegawai>> InsertMapping([FromBody] Tbl_MappingNIK_Pegawai req)
        {
            string authorization = HttpContext.Request.Headers["Authorization"];

            var token = authorization.Split(" ")[1];

            var claims = TokenManager.GetPrincipal(token);

            req.InsertedDate = DateTime.Now;
            req.InsertById = Convert.ToInt32( claims.UserId);

            var res = await _MappingService.InsertMappingNIKAsync(req);

            if (res.Npp == null)
            {
                return new ServiceResponse<Tbl_MappingNIK_Pegawai>
                {
                    Status = (int)ServiceResponseStatus.ERROR,
                    Message = ResponseErrorUpdateValidasi,
                    Data = req
                };
            }
            else
            {
                return new ServiceResponse<Tbl_MappingNIK_Pegawai>
                {
                    Status = (int)ServiceResponseStatus.SUKSES,
                    Message = ResponseSukses,
                    Data = res
                };
            }


        }

        [HttpPut("update-mapping")]
        [ProducesResponseType(typeof(ServiceResponse<Tbl_MappingNIK_Pegawai>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<Tbl_MappingNIK_Pegawai>> UpdateMapping([FromBody] Tbl_MappingNIK_Pegawai req)
        {
            string authorization = HttpContext.Request.Headers["Authorization"];


            var token = authorization.Split(" ")[1];

            var claims = TokenManager.GetPrincipal(token);

            req.UpdatedDate = DateTime.Now;
            req.UpdateById = Convert.ToInt32(claims.UserId);

            var res = await _MappingService.UpdateMappingNIKAsync(req);

            if (res.Id == 0)
            {
                return new ServiceResponse<Tbl_MappingNIK_Pegawai>
                {
                    Status = (int)ServiceResponseStatus.ERROR,
                    Message = ResponseErrorUpdateValidasi,
                    Data = req
                };
            }
            else
            {
                return new ServiceResponse<Tbl_MappingNIK_Pegawai>
                {
                    Status = (int)ServiceResponseStatus.SUKSES,
                    Message = ResponseSukses,
                    Data = res
                };
            }

        }

        [HttpDelete("delete-mapping")]
        [ProducesResponseType(typeof(ServiceResponse<string>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<string>> DeleteMapping([FromQuery] LookupByIdVM req)
        {
            int PegawaiId = 100;
            string authorization = HttpContext.Request.Headers["Authorization"];

            var token = authorization.Split(" ")[1];

            var claims = TokenManager.GetPrincipal(token);

            var dataExist = await _MappingRepository.GetById(new LookupByIdVM() { Id = req.Id });

            var log = new Tbl_MappingNIK_Pegawai_log()
            {
                NIK = dataExist.NIK,
                Npp = dataExist.Npp,
                Nama = dataExist.Nama,
                CreatedDate = DateTime.Now,
                CreateById = Convert.ToInt32(claims.UserId),
                Keterangan = "Delete"
            };


            await _MappingRepository.DeleteData(req, log);

            return new ServiceResponse<string>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = ResponseSukses
            };
        }

    }
}
