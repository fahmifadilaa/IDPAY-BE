using System;
using System.ComponentModel.DataAnnotations;

namespace Ekr.Core.Entities.DataMaster.Lookup
{
    public class LookupVM
    {
        public Int64 Number { get; set; }
        public int Id { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public int Value { get; set; }
        public int Order_By { get; set; }
        public string Status { get; set; }
        public bool IsActive { get; set; }
        public string Created_Time { get; set; }
        public string Created_By { get; set; }
        public string Updated_Time { get; set; }
        public string Updated_By { get; set; }
        public string Deleted_By { get; set; }
        public string Deleted_Time { get; set; }
        public LookupVM()
        {
            //baru
            IsActive = true;
        }
    }

    public class TblLookup
    {
        [RegularExpression("^[0-9]*$", ErrorMessage = "Bad Request")]
        public int? Id { get; set; }
        [RegularExpression("^[0-9]*$", ErrorMessage = "Bad Request")]
        public int? Aplikasi_Id { get; set; }
        [RegularExpression("^[-a-zA-Z0-9/ ,'.]*$", ErrorMessage = "Bad Request")]
        public string Type { get; set; }
        [RegularExpression("^[-a-zA-Z0-9/ ,'.]*$", ErrorMessage = "Bad Request")]
        public string Name { get; set; }
        [RegularExpression("^[0-9]*$", ErrorMessage = "Bad Request")]
        public int? Value { get; set; }
        [RegularExpression("^[0-9]*$", ErrorMessage = "Bad Request")]
        public int? Order_By { get; set; }
        //[RegularExpression("^[0-9,:-. ]*$", ErrorMessage = "Bad Request")]
        public DateTime? CreatedTime { get; set; }
        //[RegularExpression("^[0-9,:-. ]*$", ErrorMessage = "Bad Request")]
        public DateTime? UpdatedTime { get; set; }
        //[RegularExpression("^[0-9,:-. ]*$", ErrorMessage = "Bad Request")]
        public DateTime? DeletedTime { get; set; }
        [RegularExpression("^[0-9]*$", ErrorMessage = "Bad Request")]
        public int? CreatedBy_Id { get; set; }
        [RegularExpression("^[0-9]*$", ErrorMessage = "Bad Request")]
        public int? UpdatedBy_Id { get; set; }
        [RegularExpression("^[0-9]*$", ErrorMessage = "Bad Request")]
        public int? DeletedBy_Id { get; set; }
        [RegularExpression("^[a-zA-Z0-9]*$", ErrorMessage = "Bad Request")]
        public bool? IsDeleted { get; set; }
        [RegularExpression("^[a-zA-Z0-9]*$", ErrorMessage = "Bad Request")]
        public bool? IsActive { get; set; }
    }
}
