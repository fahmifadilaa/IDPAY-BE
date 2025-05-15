using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ekr.Core.Entities.DataMaster.Unit
{
    public class UnitVM
    {
        public int Id { get; set; }
        public Int64 Number { get; set; }
        public int? Parent_Id { get; set; }
        public string Parent_Name { get; set; }
        public string Short_Name { get; set; }
        public int? Type_Unit_Id { get; set; }
        public string Type_Unit_Name { get; set; }
        public string Kode_Unit { get; set; }
        public string Nama_Unit { get; set; }
        public string Alamat { get; set; }
        public string No_Telepon { get; set; }
        public string No_Fax { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; }
        public string Status { get; set; }
        public string Created_Time { get; set; }
        public string Created_By { get; set; }
        public string Updated_Time { get; set; }
        public string Updated_By { get; set; }
        public string Deleted_By { get; set; }
        public string Deleted_Time { get; set; }
        public UnitVM()
        {
            //baru
            IsActive = true;
        }
    }

    public partial class TblUnit
    {
        public TblUnit()
        {
            InverseParent = new HashSet<TblUnit>();
        }

        public int Id { get; set; }
        public int? Parent_Id { get; set; }
        public int Type { get; set; }
        public string KodeWilayah { get; set; }
        public string Code { get; set; }
        public string FullCode { get; set; }
        public string ShortName { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }
        public string Telepon { get; set; }
        public string StatusOutlet { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? CreatedTime { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public int? CreatedBy_Id { get; set; }
        public int? UpdatedBy_Id { get; set; }
        public DateTime? DeletedTime { get; set; }
        public int? DeletedBy_Id { get; set; }
        public bool? IsDelete { get; set; }

        public virtual TblUnit Parent { get; set; }
        public virtual ICollection<TblUnit> InverseParent { get; set; }
    }

    public partial class TblUnitVM
    {
        public TblUnitVM()
        {
            InverseParent = new HashSet<TblUnitVM>();
        }
        [RegularExpression("^[0-9]*$", ErrorMessage = "Bad Request")]
        public int? Id { get; set; }
        [RegularExpression("^[0-9]*$", ErrorMessage = "Bad Request")]
        public int? ParentId { get; set; }
        [RegularExpression("^[0-9]*$", ErrorMessage = "Bad Request")]
        public int Type { get; set; }
        [RegularExpression("^[a-zA-Z0-9]*$", ErrorMessage = "Bad Request")]
        public string KodeWilayah { get; set; }
        [RegularExpression("^[a-zA-Z0-9]*$", ErrorMessage = "Bad Request")]
        public string Code { get; set; }
        [RegularExpression("^[a-zA-Z0-9]*$", ErrorMessage = "Bad Request")]
        public string FullCode { get; set; }
        [RegularExpression("^[a-zA-Z0-9/,'. ]*$", ErrorMessage = "Bad Request")]
        public string ShortName { get; set; }
        [RegularExpression("^[a-zA-Z0-9/,'. ]*$", ErrorMessage = "Bad Request")]
        public string Name { get; set; }
        [RegularExpression("^[-a-zA-Z0-9/,'. ]*$", ErrorMessage = "Bad Request")]
        public string Address { get; set; }
        [RegularExpression("^[a-zA-Z0-9.@_]*$", ErrorMessage = "Bad Request")]
        public string Email { get; set; }
        [RegularExpression("^[0-9+-]*$", ErrorMessage = "Bad Request")]
        public string Telepon { get; set; }
        [RegularExpression("^[a-zA-Z0-9/,'. ]*$", ErrorMessage = "Bad Request")]
        public string StatusOutlet { get; set; }
        [RegularExpression("^[-0-9.,:/' ]*$", ErrorMessage = "Bad Request")]
        public string Latitude { get; set; }
        [RegularExpression("^[-0-9.,:/' ]*$", ErrorMessage = "Bad Request")]
        public string Longitude { get; set; }
        [RegularExpression("^[a-zA-Z0-9]*$", ErrorMessage = "Bad Request")]
        public bool? IsActive { get; set; }
        //[RegularExpression("^[-0-9,:-. ]*$", ErrorMessage = "Bad Request")]
        public DateTime? CreatedTime { get; set; }
        //[RegularExpression("^[-0-9,:-. ]*$", ErrorMessage = "Bad Request")]
        public DateTime? UpdatedTime { get; set; }
        [RegularExpression("^[0-9]*$", ErrorMessage = "Bad Request")]
        public int? CreatedById { get; set; }
        [RegularExpression("^[0-9]*$", ErrorMessage = "Bad Request")]
        public int? UpdatedById { get; set; }
        //[RegularExpression("^[-0-9,:. ]*$", ErrorMessage = "Bad Request")]
        public DateTime? DeletedTime { get; set; }
        [RegularExpression("^[0-9]*$", ErrorMessage = "Bad Request")]
        public int? DeletedById { get; set; }
        [RegularExpression("^[a-zA-Z0-9]*$", ErrorMessage = "Bad Request")]
        public bool? IsDelete { get; set; }

        public virtual TblUnitVM Parent { get; set; }
        public virtual ICollection<TblUnitVM> InverseParent { get; set; }
    }
}
