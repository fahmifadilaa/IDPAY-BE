using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;
using UdpClient;
using UdpClient.Services;

class Program
{
    static async Task Main(string[] args)
    {
        // Setup configuration from appsettings.json
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        // Setup DI
        var services = new ServiceCollection();
        services.AddLogging(config => config.AddConsole());

        services.AddSingleton<UdpLogService>();

        var serviceProvider = services.BuildServiceProvider();

        var udpLogger = serviceProvider.GetRequiredService<UdpLogService>();
        //await udpLogger.SendAsync("Log from console with config!");
    }
}
