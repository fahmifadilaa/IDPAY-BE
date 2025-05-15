using Ekr.Api.DataFingerIso.Filters;
using Ekr.Auth;
using Ekr.Business.Contracts.Enrollment;
using Ekr.Core.Constant;
using Ekr.Core.Entities.Enrollment;
using Ekr.Core.Entities.ThirdParty;
using Ekr.Repository.Contracts.Enrollment;
using Ekr.Services.Contracts.Account;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System;
using System.Threading.Tasks;

namespace Ekr.Api.DataFingerIso.Controllers
{
    [Route("enrollment-encrypt-iso")]
    [ApiController]
    public class EnrollmentEncryptISOController : ControllerBase
	{
        private readonly IEnrollmentService _enrollmentService;
        private readonly IEnrollmentKTPRepository _enrollmentKTPRepository;
        private readonly ICIFService _cifService;
        private readonly IConfiguration _configuration;
        private readonly string HostSoa = "";
        private readonly string systemId = "";
        private readonly string idType = "";
        private readonly string teller = "";
        private readonly string branch = "";
        private readonly string BaseUrlNonSoa = "";
        private readonly string EndPointNonSoa = "";
        private readonly bool IsHitSOA = false;
        private readonly string ResponseSukses = "";
        private readonly string ResponseParameterKosong = "";
        private readonly string ResponseDataKosong = "";
        private readonly string ResponseBelumEnroll = "";

        public EnrollmentEncryptISOController(IEnrollmentService enrollmentService,
            IEnrollmentKTPRepository enrollmentKTPRepository,
            ICIFService cifService,
            IConfiguration configuration)
        {
            _enrollmentService = enrollmentService;
            _enrollmentKTPRepository = enrollmentKTPRepository;
            _cifService = cifService;
            _configuration = configuration;

            HostSoa = _configuration.GetValue<string>("SOAApi:Host");
            systemId = _configuration.GetValue<string>("SOAApi:systemId");
            idType = _configuration.GetValue<string>("SOAApi:idType");
            teller = _configuration.GetValue<string>("SOAApi:teller");
            branch = _configuration.GetValue<string>("SOAApi:branch");
            IsHitSOA = _configuration.GetValue<bool>("isHitSOA");
            BaseUrlNonSoa = _configuration.GetValue<string>("NonSOAApi:base_url");
            EndPointNonSoa = _configuration.GetValue<string>("NonSOAApi:endpoint");
            ResponseSukses = _configuration.GetValue<string>("Response:Sukses");
            ResponseParameterKosong = _configuration.GetValue<string>("Response:ErrorParameterKosong");
            ResponseDataKosong = _configuration.GetValue<string>("Response:DataKosong");
            ResponseBelumEnroll = _configuration.GetValue<string>("Response:DataBelumEnroll");

        }

        /// <summary>
        /// To Create KTP data non employee encrypted ISO Version
        /// </summary>
        /// <param name="enroll"></param>
        /// <returns></returns>
        [HttpPost("submit-finger-enc-only-iso")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse<EnrollKTP>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "submit-finger-enc-only ISO Version")]
        public async Task<Core.Entities.ServiceResponse<EnrollKTP>> SubmitEnrollFingerEncOnlyISO(EnrollKTP enroll)
        {
            string authorization = HttpContext.Request.Headers["Authorization"];

            var reqSOA = new ApiSOA
            {
                systemId = systemId,
                numId = enroll.KtpNIK,
                idType = idType,
                host = HostSoa,
                teller = teller,
                branch = branch,
                baseUrlNonSoa = BaseUrlNonSoa,
                UrlEndPointNonSoa = EndPointNonSoa
            };

            if (string.IsNullOrWhiteSpace(authorization))
            {
                var (msg, code, cif) = await _enrollmentService.SubmitEnrollmentFingerEncryptedOnlyISO(IsHitSOA, reqSOA, enroll, 24, "tes", "0001", 1);
                enroll.KtpCif = cif;
                return new Core.Entities.ServiceResponse<EnrollKTP>
                {
                    Message = ResponseSukses,
                    Status = (int)ServiceResponseStatus.SUKSES,
                    Code = code
                };
            }

            var token = authorization.Split(" ")[1];

            var claims = TokenManager.GetPrincipal(token);

            var res = await _enrollmentService.SubmitEnrollmentFingerEncryptedOnlyISO(IsHitSOA, reqSOA, enroll, int.Parse(claims.PegawaiId),
                claims.NIK, claims.KodeUnit, int.Parse(claims.UnitId));

            enroll.KtpCif = res.cif;

            return new Core.Entities.ServiceResponse<EnrollKTP>
            {
                Message = ResponseSukses,
                Status = (int)ServiceResponseStatus.SUKSES,
                Code = res.code
            };
        }

        /// <summary>
        /// To Updates KTP data non employee encrypted ISO Version
        /// </summary>
        /// <param name="enroll"></param>
        /// <returns></returns>
        [HttpPost("resubmit-finger-enc-only-iso")]
        [ProducesResponseType(typeof(Core.Entities.ServiceResponse<EnrollKTP>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "To Updates KTP data non employee encrypted ISO Version")]
        public async Task<Core.Entities.ServiceResponse<EnrollKTP>> ReSubmitEnrollFingerEncOnlyISO(EnrollKTP enroll)
        {
            string authorization = HttpContext.Request.Headers["Authorization"];

            var reqSOA = new ApiSOA
            {
                systemId = systemId,
                numId = enroll.KtpNIK,
                idType = idType,
                host = HostSoa,
                teller = teller,
                branch = branch,
                baseUrlNonSoa = BaseUrlNonSoa,
                UrlEndPointNonSoa = EndPointNonSoa
            };

            if (string.IsNullOrWhiteSpace(authorization))
            {
                var (msg, code, cif) = await _enrollmentService.ReSubmitEnrollmentFingerEncryptedOnlyISO(IsHitSOA, reqSOA, enroll, 24, "tes", "0001", 1);
                enroll.KtpCif = cif;
                if (code == 0)
                {
                    return new Core.Entities.ServiceResponse<EnrollKTP>
                    {
                        Message = ResponseSukses,
                        Status = (int)ServiceResponseStatus.SUKSES,
                        Code = code
                    };
                }
                else
                {
                    return new Core.Entities.ServiceResponse<EnrollKTP>
                    {
                        Message = msg,
                        Status = (int)ServiceResponseStatus.ERROR,
                        Code = code
                    };
                }
            }

            var token = authorization.Split(" ")[1];

            var claims = TokenManager.GetPrincipal(token);

            var res = await _enrollmentService.ReSubmitEnrollmentFingerEncryptedOnlyISO(IsHitSOA, reqSOA, enroll, int.Parse(claims.PegawaiId),
                claims.NIK, claims.KodeUnit, int.Parse(claims.UnitId));

            enroll.KtpCif = res.cif;

            if (res.code == 0)
            {
                return new Core.Entities.ServiceResponse<EnrollKTP>
                {
                    Message = ResponseSukses,
                    Status = (int)ServiceResponseStatus.SUKSES,
                    Code = res.code
                };
            }
            else
            {
                return new Core.Entities.ServiceResponse<EnrollKTP>
                {
                    Message = res.msg,
                    Status = (int)ServiceResponseStatus.ERROR,
                    Code = res.code
                };
            }
        }
    }
}
