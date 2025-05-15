using Ekr.Core.Entities.Base;

namespace Ekr.Core.Entities.MessageCode
{
    public class MessageCodeFilter : BaseSqlGridFilter
    {
        public string Message { get; set; } = null;
        public string Code { get; set; } = null;
    }

    public class MessageCodeByIdVM
    {
        public int Id { get; set; }
    }
}
