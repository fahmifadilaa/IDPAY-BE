namespace Ekr.Core.Entities.Base
{
    public class BaseSqlGridFilter
    {
        public string SortColumn { get; set; } = "Id";
        public string SortColumnDir { get; set; } = "desc";
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}
