using Ekr.Core.Entities.Recognition;
using System.Collections.Generic;

namespace Ekr.Core.Entities.DataKTP
{
    public class ProfileByNik
    {
        public int Id { get; set; }
        public string ktp_NIK { get; set; }
        public string ktp_CIF { get; set; }
        public string ktp_Nama { get; set; }
        public string ktp_TempatLahir { get; set; }
        public string ktp_TanggalLahir { get; set; }
        public string ktp_TTL { get; set; }
        public string ktp_JanisKelamin { get; set; }
        public string ktp_GolonganDarah { get; set; }
        public string ktp_Alamat { get; set; }
        public string ktp_RT { get; set; }
        public string ktp_RW { get; set; }
        public string ktp_RTRW { get; set; }
        public string ktp_Kelurahan { get; set; }
        public string Desa { get; set; }
        public string ktp_Kecamatan { get; set; }
        public string ktp_Kota { get; set; }
        public string ktp_Provinsi { get; set; }
        public string ktp_Agama { get; set; }
        public string ktp_KodePos { get; set; }
        public string ktp_Latitude { get; set; }
        public string ktp_Longitude { get; set; }
        public string ktp_StatusPerkawinan { get; set; }
        public string ktp_Pekerjaan { get; set; }
        public string ktp_Kewarganegaraan { get; set; }
        public string ktp_MasaBerlaku { get; set; }
        public string ktp_AlamatConvertLengkap { get; set; }
        public string ktp_AlamatConvertLatlong { get; set; }
        public string ktp_PhotoKTP { get; set; }
        public string ktp_PhotoCam { get; set; }
        public string ktp_FingerKanan { get; set; }
        public string ktp_FingerKiri { get; set; }
        public string ktp_Signature { get; set; }
        public string ktp_TypeJariKanan { get; set; }
        public string ktp_TypeJariKiri { get; set; }

        public string RequestedImg { get; set; }
        public string ImgUrlPath1 { get; set; }
        public string ImgUrlPath2 { get; set; }
        public string ErrorMsg { get; set; }
        public decimal ktp_MatchScore { get; set; }
    }

    public class ProfileByNikOnlyImg
    {
        public int Id { get; set; }
        public string ktp_NIK { get; set; }
        public string ktp_CIF { get; set; }
        public string ktp_Nama { get; set; }
        public string ktp_TempatLahir { get; set; }
        public string ktp_TanggalLahir { get; set; }
        public string ktp_TTL { get; set; }
        public string ktp_JanisKelamin { get; set; }
        public string ktp_GolonganDarah { get; set; }
        public string ktp_Alamat { get; set; }
        public string ktp_RT { get; set; }
        public string ktp_RW { get; set; }
        public string ktp_RTRW { get; set; }
        public string ktp_Kelurahan { get; set; }
        public string Desa { get; set; }
        public string ktp_Kecamatan { get; set; }
        public string ktp_Kota { get; set; }
        public string ktp_Provinsi { get; set; }
        public string ktp_Agama { get; set; }
        public string ktp_KodePos { get; set; }
        public string ktp_Latitude { get; set; }
        public string ktp_Longitude { get; set; }
        public string ktp_StatusPerkawinan { get; set; }
        public string ktp_Pekerjaan { get; set; }
        public string ktp_Kewarganegaraan { get; set; }
        public string ktp_MasaBerlaku { get; set; }
        public string ktp_AlamatConvertLengkap { get; set; }
        public string ktp_AlamatConvertLatlong { get; set; }
        public string ktp_PhotoCam { get; set; }
        public string ktp_Signature { get; set; }
        public string ktp_PhotoKTP { get; set; }

        public string RequestedImg { get; set; }
        public string ErrorMsg { get; set; }
    }

    public class ProfileByNikOnlyFinger
    {
        public int Id { get; set; }
        public string ktp_NIK { get; set; }
        public string ktp_CIF { get; set; }
        public string ktp_Nama { get; set; }
        public string ktp_PhotoKTP { get; set; }
        public string ktp_PhotoCam { get; set; }
        public string ktp_FingerKanan { get; set; }
        public string ktp_FingerKiri { get; set; }
        public string ktp_FingerKananISO { get; set; }
        public string ktp_FingerKiriISO { get; set; }
        public string ktp_Signature { get; set; }
        public string ktp_TypeJariKanan { get; set; }
        public string ktp_TypeJariKiri { get; set; }

        public string RequestedImg { get; set; }
        public string ImgUrlPath1 { get; set; }
        public string ImgUrlPath2 { get; set; }
        public string ImgUrlPathISO1 { get; set; }
        public string ImgUrlPathISO2 { get; set; }
        public string ErrorMsg { get; set; }
        public decimal ktp_MatchScore { get; set; }
    }
}
