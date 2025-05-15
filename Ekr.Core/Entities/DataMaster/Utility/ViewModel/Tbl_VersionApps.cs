using ServiceStack.DataAnnotations;
using System;

namespace Ekr.Core.Entities.DataMaster.Utility.ViewModel
{
    public class Tbl_VersionAgent
    {
        [AutoIncrement]
        public int Id { get; set; }
        public decimal Version { get; set; }
        public string Keterangan { get; set; }
        public string Path { get; set; }
        public string FileName { get; set; }
        public int CreatedById { get; set; }
        public string CreatedByNpp { get; set; }
        public DateTime? CreatedTime { get; set; } = DateTime.Now;
        public string CreatedByUnit { get; set; }
        public int UpdatedById { get; set; }
        public string UpdatedByNpp { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public string UpdatedByUnit { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
    }
}
