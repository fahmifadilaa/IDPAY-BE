using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Reflection;

namespace ConsoleWriteLog.Tools
{
    public class Config
    {
        public static IConfiguration AppSetting { get; }
        static Config()
        {
            AppSetting = new ConfigurationBuilder()
                    .SetBasePath(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location))
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();
        }
    }

}
