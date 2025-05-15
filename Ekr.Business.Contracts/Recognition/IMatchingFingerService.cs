using Ekr.Core.Entities.DataKTP;
using System;
using System.Threading.Tasks;

namespace Ekr.Business.Contracts.Recognition
{
    public interface IMatchingFingerService
    {
        Task<(string msg, string status)> MatchFinger(ProfileLoopReq req, string BaseUrl, string EndPoint, DateTime RequestTime, string AuthNpp, string AuthUnitCode);
        Task<(string msg, string status)> MatchFingerType(string base64Img, string nik, string fingerType, string BaseUrl, string Endpoint);
        Task<(string msg, string status)> MatchFingerEmp(ProfileLoopNppReq req, string nik, string BaseUrl, string EndPoint, DateTime RequestTime, string AuthNpp, string AuthUnitCode);
        Task<bool> IsMatchFinger(ProfileLoopReq req, string BaseUrl, string EndPoint, DateTime RequestTime, string AuthNpp, string AuthUnitCode);
        Task<bool> IsMatchFingerEmp(ProfileLoopReq req, string BaseUrl, string EndPoint, DateTime RequestTime, string AuthNpp, string AuthUnitCode);
        Task<bool> MatchFingerTypeEmpBool(string base64Img, string nik, string fingerType, string BaseUrl, string Endpoint);
        Task<(string msg, string status)> MatchFingerTypeEmp(string base64Img, string nik, string fingerType, string BaseUrl, string Endpoint);
        Task<bool> IsMatchFingerType(string base64Img, string nik, string fingerType, string BaseUrl, string Endpoint);
    }
}
