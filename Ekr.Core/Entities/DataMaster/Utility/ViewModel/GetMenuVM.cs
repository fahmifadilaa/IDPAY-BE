using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ekr.Core.Entities.DataMaster.Utility.ViewModel
{
    public class GetMenuVM
    {
        public int Id { get; set; }
        public int? Type { get; set; }
        public string Name { get; set; }
        public string Route { get; set; }
        public int? Order { get; set; }
        public int? Visible { get; set; }
        public int? ParentNavigationId { get; set; }
        public string IconClass { get; set; }
        public int? Jumlah { get; set; }
    }
}
