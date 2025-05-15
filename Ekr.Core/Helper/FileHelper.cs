using Ekr.Core.Constant;
using System;
using System.IO;
using System.Text;

namespace Ekr.Core.Helper
{
    public static class FileHelper
    {
        public static void WriteOrReplaceFileContent(string content, string fileName, string extention)
        {
            var path = Directory.GetCurrentDirectory() + @"/" + fileName + "." + extention;

            // Generate file if the file doesn't exist
            if (!File.Exists(path))
            {
                File.WriteAllText(path, "");
            }

            // Read file content
            string exContent;

            using (var r = new StreamReader(path))
            {
                exContent = r.ReadToEnd();
            }

            // Update the content
            StringBuilder sb = new StringBuilder(exContent);

            sb.AppendLine(content);

            File.WriteAllText(path, sb.ToString());
        }

        public static void WriteOrReplaceFileContentOrCreateNewFile(string content, string path, string fileName, string extention, TimingInterval interval)
        {
            string time = interval switch
            {
                TimingInterval.DAILY => DateTime.Now.ToString("yyyyMMdd"),
                TimingInterval.MONTHLY => DateTime.Now.ToString("yyyyMM"),
                TimingInterval.YEARLY => DateTime.Now.ToString("yyyy"),
                _ => DateTime.Now.ToString("yyMMdd"),
            };

            path = path + @"/" + DateTime.Now.ToString("yyyy") + @"/" + DateTime.Now.ToString("MMMM")
                + @"/";

            var fullPath = path + fileName + time + "." + extention;

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            // Generate file if the file doesn't exist
            if (!File.Exists(fullPath))
            {
                File.WriteAllText(fullPath, "");
            }

            // Read file content
            string exContent;

            using (var r = new StreamReader(fullPath))
            {
                exContent = r.ReadToEnd();
            }

            // Update the content
            StringBuilder sb = new StringBuilder(exContent);

            sb.AppendLine(content);

            File.WriteAllText(fullPath, sb.ToString());
        }

    }
}
