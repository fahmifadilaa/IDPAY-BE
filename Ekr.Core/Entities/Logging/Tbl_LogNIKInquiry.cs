using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ekr.Core.Entities.Logging
{
    public class Tbl_LogNIKInquiry
	{
		[AutoIncrement]
		public int Id { get; set; }
		public string Npp { get; set; }
		public string Url { get; set; }
		public string Nik { get; set; }
		public string SearchParam { get; set; }
		public string Action { get; set; }
		public DateTime CreatedTime { get; set; }
		public string IpAddress { get; set; }
		public string Browser { get; set; }
	}
}
