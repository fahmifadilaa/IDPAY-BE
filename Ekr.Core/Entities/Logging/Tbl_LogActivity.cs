using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ekr.Core.Entities.Logging
{
    public class Tbl_LogActivity
    {
        [AutoIncrement]
        public int Id { get; set; }
		public int UserId { get; set; }
		public int Unit_Id { get; set; }
		public string Npp { get; set; }
		public string Url { get; set; }
		public string DataLama { get; set; }
		public string DataBaru { get; set; }
		public DateTime ActionTime { get; set; }
		public string Browser { get; set; }
		public string IP { get; set; }
		public string OS { get; set; }
		public string ClientInfo { get; set; }
		public string Keterangan { get; set; }
	}
}
