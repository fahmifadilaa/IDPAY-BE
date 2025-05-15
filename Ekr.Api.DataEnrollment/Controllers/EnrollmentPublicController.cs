using Ekr.Api.DataEnrollment.Filters;
using Ekr.Core.Constant;
using Ekr.Core.Entities;
using Ekr.Repository.Contracts.Enrollment;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Ekr.Api.DataEnrollment.Controllers
{
    [Route("enrollment-public")]
    [ApiController]
    public class EnrollmentPublicController : ControllerBase
    {
        private readonly IEnrollmentKTPRepository _enrollmentKTPRepository;

        public EnrollmentPublicController(IEnrollmentKTPRepository enrollmentKTPRepository)
        {
            _enrollmentKTPRepository = enrollmentKTPRepository;
        }

        /// <summary>
        /// To check whether NPP have been enrolled or not
        /// </summary>
        /// <param name="npp"></param>
        /// <returns></returns>
        [HttpGet("is-npp-enrolled")]
        [ProducesResponseType(typeof(ServiceResponse<bool>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "To check whether NPP have been enrolled or not")]
        public async Task<ServiceResponse<bool>> CheckExistingNpp(string npp)
        {
            if (string.IsNullOrEmpty(npp))
            {
                return new ServiceResponse<bool>
                {
                    Status = (int)ServiceResponseStatus.EMPTY_PARAMETER,
                    Message = nameof(ServiceResponseStatus.EMPTY_PARAMETER)
                };
            }

            var res = await _enrollmentKTPRepository.IsNppEnrolled(npp);

            return new ServiceResponse<bool>
            {
                Data = res,
                Message = nameof(ServiceResponseStatus.SUKSES),
                Status = (int)ServiceResponseStatus.SUKSES,
            };
        }
    }
}
