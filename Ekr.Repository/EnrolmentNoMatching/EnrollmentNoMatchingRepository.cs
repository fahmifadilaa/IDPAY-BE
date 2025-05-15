using Dapper;
using Ekr.Core.Configuration;
using Ekr.Core.Constant;
using Ekr.Core.Entities;
using Ekr.Core.Entities.DataEnrollment;
using Ekr.Core.Entities.DataEnrollment.ViewModel;
using Ekr.Core.Entities.DataKTP;
using Ekr.Core.Entities.DataMaster.AlatReader;
using Ekr.Core.Entities.DataMaster.DataReader.Entity;
using Ekr.Core.Entities.DataMaster.Unit;
using Ekr.Core.Entities.DataMaster.Utility;
using Ekr.Core.Entities.Enrollment;
using Ekr.Core.Entities.Logging;
using Ekr.Core.Entities.SettingThreshold;
using Ekr.Core.Entities.ThirdParty;
using Ekr.Core.Helper;
using Ekr.Core.Securities.Symmetric;
using Ekr.Dapper.Connection.Base;
using Ekr.Dapper.Connection.Contracts.Base;
using Ekr.Dapper.Connection.Contracts.Sql;
using Ekr.Repository.Contracts.DataMaster.SystemParameters;
using Ekr.Repository.Contracts.EnrollmentNoMatching;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using ServiceStack.OrmLite;
using ServiceStack.Text;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace Ekr.Repository.EnrolmentNoMatching
{
    public class EnrollmentNoMatchingRepository : BaseRepository, IEnrollmentNoMatchingRepository
    {
        private readonly IBaseConnection _baseConnection;
        private readonly IBaseConnection _baseConnectionLog;
        private readonly ISysParameterRepository _sysParameterRepository;
        public EnrollmentNoMatchingRepository(IEKtpReaderBackendDb con,
            IOptions<ConnectionStringConfig> options, IOptions<ErrorMessageConfig> options2,
            ISysParameterRepository sysParameterRepository) : base(con)
        {
            _baseConnection = new SqlServerConnection(options.Value.dbConnection2, options2);
            _baseConnectionLog = new SqlServerConnection(options.Value.dbConnection1, options2);
            _sysParameterRepository = sysParameterRepository;
        }

        #region INSERT
        public void InsertEnrollFlow(Tbl_DataKTP_Demografis demografis, Tbl_DataKTP_Demografis_Log demografis_Log,
            Tbl_DataKTP_Photo photo,
            Tbl_DataKTP_Photo_Log photo_Log, Tbl_DataKTP_Signature signature,
            Tbl_DataKTP_Signature_Log signature_Log, Tbl_DataKTP_PhotoCam photoCam,
            Tbl_DataKTP_PhotoCam_Log photoCam_Log, List<Tbl_DataKTP_Finger> finger, List<Tbl_DataKTP_Finger_Log> finger_Log,
            Tbl_DataKTP_Photo exPhoto, Tbl_DataKTP_Signature exSignature, Tbl_DataKTP_PhotoCam exPhotoCam,
            List<Tbl_DataKTP_Finger> exFinger, Tbl_Mapping_Pegawai_KTP pegawai_KTP,
            Tbl_MasterAlatReaderLog readerLog, List<Tbl_DataKTP_Finger_Employee> fingerEmployee, List<Tbl_DataKTP_Finger_Employee> exFingerEmployee)
        {
            Db.WithTransaction(c =>
            {
                if (demografis_Log?.Id != 0)
                {
                    _ = c.Insert(demografis_Log, true);
                    c.Update(demografis);
                }
                else
                {
                    if (demografis.CIF == "null" || demografis.CIF == null || string.IsNullOrWhiteSpace(demografis.CIF))
                    {
                        demografis.CIF = null;
                    }
                    _ = c.Insert(demografis, true);
                }
                if (photo_Log?.Id != 0)
                {
                    _ = c.Insert(photo_Log, true);
                    c.Delete(exPhoto);
                }
                if (photo?.Nik != null) _ = c.Insert(photo, true);
                if (signature_Log?.Id != 0)
                {
                    _ = c.Insert(signature_Log, true);
                    c.Delete(exSignature);
                }
                if (signature?.Nik != null) _ = c.Insert(signature, true);
                if (photoCam_Log?.Nik != null)
                {
                    _ = c.Insert(photoCam_Log, true);
                    c.Delete(exPhotoCam);
                }
                if (photoCam?.Nik != null) _ = c.Insert(photoCam, true);
                if (finger_Log != null)
                {
                    foreach (var f in finger_Log)
                    {
                        if (f.Nik != null) _ = c.Insert(f, true);
                    }
                    foreach (var e in exFinger)
                    {
                        if (e.Nik != null) c.Delete(e);
                    }
                }
                if (finger != null)
                {
                    foreach (var f in finger)
                    {
                        if (f.Nik != null) _ = c.Insert(f, true);
                    }
                }
                if (fingerEmployee != null)
                {
                    foreach (var f in fingerEmployee)
                    {
                        if (f.Nik != null) _ = c.Insert(f, true);
                    }
                }
                if (pegawai_KTP?.NIK != null)
                {
                    c.Insert(pegawai_KTP, true);
                }
                if (readerLog?.Uid != null)
                {
                    c.Insert(readerLog, true);
                }
            });
        }

        public async Task<(bool status, string msg)> InsertEnrollFlowNoMatching(EnrollmentFRLog mappingFRNIK, Tbl_DataKTP_Demografis_Temp demografis, Tbl_DataKTP_Demografis_Log demografis_Log,
            Tbl_DataKTP_Photo_Temp photo,
            Tbl_DataKTP_Photo_Log photo_Log, Tbl_DataKTP_Signature_Temp signature,
            Tbl_DataKTP_Signature_Log signature_Log, Tbl_DataKTP_PhotoCam_Temp photoCam,
            Tbl_DataKTP_PhotoCam_Log photoCam_Log, List<Tbl_DataKTP_Finger_Temp> finger, List<Tbl_DataKTP_Finger_Log> finger_Log,
            Tbl_DataKTP_Photo_Temp exPhoto, Tbl_DataKTP_Signature_Temp exSignature, Tbl_DataKTP_PhotoCam_Temp exPhotoCam,
            List<Tbl_DataKTP_Finger_Temp> exFinger, Tbl_Mapping_Pegawai_KTP pegawai_KTP,
            Tbl_MasterAlatReaderLog readerLog, List<Tbl_DataKTP_Finger_Employee_Temp> fingerEmployee, List<Tbl_DataKTP_Finger_Employee_Temp> exFingerEmployee,
            Tbl_Inbox_Enrollment_Temp inboxTemp, Tbl_Inbox_Enrollment_Temp_Detail inboxTempDetail)
        {

            try {
                var idDemo = 0;
                var idInboxTemp = 0;

                await Db.WithTransaction(async c =>
                {
                    if (demografis_Log?.NIK != null)
                    {
                        //idDemo = await InsertDemografiLogReturnId(demografis_Log);
                        if (demografis.Id != 0)
                        {
                            _ = UpdateDemografiTemp(demografis);
                        }
                        else {
                            if (demografis.CIF == "null" || demografis.CIF == null || string.IsNullOrWhiteSpace(demografis.CIF))
                            {
                                demografis.CIF = null;
                            }
                            idDemo = await InsertDemografiTemp(demografis);
                        }

                    }
                    else
                    {
                        if (demografis.CIF == "null" || demografis.CIF == null || string.IsNullOrWhiteSpace(demografis.CIF))
                        {
                            demografis.CIF = null;
                        }
                        idDemo = await InsertDemografiTemp(demografis);
                        
                    }

                    if (photo_Log?.Nik != null)
                    {
                        //_ = InsertPhotoLog(photo_Log);
                        if(exPhoto != null) _ = DeletePhotoTemp(exPhoto);
                    }
                    photo.DemografisTempId = idDemo;
                    if (photo?.Nik != null) _ = InsertPhotoTemp(photo);

                    if (signature_Log?.Nik != null)
                    {
                        _ = InsertSignatureLog(signature_Log);
                        if (exSignature != null) _ = DeleteSignatureTemp(exSignature);
                    }
                    signature.DemografisTempId = idDemo;
                    if (signature?.Nik != null) _ = InsertSignatureTemp(signature);

                    if (photoCam_Log?.Nik != null)
                    {
                        //_ = InsertPhotoCamLog(photoCam_Log);
                        if (exPhotoCam != null) _ = DeletePhotoCamTemp(exPhotoCam);
                    }
                    photoCam.DemografisTempId = idDemo;
                    if (photoCam?.Nik != null) _ = InsertPhotoCamTemp(photoCam);

                    //if (finger_Log != null)
                    //{
                    //    foreach (var f in finger_Log)
                    //    {
                    //if (f.Nik != null) _ = InsertFingerLog(f);
                    //    }
                    //    foreach (var e in exFinger)
                    //    {
                    //        if (e.Nik != null) _ = DeleteFingerTemp(e);
                    //    }
                    //}

                    if (exFinger != null) {
                        foreach (var e in exFinger)
                        {
                            if (e.Nik != null) _ = DeleteFingerTemp(e);
                        }
                    }
                    if (finger != null)
                    {
                        foreach (var f in finger)
                        {
                            f.DemografisTempId = idDemo;
                            if (f.Nik != null) _ = InsertFingerTemp(f);
                        }
                    }

                    if (exFingerEmployee != null) {
                        foreach (var f in exFingerEmployee)
                        {
                            f.DemografisTempId = idDemo;
                            if (f.Nik != null) _ = DeleteFingerEmployeeTemp(f);
                        }
                    }

                    if (fingerEmployee != null)
                    {
                        foreach (var f in fingerEmployee)
                        {
                            f.DemografisTempId = idDemo;
                            if (f.Nik != null) _ = InsertFingerEmployeeTemp(f);
                        }
                    }

                    if (pegawai_KTP?.NIK != null)
                    {
                        if (pegawai_KTP.Id != 0)
                        {
                            _ = UpdateMappingKTP(pegawai_KTP);
                        }
                        else
                        {
                            _ = InsertMappingPegawaiKTP(pegawai_KTP);
                        }
                    }

                    inboxTemp.DemografisTempId = idDemo;
                    inboxTemp.NoPengajuan = demografis.NoPengajuan;

                    if (readerLog?.Uid != null)
                    {
                        _ = InsertReaderLog(readerLog);
                    }


                });


                const string queryInboxEnrollmentTemp = "Insert Into [Tbl_Inbox_Enrollment_Temp] (" +
                    "[DemografisTempId]," +
                    "[ApprovedByRoleId]," +
                    "[ApprovedByUnitId]," +
                    "[CreatedById]," +
                    "[CreatedByNpp]," +
                    "[CreatedTime]," +
                    "[CreatedByUnitId]," +
                    "[CreatedByUnitCode]," +
                    "[ApprovedStatus]," +
                    "[UpdatedTime]," +
                    "[UpdatedById]," +
                    "[UpdatedByNpp]," +
                    "[UpdatedByUnitId]," +
                    "[UpdatedByUnitCode]," +
                    "[NoPengajuan]," +
                    "[ApprovedByEmployeeId]," +
                    "[ApprovedByEmployeeId2]," +
                    "[Status]) " +
                    "OUTPUT INSERTED.[Id] " +
                "values(" +
                    "@DemografisTempId," +
                    "@ApprovedByRoleId," +
                    "@ApprovedByUnitId," +
                    "@CreatedById," +
                    "@CreatedByNpp," +
                    "@CreatedTime," +
                    "@CreatedByUnitId," +
                    "@CreatedByUnitCode," +
                    "@ApprovedStatus," +
                    "@UpdatedTime," +
                    "@UpdatedById," +
                    "@UpdatedByNpp," +
                    "@UpdatedByUnitId," +
                    "@UpdatedByUnitCode," +
                    "@NoPengajuan," +
                    "@ApprovedByEmployeeId," +
                    "@ApprovedByEmployeeId2," +
                    "@Status)";

                idInboxTemp = await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(queryInboxEnrollmentTemp, new
                {
                    inboxTemp.DemografisTempId,
                    inboxTemp.ApprovedByRoleId,
                    inboxTemp.ApprovedByUnitId,
                    inboxTemp.CreatedById,
                    inboxTemp.CreatedByNpp,
                    inboxTemp.CreatedTime,
                    inboxTemp.CreatedByUnitId,
                    inboxTemp.CreatedByUnitCode,
                    inboxTemp.ApprovedStatus,
                    inboxTemp.UpdatedTime,
                    inboxTemp.UpdatedById,
                    inboxTemp.UpdatedByNpp,
                    inboxTemp.UpdatedByUnitId,
                    inboxTemp.UpdatedByUnitCode,
                    inboxTemp.NoPengajuan,
                    inboxTemp.ApprovedByEmployeeId,
                    inboxTemp.ApprovedByEmployeeId2,
                    inboxTemp.Status
                }));

                inboxTempDetail.InboxEnrollmentTempId = idInboxTemp;
                inboxTempDetail.NoPengajuan = demografis.NoPengajuan;
                mappingFRNIK.inboxEnrollmentId = idInboxTemp;
                await InsertMappingNIKFR(mappingFRNIK);


                const string queryInboxEnrollmentTempDetail = "Insert Into [Tbl_Inbox_Enrollment_Temp_Detail] (" +
                    "[InboxEnrollmentTempId]," +
                    "[Notes]," +
                    "[SubmitById]," +
                    "[SubmitByNpp]," +
                    "[CreatedTime]," +
                    "[SubmitedByUnitId]," +
                    "[SubmitedByUnitCode]," +
                    "[ApprovedStatus]," +
                    "[NoPengajuan]," +
                    "[Status]) " +
                "values(" +
                    "@InboxEnrollmentTempId," +
                    "@Notes," +
                    "@SubmitById," +
                    "@SubmitByNpp," +
                    "@CreatedTime," +
                    "@SubmitedByUnitId," +
                    "@SubmitedByUnitCode," +
                    "@ApprovedStatus," +
                    "@NoPengajuan," +
                    "@Status)";

                await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(queryInboxEnrollmentTempDetail, new
                {
                    inboxTempDetail.InboxEnrollmentTempId,
                    inboxTempDetail.Notes,
                    inboxTempDetail.SubmitById,
                    inboxTempDetail.SubmitByNpp,
                    inboxTempDetail.CreatedTime,
                    inboxTempDetail.SubmitedByUnitId,
                    inboxTempDetail.SubmitedByUnitCode,
                    inboxTempDetail.ApprovedStatus,
                    inboxTempDetail.NoPengajuan,
                    inboxTempDetail.Status
                }));

                return (true, "");
            }
            catch (Exception Ex) {

                return (false, Ex.Message);
            }
        }

        public async Task<(bool status, string msg)> InsertEnrollFlowNoMatchingv2(EnrollmentFRLog mappingFRNIK, Tbl_DataKTP_Demografis_Temp demografis, Tbl_DataKTP_Demografis_Log demografis_Log,
            Tbl_DataKTP_Photo_Temp photo,
            Tbl_DataKTP_Photo_Log photo_Log, Tbl_DataKTP_Signature_Temp signature,
            Tbl_DataKTP_Signature_Log signature_Log, Tbl_DataKTP_PhotoCam_Temp photoCam,
            Tbl_DataKTP_PhotoCam_Log photoCam_Log, List<Tbl_DataKTP_Finger_Temp> finger, List<Tbl_DataKTP_Finger_Log> finger_Log,
            Tbl_DataKTP_Photo_Temp exPhoto, Tbl_DataKTP_Signature_Temp exSignature, Tbl_DataKTP_PhotoCam_Temp exPhotoCam,
            List<Tbl_DataKTP_Finger_Temp> exFinger, Tbl_Mapping_Pegawai_KTP pegawai_KTP,
            Tbl_MasterAlatReaderLog readerLog, List<Tbl_DataKTP_Finger_Employee_Temp> fingerEmployee, List<Tbl_DataKTP_Finger_Employee_Temp> exFingerEmployee,
            Tbl_Inbox_Enrollment_Temp inboxTemp, Tbl_Inbox_Enrollment_Temp_Detail inboxTempDetail, Tbl_Inbox_Enrollment_Temp_Detail inboxTempDetailPenyelia)
        {

            try
            {
                var idDemo = 0;
                var idInboxTemp = 0;

                await Db.WithTransaction(async c =>
                {
                    if (demografis_Log?.NIK != null)
                    {
                        //idDemo = await InsertDemografiLogReturnId(demografis_Log);
                        if (demografis.Id != 0)
                        {
                            _ = UpdateDemografiTemp(demografis);
                        }
                        else
                        {
                            if (demografis.CIF == "null" || demografis.CIF == null || string.IsNullOrWhiteSpace(demografis.CIF))
                            {
                                demografis.CIF = null;
                            }
                            idDemo = await InsertDemografiTemp(demografis);
                        }

                    }
                    else
                    {
                        if (demografis.CIF == "null" || demografis.CIF == null || string.IsNullOrWhiteSpace(demografis.CIF))
                        {
                            demografis.CIF = null;
                        }
                        idDemo = await InsertDemografiTemp(demografis);

                    }

                    if (photo_Log?.Nik != null)
                    {
                        //_ = InsertPhotoLog(photo_Log);
                        if (exPhoto != null) _ = DeletePhotoTemp(exPhoto);
                    }
                    photo.DemografisTempId = idDemo;
                    if (photo?.Nik != null) _ = InsertPhotoTemp(photo);

                    if (signature_Log?.Nik != null)
                    {
                        _ = InsertSignatureLog(signature_Log);
                        if (exSignature != null) _ = DeleteSignatureTemp(exSignature);
                    }
                    signature.DemografisTempId = idDemo;
                    if (signature?.Nik != null) _ = InsertSignatureTemp(signature);

                    if (photoCam_Log?.Nik != null)
                    {
                        //_ = InsertPhotoCamLog(photoCam_Log);
                        if (exPhotoCam != null) _ = DeletePhotoCamTemp(exPhotoCam);
                    }
                    photoCam.DemografisTempId = idDemo;
                    if (photoCam?.Nik != null) _ = InsertPhotoCamTemp(photoCam);

                    //if (finger_Log != null)
                    //{
                    //    foreach (var f in finger_Log)
                    //    {
                    //if (f.Nik != null) _ = InsertFingerLog(f);
                    //    }
                    //    foreach (var e in exFinger)
                    //    {
                    //        if (e.Nik != null) _ = DeleteFingerTemp(e);
                    //    }
                    //}

                    if (exFinger != null)
                    {
                        foreach (var e in exFinger)
                        {
                            if (e.Nik != null) _ = DeleteFingerTemp(e);
                        }
                    }
                    if (finger != null)
                    {
                        foreach (var f in finger)
                        {
                            f.DemografisTempId = idDemo;
                            if (f.Nik != null) _ = InsertFingerTemp(f);
                        }
                    }

                    if (exFingerEmployee != null)
                    {
                        foreach (var f in exFingerEmployee)
                        {
                            f.DemografisTempId = idDemo;
                            if (f.Nik != null) _ = DeleteFingerEmployeeTemp(f);
                        }
                    }

                    if (fingerEmployee != null)
                    {
                        foreach (var f in fingerEmployee)
                        {
                            f.DemografisTempId = idDemo;
                            if (f.Nik != null) _ = InsertFingerEmployeeTemp(f);
                        }
                    }

                    if (pegawai_KTP?.NIK != null)
                    {
                        if (pegawai_KTP.Id != 0)
                        {
                            _ = UpdateMappingKTP(pegawai_KTP);
                        }
                        else
                        {
                            _ = InsertMappingPegawaiKTP(pegawai_KTP);
                        }
                    }

                    inboxTemp.DemografisTempId = idDemo;
                    inboxTemp.NoPengajuan = demografis.NoPengajuan;

                    if (readerLog?.Uid != null)
                    {
                        _ = InsertReaderLog(readerLog);
                    }


                });


                const string queryInboxEnrollmentTemp = "Insert Into [Tbl_Inbox_Enrollment_Temp] (" +
                    "[DemografisTempId]," +
                    "[ApprovedByRoleId]," +
                    "[ApprovedByUnitId]," +
                    "[CreatedById]," +
                    "[CreatedByNpp]," +
                    "[CreatedTime]," +
                    "[CreatedByUnitId]," +
                    "[CreatedByUnitCode]," +
                    "[ApprovedStatus]," +
                    "[UpdatedTime]," +
                    "[UpdatedById]," +
                    "[UpdatedByNpp]," +
                    "[UpdatedByUnitId]," +
                    "[UpdatedByUnitCode]," +
                    "[NoPengajuan]," +
                    "[ApprovedByEmployeeId]," +
                    "[ApprovedByEmployeeId2]," +
                    "[Status]) " +
                    "OUTPUT INSERTED.[Id] " +
                "values(" +
                    "@DemografisTempId," +
                    "@ApprovedByRoleId," +
                    "@ApprovedByUnitId," +
                    "@CreatedById," +
                    "@CreatedByNpp," +
                    "@CreatedTime," +
                    "@CreatedByUnitId," +
                    "@CreatedByUnitCode," +
                    "@ApprovedStatus," +
                    "@UpdatedTime," +
                    "@UpdatedById," +
                    "@UpdatedByNpp," +
                    "@UpdatedByUnitId," +
                    "@UpdatedByUnitCode," +
                    "@NoPengajuan," +
                    "@ApprovedByEmployeeId," +
                    "@ApprovedByEmployeeId2," +
                    "@Status)";

                idInboxTemp = await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(queryInboxEnrollmentTemp, new
                {
                    inboxTemp.DemografisTempId,
                    inboxTemp.ApprovedByRoleId,
                    inboxTemp.ApprovedByUnitId,
                    inboxTemp.CreatedById,
                    inboxTemp.CreatedByNpp,
                    inboxTemp.CreatedTime,
                    inboxTemp.CreatedByUnitId,
                    inboxTemp.CreatedByUnitCode,
                    inboxTemp.ApprovedStatus,
                    inboxTemp.UpdatedTime,
                    inboxTemp.UpdatedById,
                    inboxTemp.UpdatedByNpp,
                    inboxTemp.UpdatedByUnitId,
                    inboxTemp.UpdatedByUnitCode,
                    inboxTemp.NoPengajuan,
                    inboxTemp.ApprovedByEmployeeId,
                    inboxTemp.ApprovedByEmployeeId2,
                    inboxTemp.Status
                }));

                inboxTempDetail.InboxEnrollmentTempId = idInboxTemp;
                inboxTempDetail.NoPengajuan = demografis.NoPengajuan;
                inboxTempDetailPenyelia.InboxEnrollmentTempId = idInboxTemp;
                inboxTempDetailPenyelia.NoPengajuan = demografis.NoPengajuan;
                mappingFRNIK.inboxEnrollmentId = idInboxTemp;
                await InsertMappingNIKFR(mappingFRNIK);


                const string queryInboxEnrollmentTempDetail = "Insert Into [Tbl_Inbox_Enrollment_Temp_Detail] (" +
                    "[InboxEnrollmentTempId]," +
                    "[Notes]," +
                    "[SubmitById]," +
                    "[SubmitByNpp]," +
                    "[CreatedTime]," +
                    "[SubmitedByUnitId]," +
                    "[SubmitedByUnitCode]," +
                    "[ApprovedStatus]," +
                    "[NoPengajuan]," +
                    "[Status]) " +
                "values(" +
                    "@InboxEnrollmentTempId," +
                    "@Notes," +
                    "@SubmitById," +
                    "@SubmitByNpp," +
                    "@CreatedTime," +
                    "@SubmitedByUnitId," +
                    "@SubmitedByUnitCode," +
                    "@ApprovedStatus," +
                    "@NoPengajuan," +
                    "@Status)";

                await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(queryInboxEnrollmentTempDetail, new
                {
                    inboxTempDetail.InboxEnrollmentTempId,
                    inboxTempDetail.Notes,
                    inboxTempDetail.SubmitById,
                    inboxTempDetail.SubmitByNpp,
                    inboxTempDetail.CreatedTime,
                    inboxTempDetail.SubmitedByUnitId,
                    inboxTempDetail.SubmitedByUnitCode,
                    inboxTempDetail.ApprovedStatus,
                    inboxTempDetail.NoPengajuan,
                    inboxTempDetail.Status
                }));

                await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(queryInboxEnrollmentTempDetail, new
                {
                    inboxTempDetailPenyelia.InboxEnrollmentTempId,
                    inboxTempDetailPenyelia.Notes,
                    inboxTempDetailPenyelia.SubmitById,
                    inboxTempDetailPenyelia.SubmitByNpp,
                    inboxTempDetailPenyelia.CreatedTime,
                    inboxTempDetailPenyelia.SubmitedByUnitId,
                    inboxTempDetailPenyelia.SubmitedByUnitCode,
                    inboxTempDetailPenyelia.ApprovedStatus,
                    inboxTempDetailPenyelia.NoPengajuan,
                    inboxTempDetailPenyelia.Status
                }));

                return (true, "");
            }
            catch (Exception Ex)
            {

                return (false, Ex.Message);
            }
        }


        public async Task<(bool status, string msg)> InsertEnrollFlowNoMatchingThirdParty(EnrollmentFRLog mappingFRNIK, Tbl_DataKTP_Demografis_Temp demografis, Tbl_DataKTP_Demografis_Log demografis_Log,
            Tbl_DataKTP_Photo_Temp photo,
            Tbl_DataKTP_Photo_Log photo_Log, Tbl_DataKTP_Signature_Temp signature,
            Tbl_DataKTP_Signature_Log signature_Log, Tbl_DataKTP_PhotoCam_Temp photoCam,
            Tbl_DataKTP_PhotoCam_Log photoCam_Log, List<Tbl_DataKTP_Finger_Temp> finger, List<Tbl_DataKTP_Finger_Log> finger_Log,
            Tbl_DataKTP_Photo_Temp exPhoto, Tbl_DataKTP_Signature_Temp exSignature, Tbl_DataKTP_PhotoCam_Temp exPhotoCam,
            List<Tbl_DataKTP_Finger_Temp> exFinger, Tbl_Mapping_Pegawai_KTP pegawai_KTP,
            Tbl_MasterAlatReaderLog readerLog, List<Tbl_DataKTP_Finger_Employee_Temp> fingerEmployee, List<Tbl_DataKTP_Finger_Employee_Temp> exFingerEmployee,
            Tbl_Inbox_Enrollment_Temp inboxTemp, Tbl_Inbox_Enrollment_Temp_Detail inboxTempDetail)
        {

            try
            {
                var idDemo = 0;
                var idInboxTemp = 0;



                await Db.WithTransaction(async c =>
                {
                    if (demografis_Log?.NIK != null)
                    {
                        //idDemo = await InsertDemografiLogReturnId(demografis_Log);
                        if (demografis.Id != 0)
                        {
                            _ = UpdateDemografiTemp(demografis);
                        }
                        else
                        {
                            if (demografis.CIF == "null" || demografis.CIF == null || string.IsNullOrWhiteSpace(demografis.CIF))
                            {
                                demografis.CIF = null;
                            }
                            idDemo = await InsertDemografiTemp(demografis);
                        }

                    }
                    else
                    {
                        if (demografis.CIF == "null" || demografis.CIF == null || string.IsNullOrWhiteSpace(demografis.CIF))
                        {
                            demografis.CIF = null;
                        }
                        idDemo = await InsertDemografiTemp(demografis);
                    }

                    if (photo_Log?.Nik != null)
                    {
                        //_ = InsertPhotoLog(photo_Log);
                        if (exPhoto != null) _ = DeletePhotoTemp(exPhoto);
                    }
                    photo.DemografisTempId = idDemo;
                    if (photo?.Nik != null) _ = InsertPhotoTemp(photo);

                    if (signature_Log?.Nik != null)
                    {
                        _ = InsertSignatureLog(signature_Log);
                        if (exSignature != null) _ = DeleteSignatureTemp(exSignature);
                    }
                    signature.DemografisTempId = idDemo;
                    if (signature?.Nik != null) _ = InsertSignatureTemp(signature);

                    if (photoCam_Log?.Nik != null)
                    {
                        //_ = InsertPhotoCamLog(photoCam_Log);
                        if (exPhotoCam != null) _ = DeletePhotoCamTemp(exPhotoCam);
                    }
                    photoCam.DemografisTempId = idDemo;
                    if (photoCam?.Nik != null) _ = InsertPhotoCamTemp(photoCam);

                    //if (finger_Log != null)
                    //{
                    //    foreach (var f in finger_Log)
                    //    {
                    //if (f.Nik != null) _ = InsertFingerLog(f);
                    //    }
                    //    foreach (var e in exFinger)
                    //    {
                    //        if (e.Nik != null) _ = DeleteFingerTemp(e);
                    //    }
                    //}

                    if (exFinger != null)
                    {
                        foreach (var e in exFinger)
                        {
                            if (e.Nik != null) _ = DeleteFingerTemp(e);
                        }
                    }
                    if (finger != null)
                    {
                        foreach (var f in finger)
                        {
                            f.DemografisTempId = idDemo;
                            if (f.Nik != null) _ = InsertFingerTemp(f);
                        }
                    }

                    if (exFingerEmployee != null)
                    {
                        foreach (var f in exFingerEmployee)
                        {
                            f.DemografisTempId = idDemo;
                            if (f.Nik != null) _ = DeleteFingerEmployeeTemp(f);
                        }
                    }

                    if (fingerEmployee != null)
                    {
                        foreach (var f in fingerEmployee)
                        {
                            f.DemografisTempId = idDemo;
                            if (f.Nik != null) _ = InsertFingerEmployeeTemp(f);
                        }
                    }

                    if (pegawai_KTP?.NIK != null)
                    {
                        if (pegawai_KTP.Id != 0)
                        {
                            _ = UpdateMappingKTP(pegawai_KTP);
                        }
                        else
                        {
                            _ = InsertMappingPegawaiKTP(pegawai_KTP);
                        }
                    }

                    inboxTemp.DemografisTempId = idDemo;
                    inboxTemp.NoPengajuan = demografis.NoPengajuan;

                    if (readerLog?.Uid != null)
                    {
                        _ = InsertReaderLog(readerLog);
                    }


                });


                const string queryInboxEnrollmentTemp = "Insert Into [Tbl_Inbox_Enrollment_Temp] (" +
                    "[DemografisTempId]," +
                    "[ApprovedByRoleId]," +
                    "[ApprovedByUnitId]," +
                    "[CreatedById]," +
                    "[CreatedByNpp]," +
                    "[CreatedTime]," +
                    "[CreatedByUnitId]," +
                    "[CreatedByUnitCode]," +
                    "[ApprovedStatus]," +
                    "[UpdatedTime]," +
                    "[UpdatedById]," +
                    "[UpdatedByNpp]," +
                    "[UpdatedByUnitId]," +
                    "[UpdatedByUnitCode]," +
                    "[NoPengajuan]," +
                    "[ApprovedByEmployeeId]," +
                    "[ApprovedByEmployeeId2]," +
                    "[Status]) " +
                    "OUTPUT INSERTED.[Id] " +
                "values(" +
                    "@DemografisTempId," +
                    "@ApprovedByRoleId," +
                    "@ApprovedByUnitId," +
                    "@CreatedById," +
                    "@CreatedByNpp," +
                    "@CreatedTime," +
                    "@CreatedByUnitId," +
                    "@CreatedByUnitCode," +
                    "@ApprovedStatus," +
                    "@UpdatedTime," +
                    "@UpdatedById," +
                    "@UpdatedByNpp," +
                    "@UpdatedByUnitId," +
                    "@UpdatedByUnitCode," +
                    "@NoPengajuan," +
                    "@ApprovedByEmployeeId," +
                    "@ApprovedByEmployeeId2," +
                    "@Status)";

                idInboxTemp = await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(queryInboxEnrollmentTemp, new
                {
                    inboxTemp.DemografisTempId,
                    inboxTemp.ApprovedByRoleId,
                    inboxTemp.ApprovedByUnitId,
                    inboxTemp.CreatedById,
                    inboxTemp.CreatedByNpp,
                    inboxTemp.CreatedTime,
                    inboxTemp.CreatedByUnitId,
                    inboxTemp.CreatedByUnitCode,
                    inboxTemp.ApprovedStatus,
                    inboxTemp.UpdatedTime,
                    inboxTemp.UpdatedById,
                    inboxTemp.UpdatedByNpp,
                    inboxTemp.UpdatedByUnitId,
                    inboxTemp.UpdatedByUnitCode,
                    inboxTemp.NoPengajuan,
                    inboxTemp.ApprovedByEmployeeId,
                    inboxTemp.ApprovedByEmployeeId2,
                    inboxTemp.Status
                }));

                inboxTempDetail.InboxEnrollmentTempId = idInboxTemp;
                inboxTempDetail.NoPengajuan = demografis.NoPengajuan;

                mappingFRNIK.inboxEnrollmentId = idInboxTemp;
                await InsertMappingNIKFR(mappingFRNIK);

                const string queryInboxEnrollmentTempDetail = "Insert Into [Tbl_Inbox_Enrollment_Temp_Detail] (" +
                    "[InboxEnrollmentTempId]," +
                    "[Notes]," +
                    "[SubmitById]," +
                    "[SubmitByNpp]," +
                    "[CreatedTime]," +
                    "[SubmitedByUnitId]," +
                    "[SubmitedByUnitCode]," +
                    "[ApprovedStatus]," +
                    "[NoPengajuan]," +
                    "[Status]) " +
                "values(" +
                    "@InboxEnrollmentTempId," +
                    "@Notes," +
                    "@SubmitById," +
                    "@SubmitByNpp," +
                    "@CreatedTime," +
                    "@SubmitedByUnitId," +
                    "@SubmitedByUnitCode," +
                    "@ApprovedStatus," +
                    "@NoPengajuan," +
                    "@Status)";

                await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(queryInboxEnrollmentTempDetail, new
                {
                    inboxTempDetail.InboxEnrollmentTempId,
                    inboxTempDetail.Notes,
                    inboxTempDetail.SubmitById,
                    inboxTempDetail.SubmitByNpp,
                    inboxTempDetail.CreatedTime,
                    inboxTempDetail.SubmitedByUnitId,
                    inboxTempDetail.SubmitedByUnitCode,
                    inboxTempDetail.ApprovedStatus,
                    inboxTempDetail.NoPengajuan,
                    inboxTempDetail.Status
                }));

                return (true, "");
            }
            catch (Exception Ex)
            {

                return (false, Ex.Message);
            }
        }
        public async Task<(bool status, string msg)> InsertEnrollFlowNoMatchingIKD(Tbl_Enrollment_IKD mappingIKD , Tbl_DataKTP_Demografis_Temp demografis, Tbl_DataKTP_Demografis_Log demografis_Log,
            Tbl_DataKTP_Photo_Temp photo,
            Tbl_DataKTP_Photo_Log photo_Log, Tbl_DataKTP_Signature_Temp signature,
            Tbl_DataKTP_Signature_Log signature_Log, Tbl_DataKTP_PhotoCam_Temp photoCam,
            Tbl_DataKTP_PhotoCam_Log photoCam_Log, List<Tbl_DataKTP_Finger_Temp> finger, List<Tbl_DataKTP_Finger_Log> finger_Log,
            Tbl_DataKTP_Photo_Temp exPhoto, Tbl_DataKTP_Signature_Temp exSignature, Tbl_DataKTP_PhotoCam_Temp exPhotoCam,
            List<Tbl_DataKTP_Finger_Temp> exFinger, Tbl_Mapping_Pegawai_KTP pegawai_KTP,
            Tbl_MasterAlatReaderLog readerLog, List<Tbl_DataKTP_Finger_Employee_Temp> fingerEmployee, List<Tbl_DataKTP_Finger_Employee_Temp> exFingerEmployee,
            Tbl_Inbox_Enrollment_Temp inboxTemp, Tbl_Inbox_Enrollment_Temp_Detail inboxTempDetail)
        {

            try
            {
                var idDemo = 0;
                var idInboxTemp = 0;



                await Db.WithTransaction(async c =>
                {
                    if (demografis_Log?.NIK != null)
                    {
                        //idDemo = await InsertDemografiLogReturnId(demografis_Log);
                        if (demografis.Id != 0)
                        {
                            _ = UpdateDemografiTemp(demografis);
                        }
                        else
                        {
                            if (demografis.CIF == "null" || demografis.CIF == null || string.IsNullOrWhiteSpace(demografis.CIF))
                            {
                                demografis.CIF = null;
                            }
                            idDemo = await InsertDemografiTemp(demografis);
                            await InsertMappingNIKIKD(mappingIKD);
                        }

                    }
                    else
                    {
                        if (demografis.CIF == "null" || demografis.CIF == null || string.IsNullOrWhiteSpace(demografis.CIF))
                        {
                            demografis.CIF = null;
                        }
                        idDemo = await InsertDemografiTemp(demografis);
                        await InsertMappingNIKIKD(mappingIKD);
                    }

                    if (photo_Log?.Nik != null)
                    {
                        //_ = InsertPhotoLog(photo_Log);
                        if (exPhoto != null) _ = DeletePhotoTemp(exPhoto);
                    }
                    photo.DemografisTempId = idDemo;
                    if (photo?.Nik != null) _ = InsertPhotoTemp(photo);

                    if (signature_Log?.Nik != null)
                    {
                        _ = InsertSignatureLog(signature_Log);
                        if (exSignature != null) _ = DeleteSignatureTemp(exSignature);
                    }
                    signature.DemografisTempId = idDemo;
                    if (signature?.Nik != null) _ = InsertSignatureTemp(signature);

                    if (photoCam_Log?.Nik != null)
                    {
                        //_ = InsertPhotoCamLog(photoCam_Log);
                        if (exPhotoCam != null) _ = DeletePhotoCamTemp(exPhotoCam);
                    }
                    photoCam.DemografisTempId = idDemo;
                    if (photoCam?.Nik != null) _ = InsertPhotoCamTemp(photoCam);

                    //if (finger_Log != null)
                    //{
                    //    foreach (var f in finger_Log)
                    //    {
                    //if (f.Nik != null) _ = InsertFingerLog(f);
                    //    }
                    //    foreach (var e in exFinger)
                    //    {
                    //        if (e.Nik != null) _ = DeleteFingerTemp(e);
                    //    }
                    //}

                    if (exFinger != null)
                    {
                        foreach (var e in exFinger)
                        {
                            if (e.Nik != null) _ = DeleteFingerTemp(e);
                        }
                    }
                    if (finger != null)
                    {
                        foreach (var f in finger)
                        {
                            f.DemografisTempId = idDemo;
                            if (f.Nik != null) _ = InsertFingerTemp(f);
                        }
                    }

                    if (exFingerEmployee != null)
                    {
                        foreach (var f in exFingerEmployee)
                        {
                            f.DemografisTempId = idDemo;
                            if (f.Nik != null) _ = DeleteFingerEmployeeTemp(f);
                        }
                    }

                    if (fingerEmployee != null)
                    {
                        foreach (var f in fingerEmployee)
                        {
                            f.DemografisTempId = idDemo;
                            if (f.Nik != null) _ = InsertFingerEmployeeTemp(f);
                        }
                    }

                    if (pegawai_KTP?.NIK != null)
                    {
                        if (pegawai_KTP.Id != 0)
                        {
                            _ = UpdateMappingKTP(pegawai_KTP);
                        }
                        else
                        {
                            _ = InsertMappingPegawaiKTP(pegawai_KTP);
                        }
                    }

                    inboxTemp.DemografisTempId = idDemo;
                    inboxTemp.NoPengajuan = demografis.NoPengajuan;

                    if (readerLog?.Uid != null)
                    {
                        _ = InsertReaderLog(readerLog);
                    }


                });


                const string queryInboxEnrollmentTemp = "Insert Into [Tbl_Inbox_Enrollment_Temp] (" +
                    "[DemografisTempId]," +
                    "[ApprovedByRoleId]," +
                    "[ApprovedByUnitId]," +
                    "[CreatedById]," +
                    "[CreatedByNpp]," +
                    "[CreatedTime]," +
                    "[CreatedByUnitId]," +
                    "[CreatedByUnitCode]," +
                    "[ApprovedStatus]," +
                    "[UpdatedTime]," +
                    "[UpdatedById]," +
                    "[UpdatedByNpp]," +
                    "[UpdatedByUnitId]," +
                    "[UpdatedByUnitCode]," +
                    "[NoPengajuan]," +
                    "[ApprovedByEmployeeId]," +
                    "[ApprovedByEmployeeId2]," +
                    "[Status]) " +
                    "OUTPUT INSERTED.[Id] " +
                "values(" +
                    "@DemografisTempId," +
                    "@ApprovedByRoleId," +
                    "@ApprovedByUnitId," +
                    "@CreatedById," +
                    "@CreatedByNpp," +
                    "@CreatedTime," +
                    "@CreatedByUnitId," +
                    "@CreatedByUnitCode," +
                    "@ApprovedStatus," +
                    "@UpdatedTime," +
                    "@UpdatedById," +
                    "@UpdatedByNpp," +
                    "@UpdatedByUnitId," +
                    "@UpdatedByUnitCode," +
                    "@NoPengajuan," +
                    "@ApprovedByEmployeeId," +
                    "@ApprovedByEmployeeId2," +
                    "@Status)";

                idInboxTemp = await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(queryInboxEnrollmentTemp, new
                {
                    inboxTemp.DemografisTempId,
                    inboxTemp.ApprovedByRoleId,
                    inboxTemp.ApprovedByUnitId,
                    inboxTemp.CreatedById,
                    inboxTemp.CreatedByNpp,
                    inboxTemp.CreatedTime,
                    inboxTemp.CreatedByUnitId,
                    inboxTemp.CreatedByUnitCode,
                    inboxTemp.ApprovedStatus,
                    inboxTemp.UpdatedTime,
                    inboxTemp.UpdatedById,
                    inboxTemp.UpdatedByNpp,
                    inboxTemp.UpdatedByUnitId,
                    inboxTemp.UpdatedByUnitCode,
                    inboxTemp.NoPengajuan,
                    inboxTemp.ApprovedByEmployeeId,
                    inboxTemp.ApprovedByEmployeeId2,
                    inboxTemp.Status
                }));

                inboxTempDetail.InboxEnrollmentTempId = idInboxTemp;
                inboxTempDetail.NoPengajuan = demografis.NoPengajuan;


                const string queryInboxEnrollmentTempDetail = "Insert Into [Tbl_Inbox_Enrollment_Temp_Detail] (" +
                    "[InboxEnrollmentTempId]," +
                    "[Notes]," +
                    "[SubmitById]," +
                    "[SubmitByNpp]," +
                    "[CreatedTime]," +
                    "[SubmitedByUnitId]," +
                    "[SubmitedByUnitCode]," +
                    "[ApprovedStatus]," +
                    "[NoPengajuan]," +
                    "[Status]) " +
                "values(" +
                    "@InboxEnrollmentTempId," +
                    "@Notes," +
                    "@SubmitById," +
                    "@SubmitByNpp," +
                    "@CreatedTime," +
                    "@SubmitedByUnitId," +
                    "@SubmitedByUnitCode," +
                    "@ApprovedStatus," +
                    "@NoPengajuan," +
                    "@Status)";

                await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(queryInboxEnrollmentTempDetail, new
                {
                    inboxTempDetail.InboxEnrollmentTempId,
                    inboxTempDetail.Notes,
                    inboxTempDetail.SubmitById,
                    inboxTempDetail.SubmitByNpp,
                    inboxTempDetail.CreatedTime,
                    inboxTempDetail.SubmitedByUnitId,
                    inboxTempDetail.SubmitedByUnitCode,
                    inboxTempDetail.ApprovedStatus,
                    inboxTempDetail.NoPengajuan,
                    inboxTempDetail.Status
                }));

                return (true, "");
            }
            catch (Exception Ex)
            {

                return (false, Ex.Message);
            }



        }


        public async Task<(bool status, string msg)> InsertEnrollFlowIKD(Tbl_DataKTP_Demografis_Temp demografis, Tbl_DataKTP_Demografis_Log demografis_Log,
            Tbl_DataKTP_Photo_Temp photo,
            Tbl_DataKTP_Photo_Log photo_Log, Tbl_DataKTP_Signature_Temp signature,
            Tbl_DataKTP_Signature_Log signature_Log, Tbl_DataKTP_PhotoCam_Temp photoCam,
            Tbl_DataKTP_PhotoCam_Log photoCam_Log, List<Tbl_DataKTP_Finger_Temp> finger, List<Tbl_DataKTP_Finger_Log> finger_Log,
            Tbl_DataKTP_Photo_Temp exPhoto, Tbl_DataKTP_Signature_Temp exSignature, Tbl_DataKTP_PhotoCam_Temp exPhotoCam,
            List<Tbl_DataKTP_Finger_Temp> exFinger, Tbl_Mapping_Pegawai_KTP pegawai_KTP,
            Tbl_MasterAlatReaderLog readerLog, List<Tbl_DataKTP_Finger_Employee_Temp> fingerEmployee, List<Tbl_DataKTP_Finger_Employee_Temp> exFingerEmployee,
            Tbl_Inbox_Enrollment_Temp inboxTemp, Tbl_Inbox_Enrollment_Temp_Detail inboxTempDetail, Tbl_Enrollment_IKD MappingIKD)
        {

            try
            {
                var idDemo = 0;
                var idInboxTemp = 0;
                var idMapping = 0;



                await Db.WithTransaction(async c =>
                {
                    if (demografis_Log?.NIK != null)
                    {
                        //idDemo = await InsertDemografiLogReturnId(demografis_Log);
                        if (demografis.Id != 0)
                        {
                            _ = UpdateDemografiTemp(demografis);
                        }
                        else
                        {
                            if (demografis.CIF == "null" || demografis.CIF == null || string.IsNullOrWhiteSpace(demografis.CIF))
                            {
                                demografis.CIF = null;
                            }
                            idDemo = await InsertDemografiTemp(demografis);
                            idMapping = await InsertMappingIKD(MappingIKD);
                        }

                    }
                    else
                    {
                        if (demografis.CIF == "null" || demografis.CIF == null || string.IsNullOrWhiteSpace(demografis.CIF))
                        {
                            demografis.CIF = null;
                        }
                        idDemo = await InsertDemografiTemp(demografis);
                        idMapping = await InsertMappingIKD(MappingIKD);

                    }

                    if (photo_Log?.Nik != null)
                    {
                        //_ = InsertPhotoLog(photo_Log);
                        if (exPhoto != null) _ = DeletePhotoTemp(exPhoto);
                    }
                    photo.DemografisTempId = idDemo;
                    if (photo?.Nik != null) _ = InsertPhotoTemp(photo);

                    if (signature_Log?.Nik != null)
                    {
                        _ = InsertSignatureLog(signature_Log);
                        if (exSignature != null) _ = DeleteSignatureTemp(exSignature);
                    }
                    signature.DemografisTempId = idDemo;
                    if (signature?.Nik != null) _ = InsertSignatureTemp(signature);

                    if (photoCam_Log?.Nik != null)
                    {
                        //_ = InsertPhotoCamLog(photoCam_Log);
                        if (exPhotoCam != null) _ = DeletePhotoCamTemp(exPhotoCam);
                    }
                    photoCam.DemografisTempId = idDemo;
                    if (photoCam?.Nik != null) _ = InsertPhotoCamTemp(photoCam);

                    //if (finger_Log != null)
                    //{
                    //    foreach (var f in finger_Log)
                    //    {
                    //if (f.Nik != null) _ = InsertFingerLog(f);
                    //    }
                    //    foreach (var e in exFinger)
                    //    {
                    //        if (e.Nik != null) _ = DeleteFingerTemp(e);
                    //    }
                    //}

                    if (exFinger != null)
                    {
                        foreach (var e in exFinger)
                        {
                            if (e.Nik != null) _ = DeleteFingerTemp(e);
                        }
                    }
                    if (finger != null)
                    {
                        foreach (var f in finger)
                        {
                            f.DemografisTempId = idDemo;
                            if (f.Nik != null) _ = InsertFingerTemp(f);
                        }
                    }

                    if (exFingerEmployee != null)
                    {
                        foreach (var f in exFingerEmployee)
                        {
                            f.DemografisTempId = idDemo;
                            if (f.Nik != null) _ = DeleteFingerEmployeeTemp(f);
                        }
                    }

                    if (fingerEmployee != null)
                    {
                        foreach (var f in fingerEmployee)
                        {
                            f.DemografisTempId = idDemo;
                            if (f.Nik != null) _ = InsertFingerEmployeeTemp(f);
                        }
                    }

                    if (pegawai_KTP?.NIK != null)
                    {
                        if (pegawai_KTP.Id != 0)
                        {
                            _ = UpdateMappingKTP(pegawai_KTP);
                        }
                        else
                        {
                            _ = InsertMappingPegawaiKTP(pegawai_KTP);
                        }
                    }

                    inboxTemp.DemografisTempId = idDemo;
                    inboxTemp.NoPengajuan = demografis.NoPengajuan;

                    if (readerLog?.Uid != null)
                    {
                        _ = InsertReaderLog(readerLog);
                    }


                });


                const string queryInboxEnrollmentTemp = "Insert Into [Tbl_Inbox_Enrollment_Temp] (" +
                    "[DemografisTempId]," +
                    "[ApprovedByRoleId]," +
                    "[ApprovedByUnitId]," +
                    "[CreatedById]," +
                    "[CreatedByNpp]," +
                    "[CreatedTime]," +
                    "[CreatedByUnitId]," +
                    "[CreatedByUnitCode]," +
                    "[ApprovedStatus]," +
                    "[UpdatedTime]," +
                    "[UpdatedById]," +
                    "[UpdatedByNpp]," +
                    "[UpdatedByUnitId]," +
                    "[UpdatedByUnitCode]," +
                    "[NoPengajuan]," +
                    "[ApprovedByEmployeeId]," +
                    "[ApprovedByEmployeeId2]," +
                    "[Status]) " +
                    "OUTPUT INSERTED.[Id] " +
                "values(" +
                    "@DemografisTempId," +
                    "@ApprovedByRoleId," +
                    "@ApprovedByUnitId," +
                    "@CreatedById," +
                    "@CreatedByNpp," +
                    "@CreatedTime," +
                    "@CreatedByUnitId," +
                    "@CreatedByUnitCode," +
                    "@ApprovedStatus," +
                    "@UpdatedTime," +
                    "@UpdatedById," +
                    "@UpdatedByNpp," +
                    "@UpdatedByUnitId," +
                    "@UpdatedByUnitCode," +
                    "@NoPengajuan," +
                    "@ApprovedByEmployeeId," +
                    "@ApprovedByEmployeeId2," +
                    "@Status)";

                idInboxTemp = await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(queryInboxEnrollmentTemp, new
                {
                    inboxTemp.DemografisTempId,
                    inboxTemp.ApprovedByRoleId,
                    inboxTemp.ApprovedByUnitId,
                    inboxTemp.CreatedById,
                    inboxTemp.CreatedByNpp,
                    inboxTemp.CreatedTime,
                    inboxTemp.CreatedByUnitId,
                    inboxTemp.CreatedByUnitCode,
                    inboxTemp.ApprovedStatus,
                    inboxTemp.UpdatedTime,
                    inboxTemp.UpdatedById,
                    inboxTemp.UpdatedByNpp,
                    inboxTemp.UpdatedByUnitId,
                    inboxTemp.UpdatedByUnitCode,
                    inboxTemp.NoPengajuan,
                    inboxTemp.ApprovedByEmployeeId,
                    inboxTemp.ApprovedByEmployeeId2,
                    inboxTemp.Status
                }));

                inboxTempDetail.InboxEnrollmentTempId = idInboxTemp;
                inboxTempDetail.NoPengajuan = demografis.NoPengajuan;


                const string queryInboxEnrollmentTempDetail = "Insert Into [Tbl_Inbox_Enrollment_Temp_Detail] (" +
                    "[InboxEnrollmentTempId]," +
                    "[Notes]," +
                    "[SubmitById]," +
                    "[SubmitByNpp]," +
                    "[CreatedTime]," +
                    "[SubmitedByUnitId]," +
                    "[SubmitedByUnitCode]," +
                    "[ApprovedStatus]," +
                    "[NoPengajuan]," +
                    "[Status]) " +
                "values(" +
                    "@InboxEnrollmentTempId," +
                    "@Notes," +
                    "@SubmitById," +
                    "@SubmitByNpp," +
                    "@CreatedTime," +
                    "@SubmitedByUnitId," +
                    "@SubmitedByUnitCode," +
                    "@ApprovedStatus," +
                    "@NoPengajuan," +
                    "@Status)";

                await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(queryInboxEnrollmentTempDetail, new
                {
                    inboxTempDetail.InboxEnrollmentTempId,
                    inboxTempDetail.Notes,
                    inboxTempDetail.SubmitById,
                    inboxTempDetail.SubmitByNpp,
                    inboxTempDetail.CreatedTime,
                    inboxTempDetail.SubmitedByUnitId,
                    inboxTempDetail.SubmitedByUnitCode,
                    inboxTempDetail.ApprovedStatus,
                    inboxTempDetail.NoPengajuan,
                    inboxTempDetail.Status
                }));

                return (true, "");
            }
            catch (Exception Ex)
            {

                return (false, Ex.Message);
            }



        }






        public async Task<(bool status, string msg)> InsertEnrollFlow2(Tbl_DataKTP_Demografis demografis, Tbl_DataKTP_Demografis_Log demografis_Log,
            Tbl_DataKTP_Photo photo,
            Tbl_DataKTP_Photo_Log photo_Log, Tbl_DataKTP_Signature signature,
            Tbl_DataKTP_Signature_Log signature_Log, Tbl_DataKTP_PhotoCam photoCam,
            Tbl_DataKTP_PhotoCam_Log photoCam_Log, List<Tbl_DataKTP_Finger> finger, List<Tbl_DataKTP_Finger_Employee> finger_Employee,
            List<Tbl_DataKTP_Finger_Log> finger_Log, List<Tbl_DataKTP_Finger_Employee_Log> finger_Employee_Log,
            Tbl_DataKTP_Photo exPhoto, Tbl_DataKTP_Signature exSignature, Tbl_DataKTP_PhotoCam exPhotoCam,
            List<Tbl_DataKTP_Finger> exFinger, List<Tbl_DataKTP_Finger_Employee> exFingerEmployee, Tbl_MasterAlatReaderLog readerLog, Tbl_Mapping_Pegawai_KTP dataMapping)
        {
            try
            {
                Db.WithTransaction(c =>
                {
                    if (demografis_Log?.NIK != null)
                    {
                        _ = InsertDemografiLog(demografis_Log);
                        _ = UpdateDemografi(demografis);
                    }
                    if (photo_Log?.Nik != null)
                    {
                        _ = InsertPhotoLog(photo_Log);
                        _ = DeletePhoto(exPhoto);
                    }
                    if (photo?.Nik != null) _ = InsertPhoto(photo);
                    if (signature_Log?.Nik != null)
                    {
                        _ = InsertSignatureLog(signature_Log);
                        _ = DeleteSignature(signature);
                    }
                    if (signature?.Nik != null) _ = InsertSignature(signature);
                    if (photoCam_Log?.Nik != null)
                    {
                        _ = InsertPhotoCamLog(photoCam_Log);
                        _ = DeletePhotoCam(photoCam);
                    }
                    if (photoCam?.Nik != null) _ = InsertPhotoCam(photoCam);
                    if (finger_Log.Count != 0)
                    {
                        foreach (var f in finger_Log)
                        {
                            if (f.Nik != null) _ = InsertFingerLog(f);
                        }
                        foreach (var e in exFinger)
                        {
                            if (e.Nik != null) _ = DeleteFinger(e);
                        }
                    }
                    if (finger.Count != 0)
                    {
                        foreach (var f in finger)
                        {
                            if (f.Nik != null) _ = InsertFinger(f);
                        }
                    }
                    if (finger_Employee_Log.Count != 0)
                    {
                        foreach (var f in finger_Employee_Log)
                        {
                            if (f.Nik != null) _ = InsertFingerEmployeeLog(f);
                        }
                        foreach (var e in exFingerEmployee)
                        {
                            if (e.Nik != null) _ = DeleteFingerEmployee(e);
                        }
                    }
                    if (finger_Employee.Count != 0)
                    {
                        foreach (var f in finger_Employee)
                        {
                            if (f.Nik != null) _ = InsertFingerEmployee(f);
                        }
                    }
                    if (dataMapping.NIK != null)
                    {
                        if (dataMapping.Id != 0)
                        {
                            _ = UpdateMappingKTP(dataMapping);
                        }
                        else
                        {
                            _ = InsertMappingPegawaiKTP(dataMapping);
                        }
                    }
                });

                //Db.WithTransaction(c =>
                //{
                //    if (demografis_Log?.Id != 0)
                //    {
                //        _ = c.Insert(demografis_Log, true);
                //        c.Update(demografis);
                //    }
                //    else
                //    {
                //        if (demografis.CIF == "null" || demografis.CIF == null || string.IsNullOrWhiteSpace(demografis.CIF))
                //        {
                //            demografis.CIF = null;
                //        }
                //        _ = c.Insert(demografis, true);
                //    }
                //    if (photo_Log?.Id != 0)
                //    {
                //        _ = c.Insert(photo_Log, true);
                //        c.Delete(exPhoto);
                //    }
                //    if (photo?.Nik != null) _ = c.Insert(photo, true);
                //    if (signature_Log?.Id != 0)
                //    {
                //        _ = c.Insert(signature_Log, true);
                //        c.Delete(exSignature);
                //    }
                //    if (signature?.Nik != null) _ = c.Insert(signature, true);
                //    if (photoCam_Log?.Nik != null)
                //    {
                //        _ = c.Insert(photoCam_Log, true);
                //        c.Delete(exPhotoCam);
                //    }
                //    if (photoCam?.Nik != null) _ = c.Insert(photoCam, true);
                //    if (finger_Log != null)
                //    {
                //        foreach (var f in finger_Log)
                //        {
                //            if (f.Nik != null) _ = c.Insert(f, true);
                //        }
                //        foreach (var e in exFinger)
                //        {
                //            if (e.Nik != null) c.Delete(e);
                //        }
                //    }
                //    if (finger != null)
                //    {
                //        foreach (var f in finger)
                //        {
                //            if (f.Nik != null) _ = c.Insert(f, true);
                //        }
                //    }
                //    if (finger_Employee_Log != null)
                //    {
                //        foreach (var f in finger_Employee_Log)
                //        {
                //            if (f.Nik != null) _ = c.Insert(f, true);
                //        }
                //        foreach (var e in exFingerEmployee)
                //        {
                //            if (e.Nik != null) c.Delete(e);
                //        }
                //    }
                //    if (finger_Employee != null)
                //    {
                //        foreach (var f in finger_Employee)
                //        {
                //            if (f.Nik != null) _ = c.Insert(f, true);
                //        }
                //    }
                //    if (dataMapping?.NIK != null)
                //    {
                //        c.Insert(dataMapping, true);
                //    }
                //});

                return (true, "");
            }
            catch (Exception e)
            {
                return (false, e.Message);
            }
        }
        public void UpdatesEnrollFlow(Tbl_DataKTP_Demografis demografis, Tbl_DataKTP_Demografis_Log demografis_Log,
            Tbl_DataKTP_Photo photo,
            Tbl_DataKTP_Photo_Log photo_Log, Tbl_DataKTP_Signature signature,
            Tbl_DataKTP_Signature_Log signature_Log, Tbl_DataKTP_PhotoCam photoCam,
            Tbl_DataKTP_PhotoCam_Log photoCam_Log, Tbl_DataKTP_Finger finger_kanan, Tbl_DataKTP_Finger finger_kiri,
            Tbl_DataKTP_Finger_Log finger_kanan_log, Tbl_DataKTP_Finger_Log finger_kiri_log,
            Tbl_DataKTP_Finger_Employee fingerEmployee_kanan, Tbl_DataKTP_Finger_Employee fingerEmployee_kiri,
            Tbl_DataKTP_Finger_Employee_Log fingerEmployee_kanan_Log, Tbl_DataKTP_Finger_Employee_Log fingerEmployee_kiri_Log,
            Tbl_MasterAlatReaderLogActvity LogActivity, Tbl_MasterAlatReaderLog ReaderLog)
        {
            Db.WithTransaction(c =>
            {
                if (demografis?.Id != 0)
                {
                    _ = c.Insert(demografis_Log, true);
                    c.Update(demografis);
                }
                if (photo?.Id != 0)
                {
                    _ = c.Insert(photo_Log, true);
                    c.Update(photo);
                }
                if (signature?.Id != 0)
                {
                    _ = c.Insert(signature_Log, true);
                    c.Update(signature);
                }
                if (photoCam?.Id != 0)
                {
                    _ = c.Insert(photoCam_Log, true);
                    c.Update(photoCam);
                }
                if (finger_kanan?.Id != 0)
                {
                    _ = c.Insert(finger_kanan_log, true);
                    c.Update(finger_kanan);
                }
                if (finger_kiri?.Id != 0)
                {
                    _ = c.Insert(finger_kiri_log, true);
                    c.Update(finger_kiri);
                }
                if (fingerEmployee_kanan?.Id != 0)
                {
                    _ = c.Insert(fingerEmployee_kanan_Log, true);
                    c.Update(fingerEmployee_kanan);
                }
                if (fingerEmployee_kiri?.Id != 0)
                {
                    _ = c.Insert(fingerEmployee_kiri_Log, true);
                    c.Update(fingerEmployee_kiri);
                }
                //if (ReaderLog?.Uid != null)
                //{
                //    //_ = c.Insert(ReaderLog, true);
                //    _ = c.Insert(LogActivity, true);
                //}
            });
        }

        public void UpdatesPhotoCam(Tbl_DataKTP_PhotoCam photoCam, Tbl_DataKTP_PhotoCam_Log photoCam_Log,
            Tbl_MasterAlatReaderLogActvity LogActivity, Tbl_MasterAlatReaderLog ReaderLog)
        {
            Db.WithTransaction(c =>
            {
                if (photoCam?.Id != 0)
                {
                    _ = c.Insert(photoCam_Log, true);
                    c.Update(photoCam);
                }
                //if (ReaderLog != null)
                //{
                //    c.Insert(ReaderLog, true);
                //}
                //if (LogActivity != null)
                //{
                //    c.Insert(LogActivity, true);
                //}
            });
        }

        public long InsertKtpFingerLog(Tbl_DataKTP_Finger_Log log)
        {
            return InsertIncrement(log);
        }
        public long InsertKtpEmployeeFingerLog(Tbl_DataKTP_Finger_Employee_Log log)
        {
            return InsertIncrement(log);
        }

        public async Task<string> MigrateDataEnroll(string apikey, bool isProxy, string ipProxy)
        {
            try
            {
                const string proc = "[sp_getDataMigrasi]";

                var dataToMigrate = await _baseConnection.WithConnectionAsync(c => c.QueryAsync<MigrateEnrollment>(proc, null, commandType: CommandType.StoredProcedure));

                foreach (var data in dataToMigrate)
                {
                    if (!string.IsNullOrEmpty(data.PathDownloadFotoKTP) && !string.IsNullOrEmpty(data.PathDownloadFotoTTD) && !string.IsNullOrEmpty(data.FingerKiri) && !string.IsNullOrEmpty(data.FingerKanan))
                    {
                        var idPegawai = await GetPegawaiByNpp(data.CreatedBy_Npp);

                        var IdUnit = await GetUnitByCode(data.Kode_Unit);
                        var LatLongRes = await ConvertLatLong(apikey, data.Alamat_Lengkap, isProxy, ipProxy);
                        string latitude = null;
                        string longitude = null;
                        string AlamatGoogle = null;
                        string kodepos = "";

                        if (LatLongRes.results.Count != 0)
                        {
                            latitude = LatLongRes.results[0].geometry.location.lat;
                            longitude = LatLongRes.results[0].geometry.location.lng;
                            AlamatGoogle = LatLongRes.results[0].formatted_address;

                            try
                            {
                                if (LatLongRes.results[0].address_components[7] != null)
                                {
                                    if (LatLongRes.results[0].address_components[7].long_name != null)
                                    {
                                        kodepos = LatLongRes.results[0].address_components[7].long_name;
                                    }
                                }
                            }
                            catch (Exception)
                            {
                                kodepos = "";
                            }
                        }

                        #region check employee or not
                        bool isEmployee = false;
                        var _empData = await IsEmployee(data.NIK);
                        if (_empData != null)
                        {
                            isEmployee = true;
                        }
                        #endregion

                        var dataNpp = new Tbl_Mapping_Pegawai_KTP();

                        //string tgl_lahir = data.Tanggal_Lahir.ToString();
                        //if (tgl_lahir.Contains("/"))
                        //{
                        //    tgl_lahir = tgl_lahir.Replace(".", "");

                        //    tgl_lahir = tgl_lahir.Replace("/", "-");

                        //    data.Tanggal_Lahir = DateTime.ParseExact(tgl_lahir, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                        //}

                        var dataDemografis = new Tbl_DataKTP_Demografis
                        {
                            Agama = data.Agama,
                            Alamat = data.Alamat,
                            AlamatLengkap = data.Alamat_Lengkap,
                            CIF = data.CIF,
                            CreatedByNpp = data.CreatedBy_Npp,
                            CreatedTime = data.Created_Time,
                            Desa = data.Desa,
                            GolonganDarah = data.Golongan_Darah,
                            IsActive = true,
                            JenisKelamin = data.Jenis_Kelamin,
                            Kecamatan = data.Kecamatan,
                            Kelurahan = data.Kelurahan,
                            Kewarganegaraan = data.Kewarganegaraan,
                            Kota = data.Kota,
                            MasaBerlaku = data.Masa_Berlaku,
                            Nama = data.Nama,
                            NIK = data.NIK,
                            Pekerjaan = data.Pekerjaan,
                            Provinsi = data.Provinsi,
                            RT = data.RT,
                            RW = data.RW,
                            IsDeleted = false,
                            StatusPerkawinan = data.Status_Perkawinan,
                            TanggalLahir = data.Tanggal_Lahir,
                            TempatLahir = data.Tempat_Lahir,
                            CreatedByUID = data.CreatedBy_SerialNumber,
                            CreatedById = idPegawai,
                            isMigrate = true,
                            CreatedByUnitId = IdUnit,
                            CreatedByUnitCode = data.Kode_Unit,
                            AlamatGoogle = AlamatGoogle,
                            KodePos = kodepos,
                            Latitude = latitude,
                            Longitude = longitude
                        };

                        if (isEmployee)
                        {
                            dataNpp = new Tbl_Mapping_Pegawai_KTP
                            {
                                CreatedById = idPegawai,
                                NIK = data.NIK,
                                Npp = _empData.Npp,
                                CreatedByNpp = _empData.Npp,
                                CreatedByUID = data.CreatedBy_SerialNumber,
                                CreatedTime = DateTime.Now,
                                CreatedByUnit = data.Kode_Unit
                            };
                        }

                        string JamServer = DateTime.Now.ToString("ddMMyyyyHHmmss");

                        var sysPathFolder = await _sysParameterRepository.GetPathFolder("PathFolderData");
                        string pathFolder = sysPathFolder.Value;

                        using WebClient webClient = new();

                        var ktpPhoto = new Tbl_DataKTP_Photo();

                        if (!string.IsNullOrEmpty(data.PathDownloadFotoKTP))
                        {
                            var systemParameterPath = await _sysParameterRepository.GetPathFolder("FolderPhoto")
                                                            .ConfigureAwait(false);
                            string pathFolderFoto = systemParameterPath.Value;

                            try
                            {
                                byte[] fileKtp = webClient.DownloadData(data.PathDownloadFotoKTP);

                                string subPathFolderPhotoKtp = pathFolder + "/" + pathFolderFoto + "/" + data.NIK + "/";

                                string fileNameKtp = "Foto_" + data.NIK + "_" + JamServer + ".jpg";

                                if (!Directory.Exists(subPathFolderPhotoKtp))
                                {
                                    Directory.CreateDirectory(subPathFolderPhotoKtp);
                                }

                                var filePathKtp = subPathFolderPhotoKtp + fileNameKtp;

                                File.WriteAllBytes(filePathKtp, fileKtp);

                                ktpPhoto = new Tbl_DataKTP_Photo
                                {
                                    CreatedByNpp = data.CreatedBy_Npp,
                                    FileName = fileNameKtp,
                                    Nik = data.NIK,
                                    PathFile = filePathKtp,
                                    CreatedTime = data.Created_Time,
                                    IsActive = true,
                                    CreatedById = idPegawai,
                                    CreatedByUnit = data.Kode_Unit,
                                    CreatedByUid = data.CreatedBy_SerialNumber,
                                    IsDeleted = false
                                };

                            }
                            catch (Exception ex)
                            {
                                ktpPhoto = new Tbl_DataKTP_Photo
                                {
                                    CreatedByNpp = data.CreatedBy_Npp,
                                    FileName = "Tidak Ditemukan",
                                    Nik = data.NIK,
                                    PathFile = "Tidak Ditemukan",
                                    CreatedTime = data.Created_Time,
                                    IsActive = true,
                                    CreatedById = idPegawai,
                                    CreatedByUnit = data.Kode_Unit,
                                    CreatedByUid = data.CreatedBy_SerialNumber,
                                    IsDeleted = false
                                };
                            }

                            //InsertIncrement(ktpPhoto);
                        }

                        var ktpSignature = new Tbl_DataKTP_Signature();

                        if (!string.IsNullOrEmpty(data.PathDownloadFotoTTD))
                        {
                            var systemParameterPath = await _sysParameterRepository.GetPathFolder("FolderSignature")
                                                            .ConfigureAwait(false);
                            string pathFolderFoto = systemParameterPath.Value;

                            try
                            {
                                byte[] file = webClient.DownloadData(data.PathDownloadFotoTTD);

                                string subPathFolderPhoto = pathFolder + "/" + pathFolderFoto + "/" + data.NIK + "/";

                                string fileName = "Signature_" + data.NIK + "_" + JamServer + ".jpg";

                                if (!Directory.Exists(subPathFolderPhoto))
                                {
                                    Directory.CreateDirectory(subPathFolderPhoto);
                                }

                                var filePath = subPathFolderPhoto + fileName;

                                File.WriteAllBytes(filePath, file);

                                ktpSignature = new Tbl_DataKTP_Signature
                                {
                                    CreatedByNpp = data.CreatedBy_Npp,
                                    CreatedTime = data.Created_Time,
                                    FileName = fileName,
                                    IsActive = true,
                                    Nik = data.NIK,
                                    PathFile = filePath,
                                    CreatedById = idPegawai,
                                    CreatedByUnit = data.Kode_Unit,
                                    CreatedByUid = data.CreatedBy_SerialNumber,
                                    IsDeleted = false
                                };
                            }
                            catch (Exception ex)
                            {
                                ktpSignature = new Tbl_DataKTP_Signature
                                {
                                    CreatedByNpp = data.CreatedBy_Npp,
                                    CreatedTime = data.Created_Time,
                                    FileName = "Tidak Ditemukan",
                                    IsActive = true,
                                    Nik = data.NIK,
                                    PathFile = "Tidak Ditemukan",
                                    CreatedById = idPegawai,
                                    CreatedByUnit = data.Kode_Unit,
                                    CreatedByUid = data.CreatedBy_SerialNumber,
                                    IsDeleted = false
                                };
                            }

                            //InsertIncrement(ktpSignature);
                        }

                        var ktpPhotoCam = new Tbl_DataKTP_PhotoCam();

                        if (!string.IsNullOrEmpty(data.PathDownloadFotoWebcam))
                        {
                            var systemParameterPath = await _sysParameterRepository.GetPathFolder("FolderWebcam")
                                                            .ConfigureAwait(false);
                            string pathFolderFoto = systemParameterPath.Value;

                            try
                            {
                                byte[] file = webClient.DownloadData(data.PathDownloadFotoWebcam);

                                string subPathFolderPhoto = pathFolder + "/" + pathFolderFoto + "/" + data.NIK + "/";

                                string fileName = "PhotoCam_" + data.NIK + "_" + JamServer + ".jpg";

                                if (!Directory.Exists(subPathFolderPhoto))
                                {
                                    Directory.CreateDirectory(subPathFolderPhoto);
                                }

                                var filePath = subPathFolderPhoto + fileName;

                                File.WriteAllBytes(filePath, file);

                                ktpPhotoCam = new Tbl_DataKTP_PhotoCam
                                {
                                    CreatedByNpp = data.CreatedBy_Npp,
                                    CreatedTime = data.Created_Time,
                                    FileName = fileName,
                                    IsActive = true,
                                    Nik = data.NIK,
                                    PathFile = filePath,
                                    CreatedById = idPegawai,
                                    CreatedByUnit = data.Kode_Unit,
                                    CreatedByUid = data.CreatedBy_SerialNumber,
                                    IsDeleted = false
                                };

                            }
                            catch (Exception ex)
                            {
                                ktpPhotoCam = new Tbl_DataKTP_PhotoCam
                                {
                                    CreatedByNpp = data.CreatedBy_Npp,
                                    CreatedTime = data.Created_Time,
                                    FileName = "Tidak Ditemukan",
                                    IsActive = true,
                                    Nik = data.NIK,
                                    PathFile = "Tidak Ditemukan",
                                    CreatedById = idPegawai,
                                    CreatedByUnit = data.Kode_Unit,
                                    CreatedByUid = data.CreatedBy_SerialNumber,
                                    IsDeleted = false
                                };
                            }

                            //InsertIncrement(ktpPhotoCam);
                        }

                        var fingerKanan = new Tbl_DataKTP_Finger();

                        var fingerKananEmployee = new Tbl_DataKTP_Finger_Employee();

                        if (!string.IsNullOrEmpty(data.FingerKanan))
                        {
                            var systemParameterPath = await _sysParameterRepository.GetPathFolder("FolderFinger")
                                                            .ConfigureAwait(false);
                            string pathFolderFoto = systemParameterPath.Value;

                            try
                            {
                                byte[] file = webClient.DownloadData(data.FingerKanan);

                                string subPathFolderPhoto = pathFolder + "/" + pathFolderFoto + "/" + data.NIK + "/";

                                string fileName = "PhotoFinger_JariTelunjukKanan_" + data.NIK + "_" + JamServer + ".jpg";

                                if (!Directory.Exists(subPathFolderPhoto))
                                {
                                    Directory.CreateDirectory(subPathFolderPhoto);
                                }

                                var filePath = subPathFolderPhoto + fileName;

                                File.WriteAllBytes(filePath, file);

                                fingerKanan = new Tbl_DataKTP_Finger
                                {
                                    CreatedByNpp = data.CreatedBy_Npp,
                                    CreatedTime = data.Created_Time,
                                    FileName = fileName,
                                    IsActive = true,
                                    Nik = data.NIK,
                                    PathFile = filePath,
                                    TypeFinger = data.TypeFingerKanan,
                                    CreatedById = idPegawai,
                                    CreatedByUnit = data.Kode_Unit,
                                    CreatedByUid = data.CreatedBy_SerialNumber,
                                    IsDeleted = false
                                };

                                if (isEmployee)
                                {
                                    fingerKananEmployee = new Tbl_DataKTP_Finger_Employee
                                    {
                                        CreatedByNpp = data.CreatedBy_Npp,
                                        CreatedTime = data.Created_Time,
                                        FileName = fileName,
                                        IsActive = true,
                                        Nik = data.NIK,
                                        PathFile = filePath,
                                        TypeFinger = data.TypeFingerKanan,
                                        CreatedById = idPegawai,
                                        CreatedByUnit = data.Kode_Unit,
                                        CreatedByUid = data.CreatedBy_SerialNumber,
                                        IsDeleted = false
                                    };
                                }
                            }
                            catch (Exception ex)
                            {
                                fingerKanan = new Tbl_DataKTP_Finger
                                {
                                    CreatedByNpp = data.CreatedBy_Npp,
                                    CreatedTime = data.Created_Time,
                                    FileName = "Tidak Ditemukan",
                                    IsActive = true,
                                    Nik = data.NIK,
                                    PathFile = "Tidak Ditemukan",
                                    TypeFinger = data.TypeFingerKanan,
                                    CreatedById = idPegawai,
                                    CreatedByUnit = data.Kode_Unit,
                                    CreatedByUid = data.CreatedBy_SerialNumber,
                                    IsDeleted = false
                                };

                                if (isEmployee)
                                {
                                    fingerKananEmployee = new Tbl_DataKTP_Finger_Employee
                                    {
                                        CreatedByNpp = data.CreatedBy_Npp,
                                        CreatedTime = data.Created_Time,
                                        FileName = "Tidak Ditemukan",
                                        IsActive = true,
                                        Nik = data.NIK,
                                        PathFile = "Tidak Ditemukan",
                                        TypeFinger = data.TypeFingerKanan,
                                        CreatedById = idPegawai,
                                        CreatedByUnit = data.Kode_Unit,
                                        CreatedByUid = data.CreatedBy_SerialNumber,
                                        IsDeleted = false
                                    };
                                }
                            }

                        }

                        var FingerKiri = new Tbl_DataKTP_Finger();
                        var FingerKiriEmployee = new Tbl_DataKTP_Finger_Employee();

                        if (!string.IsNullOrEmpty(data.FingerKiri))
                        {
                            var systemParameterPath = await _sysParameterRepository.GetPathFolder("FolderFinger")
                                                            .ConfigureAwait(false);
                            string pathFolderFoto = systemParameterPath.Value;

                            try
                            {
                                byte[] file = webClient.DownloadData(data.FingerKiri);

                                string subPathFolderPhoto = pathFolder + "/" + pathFolderFoto + "/" + data.NIK + "/";

                                string fileName = "PhotoFinger_JariTelunjukKiri_" + data.NIK + "_" + JamServer + ".jpg";

                                if (!Directory.Exists(subPathFolderPhoto))
                                {
                                    Directory.CreateDirectory(subPathFolderPhoto);
                                }

                                var filePath = subPathFolderPhoto + fileName;

                                File.WriteAllBytes(filePath, file);

                                FingerKiri = new Tbl_DataKTP_Finger
                                {
                                    CreatedByNpp = data.CreatedBy_Npp,
                                    CreatedTime = data.Created_Time,
                                    FileName = fileName,
                                    IsActive = true,
                                    Nik = data.NIK,
                                    PathFile = filePath,
                                    TypeFinger = data.TypeFingerKiri,
                                    CreatedById = idPegawai,
                                    CreatedByUnit = data.Kode_Unit,
                                    CreatedByUid = data.CreatedBy_SerialNumber,
                                    IsDeleted = false
                                };

                                if (isEmployee)
                                {
                                    FingerKiriEmployee = new Tbl_DataKTP_Finger_Employee
                                    {
                                        CreatedByNpp = data.CreatedBy_Npp,
                                        CreatedTime = data.Created_Time,
                                        FileName = fileName,
                                        IsActive = true,
                                        Nik = data.NIK,
                                        PathFile = filePath,
                                        TypeFinger = data.TypeFingerKiri,
                                        CreatedById = idPegawai,
                                        CreatedByUnit = data.Kode_Unit,
                                        CreatedByUid = data.CreatedBy_SerialNumber,
                                        IsDeleted = false
                                    };
                                }
                            }
                            catch (Exception ex)
                            {
                                FingerKiri = new Tbl_DataKTP_Finger
                                {
                                    CreatedByNpp = data.CreatedBy_Npp,
                                    CreatedTime = data.Created_Time,
                                    FileName = "Tidak Ditemukan",
                                    IsActive = true,
                                    Nik = data.NIK,
                                    PathFile = "Tidak Ditemukan",
                                    TypeFinger = data.TypeFingerKiri,
                                    CreatedById = idPegawai,
                                    CreatedByUnit = data.Kode_Unit,
                                    CreatedByUid = data.CreatedBy_SerialNumber,
                                    IsDeleted = false
                                };
                            }

                            if (isEmployee)
                            {
                                FingerKiriEmployee = new Tbl_DataKTP_Finger_Employee
                                {
                                    CreatedByNpp = data.CreatedBy_Npp,
                                    CreatedTime = data.Created_Time,
                                    FileName = "Tidak Ditemukan",
                                    IsActive = true,
                                    Nik = data.NIK,
                                    PathFile = "Tidak Ditemukan",
                                    TypeFinger = data.TypeFingerKiri,
                                    CreatedById = idPegawai,
                                    CreatedByUnit = data.Kode_Unit,
                                    CreatedByUid = data.CreatedBy_SerialNumber,
                                    IsDeleted = false
                                };
                            }
                        }

                        Db.WithTransaction(c =>
                        {
                            if (dataDemografis != null)
                            {
                                c.Insert(dataDemografis, true);
                            }

                            if (dataNpp != null)
                            {
                                c.Insert(dataNpp, true);
                            }

                            if (ktpPhoto != null)
                            {
                                c.Insert(ktpPhoto, true);
                            }

                            if (ktpSignature != null)
                            {
                                c.Insert(ktpSignature, true);
                            }

                            if (ktpPhotoCam != null)
                            {
                                c.Insert(ktpPhotoCam, true);
                            }

                            if (fingerKanan != null)
                            {
                                c.Insert(fingerKanan, true);
                            }

                            if (FingerKiri != null)
                            {
                                c.Insert(FingerKiri, true);
                            }

                            if (fingerKananEmployee != null)
                            {
                                c.Insert(fingerKananEmployee, true);
                            }

                            if (FingerKiriEmployee != null)
                            {
                                c.Insert(FingerKiriEmployee, true);
                            }
                        });

                        var _ = MigrateFingerJpgToEncTxtByNIK(data.NIK);
                        if (isEmployee)
                        {
                            _ = MigrateFingerJpgToEncTxtByNIK(data.NIK);
                        }
                    }
                }

                return "";
            }
            catch (Exception ex)
            {
                return ex.Message + " _ " + ex.InnerException.Message;
            }
        }
        #endregion

        #region GET
        public async Task<GridResponse<InboxEnrollNoMatchingData>> GetDBEnroll(DataEnrollTempFilter filter)
        {
            const string proc = "[ProcMonitoringEnrollDataTemp]";

            var val = new
            {
                SColumn = new DbString { Value = string.IsNullOrWhiteSpace(filter.SortColumn) ? "Id" : filter.SortColumn, Length = 100 },
                SColumnValue = new DbString { Value = string.IsNullOrWhiteSpace(filter.SortColumnDir) ? "desc" : filter.SortColumnDir, Length = 10 },
                Nama = new DbString { Value = string.IsNullOrWhiteSpace(filter.Nama) ? "" : filter.Nama, Length = 350 },
                NIK = new DbString { Value = string.IsNullOrWhiteSpace(filter.NIK) ? "" : filter.NIK, Length = 50 },
                Page = filter.PageNumber,
                Rows = filter.PageSize,
                UnitIds = new DbString { Value = filter.UnitIds.ToString(), Length = 10 }
            };

            var res = await Db.WithConnectionAsync(c => c.QueryAsync<InboxEnrollNoMatchingData>(proc, val, commandType: CommandType.StoredProcedure))
                .ConfigureAwait(false);

            const string procCount = "[ProcMonitoringEnrollTotalTemp]";

            var count = await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(procCount, new
            {
                Nama = new DbString { Value = string.IsNullOrWhiteSpace(filter.Nama) ? "" : filter.Nama, Length = 350 },
                NIK = new DbString { Value = string.IsNullOrWhiteSpace(filter.NIK) ? "" : filter.NIK, Length = 50 },
                UnitIds = new DbString { Value = filter.UnitIds.ToString(), Length = 10 }
            }, commandType: CommandType.StoredProcedure))
                .ConfigureAwait(false);

            return new GridResponse<InboxEnrollNoMatchingData>
            {
                Count = count,
                Data = res
            };
        }

        public async Task<GridResponse<MonitoringEnroll>> GetDBEnrollSec(DataEnrollTempFilter filter)
        {
            const string proc = "[ProcMonitoringEnrollDataSec]";

            var val = new
            {
                SColumn = string.IsNullOrWhiteSpace(filter.SortColumn) ? "Id" : filter.SortColumn,
                SColumnValue = string.IsNullOrWhiteSpace(filter.SortColumnDir) ? "desc" : filter.SortColumnDir,
                Nama = string.IsNullOrWhiteSpace(filter.Nama) ? "" : filter.Nama,
                NIK = string.IsNullOrWhiteSpace(filter.NIK) ? "" : filter.NIK,
                Page = filter.PageNumber,
                Rows = filter.PageSize,
                filter.LoginPegawaiId,
                filter.LoginRoleId,
                filter.LoginUnitId,
                filter.UnitIds
            };

            var res = await Db.WithConnectionAsync(c => c.QueryAsync<MonitoringEnroll>(proc, val, commandType: CommandType.StoredProcedure))
                .ConfigureAwait(false);

            const string procCount = "[ProcMonitoringEnrollTotalSec]";

            var count = await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(procCount, new
            {
                Nama = string.IsNullOrWhiteSpace(filter.Nama) ? "" : filter.Nama,
                NIK = string.IsNullOrWhiteSpace(filter.NIK) ? "" : filter.NIK,
                filter.LoginPegawaiId,
                filter.LoginRoleId,
                filter.LoginUnitId,
                filter.UnitIds
            }, commandType: CommandType.StoredProcedure))
                .ConfigureAwait(false);

            return new GridResponse<MonitoringEnroll>
            {
                Count = count,
                Data = res
            };
        }

        public async Task<IEnumerable<ExportMonitoringEnroll>> ExportDBEnroll(ExportDataEnrollTempFilter filter)
        {
            const string proc = "[ProcExportMonitoringEnrollData]";

            var val = new
            {
                SColumn = string.IsNullOrWhiteSpace(filter.SortColumn) ? "Id" : filter.SortColumn,
                SColumnValue = string.IsNullOrWhiteSpace(filter.SortColumnDir) ? "desc" : filter.SortColumnDir,
                Nama = string.IsNullOrWhiteSpace(filter.Nama) ? "" : filter.Nama,
                NIK = string.IsNullOrWhiteSpace(filter.NIK) ? "" : filter.NIK,
                filter.LoginPegawaiId,
                filter.LoginRoleId,
                filter.LoginUnitId,
                filter.UnitIds
            };

            var res = await Db.WithConnectionAsync(c => c.QueryAsync<ExportMonitoringEnroll>(proc, val, commandType: CommandType.StoredProcedure, commandTimeout: 12000))
                .ConfigureAwait(false);

            return res;
        }

        public async Task<bool> IsNppEnrolled(string npp)
        {
            const string query = "select count(*) from [dbo].[Tbl_Mapping_Pegawai_KTP] " +
                "where Npp = @npp and IsActive = 1 and (IsDeleted = 0 or IsDeleted is null)";

            return await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new { npp }))
                .ConfigureAwait(false) > 0;
        }

        public Task<string> GetNIK(string npp)
        {
            const string query = "SELECT Top(1) [NIK] FROM [dbo].[Tbl_Mapping_Pegawai_KTP] where Npp = @npp";

            return Db.WithConnectionAsync(c => c.ExecuteScalarAsync<string>(query, new { npp }));
        }

        public Task<IEnumerable<Tbl_DataKTP_Finger>> GetISO(string nik)
        {
            const string query = "select * from Tbl_DataKTP_Finger where nik = @nik and isactive = 1 and isdeleted = 0";

            return Db.WithConnectionAsync(c => c.QueryAsync<Tbl_DataKTP_Finger>(query, new { nik }));
        }
        public Task<IEnumerable<Tbl_DataKTP_Finger>> GetISOEmp(string nik)
        {
            const string query = "select * from Tbl_DataKTP_Finger_Employee where nik = @nik and isactive = 1 and isdeleted = 0";

            return Db.WithConnectionAsync(c => c.QueryAsync<Tbl_DataKTP_Finger>(query, new { nik }));
        }

        public Task<int> GetPegawaiByNpp(string npp)
        {
            const string query = "SELECT Id FROM [dbo].[Tbl_Pegawai] where Nik = @npp";

            return Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new { npp }));
        }

        public Task<int> GetUnitByCode(string code)
        {
            const string query = "SELECT Id FROM [dbo].[Tbl_Unit] where FullCode = @code";

            return Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new { code }));
        }

        public Task<IEnumerable<Tbl_DataKTP_Finger>> GetDataKtpFingersJpgFormatted()
        {
            const string query = "select * from Tbl_DataKTP_Finger where FileName like '%.jpg'";

            return Db.WithConnectionAsync(c => c.QueryAsync<Tbl_DataKTP_Finger>(query));
        }

        public Task<IEnumerable<Tbl_DataKTP_Finger>> GetDataKtpFingersJpgFormattedbyNIK(string nik)
        {
            const string query = "select * from Tbl_DataKTP_Finger where FileName like '%.jpg' and nik = @nik";

            return Db.WithConnectionAsync(c => c.QueryAsync<Tbl_DataKTP_Finger>(query, new { nik }));
        }

        public Task<IEnumerable<Tbl_DataKTP_Demografis>> GetDataDemografisByNik(string nik)
        {
            const string query = "select * from Tbl_DataKTP_Demografis where nik = @nik and isactive = 1 and isdeleted = 0";

            return Db.WithConnectionAsync(c => c.QueryAsync<Tbl_DataKTP_Demografis>(query, new { nik }));
        }

        public Task<IEnumerable<Tbl_DataKTP_Finger_Employee>> GetDataKtpEmployeeFingersJpgFormattedbyNIK(string nik)
        {
            const string query = "select * from Tbl_DataKTP_Finger_Employee where FileName like '%.jpg' and nik = @nik";

            return Db.WithConnectionAsync(c => c.QueryAsync<Tbl_DataKTP_Finger_Employee>(query, new { nik }));
        }

        public async Task<Tbl_MappingNIK_Pegawai> IsEmployee(string nik)
        {
            const string query = "select * from Tbl_MappingNIK_Pegawai where Nik = @nik";

            return await Db.WithConnectionAsync(c => c.QueryFirstOrDefaultAsync<Tbl_MappingNIK_Pegawai>(query, new { nik }));
        }

        public Task<Tbl_DataNIK_Pegawai> MappingNppNik(string npp)
        {
            //const string query = "select * from Tbl_Mapping_Pegawai_KTP where Npp = @npp";
            const string query = "select nik from Tbl_Mapping_Pegawai_KTP where Npp = @npp";

            return Db.WithConnectionAsync(c => c.QueryFirstOrDefaultAsync<Tbl_DataNIK_Pegawai>(query, new { npp = new DbString { Value = npp, Length = 50 } }));
        }

        public Task<Tbl_Mapping_Pegawai_KTP> MappingNppNikByNik(string nik)
        {
            const string query = "select * from Tbl_Mapping_Pegawai_KTP where Nik = @nik";

            return Db.WithConnectionAsync(c => c.QueryFirstOrDefaultAsync<Tbl_Mapping_Pegawai_KTP>(query, new { nik }));
        }
        #endregion

        #region Migrate Data KTP
        public async Task<int> MigrateFingerJpgToEncTxtByNIK(string nik)
        {
            var count = 0;

            var fingers = await GetDataKtpFingersJpgFormattedbyNIK(nik);

            if (!fingers.Any()) return 0;

            string JamServer = DateTime.Now.ToString("ddMMyyyyHHmmss");

            var sysPathFolder = await _sysParameterRepository.GetPathFolder("PathFolderData");
            string pathFolder = sysPathFolder.Value;

            var systemParameterPath = await _sysParameterRepository.GetPathFolder("FolderFinger")
                                                .ConfigureAwait(false);
            string pathFolderFoto = systemParameterPath.Value;

            foreach (var finger in fingers)
            {
                var oldFilePath = finger.PathFile;

                using WebClient webClient = new();

                byte[] data = webClient.DownloadData(oldFilePath);

                var imgB64String = Convert.ToBase64String(data);

                string imageEncrypted = imgB64String.Encrypt(Phrase.FileEncryption);

                // Create new one
                string subPathFolderPhotoFinger = pathFolder + "/" + pathFolderFoto + "/" + finger.Nik + "/";

                string JenisJari = finger.TypeFinger.Replace(" ", "");

                string fileName = "PhotoFinger_" + JenisJari + "_" + finger.Nik + "_" + JamServer + ".txt";

                if (!Directory.Exists(subPathFolderPhotoFinger))
                {
                    Directory.CreateDirectory(subPathFolderPhotoFinger);
                }

                var filePath = subPathFolderPhotoFinger + fileName;
                File.WriteAllText(filePath, imageEncrypted);

                finger.PathFile = filePath;
                finger.FileName = fileName;

                await UpdateDataKtpFingerAsync(finger);

                // Backup old file
                string subPathFolderPhotoFingerBackup = pathFolder + "/PhotoFingerBackup/" + finger.Nik + "/";

                string fileNameBackup = "PhotoFinger_" + JenisJari + "_" + finger.Nik + "_" + JamServer + ".jpg";

                if (!Directory.Exists(subPathFolderPhotoFingerBackup))
                {
                    Directory.CreateDirectory(subPathFolderPhotoFingerBackup);
                }

                var filePathBackup = subPathFolderPhotoFingerBackup + fileNameBackup;
                File.WriteAllBytes(filePathBackup, data);

                finger.PathFile = filePathBackup;
                finger.FileName = fileNameBackup;

                CreateFingerLog(finger);

                File.Delete(oldFilePath);

                count++;
            }

            return count;
        }

        public async Task<int> MigrateFingerEmployeeJpgToEncTxtByNIK(string nik)
        {
            var count = 0;

            var fingers = await GetDataKtpEmployeeFingersJpgFormattedbyNIK(nik);

            if (!fingers.Any()) return 0;

            string JamServer = DateTime.Now.ToString("ddMMyyyyHHmmss");

            var sysPathFolder = await _sysParameterRepository.GetPathFolder("PathFolderData");
            string pathFolder = sysPathFolder.Value;

            var systemParameterPath = await _sysParameterRepository.GetPathFolder("FolderFinger")
                                                .ConfigureAwait(false);
            string pathFolderFoto = systemParameterPath.Value;

            foreach (var finger in fingers)
            {
                var oldFilePath = finger.PathFile;

                using WebClient webClient = new();

                byte[] data = webClient.DownloadData(oldFilePath);

                var imgB64String = Convert.ToBase64String(data);

                string imageEncrypted = imgB64String.Encrypt(Phrase.FileEncryption);

                // Create new one
                string subPathFolderPhotoFinger = pathFolder + "/" + pathFolderFoto + "/" + finger.Nik + "/";

                string JenisJari = finger.TypeFinger.Replace(" ", "");

                string fileName = "PhotoFinger_" + JenisJari + "_" + finger.Nik + "_" + JamServer + ".txt";

                if (!Directory.Exists(subPathFolderPhotoFinger))
                {
                    Directory.CreateDirectory(subPathFolderPhotoFinger);
                }

                var filePath = subPathFolderPhotoFinger + fileName;
                File.WriteAllText(filePath, imageEncrypted);

                finger.PathFile = filePath;
                finger.FileName = fileName;

                await UpdateDataEmployeeKtpFingerAsync(finger);

                // Backup old file
                string subPathFolderPhotoFingerBackup = pathFolder + "/PhotoFingerBackup/" + finger.Nik + "/";

                string fileNameBackup = "PhotoFinger_" + JenisJari + "_" + finger.Nik + "_" + JamServer + ".jpg";

                if (!Directory.Exists(subPathFolderPhotoFingerBackup))
                {
                    Directory.CreateDirectory(subPathFolderPhotoFingerBackup);
                }

                var filePathBackup = subPathFolderPhotoFingerBackup + fileNameBackup;
                File.WriteAllBytes(filePathBackup, data);

                finger.PathFile = filePathBackup;
                finger.FileName = fileNameBackup;

                CreateFingerEmployeeLog(finger);

                File.Delete(oldFilePath);

                count++;
            }

            return count;
        }

        private long CreateFingerLog(Tbl_DataKTP_Finger finger)
        {
            return InsertKtpFingerLog(new Tbl_DataKTP_Finger_Log
            {
                Nik = finger.Nik,
                TypeFinger = finger.TypeFinger,
                CreatedById = finger.CreatedById,
                CreatedByNpp = finger.CreatedByNpp,
                CreatedByUid = finger.CreatedByUid,
                CreatedByUnit = finger.CreatedByUnit,
                CreatedByUnitId = finger.CreatedByUnitId,
                CreatedTime = finger.CreatedTime,
                //FileJari = finger.FileJari,
                FileName = finger.FileName,
                PathFile = finger.PathFile,
                FileNameISO = finger.FileNameISO,
                PathFileISO = finger.PathFileISO
            });
        }
        private long CreateFingerEmployeeLog(Tbl_DataKTP_Finger_Employee finger)
        {
            return InsertKtpEmployeeFingerLog(new Tbl_DataKTP_Finger_Employee_Log
            {
                Nik = finger.Nik,
                TypeFinger = finger.TypeFinger,
                CreatedById = finger.CreatedById,
                CreatedByNpp = finger.CreatedByNpp,
                CreatedByUid = finger.CreatedByUid,
                CreatedByUnit = finger.CreatedByUnit,
                CreatedByUnitId = finger.CreatedByUnitId,
                CreatedTime = finger.CreatedTime,
                //FileJari = finger.FileJari,
                FileName = finger.FileName,
                PathFile = finger.PathFile,
                FileNameISO = finger.FileNameISO,
                PathFileISO = finger.PathFileISO
            });
        }

        public async Task<ConvertLatLong_ViewModels> ConvertLatLong(string apikey, string alamatEncode, bool isProxy, string ipProxy)
        {
            string url = @"https://maps.googleapis.com/maps/api/geocode/json?address=" + alamatEncode + "&key=" + apikey;

            WebRequest request = WebRequest.Create(url);

            if (isProxy)
            {
                WebProxy myProxy = new WebProxy();
                // Obtain the Proxy Prperty of the  Default browser.

                //// Create a new Uri object.
                Uri newUri = new Uri(ipProxy);

                // Associate the new Uri object to the myProxy object.
                myProxy.Address = newUri;

                request.Proxy = myProxy;
            }

            WebResponse response = request.GetResponse();

            Stream data = response.GetResponseStream();

            StreamReader reader = new StreamReader(data);

            // json-formatted string from maps api
            string responseFromServer = reader.ReadToEnd();

            var res = JsonConvert.DeserializeObject<ConvertLatLong_ViewModels>(responseFromServer);
            return res;
        }
        #endregion

        #region Load Data
        public async Task<GridResponse<InboxDataEnrollVM>> LoadDataInboxEnroll(InboxDataEnrollFilterVM req)
        {
            const string sp = "[ProcInboxDataEnroll]";
            var values = new
            {
                req.UnitCode,
                req.Npp,
                SColumn = req.SortColumn,
                SColumnValue = req.SortColumnDir,
                Page = req.PageNumber,
                Rows = req.PageSize
            };
            var data = Db.WithConnectionAsync(db => db.QueryAsync<InboxDataEnrollVM>(sp, values, commandType: CommandType.StoredProcedure));

            const string spCount = "[ProcInboxDataEnrollNum]";
            var valuesCount = new
            {
                req.UnitCode,
                req.Npp,
            };
            var count = Db.WithConnectionAsync(db => db.ExecuteScalarAsync<int>(spCount, valuesCount, commandType: CommandType.StoredProcedure));

            await Task.WhenAll(data, count).ConfigureAwait(false);

            return new GridResponse<InboxDataEnrollVM>
            {
                Count = count.Result,
                Data = data.Result
            };
        }

        public async Task<List<Tbl_LogHistoryPengajuanVM>> LoadDataHistoryPengajuan(HistorySubmissionFilterVM historySubmissionFilterVM)
        {
            const string query = "select " +
                                        "LH.[Id] " +
                                        ",LH.[IsVerified] " +
                                        ",(case " +
                                            "when LH.[IsVerified] = 1 " +
                                                "then 'Ya' " +
                                            "else 'Tidak' " +
                                            "end) as Verifikasi" +
                                        ",LH.[DataKTPId] " +
                                        ",LH.[DataKTPNIK] " +
                                        ",LH.[ConfirmedByNpp] " +
                                        ",P.[Nama] ConfirmedByName " +
                                        ",LH.[Comment] " +
                                        ",LH.[CreatedTime] " +
                                        ",LH.[CreatedBy_Id] " +
                                    " from [dbo].[Tbl_LogHistoryPengajuan] LH " +
                                    " LEFT JOIN [dbo].[Tbl_Pegawai] P ON LH.CreatedBy_Id = P.Id " +
                                    "where DataKTPId = @DataKTPId";

            var data = await Db.WithConnectionAsync(c => c.QueryAsync<Tbl_LogHistoryPengajuanVM>(query, new { historySubmissionFilterVM.DataKTPId })) ?? new List<Tbl_LogHistoryPengajuanVM>();
            return data.ToList();
        }
        #endregion

        #region UPDATE
        public Task<int> UpdateDataKtpFingerAsync(Tbl_DataKTP_Finger data)
        {
            return UpdateAsync(data);
        }
        public Task<int> UpdateDataEmployeeKtpFingerAsync(Tbl_DataKTP_Finger_Employee data)
        {
            return UpdateAsync(data);
        }
        #endregion

        #region Insert Log SOA
        public async Task<Tbl_ThirdPartyLog> CreateThirdPartyLog(Tbl_ThirdPartyLog req)
        {
            const string query = "Insert Into Tbl_ThirdPartyLog (" +
                "[FeatureName]," +
                "[HostUrl]," +
                "[Request]," +
                "[Status]," +
                "[Response]," +
                "[CreatedDate]," +
                "[CreatedBy])" +
            "values(" +
                "@FeatureName," +
                "@HostUrl," +
                "@Request," +
                "@Status," +
                "@Response," +
                "@CreatedDate," +
                "@CreatedBy)";

            await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
            {
                req.FeatureName,
                req.HostUrl,
                req.Request,
                req.Status,
                req.Response,
                req.CreatedDate,
                req.CreatedBy
            }));

            return req;
        }
        #endregion

        #region insert updates data KTP by Manual Dapper

        #region demografi
        public async Task<Tbl_DataKTP_Demografis> InsertDemografi(Tbl_DataKTP_Demografis req)
        {
            try
            {
                const string query = "Insert Into [Tbl_DataKTP_Demografis] (" +
                "[NIK]," +
                "[Nama]," +
                "[TempatLahir]," +
                "[TanggalLahir]," +
                "[JenisKelamin]," +
                "[GolonganDarah]," +
                "[Alamat]," +
                "[RT]," +
                "[RW]," +
                "[Kelurahan]," +
                "[Desa]," +
                "[Kecamatan]," +
                "[Kota]," +
                "[Provinsi]," +
                "[Agama]," +
                "[KodePos]," +
                "[StatusPerkawinan]," +
                "[Pekerjaan]," +
                "[Kewarganegaraan]," +
                "[MasaBerlaku]," +
                "[Latitude]," +
                "[Longitude]," +
                "[AlamatLengkap]," +
                "[AlamatGoogle]," +
                "[AlamatGoogle]," +
                "[CreatedById]," +
                "[CreatedByNpp]," +
                "[CreatedTime]," +
                "[CreatedByUnitId]," +
                "[CreatedByUnitCode]," +
                "[CreatedByUID]," +
                "[CIF]," +
                "[UpdatedById]," +
                "[UpdatedByNpp]," +
                "[UpdatedTime]," +
                "[UpdatedByUnitId]," +
                "[UpdatedByUnitCode]," +
                "[UpdatedByUID]," +
                "[IsActive]," +
                "[IsDeleted]," +
                "[IsVerified]," +
                "[IsNasabahTemp]," +
                "[UpdatedCIFByBS_Time]," +
                "[UpdatedCIFByBS_Username]," +
                "[VerifiedByNpp]," +
                "[VerifyComment]," +
                "[isMigrate]) " +
            "values(" +
                "@NIK," +
                "@Nama," +
                "@TempatLahir," +
                "@TanggalLahir," +
                "@JenisKelamin," +
                "@GolonganDarah," +
                "@Alamat," +
                "@RT," +
                "@RW," +
                "@Kelurahan," +
                "@Desa," +
                "@Kecamatan," +
                "@Kota," +
                "@Provinsi," +
                "@Agama," +
                "@KodePos," +
                "@StatusPerkawinan," +
                "@Pekerjaan," +
                "@Kewarganegaraan," +
                "@MasaBerlaku," +
                "@Latitude," +
                "@Longitude," +
                "@AlamatLengkap," +
                "@AlamatGoogle," +
                "@CreatedById," +
                "@CreatedByNpp," +
                "@CreatedTime," +
                "@CreatedByUnitId," +
                "@CreatedByUnitCode," +
                "@CreatedByUID," +
                "@CIF," +
                "@UpdatedById," +
                "@UpdatedByNpp," +
                "@UpdatedTime," +
                "@UpdatedByUnitId," +
                "@UpdatedByUnitCode," +
                "@UpdatedByUID," +
                "@IsActive," +
                "@IsDeleted," +
                "@IsVerified," +
                "@IsNasabahTemp," +
                "@UpdatedCIFByBS_Time," +
                "@UpdatedCIFByBS_Username," +
                "@VerifiedByNpp," +
                "@VerifyComment," +
                "@isMigrate)";

                await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
                {
                    req.NIK,
                    req.Nama,
                    req.TempatLahir,
                    req.TanggalLahir,
                    req.JenisKelamin,
                    req.GolonganDarah,
                    req.Alamat,
                    req.RT,
                    req.RW,
                    req.Kelurahan,
                    req.Desa,
                    req.Kecamatan,
                    req.Kota,
                    req.Provinsi,
                    req.Agama,
                    req.KodePos,
                    req.StatusPerkawinan,
                    req.Pekerjaan,
                    req.Kewarganegaraan,
                    req.MasaBerlaku,
                    req.Latitude,
                    req.Longitude,
                    req.AlamatLengkap,
                    req.AlamatGoogle,
                    req.CreatedById,
                    req.CreatedByNpp,
                    req.CreatedTime,
                    req.CreatedByUnitId,
                    req.CreatedByUnitCode,
                    req.CIF,
                    req.UpdatedById,
                    req.UpdatedByNpp,
                    req.UpdatedTime,
                    req.UpdatedByUnitId,
                    req.UpdatedByUnitCode,
                    req.UpdatedByUID,
                    req.IsActive,
                    req.IsDeleted,
                    req.IsVerified,
                    req.IsNasabahTemp,
                    req.UpdatedCIFByBS_Time,
                    req.UpdatedCIFByBS_Username,
                    req.VerifiedByNpp,
                    req.VerifyComment,
                    req.isMigrate
                }));

            }
            catch (Exception ex)
            {

            }

            return req;
        }

        public async Task<int> InsertDemografiTemp(Tbl_DataKTP_Demografis_Temp req)
        {
            var data = 0;

            try
            {
                const string query = "Insert Into [Tbl_DataKTP_Demografis_Temp] (" +
                "[NIK]," +
                "[Nama]," +
                "[TempatLahir]," +
                "[TanggalLahir]," +
                "[JenisKelamin]," +
                "[GolonganDarah]," +
                "[Alamat]," +
                "[RT]," +
                "[RW]," +
                "[Kelurahan]," +
                "[Desa]," +
                "[Kecamatan]," +
                "[Kota]," +
                "[Provinsi]," +
                "[Agama]," +
                "[KodePos]," +
                "[StatusPerkawinan]," +
                "[Pekerjaan]," +
                "[Kewarganegaraan]," +
                "[MasaBerlaku]," +
                "[Latitude]," +
                "[Longitude]," +
                "[AlamatLengkap]," +
                "[AlamatGoogle]," +
                "[CreatedById]," +
                "[CreatedByNpp]," +
                "[CreatedTime]," +
                "[CreatedByUnitId]," +
                "[CreatedByUnitCode]," +
                "[CreatedByUID]," +
                "[CIF]," +
                "[UpdatedById]," +
                "[UpdatedByNpp]," +
                "[UpdatedTime]," +
                "[UpdatedByUnitId]," +
                "[UpdatedByUnitCode]," +
                "[UpdatedByUID]," +
                "[IsActive]," +
                "[IsDeleted]," +
                "[IsVerified]," +
                "[IsNasabahTemp]," +
                "[UpdatedCIFByBS_Time]," +
                "[UpdatedCIFByBS_Username]," +
                "[VerifiedByNpp]," +
                "[VerifyComment]," +
                "[isMigrate]," +
                "[NoPengajuan]," +
                "[IsApprove]) " +
                 "OUTPUT INSERTED.[Id] " +
            "values(" +
                "@NIK," +
                "@Nama," +
                "@TempatLahir," +
                "@TanggalLahir," +
                "@JenisKelamin," +
                "@GolonganDarah," +
                "@Alamat," +
                "@RT," +
                "@RW," +
                "@Kelurahan," +
                "@Desa," +
                "@Kecamatan," +
                "@Kota," +
                "@Provinsi," +
                "@Agama," +
                "@KodePos," +
                "@StatusPerkawinan," +
                "@Pekerjaan," +
                "@Kewarganegaraan," +
                "@MasaBerlaku," +
                "@Latitude," +
                "@Longitude," +
                "@AlamatLengkap," +
                "@AlamatGoogle," +
                "@CreatedById," +
                "@CreatedByNpp," +
                "@CreatedTime," +
                "@CreatedByUnitId," +
                "@CreatedByUnitCode," +
                "@CreatedByUID," +
                "@CIF," +
                "@UpdatedById," +
                "@UpdatedByNpp," +
                "@UpdatedTime," +
                "@UpdatedByUnitId," +
                "@UpdatedByUnitCode," +
                "@UpdatedByUID," +
                "@IsActive," +
                "@IsDeleted," +
                "@IsVerified," +
                "@IsNasabahTemp," +
                "@UpdatedCIFByBS_Time," +
                "@UpdatedCIFByBS_Username," +
                "@VerifiedByNpp," +
                "@VerifyComment," +
                "@isMigrate," +
                "@NoPengajuan," +
                "@IsApprove)";

                data = await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
                {
                    req.NIK,
                    req.Nama,
                    req.TempatLahir,
                    req.TanggalLahir,
                    req.JenisKelamin,
                    req.GolonganDarah,
                    req.Alamat,
                    req.RT,
                    req.RW,
                    req.Kelurahan,
                    req.Desa,
                    req.Kecamatan,
                    req.Kota,
                    req.Provinsi,
                    req.Agama,
                    req.KodePos,
                    req.StatusPerkawinan,
                    req.Pekerjaan,
                    req.Kewarganegaraan,
                    req.MasaBerlaku,
                    req.Latitude,
                    req.Longitude,
                    req.AlamatLengkap,
                    req.AlamatGoogle,
                    req.CreatedById,
                    req.CreatedByNpp,
                    req.CreatedTime,
                    req.CreatedByUnitId,
                    req.CreatedByUnitCode,
                    req.CreatedByUID,
                    req.CIF,
                    req.UpdatedById,
                    req.UpdatedByNpp,
                    req.UpdatedTime,
                    req.UpdatedByUnitId,
                    req.UpdatedByUnitCode,
                    req.UpdatedByUID,
                    req.IsActive,
                    req.IsDeleted,
                    req.IsVerified,
                    req.IsNasabahTemp,
                    req.UpdatedCIFByBS_Time,
                    req.UpdatedCIFByBS_Username,
                    req.VerifiedByNpp,
                    req.VerifyComment,
                    req.isMigrate,
                    req.NoPengajuan,
                    req.IsApprove
                }));

            }
            catch (Exception ex)
            {

            }

            return data;
        }

        public async Task<int> InsertMappingNIKFR(EnrollmentFRLog req)
        {
            var data = 0;

            try
            {
                const string query = "Insert Into [Tbl_Enrollment_FR] (" +
                "[NIK]," +
                "[CreatedById]," +
                "[CreatedByNpp]," +
                "[CreatedTime]," +
                "[MatchScore]," +
                "[CreatedByUnitId]," +
                "[inboxEnrollmentId]) " +
            "values(" +
                "@NIK," +
                "@CreatedById," +
                "@CreatedByNpp," +
                "@CreatedTime," +
                "@MatchScore," +
                "@CreatedByUnitId," +
                "@inboxEnrollmentId)";

                data = await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
                {
                    req.NIK,
                    req.CreatedById,
                    req.CreatedByNpp,
                    req.CreatedTime,
                    req.MatchScore,
                    req.CreatedByUnitId,
                    req.inboxEnrollmentId
                }));

            }
            catch (Exception ex)
            {

            }

            return data;
        }

        public async Task<int> InsertMappingNIKIKD(Tbl_Enrollment_IKD req)
        {
            var data = 0;

            try
            {
                const string query = "Insert Into [Tbl_Enrollment_IKD] (" +
                "[NIK]," +
                "[CreatedById]," +
                "[CreatedByNpp]," +
                "[CreatedTime]," +
                "[CreatedByUnitId]) " +
            "values(" +
                "@NIK," +
                "@CreatedById," +
                "@CreatedByNpp," +
                "@CreatedTime," +
                "@CreatedByUnitId)";

                data = await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
                {
                    req.NIK,
                    req.CreatedById,
                    req.CreatedByNpp,
                    req.CreatedTime,
                    req.CreatedByUnitId
                }));

            }
            catch (Exception ex)
            {

            }

            return data;
        }

        public async Task<int> InsertMappingIKD(Tbl_Enrollment_IKD req)
        {
            var data = 0;

            try
            {
                const string query = "Insert Into [Tbl_Enrollment_IKD] (" +
                "[NIK]," +
                "[CreatedById]," +
                "[CreatedByNpp]," +
                "[CreatedTime]," +
                "[CreatedByUnitId]) " +
                 "OUTPUT INSERTED.[Id] " +
            "values(" +
                "@NIK," +
                "@CreatedById," +
                "@CreatedByNpp," +
                "@CreatedTime," +
                "@CreatedByUnitId)";

                data = await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
                {
                    req.NIK,
                    req.CreatedById,
                    req.CreatedByNpp,
                    req.CreatedTime,
                    req.CreatedByUnitId,
                }));

            }
            catch (Exception ex)
            {

            }

            return data;
        }

        public async Task<Tbl_DataKTP_Demografis> UpdateDemografi(Tbl_DataKTP_Demografis req)
        {
            try
            {
                const string query = "Update Tbl_DataKTP_Demografis set " +
                    "NIK = @NIK, " +
                    "Nama = @Nama, " +
                    "TempatLahir = @TempatLahir, " +
                    "TanggalLahir = @TanggalLahir, " +
                    "JenisKelamin = @JenisKelamin, " +
                    "GolonganDarah = @GolonganDarah, " +
                    "Alamat = @Alamat, " +
                    "RT = @RT, " +
                    "RW = @RW, " +
                    "Kelurahan = @Kelurahan, " +
                    "Desa = @Desa, " +
                    "Kecamatan = @Kecamatan, " +
                    "Kota = @Kota, " +
                    "Provinsi = @Provinsi, " +
                    "Agama = @Agama, " +
                    "KodePos = @KodePos, " +
                    "StatusPerkawinan = @StatusPerkawinan, " +
                    "Pekerjaan = @Pekerjaan, " +
                    "Kewarganegaraan = @Kewarganegaraan, " +
                    "MasaBerlaku = @MasaBerlaku, " +
                    "Latitude = @Latitude, " +
                    "Longitude = @Longitude, " +
                    "AlamatLengkap = @AlamatLengkap, " +
                    "AlamatGoogle = @AlamatGoogle, " +
                    "CreatedById = @CreatedById, " +
                    "CreatedByNpp = @CreatedByNpp, " +
                    "CreatedTime = @CreatedTime, " +
                    "CreatedByUnitId = @CreatedByUnitId, " +
                    "CreatedByUnitCode = @CreatedByUnitCode, " +
                    "CreatedByUID = @CreatedByUID, " +
                    "UpdatedById = @UpdatedById, " +
                    "UpdatedByNpp = @UpdatedByNpp, " +
                    "UpdatedTime = @UpdatedTime, " +
                    "UpdatedByUnitId = @UpdatedByUnitId, " +
                    "UpdatedByUnitCode = @UpdatedByUnitCode, " +
                    "UpdatedByUID = @UpdatedByUID, " +
                    "IsDeleted = @IsDeleted, " +
                    "CIF = @CIF, " +
                    "IsVerified = @IsVerified, " +
                    "IsNasabahTemp = @IsNasabahTemp, " +
                    "UpdatedCIFByBS_Time = @UpdatedCIFByBS_Time, " +
                    "UpdatedCIFByBS_Username = @UpdatedCIFByBS_Username, " +
                    "VerifiedByNpp = @VerifiedByNpp, " +
                    "VerifyComment = @VerifyComment, " +
                    "isMigrate = @isMigrate " +
                "Where Id = @Id";

                await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
                {
                    req.NIK,
                    req.Nama,
                    req.TempatLahir,
                    req.TanggalLahir,
                    req.JenisKelamin,
                    req.GolonganDarah,
                    req.Alamat,
                    req.RT,
                    req.RW,
                    req.Kelurahan,
                    req.Desa,
                    req.Kecamatan,
                    req.Kota,
                    req.Provinsi,
                    req.Agama,
                    req.KodePos,
                    req.StatusPerkawinan,
                    req.Pekerjaan,
                    req.Kewarganegaraan,
                    req.MasaBerlaku,
                    req.Latitude,
                    req.Longitude,
                    req.AlamatLengkap,
                    req.AlamatGoogle,
                    req.CreatedById,
                    req.CreatedByNpp,
                    req.CreatedTime,
                    req.CreatedByUnitId,
                    req.CreatedByUnitCode,
                    req.CreatedByUID,
                    req.UpdatedById,
                    req.UpdatedByNpp,
                    req.UpdatedTime,
                    req.UpdatedByUnitId,
                    req.UpdatedByUnitCode,
                    req.UpdatedByUID,
                    req.IsActive,
                    req.IsDeleted,
                    req.CIF,
                    req.IsVerified,
                    req.IsNasabahTemp,
                    req.UpdatedCIFByBS_Time,
                    req.UpdatedCIFByBS_Username,
                    req.VerifiedByNpp,
                    req.VerifyComment,
                    req.isMigrate,
                    req.Id
                }));
            }
            catch (Exception ex)
            {

            }

            return req;
        }

        public async Task<Tbl_DataKTP_Demografis_Temp> UpdateDemografiTemp(Tbl_DataKTP_Demografis_Temp req)
        {
            try
            {
                const string query = "Update Tbl_DataKTP_Demografis_Temp set " +
                    "NIK = @NIK, " +
                    "Nama = @Nama, " +
                    "TempatLahir = @TempatLahir, " +
                    "TanggalLahir = @TanggalLahir, " +
                    "JenisKelamin = @JenisKelamin, " +
                    "GolonganDarah = @GolonganDarah, " +
                    "Alamat = @Alamat, " +
                    "RT = @RT, " +
                    "RW = @RW, " +
                    "Kelurahan = @Kelurahan, " +
                    "Desa = @Desa, " +
                    "Kecamatan = @Kecamatan, " +
                    "Kota = @Kota, " +
                    "Provinsi = @Provinsi, " +
                    "Agama = @Agama, " +
                    "KodePos = @KodePos, " +
                    "StatusPerkawinan = @StatusPerkawinan, " +
                    "Pekerjaan = @Pekerjaan, " +
                    "Kewarganegaraan = @Kewarganegaraan, " +
                    "MasaBerlaku = @MasaBerlaku, " +
                    "Latitude = @Latitude, " +
                    "Longitude = @Longitude, " +
                    "AlamatLengkap = @AlamatLengkap, " +
                    "AlamatGoogle = @AlamatGoogle, " +
                    "CreatedById = @CreatedById, " +
                    "CreatedByNpp = @CreatedByNpp, " +
                    "CreatedTime = @CreatedTime, " +
                    "CreatedByUnitId = @CreatedByUnitId, " +
                    "CreatedByUnitCode = @CreatedByUnitCode, " +
                    "CreatedByUID = @CreatedByUID, " +
                    "UpdatedById = @UpdatedById, " +
                    "UpdatedByNpp = @UpdatedByNpp, " +
                    "UpdatedTime = @UpdatedTime, " +
                    "UpdatedByUnitId = @UpdatedByUnitId, " +
                    "UpdatedByUnitCode = @UpdatedByUnitCode, " +
                    "UpdatedByUID = @UpdatedByUID, " +
                    "IsDeleted = @IsDeleted, " +
                    "CIF = @CIF, " +
                    "IsVerified = @IsVerified, " +
                    "IsNasabahTemp = @IsNasabahTemp, " +
                    "UpdatedCIFByBS_Time = @UpdatedCIFByBS_Time, " +
                    "UpdatedCIFByBS_Username = @UpdatedCIFByBS_Username, " +
                    "VerifiedByNpp = @VerifiedByNpp, " +
                    "VerifyComment = @VerifyComment, " +
                    "isMigrate = @isMigrate " +
                "Where Id = @Id";

                await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
                {
                    req.NIK,
                    req.Nama,
                    req.TempatLahir,
                    req.TanggalLahir,
                    req.JenisKelamin,
                    req.GolonganDarah,
                    req.Alamat,
                    req.RT,
                    req.RW,
                    req.Kelurahan,
                    req.Desa,
                    req.Kecamatan,
                    req.Kota,
                    req.Provinsi,
                    req.Agama,
                    req.KodePos,
                    req.StatusPerkawinan,
                    req.Pekerjaan,
                    req.Kewarganegaraan,
                    req.MasaBerlaku,
                    req.Latitude,
                    req.Longitude,
                    req.AlamatLengkap,
                    req.AlamatGoogle,
                    req.CreatedById,
                    req.CreatedByNpp,
                    req.CreatedTime,
                    req.CreatedByUnitId,
                    req.CreatedByUnitCode,
                    req.CreatedByUID,
                    req.UpdatedById,
                    req.UpdatedByNpp,
                    req.UpdatedTime,
                    req.UpdatedByUnitId,
                    req.UpdatedByUnitCode,
                    req.UpdatedByUID,
                    req.IsActive,
                    req.IsDeleted,
                    req.CIF,
                    req.IsVerified,
                    req.IsNasabahTemp,
                    req.UpdatedCIFByBS_Time,
                    req.UpdatedCIFByBS_Username,
                    req.VerifiedByNpp,
                    req.VerifyComment,
                    req.isMigrate,
                    req.Id
                }));
            }
            catch (Exception ex)
            {

            }

            return req;
        }

        public async Task<Tbl_DataKTP_Demografis_Log> InsertDemografiLog(Tbl_DataKTP_Demografis_Log req)
        {
            try
            {
                const string query = "Insert Into [Tbl_DataKTP_Demografis_Log] (" +
                "[NIK]," +
                "[Nama]," +
                "[TempatLahir]," +
                "[TanggalLahir]," +
                "[JenisKelamin]," +
                "[GolonganDarah]," +
                "[Alamat]," +
                "[RT]," +
                "[RW]," +
                "[Kelurahan]," +
                "[Desa]," +
                "[Kecamatan]," +
                "[Kota]," +
                "[Provinsi]," +
                "[Agama]," +
                "[KodePos]," +
                "[StatusPerkawinan]," +
                "[Pekerjaan]," +
                "[Kewarganegaraan]," +
                "[MasaBerlaku]," +
                "[Latitude]," +
                "[Longitude]," +
                "[AlamatLengkap]," +
                "[AlamatGoogle]," +
                "[CreatedById]," +
                "[CreatedByNpp]," +
                "[CreatedTime]," +
                "[CreatedByUnitId]," +
                "[CreatedByUID]," +
                "[CIF]) " +
            "values(" +
                "@NIK," +
                "@Nama," +
                "@TempatLahir," +
                "@TanggalLahir," +
                "@JenisKelamin," +
                "@GolonganDarah," +
                "@Alamat," +
                "@RT," +
                "@RW," +
                "@Kelurahan," +
                "@Desa," +
                "@Kecamatan," +
                "@Kota," +
                "@Provinsi," +
                "@Agama," +
                "@KodePos," +
                "@StatusPerkawinan," +
                "@Pekerjaan," +
                "@Kewarganegaraan," +
                "@MasaBerlaku," +
                "@Latitude," +
                "@Longitude," +
                "@AlamatLengkap," +
                "@AlamatGoogle," +
                "@CreatedById," +
                "@CreatedByNpp," +
                "@CreatedTime," +
                "@CreatedByUnitId," +
                "@CreatedByUID," +
                "@CIF)";

                await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
                {
                    req.NIK,
                    req.Nama,
                    req.TempatLahir,
                    req.TanggalLahir,
                    req.JenisKelamin,
                    req.GolonganDarah,
                    req.Alamat,
                    req.RT,
                    req.RW,
                    req.Kelurahan,
                    req.Desa,
                    req.Kecamatan,
                    req.Kota,
                    req.Provinsi,
                    req.Agama,
                    req.KodePos,
                    req.StatusPerkawinan,
                    req.Pekerjaan,
                    req.Kewarganegaraan,
                    req.MasaBerlaku,
                    req.Latitude,
                    req.Longitude,
                    req.AlamatLengkap,
                    req.AlamatGoogle,
                    req.CreatedById,
                    req.CreatedByNpp,
                    req.CreatedTime,
                    req.CreatedByUnitId,
                    req.CreatedByUID,
                    req.CIF
                }));
            }
            catch (Exception ex)
            {

            }

            return req;
        }

        public async Task<int> InsertDemografiLogReturnId(Tbl_DataKTP_Demografis_Log req)
        {
            var data = 0;
            try
            {
                const string query = "Insert Into [Tbl_DataKTP_Demografis_Log] (" +
                "[NIK]," +
                "[Nama]," +
                "[TempatLahir]," +
                "[TanggalLahir]," +
                "[JenisKelamin]," +
                "[GolonganDarah]," +
                "[Alamat]," +
                "[RT]," +
                "[RW]," +
                "[Kelurahan]," +
                "[Desa]," +
                "[Kecamatan]," +
                "[Kota]," +
                "[Provinsi]," +
                "[Agama]," +
                "[KodePos]," +
                "[StatusPerkawinan]," +
                "[Pekerjaan]," +
                "[Kewarganegaraan]," +
                "[MasaBerlaku]," +
                "[Latitude]," +
                "[Longitude]," +
                "[AlamatLengkap]," +
                "[AlamatGoogle]," +
                "[CreatedById]," +
                "[CreatedByNpp]," +
                "[CreatedTime]," +
                "[CreatedByUnitId]," +
                "[CreatedByUID]," +
                "[CIF]) " +
                "OUTPUT INSERTED.[Id] " +
            "values(" +
                "@NIK," +
                "@Nama," +
                "@TempatLahir," +
                "@TanggalLahir," +
                "@JenisKelamin," +
                "@GolonganDarah," +
                "@Alamat," +
                "@RT," +
                "@RW," +
                "@Kelurahan," +
                "@Desa," +
                "@Kecamatan," +
                "@Kota," +
                "@Provinsi," +
                "@Agama," +
                "@KodePos," +
                "@StatusPerkawinan," +
                "@Pekerjaan," +
                "@Kewarganegaraan," +
                "@MasaBerlaku," +
                "@Latitude," +
                "@Longitude," +
                "@AlamatLengkap," +
                "@AlamatGoogle," +
                "@CreatedById," +
                "@CreatedByNpp," +
                "@CreatedTime," +
                "@CreatedByUnitId," +
                "@CreatedByUID," +
                "@CIF)";

                data = await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
                {
                    req.NIK,
                    req.Nama,
                    req.TempatLahir,
                    req.TanggalLahir,
                    req.JenisKelamin,
                    req.GolonganDarah,
                    req.Alamat,
                    req.RT,
                    req.RW,
                    req.Kelurahan,
                    req.Desa,
                    req.Kecamatan,
                    req.Kota,
                    req.Provinsi,
                    req.Agama,
                    req.KodePos,
                    req.StatusPerkawinan,
                    req.Pekerjaan,
                    req.Kewarganegaraan,
                    req.MasaBerlaku,
                    req.Latitude,
                    req.Longitude,
                    req.AlamatLengkap,
                    req.AlamatGoogle,
                    req.CreatedById,
                    req.CreatedByNpp,
                    req.CreatedTime,
                    req.CreatedByUnitId,
                    req.CreatedByUID,
                    req.CIF
                }));
            }
            catch (Exception ex)
            {

            }

            return data;
        }

        public async Task<Tbl_Mapping_Pegawai_KTP> InsertMappingPegawaiKTP(Tbl_Mapping_Pegawai_KTP req)
        {
            try
            {
                const string query = "Insert Into [Tbl_Mapping_Pegawai_KTP] (" +
                "[Npp]," +
                "[NIK]," +
                "[CreatedById]," +
                "[CreatedByNpp]," +
                "[CreatedTime]," +
                "[CreatedByUnit]," +
                "[CreatedByUID]," +
                "[UpdatedById]," +
                "[UpdatedByNpp]," +
                "[UpdatedTime]," +
                "[UpdatedByUnit]," +
                "[UpdatedByUID]," +
                "[IsActive]," +
                "[IsDeleted]) " +
            "values(" +
                "@Npp," +
                "@NIK," +
                "@CreatedById," +
                "@CreatedByNpp," +
                "@CreatedTime," +
                "@CreatedByUnit," +
                "@CreatedByUID," +
                "@UpdatedById," +
                "@UpdatedByNpp," +
                "@UpdatedTime," +
                "@UpdatedByUnit," +
                "@UpdatedByUID," +
                "@IsActive," +
                "@IsDeleted)";

                await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
                {
                    req.Npp,
                    req.NIK,
                    req.CreatedById,
                    req.CreatedByNpp,
                    req.CreatedTime,
                    req.CreatedByUnit,
                    req.CreatedByUID,
                    req.UpdatedById,
                    req.UpdatedByNpp,
                    req.UpdatedTime,
                    req.UpdatedByUnit,
                    req.UpdatedByUID,
                    req.IsActive,
                    req.IsDeleted
                }));

            }
            catch (Exception ex)
            {

            }

            return req;
        }
        
        public async Task<Tbl_MasterAlatReaderLog> InsertReaderLog(Tbl_MasterAlatReaderLog req)
        {
            try
            {
                const string query = "INSERT INTO [dbo].[Tbl_MasterAlatReaderLog] ([UID],[Serial_Number],[Type],[NIK],[PegawaiId],[UnitId],[IsActive],[IsDeleted],[CreatedTime],[CreatedBy_Id]) " +
                    "VALUES (@Uid, @Serial_Number, @Type, @Nik, @PegawaiId, @UnitId,@IsActive, @IsDeleted, @CreatedTime, @CreatedBy_Id);";

                await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
                {
                    req.Uid,
                    req.Serial_Number,
                    req.Type,
                    req.Nik,
                    req.PegawaiId,
                    req.UnitId,
                    req.IsActive,
                    req.IsDeleted,
                    req.CreatedTime,
                    req.CreatedBy_Id
                }));

            }
            catch (Exception ex)
            {

            }

            return req;
        }

        public async Task<Tbl_Mapping_Pegawai_KTP> UpdateMappingKTP(Tbl_Mapping_Pegawai_KTP req)
        {
            try
            {
                const string query = "Update Tbl_Mapping_Pegawai_KTP set " +
                    "Npp = @Npp, " +
                    "NIK = @NIK, " +
                    "UpdatedById = @UpdatedById, " +
                    "UpdatedByNpp = @UpdatedByNpp, " +
                    "UpdatedTime = @UpdatedTime, " +
                    "UpdatedByUnit = @UpdatedByUnit, " +
                    "UpdatedByUID = @UpdatedByUID " +
                "Where Id = @Id";

                await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
                {
                    req.Npp,
                    req.NIK,
                    req.UpdatedById,
                    req.UpdatedByNpp,
                    req.UpdatedTime,
                    req.UpdatedByUnit,
                    req.UpdatedByUID,
                    req.Id
                }));
            }
            catch (Exception ex)
            {

            }

            return req;
        }
        #endregion

        #region photo
        public async Task<Tbl_DataKTP_Photo> InsertPhoto(Tbl_DataKTP_Photo req)
        {
            const string query = "Insert Into [Tbl_DataKTP_Photo] (" +
                    "[NIK]," +
                    "[PathFile]," +
                    "[FileName]," +
                    "[CreatedById]," +
                    "[CreatedByNpp]," +
                    "[CreatedTime]," +
                    "[CreatedByUnit]," +
                    "[CreatedByUID]," +
                    "[UpdatedById]," +
                    "[UpdatedByNpp]," +
                    "[UpdatedTime]," +
                    "[UpdatedByUnit]," +
                    "[UpdatedByUID]," +
                    "[IsActive]," +
                    "[IsDeleted]) " +
                "values(" +
                    "@NIK," +
                    "@PathFile," +
                    "@FileName," +
                    "@CreatedById," +
                    "@CreatedByNpp," +
                    "@CreatedTime," +
                    "@CreatedByUnit," +
                    "@CreatedByUID," +
                    "@UpdatedById," +
                    "@UpdatedByNpp," +
                    "@UpdatedTime," +
                    "@UpdatedByUnit," +
                    "@UpdatedByUID," +
                    "@IsActive," +
                    "@IsDeleted)";

            await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
            {
                NIK = req.Nik,
                req.PathFile,
                req.FileName,
                req.CreatedById,
                req.CreatedByNpp,
                req.CreatedTime,
                req.CreatedByUnit,
                CreatedByUID = req.CreatedByUid,
                req.UpdatedById,
                req.UpdatedByNpp,
                req.UpdatedTime,
                req.UpdatedByUnit,
                UpdatedByUID = req.UpdatedByUid,
                req.IsActive,
                req.IsDeleted
            }));

            return req;
        }

        public async Task<Tbl_DataKTP_Photo_Temp> InsertPhotoTemp(Tbl_DataKTP_Photo_Temp req)
        {
            const string query = "Insert Into [Tbl_DataKTP_Photo_Temp] (" +
                    "[NIK]," +
                    "[PathFile]," +
                    "[FileName]," +
                    "[CreatedById]," +
                    "[CreatedByNpp]," +
                    "[CreatedTime]," +
                    "[CreatedByUnit]," +
                    "[CreatedByUID]," +
                    "[UpdatedById]," +
                    "[UpdatedByNpp]," +
                    "[UpdatedTime]," +
                    "[UpdatedByUnit]," +
                    "[UpdatedByUID]," +
                    "[IsActive]," +
                    "[IsDeleted]," +
                    "[NoPengajuan]," +
                    "[DemografisTempId]," +
                    "[IsApprove]) " +
                "values(" +
                    "@NIK," +
                    "@PathFile," +
                    "@FileName," +
                    "@CreatedById," +
                    "@CreatedByNpp," +
                    "@CreatedTime," +
                    "@CreatedByUnit," +
                    "@CreatedByUID," +
                    "@UpdatedById," +
                    "@UpdatedByNpp," +
                    "@UpdatedTime," +
                    "@UpdatedByUnit," +
                    "@UpdatedByUID," +
                    "@IsActive," +
                    "@IsDeleted," +
                    "@NoPengajuan," +
                    "@DemografisTempId," +
                    "@IsApprove)";

            await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
            {
                NIK = req.Nik,
                req.PathFile,
                req.FileName,
                req.CreatedById,
                req.CreatedByNpp,
                req.CreatedTime,
                req.CreatedByUnit,
                req.CreatedByUID,
                req.UpdatedById,
                req.UpdatedByNpp,
                req.UpdatedTime,
                req.UpdatedByUnit,
                req.UpdatedByUID,
                req.IsActive,
                req.IsDeleted,
                req.NoPengajuan,
                req.DemografisTempId,
                req.IsApprove
            }));

            return req;
        }
        public async Task<Tbl_DataKTP_Photo> DeletePhoto(Tbl_DataKTP_Photo req)
        {
            const string query = "delete from Tbl_DataKTP_Photo " +
                    "Where Id = @Id";

            await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
            {
                req.Id
            }));

            return req;
        }

        public async Task<Tbl_DataKTP_Photo_Temp> DeletePhotoTemp(Tbl_DataKTP_Photo_Temp req)
        {
            const string query = "delete from Tbl_DataKTP_Photo_Temp " +
                    "Where Id = @Id";

            await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
            {
                req.Id
            }));

            return req;
        }
        public async Task<Tbl_DataKTP_Photo_Log> InsertPhotoLog(Tbl_DataKTP_Photo_Log req)
        {
            const string query = "Insert Into [Tbl_DataKTP_Photo_Log] (" +
                "[NIK]," +
                "[PathFile]," +
                "[FileName]," +
                "[CreatedById]," +
                "[CreatedByNpp]," +
                "[CreatedTime]," +
                "[CreatedByUnit]," +
                "[CreatedByUID]) " +
            "values(" +
                "@NIK," +
                "@PathFile," +
                "@FileName," +
                "@CreatedById," +
                "@CreatedByNpp," +
                "@CreatedTime," +
                "@CreatedByUnit," +
                "@CreatedByUID)";

            await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
            {
                NIK = req.Nik,
                req.PathFile,
                req.FileName,
                req.CreatedById,
                req.CreatedByNpp,
                req.CreatedTime,
                req.CreatedByUnit,
                CreatedByUID = req.CreatedByUid
            }));

            return req;
        }


        #endregion

        #region signature
        public async Task<Tbl_DataKTP_Signature> InsertSignature(Tbl_DataKTP_Signature req)
        {
            const string query = "Insert Into [Tbl_DataKTP_Signature] (" +
                "[NIK]," +
                "[PathFile]," +
                "[FileName]," +
                "[CreatedById]," +
                "[CreatedByNpp]," +
                "[CreatedTime]," +
                "[CreatedByUnit]," +
                "[CreatedByUID]," +
                "[UpdatedById]," +
                "[UpdatedByNpp]," +
                "[UpdatedTime]," +
                "[UpdatedByUnit]," +
                "[UpdatedByUID]," +
                "[IsActive]," +
                "[IsDeleted]) " +
            "values(" +
                "@NIK," +
                "@PathFile," +
                "@FileName," +
                "@CreatedById," +
                "@CreatedByNpp," +
                "@CreatedTime," +
                "@CreatedByUnit," +
                "@CreatedByUID," +
                "@UpdatedById," +
                "@UpdatedByNpp," +
                "@UpdatedTime," +
                "@UpdatedByUnit," +
                "@UpdatedByUID," +
                "@IsActive," +
                "@IsDeleted)";

            await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
            {
                NIK = req.Nik,
                req.PathFile,
                req.FileName,
                req.CreatedById,
                req.CreatedByNpp,
                req.CreatedTime,
                req.CreatedByUnit,
                CreatedByUID = req.CreatedByUid,
                req.UpdatedById,
                req.UpdatedByNpp,
                req.UpdatedTime,
                req.UpdatedByUnit,
                UpdatedByUID = req.UpdatedByUid,
                req.IsActive,
                req.IsDeleted
            }));

            return req;
        }

        public async Task<Tbl_DataKTP_Signature_Temp> InsertSignatureTemp(Tbl_DataKTP_Signature_Temp req)
        {
            const string query = "Insert Into [Tbl_DataKTP_Signature_Temp] (" +
                "[NIK]," +
                "[PathFile]," +
                "[FileName]," +
                "[CreatedById]," +
                "[CreatedByNpp]," +
                "[CreatedTime]," +
                "[CreatedByUnit]," +
                "[CreatedByUID]," +
                "[UpdatedById]," +
                "[UpdatedByNpp]," +
                "[UpdatedTime]," +
                "[UpdatedByUnit]," +
                "[UpdatedByUID]," +
                "[IsActive]," +
                "[IsDeleted]," +
                "[NoPengajuan]," +
                "[DemografisTempId]," +
                "[IsApprove]) " +
            "values(" +
                "@NIK," +
                "@PathFile," +
                "@FileName," +
                "@CreatedById," +
                "@CreatedByNpp," +
                "@CreatedTime," +
                "@CreatedByUnit," +
                "@CreatedByUID," +
                "@UpdatedById," +
                "@UpdatedByNpp," +
                "@UpdatedTime," +
                "@UpdatedByUnit," +
                "@UpdatedByUID," +
                "@IsActive," +
                "@IsDeleted," +
                "@NoPengajuan," +
                "@DemografisTempId," +
                "@IsApprove)";

            await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
            {
                NIK = req.Nik,
                req.PathFile,
                req.FileName,
                req.CreatedById,
                req.CreatedByNpp,
                req.CreatedTime,
                req.CreatedByUnit,
                req.CreatedByUID,
                req.UpdatedById,
                req.UpdatedByNpp,
                req.UpdatedTime,
                req.UpdatedByUnit,
                req.UpdatedByUID,
                req.IsActive,
                req.IsDeleted,
                req.NoPengajuan,
                req.DemografisTempId,
                req.IsApprove
            }));

            return req;
        }
        public async Task<Tbl_DataKTP_Signature> DeleteSignature(Tbl_DataKTP_Signature req)
        {
            const string query = "delete from Tbl_DataKTP_Signature " +
                    "Where Id = @Id";

            await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
            {
                req.Id
            }));

            return req;
        }

        public async Task<Tbl_DataKTP_Signature_Temp> DeleteSignatureTemp(Tbl_DataKTP_Signature_Temp req)
        {
            const string query = "delete from Tbl_DataKTP_Signature_Temp " +
                    "Where Id = @Id";

            await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
            {
                req.Id
            }));

            return req;
        }
        public async Task<Tbl_DataKTP_Signature_Log> InsertSignatureLog(Tbl_DataKTP_Signature_Log req)
        {
            const string query = "Insert Into [Tbl_DataKTP_Signature_Log] (" +
                "[NIK]," +
                "[PathFile]," +
                "[FileName]," +
                "[CreatedById]," +
                "[CreatedByNpp]," +
                "[CreatedTime]," +
                "[CreatedByUnit]," +
                "[CreatedByUID]) " +
            "values(" +
                "@NIK," +
                "@PathFile," +
                "@FileName," +
                "@CreatedById," +
                "@CreatedByNpp," +
                "@CreatedTime," +
                "@CreatedByUnit," +
                "@CreatedByUID)";

            await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
            {
                NIK = req.Nik,
                req.PathFile,
                req.FileName,
                req.CreatedById,
                req.CreatedByNpp,
                req.CreatedTime,
                req.CreatedByUnit,
                CreatedByUID = req.CreatedByUid
            }));

            return req;
        }
        #endregion

        #region PhotoCam
        public async Task<Tbl_DataKTP_PhotoCam> InsertPhotoCam(Tbl_DataKTP_PhotoCam req)
        {
            const string query = "Insert Into [Tbl_DataKTP_PhotoCam] (" +
                "[NIK]," +
                "[PathFile]," +
                "[FileName]," +
                "[CreatedById]," +
                "[CreatedByNpp]," +
                "[CreatedTime]," +
                "[CreatedByUnit]," +
                "[CreatedByUID]," +
                "[UpdatedById]," +
                "[UpdatedByNpp]," +
                "[UpdatedTime]," +
                "[UpdatedByUnit]," +
                "[UpdatedByUID]," +
                "[IsActive]," +
                "[IsDeleted]) " +
            "values(" +
                "@NIK," +
                "@PathFile," +
                "@FileName," +
                "@CreatedById," +
                "@CreatedByNpp," +
                "@CreatedTime," +
                "@CreatedByUnit," +
                "@CreatedByUID," +
                "@UpdatedById," +
                "@UpdatedByNpp," +
                "@UpdatedTime," +
                "@UpdatedByUnit," +
                "@UpdatedByUID," +
                "@IsActive," +
                "@IsDeleted)";

            await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
            {
                NIK = req.Nik,
                req.PathFile,
                req.FileName,
                req.CreatedById,
                req.CreatedByNpp,
                req.CreatedTime,
                req.CreatedByUnit,
                CreatedByUID = req.CreatedByUid,
                req.UpdatedById,
                req.UpdatedByNpp,
                req.UpdatedTime,
                req.UpdatedByUnit,
                UpdatedByUID = req.UpdatedByUid,
                req.IsActive,
                req.IsDeleted
            }));

            return req;
        }

        public async Task<Tbl_DataKTP_PhotoCam_Temp> InsertPhotoCamTemp(Tbl_DataKTP_PhotoCam_Temp req)
        {
            try {
                const string query = "Insert Into [Tbl_DataKTP_PhotoCam_Temp] (" +
                 "[NIK]," +
                 "[PathFile]," +
                 "[FileName]," +
                 "[CreatedById]," +
                 "[CreatedByNpp]," +
                 "[CreatedTime]," +
                 "[CreatedByUnit]," +
                 "[CreatedByUID]," +
                 "[UpdatedById]," +
                 "[UpdatedByNpp]," +
                 "[UpdatedTime]," +
                 "[UpdatedByUnit]," +
                 "[UpdatedByUID]," +
                 "[IsActive]," +
                 "[IsDeleted]," +
                 "[NoPengajuan]," +
                 "[DemografisTempId]," +
                 "[IsApprove]) " +
             "values(" +
                 "@NIK," +
                 "@PathFile," +
                 "@FileName," +
                 "@CreatedById," +
                 "@CreatedByNpp," +
                 "@CreatedTime," +
                 "@CreatedByUnit," +
                 "@CreatedByUID," +
                 "@UpdatedById," +
                 "@UpdatedByNpp," +
                 "@UpdatedTime," +
                 "@UpdatedByUnit," +
                 "@UpdatedByUID," +
                 "@IsActive," +
                 "@IsDeleted," +
                 "@NoPengajuan," +
                 "@DemografisTempId," +
                 "@IsApprove)";

                var data = await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
                {
                    NIK = req.Nik,
                    req.PathFile,
                    req.FileName,
                    req.CreatedById,
                    req.CreatedByNpp,
                    req.CreatedTime,
                    req.CreatedByUnit,
                    req.CreatedByUID,
                    req.UpdatedById,
                    req.UpdatedByNpp,
                    req.UpdatedTime,
                    req.UpdatedByUnit,
                    req.UpdatedByUID,
                    req.IsActive,
                    req.IsDeleted,
                    req.NoPengajuan,
                    req.DemografisTempId,
                    req.IsApprove
                }));
            }
         
            catch(Exception Ex){ 
            
            }

            return req;
        }
        public async Task<Tbl_DataKTP_PhotoCam> DeletePhotoCam(Tbl_DataKTP_PhotoCam req)
        {
            const string query = "delete from Tbl_DataKTP_PhotoCam " +
                "Where Id = @Id";

            await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
            {
                req.Id
            }));

            return req;
        }

        public async Task<Tbl_DataKTP_PhotoCam_Temp> DeletePhotoCamTemp(Tbl_DataKTP_PhotoCam_Temp req)
        {
            const string query = "delete from Tbl_DataKTP_PhotoCam_Temp " +
                "Where Id = @Id";

            await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
            {
                req.Id
            }));

            return req;
        }
        public async Task<Tbl_DataKTP_PhotoCam_Log> InsertPhotoCamLog(Tbl_DataKTP_PhotoCam_Log req)
        {
            const string query = "Insert Into [Tbl_DataKTP_PhotoCam_Log] (" +
                "[NIK]," +
                "[PathFile]," +
                "[FileName]," +
                "[CreatedById]," +
                "[CreatedByNpp]," +
                "[CreatedTime]," +
                "[CreatedByUnit]," +
                "[CreatedByUID]) " +
            "values(" +
                "@NIK," +
                "@PathFile," +
                "@FileName," +
                "@CreatedById," +
                "@CreatedByNpp," +
                "@CreatedTime," +
                "@CreatedByUnit," +
                "@CreatedByUID)";

            await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
            {
                NIK = req.Nik,
                req.PathFile,
                req.FileName,
                req.CreatedById,
                req.CreatedByNpp,
                req.CreatedTime,
                req.CreatedByUnit,
                CreatedByUID = req.CreatedByUid
            }));

            return req;
        }
        #endregion

        #region finger
        public async Task<Tbl_DataKTP_Finger> InsertFinger(Tbl_DataKTP_Finger req)
        {
            try
            {
                const string query = "Insert Into [Tbl_DataKTP_Finger] (" +
                                "[NIK]," +
                                "[TypeFinger]," +
                                "[PathFile]," +
                                "[FileName]," +
                                "[PathFileISO]," +
                                "[FileNameISO]," +
                                //"[FileJari]," +
                                "[FileJariISO]," +
                                "[CreatedById]," +
                                "[CreatedByNpp]," +
                                "[CreatedTime]," +
                                "[CreatedByUnitId]," +
                                "[CreatedByUnit]," +
                                "[CreatedByUID]," +
                                "[UpdatedById]," +
                                "[UpdatedByNpp]," +
                                "[UpdatedTime]," +
                                "[UpdatedByUnit]," +
                                "[UpdatedByUID]," +
                                "[IsActive]," +
                                "[IsDeleted]) " +
                            "values(" +
                                "@NIK," +
                                "@TypeFinger," +
                                "@PathFile," +
                                "@FileName," +
                                "@PathFileISO," +
                                "@FileNameISO," +
                                //"@FileJari," +
                                "@FileJariISO," +
                                "@CreatedById," +
                                "@CreatedByNpp," +
                                "@CreatedTime," +
                                "@CreatedByUnitId," +
                                "@CreatedByUnit," +
                                "@CreatedByUID," +
                                "@UpdatedById," +
                                "@UpdatedByNpp," +
                                "@UpdatedTime," +
                                "@UpdatedByUnit," +
                                "@UpdatedByUID," +
                                "@IsActive," +
                                "@IsDeleted)";

                await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
                {
                    NIK = req.Nik,
                    req.TypeFinger,
                    req.PathFile,
                    req.FileName,
                    req.PathFileISO,
                    req.FileNameISO,
                    //req.FileJari,
                    req.FileJariISO,
                    req.CreatedById,
                    req.CreatedByNpp,
                    req.CreatedTime,
                    req.CreatedByUnitId,
                    req.CreatedByUnit,
                    CreatedByUID = req.CreatedByUid,
                    req.UpdatedById,
                    req.UpdatedByNpp,
                    req.UpdatedTime,
                    req.UpdatedByUnit,
                    UpdatedByUID = req.UpdatedByUid,
                    req.IsActive,
                    req.IsDeleted
                }));
            }
            catch (Exception ex)
            {

            }

            return req;
        }

        public async Task<Tbl_DataKTP_Finger_Temp> InsertFingerTemp(Tbl_DataKTP_Finger_Temp req)
        {
            try
            {
                const string query = "Insert Into [Tbl_DataKTP_Finger_Temp] (" +
                                "[NIK]," +
                                "[TypeFinger]," +
                                "[PathFile]," +
                                "[FileName]," +
                                "[PathFileISO]," +
                                "[FileNameISO]," +
                                //"[FileJari]," +
                                "[FileJariISO]," +
                                "[CreatedById]," +
                                "[CreatedByNpp]," +
                                "[CreatedTime]," +
                                "[CreatedByUnitId]," +
                                "[CreatedByUnit]," +
                                "[CreatedByUID]," +
                                "[UpdatedById]," +
                                "[UpdatedByNpp]," +
                                "[UpdatedTime]," +
                                "[UpdatedByUnit]," +
                                "[UpdatedByUID]," +
                                "[IsActive]," +
                                "[IsDeleted]," +
                                "[NoPengajuan]," +
                                "[DemografisTempId]," +
                                "[IsApprove]) " +
                            "values(" +
                                "@NIK," +
                                "@TypeFinger," +
                                "@PathFile," +
                                "@FileName," +
                                "@PathFileISO," +
                                "@FileNameISO," +
                                //"@FileJari," +
                                "@FileJariISO," +
                                "@CreatedById," +
                                "@CreatedByNpp," +
                                "@CreatedTime," +
                                "@CreatedByUnitId," +
                                "@CreatedByUnit," +
                                "@CreatedByUID," +
                                "@UpdatedById," +
                                "@UpdatedByNpp," +
                                "@UpdatedTime," +
                                "@UpdatedByUnit," +
                                "@UpdatedByUID," +
                                "@IsActive," +
                                "@IsDeleted," +
                                "@NoPengajuan," +
                                "@DemografisTempId," +
                                "@IsApprove)";

                await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
                {
                    NIK = req.Nik,
                    req.TypeFinger,
                    req.PathFile,
                    req.FileName,
                    req.PathFileISO,
                    req.FileNameISO,
                    //req.FileJari,
                    req.FileJariISO,
                    req.CreatedById,
                    req.CreatedByNpp,
                    req.CreatedTime,
                    req.CreatedByUnitId,
                    req.CreatedByUnit,
                    req.CreatedByUID,
                    req.UpdatedById,
                    req.UpdatedByNpp,
                    req.UpdatedTime,
                    req.UpdatedByUnit,
                    req.UpdatedByUID,
                    req.IsActive,
                    req.IsDeleted,
                    req.NoPengajuan,
                    req.DemografisTempId,
                    req.IsApprove
                }));
            }
            catch (Exception ex)
            {

            }

            return req;
        }

        public async Task<Tbl_DataKTP_Finger> DeleteFinger(Tbl_DataKTP_Finger req)
        {
            const string query = "delete from Tbl_DataKTP_Finger " +
                "Where Id = @Id";

            await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
            {
                req.Id
            }));

            return req;
        }

        public async Task<Tbl_DataKTP_Finger_Temp> DeleteFingerTemp(Tbl_DataKTP_Finger_Temp req)
        {
            const string query = "delete from Tbl_DataKTP_Finger_Temp " +
                "Where Id = @Id";

            await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
            {
                req.Id
            }));

            return req;
        }
        public async Task<Tbl_DataKTP_Finger_Log> InsertFingerLog(Tbl_DataKTP_Finger_Log req)
        {
            try
            {
                const string query = "Insert Into [Tbl_DataKTP_Finger_Log] (" +
                "[NIK]," +
                "[TypeFinger]," +
                "[PathFile]," +
                "[FileName]," +
                "[PathFileISO]," +
                "[FileNameISO]," +
                //"[FileJari]," +
                "[FileJariISO]," +
                "[CreatedById]," +
                "[CreatedByNpp]," +
                "[CreatedTime]," +
                "[CreatedByUnit]," +
                "[CreatedByUID]) " +
            "values(" +
                "@NIK," +
                "@TypeFinger," +
                "@PathFile," +
                "@FileName," +
                "@PathFileISO," +
                "@FileNameISO," +
                //"@FileJari," +
                "@FileJariISO," +
                "@CreatedById," +
                "@CreatedByNpp," +
                "@CreatedTime," +
                "@CreatedByUnit," +
                "@CreatedByUID)";

                await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
                {
                    NIK = req.Nik,
                    req.TypeFinger,
                    req.PathFile,
                    req.FileName,
                    req.PathFileISO,
                    req.FileNameISO,
                    //req.FileJari,
                    req.FileJariISO,
                    req.CreatedById,
                    req.CreatedByNpp,
                    req.CreatedTime,
                    req.CreatedByUnit,
                    CreatedByUID = req.CreatedByUid
                }));

            }
            catch (Exception ex)
            {

            }

            return req;
        }
        #endregion

        #region finger employee
        public async Task<Tbl_DataKTP_Finger_Employee> InsertFingerEmployee(Tbl_DataKTP_Finger_Employee req)
        {
            try
            {
                const string query = "Insert Into [Tbl_DataKTP_Finger_Employee] (" +
                "[NIK]," +
                "[TypeFinger]," +
                "[PathFile]," +
                "[FileName]," +
                "[PathFileISO]," +
                "[FileNameISO]," +
                //"[FileJari]," +
                "[FileJariISO]," +
                "[CreatedById]," +
                "[CreatedByNpp]," +
                "[CreatedTime]," +
                "[CreatedByUnitId]," +
                "[CreatedByUnit]," +
                "[CreatedByUID]," +
                "[UpdatedById]," +
                "[UpdatedByNpp]," +
                "[UpdatedTime]," +
                "[UpdatedByUnit]," +
                "[UpdatedByUID]," +
                "[IsActive]," +
                "[IsDeleted]) " +
            "values(" +
                "@NIK," +
                "@TypeFinger," +
                "@PathFile," +
                "@FileName," +
                "@PathFileISO," +
                "@FileNameISO," +
                //"@FileJari," +
                "@FileJariISO," +
                "@CreatedById," +
                "@CreatedByNpp," +
                "@CreatedTime," +
                "@CreatedByUnitId," +
                "@CreatedByUnit," +
                "@CreatedByUID," +
                "@UpdatedById," +
                "@UpdatedByNpp," +
                "@UpdatedTime," +
                "@UpdatedByUnit," +
                "@UpdatedByUID," +
                "@IsActive," +
                "@IsDeleted)";

                await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
                {
                    NIK = req.Nik,
                    req.TypeFinger,
                    req.PathFile,
                    req.FileName,
                    req.PathFileISO,
                    req.FileNameISO,
                    //req.FileJari,
                    req.FileJariISO,
                    req.CreatedById,
                    req.CreatedByNpp,
                    req.CreatedTime,
                    req.CreatedByUnit,
                    req.CreatedByUnitId,
                    CreatedByUID = req.CreatedByUid,
                    req.UpdatedById,
                    req.UpdatedByNpp,
                    req.UpdatedTime,
                    req.UpdatedByUnit,
                    UpdatedByUID = req.UpdatedByUid,
                    req.IsActive,
                    req.IsDeleted
                }));
            }
            catch (Exception ex)
            {

            }

            return req;
        }

        public async Task<Tbl_DataKTP_Finger_Employee_Temp> InsertFingerEmployeeTemp(Tbl_DataKTP_Finger_Employee_Temp req)
        {
            try
            {
                const string query = "Insert Into [Tbl_DataKTP_Finger_Employee_Temp] (" +
                "[NIK]," +
                "[TypeFinger]," +
                "[PathFile]," +
                "[FileName]," +
                "[PathFileISO]," +
                "[FileNameISO]," +
                //"[FileJari]," +
                "[FileJariISO]," +
                "[CreatedById]," +
                "[CreatedByNpp]," +
                "[CreatedTime]," +
                "[CreatedByUnitId]," +
                "[CreatedByUnit]," +
                "[CreatedByUID]," +
                "[UpdatedById]," +
                "[UpdatedByNpp]," +
                "[UpdatedTime]," +
                "[UpdatedByUnit]," +
                "[UpdatedByUID]," +
                "[IsActive]," +
                "[IsDeleted]," +
                "[NoPengajuan]," +
                "[DemografisTempId]," +
                "[IsApprove]) " +
            "values(" +
                "@NIK," +
                "@TypeFinger," +
                "@PathFile," +
                "@FileName," +
                "@PathFileISO," +
                "@FileNameISO," +
                //"@FileJari," +
                "@FileJariISO," +
                "@CreatedById," +
                "@CreatedByNpp," +
                "@CreatedTime," +
                "@CreatedByUnitId," +
                "@CreatedByUnit," +
                "@CreatedByUID," +
                "@UpdatedById," +
                "@UpdatedByNpp," +
                "@UpdatedTime," +
                "@UpdatedByUnit," +
                "@UpdatedByUID," +
                "@IsActive," +
                "@IsDeleted," +
                "@NoPengajuan," +
                "@DemografisTempId," +
                "@IsApprove)";

                await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
                {
                    NIK = req.Nik,
                    req.TypeFinger,
                    req.PathFile,
                    req.FileName,
                    req.PathFileISO,
                    req.FileNameISO,
                    //req.FileJari,
                    req.FileJariISO,
                    req.CreatedById,
                    req.CreatedByNpp,
                    req.CreatedTime,
                    req.CreatedByUnit,
                    req.CreatedByUnitId,
                    req.CreatedByUID,
                    req.UpdatedById,
                    req.UpdatedByNpp,
                    req.UpdatedTime,
                    req.UpdatedByUnit,
                    req.UpdatedByUID,
                    req.IsActive,
                    req.IsDeleted,
                    req.NoPengajuan,
                    req.DemografisTempId,
                    req.IsApprove
                }));
            }
            catch (Exception ex)
            {

            }

            return req;
        }
        public async Task<Tbl_DataKTP_Finger_Employee> DeleteFingerEmployee(Tbl_DataKTP_Finger_Employee req)
        {
            const string query = "delete from Tbl_DataKTP_Finger_Employee " +
                "Where Id = @Id";

            await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
            {
                req.Id
            }));

            return req;
        }

        public async Task<Tbl_DataKTP_Finger_Employee_Temp> DeleteFingerEmployeeTemp(Tbl_DataKTP_Finger_Employee_Temp req)
        {
            const string query = "delete from Tbl_DataKTP_Finger_Employee_Temp " +
                "Where Id = @Id";

            await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
            {
                req.Id
            }));

            return req;
        }
        public async Task<Tbl_DataKTP_Finger_Employee_Log> InsertFingerEmployeeLog(Tbl_DataKTP_Finger_Employee_Log req)
        {
            try
            {
                const string query = "Insert Into [Tbl_DataKTP_Finger_Employee_Log] (" +
                                "[NIK]," +
                                "[TypeFinger]," +
                                "[PathFile]," +
                                "[FileName]," +
                                "[PathFileISO]," +
                                "[FileNameISO]," +
                                //"[FileJari]," +
                                "[FileJariISO]," +
                                "[CreatedById]," +
                                "[CreatedByNpp]," +
                                "[CreatedTime]," +
                                "[CreatedByUnit]," +
                                "[CreatedByUID]) " +
                            "values(" +
                                "@NIK," +
                                "@TypeFinger," +
                                "@PathFile," +
                                "@FileName," +
                                "@PathFileISO," +
                                "@FileNameISO," +
                                //"@FileJari," +
                                "@FileJariISO," +
                                "@CreatedById," +
                                "@CreatedByNpp," +
                                "@CreatedTime," +
                                "@CreatedByUnit," +
                                "@CreatedByUID)";

                await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
                {
                    NIK = req.Nik,
                    req.TypeFinger,
                    req.PathFile,
                    req.FileName,
                    req.PathFileISO,
                    req.FileNameISO,
                    //req.FileJari,
                    req.FileJariISO,
                    req.CreatedById,
                    req.CreatedByNpp,
                    req.CreatedTime,
                    req.CreatedByUnit,
                    CreatedByUID = req.CreatedByUid
                }));
            }
            catch (Exception ex)
            {

            }


            return req;
        }


        #endregion

        #endregion

        public async Task<GridResponse<EnrollNoMatchingData>> GetPengajuanNoMatchingList(EnrollNoMatchingFilter filter, int currentUserId)
        {
            const string proc = "[ProcEnrollNoMatchingData]";

            var val = new
            {
                SColumn = new DbString { Value = string.IsNullOrWhiteSpace(filter.SortColumn) ? "Id" : filter.SortColumn, Length = 100 },
                SColumnValue = new DbString { Value = string.IsNullOrWhiteSpace(filter.SortColumnDir) ? "desc" : filter.SortColumnDir, Length = 10 },
                StatusPengajuan = filter.StatusPengajuan != null && filter.StatusPengajuan.Count > 0 ? string.Join(',', filter.StatusPengajuan) : "",
                ID = filter.ID.GetValueOrDefault(),
                NIK = new DbString { Value = string.IsNullOrWhiteSpace(filter.NIK) ? "" : filter.NIK, Length = 50 },
                Page = filter.PageNumber,
                Rows = filter.PageSize,
                CurrentUserId = currentUserId,
            };

            var res = await Db.WithConnectionAsync(c => c.QueryAsync<EnrollNoMatchingData>(proc, val, commandType: CommandType.StoredProcedure))
                .ConfigureAwait(false);

            const string procCount = "[ProcEnrollNoMatchingDataTotal]";

            var count = await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(procCount, new
            {
                StatusPengajuan = filter.StatusPengajuan != null && filter.StatusPengajuan.Count > 0 ? string.Join(',', filter.StatusPengajuan) : "",
                ID = filter.ID.GetValueOrDefault(),
                NIK = new DbString { Value = string.IsNullOrWhiteSpace(filter.NIK) ? "" : filter.NIK, Length = 50 },
                CurrentUserId = currentUserId,
            }, commandType: CommandType.StoredProcedure))
                .ConfigureAwait(false);

            return new GridResponse<EnrollNoMatchingData>
            {
                Count = count,
                Data = res
            };
        }

        public async Task<Tbl_Inbox_Enrollment_Temp_Detail> InsertEnrollNoMatchingLogAsync(Tbl_Inbox_Enrollment_Temp_Detail req)
        {
            const string queryNoPengajuan = "select NoPengajuan from [dbo].[Tbl_Inbox_Enrollment_Temp] where Id = @id";

            var dataNoPengajuan = await Db.WithConnectionAsync(db => db.QueryFirstOrDefaultAsync<string>(queryNoPengajuan, new { id = req.InboxEnrollmentTempId }));

            const string query = "INSERT INTO [dbo].[Tbl_Inbox_Enrollment_Temp_Detail] " +
                    "([InboxEnrollmentTempId]" +
                    ",[Notes] " +
                    ",[SubmitById]" +
                    ",[SubmitByNpp]" +
                    ",[CreatedTime]" +
                    ",[SubmitedByUnitId]" +
                    ",[SubmitedByUnitCode]" +
                    ",[ApprovedStatus]" +
                    ",[Status]" +
                    ",[NoPengajuan])" +
                "values(" +
                    "@InboxEnrollmentTempId" +
                    ",@Notes " +
                    ",@SubmitById" +
                    ",@SubmitByNpp" +
                    ",@CreatedTime" +
                    ",@SubmitedByUnitId" +
                    ",@SubmitedByUnitCode" +
                    ",@ApprovedStatus" +
                    ",@Status" +
                    ",@NoPengajuan)";

            req.NoPengajuan = dataNoPengajuan;

            int insert = await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
            {
                InboxEnrollmentTempId = req.InboxEnrollmentTempId,
                req.Notes,
                req.SubmitById,
                req.SubmitByNpp,
                CreatedTime = DateTime.Now,
                req.SubmitedByUnitId,
                req.SubmitedByUnitCode,
                req.ApprovedStatus,
                req.Status,
                req.NoPengajuan
            }));

            return CommonConverter.ConvertToDerived<Tbl_Inbox_Enrollment_Temp_Detail>(req);
        }

        public async Task UpdateEnrollNoMatchingStatusAsync(EnrollNoMatchingStatusRequest req, int updatedById, string npp,
            string unitCode, int unitId, int roleid)
        {
            const string query = "Update Tbl_Inbox_Enrollment_Temp set " +
                        "UpdatedTime = @UpdatedTime, " +
                        "UpdatedById = @UpdatedById, " +
                        "UpdatedByNpp = @UpdatedByNpp, " +
                        "UpdatedByUnitId = @UpdatedByUnitId, " +
                        "UpdatedByUnitCode = @UpdatedByUnitCode, " +
                        "ApprovedByRoleId = @ApprovedByRoleId, " +
                        "ApprovedByUnitId = @ApprovedByUnitId, " +
                        "ApprovedStatus = @ApprovedStatus, " +
                        "Status = @Status " +
                        "Where ID = @Id";

            await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
            {
                UpdatedTime = DateTime.Now,
                UpdatedById = updatedById,
                UpdatedByNpp = npp,
                UpdatedByUnitId = unitId,
                UpdatedByUnitCode = unitCode,
                ApprovedByRoleId = roleid,
                ApprovedByUnitId = unitId,
                ApprovedStatus = req.Status,
                req.Status,
                req.Id
            }));
        }

        public async Task<string> GetProbabilityDivision(string nik)
        {
            const string query = "select B.[Name] from [dbo].[Tbl_Setting_Threshold] A LEFT JOIN [dbo].[Tbl_Lookup] B ON A.Probability_Division = B.Value AND B.Type = 'TresholdValue' where A.NIK = @nik";

            return await Db.WithConnectionAsync(db => db.QueryFirstOrDefaultAsync<string>(query, new { nik }));
        }

        public async Task<string> GetProbabilityDivisionV2(string tipeTreshold)
        {
            const string query = "select CAST(A.value as NVARCHAR) from [dbo].[Tbl_Master_Treshold] A LEFT JOIN Tbl_Lookup B ON B.Value = A.tipeid AND B.[Type] = 'Treshold' where  B.[Name] = @tipeTreshold";

            return await Db.WithConnectionAsync(db => db.QueryFirstOrDefaultAsync<string>(query, new { tipeTreshold }));
        }

        public async Task<Tbl_LogError_FaceRecognition> InsertLogFacerecognition(Tbl_LogError_FaceRecognition req)
        {
            try
            {
                const string query = "Insert Into Tbl_LogError_FaceRecognition (" +
                    "payload, " +
                    "response, " +
                    "createdTime) " +
                "values(" +
                    "@payload, " +
                    "@response, " +
                    "@createdTime) ";

                _baseConnectionLog.WithConnection(c => c.ExecuteScalar<int>(query, new
                {
                    req.payload,
                    req.response,
                    req.createdTime
                }));
            }
            catch (Exception ex) { };


            return req;
        }

        public async Task MoveTempDataToMainTable(EnrollNoMatchingStatusRequest req)
        {
            try {
                const string query = "select NoPengajuan from [dbo].[Tbl_Inbox_Enrollment_Temp] where Id = @id";

                var dataNoPengajuan = await Db.WithConnectionAsync(db => db.QueryFirstOrDefaultAsync<string>(query, new { id = req.Id }));

                if (dataNoPengajuan != "")
                {

                    const string queryMoveFingerDemografis = "INSERT INTO [dbo].[Tbl_DataKTP_Demografis] ([NIK],[Nama],[TempatLahir],[TanggalLahir],[JenisKelamin]" +
                       ",[GolonganDarah],[Alamat],[RT],[RW],[Kelurahan],[Desa],[Kecamatan],[Kota],[Provinsi],[Agama],[KodePos],[StatusPerkawinan],[Pekerjaan]" +
                       ",[Kewarganegaraan],[MasaBerlaku],[Latitude],[Longitude],[AlamatLengkap],[AlamatGoogle],[CreatedById],[CreatedByNpp],[CreatedTime]" +
                       ",[CreatedByUnitId],[CreatedByUnitCode],[CreatedByUID],[UpdatedById],[UpdatedByNpp],[UpdatedTime],[UpdatedByUnitId],[UpdatedByUnitCode]" +
                       ",[UpdatedByUID],[IsActive],[IsDeleted],[CIF],[IsVerified],[IsNasabahTemp],[UpdatedCIFByBS_Time],[UpdatedCIFByBS_Username],[VerifiedByNpp],[VerifyComment],[CIFDash],[isMigrate], [isEnrollFR])" +
                       " SELECT [NIK],[Nama],[TempatLahir],[TanggalLahir],[JenisKelamin],[GolonganDarah],[Alamat],[RT],[RW],[Kelurahan],[Desa]" +
                       ",[Kecamatan],[Kota],[Provinsi],[Agama],[KodePos],[StatusPerkawinan],[Pekerjaan],[Kewarganegaraan],[MasaBerlaku],[Latitude],[Longitude]" +
                       ",[AlamatLengkap],[AlamatGoogle],[CreatedById],[CreatedByNpp],[CreatedTime],[CreatedByUnitId],[CreatedByUnitCode],[CreatedByUID]" +
                       ",[UpdatedById],[UpdatedByNpp],[UpdatedTime],[UpdatedByUnitId],[UpdatedByUnitCode],[UpdatedByUID],[IsActive],[IsDeleted],[CIF]" +
                       ",[IsVerified],[IsNasabahTemp],[UpdatedCIFByBS_Time],[UpdatedCIFByBS_Username],[VerifiedByNpp],[VerifyComment],[CIFDash],[isMigrate], 1 FROM [dbo].[Tbl_DataKTP_Demografis_Temp] where NoPengajuan = @noPengajuan";

                    await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(queryMoveFingerDemografis, new
                    {
                        noPengajuan = dataNoPengajuan
                    }));

                    const string queryDemoLog = "Insert Into [Tbl_DataKTP_Demografis_Log] (" +
                    "[NIK]," +
                    "[Nama]," +
                    "[TempatLahir]," +
                    "[TanggalLahir]," +
                    "[JenisKelamin]," +
                    "[GolonganDarah]," +
                    "[Alamat]," +
                    "[RT]," +
                    "[RW]," +
                    "[Kelurahan]," +
                    "[Desa]," +
                    "[Kecamatan]," +
                    "[Kota]," +
                    "[Provinsi]," +
                    "[Agama]," +
                    "[KodePos]," +
                    "[StatusPerkawinan]," +
                    "[Pekerjaan]," +
                    "[Kewarganegaraan]," +
                    "[MasaBerlaku]," +
                    "[Latitude]," +
                    "[Longitude]," +
                    "[AlamatLengkap]," +
                    "[AlamatGoogle]," +
                    "[CreatedById]," +
                    "[CreatedByNpp]," +
                    "[CreatedTime]," +
                    "[CreatedByUnitId]," +
                    "[CreatedByUID]," +
                    "[CIF]) " +
                    " SELECT [NIK],[Nama],[TempatLahir],[TanggalLahir],[JenisKelamin],[GolonganDarah],[Alamat],[RT],[RW],[Kelurahan],[Desa] " +
                    ",[Kecamatan],[Kota],[Provinsi],[Agama],[KodePos],[StatusPerkawinan],[Pekerjaan],[Kewarganegaraan],[MasaBerlaku],[Latitude],[Longitude] " +
                    ",[AlamatLengkap],[AlamatGoogle],[CreatedById],[CreatedByNpp],[CreatedTime],[CreatedByUnitId],[CreatedByUID],[CIF] " +
                    "FROM [dbo].[Tbl_DataKTP_Demografis_Temp] where NoPengajuan = @noPengajuan";

                    await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(queryDemoLog, new
                    {
                        noPengajuan = dataNoPengajuan
                    }));

                    const string queryMoveSignature = "INSERT INTO [dbo].[Tbl_DataKTP_Signature]" +
                        "([NIK],[PathFile],[FileName],[CreatedById],[CreatedByNpp],[CreatedTime],[CreatedByUnit],[CreatedByUID]" +
                        ",[UpdatedById],[UpdatedByNpp],[UpdatedTime],[UpdatedByUnit],[UpdatedByUID],[IsActive],[IsDeleted]) " +
                        "SELECT [NIK]" +
                        ",[PathFile],[FileName],[CreatedById],[CreatedByNpp],[CreatedTime],[CreatedByUnit],[CreatedByUID],[UpdatedById]" +
                        ",[UpdatedByNpp],[UpdatedTime],[UpdatedByUnit],[UpdatedByUID],[IsActive],[IsDeleted] " +
                        "FROM [dbo].[Tbl_DataKTP_Signature_Temp] " +
                        "where NoPengajuan = @noPengajuan";

                    await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(queryMoveSignature, new
                    {
                        noPengajuan = dataNoPengajuan
                    }));

                    const string querySignatureLog = "Insert Into [Tbl_DataKTP_Signature_Log] (" +
                    "[NIK]," +
                    "[PathFile]," +
                    "[FileName]," +
                    "[CreatedById]," +
                    "[CreatedByNpp]," +
                    "[CreatedTime]," +
                    "[CreatedByUnit]," +
                    "[CreatedByUID]) " +
                    "SELECT [NIK]" +
                    ",[PathFile],[FileName],[CreatedById],[CreatedByNpp],[CreatedTime],[CreatedByUnit],[CreatedByUID] " +
                    "FROM [dbo].[Tbl_DataKTP_Signature_Temp] " +
                    "where NoPengajuan = @noPengajuan";

                    await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(querySignatureLog, new
                    {
                        noPengajuan = dataNoPengajuan
                    }));

                    const string queryMoveCam = "INSERT INTO [dbo].[Tbl_DataKTP_PhotoCam] ([NIK],[PathFile],[FileName],[CreatedById]" +
                        ",[CreatedByNpp],[CreatedTime],[CreatedByUnit],[CreatedByUID],[UpdatedById],[UpdatedByNpp],[UpdatedTime],[UpdatedByUnit],[UpdatedByUID],[IsActive],[IsDeleted]) " +
                        "SELECT [NIK],[PathFile],[FileName],[CreatedById],[CreatedByNpp],[CreatedTime],[CreatedByUnit],[CreatedByUID],[UpdatedById],[UpdatedByNpp],[UpdatedTime]" +
                        ",[UpdatedByUnit],[UpdatedByUID],[IsActive],[IsDeleted] FROM[dbo].[Tbl_DataKTP_PhotoCam_Temp]  where NoPengajuan = @noPengajuan";

                    await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(queryMoveCam, new
                    {
                        noPengajuan = dataNoPengajuan
                    }));

                    const string queryPhotoCamLog = "Insert Into [Tbl_DataKTP_PhotoCam_Log] (" +
                    "[NIK]," +
                    "[PathFile]," +
                    "[FileName]," +
                    "[CreatedById]," +
                    "[CreatedByNpp]," +
                    "[CreatedTime]," +
                    "[CreatedByUnit]," +
                    "[CreatedByUID]) " +
                    "SELECT [NIK],[PathFile],[FileName],[CreatedById],[CreatedByNpp],[CreatedTime],[CreatedByUnit],[CreatedByUID] " +
                    "FROM[dbo].[Tbl_DataKTP_PhotoCam_Temp]  where NoPengajuan = @noPengajuan";

                    await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(queryPhotoCamLog, new
                    {
                        noPengajuan = dataNoPengajuan
                    }));

                    const string queryMovePhoto = "INSERT INTO [dbo].[Tbl_DataKTP_Photo] ([NIK],[PathFile],[FileName],[CreatedById],[CreatedByNpp],[CreatedTime]" +
                        ",[CreatedByUnit],[CreatedByUID],[UpdatedById],[UpdatedByNpp],[UpdatedTime],[UpdatedByUnit],[UpdatedByUID],[IsActive],[IsDeleted])" +
                        " SELECT[NIK],[PathFile],[FileName],[CreatedById],[CreatedByNpp],[CreatedTime],[CreatedByUnit],[CreatedByUID],[UpdatedById],[UpdatedByNpp]" +
                        ",[UpdatedTime],[UpdatedByUnit],[UpdatedByUID],[IsActive],[IsDeleted] FROM[dbo].[Tbl_DataKTP_Photo_Temp] where NoPengajuan = @noPengajuan";

                    await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(queryMovePhoto, new
                    {
                        noPengajuan = dataNoPengajuan
                    }));

                    const string queryKTPphotoLog = "Insert Into [Tbl_DataKTP_Photo_Log] (" +
                    "[NIK]," +
                    "[PathFile]," +
                    "[FileName]," +
                    "[CreatedById]," +
                    "[CreatedByNpp]," +
                    "[CreatedTime]," +
                    "[CreatedByUnit]," +
                    "[CreatedByUID]) " +
                    "SELECT [NIK],[PathFile],[FileName],[CreatedById],[CreatedByNpp],[CreatedTime],[CreatedByUnit],[CreatedByUID] " +
                    "FROM[dbo].[Tbl_DataKTP_Photo_Temp] where NoPengajuan = @noPengajuan";

                    await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(queryKTPphotoLog, new
                    {
                        noPengajuan = dataNoPengajuan
                    }));

                    const string queryMoveFinger = "INSERT INTO [dbo].[Tbl_DataKTP_Finger] ([NIK],[TypeFinger],[PathFile],[PathFileISO],[FileName],[FileNameISO],[FileJari]" +
                        ",[CreatedById],[CreatedByNpp],[CreatedTime],[CreatedByUnitId],[CreatedByUnit],[CreatedByUID],[UpdatedById],[UpdatedByNpp],[UpdatedTime],[UpdatedByUnit]" +
                        ",[UpdatedByUID],[IsActive],[IsDeleted],[FileJariISO]) " +
                        "SELECT [NIK],[TypeFinger],[PathFile],[PathFileISO],[FileName],[FileNameISO],[FileJari],[CreatedById],[CreatedByNpp],[CreatedTime],[CreatedByUnitId],[CreatedByUnit]" +
                        ",[CreatedByUID],[UpdatedById],[UpdatedByNpp],[UpdatedTime],[UpdatedByUnit],[UpdatedByUID],[IsActive],[IsDeleted],[FileJariISO] FROM [dbo].[Tbl_DataKTP_Finger_Temp] where NoPengajuan = @noPengajuan";

                    await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(queryMoveFinger, new
                    {
                        noPengajuan = dataNoPengajuan
                    }));

                    const string queryFingerLog = "Insert Into [Tbl_DataKTP_Finger_Log] (" +
                    "[NIK]," +
                    "[TypeFinger]," +
                    "[PathFile]," +
                    "[FileName]," +
                    "[PathFileISO]," +
                    "[FileNameISO]," +
                    "[FileJariISO]," +
                    "[CreatedById]," +
                    "[CreatedByNpp]," +
                    "[CreatedTime]," +
                    "[CreatedByUnit]," +
                    "[CreatedByUID]) " +
                    "SELECT [NIK],[TypeFinger],[PathFile],[FileName],[PathFileISO],[FileNameISO],[FileJariISO],[CreatedById],[CreatedByNpp],[CreatedTime],[CreatedByUnit]" +
                    ",[CreatedByUID] FROM [dbo].[Tbl_DataKTP_Finger_Temp] where NoPengajuan = @noPengajuan";

                    await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(queryFingerLog, new
                    {
                        noPengajuan = dataNoPengajuan
                    }));

                    const string queryMoveFingerEmployee = "INSERT INTO [dbo].[Tbl_DataKTP_Finger_Employee] ([NIK],[TypeFinger],[PathFile],[PathFileISO],[FileName],[FileNameISO],[FileJari]" +
                        ",[CreatedById],[CreatedByNpp],[CreatedTime],[CreatedByUnitId],[CreatedByUnit],[CreatedByUID],[UpdatedById],[UpdatedByNpp],[UpdatedTime],[UpdatedByUnit]" +
                        ",[UpdatedByUID],[IsActive],[IsDeleted],[FileJariISO])" +
                        " SELECT [NIK],[TypeFinger],[PathFile],[PathFileISO],[FileName],[FileNameISO],[FileJari],[CreatedById],[CreatedByNpp],[CreatedTime],[CreatedByUnitId]" +
                        ",[CreatedByUnit],[CreatedByUID],[UpdatedById],[UpdatedByNpp],[UpdatedTime],[UpdatedByUnit],[UpdatedByUID],[IsActive],[IsDeleted],[FileJariISO] FROM [dbo].[Tbl_DataKTP_Finger_Employee_Temp] where NoPengajuan = @noPengajuan";

                    await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(queryMoveFingerEmployee, new
                    {
                        noPengajuan = dataNoPengajuan
                    }));

                    const string queryFingerEmpLog = "Insert Into [Tbl_DataKTP_Finger_Employee_Log] (" +
                                    "[NIK]," +
                                    "[TypeFinger]," +
                                    "[PathFile]," +
                                    "[FileName]," +
                                    "[PathFileISO]," +
                                    "[FileNameISO]," +
                                    "[FileJariISO]," +
                                    "[CreatedById]," +
                                    "[CreatedByNpp]," +
                                    "[CreatedTime]," +
                                    "[CreatedByUnit]," +
                                    "[CreatedByUID]) " +
                                    "SELECT [NIK],[TypeFinger],[PathFile],[FileName],[PathFileISO],[FileNameISO],[FileJariISO],[CreatedById],[CreatedByNpp],[CreatedTime]" +
                                    ",[CreatedByUnit],[CreatedByUID] FROM [dbo].[Tbl_DataKTP_Finger_Employee_Temp] where NoPengajuan = @noPengajuan";

                    await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(queryFingerEmpLog, new
                    {
                        noPengajuan = dataNoPengajuan
                    }));

                   

                    //const string queryDeleteSignature = "DELETE FROM [dbo].[Tbl_DataKTP_Signature_Temp] where NoPengajuan = @noPengajuan";

                    //await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(queryDeleteSignature, new
                    //{
                    //    noPengajuan = dataNoPengajuan
                    //}));

                    //const string queryDeleteCam = "DELETE FROM [dbo].[Tbl_DataKTP_PhotoCam_Temp] where NoPengajuan = @noPengajuan";

                    //await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(queryDeleteCam, new
                    //{
                    //    noPengajuan = dataNoPengajuan
                    //}));

                    //const string queryDeletePhoto = "DELETE FROM [dbo].[Tbl_DataKTP_Photo_Temp] where NoPengajuan = @noPengajuan";

                    //await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(queryDeletePhoto, new
                    //{
                    //    noPengajuan = dataNoPengajuan
                    //}));

                    //const string queryDeleteFinger = "DELETE FROM [dbo].[Tbl_DataKTP_Finger_Temp] where NoPengajuan = @noPengajuan";

                    //await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(queryDeleteFinger, new
                    //{
                    //    noPengajuan = dataNoPengajuan
                    //}));

                    //const string queryDeleteFingerEmployee = "DELETE FROM [dbo].[Tbl_DataKTP_Finger_Employee_Temp] where NoPengajuan = @noPengajuan";

                    //await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(queryDeleteFingerEmployee, new
                    //{
                    //    noPengajuan = dataNoPengajuan
                    //}));

                    //const string queryDeleteFingerDemografis = "DELETE FROM [dbo].[Tbl_DataKTP_Demografis_Temp] where NoPengajuan = @noPengajuan";

                    //await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(queryDeleteFingerDemografis, new
                    //{
                    //    noPengajuan = dataNoPengajuan
                    //}));
                }
            }
            catch (Exception ex) { 
                
            }
        }

        public async Task UpdateTempDataToMainTable(EnrollNoMatchingStatusRequest req, string nik)
        {

            const string querySignatureLog = "Insert Into [Tbl_DataKTP_Signature_Log] (" +
                "[NIK]," +
                "[PathFile]," +
                "[FileName]," +
                "[CreatedById]," +
                "[CreatedByNpp]," +
                "[CreatedTime]," +
                "[CreatedByUnit]," +
                "[CreatedByUID]) " +
                "SELECT [NIK]" +
                ",[PathFile],[FileName],[CreatedById],[CreatedByNpp],[CreatedTime],[CreatedByUnit],[CreatedByUID] " +
                "FROM [dbo].[Tbl_DataKTP_Signature] " +
                "where NIK = @nik";

            await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(querySignatureLog, new
            {
                nik
            }));

            const string queryDeleteSignatureMain = "DELETE FROM [dbo].[Tbl_DataKTP_Signature] where NIK = @nik";

            await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(queryDeleteSignatureMain, new
            {
                nik
            }));

            const string queryPhotoCamLog = "Insert Into [Tbl_DataKTP_PhotoCam_Log] (" +
                "[NIK]," +
                "[PathFile]," +
                "[FileName]," +
                "[CreatedById]," +
                "[CreatedByNpp]," +
                "[CreatedTime]," +
                "[CreatedByUnit]," +
                "[CreatedByUID]) " +
                "SELECT [NIK],[PathFile],[FileName],[CreatedById],[CreatedByNpp],[CreatedTime],[CreatedByUnit],[CreatedByUID] " +
                "FROM [dbo].[Tbl_DataKTP_PhotoCam]  where NIK = @nik";

            await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(queryPhotoCamLog, new
            {
                nik
            }));

            const string queryDeleteCamMain = "DELETE FROM [dbo].[Tbl_DataKTP_PhotoCam] where NIK = @nik";

            await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(queryDeleteCamMain, new
            {
                nik
            }));

            const string queryKTPphotoLog = "Insert Into [Tbl_DataKTP_Photo_Log] (" +
                "[NIK]," +
                "[PathFile]," +
                "[FileName]," +
                "[CreatedById]," +
                "[CreatedByNpp]," +
                "[CreatedTime]," +
                "[CreatedByUnit]," +
                "[CreatedByUID]) " +
                "SELECT [NIK],[PathFile],[FileName],[CreatedById],[CreatedByNpp],[CreatedTime],[CreatedByUnit],[CreatedByUID] " +
                "FROM [dbo].[Tbl_DataKTP_Photo] where NIK = @nik";

            await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(queryKTPphotoLog, new
            {
                nik
            }));

            const string queryDeletePhotoMain = "DELETE FROM [dbo].[Tbl_DataKTP_Photo] where NIK = @nik";

            await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(queryDeletePhotoMain, new
            {
                nik
            }));

            const string queryFingerLog = "Insert Into [Tbl_DataKTP_Finger_Log] (" +
                "[NIK]," +
                "[TypeFinger]," +
                "[PathFile]," +
                "[FileName]," +
                "[PathFileISO]," +
                "[FileNameISO]," +
                "[FileJariISO]," +
                "[CreatedById]," +
                "[CreatedByNpp]," +
                "[CreatedTime]," +
                "[CreatedByUnit]," +
                "[CreatedByUID]) " +
                "SELECT [NIK],[TypeFinger],[PathFile],[FileName],[PathFileISO],[FileNameISO],[FileJariISO],[CreatedById],[CreatedByNpp],[CreatedTime],[CreatedByUnit]" +
                ",[CreatedByUID] FROM [dbo].[Tbl_DataKTP_Finger] where NIK = @nik";

            await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(queryFingerLog, new
            {
                nik
            }));

            const string queryDeleteFingerMain = "DELETE FROM [dbo].[Tbl_DataKTP_Finger] where NIK = @nik";

            await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(queryDeleteFingerMain, new
            {
                nik
            }));


            const string queryFingerEmpLog = "Insert Into [Tbl_DataKTP_Finger_Employee_Log] (" +
                                "[NIK]," +
                                "[TypeFinger]," +
                                "[PathFile]," +
                                "[FileName]," +
                                "[PathFileISO]," +
                                "[FileNameISO]," +
                                "[FileJariISO]," +
                                "[CreatedById]," +
                                "[CreatedByNpp]," +
                                "[CreatedTime]," +
                                "[CreatedByUnit]," +
                                "[CreatedByUID]) " +
                                "SELECT [NIK],[TypeFinger],[PathFile],[FileName],[PathFileISO],[FileNameISO],[FileJariISO],[CreatedById],[CreatedByNpp],[CreatedTime]" +
                                ",[CreatedByUnit],[CreatedByUID] FROM [dbo].[Tbl_DataKTP_Finger_Employee] where NIK = @nik";

            await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(queryFingerEmpLog, new
            {
                nik
            }));

            const string queryDeleteFingerEmployeeMain = "DELETE FROM [dbo].[Tbl_DataKTP_Finger_Employee] where NIK = @nik";

            await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(queryDeleteFingerEmployeeMain, new
            {
                nik
            }));

            const string queryDemoLog = "Insert Into [Tbl_DataKTP_Demografis_Log] (" +
                "[NIK]," +
                "[Nama]," +
                "[TempatLahir]," +
                "[TanggalLahir]," +
                "[JenisKelamin]," +
                "[GolonganDarah]," +
                "[Alamat]," +
                "[RT]," +
                "[RW]," +
                "[Kelurahan]," +
                "[Desa]," +
                "[Kecamatan]," +
                "[Kota]," +
                "[Provinsi]," +
                "[Agama]," +
                "[KodePos]," +
                "[StatusPerkawinan]," +
                "[Pekerjaan]," +
                "[Kewarganegaraan]," +
                "[MasaBerlaku]," +
                "[Latitude]," +
                "[Longitude]," +
                "[AlamatLengkap]," +
                "[AlamatGoogle]," +
                "[CreatedById]," +
                "[CreatedByNpp]," +
                "[CreatedTime]," +
                "[CreatedByUnitId]," +
                "[CreatedByUID]," +
                "[CIF]) " +
                " SELECT [NIK],[Nama],[TempatLahir],[TanggalLahir],[JenisKelamin],[GolonganDarah],[Alamat],[RT],[RW],[Kelurahan],[Desa] " +
                ",[Kecamatan],[Kota],[Provinsi],[Agama],[KodePos],[StatusPerkawinan],[Pekerjaan],[Kewarganegaraan],[MasaBerlaku],[Latitude],[Longitude] " +
                ",[AlamatLengkap],[AlamatGoogle],[CreatedById],[CreatedByNpp],[CreatedTime],[CreatedByUnitId],[CreatedByUID],[CIF] " +
                "FROM [dbo].[Tbl_DataKTP_Demografis] where NIK = @nik";

            await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(queryDemoLog, new
            {
                nik
            }));

            const string queryDeleteFingerDemografisMain = "DELETE FROM [dbo].[Tbl_DataKTP_Demografis] where NIK = @nik";

            await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(queryDeleteFingerDemografisMain, new
            {
                nik
            }));

            const string query = "select NoPengajuan from [dbo].[Tbl_Inbox_Enrollment_Temp] where Id = @id";

            var dataNoPengajuan = await Db.WithConnectionAsync(db => db.QueryFirstOrDefaultAsync<string>(query, new { id = req.Id }));

            if (dataNoPengajuan != "")
            {

                const string queryMoveFingerDemografis = "INSERT INTO [dbo].[Tbl_DataKTP_Demografis] ([NIK],[Nama],[TempatLahir],[TanggalLahir],[JenisKelamin]" +
                 ",[GolonganDarah],[Alamat],[RT],[RW],[Kelurahan],[Desa],[Kecamatan],[Kota],[Provinsi],[Agama],[KodePos],[StatusPerkawinan],[Pekerjaan]" +
                 ",[Kewarganegaraan],[MasaBerlaku],[Latitude],[Longitude],[AlamatLengkap],[AlamatGoogle],[CreatedById],[CreatedByNpp],[CreatedTime]" +
                 ",[CreatedByUnitId],[CreatedByUnitCode],[CreatedByUID],[UpdatedById],[UpdatedByNpp],[UpdatedTime],[UpdatedByUnitId],[UpdatedByUnitCode]" +
                 ",[UpdatedByUID],[IsActive],[IsDeleted],[CIF],[IsVerified],[IsNasabahTemp],[UpdatedCIFByBS_Time],[UpdatedCIFByBS_Username],[VerifiedByNpp],[VerifyComment],[CIFDash],[isMigrate], [isEnrollFR])" +
                 " SELECT [NIK],[Nama],[TempatLahir],[TanggalLahir],[JenisKelamin],[GolonganDarah],[Alamat],[RT],[RW],[Kelurahan],[Desa]" +
                 ",[Kecamatan],[Kota],[Provinsi],[Agama],[KodePos],[StatusPerkawinan],[Pekerjaan],[Kewarganegaraan],[MasaBerlaku],[Latitude],[Longitude]" +
                 ",[AlamatLengkap],[AlamatGoogle],[CreatedById],[CreatedByNpp],[CreatedTime],[CreatedByUnitId],[CreatedByUnitCode],[CreatedByUID]" +
                 ",[UpdatedById],[UpdatedByNpp],[UpdatedTime],[UpdatedByUnitId],[UpdatedByUnitCode],[UpdatedByUID],[IsActive],[IsDeleted],[CIF]" +
                 ",[IsVerified],[IsNasabahTemp],[UpdatedCIFByBS_Time],[UpdatedCIFByBS_Username],[VerifiedByNpp],[VerifyComment],[CIFDash],[isMigrate], 1 FROM [dbo].[Tbl_DataKTP_Demografis_Temp] where NoPengajuan = @noPengajuan";

                await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(queryMoveFingerDemografis, new
                {
                    noPengajuan = dataNoPengajuan
                }));

                const string queryMoveSignature = "INSERT INTO [dbo].[Tbl_DataKTP_Signature]" +
                    "([NIK],[PathFile],[FileName],[CreatedById],[CreatedByNpp],[CreatedTime],[CreatedByUnit],[CreatedByUID]" +
                    ",[UpdatedById],[UpdatedByNpp],[UpdatedTime],[UpdatedByUnit],[UpdatedByUID],[IsActive],[IsDeleted]) " +
                    "SELECT [NIK]" +
                    ",[PathFile],[FileName],[CreatedById],[CreatedByNpp],[CreatedTime],[CreatedByUnit],[CreatedByUID],[UpdatedById]" +
                    ",[UpdatedByNpp],[UpdatedTime],[UpdatedByUnit],[UpdatedByUID],[IsActive],[IsDeleted] " +
                    "FROM [dbo].[Tbl_DataKTP_Signature_Temp] " +
                    "where NoPengajuan = @noPengajuan";

                await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(queryMoveSignature, new
                {
                    noPengajuan = dataNoPengajuan
                }));

                const string queryMoveCam = "INSERT INTO [dbo].[Tbl_DataKTP_PhotoCam] ([NIK],[PathFile],[FileName],[CreatedById]" +
                    ",[CreatedByNpp],[CreatedTime],[CreatedByUnit],[CreatedByUID],[UpdatedById],[UpdatedByNpp],[UpdatedTime],[UpdatedByUnit],[UpdatedByUID],[IsActive],[IsDeleted]) " +
                    "SELECT [NIK],[PathFile],[FileName],[CreatedById],[CreatedByNpp],[CreatedTime],[CreatedByUnit],[CreatedByUID],[UpdatedById],[UpdatedByNpp],[UpdatedTime]" +
                    ",[UpdatedByUnit],[UpdatedByUID],[IsActive],[IsDeleted] FROM[dbo].[Tbl_DataKTP_PhotoCam_Temp]  where NoPengajuan = @noPengajuan";

                await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(queryMoveCam, new
                {
                    noPengajuan = dataNoPengajuan
                }));

                const string queryMovePhoto = "INSERT INTO [dbo].[Tbl_DataKTP_Photo] ([NIK],[PathFile],[FileName],[CreatedById],[CreatedByNpp],[CreatedTime]" +
                    ",[CreatedByUnit],[CreatedByUID],[UpdatedById],[UpdatedByNpp],[UpdatedTime],[UpdatedByUnit],[UpdatedByUID],[IsActive],[IsDeleted])" +
                    " SELECT[NIK],[PathFile],[FileName],[CreatedById],[CreatedByNpp],[CreatedTime],[CreatedByUnit],[CreatedByUID],[UpdatedById],[UpdatedByNpp]" +
                    ",[UpdatedTime],[UpdatedByUnit],[UpdatedByUID],[IsActive],[IsDeleted] FROM[dbo].[Tbl_DataKTP_Photo_Temp] where NoPengajuan = @noPengajuan";

                await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(queryMovePhoto, new
                {
                    noPengajuan = dataNoPengajuan
                }));

                const string queryMoveFinger = "INSERT INTO [dbo].[Tbl_DataKTP_Finger] ([NIK],[TypeFinger],[PathFile],[PathFileISO],[FileName],[FileNameISO],[FileJari]" +
                    ",[CreatedById],[CreatedByNpp],[CreatedTime],[CreatedByUnitId],[CreatedByUnit],[CreatedByUID],[UpdatedById],[UpdatedByNpp],[UpdatedTime],[UpdatedByUnit]" +
                    ",[UpdatedByUID],[IsActive],[IsDeleted],[FileJariISO]) " +
                    "SELECT[NIK],[TypeFinger],[PathFile],[PathFileISO],[FileName],[FileNameISO],[FileJari],[CreatedById],[CreatedByNpp],[CreatedTime],[CreatedByUnitId],[CreatedByUnit]" +
                    ",[CreatedByUID],[UpdatedById],[UpdatedByNpp],[UpdatedTime],[UpdatedByUnit],[UpdatedByUID],[IsActive],[IsDeleted],[FileJariISO] FROM[dbo].[Tbl_DataKTP_Finger_Temp] where NoPengajuan = @noPengajuan";

                await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(queryMoveFinger, new
                {
                    noPengajuan = dataNoPengajuan
                }));

                const string queryMoveFingerEmployee = "INSERT INTO [dbo].[Tbl_DataKTP_Finger_Employee] ([NIK],[TypeFinger],[PathFile],[PathFileISO],[FileName],[FileNameISO],[FileJari]" +
                    ",[CreatedById],[CreatedByNpp],[CreatedTime],[CreatedByUnitId],[CreatedByUnit],[CreatedByUID],[UpdatedById],[UpdatedByNpp],[UpdatedTime],[UpdatedByUnit]" +
                    ",[UpdatedByUID],[IsActive],[IsDeleted],[FileJariISO])" +
                    " SELECT [NIK],[TypeFinger],[PathFile],[PathFileISO],[FileName],[FileNameISO],[FileJari],[CreatedById],[CreatedByNpp],[CreatedTime],[CreatedByUnitId]" +
                    ",[CreatedByUnit],[CreatedByUID],[UpdatedById],[UpdatedByNpp],[UpdatedTime],[UpdatedByUnit],[UpdatedByUID],[IsActive],[IsDeleted],[FileJariISO] FROM [dbo].[Tbl_DataKTP_Finger_Employee_Temp] where NoPengajuan = @noPengajuan";

                await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(queryMoveFingerEmployee, new
                {
                    noPengajuan = dataNoPengajuan
                }));

             

                //const string queryDeleteSignature = "DELETE FROM [dbo].[Tbl_DataKTP_Signature_Temp] where NoPengajuan = @noPengajuan";

                //await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(queryDeleteSignature, new
                //{
                //    noPengajuan = dataNoPengajuan
                //}));

                //const string queryDeleteCam = "DELETE FROM [dbo].[Tbl_DataKTP_PhotoCam_Temp] where NoPengajuan = @noPengajuan";

                //await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(queryDeleteCam, new
                //{
                //    noPengajuan = dataNoPengajuan
                //}));

                //const string queryDeletePhoto = "DELETE FROM [dbo].[Tbl_DataKTP_Photo_Temp] where NoPengajuan = @noPengajuan";

                //await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(queryDeletePhoto, new
                //{
                //    noPengajuan = dataNoPengajuan
                //}));

                //const string queryDeleteFinger = "DELETE FROM [dbo].[Tbl_DataKTP_Finger_Temp] where NoPengajuan = @noPengajuan";

                //await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(queryDeleteFinger, new
                //{
                //    noPengajuan = dataNoPengajuan
                //}));

                //const string queryDeleteFingerEmployee = "DELETE FROM [dbo].[Tbl_DataKTP_Finger_Employee_Temp] where NoPengajuan = @noPengajuan";

                //await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(queryDeleteFingerEmployee, new
                //{
                //    noPengajuan = dataNoPengajuan
                //}));

                //const string queryDeleteFingerDemografis = "DELETE FROM [dbo].[Tbl_DataKTP_Demografis_Temp] where NoPengajuan = @noPengajuan";

                //await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(queryDeleteFingerDemografis, new
                //{
                //    noPengajuan = dataNoPengajuan
                //}));
            }
        }

        public async Task UpdateIsactiveTempTable(EnrollNoMatchingStatusRequest req)
        {
            const string query = "select NoPengajuan from [dbo].[Tbl_Inbox_Enrollment_Temp] where Id = @id";

            var dataNoPengajuan = await Db.WithConnectionAsync(db => db.QueryFirstOrDefaultAsync<string>(query, new { id = req.Id }));

            if (dataNoPengajuan != "")
            {
                const string queryMoveSignature = "UPDATE [dbo].[Tbl_DataKTP_Signature_Temp] SET [IsActive] = 0 where NoPengajuan = @noPengajuan";

                await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(queryMoveSignature, new
                {
                    noPengajuan = dataNoPengajuan
                }));

                const string queryMoveCam = "UPDATE [dbo].[Tbl_DataKTP_PhotoCam_Temp] SET [IsActive] = 0 where NoPengajuan = @noPengajuan";

                await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(queryMoveCam, new
                {
                    noPengajuan = dataNoPengajuan
                }));

                const string queryMovePhoto = "UPDATE [dbo].[Tbl_DataKTP_Photo_Temp] SET [IsActive] = 0 where NoPengajuan = @noPengajuan";

                await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(queryMovePhoto, new
                {
                    noPengajuan = dataNoPengajuan
                }));

                const string queryMoveFinger = "UPDATE [dbo].[Tbl_DataKTP_Finger_Temp] SET [IsActive] = 0 where NoPengajuan = @noPengajuan";

                await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(queryMoveFinger, new
                {
                    noPengajuan = dataNoPengajuan
                }));

                const string queryMoveFingerEmployee = "UPDATE [dbo].[Tbl_DataKTP_Finger_Employee_Temp] SET [IsActive] = 0 where NoPengajuan = @noPengajuan";

                await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(queryMoveFingerEmployee, new
                {
                    noPengajuan = dataNoPengajuan
                }));

                const string queryMoveFingerDemografis = "UPDATE [dbo].[Tbl_DataKTP_Demografis_Temp] SET [IsActive] = 0 where NoPengajuan = @noPengajuan";

                await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(queryMoveFingerDemografis, new
                {
                    noPengajuan = dataNoPengajuan
                }));
            }
        }

        public async Task<GridResponse<DataDropdownServerSide>> GetListPenyelia(int unitId)
        {
            const string proc = "[ProcSettingThrehsoldPenyelia]";

            IEnumerable<Pegawai> queryResponseList = await Db.WithConnectionAsync(db => db.QueryAsync<Pegawai>(proc, new { UnitId = unitId }, commandType: CommandType.StoredProcedure));

            var dataDropdown = new List<DataDropdownServerSide>();
            foreach (var queryResponse in queryResponseList)
            {
                dataDropdown.Add(new DataDropdownServerSide
                {
                    id = queryResponse.Id,
                    text = $"{queryResponse.NIK} - {queryResponse.Nama}",
                });
            }

            return new GridResponse<DataDropdownServerSide>
            {
                Count = dataDropdown != null ? dataDropdown.Count() : 0,
                Data = dataDropdown
            };
        }

        public async Task<GridResponse<DataDropdownServerSide>> GetListPemimpin(int unitId)
        {
            const string proc = "[ProcSettingThrehsoldPemimpin]";

            IEnumerable<Pegawai> queryResponseList = await Db.WithConnectionAsync(db => db.QueryAsync<Pegawai>(proc, new { UnitId = unitId }, commandType: CommandType.StoredProcedure));


            var dataDropdown = new List<DataDropdownServerSide>();
            foreach (var queryResponse in queryResponseList)
            {
                dataDropdown.Add(new DataDropdownServerSide
                {
                    id = queryResponse.Id,
                    text = $"{queryResponse.NIK} - {queryResponse.Nama}",
                });
            }

            return new GridResponse<DataDropdownServerSide>
            {
                Count = dataDropdown != null ? dataDropdown.Count() : 0,
                Data = dataDropdown
            };
        }

        public async Task<GridResponse<TblEnrollNoMatchingLogVM>> GetEnrollNoMatchingLogAsync(int id)
        {
            const string query = "[ProcInboxEnrollmentNoMatchingLog]";

            IEnumerable<TblEnrollNoMatchingLogVM> queryResponse = await Db.WithConnectionAsync(db => db.QueryAsync<TblEnrollNoMatchingLogVM>(query, new { id }, commandType: CommandType.StoredProcedure));

            return new GridResponse<TblEnrollNoMatchingLogVM>
            {
                Count = queryResponse != null ? queryResponse.Count() : 0,
                Data = queryResponse
            };
        }

        public async Task<bool> GetEnrollwithourFRAsync(string nik)
        {
            const string query = "select COUNT(A.Id) FROM Tbl_DataKTP_Demografis A where A.NIK = @nik AND A.isEnrollFR = 0";

            var count = await Db.WithConnectionAsync(db => db.ExecuteScalarAsync<int>(query, new { nik }));

            if (count == 0)
            {
                return false;
            }
            else {
                return true;
            }
        }

        public async Task<bool> GetEnrollStatusFRAsync(string nik)
        {
            const string query1 = "select COUNT(A.Id) FROM [Tbl_Inbox_Enrollment_Temp] A LEFT JOIN [Tbl_DataKTP_Demografis_Temp] B ON A.DemografisTempId = B.Id where B.NIK = @nik";

            var count1 = await Db.WithConnectionAsync(db => db.ExecuteScalarAsync<int>(query1, new { nik }));

            if (count1 == 0)
            {
                return true;
            }

            const string query = "select COUNT(A.Id) FROM [Tbl_Inbox_Enrollment_Temp] A LEFT JOIN [Tbl_DataKTP_Demografis_Temp] B ON A.DemografisTempId = B.Id where B.NIK = @nik AND A.Status not IN (3,4,5,6)";

            var count = await Db.WithConnectionAsync(db => db.ExecuteScalarAsync<int>(query, new { nik }));

            if (count > 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public async Task<bool> GetEnrollStatusFRAsyncForEnrollBiasa(string nik)
        {
            const string query1 = "select COUNT(A.Id) FROM [Tbl_Inbox_Enrollment_Temp] A LEFT JOIN [Tbl_DataKTP_Demografis_Temp] B ON A.DemografisTempId = B.Id where B.NIK = @nik";

            var count1 = await Db.WithConnectionAsync(db => db.ExecuteScalarAsync<int>(query1, new { nik }));

            if (count1 == 0)
            {
                return true;
            }

            const string query = "select COUNT(A.Id) FROM [Tbl_Inbox_Enrollment_Temp] A LEFT JOIN [Tbl_DataKTP_Demografis_Temp] B ON A.DemografisTempId = B.Id where B.NIK = @nik AND A.Status not IN (4,5,6)";

            var count = await Db.WithConnectionAsync(db => db.ExecuteScalarAsync<int>(query, new { nik }));

            if (count > 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public Task<Tbl_ScanIKD_Session> CheckIKDSession(string npp)
        {
            //const string query = "select * from Tbl_Mapping_Pegawai_KTP where Npp = @npp";
            const string query = "select * from Tbl_ScanIKD_Session where Npp = @npp";

            return Db.WithConnectionAsync(c => c.QueryFirstOrDefaultAsync<Tbl_ScanIKD_Session>(query, new { npp = new DbString { Value = npp, Length = 50 } }));
        }

        public async Task<Tbl_ScanIKD_Session> InsertNewIKDSession(Tbl_ScanIKD_Session req)
        {
            const string query = "Insert Into Tbl_ScanIKD_Session (" +
                "[UserId]," +
                "[npp]," +
                "[RoleId]," +
                "[UnitId]," +
                "[LastActive]," +
                "[Attempt]," +
                "[LastAttempt])" +
            "values(" +
                "@UserId," +
                "@npp," +
                "@RoleId," +
                "@UnitId," +
                "@LastActive," +
                "@Attempt," +
                "@LastAttempt)";

            await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
            {
                req.UserId,
                req.npp,
                req.RoleId,
                req.UnitId,
                req.LastActive,
                req.Attempt,
                req.LastAttempt
            }));

            return req;
        }

        public async Task<Tbl_ScanIKD_Session> UpdateIKDSession(Tbl_ScanIKD_Session req)
        {
            const string query = "Update Tbl_ScanIKD_Session Set " +
                "[UserId] = @UserId, " +
                "[npp] = @npp, " +
                "[RoleId] = @RoleId, " +
                "[UnitId] = @UnitId, " +
                "[LastActive] = @LastActive, " +
                "[Attempt] = @Attempt, " +
                "[LastAttempt] = @LastAttempt " +
                "where " +
                "[id] = @Id"
                ;

            await Db.WithConnectionAsync(c => c.ExecuteScalarAsync<int>(query, new
            {
                req.UserId,
                req.npp,
                req.RoleId,
                req.UnitId,
                req.LastActive,
                req.Attempt,
                req.LastAttempt,
                req.Id
            }));

            return req;
        }
    }
}
