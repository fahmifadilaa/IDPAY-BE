using ServiceStack.DataAnnotations;
using System;

namespace Ekr.Core.Entities.DataKTP
{
    public class Tbl_Mapping_Pegawai_KTP
    {
        [AutoIncrement]
        public int Id { get; set; }
        public string Npp { get; set; }
        public string NIK { get; set; }
        public int CreatedById { get; set; }
        public string CreatedByNpp { get; set; }
        public DateTime? CreatedTime { get; set; }
        public string CreatedByUnit { get; set; }
        public string CreatedByUID { get; set; }
        public int UpdatedById { get; set; }
        public string UpdatedByNpp { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public string UpdatedByUnit { get; set; }
        public string UpdatedByUID { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
    }
}
