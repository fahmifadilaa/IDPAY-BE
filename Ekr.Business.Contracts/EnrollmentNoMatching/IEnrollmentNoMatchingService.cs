using Ekr.Core.Entities.Auth;
using Ekr.Core.Entities.DataEnrollment.ViewModel;
using Ekr.Core.Entities.DataKTP;
using Ekr.Core.Entities.Enrollment;
using Ekr.Core.Entities.Recognition;
using Ekr.Core.Entities.SettingThreshold;
using Ekr.Core.Entities.ThirdParty;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ekr.Business.Contracts.EnrollmentNoMatching
{
    public interface IEnrollmentNoMatchingService
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
        Task<(string msg, int code, string cif)> ReSubmitEnrollmentFingerEncryptedOnly(bool isHitSOA, ApiSOA ReqSoa, EnrollKTP enroll, int Id, string npp,
            string unitCode, int unitId);
        Task<(string msg, int code, string cif)> SubmitEnrollmentFingerEncryptedOnlyISO(bool isHitSOA, ApiSOA ReqSoa, EnrollKTP enroll, int Id, string npp,
            string unitCode, int unitId);
        Task<(string msg, int code, string cif)> ReSubmitEnrollmentFingerEncryptedOnlyISO(bool isHitSOA, ApiSOA ReqSoa, EnrollKTP enroll, int Id, string npp,
            string unitCode, int unitId);

        Task<(string msg, int code, string cif)> SubmitEnrollmentFingerEncryptedOnlyNoMatching(bool isHitSOA, ApiSOA ReqSoa, EnrollKTPNoMatching enroll, int Id, string npp,
            string unitCode, int unitId, int roleid);
        Task<(string msg, int code, string cif)> SubmitEnrollmentFingerEncryptedOnlyNoMatchingv2(bool isHitSOA, ApiSOA ReqSoa, EnrollKTPNoMatchingv2 enroll, int Id, string npp,
            string unitCode, int unitId, int roleid);

        Task<(string msg, int code, string cif)> SubmitEnrollmentFingerEncryptedOnlyIKD(bool isHitSOA, ApiSOA ReqSoa, EnrollKTPNoMatching enroll, int Id, string npp,
            string unitCode, int unitId, int roleid);

        Task<(string msg, int code, string cif)> ReSubmitEnrollmentFingerEncryptedOnlyNoMatching(bool isHitSOA, ApiSOA ReqSoa, EnrollKTPNoMatching enroll, int Id, string npp,
            string unitCode, int unitId);
        Task<(string msg, int code, string cif)> ReSubmitEnrollmentFingerEncryptedOnlyNoMatchingv2(bool isHitSOA, ApiSOA ReqSoa, EnrollKTPNoMatchingv2 enroll, int Id, string npp,
            string unitCode, int unitId);

        Task<(string msg, int code, string cif)> ReSubmitEnrollmentFingerEncryptedOnlyNoMatchingIKD(bool isHitSOA, ApiSOA ReqSoa, EnrollKTPNoMatching enroll, int Id, string npp,
            string unitCode, int unitId);

        Task<(bool status, string msg)> ConvertUrlToB64(string path);
        Task<List<FingerISOVM>> GetISO(string nik);
        Task UpdateEnrollNoMatchingStatusAsync(EnrollNoMatchingStatusRequest req, int updatedById, string npp,
            string unitCode, int unitId, int roleid);

        Task<FaceRecogResponse> MatchImageBase64ToBase64(FaceRecogRequest req, UrlRequestRecognitionFR UrlReq);
        Task<FaceRecogResponseV2> MatchImageBase64ToBase64FRV2(FaceRecogRequestV2 req, UrlRequestRecognitionFR UrlReq);
        Task<ScanResponse> ScanQRIKD(ScanQRIKDV2Req req, UrlRequestRecognitionFR UrlReq);
        Task<ScanResponse> ScanQRIKDXML(ScanQRIKDReq req, UrlRequestRecognitionFR UrlReq);
        Task<ScanResponse> ScanQRIKDV2(ScanQRIKDV2Req req, UrlRequestRecognitionFR UrlReq);
        Task<ScanResponseEncrypt> ScanQRIKDLimit(ScanQRIKDV2Req req, UrlRequestRecognitionFR UrlReq, string npp, int maxLimit, int timeLimit, int userId, int roleId, int unitId, string aesKey);
        Task<ScanResponse> ScanQRIKDLimitNotEncrypted(ScanQRIKDV2Req req, UrlRequestRecognitionFR UrlReq, string npp, int maxLimit, int timeLimit, int userId, int roleId, int unitId);
        Task<ScanResponseEncrypt> ScanQRIKDEncrypt(ScanQRIKDV2Req req, UrlRequestRecognitionFR UrlReq, string aesKey);
        Task<CrawlingSubContent> CrawlingDukcapilHIT(CrawlingRequest req, UrlRequestCrawlingDukcapil UrlReq);
        Task<FaceRecogResponse> MatchUrlImagesToBase64Json(FaceRecogRequest req, UrlRequestRecognitionFR UrlReq);
    }
}
