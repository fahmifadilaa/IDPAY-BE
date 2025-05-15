using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Ekr.Core.Entities.Enrollment
{
    public class EnrollKTP
    {
        [RegularExpression("^[0-9]*$", ErrorMessage = "Bad Request")]
        public string KtpNIK { get; set; }
        //[RegularExpression("^[A-Za-z-\\'., ]*$", ErrorMessage = "Bad Request")]
        public string KtpNama { get; set; }
        //[RegularExpression("^[0-9-/A-Za-z,. ]*$", ErrorMessage = "Bad Request")]
        public string KtpTTL { get; set; }
        //[RegularExpression("^[a-zA-Z.,-/' ]*$", ErrorMessage = "Bad Request")]
        public string KtpTempatLahir { get; set; }
        //[RegularExpression("^[0-9-/,]*$", ErrorMessage = "Bad Request")]
        public string KtpTanggalLahir { get; set; }
        //[RegularExpression("^[A-Za-z+-]*$", ErrorMessage = "Bad Request")]
        public string KtpGolonganDarah { get; set; }
        //[RegularExpression("^[A-Za-z-/ ]*$", ErrorMessage = "Bad Request")]
        public string KtpJanisKelamin { get; set; }
        //[RegularExpression("^[0-9a-zA-Z.,-/' ]*$", ErrorMessage = "Bad Request")]
        public string KtpAlamat { get; set; }
        //[RegularExpression("^[A-Za-z0-9/ ]*$", ErrorMessage = "Bad Request")]
        public string KtpRTRW { get; set; }
        //[RegularExpression("^[0-9A-Za-z]*$", ErrorMessage = "Bad Request")]
        public string KtpRT { get; set; }
        //[RegularExpression("^[0-9A-Za-z]*$", ErrorMessage = "Bad Request")]
        public string KtpRW { get; set; }
        //[RegularExpression("^[0-9a-zA-Z.'/-, ]*$", ErrorMessage = "Bad Request")]
        public string KtpKelurahan { get; set; }
        //[RegularExpression("^[0-9a-zA-Z.'/-, ]*$", ErrorMessage = "Bad Request")]
        public string KtpKecamatan { get; set; }
        //[RegularExpression("^[0-9a-zA-Z.'/-, ]*$", ErrorMessage = "Bad Request")]
        public string KtpKota { get; set; }
        //[RegularExpression("^[0-9a-zA-Z.'/-, ]*$", ErrorMessage = "Bad Request")]
        public string KtpProvinsi { get; set; }
        //[RegularExpression("^[0-9a-zA-Z ]*$", ErrorMessage = "Bad Request")]
        public string KtpAgama { get; set; }
        //[RegularExpression("^[0-9a-zA-Z./-', ]*$", ErrorMessage = "Bad Request")]
        public string KtpStatusPerkawinan { get; set; }
        //[RegularExpression("^[0-9a-zA-Z/.,'- ]*$", ErrorMessage = "Bad Request")]
        public string KtpPekerjaan { get; set; }
        //[RegularExpression("^[0-9a-zA-Z/.,'- ]*$", ErrorMessage = "Bad Request")]
        public string KtpKewarganegaraan { get; set; }
        //[RegularExpression("^[0-9a-zA-Z-/ ]*$", ErrorMessage = "Bad Request")]
        public string KtpMasaBerlaku { get; set; }
        //[RegularExpression("^[0-9.,-/ ]*$", ErrorMessage = "Bad Request")]
        [AllowNull]
        public string KtpLatitude { get; set; }
        //[RegularExpression("^[0-9.,-/ ]*$", ErrorMessage = "Bad Request")]
        [AllowNull]
        public string KtpLongitude { get; set; }
        [AllowNull]
        //[RegularExpression("^[0-9a-zA-Z.,-/' ]*$", ErrorMessage = "Bad Request")]
        public string KtpAlamatConvertLengkap { get; set; }
        [AllowNull]
        //[RegularExpression("^[0-9.,-/ ]*$", ErrorMessage = "Bad Request")]
        public string KtpAlamatConvertLatlong { get; set; }
        //[RegularExpression("^[0-9a-zA-Z+/=]*$|^NULL$", ErrorMessage = "Bad Request")]
        public string KtpPhotoKTP { get; set; }
        //[RegularExpression("^[0-9a-zA-Z+/=]*$", ErrorMessage = "Bad Request")]
        public string KtpFingerKanan { get; set; }
        //[RegularExpression("^[0-9a-zA-Z/+=]*$", ErrorMessage = "Bad Request")]
        public string KtpFingerKananIso { get; set; }
        //[RegularExpression("^[0-9a-zA-Z+/=]*$", ErrorMessage = "Bad Request")]
        public string KtpFingerKiri { get; set; }
        //[RegularExpression("^[0-9a-zA-Z+/=]*$", ErrorMessage = "Bad Request")]
        public string KtpFingerKiriIso { get; set; }
        //[RegularExpression("^[0-9a-zA-Z+/=]*$", ErrorMessage = "Bad Request")]
        public string KtpPhotoCam { get; set; }
        //[RegularExpression("^[0-9a-zA-Z+/=]*$", ErrorMessage = "Bad Request")]
        public string KtpSignature { get; set; }
        //[RegularExpression("^[0-9]*$", ErrorMessage = "Bad Request")]
        public string KtpKodePos { get; set; }
        //[RegularExpression("^[0-9a-zA-Z\\[\\],-' ]*$", ErrorMessage = "Bad Request")]
        public string UID { get; set; }
        //[RegularExpression("^[0-9a-zA-Z+/= ]*$", ErrorMessage = "Bad Request")]
        public string KtpTypeJariKanan { get; set; }
        //[RegularExpression("^[0-9a-zA-Z+/= ]*$", ErrorMessage = "Bad Request")]
        public string KtpTypeJariKiri { get; set; }
        public string IpAddress { get; set; }
        public string KtpCif { get; set; }
    }

    public class EnrollKTPBiasaThirdParty
    {
        public string KtpNIK { get; set; }
        public string KtpNama { get; set; }
        public string KtpTTL { get; set; }
        public string KtpTempatLahir { get; set; }
        public string KtpTanggalLahir { get; set; }
        public string KtpGolonganDarah { get; set; }

        public string KtpJanisKelamin { get; set; }
        public string KtpAlamat { get; set; }
        public string KtpRTRW { get; set; }
        public string KtpRT { get; set; }
        public string KtpRW { get; set; }

        public string KtpKelurahan { get; set; }
        public string KtpKecamatan { get; set; }
        public string KtpKota { get; set; }
        public string KtpProvinsi { get; set; }
        public string KtpAgama { get; set; }
        public string KtpStatusPerkawinan { get; set; }
        public string KtpPekerjaan { get; set; }
        public string KtpKewarganegaraan { get; set; }
        public string KtpMasaBerlaku { get; set; }
        [AllowNull]
        public string KtpLatitude { get; set; }
        [AllowNull]
        public string KtpLongitude { get; set; }
        [AllowNull]
        public string KtpAlamatConvertLengkap { get; set; }
        [AllowNull]
        public string KtpAlamatConvertLatlong { get; set; }
        public string KtpPhotoKTP { get; set; }
        public string KtpFingerKanan { get; set; }
        public string KtpFingerKananIso { get; set; }
        public string KtpFingerKiri { get; set; }
        public string KtpFingerKiriIso { get; set; }
        public string KtpPhotoCam { get; set; }
        public string KtpSignature { get; set; }
        public string KtpKodePos { get; set; }
        public string UID { get; set; }
        public string KtpTypeJariKanan { get; set; }
        public string KtpTypeJariKiri { get; set; }
        public string IpAddress { get; set; }
        public string KtpCif { get; set; }
        public string NppCS { get; set; }

    }

    public class EnrollKTPNoMatching
    {
        public string KtpNIK { get; set; }
        public string KtpNama { get; set; }
        public string KtpTTL { get; set; }
        public string KtpTempatLahir { get; set; }
        public string KtpTanggalLahir { get; set; }
        public string KtpGolonganDarah { get; set; }

        public string KtpJanisKelamin { get; set; }
        public string KtpAlamat { get; set; }
        public string KtpRTRW { get; set; }
        public string KtpRT { get; set; }
        public string KtpRW { get; set; }

        public string KtpKelurahan { get; set; }
        public string KtpKecamatan { get; set; }
        public string KtpKota { get; set; }
        public string KtpProvinsi { get; set; }
        public string KtpAgama { get; set; }
        public string KtpStatusPerkawinan { get; set; }
        public string KtpPekerjaan { get; set; }
        public string KtpKewarganegaraan { get; set; }
        public string KtpMasaBerlaku { get; set; }
        public string KtpLatitude { get; set; }
        public string KtpLongitude { get; set; }
        public string KtpAlamatConvertLengkap { get; set; }
        public string KtpAlamatConvertLatlong { get; set; }
        public string KtpPhotoKTP { get; set; }
        public string KtpFingerKanan { get; set; }
        public string KtpFingerKananIso { get; set; }
        public string KtpFingerKiri { get; set; }
        public string KtpFingerKiriIso { get; set; }
        public string KtpPhotoCam { get; set; }
        public string KtpSignature { get; set; }
        public string KtpKodePos { get; set; }
        public string UID { get; set; }
        public string KtpTypeJariKanan { get; set; }
        public string KtpTypeJariKiri { get; set; }
        public string IpAddress { get; set; }
        public string KtpCif { get; set; }
        public string Notes { get; set; }
        [Required]
        public int ApprovedByEmployeeId { get; set; }
        [Required]
        public int ApprovedByEmployeeId2 { get; set; }
        public decimal MatchScore { get; set; }
    }

    public class EnrollKTPNoMatchingv2
    {
        public string KtpNIK { get; set; }
        public string KtpNama { get; set; }
        public string KtpTTL { get; set; }
        public string KtpTempatLahir { get; set; }
        public string KtpTanggalLahir { get; set; }
        public string KtpGolonganDarah { get; set; }

        public string KtpJanisKelamin { get; set; }
        public string KtpAlamat { get; set; }
        public string KtpRTRW { get; set; }
        public string KtpRT { get; set; }
        public string KtpRW { get; set; }

        public string KtpKelurahan { get; set; }
        public string KtpKecamatan { get; set; }
        public string KtpKota { get; set; }
        public string KtpProvinsi { get; set; }
        public string KtpAgama { get; set; }
        public string KtpStatusPerkawinan { get; set; }
        public string KtpPekerjaan { get; set; }
        public string KtpKewarganegaraan { get; set; }
        public string KtpMasaBerlaku { get; set; }
        public string KtpLatitude { get; set; }
        public string KtpLongitude { get; set; }
        public string KtpAlamatConvertLengkap { get; set; }
        public string KtpAlamatConvertLatlong { get; set; }
        public string KtpPhotoKTP { get; set; }
        public string KtpFingerKanan { get; set; }
        public string KtpFingerKananIso { get; set; }
        public string KtpFingerKiri { get; set; }
        public string KtpFingerKiriIso { get; set; }
        public string KtpPhotoCam { get; set; }
        public string KtpSignature { get; set; }
        public string KtpKodePos { get; set; }
        public string UID { get; set; }
        public string KtpTypeJariKanan { get; set; }
        public string KtpTypeJariKiri { get; set; }
        public string IpAddress { get; set; }
        public string KtpCif { get; set; }
        public string Notes { get; set; }
        [Required]
        public string nppPenyelia { get; set; }
        [Required]
        public int ApprovedByEmployeeId2 { get; set; }
        public decimal MatchScore { get; set; }
        public DateTime? otorisasiPenyelia { get; set; }
    }

    public class EnrollKTPThirdParty
    {
        public string KtpNIK { get; set; }
        public string KtpNama { get; set; }
        public string KtpTTL { get; set; }
        public string KtpTempatLahir { get; set; }
        public string KtpTanggalLahir { get; set; }
        public string KtpGolonganDarah { get; set; }

        public string KtpJanisKelamin { get; set; }
        public string KtpAlamat { get; set; }
        public string KtpRTRW { get; set; }
        public string KtpRT { get; set; }
        public string KtpRW { get; set; }

        public string KtpKelurahan { get; set; }
        public string KtpKecamatan { get; set; }
        public string KtpKota { get; set; }
        public string KtpProvinsi { get; set; }
        public string KtpAgama { get; set; }
        public string KtpStatusPerkawinan { get; set; }
        public string KtpPekerjaan { get; set; }
        public string KtpKewarganegaraan { get; set; }
        public string KtpMasaBerlaku { get; set; }
        [AllowNull]
        public string KtpLatitude { get; set; }
        [AllowNull]
        public string KtpLongitude { get; set; }
        [AllowNull]
        public string KtpAlamatConvertLengkap { get; set; }
        [AllowNull]
        public string KtpAlamatConvertLatlong { get; set; }
        public string KtpPhotoKTP { get; set; }
        public string KtpFingerKanan { get; set; }
        public string KtpFingerKananIso { get; set; }
        public string KtpFingerKiri { get; set; }
        public string KtpFingerKiriIso { get; set; }
        public string KtpPhotoCam { get; set; }
        public string KtpSignature { get; set; }
        public string KtpKodePos { get; set; }
        public string UID { get; set; }
        public string KtpTypeJariKanan { get; set; }
        public string KtpTypeJariKiri { get; set; }
        public string IpAddress { get; set; }
        public string KtpCif { get; set; }
        public string Notes { get; set; }
        [Required]
        public string NppCS { get; set; }
        [Required]
        public string NppPenyelia { get; set; }
        [Required]
        public string NppPemimpin { get; set; }
        public decimal MatchScore { get; set; }
    }
    public class EnrollKTPThirdParty2VM
    {
        [Required]
        public string KtpNIK { get; set; }
        [Required]
        public string KtpNama { get; set; }
        [Required]
        public string KtpTTL { get; set; }
        [Required]
        public string KtpTempatLahir { get; set; }
        [Required]
        public string KtpTanggalLahir { get; set; }
        [Required]
        public string KtpGolonganDarah { get; set; }
        [Required]
        public string KtpJanisKelamin { get; set; }
        [Required]
        public string KtpAlamat { get; set; }
        [Required]
        public string KtpRTRW { get; set; }
        [Required]
        public string KtpRT { get; set; }
        [Required]
        public string KtpRW { get; set; }
        [Required]
        public string KtpKelurahan { get; set; }
        [Required]
        public string KtpKecamatan { get; set; }
        [Required]
        public string KtpKota { get; set; }
        [Required]
        public string KtpProvinsi { get; set; }
        [Required]
        public string KtpAgama { get; set; }
        [Required]
        public string KtpStatusPerkawinan { get; set; }
        [Required]
        public string KtpPekerjaan { get; set; }
        [Required]
        public string KtpKewarganegaraan { get; set; }
        [Required]
        public string KtpMasaBerlaku { get; set; }
        
        public string KtpLatitude { get; set; }
       
        public string KtpLongitude { get; set; }
        
        public string KtpAlamatConvertLengkap { get; set; }
       
        public string KtpAlamatConvertLatlong { get; set; }
        [Required]
        public string KtpPhotoKTP { get; set; }
        //[Required]
        public string KtpFingerKanan { get; set; }
        //[Required]
        public string KtpFingerKananIso { get; set; }
        //[Required]
        public string KtpFingerKiri { get; set; }
        //[Required]
        public string KtpFingerKiriIso { get; set; }
        [Required]
        public string KtpPhotoCam { get; set; }
        [Required]
        public string KtpSignature { get; set; }
        //[Required]
        public string KtpKodePos { get; set; }
        //public string UID { get; set; }
        //[Required]
        public string KtpTypeJariKanan { get; set; }
        //[Required]
        public string KtpTypeJariKiri { get; set; }
        //public string IpAddress { get; set; }
        public string KtpCif { get; set; }
        public string Notes { get; set; }
        [Required]
        public string JournalID { get; set; }
        //public string NppCS { get; set; }
        //public decimal MatchScore { get; set; }
    }

}