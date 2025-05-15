using System.Collections.Generic;

namespace Ekr.Core.Entities
{
    public class ServiceResponse<T>
    {
        public string Message { get; set; }
        public int Status { get; set; }
        public int Code { get; set; }
        public T Data { get; set; }
    }
    public class ServiceResponse
    {
        public string Message { get; set; }
        public int Status { get; set; }
    }

    public class ServiceResponses<T>
    {
        public string Message { get; set; }
        public int Status { get; set; }
        public IEnumerable<T> Data { get; set; }
    }

    public class ServiceResponseResult<T>
    {
        public string Message { get; set; }
        public int Status { get; set; }
        public int Code { get; set; }
        public T Result { get; set; }
    }
}
