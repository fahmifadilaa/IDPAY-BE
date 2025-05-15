using ServiceStack.DataAnnotations;
using System;

namespace ConsoleWriteLog.ViewModels
{
    public class Tbl_LogError
    {
        [AutoIncrement]
        public int Id { get; set; }
        public string Payload { get; set; }
        public string StackTrace { get; set; }
        public string InnerException { get; set; }
        public string Source { get; set; }
        public string SystemName { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
