using Ekr.Core.Entities.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ekr.Core.Entities.DataMaster.MasterTreshold
{
    public class TresholdFilter : BaseSqlGridFilter
    {
        public string Filter { get; set; } = null;
    }

    public class TresholdByIdVM
    {
        public int Id { get; set; }
    }
}
