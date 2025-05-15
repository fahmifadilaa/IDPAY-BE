using FluentFTP;
using Renci.SshNet;
using Renci.SshNet.Async;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Ekr.Core.Helper
{
    public static class SftpHelper
    {
        public static async Task<string> UploadToStaticServer(this Stream stream, string path, string fileName,
            string host, int port, string username, string password, string rootDirectory, string url)
        {
            using (stream)
            {
                using var sftp = new SftpClient(host, port, username, password);
                sftp.Connect();
                if (!sftp.IsConnected) return string.Empty;

                var thePath = path + "/" + fileName;
                var fullPath = rootDirectory + "/" + thePath;

                await sftp.UploadAsync(stream, fullPath, true);
                return url + thePath.Replace("/static", string.Empty);
            }
        }

        public static async Task<(string fileName, string filePath)> UploadToFTPServer(this Stream stream, string path, string fileName,
            string host, string username, string password, string rootDirectory, string url)
        {
            using (stream)
            {
                var token = new CancellationToken();
                using var ftp = new FtpClient(host, username, password);

                await ftp.ConnectAsync(token).ConfigureAwait(false);

                var thePath = path + "/" + fileName;
                var fullPath = rootDirectory + "/" + thePath;

                // upload a file and ensure the FTP directory is created on the server
                await ftp.UploadAsync(stream, fullPath, FtpRemoteExists.Overwrite, true).ConfigureAwait(false);

                return (fileName, url + thePath);
            }
        }
    }
}
