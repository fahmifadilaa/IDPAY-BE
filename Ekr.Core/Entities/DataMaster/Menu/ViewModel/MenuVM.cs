
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ekr.Core.Entities.DataMaster.Menu.ViewModel
{
    public class MenuVM
    {
        public Int64 Number { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public int Type { get; set; }
        public string Type_Menu_Name { get; set; }
        public string Route { get; set; }
        public int? Order { get; set; }
        public int Visible { get; set; }
        public string Visible_Name { get; set; }
        public int? ParentNavigation_Id { get; set; }
        public string Parent_Menu { get; set; }
        public string Created_Time { get; set; }
        public string Created_By { get; set; }
        public string Updated_Time { get; set; }
        public string Updated_By { get; set; }
        public string Deleted_By { get; set; }
        public string Deleted_Time { get; set; }
        public string Roles { get; set; }
    }

    public class ManageMenuVM
    {
        public Int64 Number { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public int Type_Menu { get; set; }
        public string Type_Menu_Name { get; set; }
        public string Parent_Id { get; set; }
        public string Parent_Menu { get; set; }
        public string Route { get; set; }
        public bool IsVisible { get; set; }
        public string Visible_Name { get; set; }
        public int Order { get; set; }
        public string Roles { get; set; }
        public string Created_Time { get; set; }
        public string Created_By { get; set; }
        public string Updated_Time { get; set; }
        public string Updated_By { get; set; }
        public string Deleted_By { get; set; }
        public string Deleted_Time { get; set; }
    }
}
