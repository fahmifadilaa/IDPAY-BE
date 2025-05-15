using Ekr.Core.Entities;
using Ekr.Core.Entities.DataEnrollment;
using Ekr.Core.Entities.DataEnrollment.ViewModel;
using Ekr.Core.Entities.DataKTP;
using Ekr.Core.Entities.DataMaster.AlatReader;
using Ekr.Core.Entities.DataMaster.DataReader.Entity;
using Ekr.Core.Entities.DataMaster.Utility;
using Ekr.Core.Entities.Enrollment;
using Ekr.Core.Entities.Logging;
using Ekr.Core.Entities.SettingThreshold;
using Ekr.Core.Entities.ThirdParty;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ekr.Repository.Contracts.EnrollmentNoMatching
{
    public interface IEnrollmentNoMatchingRepository
    {

        Task<GridResponse<EnrollNoMatchingData>> GetPengajuanNoMatchingList(EnrollNoMatchingFilter filter, int currentUserId);
        Task<GridResponse<TblEnrollNoMatchingLogVM>> GetEnrollNoMatchingLogAsync(int id);
        Task<bool> GetEnrollwithourFRAsync(string nik);
        Task<bool> GetEnrollStatusFRAsync(string nik);
        Task<bool> GetEnrollStatusFRAsyncForEnrollBiasa(string nik);
        Task<Tbl_Inbox_Enrollment_Temp_Detail> InsertEnrollNoMatchingLogAsync(Tbl_Inbox_Enrollment_Temp_Detail req);
        Task<string> GetProbabilityDivision(string nik);
        Task<string> GetProbabilityDivisionV2(string tipeTreshold);
        Task UpdateEnrollNoMatchingStatusAsync(EnrollNoMatchingStatusRequest req, int updatedById, string npp,
            string unitCode, int unitId, int roleid);
        Task MoveTempDataToMainTable(EnrollNoMatchingStatusRequest req);
        Task UpdateTempDataToMainTable(EnrollNoMatchingStatusRequest req, string nik);
        Task UpdateIsactiveTempTable(EnrollNoMatchingStatusRequest req);
        Task<GridResponse<DataDropdownServerSide>> GetListPenyelia(int unitId);
        Task<GridResponse<DataDropdownServerSide>> GetListPemimpin(int unitId);
        void InsertEnrollFlow(Tbl_DataKTP_Demografis demografis, Tbl_DataKTP_Demografis_Log demografis_Log,
            Tbl_DataKTP_Photo photo,
            Tbl_DataKTP_Photo_Log photo_Log, Tbl_DataKTP_Signature signature,
            Tbl_DataKTP_Signature_Log signature_Log, Tbl_DataKTP_PhotoCam photoCam,
            Tbl_DataKTP_PhotoCam_Log photoCam_Log, List<Tbl_DataKTP_Finger> finger, List<Tbl_DataKTP_Finger_Log> finger_Log,
            Tbl_DataKTP_Photo exPhoto, Tbl_DataKTP_Signature exSignature, Tbl_DataKTP_PhotoCam exPhotoCam,
            List<Tbl_DataKTP_Finger> exFinger, Tbl_Mapping_Pegawai_KTP pegawai_KTP,
            Tbl_MasterAlatReaderLog readerLog, List<Tbl_DataKTP_Finger_Employee> fingerEmployee, List<Tbl_DataKTP_Finger_Employee> exFingerEmployee);

        Task<(bool status, string msg)> InsertEnrollFlowNoMatching(EnrollmentFRLog mappingFRNIK, Tbl_DataKTP_Demografis_Temp demografis, Tbl_DataKTP_Demografis_Log demografis_Log,
            Tbl_DataKTP_Photo_Temp photo,
            Tbl_DataKTP_Photo_Log photo_Log, Tbl_DataKTP_Signature_Temp signature,
            Tbl_DataKTP_Signature_Log signature_Log, Tbl_DataKTP_PhotoCam_Temp photoCam,
            Tbl_DataKTP_PhotoCam_Log photoCam_Log, List<Tbl_DataKTP_Finger_Temp> finger, List<Tbl_DataKTP_Finger_Log> finger_Log,
            Tbl_DataKTP_Photo_Temp exPhoto, Tbl_DataKTP_Signature_Temp exSignature, Tbl_DataKTP_PhotoCam_Temp exPhotoCam,
            List<Tbl_DataKTP_Finger_Temp> exFinger, Tbl_Mapping_Pegawai_KTP pegawai_KTP,
            Tbl_MasterAlatReaderLog readerLog, List<Tbl_DataKTP_Finger_Employee_Temp> fingerEmployee, List<Tbl_DataKTP_Finger_Employee_Temp> exFingerEmployee,
            Tbl_Inbox_Enrollment_Temp inboxTemp, Tbl_Inbox_Enrollment_Temp_Detail inboxTempDetail);
        Task<(bool status, string msg)> InsertEnrollFlowNoMatchingv2(EnrollmentFRLog mappingFRNIK, Tbl_DataKTP_Demografis_Temp demografis, Tbl_DataKTP_Demografis_Log demografis_Log,
            Tbl_DataKTP_Photo_Temp photo,
            Tbl_DataKTP_Photo_Log photo_Log, Tbl_DataKTP_Signature_Temp signature,
            Tbl_DataKTP_Signature_Log signature_Log, Tbl_DataKTP_PhotoCam_Temp photoCam,
            Tbl_DataKTP_PhotoCam_Log photoCam_Log, List<Tbl_DataKTP_Finger_Temp> finger, List<Tbl_DataKTP_Finger_Log> finger_Log,
            Tbl_DataKTP_Photo_Temp exPhoto, Tbl_DataKTP_Signature_Temp exSignature, Tbl_DataKTP_PhotoCam_Temp exPhotoCam,
            List<Tbl_DataKTP_Finger_Temp> exFinger, Tbl_Mapping_Pegawai_KTP pegawai_KTP,
            Tbl_MasterAlatReaderLog readerLog, List<Tbl_DataKTP_Finger_Employee_Temp> fingerEmployee, List<Tbl_DataKTP_Finger_Employee_Temp> exFingerEmployee,
            Tbl_Inbox_Enrollment_Temp inboxTemp, Tbl_Inbox_Enrollment_Temp_Detail inboxTempDetail, Tbl_Inbox_Enrollment_Temp_Detail inboxTempDetailPenyelia);

        Task<(bool status, string msg)> InsertEnrollFlowNoMatchingThirdParty(EnrollmentFRLog mappingFRNIK, Tbl_DataKTP_Demografis_Temp demografis, Tbl_DataKTP_Demografis_Log demografis_Log,
            Tbl_DataKTP_Photo_Temp photo,
            Tbl_DataKTP_Photo_Log photo_Log, Tbl_DataKTP_Signature_Temp signature,
            Tbl_DataKTP_Signature_Log signature_Log, Tbl_DataKTP_PhotoCam_Temp photoCam,
            Tbl_DataKTP_PhotoCam_Log photoCam_Log, List<Tbl_DataKTP_Finger_Temp> finger, List<Tbl_DataKTP_Finger_Log> finger_Log,
            Tbl_DataKTP_Photo_Temp exPhoto, Tbl_DataKTP_Signature_Temp exSignature, Tbl_DataKTP_PhotoCam_Temp exPhotoCam,
            List<Tbl_DataKTP_Finger_Temp> exFinger, Tbl_Mapping_Pegawai_KTP pegawai_KTP,
            Tbl_MasterAlatReaderLog readerLog, List<Tbl_DataKTP_Finger_Employee_Temp> fingerEmployee, List<Tbl_DataKTP_Finger_Employee_Temp> exFingerEmployee,
            Tbl_Inbox_Enrollment_Temp inboxTemp, Tbl_Inbox_Enrollment_Temp_Detail inboxTempDetail);
        Task<(bool status, string msg)> InsertEnrollFlowNoMatchingIKD(Tbl_Enrollment_IKD mappingNIK, Tbl_DataKTP_Demografis_Temp demografis, Tbl_DataKTP_Demografis_Log demografis_Log,
            Tbl_DataKTP_Photo_Temp photo,
            Tbl_DataKTP_Photo_Log photo_Log, Tbl_DataKTP_Signature_Temp signature,
            Tbl_DataKTP_Signature_Log signature_Log, Tbl_DataKTP_PhotoCam_Temp photoCam,
            Tbl_DataKTP_PhotoCam_Log photoCam_Log, List<Tbl_DataKTP_Finger_Temp> finger, List<Tbl_DataKTP_Finger_Log> finger_Log,
            Tbl_DataKTP_Photo_Temp exPhoto, Tbl_DataKTP_Signature_Temp exSignature, Tbl_DataKTP_PhotoCam_Temp exPhotoCam,
            List<Tbl_DataKTP_Finger_Temp> exFinger, Tbl_Mapping_Pegawai_KTP pegawai_KTP,
            Tbl_MasterAlatReaderLog readerLog, List<Tbl_DataKTP_Finger_Employee_Temp> fingerEmployee, List<Tbl_DataKTP_Finger_Employee_Temp> exFingerEmployee,
            Tbl_Inbox_Enrollment_Temp inboxTemp, Tbl_Inbox_Enrollment_Temp_Detail inboxTempDetail);

        Task<(bool status, string msg)> InsertEnrollFlowIKD(Tbl_DataKTP_Demografis_Temp demografis, Tbl_DataKTP_Demografis_Log demografis_Log,
            Tbl_DataKTP_Photo_Temp photo,
            Tbl_DataKTP_Photo_Log photo_Log, Tbl_DataKTP_Signature_Temp signature,
            Tbl_DataKTP_Signature_Log signature_Log, Tbl_DataKTP_PhotoCam_Temp photoCam,
            Tbl_DataKTP_PhotoCam_Log photoCam_Log, List<Tbl_DataKTP_Finger_Temp> finger, List<Tbl_DataKTP_Finger_Log> finger_Log,
            Tbl_DataKTP_Photo_Temp exPhoto, Tbl_DataKTP_Signature_Temp exSignature, Tbl_DataKTP_PhotoCam_Temp exPhotoCam,
            List<Tbl_DataKTP_Finger_Temp> exFinger, Tbl_Mapping_Pegawai_KTP pegawai_KTP,
            Tbl_MasterAlatReaderLog readerLog, List<Tbl_DataKTP_Finger_Employee_Temp> fingerEmployee, List<Tbl_DataKTP_Finger_Employee_Temp> exFingerEmployee,
            Tbl_Inbox_Enrollment_Temp inboxTemp, Tbl_Inbox_Enrollment_Temp_Detail inboxTempDetail, Tbl_Enrollment_IKD MappingIKD);

        Task<(bool status, string msg)> InsertEnrollFlow2(Tbl_DataKTP_Demografis demografis, Tbl_DataKTP_Demografis_Log demografis_Log,
            Tbl_DataKTP_Photo photo,
            Tbl_DataKTP_Photo_Log photo_Log, Tbl_DataKTP_Signature signature,
            Tbl_DataKTP_Signature_Log signature_Log, Tbl_DataKTP_PhotoCam photoCam,
            Tbl_DataKTP_PhotoCam_Log photoCam_Log, List<Tbl_DataKTP_Finger> finger, List<Tbl_DataKTP_Finger_Employee> finger_Employee,
            List<Tbl_DataKTP_Finger_Log> finger_Log, List<Tbl_DataKTP_Finger_Employee_Log> finger_Employee_Log,
            Tbl_DataKTP_Photo exPhoto, Tbl_DataKTP_Signature exSignature, Tbl_DataKTP_PhotoCam exPhotoCam,
            List<Tbl_DataKTP_Finger> exFinger, List<Tbl_DataKTP_Finger_Employee> exFingerEmployee, Tbl_MasterAlatReaderLog readerLog, Tbl_Mapping_Pegawai_KTP dataMapping);
        void UpdatesEnrollFlow(Tbl_DataKTP_Demografis demografis, Tbl_DataKTP_Demografis_Log demografis_Log,
            Tbl_DataKTP_Photo photo,
            Tbl_DataKTP_Photo_Log photo_Log, Tbl_DataKTP_Signature signature,
            Tbl_DataKTP_Signature_Log signature_Log, Tbl_DataKTP_PhotoCam photoCam,
            Tbl_DataKTP_PhotoCam_Log photoCam_Log, Tbl_DataKTP_Finger finger_kanan, Tbl_DataKTP_Finger finger_kiri,
            Tbl_DataKTP_Finger_Log finger_kanan_log, Tbl_DataKTP_Finger_Log finger_kiri_log,
            Tbl_DataKTP_Finger_Employee fingerEmployee_kanan, Tbl_DataKTP_Finger_Employee fingerEmployee_kiri,
            Tbl_DataKTP_Finger_Employee_Log fingerEmployee_kanan_Log, Tbl_DataKTP_Finger_Employee_Log fingerEmployee_kiri_Log,
            Tbl_MasterAlatReaderLogActvity LogActivity, Tbl_MasterAlatReaderLog ReaderLog);
        void UpdatesPhotoCam(Tbl_DataKTP_PhotoCam photoCam, Tbl_DataKTP_PhotoCam_Log photoCam_Log,
            Tbl_MasterAlatReaderLogActvity LogActivity, Tbl_MasterAlatReaderLog ReaderLog);
        Task<string> MigrateDataEnroll(string apikey, bool isProxy, string ipProxy);
        Task<IEnumerable<Tbl_DataKTP_Finger>> GetDataKtpFingersJpgFormattedbyNIK(string nik);
        Task<IEnumerable<Tbl_DataKTP_Demografis>> GetDataDemografisByNik(string nik);
        Task<GridResponse<InboxEnrollNoMatchingData>> GetDBEnroll(DataEnrollTempFilter filter);
        Task<GridResponse<MonitoringEnroll>> GetDBEnrollSec(DataEnrollTempFilter filter);
        Task<bool> IsNppEnrolled(string npp);
        Task<string> GetNIK(string npp);
        Task<int> GetPegawaiByNpp(string npp);
        Task<int> GetUnitByCode(string code);
        Task<ConvertLatLong_ViewModels> ConvertLatLong(string apikey, string alamatEncode, bool isProxy, string ipProxy);
        Task<GridResponse<InboxDataEnrollVM>> LoadDataInboxEnroll(InboxDataEnrollFilterVM inboxDataEnrollFilterVM);
        Task<List<Tbl_LogHistoryPengajuanVM>> LoadDataHistoryPengajuan(HistorySubmissionFilterVM historySubmissionFilterVM);
        long InsertKtpFingerLog(Tbl_DataKTP_Finger_Log log);
        Task<IEnumerable<Tbl_DataKTP_Finger>> GetDataKtpFingersJpgFormatted();
        Task<int> UpdateDataKtpFingerAsync(Tbl_DataKTP_Finger data);
        Task<Tbl_ThirdPartyLog> CreateThirdPartyLog(Tbl_ThirdPartyLog req);
        Task<IEnumerable<ExportMonitoringEnroll>> ExportDBEnroll(ExportDataEnrollTempFilter filter);
        Task<Tbl_MappingNIK_Pegawai> IsEmployee(string nik);
        Task<Tbl_DataNIK_Pegawai> MappingNppNik(string npp);
        Task<Tbl_Mapping_Pegawai_KTP> MappingNppNikByNik(string nik);
        Task<IEnumerable<Tbl_DataKTP_Finger>> GetISO(string nik);
        Task<IEnumerable<Tbl_DataKTP_Finger>> GetISOEmp(string nik);
        Task<Tbl_LogError_FaceRecognition> InsertLogFacerecognition(Tbl_LogError_FaceRecognition req);

        Task<Tbl_ScanIKD_Session> CheckIKDSession(string npp);
        Task<Tbl_ScanIKD_Session> InsertNewIKDSession(Tbl_ScanIKD_Session req);
        Task<Tbl_ScanIKD_Session> UpdateIKDSession(Tbl_ScanIKD_Session req);
    }
}
