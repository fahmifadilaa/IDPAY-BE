using Ekr.Core.Entities.DataEnrollment.ViewModel;
using Ekr.Core.Entities.DataKTP;
using Ekr.Core.Entities.Enrollment;
using Ekr.Core.Entities.ThirdParty;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ekr.Business.Contracts.Enrollment
{
    public interface IEnrollmentService
    {
        Task<(string msg, int code, string cif)> SubmitEnrollment(bool isHitSOA, ApiSOA ReqSoa, EnrollKTP enroll, int Id, string npp,
            string unitCode, int unitId);
        Task<(string msg, int code, string cif)> ReSubmitEnrollment(bool isHitSOA, ApiSOA ReqSoa, EnrollKTP enroll, int Id, string npp,
            string unitCode, int unitId);
        Task<(string msg, int code, string cif)> UpdatesPhotoCam(EnrollKTP enroll, int Id, string npp,
            string unitCode, int unitId);
        Task<string> VerifyEnrollment(string nik, string npp, string comment);
        Task<string> ConfirmSubmission(ConfirmEnrollSubmissionVM confirmEnrollSubmissionVM);
        Task<(string msg, int code, string cif)> SubmitEnrollmentFingerEncryptedOnly(bool isHitSOA, ApiSOA ReqSoa, EnrollKTP enroll, int Id, string npp,
            string unitCode, int unitId);

        Task<(string msg, int code, string cif)> SubmitEnrollmentFingerEncryptedOnlyThirdParty(string AppsChannel, bool isHitSOA, ApiSOA ReqSoa, EnrollKTPBiasaThirdParty enroll);


        Task<(string msg, int code, string cif)> ReSubmitEnrollmentFingerEncryptedOnly(bool isHitSOA, ApiSOA ReqSoa, EnrollKTP enroll, int Id, string npp,
            string unitCode, int unitId);
        Task<(string msg, int code, string cif)> SubmitEnrollmentFingerEncryptedOnlyISO(bool isHitSOA, ApiSOA ReqSoa, EnrollKTP enroll, int Id, string npp,
            string unitCode, int unitId);

        Task<(string msg, int code, string cif)> SubmitEnrollmentFingerEncryptedOnlyISOThirdParty(string AppsChannel, bool isHitSOA, ApiSOA ReqSoa, EnrollKTPThirdParty2VM enroll, string remoteIpAddress);

        Task<(string msg, int code, string cif)> ReSubmitEnrollmentFingerEncryptedOnlyISO(bool isHitSOA, ApiSOA ReqSoa, EnrollKTP enroll, int Id, string npp,
            string unitCode, int unitId);
        Task<(bool status, string msg)> ConvertUrlToB64(string path);
        Task<List<FingerISOVM>> GetISO(string nik);
    }
}
