using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ekr.Core.Entities.DataMaster.Menu.ViewModel
{
    public class SettingMenuVM
    {
        public int Id { get; set; }
        public int Type { get; set; }
        public string Name { get; set; }
        public string Route { get; set; }
        public int? Order { get; set; }
        public int Visible { get; set; }
        public int? ParentNavigation_Id { get; set; }
        public DateTime? CreatedTime { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public int? CreatedBy_Id { get; set; }
        public int? UpdatedBy_Id { get; set; }
        public string IconClass { get; set; }
        public bool? IsDeleted { get; set; }
        public int? DeletedById { get; set; }
        public DateTime? DeletedTime { get; set; }
        public string Roles { get; set; }
        public string AppsVal { get; set; }
    }
    public class SettingMenuReqVM
    {
        [RegularExpression("^[0-9]*$", ErrorMessage = "Bad Request")]
        public int? Id { get; set; }
        [RegularExpression("^[0-9]*$", ErrorMessage = "Bad Request")]
        public int? Type { get; set; }
        [RegularExpression("^[-a-zA-Z0-9,/'._ ]*$", ErrorMessage = "Bad Request")]
        public string Name { get; set; }
        [RegularExpression("^[-a-zA-Z0-9./_:]*$", ErrorMessage = "Bad Request")]
        public string Route { get; set; }
        [RegularExpression("^[0-9]*$", ErrorMessage = "Bad Request")]
        public int? Order { get; set; }
        [RegularExpression("^[0-9]*$", ErrorMessage = "Bad Request")]
        public int? Visible { get; set; }
        [RegularExpression("^[0-9]*$", ErrorMessage = "Bad Request")]
        public int? ParentNavigation_Id { get; set; }
        [RegularExpression("^[-a-zA-Z./' ]*$", ErrorMessage = "Bad Request")]
        public string? IconClass { get; set; }
        [RegularExpression("^[a-zA-Z0-9]*$", ErrorMessage = "Bad Request")]
        public bool? IsDeleted { get; set; }
        [RegularExpression("^[0-9,]*$", ErrorMessage = "Bad Request")]
        public string? Roles { get; set; }
        [RegularExpression("^[0-9,]*$", ErrorMessage = "Bad Request")]
        public string? AppsVal { get; set; }
        [RegularExpression("^[0-9]*$", ErrorMessage = "Bad Request")]
        public int? CreatedBy_Id { get; set; }
        [RegularExpression("^[0-9]*$", ErrorMessage = "Bad Request")]
        public int? UpdatedBy_Id { get; set; }
    }
}
