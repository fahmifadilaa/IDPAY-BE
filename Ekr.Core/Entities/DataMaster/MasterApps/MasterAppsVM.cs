using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ekr.Core.Entities.DataMaster.MasterApps
{
    public class MasterAppsVM
    {
        public Int64 Number { get; set; }
        public int Id { get; set; }
        public string Nama { get; set; }
        public string Token { get; set; }
        public DateTime? CreatedTime { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public DateTime? DeletedTime { get; set; }
        public string CreatedByNpp { get; set; }
        public string UpdatedByNpp { get; set; }
        public string DeletedByNpp { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
    }
}
