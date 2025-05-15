using Ekr.Core.Entities.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ekr.Core.Entities.DataMaster.Menu.ViewModel
{
    public class MenuFilterVM : BaseSqlGridFilter
    {
        public string Type { get; set; }
        public string Name { get; set; }
    }
    public class ManageMenuFilterVM : BaseSqlGridFilter
    {
        public string Type { get; set; }
        public string Name { get; set; }
    }
}
