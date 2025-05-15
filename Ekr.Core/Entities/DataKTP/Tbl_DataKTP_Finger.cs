using ServiceStack.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ekr.Core.Entities.DataKTP
{
    public class Tbl_DataKTP_Finger
    {
        [AutoIncrement]
        public int Id { get; set; }
        public string Nik { get; set; }
        public string TypeFinger { get; set; }
        public string PathFile { get; set; }
        public string PathFileISO { get; set; }
        public string FileName { get; set; }
        public string FileNameISO { get; set; }
        //public string FileJari { get; set; }
        public string FileJariISO { get; set; }
        public int? CreatedById { get; set; }
        public string CreatedByNpp { get; set; }
        public DateTime? CreatedTime { get; set; }
        public int? CreatedByUnitId { get; set; }
        public string CreatedByUnit { get; set; }
        public string CreatedByUid { get; set; }
        public int? UpdatedById { get; set; }
        public string UpdatedByNpp { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public string UpdatedByUnit { get; set; }
        public string UpdatedByUid { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsDeleted { get; set; }
    }

    public class Tbl_DataKTP_Finger_Employee
    {
        [AutoIncrement]
        public int Id { get; set; }
        public string Nik { get; set; }
        public string TypeFinger { get; set; }
        public string PathFile { get; set; }
        public string PathFileISO { get; set; }
        public string FileName { get; set; }
        public string FileNameISO { get; set; }
        //public string FileJari { get; set; }
        public string FileJariISO { get; set; }
        public int? CreatedById { get; set; }
        public string CreatedByNpp { get; set; }
        public DateTime? CreatedTime { get; set; }
        public int? CreatedByUnitId { get; set; }
        public string CreatedByUnit { get; set; }
        public string CreatedByUid { get; set; }
        public int? UpdatedById { get; set; }
        public string UpdatedByNpp { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public string UpdatedByUnit { get; set; }
        public string UpdatedByUid { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsDeleted { get; set; }
    }

    public class Tbl_DataKTP_Finger_Employee_Log
    {
        [AutoIncrement]
        public int Id { get; set; }
        public string Nik { get; set; }
        public string TypeFinger { get; set; }
        public string PathFile { get; set; }
        public string PathFileISO { get; set; }
        public string FileName { get; set; }
        public string FileNameISO { get; set; }
        //public string FileJari { get; set; }
        public string FileJariISO { get; set; }
        public int? CreatedById { get; set; }
        public string CreatedByNpp { get; set; }
        public DateTime? CreatedTime { get; set; }
        public int? CreatedByUnitId { get; set; }
        public string CreatedByUnit { get; set; }
        public string CreatedByUid { get; set; }
    }

    public class Tbl_DataKTP_Finger_Log
    {
        [AutoIncrement]
        public int Id { get; set; }
        public string Nik { get; set; }
        public string TypeFinger { get; set; }
        public string PathFile { get; set; }
        public string PathFileISO { get; set; }
        public string FileName { get; set; }
        public string FileNameISO { get; set; }
        //public string FileJari { get; set; }
        public string FileJariISO { get; set; }
        public int? CreatedById { get; set; }
        public string CreatedByNpp { get; set; }
        public DateTime? CreatedTime { get; set; }
        public int? CreatedByUnitId { get; set; }
        public string CreatedByUnit { get; set; }
        public string CreatedByUid { get; set; }
    }

    public class FingerISOVM
    {
        public string TypeFinger { get; set; }
        public string Base64Iso { get; set; }
    }

    public class Tbl_DataKTP_Finger_Temp
    {
        [AutoIncrement]
        [Column(TypeName = "bigint")]
        public int Id { get; set; }
        [Column(TypeName = "bigint")]
        public int DemografisTempId { get; set; }
        [Column(TypeName = "nvarchar(50)")]
        public string Nik { get; set; }
        [Column(TypeName = "nvarchar(100)")]
        public string TypeFinger { get; set; }
        [Column(TypeName = "nvarchar(750)")]
        public string PathFile { get; set; }
        [Column(TypeName = "nvarchar(750)")]
        public string PathFileISO { get; set; }
        [Column(TypeName = "nvarchar(500)")]
        public string FileName { get; set; }
        [Column(TypeName = "nvarchar(500)")]
        public string FileNameISO { get; set; }
        //public string FileJari { get; set; }
        [Column(TypeName = "text")]
        public string FileJariISO { get; set; }
        public int? CreatedById { get; set; }
        [Column(TypeName = "nvarchar(50)")]
        public string CreatedByNpp { get; set; }
        public DateTime? CreatedTime { get; set; }
        public int? CreatedByUnitId { get; set; }
        [Column(TypeName = "nvarchar(80)")]
        public string CreatedByUnit { get; set; }
        [Column(TypeName = "nvarchar(250)")]
        public string CreatedByUID { get; set; }
        public int? UpdatedById { get; set; }
        [Column(TypeName = "nvarchar(50)")]
        public string UpdatedByNpp { get; set; }
        public DateTime? UpdatedTime { get; set; }
        [Column(TypeName = "nvarchar(80)")]
        public string UpdatedByUnit { get; set; }
        [Column(TypeName = "nvarchar(250)")]
        public string UpdatedByUID { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public bool? IsApprove { get; set; }
        [Column(TypeName = "nvarchar(100)")]
        public string NoPengajuan { get; set; }
    }

    public class Tbl_DataKTP_Finger_Employee_Temp
    {
        [AutoIncrement]
        [Column(TypeName = "bigint")]
        public int Id { get; set; }
        [Column(TypeName = "bigint")]
        public int DemografisTempId { get; set; }
        [Column(TypeName = "nvarchar(50)")]
        public string Nik { get; set; }
        [Column(TypeName = "nvarchar(100)")]
        public string TypeFinger { get; set; }
        [Column(TypeName = "nvarchar(750)")]
        public string PathFile { get; set; }
        [Column(TypeName = "nvarchar(750)")]
        public string PathFileISO { get; set; }
        [Column(TypeName = "nvarchar(500)")]
        public string FileName { get; set; }
        [Column(TypeName = "nvarchar(500)")]
        public string FileNameISO { get; set; }
        //public string FileJari { get; set; }
        [Column(TypeName = "text")]
        public string FileJariISO { get; set; }
        public int? CreatedById { get; set; }
        [Column(TypeName = "nvarchar(50)")]
        public string CreatedByNpp { get; set; }
        public DateTime? CreatedTime { get; set; }
        public int? CreatedByUnitId { get; set; }
        [Column(TypeName = "nvarchar(80)")]
        public string CreatedByUnit { get; set; }
        [Column(TypeName = "nvarchar(250)")]
        public string CreatedByUID { get; set; }
        public int? UpdatedById { get; set; }
        [Column(TypeName = "nvarchar(50)")]
        public string UpdatedByNpp { get; set; }
        public DateTime? UpdatedTime { get; set; }
        [Column(TypeName = "nvarchar(80)")]
        public string UpdatedByUnit { get; set; }
        [Column(TypeName = "nvarchar(250)")]
        public string UpdatedByUID { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public bool? IsApprove { get; set; }
        [Column(TypeName = "nvarchar(100)")]
        public string NoPengajuan { get; set; }
    }
}
