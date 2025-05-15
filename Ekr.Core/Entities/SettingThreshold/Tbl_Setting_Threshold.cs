using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ekr.Core.Entities.SettingThreshold
{
    public class Tbl_Setting_Threshold
    {
        [AutoIncrement]
        public int Id { get; set; }
        public string NIK { get; set; }
        public virtual DateTime? Start_Date { get; set; }
        public virtual DateTime? End_Date { get; set; }
        public int? IsTemp { get; set; }
        public int? Probability_Division { get; set; }
        public string? Keterangan { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public DateTime? CreatedTime { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public DateTime? DeletedTime { get; set; }
        public int? CreatedBy_Id { get; set; }
        public int? UpdatedBy_Id { get; set; }
        public int? DeletedBy_Id { get; set; }
        public int? ApproverByEmployeeId { get; set; }
        public int? ApproverByEmployeeId2 { get; set; }
        public int? UnitId { get; set; }
        public int? StatusPengajuan { get; set; }
    }
}
