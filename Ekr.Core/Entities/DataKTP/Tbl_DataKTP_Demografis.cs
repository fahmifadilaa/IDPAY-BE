using ServiceStack.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Ekr.Core.Entities.DataKTP
{
	public class Tbl_DataKTP_Demografis
	{
		[AutoIncrement]
        [Column(TypeName = "bigint")]
        public long Id { get; set; }
        [Column(TypeName = "nvarchar(250)")]
        public string NIK { get; set; }
        [Column(TypeName = "nvarchar(350)")]
        public string Nama { get; set; }
        [Column(TypeName = "nvarchar(250)")]
        public string TempatLahir { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime? TanggalLahir { get; set; }
        [Column(TypeName = "nvarchar(50)")]
        public string JenisKelamin { get; set; }
        [Column(TypeName = "nvarchar(50)")]
        public string GolonganDarah { get; set; }
        [Column(TypeName = "nvarchar(500)")]
        public string Alamat { get; set; }
        [Column(TypeName = "nvarchar(50)")]
        public string RT { get; set; }
        [Column(TypeName = "nvarchar(50)")]
        public string RW { get; set; }
        [Column(TypeName = "nvarchar(150)")]
        public string Kelurahan { get; set; }
        [Column(TypeName = "nvarchar(150)")]
        public string Desa { get; set; }
        [Column(TypeName = "nvarchar(350)")]
        public string Kecamatan { get; set; }
        [Column(TypeName = "nvarchar(150)")]
        public string Kota { get; set; }
        [Column(TypeName = "nvarchar(150)")]
        public string Provinsi { get; set; }
        [Column(TypeName = "nvarchar(150)")]
        public string Agama { get; set; }
        [Column(TypeName = "nvarchar(50)")]
        public string KodePos { get; set; }
        [Column(TypeName = "nvarchar(50)")]
        public string StatusPerkawinan { get; set; }
        [Column(TypeName = "nvarchar(250)")]
        public string Pekerjaan { get; set; }
        [Column(TypeName = "nvarchar(50)")]
        public string Kewarganegaraan { get; set; }
        [Column(TypeName = "nvarchar(150)")]
        public string MasaBerlaku { get; set; }
        [Column(TypeName = "nvarchar(250)")]
        [AllowNull]
        public string Latitude { get; set; }
        [Column(TypeName = "nvarchar(250)")]
        [AllowNull]
        public string Longitude { get; set; }
        [Column(TypeName = "nvarchar(500)")]
        [AllowNull]
        public string AlamatLengkap { get; set; }
        [Column(TypeName = "nvarchar(500)")]
        [AllowNull]
        public string AlamatGoogle { get; set; }
		public int? CreatedById { get; set; }
        [Column(TypeName = "nvarchar(50)")]
        public string CreatedByNpp { get; set; }
		public DateTime? CreatedTime { get; set; }
		public int? CreatedByUnitId { get; set; }
        [Column(TypeName = "nvarchar(80)")]
        public string CreatedByUnitCode { get; set; }
        [Column(TypeName = "nvarchar(250)")]
        public string CreatedByUID { get; set; }
		public int? UpdatedById { get; set; }
        [Column(TypeName = "nvarchar(50)")]
        public string UpdatedByNpp { get; set; }
		public DateTime? UpdatedTime { get; set; }
		public int? UpdatedByUnitId { get; set; }
        [Column(TypeName = "nvarchar(80)")]
        public string UpdatedByUnitCode { get; set; }
        [Column(TypeName = "nvarchar(250)")]
        public string UpdatedByUID { get; set; }
		public bool? IsActive { get; set; }
		public bool? IsDeleted { get; set; }
        [Column(TypeName = "nvarchar(70)")]
        public string CIF { get; set; }
		public bool? IsVerified { get; set; }
		public bool? IsNasabahTemp { get; set; }
		public DateTime? UpdatedCIFByBS_Time { get; set; }
        [Column(TypeName = "nvarchar(150)")]
        public string UpdatedCIFByBS_Username { get; set; }
        [Column(TypeName = "nvarchar(150)")]
        public string VerifiedByNpp { get; set; }
        [Column(TypeName = "nvarchar(MAX)")]
        public string VerifyComment { get; set; }
        [Column(TypeName = "nvarchar(70)")]
        public string CIFDash { get; set; }
		public bool isMigrate { get; set; }
		public bool isEnrollFR { get; set; }
	}

	public class Tbl_DataKTP_Demografis_Log
	{
		[AutoIncrement]
		public int Id { get; set; }
		public string NIK { get; set; }
		public string Nama { get; set; }
		public string TempatLahir { get; set; }
		public DateTime? TanggalLahir { get; set; }
		public string JenisKelamin { get; set; }
		public string GolonganDarah { get; set; }
		public string Alamat { get; set; }
		public string RT { get; set; }
		public string RW { get; set; }
		public string Kelurahan { get; set; }
		public string Desa { get; set; }
		public string Kecamatan { get; set; }
		public string Kota { get; set; }
		public string Provinsi { get; set; }
		public string Agama { get; set; }
		public string KodePos { get; set; }
		public string StatusPerkawinan { get; set; }
		public string Pekerjaan { get; set; }
		public string Kewarganegaraan { get; set; }
		public string MasaBerlaku { get; set; }
		public string Latitude { get; set; }
		public string Longitude { get; set; }
		public string AlamatLengkap { get; set; }
		public string AlamatGoogle { get; set; }
		public string CreatedByNpp { get; set; }
		public int? CreatedById { get; set; }
		public DateTime? CreatedTime { get; set; }
		public int? CreatedByUnitId { get; set; }
		public string CreatedByUID { get; set; }
		public string CIF { get; set; }
	}
    public class Tbl_DataKTP_Demografis_Temp
    {
        [AutoIncrement]
        [Column(TypeName = "bigint")]
        public long Id { get; set; }
        [Column(TypeName = "nvarchar(250)")]
        public string NIK { get; set; }
        [Column(TypeName = "nvarchar(350)")]
        public string Nama { get; set; }
        [Column(TypeName = "nvarchar(250)")]
        public string TempatLahir { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime? TanggalLahir { get; set; }
        [Column(TypeName = "nvarchar(50)")]
        public string JenisKelamin { get; set; }
        [Column(TypeName = "nvarchar(50)")]
        public string GolonganDarah { get; set; }
        [Column(TypeName = "nvarchar(500)")]
        public string Alamat { get; set; }
        [Column(TypeName = "nvarchar(50)")]
        public string RT { get; set; }
        [Column(TypeName = "nvarchar(50)")]
        public string RW { get; set; }
        [Column(TypeName = "nvarchar(150)")]
        public string Kelurahan { get; set; }
        [Column(TypeName = "nvarchar(150)")]
        public string Desa { get; set; }
        [Column(TypeName = "nvarchar(350)")]
        public string Kecamatan { get; set; }
        [Column(TypeName = "nvarchar(150)")]
        public string Kota { get; set; }
        [Column(TypeName = "nvarchar(150)")]
        public string Provinsi { get; set; }
        [Column(TypeName = "nvarchar(150)")]
        public string Agama { get; set; }
        [Column(TypeName = "nvarchar(50)")]
        public string KodePos { get; set; }
        [Column(TypeName = "nvarchar(50)")]
        public string StatusPerkawinan { get; set; }
        [Column(TypeName = "nvarchar(250)")]
        public string Pekerjaan { get; set; }
        [Column(TypeName = "nvarchar(50)")]
        public string Kewarganegaraan { get; set; }
        [Column(TypeName = "nvarchar(150)")]
        public string MasaBerlaku { get; set; }
        [Column(TypeName = "nvarchar(250)")]
        public string Latitude { get; set; }
        [Column(TypeName = "nvarchar(250)")]
        public string Longitude { get; set; }
        [Column(TypeName = "nvarchar(500)")]
        public string AlamatLengkap { get; set; }
        [Column(TypeName = "nvarchar(500)")]
        public string AlamatGoogle { get; set; }
        public int? CreatedById { get; set; }
        [Column(TypeName = "nvarchar(50)")]
        public string CreatedByNpp { get; set; }
        public DateTime? CreatedTime { get; set; }
        public int? CreatedByUnitId { get; set; }
        [Column(TypeName = "nvarchar(80)")]
        public string CreatedByUnitCode { get; set; }
        [Column(TypeName = "nvarchar(250)")]
        public string CreatedByUID { get; set; }
        public int? UpdatedById { get; set; }
        [Column(TypeName = "nvarchar(50)")]
        public string UpdatedByNpp { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public int? UpdatedByUnitId { get; set; }
        [Column(TypeName = "nvarchar(80)")]
        public string UpdatedByUnitCode { get; set; }
        [Column(TypeName = "nvarchar(250)")]
        public string UpdatedByUID { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        [Column(TypeName = "nvarchar(70)")]
        public string CIF { get; set; }
        public bool? IsVerified { get; set; }
        public bool? IsNasabahTemp { get; set; }
        public DateTime? UpdatedCIFByBS_Time { get; set; }
        [Column(TypeName = "nvarchar(150)")]
        public string UpdatedCIFByBS_Username { get; set; }
        [Column(TypeName = "nvarchar(150)")]
        public string VerifiedByNpp { get; set; }
        [Column(TypeName = "nvarchar(MAX)")]
        public string VerifyComment { get; set; }
        [Column(TypeName = "nvarchar(70)")]
        public string CIFDash { get; set; }
        public bool isMigrate { get; set; }
        public bool IsApprove { get; set; }
        [Column(TypeName = "nvarchar(100)")]
        public string NoPengajuan { get; set; }


    }

    public class Tbl_Inbox_Enrollment_Temp
    {
        [AutoIncrement]
        [Column(TypeName = "bigint")]
        public long Id { get; set; }
        [Column(TypeName = "bigint")]
        public long DemografisTempId { get; set; }
        public int? CreatedById { get; set; }
        [Column(TypeName = "nvarchar(50)")]
        public string CreatedByNpp { get; set; }
        public DateTime? CreatedTime { get; set; }
        public int? CreatedByUnitId { get; set; }
        [Column(TypeName = "nvarchar(80)")]
        public string CreatedByUnitCode { get; set; }
        public int? UpdatedById { get; set; }
        [Column(TypeName = "nvarchar(50)")]
        public string UpdatedByNpp { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public int? UpdatedByUnitId { get; set; }
        [Column(TypeName = "nvarchar(80)")]
        public string UpdatedByUnitCode { get; set; }
        public int? ApprovedByRoleId { get; set; }
        public int? ApprovedByUnitId { get; set; }
        public int? ApprovedStatus { get; set; }
        public int? Status { get; set; }
        public int? ApprovedByEmployeeId { get; set; }
        public int? ApprovedByEmployeeId2 { get; set; }
        [Column(TypeName = "nvarchar(100)")]
        public string NoPengajuan { get; set; }
    }

    public class Tbl_Inbox_Enrollment_Temp_Detail
    {
        [AutoIncrement]
        [Column(TypeName = "bigint")]
        public long Id { get; set; }
        [Column(TypeName = "bigint")]
        public long InboxEnrollmentTempId { get; set; }
        [Column(TypeName = "text")]
        public string Notes { get; set; }
        public DateTime? CreatedTime { get; set; }
        public int? SubmitedByUnitId { get; set; }
        public int? SubmitById { get; set; }
        [Column(TypeName = "nvarchar(80)")]
        public string SubmitedByUnitCode { get; set; }
        [Column(TypeName = "nvarchar(50)")]
        public string SubmitByNpp { get; set; }
        public int? Status { get; set; }
        public int? ApprovedStatus { get; set; }
        [Column(TypeName = "nvarchar(100)")]
        public string NoPengajuan { get; set; }
    }
}
