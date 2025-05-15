using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ekr.Core.Entities.MappingNIKPegawai
{
    public class Tbl_MappingNIK_Pegawai
    {
        [AutoIncrement]
        public int Id { get; set; }
        [RegularExpression("^[a-zA-Z0-9/,'. ]*$", ErrorMessage = "Bad Request")]
        public string Npp { get; set; }
        [RegularExpression("^[a-zA-Z0-9/,'. ]*$", ErrorMessage = "Bad Request")]
        public string Nama { get; set; }
        [RegularExpression("^[0-9]*$", ErrorMessage = "Bad Request")]
        public string NIK { get; set; }
        public virtual DateTime? InsertedDate { get; set; }
        public virtual DateTime? UpdatedDate { get; set; }
        [RegularExpression("^[0-9]*$", ErrorMessage = "Bad Request")]
        public int InsertById { get; set; }
        [RegularExpression("^[0-9]*$", ErrorMessage = "Bad Request")]
        public int UpdateById { get; set; }
    }

    public class Tbl_MappingNIK_Pegawai_log
    {
        [AutoIncrement]
        public int Id { get; set; }
        public string Npp { get; set; }
        public string Nama { get; set; }
        public string Keterangan { get; set; }
        public string NIK { get; set; }
        public virtual DateTime? CreatedDate { get; set; }
        public int CreateById { get; set; }
    }

    public class MappingNIKVM
    {
        public Int64 Number { get; set; }
        public int Id { get; set; }
        public string Npp { get; set; }
        public string Nama { get; set; }
        public string NIK { get; set; }
        public virtual DateTime? InsertedDate { get; set; }
        public string InsertedDateString { get; set; }
    }

    public class requestExist
    {
        public string Npp { get; set; }
        public string NIK { get; set; }
        public int Id { get; set; }
    }

    public class mappingGrid
    {
        public string filterNIK { get; set; }
        public string filterNPP { get; set; }
        public string SortColumn { get; set; }
        public string SortColumnDir { get; set; }
        public int PageSize { get; set; }
        public int PageNumber { get; set; }
    }
}
