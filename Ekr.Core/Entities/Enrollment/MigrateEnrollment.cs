using System;

namespace Ekr.Core.Entities.Enrollment
{
    public class MigrateEnrollment
    {
        public string CIF { get; set; }
        public string CIFNoDash { get; set; }
        public string NIK { get; set; }
        public string Nama { get; set; }
        public string Tempat_Lahir { get; set; }
        public DateTime? Tanggal_Lahir { get; set; }
        public string Jenis_Kelamin { get; set; }
        public string Golongan_Darah { get; set; }
        public string Alamat { get; set; }
        public string RT { get; set; }
        public string RW { get; set; }
        public string Kelurahan { get; set; }
        public string Desa { get; set; }
        public string Kecamatan { get; set; }
        public string Kota { get; set; }
        public string Provinsi { get; set; }
        public string Alamat_Lengkap { get; set; }
        public string Agama { get; set; }
        public string Status_Perkawinan { get; set; }
        public string Pekerjaan { get; set; }
        public string Kewarganegaraan { get; set; }
        public string Masa_Berlaku { get; set; }
        public string PathDownloadFotoKTP { get; set; }
        public string PathDownloadFotoTTD { get; set; }
        public string PathDownloadFotoWebcam { get; set; }
        public string FingerKanan { get; set; }
        public string FingerKiri { get; set; }
        public string CreatedBy_Npp { get; set; }
        public DateTime? Created_Time { get; set; }
        public string CreatedBy_SerialNumber { get; set; }
        public string Kode_Unit { get; set; }
        public string TypeFingerKanan { get; set; }
        public string TypeFingerKiri { get; set; }
    }
}
