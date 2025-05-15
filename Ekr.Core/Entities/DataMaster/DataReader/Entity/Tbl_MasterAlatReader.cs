using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Ekr.Core.Entities.DataMaster.DataReader.Entity
{
    public class Tbl_MasterAlatReader
    {
        [AutoIncrement]
        [RegularExpression("^[0-9]*$", ErrorMessage = "Bad Request")]
        public int Id { get; set; }
        [RegularExpression("^[0-9]*$", ErrorMessage = "Bad Request")]
        public int? Unit_Id { get; set; }
        [RegularExpression("^[a-zA-Z0-9]*$", ErrorMessage = "Bad Request")]
        public string Kode { get; set; }
        [RegularExpression("^[a-zA-Z0-9]*$", ErrorMessage = "Bad Request")]
        public string Nama { get; set; }
        [RegularExpression("^[a-zA-Z0-9]*$", ErrorMessage = "Bad Request")]
        public string SN_Unit { get; set; }
        [RegularExpression("^[a-zA-Z0-9]*$", ErrorMessage = "Bad Request")]
        public string No_Perso_SAM { get; set; }
        [RegularExpression("^[a-zA-Z0-9]*$", ErrorMessage = "Bad Request")]
        public string No_Kartu { get; set; }
        [RegularExpression("^[a-zA-Z0-9]*$", ErrorMessage = "Bad Request")]
        public string PCID { get; set; }
        [RegularExpression("^[a-zA-Z0-9]*$", ErrorMessage = "Bad Request")]
        public string Confiq { get; set; }
        [RegularExpression("^[a-zA-Z0-9]*$", ErrorMessage = "Bad Request")]
        public string UID { get; set; }
        [RegularExpression("^[a-zA-Z0-9]*$", ErrorMessage = "Bad Request")]
        public string Status { get; set; }
        [RegularExpression("^[a-zA-Z0-9/,'.: ]*$", ErrorMessage = "Bad Request")]
        //[RegularExpression("^[a-zA-Z0-9,./-: ]*$", ErrorMessage = "Bad Request")]
        public string LastIP { get; set; }
        //[RegularExpression("^[0-9,:-. ]*$", ErrorMessage = "Bad Request")]
        public DateTime? LastPingIP { get; set; }
        //[RegularExpression("^[0-9,:-. ]*$", ErrorMessage = "Bad Request")]
        public DateTime? LastActive { get; set; }
        //[RegularExpression("^[a-zA-Z0-9,./-: ]*$", ErrorMessage = "Bad Request")]
        public string Latitude { get; set; }
        //[RegularExpression("^[a-zA-Z0-9,./-: ]*$", ErrorMessage = "Bad Request")]
        public string Longitude { get; set; }
        [RegularExpression("^[0-9]*$", ErrorMessage = "Bad Request")]
        public int? LastUserId { get; set; }
        [RegularExpression("^[0-9]*$", ErrorMessage = "Bad Request")]
        public int? LastPegawaiId { get; set; }
        [RegularExpression("^[0-9]*$", ErrorMessage = "Bad Request")]
        public int? LastUnitId { get; set; }
        //[RegularExpression("^[0-9,:-. ]*$", ErrorMessage = "Bad Request")]
        public DateTime? LastUsed { get; set; }
        [RegularExpression("^[a-zA-Z0-9]*$", ErrorMessage = "Bad Request")]
        public bool? IsActive { get; set; }
        [RegularExpression("^[a-zA-Z0-9]*$", ErrorMessage = "Bad Request")]
        public bool? IsDeleted { get; set; }
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

    }

    public class Tbl_MasterAlatReaderLog
    {
        [AutoIncrement]
        public int Id { get; set; }
        public string Uid { get; set; }
        public string Serial_Number { get; set; }
        public string Type { get; set; }
        public string Nik { get; set; }
        public int? PegawaiId { get; set; }
        public int? UnitId { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public DateTime? CreatedTime { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public DateTime? DeletedTime { get; set; }
        public int? CreatedBy_Id { get; set; }
        public int? UpdatedBy_Id { get; set; }
        public int? DeletedBy_Id { get; set; }
    }
}
