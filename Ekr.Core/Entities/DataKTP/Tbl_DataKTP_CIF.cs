using ServiceStack.DataAnnotations;
using System;

namespace Ekr.Core.Entities.DataKTP
{
    public class Tbl_DataKTP_CIF
    {
		[AutoIncrement]
		public int Id { get; set; }
		public string NIK { get; set; }
		public string CIF { get; set; }
		public string Source { get; set; }
		public int? CreatedById { get; set; }
		public DateTime? CreatedTime { get; set; }
		public int? UpdatedById { get; set; }
		public DateTime? UpdatedTime { get; set; }
		public bool? IsActive { get; set; }
		public bool? IsDeleted { get; set; }
	}
}
