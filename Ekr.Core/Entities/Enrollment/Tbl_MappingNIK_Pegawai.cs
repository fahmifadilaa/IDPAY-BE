using System;
using System.ComponentModel.DataAnnotations;

namespace Ekr.Core.Entities.Enrollment
{
    public class Tbl_MappingNIK_Pegawai
    {
        [RegularExpression("^[0-9]*$", ErrorMessage = "Bad Request")]
        public int Id { get; set; }
        [RegularExpression("^[a-zA-Z0-9/,'. ]*$", ErrorMessage = "Bad Request")]
        public string Npp { get; set; }
        [RegularExpression("^[a-zA-Z0-9/,'. ]*$", ErrorMessage = "Bad Request")]
        public string Nama { get; set; }
        [RegularExpression("^[0-9]*$", ErrorMessage = "Bad Request")]
        public string NIK { get; set; }
        //[RegularExpression("^[0-9,:-. ]*$", ErrorMessage = "Bad Request")]
        public DateTime? InsertedDate { get; set; }
	}

	public class Tbl_DataNIK_Pegawai
    {
		public int Id { get; set; }
		public string Npp { get; set; }
		public string Nama { get; set; }
		public string Nik { get; set; }
		public int CreatedById { get; set; }
		public DateTime? CreatedTime { get; set; }
	}
}
