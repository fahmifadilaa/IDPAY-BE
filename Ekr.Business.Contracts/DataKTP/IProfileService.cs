using Ekr.Core.Entities.DataKTP;
using Ekr.Core.Entities.Recognition;
using System;
using System.Threading.Tasks;

namespace Ekr.Business.Contracts.DataKTP
{
    public interface IProfileService
    {
        Task<ServiceResponse<ProfileByNik>> GetAuthKTPData(string base64Img, string nik, string fingerType, string baseUrl, string endPoint);
        Task<ServiceResponse<ProfileByNik>> GetKTPData(string nik);
        Task<ServiceResponse<ProfileByNik>> GetAuthKTPData(string base64Img, string nik, string baseUrl, string endPoint);
        Task<Tbl_DataKTP_Demografis> UpdateCIF(string nik, string cif, string source, string npp, string uname, string unitCode);
        Task<ServiceResponse<ProfileByNik>> GetKTPDataFingerEncOnly(string nik);
        Task<ServiceResponse<ProfileByNik>> GetKTPDataFingerEncOnlyNoMatching(string nik);
        Task<ServiceResponse<ProfileByNik>> GetKTPDataFingerEncOnlyNoMatchingNew(string nik,int id);
        Task<ServiceResponse<ProfileByNik>> GetKTPDataByCifFingerEncOnly(string cif);
		Task<ServiceResponse<ProfileByNik>> GetAuthKTPDataFingerEncOnly(string base64Img, string nik, string fingerType, string baseUrl, string endPoint);
		Task<ServiceResponse<ProfileByNik>> GetAuthKTPDataFingerEncOnly(ProfileLoopReq req, string baseUrl, string endPoint, DateTime RequestTime, string AuthNpp, string AuthUnitCode);
		Task<ServiceResponse<ProfileByNik>> GetAuthKTPDataFingerEncOnlyNew(ProfileLoopReq req, string baseUrl, string endPoint, DateTime RequestTime, string AuthNpp, string AuthUnitCode);
		Task<ServiceResponse<ProfileByNik>> GetAuthKTPDataFingerEncOnlyNewData(ProfileLoopReq req, string baseUrl, string endPoint, DateTime RequestTime, string AuthNpp, string AuthUnitCode);
		Task<ServiceResponse<ProfileByNik>> GetAuthKTPDataFingerEncOnlyNewFile(ProfileLoopReq req, string baseUrl, string endPoint, DateTime RequestTime, string AuthNpp, string AuthUnitCode);
        Task<ServiceResponse<ProfileByNik>> GetAuthKTPDataFingerEncOnlyCompressed(ProfileLoopReq req, string baseUrl, string endPoint, DateTime RequestTime, string AuthNpp, string AuthUnitCode);
        Task<ServiceResponse<ProfileByNik>> GetAuthKTPDataFingerEmpEncOnly(ProfileLoopReq req, string baseUrl, string endPoint, DateTime RequestTime, string AuthNpp, string AuthUnitCode);
        Task<ServiceResponse<ProfileByNik>> GetAuthKTPDataFingerEmpEncOnlyIso(ProfileLoopReq req, string baseUrl, string endPoint, DateTime RequestTime, string AuthNpp, string AuthUnitCode);
        Task<ServiceResponse<ProfileByNik>> GetAuthKTPDataByCifFingerEncOnly(ProfileLoopByCifReq req, string baseUrl, string endPoint, DateTime RequestTime);
        Task<ServiceResponse<ProfileByNik>> GetAuthKTPDataByCifFingerEncOnlyNew(ProfileLoopByCifReq req, string baseUrl, string endPoint, DateTime RequestTime);
        Task<ServiceResponse<ProfileByNik>> GetAuthKTPDataByCifFingerEncOnlyNewData(ProfileLoopByCifReq req, string baseUrl, string endPoint, DateTime RequestTime);
        Task<ServiceResponse<ProfileByNik>> GetAuthKTPDataByCifFingerEncOnlyNewFile(ProfileLoopByCifReq req, string baseUrl, string endPoint, DateTime RequestTime);
        Task<ServiceResponse<ProfileByNik>> GetEmpAuthKTPDataByCifFingerEncOnly(string base64Img, string cif, string baseUrl, string endPoint);
        Task<ServiceResponse<ProfileByNik>> IsFingerLoopMatch(ProfileLoopReq req, string baseUrl, string endPoint, DateTime RequestTime, string AuthNpp, string AuthUnitCode);
        Task<ServiceResponse<ProfileByNik>> IsFingerLoopMatchNew(ProfileLoopReq req, string baseUrl, string endPoint, DateTime RequestTime, string AuthNpp, string AuthUnitCode);
        Task<ServiceResponse<ProfileByNik>> IsFingerLoopMatchNewData(ProfileLoopReq req, string baseUrl, string endPoint, DateTime RequestTime, string AuthNpp, string AuthUnitCode);
        Task<ServiceResponse<ProfileByNik>> IsFingerLoopMatchNewIsoFile(ProfileLoopReq req, string baseUrl, string endPoint, DateTime RequestTime, string AuthNpp, string AuthUnitCode);
        Task<ServiceResponse<ProfileByNik>> IsFingerByCifMatch(ProfileLoopByCifReq req, string baseUrl, string endPoint, DateTime RequestTime);
        Task<ServiceResponse<ProfileByNik>> IsFingerNppIsoMatch(ProfileLoopReq req, string baseUrl, string endPoint, DateTime RequestTime, string AuthNpp, string AuthUnitCode);
        Task<ServiceResponse<ProfileByNik>> GetAuthKTPDataFingerEncOnlyISO(ProfileLoopReq req, string baseUrl, string endPoint, DateTime RequestTime, string AuthNpp, string AuthUnitCode);
        Task<ServiceResponse<ProfileByNik>> GetAuthKTPDataFingerEncOnlyFR(ProfileFRReq req, DateTime RequestTime, string AuthNpp, string AuthUnitCode);
        Task<ServiceResponse<ProfileByNik>> GetAuthKTPDataFingerEncOnlyISODB(ProfileLoopReq req, string baseUrl, string endPoint, DateTime RequestTime, string AuthNpp, string AuthUnitCode);
        Task<ServiceResponse<ProfileByNik>> IsFingerNppIsoMatchDB(ProfileLoopReq req, string baseUrl, string endPoint, DateTime RequestTime, string AuthNpp, string AuthUnitCode);
        Task<ServiceResponse<ProfileByNik>> GetAuthKTPDataByCifFingerEncOnlyIso(ProfileLoopByCifReq req, string baseUrl, string endPoint, DateTime RequestTime);
        Task<ServiceResponse<ProfileByNik>> GetAuthKTPDataFingerEncOnlyIso(string base64Img, string nik, string fingerType, string baseUrl, string endPoint);
        Task<ServiceResponse<ProfileByNik>> GetAuthKTPDataByCifFingerEncOnlyIsoDb(ProfileLoopByCifReq req, string baseUrl, string endPoint, DateTime RequestTime);
        Task<ServiceResponse<ProfileByNik>> GetAuthKTPDataFingerEncOnlyIsoDB(string base64Img, string nik, string fingerType, string baseUrl, string endPoint);
        Task<ServiceResponse<ProfileByNik>> GetAuthKTPDataFingerEncOnlyISOFileWData(ProfileLoopReq req, string baseUrl, string endPoint, DateTime RequestTime, string AuthNpp, string AuthUnitCode);
        Task<ServiceResponse<ProfileByNik>> GetAuthKTPDataFingerEncOnlyISOWData(ProfileLoopReq req, string baseUrl, string endPoint, DateTime RequestTime, string AuthNpp, string AuthUnitCode);
        Task<ServiceResponse<ProfileByNik>> GetAuthKTPDataFingerEncOnlyISOThirdParty(ProfileLoopReq req, string baseUrl, string endPoint, DateTime RequestTime, string AuthUnitCode);
        Task<ServiceResponse<ProfileByNikOnlyImg>> GetAuthKTPDataFingerEncOnlyISOThirdPartyDemo(ProfileLoopReq req, string baseUrl, string endPoint, DateTime RequestTime, string AuthUnitCode);
        Task<ServiceResponse<ProfileByNikOnlyFinger>> GetAuthKTPDataFingerEncOnlyISOThirdPartyBio(ProfileLoopReq req, string baseUrl, string endPoint, DateTime RequestTime, string AuthUnitCode);
    }
}
