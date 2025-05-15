using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ekr.Core.Entities.SettingThreshold
{
    public class SettingThresholdData
    {
        public int Number { get; set; }
        public int Id { get; set; }
        public string NIK { get; set; }
        public string Start_Date { get; set; }
        public string End_Date { get; set; }
        public int? IsTemp { get; set; }
        public int? Probability_Division { get; set; }
        public string? Keterangan { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public string? CreatedTime { get; set; } = null;
        public string? UpdatedTime { get; set; } = null;
        public string? DeletedTime { get; set; } = null;
        public string? CreatedBy { get; set; } = null;
        public string ApproverName { get; set; }
        public string UnitName { get; set; }
        public string StatusPengajuan { get; set; }
    }
}
