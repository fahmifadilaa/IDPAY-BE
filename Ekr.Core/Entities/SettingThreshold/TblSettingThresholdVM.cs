using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Ekr.Core.Entities.SettingThreshold
{
    public class TblSettingThresholdVM : Tbl_Setting_Threshold
    {

        [JsonIgnore]
        public override DateTime? Start_Date { get; set; }

        [JsonIgnore]
        public override DateTime? End_Date { get; set; }

        [JsonPropertyName("startDate")]
        public string StartDateString
        {
            get { return Start_Date != null ? Start_Date.Value.ToString("dd/MM/yyyy") : null; }
            set { Start_Date = Start_Date != null ? DateTime.ParseExact(value, "dd/MM/yyyy", null) : null; }
        }

        [JsonPropertyName("endDate")]
        public string EndDateString
        {
            get { return End_Date != null ? End_Date.Value.ToString("dd/MM/yyyy") : null; }
            set { End_Date = End_Date != null ? DateTime.ParseExact(value, "dd/MM/yyyy", null) : null; }
        }
    }
}
