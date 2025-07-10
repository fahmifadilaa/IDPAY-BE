using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace UdpClient.Services
{
    public class UdpLogService
    {
        private readonly ILogger<UdpLogService> _logger;

        public UdpLogService(IConfiguration configuration, ILogger<UdpLogService> logger)
        {
            _logger = logger;
        }

        public LogObject CreateLogObject(
            Guid id,
            DateTime startTime,
            DateTime endTime,
            string clientIp,
            string clientPort,
            string serviceName,
            string methodName,
            string processName,
            object payload,
            int httpResponseCode,
            string responseMessage)
        {
            var elapsed = endTime - startTime;

            return new LogObject
            {
                ID = id,
                Time = new LogTime
                {
                    StartedTime = startTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    EndTime = endTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    ElapsedTime = $"{(int)elapsed.TotalMilliseconds} ms"
                },
                Client = new LogClient
                {
                    IP = clientIp,
                    Port = clientPort
                },
                Service = new LogService
                {
                    ServiceName = serviceName,
                    MethodName = methodName,
                    ProcessName = processName,
                },
                Payload = payload,
                Response = new LogResponse
                {
                    HttpResponse = httpResponseCode.ToString(),
                    Message = responseMessage
                }
            };
        }

        /// <summary>
        /// Masks sensitive fields present in the given object by replacing their values with "*****".
        /// </summary>
        /// <param name="obj">The original object containing fields to mask.</param>
        /// <param name="fieldsToMask">Array of field names to be masked.</param>
        /// <returns>A new object with the sensitive fields masked.</returns>
        public static object MaskSensitiveFields(object obj, string[] fieldsToMask)
        {
            var dict = new Dictionary<string, object>();

            if (obj == null)
                return dict;

            foreach (var prop in obj.GetType().GetProperties())
            {
                var name = prop.Name;
                var value = prop.GetValue(obj);

                if (fieldsToMask.Contains(name, StringComparer.OrdinalIgnoreCase))
                {
                    dict[name] = value != null ? "*****" : null;
                }
                else
                {
                    dict[name] = value;
                }
            }

            return dict;
        }

        public async Task SendAsync(string message, string host, int port)
        {
            using var udpClientInstance = new System.Net.Sockets.UdpClient(); // Fully qualify the type to avoid ambiguity
            try
            {
                byte[] bytes = Encoding.UTF8.GetBytes(message);
                await udpClientInstance.SendAsync(bytes, bytes.Length, host, port);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send UDP log: {Message}", ex.Message);
            }
        }
    }
}
