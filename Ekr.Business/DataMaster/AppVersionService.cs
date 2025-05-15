using Ekr.Business.Contracts.DataMaster;
using Ekr.Core.Configuration;
using Ekr.Core.Entities.DataMaster.AlatReader;
using Ekr.Core.Entities.DataMaster.Utility.ViewModel;
using Ekr.Core.Helper;
using Ekr.Repository.Contracts.DataMaster.SystemParameters;
using Ekr.Repository.Contracts.DataMaster.Utility;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Ekr.Business.DataMaster
{
    public class AppVersionService : IAppVersionService
    {
        private readonly IUtilityRepository _utilityRepository;
        private readonly ISysParameterRepository _sysParameterRepository;
        private readonly SftpConfig _sftpConfig;

        public AppVersionService(IUtilityRepository utilityRepository,
            ISysParameterRepository sysParameterRepository,
            IOptions<SftpConfig> options)
        {
            _utilityRepository = utilityRepository;
            _sysParameterRepository = sysParameterRepository;
            _sftpConfig = options.Value;
        }

        public async Task UploadApps(UploadAppsReq uploadAppsReq, int Id, string npp, string unitCode)
        {
            if (uploadAppsReq.File.Length > 0)
            {
                using var memoryStream = new MemoryStream();

                await uploadAppsReq.File.CopyToAsync(memoryStream)
                    .ConfigureAwait(false);

                var byteFile = memoryStream.ToArray();

                //var sysPathFolder = await _sysParameterRepository.GetPathFolder("PathFolderData");
                var sysPathFolder = await _sysParameterRepository.GetPathFolder("PathFolderApps");
                string pathFolder = sysPathFolder.Value;

                string filePath = "";

                //const string pathFolderFoto = "Apps";

                //string subPathFolderPhotoFinger = pathFolder + "/" + pathFolderFoto + "/" + uploadAppsReq.Version.ToString() + "/";
                string subPathFolderPhotoFinger = pathFolder + "/";

                string fileName = "AppsAgent_" + uploadAppsReq.Version.ToString() + "_" + DateTime.Now.ToString("ddMMyyyyHHmmss") + ".exe";

                if (_sftpConfig.IsActive)
                {
                    (var fname, var fPath) = await memoryStream.UploadToFTPServer(subPathFolderPhotoFinger, fileName, _sftpConfig.Host,
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
                    File.WriteAllBytes(filePath, byteFile);
                }

                _utilityRepository.UploadAppsVersion(new Tbl_VersionAgent
                {
                    Version = uploadAppsReq.Version,
                    CreatedById = Id,
                    Keterangan = uploadAppsReq.Keterangan,
                    Path = fileName,
                    FileName = fileName,
                    CreatedByNpp = npp,
                    CreatedByUnit = unitCode
                });
            }
        }

        public async Task<(byte[] fileByte, string appsVersion)> CheckVersion(CheckAppsVersionRequest checkAppsVersionRequest)
        {
            var apps = _utilityRepository.GetLatestAppsVersion();
            if (checkAppsVersionRequest.Version >= apps.Version || apps.Path == null)
            {
                return (null, null);
            }

            string localFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\" + apps.Path);

            var fileStream = new StreamContent(new FileStream(localFilePath, FileMode.Open, FileAccess.Read));
            var fileByte = await fileStream.ReadAsByteArrayAsync();

            return (fileByte, apps.Version.ToString());
        }

        public async Task<(string fileBase64, string filePath)> CheckVersionV2(CheckAppsVersionRequest checkAppsVersionRequest)
        {
            var apps = _utilityRepository.GetLatestAppsVersion();
            if (checkAppsVersionRequest.Version >= apps.Version || apps.Path == null)
            {
                return (null, null);
            }

            var virtualURL = await _sysParameterRepository.GetPathFolder("VirtualPathApps");
            string urlApps = virtualURL.Value;
            urlApps += "/" + apps.FileName;

            var localFile = await _sysParameterRepository.GetPathFolder("PathFolderApps");
            string localFilePath = localFile.Value + "/" + apps.FileName;

            Byte[] bytes = File.ReadAllBytes(localFilePath);
            String fileBase64 = Convert.ToBase64String(bytes);

            return (fileBase64, urlApps);
        }

        public async Task<(string fileBase64, string filePath)> GetVersionById(int Id)
        {
            var apps = _utilityRepository.GetAppsVersionById(Id);
            if (apps == null || Id == 0)
            {
                return (null, null);
            }

            var virtualURL = await _sysParameterRepository.GetPathFolder("VirtualPathApps");
            string urlApps = virtualURL.Value;
            urlApps += "/" + apps.FileName;

            var localFile = await _sysParameterRepository.GetPathFolder("PathFolderApps");
            string localFilePath = localFile.Value + "/" + apps.FileName;

            Byte[] bytes = File.ReadAllBytes(localFilePath);
            String fileBase64 = Convert.ToBase64String(bytes);

            return (fileBase64, urlApps);
        }
    }
}
