using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ekr.Core.Entities.DataMaster.MasterTreshold
{
    public class TresholdVM
    {
        public Int64 Number { get; set; }
        public int? Id { get; set; }
        public int? TipeId { get; set; }
        public string TipeName { get; set; }
        public decimal Value { get; set; }
        public string Status { get; set; }
        public bool IsActive { get; set; }
        public bool IsDelete { get; set; }
        public string Created_Time { get; set; }
        public string Created_By { get; set; }
        public string Updated_Time { get; set; }
        public string Updated_By { get; set; }
        public string Deleted_By { get; set; }
        public string Deleted_Time { get; set; }
        public TresholdVM()
        {
            //baru
            IsActive = true;
        }
    }

    public class TblMasterTreshold
    {
        [RegularExpression("^[0-9]*$", ErrorMessage = "Bad Request")]
        public int? id { get; set; }
        [RegularExpression("^[0-9]*$", ErrorMessage = "Bad Request")]
        public int? tipeid { get; set; }
        [RegularExpression("^[0-9.,]*$", ErrorMessage = "Bad Request")]
        public decimal value { get; set; }
        //[RegularExpression("^[0-9,:-. ]*$", ErrorMessage = "Bad Request")]
        public DateTime? createdTime { get; set; }
        //[RegularExpression("^[0-9,:-. ]*$", ErrorMessage = "Bad Request")]
        public DateTime? updatedTime { get; set; }
        //[RegularExpression("^[0-9,:-. ]*$", ErrorMessage = "Bad Request")]
        public DateTime? deletedTime { get; set; }
        [RegularExpression("^[0-9]*$", ErrorMessage = "Bad Request")]
        public int? createdById { get; set; }
        [RegularExpression("^[0-9]*$", ErrorMessage = "Bad Request")]
        public int? updatedById { get; set; }
        [RegularExpression("^[0-9]*$", ErrorMessage = "Bad Request")]
        public int? deletedById { get; set; }
        [RegularExpression("^[a-zA-Z0-9]*$", ErrorMessage = "Bad Request")]
        public bool? isDelete { get; set; }
        [RegularExpression("^[a-zA-Z0-9]*$", ErrorMessage = "Bad Request")]
        public bool? isActive { get; set; }
    }
}
