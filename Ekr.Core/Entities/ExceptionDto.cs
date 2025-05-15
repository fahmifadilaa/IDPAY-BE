using System;

namespace Ekr.Core.Entities
{
    public class ExceptionDto
    {
        public string ExceptionMessage { get; set; }
        public Exception ExceptionTrace { get; set; }
        public string TicketNumber { get; set; }
    }
}
