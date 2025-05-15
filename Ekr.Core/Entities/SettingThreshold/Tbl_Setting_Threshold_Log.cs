using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ekr.Core.Entities.SettingThreshold
{
    public class Tbl_Setting_Threshold_Log
    {
        [AutoIncrement]
        public int Id { get; set; }
        public int Threshold_Id { get; set; }
        public string Alasan { get; set; }
        public int Status { get; set; }
        public virtual DateTime? CreatedTime { get; set; }
        public int? CreatedBy_Id { get; set; }
        public int? Approver_Id { get; set; }
    }
}
