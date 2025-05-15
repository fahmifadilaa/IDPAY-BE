using System;

namespace Ekr.Core.Entities.DataMaster.AlatReader
{
	public class Tbl_MasterAlatReaderLogActvity
	{
		public int Id { get; set; }
		public string UID { get; set; }
		public string Type { get; set; }
		public string NIK { get; set; }
		public string LastIP { get; set; }
		public int? PegawaiId { get; set; }
		public int? UnitId { get; set; }
		public string NppPegawai { get; set; }
		public string KodeUnit { get; set; }
		public string SessionId { get; set; }
		public string ReqCode { get; set; }
		public string ResultSocket { get; set; }
		public bool? IsActive { get; set; }
		public bool? IsDeleted { get; set; }
		public DateTime? CreatedTime { get; set; }
		public DateTime? UpdatedTime { get; set; }
		public DateTime? DeletedTime { get; set; }
		public int? CreatedBy_Id { get; set; }
		public int? UpdatedBy_Id { get; set; }
		public int? DeletedBy_Id { get; set; }
	}
}
