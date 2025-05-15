using Ekr.Core.Entities.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ekr.Core.Entities.DataMaster.MasterApps
{
    public class MasterAppsFilter : BaseSqlGridFilter
    {
        public string Name { get; set; } = null;
    }

    public class MasterAppsByIdVM
    {
        public int Id { get; set; }
    }
}
