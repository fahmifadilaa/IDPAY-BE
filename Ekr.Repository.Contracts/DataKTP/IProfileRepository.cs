using Ekr.Core.Entities.Auth;
using Ekr.Core.Entities.DataEnrollment.Entity;
using Ekr.Core.Entities.DataKTP;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ekr.Repository.Contracts.DataKTP
{
    public interface IProfileRepository
    {
        Task<ProfileByNik> GetProfileByNik(string nik);
        Task<ProfileByNik> GetProfileByNikNoMatching(string nik);
        Task<ProfileByNik> GetProfileByNikNoMatchingNew(string nik, int id);
        Task<string> GetNikNoMatchingByNoPengajuan(string NoPengajuan);
        Task<string> GetNikNoMatchingByIdPengajuan(int id);
        Task<ProfileByNik> GetProfileByCIF(string cif);
        Task<ProfileByNik> GetProfileEmpByCIF(string cif);
        Task<Tbl_DataKTP_Demografis> GetDataDemografis(string nik);
        Task<DetailLoginThirdParty2> GetCS();
        Task<DetailLogin> GetPenyelia(string npp, int unitId);
        Task<DetailLogin> GetPemimpin(string npp, int UnitId);
        Task<Tbl_DataKTP_Demografis_Temp> GetDataDemografisTemp(string nik);
        Task<Tbl_DataKTP_Demografis_Temp> GetDataDemografisTempOnProgress(string nik);
        Task<ProfileByNik> GetProfileByNikISO(string nik);
        Task<ProfileByNikOnlyFinger> GetProfileByNikISOBio(string nik);
        Task UpdateDataDemografis(Tbl_DataKTP_Demografis data);
        Task UpdateDataFinger(Tbl_DataKTP_Finger data);
        Task UpdateDataFingerEmployee(Tbl_DataKTP_Finger_Employee data);
        Task UpdateDataPhoto(Tbl_DataKTP_Photo data);
        Task UpdateDataPhotoCam(Tbl_DataKTP_PhotoCam data);
        Task UpdateDataSignature(Tbl_DataKTP_Signature data);
        Task<Tbl_DataKTP_Photo> GetPhotoKtp(string nik);
        Task<Tbl_DataKTP_Photo_Temp> GetPhotoKtpTemp(string nik);
        Task<Tbl_DataKTP_Signature> GetPhotoSignature(string nik);
        Task<Tbl_DataKTP_Signature_Temp> GetPhotoSignatureTemp(string nik);
        Task<Tbl_DataKTP_PhotoCam> GetPhotoCam(string nik);
        Task<Tbl_DataKTP_PhotoCam_Temp> GetPhotoCamTemp(string nik);
        Task<Tbl_DataKTP_Finger> GetPhotoFinger(string nik, string fingerType);
        Task<List<Tbl_DataKTP_Finger>> GetPhotoFingerExisting(string nik);
        Task<Tbl_DataKTP_Finger_Temp> GetPhotoFingerTemp(string nik, string fingerType);
        Task<Tbl_DataKTP_Finger_Employee> GetPhotoFingerEmployee(string nik, string fingerType);
        Task<List<Tbl_DataKTP_Finger_Employee>> GetPhotoFingerEmployeeExisting(string nik);
        Task<Tbl_DataKTP_Finger_Employee_Temp> GetPhotoFingerEmployeeTemp(string nik, string fingerType);
        Task<Tbl_DataKTP_Finger> GetPhotoFingerLike(string nik, string fingerType);
        Task<Tbl_DataKTP_Finger_Employee> GetPhotoFingerEmployeeLike(string nik, string fingerType);
        long InsertDemografiLog(Tbl_DataKTP_Demografis_Log log);
        long InsertFingerLog(Tbl_DataKTP_Finger_Log log);
        long InsertFingerEmployeeLog(Tbl_DataKTP_Finger_Employee_Log log);
        long InsertPhotoLog(Tbl_DataKTP_Photo_Log log);
        long InsertPhotoCamLog(Tbl_DataKTP_PhotoCam_Log log);
        long InsertSignatureLog(Tbl_DataKTP_Signature_Log log);
        long InsertCIFLog(Tbl_DataKTP_CIF log);
        long InsertHistoryPengajuan(Tbl_LogHistoryPengajuan tbl_LogHistoryPengajuan);
        Task<ProfileByNik> GetProfileByNikEmp(string nik);
    }
}
