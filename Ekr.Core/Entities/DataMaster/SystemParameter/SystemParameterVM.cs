using System;

namespace Ekr.Core.Entities.DataMaster.SystemParameter
{
    public class TblSystemParameter
    {
        public int Id { get; set; }
        public string KataKunci { get; set; }
        public string Value { get; set; }
        public string Keterangan { get; set; }
        public DateTime? CreatedTime { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public DateTime? DeletedTime { get; set; }
        public int? CreatedById { get; set; }
        public int? UpdatedById { get; set; }
        public int? DeletedById { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsDelete { get; set; }
    }
}
