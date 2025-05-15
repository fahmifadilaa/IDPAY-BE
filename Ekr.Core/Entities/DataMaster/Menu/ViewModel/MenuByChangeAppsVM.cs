using Ekr.Core.Entities.DataMaster.MasterAplikasi;
using Ekr.Core.Entities.DataMaster.Utility.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ekr.Core.Entities.DataMaster.Menu.ViewModel
{
    public class MenuByChangeAppsVM
    {
        public TblMasterAplikasi MasterApps { get; set; }
        public List<GetMenuVM> ListMenu { get; set; }

    }
    public class MenuByAppsVM
    {
        public int Id { get; set; }
        public int? Type { get; set; }
        public string Name { get; set; }
        public string Route { get; set; }
        public int? Jumlah { get; set; }
        public int? Order { get; set; }
        public int? Visible { get; set; }
        public int? ParentNavigationId { get; set; }
        public DateTime? CreatedTime { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public int? CreatedById { get; set; }
        public int? UpdatedById { get; set; }
        public string IconClass { get; set; }
        public bool? IsDeleted { get; set; }
        public int? DeletedById { get; set; }
        public DateTime? DeletedTime { get; set; }
        public int? Expanded { get; set; }
        public int? Activated { get; set; }
    }
}
