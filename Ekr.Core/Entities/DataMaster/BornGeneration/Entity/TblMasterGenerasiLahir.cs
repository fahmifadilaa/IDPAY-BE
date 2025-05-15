using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ekr.Core.Entities.DataMaster.BornGeneration.Entity
{
    public class TblMasterGenerasiLahir
    {
        [RegularExpression("^[0-9]*$", ErrorMessage = "Bad Request")]
        public int Id { get; set; }
        [RegularExpression("^[a-zA-Z0-9/,'. ]*$", ErrorMessage = "Bad Request")]
        public string Nama { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Only positive number allowed")]
        public int? TahunLahirAwal { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "Only positive number allowed")]
        public int? TahunLahirAkhir { get; set; }
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
