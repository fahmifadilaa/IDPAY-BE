using Ekr.Core.Entities.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ekr.Core.Entities.SettingThreshold
{
    public class SettingThresholdFilter : BaseSqlGridFilter
    {
        public string NIK { get; set; } = null;
        public List<int>? StatusPengajuan { get; set; }
        public int? ID { get; set; }
    }

    public class SettingThresholdRequest
    {
        public int Id { get; set; }
    }

    public class SettingThresholdStatusRequest : SettingThresholdRequest
    {
        public int Status { get; set; }
        public string Alasan { get; set; }
        public int ApproverId { get; set; }
    }
}
