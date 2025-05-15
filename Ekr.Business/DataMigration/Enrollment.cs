using Ekr.Business.Contracts.DataMigration;
using Ekr.Core.Constant;
using Ekr.Core.Entities.DataKTP;
using Ekr.Core.Securities.Symmetric;
using Ekr.Repository.Contracts.DataMaster.SystemParameters;
using Ekr.Repository.Contracts.Enrollment;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Ekr.Business.DataMigration
{
    public class Enrollment : IEnrollment
    {
        private readonly IEnrollmentKTPRepository _enrollmentKTPRepository;
        private readonly ISysParameterRepository _sysParameterRepository;

        public Enrollment(IEnrollmentKTPRepository enrollmentKTPRepository,
            ISysParameterRepository sysParameterRepository
            )
        {
            _enrollmentKTPRepository = enrollmentKTPRepository;
            _sysParameterRepository = sysParameterRepository;
        }

        public async Task<int> MigrateFingerJpgToEncTxt()
        {
            var count = 0;

            var fingers = await _enrollmentKTPRepository.GetDataKtpFingersJpgFormatted();

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
                string subPathFolderPhotoFinger = pathFolder + "/" + pathFolderFoto + "/" + finger.Nik+ "/";

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

                await _enrollmentKTPRepository.UpdateDataKtpFingerAsync(finger);

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

        public async Task<int> MigrateFingerJpgToEncTxtByNIK(string nik)
        {
            var count = 0;

            var fingers = await _enrollmentKTPRepository.GetDataKtpFingersJpgFormattedbyNIK(nik);

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

                await _enrollmentKTPRepository.UpdateDataKtpFingerAsync(finger);

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

        private long CreateFingerLog(Tbl_DataKTP_Finger finger)
        {
            return _enrollmentKTPRepository.InsertKtpFingerLog(new Tbl_DataKTP_Finger_Log
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
				FileJariISO = finger.FileJariISO,
				FileName = finger.FileName,
                PathFile = finger.PathFile
            });
        }
    }
}
