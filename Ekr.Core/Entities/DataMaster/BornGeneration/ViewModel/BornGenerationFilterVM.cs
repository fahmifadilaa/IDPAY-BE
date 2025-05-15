using Ekr.Core.Entities.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ekr.Core.Entities.DataMaster.BornGeneration.ViewModel
{
    public class BornGenerationFilterVM : BaseSqlGridFilter
    {
        public string Nama { get; set; }
    }
}
