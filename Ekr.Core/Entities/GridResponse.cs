using System.Collections.Generic;

namespace Ekr.Core.Entities
{
    public class GridResponse<T>
    {
        public IEnumerable<T> Data { get; set; }
        public int Count { get; set; }
    }

    public class GridResponseNew<T>
    {
        public IEnumerable<T> Data { get; set; }
        public int Count { get; set; }
        public int CountTotal { get; set; }
    }
}
