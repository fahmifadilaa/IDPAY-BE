using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ekr.Core.Entities.DataMaster.Utility.Entity
{
    public class TblUserSession
    {
        public int UserId { get; set; }
        public string SessionId { get; set; }
        public DateTime LastActive { get; set; }
        public string Info { get; set; }
        public int? RoleId { get; set; }
        public int? UnitId { get; set; }
    }
}
