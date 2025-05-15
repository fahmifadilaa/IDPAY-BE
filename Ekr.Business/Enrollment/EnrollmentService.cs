using Ekr.Business.Contracts.Enrollment;
using Ekr.Core.Configuration;
using Ekr.Core.Constant;
using Ekr.Core.Entities;
using Ekr.Core.Entities.DataEnrollment.Entity;
using Ekr.Core.Entities.DataEnrollment.ViewModel;
using Ekr.Core.Entities.DataKTP;
using Ekr.Core.Entities.DataMaster.AlatReader;
using Ekr.Core.Entities.DataMaster.DataReader.Entity;
using Ekr.Core.Entities.Enrollment;
using Ekr.Core.Entities.Logging;
using Ekr.Core.Entities.Recognition;
using Ekr.Core.Entities.ThirdParty;
using Ekr.Core.Helper;
using Ekr.Core.Securities.Symmetric;
using Ekr.Repository.Contracts.DataKTP;
using Ekr.Repository.Contracts.DataMaster.AlatReader;
using Ekr.Repository.Contracts.DataMaster.DataReader;
using Ekr.Repository.Contracts.DataMaster.SystemParameters;
using Ekr.Repository.Contracts.Enrollment;
using Ekr.Repository.Contracts.Logging;
using Ekr.Repository.Contracts.Recognition;
using Ekr.Services.Contracts.Account;
using FluentFTP.Helpers;
using Microsoft.Extensions.Options;
using NPOI.HPSF;
using NPOI.POIFS.Crypt.Dsig;
using NPOI.SS.Formula.Functions;
using Org.BouncyCastle.Ocsp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static NPOI.HSSF.Util.HSSFColor;

namespace Ekr.Business.Enrollment
{
    public class EnrollmentService : IEnrollmentService
    {
        private readonly IProfileRepository _profileRepository;
        private readonly ICIFService _cifService;
        private readonly IFingerRepository _fingerRepository;
        private readonly IEnrollmentKTPRepository _enrollmentKTPRepository;
        private readonly ISysParameterRepository _sysParameterRepository;
        private readonly IDataReaderRepository _dataReaderRepository;
        private readonly SftpConfig _sftpConfig;
        private readonly ErrorMessageConfig _ErrorMessageConfig;
        private readonly successMessageConfig _SuccessMessageConfig;
        private readonly IAlatReaderRepository _alatReaderRepository;
        public readonly OpsiAgamaConfig _OpsiAgamaConfig;
        public readonly OpsiGenderConfig _OpsiGenderConfig;
        public readonly OpsiStatusPerkawinanConfig _OpsiStatusPerkawinanConfig;
        public readonly OpsiTipeJariKananConfig _OpsiTipeJariKananConfig;
        public readonly OpsiTipeJariKiriConfig _OpsiTipeJariKiriConfig;
        private readonly OpsiTipeFileMarkerConfig _OpsiTipeFileMarkerConfig;
        private readonly OpsiGolDarahConfig _OpsiGolDarahConfig;
        private readonly OpsiKewarganegaraanConfig _OpsiKewarganegaraanConfig;
        private readonly IErrorLogRepository _errorLogRepository;

        public EnrollmentService(IProfileRepository profileRepository, ICIFService cifService,
            IEnrollmentKTPRepository enrollmentKTPRepository,
            ISysParameterRepository sysParameterRepository,
            IFingerRepository fingerRepository,
            IDataReaderRepository dataReaderRepository,
            IOptions<SftpConfig> options,
            IOptions<ErrorMessageConfig> options2,
            IOptions<successMessageConfig> options3,
            IOptions<OpsiAgamaConfig> options4,
            IOptions<OpsiGenderConfig> options5,
            IOptions<OpsiStatusPerkawinanConfig> options6,
            IOptions<OpsiTipeJariKananConfig> options7,
            IOptions<OpsiTipeJariKiriConfig> options8,
            IOptions<OpsiTipeFileMarkerConfig> options9,
            IOptions<OpsiGolDarahConfig> options10,
            IOptions<OpsiKewarganegaraanConfig> options11,
            IAlatReaderRepository alatReaderRepository,
            IErrorLogRepository errorLogRepository
            )
        {
            _profileRepository = profileRepository;
            _fingerRepository = fingerRepository;
            _cifService = cifService;
            _enrollmentKTPRepository = enrollmentKTPRepository;
            _sysParameterRepository = sysParameterRepository;
            _dataReaderRepository = dataReaderRepository;
            _sftpConfig = options.Value;
            _ErrorMessageConfig = options2.Value;
            _SuccessMessageConfig = options3.Value;
            _OpsiAgamaConfig = options4.Value;
            _OpsiGenderConfig = options5.Value;
            _OpsiStatusPerkawinanConfig = options6.Value;
            _OpsiTipeJariKananConfig = options7.Value;
            _OpsiTipeJariKiriConfig = options8.Value;
            _OpsiTipeFileMarkerConfig = options9.Value;
            _OpsiGolDarahConfig = options10.Value;
            _OpsiKewarganegaraanConfig = options11.Value;
            _alatReaderRepository = alatReaderRepository;
            _errorLogRepository = errorLogRepository;
        }

        public async Task<(string msg, int code, string cif)> ReSubmitEnrollment(bool isHitSOA, ApiSOA ReqSoa, EnrollKTP enroll, int Id, string npp,
            string unitCode, int unitId)
        {
            #region check data is employee or not
            bool isEmployee = false;
            var _empData = await _enrollmentKTPRepository.IsEmployee(enroll.KtpNIK);
            if (_empData != null)
            {
                isEmployee = true;
            }
            #endregion

            #region check UID
            var dataReader = await _dataReaderRepository.GetDatareaderUid(enroll.UID).ConfigureAwait(false);

            var dataReaderLog = new Tbl_MasterAlatReaderLog();
            var dataReaderActivityLog = new Tbl_MasterAlatReaderLogActvity();

            if (dataReader != null)
            {
                dataReaderLog = new Tbl_MasterAlatReaderLog
                {
                    CreatedBy_Id = Id,
                    CreatedTime = DateTime.Now,
                    IsActive = true,
                    IsDeleted = false,
                    Nik = enroll.KtpNIK,
                    PegawaiId = Id,
                    Serial_Number = dataReader.SN_Unit,
                    Type = "Updates Enroll",
                    Uid = enroll.UID
                };

                dataReaderActivityLog = new Tbl_MasterAlatReaderLogActvity
                {
                    CreatedBy_Id = Id,
                    IsActive = true,
                    IsDeleted = false,
                    KodeUnit = unitCode,
                    LastIP = enroll.IpAddress,
                    NIK = enroll.KtpNIK,
                    CreatedTime = DateTime.Now,
                    NppPegawai = npp,
                    PegawaiId = Id,
                    Type = "Updates Enroll",
                    UID = enroll.UID,
                    UnitId = unitId
                };
            }
            #endregion

            string JamServer = DateTime.Now.ToString("ddMMyyyyHHmmss");

            if (enroll.KtpTanggalLahir != null)
            {
                enroll.KtpTanggalLahir = enroll.KtpTanggalLahir.Replace(".", "");
                if (enroll.KtpTanggalLahir.Contains("/"))
                {
                    enroll.KtpTanggalLahir = enroll.KtpTanggalLahir.Replace("/", "-");
                }
                string[] arrayTanggl = enroll.KtpTanggalLahir.Split("-");
                string hari = "";
                string bulan = "";
                string tahun = "";

                if (arrayTanggl != null)
                {
                    if (arrayTanggl[0] != null)
                    {
                        if (arrayTanggl[0].Length != 2)
                        {
                            hari = "0" + arrayTanggl[0];

                        }
                        else
                        {
                            hari = arrayTanggl[0];

                        }
                    }

                    if (arrayTanggl[1] != null)
                    {
                        if (arrayTanggl[1].Length != 2)
                        {
                            bulan = "0" + arrayTanggl[1];

                        }
                        else
                        {
                            bulan = arrayTanggl[1];

                        }
                    }

                    if (arrayTanggl[2] != null)
                    {
                        tahun = arrayTanggl[2];
                    }
                }

                enroll.KtpTanggalLahir = hari + "-" + bulan + "-" + tahun;
            }

            var cekKTP = await _profileRepository.GetDataDemografis(enroll.KtpNIK)
                .ConfigureAwait(false);

            var dataNpp = new Tbl_Mapping_Pegawai_KTP();

            #region update data ktp
            if (cekKTP != null)
            {
                #region update demografi
                var logDemografi = new Tbl_DataKTP_Demografis_Log
                {
                    Agama = cekKTP.Agama,
                    Alamat = cekKTP.Alamat,
                    AlamatGoogle = cekKTP.AlamatGoogle,
                    AlamatLengkap = cekKTP.AlamatLengkap,
                    CreatedById = cekKTP.CreatedById,
                    CreatedByUID = cekKTP.CreatedByUID,
                    CreatedByUnitId = cekKTP.CreatedByUnitId,
                    CreatedTime = cekKTP.CreatedTime,
                    Desa = cekKTP.Desa,
                    GolonganDarah = cekKTP.GolonganDarah,
                    JenisKelamin = cekKTP.JenisKelamin,
                    Kecamatan = cekKTP.Kecamatan,
                    Kelurahan = cekKTP.Kelurahan,
                    Kewarganegaraan = cekKTP.Kewarganegaraan,
                    KodePos = cekKTP.KodePos,
                    Kota = cekKTP.Kota,
                    Latitude = cekKTP.Latitude,
                    Longitude = cekKTP.Longitude,
                    MasaBerlaku = cekKTP.MasaBerlaku,
                    Nama = cekKTP.Nama,
                    NIK = cekKTP.NIK,
                    Pekerjaan = cekKTP.Pekerjaan,
                    Provinsi = cekKTP.Provinsi,
                    RT = cekKTP.RT,
                    RW = cekKTP.RW,
                    StatusPerkawinan = cekKTP.StatusPerkawinan,
                    TanggalLahir = cekKTP.TanggalLahir,
                    TempatLahir = cekKTP.TempatLahir,
                    CIF = cekKTP.CIF,
                    CreatedByNpp = cekKTP.CreatedByNpp
                };

                var stringPerubahan = "";

                if (cekKTP.Nama != enroll.KtpNama)
                {
                    stringPerubahan = stringPerubahan + "NAMA : " + cekKTP.Nama + " -> " + enroll.KtpNama + " <br/>";
                    cekKTP.Nama = enroll.KtpNama;
                }

                if (cekKTP.TempatLahir != enroll.KtpTempatLahir)
                {
                    stringPerubahan = stringPerubahan + "TEMPAT LAHIR : " + cekKTP.TempatLahir + " -> " + enroll.KtpTempatLahir + " <br/>";
                    cekKTP.TempatLahir = enroll.KtpTempatLahir;
                }

                if (enroll.KtpTanggalLahir != null)
                {
                    var _ttl = DateTime.ParseExact(enroll.KtpTanggalLahir, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                    if (cekKTP.TanggalLahir != _ttl)
                    {
                        stringPerubahan = stringPerubahan + "TANGGAL LAHIR : " + cekKTP.TanggalLahir + " -> " + enroll.KtpTanggalLahir + " <br/>";
                        cekKTP.TanggalLahir = DateTime.ParseExact(enroll.KtpTanggalLahir, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                    }
                }

                if (cekKTP.JenisKelamin != enroll.KtpJanisKelamin)
                {
                    stringPerubahan = stringPerubahan + "JENIS KELAMIN : " + cekKTP.JenisKelamin + " -> " + enroll.KtpJanisKelamin + " <br/>";
                    cekKTP.JenisKelamin = enroll.KtpJanisKelamin;
                }

                if (cekKTP.GolonganDarah != enroll.KtpGolonganDarah)
                {
                    stringPerubahan = stringPerubahan + "GOLONGAN DARAH : " + cekKTP.GolonganDarah + " -> " + enroll.KtpGolonganDarah + " <br/>";
                    cekKTP.GolonganDarah = enroll.KtpGolonganDarah;
                }

                if (cekKTP.Alamat != enroll.KtpAlamat)
                {
                    stringPerubahan = stringPerubahan + "ALAMAT : " + cekKTP.Alamat + " -> " + enroll.KtpAlamat + " <br/>";
                    cekKTP.Alamat = enroll.KtpAlamat;
                }

                if (cekKTP.RT != enroll.KtpRT)
                {
                    stringPerubahan = stringPerubahan + "RT : " + cekKTP.RT + " -> " + enroll.KtpRT + " <br/>";
                    cekKTP.RT = enroll.KtpRT;
                }

                if (cekKTP.RW != enroll.KtpRW)
                {
                    stringPerubahan = stringPerubahan + "RW : " + cekKTP.RW + " -> " + enroll.KtpRW + " <br/>";
                    cekKTP.RW = enroll.KtpRW;
                }

                if (cekKTP.Kelurahan != enroll.KtpKelurahan)
                {
                    stringPerubahan = stringPerubahan + "KELURAHAN : " + cekKTP.Kelurahan + " -> " + enroll.KtpKelurahan + " <br/>";
                    cekKTP.Kelurahan = enroll.KtpKelurahan;
                }

                if (cekKTP.Kecamatan != enroll.KtpKecamatan)
                {
                    stringPerubahan = stringPerubahan + "KECAMATAN : " + cekKTP.Kecamatan + " -> " + enroll.KtpKecamatan + " <br/>";
                    cekKTP.Kecamatan = enroll.KtpKecamatan;
                }

                if (cekKTP.Kota != enroll.KtpKota)
                {
                    stringPerubahan = stringPerubahan + "KOTA/KABUPATEN : " + cekKTP.Kota + " -> " + enroll.KtpKota + " <br/>";
                    cekKTP.Kota = enroll.KtpKota;
                }

                if (cekKTP.Provinsi != enroll.KtpProvinsi)
                {
                    stringPerubahan = stringPerubahan + "PROVINSI  : " + cekKTP.Provinsi + " -> " + enroll.KtpProvinsi + " <br/>";
                    cekKTP.Provinsi = enroll.KtpProvinsi;
                }

                if (cekKTP.Agama != enroll.KtpAgama)
                {
                    stringPerubahan = stringPerubahan + "AGAMA  : " + cekKTP.Agama + " -> " + enroll.KtpAgama + " <br/>";
                    cekKTP.Agama = enroll.KtpAgama;
                }

                if (cekKTP.StatusPerkawinan != enroll.KtpStatusPerkawinan)
                {
                    stringPerubahan = stringPerubahan + "STATUS PERKAWINAN  : " + cekKTP.Agama + " -> " + enroll.KtpAgama + " <br/>";
                    cekKTP.StatusPerkawinan = enroll.KtpStatusPerkawinan;
                }

                if (cekKTP.Pekerjaan != enroll.KtpPekerjaan)
                {
                    stringPerubahan = stringPerubahan + "PEKERJAAN  : " + cekKTP.Agama + " -> " + enroll.KtpAgama + " <br/>";
                    cekKTP.Pekerjaan = enroll.KtpPekerjaan;
                }

                if (cekKTP.Kewarganegaraan != enroll.KtpKewarganegaraan)
                {
                    stringPerubahan = stringPerubahan + "KEWARGANEGARAAN  : " + cekKTP.Kewarganegaraan + " -> " + enroll.KtpKewarganegaraan + " <br/>";
                    cekKTP.Kewarganegaraan = enroll.KtpKewarganegaraan;
                }
                if (cekKTP.MasaBerlaku != enroll.KtpMasaBerlaku)
                {
                    stringPerubahan = stringPerubahan + "MASA BERLAKU  : " + cekKTP.MasaBerlaku + " -> " + enroll.KtpMasaBerlaku + " <br/>";
                    cekKTP.MasaBerlaku = enroll.KtpMasaBerlaku;
                }

                cekKTP.UpdatedTime = DateTime.Now;
                cekKTP.UpdatedById = Id;
                cekKTP.UpdatedByUID = enroll.UID;
                cekKTP.UpdatedByUnitCode = unitCode;
                cekKTP.UpdatedByUnitId = unitId;
                cekKTP.UpdatedByNpp = npp;

                if (string.IsNullOrWhiteSpace(stringPerubahan))
                {
                    stringPerubahan = stringPerubahan + "Data Demografi tidak ada perubahan" + " <br/>";
                }

                if (isEmployee)
                {
                    dataNpp = new Tbl_Mapping_Pegawai_KTP
                    {
                        CreatedById = Id,
                        NIK = enroll.KtpNIK,
                        Npp = _empData.Npp,
                        CreatedByNpp = npp,
                        CreatedByUID = enroll.UID,
                        CreatedTime = DateTime.Now
                    };
                }
                #endregion

                var sysPathFolder = await _sysParameterRepository.GetPathFolder("PathFolderData");
                string pathFolder = sysPathFolder.Value;

                var photoKtpLog = new Tbl_DataKTP_Photo_Log();
                var photoKTPData = await _profileRepository.GetPhotoKtp(enroll.KtpNIK)
                        .ConfigureAwait(false);
                var exPhotoKtp = photoKTPData;

                #region update Photo KTP
                if (!string.IsNullOrWhiteSpace(enroll.KtpPhotoKTP) && photoKTPData != null)
                {
                    string imageEncrypted = enroll.KtpPhotoKTP.Encrypt(Phrase.FileEncryption);

                    string filePath = "";

                    var systemParameterPath = await _sysParameterRepository.GetPathFolder("FolderPhoto")
                                                .ConfigureAwait(false);
                    string pathFolderFoto = systemParameterPath.Value;

                    string subPathFolderFoto = pathFolder + "/" + pathFolderFoto + "/" + enroll.KtpNIK + "/";

                    string fileName = "Foto_" + enroll.KtpNIK + "_" + JamServer + ".txt";

                    if (_sftpConfig.IsActive)
                    {
                        using var stream = new MemoryStream(Encoding.ASCII.GetBytes(imageEncrypted));

                        (var fname, var fPath) = await stream.UploadToFTPServer(subPathFolderFoto, fileName, _sftpConfig.Host,
                            _sftpConfig.Username, _sftpConfig.Password, _sftpConfig.RootDirectory, _sftpConfig.Url).ConfigureAwait(false);

                        filePath = fPath;
                    }
                    else
                    {
                        if (!Directory.Exists(subPathFolderFoto))
                        {
                            Directory.CreateDirectory(subPathFolderFoto);
                        }

                        filePath = subPathFolderFoto + fileName;
                        File.WriteAllText(filePath, imageEncrypted);
                    }

                    photoKtpLog.CreatedById = photoKTPData.CreatedById;
                    photoKtpLog.CreatedByNpp = photoKTPData.CreatedByNpp;
                    photoKtpLog.CreatedByUid = photoKTPData.CreatedByUid;
                    photoKtpLog.CreatedByUnit = photoKTPData.CreatedByUnit;
                    photoKtpLog.CreatedTime = photoKTPData.CreatedTime;
                    photoKtpLog.FileName = photoKTPData.FileName;
                    photoKtpLog.Nik = photoKTPData.Nik;
                    photoKtpLog.PathFile = photoKTPData.PathFile;

                    photoKTPData.PathFile = filePath;
                    photoKTPData.Nik = enroll.KtpNIK;
                    photoKTPData.FileName = fileName;
                    photoKTPData.IsActive = true;
                    photoKTPData.IsDeleted = false;
                    photoKTPData.UpdatedById = Id;
                    photoKTPData.UpdatedByNpp = npp;
                    photoKTPData.UpdatedByUid = enroll.UID;
                    photoKTPData.UpdatedTime = DateTime.Now;
                }
                else
                {
                    stringPerubahan = stringPerubahan + "Data Photo Tidak Ditemukan" + " <br/>";
                }
                #endregion

                var photoSignatureLog = new Tbl_DataKTP_Signature_Log();

                var photoSignatureData = await _profileRepository.GetPhotoSignature(enroll.KtpNIK)
                        .ConfigureAwait(false);
                var exphotoSignatureData = photoSignatureData;

                #region update signature
                if (!string.IsNullOrWhiteSpace(enroll.KtpSignature) && photoSignatureData != null)
                {
                    string imageEncrypted = enroll.KtpSignature.Encrypt(Phrase.FileEncryption);

                    string filePath = "";

                    var systemParameterPath = await _sysParameterRepository.GetPathFolder("FolderSignature")
                                                .ConfigureAwait(false);
                    string pathFolderFoto = systemParameterPath.Value;

                    string subPathFolderSignature = pathFolder + "/" + pathFolderFoto + "/" + enroll.KtpNIK + "/";

                    string fileName = "Signature_" + enroll.KtpNIK + "_" + JamServer + ".txt";

                    if (_sftpConfig.IsActive)
                    {
                        using var stream = new MemoryStream(Encoding.ASCII.GetBytes(imageEncrypted));

                        (var fname, var fPath) = await stream.UploadToFTPServer(subPathFolderSignature, fileName, _sftpConfig.Host,
                            _sftpConfig.Username, _sftpConfig.Password, _sftpConfig.RootDirectory, _sftpConfig.Url).ConfigureAwait(false);

                        filePath = fPath;
                    }
                    else
                    {
                        if (!Directory.Exists(subPathFolderSignature))
                        {
                            Directory.CreateDirectory(subPathFolderSignature);
                        }

                        filePath = subPathFolderSignature + fileName;
                        File.WriteAllText(filePath, imageEncrypted);
                    }

                    if (photoSignatureData != null)
                    {
                        photoSignatureLog.CreatedById = photoSignatureData.CreatedById;
                        photoSignatureLog.CreatedByNpp = photoSignatureData.CreatedByNpp;
                        photoSignatureLog.CreatedByUid = photoSignatureData.CreatedByUid;
                        photoSignatureLog.CreatedByUnit = photoSignatureData.CreatedByUnit;
                        photoSignatureLog.CreatedTime = photoSignatureData.CreatedTime;
                        photoSignatureLog.FileName = photoSignatureData.FileName;
                        photoSignatureLog.Nik = photoSignatureData.Nik;
                        photoSignatureLog.PathFile = photoSignatureData.PathFile;
                    }

                    photoSignatureData.UpdatedById = Id;
                    photoSignatureData.UpdatedByNpp = npp;
                    photoSignatureData.UpdatedByUid = enroll.UID;
                    photoSignatureData.UpdatedTime = DateTime.Now;
                    photoSignatureData.IsActive = true;
                    photoSignatureData.IsDeleted = false;
                    photoSignatureData.Nik = enroll.KtpNIK;
                    photoSignatureData.FileName = fileName;
                    photoSignatureData.PathFile = filePath;
                }
                else
                {
                    stringPerubahan = stringPerubahan + "Data Signature Tidak Ditemukan" + " <br/>";
                }
                #endregion

                var photoCamLog = new Tbl_DataKTP_PhotoCam_Log();
                var photoCamData = await _profileRepository.GetPhotoCam(enroll.KtpNIK)
                        .ConfigureAwait(false);
                var exphotoCamData = photoCamData;

                #region update photo cam
                if (!string.IsNullOrWhiteSpace(enroll.KtpPhotoCam) && photoCamData != null)
                {
                    string imageEncrypted = enroll.KtpPhotoCam.Encrypt(Phrase.FileEncryption);

                    string filePath = "";

                    var systemParameterPath = await _sysParameterRepository.GetPathFolder("FolderWebcam")
                                                .ConfigureAwait(false);
                    string pathFolderFoto = systemParameterPath.Value;

                    string SubPathFolderPhotoCam = pathFolder + "/" + pathFolderFoto + "/" + enroll.KtpNIK + "/";

                    string fileName = "PhotoCam_" + enroll.KtpNIK + "_" + JamServer + ".txt";

                    if (_sftpConfig.IsActive)
                    {
                        using var stream = new MemoryStream(Encoding.ASCII.GetBytes(imageEncrypted));

                        (var fname, var fPath) = await stream.UploadToFTPServer(SubPathFolderPhotoCam, fileName, _sftpConfig.Host,
                            _sftpConfig.Username, _sftpConfig.Password, _sftpConfig.RootDirectory, _sftpConfig.Url).ConfigureAwait(false);

                        filePath = fPath;
                    }
                    else
                    {
                        if (!Directory.Exists(SubPathFolderPhotoCam))
                        {
                            Directory.CreateDirectory(SubPathFolderPhotoCam);
                        }

                        filePath = SubPathFolderPhotoCam + fileName;
                        File.WriteAllText(filePath, imageEncrypted);
                    }
                    photoCamLog.CreatedById = photoCamData.CreatedById;
                    photoCamLog.CreatedByNpp = photoCamData.CreatedByNpp;
                    photoCamLog.CreatedByUid = photoCamData.CreatedByUid;
                    photoCamLog.CreatedByUnit = photoCamData.CreatedByUnit;
                    photoCamLog.CreatedTime = photoCamData.CreatedTime;
                    photoCamLog.FileName = photoCamData.FileName;
                    photoCamLog.Nik = photoCamData.Nik;
                    photoCamLog.PathFile = photoCamData.PathFile;

                    photoCamData.PathFile = filePath;
                    photoCamData.Nik = enroll.KtpNIK;
                    photoCamData.FileName = fileName;
                    photoCamData.IsActive = true;
                    photoCamData.IsDeleted = false;
                    photoCamData.UpdatedById = Id;
                    photoCamData.UpdatedByNpp = npp;
                    photoCamData.UpdatedByUid = enroll.UID;
                    photoCamData.UpdatedTime = DateTime.Now;
                }
                #endregion

                var exPhotoFinger = new List<Tbl_DataKTP_Finger>();
                var exPhotoFingerEmployee = new List<Tbl_DataKTP_Finger_Employee>();
                var photoFingerLogs = new List<Tbl_DataKTP_Finger_Log>();
                var photoFingers = new List<Tbl_DataKTP_Finger>();
                var photoFingersEmployee = new List<Tbl_DataKTP_Finger_Employee>();
                var photoFingersEmployeeLogs = new List<Tbl_DataKTP_Finger_Employee_Log>();

                #region finger
                if (!string.IsNullOrWhiteSpace(enroll.KtpFingerKanan))
                {
                    //var photoFingerData = await _profileRepository.GetPhotoFinger(enroll.KtpNIK, enroll.KtpTypeJariKanan)
                    //    .ConfigureAwait(false);

                    var photoFingerDataEmployee = new Tbl_DataKTP_Finger_Employee();


                    string imageEncrypted = enroll.KtpFingerKanan.Encrypt(Phrase.FileEncryption);

                    string filePath = "";

                    var systemParameterPath = await _sysParameterRepository.GetPathFolder("FolderFinger")
                                                .ConfigureAwait(false);
                    string pathFolderFoto = systemParameterPath.Value;

                    string subPathFolderPhotoFinger = pathFolder + "/" + pathFolderFoto + "/" + enroll.KtpNIK + "/";

                    string JenisJari = enroll.KtpTypeJariKanan.Replace(" ", "");
                    string fileName = "PhotoFinger_" + JenisJari + "_" + enroll.KtpNIK + "_" + JamServer + ".txt";

                    if (_sftpConfig.IsActive)
                    {
                        using var stream = new MemoryStream(Encoding.ASCII.GetBytes(imageEncrypted));

                        (var fname, var fPath) = await stream.UploadToFTPServer(subPathFolderPhotoFinger, fileName, _sftpConfig.Host,
                            _sftpConfig.Username, _sftpConfig.Password, _sftpConfig.RootDirectory, _sftpConfig.Url).ConfigureAwait(false);

                        filePath = fPath;
                    }
                    else
                    {
                        if (!Directory.Exists(subPathFolderPhotoFinger))
                        {
                            Directory.CreateDirectory(subPathFolderPhotoFinger);
                        }

                        filePath = subPathFolderPhotoFinger + fileName;
                        File.WriteAllText(filePath, imageEncrypted);
                    }

                    var typeFingerMain = await _fingerRepository.GetFingersEnrolled(enroll.KtpNIK);

                    foreach (var i in typeFingerMain)
                    {

                        if (i.TypeFinger.Contains("Kanan"))
                        {
                            var photoFingerData = await _profileRepository.GetPhotoFinger(enroll.KtpNIK, i.TypeFinger).ConfigureAwait(false);

                            if (isEmployee)
                            {
                                photoFingerDataEmployee = await _profileRepository.GetPhotoFingerEmployee(enroll.KtpNIK, i.TypeFinger)
                                .ConfigureAwait(false);
                            }

                            if (photoFingerData != null)
                            {
                                photoFingerLogs.Add(new Tbl_DataKTP_Finger_Log
                                {
                                    CreatedById = photoFingerData.CreatedById,
                                    CreatedByNpp = photoFingerData.CreatedByNpp,
                                    CreatedByUid = photoFingerData.CreatedByUid,
                                    CreatedByUnit = photoFingerData.CreatedByUnit,
                                    CreatedByUnitId = photoFingerData.CreatedByUnitId,
                                    CreatedTime = photoFingerData.CreatedTime,
                                    //FileJari = photoFingerData.FileJari,
                                    FileName = photoFingerData.FileName,
                                    Nik = photoFingerData.Nik,
                                    PathFile = photoFingerData.PathFile,
                                    TypeFinger = photoFingerData.TypeFinger
                                });

                                exPhotoFinger.Add(photoFingerData);

                                photoFingers.Add(new Tbl_DataKTP_Finger
                                {
                                    CreatedById = photoFingerData.CreatedById,
                                    IsActive = true,
                                    IsDeleted = false,
                                    Nik = enroll.KtpNIK,
                                    PathFile = filePath,
                                    CreatedByNpp = photoFingerData.CreatedByNpp,
                                    CreatedByUid = photoFingerData.CreatedByUid,
                                    CreatedTime = photoFingerData.CreatedTime,
                                    UpdatedById = Id,
                                    UpdatedByNpp = npp,
                                    UpdatedByUid = enroll.UID,
                                    UpdatedByUnit = unitCode,
                                    CreatedByUnitId = photoFingerData.CreatedByUnitId,
                                    CreatedByUnit = photoFingerData.CreatedByUnit,
                                    //FileJari = photoFingerData.FileJari,
                                    UpdatedTime = DateTime.Now,
                                    FileName = fileName,
                                    TypeFinger = enroll.KtpTypeJariKanan
                                });
                            }

                            if (isEmployee)
                            {
                                if (photoFingerDataEmployee == null)
                                {
                                    photoFingersEmployeeLogs.Add(new Tbl_DataKTP_Finger_Employee_Log
                                    {
                                        CreatedById = photoFingerDataEmployee.CreatedById,
                                        CreatedByNpp = photoFingerDataEmployee.CreatedByNpp,
                                        CreatedByUid = photoFingerDataEmployee.CreatedByUid,
                                        CreatedByUnit = photoFingerDataEmployee.CreatedByUnit,
                                        CreatedByUnitId = photoFingerDataEmployee.CreatedByUnitId,
                                        CreatedTime = photoFingerDataEmployee.CreatedTime,
                                        //FileJari = photoFingerDataEmployee.FileJari,
                                        FileName = photoFingerDataEmployee.FileName,
                                        Nik = photoFingerDataEmployee.Nik,
                                        PathFile = photoFingerDataEmployee.PathFile,
                                        TypeFinger = photoFingerDataEmployee.TypeFinger
                                    });
                                };

                                exPhotoFingerEmployee.Add(photoFingerDataEmployee);

                                photoFingersEmployee.Add(new Tbl_DataKTP_Finger_Employee
                                {
                                    CreatedById = photoFingerDataEmployee.CreatedById,
                                    IsActive = true,
                                    IsDeleted = false,
                                    Nik = enroll.KtpNIK,
                                    PathFile = filePath,
                                    CreatedByNpp = photoFingerDataEmployee.CreatedByNpp,
                                    CreatedByUid = photoFingerDataEmployee.CreatedByUid,
                                    CreatedTime = photoFingerDataEmployee.CreatedTime,
                                    UpdatedById = Id,
                                    UpdatedByNpp = npp,
                                    UpdatedByUid = enroll.UID,
                                    UpdatedByUnit = unitCode,
                                    CreatedByUnitId = photoFingerDataEmployee.CreatedByUnitId,
                                    CreatedByUnit = photoFingerDataEmployee.CreatedByUnit,
                                    //FileJari = photoFingerDataEmployee.FileJari,
                                    UpdatedTime = DateTime.Now,
                                    FileName = fileName,
                                    TypeFinger = enroll.KtpTypeJariKanan
                                });
                            }


                        }
                    }
                }

                if (!string.IsNullOrWhiteSpace(enroll.KtpFingerKiri))
                {
                    //var photoFingerData = await _profileRepository.GetPhotoFinger(enroll.KtpNIK, enroll.KtpTypeJariKiri)
                    //    .ConfigureAwait(false);

                    var photoFingerDataEmployee = new Tbl_DataKTP_Finger_Employee();

                    //if (isEmployee)
                    //{
                    //    photoFingerDataEmployee = await _profileRepository.GetPhotoFingerEmployee(enroll.KtpNIK, enroll.KtpTypeJariKiri)
                    //    .ConfigureAwait(false);
                    //}

                    string imageEncrypted = enroll.KtpFingerKiri.Encrypt(Phrase.FileEncryption);

                    string filePath = "";

                    var systemParameterPath = await _sysParameterRepository.GetPathFolder("FolderFinger")
                                                .ConfigureAwait(false);
                    string pathFolderFoto = systemParameterPath.Value;

                    string subPathFolderPhotoFinger = pathFolder + "/" + pathFolderFoto + "/" + enroll.KtpNIK + "/";

                    string JenisJari = enroll.KtpTypeJariKiri.Replace(" ", "");
                    string fileName = "PhotoFinger_" + JenisJari + "_" + enroll.KtpNIK + "_" + JamServer + ".txt";

                    if (_sftpConfig.IsActive)
                    {
                        using var stream = new MemoryStream(Encoding.ASCII.GetBytes(imageEncrypted));

                        (var fname, var fPath) = await stream.UploadToFTPServer(subPathFolderPhotoFinger, fileName, _sftpConfig.Host,
                            _sftpConfig.Username, _sftpConfig.Password, _sftpConfig.RootDirectory, _sftpConfig.Url).ConfigureAwait(false);

                        filePath = fPath;
                    }
                    else
                    {
                        if (!Directory.Exists(subPathFolderPhotoFinger))
                        {
                            Directory.CreateDirectory(subPathFolderPhotoFinger);
                        }

                        filePath = subPathFolderPhotoFinger + fileName;
                        File.WriteAllText(filePath, imageEncrypted);
                    }

                    var typeFingerMain = await _fingerRepository.GetFingersEnrolled(enroll.KtpNIK);

                    foreach (var i in typeFingerMain)
                    {

                        if (i.TypeFinger.Contains("Kiri"))
                        {
                            var photoFingerData = await _profileRepository.GetPhotoFinger(enroll.KtpNIK, i.TypeFinger).ConfigureAwait(false);

                            if (isEmployee)
                            {
                                photoFingerDataEmployee = await _profileRepository.GetPhotoFingerEmployee(enroll.KtpNIK, i.TypeFinger)
                                .ConfigureAwait(false);
                            }

                            if (photoFingerData != null)
                            {
                                photoFingerLogs.Add(new Tbl_DataKTP_Finger_Log
                                {
                                    CreatedById = photoFingerData.CreatedById,
                                    CreatedByNpp = photoFingerData.CreatedByNpp,
                                    CreatedByUid = photoFingerData.CreatedByUid,
                                    CreatedByUnit = photoFingerData.CreatedByUnit,
                                    CreatedByUnitId = photoFingerData.CreatedByUnitId,
                                    CreatedTime = photoFingerData.CreatedTime,
                                    //FileJari = photoFingerData.FileJari,
                                    FileName = photoFingerData.FileName,
                                    Nik = photoFingerData.Nik,
                                    PathFile = photoFingerData.PathFile,
                                    TypeFinger = photoFingerData.TypeFinger
                                });

                                exPhotoFinger.Add(photoFingerData);

                                photoFingers.Add(new Tbl_DataKTP_Finger
                                {
                                    CreatedById = photoFingerData.CreatedById,
                                    IsActive = true,
                                    IsDeleted = false,
                                    Nik = enroll.KtpNIK,
                                    PathFile = filePath,
                                    CreatedByNpp = photoFingerData.CreatedByNpp,
                                    CreatedByUid = photoFingerData.CreatedByUid,
                                    CreatedTime = photoFingerData.CreatedTime,
                                    UpdatedById = Id,
                                    UpdatedByNpp = npp,
                                    UpdatedByUid = enroll.UID,
                                    UpdatedByUnit = unitCode,
                                    CreatedByUnitId = photoFingerData.CreatedByUnitId,
                                    CreatedByUnit = photoFingerData.CreatedByUnit,
                                    //FileJari = photoFingerData.FileJari,
                                    UpdatedTime = DateTime.Now,
                                    FileName = fileName,
                                    TypeFinger = enroll.KtpTypeJariKiri
                                });
                            }

                            if (isEmployee)
                            {
                                if (photoFingerDataEmployee == null)
                                {
                                    photoFingersEmployeeLogs.Add(new Tbl_DataKTP_Finger_Employee_Log
                                    {
                                        CreatedById = photoFingerDataEmployee.CreatedById,
                                        CreatedByNpp = photoFingerDataEmployee.CreatedByNpp,
                                        CreatedByUid = photoFingerDataEmployee.CreatedByUid,
                                        CreatedByUnit = photoFingerDataEmployee.CreatedByUnit,
                                        CreatedByUnitId = photoFingerDataEmployee.CreatedByUnitId,
                                        CreatedTime = photoFingerDataEmployee.CreatedTime,
                                        //FileJari = photoFingerDataEmployee.FileJari,
                                        FileName = photoFingerDataEmployee.FileName,
                                        Nik = photoFingerDataEmployee.Nik,
                                        PathFile = photoFingerDataEmployee.PathFile,
                                        TypeFinger = photoFingerDataEmployee.TypeFinger
                                    });
                                };

                                exPhotoFingerEmployee.Add(photoFingerDataEmployee);

                                photoFingersEmployee.Add(new Tbl_DataKTP_Finger_Employee
                                {
                                    CreatedById = photoFingerDataEmployee.CreatedById,
                                    IsActive = true,
                                    IsDeleted = false,
                                    Nik = enroll.KtpNIK,
                                    PathFile = filePath,
                                    CreatedByNpp = photoFingerDataEmployee.CreatedByNpp,
                                    CreatedByUid = photoFingerDataEmployee.CreatedByUid,
                                    CreatedTime = photoFingerDataEmployee.CreatedTime,
                                    UpdatedById = Id,
                                    UpdatedByNpp = npp,
                                    UpdatedByUid = enroll.UID,
                                    UpdatedByUnit = unitCode,
                                    CreatedByUnitId = photoFingerDataEmployee.CreatedByUnitId,
                                    CreatedByUnit = photoFingerDataEmployee.CreatedByUnit,
                                    //FileJari = photoFingerDataEmployee.FileJari,
                                    UpdatedTime = DateTime.Now,
                                    FileName = fileName,
                                    TypeFinger = enroll.KtpTypeJariKanan
                                });
                            }
                        }
                    }
                }
                #endregion

                var (status, msg) = await _enrollmentKTPRepository.InsertEnrollFlow2(cekKTP, logDemografi, photoKTPData, photoKtpLog, photoSignatureData,
                    photoSignatureLog, photoCamData, photoCamLog, photoFingers, photoFingersEmployee, photoFingerLogs, photoFingersEmployeeLogs, exPhotoKtp, exphotoSignatureData, exphotoCamData, exPhotoFinger, exPhotoFingerEmployee, dataReaderLog, dataNpp);

                #region Logging Reader Activity
                _ = _alatReaderRepository.CreateLogActivity2(dataReaderActivityLog);
                _ = _alatReaderRepository.CreateAlatReaderLog(dataReaderLog);
                #endregion

                if (status)
                {
                    return (_SuccessMessageConfig.NasabahSuksesUpdate, (int)ServiceResponseStatus.SUKSES, cekKTP.CIF);
                }
                else
                {
                    return (_ErrorMessageConfig.NasabahGagalEnroll, (int)ServiceResponseStatus.ERROR, "");
                }
            }
            else
            {
                #region Logging Reader Activity
                _ = _alatReaderRepository.CreateLogActivity2(dataReaderActivityLog);
                _ = _alatReaderRepository.CreateAlatReaderLog(dataReaderLog);
                #endregion

                return (_ErrorMessageConfig.DemografiTidakDitemukan, (int)ServiceResponseStatus.Data_Empty, "");
            }
            #endregion
        }

        public async Task<(string msg, int code, string cif)> UpdatesPhotoCam(EnrollKTP enroll, int Id, string npp,
            string unitCode, int unitId)
        {
            var cekKTP = await _profileRepository.GetDataDemografis(enroll.KtpNIK)
                .ConfigureAwait(false);

            string JamServer = DateTime.Now.ToString("ddMMyyyyHHmmss");
            var sysPathFolder = await _sysParameterRepository.GetPathFolder("PathFolderData");
            string pathFolder = sysPathFolder.Value;

            #region check UID
            var dataReader = await _dataReaderRepository.GetDatareaderUid(enroll.UID).ConfigureAwait(false);

            var dataReaderLog = new Tbl_MasterAlatReaderLog();
            var dataReaderActivityLog = new Tbl_MasterAlatReaderLogActvity();

            if (dataReader != null)
            {
                dataReaderLog = new Tbl_MasterAlatReaderLog
                {
                    CreatedBy_Id = Id,
                    CreatedTime = DateTime.Now,
                    IsActive = true,
                    IsDeleted = false,
                    Nik = enroll.KtpNIK,
                    PegawaiId = Id,
                    Serial_Number = dataReader.SN_Unit,
                    Type = "Updates Enroll",
                    Uid = enroll.UID
                };

                dataReaderActivityLog = new Tbl_MasterAlatReaderLogActvity
                {
                    CreatedBy_Id = Id,
                    IsActive = true,
                    IsDeleted = false,
                    KodeUnit = unitCode,
                    LastIP = enroll.IpAddress,
                    NIK = enroll.KtpNIK,
                    CreatedTime = DateTime.Now,
                    NppPegawai = npp,
                    PegawaiId = Id,
                    Type = "Updates Enroll",
                    UID = enroll.UID,
                    UnitId = unitId
                };
            }
            #endregion

            #region update data ktp
            if (cekKTP != null)
            {
                var photoCamLog = new Tbl_DataKTP_PhotoCam_Log();
                var photoCamData = await _profileRepository.GetPhotoCam(enroll.KtpNIK)
                        .ConfigureAwait(false);

                #region update photo cam
                if (!string.IsNullOrWhiteSpace(enroll.KtpPhotoCam) && photoCamData != null)
                {
                    string imageEncrypted = enroll.KtpPhotoCam.Encrypt(Phrase.FileEncryption);

                    string filePath = "";

                    var systemParameterPath = await _sysParameterRepository.GetPathFolder("FolderWebcam")
                                                .ConfigureAwait(false);
                    string pathFolderFoto = systemParameterPath.Value;

                    string SubPathFolderPhotoCam = pathFolder + "/" + pathFolderFoto + "/" + enroll.KtpNIK + "/";

                    string fileName = "PhotoCam_" + enroll.KtpNIK + "_" + JamServer + ".txt";

                    if (_sftpConfig.IsActive)
                    {
                        using var stream = new MemoryStream(Encoding.ASCII.GetBytes(imageEncrypted));

                        (var fname, var fPath) = await stream.UploadToFTPServer(SubPathFolderPhotoCam, fileName, _sftpConfig.Host,
                            _sftpConfig.Username, _sftpConfig.Password, _sftpConfig.RootDirectory, _sftpConfig.Url).ConfigureAwait(false);

                        filePath = fPath;
                    }
                    else
                    {
                        if (!Directory.Exists(SubPathFolderPhotoCam))
                        {
                            Directory.CreateDirectory(SubPathFolderPhotoCam);
                        }

                        filePath = SubPathFolderPhotoCam + fileName;
                        File.WriteAllText(filePath, imageEncrypted);
                    }
                    photoCamLog.CreatedById = photoCamData.CreatedById;
                    photoCamLog.CreatedByNpp = photoCamData.CreatedByNpp;
                    photoCamLog.CreatedByUid = photoCamData.CreatedByUid;
                    photoCamLog.CreatedByUnit = photoCamData.CreatedByUnit;
                    photoCamLog.CreatedTime = photoCamData.CreatedTime;
                    photoCamLog.FileName = photoCamData.FileName;
                    photoCamLog.Nik = photoCamData.Nik;
                    photoCamLog.PathFile = photoCamData.PathFile;

                    photoCamData.PathFile = filePath;
                    photoCamData.Nik = enroll.KtpNIK;
                    photoCamData.FileName = fileName;
                    photoCamData.IsActive = true;
                    photoCamData.IsDeleted = false;
                    photoCamData.UpdatedById = Id;
                    photoCamData.UpdatedByNpp = npp;
                    photoCamData.UpdatedByUid = enroll.UID;
                    photoCamData.UpdatedTime = DateTime.Now;
                }
                #endregion

                _enrollmentKTPRepository.UpdatesPhotoCam(photoCamData, photoCamLog, dataReaderActivityLog, dataReaderLog);

                #region Logging Reader Activity
                _ = _alatReaderRepository.CreateLogActivity2(dataReaderActivityLog);
                _ = _alatReaderRepository.CreateAlatReaderLog(dataReaderLog);
                #endregion

                return (_SuccessMessageConfig.NasabahSuksesUpdate, (int)ServiceResponseStatus.SUKSES, cekKTP.CIF);
            }
            else
            {
                #region Logging Reader Activity
                _ = _alatReaderRepository.CreateLogActivity2(dataReaderActivityLog);
                _ = _alatReaderRepository.CreateAlatReaderLog(dataReaderLog);
                #endregion

                return (_ErrorMessageConfig.DemografiTidakDitemukan, (int)ServiceResponseStatus.Data_Empty, "");
            }
            #endregion

        }

        public async Task<(string msg, int code, string cif)> SubmitEnrollment(bool isHitSOA, ApiSOA ReqSoa, EnrollKTP enroll, int Id, string npp,
            string unitCode, int unitId)
        {
            bool isEmployee = false;
            var _empData = await _enrollmentKTPRepository.IsEmployee(enroll.KtpNIK);
            if (_empData != null)
            {
                isEmployee = true;
            }

            if (enroll.KtpTanggalLahir != null)
            {
                enroll.KtpTanggalLahir = enroll.KtpTanggalLahir.Replace(".", "");
                if (enroll.KtpTanggalLahir.Contains("/"))
                {
                    enroll.KtpTanggalLahir = enroll.KtpTanggalLahir.Replace("/", "-");
                }
                string[] arrayTanggl = enroll.KtpTanggalLahir.Split("-");
                string hari = "";
                string bulan = "";
                string tahun = "";

                if (arrayTanggl != null)
                {
                    if (arrayTanggl[0] != null)
                    {
                        if (arrayTanggl[0].Length != 2)
                        {
                            hari = "0" + arrayTanggl[0];

                        }
                        else
                        {
                            hari = arrayTanggl[0];

                        }
                    }

                    if (arrayTanggl[1] != null)
                    {
                        if (arrayTanggl[1].Length != 2)
                        {
                            bulan = "0" + arrayTanggl[1];

                        }
                        else
                        {
                            bulan = arrayTanggl[1];

                        }
                    }

                    if (arrayTanggl[2] != null)
                    {
                        tahun = arrayTanggl[2];
                    }
                }

                enroll.KtpTanggalLahir = hari + "-" + bulan + "-" + tahun;
            }

            const bool IsNewEnroll = true;
            string JamServer = DateTime.Now.ToString("ddMMyyyyHHmmss");
            bool IsNasabah = false;

            var cekKTP = await _profileRepository.GetDataDemografis(enroll.KtpNIK)
                .ConfigureAwait(false);

            if (cekKTP != null)
            {
                var logDemografi = new Tbl_DataKTP_Demografis_Log
                {
                    Agama = cekKTP.Agama,
                    Alamat = cekKTP.Alamat,
                    AlamatGoogle = cekKTP.AlamatGoogle,
                    AlamatLengkap = cekKTP.AlamatLengkap,
                    CreatedById = cekKTP.CreatedById,
                    CreatedByUID = cekKTP.CreatedByUID,
                    CreatedByUnitId = cekKTP.CreatedByUnitId,
                    CreatedTime = cekKTP.CreatedTime,
                    Desa = cekKTP.Desa,
                    GolonganDarah = cekKTP.GolonganDarah,
                    JenisKelamin = cekKTP.JenisKelamin,
                    Kecamatan = cekKTP.Kecamatan,
                    Kelurahan = cekKTP.Kelurahan,
                    Kewarganegaraan = cekKTP.Kewarganegaraan,
                    KodePos = cekKTP.KodePos,
                    Kota = cekKTP.Kota,
                    Latitude = cekKTP.Latitude,
                    Longitude = cekKTP.Longitude,
                    MasaBerlaku = cekKTP.MasaBerlaku,
                    Nama = cekKTP.Nama,
                    NIK = cekKTP.NIK,
                    Pekerjaan = cekKTP.Pekerjaan,
                    Provinsi = cekKTP.Provinsi,
                    RT = cekKTP.RT,
                    RW = cekKTP.RW,
                    StatusPerkawinan = cekKTP.StatusPerkawinan,
                    TanggalLahir = cekKTP.TanggalLahir,
                    TempatLahir = cekKTP.TempatLahir,
                    CIF = cekKTP.CIF,
                    CreatedByNpp = cekKTP.CreatedByNpp
                };

                var stringPerubahan = "";

                if (cekKTP.Nama != enroll.KtpNama)
                {
                    stringPerubahan = stringPerubahan + "NAMA : " + cekKTP.Nama + " -> " + enroll.KtpNama + " <br/>";
                    cekKTP.Nama = enroll.KtpNama;
                }

                if (cekKTP.TempatLahir != enroll.KtpTempatLahir)
                {
                    stringPerubahan = stringPerubahan + "TEMPAT LAHIR : " + cekKTP.TempatLahir + " -> " + enroll.KtpTempatLahir + " <br/>";
                    cekKTP.TempatLahir = enroll.KtpTempatLahir;
                }

                if (enroll.KtpTanggalLahir != null)
                {
                    var _ttl = DateTime.ParseExact(enroll.KtpTanggalLahir, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                    if (cekKTP.TanggalLahir != _ttl)
                    {
                        stringPerubahan = stringPerubahan + "TANGGAL LAHIR : " + cekKTP.TanggalLahir + " -> " + enroll.KtpTanggalLahir + " <br/>";
                        cekKTP.TanggalLahir = DateTime.ParseExact(enroll.KtpTanggalLahir, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                    }
                }

                if (cekKTP.JenisKelamin != enroll.KtpJanisKelamin)
                {
                    stringPerubahan = stringPerubahan + "JENIS KELAMIN : " + cekKTP.JenisKelamin + " -> " + enroll.KtpJanisKelamin + " <br/>";
                    cekKTP.JenisKelamin = enroll.KtpJanisKelamin;
                }

                if (cekKTP.GolonganDarah != enroll.KtpGolonganDarah)
                {
                    stringPerubahan = stringPerubahan + "GOLONGAN DARAH : " + cekKTP.GolonganDarah + " -> " + enroll.KtpGolonganDarah + " <br/>";
                    cekKTP.GolonganDarah = enroll.KtpGolonganDarah;
                }

                if (cekKTP.Alamat != enroll.KtpAlamat)
                {
                    stringPerubahan = stringPerubahan + "ALAMAT : " + cekKTP.Alamat + " -> " + enroll.KtpAlamat + " <br/>";
                    cekKTP.Alamat = enroll.KtpAlamat;
                }

                if (cekKTP.RT != enroll.KtpRT)
                {
                    stringPerubahan = stringPerubahan + "RT : " + cekKTP.RT + " -> " + enroll.KtpRT + " <br/>";
                    cekKTP.RT = enroll.KtpRT;
                }

                if (cekKTP.RW != enroll.KtpRW)
                {
                    stringPerubahan = stringPerubahan + "RW : " + cekKTP.RW + " -> " + enroll.KtpRW + " <br/>";
                    cekKTP.RW = enroll.KtpRW;
                }

                if (cekKTP.Kelurahan != enroll.KtpKelurahan)
                {
                    stringPerubahan = stringPerubahan + "KELURAHAN : " + cekKTP.Kelurahan + " -> " + enroll.KtpKelurahan + " <br/>";
                    cekKTP.Kelurahan = enroll.KtpKelurahan;
                }

                if (cekKTP.Kecamatan != enroll.KtpKecamatan)
                {
                    stringPerubahan = stringPerubahan + "KECAMATAN : " + cekKTP.Kecamatan + " -> " + enroll.KtpKecamatan + " <br/>";
                    cekKTP.Kecamatan = enroll.KtpKecamatan;
                }

                if (cekKTP.Kota != enroll.KtpKota)
                {
                    stringPerubahan = stringPerubahan + "KOTA/KABUPATEN : " + cekKTP.Kota + " -> " + enroll.KtpKota + " <br/>";
                    cekKTP.Kota = enroll.KtpKota;
                }

                if (cekKTP.Provinsi != enroll.KtpProvinsi)
                {
                    stringPerubahan = stringPerubahan + "PROVINSI  : " + cekKTP.Provinsi + " -> " + enroll.KtpProvinsi + " <br/>";
                    cekKTP.Provinsi = enroll.KtpProvinsi;
                }

                if (cekKTP.Agama != enroll.KtpAgama)
                {
                    stringPerubahan = stringPerubahan + "AGAMA  : " + cekKTP.Agama + " -> " + enroll.KtpAgama + " <br/>";
                    cekKTP.Agama = enroll.KtpAgama;
                }

                if (cekKTP.StatusPerkawinan != enroll.KtpStatusPerkawinan)
                {
                    stringPerubahan = stringPerubahan + "STATUS PERKAWINAN  : " + cekKTP.Agama + " -> " + enroll.KtpAgama + " <br/>";
                    cekKTP.StatusPerkawinan = enroll.KtpStatusPerkawinan;
                }

                if (cekKTP.Pekerjaan != enroll.KtpPekerjaan)
                {
                    stringPerubahan = stringPerubahan + "PEKERJAAN  : " + cekKTP.Agama + " -> " + enroll.KtpAgama + " <br/>";
                    cekKTP.Pekerjaan = enroll.KtpPekerjaan;
                }

                if (cekKTP.Kewarganegaraan != enroll.KtpKewarganegaraan)
                {
                    stringPerubahan = stringPerubahan + "KEWARGANEGARAAN  : " + cekKTP.Kewarganegaraan + " -> " + enroll.KtpKewarganegaraan + " <br/>";
                    cekKTP.Kewarganegaraan = enroll.KtpKewarganegaraan;
                }
                if (cekKTP.MasaBerlaku != enroll.KtpMasaBerlaku)
                {
                    stringPerubahan = stringPerubahan + "MASA BERLAKU  : " + cekKTP.MasaBerlaku + " -> " + enroll.KtpMasaBerlaku + " <br/>";
                    cekKTP.MasaBerlaku = enroll.KtpMasaBerlaku;
                }

                cekKTP.UpdatedTime = DateTime.Now;
                cekKTP.UpdatedById = Id;
                cekKTP.UpdatedByUID = enroll.UID;
                cekKTP.UpdatedByUnitCode = unitCode;
                cekKTP.UpdatedByUnitId = unitId;

                await _profileRepository.UpdateDataDemografis(cekKTP)
                    .ConfigureAwait(false);

                if (string.IsNullOrWhiteSpace(stringPerubahan))
                {
                    return (_ErrorMessageConfig.NasabahPernahEnroll, (int)EnrollStatus.Sudah_di_enroll_sebelumnya, "");
                }

                _profileRepository.InsertDemografiLog(logDemografi);

                return (stringPerubahan, (int)EnrollStatus.Berhasil_mengubah_beberapa_perubahan, "");
            }

            if (IsNewEnroll)
            {
                if (enroll?.KtpNIK == null) return (_ErrorMessageConfig.InputTidakLengkap, (int)EnrollStatus.Inputan_tidak_lengkap, "");

                #region hit soa and logging it
                var cifData = new ApiSOAResponse();

                if (isHitSOA == true)
                {
                    cifData = await _cifService.GetSOAByCif(ReqSoa)
                    .ConfigureAwait(false);

                    var status = 0;
                    if (cifData.cif != null)
                    {
                        status = 1;
                    }

                    var _log = new Tbl_ThirdPartyLog
                    {
                        FeatureName = "SubmitEnrollment",
                        HostUrl = ReqSoa.host,
                        Request = Newtonsoft.Json.JsonConvert.SerializeObject(ReqSoa),
                        Status = status,
                        Response = Newtonsoft.Json.JsonConvert.SerializeObject(cifData),
                        CreatedDate = System.DateTime.Now,
                        CreatedBy = npp
                    };

                    _ = _enrollmentKTPRepository.CreateThirdPartyLog(_log);
                }
                else
                {
                    var res = await _cifService.GetCIF(
                        new NikDto { Nik = enroll.KtpNIK },
                        new NikDtoUrl { baseUrl = ReqSoa.baseUrlNonSoa, endpoint = ReqSoa.UrlEndPointNonSoa });
                    if (res.Data == null)
                    {
                        cifData.cif = null;
                    }
                    else
                    {
                        cifData.cif = res.Data.Cif;
                    };
                }
                #endregion

                if (cifData.cif != null)
                {
                    IsNasabah = true;
                    var str = cifData.cif.Trim();
                    cifData.cif = str;
                }

                var sysPathFolder = await _sysParameterRepository.GetPathFolder("PathFolderData");
                string pathFolder = sysPathFolder.Value;

                var dataDemografis = new Tbl_DataKTP_Demografis();

                var dataDemografisLog = new Tbl_DataKTP_Demografis_Log();

                var dataNpp = new Tbl_Mapping_Pegawai_KTP();

                if (cekKTP != null)
                {
                    dataDemografisLog = new Tbl_DataKTP_Demografis_Log
                    {
                        Agama = cekKTP.Agama,
                        Alamat = cekKTP.Alamat,
                        AlamatGoogle = cekKTP.AlamatGoogle,
                        AlamatLengkap = cekKTP.AlamatLengkap,
                        CreatedById = cekKTP.CreatedById,
                        CreatedByUID = cekKTP.CreatedByUID,
                        CreatedByUnitId = cekKTP.CreatedByUnitId,
                        CreatedTime = cekKTP.CreatedTime,
                        Desa = cekKTP.Desa,
                        GolonganDarah = cekKTP.GolonganDarah,
                        JenisKelamin = cekKTP.JenisKelamin,
                        Kecamatan = cekKTP.Kecamatan,
                        Kelurahan = cekKTP.Kelurahan,
                        Kewarganegaraan = cekKTP.Kewarganegaraan,
                        KodePos = cekKTP.KodePos,
                        Kota = cekKTP.Kota,
                        Latitude = cekKTP.Latitude,
                        Longitude = cekKTP.Longitude,
                        MasaBerlaku = cekKTP.MasaBerlaku,
                        Nama = cekKTP.Nama,
                        NIK = cekKTP.NIK,
                        Pekerjaan = cekKTP.Pekerjaan,
                        Provinsi = cekKTP.Provinsi,
                        RT = cekKTP.RT,
                        RW = cekKTP.RW,
                        StatusPerkawinan = cekKTP.StatusPerkawinan,
                        TanggalLahir = cekKTP.TanggalLahir,
                        TempatLahir = cekKTP.TempatLahir
                    };

                    cekKTP.Agama = enroll.KtpAgama;
                    cekKTP.Alamat = enroll.KtpAlamat;
                    cekKTP.AlamatGoogle = enroll.KtpAlamatConvertLatlong;
                    cekKTP.AlamatLengkap = enroll.KtpAlamatConvertLengkap;
                    if (string.IsNullOrWhiteSpace(cifData.cif))
                    {
                        cekKTP.CIF = cifData.cif;
                    }
                    cekKTP.GolonganDarah = enroll.KtpGolonganDarah;
                    cekKTP.JenisKelamin = enroll.KtpJanisKelamin;
                    cekKTP.Kecamatan = enroll.KtpKecamatan;
                    cekKTP.Kelurahan = enroll.KtpKelurahan;
                    cekKTP.Kewarganegaraan = enroll.KtpKewarganegaraan;
                    cekKTP.KodePos = enroll.KtpKodePos;
                    cekKTP.Kota = enroll.KtpKota;
                    cekKTP.Latitude = enroll.KtpLatitude;
                    cekKTP.Longitude = enroll.KtpLongitude;
                    cekKTP.MasaBerlaku = enroll.KtpMasaBerlaku;
                    cekKTP.Nama = enroll.KtpNama;
                    cekKTP.NIK = enroll.KtpNIK;
                    cekKTP.Pekerjaan = enroll.KtpPekerjaan;
                    cekKTP.Provinsi = enroll.KtpProvinsi;
                    cekKTP.RT = enroll.KtpRT;
                    cekKTP.RW = enroll.KtpRW;
                    cekKTP.StatusPerkawinan = enroll.KtpStatusPerkawinan;
                    cekKTP.TanggalLahir = string.IsNullOrWhiteSpace(enroll.KtpTanggalLahir) ? null : DateTime.ParseExact(enroll.KtpTanggalLahir, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                    cekKTP.TempatLahir = enroll.KtpTempatLahir;
                    cekKTP.UpdatedById = Id;
                    cekKTP.UpdatedByNpp = npp;
                    cekKTP.UpdatedByUID = enroll.UID;
                    cekKTP.UpdatedTime = DateTime.Now;
                    cekKTP.UpdatedByUnitCode = unitCode;
                    cekKTP.UpdatedByUnitId = unitId;
                }
                else
                {
                    dataDemografis = new Tbl_DataKTP_Demografis
                    {
                        Agama = enroll.KtpAgama,
                        Alamat = enroll.KtpAlamat,
                        AlamatGoogle = enroll.KtpAlamatConvertLatlong,
                        AlamatLengkap = enroll.KtpAlamatConvertLengkap,
                        CIF = (cifData.cif),
                        NIK = enroll.KtpNIK,
                        CreatedById = Id,
                        IsActive = true,
                        IsDeleted = false,
                        JenisKelamin = enroll.KtpJanisKelamin,
                        Kecamatan = enroll.KtpKecamatan,
                        Kelurahan = enroll.KtpKelurahan,
                        CreatedByUID = enroll.UID,
                        CreatedTime = DateTime.Now,
                        GolonganDarah = enroll.KtpGolonganDarah,
                        Kewarganegaraan = enroll.KtpKewarganegaraan,
                        KodePos = enroll.KtpKodePos,
                        Kota = enroll.KtpKota,
                        Latitude = enroll.KtpLatitude,
                        Longitude = enroll.KtpLongitude,
                        MasaBerlaku = enroll.KtpMasaBerlaku,
                        Nama = enroll.KtpNama,
                        Pekerjaan = enroll.KtpPekerjaan,
                        Provinsi = enroll.KtpProvinsi,
                        RT = enroll.KtpRT,
                        RW = enroll.KtpRW,
                        StatusPerkawinan = enroll.KtpStatusPerkawinan,
                        TanggalLahir = string.IsNullOrWhiteSpace(enroll.KtpTanggalLahir) ? null : DateTime.ParseExact(enroll.KtpTanggalLahir, "dd-MM-yyyy", CultureInfo.InvariantCulture),
                        TempatLahir = enroll.KtpTempatLahir,
                        CreatedByNpp = npp,
                        CreatedByUnitCode = unitCode,
                        CreatedByUnitId = unitId,
                        IsVerified = false,
                        IsNasabahTemp = string.IsNullOrWhiteSpace(cifData.cif)
                    };

                    if (isEmployee)
                    {
                        dataNpp = new Tbl_Mapping_Pegawai_KTP
                        {
                            CreatedById = Id,
                            NIK = enroll.KtpNIK,
                            Npp = _empData.Npp,
                            CreatedByNpp = npp,
                            CreatedByUID = enroll.UID,
                            CreatedTime = DateTime.Now
                        };
                    }
                }

                var exPhotoKtp = new Tbl_DataKTP_Photo();

                var photoKtpLog = new Tbl_DataKTP_Photo_Log();

                var photoKtp = new Tbl_DataKTP_Photo();

                if (!string.IsNullOrWhiteSpace(enroll.KtpPhotoKTP))
                {
                    var photoKTPData = await _profileRepository.GetPhotoKtp(enroll.KtpNIK)
                        .ConfigureAwait(false);

                    string imageEncrypted = enroll.KtpPhotoKTP.Encrypt(Phrase.FileEncryption);

                    string filePath = "";

                    var systemParameterPath = await _sysParameterRepository.GetPathFolder("FolderPhoto")
                                                .ConfigureAwait(false);
                    string pathFolderFoto = systemParameterPath.Value;

                    string subPathFolderFoto = pathFolder + "/" + pathFolderFoto + "/" + enroll.KtpNIK + "/";

                    string fileName = "Foto_" + enroll.KtpNIK + "_" + JamServer + ".txt";

                    if (_sftpConfig.IsActive)
                    {
                        using var stream = new MemoryStream(Encoding.ASCII.GetBytes(imageEncrypted));

                        (var fname, var fPath) = await stream.UploadToFTPServer(subPathFolderFoto, fileName, _sftpConfig.Host,
                            _sftpConfig.Username, _sftpConfig.Password, _sftpConfig.RootDirectory, _sftpConfig.Url).ConfigureAwait(false);

                        filePath = fPath;
                    }
                    else
                    {

                        if (!Directory.Exists(subPathFolderFoto))
                        {
                            Directory.CreateDirectory(subPathFolderFoto);
                        }

                        filePath = subPathFolderFoto + fileName;
                        File.WriteAllText(filePath, imageEncrypted);
                    }

                    if (photoKTPData != null)
                    {
                        photoKtpLog.CreatedById = photoKTPData.CreatedById;
                        photoKtpLog.CreatedByNpp = photoKTPData.CreatedByNpp;
                        photoKtpLog.CreatedByUid = photoKTPData.CreatedByUid;
                        photoKtpLog.CreatedByUnit = photoKTPData.CreatedByUnit;
                        photoKtpLog.CreatedTime = photoKTPData.CreatedTime;
                        photoKtpLog.FileName = photoKTPData.FileName;
                        photoKtpLog.Nik = photoKTPData.Nik;
                        photoKtpLog.PathFile = photoKTPData.PathFile;

                        exPhotoKtp = photoKTPData;
                    }

                    photoKtp = new Tbl_DataKTP_Photo
                    {
                        PathFile = filePath,
                        Nik = enroll.KtpNIK,
                        FileName = fileName,
                        IsActive = true,
                        IsDeleted = false,
                        CreatedById = Id,
                        CreatedByNpp = npp,
                        CreatedByUid = enroll.UID,
                        CreatedTime = DateTime.Now
                    };
                }

                var exPhotoSignature = new Tbl_DataKTP_Signature();

                var photoSignatureLog = new Tbl_DataKTP_Signature_Log();

                var photoSignature = new Tbl_DataKTP_Signature();

                if (!string.IsNullOrWhiteSpace(enroll.KtpSignature))
                {
                    var photoSignatureData = await _profileRepository.GetPhotoSignature(enroll.KtpNIK)
                        .ConfigureAwait(false);

                    string imageEncrypted = enroll.KtpSignature.Encrypt(Phrase.FileEncryption);

                    string filePath = "";

                    var systemParameterPath = await _sysParameterRepository.GetPathFolder("FolderSignature")
                                                .ConfigureAwait(false);
                    string pathFolderFoto = systemParameterPath.Value;

                    string subPathFolderSignature = pathFolder + "/" + pathFolderFoto + "/" + enroll.KtpNIK + "/";

                    string fileName = "Signature_" + enroll.KtpNIK + "_" + JamServer + ".txt";

                    if (_sftpConfig.IsActive)
                    {
                        using var stream = new MemoryStream(Encoding.ASCII.GetBytes(imageEncrypted));

                        (var fname, var fPath) = await stream.UploadToFTPServer(subPathFolderSignature, fileName, _sftpConfig.Host,
                            _sftpConfig.Username, _sftpConfig.Password, _sftpConfig.RootDirectory, _sftpConfig.Url).ConfigureAwait(false);

                        filePath = fPath;
                    }
                    else
                    {
                        if (!Directory.Exists(subPathFolderSignature))
                        {
                            Directory.CreateDirectory(subPathFolderSignature);
                        }

                        filePath = subPathFolderSignature + fileName;
                        File.WriteAllText(filePath, imageEncrypted);
                    }

                    if (photoSignatureData != null)
                    {
                        photoSignatureLog.CreatedById = photoSignatureData.CreatedById;
                        photoSignatureLog.CreatedByNpp = photoSignatureData.CreatedByNpp;
                        photoSignatureLog.CreatedByUid = photoSignatureData.CreatedByUid;
                        photoSignatureLog.CreatedByUnit = photoSignatureData.CreatedByUnit;
                        photoSignatureLog.CreatedTime = photoSignatureData.CreatedTime;
                        photoSignatureLog.FileName = photoSignatureData.FileName;
                        photoSignatureLog.Nik = photoSignatureData.Nik;
                        photoSignatureLog.PathFile = photoSignatureData.PathFile;

                        exPhotoSignature = photoSignatureData;
                    }

                    photoSignature = new Tbl_DataKTP_Signature
                    {
                        CreatedById = Id,
                        CreatedByNpp = npp,
                        IsActive = true,
                        IsDeleted = false,
                        Nik = enroll.KtpNIK,
                        FileName = fileName,
                        CreatedByUid = enroll.UID,
                        CreatedTime = DateTime.Now,
                        PathFile = filePath
                    };
                }

                var exPhotoCam = new Tbl_DataKTP_PhotoCam();

                var photoCamLog = new Tbl_DataKTP_PhotoCam_Log();

                var photoCam = new Tbl_DataKTP_PhotoCam();

                if (!string.IsNullOrWhiteSpace(enroll.KtpPhotoCam))
                {
                    var photoCamData = await _profileRepository.GetPhotoCam(enroll.KtpNIK)
                        .ConfigureAwait(false);

                    string imageEncrypted = enroll.KtpPhotoCam.Encrypt(Phrase.FileEncryption);

                    string filePath = "";

                    var systemParameterPath = await _sysParameterRepository.GetPathFolder("FolderWebcam")
                                                .ConfigureAwait(false);
                    string pathFolderFoto = systemParameterPath.Value;

                    string SubPathFolderPhotoCam = pathFolder + "/" + pathFolderFoto + "/" + enroll.KtpNIK + "/";

                    string fileName = "PhotoCam_" + enroll.KtpNIK + "_" + JamServer + ".txt";

                    if (_sftpConfig.IsActive)
                    {
                        using var stream = new MemoryStream(Encoding.ASCII.GetBytes(imageEncrypted));

                        (var fname, var fPath) = await stream.UploadToFTPServer(SubPathFolderPhotoCam, fileName, _sftpConfig.Host,
                            _sftpConfig.Username, _sftpConfig.Password, _sftpConfig.RootDirectory, _sftpConfig.Url).ConfigureAwait(false);

                        filePath = fPath;
                    }
                    else
                    {
                        if (!Directory.Exists(SubPathFolderPhotoCam))
                        {
                            Directory.CreateDirectory(SubPathFolderPhotoCam);
                        }

                        filePath = SubPathFolderPhotoCam + fileName;
                        File.WriteAllText(filePath, imageEncrypted);
                    }

                    if (photoCamData != null)
                    {
                        photoCamLog.CreatedById = photoCamData.CreatedById;
                        photoCamLog.CreatedByNpp = photoCamData.CreatedByNpp;
                        photoCamLog.CreatedByUid = photoCamData.CreatedByUid;
                        photoCamLog.CreatedByUnit = photoCamData.CreatedByUnit;
                        photoCamLog.CreatedTime = photoCamData.CreatedTime;
                        photoCamLog.FileName = photoCamData.FileName;
                        photoCamLog.Nik = photoCamData.Nik;
                        photoCamLog.PathFile = photoCamData.PathFile;

                        exPhotoCam = photoCamData;
                    }

                    photoCam = new Tbl_DataKTP_PhotoCam
                    {
                        PathFile = filePath,//
                        Nik = enroll.KtpNIK,
                        FileName = fileName,//
                        CreatedTime = DateTime.Now,
                        CreatedById = Id,
                        IsActive = true,
                        IsDeleted = false,
                        CreatedByNpp = npp,
                        CreatedByUid = enroll.UID
                    };
                }

                var exPhotoFinger = new List<Tbl_DataKTP_Finger>();

                var exPhotoFingerEmployee = new List<Tbl_DataKTP_Finger_Employee>();

                var photoFingerLogs = new List<Tbl_DataKTP_Finger_Log>();

                var photoFingers = new List<Tbl_DataKTP_Finger>();

                var photoFingersEmployee = new List<Tbl_DataKTP_Finger_Employee>();

                if (!string.IsNullOrWhiteSpace(enroll.KtpFingerKanan))
                {
                    var photoFingerDataEmployee = new Tbl_DataKTP_Finger_Employee();

                    if (isEmployee)
                    {
                        photoFingerDataEmployee = await _profileRepository.GetPhotoFingerEmployee(enroll.KtpNIK, enroll.KtpTypeJariKanan)
                            .ConfigureAwait(false);
                    }

                    string imageEncrypted = enroll.KtpFingerKanan.Encrypt(Phrase.FileEncryption);

                    string filePath = "";

                    var systemParameterPath = await _sysParameterRepository.GetPathFolder("FolderFinger")
                                                .ConfigureAwait(false);
                    string pathFolderFoto = systemParameterPath.Value;

                    string subPathFolderPhotoFinger = pathFolder + "/" + pathFolderFoto + "/" + enroll.KtpNIK + "/";

                    string JenisJari = enroll.KtpTypeJariKanan.Replace(" ", "");
                    string fileName = "PhotoFinger_" + JenisJari + "_" + enroll.KtpNIK + "_" + JamServer + ".txt";

                    if (_sftpConfig.IsActive)
                    {
                        using var stream = new MemoryStream(Encoding.ASCII.GetBytes(imageEncrypted));

                        (var fname, var fPath) = await stream.UploadToFTPServer(subPathFolderPhotoFinger, fileName, _sftpConfig.Host,
                            _sftpConfig.Username, _sftpConfig.Password, _sftpConfig.RootDirectory, _sftpConfig.Url).ConfigureAwait(false);

                        filePath = fPath;
                    }
                    else
                    {
                        if (!Directory.Exists(subPathFolderPhotoFinger))
                        {
                            Directory.CreateDirectory(subPathFolderPhotoFinger);
                        }

                        filePath = subPathFolderPhotoFinger + fileName;
                        File.WriteAllText(filePath, imageEncrypted);
                    }

                    var typeFingerMain = await _fingerRepository.GetFingersEnrolled(enroll.KtpNIK);

                    foreach (var i in typeFingerMain)
                    {

                        if (i.TypeFinger.Contains("Kanan"))
                        {
                            var photoFingerData = await _profileRepository.GetPhotoFinger(enroll.KtpNIK, i.TypeFinger).ConfigureAwait(false);

                            if (photoFingerData != null)
                            {
                                photoFingerLogs.Add(new Tbl_DataKTP_Finger_Log
                                {
                                    CreatedById = photoFingerData.CreatedById,
                                    CreatedByNpp = photoFingerData.CreatedByNpp,
                                    CreatedByUid = photoFingerData.CreatedByUid,
                                    CreatedByUnit = photoFingerData.CreatedByUnit,
                                    CreatedByUnitId = photoFingerData.CreatedByUnitId,
                                    CreatedTime = photoFingerData.CreatedTime,
                                    //FileJari = photoFingerData.FileJari,
                                    FileName = photoFingerData.FileName,
                                    Nik = photoFingerData.Nik,
                                    PathFile = photoFingerData.PathFile,
                                    TypeFinger = photoFingerData.TypeFinger
                                });

                                exPhotoFinger.Add(photoFingerData);
                            }
                        }
                    }

                    photoFingers.Add(new Tbl_DataKTP_Finger
                    {
                        CreatedById = Id,
                        IsActive = true,
                        IsDeleted = false,
                        Nik = enroll.KtpNIK,
                        PathFile = filePath,//
                        CreatedByNpp = npp,
                        CreatedByUid = enroll.UID,
                        CreatedTime = DateTime.Now,
                        FileName = fileName,//
                        TypeFinger = enroll.KtpTypeJariKanan
                    });

                    if (isEmployee)
                    {
                        photoFingersEmployee.Add(new Tbl_DataKTP_Finger_Employee
                        {
                            CreatedById = Id,
                            IsActive = true,
                            IsDeleted = false,
                            Nik = enroll.KtpNIK,
                            PathFile = filePath,//
                            CreatedByNpp = npp,
                            CreatedByUid = enroll.UID,
                            CreatedTime = DateTime.Now,
                            FileName = fileName,//
                            TypeFinger = enroll.KtpTypeJariKanan
                        });
                    }
                }

                if (!string.IsNullOrWhiteSpace(enroll.KtpFingerKiri))
                {
                    var photoFingerDataEmployee = new Tbl_DataKTP_Finger_Employee();

                    if (isEmployee)
                    {
                        photoFingerDataEmployee = await _profileRepository.GetPhotoFingerEmployee(enroll.KtpNIK, enroll.KtpTypeJariKiri)
                            .ConfigureAwait(false);
                    }

                    //var photoFingerData = await _profileRepository.GetPhotoFinger(enroll.KtpNIK, enroll.KtpTypeJariKiri)
                    //    .ConfigureAwait(false);

                    string imageEncrypted = enroll.KtpFingerKiri.Encrypt(Phrase.FileEncryption);

                    string filePath = "";

                    var systemParameterPath = await _sysParameterRepository.GetPathFolder("FolderFinger")
                                                .ConfigureAwait(false);
                    string pathFolderFoto = systemParameterPath.Value;

                    string subPathFolderPhotoFinger = pathFolder + "/" + pathFolderFoto + "/" + enroll.KtpNIK + "/";

                    string JenisJari = enroll.KtpTypeJariKiri.Replace(" ", "");
                    string fileName = "PhotoFinger_" + JenisJari + "_" + enroll.KtpNIK + "_" + JamServer + ".txt";

                    if (_sftpConfig.IsActive)
                    {
                        using var stream = new MemoryStream(Encoding.ASCII.GetBytes(imageEncrypted));

                        (var fname, var fPath) = await stream.UploadToFTPServer(subPathFolderPhotoFinger, fileName, _sftpConfig.Host,
                            _sftpConfig.Username, _sftpConfig.Password, _sftpConfig.RootDirectory, _sftpConfig.Url).ConfigureAwait(false);

                        filePath = fPath;
                    }
                    else
                    {
                        if (!Directory.Exists(subPathFolderPhotoFinger))
                        {
                            Directory.CreateDirectory(subPathFolderPhotoFinger);
                        }

                        filePath = subPathFolderPhotoFinger + fileName;
                        File.WriteAllText(filePath, imageEncrypted);
                    }

                    var typeFingerMain = await _fingerRepository.GetFingersEnrolled(enroll.KtpNIK);



                    foreach (var i in typeFingerMain)
                    {

                        if (i.TypeFinger.Contains("Kiri"))
                        {
                            var photoFingerData = await _profileRepository.GetPhotoFinger(enroll.KtpNIK, i.TypeFinger).ConfigureAwait(false);

                            if (photoFingerData != null)
                            {
                                photoFingerLogs.Add(new Tbl_DataKTP_Finger_Log
                                {
                                    CreatedById = photoFingerData.CreatedById,
                                    CreatedByNpp = photoFingerData.CreatedByNpp,
                                    CreatedByUid = photoFingerData.CreatedByUid,
                                    CreatedByUnit = photoFingerData.CreatedByUnit,
                                    CreatedByUnitId = photoFingerData.CreatedByUnitId,
                                    CreatedTime = photoFingerData.CreatedTime,
                                    //FileJari = photoFingerData.FileJari,
                                    FileName = photoFingerData.FileName,
                                    Nik = photoFingerData.Nik,
                                    PathFile = photoFingerData.PathFile,
                                    TypeFinger = photoFingerData.TypeFinger
                                });

                                exPhotoFinger.Add(photoFingerData);
                            }
                        }
                    }

                    photoFingers.Add(new Tbl_DataKTP_Finger
                    {
                        CreatedById = Id,
                        IsActive = true,
                        IsDeleted = false,
                        Nik = enroll.KtpNIK,
                        PathFile = filePath,//
                        CreatedByNpp = npp,
                        CreatedByUid = enroll.UID,
                        CreatedTime = DateTime.Now,
                        FileName = fileName,//
                        TypeFinger = enroll.KtpTypeJariKiri
                    });

                    if (isEmployee)
                    {
                        photoFingersEmployee.Add(new Tbl_DataKTP_Finger_Employee
                        {
                            CreatedById = Id,
                            IsActive = true,
                            IsDeleted = false,
                            Nik = enroll.KtpNIK,
                            PathFile = filePath,//
                            CreatedByNpp = npp,
                            CreatedByUid = enroll.UID,
                            CreatedTime = DateTime.Now,
                            FileName = fileName,//
                            TypeFinger = enroll.KtpTypeJariKiri
                        });
                    }
                }

                // to do: alat reader log
                var dataReader = await _dataReaderRepository.GetDatareaderUid(enroll.UID).ConfigureAwait(false);

                var dataReaderLog = new Tbl_MasterAlatReaderLog();

                if (dataReader != null)
                {
                    dataReaderLog = new Tbl_MasterAlatReaderLog
                    {
                        CreatedBy_Id = Id,
                        CreatedTime = DateTime.Now,
                        IsActive = true,
                        IsDeleted = false,
                        Nik = enroll.KtpNIK,
                        PegawaiId = Id,
                        Serial_Number = dataReader.SN_Unit,
                        Type = "Enroll",
                        Uid = enroll.UID
                    };
                }

                _enrollmentKTPRepository.InsertEnrollFlow(dataDemografis, dataDemografisLog, photoKtp, photoKtpLog, photoSignature, photoSignatureLog,
                    photoCam, photoCamLog, photoFingers, photoFingerLogs, exPhotoKtp, exPhotoSignature, exPhotoCam, exPhotoFinger, dataNpp, dataReaderLog, photoFingersEmployee, exPhotoFingerEmployee);

                await _alatReaderRepository.CreateLogActivity2(new Tbl_MasterAlatReaderLogActvity
                {
                    CreatedBy_Id = Id,
                    IsActive = true,
                    IsDeleted = false,
                    KodeUnit = unitCode,
                    LastIP = enroll.IpAddress,
                    NIK = enroll.KtpNIK,
                    CreatedTime = DateTime.Now,
                    NppPegawai = npp,
                    PegawaiId = Id,
                    Type = "Enroll",
                    UID = enroll.UID,
                    UnitId = unitId
                });
            }

            if (IsNasabah)
            {
                return (_SuccessMessageConfig.NasabahSuksesEnrollNasabah, (int)EnrollStatus.Nasabah_Berhasil_di_enroll, cekKTP.CIF);
            }
            else
            {
                return (_SuccessMessageConfig.NasabahSuksesEnrollNonNasabah, (int)EnrollStatus.Nasabah_Berhasil_di_enroll, cekKTP.CIF);
            }
        }

        public async Task<(string msg, int code, string cif)> SubmitEnrollmentFingerEncryptedOnly(bool isHitSOA, ApiSOA ReqSoa, EnrollKTP enroll, int Id, string npp,
            string unitCode, int unitId)
        {
            bool isEmployee = false;
            var _empData = await _enrollmentKTPRepository.IsEmployee(enroll.KtpNIK);
            if (_empData != null)
            {
                isEmployee = true;
            }

            //#region regex backslash

            //if ((enroll.KtpNIK.Contains("\\") || enroll.KtpNama.Contains("\\") || enroll.KtpTTL.Contains("\\") || enroll.KtpTempatLahir.Contains("\\") || enroll.KtpTanggalLahir.Contains("\\") || enroll.KtpGolonganDarah.Contains("\\") || enroll.KtpJanisKelamin.Contains("\\") || enroll.KtpAlamat.Contains("\\") || enroll.KtpRTRW.Contains("\\") || enroll.KtpRT.Contains("\\") || enroll.KtpRW.Contains("\\") || enroll.KtpKelurahan.Contains("\\") || enroll.KtpKecamatan.Contains("\\") || enroll.KtpKota.Contains("\\") || enroll.KtpProvinsi.Contains("\\") || enroll.KtpAgama.Contains("\\") || enroll.KtpStatusPerkawinan.Contains("\\") || enroll.KtpPekerjaan.Contains("\\") || enroll.KtpKewarganegaraan.Contains("\\") || enroll.KtpMasaBerlaku.Contains("\\") || enroll.KtpLatitude.Contains("\\") || enroll.KtpLongitude.Contains("\\") || enroll.KtpAlamatConvertLatlong.Contains("\\") || enroll.KtpAlamatConvertLengkap.Contains("\\") || enroll.KtpPhotoKTP.Contains("\\") || enroll.KtpFingerKanan.Contains("\\") || enroll.KtpFingerKiri.Contains("\\") || enroll.KtpPhotoCam.Contains("\\") || enroll.KtpSignature.Contains("\\") || enroll.KtpKodePos.Contains("\\") || enroll.UID.Contains("\\") || enroll.KtpTypeJariKanan.Contains("\\") || enroll.KtpTypeJariKanan.Contains("\\")))
            //{
            //    return (_ErrorMessageConfig.InputTidakLengkap, (int)EnrollStatus.Inputan_tidak_lengkap, "");
            //}

            //if ((enroll.KtpNIK.Contains('"') || enroll.KtpNama.Contains('"') || enroll.KtpTTL.Contains('"') || enroll.KtpTempatLahir.Contains('"') || enroll.KtpTanggalLahir.Contains('"') || enroll.KtpGolonganDarah.Contains('"') || enroll.KtpJanisKelamin.Contains('"') || enroll.KtpAlamat.Contains('"') || enroll.KtpRTRW.Contains('"') || enroll.KtpRT.Contains('"') || enroll.KtpRW.Contains('"') || enroll.KtpKelurahan.Contains('"') || enroll.KtpKecamatan.Contains('"') || enroll.KtpKota.Contains('"') || enroll.KtpProvinsi.Contains('"') || enroll.KtpAgama.Contains('"') || enroll.KtpStatusPerkawinan.Contains('"') || enroll.KtpPekerjaan.Contains('"') || enroll.KtpKewarganegaraan.Contains('"') || enroll.KtpMasaBerlaku.Contains('"') || enroll.KtpLatitude.Contains('"') || enroll.KtpLongitude.Contains('"') || enroll.KtpAlamatConvertLatlong.Contains('"') || enroll.KtpAlamatConvertLengkap.Contains('"') || enroll.KtpPhotoKTP.Contains('"') || enroll.KtpFingerKanan.Contains('"') || enroll.KtpFingerKiri.Contains('"') || enroll.KtpPhotoCam.Contains('"') || enroll.KtpSignature.Contains('"') || enroll.KtpKodePos.Contains('"') || enroll.UID.Contains('"') || enroll.KtpTypeJariKanan.Contains('"') || enroll.KtpTypeJariKanan.Contains('"')))
            //{
            //    return (_ErrorMessageConfig.InputTidakLengkap, (int)EnrollStatus.Inputan_tidak_lengkap, "");
            //}

            //#endregion

            if (enroll.KtpTanggalLahir != null)
            {
                enroll.KtpTanggalLahir = enroll.KtpTanggalLahir.Replace(".", "");
                if (enroll.KtpTanggalLahir.Contains("/"))
                {
                    enroll.KtpTanggalLahir = enroll.KtpTanggalLahir.Replace("/", "-");
                }
                string[] arrayTanggl = enroll.KtpTanggalLahir.Split("-");
                string hari = "";
                string bulan = "";
                string tahun = "";

                if (arrayTanggl != null)
                {
                    if (arrayTanggl[0] != null)
                    {
                        if (arrayTanggl[0].Length != 2)
                        {
                            hari = "0" + arrayTanggl[0];

                        }
                        else
                        {
                            hari = arrayTanggl[0];

                        }
                    }

                    if (arrayTanggl[1] != null)
                    {
                        if (arrayTanggl[1].Length != 2)
                        {
                            bulan = "0" + arrayTanggl[1];

                        }
                        else
                        {
                            bulan = arrayTanggl[1];

                        }
                    }

                    if (arrayTanggl[2] != null)
                    {
                        tahun = arrayTanggl[2];
                    }
                }

                enroll.KtpTanggalLahir = hari + "-" + bulan + "-" + tahun;
            }

            const bool IsNewEnroll = true;
            string JamServer = DateTime.Now.ToString("ddMMyyyyHHmmss");
            bool IsNasabah = false;

            var cekKTP = await _profileRepository.GetDataDemografis(enroll.KtpNIK)
                .ConfigureAwait(false);

            var cekKTPTemp = await _profileRepository.GetDataDemografisTempOnProgress(enroll.KtpNIK)
                .ConfigureAwait(false); ;

            if (cekKTP != null)
            {
                var logDemografi = new Tbl_DataKTP_Demografis_Log
                {
                    Agama = cekKTP.Agama,
                    Alamat = cekKTP.Alamat,
                    AlamatGoogle = cekKTP.AlamatGoogle,
                    AlamatLengkap = cekKTP.AlamatLengkap,
                    CreatedById = cekKTP.CreatedById,
                    CreatedByUID = cekKTP.CreatedByUID,
                    CreatedByUnitId = cekKTP.CreatedByUnitId,
                    CreatedTime = cekKTP.CreatedTime,
                    Desa = cekKTP.Desa,
                    GolonganDarah = cekKTP.GolonganDarah,
                    JenisKelamin = cekKTP.JenisKelamin,
                    Kecamatan = cekKTP.Kecamatan,
                    Kelurahan = cekKTP.Kelurahan,
                    Kewarganegaraan = cekKTP.Kewarganegaraan,
                    KodePos = cekKTP.KodePos,
                    Kota = cekKTP.Kota,
                    Latitude = cekKTP.Latitude,
                    Longitude = cekKTP.Longitude,
                    MasaBerlaku = cekKTP.MasaBerlaku,
                    Nama = cekKTP.Nama,
                    NIK = cekKTP.NIK,
                    Pekerjaan = cekKTP.Pekerjaan,
                    Provinsi = cekKTP.Provinsi,
                    RT = cekKTP.RT,
                    RW = cekKTP.RW,
                    StatusPerkawinan = cekKTP.StatusPerkawinan,
                    TanggalLahir = cekKTP.TanggalLahir,
                    TempatLahir = cekKTP.TempatLahir,
                    CIF = cekKTP.CIF,
                    CreatedByNpp = cekKTP.CreatedByNpp
                };

                var stringPerubahan = "";

                if (cekKTP.Nama != enroll.KtpNama)
                {
                    stringPerubahan = stringPerubahan + "NAMA : " + cekKTP.Nama + " -> " + enroll.KtpNama + " <br/>";
                    cekKTP.Nama = enroll.KtpNama;
                }

                if (cekKTP.TempatLahir != enroll.KtpTempatLahir)
                {
                    stringPerubahan = stringPerubahan + "TEMPAT LAHIR : " + cekKTP.TempatLahir + " -> " + enroll.KtpTempatLahir + " <br/>";
                    cekKTP.TempatLahir = enroll.KtpTempatLahir;
                }

                if (enroll.KtpTanggalLahir != null)
                {
                    var _ttl = DateTime.ParseExact(enroll.KtpTanggalLahir, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                    if (cekKTP.TanggalLahir != _ttl)
                    {
                        stringPerubahan = stringPerubahan + "TANGGAL LAHIR : " + cekKTP.TanggalLahir + " -> " + enroll.KtpTanggalLahir + " <br/>";
                        cekKTP.TanggalLahir = DateTime.ParseExact(enroll.KtpTanggalLahir, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                    }
                }

                if (cekKTP.JenisKelamin != enroll.KtpJanisKelamin)
                {
                    stringPerubahan = stringPerubahan + "JENIS KELAMIN : " + cekKTP.JenisKelamin + " -> " + enroll.KtpJanisKelamin + " <br/>";
                    cekKTP.JenisKelamin = enroll.KtpJanisKelamin;
                }

                if (cekKTP.GolonganDarah != enroll.KtpGolonganDarah)
                {
                    stringPerubahan = stringPerubahan + "GOLONGAN DARAH : " + cekKTP.GolonganDarah + " -> " + enroll.KtpGolonganDarah + " <br/>";
                    cekKTP.GolonganDarah = enroll.KtpGolonganDarah;
                }

                if (cekKTP.Alamat != enroll.KtpAlamat)
                {
                    stringPerubahan = stringPerubahan + "ALAMAT : " + cekKTP.Alamat + " -> " + enroll.KtpAlamat + " <br/>";
                    cekKTP.Alamat = enroll.KtpAlamat;
                }

                if (cekKTP.RT != enroll.KtpRT)
                {
                    stringPerubahan = stringPerubahan + "RT : " + cekKTP.RT + " -> " + enroll.KtpRT + " <br/>";
                    cekKTP.RT = enroll.KtpRT;
                }

                if (cekKTP.RW != enroll.KtpRW)
                {
                    stringPerubahan = stringPerubahan + "RW : " + cekKTP.RW + " -> " + enroll.KtpRW + " <br/>";
                    cekKTP.RW = enroll.KtpRW;
                }

                if (cekKTP.Kelurahan != enroll.KtpKelurahan)
                {
                    stringPerubahan = stringPerubahan + "KELURAHAN : " + cekKTP.Kelurahan + " -> " + enroll.KtpKelurahan + " <br/>";
                    cekKTP.Kelurahan = enroll.KtpKelurahan;
                }

                if (cekKTP.Kecamatan != enroll.KtpKecamatan)
                {
                    stringPerubahan = stringPerubahan + "KECAMATAN : " + cekKTP.Kecamatan + " -> " + enroll.KtpKecamatan + " <br/>";
                    cekKTP.Kecamatan = enroll.KtpKecamatan;
                }

                if (cekKTP.Kota != enroll.KtpKota)
                {
                    stringPerubahan = stringPerubahan + "KOTA/KABUPATEN : " + cekKTP.Kota + " -> " + enroll.KtpKota + " <br/>";
                    cekKTP.Kota = enroll.KtpKota;
                }

                if (cekKTP.Provinsi != enroll.KtpProvinsi)
                {
                    stringPerubahan = stringPerubahan + "PROVINSI  : " + cekKTP.Provinsi + " -> " + enroll.KtpProvinsi + " <br/>";
                    cekKTP.Provinsi = enroll.KtpProvinsi;
                }

                if (cekKTP.Agama != enroll.KtpAgama)
                {
                    stringPerubahan = stringPerubahan + "AGAMA  : " + cekKTP.Agama + " -> " + enroll.KtpAgama + " <br/>";
                    cekKTP.Agama = enroll.KtpAgama;
                }

                if (cekKTP.StatusPerkawinan != enroll.KtpStatusPerkawinan)
                {
                    stringPerubahan = stringPerubahan + "STATUS PERKAWINAN  : " + cekKTP.Agama + " -> " + enroll.KtpAgama + " <br/>";
                    cekKTP.StatusPerkawinan = enroll.KtpStatusPerkawinan;
                }

                if (cekKTP.Pekerjaan != enroll.KtpPekerjaan)
                {
                    stringPerubahan = stringPerubahan + "PEKERJAAN  : " + cekKTP.Agama + " -> " + enroll.KtpAgama + " <br/>";
                    cekKTP.Pekerjaan = enroll.KtpPekerjaan;
                }

                if (cekKTP.Kewarganegaraan != enroll.KtpKewarganegaraan)
                {
                    stringPerubahan = stringPerubahan + "KEWARGANEGARAAN  : " + cekKTP.Kewarganegaraan + " -> " + enroll.KtpKewarganegaraan + " <br/>";
                    cekKTP.Kewarganegaraan = enroll.KtpKewarganegaraan;
                }
                if (cekKTP.MasaBerlaku != enroll.KtpMasaBerlaku)
                {
                    stringPerubahan = stringPerubahan + "MASA BERLAKU  : " + cekKTP.MasaBerlaku + " -> " + enroll.KtpMasaBerlaku + " <br/>";
                    cekKTP.MasaBerlaku = enroll.KtpMasaBerlaku;
                }

                cekKTP.UpdatedTime = DateTime.Now;
                cekKTP.UpdatedById = Id;
                cekKTP.UpdatedByUID = enroll.UID;
                cekKTP.UpdatedByUnitCode = unitCode;
                cekKTP.UpdatedByUnitId = unitId;
                cekKTP.isEnrollFR = false;

                await _profileRepository.UpdateDataDemografis(cekKTP)
                    .ConfigureAwait(false);

                if (string.IsNullOrWhiteSpace(stringPerubahan))
                {
                    return (_ErrorMessageConfig.NasabahPernahEnroll, (int)EnrollStatus.Sudah_di_enroll_sebelumnya, "");
                }

                _profileRepository.InsertDemografiLog(logDemografi);

                return (stringPerubahan, (int)EnrollStatus.Berhasil_mengubah_beberapa_perubahan, "");
            }

            if (cekKTPTemp != null)
            {
                var logDemografi = new Tbl_DataKTP_Demografis_Log
                {
                    Agama = cekKTPTemp.Agama,
                    Alamat = cekKTPTemp.Alamat,
                    AlamatGoogle = cekKTPTemp.AlamatGoogle,
                    AlamatLengkap = cekKTPTemp.AlamatLengkap,
                    CreatedById = cekKTPTemp.CreatedById,
                    CreatedByUID = cekKTPTemp.CreatedByUID,
                    CreatedByUnitId = cekKTPTemp.CreatedByUnitId,
                    CreatedTime = cekKTPTemp.CreatedTime,
                    Desa = cekKTPTemp.Desa,
                    GolonganDarah = cekKTPTemp.GolonganDarah,
                    JenisKelamin = cekKTPTemp.JenisKelamin,
                    Kecamatan = cekKTPTemp.Kecamatan,
                    Kelurahan = cekKTPTemp.Kelurahan,
                    Kewarganegaraan = cekKTPTemp.Kewarganegaraan,
                    KodePos = cekKTPTemp.KodePos,
                    Kota = cekKTPTemp.Kota,
                    Latitude = cekKTPTemp.Latitude,
                    Longitude = cekKTPTemp.Longitude,
                    MasaBerlaku = cekKTPTemp.MasaBerlaku,
                    Nama = cekKTPTemp.Nama,
                    NIK = cekKTPTemp.NIK,
                    Pekerjaan = cekKTPTemp.Pekerjaan,
                    Provinsi = cekKTPTemp.Provinsi,
                    RT = cekKTPTemp.RT,
                    RW = cekKTPTemp.RW,
                    StatusPerkawinan = cekKTPTemp.StatusPerkawinan,
                    TanggalLahir = cekKTPTemp.TanggalLahir,
                    TempatLahir = cekKTPTemp.TempatLahir,
                    CIF = cekKTPTemp.CIF,
                    CreatedByNpp = cekKTPTemp.CreatedByNpp
                };

                var stringPerubahan = "";

                if (cekKTPTemp.Nama != enroll.KtpNama)
                {
                    stringPerubahan = stringPerubahan + "NAMA : " + cekKTPTemp.Nama + " -> " + enroll.KtpNama + " <br/>";
                    cekKTPTemp.Nama = enroll.KtpNama;
                }

                if (cekKTPTemp.TempatLahir != enroll.KtpTempatLahir)
                {
                    stringPerubahan = stringPerubahan + "TEMPAT LAHIR : " + cekKTPTemp.TempatLahir + " -> " + enroll.KtpTempatLahir + " <br/>";
                    cekKTPTemp.TempatLahir = enroll.KtpTempatLahir;
                }

                if (enroll.KtpTanggalLahir != null)
                {
                    var _ttl = DateTime.ParseExact(enroll.KtpTanggalLahir, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                    if (cekKTPTemp.TanggalLahir != _ttl)
                    {
                        stringPerubahan = stringPerubahan + "TANGGAL LAHIR : " + cekKTPTemp.TanggalLahir + " -> " + enroll.KtpTanggalLahir + " <br/>";
                        cekKTPTemp.TanggalLahir = DateTime.ParseExact(enroll.KtpTanggalLahir, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                    }
                }

                if (cekKTPTemp.JenisKelamin != enroll.KtpJanisKelamin)
                {
                    stringPerubahan = stringPerubahan + "JENIS KELAMIN : " + cekKTPTemp.JenisKelamin + " -> " + enroll.KtpJanisKelamin + " <br/>";
                    cekKTPTemp.JenisKelamin = enroll.KtpJanisKelamin;
                }

                if (cekKTPTemp.GolonganDarah != enroll.KtpGolonganDarah)
                {
                    stringPerubahan = stringPerubahan + "GOLONGAN DARAH : " + cekKTPTemp.GolonganDarah + " -> " + enroll.KtpGolonganDarah + " <br/>";
                    cekKTPTemp.GolonganDarah = enroll.KtpGolonganDarah;
                }

                if (cekKTPTemp.Alamat != enroll.KtpAlamat)
                {
                    stringPerubahan = stringPerubahan + "ALAMAT : " + cekKTPTemp.Alamat + " -> " + enroll.KtpAlamat + " <br/>";
                    cekKTPTemp.Alamat = enroll.KtpAlamat;
                }

                if (cekKTPTemp.RT != enroll.KtpRT)
                {
                    stringPerubahan = stringPerubahan + "RT : " + cekKTPTemp.RT + " -> " + enroll.KtpRT + " <br/>";
                    cekKTPTemp.RT = enroll.KtpRT;
                }

                if (cekKTPTemp.RW != enroll.KtpRW)
                {
                    stringPerubahan = stringPerubahan + "RW : " + cekKTPTemp.RW + " -> " + enroll.KtpRW + " <br/>";
                    cekKTPTemp.RW = enroll.KtpRW;
                }

                if (cekKTPTemp.Kelurahan != enroll.KtpKelurahan)
                {
                    stringPerubahan = stringPerubahan + "KELURAHAN : " + cekKTPTemp.Kelurahan + " -> " + enroll.KtpKelurahan + " <br/>";
                    cekKTPTemp.Kelurahan = enroll.KtpKelurahan;
                }

                if (cekKTPTemp.Kecamatan != enroll.KtpKecamatan)
                {
                    stringPerubahan = stringPerubahan + "KECAMATAN : " + cekKTPTemp.Kecamatan + " -> " + enroll.KtpKecamatan + " <br/>";
                    cekKTPTemp.Kecamatan = enroll.KtpKecamatan;
                }

                if (cekKTPTemp.Kota != enroll.KtpKota)
                {
                    stringPerubahan = stringPerubahan + "KOTA/KABUPATEN : " + cekKTPTemp.Kota + " -> " + enroll.KtpKota + " <br/>";
                    cekKTPTemp.Kota = enroll.KtpKota;
                }

                if (cekKTPTemp.Provinsi != enroll.KtpProvinsi)
                {
                    stringPerubahan = stringPerubahan + "PROVINSI  : " + cekKTPTemp.Provinsi + " -> " + enroll.KtpProvinsi + " <br/>";
                    cekKTPTemp.Provinsi = enroll.KtpProvinsi;
                }

                if (cekKTPTemp.Agama != enroll.KtpAgama)
                {
                    stringPerubahan = stringPerubahan + "AGAMA  : " + cekKTPTemp.Agama + " -> " + enroll.KtpAgama + " <br/>";
                    cekKTPTemp.Agama = enroll.KtpAgama;
                }

                if (cekKTPTemp.StatusPerkawinan != enroll.KtpStatusPerkawinan)
                {
                    stringPerubahan = stringPerubahan + "STATUS PERKAWINAN  : " + cekKTPTemp.Agama + " -> " + enroll.KtpAgama + " <br/>";
                    cekKTPTemp.StatusPerkawinan = enroll.KtpStatusPerkawinan;
                }

                if (cekKTPTemp.Pekerjaan != enroll.KtpPekerjaan)
                {
                    stringPerubahan = stringPerubahan + "PEKERJAAN  : " + cekKTPTemp.Agama + " -> " + enroll.KtpAgama + " <br/>";
                    cekKTPTemp.Pekerjaan = enroll.KtpPekerjaan;
                }

                if (cekKTPTemp.Kewarganegaraan != enroll.KtpKewarganegaraan)
                {
                    stringPerubahan = stringPerubahan + "KEWARGANEGARAAN  : " + cekKTPTemp.Kewarganegaraan + " -> " + enroll.KtpKewarganegaraan + " <br/>";
                    cekKTPTemp.Kewarganegaraan = enroll.KtpKewarganegaraan;
                }
                if (cekKTPTemp.MasaBerlaku != enroll.KtpMasaBerlaku)
                {
                    stringPerubahan = stringPerubahan + "MASA BERLAKU  : " + cekKTPTemp.MasaBerlaku + " -> " + enroll.KtpMasaBerlaku + " <br/>";
                    cekKTPTemp.MasaBerlaku = enroll.KtpMasaBerlaku;
                }

                var dataKTPTemp = new Tbl_DataKTP_Demografis
                {
                    Agama = cekKTPTemp.Agama,
                    Alamat = cekKTPTemp.Alamat,
                    AlamatGoogle = cekKTPTemp.AlamatGoogle,
                    AlamatLengkap = cekKTPTemp.AlamatLengkap,
                    CreatedById = cekKTPTemp.CreatedById,
                    CreatedByUID = cekKTPTemp.CreatedByUID,
                    CreatedByUnitId = cekKTPTemp.CreatedByUnitId,
                    CreatedTime = cekKTPTemp.CreatedTime,
                    Desa = cekKTPTemp.Desa,
                    GolonganDarah = cekKTPTemp.GolonganDarah,
                    JenisKelamin = cekKTPTemp.JenisKelamin,
                    Kecamatan = cekKTPTemp.Kecamatan,
                    Kelurahan = cekKTPTemp.Kelurahan,
                    Kewarganegaraan = cekKTPTemp.Kewarganegaraan,
                    KodePos = cekKTPTemp.KodePos,
                    Kota = cekKTPTemp.Kota,
                    Latitude = cekKTPTemp.Latitude,
                    Longitude = cekKTPTemp.Longitude,
                    MasaBerlaku = cekKTPTemp.MasaBerlaku,
                    Nama = cekKTPTemp.Nama,
                    NIK = cekKTPTemp.NIK,
                    Pekerjaan = cekKTPTemp.Pekerjaan,
                    Provinsi = cekKTPTemp.Provinsi,
                    RT = cekKTPTemp.RT,
                    RW = cekKTPTemp.RW,
                    StatusPerkawinan = cekKTPTemp.StatusPerkawinan,
                    TanggalLahir = cekKTPTemp.TanggalLahir,
                    TempatLahir = cekKTPTemp.TempatLahir,
                    CIF = cekKTPTemp.CIF,
                    CreatedByNpp = cekKTPTemp.CreatedByNpp,
                    isEnrollFR = true
                };


                cekKTPTemp.UpdatedTime = DateTime.Now;
                cekKTPTemp.UpdatedById = Id;
                cekKTPTemp.UpdatedByUID = enroll.UID;
                cekKTPTemp.UpdatedByUnitCode = unitCode;
                cekKTPTemp.UpdatedByUnitId = unitId;
                //cekKTPTemp.isEnrollFR = true;

                await _profileRepository.UpdateDataDemografis(dataKTPTemp)
                    .ConfigureAwait(false);

                if (string.IsNullOrWhiteSpace(stringPerubahan))
                {
                    return (_ErrorMessageConfig.NasabahPernahEnroll, (int)EnrollStatus.Sudah_di_enroll_sebelumnya, "");
                }

                _profileRepository.InsertDemografiLog(logDemografi);

                return (stringPerubahan, (int)EnrollStatus.Berhasil_mengubah_beberapa_perubahan, "");
            }

            if (IsNewEnroll)
            {
                if (enroll?.KtpNIK == null) return (_ErrorMessageConfig.InputTidakLengkap, (int)EnrollStatus.Inputan_tidak_lengkap, "");

                var cifData = new ApiSOAResponse();
                #region Hit SOA And Loggging it
                if (isHitSOA == true)
                {
                    cifData = await _cifService.GetSOAByCif(ReqSoa)
                    .ConfigureAwait(false);

                    var status = 0;
                    if (cifData.cif != null)
                    {
                        status = 1;
                    }

                    var _log = new Tbl_ThirdPartyLog
                    {
                        FeatureName = "SubmitEnrollmentFingerEncryptedOnly",
                        HostUrl = ReqSoa.host,
                        Request = Newtonsoft.Json.JsonConvert.SerializeObject(ReqSoa),
                        Status = status,
                        Response = Newtonsoft.Json.JsonConvert.SerializeObject(cifData),
                        CreatedDate = System.DateTime.Now,
                        CreatedBy = npp
                    };

                    _ = _enrollmentKTPRepository.CreateThirdPartyLog(_log);
                }
                else
                {
                    var res = await _cifService.GetCIF(new NikDto { Nik = enroll.KtpNIK },
                        new NikDtoUrl { baseUrl = ReqSoa.baseUrlNonSoa, endpoint = ReqSoa.UrlEndPointNonSoa });
                    if (res.Data == null)
                    {
                        cifData.cif = null;
                    }
                    else
                    {
                        cifData.cif = res.Data.Cif;
                    };
                }
                #endregion

                if (!String.IsNullOrEmpty(cifData.cif))
                {
                    IsNasabah = true;
                    cifData.cif = cifData.cif.Trim();
                }

                var sysPathFolder = await _sysParameterRepository.GetPathFolder("PathFolderData");
                string pathFolder = sysPathFolder.Value;

                var dataDemografis = new Tbl_DataKTP_Demografis();

                var dataDemografisLog = new Tbl_DataKTP_Demografis_Log();

                var dataNpp = new Tbl_Mapping_Pegawai_KTP();

                if (cekKTP != null)
                {
                    dataDemografisLog = new Tbl_DataKTP_Demografis_Log
                    {
                        Agama = cekKTP.Agama,
                        Alamat = cekKTP.Alamat,
                        AlamatGoogle = cekKTP.AlamatGoogle,
                        AlamatLengkap = cekKTP.AlamatLengkap,
                        CreatedById = cekKTP.CreatedById,
                        CreatedByUID = cekKTP.CreatedByUID,
                        CreatedByUnitId = cekKTP.CreatedByUnitId,
                        CreatedTime = cekKTP.CreatedTime,
                        Desa = cekKTP.Desa,
                        GolonganDarah = cekKTP.GolonganDarah,
                        JenisKelamin = cekKTP.JenisKelamin,
                        Kecamatan = cekKTP.Kecamatan,
                        Kelurahan = cekKTP.Kelurahan,
                        Kewarganegaraan = cekKTP.Kewarganegaraan,
                        KodePos = cekKTP.KodePos,
                        Kota = cekKTP.Kota,
                        Latitude = cekKTP.Latitude,
                        Longitude = cekKTP.Longitude,
                        MasaBerlaku = cekKTP.MasaBerlaku,
                        Nama = cekKTP.Nama,
                        NIK = cekKTP.NIK,
                        Pekerjaan = cekKTP.Pekerjaan,
                        Provinsi = cekKTP.Provinsi,
                        RT = cekKTP.RT,
                        RW = cekKTP.RW,
                        StatusPerkawinan = cekKTP.StatusPerkawinan,
                        TanggalLahir = cekKTP.TanggalLahir,
                        TempatLahir = cekKTP.TempatLahir
                    };

                    cekKTP.Agama = enroll.KtpAgama;
                    cekKTP.Alamat = enroll.KtpAlamat;
                    cekKTP.AlamatGoogle = enroll.KtpAlamatConvertLatlong;
                    cekKTP.AlamatLengkap = enroll.KtpAlamatConvertLengkap;
                    if (string.IsNullOrWhiteSpace(cifData.cif))
                    {
                        cekKTP.CIF = cifData.cif;
                    }
                    cekKTP.GolonganDarah = enroll.KtpGolonganDarah;
                    cekKTP.JenisKelamin = enroll.KtpJanisKelamin;
                    cekKTP.Kecamatan = enroll.KtpKecamatan;
                    cekKTP.Kelurahan = enroll.KtpKelurahan;
                    cekKTP.Kewarganegaraan = enroll.KtpKewarganegaraan;
                    cekKTP.KodePos = enroll.KtpKodePos;
                    cekKTP.Kota = enroll.KtpKota;
                    cekKTP.Latitude = enroll.KtpLatitude;
                    cekKTP.Longitude = enroll.KtpLongitude;
                    cekKTP.MasaBerlaku = enroll.KtpMasaBerlaku;
                    cekKTP.Nama = enroll.KtpNama;
                    cekKTP.NIK = enroll.KtpNIK;
                    cekKTP.Pekerjaan = enroll.KtpPekerjaan;
                    cekKTP.Provinsi = enroll.KtpProvinsi;
                    cekKTP.RT = enroll.KtpRT;
                    cekKTP.RW = enroll.KtpRW;
                    cekKTP.StatusPerkawinan = enroll.KtpStatusPerkawinan;
                    cekKTP.TanggalLahir = string.IsNullOrWhiteSpace(enroll.KtpTanggalLahir) ? null : DateTime.ParseExact(enroll.KtpTanggalLahir, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                    cekKTP.TempatLahir = enroll.KtpTempatLahir;
                    cekKTP.UpdatedById = Id;
                    cekKTP.UpdatedByNpp = npp;
                    cekKTP.UpdatedByUID = enroll.UID;
                    cekKTP.UpdatedTime = DateTime.Now;
                    cekKTP.UpdatedByUnitCode = unitCode;
                    cekKTP.UpdatedByUnitId = unitId;
                }
                else
                {
                    dataDemografis = new Tbl_DataKTP_Demografis
                    {
                        Agama = enroll.KtpAgama,
                        Alamat = enroll.KtpAlamat,
                        AlamatGoogle = enroll.KtpAlamatConvertLatlong,
                        AlamatLengkap = enroll.KtpAlamatConvertLengkap,
                        CIF = (cifData.cif),
                        NIK = enroll.KtpNIK,
                        CreatedById = Id,
                        IsActive = true,
                        IsDeleted = false,
                        JenisKelamin = enroll.KtpJanisKelamin,
                        Kecamatan = enroll.KtpKecamatan,
                        Kelurahan = enroll.KtpKelurahan,
                        CreatedByUID = enroll.UID,
                        CreatedTime = DateTime.Now,
                        GolonganDarah = enroll.KtpGolonganDarah,
                        Kewarganegaraan = enroll.KtpKewarganegaraan,
                        KodePos = enroll.KtpKodePos,
                        Kota = enroll.KtpKota,
                        Latitude = enroll.KtpLatitude,
                        Longitude = enroll.KtpLongitude,
                        MasaBerlaku = enroll.KtpMasaBerlaku,
                        Nama = enroll.KtpNama,
                        Pekerjaan = enroll.KtpPekerjaan,
                        Provinsi = enroll.KtpProvinsi,
                        RT = enroll.KtpRT,
                        RW = enroll.KtpRW,
                        StatusPerkawinan = enroll.KtpStatusPerkawinan,
                        TanggalLahir = string.IsNullOrWhiteSpace(enroll.KtpTanggalLahir) ? null : DateTime.ParseExact(enroll.KtpTanggalLahir, "dd-MM-yyyy", CultureInfo.InvariantCulture),
                        TempatLahir = enroll.KtpTempatLahir,
                        CreatedByNpp = npp,
                        CreatedByUnitCode = unitCode,
                        CreatedByUnitId = unitId,
                        IsVerified = false,
                        IsNasabahTemp = string.IsNullOrWhiteSpace(cifData.cif),
                        isEnrollFR = false
                    };

                    if (isEmployee)
                    {
                        dataNpp = new Tbl_Mapping_Pegawai_KTP
                        {
                            CreatedById = Id,
                            NIK = enroll.KtpNIK,
                            Npp = _empData.Npp,
                            CreatedByNpp = npp,
                            CreatedByUID = enroll.UID,
                            CreatedTime = DateTime.Now
                        };
                    }
                }

                var exPhotoKtp = new Tbl_DataKTP_Photo();

                var photoKtpLog = new Tbl_DataKTP_Photo_Log();

                var photoKtp = new Tbl_DataKTP_Photo();

                if (!string.IsNullOrWhiteSpace(enroll.KtpPhotoKTP))
                {
                    var photoKTPData = await _profileRepository.GetPhotoKtp(enroll.KtpNIK)
                        .ConfigureAwait(false);

                    byte[] imgBytes = Convert.FromBase64String(enroll.KtpPhotoKTP);

                    string filePath = "";

                    var systemParameterPath = await _sysParameterRepository.GetPathFolder("FolderPhoto")
                                                .ConfigureAwait(false);
                    string pathFolderFoto = systemParameterPath.Value;

                    string subPathFolderFoto = pathFolder + "/" + pathFolderFoto + "/" + enroll.KtpNIK + "/";

                    string fileName = "Foto_" + enroll.KtpNIK + "_" + JamServer + ".jpg";

                    if (_sftpConfig.IsActive)
                    {
                        using var stream = new MemoryStream(imgBytes);

                        (var fname, var fPath) = await stream.UploadToFTPServer(subPathFolderFoto, fileName, _sftpConfig.Host,
                            _sftpConfig.Username, _sftpConfig.Password, _sftpConfig.RootDirectory, _sftpConfig.Url).ConfigureAwait(false);

                        filePath = fPath;
                    }
                    else
                    {

                        if (!Directory.Exists(subPathFolderFoto))
                        {
                            Directory.CreateDirectory(subPathFolderFoto);
                        }

                        filePath = subPathFolderFoto + fileName;
                        File.WriteAllBytes(filePath, imgBytes);
                    }

                    if (photoKTPData != null)
                    {
                        photoKtpLog.CreatedById = photoKTPData.CreatedById;
                        photoKtpLog.CreatedByNpp = photoKTPData.CreatedByNpp;
                        photoKtpLog.CreatedByUid = photoKTPData.CreatedByUid;
                        photoKtpLog.CreatedByUnit = photoKTPData.CreatedByUnit;
                        photoKtpLog.CreatedTime = photoKTPData.CreatedTime;
                        photoKtpLog.FileName = photoKTPData.FileName;
                        photoKtpLog.Nik = photoKTPData.Nik;
                        photoKtpLog.PathFile = photoKTPData.PathFile;

                        exPhotoKtp = photoKTPData;
                    }

                    photoKtp = new Tbl_DataKTP_Photo
                    {
                        PathFile = filePath,
                        Nik = enroll.KtpNIK,
                        FileName = fileName,
                        IsActive = true,
                        IsDeleted = false,
                        CreatedById = Id,
                        CreatedByNpp = npp,
                        CreatedByUid = enroll.UID,
                        CreatedTime = DateTime.Now
                    };
                }

                var exPhotoSignature = new Tbl_DataKTP_Signature();

                var photoSignatureLog = new Tbl_DataKTP_Signature_Log();

                var photoSignature = new Tbl_DataKTP_Signature();

                if (!string.IsNullOrWhiteSpace(enroll.KtpSignature))
                {
                    var photoSignatureData = await _profileRepository.GetPhotoSignature(enroll.KtpNIK)
                        .ConfigureAwait(false);

                    byte[] imgBytes = Convert.FromBase64String(enroll.KtpSignature);

                    string filePath = "";

                    var systemParameterPath = await _sysParameterRepository.GetPathFolder("FolderSignature")
                                                .ConfigureAwait(false);
                    string pathFolderFoto = systemParameterPath.Value;

                    string subPathFolderSignature = pathFolder + "/" + pathFolderFoto + "/" + enroll.KtpNIK + "/";
                    //string subPathFolderSignature = pathFolder + pathFolderFoto + enroll.KtpNIK;

                    string fileName = "Signature_" + enroll.KtpNIK + "_" + JamServer + ".jpg";

                    if (_sftpConfig.IsActive)
                    {
                        using var stream = new MemoryStream(imgBytes);

                        (var fname, var fPath) = await stream.UploadToFTPServer(subPathFolderSignature, fileName, _sftpConfig.Host,
                            _sftpConfig.Username, _sftpConfig.Password, _sftpConfig.RootDirectory, _sftpConfig.Url).ConfigureAwait(false);

                        filePath = fPath;
                    }
                    else
                    {
                        if (!Directory.Exists(subPathFolderSignature))
                        {
                            Directory.CreateDirectory(subPathFolderSignature);
                        }

                        filePath = subPathFolderSignature + fileName;
                        File.WriteAllBytes(filePath, imgBytes);
                    }

                    if (photoSignatureData != null)
                    {
                        photoSignatureLog.CreatedById = photoSignatureData.CreatedById;
                        photoSignatureLog.CreatedByNpp = photoSignatureData.CreatedByNpp;
                        photoSignatureLog.CreatedByUid = photoSignatureData.CreatedByUid;
                        photoSignatureLog.CreatedByUnit = photoSignatureData.CreatedByUnit;
                        photoSignatureLog.CreatedTime = photoSignatureData.CreatedTime;
                        photoSignatureLog.FileName = photoSignatureData.FileName;
                        photoSignatureLog.Nik = photoSignatureData.Nik;
                        photoSignatureLog.PathFile = photoSignatureData.PathFile;

                        exPhotoSignature = photoSignatureData;
                    }

                    photoSignature = new Tbl_DataKTP_Signature
                    {
                        CreatedById = Id,
                        CreatedByNpp = npp,
                        IsActive = true,
                        IsDeleted = false,
                        Nik = enroll.KtpNIK,
                        FileName = fileName,
                        CreatedByUid = enroll.UID,
                        CreatedTime = DateTime.Now,
                        PathFile = filePath
                    };
                }

                var exPhotoCam = new Tbl_DataKTP_PhotoCam();

                var photoCamLog = new Tbl_DataKTP_PhotoCam_Log();

                var photoCam = new Tbl_DataKTP_PhotoCam();

                if (!string.IsNullOrWhiteSpace(enroll.KtpPhotoCam))
                {
                    var photoCamData = await _profileRepository.GetPhotoCam(enroll.KtpNIK)
                        .ConfigureAwait(false);

                    byte[] imgBytes = Convert.FromBase64String(enroll.KtpPhotoCam);

                    string filePath = "";

                    var systemParameterPath = await _sysParameterRepository.GetPathFolder("FolderWebcam")
                                                .ConfigureAwait(false);
                    string pathFolderFoto = systemParameterPath.Value;

                    string SubPathFolderPhotoCam = pathFolder + "/" + pathFolderFoto + "/" + enroll.KtpNIK + "/";
                    //string SubPathFolderPhotoCam = pathFolder + pathFolderFoto + enroll.KtpNIK;

                    string fileName = "PhotoCam_" + enroll.KtpNIK + "_" + JamServer + ".txt";

                    if (_sftpConfig.IsActive)
                    {
                        using var stream = new MemoryStream(imgBytes);

                        (var fname, var fPath) = await stream.UploadToFTPServer(SubPathFolderPhotoCam, fileName, _sftpConfig.Host,
                            _sftpConfig.Username, _sftpConfig.Password, _sftpConfig.RootDirectory, _sftpConfig.Url).ConfigureAwait(false);

                        filePath = fPath;
                    }
                    else
                    {
                        if (!Directory.Exists(SubPathFolderPhotoCam))
                        {
                            Directory.CreateDirectory(SubPathFolderPhotoCam);
                        }

                        filePath = SubPathFolderPhotoCam + fileName;
                        File.WriteAllBytes(filePath, imgBytes);
                    }

                    if (photoCamData != null)
                    {
                        photoCamLog.CreatedById = photoCamData.CreatedById;
                        photoCamLog.CreatedByNpp = photoCamData.CreatedByNpp;
                        photoCamLog.CreatedByUid = photoCamData.CreatedByUid;
                        photoCamLog.CreatedByUnit = photoCamData.CreatedByUnit;
                        photoCamLog.CreatedTime = photoCamData.CreatedTime;
                        photoCamLog.FileName = photoCamData.FileName;
                        photoCamLog.Nik = photoCamData.Nik;
                        photoCamLog.PathFile = photoCamData.PathFile;

                        exPhotoCam = photoCamData;
                    }

                    photoCam = new Tbl_DataKTP_PhotoCam
                    {
                        PathFile = filePath,//
                        Nik = enroll.KtpNIK,
                        FileName = fileName,//
                        CreatedTime = DateTime.Now,
                        CreatedById = Id,
                        IsActive = true,
                        IsDeleted = false,
                        CreatedByNpp = npp,
                        CreatedByUid = enroll.UID
                    };
                }

                var exPhotoFinger = new List<Tbl_DataKTP_Finger>();

                var exPhotoFingerEmployee = new List<Tbl_DataKTP_Finger_Employee>();

                var photoFingerLogs = new List<Tbl_DataKTP_Finger_Log>();

                var photoFingers = new List<Tbl_DataKTP_Finger>();

                var photoFingersEmployee = new List<Tbl_DataKTP_Finger_Employee>();

                if (!string.IsNullOrWhiteSpace(enroll.KtpFingerKanan))
                {

                    var photoFingerDataEmployee = new Tbl_DataKTP_Finger_Employee();

                    if (isEmployee)
                    {
                        photoFingerDataEmployee = await _profileRepository.GetPhotoFingerEmployee(enroll.KtpNIK, enroll.KtpTypeJariKanan)
                        .ConfigureAwait(false);
                    }

                    string imageEncrypted = enroll.KtpFingerKanan.Encrypt(Phrase.FileEncryption);

                    string filePath = "";
                    string filePathIso = "";

                    var systemParameterPath = await _sysParameterRepository.GetPathFolder("FolderFinger")
                                                .ConfigureAwait(false);
                    string pathFolderFoto = systemParameterPath.Value;

                    string subPathFolderPhotoFinger = pathFolder + "/" + pathFolderFoto + "/" + enroll.KtpNIK + "/";

                    string JenisJari = enroll.KtpTypeJariKanan.Replace(" ", "");
                    string fileName = "PhotoFinger_" + JenisJari + "_" + enroll.KtpNIK + "_" + JamServer + ".txt";
                    string fileNameIso = "";

                    if (_sftpConfig.IsActive)
                    {
                        using var stream = new MemoryStream(Encoding.ASCII.GetBytes(imageEncrypted));

                        (var fname, var fPath) = await stream.UploadToFTPServer(subPathFolderPhotoFinger, fileName, _sftpConfig.Host,
                            _sftpConfig.Username, _sftpConfig.Password, _sftpConfig.RootDirectory, _sftpConfig.Url).ConfigureAwait(false);

                        filePath = fPath;
                    }
                    else
                    {
                        if (!Directory.Exists(subPathFolderPhotoFinger))
                        {
                            Directory.CreateDirectory(subPathFolderPhotoFinger);
                        }

                        filePath = subPathFolderPhotoFinger + fileName;
                        File.WriteAllText(filePath, imageEncrypted);
                    }

                    var typeFingerMain = await _fingerRepository.GetFingersEnrolled(enroll.KtpNIK);
                    if(typeFingerMain.Count() == 0)
                    {
                        photoFingers.Add(new Tbl_DataKTP_Finger
                        {
                            CreatedById = Id,
                            IsActive = true,
                            IsDeleted = false,
                            Nik = enroll.KtpNIK,
                            PathFile = filePath,//
                                                //PathFileISO = filePathIso,//
                            CreatedByNpp = npp,
                            CreatedByUid = enroll.UID,
                            CreatedTime = DateTime.Now,
                            FileName = fileName,//
                                                //FileNameISO = fileNameIso,//
                                                //FileJariISO = enroll.KtpFingerKananIso,
                            TypeFinger = enroll.KtpTypeJariKanan,
                            CreatedByUnit = unitCode,
                            CreatedByUnitId = unitId
                        });

                        if (isEmployee)
                        {
                            photoFingersEmployee.Add(new Tbl_DataKTP_Finger_Employee
                            {
                                CreatedById = Id,
                                IsActive = true,
                                IsDeleted = false,
                                Nik = enroll.KtpNIK,
                                PathFile = filePath,//
                                                    //PathFileISO = filePathIso,//
                                CreatedByNpp = npp,
                                CreatedByUid = enroll.UID,
                                CreatedTime = DateTime.Now,
                                FileName = fileName,//
                                                    //FileNameISO = fileNameIso,//
                                                    //FileJariISO = enroll.KtpFingerKananIso,
                                TypeFinger = enroll.KtpTypeJariKanan,
                                CreatedByUnit = unitCode,
                                CreatedByUnitId = unitId
                            });
                        }
                    }

                    foreach (var i in typeFingerMain)
                    {
                        if (i.TypeFinger.Contains("Kanan"))
                        {
                            var photoFingerData = await _profileRepository.GetPhotoFinger(enroll.KtpNIK, i.TypeFinger).ConfigureAwait(false);

                            if (photoFingerData != null)
                            {
                                photoFingerLogs.Add(new Tbl_DataKTP_Finger_Log
                                {
                                    CreatedById = photoFingerData.CreatedById,
                                    CreatedByNpp = photoFingerData.CreatedByNpp,
                                    CreatedByUid = photoFingerData.CreatedByUid,
                                    CreatedByUnit = photoFingerData.CreatedByUnit,
                                    CreatedByUnitId = photoFingerData.CreatedByUnitId,
                                    CreatedTime = photoFingerData.CreatedTime,
                                    //FileJari = photoFingerData.FileJari,
                                    FileJariISO = photoFingerData.FileJariISO,
                                    FileName = photoFingerData.FileName,
                                    FileNameISO = photoFingerData.FileNameISO,
                                    Nik = photoFingerData.Nik,
                                    PathFile = photoFingerData.PathFile,
                                    PathFileISO = photoFingerData.PathFileISO,
                                    TypeFinger = photoFingerData.TypeFinger
                                });

                                exPhotoFinger.Add(photoFingerData);
                            }
                            photoFingers.Add(new Tbl_DataKTP_Finger
                            {
                                CreatedById = Id,
                                IsActive = true,
                                IsDeleted = false,
                                Nik = enroll.KtpNIK,
                                PathFile = filePath,//
                                                    //PathFileISO = filePathIso,//
                                CreatedByNpp = npp,
                                CreatedByUid = enroll.UID,
                                CreatedTime = DateTime.Now,
                                FileName = fileName,//
                                //FileNameISO = fileNameIso,//
                                //FileJariISO = enroll.KtpFingerKananIso,
                                TypeFinger = enroll.KtpTypeJariKanan,
                                CreatedByUnit = unitCode,
                                CreatedByUnitId = unitId
                            });

                            if (isEmployee)
                            {
                                photoFingersEmployee.Add(new Tbl_DataKTP_Finger_Employee
                                {
                                    CreatedById = Id,
                                    IsActive = true,
                                    IsDeleted = false,
                                    Nik = enroll.KtpNIK,
                                    PathFile = filePath,//
                                                        //PathFileISO = filePathIso,//
                                    CreatedByNpp = npp,
                                    CreatedByUid = enroll.UID,
                                    CreatedTime = DateTime.Now,
                                    FileName = fileName,//
                                    //FileNameISO = fileNameIso,//
                                    //FileJariISO = enroll.KtpFingerKananIso,
                                    TypeFinger = enroll.KtpTypeJariKanan,
                                    CreatedByUnit = unitCode,
                                    CreatedByUnitId = unitId
                                });
                            }
                        }
                    }
                }


                if (!string.IsNullOrWhiteSpace(enroll.KtpFingerKiri))
                {

                    var photoFingerDataEmployee = new Tbl_DataKTP_Finger_Employee();

                    if (isEmployee)
                    {
                        photoFingerDataEmployee = await _profileRepository.GetPhotoFingerEmployee(enroll.KtpNIK, enroll.KtpTypeJariKiri)
                        .ConfigureAwait(false);
                    }

                    string imageEncrypted = enroll.KtpFingerKiri.Encrypt(Phrase.FileEncryption);

                    string filePath = "";
                    string filePathIso = "";

                    var systemParameterPath = await _sysParameterRepository.GetPathFolder("FolderFinger")
                                                .ConfigureAwait(false);
                    string pathFolderFoto = systemParameterPath.Value;

                    string subPathFolderPhotoFinger = pathFolder + "/" + pathFolderFoto + "/" + enroll.KtpNIK + "/";

                    string JenisJari = enroll.KtpTypeJariKiri.Replace(" ", "");
                    string fileName = "PhotoFinger_" + JenisJari + "_" + enroll.KtpNIK + "_" + JamServer + ".txt";
                    string fileNameIso = "";

                    if (_sftpConfig.IsActive)
                    {
                        using var stream = new MemoryStream(Encoding.ASCII.GetBytes(imageEncrypted));

                        (var fname, var fPath) = await stream.UploadToFTPServer(subPathFolderPhotoFinger, fileName, _sftpConfig.Host,
                            _sftpConfig.Username, _sftpConfig.Password, _sftpConfig.RootDirectory, _sftpConfig.Url).ConfigureAwait(false);

                        filePath = fPath;
                    }
                    else
                    {
                        if (!Directory.Exists(subPathFolderPhotoFinger))
                        {
                            Directory.CreateDirectory(subPathFolderPhotoFinger);
                        }

                        filePath = subPathFolderPhotoFinger + fileName;
                        File.WriteAllText(filePath, imageEncrypted);
                    }


                    var typeFingerMain = await _fingerRepository.GetFingersEnrolled(enroll.KtpNIK);
                    if(typeFingerMain.Count() == 0)
                    {
                        photoFingers.Add(new Tbl_DataKTP_Finger
                        {
                            CreatedById = Id,
                            IsActive = true,
                            IsDeleted = false,
                            Nik = enroll.KtpNIK,
                            PathFile = filePath,//
                                                //PathFileISO = filePathIso,//
                            CreatedByNpp = npp,
                            CreatedByUid = enroll.UID,
                            CreatedTime = DateTime.Now,
                            FileName = fileName,//
                                                //FileNameISO = fileNameIso,//
                                                //FileJariISO= enroll.KtpFingerKiriIso,//
                            TypeFinger = enroll.KtpTypeJariKiri,
                            CreatedByUnit = unitCode,
                            CreatedByUnitId = unitId
                        });

                        if (isEmployee)
                        {
                            photoFingersEmployee.Add(new Tbl_DataKTP_Finger_Employee
                            {
                                CreatedById = Id,
                                IsActive = true,
                                IsDeleted = false,
                                Nik = enroll.KtpNIK,
                                PathFile = filePath,//
                                                    //PathFileISO = filePathIso,//
                                CreatedByNpp = npp,
                                CreatedByUid = enroll.UID,
                                CreatedTime = DateTime.Now,
                                FileName = fileName,//
                                                    //FileNameISO = fileNameIso,//
                                                    //FileJariISO = enroll.KtpFingerKiriIso,//
                                TypeFinger = enroll.KtpTypeJariKiri,
                                CreatedByUnit = unitCode,
                                CreatedByUnitId = unitId
                            });
                        }
                    }

                    foreach (var i in typeFingerMain)
                    {
                        if (i.TypeFinger.Contains("Kiri"))
                        {
                            var photoFingerData = await _profileRepository.GetPhotoFinger(enroll.KtpNIK, i.TypeFinger).ConfigureAwait(false);

                            if (photoFingerData != null)
                            {
                                photoFingerLogs.Add(new Tbl_DataKTP_Finger_Log
                                {
                                    CreatedById = photoFingerData.CreatedById,
                                    CreatedByNpp = photoFingerData.CreatedByNpp,
                                    CreatedByUid = photoFingerData.CreatedByUid,
                                    CreatedByUnit = photoFingerData.CreatedByUnit,
                                    CreatedByUnitId = photoFingerData.CreatedByUnitId,
                                    CreatedTime = photoFingerData.CreatedTime,
                                    //FileJari = photoFingerData.FileJari,
                                    FileJariISO = photoFingerData.FileJariISO,
                                    FileName = photoFingerData.FileName,
                                    FileNameISO = photoFingerData.FileNameISO,
                                    Nik = photoFingerData.Nik,
                                    PathFile = photoFingerData.PathFile,
                                    PathFileISO = photoFingerData.PathFileISO,
                                    TypeFinger = photoFingerData.TypeFinger
                                });

                                exPhotoFinger.Add(photoFingerData);
                            }
                            photoFingers.Add(new Tbl_DataKTP_Finger
                            {
                                CreatedById = Id,
                                IsActive = true,
                                IsDeleted = false,
                                Nik = enroll.KtpNIK,
                                PathFile = filePath,//
                                                    //PathFileISO = filePathIso,//
                                CreatedByNpp = npp,
                                CreatedByUid = enroll.UID,
                                CreatedTime = DateTime.Now,
                                FileName = fileName,//
                                                    //FileNameISO = fileNameIso,//
                                                    //FileJariISO= enroll.KtpFingerKiriIso,//
                                TypeFinger = enroll.KtpTypeJariKiri,
                                CreatedByUnit = unitCode,
                                CreatedByUnitId = unitId
                            });

                            if (isEmployee)
                            {
                                photoFingersEmployee.Add(new Tbl_DataKTP_Finger_Employee
                                {
                                    CreatedById = Id,
                                    IsActive = true,
                                    IsDeleted = false,
                                    Nik = enroll.KtpNIK,
                                    PathFile = filePath,//
                                                        //PathFileISO = filePathIso,//
                                    CreatedByNpp = npp,
                                    CreatedByUid = enroll.UID,
                                    CreatedTime = DateTime.Now,
                                    FileName = fileName,//
                                                        //FileNameISO = fileNameIso,//
                                                        //FileJariISO = enroll.KtpFingerKiriIso,//
                                    TypeFinger = enroll.KtpTypeJariKiri,
                                    CreatedByUnit = unitCode,
                                    CreatedByUnitId = unitId
                                });
                            }
                        }
                    }
                }


                // to do: alat reader log
                var dataReader = await _dataReaderRepository.GetDatareaderUid(enroll.UID).ConfigureAwait(false);

                var dataReaderLog = new Tbl_MasterAlatReaderLog();

                if (dataReader != null)
                {
                    dataReaderLog = new Tbl_MasterAlatReaderLog
                    {
                        CreatedBy_Id = Id,
                        CreatedTime = DateTime.Now,
                        IsActive = true,
                        IsDeleted = false,
                        Nik = enroll.KtpNIK,
                        PegawaiId = Id,
                        Serial_Number = dataReader.SN_Unit,
                        Type = "Enroll",
                        Uid = enroll.UID
                    };
                }

                _enrollmentKTPRepository.InsertEnrollFlow(dataDemografis, dataDemografisLog, photoKtp, photoKtpLog, photoSignature, photoSignatureLog,
                    photoCam, photoCamLog, photoFingers, photoFingerLogs, exPhotoKtp, exPhotoSignature, exPhotoCam, exPhotoFinger, dataNpp, dataReaderLog, photoFingersEmployee, exPhotoFingerEmployee);

                await _alatReaderRepository.CreateLogActivity2(new Tbl_MasterAlatReaderLogActvity
                {
                    CreatedBy_Id = Id,
                    IsActive = true,
                    IsDeleted = false,
                    KodeUnit = unitCode,
                    LastIP = enroll.IpAddress,
                    NIK = enroll.KtpNIK,
                    CreatedTime = DateTime.Now,
                    NppPegawai = npp,
                    PegawaiId = Id,
                    Type = "Enroll",
                    UID = enroll.UID,
                    UnitId = unitId
                });
            }

            if (IsNasabah)
            {
                return (_SuccessMessageConfig.NasabahSuksesEnrollNasabah, (int)EnrollStatus.Nasabah_Berhasil_di_enroll, cekKTP?.CIF);
            }
            else
            {
                return (_SuccessMessageConfig.NasabahSuksesEnrollNonNasabah, (int)EnrollStatus.Nasabah_Berhasil_di_enroll, cekKTP?.CIF);
            }
        }

        public async Task<(string msg, int code, string cif)> SubmitEnrollmentFingerEncryptedOnlyThirdParty(string AppsChannel, bool isHitSOA, ApiSOA ReqSoa, EnrollKTPBiasaThirdParty enroll)
        {
            bool isEmployee = false;
            var _empData = await _enrollmentKTPRepository.IsEmployee(enroll.KtpNIK);
            if (_empData != null)
            {
                isEmployee = true;
            }
            var cekNPPCs = await _profileRepository.GetCS()
                .ConfigureAwait(false);
            if (cekNPPCs == null) return (_ErrorMessageConfig.UserCSNotFound, (int)EnrollStatus.Inputan_tidak_lengkap, "");

            if (enroll.KtpTanggalLahir != null)
            {
                enroll.KtpTanggalLahir = enroll.KtpTanggalLahir.Replace(".", "");
                if (enroll.KtpTanggalLahir.Contains("/"))
                {
                    enroll.KtpTanggalLahir = enroll.KtpTanggalLahir.Replace("/", "-");
                }
                string[] arrayTanggl = enroll.KtpTanggalLahir.Split("-");
                string hari = "";
                string bulan = "";
                string tahun = "";

                if (arrayTanggl != null)
                {
                    if (arrayTanggl[0] != null)
                    {
                        if (arrayTanggl[0].Length != 2)
                        {
                            hari = "0" + arrayTanggl[0];

                        }
                        else
                        {
                            hari = arrayTanggl[0];

                        }
                    }

                    if (arrayTanggl[1] != null)
                    {
                        if (arrayTanggl[1].Length != 2)
                        {
                            bulan = "0" + arrayTanggl[1];

                        }
                        else
                        {
                            bulan = arrayTanggl[1];

                        }
                    }

                    if (arrayTanggl[2] != null)
                    {
                        tahun = arrayTanggl[2];
                    }
                }

                enroll.KtpTanggalLahir = hari + "-" + bulan + "-" + tahun;
            }

            const bool IsNewEnroll = true;
            string JamServer = DateTime.Now.ToString("ddMMyyyyHHmmss");
            bool IsNasabah = false;

            var cekKTP = await _profileRepository.GetDataDemografis(enroll.KtpNIK)
                .ConfigureAwait(false);

            if (cekKTP != null)
            {
                var dataNpp = new Tbl_Mapping_Pegawai_KTP();
                #region Old Logic
                //var logDemografi = new Tbl_DataKTP_Demografis_Log
                //{
                //    Agama = cekKTP.Agama,
                //    Alamat = cekKTP.Alamat,
                //    AlamatGoogle = cekKTP.AlamatGoogle,
                //    AlamatLengkap = cekKTP.AlamatLengkap,
                //    CreatedById = cekKTP.CreatedById,
                //    CreatedByUID = cekKTP.CreatedByUID,
                //    CreatedByUnitId = cekKTP.CreatedByUnitId,
                //    CreatedTime = cekKTP.CreatedTime,
                //    Desa = cekKTP.Desa,
                //    GolonganDarah = cekKTP.GolonganDarah,
                //    JenisKelamin = cekKTP.JenisKelamin,
                //    Kecamatan = cekKTP.Kecamatan,
                //    Kelurahan = cekKTP.Kelurahan,
                //    Kewarganegaraan = cekKTP.Kewarganegaraan,
                //    KodePos = cekKTP.KodePos,
                //    Kota = cekKTP.Kota,
                //    Latitude = cekKTP.Latitude,
                //    Longitude = cekKTP.Longitude,
                //    MasaBerlaku = cekKTP.MasaBerlaku,
                //    Nama = cekKTP.Nama,
                //    NIK = cekKTP.NIK,
                //    Pekerjaan = cekKTP.Pekerjaan,
                //    Provinsi = cekKTP.Provinsi,
                //    RT = cekKTP.RT,
                //    RW = cekKTP.RW,
                //    StatusPerkawinan = cekKTP.StatusPerkawinan,
                //    TanggalLahir = cekKTP.TanggalLahir,
                //    TempatLahir = cekKTP.TempatLahir,
                //    CIF = cekKTP.CIF,
                //    CreatedByNpp = cekKTP.CreatedByNpp
                //};

                //var stringPerubahan = "";

                //if (cekKTP.Nama != enroll.KtpNama)
                //{
                //    stringPerubahan = stringPerubahan + "NAMA : " + cekKTP.Nama + " -> " + enroll.KtpNama + " <br/>";
                //    cekKTP.Nama = enroll.KtpNama;
                //}

                //if (cekKTP.TempatLahir != enroll.KtpTempatLahir)
                //{
                //    stringPerubahan = stringPerubahan + "TEMPAT LAHIR : " + cekKTP.TempatLahir + " -> " + enroll.KtpTempatLahir + " <br/>";
                //    cekKTP.TempatLahir = enroll.KtpTempatLahir;
                //}

                //if (enroll.KtpTanggalLahir != null)
                //{

                //    if (cekKTP.TempatLahir != enroll.KtpTempatLahir)
                //    {
                //        stringPerubahan = stringPerubahan + "TEMPAT LAHIR : " + cekKTP.TanggalLahir + " -> " + enroll.KtpTanggalLahir + " <br/>";
                //        cekKTP.TanggalLahir = DateTime.ParseExact(enroll.KtpTanggalLahir, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                //    }

                //}

                //if (cekKTP.JenisKelamin != enroll.KtpJanisKelamin)
                //{
                //    stringPerubahan = stringPerubahan + "JENIS KELAMIN : " + cekKTP.JenisKelamin + " -> " + enroll.KtpJanisKelamin + " <br/>";
                //    cekKTP.JenisKelamin = enroll.KtpJanisKelamin;
                //}

                //if (cekKTP.GolonganDarah != enroll.KtpGolonganDarah)
                //{
                //    stringPerubahan = stringPerubahan + "GOLONGAN DARAH : " + cekKTP.GolonganDarah + " -> " + enroll.KtpGolonganDarah + " <br/>";
                //    cekKTP.GolonganDarah = enroll.KtpGolonganDarah;
                //}

                //if (cekKTP.Alamat != enroll.KtpAlamat)
                //{
                //    stringPerubahan = stringPerubahan + "ALAMAT : " + cekKTP.Alamat + " -> " + enroll.KtpAlamat + " <br/>";
                //    cekKTP.Alamat = enroll.KtpAlamat;
                //}

                //if (cekKTP.RT != enroll.KtpRT)
                //{
                //    stringPerubahan = stringPerubahan + "RT : " + cekKTP.RT + " -> " + enroll.KtpRT + " <br/>";
                //    cekKTP.RT = enroll.KtpRT;
                //}

                //if (cekKTP.RW != enroll.KtpRW)
                //{
                //    stringPerubahan = stringPerubahan + "RW : " + cekKTP.RW + " -> " + enroll.KtpRW + " <br/>";
                //    cekKTP.RW = enroll.KtpRW;
                //}

                //if (cekKTP.Kelurahan != enroll.KtpKelurahan)
                //{
                //    stringPerubahan = stringPerubahan + "KELURAHAN : " + cekKTP.Kelurahan + " -> " + enroll.KtpKelurahan + " <br/>";
                //    cekKTP.Kelurahan = enroll.KtpKelurahan;
                //}

                //if (cekKTP.Kecamatan != enroll.KtpKecamatan)
                //{
                //    stringPerubahan = stringPerubahan + "KECAMATAN : " + cekKTP.Kecamatan + " -> " + enroll.KtpKecamatan + " <br/>";
                //    cekKTP.Kecamatan = enroll.KtpKecamatan;
                //}

                //if (cekKTP.Kota != enroll.KtpKota)
                //{
                //    stringPerubahan = stringPerubahan + "KOTA/KABUPATEN : " + cekKTP.Kota + " -> " + enroll.KtpKota + " <br/>";
                //    cekKTP.Kota = enroll.KtpKota;
                //}

                //if (cekKTP.Provinsi != enroll.KtpProvinsi)
                //{
                //    stringPerubahan = stringPerubahan + "PROVINSI  : " + cekKTP.Provinsi + " -> " + enroll.KtpProvinsi + " <br/>";
                //    cekKTP.Provinsi = enroll.KtpProvinsi;
                //}

                //if (cekKTP.Agama != enroll.KtpAgama)
                //{
                //    stringPerubahan = stringPerubahan + "AGAMA  : " + cekKTP.Agama + " -> " + enroll.KtpAgama + " <br/>";
                //    cekKTP.Agama = enroll.KtpAgama;
                //}

                //if (cekKTP.StatusPerkawinan != enroll.KtpStatusPerkawinan)
                //{
                //    stringPerubahan = stringPerubahan + "STATUS PERKAWINAN  : " + cekKTP.Agama + " -> " + enroll.KtpAgama + " <br/>";
                //    cekKTP.StatusPerkawinan = enroll.KtpStatusPerkawinan;
                //}

                //if (cekKTP.Pekerjaan != enroll.KtpPekerjaan)
                //{
                //    stringPerubahan = stringPerubahan + "PEKERJAAN  : " + cekKTP.Agama + " -> " + enroll.KtpAgama + " <br/>";
                //    cekKTP.Pekerjaan = enroll.KtpPekerjaan;
                //}

                //if (cekKTP.Kewarganegaraan != enroll.KtpKewarganegaraan)
                //{
                //    stringPerubahan = stringPerubahan + "KEWARGANEGARAAN  : " + cekKTP.Kewarganegaraan + " -> " + enroll.KtpKewarganegaraan + " <br/>";
                //    cekKTP.Kewarganegaraan = enroll.KtpKewarganegaraan;
                //}
                //if (cekKTP.MasaBerlaku != enroll.KtpMasaBerlaku)
                //{
                //    stringPerubahan = stringPerubahan + "MASA BERLAKU  : " + cekKTP.MasaBerlaku + " -> " + enroll.KtpMasaBerlaku + " <br/>";
                //    cekKTP.MasaBerlaku = enroll.KtpMasaBerlaku;
                //}

                //cekKTP.UpdatedTime = DateTime.Now;
                //cekKTP.UpdatedById = cekNPPCs.PegawaiId;
                //cekKTP.UpdatedByUID = enroll.UID;
                //cekKTP.UpdatedByUnitCode = cekNPPCs.Kode_Unit;
                //cekKTP.UpdatedByUnitId = int.Parse(cekNPPCs.Unit_Id);
                //cekKTP.isEnrollFR = false;

                //await _profileRepository.UpdateDataDemografis(cekKTP)
                //    .ConfigureAwait(false);

                //await _alatReaderRepository.InsertLogEnrollThirdParty(new Tbl_Enrollment_ThirdParty_Log
                //{
                //    NIK = enroll.KtpNIK,
                //    AppsChannel = AppsChannel,
                //    SubmitDate = DateTime.Now

                //});

                //if (string.IsNullOrWhiteSpace(stringPerubahan))
                //{
                //    return (_ErrorMessageConfig.NasabahPernahEnroll, (int)EnrollStatus.Sudah_di_enroll_sebelumnya, "");
                //}

                //_profileRepository.InsertDemografiLog(logDemografi);

                //return (stringPerubahan, (int)EnrollStatus.Berhasil_mengubah_beberapa_perubahan, "");

                #endregion

                #region update demografi
                var logDemografi = new Tbl_DataKTP_Demografis_Log
                {
                    Agama = cekKTP.Agama,
                    Alamat = cekKTP.Alamat,
                    AlamatGoogle = cekKTP.AlamatGoogle,
                    AlamatLengkap = cekKTP.AlamatLengkap,
                    CreatedById = cekKTP.CreatedById,
                    CreatedByUID = cekKTP.CreatedByUID,
                    CreatedByUnitId = cekKTP.CreatedByUnitId,
                    CreatedTime = cekKTP.CreatedTime,
                    Desa = cekKTP.Desa,
                    GolonganDarah = cekKTP.GolonganDarah,
                    JenisKelamin = cekKTP.JenisKelamin,
                    Kecamatan = cekKTP.Kecamatan,
                    Kelurahan = cekKTP.Kelurahan,
                    Kewarganegaraan = cekKTP.Kewarganegaraan,
                    KodePos = cekKTP.KodePos,
                    Kota = cekKTP.Kota,
                    Latitude = cekKTP.Latitude,
                    Longitude = cekKTP.Longitude,
                    MasaBerlaku = cekKTP.MasaBerlaku,
                    Nama = cekKTP.Nama,
                    NIK = cekKTP.NIK,
                    Pekerjaan = cekKTP.Pekerjaan,
                    Provinsi = cekKTP.Provinsi,
                    RT = cekKTP.RT,
                    RW = cekKTP.RW,
                    StatusPerkawinan = cekKTP.StatusPerkawinan,
                    TanggalLahir = cekKTP.TanggalLahir,
                    TempatLahir = cekKTP.TempatLahir,
                    CIF = cekKTP.CIF,
                    CreatedByNpp = cekKTP.CreatedByNpp
                };

                var stringPerubahan = "";

                if (cekKTP.Nama != enroll.KtpNama)
                {
                    stringPerubahan = stringPerubahan + "NAMA : " + cekKTP.Nama + " -> " + enroll.KtpNama + " <br/>";
                    cekKTP.Nama = enroll.KtpNama;
                }

                if (cekKTP.TempatLahir != enroll.KtpTempatLahir)
                {
                    stringPerubahan = stringPerubahan + "TEMPAT LAHIR : " + cekKTP.TempatLahir + " -> " + enroll.KtpTempatLahir + " <br/>";
                    cekKTP.TempatLahir = enroll.KtpTempatLahir;
                }

                if (enroll.KtpTanggalLahir != null)
                {
                    var _ttl = DateTime.ParseExact(enroll.KtpTanggalLahir, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                    if (cekKTP.TanggalLahir != _ttl)
                    {
                        stringPerubahan = stringPerubahan + "TANGGAL LAHIR : " + cekKTP.TanggalLahir + " -> " + enroll.KtpTanggalLahir + " <br/>";
                        cekKTP.TanggalLahir = DateTime.ParseExact(enroll.KtpTanggalLahir, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                    }
                }

                if (cekKTP.JenisKelamin != enroll.KtpJanisKelamin)
                {
                    stringPerubahan = stringPerubahan + "JENIS KELAMIN : " + cekKTP.JenisKelamin + " -> " + enroll.KtpJanisKelamin + " <br/>";
                    cekKTP.JenisKelamin = enroll.KtpJanisKelamin;
                }

                if (cekKTP.GolonganDarah != enroll.KtpGolonganDarah)
                {
                    stringPerubahan = stringPerubahan + "GOLONGAN DARAH : " + cekKTP.GolonganDarah + " -> " + enroll.KtpGolonganDarah + " <br/>";
                    cekKTP.GolonganDarah = enroll.KtpGolonganDarah;
                }

                if (cekKTP.Alamat != enroll.KtpAlamat)
                {
                    stringPerubahan = stringPerubahan + "ALAMAT : " + cekKTP.Alamat + " -> " + enroll.KtpAlamat + " <br/>";
                    cekKTP.Alamat = enroll.KtpAlamat;
                }

                if (cekKTP.RT != enroll.KtpRT)
                {
                    stringPerubahan = stringPerubahan + "RT : " + cekKTP.RT + " -> " + enroll.KtpRT + " <br/>";
                    cekKTP.RT = enroll.KtpRT;
                }

                if (cekKTP.RW != enroll.KtpRW)
                {
                    stringPerubahan = stringPerubahan + "RW : " + cekKTP.RW + " -> " + enroll.KtpRW + " <br/>";
                    cekKTP.RW = enroll.KtpRW;
                }

                if (cekKTP.Kelurahan != enroll.KtpKelurahan)
                {
                    stringPerubahan = stringPerubahan + "KELURAHAN : " + cekKTP.Kelurahan + " -> " + enroll.KtpKelurahan + " <br/>";
                    cekKTP.Kelurahan = enroll.KtpKelurahan;
                }

                if (cekKTP.Kecamatan != enroll.KtpKecamatan)
                {
                    stringPerubahan = stringPerubahan + "KECAMATAN : " + cekKTP.Kecamatan + " -> " + enroll.KtpKecamatan + " <br/>";
                    cekKTP.Kecamatan = enroll.KtpKecamatan;
                }

                if (cekKTP.Kota != enroll.KtpKota)
                {
                    stringPerubahan = stringPerubahan + "KOTA/KABUPATEN : " + cekKTP.Kota + " -> " + enroll.KtpKota + " <br/>";
                    cekKTP.Kota = enroll.KtpKota;
                }

                if (cekKTP.Provinsi != enroll.KtpProvinsi)
                {
                    stringPerubahan = stringPerubahan + "PROVINSI  : " + cekKTP.Provinsi + " -> " + enroll.KtpProvinsi + " <br/>";
                    cekKTP.Provinsi = enroll.KtpProvinsi;
                }

                if (cekKTP.Agama != enroll.KtpAgama)
                {
                    stringPerubahan = stringPerubahan + "AGAMA  : " + cekKTP.Agama + " -> " + enroll.KtpAgama + " <br/>";
                    cekKTP.Agama = enroll.KtpAgama;
                }

                if (cekKTP.StatusPerkawinan != enroll.KtpStatusPerkawinan)
                {
                    stringPerubahan = stringPerubahan + "STATUS PERKAWINAN  : " + cekKTP.Agama + " -> " + enroll.KtpAgama + " <br/>";
                    cekKTP.StatusPerkawinan = enroll.KtpStatusPerkawinan;
                }

                if (cekKTP.Pekerjaan != enroll.KtpPekerjaan)
                {
                    stringPerubahan = stringPerubahan + "PEKERJAAN  : " + cekKTP.Agama + " -> " + enroll.KtpAgama + " <br/>";
                    cekKTP.Pekerjaan = enroll.KtpPekerjaan;
                }

                if (cekKTP.Kewarganegaraan != enroll.KtpKewarganegaraan)
                {
                    stringPerubahan = stringPerubahan + "KEWARGANEGARAAN  : " + cekKTP.Kewarganegaraan + " -> " + enroll.KtpKewarganegaraan + " <br/>";
                    cekKTP.Kewarganegaraan = enroll.KtpKewarganegaraan;
                }
                if (cekKTP.MasaBerlaku != enroll.KtpMasaBerlaku)
                {
                    stringPerubahan = stringPerubahan + "MASA BERLAKU  : " + cekKTP.MasaBerlaku + " -> " + enroll.KtpMasaBerlaku + " <br/>";
                    cekKTP.MasaBerlaku = enroll.KtpMasaBerlaku;
                }
                if (cekKTP.CIF == null)
                {
                    var cifData = new ApiSOAResponse();
                    #region Hit SOA And Loggging it
                    if (isHitSOA == true)
                    {
                        cifData = await _cifService.GetSOAByCif(ReqSoa)
                        .ConfigureAwait(false);

                        var _status = 0;
                        if (cifData.cif != null)
                        {
                            _status = 1;
                        }

                        var _log = new Tbl_ThirdPartyLog
                        {
                            FeatureName = "ReSubmitEnrollmentFingerEncryptedOnly",
                            HostUrl = ReqSoa.host,
                            Request = Newtonsoft.Json.JsonConvert.SerializeObject(ReqSoa),
                            Status = _status,
                            Response = Newtonsoft.Json.JsonConvert.SerializeObject(cifData),
                            CreatedDate = System.DateTime.Now,
                            CreatedBy = cekNPPCs.NIK
                        };

                        _ = _enrollmentKTPRepository.CreateThirdPartyLog(_log);
                    }
                    else
                    {
                        var res = await _cifService.GetCIF(new NikDto { Nik = enroll.KtpNIK },
                            new NikDtoUrl { baseUrl = ReqSoa.baseUrlNonSoa, endpoint = ReqSoa.UrlEndPointNonSoa });
                        if (res.Data == null)
                        {
                            cifData.cif = null;
                        }
                        else
                        {
                            cifData.cif = res.Data.Cif;
                        };
                    }
                    #endregion

                    if (!String.IsNullOrEmpty(cifData.cif))
                    {
                        cifData.cif = cifData.cif.Trim();
                    }

                    stringPerubahan = stringPerubahan + "CIF  : " + cekKTP.CIF + " -> " + cifData.cif + " <br/>";
                    cekKTP.CIF = cifData.cif;
                }

                cekKTP.UpdatedTime = DateTime.Now;
                cekKTP.UpdatedById = cekNPPCs.PegawaiId;
                cekKTP.UpdatedByUID = enroll.UID;
                cekKTP.UpdatedByUnitCode = cekNPPCs.Kode_Unit;
                cekKTP.UpdatedByUnitId = int.Parse(cekNPPCs.Unit_Id);
                cekKTP.UpdatedByNpp = cekNPPCs.NIK;
                cekKTP.isEnrollFR = false;

                if (string.IsNullOrWhiteSpace(stringPerubahan))
                {
                    stringPerubahan = stringPerubahan + "Data Demografi tidak ada perubahan" + " <br/>";
                }
                #endregion

                if (isEmployee)
                {
                    var _mappingData = await _enrollmentKTPRepository.MappingNppNikByNik(enroll.KtpNIK);
                    if (_mappingData == null)
                    {
                        dataNpp = new Tbl_Mapping_Pegawai_KTP
                        {
                            CreatedById = cekNPPCs.PegawaiId,
                            NIK = enroll.KtpNIK,
                            Npp = _empData.Npp,
                            CreatedByNpp = cekNPPCs.NIK,
                            CreatedByUID = enroll.UID,
                            CreatedTime = DateTime.Now,
                            CreatedByUnit = cekNPPCs.Kode_Unit,
                            UpdatedByNpp = cekNPPCs.NIK,
                            UpdatedByUID = enroll.UID,
                            UpdatedByUnit = cekNPPCs.Kode_Unit,
                            UpdatedTime = DateTime.Now,
                        };
                    }
                    else
                    {
                        dataNpp = _mappingData;
                        dataNpp.UpdatedByNpp = cekNPPCs.NIK;
                        dataNpp.UpdatedByUID = enroll.UID;
                        dataNpp.UpdatedByUnit = cekNPPCs.Kode_Unit;
                        dataNpp.UpdatedTime = DateTime.Now;
                    }
                }

                var sysPathFolder = await _sysParameterRepository.GetPathFolder("PathFolderData");
                string pathFolder = sysPathFolder.Value;

                var photoKtpLog = new Tbl_DataKTP_Photo_Log();
                var photoKTPData = await _profileRepository.GetPhotoKtp(enroll.KtpNIK)
                        .ConfigureAwait(false);
                var exPhotoKtp = photoKTPData;

                #region update Photo KTP
                if (!string.IsNullOrWhiteSpace(enroll.KtpPhotoKTP))
                {
                    byte[] imgBytes = Convert.FromBase64String(enroll.KtpPhotoKTP);

                    string filePath = "";

                    var systemParameterPath = await _sysParameterRepository.GetPathFolder("FolderPhoto")
                                                .ConfigureAwait(false);
                    string pathFolderFoto = systemParameterPath.Value;

                    string subPathFolderFoto = pathFolder + "/" + pathFolderFoto + "/" + enroll.KtpNIK + "/";

                    string fileName = "Foto_" + enroll.KtpNIK + "_" + JamServer + ".jpg";

                    if (_sftpConfig.IsActive)
                    {
                        using var stream = new MemoryStream(imgBytes);

                        (var fname, var fPath) = await stream.UploadToFTPServer(subPathFolderFoto, fileName, _sftpConfig.Host,
                            _sftpConfig.Username, _sftpConfig.Password, _sftpConfig.RootDirectory, _sftpConfig.Url).ConfigureAwait(false);

                        filePath = fPath;
                    }
                    else
                    {
                        if (!Directory.Exists(subPathFolderFoto))
                        {
                            Directory.CreateDirectory(subPathFolderFoto);
                        }

                        filePath = subPathFolderFoto + fileName;
                        File.WriteAllBytes(filePath, imgBytes);
                    }

                    if (photoKTPData == null)
                    {
                        photoKTPData = new Tbl_DataKTP_Photo
                        {
                            PathFile = filePath,
                            Nik = enroll.KtpNIK,
                            FileName = fileName,
                            IsActive = true,
                            IsDeleted = false,
                            UpdatedById = cekNPPCs.PegawaiId,
                            UpdatedByNpp = cekNPPCs.NIK,
                            UpdatedByUid = enroll.UID,
                            UpdatedByUnit = cekNPPCs.Kode_Unit,
                            UpdatedTime = DateTime.Now,
                            CreatedById = cekNPPCs.PegawaiId,
                            CreatedByNpp = cekNPPCs.NIK,
                            CreatedByUid = enroll.UID,
                            CreatedByUnit = cekNPPCs.Kode_Unit,
                            CreatedTime = DateTime.Now,
                        };
                    }
                    else
                    {
                        photoKtpLog.CreatedById = photoKTPData.CreatedById;
                        photoKtpLog.CreatedByNpp = photoKTPData.CreatedByNpp;
                        photoKtpLog.CreatedByUid = photoKTPData.CreatedByUid;
                        photoKtpLog.CreatedByUnit = photoKTPData.CreatedByUnit;
                        photoKtpLog.CreatedTime = photoKTPData.CreatedTime;
                        photoKtpLog.FileName = photoKTPData.FileName;
                        photoKtpLog.Nik = photoKTPData.Nik;
                        photoKtpLog.PathFile = photoKTPData.PathFile;

                        photoKTPData.PathFile = filePath;
                        photoKTPData.Nik = enroll.KtpNIK;
                        photoKTPData.FileName = fileName;
                        photoKTPData.IsActive = true;
                        photoKTPData.IsDeleted = false;
                        photoKTPData.UpdatedById = cekNPPCs.PegawaiId;
                        photoKTPData.UpdatedByNpp = cekNPPCs.NIK;
                        photoKTPData.UpdatedByUid = enroll.UID;
                        photoKTPData.UpdatedByUnit = cekNPPCs.Kode_Unit;
                        photoKTPData.UpdatedTime = DateTime.Now;
                    }
                }
                else
                {
                    stringPerubahan = stringPerubahan + "Request Payload Photo Tidak Ditemukan" + " <br/>";
                }
                #endregion

                var photoSignatureLog = new Tbl_DataKTP_Signature_Log();
                var photoSignatureData = await _profileRepository.GetPhotoSignature(enroll.KtpNIK)
                        .ConfigureAwait(false);
                var exphotoSignatureData = photoSignatureData;

                #region update signature
                if (!string.IsNullOrWhiteSpace(enroll.KtpSignature))
                {
                    byte[] imgBytes = Convert.FromBase64String(enroll.KtpSignature);

                    string filePath = "";

                    var systemParameterPath = await _sysParameterRepository.GetPathFolder("FolderSignature")
                                                .ConfigureAwait(false);
                    string pathFolderFoto = systemParameterPath.Value;

                    string subPathFolderSignature = pathFolder + "/" + pathFolderFoto + "/" + enroll.KtpNIK + "/";

                    string fileName = "Signature_" + enroll.KtpNIK + "_" + JamServer + ".jpg";

                    if (_sftpConfig.IsActive)
                    {
                        using var stream = new MemoryStream(imgBytes);

                        (var fname, var fPath) = await stream.UploadToFTPServer(subPathFolderSignature, fileName, _sftpConfig.Host,
                            _sftpConfig.Username, _sftpConfig.Password, _sftpConfig.RootDirectory, _sftpConfig.Url).ConfigureAwait(false);

                        filePath = fPath;
                    }
                    else
                    {
                        if (!Directory.Exists(subPathFolderSignature))
                        {
                            Directory.CreateDirectory(subPathFolderSignature);
                        }

                        filePath = subPathFolderSignature + fileName;
                        File.WriteAllBytes(filePath, imgBytes);
                    }

                    if (photoSignatureData == null)
                    {
                        photoSignatureData = new Tbl_DataKTP_Signature();
                        photoSignatureData.UpdatedById = cekNPPCs.PegawaiId;
                        photoSignatureData.UpdatedByNpp = cekNPPCs.NIK;
                        photoSignatureData.UpdatedByUid = enroll.UID;
                        photoSignatureData.UpdatedTime = DateTime.Now;
                        photoSignatureData.UpdatedByUnit = cekNPPCs.Kode_Unit;
                        photoSignatureData.IsActive = true;
                        photoSignatureData.IsDeleted = false;
                        photoSignatureData.Nik = enroll.KtpNIK;
                        photoSignatureData.FileName = fileName;
                        photoSignatureData.PathFile = filePath;
                        photoSignatureData.CreatedById = cekNPPCs.PegawaiId;
                        photoSignatureData.CreatedByNpp = cekNPPCs.NIK;
                        photoSignatureData.CreatedByUid = enroll.UID;
                        photoSignatureData.CreatedByUnit = cekNPPCs.Kode_Unit;
                        photoSignatureData.CreatedTime = DateTime.Now;
                    }
                    else
                    {
                        photoSignatureLog.CreatedById = photoSignatureData.CreatedById;
                        photoSignatureLog.CreatedByNpp = photoSignatureData.CreatedByNpp;
                        photoSignatureLog.CreatedByUid = photoSignatureData.CreatedByUid;
                        photoSignatureLog.CreatedByUnit = photoSignatureData.CreatedByUnit;
                        photoSignatureLog.CreatedTime = photoSignatureData.CreatedTime;
                        photoSignatureLog.FileName = photoSignatureData.FileName;
                        photoSignatureLog.Nik = photoSignatureData.Nik;
                        photoSignatureLog.PathFile = photoSignatureData.PathFile;

                        photoSignatureData.UpdatedById = cekNPPCs.PegawaiId;
                        photoSignatureData.UpdatedByNpp = cekNPPCs.NIK;
                        photoSignatureData.UpdatedByUid = enroll.UID;
                        photoSignatureData.UpdatedTime = DateTime.Now;
                        photoSignatureData.IsActive = true;
                        photoSignatureData.IsDeleted = false;
                        photoSignatureData.Nik = enroll.KtpNIK;
                        photoSignatureData.FileName = fileName;
                        photoSignatureData.PathFile = filePath;
                    }
                }
                else
                {
                    stringPerubahan = stringPerubahan + "Request Payload Signature Tidak Ditemukan" + " <br/>";
                }
                #endregion

                var photoCamLog = new Tbl_DataKTP_PhotoCam_Log();
                var photoCamData = await _profileRepository.GetPhotoCam(enroll.KtpNIK)
                        .ConfigureAwait(false);
                var exphotoCamData = photoCamData;

                #region update photo cam
                if (!string.IsNullOrWhiteSpace(enroll.KtpPhotoCam))
                {
                    byte[] imgBytes = Convert.FromBase64String(enroll.KtpPhotoCam);

                    string filePath = "";

                    var systemParameterPath = await _sysParameterRepository.GetPathFolder("FolderWebcam")
                                                .ConfigureAwait(false);
                    string pathFolderFoto = systemParameterPath.Value;

                    string SubPathFolderPhotoCam = pathFolder + "/" + pathFolderFoto + "/" + enroll.KtpNIK + "/";

                    string fileName = "PhotoCam_" + enroll.KtpNIK + "_" + JamServer + ".txt";

                    if (_sftpConfig.IsActive)
                    {
                        using var stream = new MemoryStream(imgBytes);

                        (var fname, var fPath) = await stream.UploadToFTPServer(SubPathFolderPhotoCam, fileName, _sftpConfig.Host,
                            _sftpConfig.Username, _sftpConfig.Password, _sftpConfig.RootDirectory, _sftpConfig.Url).ConfigureAwait(false);

                        filePath = fPath;
                    }
                    else
                    {
                        if (!Directory.Exists(SubPathFolderPhotoCam))
                        {
                            Directory.CreateDirectory(SubPathFolderPhotoCam);
                        }

                        filePath = SubPathFolderPhotoCam + fileName;
                        File.WriteAllBytes(filePath, imgBytes);
                    }

                    if (photoCamData == null)
                    {
                        photoCamData = new Tbl_DataKTP_PhotoCam();
                        photoCamData.PathFile = filePath;
                        photoCamData.Nik = enroll.KtpNIK;
                        photoCamData.FileName = fileName;
                        photoCamData.IsActive = true;
                        photoCamData.IsDeleted = false;
                        photoCamData.UpdatedById = cekNPPCs.PegawaiId;
                        photoCamData.UpdatedByNpp = cekNPPCs.NIK;
                        photoCamData.UpdatedByUid = enroll.UID;
                        photoCamData.UpdatedTime = DateTime.Now;
                        photoCamData.UpdatedByUnit = cekNPPCs.Kode_Unit;
                        photoCamLog.CreatedById = photoCamData.CreatedById;
                        photoCamLog.CreatedByNpp = photoCamData.CreatedByNpp;
                        photoCamLog.CreatedByUid = photoCamData.CreatedByUid;
                        photoCamLog.CreatedByUnit = photoCamData.CreatedByUnit;
                        photoCamLog.CreatedTime = photoCamData.CreatedTime;
                    }
                    else
                    {
                        photoCamLog.CreatedById = photoCamData.CreatedById;
                        photoCamLog.CreatedByNpp = photoCamData.CreatedByNpp;
                        photoCamLog.CreatedByUid = photoCamData.CreatedByUid;
                        photoCamLog.CreatedByUnit = photoCamData.CreatedByUnit;
                        photoCamLog.CreatedTime = photoCamData.CreatedTime;
                        photoCamLog.FileName = photoCamData.FileName;
                        photoCamLog.Nik = photoCamData.Nik;
                        photoCamLog.PathFile = photoCamData.PathFile;

                        photoCamData.PathFile = filePath;
                        photoCamData.Nik = enroll.KtpNIK;
                        photoCamData.FileName = fileName;
                        photoCamData.IsActive = true;
                        photoCamData.IsDeleted = false;
                        photoCamData.UpdatedById = cekNPPCs.PegawaiId;
                        photoCamData.UpdatedByNpp = cekNPPCs.NIK;
                        photoCamData.UpdatedByUid = enroll.UID;
                        photoCamData.UpdatedTime = DateTime.Now;
                    }
                }
                else
                {
                    stringPerubahan = stringPerubahan + "Request Payload Photo Cam Tidak Ditemukan" + " <br/>";
                }
                #endregion

                #region finger
                var exPhotoFinger = new List<Tbl_DataKTP_Finger>();

                var exPhotoFingerEmployee = new List<Tbl_DataKTP_Finger_Employee>();

                var photoFingerLogs = new List<Tbl_DataKTP_Finger_Log>();

                var photoFingers = new List<Tbl_DataKTP_Finger>();

                var photoFingersEmployee = new List<Tbl_DataKTP_Finger_Employee>();

                var photoFingersEmployeeLogs = new List<Tbl_DataKTP_Finger_Employee_Log>();

                if (!string.IsNullOrWhiteSpace(enroll.KtpFingerKanan))
                {

                    var photoFingerData = await _profileRepository.GetPhotoFinger(enroll.KtpNIK, enroll.KtpTypeJariKanan)
                        .ConfigureAwait(false);

                    var photoFingerDataEmployee = new Tbl_DataKTP_Finger_Employee();

                    if (isEmployee)
                    {
                        photoFingerDataEmployee = await _profileRepository.GetPhotoFingerEmployee(enroll.KtpNIK, enroll.KtpTypeJariKanan)
                        .ConfigureAwait(false);
                    }

                    string imageEncrypted = enroll.KtpFingerKanan.Encrypt(Phrase.FileEncryption);

                    string filePath = "";
                    string filePathIso = "";

                    var systemParameterPath = await _sysParameterRepository.GetPathFolder("FolderFinger")
                                                .ConfigureAwait(false);
                    string pathFolderFoto = systemParameterPath.Value;

                    string subPathFolderPhotoFinger = pathFolder + "/" + pathFolderFoto + "/" + enroll.KtpNIK + "/";

                    string JenisJari = enroll.KtpTypeJariKanan.Replace(" ", "");
                    string fileName = "PhotoFinger_" + JenisJari + "_" + enroll.KtpNIK + "_" + JamServer + ".txt";
                    string fileNameIso = "";

                    if (_sftpConfig.IsActive)
                    {
                        using var stream = new MemoryStream(Encoding.ASCII.GetBytes(imageEncrypted));

                        (var fname, var fPath) = await stream.UploadToFTPServer(subPathFolderPhotoFinger, fileName, _sftpConfig.Host,
                            _sftpConfig.Username, _sftpConfig.Password, _sftpConfig.RootDirectory, _sftpConfig.Url).ConfigureAwait(false);

                        filePath = fPath;
                    }
                    else
                    {
                        if (!Directory.Exists(subPathFolderPhotoFinger))
                        {
                            Directory.CreateDirectory(subPathFolderPhotoFinger);
                        }

                        filePath = subPathFolderPhotoFinger + fileName;
                        File.WriteAllText(filePath, imageEncrypted);
                    }

                    //if (!string.IsNullOrWhiteSpace(enroll.KtpFingerKananIso))
                    //{
                    //    string isoEncrypted = enroll.KtpFingerKananIso.Encrypt(Phrase.FileEncryption);
                    //    fileNameIso = "PhotoFingerISO_" + JenisJari + "_" + enroll.KtpNIK + "_" + JamServer + ".txt";

                    //    if (_sftpConfig.IsActive)
                    //    {
                    //        using var stream = new MemoryStream(Encoding.ASCII.GetBytes(isoEncrypted));

                    //        (var fname, var fPath) = await stream.UploadToFTPServer(subPathFolderPhotoFinger, fileNameIso, _sftpConfig.Host,
                    //            _sftpConfig.Username, _sftpConfig.Password, _sftpConfig.RootDirectory, _sftpConfig.Url).ConfigureAwait(false);

                    //        filePathIso = fPath;
                    //    }
                    //    else
                    //    {
                    //        if (!Directory.Exists(subPathFolderPhotoFinger))
                    //        {
                    //            Directory.CreateDirectory(subPathFolderPhotoFinger);
                    //        }

                    //        filePathIso = subPathFolderPhotoFinger + fileNameIso;
                    //        File.WriteAllText(filePathIso, isoEncrypted);
                    //    }
                    //}

                    if (photoFingerData == null)
                    {
                        photoFingerData = new Tbl_DataKTP_Finger
                        {
                            CreatedById = cekNPPCs.PegawaiId,
                            IsActive = true,
                            IsDeleted = false,
                            Nik = enroll.KtpNIK,
                            PathFile = filePath,
                            //PathFileISO = filePathIso,
                            CreatedByNpp = cekNPPCs.NIK,
                            CreatedByUid = enroll.UID,
                            CreatedTime = DateTime.Now,
                            CreatedByUnit = cekNPPCs.Kode_Unit,
                            FileName = fileName,
                            //FileNameISO = fileNameIso,
                            //FileJariISO = enroll.KtpFingerKananIso,
                            TypeFinger = enroll.KtpTypeJariKanan
                        };

                        photoFingers.Add(new Tbl_DataKTP_Finger
                        {
                            CreatedById = photoFingerData.CreatedById,
                            IsActive = true,
                            IsDeleted = false,
                            Nik = enroll.KtpNIK,
                            PathFile = filePath,
                            //PathFileISO = filePathIso,
                            CreatedByNpp = photoFingerData.CreatedByNpp,
                            CreatedByUid = photoFingerData.CreatedByUid,
                            CreatedTime = photoFingerData.CreatedTime,
                            UpdatedById = cekNPPCs.PegawaiId,
                            UpdatedByNpp = cekNPPCs.NIK,
                            UpdatedByUid = enroll.UID,
                            UpdatedByUnit = cekNPPCs.Kode_Unit,
                            CreatedByUnitId = photoFingerData.CreatedByUnitId,
                            CreatedByUnit = photoFingerData.CreatedByUnit,
                            //FileJari = photoFingerData.FileJari,
                            //FileJariISO = photoFingerData.FileJariISO,
                            UpdatedTime = DateTime.Now,
                            FileName = fileName,
                            //FileNameISO = fileNameIso,
                            TypeFinger = enroll.KtpTypeJariKanan
                        });
                    }
                    else
                    {
                        photoFingerLogs.Add(new Tbl_DataKTP_Finger_Log
                        {
                            CreatedById = photoFingerData.CreatedById,
                            CreatedByNpp = photoFingerData.CreatedByNpp,
                            CreatedByUid = photoFingerData.CreatedByUid,
                            CreatedByUnit = photoFingerData.CreatedByUnit,
                            CreatedByUnitId = photoFingerData.CreatedByUnitId,
                            CreatedTime = photoFingerData.CreatedTime,
                            //FileJari = photoFingerData.FileJari,
                            //FileJariISO = photoFingerData.FileJariISO,
                            //FileName = photoFingerData.FileName,
                            //FileNameISO = photoFingerData.FileNameISO,
                            Nik = photoFingerData.Nik,
                            PathFile = photoFingerData.PathFile,
                            PathFileISO = photoFingerData.PathFileISO,
                            TypeFinger = photoFingerData.TypeFinger
                        });

                        exPhotoFinger.Add(photoFingerData);

                        photoFingers.Add(new Tbl_DataKTP_Finger
                        {
                            CreatedById = photoFingerData.CreatedById,
                            IsActive = true,
                            IsDeleted = false,
                            Nik = enroll.KtpNIK,
                            PathFile = filePath,
                            //PathFileISO = filePathIso,
                            CreatedByNpp = photoFingerData.CreatedByNpp,
                            CreatedByUid = photoFingerData.CreatedByUid,
                            CreatedTime = photoFingerData.CreatedTime,
                            UpdatedById = cekNPPCs.PegawaiId,
                            UpdatedByNpp = cekNPPCs.NIK,
                            UpdatedByUid = enroll.UID,
                            UpdatedByUnit = cekNPPCs.Kode_Unit,
                            CreatedByUnitId = photoFingerData.CreatedByUnitId,
                            CreatedByUnit = photoFingerData.CreatedByUnit,
                            //FileJari = enroll.KtpFingerKanan,
                            UpdatedTime = DateTime.Now,
                            FileName = fileName,
                            //FileNameISO = fileNameIso,
                            //FileJariISO = enroll.KtpFingerKananIso,
                            TypeFinger = enroll.KtpTypeJariKanan
                        });
                    }

                    if (isEmployee)
                    {
                        if (photoFingerDataEmployee == null)
                        {
                            photoFingerDataEmployee = new Tbl_DataKTP_Finger_Employee
                            {
                                CreatedById = cekNPPCs.PegawaiId,
                                IsActive = true,
                                IsDeleted = false,
                                Nik = enroll.KtpNIK,
                                PathFile = filePath,//
                                //PathFileISO = filePathIso,//
                                CreatedByNpp = cekNPPCs.NIK,
                                CreatedByUid = enroll.UID,
                                CreatedTime = DateTime.Now,
                                FileName = fileName,//
                                //FileNameISO = fileNameIso,//
                                //FileJariISO = enroll.KtpFingerKananIso,
                                TypeFinger = enroll.KtpTypeJariKanan
                            };

                            photoFingersEmployee.Add(new Tbl_DataKTP_Finger_Employee
                            {
                                CreatedById = photoFingerDataEmployee.CreatedById,
                                IsActive = true,
                                IsDeleted = false,
                                Nik = enroll.KtpNIK,
                                PathFile = filePath,
                                //PathFileISO = filePathIso,
                                CreatedByNpp = photoFingerDataEmployee.CreatedByNpp,
                                CreatedByUid = photoFingerDataEmployee.CreatedByUid,
                                CreatedTime = photoFingerDataEmployee.CreatedTime,
                                UpdatedById = cekNPPCs.PegawaiId,
                                UpdatedByNpp = cekNPPCs.NIK,
                                UpdatedByUid = enroll.UID,
                                UpdatedByUnit = cekNPPCs.Kode_Unit,
                                CreatedByUnitId = photoFingerDataEmployee.CreatedByUnitId,
                                CreatedByUnit = photoFingerDataEmployee.CreatedByUnit,
                                //FileJari = photoFingerDataEmployee.FileJari,
                                //FileJariISO = photoFingerDataEmployee.FileJariISO,
                                UpdatedTime = DateTime.Now,
                                FileName = fileName,
                                //FileNameISO = fileNameIso,
                                TypeFinger = enroll.KtpTypeJariKanan
                            });
                        }
                        else
                        {
                            photoFingersEmployeeLogs.Add(new Tbl_DataKTP_Finger_Employee_Log
                            {
                                CreatedById = photoFingerDataEmployee.CreatedById,
                                CreatedByNpp = photoFingerDataEmployee.CreatedByNpp,
                                CreatedByUid = photoFingerDataEmployee.CreatedByUid,
                                CreatedByUnit = photoFingerDataEmployee.CreatedByUnit,
                                CreatedByUnitId = photoFingerDataEmployee.CreatedByUnitId,
                                CreatedTime = photoFingerDataEmployee.CreatedTime,
                                //FileJari = photoFingerDataEmployee.FileJari,
                                //FileJariISO = photoFingerDataEmployee.FileJariISO,
                                FileName = photoFingerDataEmployee.FileName,
                                //FileNameISO = photoFingerDataEmployee.FileNameISO,
                                Nik = photoFingerDataEmployee.Nik,
                                PathFile = photoFingerDataEmployee.PathFile,
                                //PathFileISO = photoFingerDataEmployee.PathFileISO,
                                TypeFinger = photoFingerDataEmployee.TypeFinger
                            });

                            exPhotoFingerEmployee.Add(photoFingerDataEmployee);

                            photoFingersEmployee.Add(new Tbl_DataKTP_Finger_Employee
                            {
                                CreatedById = photoFingerDataEmployee.CreatedById,
                                IsActive = true,
                                IsDeleted = false,
                                Nik = enroll.KtpNIK,
                                PathFile = filePath,
                                //PathFileISO = filePathIso,
                                CreatedByNpp = photoFingerDataEmployee.CreatedByNpp,
                                CreatedByUid = photoFingerDataEmployee.CreatedByUid,
                                CreatedTime = photoFingerDataEmployee.CreatedTime,
                                UpdatedById = cekNPPCs.PegawaiId,
                                UpdatedByNpp = cekNPPCs.NIK,
                                UpdatedByUid = enroll.UID,
                                UpdatedByUnit = cekNPPCs.Kode_Unit,
                                CreatedByUnitId = photoFingerDataEmployee.CreatedByUnitId,
                                CreatedByUnit = photoFingerDataEmployee.CreatedByUnit,
                                //FileJari = enroll.KtpFingerKanan,
                                //FileJariISO = enroll.KtpFingerKananIso,
                                UpdatedTime = DateTime.Now,
                                FileName = fileName,
                                //FileNameISO = fileNameIso,
                                TypeFinger = enroll.KtpTypeJariKanan
                            });
                        }
                    }
                }
                else
                {
                    #region get data if no payload but we have to replace all data with no data
                    var photoFingerData = await _profileRepository.GetPhotoFinger(enroll.KtpNIK, enroll.KtpTypeJariKanan)
                        .ConfigureAwait(false);
                    if (photoFingerData != null)
                    {
                        exPhotoFinger.Add(photoFingerData);

                        photoFingerLogs.Add(new Tbl_DataKTP_Finger_Log
                        {
                            CreatedById = photoFingerData.CreatedById,
                            CreatedByNpp = photoFingerData.CreatedByNpp,
                            CreatedByUid = photoFingerData.CreatedByUid,
                            CreatedByUnit = photoFingerData.CreatedByUnit,
                            CreatedByUnitId = photoFingerData.CreatedByUnitId,
                            CreatedTime = photoFingerData.CreatedTime,
                            //FileJari = photoFingerData.FileJari,
                            FileName = photoFingerData.FileName,
                            //FileNameISO = photoFingerData.FileNameISO,
                            Nik = photoFingerData.Nik,
                            PathFile = photoFingerData.PathFile,
                            TypeFinger = photoFingerData.TypeFinger
                        });
                    }

                    if (isEmployee)
                    {
                        var photoFingerDataEmployee = await _profileRepository.GetPhotoFingerEmployee(enroll.KtpNIK, enroll.KtpTypeJariKanan)
                        .ConfigureAwait(false);
                        if (photoFingerDataEmployee != null)
                        {
                            exPhotoFingerEmployee.Add(photoFingerDataEmployee);

                            photoFingersEmployeeLogs.Add(new Tbl_DataKTP_Finger_Employee_Log
                            {
                                CreatedById = photoFingerDataEmployee.CreatedById,
                                CreatedByNpp = photoFingerDataEmployee.CreatedByNpp,
                                CreatedByUid = photoFingerDataEmployee.CreatedByUid,
                                CreatedByUnit = photoFingerDataEmployee.CreatedByUnit,
                                CreatedByUnitId = photoFingerDataEmployee.CreatedByUnitId,
                                CreatedTime = photoFingerDataEmployee.CreatedTime,
                                //FileJari = photoFingerDataEmployee.FileJari,
                                FileName = photoFingerDataEmployee.FileName,
                                //FileNameISO = photoFingerDataEmployee.FileNameISO,
                                Nik = photoFingerDataEmployee.Nik,
                                PathFile = photoFingerDataEmployee.PathFile,
                                //PathFileISO = photoFingerDataEmployee.PathFileISO,
                                TypeFinger = photoFingerDataEmployee.TypeFinger
                            });
                        }
                    }
                    #endregion

                    stringPerubahan = stringPerubahan + "Request Payload Finger Kanan Tidak Ditemukan" + " <br/>";
                }

                if (!string.IsNullOrWhiteSpace(enroll.KtpFingerKiri))
                {
                    var photoFingerData = await _profileRepository.GetPhotoFinger(enroll.KtpNIK, enroll.KtpTypeJariKiri)
                        .ConfigureAwait(false);

                    var photoFingerDataEmployee = new Tbl_DataKTP_Finger_Employee();

                    if (isEmployee)
                    {
                        photoFingerDataEmployee = await _profileRepository.GetPhotoFingerEmployee(enroll.KtpNIK, enroll.KtpTypeJariKiri)
                        .ConfigureAwait(false);
                    }

                    string imageEncrypted = enroll.KtpFingerKiri.Encrypt(Phrase.FileEncryption);

                    string filePath = "";
                    string filePathIso = "";

                    var systemParameterPath = await _sysParameterRepository.GetPathFolder("FolderFinger")
                                                .ConfigureAwait(false);
                    string pathFolderFoto = systemParameterPath.Value;

                    string subPathFolderPhotoFinger = pathFolder + "/" + pathFolderFoto + "/" + enroll.KtpNIK + "/";

                    string JenisJari = enroll.KtpTypeJariKiri.Replace(" ", "");
                    string fileName = "PhotoFinger_" + JenisJari + "_" + enroll.KtpNIK + "_" + JamServer + ".txt";
                    string fileNameIso = "";

                    if (_sftpConfig.IsActive)
                    {
                        using var stream = new MemoryStream(Encoding.ASCII.GetBytes(imageEncrypted));

                        (var fname, var fPath) = await stream.UploadToFTPServer(subPathFolderPhotoFinger, fileName, _sftpConfig.Host,
                            _sftpConfig.Username, _sftpConfig.Password, _sftpConfig.RootDirectory, _sftpConfig.Url).ConfigureAwait(false);

                        filePath = fPath;
                    }
                    else
                    {
                        if (!Directory.Exists(subPathFolderPhotoFinger))
                        {
                            Directory.CreateDirectory(subPathFolderPhotoFinger);
                        }

                        filePath = subPathFolderPhotoFinger + fileName;
                        File.WriteAllText(filePath, imageEncrypted);
                    }

                    //if (!string.IsNullOrWhiteSpace(enroll.KtpFingerKiriIso))
                    //{
                    //    string isoEncrypted = enroll.KtpFingerKiriIso.Encrypt(Phrase.FileEncryption);
                    //    fileNameIso = "PhotoFingerISO_" + JenisJari + "_" + enroll.KtpNIK + "_" + JamServer + ".txt";

                    //    if (_sftpConfig.IsActive)
                    //    {
                    //        using var stream = new MemoryStream(Encoding.ASCII.GetBytes(isoEncrypted));

                    //        (var fname, var fPath) = await stream.UploadToFTPServer(subPathFolderPhotoFinger, fileNameIso, _sftpConfig.Host,
                    //            _sftpConfig.Username, _sftpConfig.Password, _sftpConfig.RootDirectory, _sftpConfig.Url).ConfigureAwait(false);

                    //        filePathIso = fPath;
                    //    }
                    //    else
                    //    {
                    //        if (!Directory.Exists(subPathFolderPhotoFinger))
                    //        {
                    //            Directory.CreateDirectory(subPathFolderPhotoFinger);
                    //        }

                    //        filePathIso = subPathFolderPhotoFinger + fileNameIso;
                    //        File.WriteAllText(filePathIso, isoEncrypted);
                    //    }
                    //}

                    if (photoFingerData == null)
                    {
                        photoFingerData = new Tbl_DataKTP_Finger
                        {
                            CreatedById = cekNPPCs.PegawaiId,
                            IsActive = true,
                            IsDeleted = false,
                            Nik = enroll.KtpNIK,
                            PathFile = filePath,
                            //PathFileISO = filePathIso,
                            CreatedByNpp = cekNPPCs.NIK,
                            CreatedByUid = enroll.UID,
                            CreatedTime = DateTime.Now,
                            FileName = fileName,
                            //FileNameISO = fileNameIso,
                            //FileJariISO = enroll.KtpFingerKiriIso,
                            TypeFinger = enroll.KtpTypeJariKiri
                        };

                        photoFingers.Add(new Tbl_DataKTP_Finger
                        {
                            CreatedById = photoFingerData.CreatedById,
                            IsActive = true,
                            IsDeleted = false,
                            Nik = enroll.KtpNIK,
                            PathFile = filePath,
                            //PathFileISO = filePathIso,
                            //FileNameISO = fileNameIso,
                            CreatedByNpp = photoFingerData.CreatedByNpp,
                            CreatedByUid = photoFingerData.CreatedByUid,
                            CreatedTime = photoFingerData.CreatedTime,
                            UpdatedById = cekNPPCs.PegawaiId,
                            UpdatedByNpp = cekNPPCs.NIK,
                            UpdatedByUid = enroll.UID,
                            UpdatedByUnit = cekNPPCs.Kode_Unit,
                            CreatedByUnitId = photoFingerData.CreatedByUnitId,
                            CreatedByUnit = photoFingerData.CreatedByUnit,
                            //FileJari = photoFingerData.FileJari,
                            //FileJariISO = photoFingerData.FileJariISO,
                            UpdatedTime = DateTime.Now,
                            FileName = fileName,
                            TypeFinger = enroll.KtpTypeJariKiri
                        });
                    }
                    else
                    {
                        photoFingerLogs.Add(new Tbl_DataKTP_Finger_Log
                        {
                            CreatedById = photoFingerData.CreatedById,
                            CreatedByNpp = photoFingerData.CreatedByNpp,
                            CreatedByUid = photoFingerData.CreatedByUid,
                            CreatedByUnit = photoFingerData.CreatedByUnit,
                            CreatedByUnitId = photoFingerData.CreatedByUnitId,
                            CreatedTime = photoFingerData.CreatedTime,
                            //FileJari = photoFingerData.FileJari,
                            //FileJariISO = photoFingerData.FileJariISO,
                            FileName = photoFingerData.FileName,
                            //PathFileISO = photoFingerData.PathFileISO,
                            //FileNameISO = photoFingerData.FileNameISO,
                            Nik = photoFingerData.Nik,
                            PathFile = photoFingerData.PathFile,
                            TypeFinger = photoFingerData.TypeFinger
                        });

                        exPhotoFinger.Add(photoFingerData);

                        photoFingers.Add(new Tbl_DataKTP_Finger
                        {
                            CreatedById = photoFingerData.CreatedById,
                            IsActive = true,
                            IsDeleted = false,
                            Nik = enroll.KtpNIK,
                            PathFile = filePath,
                            //PathFileISO = filePathIso,
                            CreatedByNpp = photoFingerData.CreatedByNpp,
                            CreatedByUid = photoFingerData.CreatedByUid,
                            CreatedTime = photoFingerData.CreatedTime,
                            UpdatedById = cekNPPCs.PegawaiId,
                            UpdatedByNpp = cekNPPCs.NIK,
                            UpdatedByUid = enroll.UID,
                            UpdatedByUnit = cekNPPCs.Kode_Unit,
                            CreatedByUnitId = photoFingerData.CreatedByUnitId,
                            CreatedByUnit = photoFingerData.CreatedByUnit,
                            //FileJari = enroll.KtpFingerKiri,
                            //FileJariISO = enroll.KtpFingerKiriIso,
                            UpdatedTime = DateTime.Now,
                            FileName = fileName,
                            //FileNameISO = fileNameIso,
                            TypeFinger = enroll.KtpTypeJariKiri
                        });
                    }

                    if (isEmployee)
                    {
                        if (photoFingerDataEmployee == null)
                        {
                            photoFingerDataEmployee = new Tbl_DataKTP_Finger_Employee
                            {
                                CreatedById = cekNPPCs.PegawaiId,
                                IsActive = true,
                                IsDeleted = false,
                                Nik = enroll.KtpNIK,
                                PathFile = filePath,//
                                //PathFileISO = filePathIso,//
                                CreatedByNpp = cekNPPCs.NIK,
                                CreatedByUid = enroll.UID,
                                CreatedTime = DateTime.Now,
                                FileName = fileName,//
                                //FileNameISO = fileNameIso,//
                                //FileJariISO = enroll.KtpFingerKiriIso,
                                //FileJari = enroll.KtpFingerKiri,
                                TypeFinger = enroll.KtpTypeJariKiri
                            };

                            photoFingersEmployee.Add(new Tbl_DataKTP_Finger_Employee
                            {
                                CreatedById = photoFingerDataEmployee.CreatedById,
                                IsActive = true,
                                IsDeleted = false,
                                Nik = enroll.KtpNIK,
                                PathFile = filePath,
                                //PathFileISO = filePathIso,
                                CreatedByNpp = photoFingerDataEmployee.CreatedByNpp,
                                CreatedByUid = photoFingerDataEmployee.CreatedByUid,
                                CreatedTime = photoFingerDataEmployee.CreatedTime,
                                UpdatedById = cekNPPCs.PegawaiId,
                                UpdatedByNpp = cekNPPCs.NIK,
                                UpdatedByUid = enroll.UID,
                                UpdatedByUnit = cekNPPCs.Kode_Unit,
                                CreatedByUnitId = photoFingerDataEmployee.CreatedByUnitId,
                                CreatedByUnit = photoFingerDataEmployee.CreatedByUnit,
                                //FileJari = photoFingerDataEmployee.FileJari,
                                //FileJariISO = photoFingerDataEmployee.FileJariISO,
                                UpdatedTime = DateTime.Now,
                                FileName = fileName,
                                //FileNameISO = fileNameIso,
                                TypeFinger = enroll.KtpTypeJariKiri
                            });
                        }
                        else
                        {
                            photoFingersEmployeeLogs.Add(new Tbl_DataKTP_Finger_Employee_Log
                            {
                                CreatedById = photoFingerDataEmployee.CreatedById,
                                CreatedByNpp = photoFingerDataEmployee.CreatedByNpp,
                                CreatedByUid = photoFingerDataEmployee.CreatedByUid,
                                CreatedByUnit = photoFingerDataEmployee.CreatedByUnit,
                                CreatedByUnitId = photoFingerDataEmployee.CreatedByUnitId,
                                CreatedTime = photoFingerDataEmployee.CreatedTime,
                                //FileJari = photoFingerDataEmployee.FileJari,
                                //FileJariISO = photoFingerDataEmployee.FileJariISO,
                                FileName = photoFingerDataEmployee.FileName,
                                //FileNameISO = photoFingerDataEmployee.FileNameISO,
                                Nik = photoFingerDataEmployee.Nik,
                                PathFile = photoFingerDataEmployee.PathFile,
                                //PathFileISO = photoFingerDataEmployee.PathFileISO,
                                TypeFinger = photoFingerDataEmployee.TypeFinger
                            });

                            exPhotoFingerEmployee.Add(photoFingerDataEmployee);

                            photoFingersEmployee.Add(new Tbl_DataKTP_Finger_Employee
                            {
                                CreatedById = photoFingerDataEmployee.CreatedById,
                                IsActive = true,
                                IsDeleted = false,
                                Nik = enroll.KtpNIK,
                                PathFile = filePath,
                                //PathFileISO = filePathIso,
                                CreatedByNpp = photoFingerDataEmployee.CreatedByNpp,
                                CreatedByUid = photoFingerDataEmployee.CreatedByUid,
                                CreatedTime = photoFingerDataEmployee.CreatedTime,
                                UpdatedById = cekNPPCs.PegawaiId,
                                UpdatedByNpp = cekNPPCs.NIK,
                                UpdatedByUid = enroll.UID,
                                UpdatedByUnit = cekNPPCs.Kode_Unit,
                                CreatedByUnitId = photoFingerDataEmployee.CreatedByUnitId,
                                CreatedByUnit = photoFingerDataEmployee.CreatedByUnit,
                                //FileJari = enroll.KtpFingerKiri,
                                //FileJariISO = enroll.KtpFingerKiriIso,
                                UpdatedTime = DateTime.Now,
                                FileName = fileName,
                                //FileNameISO = fileNameIso,
                                TypeFinger = enroll.KtpTypeJariKiri
                            });
                        }
                    }
                }
                else
                {
                    #region get data if no payload but we have to replace all data with no data
                    var photoFingerData = await _profileRepository.GetPhotoFinger(enroll.KtpNIK, enroll.KtpTypeJariKiri)
                        .ConfigureAwait(false);
                    if (photoFingerData != null)
                    {
                        exPhotoFinger.Add(photoFingerData);

                        photoFingerLogs.Add(new Tbl_DataKTP_Finger_Log
                        {
                            CreatedById = photoFingerData.CreatedById,
                            CreatedByNpp = photoFingerData.CreatedByNpp,
                            CreatedByUid = photoFingerData.CreatedByUid,
                            CreatedByUnit = photoFingerData.CreatedByUnit,
                            CreatedByUnitId = photoFingerData.CreatedByUnitId,
                            CreatedTime = photoFingerData.CreatedTime,
                            //FileJari = photoFingerData.FileJari,
                            FileName = photoFingerData.FileName,
                            //FileNameISO = photoFingerData.FileNameISO,
                            Nik = photoFingerData.Nik,
                            PathFile = photoFingerData.PathFile,
                            //PathFileISO = photoFingerData.PathFileISO,
                            TypeFinger = photoFingerData.TypeFinger
                        });
                    }
                    if (isEmployee)
                    {
                        var photoFingerDataEmployee = await _profileRepository.GetPhotoFingerEmployee(enroll.KtpNIK, enroll.KtpTypeJariKiri)
                        .ConfigureAwait(false);
                        if (photoFingerDataEmployee != null)
                        {
                            exPhotoFingerEmployee.Add(photoFingerDataEmployee);

                            photoFingersEmployeeLogs.Add(new Tbl_DataKTP_Finger_Employee_Log
                            {
                                CreatedById = photoFingerDataEmployee.CreatedById,
                                CreatedByNpp = photoFingerDataEmployee.CreatedByNpp,
                                CreatedByUid = photoFingerDataEmployee.CreatedByUid,
                                CreatedByUnit = photoFingerDataEmployee.CreatedByUnit,
                                CreatedByUnitId = photoFingerDataEmployee.CreatedByUnitId,
                                CreatedTime = photoFingerDataEmployee.CreatedTime,
                                //FileJari = photoFingerDataEmployee.FileJari,
                                FileName = photoFingerDataEmployee.FileName,
                                //FileNameISO = photoFingerDataEmployee.FileNameISO,
                                Nik = photoFingerDataEmployee.Nik,
                                PathFile = photoFingerDataEmployee.PathFile,
                                //PathFileISO = photoFingerDataEmployee.PathFileISO,
                                TypeFinger = photoFingerDataEmployee.TypeFinger
                            });
                        }
                    }
                    #endregion

                    stringPerubahan = stringPerubahan + "Request Payload Finger Kanan Tidak Ditemukan" + " <br/>";
                }
                #endregion

                cekKTP.isEnrollFR = false;

                var dataReaderLog = new Tbl_MasterAlatReaderLog();

                var (status, msg) = await _enrollmentKTPRepository.InsertEnrollFlow2(cekKTP, logDemografi, photoKTPData, photoKtpLog, photoSignatureData,
                    photoSignatureLog, photoCamData, photoCamLog, photoFingers, photoFingersEmployee, photoFingerLogs, photoFingersEmployeeLogs, exPhotoKtp, exphotoSignatureData, exphotoCamData, exPhotoFinger, exPhotoFingerEmployee, dataReaderLog, dataNpp);



                if (status)
                {
                    await _alatReaderRepository.InsertLogEnrollThirdParty(new Tbl_Enrollment_ThirdParty_Log
                    {
                        NIK = enroll.KtpNIK,
                        AppsChannel = AppsChannel,
                        SubmitDate = DateTime.Now

                    });
                    return (_SuccessMessageConfig.NasabahSuksesUpdate, (int)EnrollStatus.Nasabah_Berhasil_di_enroll, cekKTP.CIF);
                }
                else
                {
                    return (_ErrorMessageConfig.DemografiGagalEnroll, (int)ServiceResponseStatus.ERROR, msg + " " + stringPerubahan);
                }
            }

            if (IsNewEnroll)
            {
                if (enroll?.KtpNIK == null) return (_ErrorMessageConfig.InputTidakLengkap, (int)EnrollStatus.Inputan_tidak_lengkap, "");

                var cifData = new ApiSOAResponse();
                #region Hit SOA And Loggging it
                if (isHitSOA == true)
                {
                    cifData = await _cifService.GetSOAByCif(ReqSoa)
                    .ConfigureAwait(false);

                    var status = 0;
                    if (cifData.cif != null)
                    {
                        status = 1;
                    }

                    var _log = new Tbl_ThirdPartyLog
                    {
                        FeatureName = "SubmitEnrollmentFingerEncryptedOnly",
                        HostUrl = ReqSoa.host,
                        Request = Newtonsoft.Json.JsonConvert.SerializeObject(ReqSoa),
                        Status = status,
                        Response = Newtonsoft.Json.JsonConvert.SerializeObject(cifData),
                        CreatedDate = System.DateTime.Now,
                        CreatedBy = cekNPPCs.NIK
                    };

                    _ = _enrollmentKTPRepository.CreateThirdPartyLog(_log);
                }
                else
                {
                    var res = await _cifService.GetCIF(new NikDto { Nik = enroll.KtpNIK },
                        new NikDtoUrl { baseUrl = ReqSoa.baseUrlNonSoa, endpoint = ReqSoa.UrlEndPointNonSoa });
                    if (res.Data == null)
                    {
                        cifData.cif = null;
                    }
                    else
                    {
                        cifData.cif = res.Data.Cif;
                    };
                }
                #endregion

                if (!String.IsNullOrEmpty(cifData.cif))
                {
                    IsNasabah = true;
                    cifData.cif = cifData.cif.Trim();
                }

                var sysPathFolder = await _sysParameterRepository.GetPathFolder("PathFolderData");
                string pathFolder = sysPathFolder.Value;

                var dataDemografis = new Tbl_DataKTP_Demografis();

                var dataDemografisLog = new Tbl_DataKTP_Demografis_Log();

                var dataNpp = new Tbl_Mapping_Pegawai_KTP();

                if (cekKTP != null)
                {
                    dataDemografisLog = new Tbl_DataKTP_Demografis_Log
                    {
                        Agama = cekKTP.Agama,
                        Alamat = cekKTP.Alamat,
                        AlamatGoogle = cekKTP.AlamatGoogle,
                        AlamatLengkap = cekKTP.AlamatLengkap,
                        CreatedById = cekKTP.CreatedById,
                        CreatedByUID = cekKTP.CreatedByUID,
                        CreatedByUnitId = cekKTP.CreatedByUnitId,
                        CreatedTime = cekKTP.CreatedTime,
                        Desa = cekKTP.Desa,
                        GolonganDarah = cekKTP.GolonganDarah,
                        JenisKelamin = cekKTP.JenisKelamin,
                        Kecamatan = cekKTP.Kecamatan,
                        Kelurahan = cekKTP.Kelurahan,
                        Kewarganegaraan = cekKTP.Kewarganegaraan,
                        KodePos = cekKTP.KodePos,
                        Kota = cekKTP.Kota,
                        Latitude = cekKTP.Latitude,
                        Longitude = cekKTP.Longitude,
                        MasaBerlaku = cekKTP.MasaBerlaku,
                        Nama = cekKTP.Nama,
                        NIK = cekKTP.NIK,
                        Pekerjaan = cekKTP.Pekerjaan,
                        Provinsi = cekKTP.Provinsi,
                        RT = cekKTP.RT,
                        RW = cekKTP.RW,
                        StatusPerkawinan = cekKTP.StatusPerkawinan,
                        TanggalLahir = cekKTP.TanggalLahir,
                        TempatLahir = cekKTP.TempatLahir
                    };

                    cekKTP.Agama = enroll.KtpAgama;
                    cekKTP.Alamat = enroll.KtpAlamat;
                    cekKTP.AlamatGoogle = enroll.KtpAlamatConvertLatlong;
                    cekKTP.AlamatLengkap = enroll.KtpAlamatConvertLengkap;
                    if (string.IsNullOrWhiteSpace(cifData.cif))
                    {
                        cekKTP.CIF = cifData.cif;
                    }
                    cekKTP.GolonganDarah = enroll.KtpGolonganDarah;
                    cekKTP.JenisKelamin = enroll.KtpJanisKelamin;
                    cekKTP.Kecamatan = enroll.KtpKecamatan;
                    cekKTP.Kelurahan = enroll.KtpKelurahan;
                    cekKTP.Kewarganegaraan = enroll.KtpKewarganegaraan;
                    cekKTP.KodePos = enroll.KtpKodePos;
                    cekKTP.Kota = enroll.KtpKota;
                    cekKTP.Latitude = enroll.KtpLatitude;
                    cekKTP.Longitude = enroll.KtpLongitude;
                    cekKTP.MasaBerlaku = enroll.KtpMasaBerlaku;
                    cekKTP.Nama = enroll.KtpNama;
                    cekKTP.NIK = enroll.KtpNIK;
                    cekKTP.Pekerjaan = enroll.KtpPekerjaan;
                    cekKTP.Provinsi = enroll.KtpProvinsi;
                    cekKTP.RT = enroll.KtpRT;
                    cekKTP.RW = enroll.KtpRW;
                    cekKTP.StatusPerkawinan = enroll.KtpStatusPerkawinan;
                    cekKTP.TanggalLahir = string.IsNullOrWhiteSpace(enroll.KtpTanggalLahir) ? null : DateTime.ParseExact(enroll.KtpTanggalLahir, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                    cekKTP.TempatLahir = enroll.KtpTempatLahir;
                    cekKTP.UpdatedById = cekNPPCs.PegawaiId;
                    cekKTP.UpdatedByNpp = cekNPPCs.NIK;
                    cekKTP.UpdatedByUID = enroll.UID;
                    cekKTP.UpdatedTime = DateTime.Now;
                    cekKTP.UpdatedByUnitCode = cekNPPCs.Kode_Unit;
                    cekKTP.UpdatedByUnitId = int.Parse(cekNPPCs.Unit_Id);
                }
                else
                {
                    dataDemografis = new Tbl_DataKTP_Demografis
                    {
                        Agama = enroll.KtpAgama,
                        Alamat = enroll.KtpAlamat,
                        AlamatGoogle = enroll.KtpAlamatConvertLatlong,
                        AlamatLengkap = enroll.KtpAlamatConvertLengkap,
                        CIF = (cifData.cif),
                        NIK = enroll.KtpNIK,
                        CreatedById = cekNPPCs.PegawaiId,
                        IsActive = true,
                        IsDeleted = false,
                        JenisKelamin = enroll.KtpJanisKelamin,
                        Kecamatan = enroll.KtpKecamatan,
                        Kelurahan = enroll.KtpKelurahan,
                        CreatedByUID = enroll.UID,
                        CreatedTime = DateTime.Now,
                        GolonganDarah = enroll.KtpGolonganDarah,
                        Kewarganegaraan = enroll.KtpKewarganegaraan,
                        KodePos = enroll.KtpKodePos,
                        Kota = enroll.KtpKota,
                        Latitude = enroll.KtpLatitude,
                        Longitude = enroll.KtpLongitude,
                        MasaBerlaku = enroll.KtpMasaBerlaku,
                        Nama = enroll.KtpNama,
                        Pekerjaan = enroll.KtpPekerjaan,
                        Provinsi = enroll.KtpProvinsi,
                        RT = enroll.KtpRT,
                        RW = enroll.KtpRW,
                        StatusPerkawinan = enroll.KtpStatusPerkawinan,
                        TanggalLahir = string.IsNullOrWhiteSpace(enroll.KtpTanggalLahir) ? null : DateTime.ParseExact(enroll.KtpTanggalLahir, "dd-MM-yyyy", CultureInfo.InvariantCulture),
                        TempatLahir = enroll.KtpTempatLahir,
                        CreatedByNpp = cekNPPCs.NIK,
                        CreatedByUnitCode = cekNPPCs.Kode_Unit,
                        CreatedByUnitId = int.Parse(cekNPPCs.Unit_Id),
                        IsVerified = false,
                        IsNasabahTemp = string.IsNullOrWhiteSpace(cifData.cif),
                        isEnrollFR = false
                    };

                    if (isEmployee)
                    {
                        dataNpp = new Tbl_Mapping_Pegawai_KTP
                        {
                            CreatedById = cekNPPCs.PegawaiId,
                            NIK = enroll.KtpNIK,
                            Npp = _empData.Npp,
                            CreatedByNpp = cekNPPCs.NIK,
                            CreatedByUID = enroll.UID,
                            CreatedTime = DateTime.Now
                        };
                    }
                }

                var exPhotoKtp = new Tbl_DataKTP_Photo();

                var photoKtpLog = new Tbl_DataKTP_Photo_Log();

                var photoKtp = new Tbl_DataKTP_Photo();

                if (!string.IsNullOrWhiteSpace(enroll.KtpPhotoKTP))
                {
                    var photoKTPData = await _profileRepository.GetPhotoKtp(enroll.KtpNIK)
                        .ConfigureAwait(false);

                    byte[] imgBytes = Convert.FromBase64String(enroll.KtpPhotoKTP);

                    string filePath = "";

                    var systemParameterPath = await _sysParameterRepository.GetPathFolder("FolderPhoto")
                                                .ConfigureAwait(false);
                    string pathFolderFoto = systemParameterPath.Value;

                    string subPathFolderFoto = pathFolder + "/" + pathFolderFoto + "/" + enroll.KtpNIK + "/";

                    string fileName = "Foto_" + enroll.KtpNIK + "_" + JamServer + ".jpg";

                    if (_sftpConfig.IsActive)
                    {
                        using var stream = new MemoryStream(imgBytes);

                        (var fname, var fPath) = await stream.UploadToFTPServer(subPathFolderFoto, fileName, _sftpConfig.Host,
                            _sftpConfig.Username, _sftpConfig.Password, _sftpConfig.RootDirectory, _sftpConfig.Url).ConfigureAwait(false);

                        filePath = fPath;
                    }
                    else
                    {

                        if (!Directory.Exists(subPathFolderFoto))
                        {
                            Directory.CreateDirectory(subPathFolderFoto);
                        }

                        filePath = subPathFolderFoto + fileName;
                        File.WriteAllBytes(filePath, imgBytes);
                    }

                    if (photoKTPData != null)
                    {
                        photoKtpLog.CreatedById = photoKTPData.CreatedById;
                        photoKtpLog.CreatedByNpp = photoKTPData.CreatedByNpp;
                        photoKtpLog.CreatedByUid = photoKTPData.CreatedByUid;
                        photoKtpLog.CreatedByUnit = photoKTPData.CreatedByUnit;
                        photoKtpLog.CreatedTime = photoKTPData.CreatedTime;
                        photoKtpLog.FileName = photoKTPData.FileName;
                        photoKtpLog.Nik = photoKTPData.Nik;
                        photoKtpLog.PathFile = photoKTPData.PathFile;

                        exPhotoKtp = photoKTPData;
                    }

                    photoKtp = new Tbl_DataKTP_Photo
                    {
                        PathFile = filePath,
                        Nik = enroll.KtpNIK,
                        FileName = fileName,
                        IsActive = true,
                        IsDeleted = false,
                        CreatedById = cekNPPCs.PegawaiId,
                        CreatedByNpp = cekNPPCs.NIK,
                        CreatedByUid = enroll.UID,
                        CreatedTime = DateTime.Now
                    };
                }

                var exPhotoSignature = new Tbl_DataKTP_Signature();

                var photoSignatureLog = new Tbl_DataKTP_Signature_Log();

                var photoSignature = new Tbl_DataKTP_Signature();

                if (!string.IsNullOrWhiteSpace(enroll.KtpSignature))
                {
                    var photoSignatureData = await _profileRepository.GetPhotoSignature(enroll.KtpNIK)
                        .ConfigureAwait(false);

                    byte[] imgBytes = Convert.FromBase64String(enroll.KtpSignature);

                    string filePath = "";

                    var systemParameterPath = await _sysParameterRepository.GetPathFolder("FolderSignature")
                                                .ConfigureAwait(false);
                    string pathFolderFoto = systemParameterPath.Value;

                    string subPathFolderSignature = pathFolder + "/" + pathFolderFoto + "/" + enroll.KtpNIK + "/";
                    //string subPathFolderSignature = pathFolder + pathFolderFoto + enroll.KtpNIK;

                    string fileName = "Signature_" + enroll.KtpNIK + "_" + JamServer + ".jpg";

                    if (_sftpConfig.IsActive)
                    {
                        using var stream = new MemoryStream(imgBytes);

                        (var fname, var fPath) = await stream.UploadToFTPServer(subPathFolderSignature, fileName, _sftpConfig.Host,
                            _sftpConfig.Username, _sftpConfig.Password, _sftpConfig.RootDirectory, _sftpConfig.Url).ConfigureAwait(false);

                        filePath = fPath;
                    }
                    else
                    {
                        if (!Directory.Exists(subPathFolderSignature))
                        {
                            Directory.CreateDirectory(subPathFolderSignature);
                        }

                        filePath = subPathFolderSignature + fileName;
                        File.WriteAllBytes(filePath, imgBytes);
                    }

                    if (photoSignatureData != null)
                    {
                        photoSignatureLog.CreatedById = photoSignatureData.CreatedById;
                        photoSignatureLog.CreatedByNpp = photoSignatureData.CreatedByNpp;
                        photoSignatureLog.CreatedByUid = photoSignatureData.CreatedByUid;
                        photoSignatureLog.CreatedByUnit = photoSignatureData.CreatedByUnit;
                        photoSignatureLog.CreatedTime = photoSignatureData.CreatedTime;
                        photoSignatureLog.FileName = photoSignatureData.FileName;
                        photoSignatureLog.Nik = photoSignatureData.Nik;
                        photoSignatureLog.PathFile = photoSignatureData.PathFile;

                        exPhotoSignature = photoSignatureData;
                    }

                    photoSignature = new Tbl_DataKTP_Signature
                    {
                        CreatedById = cekNPPCs.PegawaiId,
                        CreatedByNpp = cekNPPCs.NIK,
                        IsActive = true,
                        IsDeleted = false,
                        Nik = enroll.KtpNIK,
                        FileName = fileName,
                        CreatedByUid = enroll.UID,
                        CreatedTime = DateTime.Now,
                        PathFile = filePath
                    };
                }

                var exPhotoCam = new Tbl_DataKTP_PhotoCam();

                var photoCamLog = new Tbl_DataKTP_PhotoCam_Log();

                var photoCam = new Tbl_DataKTP_PhotoCam();

                if (!string.IsNullOrWhiteSpace(enroll.KtpPhotoCam))
                {
                    var photoCamData = await _profileRepository.GetPhotoCam(enroll.KtpNIK)
                        .ConfigureAwait(false);

                    byte[] imgBytes = Convert.FromBase64String(enroll.KtpPhotoCam);

                    string filePath = "";

                    var systemParameterPath = await _sysParameterRepository.GetPathFolder("FolderWebcam")
                                                .ConfigureAwait(false);
                    string pathFolderFoto = systemParameterPath.Value;

                    string SubPathFolderPhotoCam = pathFolder + "/" + pathFolderFoto + "/" + enroll.KtpNIK + "/";
                    //string SubPathFolderPhotoCam = pathFolder + pathFolderFoto + enroll.KtpNIK;

                    string fileName = "PhotoCam_" + enroll.KtpNIK + "_" + JamServer + ".txt";

                    if (_sftpConfig.IsActive)
                    {
                        using var stream = new MemoryStream(imgBytes);

                        (var fname, var fPath) = await stream.UploadToFTPServer(SubPathFolderPhotoCam, fileName, _sftpConfig.Host,
                            _sftpConfig.Username, _sftpConfig.Password, _sftpConfig.RootDirectory, _sftpConfig.Url).ConfigureAwait(false);

                        filePath = fPath;
                    }
                    else
                    {
                        if (!Directory.Exists(SubPathFolderPhotoCam))
                        {
                            Directory.CreateDirectory(SubPathFolderPhotoCam);
                        }

                        filePath = SubPathFolderPhotoCam + fileName;
                        File.WriteAllBytes(filePath, imgBytes);
                    }

                    if (photoCamData != null)
                    {
                        photoCamLog.CreatedById = photoCamData.CreatedById;
                        photoCamLog.CreatedByNpp = photoCamData.CreatedByNpp;
                        photoCamLog.CreatedByUid = photoCamData.CreatedByUid;
                        photoCamLog.CreatedByUnit = photoCamData.CreatedByUnit;
                        photoCamLog.CreatedTime = photoCamData.CreatedTime;
                        photoCamLog.FileName = photoCamData.FileName;
                        photoCamLog.Nik = photoCamData.Nik;
                        photoCamLog.PathFile = photoCamData.PathFile;

                        exPhotoCam = photoCamData;
                    }

                    photoCam = new Tbl_DataKTP_PhotoCam
                    {
                        PathFile = filePath,//
                        Nik = enroll.KtpNIK,
                        FileName = fileName,//
                        CreatedTime = DateTime.Now,
                        CreatedById = cekNPPCs.PegawaiId,
                        IsActive = true,
                        IsDeleted = false,
                        CreatedByNpp = cekNPPCs.NIK,
                        CreatedByUid = enroll.UID
                    };
                }

                var exPhotoFinger = new List<Tbl_DataKTP_Finger>();

                var exPhotoFingerEmployee = new List<Tbl_DataKTP_Finger_Employee>();

                var photoFingerLogs = new List<Tbl_DataKTP_Finger_Log>();

                var photoFingers = new List<Tbl_DataKTP_Finger>();

                var photoFingersEmployee = new List<Tbl_DataKTP_Finger_Employee>();

                if (!string.IsNullOrWhiteSpace(enroll.KtpFingerKanan))
                {

                    var photoFingerData = await _profileRepository.GetPhotoFinger(enroll.KtpNIK, enroll.KtpTypeJariKanan)
                        .ConfigureAwait(false);

                    var photoFingerDataEmployee = new Tbl_DataKTP_Finger_Employee();

                    if (isEmployee)
                    {
                        photoFingerDataEmployee = await _profileRepository.GetPhotoFingerEmployee(enroll.KtpNIK, enroll.KtpTypeJariKanan)
                        .ConfigureAwait(false);
                    }

                    string imageEncrypted = enroll.KtpFingerKanan.Encrypt(Phrase.FileEncryption);

                    string filePath = "";
                    string filePathIso = "";

                    var systemParameterPath = await _sysParameterRepository.GetPathFolder("FolderFinger")
                                                .ConfigureAwait(false);
                    string pathFolderFoto = systemParameterPath.Value;

                    string subPathFolderPhotoFinger = pathFolder + "/" + pathFolderFoto + "/" + enroll.KtpNIK + "/";

                    string JenisJari = enroll.KtpTypeJariKanan.Replace(" ", "");
                    string fileName = "PhotoFinger_" + JenisJari + "_" + enroll.KtpNIK + "_" + JamServer + ".txt";
                    string fileNameIso = "";

                    if (_sftpConfig.IsActive)
                    {
                        using var stream = new MemoryStream(Encoding.ASCII.GetBytes(imageEncrypted));

                        (var fname, var fPath) = await stream.UploadToFTPServer(subPathFolderPhotoFinger, fileName, _sftpConfig.Host,
                            _sftpConfig.Username, _sftpConfig.Password, _sftpConfig.RootDirectory, _sftpConfig.Url).ConfigureAwait(false);

                        filePath = fPath;
                    }
                    else
                    {
                        if (!Directory.Exists(subPathFolderPhotoFinger))
                        {
                            Directory.CreateDirectory(subPathFolderPhotoFinger);
                        }

                        filePath = subPathFolderPhotoFinger + fileName;
                        File.WriteAllText(filePath, imageEncrypted);
                    }

                    //if (!string.IsNullOrWhiteSpace(enroll.KtpFingerKananIso))
                    //{
                    //    string isoEncrypted = enroll.KtpFingerKananIso.Encrypt(Phrase.FileEncryption);
                    //    fileNameIso = "PhotoFingerISO_" + JenisJari + "_" + enroll.KtpNIK + "_" + JamServer + ".txt";

                    //    if (_sftpConfig.IsActive)
                    //    {
                    //        using var stream = new MemoryStream(Encoding.ASCII.GetBytes(isoEncrypted));

                    //        (var fname, var fPath) = await stream.UploadToFTPServer(subPathFolderPhotoFinger, fileNameIso, _sftpConfig.Host,
                    //            _sftpConfig.Username, _sftpConfig.Password, _sftpConfig.RootDirectory, _sftpConfig.Url).ConfigureAwait(false);

                    //        filePathIso = fPath;
                    //    }
                    //    else
                    //    {
                    //        if (!Directory.Exists(subPathFolderPhotoFinger))
                    //        {
                    //            Directory.CreateDirectory(subPathFolderPhotoFinger);
                    //        }

                    //        filePathIso = subPathFolderPhotoFinger + fileNameIso;
                    //        File.WriteAllText(filePathIso, isoEncrypted);
                    //    }
                    //}

                    if (photoFingerData != null)
                    {
                        photoFingerLogs.Add(new Tbl_DataKTP_Finger_Log
                        {
                            CreatedById = photoFingerData.CreatedById,
                            CreatedByNpp = photoFingerData.CreatedByNpp,
                            CreatedByUid = photoFingerData.CreatedByUid,
                            CreatedByUnit = photoFingerData.CreatedByUnit,
                            CreatedByUnitId = photoFingerData.CreatedByUnitId,
                            CreatedTime = photoFingerData.CreatedTime,
                            //FileJari = photoFingerData.FileJari,
                            //FileJariISO = photoFingerData.FileJariISO,
                            FileName = photoFingerData.FileName,
                            //FileNameISO = photoFingerData.FileNameISO,
                            Nik = photoFingerData.Nik,
                            PathFile = photoFingerData.PathFile,
                            //PathFileISO = photoFingerData.PathFileISO,
                            TypeFinger = photoFingerData.TypeFinger
                        });

                        exPhotoFinger.Add(photoFingerData);
                    }

                    photoFingers.Add(new Tbl_DataKTP_Finger
                    {
                        CreatedById = cekNPPCs.PegawaiId,
                        IsActive = true,
                        IsDeleted = false,
                        Nik = enroll.KtpNIK,
                        PathFile = filePath,//
                        //PathFileISO = filePathIso,//
                        CreatedByNpp = cekNPPCs.NIK,
                        CreatedByUid = enroll.UID,
                        CreatedTime = DateTime.Now,
                        FileName = fileName,//
                        //FileNameISO = fileNameIso,//
                        //FileJariISO = enroll.KtpFingerKananIso,
                        TypeFinger = enroll.KtpTypeJariKanan
                    });

                    if (isEmployee)
                    {
                        photoFingersEmployee.Add(new Tbl_DataKTP_Finger_Employee
                        {
                            CreatedById = cekNPPCs.PegawaiId,
                            IsActive = true,
                            IsDeleted = false,
                            Nik = enroll.KtpNIK,
                            PathFile = filePath,//
                            //PathFileISO = filePathIso,//
                            CreatedByNpp = cekNPPCs.NIK,
                            CreatedByUid = enroll.UID,
                            CreatedTime = DateTime.Now,
                            FileName = fileName,//
                            //FileNameISO = fileNameIso,//
                            //FileJariISO = enroll.KtpFingerKananIso,
                            TypeFinger = enroll.KtpTypeJariKanan
                        });
                    }
                }

                if (!string.IsNullOrWhiteSpace(enroll.KtpFingerKiri))
                {
                    var photoFingerData = await _profileRepository.GetPhotoFinger(enroll.KtpNIK, enroll.KtpTypeJariKiri)
                        .ConfigureAwait(false);

                    var photoFingerDataEmployee = new Tbl_DataKTP_Finger_Employee();

                    if (isEmployee)
                    {
                        photoFingerDataEmployee = await _profileRepository.GetPhotoFingerEmployee(enroll.KtpNIK, enroll.KtpTypeJariKiri)
                        .ConfigureAwait(false);
                    }

                    string imageEncrypted = enroll.KtpFingerKiri.Encrypt(Phrase.FileEncryption);

                    string filePath = "";
                    string filePathIso = "";

                    var systemParameterPath = await _sysParameterRepository.GetPathFolder("FolderFinger")
                                                .ConfigureAwait(false);
                    string pathFolderFoto = systemParameterPath.Value;

                    string subPathFolderPhotoFinger = pathFolder + "/" + pathFolderFoto + "/" + enroll.KtpNIK + "/";

                    string JenisJari = enroll.KtpTypeJariKiri.Replace(" ", "");
                    string fileName = "PhotoFinger_" + JenisJari + "_" + enroll.KtpNIK + "_" + JamServer + ".txt";
                    string fileNameIso = "";

                    if (_sftpConfig.IsActive)
                    {
                        using var stream = new MemoryStream(Encoding.ASCII.GetBytes(imageEncrypted));

                        (var fname, var fPath) = await stream.UploadToFTPServer(subPathFolderPhotoFinger, fileName, _sftpConfig.Host,
                            _sftpConfig.Username, _sftpConfig.Password, _sftpConfig.RootDirectory, _sftpConfig.Url).ConfigureAwait(false);

                        filePath = fPath;
                    }
                    else
                    {
                        if (!Directory.Exists(subPathFolderPhotoFinger))
                        {
                            Directory.CreateDirectory(subPathFolderPhotoFinger);
                        }

                        filePath = subPathFolderPhotoFinger + fileName;
                        File.WriteAllText(filePath, imageEncrypted);
                    }

                    //if (!string.IsNullOrWhiteSpace(enroll.KtpFingerKiriIso))
                    //{
                    //    string isoEncrypted = enroll.KtpFingerKiriIso.Encrypt(Phrase.FileEncryption);
                    //    fileNameIso = "PhotoFingerISO_" + JenisJari + "_" + enroll.KtpNIK + "_" + JamServer + ".txt";

                    //    if (_sftpConfig.IsActive)
                    //    {
                    //        using var stream = new MemoryStream(Encoding.ASCII.GetBytes(isoEncrypted));

                    //        (var fname, var fPath) = await stream.UploadToFTPServer(subPathFolderPhotoFinger, fileNameIso, _sftpConfig.Host,
                    //            _sftpConfig.Username, _sftpConfig.Password, _sftpConfig.RootDirectory, _sftpConfig.Url).ConfigureAwait(false);

                    //        filePathIso = fPath;
                    //    }
                    //    else
                    //    {
                    //        if (!Directory.Exists(subPathFolderPhotoFinger))
                    //        {
                    //            Directory.CreateDirectory(subPathFolderPhotoFinger);
                    //        }

                    //        filePathIso = subPathFolderPhotoFinger + fileNameIso;
                    //        File.WriteAllText(filePathIso, isoEncrypted);
                    //    }
                    //}

                    if (photoFingerData != null)
                    {
                        photoFingerLogs.Add(new Tbl_DataKTP_Finger_Log
                        {
                            CreatedById = photoFingerData.CreatedById,
                            CreatedByNpp = photoFingerData.CreatedByNpp,
                            CreatedByUid = photoFingerData.CreatedByUid,
                            CreatedByUnit = photoFingerData.CreatedByUnit,
                            CreatedByUnitId = photoFingerData.CreatedByUnitId,
                            CreatedTime = photoFingerData.CreatedTime,
                            //FileJari = photoFingerData.FileJari,
                            //FileJariISO = photoFingerData.FileJariISO,
                            FileName = photoFingerData.FileName,
                            //FileNameISO = photoFingerData.FileNameISO,
                            Nik = photoFingerData.Nik,
                            PathFile = photoFingerData.PathFile,
                            //PathFileISO = photoFingerData.PathFileISO,
                            TypeFinger = photoFingerData.TypeFinger
                        });

                        exPhotoFinger.Add(photoFingerData);
                    }

                    photoFingers.Add(new Tbl_DataKTP_Finger
                    {
                        CreatedById = cekNPPCs.PegawaiId,
                        IsActive = true,
                        IsDeleted = false,
                        Nik = enroll.KtpNIK,
                        PathFile = filePath,//
                        //PathFileISO = filePathIso,//
                        CreatedByNpp = cekNPPCs.NIK,
                        CreatedByUid = enroll.UID,
                        CreatedTime = DateTime.Now,
                        FileName = fileName,//
                        //FileNameISO = fileNameIso,//
                        //FileJariISO= enroll.KtpFingerKiriIso,//
                        TypeFinger = enroll.KtpTypeJariKiri
                    });

                    if (isEmployee)
                    {
                        photoFingersEmployee.Add(new Tbl_DataKTP_Finger_Employee
                        {
                            CreatedById = cekNPPCs.PegawaiId,
                            IsActive = true,
                            IsDeleted = false,
                            Nik = enroll.KtpNIK,
                            PathFile = filePath,//
                            //PathFileISO = filePathIso,//
                            CreatedByNpp = cekNPPCs.NIK,
                            CreatedByUid = enroll.UID,
                            CreatedTime = DateTime.Now,
                            FileName = fileName,//
                            //FileNameISO = fileNameIso,//
                            //FileJariISO = enroll.KtpFingerKiriIso,//
                            TypeFinger = enroll.KtpTypeJariKiri
                        });
                    }
                }

                // to do: alat reader log
                var dataReader = await _dataReaderRepository.GetDatareaderUid(enroll.UID).ConfigureAwait(false);

                var dataReaderLog = new Tbl_MasterAlatReaderLog();

                if (dataReader != null)
                {
                    dataReaderLog = new Tbl_MasterAlatReaderLog
                    {
                        CreatedBy_Id = cekNPPCs.PegawaiId,
                        CreatedTime = DateTime.Now,
                        IsActive = true,
                        IsDeleted = false,
                        Nik = enroll.KtpNIK,
                        PegawaiId = cekNPPCs.PegawaiId,
                        Serial_Number = dataReader.SN_Unit,
                        Type = "Enroll",
                        Uid = enroll.UID
                    };
                }

                dataDemografis.isEnrollFR = false;

                _enrollmentKTPRepository.InsertEnrollFlow(dataDemografis, dataDemografisLog, photoKtp, photoKtpLog, photoSignature, photoSignatureLog,
                    photoCam, photoCamLog, photoFingers, photoFingerLogs, exPhotoKtp, exPhotoSignature, exPhotoCam, exPhotoFinger, dataNpp, dataReaderLog, photoFingersEmployee, exPhotoFingerEmployee);

                await _alatReaderRepository.CreateLogActivity2(new Tbl_MasterAlatReaderLogActvity
                {
                    CreatedBy_Id = cekNPPCs.PegawaiId,
                    IsActive = true,
                    IsDeleted = false,
                    KodeUnit = cekNPPCs.Kode_Unit,
                    LastIP = enroll.IpAddress,
                    NIK = enroll.KtpNIK,
                    CreatedTime = DateTime.Now,
                    NppPegawai = cekNPPCs.NIK,
                    PegawaiId = cekNPPCs.PegawaiId,
                    Type = "Enroll",
                    UID = enroll.UID,
                    UnitId = int.Parse(cekNPPCs.Unit_Id)
                });

                await _alatReaderRepository.InsertLogEnrollThirdParty(new Tbl_Enrollment_ThirdParty_Log
                {
                    NIK = enroll.KtpNIK,
                    AppsChannel = AppsChannel,
                    SubmitDate = DateTime.Now

                });
            }

            if (IsNasabah)
            {
                return (_SuccessMessageConfig.NasabahSuksesEnrollNasabah, (int)EnrollStatus.Nasabah_Berhasil_di_enroll, cekKTP?.CIF);
            }
            else
            {
                return (_SuccessMessageConfig.NasabahSuksesEnrollNonNasabah, (int)EnrollStatus.Nasabah_Berhasil_di_enroll, cekKTP?.CIF);
            }
        }


        #region ENrool & Re-enroll ISO
        public async Task<(string msg, int code, string cif)> SubmitEnrollmentFingerEncryptedOnlyISO(bool isHitSOA, ApiSOA ReqSoa, EnrollKTP enroll, int Id, string npp,
            string unitCode, int unitId)
        {
            bool isEmployee = false;
            

            var _empData = await _enrollmentKTPRepository.IsEmployee(enroll.KtpNIK);
            if (_empData != null)
            {
                isEmployee = true;
            }

            if (enroll.KtpTanggalLahir != null)
            {
                enroll.KtpTanggalLahir = enroll.KtpTanggalLahir.Replace(".", "");
                if (enroll.KtpTanggalLahir.Contains("/"))
                {
                    enroll.KtpTanggalLahir = enroll.KtpTanggalLahir.Replace("/", "-");
                }
                string[] arrayTanggl = enroll.KtpTanggalLahir.Split("-");
                string hari = "";
                string bulan = "";
                string tahun = "";

                if (arrayTanggl != null)
                {
                    if (arrayTanggl[0] != null)
                    {
                        if (arrayTanggl[0].Length != 2)
                        {
                            hari = "0" + arrayTanggl[0];

                        }
                        else
                        {
                            hari = arrayTanggl[0];

                        }
                    }

                    if (arrayTanggl[1] != null)
                    {
                        if (arrayTanggl[1].Length != 2)
                        {
                            bulan = "0" + arrayTanggl[1];

                        }
                        else
                        {
                            bulan = arrayTanggl[1];

                        }
                    }

                    if (arrayTanggl[2] != null)
                    {
                        tahun = arrayTanggl[2];
                    }
                }

                enroll.KtpTanggalLahir = hari + "-" + bulan + "-" + tahun;
            }

            const bool IsNewEnroll = true;
            string JamServer = DateTime.Now.ToString("ddMMyyyyHHmmss");
            bool IsNasabah = false;

            var cekKTP = await _profileRepository.GetDataDemografis(enroll.KtpNIK)
                .ConfigureAwait(false);

            var cekKTPTemp = await _profileRepository.GetDataDemografisTempOnProgress(enroll.KtpNIK)
                .ConfigureAwait(false); ;

            

            if (cekKTP != null)
            {
                var logDemografi = new Tbl_DataKTP_Demografis_Log
                {
                    Agama = cekKTP.Agama,
                    Alamat = cekKTP.Alamat,
                    AlamatGoogle = cekKTP.AlamatGoogle,
                    AlamatLengkap = cekKTP.AlamatLengkap,
                    CreatedById = cekKTP.CreatedById,
                    CreatedByUID = cekKTP.CreatedByUID,
                    CreatedByUnitId = cekKTP.CreatedByUnitId,
                    CreatedTime = cekKTP.CreatedTime,
                    Desa = cekKTP.Desa,
                    GolonganDarah = cekKTP.GolonganDarah,
                    JenisKelamin = cekKTP.JenisKelamin,
                    Kecamatan = cekKTP.Kecamatan,
                    Kelurahan = cekKTP.Kelurahan,
                    Kewarganegaraan = cekKTP.Kewarganegaraan,
                    KodePos = cekKTP.KodePos,
                    Kota = cekKTP.Kota,
                    Latitude = cekKTP.Latitude,
                    Longitude = cekKTP.Longitude,
                    MasaBerlaku = cekKTP.MasaBerlaku,
                    Nama = cekKTP.Nama,
                    NIK = cekKTP.NIK,
                    Pekerjaan = cekKTP.Pekerjaan,
                    Provinsi = cekKTP.Provinsi,
                    RT = cekKTP.RT,
                    RW = cekKTP.RW,
                    StatusPerkawinan = cekKTP.StatusPerkawinan,
                    TanggalLahir = cekKTP.TanggalLahir,
                    TempatLahir = cekKTP.TempatLahir,
                    CIF = cekKTP.CIF,
                    CreatedByNpp = cekKTP.CreatedByNpp
                };

                var stringPerubahan = "";

                if (cekKTP.Nama != enroll.KtpNama)
                {
                    stringPerubahan = stringPerubahan + "NAMA : " + cekKTP.Nama + " -> " + enroll.KtpNama + " <br/>";
                    cekKTP.Nama = enroll.KtpNama;
                }

                if (cekKTP.TempatLahir != enroll.KtpTempatLahir)
                {
                    stringPerubahan = stringPerubahan + "TEMPAT LAHIR : " + cekKTP.TempatLahir + " -> " + enroll.KtpTempatLahir + " <br/>";
                    cekKTP.TempatLahir = enroll.KtpTempatLahir;
                }

                if (enroll.KtpTanggalLahir != null)
                {
                    var _ttl = DateTime.ParseExact(enroll.KtpTanggalLahir, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                    if (cekKTP.TanggalLahir != _ttl)
                    {
                        stringPerubahan = stringPerubahan + "TANGGAL LAHIR : " + cekKTP.TanggalLahir + " -> " + enroll.KtpTanggalLahir + " <br/>";
                        cekKTP.TanggalLahir = DateTime.ParseExact(enroll.KtpTanggalLahir, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                    }
                }

                if (cekKTP.JenisKelamin != enroll.KtpJanisKelamin)
                {
                    stringPerubahan = stringPerubahan + "JENIS KELAMIN : " + cekKTP.JenisKelamin + " -> " + enroll.KtpJanisKelamin + " <br/>";
                    cekKTP.JenisKelamin = enroll.KtpJanisKelamin;
                }

                if (cekKTP.GolonganDarah != enroll.KtpGolonganDarah)
                {
                    stringPerubahan = stringPerubahan + "GOLONGAN DARAH : " + cekKTP.GolonganDarah + " -> " + enroll.KtpGolonganDarah + " <br/>";
                    cekKTP.GolonganDarah = enroll.KtpGolonganDarah;
                }

                if (cekKTP.Alamat != enroll.KtpAlamat)
                {
                    stringPerubahan = stringPerubahan + "ALAMAT : " + cekKTP.Alamat + " -> " + enroll.KtpAlamat + " <br/>";
                    cekKTP.Alamat = enroll.KtpAlamat;
                }

                if (cekKTP.RT != enroll.KtpRT)
                {
                    stringPerubahan = stringPerubahan + "RT : " + cekKTP.RT + " -> " + enroll.KtpRT + " <br/>";
                    cekKTP.RT = enroll.KtpRT;
                }

                if (cekKTP.RW != enroll.KtpRW)
                {
                    stringPerubahan = stringPerubahan + "RW : " + cekKTP.RW + " -> " + enroll.KtpRW + " <br/>";
                    cekKTP.RW = enroll.KtpRW;
                }

                if (cekKTP.Kelurahan != enroll.KtpKelurahan)
                {
                    stringPerubahan = stringPerubahan + "KELURAHAN : " + cekKTP.Kelurahan + " -> " + enroll.KtpKelurahan + " <br/>";
                    cekKTP.Kelurahan = enroll.KtpKelurahan;
                }

                if (cekKTP.Kecamatan != enroll.KtpKecamatan)
                {
                    stringPerubahan = stringPerubahan + "KECAMATAN : " + cekKTP.Kecamatan + " -> " + enroll.KtpKecamatan + " <br/>";
                    cekKTP.Kecamatan = enroll.KtpKecamatan;
                }

                if (cekKTP.Kota != enroll.KtpKota)
                {
                    stringPerubahan = stringPerubahan + "KOTA/KABUPATEN : " + cekKTP.Kota + " -> " + enroll.KtpKota + " <br/>";
                    cekKTP.Kota = enroll.KtpKota;
                }

                if (cekKTP.Provinsi != enroll.KtpProvinsi)
                {
                    stringPerubahan = stringPerubahan + "PROVINSI  : " + cekKTP.Provinsi + " -> " + enroll.KtpProvinsi + " <br/>";
                    cekKTP.Provinsi = enroll.KtpProvinsi;
                }

                if (cekKTP.Agama != enroll.KtpAgama)
                {
                    stringPerubahan = stringPerubahan + "AGAMA  : " + cekKTP.Agama + " -> " + enroll.KtpAgama + " <br/>";
                    cekKTP.Agama = enroll.KtpAgama;
                }

                if (cekKTP.StatusPerkawinan != enroll.KtpStatusPerkawinan)
                {
                    stringPerubahan = stringPerubahan + "STATUS PERKAWINAN  : " + cekKTP.Agama + " -> " + enroll.KtpAgama + " <br/>";
                    cekKTP.StatusPerkawinan = enroll.KtpStatusPerkawinan;
                }

                if (cekKTP.Pekerjaan != enroll.KtpPekerjaan)
                {
                    stringPerubahan = stringPerubahan + "PEKERJAAN  : " + cekKTP.Agama + " -> " + enroll.KtpAgama + " <br/>";
                    cekKTP.Pekerjaan = enroll.KtpPekerjaan;
                }

                if (cekKTP.Kewarganegaraan != enroll.KtpKewarganegaraan)
                {
                    stringPerubahan = stringPerubahan + "KEWARGANEGARAAN  : " + cekKTP.Kewarganegaraan + " -> " + enroll.KtpKewarganegaraan + " <br/>";
                    cekKTP.Kewarganegaraan = enroll.KtpKewarganegaraan;
                }
                if (cekKTP.MasaBerlaku != enroll.KtpMasaBerlaku)
                {
                    stringPerubahan = stringPerubahan + "MASA BERLAKU  : " + cekKTP.MasaBerlaku + " -> " + enroll.KtpMasaBerlaku + " <br/>";
                    cekKTP.MasaBerlaku = enroll.KtpMasaBerlaku;
                }

                cekKTP.UpdatedTime = DateTime.Now;
                cekKTP.UpdatedById = Id;
                cekKTP.UpdatedByUID = enroll.UID;
                cekKTP.UpdatedByUnitCode = unitCode;
                cekKTP.UpdatedByUnitId = unitId;
                cekKTP.isEnrollFR = false;

                await _profileRepository.UpdateDataDemografis(cekKTP)
                    .ConfigureAwait(false);

                if (string.IsNullOrWhiteSpace(stringPerubahan))
                {
                    return (_ErrorMessageConfig.NasabahPernahEnroll, (int)EnrollStatus.Sudah_di_enroll_sebelumnya, "");
                }

                _profileRepository.InsertDemografiLog(logDemografi);

                return (stringPerubahan, (int)EnrollStatus.Berhasil_mengubah_beberapa_perubahan, "");
            }

            if (cekKTPTemp != null)
            {
                var logDemografi = new Tbl_DataKTP_Demografis_Log
                {
                    Agama = cekKTPTemp.Agama,
                    Alamat = cekKTPTemp.Alamat,
                    AlamatGoogle = cekKTPTemp.AlamatGoogle,
                    AlamatLengkap = cekKTPTemp.AlamatLengkap,
                    CreatedById = cekKTPTemp.CreatedById,
                    CreatedByUID = cekKTPTemp.CreatedByUID,
                    CreatedByUnitId = cekKTPTemp.CreatedByUnitId,
                    CreatedTime = cekKTPTemp.CreatedTime,
                    Desa = cekKTPTemp.Desa,
                    GolonganDarah = cekKTPTemp.GolonganDarah,
                    JenisKelamin = cekKTPTemp.JenisKelamin,
                    Kecamatan = cekKTPTemp.Kecamatan,
                    Kelurahan = cekKTPTemp.Kelurahan,
                    Kewarganegaraan = cekKTPTemp.Kewarganegaraan,
                    KodePos = cekKTPTemp.KodePos,
                    Kota = cekKTPTemp.Kota,
                    Latitude = cekKTPTemp.Latitude,
                    Longitude = cekKTPTemp.Longitude,
                    MasaBerlaku = cekKTPTemp.MasaBerlaku,
                    Nama = cekKTPTemp.Nama,
                    NIK = cekKTPTemp.NIK,
                    Pekerjaan = cekKTPTemp.Pekerjaan,
                    Provinsi = cekKTPTemp.Provinsi,
                    RT = cekKTPTemp.RT,
                    RW = cekKTPTemp.RW,
                    StatusPerkawinan = cekKTPTemp.StatusPerkawinan,
                    TanggalLahir = cekKTPTemp.TanggalLahir,
                    TempatLahir = cekKTPTemp.TempatLahir,
                    CIF = cekKTPTemp.CIF,
                    CreatedByNpp = cekKTPTemp.CreatedByNpp
                };

                var dataKTPTemp = new Tbl_DataKTP_Demografis
                {
                    Agama = cekKTPTemp.Agama,
                    Alamat = cekKTPTemp.Alamat,
                    AlamatGoogle = cekKTPTemp.AlamatGoogle,
                    AlamatLengkap = cekKTPTemp.AlamatLengkap,
                    CreatedById = cekKTPTemp.CreatedById,
                    CreatedByUID = cekKTPTemp.CreatedByUID,
                    CreatedByUnitId = cekKTPTemp.CreatedByUnitId,
                    CreatedTime = cekKTPTemp.CreatedTime,
                    Desa = cekKTPTemp.Desa,
                    GolonganDarah = cekKTPTemp.GolonganDarah,
                    JenisKelamin = cekKTPTemp.JenisKelamin,
                    Kecamatan = cekKTPTemp.Kecamatan,
                    Kelurahan = cekKTPTemp.Kelurahan,
                    Kewarganegaraan = cekKTPTemp.Kewarganegaraan,
                    KodePos = cekKTPTemp.KodePos,
                    Kota = cekKTPTemp.Kota,
                    Latitude = cekKTPTemp.Latitude,
                    Longitude = cekKTPTemp.Longitude,
                    MasaBerlaku = cekKTPTemp.MasaBerlaku,
                    Nama = cekKTPTemp.Nama,
                    NIK = cekKTPTemp.NIK,
                    Pekerjaan = cekKTPTemp.Pekerjaan,
                    Provinsi = cekKTPTemp.Provinsi,
                    RT = cekKTPTemp.RT,
                    RW = cekKTPTemp.RW,
                    StatusPerkawinan = cekKTPTemp.StatusPerkawinan,
                    TanggalLahir = cekKTPTemp.TanggalLahir,
                    TempatLahir = cekKTPTemp.TempatLahir,
                    CIF = cekKTPTemp.CIF,
                    CreatedByNpp = cekKTPTemp.CreatedByNpp,
                    isEnrollFR = true
                };

                var stringPerubahan = "";

                if (cekKTPTemp.Nama != enroll.KtpNama)
                {
                    stringPerubahan = stringPerubahan + "NAMA : " + cekKTPTemp.Nama + " -> " + enroll.KtpNama + " <br/>";
                    cekKTPTemp.Nama = enroll.KtpNama;
                }

                if (cekKTPTemp.TempatLahir != enroll.KtpTempatLahir)
                {
                    stringPerubahan = stringPerubahan + "TEMPAT LAHIR : " + cekKTPTemp.TempatLahir + " -> " + enroll.KtpTempatLahir + " <br/>";
                    cekKTPTemp.TempatLahir = enroll.KtpTempatLahir;
                }

                if (enroll.KtpTanggalLahir != null)
                {
                    var _ttl = DateTime.ParseExact(enroll.KtpTanggalLahir, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                    if (cekKTPTemp.TanggalLahir != _ttl)
                    {
                        stringPerubahan = stringPerubahan + "TANGGAL LAHIR : " + cekKTPTemp.TanggalLahir + " -> " + enroll.KtpTanggalLahir + " <br/>";
                        cekKTPTemp.TanggalLahir = DateTime.ParseExact(enroll.KtpTanggalLahir, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                    }
                }

                if (cekKTPTemp.JenisKelamin != enroll.KtpJanisKelamin)
                {
                    stringPerubahan = stringPerubahan + "JENIS KELAMIN : " + cekKTPTemp.JenisKelamin + " -> " + enroll.KtpJanisKelamin + " <br/>";
                    cekKTPTemp.JenisKelamin = enroll.KtpJanisKelamin;
                }

                if (cekKTPTemp.GolonganDarah != enroll.KtpGolonganDarah)
                {
                    stringPerubahan = stringPerubahan + "GOLONGAN DARAH : " + cekKTPTemp.GolonganDarah + " -> " + enroll.KtpGolonganDarah + " <br/>";
                    cekKTPTemp.GolonganDarah = enroll.KtpGolonganDarah;
                }

                if (cekKTPTemp.Alamat != enroll.KtpAlamat)
                {
                    stringPerubahan = stringPerubahan + "ALAMAT : " + cekKTPTemp.Alamat + " -> " + enroll.KtpAlamat + " <br/>";
                    cekKTPTemp.Alamat = enroll.KtpAlamat;
                }

                if (cekKTPTemp.RT != enroll.KtpRT)
                {
                    stringPerubahan = stringPerubahan + "RT : " + cekKTPTemp.RT + " -> " + enroll.KtpRT + " <br/>";
                    cekKTPTemp.RT = enroll.KtpRT;
                }

                if (cekKTPTemp.RW != enroll.KtpRW)
                {
                    stringPerubahan = stringPerubahan + "RW : " + cekKTPTemp.RW + " -> " + enroll.KtpRW + " <br/>";
                    cekKTPTemp.RW = enroll.KtpRW;
                }

                if (cekKTPTemp.Kelurahan != enroll.KtpKelurahan)
                {
                    stringPerubahan = stringPerubahan + "KELURAHAN : " + cekKTPTemp.Kelurahan + " -> " + enroll.KtpKelurahan + " <br/>";
                    cekKTPTemp.Kelurahan = enroll.KtpKelurahan;
                }

                if (cekKTPTemp.Kecamatan != enroll.KtpKecamatan)
                {
                    stringPerubahan = stringPerubahan + "KECAMATAN : " + cekKTPTemp.Kecamatan + " -> " + enroll.KtpKecamatan + " <br/>";
                    cekKTPTemp.Kecamatan = enroll.KtpKecamatan;
                }

                if (cekKTPTemp.Kota != enroll.KtpKota)
                {
                    stringPerubahan = stringPerubahan + "KOTA/KABUPATEN : " + cekKTPTemp.Kota + " -> " + enroll.KtpKota + " <br/>";
                    cekKTPTemp.Kota = enroll.KtpKota;
                }

                if (cekKTPTemp.Provinsi != enroll.KtpProvinsi)
                {
                    stringPerubahan = stringPerubahan + "PROVINSI  : " + cekKTPTemp.Provinsi + " -> " + enroll.KtpProvinsi + " <br/>";
                    cekKTPTemp.Provinsi = enroll.KtpProvinsi;
                }

                if (cekKTPTemp.Agama != enroll.KtpAgama)
                {
                    stringPerubahan = stringPerubahan + "AGAMA  : " + cekKTPTemp.Agama + " -> " + enroll.KtpAgama + " <br/>";
                    cekKTPTemp.Agama = enroll.KtpAgama;
                }

                if (cekKTPTemp.StatusPerkawinan != enroll.KtpStatusPerkawinan)
                {
                    stringPerubahan = stringPerubahan + "STATUS PERKAWINAN  : " + cekKTPTemp.Agama + " -> " + enroll.KtpAgama + " <br/>";
                    cekKTPTemp.StatusPerkawinan = enroll.KtpStatusPerkawinan;
                }

                if (cekKTPTemp.Pekerjaan != enroll.KtpPekerjaan)
                {
                    stringPerubahan = stringPerubahan + "PEKERJAAN  : " + cekKTPTemp.Agama + " -> " + enroll.KtpAgama + " <br/>";
                    cekKTPTemp.Pekerjaan = enroll.KtpPekerjaan;
                }

                if (cekKTPTemp.Kewarganegaraan != enroll.KtpKewarganegaraan)
                {
                    stringPerubahan = stringPerubahan + "KEWARGANEGARAAN  : " + cekKTPTemp.Kewarganegaraan + " -> " + enroll.KtpKewarganegaraan + " <br/>";
                    cekKTPTemp.Kewarganegaraan = enroll.KtpKewarganegaraan;
                }
                if (cekKTPTemp.MasaBerlaku != enroll.KtpMasaBerlaku)
                {
                    stringPerubahan = stringPerubahan + "MASA BERLAKU  : " + cekKTPTemp.MasaBerlaku + " -> " + enroll.KtpMasaBerlaku + " <br/>";
                    cekKTPTemp.MasaBerlaku = enroll.KtpMasaBerlaku;
                }

                cekKTPTemp.UpdatedTime = DateTime.Now;
                cekKTPTemp.UpdatedById = Id;
                cekKTPTemp.UpdatedByUID = enroll.UID;
                cekKTPTemp.UpdatedByUnitCode = unitCode;
                cekKTPTemp.UpdatedByUnitId = unitId;
                //cekKTPTemp.isEnrollFR = false;

                await _profileRepository.UpdateDataDemografis(dataKTPTemp)
                    .ConfigureAwait(false);

                if (string.IsNullOrWhiteSpace(stringPerubahan))
                {
                    return (_ErrorMessageConfig.NasabahPernahEnroll, (int)EnrollStatus.Sudah_di_enroll_sebelumnya, "");
                }

                _profileRepository.InsertDemografiLog(logDemografi);

                return (stringPerubahan, (int)EnrollStatus.Berhasil_mengubah_beberapa_perubahan, "");
            }

            if (IsNewEnroll)
            {
                if (enroll?.KtpNIK == null) return (_ErrorMessageConfig.InputTidakLengkap, (int)EnrollStatus.Inputan_tidak_lengkap, "");

                var cifData = new ApiSOAResponse();
                #region Hit SOA And Loggging it
                if (isHitSOA == true)
                {
                    cifData = await _cifService.GetSOAByCif(ReqSoa)
                    .ConfigureAwait(false);

                    var status = 0;
                    if (cifData.cif != null)
                    {
                        status = 1;
                    }

                    var _log = new Tbl_ThirdPartyLog
                    {
                        FeatureName = "SubmitEnrollmentFingerEncryptedOnlyISO",
                        HostUrl = ReqSoa.host,
                        Request = Newtonsoft.Json.JsonConvert.SerializeObject(ReqSoa),
                        Status = status,
                        Response = Newtonsoft.Json.JsonConvert.SerializeObject(cifData),
                        CreatedDate = System.DateTime.Now,
                        CreatedBy = npp
                    };

                    _ = _enrollmentKTPRepository.CreateThirdPartyLog(_log);
                }
                else
                {
                    try
                    {
                        var res = await _cifService.GetCIF(new NikDto { Nik = enroll.KtpNIK },
                        new NikDtoUrl { baseUrl = ReqSoa.baseUrlNonSoa, endpoint = ReqSoa.UrlEndPointNonSoa });
                        if (res.Data == null)
                        {
                            cifData.cif = null;
                        }
                        else
                        {
                            cifData.cif = res.Data.Cif;
                        };
                    }
                    catch (Exception ex)
                    {
                        cifData.cif = null;
                    }

                }
                #endregion

                if (!String.IsNullOrEmpty(cifData.cif))
                {
                    IsNasabah = true;
                    //for testing purpose only 
                    //cifData.cif = "        1003863441-8";
                    cifData.cif = cifData.cif.Trim();
                }

                var sysPathFolder = await _sysParameterRepository.GetPathFolder("PathFolderData");
                string pathFolder = sysPathFolder.Value;

                var dataDemografis = new Tbl_DataKTP_Demografis();

                var dataDemografisLog = new Tbl_DataKTP_Demografis_Log();

                var dataNpp = new Tbl_Mapping_Pegawai_KTP();

                if (cekKTP != null)
                {
                    dataDemografisLog = new Tbl_DataKTP_Demografis_Log
                    {
                        Agama = cekKTP.Agama,
                        Alamat = cekKTP.Alamat,
                        AlamatGoogle = cekKTP.AlamatGoogle,
                        AlamatLengkap = cekKTP.AlamatLengkap,
                        CreatedById = cekKTP.CreatedById,
                        CreatedByUID = cekKTP.CreatedByUID,
                        CreatedByUnitId = cekKTP.CreatedByUnitId,
                        CreatedTime = cekKTP.CreatedTime,
                        Desa = cekKTP.Desa,
                        GolonganDarah = cekKTP.GolonganDarah,
                        JenisKelamin = cekKTP.JenisKelamin,
                        Kecamatan = cekKTP.Kecamatan,
                        Kelurahan = cekKTP.Kelurahan,
                        Kewarganegaraan = cekKTP.Kewarganegaraan,
                        KodePos = cekKTP.KodePos,
                        Kota = cekKTP.Kota,
                        Latitude = cekKTP.Latitude,
                        Longitude = cekKTP.Longitude,
                        MasaBerlaku = cekKTP.MasaBerlaku,
                        Nama = cekKTP.Nama,
                        NIK = cekKTP.NIK,
                        Pekerjaan = cekKTP.Pekerjaan,
                        Provinsi = cekKTP.Provinsi,
                        RT = cekKTP.RT,
                        RW = cekKTP.RW,
                        StatusPerkawinan = cekKTP.StatusPerkawinan,
                        TanggalLahir = cekKTP.TanggalLahir,
                        TempatLahir = cekKTP.TempatLahir
                    };

                    cekKTP.Agama = enroll.KtpAgama;
                    cekKTP.Alamat = enroll.KtpAlamat;
                    cekKTP.AlamatGoogle = enroll.KtpAlamatConvertLatlong;
                    cekKTP.AlamatLengkap = enroll.KtpAlamatConvertLengkap;
                    if (string.IsNullOrWhiteSpace(cifData.cif))
                    {
                        cekKTP.CIF = cifData.cif;
                    }
                    cekKTP.GolonganDarah = enroll.KtpGolonganDarah;
                    cekKTP.JenisKelamin = enroll.KtpJanisKelamin;
                    cekKTP.Kecamatan = enroll.KtpKecamatan;
                    cekKTP.Kelurahan = enroll.KtpKelurahan;
                    cekKTP.Kewarganegaraan = enroll.KtpKewarganegaraan;
                    cekKTP.KodePos = enroll.KtpKodePos;
                    cekKTP.Kota = enroll.KtpKota;
                    cekKTP.Latitude = enroll.KtpLatitude;
                    cekKTP.Longitude = enroll.KtpLongitude;
                    cekKTP.MasaBerlaku = enroll.KtpMasaBerlaku;
                    cekKTP.Nama = enroll.KtpNama;
                    cekKTP.NIK = enroll.KtpNIK;
                    cekKTP.Pekerjaan = enroll.KtpPekerjaan;
                    cekKTP.Provinsi = enroll.KtpProvinsi;
                    cekKTP.RT = enroll.KtpRT;
                    cekKTP.RW = enroll.KtpRW;
                    cekKTP.StatusPerkawinan = enroll.KtpStatusPerkawinan;
                    cekKTP.TanggalLahir = string.IsNullOrWhiteSpace(enroll.KtpTanggalLahir) ? null : DateTime.ParseExact(enroll.KtpTanggalLahir, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                    cekKTP.TempatLahir = enroll.KtpTempatLahir;
                    cekKTP.UpdatedById = Id;
                    cekKTP.UpdatedByNpp = npp;
                    cekKTP.UpdatedByUID = enroll.UID;
                    cekKTP.UpdatedTime = DateTime.Now;
                    cekKTP.UpdatedByUnitCode = unitCode;
                    cekKTP.UpdatedByUnitId = unitId;
                    cekKTP.isEnrollFR = false;
                }
                else
                {
                    dataDemografis = new Tbl_DataKTP_Demografis
                    {
                        Agama = enroll.KtpAgama,
                        Alamat = enroll.KtpAlamat,
                        AlamatGoogle = enroll.KtpAlamatConvertLatlong,
                        AlamatLengkap = enroll.KtpAlamatConvertLengkap,
                        CIF = (cifData.cif),
                        NIK = enroll.KtpNIK,
                        CreatedById = Id,
                        IsActive = true,
                        IsDeleted = false,
                        JenisKelamin = enroll.KtpJanisKelamin,
                        Kecamatan = enroll.KtpKecamatan,
                        Kelurahan = enroll.KtpKelurahan,
                        CreatedByUID = enroll.UID,
                        CreatedTime = DateTime.Now,
                        GolonganDarah = enroll.KtpGolonganDarah,
                        Kewarganegaraan = enroll.KtpKewarganegaraan,
                        KodePos = enroll.KtpKodePos,
                        Kota = enroll.KtpKota,
                        Latitude = enroll.KtpLatitude,
                        Longitude = enroll.KtpLongitude,
                        MasaBerlaku = enroll.KtpMasaBerlaku,
                        Nama = enroll.KtpNama,
                        Pekerjaan = enroll.KtpPekerjaan,
                        Provinsi = enroll.KtpProvinsi,
                        RT = enroll.KtpRT,
                        RW = enroll.KtpRW,
                        StatusPerkawinan = enroll.KtpStatusPerkawinan,
                        TanggalLahir = string.IsNullOrWhiteSpace(enroll.KtpTanggalLahir) ? null : DateTime.ParseExact(enroll.KtpTanggalLahir, "dd-MM-yyyy", CultureInfo.InvariantCulture),
                        TempatLahir = enroll.KtpTempatLahir,
                        CreatedByNpp = npp,
                        CreatedByUnitCode = unitCode,
                        CreatedByUnitId = unitId,
                        IsVerified = false,
                        IsNasabahTemp = string.IsNullOrWhiteSpace(cifData.cif),
                        isEnrollFR = false
                    };

                    if (isEmployee)
                    {
                        dataNpp = new Tbl_Mapping_Pegawai_KTP
                        {
                            CreatedById = Id,
                            NIK = enroll.KtpNIK,
                            Npp = _empData.Npp,
                            CreatedByNpp = npp,
                            CreatedByUID = enroll.UID,
                            CreatedTime = DateTime.Now,
                            CreatedByUnit = unitCode
                    };
                    }
                }

                var exPhotoKtp = new Tbl_DataKTP_Photo();

                var photoKtpLog = new Tbl_DataKTP_Photo_Log();

                var photoKtp = new Tbl_DataKTP_Photo();

                if (!string.IsNullOrWhiteSpace(enroll.KtpPhotoKTP))
                {
                    var photoKTPData = await _profileRepository.GetPhotoKtp(enroll.KtpNIK)
                        .ConfigureAwait(false);

                    byte[] imgBytes = Convert.FromBase64String(enroll.KtpPhotoKTP);

                    string filePath = "";

                    var systemParameterPath = await _sysParameterRepository.GetPathFolder("FolderPhoto")
                                                .ConfigureAwait(false);
                    string pathFolderFoto = systemParameterPath.Value;

                    string subPathFolderFoto = pathFolder + "/" + pathFolderFoto + "/" + enroll.KtpNIK + "/";

                    string fileName = "Foto_" + enroll.KtpNIK + "_" + JamServer + ".jpg";

                    if (_sftpConfig.IsActive)
                    {
                        using var stream = new MemoryStream(imgBytes);

                        (var fname, var fPath) = await stream.UploadToFTPServer(subPathFolderFoto, fileName, _sftpConfig.Host,
                            _sftpConfig.Username, _sftpConfig.Password, _sftpConfig.RootDirectory, _sftpConfig.Url).ConfigureAwait(false);

                        filePath = fPath;
                    }
                    else
                    {

                        if (!Directory.Exists(subPathFolderFoto))
                        {
                            Directory.CreateDirectory(subPathFolderFoto);
                        }

                        filePath = subPathFolderFoto + fileName;
                        File.WriteAllBytes(filePath, imgBytes);
                    }

                    if (photoKTPData != null)
                    {
                        photoKtpLog.CreatedById = photoKTPData.CreatedById;
                        photoKtpLog.CreatedByNpp = photoKTPData.CreatedByNpp;
                        photoKtpLog.CreatedByUid = photoKTPData.CreatedByUid;
                        photoKtpLog.CreatedByUnit = photoKTPData.CreatedByUnit;
                        photoKtpLog.CreatedTime = photoKTPData.CreatedTime;
                        photoKtpLog.FileName = photoKTPData.FileName;
                        photoKtpLog.Nik = photoKTPData.Nik;
                        photoKtpLog.PathFile = photoKTPData.PathFile;

                        exPhotoKtp = photoKTPData;
                    }

                    photoKtp = new Tbl_DataKTP_Photo
                    {
                        PathFile = filePath,
                        Nik = enroll.KtpNIK,
                        FileName = fileName,
                        IsActive = true,
                        IsDeleted = false,
                        CreatedById = Id,
                        CreatedByNpp = npp,
                        CreatedByUid = enroll.UID,
                        CreatedTime = DateTime.Now,
                        CreatedByUnit = unitCode
                    };
                }

                var exPhotoSignature = new Tbl_DataKTP_Signature();

                var photoSignatureLog = new Tbl_DataKTP_Signature_Log();

                var photoSignature = new Tbl_DataKTP_Signature();

                if (!string.IsNullOrWhiteSpace(enroll.KtpSignature))
                {
                    byte[] imgBytes = null;
                    bool isSkipSignature = false;

                    var photoSignatureData = await _profileRepository.GetPhotoSignature(enroll.KtpNIK)
                        .ConfigureAwait(false);

                    //byte[] imgBytes = Convert.FromBase64String(enroll.KtpSignature);
                    
                    try
                    {
                        imgBytes = Convert.FromBase64String(enroll.KtpSignature);
                    }
                    catch (Exception e)
                    {
                        isSkipSignature = true;

                        //logerror

                        var err = new Tbl_LogError
                        {
                            InnerException = "Error Convert enroll.KtpSignature to Base64.",
                            CreatedAt = DateTime.Now,
                            Message = e.ToString(),
                            Payload = "Signature Payload: " + enroll.KtpSignature + " , " + "NIK: " + enroll.KtpNIK,
                            Source = "Ekr.Api.DataFingerISO",
                            StackTrace = "",
                            SystemName = "Data Enrollment"
                        };

                         _errorLogRepository.CreateErrorLog(err);
                    }

                    if (isSkipSignature == false)
                    {
                        string filePath = "";

                        var systemParameterPath = await _sysParameterRepository.GetPathFolder("FolderSignature")
                                                    .ConfigureAwait(false);
                        string pathFolderFoto = systemParameterPath.Value;

                        string subPathFolderSignature = pathFolder + "/" + pathFolderFoto + "/" + enroll.KtpNIK + "/";
                        //string subPathFolderSignature = pathFolder + pathFolderFoto + enroll.KtpNIK;

                        string fileName = "Signature_" + enroll.KtpNIK + "_" + JamServer + ".jpg";

                        if (_sftpConfig.IsActive)
                        {
                            using var stream = new MemoryStream(imgBytes);

                            (var fname, var fPath) = await stream.UploadToFTPServer(subPathFolderSignature, fileName, _sftpConfig.Host,
                                _sftpConfig.Username, _sftpConfig.Password, _sftpConfig.RootDirectory, _sftpConfig.Url).ConfigureAwait(false);

                            filePath = fPath;
                        }
                        else
                        {
                            if (!Directory.Exists(subPathFolderSignature))
                            {
                                Directory.CreateDirectory(subPathFolderSignature);
                            }

                            filePath = subPathFolderSignature + fileName;
                            File.WriteAllBytes(filePath, imgBytes);
                        }

                        if (photoSignatureData != null)
                        {
                            photoSignatureLog.CreatedById = photoSignatureData.CreatedById;
                            photoSignatureLog.CreatedByNpp = photoSignatureData.CreatedByNpp;
                            photoSignatureLog.CreatedByUid = photoSignatureData.CreatedByUid;
                            photoSignatureLog.CreatedByUnit = photoSignatureData.CreatedByUnit;
                            photoSignatureLog.CreatedTime = photoSignatureData.CreatedTime;
                            photoSignatureLog.FileName = photoSignatureData.FileName;
                            photoSignatureLog.Nik = photoSignatureData.Nik;
                            photoSignatureLog.PathFile = photoSignatureData.PathFile;

                            exPhotoSignature = photoSignatureData;
                        }

                        photoSignature = new Tbl_DataKTP_Signature
                        {
                            CreatedById = Id,
                            CreatedByNpp = npp,
                            IsActive = true,
                            IsDeleted = false,
                            Nik = enroll.KtpNIK,
                            FileName = fileName,
                            CreatedByUid = enroll.UID,
                            CreatedTime = DateTime.Now,
                            CreatedByUnit = unitCode,
                            PathFile = filePath
                        };
                    }

                    #region old 20240304
                    //string filePath = "";

                    //var systemParameterPath = await _sysParameterRepository.GetPathFolder("FolderSignature")
                    //                            .ConfigureAwait(false);
                    //string pathFolderFoto = systemParameterPath.Value;

                    //string subPathFolderSignature = pathFolder + "/" + pathFolderFoto + "/" + enroll.KtpNIK + "/";
                    ////string subPathFolderSignature = pathFolder + pathFolderFoto + enroll.KtpNIK;

                    //string fileName = "Signature_" + enroll.KtpNIK + "_" + JamServer + ".jpg";

                    //if (_sftpConfig.IsActive)
                    //{
                    //    using var stream = new MemoryStream(imgBytes);

                    //    (var fname, var fPath) = await stream.UploadToFTPServer(subPathFolderSignature, fileName, _sftpConfig.Host,
                    //        _sftpConfig.Username, _sftpConfig.Password, _sftpConfig.RootDirectory, _sftpConfig.Url).ConfigureAwait(false);

                    //    filePath = fPath;
                    //}
                    //else
                    //{
                    //    if (!Directory.Exists(subPathFolderSignature))
                    //    {
                    //        Directory.CreateDirectory(subPathFolderSignature);
                    //    }

                    //    filePath = subPathFolderSignature + fileName;
                    //    File.WriteAllBytes(filePath, imgBytes);
                    //}

                    //if (photoSignatureData != null)
                    //{
                    //    photoSignatureLog.CreatedById = photoSignatureData.CreatedById;
                    //    photoSignatureLog.CreatedByNpp = photoSignatureData.CreatedByNpp;
                    //    photoSignatureLog.CreatedByUid = photoSignatureData.CreatedByUid;
                    //    photoSignatureLog.CreatedByUnit = photoSignatureData.CreatedByUnit;
                    //    photoSignatureLog.CreatedTime = photoSignatureData.CreatedTime;
                    //    photoSignatureLog.FileName = photoSignatureData.FileName;
                    //    photoSignatureLog.Nik = photoSignatureData.Nik;
                    //    photoSignatureLog.PathFile = photoSignatureData.PathFile;

                    //    exPhotoSignature = photoSignatureData;
                    //}

                    //photoSignature = new Tbl_DataKTP_Signature
                    //{
                    //    CreatedById = Id,
                    //    CreatedByNpp = npp,
                    //    IsActive = true,
                    //    IsDeleted = false,
                    //    Nik = enroll.KtpNIK,
                    //    FileName = fileName,
                    //    CreatedByUid = enroll.UID,
                    //    CreatedTime = DateTime.Now,
                    //    CreatedByUnit = unitCode,
                    //    PathFile = filePath
                    //};

                    #endregion
                }

                var exPhotoCam = new Tbl_DataKTP_PhotoCam();

                var photoCamLog = new Tbl_DataKTP_PhotoCam_Log();

                var photoCam = new Tbl_DataKTP_PhotoCam();

                if (!string.IsNullOrWhiteSpace(enroll.KtpPhotoCam))
                {
                    var photoCamData = await _profileRepository.GetPhotoCam(enroll.KtpNIK)
                        .ConfigureAwait(false);

                    byte[] imgBytes = Convert.FromBase64String(enroll.KtpPhotoCam);

                    string filePath = "";

                    var systemParameterPath = await _sysParameterRepository.GetPathFolder("FolderWebcam")
                                                .ConfigureAwait(false);
                    string pathFolderFoto = systemParameterPath.Value;

                    string SubPathFolderPhotoCam = pathFolder + "/" + pathFolderFoto + "/" + enroll.KtpNIK + "/";
                    //string SubPathFolderPhotoCam = pathFolder + pathFolderFoto + enroll.KtpNIK;

                    string fileName = "PhotoCam_" + enroll.KtpNIK + "_" + JamServer + ".txt";

                    if (_sftpConfig.IsActive)
                    {
                        using var stream = new MemoryStream(imgBytes);

                        (var fname, var fPath) = await stream.UploadToFTPServer(SubPathFolderPhotoCam, fileName, _sftpConfig.Host,
                            _sftpConfig.Username, _sftpConfig.Password, _sftpConfig.RootDirectory, _sftpConfig.Url).ConfigureAwait(false);

                        filePath = fPath;
                    }
                    else
                    {
                        if (!Directory.Exists(SubPathFolderPhotoCam))
                        {
                            Directory.CreateDirectory(SubPathFolderPhotoCam);
                        }

                        filePath = SubPathFolderPhotoCam + fileName;
                        File.WriteAllBytes(filePath, imgBytes);
                    }

                    if (photoCamData != null)
                    {
                        photoCamLog.CreatedById = photoCamData.CreatedById;
                        photoCamLog.CreatedByNpp = photoCamData.CreatedByNpp;
                        photoCamLog.CreatedByUid = photoCamData.CreatedByUid;
                        photoCamLog.CreatedByUnit = photoCamData.CreatedByUnit;
                        photoCamLog.CreatedTime = photoCamData.CreatedTime;
                        photoCamLog.FileName = photoCamData.FileName;
                        photoCamLog.Nik = photoCamData.Nik;
                        photoCamLog.PathFile = photoCamData.PathFile;

                        exPhotoCam = photoCamData;
                    }

                    photoCam = new Tbl_DataKTP_PhotoCam
                    {
                        PathFile = filePath,//
                        Nik = enroll.KtpNIK,
                        FileName = fileName,//
                        CreatedTime = DateTime.Now,
                        CreatedById = Id,
                        IsActive = true,
                        IsDeleted = false,
                        CreatedByNpp = npp,
                        CreatedByUid = enroll.UID,
                        CreatedByUnit = unitCode
                    };
                }

                var exPhotoFinger = new List<Tbl_DataKTP_Finger>();

                var exPhotoFingerEmployee = new List<Tbl_DataKTP_Finger_Employee>();

                var photoFingerLogs = new List<Tbl_DataKTP_Finger_Log>();

                var photoFingers = new List<Tbl_DataKTP_Finger>();

                var photoFingersEmployee = new List<Tbl_DataKTP_Finger_Employee>();

                if (!string.IsNullOrWhiteSpace(enroll.KtpFingerKanan))
                {

                    var photoFingerDataEmployee = new Tbl_DataKTP_Finger_Employee();

                    if (isEmployee)
                    {
                        photoFingerDataEmployee = await _profileRepository.GetPhotoFingerEmployee(enroll.KtpNIK, enroll.KtpTypeJariKanan)
                        .ConfigureAwait(false);
                    }

                    string imageEncrypted = enroll.KtpFingerKanan.Encrypt(Phrase.FileEncryption);

                    string filePath = "";
                    string filePathIso = "";

                    var systemParameterPath = await _sysParameterRepository.GetPathFolder("FolderFinger")
                                                .ConfigureAwait(false);
                    string pathFolderFoto = systemParameterPath.Value;

                    string subPathFolderPhotoFinger = pathFolder + "/" + pathFolderFoto + "/" + enroll.KtpNIK + "/";

                    string JenisJari = enroll.KtpTypeJariKanan.Replace(" ", "");
                    string fileName = "PhotoFinger_" + JenisJari + "_" + enroll.KtpNIK + "_" + JamServer + ".txt";
                    string fileNameIso = "";

                    if (_sftpConfig.IsActive)
                    {
                        using var stream = new MemoryStream(Encoding.ASCII.GetBytes(imageEncrypted));

                        (var fname, var fPath) = await stream.UploadToFTPServer(subPathFolderPhotoFinger, fileName, _sftpConfig.Host,
                            _sftpConfig.Username, _sftpConfig.Password, _sftpConfig.RootDirectory, _sftpConfig.Url).ConfigureAwait(false);

                        filePath = fPath;
                    }
                    else
                    {
                        if (!Directory.Exists(subPathFolderPhotoFinger))
                        {
                            Directory.CreateDirectory(subPathFolderPhotoFinger);
                        }

                        filePath = subPathFolderPhotoFinger + fileName;
                        File.WriteAllText(filePath, imageEncrypted);
                    }

                    string isoEncrypted = "";
                    if (!string.IsNullOrWhiteSpace(enroll.KtpFingerKananIso))
                    {
                        isoEncrypted = enroll.KtpFingerKananIso.Encrypt(Phrase.FileEncryption);
                        fileNameIso = "PhotoFingerISO_" + JenisJari + "_" + enroll.KtpNIK + "_" + JamServer + ".txt";

                        if (_sftpConfig.IsActive)
                        {
                            using var stream = new MemoryStream(Encoding.ASCII.GetBytes(isoEncrypted));

                            (var fname, var fPath) = await stream.UploadToFTPServer(subPathFolderPhotoFinger, fileNameIso, _sftpConfig.Host,
                                _sftpConfig.Username, _sftpConfig.Password, _sftpConfig.RootDirectory, _sftpConfig.Url).ConfigureAwait(false);

                            filePathIso = fPath;
                        }
                        else
                        {
                            if (!Directory.Exists(subPathFolderPhotoFinger))
                            {
                                Directory.CreateDirectory(subPathFolderPhotoFinger);
                            }

                            filePathIso = subPathFolderPhotoFinger + fileNameIso;
                            File.WriteAllText(filePathIso, isoEncrypted);
                        }
                    }

                    var typeFingerMain = await _fingerRepository.GetFingersEnrolled(enroll.KtpNIK);

                    foreach (var i in typeFingerMain)
                    {

                        if (i.TypeFinger.Contains("Kanan"))
                        {
                            var photoFingerData = await _profileRepository.GetPhotoFinger(enroll.KtpNIK, i.TypeFinger).ConfigureAwait(false);

                            if (photoFingerData != null)
                            {
                                photoFingerLogs.Add(new Tbl_DataKTP_Finger_Log
                                {
                                    CreatedById = photoFingerData.CreatedById,
                                    CreatedByNpp = photoFingerData.CreatedByNpp,
                                    CreatedByUid = photoFingerData.CreatedByUid,
                                    CreatedByUnit = photoFingerData.CreatedByUnit,
                                    CreatedByUnitId = photoFingerData.CreatedByUnitId,
                                    CreatedTime = photoFingerData.CreatedTime,
                                    //FileJari = photoFingerData.FileJari,
                                    FileJariISO = photoFingerData.FileJariISO,
                                    FileName = photoFingerData.FileName,
                                    FileNameISO = photoFingerData.FileNameISO,
                                    Nik = photoFingerData.Nik,
                                    PathFile = photoFingerData.PathFile,
                                    PathFileISO = photoFingerData.PathFileISO,
                                    TypeFinger = photoFingerData.TypeFinger
                                });

                                exPhotoFinger.Add(photoFingerData);
                            }
                        }
                    }

                    photoFingers.Add(new Tbl_DataKTP_Finger
                    {
                        CreatedById = Id,
                        IsActive = true,
                        IsDeleted = false,
                        Nik = enroll.KtpNIK,
                        PathFile = filePath,//
                        PathFileISO = filePathIso,//
                        CreatedByNpp = npp,
                        CreatedByUid = enroll.UID,
                        CreatedTime = DateTime.Now,
                        FileName = fileName,//
                        FileNameISO = fileNameIso,//
                        FileJariISO = isoEncrypted,
                        TypeFinger = enroll.KtpTypeJariKanan,
                        CreatedByUnit = unitCode,
                        CreatedByUnitId = unitId
                    });

                    if (isEmployee)
                    {
                        photoFingersEmployee.Add(new Tbl_DataKTP_Finger_Employee
                        {
                            CreatedById = Id,
                            IsActive = true,
                            IsDeleted = false,
                            Nik = enroll.KtpNIK,
                            PathFile = filePath,//
                            PathFileISO = filePathIso,//
                            CreatedByNpp = npp,
                            CreatedByUid = enroll.UID,
                            CreatedTime = DateTime.Now,
                            FileName = fileName,//
                            FileNameISO = fileNameIso,//
                            FileJariISO = isoEncrypted,
                            TypeFinger = enroll.KtpTypeJariKanan,
                            CreatedByUnit = unitCode,
                            CreatedByUnitId = unitId
                        });
                    }
                }

                if (!string.IsNullOrWhiteSpace(enroll.KtpFingerKiri))
                {
                    var photoFingerDataEmployee = new Tbl_DataKTP_Finger_Employee();

                    if (isEmployee)
                    {
                        photoFingerDataEmployee = await _profileRepository.GetPhotoFingerEmployee(enroll.KtpNIK, enroll.KtpTypeJariKiri)
                        .ConfigureAwait(false);
                    }

                    string imageEncrypted = enroll.KtpFingerKiri.Encrypt(Phrase.FileEncryption);

                    string filePath = "";
                    string filePathIso = "";

                    var systemParameterPath = await _sysParameterRepository.GetPathFolder("FolderFinger")
                                                .ConfigureAwait(false);
                    string pathFolderFoto = systemParameterPath.Value;

                    string subPathFolderPhotoFinger = pathFolder + "/" + pathFolderFoto + "/" + enroll.KtpNIK + "/";

                    string JenisJari = enroll.KtpTypeJariKiri.Replace(" ", "");
                    string fileName = "PhotoFinger_" + JenisJari + "_" + enroll.KtpNIK + "_" + JamServer + ".txt";
                    string fileNameIso = "";

                    if (_sftpConfig.IsActive)
                    {
                        using var stream = new MemoryStream(Encoding.ASCII.GetBytes(imageEncrypted));

                        (var fname, var fPath) = await stream.UploadToFTPServer(subPathFolderPhotoFinger, fileName, _sftpConfig.Host,
                            _sftpConfig.Username, _sftpConfig.Password, _sftpConfig.RootDirectory, _sftpConfig.Url).ConfigureAwait(false);

                        filePath = fPath;
                    }
                    else
                    {
                        if (!Directory.Exists(subPathFolderPhotoFinger))
                        {
                            Directory.CreateDirectory(subPathFolderPhotoFinger);
                        }

                        filePath = subPathFolderPhotoFinger + fileName;
                        File.WriteAllText(filePath, imageEncrypted);
                    }
                    string isoEncrypted = "";

                    if (!string.IsNullOrWhiteSpace(enroll.KtpFingerKiriIso))
                    {
                        isoEncrypted = enroll.KtpFingerKiriIso.Encrypt(Phrase.FileEncryption);
                        fileNameIso = "PhotoFingerISO_" + JenisJari + "_" + enroll.KtpNIK + "_" + JamServer + ".txt";

                        if (_sftpConfig.IsActive)
                        {
                            using var stream = new MemoryStream(Encoding.ASCII.GetBytes(isoEncrypted));

                            (var fname, var fPath) = await stream.UploadToFTPServer(subPathFolderPhotoFinger, fileNameIso, _sftpConfig.Host,
                                _sftpConfig.Username, _sftpConfig.Password, _sftpConfig.RootDirectory, _sftpConfig.Url).ConfigureAwait(false);

                            filePathIso = fPath;
                        }
                        else
                        {
                            if (!Directory.Exists(subPathFolderPhotoFinger))
                            {
                                Directory.CreateDirectory(subPathFolderPhotoFinger);
                            }

                            filePathIso = subPathFolderPhotoFinger + fileNameIso;
                            File.WriteAllText(filePathIso, isoEncrypted);
                        }
                    }
                    var typeFingerMain = await _fingerRepository.GetFingersEnrolled(enroll.KtpNIK);

                    foreach (var i in typeFingerMain)
                    {

                        if (i.TypeFinger.Contains("Kiri"))
                        {
                            var photoFingerData = await _profileRepository.GetPhotoFinger(enroll.KtpNIK, i.TypeFinger).ConfigureAwait(false);

                            if (photoFingerData != null)
                            {
                                photoFingerLogs.Add(new Tbl_DataKTP_Finger_Log
                                {
                                    CreatedById = photoFingerData.CreatedById,
                                    CreatedByNpp = photoFingerData.CreatedByNpp,
                                    CreatedByUid = photoFingerData.CreatedByUid,
                                    CreatedByUnit = photoFingerData.CreatedByUnit,
                                    CreatedByUnitId = photoFingerData.CreatedByUnitId,
                                    CreatedTime = photoFingerData.CreatedTime,
                                    //FileJari = photoFingerData.FileJari,
                                    FileJariISO = photoFingerData.FileJariISO,
                                    FileName = photoFingerData.FileName,
                                    FileNameISO = photoFingerData.FileNameISO,
                                    Nik = photoFingerData.Nik,
                                    PathFile = photoFingerData.PathFile,
                                    PathFileISO = photoFingerData.PathFileISO,
                                    TypeFinger = photoFingerData.TypeFinger
                                });

                                exPhotoFinger.Add(photoFingerData);
                            }
                        }
                    }

                    photoFingers.Add(new Tbl_DataKTP_Finger
                    {
                        CreatedById = Id,
                        IsActive = true,
                        IsDeleted = false,
                        Nik = enroll.KtpNIK,
                        PathFile = filePath,//
                        PathFileISO = filePathIso,//
                        CreatedByNpp = npp,
                        CreatedByUid = enroll.UID,
                        CreatedTime = DateTime.Now,
                        FileName = fileName,//
                        FileNameISO = fileNameIso,//
                        FileJariISO = isoEncrypted,//
                        TypeFinger = enroll.KtpTypeJariKiri,
                        CreatedByUnit = unitCode,
                        CreatedByUnitId = unitId
                    });

                    if (isEmployee)
                    {
                        photoFingersEmployee.Add(new Tbl_DataKTP_Finger_Employee
                        {
                            CreatedById = Id,
                            IsActive = true,
                            IsDeleted = false,
                            Nik = enroll.KtpNIK,
                            PathFile = filePath,//
                            PathFileISO = filePathIso,//
                            CreatedByNpp = npp,
                            CreatedByUid = enroll.UID,
                            CreatedTime = DateTime.Now,
                            FileName = fileName,//
                            FileNameISO = fileNameIso,//
                            FileJariISO = isoEncrypted,//
                            TypeFinger = enroll.KtpTypeJariKiri,
                            CreatedByUnit = unitCode,
                            CreatedByUnitId = unitId
                        });
                    }
                }

                // to do: alat reader log
                var dataReader = await _dataReaderRepository.GetDatareaderUid(enroll.UID).ConfigureAwait(false);

                var dataReaderLog = new Tbl_MasterAlatReaderLog();

                if (dataReader != null)
                {
                    dataReaderLog = new Tbl_MasterAlatReaderLog
                    {
                        CreatedBy_Id = Id,
                        CreatedTime = DateTime.Now,
                        IsActive = true,
                        IsDeleted = false,
                        Nik = enroll.KtpNIK,
                        PegawaiId = Id,
                        Serial_Number = dataReader.SN_Unit,
                        Type = "Enroll",
                        Uid = enroll.UID
                    };
                }

                _enrollmentKTPRepository.InsertEnrollFlow(dataDemografis, dataDemografisLog, photoKtp, photoKtpLog, photoSignature, photoSignatureLog,
                    photoCam, photoCamLog, photoFingers, photoFingerLogs, exPhotoKtp, exPhotoSignature, exPhotoCam, exPhotoFinger, dataNpp, dataReaderLog, photoFingersEmployee, exPhotoFingerEmployee);

                await _alatReaderRepository.CreateLogActivity2(new Tbl_MasterAlatReaderLogActvity
                {
                    CreatedBy_Id = Id,
                    IsActive = true,
                    IsDeleted = false,
                    KodeUnit = unitCode,
                    LastIP = enroll.IpAddress,
                    NIK = enroll.KtpNIK,
                    CreatedTime = DateTime.Now,
                    NppPegawai = npp,
                    PegawaiId = Id,
                    Type = "Enroll",
                    UID = enroll.UID,
                    UnitId = unitId
                });
            }

            if (IsNasabah)
            {
                return (_SuccessMessageConfig.NasabahSuksesEnrollNasabah, (int)EnrollStatus.Nasabah_Berhasil_di_enroll, cekKTP?.CIF);
            }
            else
            {
                return (_SuccessMessageConfig.NasabahSuksesEnrollNonNasabah, (int)EnrollStatus.Nasabah_Berhasil_di_enroll, cekKTP?.CIF);
            }
        }

        public async Task<(string msg, int code, string cif)> SubmitEnrollmentFingerEncryptedOnlyISOThirdParty(string AppsChannel, bool isHitSOA, ApiSOA ReqSoa, EnrollKTPThirdParty2VM enroll, string remoteIpAddress)
        {
            bool isEmployee = false;
            var _empData = await _enrollmentKTPRepository.IsEmployee(enroll.KtpNIK);
            if (_empData != null)
            {
                isEmployee = true;
            }

            #region check if finger, status, gender and religion in accordance with available option
            if ((string.IsNullOrEmpty(enroll.KtpTypeJariKanan) && string.IsNullOrEmpty (enroll.KtpTypeJariKiri)) || (string.IsNullOrEmpty(enroll.KtpFingerKanan) && string.IsNullOrEmpty(enroll.KtpFingerKiri)) || (string.IsNullOrEmpty(enroll.KtpFingerKananIso) && string.IsNullOrEmpty(enroll.KtpFingerKiriIso)))
            {
                return (_ErrorMessageConfig.FingerNotFound, (int)EnrollStatus.Inputan_tidak_lengkap, "");
            }

            #region check type, image and iso availability
            if (string.IsNullOrEmpty(enroll.KtpTypeJariKanan) || string.IsNullOrEmpty(enroll.KtpFingerKanan) || string.IsNullOrEmpty(enroll.KtpFingerKananIso))
            {
                if (string.IsNullOrEmpty(enroll.KtpTypeJariKiri) || string.IsNullOrEmpty(enroll.KtpFingerKiri) || string.IsNullOrEmpty(enroll.KtpFingerKiriIso))
                {
                    return (_ErrorMessageConfig.FingerParamsNotComplete, (int)EnrollStatus.Inputan_tidak_lengkap, "");
                }
            }
            

            if (string.IsNullOrEmpty(enroll.KtpTypeJariKiri) || string.IsNullOrEmpty(enroll.KtpFingerKiri) || string.IsNullOrEmpty(enroll.KtpFingerKiriIso))
            {
                if (string.IsNullOrEmpty(enroll.KtpTypeJariKanan) || string.IsNullOrEmpty(enroll.KtpFingerKanan) || string.IsNullOrEmpty(enroll.KtpFingerKananIso))
                {
                    return (_ErrorMessageConfig.FingerParamsNotComplete, (int)EnrollStatus.Inputan_tidak_lengkap, "");
                }
            }
            #endregion region

            if ((string.IsNullOrEmpty(enroll.KtpTypeJariKanan) == false) && (enroll.KtpTypeJariKanan.ToUpper() != _OpsiTipeJariKananConfig.IbuJariKanan) && (enroll.KtpTypeJariKanan.ToUpper() != _OpsiTipeJariKananConfig.JariTelunjukKanan) && (enroll.KtpTypeJariKanan.ToUpper() != _OpsiTipeJariKananConfig.JariTengahKanan) && (enroll.KtpTypeJariKanan.ToUpper() != _OpsiTipeJariKananConfig.JariManisKanan) && (enroll.KtpTypeJariKanan.ToUpper() != _OpsiTipeJariKananConfig.JariKelingkingKanan))
            {
                return (_ErrorMessageConfig.OpsiJariKananInvalid, (int)EnrollStatus.Inputan_tidak_lengkap, "");
            }

            if ((string.IsNullOrEmpty(enroll.KtpTypeJariKiri) == false) && (enroll.KtpTypeJariKiri.ToUpper() != _OpsiTipeJariKiriConfig.IbuJariKiri) && (enroll.KtpTypeJariKiri.ToUpper() != _OpsiTipeJariKiriConfig.JariTelunjukKiri) && (enroll.KtpTypeJariKiri.ToUpper() != _OpsiTipeJariKiriConfig.JariTengahKiri) && (enroll.KtpTypeJariKiri.ToUpper() != _OpsiTipeJariKiriConfig.JariManisKiri) && (enroll.KtpTypeJariKiri.ToUpper() != _OpsiTipeJariKiriConfig.JariKelingkingKiri))
            {
                return (_ErrorMessageConfig.OpsiJariKiriInvalid, (int)EnrollStatus.Inputan_tidak_lengkap, "");
            }

            if ((enroll.KtpStatusPerkawinan.ToUpper() != _OpsiStatusPerkawinanConfig.BelumKawin) && (enroll.KtpStatusPerkawinan.ToUpper() != _OpsiStatusPerkawinanConfig.Kawin) && (enroll.KtpStatusPerkawinan.ToUpper() != _OpsiStatusPerkawinanConfig.CeraiHidup) && (enroll.KtpStatusPerkawinan.ToUpper() != _OpsiStatusPerkawinanConfig.CeraiMati))
            {
                return (_ErrorMessageConfig.OpsiStatusPerkawinanInvalid, (int)EnrollStatus.Inputan_tidak_lengkap, "");
            }

            if ((enroll.KtpJanisKelamin.ToUpper() != _OpsiGenderConfig.LakiLaki) &&  (enroll.KtpJanisKelamin.ToUpper() != _OpsiGenderConfig.Perempuan))
            {
                return (_ErrorMessageConfig.OpsiGenderInvalid, (int)EnrollStatus.Inputan_tidak_lengkap, "");
            }

            if ((enroll.KtpAgama.ToUpper() != _OpsiAgamaConfig.Islam) && (enroll.KtpAgama.ToUpper() != _OpsiAgamaConfig.KristenKatolik) && (enroll.KtpAgama.ToUpper() != _OpsiAgamaConfig.KristenProtestan) && (enroll.KtpAgama.ToUpper() != _OpsiAgamaConfig.Hindu) && (enroll.KtpAgama.ToUpper() != _OpsiAgamaConfig.Buddha) && (enroll.KtpAgama.ToUpper() != _OpsiAgamaConfig.KongHuCu))
            {
                return (_ErrorMessageConfig.OpsiAgamaInvalid, (int)EnrollStatus.Inputan_tidak_lengkap, "");
            }
            
            
            
            #endregion

            #region check if blood type option is in accordance with available option
            if (enroll.KtpGolonganDarah == null || enroll.KtpGolonganDarah == "" || enroll.KtpGolonganDarah == " ")
            {
                enroll.KtpGolonganDarah = _OpsiGolDarahConfig.Null;
            }
            else
            {
                if ((enroll.KtpGolonganDarah.ToUpper() != _OpsiGolDarahConfig.A) && (enroll.KtpGolonganDarah.ToUpper() != _OpsiGolDarahConfig.B) && (enroll.KtpGolonganDarah.ToUpper() != _OpsiGolDarahConfig.AB) && (enroll.KtpGolonganDarah.ToUpper() != _OpsiGolDarahConfig.O) && (enroll.KtpGolonganDarah.ToUpper() != _OpsiGolDarahConfig.Null))
                {
                    return (_ErrorMessageConfig.OpsiGolDarahInvalid, (int)EnrollStatus.Inputan_tidak_lengkap, "");
                }
            }
            #endregion

            #region check wni wna
            if (enroll.KtpKewarganegaraan != null)
            {
                if ((enroll.KtpKewarganegaraan != _OpsiKewarganegaraanConfig.WNI) && (enroll.KtpKewarganegaraan != _OpsiKewarganegaraanConfig.WNA))
                {
                    return (_ErrorMessageConfig.OpsiKewarganegaraanInvalid, (int)EnrollStatus.Inputan_tidak_lengkap, "");
                }
            }

            #endregion

            #region Check ISO Validation
            if (!String.IsNullOrEmpty(enroll.KtpFingerKanan))
            {
                 //jpg & png
                if (enroll.KtpFingerKanan.Substring(0, 5) != _OpsiTipeFileMarkerConfig.JPGstart  && enroll.KtpFingerKanan.Substring(0, 5).ToUpper() != _OpsiTipeFileMarkerConfig.PNGstart)
                {
                    return (_ErrorMessageConfig.FileTypeFingerKananInvalid, (int)EnrollStatus.Inputan_tidak_lengkap, "");
                }
            }

            if (!String.IsNullOrEmpty(enroll.KtpFingerKiri))
            {
                //jpg & png
                if (enroll.KtpFingerKiri.Substring(0, 5) != _OpsiTipeFileMarkerConfig.JPGstart && enroll.KtpFingerKiri.Substring(0, 5).ToUpper() != _OpsiTipeFileMarkerConfig.PNGstart)
                {
                    return (_ErrorMessageConfig.FileTypeFingerKiriInvalid, (int)EnrollStatus.Inputan_tidak_lengkap, "");
                }

            }

            if (!String.IsNullOrEmpty(enroll.KtpFingerKananIso))
            {
                if (enroll.KtpFingerKananIso.Substring(0, 3) != _OpsiTipeFileMarkerConfig.ISOstart) 
                {
                    return (_ErrorMessageConfig.FileTypeFingerKananISOInvalid, (int)EnrollStatus.Inputan_tidak_lengkap, "");
                }
            }

            if (!String.IsNullOrEmpty(enroll.KtpFingerKiriIso))
            {
                if (enroll.KtpFingerKiriIso.Substring(0, 3) != _OpsiTipeFileMarkerConfig.ISOstart)
                {
                    return (_ErrorMessageConfig.FileTypeFingerKiriISOInvalid, (int)EnrollStatus.Inputan_tidak_lengkap, "");
                }
            }

            #endregion

            #region Check NIK number validation
            if (string.IsNullOrEmpty(enroll.KtpNIK))
            {
                return (_ErrorMessageConfig.ErrorParameterKosong, (int)EnrollStatus.Inputan_tidak_lengkap, "");
            }

            if ((enroll.KtpNIK.Length > 16) || (enroll.KtpNIK.Length < 16))
            {
                return (_ErrorMessageConfig.NIKLessOrMoreThan16, (int)EnrollStatus.Inputan_tidak_lengkap, "");
            }

            if (enroll.KtpNIK.IsNumeric() == false)
            {
                return ("KtpNIK " + _ErrorMessageConfig.MsgNumericOnly, (int)EnrollStatus.Inputan_tidak_lengkap, "");
            }
            #endregion

            #region Cek Kode Pos
            if (!(string.IsNullOrEmpty (enroll.KtpKodePos)))
            {
                if (enroll.KtpKodePos.IsNumeric() == false)
                {
                    return ("ktpKodePos " + _ErrorMessageConfig.MsgNumericOnly, (int)EnrollStatus.Inputan_tidak_lengkap, "");
                }

                else if ((enroll.KtpKodePos.Length < 5) || (enroll.KtpKodePos.Length > 10))
                {
                    return (_ErrorMessageConfig.PostalCodeBetween5And10, (int)EnrollStatus.Inputan_tidak_lengkap, "");
                }
            }
            
            #endregion

            #region ktp tanggal lahir
            if (enroll.KtpTanggalLahir != null)
            {
                enroll.KtpTanggalLahir = enroll.KtpTanggalLahir.Replace(".", "");
                if (enroll.KtpTanggalLahir.Contains("/"))
                {
                    enroll.KtpTanggalLahir = enroll.KtpTanggalLahir.Replace("/", "-");
                }
                string[] arrayTanggl = enroll.KtpTanggalLahir.Split("-");
                string hariLahir = "";
                string bulanLahir = "";
                string tahunLahir = "";

                if (arrayTanggl != null)
                {
                    if (arrayTanggl[0] != null && arrayTanggl[0].Length <= 2)
                    {
                        if (arrayTanggl[0].Length != 2)
                        {
                            hariLahir = "0" + arrayTanggl[0];

                        }
                        else
                        {
                            hariLahir = arrayTanggl[0];

                        }
                    }
                    else
                    {
                        return (_ErrorMessageConfig.FormatTglLahirInvalid, (int)EnrollStatus.Inputan_tidak_lengkap, "");
                    }

                    if (arrayTanggl[1] != null && (0 < Int32.Parse(arrayTanggl[1]) && Int32.Parse(arrayTanggl[1]) <= 12))
                    {
                        if (arrayTanggl[1].Length != 2)
                        {
                            bulanLahir = "0" + arrayTanggl[1];

                        }
                        else
                        {
                            bulanLahir = arrayTanggl[1];

                        }
                    }
                    else
                    {
                        return (_ErrorMessageConfig.FormatTglLahirInvalid, (int)EnrollStatus.Inputan_tidak_lengkap, "");
                    }

                    if (arrayTanggl[2] != null && arrayTanggl[2].Length >= 4 )
                    {
                        tahunLahir = arrayTanggl[2];
                    }
                    else
                    {
                        return (_ErrorMessageConfig.FormatTglLahirInvalid, (int)EnrollStatus.Inputan_tidak_lengkap, "");
                    }
                }

                enroll.KtpTanggalLahir = hariLahir + "-" + bulanLahir + "-" + tahunLahir;
            }
            #endregion

            #region ktp ttl
            if (enroll.KtpTTL != null)
            {
                string[] arrayTTL = enroll.KtpTTL.Split(",");
                if (arrayTTL != null)
                {
                    if (arrayTTL[1].Contains("/"))
                    {
                        arrayTTL[1] = arrayTTL[1].Replace("/", "-");
                    }

                    if (arrayTTL[1].Contains(" "))
                    {
                        arrayTTL[1] = arrayTTL[1].Replace(" ", "");
                    }

                    string[] arrayTglLahir = arrayTTL[1].Split("-");
                    string tempatTTL = arrayTTL[0];
                    string hariTTL = "";
                    string bulanTTL = "";
                    string tahunTTL = "";

                    if (arrayTglLahir != null)
                    {
                        if (arrayTglLahir[0] != null && arrayTglLahir[0].Length <= 2)
                        {
                            if (arrayTglLahir[0].Length != 2)
                            {
                                hariTTL = "0" + arrayTglLahir[0];
                            }
                            else
                            {
                                hariTTL = arrayTglLahir[0];
                            }
                        }
                        else
                        {
                            return (_ErrorMessageConfig.FormatTglLahirInvalid, (int)EnrollStatus.Inputan_tidak_lengkap, "");
                        }

                        if (arrayTglLahir[1] != null && (0 < Int32.Parse(arrayTglLahir[1]) && Int32.Parse(arrayTglLahir[1]) <= 12))
                        {
                            if (arrayTglLahir[1].Length != 2)
                            {
                                bulanTTL = "0" + arrayTglLahir[1];
                            }
                            else
                            {
                                bulanTTL = arrayTglLahir[1];
                            }
                        }
                        else
                        {
                            return (_ErrorMessageConfig.FormatTglLahirInvalid, (int)EnrollStatus.Inputan_tidak_lengkap, "");
                        }

                        if (arrayTglLahir[2] != null && arrayTglLahir[2].Length >= 4)
                        {
                            tahunTTL = arrayTglLahir[2];
                        }
                        else
                        {
                            return (_ErrorMessageConfig.FormatTglLahirInvalid, (int)EnrollStatus.Inputan_tidak_lengkap, "");
                        }

                        enroll.KtpTTL = tempatTTL + "," + hariTTL + "-" + bulanTTL + "-" + tahunTTL;
                    }
                }
            }
            #endregion

            #region ktp tanggal berlaku
            if (enroll.KtpMasaBerlaku != null || enroll.KtpMasaBerlaku != "-" || enroll.KtpMasaBerlaku != "" || enroll.KtpMasaBerlaku != " ")
            {
                if (enroll.KtpMasaBerlaku.Contains("/"))
                {
                    enroll.KtpMasaBerlaku = enroll.KtpMasaBerlaku.Replace("/", "-");
                }
                string[] arrayTanggl = enroll.KtpMasaBerlaku.Split("-");
                string hariKTP = "";
                string bulanKTP = "";
                string tahunKTP = "";

                if (arrayTanggl != null)
                {
                    if (arrayTanggl[0] != null && arrayTanggl[0].Length <= 2)
                    {
                        if (arrayTanggl[0].Length != 2)
                        {
                            hariKTP = "0" + arrayTanggl[0];

                        }
                        else
                        {
                            hariKTP = arrayTanggl[0];
                        }
                    }
                    else
                    {
                        return (_ErrorMessageConfig.MasaBerlakuKTPInvalid, (int)EnrollStatus.Inputan_tidak_lengkap, "");
                    }

                    if (arrayTanggl[1] != null && (0 < Int32.Parse(arrayTanggl[1]) && Int32.Parse(arrayTanggl[1]) <= 12))
                    {
                        if (arrayTanggl[1].Length != 2)
                        {
                            bulanKTP = "0" + arrayTanggl[1];

                        }
                        else
                        {
                            bulanKTP = arrayTanggl[1];

                        }
                    }
                    else
                    {
                        return (_ErrorMessageConfig.MasaBerlakuKTPInvalid, (int)EnrollStatus.Inputan_tidak_lengkap, "");
                    }

                    if (arrayTanggl[2] != null && arrayTanggl[2].Length >= 4)
                    {
                        tahunKTP = arrayTanggl[2];
                    }
                    else
                    {
                        return (_ErrorMessageConfig.MasaBerlakuKTPInvalid, (int)EnrollStatus.Inputan_tidak_lengkap, "");
                    }
                }

                enroll.KtpMasaBerlaku = hariKTP + "-" + bulanKTP + "-" + tahunKTP;
            }
            #endregion

            #region ktpRT RW
            if (enroll.KtpRT != null)
            {
                if (enroll.KtpRT.IsNumeric() == false)
                {
                    return ("KtpRT " + _ErrorMessageConfig.MsgNumericOnly, (int)EnrollStatus.Inputan_tidak_lengkap, "");
                }
                if (enroll.KtpRT.Length == 1)
                {
                    enroll.KtpRT = "00" + enroll.KtpRT;
                }

                if (enroll.KtpRT.Length == 2)
                {
                    enroll.KtpRT = '0' + enroll.KtpRT;
                }

                if (enroll.KtpRT.Length > 3)
                {
                    return (_ErrorMessageConfig.FormatRTRWInvalid, (int)EnrollStatus.Inputan_tidak_lengkap, "");
                }
            }

            if (enroll.KtpRW != null)
            {
                if (enroll.KtpRW.IsNumeric() == false)
                {
                    return ("KtpRW " + _ErrorMessageConfig.MsgNumericOnly, (int)EnrollStatus.Inputan_tidak_lengkap, "");
                }
                if (enroll.KtpRW.Length == 1)
                {
                    enroll.KtpRW = "00" + enroll.KtpRW;
                }

                if (enroll.KtpRW.Length == 2)
                {
                    enroll.KtpRW = '0' + enroll.KtpRW;
                }

                if (enroll.KtpRW.Length > 3)
                {
                    return (_ErrorMessageConfig.FormatRTRWInvalid, (int)EnrollStatus.Inputan_tidak_lengkap, "");
                }
            }

            if (enroll.KtpRTRW != null)
            {
                if (enroll.KtpRTRW.Contains("/"))
                {
                    string[] arrRTRW = enroll.KtpRTRW.Split("/");

                    string RT = "";
                    string RW = "";
                    
                    if (arrRTRW[0].IsNumeric() == false)
                    {
                        return ("RT pada KtpRTRW " + _ErrorMessageConfig.MsgNumericOnly, (int)EnrollStatus.Inputan_tidak_lengkap, "");
                    }

                    if (arrRTRW[0].Length == 1)
                    {
                        RT = "00" + arrRTRW[0];
                    }

                    if (arrRTRW[0].Length == 2)
                    {
                        RT = "0" + arrRTRW[0];
                    }

                    if (arrRTRW[0].Length > 3)
                    {
                        return (_ErrorMessageConfig.FormatRTRW2Invalid, (int)EnrollStatus.Inputan_tidak_lengkap, "");
                    }

                    if (arrRTRW[1].IsNumeric() == false)
                    {
                        return ("RW pada KtpRTRW " + _ErrorMessageConfig.MsgNumericOnly, (int)EnrollStatus.Inputan_tidak_lengkap, "");
                    }

                    if (arrRTRW[1].Length == 1)
                    {
                        RW = "00" + arrRTRW[1];
                    }

                    if (arrRTRW[1].Length == 2)
                    {
                        RW = "0" + arrRTRW[1];
                    }

                    if (arrRTRW[1].Length > 3)
                    {
                        return (_ErrorMessageConfig.FormatRTRW2Invalid, (int)EnrollStatus.Inputan_tidak_lengkap, "");
                    }

                    enroll.KtpRTRW = RT + "/" + RW;
                }
                else
                {
                    return (_ErrorMessageConfig.FormatRTRW2Invalid, (int)EnrollStatus.Inputan_tidak_lengkap, "");
                }
            }
            #endregion

            #region ktpCIF
            if (enroll.KtpCif != null)
            {
                if (enroll.KtpCif.IsNumeric() == false)
                {
                    return ("KtpCif " + _ErrorMessageConfig.MsgNumericOnly, (int)EnrollStatus.Inputan_tidak_lengkap, "");
                }
            }
            #endregion

                const bool IsNewEnroll = true;
            string JamServer = DateTime.Now.ToString("ddMMyyyyHHmmss");
            bool IsNasabah = false;

            var cekKTP = await _profileRepository.GetDataDemografis(enroll.KtpNIK)
                .ConfigureAwait(false);

            var cekNPPCs = await _profileRepository.GetCS()
                .ConfigureAwait(false);
            if (cekNPPCs == null) return (_ErrorMessageConfig.UserCSNotFound, (int)EnrollStatus.Inputan_tidak_lengkap, "");

            if (cekKTP != null)
            {
                var dataNpp = new Tbl_Mapping_Pegawai_KTP();
                #region update demografi
                var logDemografi = new Tbl_DataKTP_Demografis_Log
                {
                    Agama = cekKTP.Agama,
                    Alamat = cekKTP.Alamat,
                    AlamatGoogle = cekKTP.AlamatGoogle,
                    AlamatLengkap = cekKTP.AlamatLengkap,
                    CreatedById = cekKTP.CreatedById,
                    CreatedByUID = cekKTP.CreatedByUID,
                    CreatedByUnitId = cekKTP.CreatedByUnitId,
                    CreatedTime = cekKTP.CreatedTime,
                    Desa = cekKTP.Desa,
                    GolonganDarah = cekKTP.GolonganDarah,
                    JenisKelamin = cekKTP.JenisKelamin,
                    Kecamatan = cekKTP.Kecamatan,
                    Kelurahan = cekKTP.Kelurahan,
                    Kewarganegaraan = cekKTP.Kewarganegaraan,
                    KodePos = cekKTP.KodePos,
                    Kota = cekKTP.Kota,
                    Latitude = cekKTP.Latitude,
                    Longitude = cekKTP.Longitude,
                    MasaBerlaku = cekKTP.MasaBerlaku,
                    Nama = cekKTP.Nama,
                    NIK = cekKTP.NIK,
                    Pekerjaan = cekKTP.Pekerjaan,
                    Provinsi = cekKTP.Provinsi,
                    RT = cekKTP.RT,
                    RW = cekKTP.RW,
                    StatusPerkawinan = cekKTP.StatusPerkawinan,
                    TanggalLahir = cekKTP.TanggalLahir,
                    TempatLahir = cekKTP.TempatLahir,
                    CIF = cekKTP.CIF,
                    CreatedByNpp = cekKTP.CreatedByNpp
                };

                var stringPerubahan = "";

                if (cekKTP.Nama != enroll.KtpNama)
                {
                    stringPerubahan = stringPerubahan + "NAMA : " + cekKTP.Nama + " -> " + enroll.KtpNama + " <br/>";
                    cekKTP.Nama = enroll.KtpNama;
                }

                if (cekKTP.TempatLahir != enroll.KtpTempatLahir)
                {
                    stringPerubahan = stringPerubahan + "TEMPAT LAHIR : " + cekKTP.TempatLahir + " -> " + enroll.KtpTempatLahir + " <br/>";
                    cekKTP.TempatLahir = enroll.KtpTempatLahir;
                }

                if (enroll.KtpTanggalLahir != null)
                {
                    var _ttl = DateTime.ParseExact(enroll.KtpTanggalLahir, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                    if (cekKTP.TanggalLahir != _ttl)
                    {
                        stringPerubahan = stringPerubahan + "TANGGAL LAHIR : " + cekKTP.TanggalLahir + " -> " + enroll.KtpTanggalLahir + " <br/>";
                        cekKTP.TanggalLahir = DateTime.ParseExact(enroll.KtpTanggalLahir, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                    }
                }

                if (cekKTP.JenisKelamin != enroll.KtpJanisKelamin)
                {
                    stringPerubahan = stringPerubahan + "JENIS KELAMIN : " + cekKTP.JenisKelamin + " -> " + enroll.KtpJanisKelamin + " <br/>";
                    cekKTP.JenisKelamin = enroll.KtpJanisKelamin;
                }

                if (cekKTP.GolonganDarah != enroll.KtpGolonganDarah)
                {
                    stringPerubahan = stringPerubahan + "GOLONGAN DARAH : " + cekKTP.GolonganDarah + " -> " + enroll.KtpGolonganDarah + " <br/>";
                    cekKTP.GolonganDarah = enroll.KtpGolonganDarah;
                }

                if (cekKTP.Alamat != enroll.KtpAlamat)
                {
                    stringPerubahan = stringPerubahan + "ALAMAT : " + cekKTP.Alamat + " -> " + enroll.KtpAlamat + " <br/>";
                    cekKTP.Alamat = enroll.KtpAlamat;
                }

                if (cekKTP.RT != enroll.KtpRT)
                {
                    stringPerubahan = stringPerubahan + "RT : " + cekKTP.RT + " -> " + enroll.KtpRT + " <br/>";
                    cekKTP.RT = enroll.KtpRT;
                }

                if (cekKTP.RW != enroll.KtpRW)
                {
                    stringPerubahan = stringPerubahan + "RW : " + cekKTP.RW + " -> " + enroll.KtpRW + " <br/>";
                    cekKTP.RW = enroll.KtpRW;
                }

                if (cekKTP.Kelurahan != enroll.KtpKelurahan)
                {
                    stringPerubahan = stringPerubahan + "KELURAHAN : " + cekKTP.Kelurahan + " -> " + enroll.KtpKelurahan + " <br/>";
                    cekKTP.Kelurahan = enroll.KtpKelurahan;
                }

                if (cekKTP.Kecamatan != enroll.KtpKecamatan)
                {
                    stringPerubahan = stringPerubahan + "KECAMATAN : " + cekKTP.Kecamatan + " -> " + enroll.KtpKecamatan + " <br/>";
                    cekKTP.Kecamatan = enroll.KtpKecamatan;
                }

                if (cekKTP.Kota != enroll.KtpKota)
                {
                    stringPerubahan = stringPerubahan + "KOTA/KABUPATEN : " + cekKTP.Kota + " -> " + enroll.KtpKota + " <br/>";
                    cekKTP.Kota = enroll.KtpKota;
                }

                if (cekKTP.Provinsi != enroll.KtpProvinsi)
                {
                    stringPerubahan = stringPerubahan + "PROVINSI  : " + cekKTP.Provinsi + " -> " + enroll.KtpProvinsi + " <br/>";
                    cekKTP.Provinsi = enroll.KtpProvinsi;
                }

                if (cekKTP.Agama != enroll.KtpAgama)
                {
                    stringPerubahan = stringPerubahan + "AGAMA  : " + cekKTP.Agama + " -> " + enroll.KtpAgama + " <br/>";
                    cekKTP.Agama = enroll.KtpAgama;
                }

                if (cekKTP.StatusPerkawinan != enroll.KtpStatusPerkawinan)
                {
                    stringPerubahan = stringPerubahan + "STATUS PERKAWINAN  : " + cekKTP.Agama + " -> " + enroll.KtpAgama + " <br/>";
                    cekKTP.StatusPerkawinan = enroll.KtpStatusPerkawinan;
                }

                if (cekKTP.Pekerjaan != enroll.KtpPekerjaan)
                {
                    stringPerubahan = stringPerubahan + "PEKERJAAN  : " + cekKTP.Agama + " -> " + enroll.KtpAgama + " <br/>";
                    cekKTP.Pekerjaan = enroll.KtpPekerjaan;
                }

                if (cekKTP.Kewarganegaraan != enroll.KtpKewarganegaraan)
                {
                    stringPerubahan = stringPerubahan + "KEWARGANEGARAAN  : " + cekKTP.Kewarganegaraan + " -> " + enroll.KtpKewarganegaraan + " <br/>";
                    cekKTP.Kewarganegaraan = enroll.KtpKewarganegaraan;
                }
                if (cekKTP.MasaBerlaku != enroll.KtpMasaBerlaku)
                {
                    stringPerubahan = stringPerubahan + "MASA BERLAKU  : " + cekKTP.MasaBerlaku + " -> " + enroll.KtpMasaBerlaku + " <br/>";
                    cekKTP.MasaBerlaku = enroll.KtpMasaBerlaku;
                }
                if (cekKTP.KodePos != enroll.KtpKodePos)
                {
                    stringPerubahan = stringPerubahan + "KODE POS  : " + cekKTP.KodePos + " -> " + enroll.KtpKodePos + " <br/>";
                    cekKTP.KodePos = enroll.KtpKodePos;
                }
                if (cekKTP.CIF == null)
                {
                    var cifData = new ApiSOAResponse();
                    #region Hit SOA And Loggging it
                    if (isHitSOA == true)
                    {
                        cifData = await _cifService.GetSOAByCif(ReqSoa)
                        .ConfigureAwait(false);

                        var _status = 0;
                        if (cifData.cif != null)
                        {
                            _status = 1;
                        }

                        var _log = new Tbl_ThirdPartyLog
                        {
                            FeatureName = "SubmitEnrollmentFingerEncryptedOnlyISOThirdParty",
                            HostUrl = ReqSoa.host,
                            Request = Newtonsoft.Json.JsonConvert.SerializeObject(ReqSoa),
                            Status = _status,
                            Response = Newtonsoft.Json.JsonConvert.SerializeObject(cifData),
                            CreatedDate = System.DateTime.Now,
                            CreatedBy = cekNPPCs.NIK
                        };

                        _ = _enrollmentKTPRepository.CreateThirdPartyLog(_log);
                    }
                    else
                    {
                        try
                        {
                            var res = await _cifService.GetCIF(new NikDto { Nik = enroll.KtpNIK },
                            new NikDtoUrl { baseUrl = ReqSoa.baseUrlNonSoa, endpoint = ReqSoa.UrlEndPointNonSoa });
                            if (res.Data == null)
                            {
                                cifData.cif = null;
                            }
                            else
                            {
                                cifData.cif = res.Data.Cif;
                            };
                        }
                        catch (Exception ex)
                        {
                            cifData.cif = null;
                        }

                    }
                    #endregion

                    if (!String.IsNullOrEmpty(cifData.cif))
                    {
                        cifData.cif = cifData.cif.Trim();
                    }

                    stringPerubahan = stringPerubahan + "CIF  : " + cekKTP.CIF + " -> " + cifData.cif + " <br/>";
                    cekKTP.CIF = cifData.cif;
                }

                cekKTP.UpdatedTime = DateTime.Now;
                cekKTP.UpdatedById = cekNPPCs.PegawaiId;
                cekKTP.UpdatedByUID = null;
                cekKTP.UpdatedByUnitCode = cekNPPCs.Kode_Unit;
                cekKTP.UpdatedByUnitId = int.Parse(cekNPPCs.Unit_Id);
                cekKTP.UpdatedByNpp = cekNPPCs.NIK;
                cekKTP.isEnrollFR = false;

                if (string.IsNullOrWhiteSpace(stringPerubahan))
                {
                    stringPerubahan = stringPerubahan + "Data Demografi tidak ada perubahan" + " <br/>";
                }
                #endregion

                if (isEmployee)
                {
                    var _mappingData = await _enrollmentKTPRepository.MappingNppNikByNik(enroll.KtpNIK);
                    if (_mappingData == null)
                    {
                        dataNpp = new Tbl_Mapping_Pegawai_KTP
                        {
                            CreatedById = cekNPPCs.PegawaiId,
                            NIK = enroll.KtpNIK,
                            Npp = _empData.Npp,
                            CreatedByNpp = cekNPPCs.NIK,
                            CreatedByUID = null,
                            CreatedTime = DateTime.Now,
                            CreatedByUnit = cekNPPCs.Kode_Unit,
                            UpdatedByNpp = cekNPPCs.NIK,
                            UpdatedByUID = null,
                            UpdatedByUnit = cekNPPCs.Kode_Unit,
                            UpdatedTime = DateTime.Now,
                        };
                    }
                    else
                    {
                        dataNpp = _mappingData;
                        dataNpp.UpdatedByNpp = cekNPPCs.NIK;
                        dataNpp.UpdatedByUID = null;
                        dataNpp.UpdatedByUnit = cekNPPCs.Kode_Unit;
                        dataNpp.UpdatedTime = DateTime.Now;
                    }
                }

                var sysPathFolder = await _sysParameterRepository.GetPathFolder("PathFolderData");
                string pathFolder = sysPathFolder.Value;

                var photoKtpLog = new Tbl_DataKTP_Photo_Log();
                var photoKTPData = await _profileRepository.GetPhotoKtp(enroll.KtpNIK)
                        .ConfigureAwait(false);
                var exPhotoKtp = photoKTPData;

                #region update Photo KTP
                if (!string.IsNullOrWhiteSpace(enroll.KtpPhotoKTP))
                {
                    byte[] imgBytes = Convert.FromBase64String(enroll.KtpPhotoKTP);

                    string filePath = "";

                    var systemParameterPath = await _sysParameterRepository.GetPathFolder("FolderPhoto")
                                                .ConfigureAwait(false);
                    string pathFolderFoto = systemParameterPath.Value;

                    string subPathFolderFoto = pathFolder + "/" + pathFolderFoto + "/" + enroll.KtpNIK + "/";

                    string fileName = "Foto_" + enroll.KtpNIK + "_" + JamServer + ".jpg";

                    if (_sftpConfig.IsActive)
                    {
                        using var stream = new MemoryStream(imgBytes);

                        (var fname, var fPath) = await stream.UploadToFTPServer(subPathFolderFoto, fileName, _sftpConfig.Host,
                            _sftpConfig.Username, _sftpConfig.Password, _sftpConfig.RootDirectory, _sftpConfig.Url).ConfigureAwait(false);

                        filePath = fPath;
                    }
                    else
                    {
                        if (!Directory.Exists(subPathFolderFoto))
                        {
                            Directory.CreateDirectory(subPathFolderFoto);
                        }

                        filePath = subPathFolderFoto + fileName;
                        File.WriteAllBytes(filePath, imgBytes);
                    }

                    if (photoKTPData == null)
                    {
                        photoKTPData = new Tbl_DataKTP_Photo
                        {
                            PathFile = filePath,
                            Nik = enroll.KtpNIK,
                            FileName = fileName,
                            IsActive = true,
                            IsDeleted = false,
                            UpdatedById = cekNPPCs.PegawaiId,
                            UpdatedByNpp = cekNPPCs.NIK,
                            UpdatedByUid = null,
                            UpdatedByUnit = cekNPPCs.Kode_Unit,
                            UpdatedTime = DateTime.Now,
                            CreatedById = cekNPPCs.PegawaiId,
                            CreatedByNpp = cekNPPCs.NIK,
                            CreatedByUid = null,
                            CreatedByUnit = cekNPPCs.Kode_Unit,
                            CreatedTime = DateTime.Now,
                        };
                    }
                    else
                    {
                        photoKtpLog.CreatedById = photoKTPData.CreatedById;
                        photoKtpLog.CreatedByNpp = photoKTPData.CreatedByNpp;
                        photoKtpLog.CreatedByUid = photoKTPData.CreatedByUid;
                        photoKtpLog.CreatedByUnit = photoKTPData.CreatedByUnit;
                        photoKtpLog.CreatedTime = photoKTPData.CreatedTime;
                        photoKtpLog.FileName = photoKTPData.FileName;
                        photoKtpLog.Nik = photoKTPData.Nik;
                        photoKtpLog.PathFile = photoKTPData.PathFile;

                        photoKTPData.PathFile = filePath;
                        photoKTPData.Nik = enroll.KtpNIK;
                        photoKTPData.FileName = fileName;
                        photoKTPData.IsActive = true;
                        photoKTPData.IsDeleted = false;
                        photoKTPData.UpdatedById = cekNPPCs.PegawaiId;
                        photoKTPData.UpdatedByNpp = cekNPPCs.NIK;
                        photoKTPData.UpdatedByUid = null;
                        photoKTPData.UpdatedByUnit = cekNPPCs.Kode_Unit;
                        photoKTPData.UpdatedTime = DateTime.Now;
                    }
                }
                else
                {
                    stringPerubahan = stringPerubahan + "Request Payload Photo Tidak Ditemukan" + " <br/>";
                }
                #endregion

                var photoSignatureLog = new Tbl_DataKTP_Signature_Log();
                var photoSignatureData = await _profileRepository.GetPhotoSignature(enroll.KtpNIK)
                        .ConfigureAwait(false);
                var exphotoSignatureData = photoSignatureData;

                #region update signature
                if (!string.IsNullOrWhiteSpace(enroll.KtpSignature))
                {
                    byte[] imgBytes = Convert.FromBase64String(enroll.KtpSignature);

                    string filePath = "";

                    var systemParameterPath = await _sysParameterRepository.GetPathFolder("FolderSignature")
                                                .ConfigureAwait(false);
                    string pathFolderFoto = systemParameterPath.Value;

                    string subPathFolderSignature = pathFolder + "/" + pathFolderFoto + "/" + enroll.KtpNIK + "/";

                    string fileName = "Signature_" + enroll.KtpNIK + "_" + JamServer + ".jpg";

                    if (_sftpConfig.IsActive)
                    {
                        using var stream = new MemoryStream(imgBytes);

                        (var fname, var fPath) = await stream.UploadToFTPServer(subPathFolderSignature, fileName, _sftpConfig.Host,
                            _sftpConfig.Username, _sftpConfig.Password, _sftpConfig.RootDirectory, _sftpConfig.Url).ConfigureAwait(false);

                        filePath = fPath;
                    }
                    else
                    {
                        if (!Directory.Exists(subPathFolderSignature))
                        {
                            Directory.CreateDirectory(subPathFolderSignature);
                        }

                        filePath = subPathFolderSignature + fileName;
                        File.WriteAllBytes(filePath, imgBytes);
                    }

                    if (photoSignatureData == null)
                    {
                        photoSignatureData = new Tbl_DataKTP_Signature();
                        photoSignatureData.UpdatedById = cekNPPCs.PegawaiId;
                        photoSignatureData.UpdatedByNpp = cekNPPCs.NIK;
                        photoSignatureData.UpdatedByUid = null;
                        photoSignatureData.UpdatedTime = DateTime.Now;
                        photoSignatureData.UpdatedByUnit = cekNPPCs.Kode_Unit;
                        photoSignatureData.IsActive = true;
                        photoSignatureData.IsDeleted = false;
                        photoSignatureData.Nik = enroll.KtpNIK;
                        photoSignatureData.FileName = fileName;
                        photoSignatureData.PathFile = filePath;
                        photoSignatureData.CreatedById = cekNPPCs.PegawaiId;
                        photoSignatureData.CreatedByNpp = cekNPPCs.NIK;
                        photoSignatureData.CreatedByUid = null;
                        photoSignatureData.CreatedByUnit = cekNPPCs.Kode_Unit;
                        photoSignatureData.CreatedTime = DateTime.Now;
                    }
                    else
                    {
                        photoSignatureLog.CreatedById = photoSignatureData.CreatedById;
                        photoSignatureLog.CreatedByNpp = photoSignatureData.CreatedByNpp;
                        photoSignatureLog.CreatedByUid = photoSignatureData.CreatedByUid;
                        photoSignatureLog.CreatedByUnit = photoSignatureData.CreatedByUnit;
                        photoSignatureLog.CreatedTime = photoSignatureData.CreatedTime;
                        photoSignatureLog.FileName = photoSignatureData.FileName;
                        photoSignatureLog.Nik = photoSignatureData.Nik;
                        photoSignatureLog.PathFile = photoSignatureData.PathFile;

                        photoSignatureData.UpdatedById = cekNPPCs.PegawaiId;
                        photoSignatureData.UpdatedByNpp = cekNPPCs.NIK;
                        photoSignatureData.UpdatedByUid = null;
                        photoSignatureData.UpdatedByUnit = cekNPPCs.Kode_Unit;
                        photoSignatureData.UpdatedTime = DateTime.Now;
                        photoSignatureData.IsActive = true;
                        photoSignatureData.IsDeleted = false;
                        photoSignatureData.Nik = enroll.KtpNIK;
                        photoSignatureData.FileName = fileName;
                        photoSignatureData.PathFile = filePath;
                    }
                }
                else
                {
                    stringPerubahan = stringPerubahan + "Request Payload Signature Tidak Ditemukan" + " <br/>";
                }
                #endregion

                var photoCamLog = new Tbl_DataKTP_PhotoCam_Log();
                var photoCamData = await _profileRepository.GetPhotoCam(enroll.KtpNIK)
                        .ConfigureAwait(false);
                var exphotoCamData = photoCamData;

                #region update photo cam
                if (!string.IsNullOrWhiteSpace(enroll.KtpPhotoCam))
                {
                    byte[] imgBytes = Convert.FromBase64String(enroll.KtpPhotoCam);

                    string filePath = "";

                    var systemParameterPath = await _sysParameterRepository.GetPathFolder("FolderWebcam")
                                                .ConfigureAwait(false);
                    string pathFolderFoto = systemParameterPath.Value;

                    string SubPathFolderPhotoCam = pathFolder + "/" + pathFolderFoto + "/" + enroll.KtpNIK + "/";

                    string fileName = "PhotoCam_" + enroll.KtpNIK + "_" + JamServer + ".txt";

                    if (_sftpConfig.IsActive)
                    {
                        using var stream = new MemoryStream(imgBytes);

                        (var fname, var fPath) = await stream.UploadToFTPServer(SubPathFolderPhotoCam, fileName, _sftpConfig.Host,
                            _sftpConfig.Username, _sftpConfig.Password, _sftpConfig.RootDirectory, _sftpConfig.Url).ConfigureAwait(false);

                        filePath = fPath;
                    }
                    else
                    {
                        if (!Directory.Exists(SubPathFolderPhotoCam))
                        {
                            Directory.CreateDirectory(SubPathFolderPhotoCam);
                        }

                        filePath = SubPathFolderPhotoCam + fileName;
                        File.WriteAllBytes(filePath, imgBytes);
                    }

                    if (photoCamData == null)
                    {
                        photoCamData = new Tbl_DataKTP_PhotoCam();
                        photoCamData.PathFile = filePath;
                        photoCamData.Nik = enroll.KtpNIK;
                        photoCamData.FileName = fileName;
                        photoCamData.IsActive = true;
                        photoCamData.IsDeleted = false;
                        photoCamData.UpdatedById = cekNPPCs.PegawaiId;
                        photoCamData.UpdatedByNpp = cekNPPCs.NIK;
                        photoCamData.UpdatedByUid = null;
                        photoCamData.UpdatedTime = DateTime.Now;
                        photoCamData.UpdatedByUnit = cekNPPCs.Kode_Unit;
                        photoCamLog.CreatedById = photoCamData.CreatedById;
                        photoCamLog.CreatedByNpp = photoCamData.CreatedByNpp;
                        photoCamLog.CreatedByUid = photoCamData.CreatedByUid;
                        photoCamLog.CreatedByUnit = photoCamData.CreatedByUnit;
                        photoCamLog.CreatedTime = photoCamData.CreatedTime;
                    }
                    else
                    {
                        photoCamLog.CreatedById = photoCamData.CreatedById;
                        photoCamLog.CreatedByNpp = photoCamData.CreatedByNpp;
                        photoCamLog.CreatedByUid = photoCamData.CreatedByUid;
                        photoCamLog.CreatedByUnit = photoCamData.CreatedByUnit;
                        photoCamLog.CreatedTime = photoCamData.CreatedTime;
                        photoCamLog.FileName = photoCamData.FileName;
                        photoCamLog.Nik = photoCamData.Nik;
                        photoCamLog.PathFile = photoCamData.PathFile;

                        photoCamData.PathFile = filePath;
                        photoCamData.Nik = enroll.KtpNIK;
                        photoCamData.FileName = fileName;
                        photoCamData.IsActive = true;
                        photoCamData.IsDeleted = false;
                        photoCamData.UpdatedById = cekNPPCs.PegawaiId;
                        photoCamData.UpdatedByNpp = cekNPPCs.NIK;
                        photoCamData.UpdatedByUid = null;
                        photoCamData.UpdatedByUnit = cekNPPCs.Kode_Unit;
                        photoCamData.UpdatedTime = DateTime.Now;
                    }
                }
                else
                {
                    stringPerubahan = stringPerubahan + "Request Payload Photo Cam Tidak Ditemukan" + " <br/>";
                }
                #endregion

                #region finger
                var exPhotoFinger = new List<Tbl_DataKTP_Finger>();

                var exPhotoFingerEmployee = new List<Tbl_DataKTP_Finger_Employee>();

                var photoFingerLogs = new List<Tbl_DataKTP_Finger_Log>();

                var photoFingers = new List<Tbl_DataKTP_Finger>();

                var photoFingersEmployee = new List<Tbl_DataKTP_Finger_Employee>();

                var photoFingersEmployeeLogs = new List<Tbl_DataKTP_Finger_Employee_Log>();

                var photoFingerExisting = await _profileRepository.GetPhotoFingerExisting(enroll.KtpNIK).ConfigureAwait(false);
                var photoFingerEmployeeExisting = await _profileRepository.GetPhotoFingerEmployeeExisting(enroll.KtpNIK).ConfigureAwait(false);

                foreach (var em in photoFingerEmployeeExisting)
                {
                    photoFingersEmployeeLogs.Add(new Tbl_DataKTP_Finger_Employee_Log
                    {
                        CreatedById = em.CreatedById,
                        CreatedByNpp = em.CreatedByNpp,
                        CreatedByUid = em.CreatedByUid,
                        CreatedByUnit = em.CreatedByUnit,
                        CreatedByUnitId = em.CreatedByUnitId,
                        CreatedTime = em.CreatedTime,
                        //FileJari = em.FileJari,
                        FileName = em.FileName,
                        FileNameISO = em.FileNameISO,
                        Nik = em.Nik,
                        PathFile = em.PathFile,
                        PathFileISO = em.PathFileISO,
                        TypeFinger = em.TypeFinger
                    });
                }

                foreach (var el in photoFingerExisting)
                {
                    photoFingerLogs.Add(new Tbl_DataKTP_Finger_Log
                    {
                        CreatedById = el.CreatedById,
                        CreatedByNpp = el.CreatedByNpp,
                        CreatedByUid = el.CreatedByUid,
                        CreatedByUnit = el.CreatedByUnit,
                        CreatedByUnitId = el.CreatedByUnitId,
                        CreatedTime = el.CreatedTime,
                        //FileJari = el.FileJari,
                        FileJariISO = el.FileJariISO,
                        FileName = el.FileName,
                        FileNameISO = el.FileNameISO,
                        Nik = el.Nik,
                        PathFile = el.PathFile,
                        PathFileISO = el.PathFileISO,
                        TypeFinger = el.TypeFinger
                    });
                }



                if (!string.IsNullOrWhiteSpace(enroll.KtpFingerKanan) && !string.IsNullOrWhiteSpace(enroll.KtpTypeJariKanan) && !string.IsNullOrWhiteSpace(enroll.KtpFingerKananIso))
                {
                    var photoFingerData = await _profileRepository.GetPhotoFinger(enroll.KtpNIK, enroll.KtpTypeJariKanan)
                        .ConfigureAwait(false);

                    var photoFingerDataEmployee = new Tbl_DataKTP_Finger_Employee();

                    if (isEmployee)
                    {
                        photoFingerDataEmployee = await _profileRepository.GetPhotoFingerEmployee(enroll.KtpNIK, enroll.KtpTypeJariKanan)
                        .ConfigureAwait(false);
                    }

                    string imageEncrypted = enroll.KtpFingerKanan.Encrypt(Phrase.FileEncryption);

                    string filePath = "";
                    string filePathIso = "";

                    var systemParameterPath = await _sysParameterRepository.GetPathFolder("FolderFinger")
                                                .ConfigureAwait(false);
                    string pathFolderFoto = systemParameterPath.Value;

                    string subPathFolderPhotoFinger = pathFolder + "/" + pathFolderFoto + "/" + enroll.KtpNIK + "/";

                    string JenisJari = enroll.KtpTypeJariKanan.Replace(" ", "");
                    string fileName = "PhotoFinger_" + JenisJari + "_" + enroll.KtpNIK + "_" + JamServer + ".txt";
                    string fileNameIso = "";

                    if (_sftpConfig.IsActive)
                    {
                        using var stream = new MemoryStream(Encoding.ASCII.GetBytes(imageEncrypted));

                        (var fname, var fPath) = await stream.UploadToFTPServer(subPathFolderPhotoFinger, fileName, _sftpConfig.Host,
                            _sftpConfig.Username, _sftpConfig.Password, _sftpConfig.RootDirectory, _sftpConfig.Url).ConfigureAwait(false);

                        filePath = fPath;
                    }
                    else
                    {
                        if (!Directory.Exists(subPathFolderPhotoFinger))
                        {
                            Directory.CreateDirectory(subPathFolderPhotoFinger);
                        }

                        filePath = subPathFolderPhotoFinger + fileName;
                        File.WriteAllText(filePath, imageEncrypted);
                    }

                    string isoEncrypted = "";

                    if (!string.IsNullOrWhiteSpace(enroll.KtpFingerKananIso))
                    {
                        isoEncrypted = enroll.KtpFingerKananIso.Encrypt(Phrase.FileEncryption);
                        fileNameIso = "PhotoFingerISO_" + JenisJari + "_" + enroll.KtpNIK + "_" + JamServer + ".txt";

                        if (_sftpConfig.IsActive)
                        {
                            using var stream = new MemoryStream(Encoding.ASCII.GetBytes(isoEncrypted));

                            (var fname, var fPath) = await stream.UploadToFTPServer(subPathFolderPhotoFinger, fileNameIso, _sftpConfig.Host,
                                _sftpConfig.Username, _sftpConfig.Password, _sftpConfig.RootDirectory, _sftpConfig.Url).ConfigureAwait(false);

                            filePathIso = fPath;
                        }
                        else
                        {
                            if (!Directory.Exists(subPathFolderPhotoFinger))
                            {
                                Directory.CreateDirectory(subPathFolderPhotoFinger);
                            }

                            filePathIso = subPathFolderPhotoFinger + fileNameIso;
                            File.WriteAllText(filePathIso, isoEncrypted);
                        }
                    }

                    if (photoFingerData == null)
                    {
                        photoFingerData = new Tbl_DataKTP_Finger
                        {
                            CreatedById = cekNPPCs.PegawaiId,
                            IsActive = true,
                            IsDeleted = false,
                            Nik = enroll.KtpNIK,
                            PathFile = filePath,
                            PathFileISO = filePathIso,
                            CreatedByNpp = cekNPPCs.NIK,
                            CreatedByUid = null,
                            CreatedTime = DateTime.Now,
                            CreatedByUnit = cekNPPCs.Kode_Unit,
                            CreatedByUnitId = Int32.Parse(cekNPPCs.Unit_Id),
                            FileName = fileName,
                            FileNameISO = fileNameIso,
                            FileJariISO = isoEncrypted,
                            TypeFinger = enroll.KtpTypeJariKanan
                        };

                        photoFingers.Add(new Tbl_DataKTP_Finger
                        {
                            CreatedById = photoFingerData.CreatedById,
                            IsActive = true,
                            IsDeleted = false,
                            Nik = enroll.KtpNIK,
                            PathFile = filePath,
                            PathFileISO = filePathIso,
                            CreatedByNpp = photoFingerData.CreatedByNpp,
                            CreatedByUid = photoFingerData.CreatedByUid,
                            CreatedTime = photoFingerData.CreatedTime,
                            UpdatedById = cekNPPCs.PegawaiId,
                            UpdatedByNpp = cekNPPCs.NIK,
                            UpdatedByUid = null,
                            UpdatedByUnit = cekNPPCs.Kode_Unit,
                            CreatedByUnitId = photoFingerData.CreatedByUnitId,
                            CreatedByUnit = photoFingerData.CreatedByUnit,
                            //FileJari = photoFingerData.FileJari,
                            FileJariISO = isoEncrypted,
                            UpdatedTime = DateTime.Now,
                            FileName = fileName,
                            FileNameISO = fileNameIso,
                            TypeFinger = enroll.KtpTypeJariKanan
                        });
                    }
                    else
                    {
                        photoFingerLogs.Add(new Tbl_DataKTP_Finger_Log
                        {
                            CreatedById = photoFingerData.CreatedById,
                            CreatedByNpp = photoFingerData.CreatedByNpp,
                            CreatedByUid = photoFingerData.CreatedByUid,
                            CreatedByUnit = photoFingerData.CreatedByUnit,
                            CreatedByUnitId = photoFingerData.CreatedByUnitId,
                            CreatedTime = photoFingerData.CreatedTime,
                            //FileJari = photoFingerData.FileJari,
                            FileJariISO = photoFingerData.FileJariISO,
                            FileName = photoFingerData.FileName,
                            FileNameISO = photoFingerData.FileNameISO,
                            Nik = photoFingerData.Nik,
                            PathFile = photoFingerData.PathFile,
                            PathFileISO = photoFingerData.PathFileISO,
                            TypeFinger = photoFingerData.TypeFinger
                        });

                        exPhotoFinger.Add(photoFingerData);

                        photoFingers.Add(new Tbl_DataKTP_Finger
                        {
                            CreatedById = photoFingerData.CreatedById,
                            IsActive = true,
                            IsDeleted = false,
                            Nik = enroll.KtpNIK,
                            PathFile = filePath,
                            PathFileISO = filePathIso,
                            CreatedByNpp = photoFingerData.CreatedByNpp,
                            CreatedByUid = photoFingerData.CreatedByUid,
                            CreatedTime = photoFingerData.CreatedTime,
                            UpdatedById = cekNPPCs.PegawaiId,
                            UpdatedByNpp = cekNPPCs.NIK,
                            UpdatedByUid = null,
                            UpdatedByUnit = cekNPPCs.Kode_Unit,
                            CreatedByUnitId = Int32.Parse(cekNPPCs.Unit_Id),
                            CreatedByUnit = cekNPPCs.Kode_Unit,
                            //FileJari = enroll.KtpFingerKanan,
                            UpdatedTime = DateTime.Now,
                            FileName = fileName,
                            FileNameISO = fileNameIso,
                            FileJariISO = isoEncrypted,
                            TypeFinger = enroll.KtpTypeJariKanan
                        });
                    }

                    if (isEmployee)
                    {
                        if (photoFingerDataEmployee == null)
                        {
                            photoFingerDataEmployee = new Tbl_DataKTP_Finger_Employee
                            {
                                CreatedById = cekNPPCs.PegawaiId,
                                IsActive = true,
                                IsDeleted = false,
                                Nik = enroll.KtpNIK,
                                PathFile = filePath,//
                                PathFileISO = filePathIso,//
                                CreatedByNpp = cekNPPCs.NIK,
                                CreatedByUid = null,
                                CreatedByUnit = cekNPPCs.Kode_Unit,
                                CreatedByUnitId = Int32.Parse(cekNPPCs.Unit_Id),
                                CreatedTime = DateTime.Now,
                                FileName = fileName,//
                                FileNameISO = fileNameIso,//
                                FileJariISO = isoEncrypted,
                                TypeFinger = enroll.KtpTypeJariKanan
                            };

                            photoFingersEmployee.Add(new Tbl_DataKTP_Finger_Employee
                            {
                                CreatedById = photoFingerDataEmployee.CreatedById,
                                IsActive = true,
                                IsDeleted = false,
                                Nik = enroll.KtpNIK,
                                PathFile = filePath,
                                PathFileISO = filePathIso,
                                CreatedByNpp = photoFingerDataEmployee.CreatedByNpp,
                                CreatedByUid = photoFingerDataEmployee.CreatedByUid,
                                CreatedTime = photoFingerDataEmployee.CreatedTime,
                                UpdatedById = cekNPPCs.PegawaiId,
                                UpdatedByNpp = cekNPPCs.NIK,
                                UpdatedByUid = null,
                                UpdatedByUnit = cekNPPCs.Kode_Unit,
                                CreatedByUnitId = photoFingerDataEmployee.CreatedByUnitId,
                                CreatedByUnit = photoFingerDataEmployee.CreatedByUnit,
                                //FileJari = photoFingerDataEmployee.FileJari,
                                FileJariISO = isoEncrypted,
                                UpdatedTime = DateTime.Now,
                                FileName = fileName,
                                FileNameISO = fileNameIso,
                                TypeFinger = enroll.KtpTypeJariKanan
                            });
                        }
                        else
                        {
                            photoFingersEmployeeLogs.Add(new Tbl_DataKTP_Finger_Employee_Log
                            {
                                CreatedById = photoFingerDataEmployee.CreatedById,
                                CreatedByNpp = photoFingerDataEmployee.CreatedByNpp,
                                CreatedByUid = photoFingerDataEmployee.CreatedByUid,
                                CreatedByUnit = photoFingerDataEmployee.CreatedByUnit,
                                CreatedByUnitId = photoFingerDataEmployee.CreatedByUnitId,
                                CreatedTime = photoFingerDataEmployee.CreatedTime,
                                //FileJari = photoFingerDataEmployee.FileJari,
                                FileJariISO = photoFingerDataEmployee.FileJariISO,
                                FileName = photoFingerDataEmployee.FileName,
                                FileNameISO = photoFingerDataEmployee.FileNameISO,
                                Nik = photoFingerDataEmployee.Nik,
                                PathFile = photoFingerDataEmployee.PathFile,
                                PathFileISO = photoFingerDataEmployee.PathFileISO,
                                TypeFinger = photoFingerDataEmployee.TypeFinger
                            });

                            exPhotoFingerEmployee.Add(photoFingerDataEmployee);

                            photoFingersEmployee.Add(new Tbl_DataKTP_Finger_Employee
                            {
                                CreatedById = photoFingerDataEmployee.CreatedById,
                                IsActive = true,
                                IsDeleted = false,
                                Nik = enroll.KtpNIK,
                                PathFile = filePath,
                                PathFileISO = filePathIso,
                                CreatedByNpp = photoFingerDataEmployee.CreatedByNpp,
                                CreatedByUid = photoFingerDataEmployee.CreatedByUid,
                                CreatedTime = photoFingerDataEmployee.CreatedTime,
                                UpdatedById = cekNPPCs.PegawaiId,
                                UpdatedByNpp = cekNPPCs.NIK,
                                UpdatedByUid = null,
                                UpdatedByUnit = cekNPPCs.Kode_Unit,
                                CreatedByUnitId = photoFingerDataEmployee.CreatedByUnitId,
                                CreatedByUnit = photoFingerDataEmployee.CreatedByUnit,
                                //FileJari = enroll.KtpFingerKanan,
                                FileJariISO = isoEncrypted,
                                UpdatedTime = DateTime.Now,
                                FileName = fileName,
                                FileNameISO = fileNameIso,
                                TypeFinger = enroll.KtpTypeJariKanan
                            });
                        }
                    }
                }
                else
                {
                    #region get data if no payload but we have to replace all data with no data
                    var photoFingerData = await _profileRepository.GetPhotoFinger(enroll.KtpNIK, enroll.KtpTypeJariKanan)
                        .ConfigureAwait(false);
                    
                    
                    if (photoFingerData != null)
                    {
                        exPhotoFinger.Add(photoFingerData);

                        photoFingerLogs.Add(new Tbl_DataKTP_Finger_Log
                        {
                            CreatedById = photoFingerData.CreatedById,
                            CreatedByNpp = photoFingerData.CreatedByNpp,
                            CreatedByUid = photoFingerData.CreatedByUid,
                            CreatedByUnit = photoFingerData.CreatedByUnit,
                            CreatedByUnitId = photoFingerData.CreatedByUnitId,
                            CreatedTime = photoFingerData.CreatedTime,
                            //FileJari = photoFingerData.FileJari,
                            FileName = photoFingerData.FileName,
                            FileNameISO = photoFingerData.FileNameISO,
                            Nik = photoFingerData.Nik,
                            PathFile = photoFingerData.PathFile,
                            TypeFinger = photoFingerData.TypeFinger
                        });
                    }

                    if (isEmployee)
                    {
                        var photoFingerDataEmployee = await _profileRepository.GetPhotoFingerEmployee(enroll.KtpNIK, enroll.KtpTypeJariKanan)
                        .ConfigureAwait(false);
                        if (photoFingerDataEmployee != null)
                        {
                            exPhotoFingerEmployee.Add(photoFingerDataEmployee);

                            photoFingersEmployeeLogs.Add(new Tbl_DataKTP_Finger_Employee_Log
                            {
                                CreatedById = photoFingerDataEmployee.CreatedById,
                                CreatedByNpp = photoFingerDataEmployee.CreatedByNpp,
                                CreatedByUid = photoFingerDataEmployee.CreatedByUid,
                                CreatedByUnit = photoFingerDataEmployee.CreatedByUnit,
                                CreatedByUnitId = photoFingerDataEmployee.CreatedByUnitId,
                                CreatedTime = photoFingerDataEmployee.CreatedTime,
                                //FileJari = photoFingerDataEmployee.FileJari,
                                FileName = photoFingerDataEmployee.FileName,
                                FileNameISO = photoFingerDataEmployee.FileNameISO,
                                Nik = photoFingerDataEmployee.Nik,
                                PathFile = photoFingerDataEmployee.PathFile,
                                PathFileISO = photoFingerDataEmployee.PathFileISO,
                                TypeFinger = photoFingerDataEmployee.TypeFinger
                            });
                        }
                    }
                    #endregion

                    stringPerubahan = stringPerubahan + "Request Payload Finger Kanan Tidak Ditemukan" + " <br/>";
                }

                if (!string.IsNullOrWhiteSpace(enroll.KtpFingerKiri) && !string.IsNullOrWhiteSpace(enroll.KtpTypeJariKiri) && !string.IsNullOrWhiteSpace(enroll.KtpFingerKiriIso))
                {
                    var photoFingerData = await _profileRepository.GetPhotoFinger(enroll.KtpNIK, enroll.KtpTypeJariKiri)
                        .ConfigureAwait(false);

                    var photoFingerDataEmployee = new Tbl_DataKTP_Finger_Employee();

                    if (isEmployee)
                    {
                        photoFingerDataEmployee = await _profileRepository.GetPhotoFingerEmployee(enroll.KtpNIK, enroll.KtpTypeJariKiri)
                        .ConfigureAwait(false);
                    }

                    string imageEncrypted = enroll.KtpFingerKiri.Encrypt(Phrase.FileEncryption);

                    string filePath = "";
                    string filePathIso = "";

                    var systemParameterPath = await _sysParameterRepository.GetPathFolder("FolderFinger")
                                                .ConfigureAwait(false);
                    string pathFolderFoto = systemParameterPath.Value;

                    string subPathFolderPhotoFinger = pathFolder + "/" + pathFolderFoto + "/" + enroll.KtpNIK + "/";

                    string JenisJari = enroll.KtpTypeJariKiri.Replace(" ", "");
                    string fileName = "PhotoFinger_" + JenisJari + "_" + enroll.KtpNIK + "_" + JamServer + ".txt";
                    string fileNameIso = "";

                    if (_sftpConfig.IsActive)
                    {
                        using var stream = new MemoryStream(Encoding.ASCII.GetBytes(imageEncrypted));

                        (var fname, var fPath) = await stream.UploadToFTPServer(subPathFolderPhotoFinger, fileName, _sftpConfig.Host,
                            _sftpConfig.Username, _sftpConfig.Password, _sftpConfig.RootDirectory, _sftpConfig.Url).ConfigureAwait(false);

                        filePath = fPath;
                    }
                    else
                    {
                        if (!Directory.Exists(subPathFolderPhotoFinger))
                        {
                            Directory.CreateDirectory(subPathFolderPhotoFinger);
                        }

                        filePath = subPathFolderPhotoFinger + fileName;
                        File.WriteAllText(filePath, imageEncrypted);
                    }

                    string isoEncrypted = "";

                    if (!string.IsNullOrWhiteSpace(enroll.KtpFingerKiriIso))
                    {
                        isoEncrypted = enroll.KtpFingerKiriIso.Encrypt(Phrase.FileEncryption);
                        fileNameIso = "PhotoFingerISO_" + JenisJari + "_" + enroll.KtpNIK + "_" + JamServer + ".txt";

                        if (_sftpConfig.IsActive)
                        {
                            using var stream = new MemoryStream(Encoding.ASCII.GetBytes(isoEncrypted));

                            (var fname, var fPath) = await stream.UploadToFTPServer(subPathFolderPhotoFinger, fileNameIso, _sftpConfig.Host,
                                _sftpConfig.Username, _sftpConfig.Password, _sftpConfig.RootDirectory, _sftpConfig.Url).ConfigureAwait(false);

                            filePathIso = fPath;
                        }
                        else
                        {
                            if (!Directory.Exists(subPathFolderPhotoFinger))
                            {
                                Directory.CreateDirectory(subPathFolderPhotoFinger);
                            }

                            filePathIso = subPathFolderPhotoFinger + fileNameIso;
                            File.WriteAllText(filePathIso, isoEncrypted);
                        }
                    }

                    if (photoFingerData == null)
                    {
                        photoFingerData = new Tbl_DataKTP_Finger
                        {
                            CreatedById = cekNPPCs.PegawaiId,
                            IsActive = true,
                            IsDeleted = false,
                            Nik = enroll.KtpNIK,
                            PathFile = filePath,
                            PathFileISO = filePathIso,
                            CreatedByNpp = cekNPPCs.NIK,
                            CreatedByUid = null,
                            CreatedByUnit = cekNPPCs.Kode_Unit,
                            CreatedByUnitId = Int32.Parse(cekNPPCs.Unit_Id),
                            CreatedTime = DateTime.Now,
                            FileName = fileName,
                            FileNameISO = fileNameIso,
                            FileJariISO = isoEncrypted,
                            TypeFinger = enroll.KtpTypeJariKiri
                        };

                        photoFingers.Add(new Tbl_DataKTP_Finger
                        {
                            CreatedById = photoFingerData.CreatedById,
                            IsActive = true,
                            IsDeleted = false,
                            Nik = enroll.KtpNIK,
                            PathFile = filePath,
                            PathFileISO = filePathIso,
                            FileNameISO = fileNameIso,
                            CreatedByNpp = photoFingerData.CreatedByNpp,
                            CreatedByUid = photoFingerData.CreatedByUid,
                            CreatedTime = photoFingerData.CreatedTime,
                            UpdatedById = cekNPPCs.PegawaiId,
                            UpdatedByNpp = cekNPPCs.NIK,
                            UpdatedByUid = null,
                            UpdatedByUnit = cekNPPCs.Kode_Unit,
                            CreatedByUnitId = photoFingerData.CreatedByUnitId,
                            CreatedByUnit = photoFingerData.CreatedByUnit,
                            //FileJari = photoFingerData.FileJari,
                            FileJariISO = isoEncrypted,
                            UpdatedTime = DateTime.Now,
                            FileName = fileName,
                            TypeFinger = enroll.KtpTypeJariKiri
                        });
                    }
                    else
                    {
                        photoFingerLogs.Add(new Tbl_DataKTP_Finger_Log
                        {
                            CreatedById = photoFingerData.CreatedById,
                            CreatedByNpp = photoFingerData.CreatedByNpp,
                            CreatedByUid = photoFingerData.CreatedByUid,
                            CreatedByUnit = photoFingerData.CreatedByUnit,
                            CreatedByUnitId = photoFingerData.CreatedByUnitId,
                            CreatedTime = photoFingerData.CreatedTime,
                            //FileJari = photoFingerData.FileJari,
                            FileJariISO = photoFingerData.FileJariISO,
                            FileName = photoFingerData.FileName,
                            PathFileISO = photoFingerData.PathFileISO,
                            FileNameISO = photoFingerData.FileNameISO,
                            Nik = photoFingerData.Nik,
                            PathFile = photoFingerData.PathFile,
                            TypeFinger = photoFingerData.TypeFinger
                        });

                        exPhotoFinger.Add(photoFingerData);

                        photoFingers.Add(new Tbl_DataKTP_Finger
                        {
                            CreatedById = photoFingerData.CreatedById,
                            IsActive = true,
                            IsDeleted = false,
                            Nik = enroll.KtpNIK,
                            PathFile = filePath,
                            PathFileISO = filePathIso,
                            CreatedByNpp = photoFingerData.CreatedByNpp,
                            CreatedByUid = photoFingerData.CreatedByUid,
                            CreatedTime = photoFingerData.CreatedTime,
                            UpdatedById = cekNPPCs.PegawaiId,
                            UpdatedByNpp = cekNPPCs.NIK,
                            UpdatedByUid = null,
                            UpdatedByUnit = cekNPPCs.Kode_Unit,
                            CreatedByUnitId = photoFingerData.CreatedByUnitId,
                            CreatedByUnit = photoFingerData.CreatedByUnit,
                            //FileJari = enroll.KtpFingerKiri,
                            FileJariISO = isoEncrypted,
                            UpdatedTime = DateTime.Now,
                            FileName = fileName,
                            FileNameISO = fileNameIso,
                            TypeFinger = enroll.KtpTypeJariKiri
                        });
                    }

                    if (isEmployee)
                    {
                        if (photoFingerDataEmployee == null)
                        {
                            photoFingerDataEmployee = new Tbl_DataKTP_Finger_Employee
                            {
                                CreatedById = cekNPPCs.PegawaiId,
                                IsActive = true,
                                IsDeleted = false,
                                Nik = enroll.KtpNIK,
                                PathFile = filePath,//
                                PathFileISO = filePathIso,//
                                CreatedByNpp = cekNPPCs.NIK,
                                CreatedByUid = null,
                                CreatedTime = DateTime.Now,
                                CreatedByUnit = cekNPPCs.Kode_Unit,
                                CreatedByUnitId = Int32.Parse(cekNPPCs.Unit_Id),
                                FileName = fileName,//
                                FileNameISO = fileNameIso,//
                                FileJariISO = isoEncrypted,
                                //FileJari = enroll.KtpFingerKiri,
                                TypeFinger = enroll.KtpTypeJariKiri
                            };

                            photoFingersEmployee.Add(new Tbl_DataKTP_Finger_Employee
                            {
                                CreatedById = photoFingerDataEmployee.CreatedById,
                                IsActive = true,
                                IsDeleted = false,
                                Nik = enroll.KtpNIK,
                                PathFile = filePath,
                                PathFileISO = filePathIso,
                                CreatedByNpp = photoFingerDataEmployee.CreatedByNpp,
                                CreatedByUid = photoFingerDataEmployee.CreatedByUid,
                                CreatedTime = photoFingerDataEmployee.CreatedTime,
                                UpdatedById = cekNPPCs.PegawaiId,
                                UpdatedByNpp = cekNPPCs.NIK,
                                UpdatedByUid = null,
                                UpdatedByUnit = cekNPPCs.Kode_Unit,
                                CreatedByUnitId = photoFingerDataEmployee.CreatedByUnitId,
                                CreatedByUnit = photoFingerDataEmployee.CreatedByUnit,
                                //FileJari = photoFingerDataEmployee.FileJari,
                                FileJariISO = photoFingerDataEmployee.FileJariISO,
                                UpdatedTime = DateTime.Now,
                                FileName = fileName,
                                FileNameISO = fileNameIso,
                                TypeFinger = enroll.KtpTypeJariKiri
                            });
                        }
                        else
                        {
                            photoFingersEmployeeLogs.Add(new Tbl_DataKTP_Finger_Employee_Log
                            {
                                CreatedById = photoFingerDataEmployee.CreatedById,
                                CreatedByNpp = photoFingerDataEmployee.CreatedByNpp,
                                CreatedByUid = photoFingerDataEmployee.CreatedByUid,
                                CreatedByUnit = photoFingerDataEmployee.CreatedByUnit,
                                CreatedByUnitId = photoFingerDataEmployee.CreatedByUnitId,
                                CreatedTime = photoFingerDataEmployee.CreatedTime,
                                //FileJari = photoFingerDataEmployee.FileJari,
                                FileJariISO = photoFingerDataEmployee.FileJariISO,
                                FileName = photoFingerDataEmployee.FileName,
                                FileNameISO = photoFingerDataEmployee.FileNameISO,
                                Nik = photoFingerDataEmployee.Nik,
                                PathFile = photoFingerDataEmployee.PathFile,
                                PathFileISO = photoFingerDataEmployee.PathFileISO,
                                TypeFinger = photoFingerDataEmployee.TypeFinger
                            });

                            exPhotoFingerEmployee.Add(photoFingerDataEmployee);

                            photoFingersEmployee.Add(new Tbl_DataKTP_Finger_Employee
                            {
                                CreatedById = photoFingerDataEmployee.CreatedById,
                                IsActive = true,
                                IsDeleted = false,
                                Nik = enroll.KtpNIK,
                                PathFile = filePath,
                                PathFileISO = filePathIso,
                                CreatedByNpp = photoFingerDataEmployee.CreatedByNpp,
                                CreatedByUid = photoFingerDataEmployee.CreatedByUid,
                                CreatedTime = photoFingerDataEmployee.CreatedTime,
                                UpdatedById = cekNPPCs.PegawaiId,
                                UpdatedByNpp = cekNPPCs.NIK,
                                UpdatedByUid = null,
                                UpdatedByUnit = cekNPPCs.Kode_Unit,
                                CreatedByUnitId = photoFingerDataEmployee.CreatedByUnitId,
                                CreatedByUnit = photoFingerDataEmployee.CreatedByUnit,
                                //FileJari = enroll.KtpFingerKiri,
                                FileJariISO = isoEncrypted,
                                UpdatedTime = DateTime.Now,
                                FileName = fileName,
                                FileNameISO = fileNameIso,
                                TypeFinger = enroll.KtpTypeJariKiri
                            });
                        }
                    }
                }
                else
                {
                    #region get data if no payload but we have to replace all data with no data
                    var photoFingerData = await _profileRepository.GetPhotoFinger(enroll.KtpNIK, enroll.KtpTypeJariKiri)
                        .ConfigureAwait(false);
                    if (photoFingerData != null)
                    {
                        exPhotoFinger.Add(photoFingerData);

                        photoFingerLogs.Add(new Tbl_DataKTP_Finger_Log
                        {
                            CreatedById = photoFingerData.CreatedById,
                            CreatedByNpp = photoFingerData.CreatedByNpp,
                            CreatedByUid = photoFingerData.CreatedByUid,
                            CreatedByUnit = photoFingerData.CreatedByUnit,
                            CreatedByUnitId = photoFingerData.CreatedByUnitId,
                            CreatedTime = photoFingerData.CreatedTime,
                            //FileJari = photoFingerData.FileJari,
                            FileName = photoFingerData.FileName,
                            FileNameISO = photoFingerData.FileNameISO,
                            Nik = photoFingerData.Nik,
                            PathFile = photoFingerData.PathFile,
                            PathFileISO = photoFingerData.PathFileISO,
                            TypeFinger = photoFingerData.TypeFinger
                        });
                    }
                    if (isEmployee)
                    {
                        var photoFingerDataEmployee = await _profileRepository.GetPhotoFingerEmployee(enroll.KtpNIK, enroll.KtpTypeJariKiri)
                        .ConfigureAwait(false);
                        if (photoFingerDataEmployee != null)
                        {
                            exPhotoFingerEmployee.Add(photoFingerDataEmployee);

                            photoFingersEmployeeLogs.Add(new Tbl_DataKTP_Finger_Employee_Log
                            {
                                CreatedById = photoFingerDataEmployee.CreatedById,
                                CreatedByNpp = photoFingerDataEmployee.CreatedByNpp,
                                CreatedByUid = photoFingerDataEmployee.CreatedByUid,
                                CreatedByUnit = photoFingerDataEmployee.CreatedByUnit,
                                CreatedByUnitId = photoFingerDataEmployee.CreatedByUnitId,
                                CreatedTime = photoFingerDataEmployee.CreatedTime,
                                //FileJari = photoFingerDataEmployee.FileJari,
                                FileName = photoFingerDataEmployee.FileName,
                                FileNameISO = photoFingerDataEmployee.FileNameISO,
                                Nik = photoFingerDataEmployee.Nik,
                                PathFile = photoFingerDataEmployee.PathFile,
                                PathFileISO = photoFingerDataEmployee.PathFileISO,
                                TypeFinger = photoFingerDataEmployee.TypeFinger
                            });
                        }
                    }
                    #endregion

                    stringPerubahan = stringPerubahan + "Request Payload Finger Kanan Tidak Ditemukan" + " <br/>";
                }
                #endregion
                var dataReaderLog = new Tbl_MasterAlatReaderLog();

                var (status, msg) = await _enrollmentKTPRepository.InsertEnrollFlow2(cekKTP, logDemografi, photoKTPData, photoKtpLog, photoSignatureData,
                    photoSignatureLog, photoCamData, photoCamLog, photoFingers, photoFingersEmployee, photoFingerLogs, photoFingersEmployeeLogs, exPhotoKtp, exphotoSignatureData, exphotoCamData, photoFingerExisting, photoFingerEmployeeExisting, dataReaderLog, dataNpp);

                //var (status, msg) = await _enrollmentKTPRepository.InsertEnrollFlow3(dataKTPTemp, logDemografi, photoKTPData, photoKtpLog, photoSignatureData,
                //    photoSignatureLog, photoCamData, photoCamLog, photoFingers, photoFingerLogs, exPhotoKtp, exphotoSignatureData, exphotoCamData, photoFingerExisting, dataNpp, dataReaderLog, photoFingersEmployee, photoFingerEmployeeExisting, photoFingersEmployeeLogs);
                await _alatReaderRepository.InsertLogEnrollThirdParty(new Tbl_Enrollment_ThirdParty_Log
                {
                    NIK = enroll.KtpNIK,
                    AppsChannel = AppsChannel,
                    SubmitDate = DateTime.Now

                });

                if (status)
                {
                    return (_SuccessMessageConfig.NasabahSuksesUpdate, (int)ServiceResponseStatus.SUKSES, cekKTP.CIF);
                }
                else
                {
                    return (_ErrorMessageConfig.DemografiGagalEnroll, (int)ServiceResponseStatus.ERROR, msg + " " + stringPerubahan);
                }
            }

            if (IsNewEnroll)
            {
                if (enroll?.KtpNIK == null) return (_ErrorMessageConfig.InputTidakLengkap, (int)EnrollStatus.Inputan_tidak_lengkap, "");

                var cifData = new ApiSOAResponse();
                #region Hit SOA And Loggging it
                if (isHitSOA == true)
                {
                    cifData = await _cifService.GetSOAByCif(ReqSoa)
                    .ConfigureAwait(false);

                    var status = 0;
                    if (cifData.cif != null)
                    {
                        status = 1;
                    }

                    var _log = new Tbl_ThirdPartyLog
                    {
                        FeatureName = "SubmitEnrollmentFingerEncryptedOnlyISO",
                        HostUrl = ReqSoa.host,
                        Request = Newtonsoft.Json.JsonConvert.SerializeObject(ReqSoa),
                        Status = status,
                        Response = Newtonsoft.Json.JsonConvert.SerializeObject(cifData),
                        CreatedDate = System.DateTime.Now,
                        CreatedBy = cekNPPCs.NIK
                    };

                    _ = _enrollmentKTPRepository.CreateThirdPartyLog(_log);
                }
                else
                {
                    try
                    {
                        var res = await _cifService.GetCIF(new NikDto { Nik = enroll.KtpNIK },
                        new NikDtoUrl { baseUrl = ReqSoa.baseUrlNonSoa, endpoint = ReqSoa.UrlEndPointNonSoa });
                        if (res.Data == null)
                        {
                            cifData.cif = null;
                        }
                        else
                        {
                            cifData.cif = res.Data.Cif;
                        };
                    }
                    catch (Exception ex)
                    {
                        cifData.cif = null;
                    }

                }
                #endregion

                if (!String.IsNullOrEmpty(cifData.cif))
                {
                    IsNasabah = true;
                    cifData.cif = cifData.cif.Trim();
                }

                var sysPathFolder = await _sysParameterRepository.GetPathFolder("PathFolderData");
                string pathFolder = sysPathFolder.Value;

                var dataDemografis = new Tbl_DataKTP_Demografis();

                var dataDemografisLog = new Tbl_DataKTP_Demografis_Log();

                var dataNpp = new Tbl_Mapping_Pegawai_KTP();

                if (cekKTP != null)
                {
                    dataDemografisLog = new Tbl_DataKTP_Demografis_Log
                    {
                        Agama = cekKTP.Agama,
                        Alamat = cekKTP.Alamat,
                        AlamatGoogle = cekKTP.AlamatGoogle,
                        AlamatLengkap = cekKTP.AlamatLengkap,
                        CreatedById = cekKTP.CreatedById,
                        CreatedByUID = cekKTP.CreatedByUID,
                        CreatedByUnitId = cekKTP.CreatedByUnitId,
                        CreatedTime = cekKTP.CreatedTime,
                        Desa = cekKTP.Desa,
                        GolonganDarah = cekKTP.GolonganDarah,
                        JenisKelamin = cekKTP.JenisKelamin,
                        Kecamatan = cekKTP.Kecamatan,
                        Kelurahan = cekKTP.Kelurahan,
                        Kewarganegaraan = cekKTP.Kewarganegaraan,
                        KodePos = cekKTP.KodePos,
                        Kota = cekKTP.Kota,
                        Latitude = cekKTP.Latitude,
                        Longitude = cekKTP.Longitude,
                        MasaBerlaku = cekKTP.MasaBerlaku,
                        Nama = cekKTP.Nama,
                        NIK = cekKTP.NIK,
                        Pekerjaan = cekKTP.Pekerjaan,
                        Provinsi = cekKTP.Provinsi,
                        RT = cekKTP.RT,
                        RW = cekKTP.RW,
                        StatusPerkawinan = cekKTP.StatusPerkawinan,
                        TanggalLahir = cekKTP.TanggalLahir,
                        TempatLahir = cekKTP.TempatLahir
                    };

                    cekKTP.Agama = enroll.KtpAgama;
                    cekKTP.Alamat = enroll.KtpAlamat;
                    cekKTP.AlamatGoogle = enroll.KtpAlamatConvertLatlong;
                    cekKTP.AlamatLengkap = enroll.KtpAlamatConvertLengkap;
                    if (string.IsNullOrWhiteSpace(cifData.cif))
                    {
                        cekKTP.CIF = cifData.cif;
                    }
                    cekKTP.GolonganDarah = enroll.KtpGolonganDarah;
                    cekKTP.JenisKelamin = enroll.KtpJanisKelamin;
                    cekKTP.Kecamatan = enroll.KtpKecamatan;
                    cekKTP.Kelurahan = enroll.KtpKelurahan;
                    cekKTP.Kewarganegaraan = enroll.KtpKewarganegaraan;
                    cekKTP.KodePos = enroll.KtpKodePos;
                    cekKTP.Kota = enroll.KtpKota;
                    cekKTP.Latitude = enroll.KtpLatitude;
                    cekKTP.Longitude = enroll.KtpLongitude;
                    cekKTP.MasaBerlaku = enroll.KtpMasaBerlaku;
                    cekKTP.Nama = enroll.KtpNama;
                    cekKTP.NIK = enroll.KtpNIK;
                    cekKTP.Pekerjaan = enroll.KtpPekerjaan;
                    cekKTP.Provinsi = enroll.KtpProvinsi;
                    cekKTP.RT = enroll.KtpRT;
                    cekKTP.RW = enroll.KtpRW;
                    cekKTP.StatusPerkawinan = enroll.KtpStatusPerkawinan;
                    cekKTP.TanggalLahir = string.IsNullOrWhiteSpace(enroll.KtpTanggalLahir) ? null : DateTime.ParseExact(enroll.KtpTanggalLahir, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                    cekKTP.TempatLahir = enroll.KtpTempatLahir;
                    cekKTP.UpdatedById = cekNPPCs.PegawaiId;
                    cekKTP.UpdatedByNpp = cekNPPCs.NIK;
                    cekKTP.UpdatedByUID = null;
                    cekKTP.UpdatedTime = DateTime.Now;
                    cekKTP.UpdatedByUnitCode = cekNPPCs.Kode_Unit;
                    cekKTP.UpdatedByUnitId = int.Parse(cekNPPCs.Unit_Id);
                    cekKTP.isEnrollFR = false;
                }
                else
                {
                    dataDemografis = new Tbl_DataKTP_Demografis
                    {
                        Agama = enroll.KtpAgama,
                        Alamat = enroll.KtpAlamat,
                        AlamatGoogle = enroll.KtpAlamatConvertLatlong,
                        AlamatLengkap = enroll.KtpAlamatConvertLengkap,
                        CIF = (cifData.cif),
                        NIK = enroll.KtpNIK,
                        CreatedById = cekNPPCs.PegawaiId,
                        IsActive = true,
                        IsDeleted = false,
                        JenisKelamin = enroll.KtpJanisKelamin,
                        Kecamatan = enroll.KtpKecamatan,
                        Kelurahan = enroll.KtpKelurahan,
                        CreatedByUID = null,
                        CreatedTime = DateTime.Now,
                        GolonganDarah = enroll.KtpGolonganDarah,
                        Kewarganegaraan = enroll.KtpKewarganegaraan,
                        KodePos = enroll.KtpKodePos,
                        Kota = enroll.KtpKota,
                        Latitude = enroll.KtpLatitude,
                        Longitude = enroll.KtpLongitude,
                        MasaBerlaku = enroll.KtpMasaBerlaku,
                        Nama = enroll.KtpNama,
                        Pekerjaan = enroll.KtpPekerjaan,
                        Provinsi = enroll.KtpProvinsi,
                        RT = enroll.KtpRT,
                        RW = enroll.KtpRW,
                        StatusPerkawinan = enroll.KtpStatusPerkawinan,
                        TanggalLahir = string.IsNullOrWhiteSpace(enroll.KtpTanggalLahir) ? null : DateTime.ParseExact(enroll.KtpTanggalLahir, "dd-MM-yyyy", CultureInfo.InvariantCulture),
                        TempatLahir = enroll.KtpTempatLahir,
                        CreatedByNpp = cekNPPCs.NIK,
                        CreatedByUnitCode = cekNPPCs.Kode_Unit,
                        CreatedByUnitId = int.Parse(cekNPPCs.Unit_Id),
                        IsVerified = false,
                        IsNasabahTemp = string.IsNullOrWhiteSpace(cifData.cif),
                        isEnrollFR = false
                    };

                    if (isEmployee)
                    {
                        dataNpp = new Tbl_Mapping_Pegawai_KTP
                        {
                            CreatedById = cekNPPCs.PegawaiId,
                            NIK = enroll.KtpNIK,
                            Npp = _empData.Npp,
                            CreatedByNpp = cekNPPCs.NIK,
                            CreatedByUID = null,
                            CreatedTime = DateTime.Now
                        };
                    }
                }

                var exPhotoKtp = new Tbl_DataKTP_Photo();

                var photoKtpLog = new Tbl_DataKTP_Photo_Log();

                var photoKtp = new Tbl_DataKTP_Photo();

                if (!string.IsNullOrWhiteSpace(enroll.KtpPhotoKTP))
                {
                    var photoKTPData = await _profileRepository.GetPhotoKtp(enroll.KtpNIK)
                        .ConfigureAwait(false);

                    byte[] imgBytes = Convert.FromBase64String(enroll.KtpPhotoKTP);

                    string filePath = "";

                    var systemParameterPath = await _sysParameterRepository.GetPathFolder("FolderPhoto")
                                                .ConfigureAwait(false);
                    string pathFolderFoto = systemParameterPath.Value;

                    string subPathFolderFoto = pathFolder + "/" + pathFolderFoto + "/" + enroll.KtpNIK + "/";

                    string fileName = "Foto_" + enroll.KtpNIK + "_" + JamServer + ".jpg";

                    if (_sftpConfig.IsActive)
                    {
                        using var stream = new MemoryStream(imgBytes);

                        (var fname, var fPath) = await stream.UploadToFTPServer(subPathFolderFoto, fileName, _sftpConfig.Host,
                            _sftpConfig.Username, _sftpConfig.Password, _sftpConfig.RootDirectory, _sftpConfig.Url).ConfigureAwait(false);

                        filePath = fPath;
                    }
                    else
                    {

                        if (!Directory.Exists(subPathFolderFoto))
                        {
                            Directory.CreateDirectory(subPathFolderFoto);
                        }

                        filePath = subPathFolderFoto + fileName;
                        File.WriteAllBytes(filePath, imgBytes);
                    }

                    if (photoKTPData != null)
                    {
                        photoKtpLog.CreatedById = photoKTPData.CreatedById;
                        photoKtpLog.CreatedByNpp = photoKTPData.CreatedByNpp;
                        photoKtpLog.CreatedByUid = photoKTPData.CreatedByUid;
                        photoKtpLog.CreatedByUnit = photoKTPData.CreatedByUnit;
                        photoKtpLog.CreatedTime = photoKTPData.CreatedTime;
                        photoKtpLog.FileName = photoKTPData.FileName;
                        photoKtpLog.Nik = photoKTPData.Nik;
                        photoKtpLog.PathFile = photoKTPData.PathFile;

                        exPhotoKtp = photoKTPData;
                    }

                    photoKtp = new Tbl_DataKTP_Photo
                    {
                        PathFile = filePath,
                        Nik = enroll.KtpNIK,
                        FileName = fileName,
                        IsActive = true,
                        IsDeleted = false,
                        CreatedById = cekNPPCs.PegawaiId,
                        CreatedByNpp = cekNPPCs.NIK,
                        CreatedByUid = null,
                        CreatedByUnit = cekNPPCs.Kode_Unit,
                        CreatedTime = DateTime.Now,
                    };
                }

                var exPhotoSignature = new Tbl_DataKTP_Signature();

                var photoSignatureLog = new Tbl_DataKTP_Signature_Log();

                var photoSignature = new Tbl_DataKTP_Signature();

                if (!string.IsNullOrWhiteSpace(enroll.KtpSignature))
                {
                    var photoSignatureData = await _profileRepository.GetPhotoSignature(enroll.KtpNIK)
                        .ConfigureAwait(false);

                    byte[] imgBytes = Convert.FromBase64String(enroll.KtpSignature);

                    string filePath = "";

                    var systemParameterPath = await _sysParameterRepository.GetPathFolder("FolderSignature")
                                                .ConfigureAwait(false);
                    string pathFolderFoto = systemParameterPath.Value;

                    string subPathFolderSignature = pathFolder + "/" + pathFolderFoto + "/" + enroll.KtpNIK + "/";
                    //string subPathFolderSignature = pathFolder + pathFolderFoto + enroll.KtpNIK;

                    string fileName = "Signature_" + enroll.KtpNIK + "_" + JamServer + ".jpg";

                    if (_sftpConfig.IsActive)
                    {
                        using var stream = new MemoryStream(imgBytes);

                        (var fname, var fPath) = await stream.UploadToFTPServer(subPathFolderSignature, fileName, _sftpConfig.Host,
                            _sftpConfig.Username, _sftpConfig.Password, _sftpConfig.RootDirectory, _sftpConfig.Url).ConfigureAwait(false);

                        filePath = fPath;
                    }
                    else
                    {
                        if (!Directory.Exists(subPathFolderSignature))
                        {
                            Directory.CreateDirectory(subPathFolderSignature);
                        }

                        filePath = subPathFolderSignature + fileName;
                        File.WriteAllBytes(filePath, imgBytes);
                    }

                    if (photoSignatureData != null)
                    {
                        photoSignatureLog.CreatedById = photoSignatureData.CreatedById;
                        photoSignatureLog.CreatedByNpp = photoSignatureData.CreatedByNpp;
                        photoSignatureLog.CreatedByUid = photoSignatureData.CreatedByUid;
                        photoSignatureLog.CreatedByUnit = photoSignatureData.CreatedByUnit;
                        photoSignatureLog.CreatedTime = photoSignatureData.CreatedTime;
                        photoSignatureLog.FileName = photoSignatureData.FileName;
                        photoSignatureLog.Nik = photoSignatureData.Nik;
                        photoSignatureLog.PathFile = photoSignatureData.PathFile;

                        exPhotoSignature = photoSignatureData;
                    }

                    photoSignature = new Tbl_DataKTP_Signature
                    {
                        CreatedById = cekNPPCs.PegawaiId,
                        CreatedByNpp = cekNPPCs.NIK,
                        IsActive = true,
                        IsDeleted = false,
                        Nik = enroll.KtpNIK,
                        FileName = fileName,
                        CreatedByUid = null,
                        CreatedByUnit = cekNPPCs.Kode_Unit,
                        CreatedTime = DateTime.Now,
                        PathFile = filePath
                    };
                }

                var exPhotoCam = new Tbl_DataKTP_PhotoCam();

                var photoCamLog = new Tbl_DataKTP_PhotoCam_Log();

                var photoCam = new Tbl_DataKTP_PhotoCam();

                if (!string.IsNullOrWhiteSpace(enroll.KtpPhotoCam))
                {
                    var photoCamData = await _profileRepository.GetPhotoCam(enroll.KtpNIK)
                        .ConfigureAwait(false);

                    byte[] imgBytes = Convert.FromBase64String(enroll.KtpPhotoCam);

                    string filePath = "";

                    var systemParameterPath = await _sysParameterRepository.GetPathFolder("FolderWebcam")
                                                .ConfigureAwait(false);
                    string pathFolderFoto = systemParameterPath.Value;

                    string SubPathFolderPhotoCam = pathFolder + "/" + pathFolderFoto + "/" + enroll.KtpNIK + "/";
                    //string SubPathFolderPhotoCam = pathFolder + pathFolderFoto + enroll.KtpNIK;

                    string fileName = "PhotoCam_" + enroll.KtpNIK + "_" + JamServer + ".txt";

                    if (_sftpConfig.IsActive)
                    {
                        using var stream = new MemoryStream(imgBytes);

                        (var fname, var fPath) = await stream.UploadToFTPServer(SubPathFolderPhotoCam, fileName, _sftpConfig.Host,
                            _sftpConfig.Username, _sftpConfig.Password, _sftpConfig.RootDirectory, _sftpConfig.Url).ConfigureAwait(false);

                        filePath = fPath;
                    }
                    else
                    {
                        if (!Directory.Exists(SubPathFolderPhotoCam))
                        {
                            Directory.CreateDirectory(SubPathFolderPhotoCam);
                        }

                        filePath = SubPathFolderPhotoCam + fileName;
                        File.WriteAllBytes(filePath, imgBytes);
                    }

                    if (photoCamData != null)
                    {
                        photoCamLog.CreatedById = photoCamData.CreatedById;
                        photoCamLog.CreatedByNpp = photoCamData.CreatedByNpp;
                        photoCamLog.CreatedByUid = photoCamData.CreatedByUid;
                        photoCamLog.CreatedByUnit = photoCamData.CreatedByUnit;
                        photoCamLog.CreatedTime = photoCamData.CreatedTime;
                        photoCamLog.FileName = photoCamData.FileName;
                        photoCamLog.Nik = photoCamData.Nik;
                        photoCamLog.PathFile = photoCamData.PathFile;

                        exPhotoCam = photoCamData;
                    }

                    photoCam = new Tbl_DataKTP_PhotoCam
                    {
                        PathFile = filePath,//
                        Nik = enroll.KtpNIK,
                        FileName = fileName,//
                        CreatedTime = DateTime.Now,
                        CreatedById = cekNPPCs.PegawaiId,
                        IsActive = true,
                        IsDeleted = false,
                        CreatedByNpp = cekNPPCs.NIK,
                        CreatedByUid = null,
                        CreatedByUnit = cekNPPCs.Kode_Unit
                    };
                }

                var exPhotoFinger = new List<Tbl_DataKTP_Finger>();

                var exPhotoFingerEmployee = new List<Tbl_DataKTP_Finger_Employee>();

                var photoFingerLogs = new List<Tbl_DataKTP_Finger_Log>();

                var photoFingers = new List<Tbl_DataKTP_Finger>();

                var photoFingersEmployee = new List<Tbl_DataKTP_Finger_Employee>();

                //if (!string.IsNullOrWhiteSpace(enroll.KtpFingerKanan))
                if ((!string.IsNullOrWhiteSpace(enroll.KtpFingerKanan)) && (!string.IsNullOrWhiteSpace(enroll.KtpFingerKananIso)) && (!string.IsNullOrWhiteSpace(enroll.KtpTypeJariKanan)))
                {

                    var photoFingerData = await _profileRepository.GetPhotoFinger(enroll.KtpNIK, enroll.KtpTypeJariKanan)
                        .ConfigureAwait(false);

                    var photoFingerDataEmployee = new Tbl_DataKTP_Finger_Employee();

                    if (isEmployee)
                    {
                        photoFingerDataEmployee = await _profileRepository.GetPhotoFingerEmployee(enroll.KtpNIK, enroll.KtpTypeJariKanan)
                        .ConfigureAwait(false);
                    }

                    string imageEncrypted = enroll.KtpFingerKanan.Encrypt(Phrase.FileEncryption);

                    string filePath = "";
                    string filePathIso = "";

                    var systemParameterPath = await _sysParameterRepository.GetPathFolder("FolderFinger")
                                                .ConfigureAwait(false);
                    string pathFolderFoto = systemParameterPath.Value;

                    string subPathFolderPhotoFinger = pathFolder + "/" + pathFolderFoto + "/" + enroll.KtpNIK + "/";

                    string JenisJari = enroll.KtpTypeJariKanan.Replace(" ", "");
                    string fileName = "PhotoFinger_" + JenisJari + "_" + enroll.KtpNIK + "_" + JamServer + ".txt";
                    string fileNameIso = "";

                    if (_sftpConfig.IsActive)
                    {
                        using var stream = new MemoryStream(Encoding.ASCII.GetBytes(imageEncrypted));

                        (var fname, var fPath) = await stream.UploadToFTPServer(subPathFolderPhotoFinger, fileName, _sftpConfig.Host,
                            _sftpConfig.Username, _sftpConfig.Password, _sftpConfig.RootDirectory, _sftpConfig.Url).ConfigureAwait(false);

                        filePath = fPath;
                    }
                    else
                    {
                        if (!Directory.Exists(subPathFolderPhotoFinger))
                        {
                            Directory.CreateDirectory(subPathFolderPhotoFinger);
                        }

                        filePath = subPathFolderPhotoFinger + fileName;
                        File.WriteAllText(filePath, imageEncrypted);
                    }

                    string isoEncrypted = "";
                    if (!string.IsNullOrWhiteSpace(enroll.KtpFingerKananIso))
                    {
                        isoEncrypted = enroll.KtpFingerKananIso.Encrypt(Phrase.FileEncryption);
                        fileNameIso = "PhotoFingerISO_" + JenisJari + "_" + enroll.KtpNIK + "_" + JamServer + ".txt";

                        if (_sftpConfig.IsActive)
                        {
                            using var stream = new MemoryStream(Encoding.ASCII.GetBytes(isoEncrypted));

                            (var fname, var fPath) = await stream.UploadToFTPServer(subPathFolderPhotoFinger, fileNameIso, _sftpConfig.Host,
                                _sftpConfig.Username, _sftpConfig.Password, _sftpConfig.RootDirectory, _sftpConfig.Url).ConfigureAwait(false);

                            filePathIso = fPath;
                        }
                        else
                        {
                            if (!Directory.Exists(subPathFolderPhotoFinger))
                            {
                                Directory.CreateDirectory(subPathFolderPhotoFinger);
                            }

                            filePathIso = subPathFolderPhotoFinger + fileNameIso;
                            File.WriteAllText(filePathIso, isoEncrypted);
                        }
                    }

                    if (photoFingerData != null)
                    {
                        photoFingerLogs.Add(new Tbl_DataKTP_Finger_Log
                        {
                            CreatedById = photoFingerData.CreatedById,
                            CreatedByNpp = photoFingerData.CreatedByNpp,
                            CreatedByUid = photoFingerData.CreatedByUid,
                            CreatedByUnit = photoFingerData.CreatedByUnit,
                            CreatedByUnitId = photoFingerData.CreatedByUnitId,
                            CreatedTime = photoFingerData.CreatedTime,
                            //FileJari = photoFingerData.FileJari,
                            FileJariISO = photoFingerData.FileJariISO,
                            FileName = photoFingerData.FileName,
                            FileNameISO = photoFingerData.FileNameISO,
                            Nik = photoFingerData.Nik,
                            PathFile = photoFingerData.PathFile,
                            PathFileISO = photoFingerData.PathFileISO,
                            TypeFinger = photoFingerData.TypeFinger
                        });

                        exPhotoFinger.Add(photoFingerData);
                    }

                    photoFingers.Add(new Tbl_DataKTP_Finger
                    {
                        CreatedById = cekNPPCs.PegawaiId,
                        IsActive = true,
                        IsDeleted = false,
                        Nik = enroll.KtpNIK,
                        PathFile = filePath,//
                        PathFileISO = filePathIso,//
                        CreatedByNpp = cekNPPCs.NIK,
                        CreatedByUid = null,
                        CreatedByUnitId = Int32.Parse(cekNPPCs.Unit_Id),
                        CreatedByUnit = cekNPPCs.Kode_Unit,
                        CreatedTime = DateTime.Now,
                        FileName = fileName,//
                        FileNameISO = fileNameIso,//
                        FileJariISO = isoEncrypted,
                        TypeFinger = enroll.KtpTypeJariKanan
                    });

                    if (isEmployee)
                    {
                        photoFingersEmployee.Add(new Tbl_DataKTP_Finger_Employee
                        {
                            CreatedById = cekNPPCs.PegawaiId,
                            IsActive = true,
                            IsDeleted = false,
                            Nik = enroll.KtpNIK,
                            PathFile = filePath,//
                            PathFileISO = filePathIso,//
                            CreatedByNpp = cekNPPCs.NIK,
                            CreatedByUid = null,
                            CreatedByUnitId = Int32.Parse(cekNPPCs.Unit_Id),
                            CreatedByUnit = cekNPPCs.Kode_Unit,
                            CreatedTime = DateTime.Now,
                            FileName = fileName,//
                            FileNameISO = fileNameIso,//
                            FileJariISO = isoEncrypted,
                            TypeFinger = enroll.KtpTypeJariKanan
                        });
                    }
                }

                //if (!string.IsNullOrWhiteSpace(enroll.KtpFingerKiri))
                if ((!string.IsNullOrWhiteSpace(enroll.KtpFingerKiri))&&(!string.IsNullOrWhiteSpace(enroll.KtpFingerKiriIso)) && (!string.IsNullOrWhiteSpace(enroll.KtpTypeJariKiri)))
                {
                    var photoFingerData = await _profileRepository.GetPhotoFinger(enroll.KtpNIK, enroll.KtpTypeJariKiri)
                        .ConfigureAwait(false);

                    var photoFingerDataEmployee = new Tbl_DataKTP_Finger_Employee();

                    if (isEmployee)
                    {
                        photoFingerDataEmployee = await _profileRepository.GetPhotoFingerEmployee(enroll.KtpNIK, enroll.KtpTypeJariKiri)
                        .ConfigureAwait(false);
                    }

                    string imageEncrypted = enroll.KtpFingerKiri.Encrypt(Phrase.FileEncryption);

                    string filePath = "";
                    string filePathIso = "";

                    var systemParameterPath = await _sysParameterRepository.GetPathFolder("FolderFinger")
                                                .ConfigureAwait(false);
                    string pathFolderFoto = systemParameterPath.Value;

                    string subPathFolderPhotoFinger = pathFolder + "/" + pathFolderFoto + "/" + enroll.KtpNIK + "/";

                    string JenisJari = enroll.KtpTypeJariKiri.Replace(" ", "");
                    string fileName = "PhotoFinger_" + JenisJari + "_" + enroll.KtpNIK + "_" + JamServer + ".txt";
                    string fileNameIso = "";

                    if (_sftpConfig.IsActive)
                    {
                        using var stream = new MemoryStream(Encoding.ASCII.GetBytes(imageEncrypted));

                        (var fname, var fPath) = await stream.UploadToFTPServer(subPathFolderPhotoFinger, fileName, _sftpConfig.Host,
                            _sftpConfig.Username, _sftpConfig.Password, _sftpConfig.RootDirectory, _sftpConfig.Url).ConfigureAwait(false);

                        filePath = fPath;
                    }
                    else
                    {
                        if (!Directory.Exists(subPathFolderPhotoFinger))
                        {
                            Directory.CreateDirectory(subPathFolderPhotoFinger);
                        }

                        filePath = subPathFolderPhotoFinger + fileName;
                        File.WriteAllText(filePath, imageEncrypted);
                    }
                    string isoEncrypted = "";

                    if (!string.IsNullOrWhiteSpace(enroll.KtpFingerKiriIso))
                    {
                        isoEncrypted = enroll.KtpFingerKiriIso.Encrypt(Phrase.FileEncryption);
                        fileNameIso = "PhotoFingerISO_" + JenisJari + "_" + enroll.KtpNIK + "_" + JamServer + ".txt";

                        if (_sftpConfig.IsActive)
                        {
                            using var stream = new MemoryStream(Encoding.ASCII.GetBytes(isoEncrypted));

                            (var fname, var fPath) = await stream.UploadToFTPServer(subPathFolderPhotoFinger, fileNameIso, _sftpConfig.Host,
                                _sftpConfig.Username, _sftpConfig.Password, _sftpConfig.RootDirectory, _sftpConfig.Url).ConfigureAwait(false);

                            filePathIso = fPath;
                        }
                        else
                        {
                            if (!Directory.Exists(subPathFolderPhotoFinger))
                            {
                                Directory.CreateDirectory(subPathFolderPhotoFinger);
                            }

                            filePathIso = subPathFolderPhotoFinger + fileNameIso;
                            File.WriteAllText(filePathIso, isoEncrypted);
                        }
                    }

                    if (photoFingerData != null)
                    {
                        photoFingerLogs.Add(new Tbl_DataKTP_Finger_Log
                        {
                            CreatedById = photoFingerData.CreatedById,
                            CreatedByNpp = photoFingerData.CreatedByNpp,
                            CreatedByUid = photoFingerData.CreatedByUid,
                            CreatedByUnit = photoFingerData.CreatedByUnit,
                            CreatedByUnitId = photoFingerData.CreatedByUnitId,
                            CreatedTime = photoFingerData.CreatedTime,
                            //FileJari = photoFingerData.FileJari,
                            FileJariISO = photoFingerData.FileJariISO,
                            FileName = photoFingerData.FileName,
                            FileNameISO = photoFingerData.FileNameISO,
                            Nik = photoFingerData.Nik,
                            PathFile = photoFingerData.PathFile,
                            PathFileISO = photoFingerData.PathFileISO,
                            TypeFinger = photoFingerData.TypeFinger
                        });

                        exPhotoFinger.Add(photoFingerData);
                    }

                    photoFingers.Add(new Tbl_DataKTP_Finger
                    {
                        CreatedById = cekNPPCs.PegawaiId,
                        IsActive = true,
                        IsDeleted = false,
                        Nik = enroll.KtpNIK,
                        PathFile = filePath,//
                        PathFileISO = filePathIso,//
                        CreatedByNpp = cekNPPCs.NIK,
                        CreatedByUid = null,
                        CreatedByUnitId = Int32.Parse(cekNPPCs.Unit_Id),
                        CreatedByUnit = cekNPPCs.Kode_Unit,
                        CreatedTime = DateTime.Now,
                        FileName = fileName,//
                        FileNameISO = fileNameIso,//
                        FileJariISO = isoEncrypted,//
                        TypeFinger = enroll.KtpTypeJariKiri
                    });

                    if (isEmployee)
                    {
                        photoFingersEmployee.Add(new Tbl_DataKTP_Finger_Employee
                        {
                            CreatedById = cekNPPCs.PegawaiId,
                            IsActive = true,
                            IsDeleted = false,
                            Nik = enroll.KtpNIK,
                            PathFile = filePath,//
                            PathFileISO = filePathIso,//
                            CreatedByNpp = cekNPPCs.NIK,
                            CreatedByUid = null,
                            CreatedByUnitId = Int32.Parse(cekNPPCs.Unit_Id),
                            CreatedByUnit = cekNPPCs.Kode_Unit,
                            CreatedTime = DateTime.Now,
                            FileName = fileName,//
                            FileNameISO = fileNameIso,//
                            FileJariISO = isoEncrypted,//
                            TypeFinger = enroll.KtpTypeJariKiri
                        });
                    }
                }

                // to do: alat reader log
                //var dataReader = await _dataReaderRepository.GetDatareaderUid(enroll.UID).ConfigureAwait(false);

                var dataReaderLog = new Tbl_MasterAlatReaderLog();

                //if (dataReader != null)
                //{
                //    dataReaderLog = new Tbl_MasterAlatReaderLog
                //    {
                //        CreatedBy_Id = cekNPPCs.PegawaiId,
                //        CreatedTime = DateTime.Now,
                //        IsActive = true,
                //        IsDeleted = false,
                //        Nik = enroll.KtpNIK,
                //        PegawaiId = cekNPPCs.PegawaiId,
                //        Serial_Number = dataReader.SN_Unit,
                //        Type = "Enroll",
                //        Uid = enroll.UID
                //    };
                //}

                _enrollmentKTPRepository.InsertEnrollFlow(dataDemografis, dataDemografisLog, photoKtp, photoKtpLog, photoSignature, photoSignatureLog,
                    photoCam, photoCamLog, photoFingers, photoFingerLogs, exPhotoKtp, exPhotoSignature, exPhotoCam, exPhotoFinger, dataNpp, dataReaderLog, photoFingersEmployee, exPhotoFingerEmployee);
                
                await _alatReaderRepository.CreateLogActivity2(new Tbl_MasterAlatReaderLogActvity
                {
                    CreatedBy_Id = cekNPPCs.PegawaiId,
                    IsActive = true,
                    IsDeleted = false,
                    KodeUnit = cekNPPCs.Kode_Unit,
                    LastIP = remoteIpAddress,
                    NIK = enroll.KtpNIK,
                    CreatedTime = DateTime.Now,
                    NppPegawai = cekNPPCs.NIK,
                    PegawaiId = cekNPPCs.PegawaiId,
                    Type = "Enroll By ThirdParty",
                    UID = null,
                    UnitId = int.Parse(cekNPPCs.Unit_Id)
                });

                await _alatReaderRepository.InsertLogEnrollThirdParty(new Tbl_Enrollment_ThirdParty_Log
                {
                    NIK = enroll.KtpNIK,
                    AppsChannel = AppsChannel,
                    SubmitDate = DateTime.Now,
                    JournalID = enroll.JournalID
                });
            }

            if (IsNasabah)
            {
                return (_SuccessMessageConfig.NasabahSuksesEnrollNasabah, (int)EnrollStatus.Nasabah_Berhasil_di_enroll, cekKTP?.CIF);
            }
            else
            {
                return (_SuccessMessageConfig.NasabahSuksesEnrollNonNasabah, (int)EnrollStatus.Nasabah_Berhasil_di_enroll, cekKTP?.CIF);
            }
        }

        public async Task<(string msg, int code, string cif)> ReSubmitEnrollmentFingerEncryptedOnlyISO(bool isHitSOA, ApiSOA ReqSoa, EnrollKTP enroll, int Id, string npp,
            string unitCode, int unitId)
        {
            #region check data is employee or not
            bool isEmployee = false;
            var _empData = await _enrollmentKTPRepository.IsEmployee(enroll.KtpNIK);
            if (_empData != null)
            {
                isEmployee = true;
            }
            #endregion

            #region check UID
            var dataReader = await _dataReaderRepository.GetDatareaderUid(enroll.UID).ConfigureAwait(false);

            var dataReaderLog = new Tbl_MasterAlatReaderLog();
            var dataReaderActivityLog = new Tbl_MasterAlatReaderLogActvity();
            var dataNpp = new Tbl_Mapping_Pegawai_KTP();

            if (dataReader != null)
            {
                dataReaderLog = new Tbl_MasterAlatReaderLog
                {
                    CreatedBy_Id = Id,
                    CreatedTime = DateTime.Now,
                    IsActive = true,
                    IsDeleted = false,
                    Nik = enroll.KtpNIK,
                    PegawaiId = Id,
                    Serial_Number = dataReader.SN_Unit,
                    Type = "Updates Enroll",
                    Uid = enroll.UID
                };

                dataReaderActivityLog = new Tbl_MasterAlatReaderLogActvity
                {
                    CreatedBy_Id = Id,
                    IsActive = true,
                    IsDeleted = false,
                    KodeUnit = unitCode,
                    LastIP = enroll.IpAddress,
                    NIK = enroll.KtpNIK,
                    CreatedTime = DateTime.Now,
                    NppPegawai = npp,
                    PegawaiId = Id,
                    Type = "Updates Enroll",
                    UID = enroll.UID,
                    UnitId = unitId
                };
            }
            #endregion

            string JamServer = DateTime.Now.ToString("ddMMyyyyHHmmss");

            if (enroll.KtpTanggalLahir != null)
            {
                enroll.KtpTanggalLahir = enroll.KtpTanggalLahir.Replace(".", "");
                if (enroll.KtpTanggalLahir.Contains("/"))
                {
                    enroll.KtpTanggalLahir = enroll.KtpTanggalLahir.Replace("/", "-");
                }
                string[] arrayTanggl = enroll.KtpTanggalLahir.Split("-");
                string hari = "";
                string bulan = "";
                string tahun = "";

                if (arrayTanggl != null)
                {
                    if (arrayTanggl[0] != null)
                    {
                        if (arrayTanggl[0].Length != 2)
                        {
                            hari = "0" + arrayTanggl[0];

                        }
                        else
                        {
                            hari = arrayTanggl[0];

                        }
                    }

                    if (arrayTanggl[1] != null)
                    {
                        if (arrayTanggl[1].Length != 2)
                        {
                            bulan = "0" + arrayTanggl[1];

                        }
                        else
                        {
                            bulan = arrayTanggl[1];

                        }
                    }

                    if (arrayTanggl[2] != null)
                    {
                        tahun = arrayTanggl[2];
                    }
                }

                enroll.KtpTanggalLahir = hari + "-" + bulan + "-" + tahun;
            }

            var cekKTP = await _profileRepository.GetDataDemografis(enroll.KtpNIK)
                .ConfigureAwait(false);

            var cekKTPTemp = await _profileRepository.GetDataDemografisTempOnProgress(enroll.KtpNIK)
                .ConfigureAwait(false); 

            #region update data ktp
            if (cekKTP != null)
            {
                    #region update demografi
                    var logDemografi = new Tbl_DataKTP_Demografis_Log
                    {
                        Agama = cekKTP.Agama,
                        Alamat = cekKTP.Alamat,
                        AlamatGoogle = cekKTP.AlamatGoogle,
                        AlamatLengkap = cekKTP.AlamatLengkap,
                        CreatedById = cekKTP.CreatedById,
                        CreatedByUID = cekKTP.CreatedByUID,
                        CreatedByUnitId = cekKTP.CreatedByUnitId,
                        CreatedTime = cekKTP.CreatedTime,
                        Desa = cekKTP.Desa,
                        GolonganDarah = cekKTP.GolonganDarah,
                        JenisKelamin = cekKTP.JenisKelamin,
                        Kecamatan = cekKTP.Kecamatan,
                        Kelurahan = cekKTP.Kelurahan,
                        Kewarganegaraan = cekKTP.Kewarganegaraan,
                        KodePos = cekKTP.KodePos,
                        Kota = cekKTP.Kota,
                        Latitude = cekKTP.Latitude,
                        Longitude = cekKTP.Longitude,
                        MasaBerlaku = cekKTP.MasaBerlaku,
                        Nama = cekKTP.Nama,
                        NIK = cekKTP.NIK,
                        Pekerjaan = cekKTP.Pekerjaan,
                        Provinsi = cekKTP.Provinsi,
                        RT = cekKTP.RT,
                        RW = cekKTP.RW,
                        StatusPerkawinan = cekKTP.StatusPerkawinan,
                        TanggalLahir = cekKTP.TanggalLahir,
                        TempatLahir = cekKTP.TempatLahir,
                        CIF = cekKTP.CIF,
                        CreatedByNpp = cekKTP.CreatedByNpp
                    };



                    var stringPerubahan = "";

                    if (cekKTP.Nama != enroll.KtpNama)
                    {
                        stringPerubahan = stringPerubahan + "NAMA : " + cekKTP.Nama + " -> " + enroll.KtpNama + " <br/>";
                        cekKTP.Nama = enroll.KtpNama;
                    }

                    if (cekKTP.TempatLahir != enroll.KtpTempatLahir)
                    {
                        stringPerubahan = stringPerubahan + "TEMPAT LAHIR : " + cekKTP.TempatLahir + " -> " + enroll.KtpTempatLahir + " <br/>";
                        cekKTP.TempatLahir = enroll.KtpTempatLahir;
                    }

                    if (enroll.KtpTanggalLahir != null)
                    {
                        var _ttl = DateTime.ParseExact(enroll.KtpTanggalLahir, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                        if (cekKTP.TanggalLahir != _ttl)
                        {
                            stringPerubahan = stringPerubahan + "TANGGAL LAHIR : " + cekKTP.TanggalLahir + " -> " + enroll.KtpTanggalLahir + " <br/>";
                            cekKTP.TanggalLahir = DateTime.ParseExact(enroll.KtpTanggalLahir, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                        }
                    }

                    if (cekKTP.JenisKelamin != enroll.KtpJanisKelamin)
                    {
                        stringPerubahan = stringPerubahan + "JENIS KELAMIN : " + cekKTP.JenisKelamin + " -> " + enroll.KtpJanisKelamin + " <br/>";
                        cekKTP.JenisKelamin = enroll.KtpJanisKelamin;
                    }

                    if (cekKTP.GolonganDarah != enroll.KtpGolonganDarah)
                    {
                        stringPerubahan = stringPerubahan + "GOLONGAN DARAH : " + cekKTP.GolonganDarah + " -> " + enroll.KtpGolonganDarah + " <br/>";
                        cekKTP.GolonganDarah = enroll.KtpGolonganDarah;
                    }

                    if (cekKTP.Alamat != enroll.KtpAlamat)
                    {
                        stringPerubahan = stringPerubahan + "ALAMAT : " + cekKTP.Alamat + " -> " + enroll.KtpAlamat + " <br/>";
                        cekKTP.Alamat = enroll.KtpAlamat;
                    }

                    if (cekKTP.RT != enroll.KtpRT)
                    {
                        stringPerubahan = stringPerubahan + "RT : " + cekKTP.RT + " -> " + enroll.KtpRT + " <br/>";
                        cekKTP.RT = enroll.KtpRT;
                    }

                    if (cekKTP.RW != enroll.KtpRW)
                    {
                        stringPerubahan = stringPerubahan + "RW : " + cekKTP.RW + " -> " + enroll.KtpRW + " <br/>";
                        cekKTP.RW = enroll.KtpRW;
                    }

                    if (cekKTP.Kelurahan != enroll.KtpKelurahan)
                    {
                        stringPerubahan = stringPerubahan + "KELURAHAN : " + cekKTP.Kelurahan + " -> " + enroll.KtpKelurahan + " <br/>";
                        cekKTP.Kelurahan = enroll.KtpKelurahan;
                    }

                    if (cekKTP.Kecamatan != enroll.KtpKecamatan)
                    {
                        stringPerubahan = stringPerubahan + "KECAMATAN : " + cekKTP.Kecamatan + " -> " + enroll.KtpKecamatan + " <br/>";
                        cekKTP.Kecamatan = enroll.KtpKecamatan;
                    }

                    if (cekKTP.Kota != enroll.KtpKota)
                    {
                        stringPerubahan = stringPerubahan + "KOTA/KABUPATEN : " + cekKTP.Kota + " -> " + enroll.KtpKota + " <br/>";
                        cekKTP.Kota = enroll.KtpKota;
                    }

                    if (cekKTP.Provinsi != enroll.KtpProvinsi)
                    {
                        stringPerubahan = stringPerubahan + "PROVINSI  : " + cekKTP.Provinsi + " -> " + enroll.KtpProvinsi + " <br/>";
                        cekKTP.Provinsi = enroll.KtpProvinsi;
                    }

                    if (cekKTP.Agama != enroll.KtpAgama)
                    {
                        stringPerubahan = stringPerubahan + "AGAMA  : " + cekKTP.Agama + " -> " + enroll.KtpAgama + " <br/>";
                        cekKTP.Agama = enroll.KtpAgama;
                    }

                    if (cekKTP.StatusPerkawinan != enroll.KtpStatusPerkawinan)
                    {
                        stringPerubahan = stringPerubahan + "STATUS PERKAWINAN  : " + cekKTP.Agama + " -> " + enroll.KtpAgama + " <br/>";
                        cekKTP.StatusPerkawinan = enroll.KtpStatusPerkawinan;
                    }

                    if (cekKTP.Pekerjaan != enroll.KtpPekerjaan)
                    {
                        stringPerubahan = stringPerubahan + "PEKERJAAN  : " + cekKTP.Agama + " -> " + enroll.KtpAgama + " <br/>";
                        cekKTP.Pekerjaan = enroll.KtpPekerjaan;
                    }

                    if (cekKTP.Kewarganegaraan != enroll.KtpKewarganegaraan)
                    {
                        stringPerubahan = stringPerubahan + "KEWARGANEGARAAN  : " + cekKTP.Kewarganegaraan + " -> " + enroll.KtpKewarganegaraan + " <br/>";
                        cekKTP.Kewarganegaraan = enroll.KtpKewarganegaraan;
                    }
                    if (cekKTP.MasaBerlaku != enroll.KtpMasaBerlaku)
                    {
                        stringPerubahan = stringPerubahan + "MASA BERLAKU  : " + cekKTP.MasaBerlaku + " -> " + enroll.KtpMasaBerlaku + " <br/>";
                        cekKTP.MasaBerlaku = enroll.KtpMasaBerlaku;
                    }
                    if (cekKTP.CIF == null)
                    {
                        var cifData = new ApiSOAResponse();
                        #region Hit SOA And Loggging it
                        if (isHitSOA == true)
                        {
                            cifData = await _cifService.GetSOAByCif(ReqSoa)
                            .ConfigureAwait(false);

                            var _status = 0;
                            if (cifData.cif != null)
                            {
                                _status = 1;
                            }

                            var _log = new Tbl_ThirdPartyLog
                            {
                                FeatureName = "ReSubmitEnrollmentFingerEncryptedOnlyISO",
                                HostUrl = ReqSoa.host,
                                Request = Newtonsoft.Json.JsonConvert.SerializeObject(ReqSoa),
                                Status = _status,
                                Response = Newtonsoft.Json.JsonConvert.SerializeObject(cifData),
                                CreatedDate = System.DateTime.Now,
                                CreatedBy = npp
                            };

                            _ = _enrollmentKTPRepository.CreateThirdPartyLog(_log);
                        }
                        else
                        {
                            try
                            {
                                var res = await _cifService.GetCIF(new NikDto { Nik = enroll.KtpNIK },
                                new NikDtoUrl { baseUrl = ReqSoa.baseUrlNonSoa, endpoint = ReqSoa.UrlEndPointNonSoa });
                                if (res.Data == null)
                                {
                                    cifData.cif = null;
                                }
                                else
                                {
                                    cifData.cif = res.Data.Cif;
                                };
                            }
                            catch (Exception ex)
                            {
                                cifData.cif = null;
                            }

                        }
                        #endregion

                        if (!String.IsNullOrEmpty(cifData.cif))
                        {
                            cifData.cif = cifData.cif.Trim();
                        }

                        stringPerubahan = stringPerubahan + "CIF  : " + cekKTP.CIF + " -> " + cifData.cif + " <br/>";
                        cekKTP.CIF = cifData.cif;
                    }

                    cekKTP.UpdatedTime = DateTime.Now;
                    cekKTP.UpdatedById = Id;
                    cekKTP.UpdatedByUID = enroll.UID;
                    cekKTP.UpdatedByUnitCode = unitCode;
                    cekKTP.UpdatedByUnitId = unitId;
                    cekKTP.UpdatedByNpp = npp;
                    cekKTP.isEnrollFR = false;

                    if (string.IsNullOrWhiteSpace(stringPerubahan))
                    {
                        stringPerubahan = stringPerubahan + "Data Demografi tidak ada perubahan" + " <br/>";
                    }
                    #endregion

                    if (isEmployee)
                    {
                        var _mappingData = await _enrollmentKTPRepository.MappingNppNikByNik(enroll.KtpNIK);
                        if (_mappingData == null)
                        {
                            dataNpp = new Tbl_Mapping_Pegawai_KTP
                            {
                                CreatedById = Id,
                                NIK = enroll.KtpNIK,
                                Npp = _empData.Npp,
                                CreatedByNpp = npp,
                                CreatedByUID = enroll.UID,
                                CreatedTime = DateTime.Now,
                                CreatedByUnit = unitCode,
                                UpdatedByNpp = npp,
                                UpdatedByUID = enroll.UID,
                                UpdatedByUnit = unitCode,
                                UpdatedTime = DateTime.Now,
                            };
                        }
                        else
                        {
                            dataNpp = _mappingData;
                            dataNpp.UpdatedByNpp = npp;
                            dataNpp.UpdatedByUID = enroll.UID;
                            dataNpp.UpdatedByUnit = unitCode;
                            dataNpp.UpdatedTime = DateTime.Now;
                        }
                    }

                    var sysPathFolder = await _sysParameterRepository.GetPathFolder("PathFolderData");
                    string pathFolder = sysPathFolder.Value;

                    var photoKtpLog = new Tbl_DataKTP_Photo_Log();
                    var photoKTPData = await _profileRepository.GetPhotoKtp(enroll.KtpNIK)
                            .ConfigureAwait(false);
                    var exPhotoKtp = photoKTPData;

                    #region update Photo KTP
                    if (!string.IsNullOrWhiteSpace(enroll.KtpPhotoKTP))
                    {
                        byte[] imgBytes = Convert.FromBase64String(enroll.KtpPhotoKTP);

                        string filePath = "";

                        var systemParameterPath = await _sysParameterRepository.GetPathFolder("FolderPhoto")
                                                    .ConfigureAwait(false);
                        string pathFolderFoto = systemParameterPath.Value;

                        string subPathFolderFoto = pathFolder + "/" + pathFolderFoto + "/" + enroll.KtpNIK + "/";

                        string fileName = "Foto_" + enroll.KtpNIK + "_" + JamServer + ".jpg";

                        if (_sftpConfig.IsActive)
                        {
                            using var stream = new MemoryStream(imgBytes);

                            (var fname, var fPath) = await stream.UploadToFTPServer(subPathFolderFoto, fileName, _sftpConfig.Host,
                                _sftpConfig.Username, _sftpConfig.Password, _sftpConfig.RootDirectory, _sftpConfig.Url).ConfigureAwait(false);

                            filePath = fPath;
                        }
                        else
                        {
                            if (!Directory.Exists(subPathFolderFoto))
                            {
                                Directory.CreateDirectory(subPathFolderFoto);
                            }

                            filePath = subPathFolderFoto + fileName;
                            File.WriteAllBytes(filePath, imgBytes);
                        }

                        if (photoKTPData == null)
                        {
                            photoKTPData = new Tbl_DataKTP_Photo
                            {
                                PathFile = filePath,
                                Nik = enroll.KtpNIK,
                                FileName = fileName,
                                IsActive = true,
                                IsDeleted = false,
                                UpdatedById = Id,
                                UpdatedByNpp = npp,
                                UpdatedByUid = enroll.UID,
                                UpdatedByUnit = unitCode,
                                UpdatedTime = DateTime.Now,
                                CreatedById = Id,
                                CreatedByNpp = npp,
                                CreatedByUid = enroll.UID,
                                CreatedByUnit = unitCode,
                                CreatedTime = DateTime.Now,
                            };
                        }
                        else
                        {
                            photoKtpLog.CreatedById = photoKTPData.CreatedById;
                            photoKtpLog.CreatedByNpp = photoKTPData.CreatedByNpp;
                            photoKtpLog.CreatedByUid = photoKTPData.CreatedByUid;
                            photoKtpLog.CreatedByUnit = photoKTPData.CreatedByUnit;
                            photoKtpLog.CreatedTime = photoKTPData.CreatedTime;
                            photoKtpLog.FileName = photoKTPData.FileName;
                            photoKtpLog.Nik = photoKTPData.Nik;
                            photoKtpLog.PathFile = photoKTPData.PathFile;

                            photoKTPData.PathFile = filePath;
                            photoKTPData.Nik = enroll.KtpNIK;
                            photoKTPData.FileName = fileName;
                            photoKTPData.IsActive = true;
                            photoKTPData.IsDeleted = false;
                            photoKTPData.UpdatedById = Id;
                            photoKTPData.UpdatedByNpp = npp;
                            photoKTPData.UpdatedByUid = enroll.UID;
                            photoKTPData.UpdatedByUnit = unitCode;
                            photoKTPData.UpdatedTime = DateTime.Now;
                        }
                    }
                    else
                    {
                        stringPerubahan = stringPerubahan + "Request Payload Photo Tidak Ditemukan" + " <br/>";
                    }
                    #endregion

                    var photoSignatureLog = new Tbl_DataKTP_Signature_Log();
                    var photoSignatureData = await _profileRepository.GetPhotoSignature(enroll.KtpNIK)
                            .ConfigureAwait(false);
                    var exphotoSignatureData = photoSignatureData;

                    #region update signature
                    if (!string.IsNullOrWhiteSpace(enroll.KtpSignature))
                    {
                        //byte[] imgBytes = Convert.FromBase64String(enroll.KtpSignature);

                        byte[] imgBytes = null;
                        bool isSkipSignature = false;

                        try
                        {
                            imgBytes = Convert.FromBase64String(enroll.KtpSignature);
                        }
                        catch (Exception e)
                        {
                            isSkipSignature = true;

                            //logerror

                            var err = new Tbl_LogError
                            {
                                InnerException = "Error Convert enroll.KtpSignature to Base64.",
                                CreatedAt = DateTime.Now,
                                Message = e.ToString(),
                                Payload = "Signature Payload: " + enroll.KtpSignature + " , " + "NIK: " + enroll.KtpNIK,
                                Source = "Ekr.Api.DataFingerISO",
                                StackTrace = "",
                                SystemName = "Data Enrollment"
                            };

                            _errorLogRepository.CreateErrorLog(err);
                        }
                        
                        if (isSkipSignature == false)
                        {
                            string filePath = "";

                            var systemParameterPath = await _sysParameterRepository.GetPathFolder("FolderSignature")
                                                        .ConfigureAwait(false);
                            string pathFolderFoto = systemParameterPath.Value;

                            string subPathFolderSignature = pathFolder + "/" + pathFolderFoto + "/" + enroll.KtpNIK + "/";

                            string fileName = "Signature_" + enroll.KtpNIK + "_" + JamServer + ".jpg";

                            if (_sftpConfig.IsActive)
                            {
                                using var stream = new MemoryStream(imgBytes);

                                (var fname, var fPath) = await stream.UploadToFTPServer(subPathFolderSignature, fileName, _sftpConfig.Host,
                                    _sftpConfig.Username, _sftpConfig.Password, _sftpConfig.RootDirectory, _sftpConfig.Url).ConfigureAwait(false);

                                filePath = fPath;
                            }
                            else
                            {
                                if (!Directory.Exists(subPathFolderSignature))
                                {
                                    Directory.CreateDirectory(subPathFolderSignature);
                                }

                                filePath = subPathFolderSignature + fileName;
                                File.WriteAllBytes(filePath, imgBytes);
                            }

                            if (photoSignatureData == null)
                            {
                                photoSignatureData = new Tbl_DataKTP_Signature();
                                photoSignatureData.UpdatedById = Id;
                                photoSignatureData.UpdatedByNpp = npp;
                                photoSignatureData.UpdatedByUid = enroll.UID;
                                photoSignatureData.UpdatedTime = DateTime.Now;
                                photoSignatureData.UpdatedByUnit = unitCode;
                                photoSignatureData.IsActive = true;
                                photoSignatureData.IsDeleted = false;
                                photoSignatureData.Nik = enroll.KtpNIK;
                                photoSignatureData.FileName = fileName;
                                photoSignatureData.PathFile = filePath;
                                photoSignatureData.CreatedById = Id;
                                photoSignatureData.CreatedByNpp = npp;
                                photoSignatureData.CreatedByUid = enroll.UID;
                                photoSignatureData.CreatedByUnit = unitCode;
                                photoSignatureData.CreatedTime = DateTime.Now;
                            }
                            else
                            {
                                photoSignatureLog.CreatedById = photoSignatureData.CreatedById;
                                photoSignatureLog.CreatedByNpp = photoSignatureData.CreatedByNpp;
                                photoSignatureLog.CreatedByUid = photoSignatureData.CreatedByUid;
                                photoSignatureLog.CreatedByUnit = photoSignatureData.CreatedByUnit;
                                photoSignatureLog.CreatedTime = photoSignatureData.CreatedTime;
                                photoSignatureLog.FileName = photoSignatureData.FileName;
                                photoSignatureLog.Nik = photoSignatureData.Nik;
                                photoSignatureLog.PathFile = photoSignatureData.PathFile;

                                photoSignatureData.UpdatedById = Id;
                                photoSignatureData.UpdatedByNpp = npp;
                                photoSignatureData.UpdatedByUid = enroll.UID;
                                photoSignatureData.UpdatedByUnit = unitCode;
                                photoSignatureData.UpdatedTime = DateTime.Now;
                                photoSignatureData.IsActive = true;
                                photoSignatureData.IsDeleted = false;
                                photoSignatureData.Nik = enroll.KtpNIK;
                                photoSignatureData.FileName = fileName;
                                photoSignatureData.PathFile = filePath;
                            }
                        }
                        
                    }
                    else
                    {
                        stringPerubahan = stringPerubahan + "Request Payload Signature Tidak Ditemukan" + " <br/>";
                    }

                    #endregion

                    //var photoCamLog = new Tbl_DataKTP_PhotoCam_Log();
                    //var photoCamData = await _profileRepository.GetPhotoCam(enroll.KtpNIK)
                    //        .ConfigureAwait(false);
                    //var exphotoCamData = photoCamData;

                    //#region update photo cam
                    //if (!string.IsNullOrWhiteSpace(enroll.KtpPhotoCam))
                    //{
                    //    byte[] imgBytes = Convert.FromBase64String(enroll.KtpPhotoCam);

                    //    string filePath = "";

                    //    var systemParameterPath = await _sysParameterRepository.GetPathFolder("FolderWebcam")
                    //                                .ConfigureAwait(false);
                    //    string pathFolderFoto = systemParameterPath.Value;

                    //    string SubPathFolderPhotoCam = pathFolder + "/" + pathFolderFoto + "/" + enroll.KtpNIK + "/";

                    //    string fileName = "PhotoCam_" + enroll.KtpNIK + "_" + JamServer + ".txt";

                    //    if (_sftpConfig.IsActive)
                    //    {
                    //        using var stream = new MemoryStream(imgBytes);

                    //        (var fname, var fPath) = await stream.UploadToFTPServer(SubPathFolderPhotoCam, fileName, _sftpConfig.Host,
                    //            _sftpConfig.Username, _sftpConfig.Password, _sftpConfig.RootDirectory, _sftpConfig.Url).ConfigureAwait(false);

                    //        filePath = fPath;
                    //    }
                    //    else
                    //    {
                    //        if (!Directory.Exists(SubPathFolderPhotoCam))
                    //        {
                    //            Directory.CreateDirectory(SubPathFolderPhotoCam);
                    //        }

                    //        filePath = SubPathFolderPhotoCam + fileName;
                    //        File.WriteAllBytes(filePath, imgBytes);
                    //    }

                    //    if (photoCamData == null)
                    //    {
                    //        photoCamData = new Tbl_DataKTP_PhotoCam();
                    //        photoCamData.PathFile = filePath;
                    //        photoCamData.Nik = enroll.KtpNIK;
                    //        photoCamData.FileName = fileName;
                    //        photoCamData.IsActive = true;
                    //        photoCamData.IsDeleted = false;
                    //        photoCamData.UpdatedById = Id;
                    //        photoCamData.UpdatedByNpp = npp;
                    //        photoCamData.UpdatedByUid = enroll.UID;
                    //        photoCamData.UpdatedTime = DateTime.Now;
                    //        photoCamData.UpdatedByUnit = unitCode;
                    //        photoCamLog.CreatedById = photoCamData.CreatedById;
                    //        photoCamLog.CreatedByNpp = photoCamData.CreatedByNpp;
                    //        photoCamLog.CreatedByUid = photoCamData.CreatedByUid;
                    //        photoCamLog.CreatedByUnit = photoCamData.CreatedByUnit;
                    //        photoCamLog.CreatedTime = photoCamData.CreatedTime;
                    //    }
                    //    else
                    //    {
                    //        photoCamLog.CreatedById = photoCamData.CreatedById;
                    //        photoCamLog.CreatedByNpp = photoCamData.CreatedByNpp;
                    //        photoCamLog.CreatedByUid = photoCamData.CreatedByUid;
                    //        photoCamLog.CreatedByUnit = photoCamData.CreatedByUnit;
                    //        photoCamLog.CreatedTime = photoCamData.CreatedTime;
                    //        photoCamLog.FileName = photoCamData.FileName;
                    //        photoCamLog.Nik = photoCamData.Nik;
                    //        photoCamLog.PathFile = photoCamData.PathFile;

                    //        photoCamData.PathFile = filePath;
                    //        photoCamData.Nik = enroll.KtpNIK;
                    //        photoCamData.FileName = fileName;
                    //        photoCamData.IsActive = true;
                    //        photoCamData.IsDeleted = false;
                    //        photoCamData.UpdatedById = Id;
                    //        photoCamData.UpdatedByNpp = npp;
                    //        photoCamData.UpdatedByUid = enroll.UID;
                    //        photoCamData.UpdatedTime = DateTime.Now;
                    //    }
                    //}
                    //else
                    //{
                    //    stringPerubahan = stringPerubahan + "Request Payload Photo Cam Tidak Ditemukan" + " <br/>";
                    //}
                    //#endregion
                    #region update photo cam
                    var photoCamLog = new Tbl_DataKTP_PhotoCam_Log();
                    var photoCamData = await _profileRepository.GetPhotoCam(enroll.KtpNIK)
                            .ConfigureAwait(false);
                    var exphotoCamData = photoCamData;

                    if (!string.IsNullOrWhiteSpace(enroll.KtpPhotoCam))
                    {

                        byte[] imgBytes = Convert.FromBase64String(enroll.KtpPhotoCam);

                        string filePath = "";

                        var systemParameterPath = await _sysParameterRepository.GetPathFolder("FolderWebcam")
                                                    .ConfigureAwait(false);
                        string pathFolderFoto = systemParameterPath.Value;

                        string SubPathFolderPhotoCam = pathFolder + "/" + pathFolderFoto + "/" + enroll.KtpNIK + "/";

                        string fileName = "PhotoCam_" + enroll.KtpNIK + "_" + JamServer + ".txt";

                        if (_sftpConfig.IsActive)
                        {
                            using var stream = new MemoryStream(imgBytes);

                            (var fname, var fPath) = await stream.UploadToFTPServer(SubPathFolderPhotoCam, fileName, _sftpConfig.Host,
                                _sftpConfig.Username, _sftpConfig.Password, _sftpConfig.RootDirectory, _sftpConfig.Url).ConfigureAwait(false);

                            filePath = fPath;
                        }
                        else
                        {
                            if (!Directory.Exists(SubPathFolderPhotoCam))
                            {
                                Directory.CreateDirectory(SubPathFolderPhotoCam);
                            }

                            filePath = SubPathFolderPhotoCam + fileName;
                            File.WriteAllBytes(filePath, imgBytes);
                        }

                        if (photoCamData == null)
                        {
                            photoCamData = new Tbl_DataKTP_PhotoCam();
                            photoCamData.PathFile = filePath;
                            photoCamData.Nik = enroll.KtpNIK;
                            photoCamData.FileName = fileName;
                            photoCamData.IsActive = true;
                            photoCamData.IsDeleted = false;
                            photoCamData.UpdatedById = Id;
                            photoCamData.UpdatedByNpp = npp;
                            photoCamData.UpdatedByUid = enroll.UID;
                            photoCamData.UpdatedTime = DateTime.Now;
                            photoCamData.UpdatedByUnit = unitCode;
                            photoCamLog.CreatedById = photoCamData.CreatedById;
                            photoCamLog.CreatedByNpp = photoCamData.CreatedByNpp;
                            photoCamLog.CreatedByUid = photoCamData.CreatedByUid;
                            photoCamLog.CreatedByUnit = photoCamData.CreatedByUnit;
                            photoCamLog.CreatedTime = photoCamData.CreatedTime;
                        }
                        else
                        {
                            photoCamLog.CreatedById = photoCamData.CreatedById;
                            photoCamLog.CreatedByNpp = photoCamData.CreatedByNpp;
                            photoCamLog.CreatedByUid = photoCamData.CreatedByUid;
                            photoCamLog.CreatedByUnit = photoCamData.CreatedByUnit;
                            photoCamLog.CreatedTime = photoCamData.CreatedTime;
                            photoCamLog.FileName = photoCamData.FileName;
                            photoCamLog.Nik = photoCamData.Nik;
                            photoCamLog.PathFile = photoCamData.PathFile;

                            photoCamData.PathFile = filePath;
                            photoCamData.Nik = enroll.KtpNIK;
                            photoCamData.FileName = fileName;
                            photoCamData.IsActive = true;
                            photoCamData.IsDeleted = false;
                            photoCamData.UpdatedById = Id;
                            photoCamData.UpdatedByNpp = npp;
                            photoCamData.UpdatedByUid = enroll.UID;
                            photoCamData.UpdatedTime = DateTime.Now;
                            photoCamData.UpdatedByUnit = unitCode;
                        }
                    }
                    else
                    {
                        if (photoCamData != null)
                        {
                            photoCamLog.CreatedById = photoCamData.CreatedById;
                            photoCamLog.CreatedByNpp = photoCamData.CreatedByNpp;
                            photoCamLog.CreatedByUid = photoCamData.CreatedByUid;
                            photoCamLog.CreatedByUnit = photoCamData.CreatedByUnit;
                            photoCamLog.CreatedTime = photoCamData.CreatedTime;
                            photoCamLog.FileName = photoCamData.FileName;
                            photoCamLog.Nik = photoCamData.Nik;
                            photoCamLog.PathFile = photoCamData.PathFile;
                            photoCamData = new Tbl_DataKTP_PhotoCam();
                        }
                        stringPerubahan = stringPerubahan + "Request Payload Photo Cam Tidak Ditemukan" + " <br/>";
                    }

                    #endregion

                    #region finger
                    var exPhotoFinger = new List<Tbl_DataKTP_Finger>();

                    var exPhotoFingerEmployee = new List<Tbl_DataKTP_Finger_Employee>();

                    var photoFingerLogs = new List<Tbl_DataKTP_Finger_Log>();

                    var photoFingers = new List<Tbl_DataKTP_Finger>();

                    var photoFingersEmployee = new List<Tbl_DataKTP_Finger_Employee>();

                    var photoFingersEmployeeLogs = new List<Tbl_DataKTP_Finger_Employee_Log>();

                    var photoFingerExisting = await _profileRepository.GetPhotoFingerExisting(enroll.KtpNIK).ConfigureAwait(false);
                    var photoFingerEmployeeExisting = await _profileRepository.GetPhotoFingerEmployeeExisting(enroll.KtpNIK).ConfigureAwait(false);

                    foreach (var em in photoFingerEmployeeExisting)
                    {
                        photoFingersEmployeeLogs.Add(new Tbl_DataKTP_Finger_Employee_Log
                        {
                            CreatedById = em.CreatedById,
                            CreatedByNpp = em.CreatedByNpp,
                            CreatedByUid = em.CreatedByUid,
                            CreatedByUnit = em.CreatedByUnit,
                            CreatedByUnitId = em.CreatedByUnitId,
                            CreatedTime = em.CreatedTime,
                            //FileJari = em.FileJari,
                            FileName = em.FileName,
                            FileNameISO = em.FileNameISO,
                            Nik = em.Nik,
                            PathFile = em.PathFile,
                            PathFileISO = em.PathFileISO,
                            TypeFinger = em.TypeFinger
                        });
                    }

                    foreach (var el in photoFingerExisting)
                    {
                        photoFingerLogs.Add(new Tbl_DataKTP_Finger_Log
                        {
                            CreatedById = el.CreatedById,
                            CreatedByNpp = el.CreatedByNpp,
                            CreatedByUid = el.CreatedByUid,
                            CreatedByUnit = el.CreatedByUnit,
                            CreatedByUnitId = el.CreatedByUnitId,
                            CreatedTime = el.CreatedTime,
                            //FileJari = el.FileJari,
                            FileJariISO = el.FileJariISO,
                            FileName = el.FileName,
                            FileNameISO = el.FileNameISO,
                            Nik = el.Nik,
                            PathFile = el.PathFile,
                            PathFileISO = el.PathFileISO,
                            TypeFinger = el.TypeFinger
                        });
                    }

                    if (!string.IsNullOrWhiteSpace(enroll.KtpFingerKanan))
                    {

                        var photoFingerDataEmployee = new Tbl_DataKTP_Finger_Employee();

                        string imageEncrypted = enroll.KtpFingerKanan.Encrypt(Phrase.FileEncryption);

                        string filePath = "";
                        string filePathIso = "";

                        var systemParameterPath = await _sysParameterRepository.GetPathFolder("FolderFinger")
                                                    .ConfigureAwait(false);
                        string pathFolderFoto = systemParameterPath.Value;

                        string subPathFolderPhotoFinger = pathFolder + "/" + pathFolderFoto + "/" + enroll.KtpNIK + "/";

                        string JenisJari = enroll.KtpTypeJariKanan.Replace(" ", "");
                        string fileName = "PhotoFinger_" + JenisJari + "_" + enroll.KtpNIK + "_" + JamServer + ".txt";
                        string fileNameIso = "";

                        if (_sftpConfig.IsActive)
                        {
                            using var stream = new MemoryStream(Encoding.ASCII.GetBytes(imageEncrypted));

                            (var fname, var fPath) = await stream.UploadToFTPServer(subPathFolderPhotoFinger, fileName, _sftpConfig.Host,
                                _sftpConfig.Username, _sftpConfig.Password, _sftpConfig.RootDirectory, _sftpConfig.Url).ConfigureAwait(false);

                            filePath = fPath;
                        }
                        else
                        {
                            if (!Directory.Exists(subPathFolderPhotoFinger))
                            {
                                Directory.CreateDirectory(subPathFolderPhotoFinger);
                            }

                            filePath = subPathFolderPhotoFinger + fileName;
                            File.WriteAllText(filePath, imageEncrypted);
                        }

                        string isoEncrypted = "";

                        if (!string.IsNullOrWhiteSpace(enroll.KtpFingerKananIso))
                        {
                            isoEncrypted = enroll.KtpFingerKananIso.Encrypt(Phrase.FileEncryption);
                            fileNameIso = "PhotoFingerISO_" + JenisJari + "_" + enroll.KtpNIK + "_" + JamServer + ".txt";

                            if (_sftpConfig.IsActive)
                            {
                                using var stream = new MemoryStream(Encoding.ASCII.GetBytes(isoEncrypted));

                                (var fname, var fPath) = await stream.UploadToFTPServer(subPathFolderPhotoFinger, fileNameIso, _sftpConfig.Host,
                                    _sftpConfig.Username, _sftpConfig.Password, _sftpConfig.RootDirectory, _sftpConfig.Url).ConfigureAwait(false);

                                filePathIso = fPath;
                            }
                            else
                            {
                                if (!Directory.Exists(subPathFolderPhotoFinger))
                                {
                                    Directory.CreateDirectory(subPathFolderPhotoFinger);
                                }

                                filePathIso = subPathFolderPhotoFinger + fileNameIso;
                                File.WriteAllText(filePathIso, isoEncrypted);
                            }
                        }

                        //var typeFingerMain = await _fingerRepository.GetFingersEnrolled(enroll.KtpNIK);

                        var typeFingerMain = new List<FingerByNik> {
                            new FingerByNik { NIK = enroll.KtpNIK, TypeFinger = (enroll.KtpTypeJariKanan != null) ? enroll.KtpTypeJariKanan : ""},
                            new FingerByNik { NIK = enroll.KtpNIK, TypeFinger = (enroll.KtpTypeJariKiri != null) ? enroll.KtpTypeJariKiri : ""},
                        };

                        foreach (var i in typeFingerMain)
                        {

                            if (i.TypeFinger.Contains("Kanan"))
                            {
                                var photoFingerData = await _profileRepository.GetPhotoFinger(enroll.KtpNIK, i.TypeFinger).ConfigureAwait(false);

                                if (isEmployee)
                                {
                                    photoFingerDataEmployee = await _profileRepository.GetPhotoFingerEmployee(enroll.KtpNIK, i.TypeFinger)
                                    .ConfigureAwait(false);
                                }

                            //if (photoFingerData == null)
                            if (photoFingerData == null)
                            {
                                photoFingerData = new Tbl_DataKTP_Finger
                                {
                                    CreatedById = Id,
                                    IsActive = true,
                                    IsDeleted = false,
                                    Nik = enroll.KtpNIK,
                                    PathFile = filePath,
                                    PathFileISO = filePathIso,
                                    CreatedByNpp = npp,
                                    CreatedByUid = enroll.UID,
                                    CreatedTime = DateTime.Now,
                                    CreatedByUnit = unitCode,
                                    CreatedByUnitId = unitId,
                                    FileName = fileName,
                                    FileNameISO = fileNameIso,
                                    FileJariISO = isoEncrypted,
                                    TypeFinger = enroll.KtpTypeJariKanan
                                };

                                Tbl_DataKTP_Finger_Log fingerBeforeLogs = photoFingerLogs.FirstOrDefault(x => x.TypeFinger.Contains("Kanan"));

                                if (fingerBeforeLogs != null)
                                {
                                    photoFingerData.CreatedById = fingerBeforeLogs.CreatedById;
                                    photoFingerData.CreatedByNpp = fingerBeforeLogs.CreatedByNpp;
                                    photoFingerData.CreatedByUid = fingerBeforeLogs.CreatedByUid;
                                    photoFingerData.CreatedTime = fingerBeforeLogs.CreatedTime;
                                    photoFingerData.CreatedByUnit = fingerBeforeLogs.CreatedByUnit;
                                    photoFingerData.CreatedByUnitId = fingerBeforeLogs.CreatedByUnitId;
                                } 

                                photoFingers.Add(new Tbl_DataKTP_Finger
                                {
                                    //CreatedById = photoFingerData.CreatedById,
                                    CreatedById = photoFingerData.CreatedById,
                                    IsActive = true,
                                    IsDeleted = false,
                                    Nik = enroll.KtpNIK,
                                    PathFile = filePath,
                                    PathFileISO = filePathIso,
                                    //CreatedByNpp = photoFingerData.CreatedByNpp,
                                    //CreatedByUid = photoFingerData.CreatedByUid,
                                    //CreatedTime = photoFingerData.CreatedTime,
                                    CreatedByNpp = photoFingerData.CreatedByNpp,
                                    CreatedByUid = photoFingerData.CreatedByUid,
                                    CreatedTime = photoFingerData.CreatedTime,
                                    UpdatedById = Id,
                                    UpdatedByNpp = npp,
                                    UpdatedByUid = enroll.UID,
                                    UpdatedByUnit = unitCode,
                                    //CreatedByUnitId = photoFingerData.CreatedByUnitId,
                                    CreatedByUnitId = photoFingerData.CreatedByUnitId,
                                    CreatedByUnit = photoFingerData.CreatedByUnit,
                                    //FileJari = photoFingerData.FileJari,
                                    FileJariISO = isoEncrypted,
                                    UpdatedTime = DateTime.Now,
                                    FileName = fileName,
                                    FileNameISO = fileNameIso,
                                    TypeFinger = enroll.KtpTypeJariKanan
                                });
                                }
                                else
                                {

                                    photoFingers.Add(new Tbl_DataKTP_Finger
                                    {
                                        //CreatedById = photoFingerData.CreatedById,
                                        CreatedById = Id,
                                        IsActive = true,
                                        IsDeleted = false,
                                        Nik = enroll.KtpNIK,
                                        PathFile = filePath,
                                        PathFileISO = filePathIso,
                                        //CreatedByNpp = photoFingerData.CreatedByNpp,
                                        //CreatedByUid = photoFingerData.CreatedByUid,
                                        CreatedTime = photoFingerData.CreatedTime,
                                        CreatedByNpp = npp,
                                        CreatedByUid = enroll.UID,
                                        //CreatedTime = DateTime.Now,
                                        UpdatedById = Id,
                                        UpdatedByNpp = npp,
                                        UpdatedByUid = enroll.UID,
                                        UpdatedByUnit = unitCode,
                                        //CreatedByUnitId = photoFingerData.CreatedByUnitId,
                                        CreatedByUnitId = unitId,
                                        CreatedByUnit = photoFingerData.CreatedByUnit,
                                        //FileJari = photoFingerData.FileJari,
                                        FileJariISO = isoEncrypted,
                                        UpdatedTime = DateTime.Now,
                                        FileName = fileName,
                                        FileNameISO = fileNameIso,
                                        TypeFinger = enroll.KtpTypeJariKanan
                                    });

                                }

                                if (isEmployee)
                                {

                                if(photoFingerDataEmployee == null)
                                {
                                    Tbl_DataKTP_Finger_Employee_Log fingerEmployeeBeforeLogs = photoFingersEmployeeLogs.FirstOrDefault(x => x.TypeFinger.Contains("Kanan"));
                                    
                                    photoFingerDataEmployee = new Tbl_DataKTP_Finger_Employee
                                    {
                                        //CreatedById = fingerEmployeeBeforeLogs.CreatedById,
                                        //CreatedByNpp = fingerEmployeeBeforeLogs.CreatedByNpp,
                                        //CreatedByUid = fingerEmployeeBeforeLogs.CreatedByUid,
                                        //CreatedTime = fingerEmployeeBeforeLogs.CreatedTime,
                                        //CreatedByUnit = fingerEmployeeBeforeLogs.CreatedByUnit,
                                        //CreatedByUnitId = fingerEmployeeBeforeLogs.CreatedByUnitId,

                                        CreatedById = Id,
                                        CreatedByNpp = npp,
                                        CreatedByUid = enroll.UID,
                                        CreatedTime = DateTime.Now,
                                        CreatedByUnit = unitCode,
                                        CreatedByUnitId = unitId,
                                    };


                                    if (fingerEmployeeBeforeLogs != null)
                                    {
                                        photoFingerDataEmployee.CreatedById = fingerEmployeeBeforeLogs.CreatedById;
                                        photoFingerDataEmployee.CreatedByNpp = fingerEmployeeBeforeLogs.CreatedByNpp;
                                        photoFingerDataEmployee.CreatedByUid = fingerEmployeeBeforeLogs.CreatedByUid;
                                        photoFingerDataEmployee.CreatedTime = fingerEmployeeBeforeLogs.CreatedTime;
                                        photoFingerDataEmployee.CreatedByUnit = fingerEmployeeBeforeLogs.CreatedByUnit;
                                        photoFingerDataEmployee.CreatedByUnitId = fingerEmployeeBeforeLogs.CreatedByUnitId;
                                    }
                                }

                                photoFingersEmployee.Add(new Tbl_DataKTP_Finger_Employee
                                    {
                                        CreatedById = photoFingerDataEmployee.CreatedById,
                                        IsActive = true,
                                        IsDeleted = false,
                                        Nik = enroll.KtpNIK,
                                        PathFile = filePath,
                                        CreatedByNpp = photoFingerDataEmployee.CreatedByNpp,
                                        CreatedByUid = photoFingerDataEmployee.CreatedByUid,
                                        //CreatedTime = DateTime.Now,
                                        CreatedTime = photoFingerDataEmployee.CreatedTime,
                                        UpdatedById = Id,
                                        UpdatedByNpp = photoFingerDataEmployee.CreatedByNpp,
                                        UpdatedByUid = enroll.UID,
                                        UpdatedByUnit = unitCode,
                                        CreatedByUnitId = photoFingerDataEmployee.CreatedByUnitId,
                                        CreatedByUnit = photoFingerDataEmployee.CreatedByUnit,
                                        //FileJari = enroll.KtpFingerKanan,
                                        UpdatedTime = DateTime.Now,
                                        FileName = fileName,
                                        FileJariISO = isoEncrypted,
                                        FileNameISO = fileNameIso,
                                        PathFileISO = filePathIso,
                                        TypeFinger = enroll.KtpTypeJariKanan,
                                    });
                                }
                            }
                        }
                    }
                    else
                    {
                        #region get data if no payload but we have to replace all data with no data
                        var photoFingerDataEmployee = new Tbl_DataKTP_Finger_Employee();
                        //var typeFingerMain = await _fingerRepository.GetFingersEnrolled(enroll.KtpNIK);

                        var typeFingerMain = new List<FingerByNik> {
                            new FingerByNik { NIK = enroll.KtpNIK, TypeFinger = (enroll.KtpTypeJariKanan != null) ? enroll.KtpTypeJariKanan : ""},
                            new FingerByNik { NIK = enroll.KtpNIK, TypeFinger = (enroll.KtpTypeJariKiri != null) ? enroll.KtpTypeJariKiri : ""},
                        };

                        foreach (var i in typeFingerMain)
                        {

                            if (i.TypeFinger.Contains("Kanan"))
                            {
                                var photoFingerData = await _profileRepository.GetPhotoFinger(enroll.KtpNIK, i.TypeFinger).ConfigureAwait(false);

                                if (isEmployee)
                                {
                                    photoFingerDataEmployee = await _profileRepository.GetPhotoFingerEmployee(enroll.KtpNIK, i.TypeFinger)
                                    .ConfigureAwait(false);
                                }



                            }
                        }

                        #endregion

                        stringPerubahan = stringPerubahan + "Request Payload Finger Kanan Tidak Ditemukan" + " <br/>";
                    }

                    if (!string.IsNullOrWhiteSpace(enroll.KtpFingerKiri))
                    {
                        //var photoFingerData = await _profileRepository.GetPhotoFinger(enroll.KtpNIK, enroll.KtpTypeJariKiri)
                        //    .ConfigureAwait(false);

                        var photoFingerDataEmployee = new Tbl_DataKTP_Finger_Employee();

                        //if (isEmployee)
                        //{
                        //    photoFingerDataEmployee = await _profileRepository.GetPhotoFingerEmployee(enroll.KtpNIK, enroll.KtpTypeJariKiri)
                        //    .ConfigureAwait(false);
                        //}

                        string imageEncrypted = enroll.KtpFingerKiri.Encrypt(Phrase.FileEncryption);

                        string filePath = "";
                        string filePathIso = "";

                        var systemParameterPath = await _sysParameterRepository.GetPathFolder("FolderFinger")
                                                    .ConfigureAwait(false);
                        string pathFolderFoto = systemParameterPath.Value;

                        string subPathFolderPhotoFinger = pathFolder + "/" + pathFolderFoto + "/" + enroll.KtpNIK + "/";

                        string JenisJari = enroll.KtpTypeJariKiri.Replace(" ", "");
                        string fileName = "PhotoFinger_" + JenisJari + "_" + enroll.KtpNIK + "_" + JamServer + ".txt";
                        string fileNameIso = "";

                        if (_sftpConfig.IsActive)
                        {
                            using var stream = new MemoryStream(Encoding.ASCII.GetBytes(imageEncrypted));

                            (var fname, var fPath) = await stream.UploadToFTPServer(subPathFolderPhotoFinger, fileName, _sftpConfig.Host,
                                _sftpConfig.Username, _sftpConfig.Password, _sftpConfig.RootDirectory, _sftpConfig.Url).ConfigureAwait(false);

                            filePath = fPath;
                        }
                        else
                        {
                            if (!Directory.Exists(subPathFolderPhotoFinger))
                            {
                                Directory.CreateDirectory(subPathFolderPhotoFinger);
                            }

                            filePath = subPathFolderPhotoFinger + fileName;
                            File.WriteAllText(filePath, imageEncrypted);
                        }

                        string isoEncrypted = "";

                        if (!string.IsNullOrWhiteSpace(enroll.KtpFingerKiriIso))
                        {
                            isoEncrypted = enroll.KtpFingerKiriIso.Encrypt(Phrase.FileEncryption);
                            fileNameIso = "PhotoFingerISO_" + JenisJari + "_" + enroll.KtpNIK + "_" + JamServer + ".txt";

                            if (_sftpConfig.IsActive)
                            {
                                using var stream = new MemoryStream(Encoding.ASCII.GetBytes(isoEncrypted));

                                (var fname, var fPath) = await stream.UploadToFTPServer(subPathFolderPhotoFinger, fileNameIso, _sftpConfig.Host,
                                    _sftpConfig.Username, _sftpConfig.Password, _sftpConfig.RootDirectory, _sftpConfig.Url).ConfigureAwait(false);

                                filePathIso = fPath;
                            }
                            else
                            {
                                if (!Directory.Exists(subPathFolderPhotoFinger))
                                {
                                    Directory.CreateDirectory(subPathFolderPhotoFinger);
                                }

                                filePathIso = subPathFolderPhotoFinger + fileNameIso;
                                File.WriteAllText(filePathIso, isoEncrypted);
                            }
                        }


                        //var typeFingerMain = await _fingerRepository.GetFingersEnrolled(enroll.KtpNIK);

                        var typeFingerMain = new List<FingerByNik> {
                            new FingerByNik { NIK = enroll.KtpNIK, TypeFinger = (enroll.KtpTypeJariKanan != null) ? enroll.KtpTypeJariKanan : ""},
                            new FingerByNik { NIK = enroll.KtpNIK, TypeFinger = (enroll.KtpTypeJariKiri != null) ? enroll.KtpTypeJariKiri : ""},
                        };

                        foreach (var i in typeFingerMain)
                        {

                            if (i.TypeFinger.Contains("Kiri"))
                            {
                                var photoFingerData = await _profileRepository.GetPhotoFinger(enroll.KtpNIK, i.TypeFinger).ConfigureAwait(false);

                                if (isEmployee)
                                {
                                    photoFingerDataEmployee = await _profileRepository.GetPhotoFingerEmployee(enroll.KtpNIK, i.TypeFinger)
                                    .ConfigureAwait(false);
                                }

                                if (photoFingerData == null)
                                {
                                    photoFingerData = new Tbl_DataKTP_Finger
                                    {
                                        CreatedById = Id,
                                        IsActive = true,
                                        IsDeleted = false,
                                        Nik = enroll.KtpNIK,
                                        PathFile = filePath,
                                        PathFileISO = filePathIso,
                                        CreatedByNpp = npp,
                                        CreatedByUid = enroll.UID,
                                        CreatedTime = DateTime.Now,
                                        CreatedByUnit = unitCode,
                                        CreatedByUnitId = unitId,
                                        FileName = fileName,
                                        FileNameISO = fileNameIso,
                                        FileJariISO = isoEncrypted,
                                        TypeFinger = enroll.KtpTypeJariKiri
                                    };

                                Tbl_DataKTP_Finger_Log fingerBeforeLogs = photoFingerLogs.FirstOrDefault(x => x.TypeFinger.Contains("Kiri"));

                                if (fingerBeforeLogs != null)
                                {
                                    photoFingerData.CreatedById = fingerBeforeLogs.CreatedById;
                                    photoFingerData.CreatedByNpp = fingerBeforeLogs.CreatedByNpp;
                                    photoFingerData.CreatedByUid = fingerBeforeLogs.CreatedByUid;
                                    photoFingerData.CreatedTime = fingerBeforeLogs.CreatedTime;
                                    photoFingerData.CreatedByUnit = fingerBeforeLogs.CreatedByUnit;
                                    photoFingerData.CreatedByUnitId = fingerBeforeLogs.CreatedByUnitId;
                                }

                                photoFingers.Add(new Tbl_DataKTP_Finger
                                    {
                                        //CreatedById = photoFingerData.CreatedById,
                                        CreatedById = photoFingerData.CreatedById,
                                        IsActive = true,
                                        IsDeleted = false,
                                        Nik = enroll.KtpNIK,
                                        PathFile = filePath,
                                        PathFileISO = filePathIso,
                                        //CreatedByNpp = photoFingerData.CreatedByNpp,
                                        //CreatedByUid = photoFingerData.CreatedByUid,
                                        //CreatedTime = photoFingerData.CreatedTime,
                                        CreatedByNpp = photoFingerData.CreatedByNpp,
                                        CreatedByUid = photoFingerData.CreatedByUid,
                                        CreatedTime = photoFingerData.CreatedTime,
                                        UpdatedById = Id,
                                        UpdatedByNpp = npp,
                                        UpdatedByUid = enroll.UID,
                                        UpdatedByUnit = unitCode,
                                        //CreatedByUnitId = photoFingerData.CreatedByUnitId,
                                        CreatedByUnitId = photoFingerData.CreatedByUnitId,
                                        CreatedByUnit = photoFingerData.CreatedByUnit,
                                        //FileJari = photoFingerData.FileJari,
                                        FileJariISO = isoEncrypted,
                                        UpdatedTime = DateTime.Now,
                                        FileName = fileName,
                                        FileNameISO = fileNameIso,
                                        TypeFinger = enroll.KtpTypeJariKiri
                                    });


                                }
                                else
                                {
                                    photoFingers.Add(new Tbl_DataKTP_Finger
                                    {
                                        //CreatedById = photoFingerData.CreatedById,
                                        CreatedById = Id,
                                        IsActive = true,
                                        IsDeleted = false,
                                        Nik = enroll.KtpNIK,
                                        PathFile = filePath,
                                        PathFileISO = filePathIso,
                                        //CreatedByNpp = photoFingerData.CreatedByNpp,
                                        //CreatedByUid = photoFingerData.CreatedByUid,
                                        CreatedTime = photoFingerData.CreatedTime,
                                        CreatedByNpp = npp,
                                        CreatedByUid = enroll.UID,
                                        //CreatedTime = DateTime.Now,
                                        UpdatedById = Id,
                                        UpdatedByNpp = npp,
                                        UpdatedByUid = enroll.UID,
                                        UpdatedByUnit = unitCode,
                                        //CreatedByUnitId = photoFingerData.CreatedByUnitId,
                                        CreatedByUnitId = unitId,
                                        CreatedByUnit = photoFingerData.CreatedByUnit,
                                        //FileJari = photoFingerData.FileJari,
                                        FileJariISO = isoEncrypted,
                                        UpdatedTime = DateTime.Now,
                                        FileName = fileName,
                                        FileNameISO = fileNameIso,
                                        TypeFinger = enroll.KtpTypeJariKiri
                                    });


                                }

                            if (isEmployee)
                            {

                                if (photoFingerDataEmployee == null)
                                {
                                    Tbl_DataKTP_Finger_Employee_Log fingerEmployeeBeforeLogs = photoFingersEmployeeLogs.FirstOrDefault(x => x.TypeFinger.Contains("Kiri"));

                                    photoFingerDataEmployee = new Tbl_DataKTP_Finger_Employee
                                    {
                                        //CreatedById = fingerEmployeeBeforeLogs.CreatedById,
                                        //CreatedByNpp = fingerEmployeeBeforeLogs.CreatedByNpp,
                                        //CreatedByUid = fingerEmployeeBeforeLogs.CreatedByUid,
                                        //CreatedTime = fingerEmployeeBeforeLogs.CreatedTime,
                                        //CreatedByUnit = fingerEmployeeBeforeLogs.CreatedByUnit,
                                        //CreatedByUnitId = fingerEmployeeBeforeLogs.CreatedByUnitId,

                                        CreatedById = Id,
                                        CreatedByNpp = npp,
                                        CreatedByUid = enroll.UID,
                                        CreatedTime = DateTime.Now,
                                        CreatedByUnit = unitCode,
                                        CreatedByUnitId = unitId,
                                    };


                                    if (fingerEmployeeBeforeLogs != null)
                                    {
                                        photoFingerDataEmployee.CreatedById = fingerEmployeeBeforeLogs.CreatedById;
                                        photoFingerDataEmployee.CreatedByNpp = fingerEmployeeBeforeLogs.CreatedByNpp;
                                        photoFingerDataEmployee.CreatedByUid = fingerEmployeeBeforeLogs.CreatedByUid;
                                        photoFingerDataEmployee.CreatedTime = fingerEmployeeBeforeLogs.CreatedTime;
                                        photoFingerDataEmployee.CreatedByUnit = fingerEmployeeBeforeLogs.CreatedByUnit;
                                        photoFingerDataEmployee.CreatedByUnitId = fingerEmployeeBeforeLogs.CreatedByUnitId;
                                    }
                                }

                                

                                photoFingersEmployee.Add(new Tbl_DataKTP_Finger_Employee
                                {
                                    CreatedById = photoFingerDataEmployee.CreatedById,
                                    IsActive = true,
                                    IsDeleted = false,
                                    Nik = enroll.KtpNIK,
                                    PathFile = filePath,
                                    CreatedByNpp = photoFingerDataEmployee.CreatedByNpp,
                                    CreatedByUid = photoFingerDataEmployee.CreatedByUid,
                                    //CreatedTime = DateTime.Now,
                                    CreatedTime = photoFingerDataEmployee.CreatedTime,
                                    UpdatedById = Id,
                                    UpdatedByNpp = photoFingerDataEmployee.CreatedByNpp,
                                    UpdatedByUid = enroll.UID,
                                    UpdatedByUnit = unitCode,
                                    CreatedByUnitId = photoFingerDataEmployee.CreatedByUnitId,
                                    CreatedByUnit = photoFingerDataEmployee.CreatedByUnit,
                                    //FileJari = enroll.KtpFingerKanan,
                                    UpdatedTime = DateTime.Now,
                                    FileName = fileName,
                                    FileJariISO = isoEncrypted,
                                    FileNameISO = fileNameIso,
                                    PathFileISO = filePathIso,
                                    TypeFinger = enroll.KtpTypeJariKiri,
                                });
                            }
                        }
                        }
                    }
                    else
                    {
                        #region get data if no payload but we have to replace all data with no data
                        var photoFingerDataEmployee = new Tbl_DataKTP_Finger_Employee();
                        //var typeFingerMain = await _fingerRepository.GetFingersEnrolled(enroll.KtpNIK);
                        var typeFingerMain = new List<FingerByNik> {
                            new FingerByNik { NIK = enroll.KtpNIK, TypeFinger = (enroll.KtpTypeJariKanan != null) ? enroll.KtpTypeJariKanan : ""},
                            new FingerByNik { NIK = enroll.KtpNIK, TypeFinger = (enroll.KtpTypeJariKiri != null) ? enroll.KtpTypeJariKiri : ""},
                        };

                        foreach (var i in typeFingerMain)
                        {

                            if (i.TypeFinger.Contains("Kiri"))
                            {
                                var photoFingerData = await _profileRepository.GetPhotoFinger(enroll.KtpNIK, i.TypeFinger).ConfigureAwait(false);

                                if (isEmployee)
                                {
                                    photoFingerDataEmployee = await _profileRepository.GetPhotoFingerEmployee(enroll.KtpNIK, i.TypeFinger)
                                    .ConfigureAwait(false);
                                }

                            }
                        }
                        #endregion

                        stringPerubahan = stringPerubahan + "Request Payload Finger Kanan Tidak Ditemukan" + " <br/>";
                    }
                #endregion

                //var (status, msg) = await _enrollmentKTPRepository.InsertEnrollFlow2(cekKTP, logDemografi, photoKTPData, photoKtpLog, photoSignatureData,
                //    photoSignatureLog, photoCamData, photoCamLog, photoFingers, photoFingersEmployee, photoFingerLogs, photoFingersEmployeeLogs, exPhotoKtp, exphotoSignatureData, exphotoCamData, exPhotoFinger, exPhotoFingerEmployee, dataReaderLog, dataNpp);

                var (status, msg) = await _enrollmentKTPRepository.InsertEnrollFlow2(cekKTP, logDemografi, photoKTPData, photoKtpLog, photoSignatureData,
                    photoSignatureLog, photoCamData, photoCamLog, photoFingers, photoFingersEmployee, photoFingerLogs, photoFingersEmployeeLogs, exPhotoKtp, exphotoSignatureData, exphotoCamData, photoFingerExisting, photoFingerEmployeeExisting, dataReaderLog, dataNpp);

                #region Logging Reader Activity
                _ = _alatReaderRepository.CreateLogActivity2(dataReaderActivityLog);
                    _ = _alatReaderRepository.CreateAlatReaderLog(dataReaderLog);
                    #endregion

                    if (status)
                    {
                        return (_SuccessMessageConfig.NasabahSuksesUpdate, (int)ServiceResponseStatus.SUKSES, cekKTP.CIF);
                    }
                    else
                    {
                        return (_ErrorMessageConfig.DemografiGagalEnroll, (int)ServiceResponseStatus.ERROR, msg + " " + stringPerubahan);
                    }
            }

                #region 20231108_Validation_Update
            else if (cekKTPTemp != null)
            {
                if (enroll?.KtpNIK == null) return (_ErrorMessageConfig.InputTidakLengkap, (int)EnrollStatus.Inputan_tidak_lengkap, "");
                var cifData = new ApiSOAResponse();
                #region update demografi
                var logDemografi = new Tbl_DataKTP_Demografis_Log
                {
                    Agama = cekKTPTemp.Agama,
                    Alamat = cekKTPTemp.Alamat,
                    AlamatGoogle = cekKTPTemp.AlamatGoogle,
                    AlamatLengkap = cekKTPTemp.AlamatLengkap,
                    CreatedById = cekKTPTemp.CreatedById,
                    CreatedByUID = cekKTPTemp.CreatedByUID,
                    CreatedByUnitId = cekKTPTemp.CreatedByUnitId,
                    CreatedTime = cekKTPTemp.CreatedTime,
                    Desa = cekKTPTemp.Desa,
                    GolonganDarah = cekKTPTemp.GolonganDarah,
                    JenisKelamin = cekKTPTemp.JenisKelamin,
                    Kecamatan = cekKTPTemp.Kecamatan,
                    Kelurahan = cekKTPTemp.Kelurahan,
                    Kewarganegaraan = cekKTPTemp.Kewarganegaraan,
                    KodePos = cekKTPTemp.KodePos,
                    Kota = cekKTPTemp.Kota,
                    Latitude = cekKTPTemp.Latitude,
                    Longitude = cekKTPTemp.Longitude,
                    MasaBerlaku = cekKTPTemp.MasaBerlaku,
                    Nama = cekKTPTemp.Nama,
                    NIK = cekKTPTemp.NIK,
                    Pekerjaan = cekKTPTemp.Pekerjaan,
                    Provinsi = cekKTPTemp.Provinsi,
                    RT = cekKTPTemp.RT,
                    RW = cekKTPTemp.RW,
                    StatusPerkawinan = cekKTPTemp.StatusPerkawinan,
                    TanggalLahir = cekKTPTemp.TanggalLahir,
                    TempatLahir = cekKTPTemp.TempatLahir,
                    CIF = cekKTPTemp.CIF,
                    CreatedByNpp = cekKTPTemp.CreatedByNpp
                };

                //var dataKTPTemp = new Tbl_DataKTP_Demografis
                //{
                //    Agama = cekKTPTemp.Agama,
                //    Alamat = cekKTPTemp.Alamat,
                //    AlamatGoogle = cekKTPTemp.AlamatGoogle,
                //    AlamatLengkap = cekKTPTemp.AlamatLengkap,
                //    CreatedById = cekKTPTemp.CreatedById,
                //    CreatedByUID = cekKTPTemp.CreatedByUID,
                //    CreatedByUnitId = cekKTPTemp.CreatedByUnitId,
                //    CreatedTime = cekKTPTemp.CreatedTime,
                //    Desa = cekKTPTemp.Desa,
                //    GolonganDarah = cekKTPTemp.GolonganDarah,
                //    JenisKelamin = cekKTPTemp.JenisKelamin,
                //    Kecamatan = cekKTPTemp.Kecamatan,
                //    Kelurahan = cekKTPTemp.Kelurahan,
                //    Kewarganegaraan = cekKTPTemp.Kewarganegaraan,
                //    KodePos = cekKTPTemp.KodePos,
                //    Kota = cekKTPTemp.Kota,
                //    Latitude = cekKTPTemp.Latitude,
                //    Longitude = cekKTPTemp.Longitude,
                //    MasaBerlaku = cekKTPTemp.MasaBerlaku,
                //    Nama = cekKTPTemp.Nama,
                //    NIK = cekKTPTemp.NIK,
                //    Pekerjaan = cekKTPTemp.Pekerjaan,
                //    Provinsi = cekKTPTemp.Provinsi,
                //    RT = cekKTPTemp.RT,
                //    RW = cekKTPTemp.RW,
                //    StatusPerkawinan = cekKTPTemp.StatusPerkawinan,
                //    TanggalLahir = cekKTPTemp.TanggalLahir,
                //    TempatLahir = cekKTPTemp.TempatLahir,
                //    CIF = cekKTPTemp.CIF,
                //    CreatedByNpp = cekKTPTemp.CreatedByNpp,
                //    isEnrollFR = true,
                //    UpdatedTime = DateTime.Now,
                //    UpdatedById = Id,
                //    UpdatedByUID = enroll.UID,
                //    UpdatedByUnitCode = unitCode,
                //    UpdatedByUnitId = unitId,
                //    UpdatedByNpp = npp
                //};

                var stringPerubahan = "";

                if (cekKTPTemp.Nama != enroll.KtpNama)
                {
                    stringPerubahan = stringPerubahan + "NAMA : " + cekKTPTemp.Nama + " -> " + enroll.KtpNama + " <br/>";
                    cekKTPTemp.Nama = enroll.KtpNama;
                }

                if (cekKTPTemp.TempatLahir != enroll.KtpTempatLahir)
                {
                    stringPerubahan = stringPerubahan + "TEMPAT LAHIR : " + cekKTPTemp.TempatLahir + " -> " + enroll.KtpTempatLahir + " <br/>";
                    cekKTPTemp.TempatLahir = enroll.KtpTempatLahir;
                }

                if (enroll.KtpTanggalLahir != null)
                {
                    var _ttl = DateTime.ParseExact(enroll.KtpTanggalLahir, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                    if (cekKTPTemp.TanggalLahir != _ttl)
                    {
                        stringPerubahan = stringPerubahan + "TANGGAL LAHIR : " + cekKTPTemp.TanggalLahir + " -> " + enroll.KtpTanggalLahir + " <br/>";
                        cekKTPTemp.TanggalLahir = DateTime.ParseExact(enroll.KtpTanggalLahir, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                    }
                }

                if (cekKTPTemp.JenisKelamin != enroll.KtpJanisKelamin)
                {
                    stringPerubahan = stringPerubahan + "JENIS KELAMIN : " + cekKTPTemp.JenisKelamin + " -> " + enroll.KtpJanisKelamin + " <br/>";
                    cekKTPTemp.JenisKelamin = enroll.KtpJanisKelamin;
                }

                if (cekKTPTemp.GolonganDarah != enroll.KtpGolonganDarah)
                {
                    stringPerubahan = stringPerubahan + "GOLONGAN DARAH : " + cekKTPTemp.GolonganDarah + " -> " + enroll.KtpGolonganDarah + " <br/>";
                    cekKTPTemp.GolonganDarah = enroll.KtpGolonganDarah;
                }

                if (cekKTPTemp.Alamat != enroll.KtpAlamat)
                {
                    stringPerubahan = stringPerubahan + "ALAMAT : " + cekKTPTemp.Alamat + " -> " + enroll.KtpAlamat + " <br/>";
                    cekKTPTemp.Alamat = enroll.KtpAlamat;
                }

                if (cekKTPTemp.RT != enroll.KtpRT)
                {
                    stringPerubahan = stringPerubahan + "RT : " + cekKTPTemp.RT + " -> " + enroll.KtpRT + " <br/>";
                    cekKTPTemp.RT = enroll.KtpRT;
                }

                if (cekKTPTemp.RW != enroll.KtpRW)
                {
                    stringPerubahan = stringPerubahan + "RW : " + cekKTPTemp.RW + " -> " + enroll.KtpRW + " <br/>";
                    cekKTPTemp.RW = enroll.KtpRW;
                }

                if (cekKTPTemp.Kelurahan != enroll.KtpKelurahan)
                {
                    stringPerubahan = stringPerubahan + "KELURAHAN : " + cekKTPTemp.Kelurahan + " -> " + enroll.KtpKelurahan + " <br/>";
                    cekKTPTemp.Kelurahan = enroll.KtpKelurahan;
                }

                if (cekKTPTemp.Kecamatan != enroll.KtpKecamatan)
                {
                    stringPerubahan = stringPerubahan + "KECAMATAN : " + cekKTPTemp.Kecamatan + " -> " + enroll.KtpKecamatan + " <br/>";
                    cekKTPTemp.Kecamatan = enroll.KtpKecamatan;
                }

                if (cekKTPTemp.Kota != enroll.KtpKota)
                {
                    stringPerubahan = stringPerubahan + "KOTA/KABUPATEN : " + cekKTPTemp.Kota + " -> " + enroll.KtpKota + " <br/>";
                    cekKTPTemp.Kota = enroll.KtpKota;
                }

                if (cekKTPTemp.Provinsi != enroll.KtpProvinsi)
                {
                    stringPerubahan = stringPerubahan + "PROVINSI  : " + cekKTPTemp.Provinsi + " -> " + enroll.KtpProvinsi + " <br/>";
                    cekKTPTemp.Provinsi = enroll.KtpProvinsi;
                }

                if (cekKTPTemp.Agama != enroll.KtpAgama)
                {
                    stringPerubahan = stringPerubahan + "AGAMA  : " + cekKTPTemp.Agama + " -> " + enroll.KtpAgama + " <br/>";
                    cekKTPTemp.Agama = enroll.KtpAgama;
                }

                if (cekKTPTemp.StatusPerkawinan != enroll.KtpStatusPerkawinan)
                {
                    stringPerubahan = stringPerubahan + "STATUS PERKAWINAN  : " + cekKTPTemp.Agama + " -> " + enroll.KtpAgama + " <br/>";
                    cekKTPTemp.StatusPerkawinan = enroll.KtpStatusPerkawinan;
                }

                if (cekKTPTemp.Pekerjaan != enroll.KtpPekerjaan)
                {
                    stringPerubahan = stringPerubahan + "PEKERJAAN  : " + cekKTPTemp.Agama + " -> " + enroll.KtpAgama + " <br/>";
                    cekKTPTemp.Pekerjaan = enroll.KtpPekerjaan;
                }

                if (cekKTPTemp.Kewarganegaraan != enroll.KtpKewarganegaraan)
                {
                    stringPerubahan = stringPerubahan + "KEWARGANEGARAAN  : " + cekKTPTemp.Kewarganegaraan + " -> " + enroll.KtpKewarganegaraan + " <br/>";
                    cekKTPTemp.Kewarganegaraan = enroll.KtpKewarganegaraan;
                }
                if (cekKTPTemp.MasaBerlaku != enroll.KtpMasaBerlaku)
                {
                    stringPerubahan = stringPerubahan + "MASA BERLAKU  : " + cekKTPTemp.MasaBerlaku + " -> " + enroll.KtpMasaBerlaku + " <br/>";
                    cekKTPTemp.MasaBerlaku = enroll.KtpMasaBerlaku;
                }
                if (cekKTPTemp.CIF == null)
                {
                    //var cifData = new ApiSOAResponse();
                    #region Hit SOA And Loggging it
                    if (isHitSOA == true)
                    {
                        cifData = await _cifService.GetSOAByCif(ReqSoa)
                        .ConfigureAwait(false);

                        var _status = 0;
                        if (cifData.cif != null)
                        {
                            _status = 1;
                        }

                        var _log = new Tbl_ThirdPartyLog
                        {
                            FeatureName = "ReSubmitEnrollmentFingerEncryptedOnlyISO",
                            HostUrl = ReqSoa.host,
                            Request = Newtonsoft.Json.JsonConvert.SerializeObject(ReqSoa),
                            Status = _status,
                            Response = Newtonsoft.Json.JsonConvert.SerializeObject(cifData),
                            CreatedDate = System.DateTime.Now,
                            CreatedBy = npp
                        };

                        _ = _enrollmentKTPRepository.CreateThirdPartyLog(_log);
                    }
                    else
                    {
                        try
                        {
                            var res = await _cifService.GetCIF(new NikDto { Nik = enroll.KtpNIK },
                            new NikDtoUrl { baseUrl = ReqSoa.baseUrlNonSoa, endpoint = ReqSoa.UrlEndPointNonSoa });
                            if (res.Data == null)
                            {
                                cifData.cif = null;
                            }
                            else
                            {
                                cifData.cif = res.Data.Cif;
                            };
                        }
                        catch (Exception ex)
                        {
                            cifData.cif = null;
                        }

                    }
                    #endregion

                    if (!String.IsNullOrEmpty(cifData.cif))
                    {
                        cifData.cif = cifData.cif.Trim();
                    }

                    stringPerubahan = stringPerubahan + "CIF  : " + cekKTPTemp.CIF + " -> " + cifData.cif + " <br/>";
                    cekKTPTemp.CIF = cifData.cif;
                }

                var dataKTPTemp = new Tbl_DataKTP_Demografis
                {
                    Agama = enroll.KtpAgama,
                    Alamat = enroll.KtpAlamat,
                    AlamatGoogle = enroll.KtpAlamatConvertLatlong,
                    AlamatLengkap = enroll.KtpAlamatConvertLengkap,
                    CIF = (cifData.cif),
                    NIK = enroll.KtpNIK,
                    CreatedById = Id,
                    IsActive = true,
                    IsDeleted = false,
                    JenisKelamin = enroll.KtpJanisKelamin,
                    Kecamatan = enroll.KtpKecamatan,
                    Kelurahan = enroll.KtpKelurahan,
                    CreatedByUID = enroll.UID,
                    CreatedTime = DateTime.Now,
                    GolonganDarah = enroll.KtpGolonganDarah,
                    Kewarganegaraan = enroll.KtpKewarganegaraan,
                    KodePos = enroll.KtpKodePos,
                    Kota = enroll.KtpKota,
                    Latitude = enroll.KtpLatitude,
                    Longitude = enroll.KtpLongitude,
                    MasaBerlaku = enroll.KtpMasaBerlaku,
                    Nama = enroll.KtpNama,
                    Pekerjaan = enroll.KtpPekerjaan,
                    Provinsi = enroll.KtpProvinsi,
                    RT = enroll.KtpRT,
                    RW = enroll.KtpRW,
                    StatusPerkawinan = enroll.KtpStatusPerkawinan,
                    TanggalLahir = string.IsNullOrWhiteSpace(enroll.KtpTanggalLahir) ? null : DateTime.ParseExact(enroll.KtpTanggalLahir, "dd-MM-yyyy", CultureInfo.InvariantCulture),
                    TempatLahir = enroll.KtpTempatLahir,
                    CreatedByNpp = npp,
                    CreatedByUnitCode = unitCode,
                    CreatedByUnitId = unitId,
                    IsVerified = false,
                    IsNasabahTemp = string.IsNullOrWhiteSpace(cifData.cif),
                    isEnrollFR = true
                };

                cekKTPTemp.UpdatedTime = DateTime.Now;
                cekKTPTemp.UpdatedById = Id;
                cekKTPTemp.UpdatedByUID = enroll.UID;
                cekKTPTemp.UpdatedByUnitCode = unitCode;
                cekKTPTemp.UpdatedByUnitId = unitId;
                cekKTPTemp.UpdatedByNpp = npp;

                if (string.IsNullOrWhiteSpace(stringPerubahan))
                {
                    stringPerubahan = stringPerubahan + "Data Demografi tidak ada perubahan" + " <br/>";
                }
                #endregion

                if (isEmployee)
                {
                    var _mappingData = await _enrollmentKTPRepository.MappingNppNikByNik(enroll.KtpNIK);
                    if (_mappingData == null)
                    {
                        dataNpp = new Tbl_Mapping_Pegawai_KTP
                        {
                            CreatedById = Id,
                            NIK = enroll.KtpNIK,
                            Npp = _empData.Npp,
                            CreatedByNpp = npp,
                            CreatedByUID = enroll.UID,
                            CreatedTime = DateTime.Now,
                            CreatedByUnit = unitCode,
                            UpdatedByNpp = npp,
                            UpdatedByUID = enroll.UID,
                            UpdatedByUnit = unitCode,
                            UpdatedTime = DateTime.Now,
                        };
                    }
                    else
                    {
                        dataNpp = _mappingData;
                        dataNpp.UpdatedByNpp = npp;
                        dataNpp.UpdatedByUID = enroll.UID;
                        dataNpp.UpdatedByUnit = unitCode;
                        dataNpp.UpdatedTime = DateTime.Now;
                    }
                }

                var sysPathFolder = await _sysParameterRepository.GetPathFolder("PathFolderData");
                string pathFolder = sysPathFolder.Value;

                var photoKtpLog = new Tbl_DataKTP_Photo_Log();
                var photoKTPData = await _profileRepository.GetPhotoKtp(enroll.KtpNIK)
                        .ConfigureAwait(false);
                var exPhotoKtp = photoKTPData;

                #region update Photo KTP
                if (!string.IsNullOrWhiteSpace(enroll.KtpPhotoKTP))
                {
                    byte[] imgBytes = Convert.FromBase64String(enroll.KtpPhotoKTP);

                    string filePath = "";

                    var systemParameterPath = await _sysParameterRepository.GetPathFolder("FolderPhoto")
                                                .ConfigureAwait(false);
                    string pathFolderFoto = systemParameterPath.Value;

                    string subPathFolderFoto = pathFolder + "/" + pathFolderFoto + "/" + enroll.KtpNIK + "/";

                    string fileName = "Foto_" + enroll.KtpNIK + "_" + JamServer + ".jpg";

                    if (_sftpConfig.IsActive)
                    {
                        using var stream = new MemoryStream(imgBytes);

                        (var fname, var fPath) = await stream.UploadToFTPServer(subPathFolderFoto, fileName, _sftpConfig.Host,
                            _sftpConfig.Username, _sftpConfig.Password, _sftpConfig.RootDirectory, _sftpConfig.Url).ConfigureAwait(false);

                        filePath = fPath;
                    }
                    else
                    {
                        if (!Directory.Exists(subPathFolderFoto))
                        {
                            Directory.CreateDirectory(subPathFolderFoto);
                        }

                        filePath = subPathFolderFoto + fileName;
                        File.WriteAllBytes(filePath, imgBytes);
                    }

                    if (photoKTPData == null)
                    {
                        photoKTPData = new Tbl_DataKTP_Photo
                        {
                            PathFile = filePath,
                            Nik = enroll.KtpNIK,
                            FileName = fileName,
                            IsActive = true,
                            IsDeleted = false,
                            UpdatedById = Id,
                            UpdatedByNpp = npp,
                            UpdatedByUid = enroll.UID,
                            UpdatedByUnit = unitCode,
                            UpdatedTime = DateTime.Now,
                            CreatedById = Id,
                            CreatedByNpp = npp,
                            CreatedByUid = enroll.UID,
                            CreatedByUnit = unitCode,
                            CreatedTime = DateTime.Now,
                        };
                    }
                    else
                    {
                        photoKtpLog.CreatedById = photoKTPData.CreatedById;
                        photoKtpLog.CreatedByNpp = photoKTPData.CreatedByNpp;
                        photoKtpLog.CreatedByUid = photoKTPData.CreatedByUid;
                        photoKtpLog.CreatedByUnit = photoKTPData.CreatedByUnit;
                        photoKtpLog.CreatedTime = photoKTPData.CreatedTime;
                        photoKtpLog.FileName = photoKTPData.FileName;
                        photoKtpLog.Nik = photoKTPData.Nik;
                        photoKtpLog.PathFile = photoKTPData.PathFile;

                        photoKTPData.PathFile = filePath;
                        photoKTPData.Nik = enroll.KtpNIK;
                        photoKTPData.FileName = fileName;
                        photoKTPData.IsActive = true;
                        photoKTPData.IsDeleted = false;
                        photoKTPData.UpdatedById = Id;
                        photoKTPData.UpdatedByNpp = npp;
                        photoKTPData.UpdatedByUid = enroll.UID;
                        photoKTPData.UpdatedByUnit = unitCode;
                        photoKTPData.UpdatedTime = DateTime.Now;
                    }
                }
                else
                {
                    stringPerubahan = stringPerubahan + "Request Payload Photo Tidak Ditemukan" + " <br/>";
                }
                #endregion

                var photoSignatureLog = new Tbl_DataKTP_Signature_Log();
                var photoSignatureData = await _profileRepository.GetPhotoSignature(enroll.KtpNIK)
                        .ConfigureAwait(false);
                var exphotoSignatureData = photoSignatureData;

                #region update signature
                if (!string.IsNullOrWhiteSpace(enroll.KtpSignature))
                {
                    byte[] imgBytes = Convert.FromBase64String(enroll.KtpSignature);

                    string filePath = "";

                    var systemParameterPath = await _sysParameterRepository.GetPathFolder("FolderSignature")
                                                .ConfigureAwait(false);
                    string pathFolderFoto = systemParameterPath.Value;

                    string subPathFolderSignature = pathFolder + "/" + pathFolderFoto + "/" + enroll.KtpNIK + "/";

                    string fileName = "Signature_" + enroll.KtpNIK + "_" + JamServer + ".jpg";

                    if (_sftpConfig.IsActive)
                    {
                        using var stream = new MemoryStream(imgBytes);

                        (var fname, var fPath) = await stream.UploadToFTPServer(subPathFolderSignature, fileName, _sftpConfig.Host,
                            _sftpConfig.Username, _sftpConfig.Password, _sftpConfig.RootDirectory, _sftpConfig.Url).ConfigureAwait(false);

                        filePath = fPath;
                    }
                    else
                    {
                        if (!Directory.Exists(subPathFolderSignature))
                        {
                            Directory.CreateDirectory(subPathFolderSignature);
                        }

                        filePath = subPathFolderSignature + fileName;
                        File.WriteAllBytes(filePath, imgBytes);
                    }

                    if (photoSignatureData == null)
                    {
                        photoSignatureData = new Tbl_DataKTP_Signature();
                        photoSignatureData.UpdatedById = Id;
                        photoSignatureData.UpdatedByNpp = npp;
                        photoSignatureData.UpdatedByUid = enroll.UID;
                        photoSignatureData.UpdatedTime = DateTime.Now;
                        photoSignatureData.UpdatedByUnit = unitCode;
                        photoSignatureData.IsActive = true;
                        photoSignatureData.IsDeleted = false;
                        photoSignatureData.Nik = enroll.KtpNIK;
                        photoSignatureData.FileName = fileName;
                        photoSignatureData.PathFile = filePath;
                        photoSignatureData.CreatedById = Id;
                        photoSignatureData.CreatedByNpp = npp;
                        photoSignatureData.CreatedByUid = enroll.UID;
                        photoSignatureData.CreatedByUnit = unitCode;
                        photoSignatureData.CreatedTime = DateTime.Now;
                    }
                    else
                    {
                        photoSignatureLog.CreatedById = photoSignatureData.CreatedById;
                        photoSignatureLog.CreatedByNpp = photoSignatureData.CreatedByNpp;
                        photoSignatureLog.CreatedByUid = photoSignatureData.CreatedByUid;
                        photoSignatureLog.CreatedByUnit = photoSignatureData.CreatedByUnit;
                        photoSignatureLog.CreatedTime = photoSignatureData.CreatedTime;
                        photoSignatureLog.FileName = photoSignatureData.FileName;
                        photoSignatureLog.Nik = photoSignatureData.Nik;
                        photoSignatureLog.PathFile = photoSignatureData.PathFile;

                        photoSignatureData.UpdatedById = Id;
                        photoSignatureData.UpdatedByNpp = npp;
                        photoSignatureData.UpdatedByUid = enroll.UID;
                        photoSignatureData.UpdatedTime = DateTime.Now;
                        photoSignatureData.IsActive = true;
                        photoSignatureData.IsDeleted = false;
                        photoSignatureData.Nik = enroll.KtpNIK;
                        photoSignatureData.FileName = fileName;
                        photoSignatureData.PathFile = filePath;
                    }
                }
                else
                {
                    stringPerubahan = stringPerubahan + "Request Payload Signature Tidak Ditemukan" + " <br/>";
                }
                #endregion

                #region update photo cam
                var photoCamLog = new Tbl_DataKTP_PhotoCam_Log();
                var photoCamData = await _profileRepository.GetPhotoCam(enroll.KtpNIK)
                        .ConfigureAwait(false);
                var exphotoCamData = photoCamData;

                if (!string.IsNullOrWhiteSpace(enroll.KtpPhotoCam))
                {

                    byte[] imgBytes = Convert.FromBase64String(enroll.KtpPhotoCam);

                    string filePath = "";

                    var systemParameterPath = await _sysParameterRepository.GetPathFolder("FolderWebcam")
                                                .ConfigureAwait(false);
                    string pathFolderFoto = systemParameterPath.Value;

                    string SubPathFolderPhotoCam = pathFolder + "/" + pathFolderFoto + "/" + enroll.KtpNIK + "/";

                    string fileName = "PhotoCam_" + enroll.KtpNIK + "_" + JamServer + ".txt";

                    if (_sftpConfig.IsActive)
                    {
                        using var stream = new MemoryStream(imgBytes);

                        (var fname, var fPath) = await stream.UploadToFTPServer(SubPathFolderPhotoCam, fileName, _sftpConfig.Host,
                            _sftpConfig.Username, _sftpConfig.Password, _sftpConfig.RootDirectory, _sftpConfig.Url).ConfigureAwait(false);

                        filePath = fPath;
                    }
                    else
                    {
                        if (!Directory.Exists(SubPathFolderPhotoCam))
                        {
                            Directory.CreateDirectory(SubPathFolderPhotoCam);
                        }

                        filePath = SubPathFolderPhotoCam + fileName;
                        File.WriteAllBytes(filePath, imgBytes);
                    }

                    if (photoCamData == null)
                    {
                        photoCamData = new Tbl_DataKTP_PhotoCam();
                        photoCamData.PathFile = filePath;
                        photoCamData.Nik = enroll.KtpNIK;
                        photoCamData.FileName = fileName;
                        photoCamData.IsActive = true;
                        photoCamData.IsDeleted = false;
                        photoCamData.UpdatedById = Id;
                        photoCamData.UpdatedByNpp = npp;
                        photoCamData.UpdatedByUid = enroll.UID;
                        photoCamData.UpdatedTime = DateTime.Now;
                        photoCamData.UpdatedByUnit = unitCode;
                        photoCamLog.CreatedById = photoCamData.CreatedById;
                        photoCamLog.CreatedByNpp = photoCamData.CreatedByNpp;
                        photoCamLog.CreatedByUid = photoCamData.CreatedByUid;
                        photoCamLog.CreatedByUnit = photoCamData.CreatedByUnit;
                        photoCamLog.CreatedTime = photoCamData.CreatedTime;
                    }
                    else
                    {
                        photoCamLog.CreatedById = photoCamData.CreatedById;
                        photoCamLog.CreatedByNpp = photoCamData.CreatedByNpp;
                        photoCamLog.CreatedByUid = photoCamData.CreatedByUid;
                        photoCamLog.CreatedByUnit = photoCamData.CreatedByUnit;
                        photoCamLog.CreatedTime = photoCamData.CreatedTime;
                        photoCamLog.FileName = photoCamData.FileName;
                        photoCamLog.Nik = photoCamData.Nik;
                        photoCamLog.PathFile = photoCamData.PathFile;

                        photoCamData.PathFile = filePath;
                        photoCamData.Nik = enroll.KtpNIK;
                        photoCamData.FileName = fileName;
                        photoCamData.IsActive = true;
                        photoCamData.IsDeleted = false;
                        photoCamData.UpdatedById = Id;
                        photoCamData.UpdatedByNpp = npp;
                        photoCamData.UpdatedByUid = enroll.UID;
                        photoCamData.UpdatedTime = DateTime.Now;
                    }
                }
                else
                {
                    if (photoCamData != null)
                    {
                        photoCamLog.CreatedById = photoCamData.CreatedById;
                        photoCamLog.CreatedByNpp = photoCamData.CreatedByNpp;
                        photoCamLog.CreatedByUid = photoCamData.CreatedByUid;
                        photoCamLog.CreatedByUnit = photoCamData.CreatedByUnit;
                        photoCamLog.CreatedTime = photoCamData.CreatedTime;
                        photoCamLog.FileName = photoCamData.FileName;
                        photoCamLog.Nik = photoCamData.Nik;
                        photoCamLog.PathFile = photoCamData.PathFile;
                        photoCamData = new Tbl_DataKTP_PhotoCam();
                    }
                    stringPerubahan = stringPerubahan + "Request Payload Photo Cam Tidak Ditemukan" + " <br/>";
                }

                #endregion

                #region finger
                var exPhotoFinger = new List<Tbl_DataKTP_Finger>();

                var exPhotoFingerEmployee = new List<Tbl_DataKTP_Finger_Employee>();

                var photoFingerLogs = new List<Tbl_DataKTP_Finger_Log>();

                var photoFingers = new List<Tbl_DataKTP_Finger>();

                var photoFingersEmployee = new List<Tbl_DataKTP_Finger_Employee>();

                var photoFingersEmployeeLogs = new List<Tbl_DataKTP_Finger_Employee_Log>();

                var photoFingerExisting = await _profileRepository.GetPhotoFingerExisting(enroll.KtpNIK).ConfigureAwait(false);
                var photoFingerEmployeeExisting = await _profileRepository.GetPhotoFingerEmployeeExisting(enroll.KtpNIK).ConfigureAwait(false);

                foreach (var em in photoFingerEmployeeExisting)
                {
                    photoFingersEmployeeLogs.Add(new Tbl_DataKTP_Finger_Employee_Log
                    {
                        CreatedById = em.CreatedById,
                        CreatedByNpp = em.CreatedByNpp,
                        CreatedByUid = em.CreatedByUid,
                        CreatedByUnit = em.CreatedByUnit,
                        CreatedByUnitId = em.CreatedByUnitId,
                        CreatedTime = em.CreatedTime,
                        //FileJari = em.FileJari,
                        FileName = em.FileName,
                        FileNameISO = em.FileNameISO,
                        Nik = em.Nik,
                        PathFile = em.PathFile,
                        PathFileISO = em.PathFileISO,
                        TypeFinger = em.TypeFinger
                    });
                }

                foreach (var el in photoFingerExisting)
                {
                    photoFingerLogs.Add(new Tbl_DataKTP_Finger_Log
                    {
                        CreatedById = el.CreatedById,
                        CreatedByNpp = el.CreatedByNpp,
                        CreatedByUid = el.CreatedByUid,
                        CreatedByUnit = el.CreatedByUnit,
                        CreatedByUnitId = el.CreatedByUnitId,
                        CreatedTime = el.CreatedTime,
                        //FileJari = el.FileJari,
                        FileJariISO = el.FileJariISO,
                        FileName = el.FileName,
                        FileNameISO = el.FileNameISO,
                        Nik = el.Nik,
                        PathFile = el.PathFile,
                        PathFileISO = el.PathFileISO,
                        TypeFinger = el.TypeFinger
                    });
                }

                if (!string.IsNullOrWhiteSpace(enroll.KtpFingerKanan))
                {

                    var photoFingerDataEmployee = new Tbl_DataKTP_Finger_Employee();

                    string imageEncrypted = enroll.KtpFingerKanan.Encrypt(Phrase.FileEncryption);

                    string filePath = "";
                    string filePathIso = "";

                    var systemParameterPath = await _sysParameterRepository.GetPathFolder("FolderFinger")
                                                .ConfigureAwait(false);
                    string pathFolderFoto = systemParameterPath.Value;

                    string subPathFolderPhotoFinger = pathFolder + "/" + pathFolderFoto + "/" + enroll.KtpNIK + "/";

                    string JenisJari = enroll.KtpTypeJariKanan.Replace(" ", "");
                    string fileName = "PhotoFinger_" + JenisJari + "_" + enroll.KtpNIK + "_" + JamServer + ".txt";
                    string fileNameIso = "";

                    if (_sftpConfig.IsActive)
                    {
                        using var stream = new MemoryStream(Encoding.ASCII.GetBytes(imageEncrypted));

                        (var fname, var fPath) = await stream.UploadToFTPServer(subPathFolderPhotoFinger, fileName, _sftpConfig.Host,
                            _sftpConfig.Username, _sftpConfig.Password, _sftpConfig.RootDirectory, _sftpConfig.Url).ConfigureAwait(false);

                        filePath = fPath;
                    }
                    else
                    {
                        if (!Directory.Exists(subPathFolderPhotoFinger))
                        {
                            Directory.CreateDirectory(subPathFolderPhotoFinger);
                        }

                        filePath = subPathFolderPhotoFinger + fileName;
                        File.WriteAllText(filePath, imageEncrypted);
                    }

                    string isoEncrypted = "";

                    if (!string.IsNullOrWhiteSpace(enroll.KtpFingerKananIso))
                    {
                        isoEncrypted = enroll.KtpFingerKananIso.Encrypt(Phrase.FileEncryption);
                        fileNameIso = "PhotoFingerISO_" + JenisJari + "_" + enroll.KtpNIK + "_" + JamServer + ".txt";

                        if (_sftpConfig.IsActive)
                        {
                            using var stream = new MemoryStream(Encoding.ASCII.GetBytes(isoEncrypted));

                            (var fname, var fPath) = await stream.UploadToFTPServer(subPathFolderPhotoFinger, fileNameIso, _sftpConfig.Host,
                                _sftpConfig.Username, _sftpConfig.Password, _sftpConfig.RootDirectory, _sftpConfig.Url).ConfigureAwait(false);

                            filePathIso = fPath;
                        }
                        else
                        {
                            if (!Directory.Exists(subPathFolderPhotoFinger))
                            {
                                Directory.CreateDirectory(subPathFolderPhotoFinger);
                            }

                            filePathIso = subPathFolderPhotoFinger + fileNameIso;
                            File.WriteAllText(filePathIso, isoEncrypted);
                        }
                    }

                    //var typeFingerMain = await _fingerRepository.GetFingersEnrolled(enroll.KtpNIK);

                    var typeFingerMain = new List<FingerByNik> {
                            new FingerByNik { NIK = enroll.KtpNIK, TypeFinger = (enroll.KtpTypeJariKanan != null) ? enroll.KtpTypeJariKanan : ""},
                            new FingerByNik { NIK = enroll.KtpNIK, TypeFinger = (enroll.KtpTypeJariKiri != null) ? enroll.KtpTypeJariKiri : ""},
                        };

                    foreach (var i in typeFingerMain)
                    {

                        if (i.TypeFinger.Contains("Kanan"))
                        {
                            var photoFingerData = await _profileRepository.GetPhotoFinger(enroll.KtpNIK, i.TypeFinger).ConfigureAwait(false);

                            if (isEmployee)
                            {
                                photoFingerDataEmployee = await _profileRepository.GetPhotoFingerEmployee(enroll.KtpNIK, i.TypeFinger)
                                .ConfigureAwait(false);
                            }

                            if (photoFingerData == null)
                            {
                                photoFingerData = new Tbl_DataKTP_Finger
                                {
                                    CreatedById = Id,
                                    IsActive = true,
                                    IsDeleted = false,
                                    Nik = enroll.KtpNIK,
                                    PathFile = filePath,
                                    PathFileISO = filePathIso,
                                    CreatedByNpp = npp,
                                    CreatedByUid = enroll.UID,
                                    CreatedTime = DateTime.Now,
                                    CreatedByUnit = unitCode,
                                    FileName = fileName,
                                    FileNameISO = fileNameIso,
                                    FileJariISO = isoEncrypted,
                                    TypeFinger = enroll.KtpTypeJariKanan
                                };

                                photoFingers.Add(new Tbl_DataKTP_Finger
                                {
                                    //CreatedById = photoFingerData.CreatedById,
                                    CreatedById = Id,
                                    IsActive = true,
                                    IsDeleted = false,
                                    Nik = enroll.KtpNIK,
                                    PathFile = filePath,
                                    PathFileISO = filePathIso,
                                    //CreatedByNpp = photoFingerData.CreatedByNpp,
                                    //CreatedByUid = photoFingerData.CreatedByUid,
                                    //CreatedTime = photoFingerData.CreatedTime,
                                    CreatedByNpp = npp,
                                    CreatedByUid = enroll.UID,
                                    CreatedTime = DateTime.Now,
                                    UpdatedById = Id,
                                    UpdatedByNpp = npp,
                                    UpdatedByUid = enroll.UID,
                                    UpdatedByUnit = unitCode,
                                    //CreatedByUnitId = photoFingerData.CreatedByUnitId,
                                    CreatedByUnitId = unitId,
                                    CreatedByUnit = photoFingerData.CreatedByUnit,
                                    //FileJari = photoFingerData.FileJari,
                                    FileJariISO = isoEncrypted,
                                    UpdatedTime = DateTime.Now,
                                    FileName = fileName,
                                    FileNameISO = fileNameIso,
                                    TypeFinger = enroll.KtpTypeJariKanan
                                });
                            }
                            else
                            {

                                photoFingers.Add(new Tbl_DataKTP_Finger
                                {
                                    //CreatedById = photoFingerData.CreatedById,
                                    CreatedById = Id,
                                    IsActive = true,
                                    IsDeleted = false,
                                    Nik = enroll.KtpNIK,
                                    PathFile = filePath,
                                    PathFileISO = filePathIso,
                                    //CreatedByNpp = photoFingerData.CreatedByNpp,
                                    //CreatedByUid = photoFingerData.CreatedByUid,
                                    //CreatedTime = photoFingerData.CreatedTime,
                                    CreatedByNpp = npp,
                                    CreatedByUid = enroll.UID,
                                    CreatedTime = DateTime.Now,
                                    UpdatedById = Id,
                                    UpdatedByNpp = npp,
                                    UpdatedByUid = enroll.UID,
                                    UpdatedByUnit = unitCode,
                                    //CreatedByUnitId = photoFingerData.CreatedByUnitId,
                                    CreatedByUnitId = unitId,
                                    CreatedByUnit = photoFingerData.CreatedByUnit,
                                    //FileJari = photoFingerData.FileJari,
                                    FileJariISO = isoEncrypted,
                                    UpdatedTime = DateTime.Now,
                                    FileName = fileName,
                                    FileNameISO = fileNameIso,
                                    TypeFinger = enroll.KtpTypeJariKanan
                                });

                            }

                            if (isEmployee)
                            {

                                photoFingersEmployee.Add(new Tbl_DataKTP_Finger_Employee
                                {
                                    CreatedById = Id,
                                    IsActive = true,
                                    IsDeleted = false,
                                    Nik = enroll.KtpNIK,
                                    PathFile = filePath,
                                    CreatedByNpp = npp,
                                    CreatedByUid = enroll.UID,
                                    CreatedTime = DateTime.Now,
                                    UpdatedById = Id,
                                    UpdatedByNpp = npp,
                                    UpdatedByUid = enroll.UID,
                                    UpdatedByUnit = unitCode,
                                    CreatedByUnitId = unitId,
                                    //FileJari = enroll.KtpFingerKanan,
                                    UpdatedTime = DateTime.Now,
                                    FileName = fileName,
                                    FileJariISO = isoEncrypted,
                                    FileNameISO = fileNameIso,
                                    PathFileISO = filePathIso,
                                    TypeFinger = enroll.KtpTypeJariKanan,
                                });
                            }
                        }
                    }
                }
                else
                {
                    #region get data if no payload but we have to replace all data with no data
                    var photoFingerDataEmployee = new Tbl_DataKTP_Finger_Employee();
                    //var typeFingerMain = await _fingerRepository.GetFingersEnrolled(enroll.KtpNIK);

                    var typeFingerMain = new List<FingerByNik> {
                            new FingerByNik { NIK = enroll.KtpNIK, TypeFinger = (enroll.KtpTypeJariKanan != null) ? enroll.KtpTypeJariKanan : ""},
                            new FingerByNik { NIK = enroll.KtpNIK, TypeFinger = (enroll.KtpTypeJariKiri != null) ? enroll.KtpTypeJariKiri : ""},
                        };

                    foreach (var i in typeFingerMain)
                    {

                        if (i.TypeFinger.Contains("Kanan"))
                        {
                            var photoFingerData = await _profileRepository.GetPhotoFinger(enroll.KtpNIK, i.TypeFinger).ConfigureAwait(false);

                            if (isEmployee)
                            {
                                photoFingerDataEmployee = await _profileRepository.GetPhotoFingerEmployee(enroll.KtpNIK, i.TypeFinger)
                                .ConfigureAwait(false);
                            }



                        }
                    }

                    #endregion

                    stringPerubahan = stringPerubahan + "Request Payload Finger Kanan Tidak Ditemukan" + " <br/>";
                }

                if (!string.IsNullOrWhiteSpace(enroll.KtpFingerKiri))
                {
                    //var photoFingerData = await _profileRepository.GetPhotoFinger(enroll.KtpNIK, enroll.KtpTypeJariKiri)
                    //    .ConfigureAwait(false);

                    var photoFingerDataEmployee = new Tbl_DataKTP_Finger_Employee();

                    //if (isEmployee)
                    //{
                    //    photoFingerDataEmployee = await _profileRepository.GetPhotoFingerEmployee(enroll.KtpNIK, enroll.KtpTypeJariKiri)
                    //    .ConfigureAwait(false);
                    //}

                    string imageEncrypted = enroll.KtpFingerKiri.Encrypt(Phrase.FileEncryption);

                    string filePath = "";
                    string filePathIso = "";

                    var systemParameterPath = await _sysParameterRepository.GetPathFolder("FolderFinger")
                                                .ConfigureAwait(false);
                    string pathFolderFoto = systemParameterPath.Value;

                    string subPathFolderPhotoFinger = pathFolder + "/" + pathFolderFoto + "/" + enroll.KtpNIK + "/";

                    string JenisJari = enroll.KtpTypeJariKiri.Replace(" ", "");
                    string fileName = "PhotoFinger_" + JenisJari + "_" + enroll.KtpNIK + "_" + JamServer + ".txt";
                    string fileNameIso = "";

                    if (_sftpConfig.IsActive)
                    {
                        using var stream = new MemoryStream(Encoding.ASCII.GetBytes(imageEncrypted));

                        (var fname, var fPath) = await stream.UploadToFTPServer(subPathFolderPhotoFinger, fileName, _sftpConfig.Host,
                            _sftpConfig.Username, _sftpConfig.Password, _sftpConfig.RootDirectory, _sftpConfig.Url).ConfigureAwait(false);

                        filePath = fPath;
                    }
                    else
                    {
                        if (!Directory.Exists(subPathFolderPhotoFinger))
                        {
                            Directory.CreateDirectory(subPathFolderPhotoFinger);
                        }

                        filePath = subPathFolderPhotoFinger + fileName;
                        File.WriteAllText(filePath, imageEncrypted);
                    }

                    string isoEncrypted = "";

                    if (!string.IsNullOrWhiteSpace(enroll.KtpFingerKiriIso))
                    {
                        isoEncrypted = enroll.KtpFingerKiriIso.Encrypt(Phrase.FileEncryption);
                        fileNameIso = "PhotoFingerISO_" + JenisJari + "_" + enroll.KtpNIK + "_" + JamServer + ".txt";

                        if (_sftpConfig.IsActive)
                        {
                            using var stream = new MemoryStream(Encoding.ASCII.GetBytes(isoEncrypted));

                            (var fname, var fPath) = await stream.UploadToFTPServer(subPathFolderPhotoFinger, fileNameIso, _sftpConfig.Host,
                                _sftpConfig.Username, _sftpConfig.Password, _sftpConfig.RootDirectory, _sftpConfig.Url).ConfigureAwait(false);

                            filePathIso = fPath;
                        }
                        else
                        {
                            if (!Directory.Exists(subPathFolderPhotoFinger))
                            {
                                Directory.CreateDirectory(subPathFolderPhotoFinger);
                            }

                            filePathIso = subPathFolderPhotoFinger + fileNameIso;
                            File.WriteAllText(filePathIso, isoEncrypted);
                        }
                    }


                    //var typeFingerMain = await _fingerRepository.GetFingersEnrolled(enroll.KtpNIK);

                    var typeFingerMain = new List<FingerByNik> {
                            new FingerByNik { NIK = enroll.KtpNIK, TypeFinger = (enroll.KtpTypeJariKanan != null) ? enroll.KtpTypeJariKanan : ""},
                            new FingerByNik { NIK = enroll.KtpNIK, TypeFinger = (enroll.KtpTypeJariKiri != null) ? enroll.KtpTypeJariKiri : ""},
                        };

                    foreach (var i in typeFingerMain)
                    {

                        if (i.TypeFinger.Contains("Kiri"))
                        {
                            var photoFingerData = await _profileRepository.GetPhotoFinger(enroll.KtpNIK, i.TypeFinger).ConfigureAwait(false);

                            if (isEmployee)
                            {
                                photoFingerDataEmployee = await _profileRepository.GetPhotoFingerEmployee(enroll.KtpNIK, i.TypeFinger)
                                .ConfigureAwait(false);
                            }

                            if (photoFingerData == null)
                            {
                                photoFingerData = new Tbl_DataKTP_Finger
                                {
                                    CreatedById = Id,
                                    IsActive = true,
                                    IsDeleted = false,
                                    Nik = enroll.KtpNIK,
                                    PathFile = filePath,
                                    PathFileISO = filePathIso,
                                    CreatedByNpp = npp,
                                    CreatedByUid = enroll.UID,
                                    CreatedTime = DateTime.Now,
                                    FileName = fileName,
                                    FileNameISO = fileNameIso,
                                    FileJariISO = isoEncrypted,
                                    TypeFinger = enroll.KtpTypeJariKiri
                                };

                                photoFingers.Add(new Tbl_DataKTP_Finger
                                {
                                    //CreatedById = photoFingerData.CreatedById,
                                    CreatedById = Id,
                                    IsActive = true,
                                    IsDeleted = false,
                                    Nik = enroll.KtpNIK,
                                    PathFile = filePath,
                                    PathFileISO = filePathIso,
                                    //CreatedByNpp = photoFingerData.CreatedByNpp,
                                    //CreatedByUid = photoFingerData.CreatedByUid,
                                    //CreatedTime = photoFingerData.CreatedTime,
                                    CreatedByNpp = npp,
                                    CreatedByUid = enroll.UID,
                                    CreatedTime = DateTime.Now,
                                    UpdatedById = Id,
                                    UpdatedByNpp = npp,
                                    UpdatedByUid = enroll.UID,
                                    UpdatedByUnit = unitCode,
                                    //CreatedByUnitId = photoFingerData.CreatedByUnitId,
                                    CreatedByUnitId = unitId,
                                    CreatedByUnit = photoFingerData.CreatedByUnit,
                                    //FileJari = photoFingerData.FileJari,
                                    FileJariISO = isoEncrypted,
                                    UpdatedTime = DateTime.Now,
                                    FileName = fileName,
                                    FileNameISO = fileNameIso,
                                    TypeFinger = enroll.KtpTypeJariKiri
                                });


                            }
                            else
                            {
                                photoFingers.Add(new Tbl_DataKTP_Finger
                                {
                                    //CreatedById = photoFingerData.CreatedById,
                                    CreatedById = Id,
                                    IsActive = true,
                                    IsDeleted = false,
                                    Nik = enroll.KtpNIK,
                                    PathFile = filePath,
                                    PathFileISO = filePathIso,
                                    //CreatedByNpp = photoFingerData.CreatedByNpp,
                                    //CreatedByUid = photoFingerData.CreatedByUid,
                                    //CreatedTime = photoFingerData.CreatedTime,
                                    CreatedByNpp = npp,
                                    CreatedByUid = enroll.UID,
                                    CreatedTime = DateTime.Now,
                                    UpdatedById = Id,
                                    UpdatedByNpp = npp,
                                    UpdatedByUid = enroll.UID,
                                    UpdatedByUnit = unitCode,
                                    //CreatedByUnitId = photoFingerData.CreatedByUnitId,
                                    CreatedByUnitId = unitId,
                                    CreatedByUnit = photoFingerData.CreatedByUnit,
                                    //FileJari = photoFingerData.FileJari,
                                    FileJariISO = isoEncrypted,
                                    UpdatedTime = DateTime.Now,
                                    FileName = fileName,
                                    FileNameISO = fileNameIso,
                                    TypeFinger = enroll.KtpTypeJariKiri
                                });


                            }

                            if (isEmployee)
                            {

                                photoFingersEmployee.Add(new Tbl_DataKTP_Finger_Employee
                                {
                                    CreatedById = Id,
                                    IsActive = true,
                                    IsDeleted = false,
                                    Nik = enroll.KtpNIK,
                                    PathFile = filePath,
                                    CreatedByNpp = npp,
                                    CreatedByUid = enroll.UID,
                                    CreatedTime = DateTime.Now,
                                    UpdatedById = Id,
                                    UpdatedByNpp = npp,
                                    UpdatedByUid = enroll.UID,
                                    UpdatedByUnit = unitCode,
                                    CreatedByUnitId = unitId,
                                    //FileJari = enroll.KtpFingerKiri,
                                    UpdatedTime = DateTime.Now,
                                    FileName = fileName,
                                    FileJariISO = isoEncrypted,
                                    FileNameISO = fileNameIso,
                                    PathFileISO = filePathIso,
                                    TypeFinger = enroll.KtpTypeJariKiri,
                                });


                            }
                        }
                    }
                }
                else
                {
                    #region get data if no payload but we have to replace all data with no data
                    var photoFingerDataEmployee = new Tbl_DataKTP_Finger_Employee();
                    //var typeFingerMain = await _fingerRepository.GetFingersEnrolled(enroll.KtpNIK);
                    var typeFingerMain = new List<FingerByNik> {
                            new FingerByNik { NIK = enroll.KtpNIK, TypeFinger = (enroll.KtpTypeJariKanan != null) ? enroll.KtpTypeJariKanan : ""},
                            new FingerByNik { NIK = enroll.KtpNIK, TypeFinger = (enroll.KtpTypeJariKiri != null) ? enroll.KtpTypeJariKiri : ""},
                        };

                    foreach (var i in typeFingerMain)
                    {

                        if (i.TypeFinger.Contains("Kiri"))
                        {
                            var photoFingerData = await _profileRepository.GetPhotoFinger(enroll.KtpNIK, i.TypeFinger).ConfigureAwait(false);

                            if (isEmployee)
                            {
                                photoFingerDataEmployee = await _profileRepository.GetPhotoFingerEmployee(enroll.KtpNIK, i.TypeFinger)
                                .ConfigureAwait(false);
                            }

                        }
                    }
                    #endregion

                    stringPerubahan = stringPerubahan + "Request Payload Finger Kanan Tidak Ditemukan" + " <br/>";
                }
                #endregion
                
                var (status, msg) = await _enrollmentKTPRepository.InsertEnrollFlow3(dataKTPTemp, logDemografi, photoKTPData, photoKtpLog, photoSignatureData,
                    photoSignatureLog, photoCamData, photoCamLog, photoFingers, photoFingerLogs, exPhotoKtp, exphotoSignatureData, exphotoCamData, photoFingerExisting, dataNpp, dataReaderLog, photoFingersEmployee, photoFingerEmployeeExisting, photoFingersEmployeeLogs);

                if (status)
                {
                    return (_SuccessMessageConfig.NasabahSuksesUpdate, (int)ServiceResponseStatus.SUKSES, cekKTPTemp.CIF);
                }
                else
                {
                    return (_ErrorMessageConfig.DemografiGagalEnroll, (int)ServiceResponseStatus.ERROR, msg + " " + stringPerubahan);
                }
            }
            #endregion
            else
            {
                    #region Logging Reader Activity
                    _ = _alatReaderRepository.CreateLogActivity2(dataReaderActivityLog);
                    _ = _alatReaderRepository.CreateAlatReaderLog(dataReaderLog);
                    #endregion

                    return (_ErrorMessageConfig.DemografiTidakDitemukan, (int)ServiceResponseStatus.Data_Empty, "");
                }
                
            #endregion
        }
        #endregion

        public async Task<(string msg, int code, string cif)> ReSubmitEnrollmentFingerEncryptedOnly(bool isHitSOA, ApiSOA ReqSoa, EnrollKTP enroll, int Id, string npp,
            string unitCode, int unitId)
        {
            #region check data is employee or not
            bool isEmployee = false;
            var _empData = await _enrollmentKTPRepository.IsEmployee(enroll.KtpNIK);
            if (_empData != null)
            {
                isEmployee = true;
            }
            #endregion

            #region check UID
            var dataReader = await _dataReaderRepository.GetDatareaderUid(enroll.UID).ConfigureAwait(false);

            var dataReaderLog = new Tbl_MasterAlatReaderLog();
            var dataReaderActivityLog = new Tbl_MasterAlatReaderLogActvity();
            var dataNpp = new Tbl_Mapping_Pegawai_KTP();

            if (dataReader != null)
            {
                dataReaderLog = new Tbl_MasterAlatReaderLog
                {
                    CreatedBy_Id = Id,
                    CreatedTime = DateTime.Now,
                    IsActive = true,
                    IsDeleted = false,
                    Nik = enroll.KtpNIK,
                    PegawaiId = Id,
                    Serial_Number = dataReader.SN_Unit,
                    Type = "Updates Enroll",
                    Uid = enroll.UID
                };

                dataReaderActivityLog = new Tbl_MasterAlatReaderLogActvity
                {
                    CreatedBy_Id = Id,
                    IsActive = true,
                    IsDeleted = false,
                    KodeUnit = unitCode,
                    LastIP = enroll.IpAddress,
                    NIK = enroll.KtpNIK,
                    CreatedTime = DateTime.Now,
                    NppPegawai = npp,
                    PegawaiId = Id,
                    Type = "Updates Enroll",
                    UID = enroll.UID,
                    UnitId = unitId
                };
            }
            #endregion

            string JamServer = DateTime.Now.ToString("ddMMyyyyHHmmss");

            if (enroll.KtpTanggalLahir != null)
            {
                enroll.KtpTanggalLahir = enroll.KtpTanggalLahir.Replace(".", "");
                if (enroll.KtpTanggalLahir.Contains("/"))
                {
                    enroll.KtpTanggalLahir = enroll.KtpTanggalLahir.Replace("/", "-");
                }
                string[] arrayTanggl = enroll.KtpTanggalLahir.Split("-");
                string hari = "";
                string bulan = "";
                string tahun = "";

                if (arrayTanggl != null)
                {
                    if (arrayTanggl[0] != null)
                    {
                        if (arrayTanggl[0].Length != 2)
                        {
                            hari = "0" + arrayTanggl[0];

                        }
                        else
                        {
                            hari = arrayTanggl[0];

                        }
                    }

                    if (arrayTanggl[1] != null)
                    {
                        if (arrayTanggl[1].Length != 2)
                        {
                            bulan = "0" + arrayTanggl[1];

                        }
                        else
                        {
                            bulan = arrayTanggl[1];

                        }
                    }

                    if (arrayTanggl[2] != null)
                    {
                        tahun = arrayTanggl[2];
                    }
                }

                enroll.KtpTanggalLahir = hari + "-" + bulan + "-" + tahun;
            }

            var cekKTP = await _profileRepository.GetDataDemografis(enroll.KtpNIK)
                .ConfigureAwait(false);

            var cekKTPTemp = await _profileRepository.GetDataDemografisTempOnProgress(enroll.KtpNIK)
               .ConfigureAwait(false);

            #region update data ktp
            if (cekKTP != null)
            {
                #region update demografi
                var logDemografi = new Tbl_DataKTP_Demografis_Log
                {
                    Agama = cekKTP.Agama,
                    Alamat = cekKTP.Alamat,
                    AlamatGoogle = cekKTP.AlamatGoogle,
                    AlamatLengkap = cekKTP.AlamatLengkap,
                    CreatedById = cekKTP.CreatedById,
                    CreatedByUID = cekKTP.CreatedByUID,
                    CreatedByUnitId = cekKTP.CreatedByUnitId,
                    CreatedTime = cekKTP.CreatedTime,
                    Desa = cekKTP.Desa,
                    GolonganDarah = cekKTP.GolonganDarah,
                    JenisKelamin = cekKTP.JenisKelamin,
                    Kecamatan = cekKTP.Kecamatan,
                    Kelurahan = cekKTP.Kelurahan,
                    Kewarganegaraan = cekKTP.Kewarganegaraan,
                    KodePos = cekKTP.KodePos,
                    Kota = cekKTP.Kota,
                    Latitude = cekKTP.Latitude,
                    Longitude = cekKTP.Longitude,
                    MasaBerlaku = cekKTP.MasaBerlaku,
                    Nama = cekKTP.Nama,
                    NIK = cekKTP.NIK,
                    Pekerjaan = cekKTP.Pekerjaan,
                    Provinsi = cekKTP.Provinsi,
                    RT = cekKTP.RT,
                    RW = cekKTP.RW,
                    StatusPerkawinan = cekKTP.StatusPerkawinan,
                    TanggalLahir = cekKTP.TanggalLahir,
                    TempatLahir = cekKTP.TempatLahir,
                    CIF = cekKTP.CIF,
                    CreatedByNpp = cekKTP.CreatedByNpp
                };

                var stringPerubahan = "";

                if (cekKTP.Nama != enroll.KtpNama)
                {
                    stringPerubahan = stringPerubahan + "NAMA : " + cekKTP.Nama + " -> " + enroll.KtpNama + " <br/>";
                    cekKTP.Nama = enroll.KtpNama;
                }

                if (cekKTP.TempatLahir != enroll.KtpTempatLahir)
                {
                    stringPerubahan = stringPerubahan + "TEMPAT LAHIR : " + cekKTP.TempatLahir + " -> " + enroll.KtpTempatLahir + " <br/>";
                    cekKTP.TempatLahir = enroll.KtpTempatLahir;
                }

                if (enroll.KtpTanggalLahir != null)
                {
                    var _ttl = DateTime.ParseExact(enroll.KtpTanggalLahir, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                    if (cekKTP.TanggalLahir != _ttl)
                    {
                        stringPerubahan = stringPerubahan + "TANGGAL LAHIR : " + cekKTP.TanggalLahir + " -> " + enroll.KtpTanggalLahir + " <br/>";
                        cekKTP.TanggalLahir = DateTime.ParseExact(enroll.KtpTanggalLahir, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                    }
                }

                if (cekKTP.JenisKelamin != enroll.KtpJanisKelamin)
                {
                    stringPerubahan = stringPerubahan + "JENIS KELAMIN : " + cekKTP.JenisKelamin + " -> " + enroll.KtpJanisKelamin + " <br/>";
                    cekKTP.JenisKelamin = enroll.KtpJanisKelamin;
                }

                if (cekKTP.GolonganDarah != enroll.KtpGolonganDarah)
                {
                    stringPerubahan = stringPerubahan + "GOLONGAN DARAH : " + cekKTP.GolonganDarah + " -> " + enroll.KtpGolonganDarah + " <br/>";
                    cekKTP.GolonganDarah = enroll.KtpGolonganDarah;
                }

                if (cekKTP.Alamat != enroll.KtpAlamat)
                {
                    stringPerubahan = stringPerubahan + "ALAMAT : " + cekKTP.Alamat + " -> " + enroll.KtpAlamat + " <br/>";
                    cekKTP.Alamat = enroll.KtpAlamat;
                }

                if (cekKTP.RT != enroll.KtpRT)
                {
                    stringPerubahan = stringPerubahan + "RT : " + cekKTP.RT + " -> " + enroll.KtpRT + " <br/>";
                    cekKTP.RT = enroll.KtpRT;
                }

                if (cekKTP.RW != enroll.KtpRW)
                {
                    stringPerubahan = stringPerubahan + "RW : " + cekKTP.RW + " -> " + enroll.KtpRW + " <br/>";
                    cekKTP.RW = enroll.KtpRW;
                }

                if (cekKTP.Kelurahan != enroll.KtpKelurahan)
                {
                    stringPerubahan = stringPerubahan + "KELURAHAN : " + cekKTP.Kelurahan + " -> " + enroll.KtpKelurahan + " <br/>";
                    cekKTP.Kelurahan = enroll.KtpKelurahan;
                }

                if (cekKTP.Kecamatan != enroll.KtpKecamatan)
                {
                    stringPerubahan = stringPerubahan + "KECAMATAN : " + cekKTP.Kecamatan + " -> " + enroll.KtpKecamatan + " <br/>";
                    cekKTP.Kecamatan = enroll.KtpKecamatan;
                }

                if (cekKTP.Kota != enroll.KtpKota)
                {
                    stringPerubahan = stringPerubahan + "KOTA/KABUPATEN : " + cekKTP.Kota + " -> " + enroll.KtpKota + " <br/>";
                    cekKTP.Kota = enroll.KtpKota;
                }

                if (cekKTP.Provinsi != enroll.KtpProvinsi)
                {
                    stringPerubahan = stringPerubahan + "PROVINSI  : " + cekKTP.Provinsi + " -> " + enroll.KtpProvinsi + " <br/>";
                    cekKTP.Provinsi = enroll.KtpProvinsi;
                }

                if (cekKTP.Agama != enroll.KtpAgama)
                {
                    stringPerubahan = stringPerubahan + "AGAMA  : " + cekKTP.Agama + " -> " + enroll.KtpAgama + " <br/>";
                    cekKTP.Agama = enroll.KtpAgama;
                }

                if (cekKTP.StatusPerkawinan != enroll.KtpStatusPerkawinan)
                {
                    stringPerubahan = stringPerubahan + "STATUS PERKAWINAN  : " + cekKTP.Agama + " -> " + enroll.KtpAgama + " <br/>";
                    cekKTP.StatusPerkawinan = enroll.KtpStatusPerkawinan;
                }

                if (cekKTP.Pekerjaan != enroll.KtpPekerjaan)
                {
                    stringPerubahan = stringPerubahan + "PEKERJAAN  : " + cekKTP.Agama + " -> " + enroll.KtpAgama + " <br/>";
                    cekKTP.Pekerjaan = enroll.KtpPekerjaan;
                }

                if (cekKTP.Kewarganegaraan != enroll.KtpKewarganegaraan)
                {
                    stringPerubahan = stringPerubahan + "KEWARGANEGARAAN  : " + cekKTP.Kewarganegaraan + " -> " + enroll.KtpKewarganegaraan + " <br/>";
                    cekKTP.Kewarganegaraan = enroll.KtpKewarganegaraan;
                }
                if (cekKTP.MasaBerlaku != enroll.KtpMasaBerlaku)
                {
                    stringPerubahan = stringPerubahan + "MASA BERLAKU  : " + cekKTP.MasaBerlaku + " -> " + enroll.KtpMasaBerlaku + " <br/>";
                    cekKTP.MasaBerlaku = enroll.KtpMasaBerlaku;
                }
                if (cekKTP.CIF == null)
                {
                    var cifData = new ApiSOAResponse();
                    #region Hit SOA And Loggging it
                    if (isHitSOA == true)
                    {
                        cifData = await _cifService.GetSOAByCif(ReqSoa)
                        .ConfigureAwait(false);

                        var _status = 0;
                        if (cifData.cif != null)
                        {
                            _status = 1;
                        }

                        var _log = new Tbl_ThirdPartyLog
                        {
                            FeatureName = "ReSubmitEnrollmentFingerEncryptedOnly",
                            HostUrl = ReqSoa.host,
                            Request = Newtonsoft.Json.JsonConvert.SerializeObject(ReqSoa),
                            Status = _status,
                            Response = Newtonsoft.Json.JsonConvert.SerializeObject(cifData),
                            CreatedDate = System.DateTime.Now,
                            CreatedBy = npp
                        };

                        _ = _enrollmentKTPRepository.CreateThirdPartyLog(_log);
                    }
                    else
                    {
                        var res = await _cifService.GetCIF(new NikDto { Nik = enroll.KtpNIK },
                            new NikDtoUrl { baseUrl = ReqSoa.baseUrlNonSoa, endpoint = ReqSoa.UrlEndPointNonSoa });
                        if (res.Data == null)
                        {
                            cifData.cif = null;
                        }
                        else
                        {
                            cifData.cif = res.Data.Cif;
                        };
                    }
                    #endregion

                    if (!String.IsNullOrEmpty(cifData.cif))
                    {
                        cifData.cif = cifData.cif.Trim();
                    }

                    stringPerubahan = stringPerubahan + "CIF  : " + cekKTP.CIF + " -> " + cifData.cif + " <br/>";
                    cekKTP.CIF = cifData.cif;
                }

                cekKTP.UpdatedTime = DateTime.Now;
                cekKTP.UpdatedById = Id;
                cekKTP.UpdatedByUID = enroll.UID;
                cekKTP.UpdatedByUnitCode = unitCode;
                cekKTP.UpdatedByUnitId = unitId;
                cekKTP.UpdatedByNpp = npp;
                cekKTP.isEnrollFR = false;

                if (string.IsNullOrWhiteSpace(stringPerubahan))
                {
                    stringPerubahan = stringPerubahan + "Data Demografi tidak ada perubahan" + " <br/>";
                }
                #endregion

                if (isEmployee)
                {
                    var _mappingData = await _enrollmentKTPRepository.MappingNppNikByNik(enroll.KtpNIK);
                    if (_mappingData == null)
                    {
                        dataNpp = new Tbl_Mapping_Pegawai_KTP
                        {
                            CreatedById = Id,
                            NIK = enroll.KtpNIK,
                            Npp = _empData.Npp,
                            CreatedByNpp = npp,
                            CreatedByUID = enroll.UID,
                            CreatedTime = DateTime.Now,
                            CreatedByUnit = unitCode,
                            UpdatedByNpp = npp,
                            UpdatedByUID = enroll.UID,
                            UpdatedByUnit = unitCode,
                            UpdatedTime = DateTime.Now,
                        };
                    }
                    else
                    {
                        dataNpp = _mappingData;
                        dataNpp.UpdatedByNpp = npp;
                        dataNpp.UpdatedByUID = enroll.UID;
                        dataNpp.UpdatedByUnit = unitCode;
                        dataNpp.UpdatedTime = DateTime.Now;
                    }
                }

                var sysPathFolder = await _sysParameterRepository.GetPathFolder("PathFolderData");
                string pathFolder = sysPathFolder.Value;

                var photoKtpLog = new Tbl_DataKTP_Photo_Log();
                var photoKTPData = await _profileRepository.GetPhotoKtp(enroll.KtpNIK)
                        .ConfigureAwait(false);
                var exPhotoKtp = photoKTPData;

                #region update Photo KTP
                if (!string.IsNullOrWhiteSpace(enroll.KtpPhotoKTP))
                {
                    byte[] imgBytes = Convert.FromBase64String(enroll.KtpPhotoKTP);

                    string filePath = "";

                    var systemParameterPath = await _sysParameterRepository.GetPathFolder("FolderPhoto")
                                                .ConfigureAwait(false);
                    string pathFolderFoto = systemParameterPath.Value;

                    string subPathFolderFoto = pathFolder + "/" + pathFolderFoto + "/" + enroll.KtpNIK + "/";

                    string fileName = "Foto_" + enroll.KtpNIK + "_" + JamServer + ".jpg";

                    if (_sftpConfig.IsActive)
                    {
                        using var stream = new MemoryStream(imgBytes);

                        (var fname, var fPath) = await stream.UploadToFTPServer(subPathFolderFoto, fileName, _sftpConfig.Host,
                            _sftpConfig.Username, _sftpConfig.Password, _sftpConfig.RootDirectory, _sftpConfig.Url).ConfigureAwait(false);

                        filePath = fPath;
                    }
                    else
                    {
                        if (!Directory.Exists(subPathFolderFoto))
                        {
                            Directory.CreateDirectory(subPathFolderFoto);
                        }

                        filePath = subPathFolderFoto + fileName;
                        File.WriteAllBytes(filePath, imgBytes);
                    }

                    if (photoKTPData == null)
                    {
                        photoKTPData = new Tbl_DataKTP_Photo
                        {
                            PathFile = filePath,
                            Nik = enroll.KtpNIK,
                            FileName = fileName,
                            IsActive = true,
                            IsDeleted = false,
                            UpdatedById = Id,
                            UpdatedByNpp = npp,
                            UpdatedByUid = enroll.UID,
                            UpdatedByUnit = unitCode,
                            UpdatedTime = DateTime.Now,
                            CreatedById = Id,
                            CreatedByNpp = npp,
                            CreatedByUid = enroll.UID,
                            CreatedByUnit = unitCode,
                            CreatedTime = DateTime.Now,
                        };
                    }
                    else
                    {
                        photoKtpLog.CreatedById = photoKTPData.CreatedById;
                        photoKtpLog.CreatedByNpp = photoKTPData.CreatedByNpp;
                        photoKtpLog.CreatedByUid = photoKTPData.CreatedByUid;
                        photoKtpLog.CreatedByUnit = photoKTPData.CreatedByUnit;
                        photoKtpLog.CreatedTime = photoKTPData.CreatedTime;
                        photoKtpLog.FileName = photoKTPData.FileName;
                        photoKtpLog.Nik = photoKTPData.Nik;
                        photoKtpLog.PathFile = photoKTPData.PathFile;

                        photoKTPData.PathFile = filePath;
                        photoKTPData.Nik = enroll.KtpNIK;
                        photoKTPData.FileName = fileName;
                        photoKTPData.IsActive = true;
                        photoKTPData.IsDeleted = false;
                        photoKTPData.UpdatedById = Id;
                        photoKTPData.UpdatedByNpp = npp;
                        photoKTPData.UpdatedByUid = enroll.UID;
                        photoKTPData.UpdatedByUnit = unitCode;
                        photoKTPData.UpdatedTime = DateTime.Now;
                    }
                }
                else
                {
                    stringPerubahan = stringPerubahan + "Request Payload Photo Tidak Ditemukan" + " <br/>";
                }
                #endregion

                var photoSignatureLog = new Tbl_DataKTP_Signature_Log();
                var photoSignatureData = await _profileRepository.GetPhotoSignature(enroll.KtpNIK)
                        .ConfigureAwait(false);
                var exphotoSignatureData = photoSignatureData;

                #region update signature
                if (!string.IsNullOrWhiteSpace(enroll.KtpSignature))
                {
                    byte[] imgBytes = Convert.FromBase64String(enroll.KtpSignature);

                    string filePath = "";

                    var systemParameterPath = await _sysParameterRepository.GetPathFolder("FolderSignature")
                                                .ConfigureAwait(false);
                    string pathFolderFoto = systemParameterPath.Value;

                    string subPathFolderSignature = pathFolder + "/" + pathFolderFoto + "/" + enroll.KtpNIK + "/";

                    string fileName = "Signature_" + enroll.KtpNIK + "_" + JamServer + ".jpg";

                    if (_sftpConfig.IsActive)
                    {
                        using var stream = new MemoryStream(imgBytes);

                        (var fname, var fPath) = await stream.UploadToFTPServer(subPathFolderSignature, fileName, _sftpConfig.Host,
                            _sftpConfig.Username, _sftpConfig.Password, _sftpConfig.RootDirectory, _sftpConfig.Url).ConfigureAwait(false);

                        filePath = fPath;
                    }
                    else
                    {
                        if (!Directory.Exists(subPathFolderSignature))
                        {
                            Directory.CreateDirectory(subPathFolderSignature);
                        }

                        filePath = subPathFolderSignature + fileName;
                        File.WriteAllBytes(filePath, imgBytes);
                    }

                    if (photoSignatureData == null)
                    {
                        photoSignatureData = new Tbl_DataKTP_Signature();
                        photoSignatureData.UpdatedById = Id;
                        photoSignatureData.UpdatedByNpp = npp;
                        photoSignatureData.UpdatedByUid = enroll.UID;
                        photoSignatureData.UpdatedTime = DateTime.Now;
                        photoSignatureData.UpdatedByUnit = unitCode;
                        photoSignatureData.IsActive = true;
                        photoSignatureData.IsDeleted = false;
                        photoSignatureData.Nik = enroll.KtpNIK;
                        photoSignatureData.FileName = fileName;
                        photoSignatureData.PathFile = filePath;
                        photoSignatureData.CreatedById = Id;
                        photoSignatureData.CreatedByNpp = npp;
                        photoSignatureData.CreatedByUid = enroll.UID;
                        photoSignatureData.CreatedByUnit = unitCode;
                        photoSignatureData.CreatedTime = DateTime.Now;
                    }
                    else
                    {
                        photoSignatureLog.CreatedById = photoSignatureData.CreatedById;
                        photoSignatureLog.CreatedByNpp = photoSignatureData.CreatedByNpp;
                        photoSignatureLog.CreatedByUid = photoSignatureData.CreatedByUid;
                        photoSignatureLog.CreatedByUnit = photoSignatureData.CreatedByUnit;
                        photoSignatureLog.CreatedTime = photoSignatureData.CreatedTime;
                        photoSignatureLog.FileName = photoSignatureData.FileName;
                        photoSignatureLog.Nik = photoSignatureData.Nik;
                        photoSignatureLog.PathFile = photoSignatureData.PathFile;

                        photoSignatureData.UpdatedById = Id;
                        photoSignatureData.UpdatedByNpp = npp;
                        photoSignatureData.UpdatedByUid = enroll.UID;
                        photoSignatureData.UpdatedTime = DateTime.Now;
                        photoSignatureData.IsActive = true;
                        photoSignatureData.IsDeleted = false;
                        photoSignatureData.Nik = enroll.KtpNIK;
                        photoSignatureData.FileName = fileName;
                        photoSignatureData.PathFile = filePath;
                    }
                }
                else
                {
                    stringPerubahan = stringPerubahan + "Request Payload Signature Tidak Ditemukan" + " <br/>";
                }
                #endregion
                               
                #region update photo cam
                var photoCamLog = new Tbl_DataKTP_PhotoCam_Log();
                var photoCamData = await _profileRepository.GetPhotoCam(enroll.KtpNIK)
                        .ConfigureAwait(false);
                var exphotoCamData = photoCamData;

                if (!string.IsNullOrWhiteSpace(enroll.KtpPhotoCam))
                {

                    byte[] imgBytes = Convert.FromBase64String(enroll.KtpPhotoCam);

                    string filePath = "";

                    var systemParameterPath = await _sysParameterRepository.GetPathFolder("FolderWebcam")
                                                .ConfigureAwait(false);
                    string pathFolderFoto = systemParameterPath.Value;

                    string SubPathFolderPhotoCam = pathFolder + "/" + pathFolderFoto + "/" + enroll.KtpNIK + "/";

                    string fileName = "PhotoCam_" + enroll.KtpNIK + "_" + JamServer + ".txt";

                    if (_sftpConfig.IsActive)
                    {
                        using var stream = new MemoryStream(imgBytes);

                        (var fname, var fPath) = await stream.UploadToFTPServer(SubPathFolderPhotoCam, fileName, _sftpConfig.Host,
                            _sftpConfig.Username, _sftpConfig.Password, _sftpConfig.RootDirectory, _sftpConfig.Url).ConfigureAwait(false);

                        filePath = fPath;
                    }
                    else
                    {
                        if (!Directory.Exists(SubPathFolderPhotoCam))
                        {
                            Directory.CreateDirectory(SubPathFolderPhotoCam);
                        }

                        filePath = SubPathFolderPhotoCam + fileName;
                        File.WriteAllBytes(filePath, imgBytes);
                    }

                    if (photoCamData == null)
                    {
                        photoCamData = new Tbl_DataKTP_PhotoCam();
                        photoCamData.PathFile = filePath;
                        photoCamData.Nik = enroll.KtpNIK;
                        photoCamData.FileName = fileName;
                        photoCamData.IsActive = true;
                        photoCamData.IsDeleted = false;
                        photoCamData.UpdatedById = Id;
                        photoCamData.UpdatedByNpp = npp;
                        photoCamData.UpdatedByUid = enroll.UID;
                        photoCamData.UpdatedTime = DateTime.Now;
                        photoCamData.UpdatedByUnit = unitCode;
                        photoCamLog.CreatedById = photoCamData.CreatedById;
                        photoCamLog.CreatedByNpp = photoCamData.CreatedByNpp;
                        photoCamLog.CreatedByUid = photoCamData.CreatedByUid;
                        photoCamLog.CreatedByUnit = photoCamData.CreatedByUnit;
                        photoCamLog.CreatedTime = photoCamData.CreatedTime;
                    }
                    else
                    {
                        photoCamLog.CreatedById = photoCamData.CreatedById;
                        photoCamLog.CreatedByNpp = photoCamData.CreatedByNpp;
                        photoCamLog.CreatedByUid = photoCamData.CreatedByUid;
                        photoCamLog.CreatedByUnit = photoCamData.CreatedByUnit;
                        photoCamLog.CreatedTime = photoCamData.CreatedTime;
                        photoCamLog.FileName = photoCamData.FileName;
                        photoCamLog.Nik = photoCamData.Nik;
                        photoCamLog.PathFile = photoCamData.PathFile;

                        photoCamData.PathFile = filePath;
                        photoCamData.Nik = enroll.KtpNIK;
                        photoCamData.FileName = fileName;
                        photoCamData.IsActive = true;
                        photoCamData.IsDeleted = false;
                        photoCamData.UpdatedById = Id;
                        photoCamData.UpdatedByNpp = npp;
                        photoCamData.UpdatedByUid = enroll.UID;
                        photoCamData.UpdatedTime = DateTime.Now;
                    }
                }
                else
                {
                    if (photoCamData != null)
                    {
                        photoCamLog.CreatedById = photoCamData.CreatedById;
                        photoCamLog.CreatedByNpp = photoCamData.CreatedByNpp;
                        photoCamLog.CreatedByUid = photoCamData.CreatedByUid;
                        photoCamLog.CreatedByUnit = photoCamData.CreatedByUnit;
                        photoCamLog.CreatedTime = photoCamData.CreatedTime;
                        photoCamLog.FileName = photoCamData.FileName;
                        photoCamLog.Nik = photoCamData.Nik;
                        photoCamLog.PathFile = photoCamData.PathFile;
                        photoCamData = new Tbl_DataKTP_PhotoCam();
                    }
                    stringPerubahan = stringPerubahan + "Request Payload Photo Cam Tidak Ditemukan" + " <br/>";
                }

                #endregion

                #region finger
                var exPhotoFinger = new List<Tbl_DataKTP_Finger>();

                var exPhotoFingerEmployee = new List<Tbl_DataKTP_Finger_Employee>();

                var photoFingerLogs = new List<Tbl_DataKTP_Finger_Log>();

                var photoFingers = new List<Tbl_DataKTP_Finger>();

                var photoFingersEmployee = new List<Tbl_DataKTP_Finger_Employee>();

                var photoFingersEmployeeLogs = new List<Tbl_DataKTP_Finger_Employee_Log>();

                var photoFingerExisting = await _profileRepository.GetPhotoFingerExisting(enroll.KtpNIK).ConfigureAwait(false);
                var photoFingerEmployeeExisting = await _profileRepository.GetPhotoFingerEmployeeExisting(enroll.KtpNIK).ConfigureAwait(false);

                foreach (var em in photoFingerEmployeeExisting)
                {
                    photoFingersEmployeeLogs.Add(new Tbl_DataKTP_Finger_Employee_Log
                    {
                        CreatedById = em.CreatedById,
                        CreatedByNpp = em.CreatedByNpp,
                        CreatedByUid = em.CreatedByUid,
                        CreatedByUnit = em.CreatedByUnit,
                        CreatedByUnitId = em.CreatedByUnitId,
                        CreatedTime = em.CreatedTime,
                        //FileJari = em.FileJari,
                        FileName = em.FileName,
                        Nik = em.Nik,
                        PathFile = em.PathFile,
                        TypeFinger = em.TypeFinger
                    });
                }

                foreach (var el in photoFingerExisting)
                {
                    photoFingerLogs.Add(new Tbl_DataKTP_Finger_Log
                    {
                        CreatedById = el.CreatedById,
                        CreatedByNpp = el.CreatedByNpp,
                        CreatedByUid = el.CreatedByUid,
                        CreatedByUnit = el.CreatedByUnit,
                        CreatedByUnitId = el.CreatedByUnitId,
                        CreatedTime = el.CreatedTime,
                        //FileJari = el.FileJari,
                        FileJariISO = el.FileJariISO,
                        FileName = el.FileName,
                        FileNameISO = el.FileNameISO,
                        Nik = el.Nik,
                        PathFile = el.PathFile,
                        PathFileISO = el.PathFileISO,
                        TypeFinger = el.TypeFinger
                    });
                }

                if (!string.IsNullOrWhiteSpace(enroll.KtpFingerKanan))
                {

                    var photoFingerDataEmployee = new Tbl_DataKTP_Finger_Employee();

                    if (isEmployee)
                    {
                        photoFingerDataEmployee = await _profileRepository.GetPhotoFingerEmployee(enroll.KtpNIK, enroll.KtpTypeJariKanan)
                        .ConfigureAwait(false);
                    }

                    string imageEncrypted = enroll.KtpFingerKanan.Encrypt(Phrase.FileEncryption);

                    string filePath = "";
                    string filePathIso = "";

                    var systemParameterPath = await _sysParameterRepository.GetPathFolder("FolderFinger")
                                                .ConfigureAwait(false);
                    string pathFolderFoto = systemParameterPath.Value;

                    string subPathFolderPhotoFinger = pathFolder + "/" + pathFolderFoto + "/" + enroll.KtpNIK + "/";

                    string JenisJari = enroll.KtpTypeJariKanan.Replace(" ", "");
                    string fileName = "PhotoFinger_" + JenisJari + "_" + enroll.KtpNIK + "_" + JamServer + ".txt";
                    string fileNameIso = "";

                    if (_sftpConfig.IsActive)
                    {
                        using var stream = new MemoryStream(Encoding.ASCII.GetBytes(imageEncrypted));

                        (var fname, var fPath) = await stream.UploadToFTPServer(subPathFolderPhotoFinger, fileName, _sftpConfig.Host,
                            _sftpConfig.Username, _sftpConfig.Password, _sftpConfig.RootDirectory, _sftpConfig.Url).ConfigureAwait(false);

                        filePath = fPath;
                    }
                    else
                    {
                        if (!Directory.Exists(subPathFolderPhotoFinger))
                        {
                            Directory.CreateDirectory(subPathFolderPhotoFinger);
                        }

                        filePath = subPathFolderPhotoFinger + fileName;
                        File.WriteAllText(filePath, imageEncrypted);
                    }

                    //var typeFingerMain = await _fingerRepository.GetFingersEnrolled(enroll.KtpNIK);

                    var typeFingerMain = new List<FingerByNik> {
                        new FingerByNik { NIK = enroll.KtpNIK, TypeFinger = (enroll.KtpTypeJariKanan != null) ? enroll.KtpTypeJariKanan : ""},
                        new FingerByNik { NIK = enroll.KtpNIK, TypeFinger = (enroll.KtpTypeJariKiri != null) ? enroll.KtpTypeJariKiri : ""},
                    };

                    if (typeFingerMain.Count() == 0 || typeFingerMain.Count(x => x.TypeFinger.Contains("Kanan")) == 0)
                    {
                        var photoFingerData = new Tbl_DataKTP_Finger
                        {
                            CreatedById = Id,
                            IsActive = true,
                            IsDeleted = false,
                            Nik = enroll.KtpNIK,
                            PathFile = filePath,
                            CreatedByNpp = npp,
                            CreatedByUid = enroll.UID,
                            CreatedTime = DateTime.Now,
                            CreatedByUnit = unitCode,
                            FileName = fileName,
                            TypeFinger = enroll.KtpTypeJariKanan
                        };

                        if (isEmployee)
                        {
                            photoFingerDataEmployee = new Tbl_DataKTP_Finger_Employee
                            {
                                CreatedById = Id,
                                IsActive = true,
                                IsDeleted = false,
                                Nik = enroll.KtpNIK,
                                PathFile = filePath,
                                CreatedByNpp = npp,
                                CreatedByUid = enroll.UID,
                                CreatedTime = DateTime.Now,
                                UpdatedById = Id,
                                UpdatedByNpp = npp,
                                UpdatedByUid = enroll.UID,
                                UpdatedByUnit = unitCode,
                                CreatedByUnitId = unitId,
                                CreatedByUnit = unitCode,
                                //FileJari = photoFingerDataEmployee.FileJari,
                                UpdatedTime = DateTime.Now,
                                FileName = fileName,
                                TypeFinger = enroll.KtpTypeJariKanan
                            };


                            
                        }
                    }

                    foreach (var i in typeFingerMain)
                    {
                        if (i.TypeFinger.Contains("Kanan"))
                        {
                            var photoFingerData = await _profileRepository.GetPhotoFinger(enroll.KtpNIK, i.TypeFinger).ConfigureAwait(false);
                                                        
                            if (isEmployee)
                            {
                                photoFingerDataEmployee = await _profileRepository.GetPhotoFingerEmployee(enroll.KtpNIK, i.TypeFinger)
                                .ConfigureAwait(false);
                            }

                            if (photoFingerData == null)
                            {
                                
                                photoFingerData = new Tbl_DataKTP_Finger
                                {
                                    CreatedById = Id,
                                    IsActive = true,
                                    IsDeleted = false,
                                    Nik = enroll.KtpNIK,
                                    PathFile = filePath,
                                    CreatedByNpp = npp,
                                    CreatedByUid = enroll.UID,
                                    CreatedTime = DateTime.Now,
                                    CreatedByUnit = unitCode,
                                    FileName = fileName,
                                    TypeFinger = enroll.KtpTypeJariKanan
                                };

                                photoFingers.Add(new Tbl_DataKTP_Finger
                                {
                                    CreatedById = photoFingerData.CreatedById,
                                    IsActive = true,
                                    IsDeleted = false,
                                    Nik = enroll.KtpNIK,
                                    PathFile = filePath,
                                    CreatedByNpp = photoFingerData.CreatedByNpp,
                                    CreatedByUid = photoFingerData.CreatedByUid,
                                    CreatedTime = photoFingerData.CreatedTime,
                                    UpdatedById = Id,
                                    UpdatedByNpp = npp,
                                    UpdatedByUid = enroll.UID,
                                    UpdatedByUnit = unitCode,
                                    CreatedByUnitId = photoFingerData.CreatedByUnitId,
                                    CreatedByUnit = photoFingerData.CreatedByUnit,
                                    //FileJari = photoFingerData.FileJari,
                                    UpdatedTime = DateTime.Now,
                                    FileName = fileName,
                                    TypeFinger = enroll.KtpTypeJariKanan
                                });
                            }
                                                        
                            else
                            {
                                
                                photoFingers.Add(new Tbl_DataKTP_Finger
                                {
                                    CreatedById = photoFingerData.CreatedById,
                                    IsActive = true,
                                    IsDeleted = false,
                                    Nik = enroll.KtpNIK,
                                    PathFile = filePath,
                                    CreatedByNpp = photoFingerData.CreatedByNpp,
                                    CreatedByUid = photoFingerData.CreatedByUid,
                                    CreatedTime = photoFingerData.CreatedTime,
                                    UpdatedById = Id,
                                    UpdatedByNpp = npp,
                                    UpdatedByUid = enroll.UID,
                                    UpdatedByUnit = unitCode,
                                    CreatedByUnitId = photoFingerData.CreatedByUnitId,
                                    CreatedByUnit = photoFingerData.CreatedByUnit,
                                    //FileJari = enroll.KtpFingerKanan,
                                    UpdatedTime = DateTime.Now,
                                    FileName = fileName,
                                    TypeFinger = enroll.KtpTypeJariKanan
                                });
                            }

                            if (isEmployee)
                            {
                                if (photoFingerDataEmployee == null)
                                {
                                    photoFingerDataEmployee = new Tbl_DataKTP_Finger_Employee
                                    {
                                        CreatedById = Id,
                                        IsActive = true,
                                        IsDeleted = false,
                                        Nik = enroll.KtpNIK,
                                        PathFile = filePath,//
                                        CreatedByNpp = npp,
                                        CreatedByUid = enroll.UID,
                                        CreatedTime = DateTime.Now,
                                        FileName = fileName,//
                                        //FileJari = enroll.KtpTypeJariKanan,
                                        TypeFinger = enroll.KtpTypeJariKanan
                                    };

                                    photoFingersEmployee.Add(new Tbl_DataKTP_Finger_Employee
                                    {
                                        CreatedById = photoFingerDataEmployee.CreatedById,
                                        IsActive = true,
                                        IsDeleted = false,
                                        Nik = enroll.KtpNIK,
                                        PathFile = filePath,
                                        CreatedByNpp = photoFingerDataEmployee.CreatedByNpp,
                                        CreatedByUid = photoFingerDataEmployee.CreatedByUid,
                                        CreatedTime = photoFingerDataEmployee.CreatedTime,
                                        UpdatedById = Id,
                                        UpdatedByNpp = npp,
                                        UpdatedByUid = enroll.UID,
                                        UpdatedByUnit = unitCode,
                                        CreatedByUnitId = photoFingerDataEmployee.CreatedByUnitId,
                                        CreatedByUnit = photoFingerDataEmployee.CreatedByUnit,
                                        //FileJari = photoFingerDataEmployee.FileJari,
                                        UpdatedTime = DateTime.Now,
                                        FileName = fileName,
                                        TypeFinger = enroll.KtpTypeJariKanan
                                    });
                                }
                                else
                                {
                                    
                                    photoFingersEmployee.Add(new Tbl_DataKTP_Finger_Employee
                                    {
                                        CreatedById = photoFingerDataEmployee.CreatedById,
                                        IsActive = true,
                                        IsDeleted = false,
                                        Nik = enroll.KtpNIK,
                                        PathFile = filePath,
                                        CreatedByNpp = photoFingerDataEmployee.CreatedByNpp,
                                        CreatedByUid = photoFingerDataEmployee.CreatedByUid,
                                        CreatedTime = photoFingerDataEmployee.CreatedTime,
                                        UpdatedById = Id,
                                        UpdatedByNpp = npp,
                                        UpdatedByUid = enroll.UID,
                                        UpdatedByUnit = unitCode,
                                        CreatedByUnitId = photoFingerDataEmployee.CreatedByUnitId,
                                        CreatedByUnit = photoFingerDataEmployee.CreatedByUnit,
                                        //FileJari = enroll.KtpFingerKiri,
                                        UpdatedTime = DateTime.Now,
                                        FileName = fileName,
                                        TypeFinger = enroll.KtpTypeJariKanan
                                    });
                                }
                            }
                        }
                    }
                }
                else
                {
                    #region get data if no payload but we have to replace all data with no data
                    var photoFingerDataEmployee = new Tbl_DataKTP_Finger_Employee();
                    //var typeFingerMain = await _fingerRepository.GetFingersEnrolled(enroll.KtpNIK);

                    var typeFingerMain = new List<FingerByNik> {
                        new FingerByNik { NIK = enroll.KtpNIK, TypeFinger = (enroll.KtpTypeJariKanan != null) ? enroll.KtpTypeJariKanan : ""},
                        new FingerByNik { NIK = enroll.KtpNIK, TypeFinger = (enroll.KtpTypeJariKiri != null) ? enroll.KtpTypeJariKiri : ""},
                    };

                    foreach (var i in typeFingerMain)
                    {

                        if (i.TypeFinger.Contains("Kanan"))
                        {
                            var photoFingerData = await _profileRepository.GetPhotoFinger(enroll.KtpNIK, i.TypeFinger).ConfigureAwait(false);

                            if (isEmployee)
                            {
                                photoFingerDataEmployee = await _profileRepository.GetPhotoFingerEmployee(enroll.KtpNIK, i.TypeFinger)
                                .ConfigureAwait(false);
                            }

                        }
                    }
                    #endregion

                    stringPerubahan = stringPerubahan + "Request Payload Finger Kanan Tidak Ditemukan" + " <br/>";
                }

                if (!string.IsNullOrWhiteSpace(enroll.KtpFingerKiri))
                {
                    var photoFingerDataEmployee = new Tbl_DataKTP_Finger_Employee();

                    if (isEmployee)
                    {
                        photoFingerDataEmployee = await _profileRepository.GetPhotoFingerEmployee(enroll.KtpNIK, enroll.KtpTypeJariKiri)
                        .ConfigureAwait(false);
                    }

                    string imageEncrypted = enroll.KtpFingerKiri.Encrypt(Phrase.FileEncryption);

                    string filePath = "";
                    string filePathIso = "";

                    var systemParameterPath = await _sysParameterRepository.GetPathFolder("FolderFinger")
                                                .ConfigureAwait(false);
                    string pathFolderFoto = systemParameterPath.Value;

                    string subPathFolderPhotoFinger = pathFolder + "/" + pathFolderFoto + "/" + enroll.KtpNIK + "/";

                    string JenisJari = enroll.KtpTypeJariKiri.Replace(" ", "");
                    string fileName = "PhotoFinger_" + JenisJari + "_" + enroll.KtpNIK + "_" + JamServer + ".txt";
                    string fileNameIso = "";

                    if (_sftpConfig.IsActive)
                    {
                        using var stream = new MemoryStream(Encoding.ASCII.GetBytes(imageEncrypted));

                        (var fname, var fPath) = await stream.UploadToFTPServer(subPathFolderPhotoFinger, fileName, _sftpConfig.Host,
                            _sftpConfig.Username, _sftpConfig.Password, _sftpConfig.RootDirectory, _sftpConfig.Url).ConfigureAwait(false);

                        filePath = fPath;
                    }
                    else
                    {
                        if (!Directory.Exists(subPathFolderPhotoFinger))
                        {
                            Directory.CreateDirectory(subPathFolderPhotoFinger);
                        }

                        filePath = subPathFolderPhotoFinger + fileName;
                        File.WriteAllText(filePath, imageEncrypted);
                    }

                    //var typeFingerMain = await _fingerRepository.GetFingersEnrolled(enroll.KtpNIK);

                    var typeFingerMain = new List<FingerByNik> {
                        new FingerByNik { NIK = enroll.KtpNIK, TypeFinger = (enroll.KtpTypeJariKanan != null) ? enroll.KtpTypeJariKanan : ""},
                        new FingerByNik { NIK = enroll.KtpNIK, TypeFinger = (enroll.KtpTypeJariKiri != null) ? enroll.KtpTypeJariKiri : ""},
                    };

                    if (typeFingerMain.Count() == 0 || typeFingerMain.Count(x => x.TypeFinger.Contains("Kiri")) == 0)
                    {
                        var photoFingerData = new Tbl_DataKTP_Finger();

                        photoFingers.Add(new Tbl_DataKTP_Finger
                        {
                            CreatedById = Id,
                            IsActive = true,
                            IsDeleted = false,
                            Nik = enroll.KtpNIK,
                            PathFile = filePath,
                            CreatedByNpp = npp,
                            CreatedByUid = enroll.UID,
                            CreatedTime = DateTime.Now,
                            UpdatedById = Id,
                            UpdatedByNpp = npp,
                            UpdatedByUid = enroll.UID,
                            UpdatedByUnit = unitCode,
                            CreatedByUnitId = unitId,
                            CreatedByUnit = unitCode,
                            //FileJari = photoFingerData.FileJari,
                            UpdatedTime = DateTime.Now,
                            FileName = fileName,
                            TypeFinger = enroll.KtpTypeJariKiri
                        });

                    //    if (typeFingerMain.Count() == 0)
                    //{
                    //    var photoFingerData = new Tbl_DataKTP_Finger();

                    //    photoFingers.Add(new Tbl_DataKTP_Finger
                    //    {
                    //        CreatedById = Id,
                    //        IsActive = true,
                    //        IsDeleted = false,
                    //        Nik = enroll.KtpNIK,
                    //        PathFile = filePath,
                    //        CreatedByNpp = npp,
                    //        CreatedByUid = enroll.UID,
                    //        CreatedTime = DateTime.Now,
                    //        UpdatedById = Id,
                    //        UpdatedByNpp = npp,
                    //        UpdatedByUid = enroll.UID,
                    //        UpdatedByUnit = unitCode,
                    //        CreatedByUnitId = unitId,
                    //        CreatedByUnit = unitCode,
                    //        //FileJari = photoFingerData.FileJari,
                    //        UpdatedTime = DateTime.Now,
                    //        FileName = fileName,
                    //        TypeFinger = enroll.KtpTypeJariKiri
                    //    });


                        //if (isEmployee)
                        //{
                        //    //photoFingersEmployee.Add(new Tbl_DataKTP_Finger_Employee
                        //    //{
                        //    //    CreatedById = photoFingerDataEmployee.CreatedById,
                        //    //    IsActive = true,
                        //    //    IsDeleted = false,
                        //    //    Nik = enroll.KtpNIK,
                        //    //    PathFile = filePath,
                        //    //    CreatedByNpp = photoFingerDataEmployee.CreatedByNpp,
                        //    //    CreatedByUid = photoFingerDataEmployee.CreatedByUid,
                        //    //    CreatedTime = photoFingerDataEmployee.CreatedTime,
                        //    //    UpdatedById = Id,
                        //    //    UpdatedByNpp = npp,
                        //    //    UpdatedByUid = enroll.UID,
                        //    //    UpdatedByUnit = unitCode,
                        //    //    CreatedByUnitId = photoFingerDataEmployee.CreatedByUnitId,
                        //    //    CreatedByUnit = photoFingerDataEmployee.CreatedByUnit,
                        //    //    //FileJari = photoFingerDataEmployee.FileJari,
                        //    //    UpdatedTime = DateTime.Now,
                        //    //    FileName = fileName,
                        //    //    TypeFinger = enroll.KtpTypeJariKiri
                        //    //});
                        //    photoFingerDataEmployee = new Tbl_DataKTP_Finger_Employee
                        //    {
                        //        CreatedById = Id,
                        //        IsActive = true,
                        //        IsDeleted = false,
                        //        Nik = enroll.KtpNIK,
                        //        PathFile = filePath,
                        //        CreatedByNpp = npp,
                        //        CreatedByUid = enroll.UID,
                        //        CreatedTime = DateTime.Now,
                        //        UpdatedById = Id,
                        //        UpdatedByNpp = npp,
                        //        UpdatedByUid = enroll.UID,
                        //        UpdatedByUnit = unitCode,
                        //        CreatedByUnitId = unitId,
                        //        CreatedByUnit = unitCode,
                        //        //FileJari = photoFingerDataEmployee.FileJari,
                        //        UpdatedTime = DateTime.Now,
                        //        FileName = fileName,
                        //        TypeFinger = enroll.KtpTypeJariKanan
                        //    };

                        //}

                        if (isEmployee)
                        {
                            photoFingerDataEmployee = new Tbl_DataKTP_Finger_Employee
                            {
                                CreatedById = Id,
                                IsActive = true,
                                IsDeleted = false,
                                Nik = enroll.KtpNIK,
                                PathFile = filePath,
                                CreatedByNpp = npp,
                                CreatedByUid = enroll.UID,
                                CreatedTime = DateTime.Now,
                                UpdatedById = Id,
                                UpdatedByNpp = npp,
                                UpdatedByUid = enroll.UID,
                                UpdatedByUnit = unitCode,
                                CreatedByUnitId = unitId,
                                CreatedByUnit = unitCode,
                                //FileJari = photoFingerDataEmployee.FileJari,
                                UpdatedTime = DateTime.Now,
                                FileName = fileName,
                                TypeFinger = enroll.KtpTypeJariKiri
                            };
                        }
                    }

                    foreach (var i in typeFingerMain)
                    {

                        if (i.TypeFinger.Contains("Kiri"))
                        {
                            var photoFingerData = await _profileRepository.GetPhotoFinger(enroll.KtpNIK, i.TypeFinger).ConfigureAwait(false);

                            if (isEmployee)
                            {
                                photoFingerDataEmployee = await _profileRepository.GetPhotoFingerEmployee(enroll.KtpNIK, i.TypeFinger)
                                .ConfigureAwait(false);
                            }

                            if (photoFingerData == null)
                            {
                                
                                photoFingerData = new Tbl_DataKTP_Finger
                                {
                                    CreatedById = Id,
                                    IsActive = true,
                                    IsDeleted = false,
                                    Nik = enroll.KtpNIK,
                                    PathFile = filePath,
                                    CreatedByNpp = npp,
                                    CreatedByUid = enroll.UID,
                                    CreatedTime = DateTime.Now,
                                    FileName = fileName,
                                    TypeFinger = enroll.KtpTypeJariKiri
                                };

                                photoFingers.Add(new Tbl_DataKTP_Finger
                                {
                                    CreatedById = photoFingerData.CreatedById,
                                    IsActive = true,
                                    IsDeleted = false,
                                    Nik = enroll.KtpNIK,
                                    PathFile = filePath,
                                    CreatedByNpp = photoFingerData.CreatedByNpp,
                                    CreatedByUid = photoFingerData.CreatedByUid,
                                    CreatedTime = photoFingerData.CreatedTime,
                                    UpdatedById = Id,
                                    UpdatedByNpp = npp,
                                    UpdatedByUid = enroll.UID,
                                    UpdatedByUnit = unitCode,
                                    CreatedByUnitId = photoFingerData.CreatedByUnitId,
                                    CreatedByUnit = photoFingerData.CreatedByUnit,
                                    //FileJari = photoFingerData.FileJari,
                                    UpdatedTime = DateTime.Now,
                                    FileName = fileName,
                                    TypeFinger = enroll.KtpTypeJariKiri
                                });
                            }
                            
                            else
                            {
                                
                                photoFingers.Add(new Tbl_DataKTP_Finger
                                {
                                    CreatedById = photoFingerData.CreatedById,
                                    IsActive = true,
                                    IsDeleted = false,
                                    Nik = enroll.KtpNIK,
                                    PathFile = filePath,
                                    CreatedByNpp = photoFingerData.CreatedByNpp,
                                    CreatedByUid = photoFingerData.CreatedByUid,
                                    CreatedTime = photoFingerData.CreatedTime,
                                    UpdatedById = Id,
                                    UpdatedByNpp = npp,
                                    UpdatedByUid = enroll.UID,
                                    UpdatedByUnit = unitCode,
                                    CreatedByUnitId = photoFingerData.CreatedByUnitId,
                                    CreatedByUnit = photoFingerData.CreatedByUnit,
                                    //FileJari = enroll.KtpFingerKiri,
                                    UpdatedTime = DateTime.Now,
                                    FileName = fileName,
                                    TypeFinger = enroll.KtpTypeJariKiri
                                });
                            }

                            if (isEmployee)
                            {
                                if (photoFingerDataEmployee == null)
                                {
                                    photoFingerDataEmployee = new Tbl_DataKTP_Finger_Employee
                                    {
                                        CreatedById = Id,
                                        IsActive = true,
                                        IsDeleted = false,
                                        Nik = enroll.KtpNIK,
                                        PathFile = filePath,//
                                        CreatedByNpp = npp,
                                        CreatedByUid = enroll.UID,
                                        CreatedTime = DateTime.Now,
                                        FileName = fileName,//
                                        //FileJari = enroll.KtpFingerKiri,
                                        TypeFinger = enroll.KtpTypeJariKiri
                                    };

                                    photoFingersEmployee.Add(new Tbl_DataKTP_Finger_Employee
                                    {
                                        CreatedById = photoFingerDataEmployee.CreatedById,
                                        IsActive = true,
                                        IsDeleted = false,
                                        Nik = enroll.KtpNIK,
                                        PathFile = filePath,
                                        CreatedByNpp = photoFingerDataEmployee.CreatedByNpp,
                                        CreatedByUid = photoFingerDataEmployee.CreatedByUid,
                                        CreatedTime = photoFingerDataEmployee.CreatedTime,
                                        UpdatedById = Id,
                                        UpdatedByNpp = npp,
                                        UpdatedByUid = enroll.UID,
                                        UpdatedByUnit = unitCode,
                                        CreatedByUnitId = photoFingerDataEmployee.CreatedByUnitId,
                                        CreatedByUnit = photoFingerDataEmployee.CreatedByUnit,
                                        //FileJari = photoFingerDataEmployee.FileJari,
                                        UpdatedTime = DateTime.Now,
                                        FileName = fileName,
                                        TypeFinger = enroll.KtpTypeJariKiri
                                    });
                                }
                                else
                                {
                                    
                                    photoFingersEmployee.Add(new Tbl_DataKTP_Finger_Employee
                                    {
                                        CreatedById = photoFingerDataEmployee.CreatedById,
                                        IsActive = true,
                                        IsDeleted = false,
                                        Nik = enroll.KtpNIK,
                                        PathFile = filePath,
                                        CreatedByNpp = photoFingerDataEmployee.CreatedByNpp,
                                        CreatedByUid = photoFingerDataEmployee.CreatedByUid,
                                        CreatedTime = photoFingerDataEmployee.CreatedTime,
                                        UpdatedById = Id,
                                        UpdatedByNpp = npp,
                                        UpdatedByUid = enroll.UID,
                                        UpdatedByUnit = unitCode,
                                        CreatedByUnitId = photoFingerDataEmployee.CreatedByUnitId,
                                        CreatedByUnit = photoFingerDataEmployee.CreatedByUnit,
                                        //FileJari = enroll.KtpFingerKiri,
                                        UpdatedTime = DateTime.Now,
                                        FileName = fileName,
                                        TypeFinger = enroll.KtpTypeJariKiri
                                    });
                                }
                            }
                        }
                    }
                }
                else
                {
                    #region get data if no payload but we have to replace all data with no data
                    var photoFingerDataEmployee = new Tbl_DataKTP_Finger_Employee();
                    //var typeFingerMain = await _fingerRepository.GetFingersEnrolled(enroll.KtpNIK);

                    var typeFingerMain = new List<FingerByNik> {
                        new FingerByNik { NIK = enroll.KtpNIK, TypeFinger = (enroll.KtpTypeJariKanan != null) ? enroll.KtpTypeJariKanan : ""},
                        new FingerByNik { NIK = enroll.KtpNIK, TypeFinger = (enroll.KtpTypeJariKiri != null) ? enroll.KtpTypeJariKiri : ""},
                    };

                    foreach (var i in typeFingerMain)
                    {

                        if (i.TypeFinger.Contains("Kiri"))
                        {
                            var photoFingerData = await _profileRepository.GetPhotoFinger(enroll.KtpNIK, i.TypeFinger).ConfigureAwait(false);

                            if (isEmployee)
                            {
                                photoFingerDataEmployee = await _profileRepository.GetPhotoFingerEmployee(enroll.KtpNIK, i.TypeFinger)
                                .ConfigureAwait(false);
                            }
                        }
                    }
                    #endregion

                    stringPerubahan = stringPerubahan + "Request Payload Finger Kanan Tidak Ditemukan" + " <br/>";
                }
                #endregion

                var (status, msg) = await _enrollmentKTPRepository.InsertEnrollFlow2(cekKTP, logDemografi, photoKTPData, photoKtpLog, photoSignatureData,
                    photoSignatureLog, photoCamData, photoCamLog, photoFingers, photoFingersEmployee, photoFingerLogs, photoFingersEmployeeLogs, exPhotoKtp, exphotoSignatureData, exphotoCamData, photoFingerExisting, photoFingerEmployeeExisting, dataReaderLog, dataNpp);

                //var (status, msg) = await _enrollmentKTPRepository.InsertEnrollFlow2(cekKTP, logDemografi, photoKTPData, photoKtpLog, photoSignatureData,
                //    photoSignatureLog, photoCamData, photoCamLog, photoFingers, photoFingersEmployee, photoFingerLogs, photoFingersEmployeeLogs, exPhotoKtp, exphotoSignatureData, exphotoCamData, exPhotoFinger, exPhotoFingerEmployee, dataReaderLog, dataNpp);

                #region Logging Reader Activity
                _ = _alatReaderRepository.CreateLogActivity2(dataReaderActivityLog);
                _ = _alatReaderRepository.CreateAlatReaderLog(dataReaderLog);
                #endregion

                if (status)
                {
                    return (_SuccessMessageConfig.NasabahSuksesUpdate, (int)ServiceResponseStatus.SUKSES, cekKTP.CIF);
                }
                else
                {
                    return (_ErrorMessageConfig.DemografiGagalEnroll, (int)ServiceResponseStatus.ERROR, msg + " " + stringPerubahan);
                }
            }
            #region 20231108_Validation_Update
            else if (cekKTPTemp != null)
            {
                if (enroll?.KtpNIK == null) return (_ErrorMessageConfig.InputTidakLengkap, (int)EnrollStatus.Inputan_tidak_lengkap, "");
                var cifData = new ApiSOAResponse();
                #region update demografi
                var logDemografi = new Tbl_DataKTP_Demografis_Log
                {
                    Agama = cekKTPTemp.Agama,
                    Alamat = cekKTPTemp.Alamat,
                    AlamatGoogle = cekKTPTemp.AlamatGoogle,
                    AlamatLengkap = cekKTPTemp.AlamatLengkap,
                    CreatedById = cekKTPTemp.CreatedById,
                    CreatedByUID = cekKTPTemp.CreatedByUID,
                    CreatedByUnitId = cekKTPTemp.CreatedByUnitId,
                    CreatedTime = cekKTPTemp.CreatedTime,
                    Desa = cekKTPTemp.Desa,
                    GolonganDarah = cekKTPTemp.GolonganDarah,
                    JenisKelamin = cekKTPTemp.JenisKelamin,
                    Kecamatan = cekKTPTemp.Kecamatan,
                    Kelurahan = cekKTPTemp.Kelurahan,
                    Kewarganegaraan = cekKTPTemp.Kewarganegaraan,
                    KodePos = cekKTPTemp.KodePos,
                    Kota = cekKTPTemp.Kota,
                    Latitude = cekKTPTemp.Latitude,
                    Longitude = cekKTPTemp.Longitude,
                    MasaBerlaku = cekKTPTemp.MasaBerlaku,
                    Nama = cekKTPTemp.Nama,
                    NIK = cekKTPTemp.NIK,
                    Pekerjaan = cekKTPTemp.Pekerjaan,
                    Provinsi = cekKTPTemp.Provinsi,
                    RT = cekKTPTemp.RT,
                    RW = cekKTPTemp.RW,
                    StatusPerkawinan = cekKTPTemp.StatusPerkawinan,
                    TanggalLahir = cekKTPTemp.TanggalLahir,
                    TempatLahir = cekKTPTemp.TempatLahir,
                    CIF = cekKTPTemp.CIF,
                    CreatedByNpp = cekKTPTemp.CreatedByNpp
                };

                //var dataKTPTemp = new Tbl_DataKTP_Demografis
                //{
                //    Agama = cekKTPTemp.Agama,
                //    Alamat = cekKTPTemp.Alamat,
                //    AlamatGoogle = cekKTPTemp.AlamatGoogle,
                //    AlamatLengkap = cekKTPTemp.AlamatLengkap,
                //    CreatedById = cekKTPTemp.CreatedById,
                //    CreatedByUID = cekKTPTemp.CreatedByUID,
                //    CreatedByUnitId = cekKTPTemp.CreatedByUnitId,
                //    CreatedTime = cekKTPTemp.CreatedTime,
                //    Desa = cekKTPTemp.Desa,
                //    GolonganDarah = cekKTPTemp.GolonganDarah,
                //    JenisKelamin = cekKTPTemp.JenisKelamin,
                //    Kecamatan = cekKTPTemp.Kecamatan,
                //    Kelurahan = cekKTPTemp.Kelurahan,
                //    Kewarganegaraan = cekKTPTemp.Kewarganegaraan,
                //    KodePos = cekKTPTemp.KodePos,
                //    Kota = cekKTPTemp.Kota,
                //    Latitude = cekKTPTemp.Latitude,
                //    Longitude = cekKTPTemp.Longitude,
                //    MasaBerlaku = cekKTPTemp.MasaBerlaku,
                //    Nama = cekKTPTemp.Nama,
                //    NIK = cekKTPTemp.NIK,
                //    Pekerjaan = cekKTPTemp.Pekerjaan,
                //    Provinsi = cekKTPTemp.Provinsi,
                //    RT = cekKTPTemp.RT,
                //    RW = cekKTPTemp.RW,
                //    StatusPerkawinan = cekKTPTemp.StatusPerkawinan,
                //    TanggalLahir = cekKTPTemp.TanggalLahir,
                //    TempatLahir = cekKTPTemp.TempatLahir,
                //    CIF = cekKTPTemp.CIF,
                //    CreatedByNpp = cekKTPTemp.CreatedByNpp,
                //    isEnrollFR = true,
                //    UpdatedTime = DateTime.Now,
                //    UpdatedById = Id,
                //    UpdatedByUID = enroll.UID,
                //    UpdatedByUnitCode = unitCode,
                //    UpdatedByUnitId = unitId,
                //    UpdatedByNpp = npp
                //};

                var stringPerubahan = "";

                if (cekKTPTemp.Nama != enroll.KtpNama)
                {
                    stringPerubahan = stringPerubahan + "NAMA : " + cekKTPTemp.Nama + " -> " + enroll.KtpNama + " <br/>";
                    cekKTPTemp.Nama = enroll.KtpNama;
                }

                if (cekKTPTemp.TempatLahir != enroll.KtpTempatLahir)
                {
                    stringPerubahan = stringPerubahan + "TEMPAT LAHIR : " + cekKTPTemp.TempatLahir + " -> " + enroll.KtpTempatLahir + " <br/>";
                    cekKTPTemp.TempatLahir = enroll.KtpTempatLahir;
                }

                if (enroll.KtpTanggalLahir != null)
                {
                    var _ttl = DateTime.ParseExact(enroll.KtpTanggalLahir, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                    if (cekKTPTemp.TanggalLahir != _ttl)
                    {
                        stringPerubahan = stringPerubahan + "TANGGAL LAHIR : " + cekKTPTemp.TanggalLahir + " -> " + enroll.KtpTanggalLahir + " <br/>";
                        cekKTPTemp.TanggalLahir = DateTime.ParseExact(enroll.KtpTanggalLahir, "dd-MM-yyyy", CultureInfo.InvariantCulture);
                    }
                }

                if (cekKTPTemp.JenisKelamin != enroll.KtpJanisKelamin)
                {
                    stringPerubahan = stringPerubahan + "JENIS KELAMIN : " + cekKTPTemp.JenisKelamin + " -> " + enroll.KtpJanisKelamin + " <br/>";
                    cekKTPTemp.JenisKelamin = enroll.KtpJanisKelamin;
                }

                if (cekKTPTemp.GolonganDarah != enroll.KtpGolonganDarah)
                {
                    stringPerubahan = stringPerubahan + "GOLONGAN DARAH : " + cekKTPTemp.GolonganDarah + " -> " + enroll.KtpGolonganDarah + " <br/>";
                    cekKTPTemp.GolonganDarah = enroll.KtpGolonganDarah;
                }

                if (cekKTPTemp.Alamat != enroll.KtpAlamat)
                {
                    stringPerubahan = stringPerubahan + "ALAMAT : " + cekKTPTemp.Alamat + " -> " + enroll.KtpAlamat + " <br/>";
                    cekKTPTemp.Alamat = enroll.KtpAlamat;
                }

                if (cekKTPTemp.RT != enroll.KtpRT)
                {
                    stringPerubahan = stringPerubahan + "RT : " + cekKTPTemp.RT + " -> " + enroll.KtpRT + " <br/>";
                    cekKTPTemp.RT = enroll.KtpRT;
                }

                if (cekKTPTemp.RW != enroll.KtpRW)
                {
                    stringPerubahan = stringPerubahan + "RW : " + cekKTPTemp.RW + " -> " + enroll.KtpRW + " <br/>";
                    cekKTPTemp.RW = enroll.KtpRW;
                }

                if (cekKTPTemp.Kelurahan != enroll.KtpKelurahan)
                {
                    stringPerubahan = stringPerubahan + "KELURAHAN : " + cekKTPTemp.Kelurahan + " -> " + enroll.KtpKelurahan + " <br/>";
                    cekKTPTemp.Kelurahan = enroll.KtpKelurahan;
                }

                if (cekKTPTemp.Kecamatan != enroll.KtpKecamatan)
                {
                    stringPerubahan = stringPerubahan + "KECAMATAN : " + cekKTPTemp.Kecamatan + " -> " + enroll.KtpKecamatan + " <br/>";
                    cekKTPTemp.Kecamatan = enroll.KtpKecamatan;
                }

                if (cekKTPTemp.Kota != enroll.KtpKota)
                {
                    stringPerubahan = stringPerubahan + "KOTA/KABUPATEN : " + cekKTPTemp.Kota + " -> " + enroll.KtpKota + " <br/>";
                    cekKTPTemp.Kota = enroll.KtpKota;
                }

                if (cekKTPTemp.Provinsi != enroll.KtpProvinsi)
                {
                    stringPerubahan = stringPerubahan + "PROVINSI  : " + cekKTPTemp.Provinsi + " -> " + enroll.KtpProvinsi + " <br/>";
                    cekKTPTemp.Provinsi = enroll.KtpProvinsi;
                }

                if (cekKTPTemp.Agama != enroll.KtpAgama)
                {
                    stringPerubahan = stringPerubahan + "AGAMA  : " + cekKTPTemp.Agama + " -> " + enroll.KtpAgama + " <br/>";
                    cekKTPTemp.Agama = enroll.KtpAgama;
                }

                if (cekKTPTemp.StatusPerkawinan != enroll.KtpStatusPerkawinan)
                {
                    stringPerubahan = stringPerubahan + "STATUS PERKAWINAN  : " + cekKTPTemp.Agama + " -> " + enroll.KtpAgama + " <br/>";
                    cekKTPTemp.StatusPerkawinan = enroll.KtpStatusPerkawinan;
                }

                if (cekKTPTemp.Pekerjaan != enroll.KtpPekerjaan)
                {
                    stringPerubahan = stringPerubahan + "PEKERJAAN  : " + cekKTPTemp.Agama + " -> " + enroll.KtpAgama + " <br/>";
                    cekKTPTemp.Pekerjaan = enroll.KtpPekerjaan;
                }

                if (cekKTPTemp.Kewarganegaraan != enroll.KtpKewarganegaraan)
                {
                    stringPerubahan = stringPerubahan + "KEWARGANEGARAAN  : " + cekKTPTemp.Kewarganegaraan + " -> " + enroll.KtpKewarganegaraan + " <br/>";
                    cekKTPTemp.Kewarganegaraan = enroll.KtpKewarganegaraan;
                }
                if (cekKTPTemp.MasaBerlaku != enroll.KtpMasaBerlaku)
                {
                    stringPerubahan = stringPerubahan + "MASA BERLAKU  : " + cekKTPTemp.MasaBerlaku + " -> " + enroll.KtpMasaBerlaku + " <br/>";
                    cekKTPTemp.MasaBerlaku = enroll.KtpMasaBerlaku;
                }
                if (cekKTPTemp.CIF == null)
                {
                    //var cifData = new ApiSOAResponse();
                    #region Hit SOA And Loggging it
                    if (isHitSOA == true)
                    {
                        cifData = await _cifService.GetSOAByCif(ReqSoa)
                        .ConfigureAwait(false);

                        var _status = 0;
                        if (cifData.cif != null)
                        {
                            _status = 1;
                        }

                        var _log = new Tbl_ThirdPartyLog
                        {
                            FeatureName = "ReSubmitEnrollmentFingerEncryptedOnlyISO",
                            HostUrl = ReqSoa.host,
                            Request = Newtonsoft.Json.JsonConvert.SerializeObject(ReqSoa),
                            Status = _status,
                            Response = Newtonsoft.Json.JsonConvert.SerializeObject(cifData),
                            CreatedDate = System.DateTime.Now,
                            CreatedBy = npp
                        };

                        _ = _enrollmentKTPRepository.CreateThirdPartyLog(_log);
                    }
                    else
                    {
                        try
                        {
                            var res = await _cifService.GetCIF(new NikDto { Nik = enroll.KtpNIK },
                            new NikDtoUrl { baseUrl = ReqSoa.baseUrlNonSoa, endpoint = ReqSoa.UrlEndPointNonSoa });
                            if (res.Data == null)
                            {
                                cifData.cif = null;
                            }
                            else
                            {
                                cifData.cif = res.Data.Cif;
                            };
                        }
                        catch (Exception ex)
                        {
                            cifData.cif = null;
                        }

                    }
                    #endregion

                    if (!String.IsNullOrEmpty(cifData.cif))
                    {
                        cifData.cif = cifData.cif.Trim();
                    }

                    stringPerubahan = stringPerubahan + "CIF  : " + cekKTPTemp.CIF + " -> " + cifData.cif + " <br/>";
                    cekKTPTemp.CIF = cifData.cif;
                }

                var dataKTPTemp = new Tbl_DataKTP_Demografis
                {
                    Agama = enroll.KtpAgama,
                    Alamat = enroll.KtpAlamat,
                    AlamatGoogle = enroll.KtpAlamatConvertLatlong,
                    AlamatLengkap = enroll.KtpAlamatConvertLengkap,
                    CIF = (cifData.cif),
                    NIK = enroll.KtpNIK,
                    CreatedById = Id,
                    IsActive = true,
                    IsDeleted = false,
                    JenisKelamin = enroll.KtpJanisKelamin,
                    Kecamatan = enroll.KtpKecamatan,
                    Kelurahan = enroll.KtpKelurahan,
                    CreatedByUID = enroll.UID,
                    CreatedTime = DateTime.Now,
                    GolonganDarah = enroll.KtpGolonganDarah,
                    Kewarganegaraan = enroll.KtpKewarganegaraan,
                    KodePos = enroll.KtpKodePos,
                    Kota = enroll.KtpKota,
                    Latitude = enroll.KtpLatitude,
                    Longitude = enroll.KtpLongitude,
                    MasaBerlaku = enroll.KtpMasaBerlaku,
                    Nama = enroll.KtpNama,
                    Pekerjaan = enroll.KtpPekerjaan,
                    Provinsi = enroll.KtpProvinsi,
                    RT = enroll.KtpRT,
                    RW = enroll.KtpRW,
                    StatusPerkawinan = enroll.KtpStatusPerkawinan,
                    TanggalLahir = string.IsNullOrWhiteSpace(enroll.KtpTanggalLahir) ? null : DateTime.ParseExact(enroll.KtpTanggalLahir, "dd-MM-yyyy", CultureInfo.InvariantCulture),
                    TempatLahir = enroll.KtpTempatLahir,
                    CreatedByNpp = npp,
                    CreatedByUnitCode = unitCode,
                    CreatedByUnitId = unitId,
                    IsVerified = false,
                    IsNasabahTemp = string.IsNullOrWhiteSpace(cifData.cif),
                    isEnrollFR = true
                };

                cekKTPTemp.UpdatedTime = DateTime.Now;
                cekKTPTemp.UpdatedById = Id;
                cekKTPTemp.UpdatedByUID = enroll.UID;
                cekKTPTemp.UpdatedByUnitCode = unitCode;
                cekKTPTemp.UpdatedByUnitId = unitId;
                cekKTPTemp.UpdatedByNpp = npp;

                if (string.IsNullOrWhiteSpace(stringPerubahan))
                {
                    stringPerubahan = stringPerubahan + "Data Demografi tidak ada perubahan" + " <br/>";
                }
                #endregion

                if (isEmployee)
                {
                    var _mappingData = await _enrollmentKTPRepository.MappingNppNikByNik(enroll.KtpNIK);
                    if (_mappingData == null)
                    {
                        dataNpp = new Tbl_Mapping_Pegawai_KTP
                        {
                            CreatedById = Id,
                            NIK = enroll.KtpNIK,
                            Npp = _empData.Npp,
                            CreatedByNpp = npp,
                            CreatedByUID = enroll.UID,
                            CreatedTime = DateTime.Now,
                            CreatedByUnit = unitCode,
                            UpdatedByNpp = npp,
                            UpdatedByUID = enroll.UID,
                            UpdatedByUnit = unitCode,
                            UpdatedTime = DateTime.Now,
                        };
                    }
                    else
                    {
                        dataNpp = _mappingData;
                        dataNpp.UpdatedByNpp = npp;
                        dataNpp.UpdatedByUID = enroll.UID;
                        dataNpp.UpdatedByUnit = unitCode;
                        dataNpp.UpdatedTime = DateTime.Now;
                    }
                }

                var sysPathFolder = await _sysParameterRepository.GetPathFolder("PathFolderData");
                string pathFolder = sysPathFolder.Value;

                var photoKtpLog = new Tbl_DataKTP_Photo_Log();
                var photoKTPData = await _profileRepository.GetPhotoKtp(enroll.KtpNIK)
                        .ConfigureAwait(false);
                var exPhotoKtp = photoKTPData;

                #region update Photo KTP
                if (!string.IsNullOrWhiteSpace(enroll.KtpPhotoKTP))
                {
                    byte[] imgBytes = Convert.FromBase64String(enroll.KtpPhotoKTP);

                    string filePath = "";

                    var systemParameterPath = await _sysParameterRepository.GetPathFolder("FolderPhoto")
                                                .ConfigureAwait(false);
                    string pathFolderFoto = systemParameterPath.Value;

                    string subPathFolderFoto = pathFolder + "/" + pathFolderFoto + "/" + enroll.KtpNIK + "/";

                    string fileName = "Foto_" + enroll.KtpNIK + "_" + JamServer + ".jpg";

                    if (_sftpConfig.IsActive)
                    {
                        using var stream = new MemoryStream(imgBytes);

                        (var fname, var fPath) = await stream.UploadToFTPServer(subPathFolderFoto, fileName, _sftpConfig.Host,
                            _sftpConfig.Username, _sftpConfig.Password, _sftpConfig.RootDirectory, _sftpConfig.Url).ConfigureAwait(false);

                        filePath = fPath;
                    }
                    else
                    {
                        if (!Directory.Exists(subPathFolderFoto))
                        {
                            Directory.CreateDirectory(subPathFolderFoto);
                        }

                        filePath = subPathFolderFoto + fileName;
                        File.WriteAllBytes(filePath, imgBytes);
                    }

                    if (photoKTPData == null)
                    {
                        photoKTPData = new Tbl_DataKTP_Photo
                        {
                            PathFile = filePath,
                            Nik = enroll.KtpNIK,
                            FileName = fileName,
                            IsActive = true,
                            IsDeleted = false,
                            UpdatedById = Id,
                            UpdatedByNpp = npp,
                            UpdatedByUid = enroll.UID,
                            UpdatedByUnit = unitCode,
                            UpdatedTime = DateTime.Now,
                            CreatedById = Id,
                            CreatedByNpp = npp,
                            CreatedByUid = enroll.UID,
                            CreatedByUnit = unitCode,
                            CreatedTime = DateTime.Now,
                        };
                    }
                    else
                    {
                        photoKtpLog.CreatedById = photoKTPData.CreatedById;
                        photoKtpLog.CreatedByNpp = photoKTPData.CreatedByNpp;
                        photoKtpLog.CreatedByUid = photoKTPData.CreatedByUid;
                        photoKtpLog.CreatedByUnit = photoKTPData.CreatedByUnit;
                        photoKtpLog.CreatedTime = photoKTPData.CreatedTime;
                        photoKtpLog.FileName = photoKTPData.FileName;
                        photoKtpLog.Nik = photoKTPData.Nik;
                        photoKtpLog.PathFile = photoKTPData.PathFile;

                        photoKTPData.PathFile = filePath;
                        photoKTPData.Nik = enroll.KtpNIK;
                        photoKTPData.FileName = fileName;
                        photoKTPData.IsActive = true;
                        photoKTPData.IsDeleted = false;
                        photoKTPData.UpdatedById = Id;
                        photoKTPData.UpdatedByNpp = npp;
                        photoKTPData.UpdatedByUid = enroll.UID;
                        photoKTPData.UpdatedByUnit = unitCode;
                        photoKTPData.UpdatedTime = DateTime.Now;
                    }
                }
                else
                {
                    stringPerubahan = stringPerubahan + "Request Payload Photo Tidak Ditemukan" + " <br/>";
                }
                #endregion

                var photoSignatureLog = new Tbl_DataKTP_Signature_Log();
                var photoSignatureData = await _profileRepository.GetPhotoSignature(enroll.KtpNIK)
                        .ConfigureAwait(false);
                var exphotoSignatureData = photoSignatureData;

                #region update signature
                if (!string.IsNullOrWhiteSpace(enroll.KtpSignature))
                {
                    byte[] imgBytes = Convert.FromBase64String(enroll.KtpSignature);

                    string filePath = "";

                    var systemParameterPath = await _sysParameterRepository.GetPathFolder("FolderSignature")
                                                .ConfigureAwait(false);
                    string pathFolderFoto = systemParameterPath.Value;

                    string subPathFolderSignature = pathFolder + "/" + pathFolderFoto + "/" + enroll.KtpNIK + "/";

                    string fileName = "Signature_" + enroll.KtpNIK + "_" + JamServer + ".jpg";

                    if (_sftpConfig.IsActive)
                    {
                        using var stream = new MemoryStream(imgBytes);

                        (var fname, var fPath) = await stream.UploadToFTPServer(subPathFolderSignature, fileName, _sftpConfig.Host,
                            _sftpConfig.Username, _sftpConfig.Password, _sftpConfig.RootDirectory, _sftpConfig.Url).ConfigureAwait(false);

                        filePath = fPath;
                    }
                    else
                    {
                        if (!Directory.Exists(subPathFolderSignature))
                        {
                            Directory.CreateDirectory(subPathFolderSignature);
                        }

                        filePath = subPathFolderSignature + fileName;
                        File.WriteAllBytes(filePath, imgBytes);
                    }

                    if (photoSignatureData == null)
                    {
                        photoSignatureData = new Tbl_DataKTP_Signature();
                        photoSignatureData.UpdatedById = Id;
                        photoSignatureData.UpdatedByNpp = npp;
                        photoSignatureData.UpdatedByUid = enroll.UID;
                        photoSignatureData.UpdatedTime = DateTime.Now;
                        photoSignatureData.UpdatedByUnit = unitCode;
                        photoSignatureData.IsActive = true;
                        photoSignatureData.IsDeleted = false;
                        photoSignatureData.Nik = enroll.KtpNIK;
                        photoSignatureData.FileName = fileName;
                        photoSignatureData.PathFile = filePath;
                        photoSignatureData.CreatedById = Id;
                        photoSignatureData.CreatedByNpp = npp;
                        photoSignatureData.CreatedByUid = enroll.UID;
                        photoSignatureData.CreatedByUnit = unitCode;
                        photoSignatureData.CreatedTime = DateTime.Now;
                    }
                    else
                    {
                        photoSignatureLog.CreatedById = photoSignatureData.CreatedById;
                        photoSignatureLog.CreatedByNpp = photoSignatureData.CreatedByNpp;
                        photoSignatureLog.CreatedByUid = photoSignatureData.CreatedByUid;
                        photoSignatureLog.CreatedByUnit = photoSignatureData.CreatedByUnit;
                        photoSignatureLog.CreatedTime = photoSignatureData.CreatedTime;
                        photoSignatureLog.FileName = photoSignatureData.FileName;
                        photoSignatureLog.Nik = photoSignatureData.Nik;
                        photoSignatureLog.PathFile = photoSignatureData.PathFile;

                        photoSignatureData.UpdatedById = Id;
                        photoSignatureData.UpdatedByNpp = npp;
                        photoSignatureData.UpdatedByUid = enroll.UID;
                        photoSignatureData.UpdatedTime = DateTime.Now;
                        photoSignatureData.IsActive = true;
                        photoSignatureData.IsDeleted = false;
                        photoSignatureData.Nik = enroll.KtpNIK;
                        photoSignatureData.FileName = fileName;
                        photoSignatureData.PathFile = filePath;
                    }
                }
                else
                {
                    stringPerubahan = stringPerubahan + "Request Payload Signature Tidak Ditemukan" + " <br/>";
                }
                #endregion

                #region update photo cam
                var photoCamLog = new Tbl_DataKTP_PhotoCam_Log();
                var photoCamData = await _profileRepository.GetPhotoCam(enroll.KtpNIK)
                        .ConfigureAwait(false);
                var exphotoCamData = photoCamData;

                if (!string.IsNullOrWhiteSpace(enroll.KtpPhotoCam))
                {

                    byte[] imgBytes = Convert.FromBase64String(enroll.KtpPhotoCam);

                    string filePath = "";

                    var systemParameterPath = await _sysParameterRepository.GetPathFolder("FolderWebcam")
                                                .ConfigureAwait(false);
                    string pathFolderFoto = systemParameterPath.Value;

                    string SubPathFolderPhotoCam = pathFolder + "/" + pathFolderFoto + "/" + enroll.KtpNIK + "/";

                    string fileName = "PhotoCam_" + enroll.KtpNIK + "_" + JamServer + ".txt";

                    if (_sftpConfig.IsActive)
                    {
                        using var stream = new MemoryStream(imgBytes);

                        (var fname, var fPath) = await stream.UploadToFTPServer(SubPathFolderPhotoCam, fileName, _sftpConfig.Host,
                            _sftpConfig.Username, _sftpConfig.Password, _sftpConfig.RootDirectory, _sftpConfig.Url).ConfigureAwait(false);

                        filePath = fPath;
                    }
                    else
                    {
                        if (!Directory.Exists(SubPathFolderPhotoCam))
                        {
                            Directory.CreateDirectory(SubPathFolderPhotoCam);
                        }

                        filePath = SubPathFolderPhotoCam + fileName;
                        File.WriteAllBytes(filePath, imgBytes);
                    }

                    if (photoCamData == null)
                    {
                        photoCamData = new Tbl_DataKTP_PhotoCam();
                        photoCamData.PathFile = filePath;
                        photoCamData.Nik = enroll.KtpNIK;
                        photoCamData.FileName = fileName;
                        photoCamData.IsActive = true;
                        photoCamData.IsDeleted = false;
                        photoCamData.UpdatedById = Id;
                        photoCamData.UpdatedByNpp = npp;
                        photoCamData.UpdatedByUid = enroll.UID;
                        photoCamData.UpdatedTime = DateTime.Now;
                        photoCamData.UpdatedByUnit = unitCode;
                        photoCamLog.CreatedById = photoCamData.CreatedById;
                        photoCamLog.CreatedByNpp = photoCamData.CreatedByNpp;
                        photoCamLog.CreatedByUid = photoCamData.CreatedByUid;
                        photoCamLog.CreatedByUnit = photoCamData.CreatedByUnit;
                        photoCamLog.CreatedTime = photoCamData.CreatedTime;
                    }
                    else
                    {
                        photoCamLog.CreatedById = photoCamData.CreatedById;
                        photoCamLog.CreatedByNpp = photoCamData.CreatedByNpp;
                        photoCamLog.CreatedByUid = photoCamData.CreatedByUid;
                        photoCamLog.CreatedByUnit = photoCamData.CreatedByUnit;
                        photoCamLog.CreatedTime = photoCamData.CreatedTime;
                        photoCamLog.FileName = photoCamData.FileName;
                        photoCamLog.Nik = photoCamData.Nik;
                        photoCamLog.PathFile = photoCamData.PathFile;

                        photoCamData.PathFile = filePath;
                        photoCamData.Nik = enroll.KtpNIK;
                        photoCamData.FileName = fileName;
                        photoCamData.IsActive = true;
                        photoCamData.IsDeleted = false;
                        photoCamData.UpdatedById = Id;
                        photoCamData.UpdatedByNpp = npp;
                        photoCamData.UpdatedByUid = enroll.UID;
                        photoCamData.UpdatedTime = DateTime.Now;
                    }
                }
                else
                {
                    if (photoCamData != null)
                    {
                        photoCamLog.CreatedById = photoCamData.CreatedById;
                        photoCamLog.CreatedByNpp = photoCamData.CreatedByNpp;
                        photoCamLog.CreatedByUid = photoCamData.CreatedByUid;
                        photoCamLog.CreatedByUnit = photoCamData.CreatedByUnit;
                        photoCamLog.CreatedTime = photoCamData.CreatedTime;
                        photoCamLog.FileName = photoCamData.FileName;
                        photoCamLog.Nik = photoCamData.Nik;
                        photoCamLog.PathFile = photoCamData.PathFile;
                        photoCamData = new Tbl_DataKTP_PhotoCam();
                    }
                    stringPerubahan = stringPerubahan + "Request Payload Photo Cam Tidak Ditemukan" + " <br/>";
                }

                #endregion

                #region finger
                var exPhotoFinger = new List<Tbl_DataKTP_Finger>();

                var exPhotoFingerEmployee = new List<Tbl_DataKTP_Finger_Employee>();

                var photoFingerLogs = new List<Tbl_DataKTP_Finger_Log>();

                var photoFingers = new List<Tbl_DataKTP_Finger>();

                var photoFingersEmployee = new List<Tbl_DataKTP_Finger_Employee>();

                var photoFingersEmployeeLogs = new List<Tbl_DataKTP_Finger_Employee_Log>();

                var photoFingerExisting = await _profileRepository.GetPhotoFingerExisting(enroll.KtpNIK).ConfigureAwait(false);
                var photoFingerEmployeeExisting = await _profileRepository.GetPhotoFingerEmployeeExisting(enroll.KtpNIK).ConfigureAwait(false);

                foreach (var em in photoFingerEmployeeExisting)
                {
                    photoFingersEmployeeLogs.Add(new Tbl_DataKTP_Finger_Employee_Log
                    {
                        CreatedById = em.CreatedById,
                        CreatedByNpp = em.CreatedByNpp,
                        CreatedByUid = em.CreatedByUid,
                        CreatedByUnit = em.CreatedByUnit,
                        CreatedByUnitId = em.CreatedByUnitId,
                        CreatedTime = em.CreatedTime,
                        //FileJari = em.FileJari,
                        FileName = em.FileName,
                        FileNameISO = em.FileNameISO,
                        Nik = em.Nik,
                        PathFile = em.PathFile,
                        PathFileISO = em.PathFileISO,
                        TypeFinger = em.TypeFinger
                    });
                }

                foreach (var el in photoFingerExisting)
                {
                    photoFingerLogs.Add(new Tbl_DataKTP_Finger_Log
                    {
                        CreatedById = el.CreatedById,
                        CreatedByNpp = el.CreatedByNpp,
                        CreatedByUid = el.CreatedByUid,
                        CreatedByUnit = el.CreatedByUnit,
                        CreatedByUnitId = el.CreatedByUnitId,
                        CreatedTime = el.CreatedTime,
                        //FileJari = el.FileJari,
                        FileJariISO = el.FileJariISO,
                        FileName = el.FileName,
                        FileNameISO = el.FileNameISO,
                        Nik = el.Nik,
                        PathFile = el.PathFile,
                        PathFileISO = el.PathFileISO,
                        TypeFinger = el.TypeFinger
                    });
                }

                if (!string.IsNullOrWhiteSpace(enroll.KtpFingerKanan))
                {

                    var photoFingerDataEmployee = new Tbl_DataKTP_Finger_Employee();

                    string imageEncrypted = enroll.KtpFingerKanan.Encrypt(Phrase.FileEncryption);

                    string filePath = "";
                    string filePathIso = "";

                    var systemParameterPath = await _sysParameterRepository.GetPathFolder("FolderFinger")
                                                .ConfigureAwait(false);
                    string pathFolderFoto = systemParameterPath.Value;

                    string subPathFolderPhotoFinger = pathFolder + "/" + pathFolderFoto + "/" + enroll.KtpNIK + "/";

                    string JenisJari = enroll.KtpTypeJariKanan.Replace(" ", "");
                    string fileName = "PhotoFinger_" + JenisJari + "_" + enroll.KtpNIK + "_" + JamServer + ".txt";
                    string fileNameIso = "";

                    if (_sftpConfig.IsActive)
                    {
                        using var stream = new MemoryStream(Encoding.ASCII.GetBytes(imageEncrypted));

                        (var fname, var fPath) = await stream.UploadToFTPServer(subPathFolderPhotoFinger, fileName, _sftpConfig.Host,
                            _sftpConfig.Username, _sftpConfig.Password, _sftpConfig.RootDirectory, _sftpConfig.Url).ConfigureAwait(false);

                        filePath = fPath;
                    }
                    else
                    {
                        if (!Directory.Exists(subPathFolderPhotoFinger))
                        {
                            Directory.CreateDirectory(subPathFolderPhotoFinger);
                        }

                        filePath = subPathFolderPhotoFinger + fileName;
                        File.WriteAllText(filePath, imageEncrypted);
                    }

                    string isoEncrypted = "";

                    if (!string.IsNullOrWhiteSpace(enroll.KtpFingerKananIso))
                    {
                        isoEncrypted = enroll.KtpFingerKananIso.Encrypt(Phrase.FileEncryption);
                        fileNameIso = "PhotoFingerISO_" + JenisJari + "_" + enroll.KtpNIK + "_" + JamServer + ".txt";

                        if (_sftpConfig.IsActive)
                        {
                            using var stream = new MemoryStream(Encoding.ASCII.GetBytes(isoEncrypted));

                            (var fname, var fPath) = await stream.UploadToFTPServer(subPathFolderPhotoFinger, fileNameIso, _sftpConfig.Host,
                                _sftpConfig.Username, _sftpConfig.Password, _sftpConfig.RootDirectory, _sftpConfig.Url).ConfigureAwait(false);

                            filePathIso = fPath;
                        }
                        else
                        {
                            if (!Directory.Exists(subPathFolderPhotoFinger))
                            {
                                Directory.CreateDirectory(subPathFolderPhotoFinger);
                            }

                            filePathIso = subPathFolderPhotoFinger + fileNameIso;
                            File.WriteAllText(filePathIso, isoEncrypted);
                        }
                    }

                    //var typeFingerMain = await _fingerRepository.GetFingersEnrolled(enroll.KtpNIK);

                    var typeFingerMain = new List<FingerByNik> {
                            new FingerByNik { NIK = enroll.KtpNIK, TypeFinger = (enroll.KtpTypeJariKanan != null) ? enroll.KtpTypeJariKanan : ""},
                            new FingerByNik { NIK = enroll.KtpNIK, TypeFinger = (enroll.KtpTypeJariKiri != null) ? enroll.KtpTypeJariKiri : ""},
                        };

                    foreach (var i in typeFingerMain)
                    {

                        if (i.TypeFinger.Contains("Kanan"))
                        {
                            var photoFingerData = await _profileRepository.GetPhotoFinger(enroll.KtpNIK, i.TypeFinger).ConfigureAwait(false);

                            if (isEmployee)
                            {
                                photoFingerDataEmployee = await _profileRepository.GetPhotoFingerEmployee(enroll.KtpNIK, i.TypeFinger)
                                .ConfigureAwait(false);
                            }

                            if (photoFingerData == null)
                            {
                                photoFingerData = new Tbl_DataKTP_Finger
                                {
                                    CreatedById = Id,
                                    IsActive = true,
                                    IsDeleted = false,
                                    Nik = enroll.KtpNIK,
                                    PathFile = filePath,
                                    PathFileISO = filePathIso,
                                    CreatedByNpp = npp,
                                    CreatedByUid = enroll.UID,
                                    CreatedTime = DateTime.Now,
                                    CreatedByUnit = unitCode,
                                    FileName = fileName,
                                    FileNameISO = fileNameIso,
                                    FileJariISO = isoEncrypted,
                                    TypeFinger = enroll.KtpTypeJariKanan
                                };

                                photoFingers.Add(new Tbl_DataKTP_Finger
                                {
                                    //CreatedById = photoFingerData.CreatedById,
                                    CreatedById = Id,
                                    IsActive = true,
                                    IsDeleted = false,
                                    Nik = enroll.KtpNIK,
                                    PathFile = filePath,
                                    PathFileISO = filePathIso,
                                    //CreatedByNpp = photoFingerData.CreatedByNpp,
                                    //CreatedByUid = photoFingerData.CreatedByUid,
                                    //CreatedTime = photoFingerData.CreatedTime,
                                    CreatedByNpp = npp,
                                    CreatedByUid = enroll.UID,
                                    CreatedTime = DateTime.Now,
                                    UpdatedById = Id,
                                    UpdatedByNpp = npp,
                                    UpdatedByUid = enroll.UID,
                                    UpdatedByUnit = unitCode,
                                    //CreatedByUnitId = photoFingerData.CreatedByUnitId,
                                    CreatedByUnitId = unitId,
                                    CreatedByUnit = photoFingerData.CreatedByUnit,
                                    //FileJari = photoFingerData.FileJari,
                                    FileJariISO = isoEncrypted,
                                    UpdatedTime = DateTime.Now,
                                    FileName = fileName,
                                    FileNameISO = fileNameIso,
                                    TypeFinger = enroll.KtpTypeJariKanan
                                });
                            }
                            else
                            {

                                photoFingers.Add(new Tbl_DataKTP_Finger
                                {
                                    //CreatedById = photoFingerData.CreatedById,
                                    CreatedById = Id,
                                    IsActive = true,
                                    IsDeleted = false,
                                    Nik = enroll.KtpNIK,
                                    PathFile = filePath,
                                    PathFileISO = filePathIso,
                                    //CreatedByNpp = photoFingerData.CreatedByNpp,
                                    //CreatedByUid = photoFingerData.CreatedByUid,
                                    //CreatedTime = photoFingerData.CreatedTime,
                                    CreatedByNpp = npp,
                                    CreatedByUid = enroll.UID,
                                    CreatedTime = DateTime.Now,
                                    UpdatedById = Id,
                                    UpdatedByNpp = npp,
                                    UpdatedByUid = enroll.UID,
                                    UpdatedByUnit = unitCode,
                                    //CreatedByUnitId = photoFingerData.CreatedByUnitId,
                                    CreatedByUnitId = unitId,
                                    CreatedByUnit = photoFingerData.CreatedByUnit,
                                    //FileJari = photoFingerData.FileJari,
                                    FileJariISO = isoEncrypted,
                                    UpdatedTime = DateTime.Now,
                                    FileName = fileName,
                                    FileNameISO = fileNameIso,
                                    TypeFinger = enroll.KtpTypeJariKanan
                                });

                            }

                            if (isEmployee)
                            {

                                photoFingersEmployee.Add(new Tbl_DataKTP_Finger_Employee
                                {
                                    CreatedById = Id,
                                    IsActive = true,
                                    IsDeleted = false,
                                    Nik = enroll.KtpNIK,
                                    PathFile = filePath,
                                    CreatedByNpp = npp,
                                    CreatedByUid = enroll.UID,
                                    CreatedTime = DateTime.Now,
                                    UpdatedById = Id,
                                    UpdatedByNpp = npp,
                                    UpdatedByUid = enroll.UID,
                                    UpdatedByUnit = unitCode,
                                    CreatedByUnitId = unitId,
                                    //FileJari = enroll.KtpFingerKanan,
                                    UpdatedTime = DateTime.Now,
                                    FileName = fileName,
                                    FileJariISO = isoEncrypted,
                                    FileNameISO = fileNameIso,
                                    PathFileISO = filePathIso,
                                    TypeFinger = enroll.KtpTypeJariKanan,
                                });
                            }
                        }
                    }
                }
                else
                {
                    #region get data if no payload but we have to replace all data with no data
                    var photoFingerDataEmployee = new Tbl_DataKTP_Finger_Employee();
                    //var typeFingerMain = await _fingerRepository.GetFingersEnrolled(enroll.KtpNIK);

                    var typeFingerMain = new List<FingerByNik> {
                            new FingerByNik { NIK = enroll.KtpNIK, TypeFinger = (enroll.KtpTypeJariKanan != null) ? enroll.KtpTypeJariKanan : ""},
                            new FingerByNik { NIK = enroll.KtpNIK, TypeFinger = (enroll.KtpTypeJariKiri != null) ? enroll.KtpTypeJariKiri : ""},
                        };

                    foreach (var i in typeFingerMain)
                    {

                        if (i.TypeFinger.Contains("Kanan"))
                        {
                            var photoFingerData = await _profileRepository.GetPhotoFinger(enroll.KtpNIK, i.TypeFinger).ConfigureAwait(false);

                            if (isEmployee)
                            {
                                photoFingerDataEmployee = await _profileRepository.GetPhotoFingerEmployee(enroll.KtpNIK, i.TypeFinger)
                                .ConfigureAwait(false);
                            }



                        }
                    }

                    #endregion

                    stringPerubahan = stringPerubahan + "Request Payload Finger Kanan Tidak Ditemukan" + " <br/>";
                }

                if (!string.IsNullOrWhiteSpace(enroll.KtpFingerKiri))
                {
                    //var photoFingerData = await _profileRepository.GetPhotoFinger(enroll.KtpNIK, enroll.KtpTypeJariKiri)
                    //    .ConfigureAwait(false);

                    var photoFingerDataEmployee = new Tbl_DataKTP_Finger_Employee();

                    //if (isEmployee)
                    //{
                    //    photoFingerDataEmployee = await _profileRepository.GetPhotoFingerEmployee(enroll.KtpNIK, enroll.KtpTypeJariKiri)
                    //    .ConfigureAwait(false);
                    //}

                    string imageEncrypted = enroll.KtpFingerKiri.Encrypt(Phrase.FileEncryption);

                    string filePath = "";
                    string filePathIso = "";

                    var systemParameterPath = await _sysParameterRepository.GetPathFolder("FolderFinger")
                                                .ConfigureAwait(false);
                    string pathFolderFoto = systemParameterPath.Value;

                    string subPathFolderPhotoFinger = pathFolder + "/" + pathFolderFoto + "/" + enroll.KtpNIK + "/";

                    string JenisJari = enroll.KtpTypeJariKiri.Replace(" ", "");
                    string fileName = "PhotoFinger_" + JenisJari + "_" + enroll.KtpNIK + "_" + JamServer + ".txt";
                    string fileNameIso = "";

                    if (_sftpConfig.IsActive)
                    {
                        using var stream = new MemoryStream(Encoding.ASCII.GetBytes(imageEncrypted));

                        (var fname, var fPath) = await stream.UploadToFTPServer(subPathFolderPhotoFinger, fileName, _sftpConfig.Host,
                            _sftpConfig.Username, _sftpConfig.Password, _sftpConfig.RootDirectory, _sftpConfig.Url).ConfigureAwait(false);

                        filePath = fPath;
                    }
                    else
                    {
                        if (!Directory.Exists(subPathFolderPhotoFinger))
                        {
                            Directory.CreateDirectory(subPathFolderPhotoFinger);
                        }

                        filePath = subPathFolderPhotoFinger + fileName;
                        File.WriteAllText(filePath, imageEncrypted);
                    }

                    string isoEncrypted = "";

                    if (!string.IsNullOrWhiteSpace(enroll.KtpFingerKiriIso))
                    {
                        isoEncrypted = enroll.KtpFingerKiriIso.Encrypt(Phrase.FileEncryption);
                        fileNameIso = "PhotoFingerISO_" + JenisJari + "_" + enroll.KtpNIK + "_" + JamServer + ".txt";

                        if (_sftpConfig.IsActive)
                        {
                            using var stream = new MemoryStream(Encoding.ASCII.GetBytes(isoEncrypted));

                            (var fname, var fPath) = await stream.UploadToFTPServer(subPathFolderPhotoFinger, fileNameIso, _sftpConfig.Host,
                                _sftpConfig.Username, _sftpConfig.Password, _sftpConfig.RootDirectory, _sftpConfig.Url).ConfigureAwait(false);

                            filePathIso = fPath;
                        }
                        else
                        {
                            if (!Directory.Exists(subPathFolderPhotoFinger))
                            {
                                Directory.CreateDirectory(subPathFolderPhotoFinger);
                            }

                            filePathIso = subPathFolderPhotoFinger + fileNameIso;
                            File.WriteAllText(filePathIso, isoEncrypted);
                        }
                    }


                    //var typeFingerMain = await _fingerRepository.GetFingersEnrolled(enroll.KtpNIK);

                    var typeFingerMain = new List<FingerByNik> {
                            new FingerByNik { NIK = enroll.KtpNIK, TypeFinger = (enroll.KtpTypeJariKanan != null) ? enroll.KtpTypeJariKanan : ""},
                            new FingerByNik { NIK = enroll.KtpNIK, TypeFinger = (enroll.KtpTypeJariKiri != null) ? enroll.KtpTypeJariKiri : ""},
                        };

                    foreach (var i in typeFingerMain)
                    {

                        if (i.TypeFinger.Contains("Kiri"))
                        {
                            var photoFingerData = await _profileRepository.GetPhotoFinger(enroll.KtpNIK, i.TypeFinger).ConfigureAwait(false);

                            if (isEmployee)
                            {
                                photoFingerDataEmployee = await _profileRepository.GetPhotoFingerEmployee(enroll.KtpNIK, i.TypeFinger)
                                .ConfigureAwait(false);
                            }

                            if (photoFingerData == null)
                            {
                                photoFingerData = new Tbl_DataKTP_Finger
                                {
                                    CreatedById = Id,
                                    IsActive = true,
                                    IsDeleted = false,
                                    Nik = enroll.KtpNIK,
                                    PathFile = filePath,
                                    PathFileISO = filePathIso,
                                    CreatedByNpp = npp,
                                    CreatedByUid = enroll.UID,
                                    CreatedTime = DateTime.Now,
                                    FileName = fileName,
                                    FileNameISO = fileNameIso,
                                    FileJariISO = isoEncrypted,
                                    TypeFinger = enroll.KtpTypeJariKiri
                                };

                                photoFingers.Add(new Tbl_DataKTP_Finger
                                {
                                    //CreatedById = photoFingerData.CreatedById,
                                    CreatedById = Id,
                                    IsActive = true,
                                    IsDeleted = false,
                                    Nik = enroll.KtpNIK,
                                    PathFile = filePath,
                                    PathFileISO = filePathIso,
                                    //CreatedByNpp = photoFingerData.CreatedByNpp,
                                    //CreatedByUid = photoFingerData.CreatedByUid,
                                    //CreatedTime = photoFingerData.CreatedTime,
                                    CreatedByNpp = npp,
                                    CreatedByUid = enroll.UID,
                                    CreatedTime = DateTime.Now,
                                    UpdatedById = Id,
                                    UpdatedByNpp = npp,
                                    UpdatedByUid = enroll.UID,
                                    UpdatedByUnit = unitCode,
                                    //CreatedByUnitId = photoFingerData.CreatedByUnitId,
                                    CreatedByUnitId = unitId,
                                    CreatedByUnit = photoFingerData.CreatedByUnit,
                                    //FileJari = photoFingerData.FileJari,
                                    FileJariISO = isoEncrypted,
                                    UpdatedTime = DateTime.Now,
                                    FileName = fileName,
                                    FileNameISO = fileNameIso,
                                    TypeFinger = enroll.KtpTypeJariKiri
                                });


                            }
                            else
                            {
                                photoFingers.Add(new Tbl_DataKTP_Finger
                                {
                                    //CreatedById = photoFingerData.CreatedById,
                                    CreatedById = Id,
                                    IsActive = true,
                                    IsDeleted = false,
                                    Nik = enroll.KtpNIK,
                                    PathFile = filePath,
                                    PathFileISO = filePathIso,
                                    //CreatedByNpp = photoFingerData.CreatedByNpp,
                                    //CreatedByUid = photoFingerData.CreatedByUid,
                                    //CreatedTime = photoFingerData.CreatedTime,
                                    CreatedByNpp = npp,
                                    CreatedByUid = enroll.UID,
                                    CreatedTime = DateTime.Now,
                                    UpdatedById = Id,
                                    UpdatedByNpp = npp,
                                    UpdatedByUid = enroll.UID,
                                    UpdatedByUnit = unitCode,
                                    //CreatedByUnitId = photoFingerData.CreatedByUnitId,
                                    CreatedByUnitId = unitId,
                                    CreatedByUnit = photoFingerData.CreatedByUnit,
                                    //FileJari = photoFingerData.FileJari,
                                    FileJariISO = isoEncrypted,
                                    UpdatedTime = DateTime.Now,
                                    FileName = fileName,
                                    FileNameISO = fileNameIso,
                                    TypeFinger = enroll.KtpTypeJariKiri
                                });


                            }

                            if (isEmployee)
                            {

                                photoFingersEmployee.Add(new Tbl_DataKTP_Finger_Employee
                                {
                                    CreatedById = Id,
                                    IsActive = true,
                                    IsDeleted = false,
                                    Nik = enroll.KtpNIK,
                                    PathFile = filePath,
                                    CreatedByNpp = npp,
                                    CreatedByUid = enroll.UID,
                                    CreatedTime = DateTime.Now,
                                    UpdatedById = Id,
                                    UpdatedByNpp = npp,
                                    UpdatedByUid = enroll.UID,
                                    UpdatedByUnit = unitCode,
                                    CreatedByUnitId = unitId,
                                    //FileJari = enroll.KtpFingerKiri,
                                    UpdatedTime = DateTime.Now,
                                    FileName = fileName,
                                    FileJariISO = isoEncrypted,
                                    FileNameISO = fileNameIso,
                                    PathFileISO = filePathIso,
                                    TypeFinger = enroll.KtpTypeJariKiri,
                                });


                            }
                        }
                    }
                }
                else
                {
                    #region get data if no payload but we have to replace all data with no data
                    var photoFingerDataEmployee = new Tbl_DataKTP_Finger_Employee();
                    //var typeFingerMain = await _fingerRepository.GetFingersEnrolled(enroll.KtpNIK);
                    var typeFingerMain = new List<FingerByNik> {
                            new FingerByNik { NIK = enroll.KtpNIK, TypeFinger = (enroll.KtpTypeJariKanan != null) ? enroll.KtpTypeJariKanan : ""},
                            new FingerByNik { NIK = enroll.KtpNIK, TypeFinger = (enroll.KtpTypeJariKiri != null) ? enroll.KtpTypeJariKiri : ""},
                        };

                    foreach (var i in typeFingerMain)
                    {

                        if (i.TypeFinger.Contains("Kiri"))
                        {
                            var photoFingerData = await _profileRepository.GetPhotoFinger(enroll.KtpNIK, i.TypeFinger).ConfigureAwait(false);

                            if (isEmployee)
                            {
                                photoFingerDataEmployee = await _profileRepository.GetPhotoFingerEmployee(enroll.KtpNIK, i.TypeFinger)
                                .ConfigureAwait(false);
                            }

                        }
                    }
                    #endregion

                    stringPerubahan = stringPerubahan + "Request Payload Finger Kanan Tidak Ditemukan" + " <br/>";
                }
                #endregion

                var (status, msg) = await _enrollmentKTPRepository.InsertEnrollFlow3(dataKTPTemp, logDemografi, photoKTPData, photoKtpLog, photoSignatureData,
                    photoSignatureLog, photoCamData, photoCamLog, photoFingers, photoFingerLogs, exPhotoKtp, exphotoSignatureData, exphotoCamData, photoFingerExisting, dataNpp, dataReaderLog, photoFingersEmployee, photoFingerEmployeeExisting, photoFingersEmployeeLogs);

                if (status)
                {
                    return (_SuccessMessageConfig.NasabahSuksesUpdate, (int)ServiceResponseStatus.SUKSES, cekKTPTemp.CIF);
                }
                else
                {
                    return (_ErrorMessageConfig.DemografiGagalEnroll, (int)ServiceResponseStatus.ERROR, msg + " " + stringPerubahan);
                }
            }
            #endregion
            else
            {
                #region Logging Reader Activity
                _ = _alatReaderRepository.CreateLogActivity2(dataReaderActivityLog);
                _ = _alatReaderRepository.CreateAlatReaderLog(dataReaderLog);
                #endregion

                return (_ErrorMessageConfig.DemografiTidakDitemukan, (int)ServiceResponseStatus.Data_Empty, "");
            }
            #endregion
        }

        public async Task<string> VerifyEnrollment(string nik, string npp, string comment)
        {
            var prof = await _profileRepository.GetDataDemografis(nik).ConfigureAwait(false);

            if (prof == null) return "data empty";

            prof.IsVerified = true;
            prof.VerifiedByNpp = npp;
            prof.VerifyComment = comment;

            await _profileRepository.UpdateDataDemografis(prof).ConfigureAwait(false);

            return "SUKSES";
        }

        public async Task<string> ConfirmSubmission(ConfirmEnrollSubmissionVM req)
        {
            var prof = await _profileRepository.GetDataDemografis(req.NIK).ConfigureAwait(false);

            if (prof == null) return "data empty";

            prof.IsVerified = req.IsVerified;
            prof.VerifiedByNpp = req.VerifiedByNpp;
            prof.VerifyComment = req.VerifyComment;
            prof.UpdatedTime = DateTime.Now;
            prof.UpdatedById = req.UpdatedById;

            await _profileRepository.UpdateDataDemografis(prof).ConfigureAwait(false);

            Tbl_LogHistoryPengajuan tbl_LogHistoryPengajuan = new Tbl_LogHistoryPengajuan();
            tbl_LogHistoryPengajuan.IsVerified = req.IsVerified;
            tbl_LogHistoryPengajuan.DataKTPNIK = req.NIK;
            tbl_LogHistoryPengajuan.DataKTPId = prof.Id;
            tbl_LogHistoryPengajuan.CreatedTime = DateTime.Now;
            tbl_LogHistoryPengajuan.CreatedBy_Id = req.UpdatedById;
            tbl_LogHistoryPengajuan.ConfirmedByNpp = req.VerifiedByNpp;
            tbl_LogHistoryPengajuan.Comment = req.VerifyComment;

            _profileRepository.InsertHistoryPengajuan(tbl_LogHistoryPengajuan);

            return "SUKSES";
        }

        public async Task<List<FingerISOVM>> GetISO(string nik)
        {
            var resp = new List<FingerISOVM>();
            try
            {
                var _emp = await _enrollmentKTPRepository.IsEmployee(nik);

                if (_emp == null)
                {
                    var res = await _enrollmentKTPRepository.GetISO(nik);

                    if (res == null)
                    {
                        return null;
                    }

                    foreach (var item in res)
                    {
                        if (item.FileNameISO == null || item.PathFileISO == null)
                        {
                            return null;
                        }
                        var (stats, _decrypt) = await ConvertUrlToB64(item.PathFileISO);

                        resp.Add(new FingerISOVM
                        {
                            TypeFinger = item.TypeFinger,
                            Base64Iso = stats ? _decrypt : ""
                        });
                    }
                    return resp;
                }
                else
                {
                    var res = await _enrollmentKTPRepository.GetISOEmp(nik);
                    if (res == null)
                    {
                        return null;
                    }

                    foreach (var item in res)
                    {
                        if (item.FileNameISO == null || item.PathFileISO == null)
                        {
                            return null;
                        }
                        var (stats, _decrypt) = await ConvertUrlToB64(item.PathFileISO);

                        resp.Add(new FingerISOVM
                        {
                            TypeFinger = item.TypeFinger,
                            Base64Iso = stats ? _decrypt : ""
                        });
                    }

                    return resp;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<(bool status, string msg)> ConvertUrlToB64(string path)
        {
            try
            {
                using WebClient webClient = new();

                var b64 = webClient.DownloadData(path ?? "");

                var b64String = "";

                using (var r = new StreamReader(new MemoryStream(b64)))
                {
                    var text = r.ReadToEnd();
                    b64String = text.Decrypt(Phrase.FileEncryption);
                }

                return (true, b64String);
            }
            catch (Exception ex)
            {
                return (false, ex.Message.ToString());
            }
        }
    }
}
