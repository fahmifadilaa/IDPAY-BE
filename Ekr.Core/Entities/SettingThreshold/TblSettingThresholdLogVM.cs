using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Ekr.Core.Entities.SettingThreshold
{
    public class TblSettingThresholdLogVM : Tbl_Setting_Threshold_Log
    {

        [JsonIgnore]
        public override DateTime? CreatedTime { get; set; }

        [JsonPropertyName("createdTime")]
        public string CreatedTimeString { get; set; }

        [JsonPropertyName("createdByName")]
        public string CreatedByName { get; set; }

        [JsonPropertyName("statusName")]
        public string StatusName { get; set; }
    }
}
