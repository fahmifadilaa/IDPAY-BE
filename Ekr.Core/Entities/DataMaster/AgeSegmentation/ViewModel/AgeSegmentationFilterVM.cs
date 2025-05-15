using Ekr.Core.Entities.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ekr.Core.Entities.DataMaster.AgeSegmentation.ViewModel
{
    public class AgeSegmentationFilterVM : BaseSqlGridFilter
    {
        public string Nama { get; set; }
    }
}
