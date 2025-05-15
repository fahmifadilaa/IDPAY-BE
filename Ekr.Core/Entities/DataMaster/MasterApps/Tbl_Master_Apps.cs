using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ekr.Core.Entities.DataMaster.MasterApps
{
    public class Tbl_Master_Apps
    {
        [RegularExpression("^[0-9]*$", ErrorMessage = "Bad Request")]
        public int Id { get; set; }
        [RegularExpression("^[-a-zA-Z0-9:,/'._ ]*$", ErrorMessage = "Bad Request")]
        public string Nama { get; set; }
        public string Token { get; set; }
        //[RegularExpression("^[0-9,:-. ]*$", ErrorMessage = "Bad Request")]
        public DateTime? CreatedTime { get; set; }
        //[RegularExpression("^[0-9,:-. ]*$", ErrorMessage = "Bad Request")]
        public DateTime? UpdatedTime { get; set; }
        //[RegularExpression("^[0-9,:-. ]*$", ErrorMessage = "Bad Request")]
        public DateTime? DeletedTime { get; set; }
        [RegularExpression("^[-a-zA-Z0-9:,/'._ ]*$", ErrorMessage = "Bad Request")]
        public string CreatedByNpp { get; set; }
        [RegularExpression("^[-a-zA-Z0-9:,/'._ ]*$", ErrorMessage = "Bad Request")]
        public string UpdatedByNpp { get; set; }
        [RegularExpression("^[-a-zA-Z0-9:,/'._ ]*$", ErrorMessage = "Bad Request")]
        public string DeletedByNpp { get; set; }
        [RegularExpression("^[a-zA-Z0-9]*$", ErrorMessage = "Bad Request")]
        public bool? IsDeleted { get; set; }
        [RegularExpression("^[a-zA-Z0-9]*$", ErrorMessage = "Bad Request")]
        public bool? IsActive { get; set; }
    }
}
