using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UdpClient
{
    public class LogObject
    {
        public Guid ID { get; set; }
        public LogTime Time { get; set; }
        public LogClient Client { get; set; }
        public LogService Service { get; set; }
        public object Payload { get; set; }
        public LogResponse Response { get; set; }
    }

    public class LogTime
    {
        public string StartedTime { get; set; }
        public string EndTime { get; set; }
        public string ElapsedTime { get; set; }
    }

    public class LogClient
    {
        public string IP { get; set; }
        public string Port { get; set; }
    }

    public class LogService
    {
        public string ServiceName { get; set; }
        public string MethodName { get; set; }
        public string ProcessName { get; set; }
    }

    public class LogResponse
    {
        public string HttpResponse { get; set; }
        public string Message { get; set; }
    }

}
