using Ekr.Api.DataMigration.Filters;
using Ekr.Business.Contracts.DataMigration;
using Ekr.Core.Constant;
using Ekr.Core.Entities;
using Ekr.Repository.Contracts.Enrollment;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace Ekr.Api.DataMigration.Controllers
{
    [Route("ktp")]
    [ApiController]
    public class KtpDataController : ControllerBase
    {
        private readonly IEnrollment _enrollment;
        private readonly IConfiguration _config;
        private readonly IEnrollmentKTPRepository _enrollmentKTPRepository;

        public KtpDataController(IEnrollment enrollment, IEnrollmentKTPRepository enrollmentKTPRepository, IConfiguration config)
        {
            _enrollment = enrollment;
            _enrollmentKTPRepository = enrollmentKTPRepository;
            _config = config;
        }

        /// <summary>
        /// To migrate existing file jpg formatted finger photo to encrypted txt
        /// </summary>
        /// <returns></returns>
        [HttpGet("migrate/finger/photos")]
        [ProducesResponseType(typeof(ServiceResponse<int>), 200)]
        [ProducesResponseType(typeof(ExceptionDto), 500)]
        [LogActivity(Keterangan = "Data Migration")]
        public async Task<ServiceResponse<int>> GetDataReader()
        {
            //var res = await _enrollmentKTPRepository.UpsertEnroll(); //kalo mau digabung, ini aktifin

            var res = await _enrollment.MigrateFingerJpgToEncTxt();

            return new ServiceResponse<int>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = res
            };
        }

        /// <summary>
        /// To migrate Data KTP
        /// </summary>
        /// <returns></returns>
        [HttpGet("migrate/ktp_demografis")]
        [ProducesResponseType(typeof(ServiceResponse<string>), 200)]
        [ProducesResponseType(typeof(ExceptionDto), 500)]
        [LogActivity(Keterangan = "Data Migration")]
        public async Task<ServiceResponse<string>> MigrateKTP()
        {
            var isProxy = _config.GetValue<bool>("isProxy");
            var IpProxy = _config.GetValue<string>("IpProxy");
            var apiKey = _config.GetValue<string>("apiKey");
            // migrate to new IBS
            var res = await _enrollmentKTPRepository.MigrateDataEnroll(apiKey, isProxy, IpProxy);

            if(res != "")
            {
                return new ServiceResponse<string>
                {
                    Status = (int)ServiceResponseStatus.ERROR,
                    Message = "Internal Server Error on Migrate Data Enroll",
                    Data = res
                };
            }

            return new ServiceResponse<string>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = res
            };
        }
    }
}
