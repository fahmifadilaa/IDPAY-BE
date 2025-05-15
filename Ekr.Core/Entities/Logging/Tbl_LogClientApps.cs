using ServiceStack.DataAnnotations;
using System;

namespace Ekr.Core.Entities.Logging
{
	public class Tbl_LogClientApps
	{
        [AutoIncrement]
        public int Id { get; set; }
        public string Param { get; set; }
        public string LvTeller { get; set; }
        public string Branch { get; set; }
        public string SubBranch { get; set; }
        public string ClientApps { get; set; }
        public string EndPoint { get; set; }
        public DateTime RequestTime { get; set; }
        public DateTime ResponseTime { get; set; }
        public string ErrorMessage { get; set; }
        public DateTime CreatedTime { get; set; }
        public string CreatedByNpp { get; set; }
        public string CreatedByUnitCode { get; set; }
    }
}
