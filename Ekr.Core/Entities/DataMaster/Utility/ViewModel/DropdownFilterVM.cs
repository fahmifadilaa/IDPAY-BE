using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ekr.Core.Entities.DataMaster.Utility.ViewModel
{
    public class DropdownFilterVM
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }

    public class DropdownMenuFilterVM : DropdownFilterVM
    {
        public string Parameter { get; set; }
    }
    public class DropdownLookupFilterVM : DropdownFilterVM
    {
        public string Parameter { get; set; }
        public string Type { get; set; }
    }
}
