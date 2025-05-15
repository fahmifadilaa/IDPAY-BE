using Ekr.Core.Entities.Base;
using Ekr.Core.Entities.DataKTP;
using Ekr.Core.Entities.SettingThreshold;
using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Ekr.Core.Entities.Enrollment
{
    public class EnrollNoMatchingData { 
        public Int64 Number { get; set; }
        public int Id { get; set; }
        public bool isIKD { get; set; }
        public string NoPengajuan { get; set; }
        public string Nama { get; set; }
        public string NIK { get; set; }
        public string CIF { get; set; }
        public string TempatLahir { get; set; }
        public string TanggalLahir { get; set; }
        public string JenisKelamin { get; set; }
        public string AlamatLengkap { get; set; }
        public string PathFile { get; set; }
        public string File { get; set; }
        public string CreatedTime { get; set; }
        public string EnrollBy { get; set; }
        public string StatusPengajuan { get; set; }
        public string StatusData { get; set; }
        public string CreatedBy { get; set; }
        public string PenyeliaName { get; set; }
        public string PemimpinName { get; set; }
        public string UnitName { get; set; }
        public decimal ktp_MatchScore { get; set; }
    }

    public class EnrollmentFRLog { 
        public Int64 Id { get; set; }
        public string NIK { get; set; }
        public int CreatedById { get; set; }
        public string CreatedByNpp { get; set; }
        public DateTime CreatedTime { get; set; }
        public int CreatedByUnitId { get; set; }
        public decimal MatchScore { get; set; }
        public int inboxEnrollmentId { get; set; }
    }

    public class EnrollNoMatchingFilter : BaseSqlGridFilter
    {
        public string NIK { get; set; } = null;
        public List<int>? StatusPengajuan { get; set; }
        public int? ID { get; set; }
    }

    public class EnrollNoMatchingRequest
    {
        public int Id { get; set; }
        public string NoPengajuan { get; set; }
    }

    public class ScanQRIKDReq
    {
        public string qrCode { get; set; }
        public string api_key { get; set; }
        public string client_key { get; set; }
        public string channel { get; set; }
    }

    public class ScanQRIKDV2Req
    {
        public string qrCode { get; set; }
        public string channel { get; set; }
    }

    public class EnrollNoMatchingStatusRequest : EnrollNoMatchingRequest
    {
        public int Status { get; set; }
        public string Alasan { get; set; }
        public int ApproverId { get; set; }
    }

    public class FaceRecogRequest {
        [Required]
        public string idNum { get; set; }
        [Required]
        public string transactionId { get; set; }
        [Required]
        public string image { get; set; }
        [Required]
        public string photoThresholdDukcapil { get; set; }
    }

    public class FaceRecogRequestV2
    {
        [Required]
        public string nik { get; set; }
        [Required]
        public string trx_id { get; set; }
        public string channel { get; set; }
        public string name { get; set; }
        public string birthdate { get; set; }
        public string birthplace { get; set; }
        public string identity_photo { get; set; }
        [Required]
        public string selfie_photo { get; set; }
    }

    public class FaceRecogResponse { 
        public int? httpResponseCode { get; set; }
        public string matchScore { get; set; }
        public string transactionId { get; set; }
        public string uid { get; set; }
        public bool? verificationResult { get; set; }

    }

    public class FaceRecogResponseV2
    {
        public string status { get; set; }
        public string trx_id { get; set; }
        public string channel { get; set; }
        public string birthdate { get; set; }
        public string birthplace { get; set; }
        public string selfie_photo { get; set; }
        public string timestamp { get; set; }
        public string address { get; set; }
        public string ref_id { get; set; }
        public string errorCode { get; set; }
        public string errorMessage { get; set; }
    }

    public class CrawlingRequest { 
        public string nik { get; set; }
    }

    public class CrawlingResponse { 
        public bool error { get; set; }
        public string message { get; set; }
        public CrawlingResult data { get; set; }
    }

    public class CrawlingResult { 
        public CrawlingContent result { get; set; }
    }

    public class CrawlingContent { 
        public CrawlingSubContent content { get; set; }
    }

    public class CrawlingSubContent { 
        public string nik { get; set; }
        public string nama_lgkp { get; set; }
        public string tmpt_lhr { get; set; }
        public string tgl_lhr { get; set; }
        public string jenis_klmin { get; set; }
        public string jenis_pkrjn { get; set; }
        public string nama_lgkp_ibu { get; set; }
        public string status_kawin { get; set; }
        public string no_kk { get; set; }
        public string alamat { get; set; }
        public string no_rt { get; set; }
        public string no_rw { get; set; }
        public string no_kel { get; set; }
        public string kel_name { get; set; }
        public string no_kec { get; set; }
        public string kec_name { get; set; }
        public string no_kab { get; set; }
        public string kab_name { get; set; }
        public string no_prop { get; set; }
        public string prop_name { get; set; }
        public string foto { get; set; }
        public string status { get; set; }
        public string errorCode { get; set; }
        public string errorMessage { get; set; }
        public string timeMessage { get; set; }

    }

    public class ScanResponse { 
        public int err_code { get; set; }
        public string err_msg { get; set;}
        public IKDObject obj { get; set; }

    }

    public class ScanResponseV2
    {
        public string responseCode { get; set; }
        public string responseMessage { get; set; }
        public int err_code { get; set; }
        public string err_msg { get; set; }
        public string status { get; set; }
        public IKDObject data { get; set; }

    }

    public class ScanResponseEncrypt
    {
        public int err_code { get; set; }
        public string err_msg { get; set; }
        public string obj { get; set; }

    }

    public class ScanResponseXML
    {
        public int responseCode { get; set; }
        public string responseDesc { get; set; }
        public IKDObject data { get; set; }

    }

    public class IKDObject {
        public string nik { get; set; }
        public string foto { get; set; }
        public string kk { get; set; }
        public string nama { get; set; }
        public string tempat_lahir { get; set; }
        public string tanggal_lahir { get; set; }
        public string alamat { get; set; }
        public string rt { get; set; }
        public string rw { get; set; }
        public string kel_Desa { get; set; }
        public string kecamatan { get; set; }
        public string kabupaten_kota { get; set; }
        public string provinsi { get; set; }
        public string kode_pos { get; set; }
        public string golongan_darah { get; set; }
        public string status_pernikahan { get; set; }
        public string pekerjaan { get; set; }
        public string agama { get; set; }
        public string jenis_kelamin { get; set; }
        public string pendidikan { get; set; }
        public string status_hubungan_keluarga { get; set; }
        public string nama_ibu { get; set; }
        public string nik_ibu { get; set; }
        public string nama_ayah { get; set; }
        public string nik_ayah { get; set; }

    }

    public class IKDSessionResponse
    {
        public int err_code { get; set; }
        public string err_msg { get; set; }
        public IKDSession obj { get; set; }
    }

    public class IKDSession
    {
        public int id { get; set; }
        public int user_id { get; set; }
        public string npp { get; set; }
        public int role_id { get; set; }
        public int unit_id { get; set; }
        public DateTime? last_active { get; set; }
        public int attempts { get; set; }
        public DateTime? last_attempt { get; set; }
    }

    public class UrlRequestFaceRecognition
    {
        public string BaseUrl { get; set; }
        public string EndPoint { get; set; }
    }

    public class ServiceResponseFR<T>
    {
        public string Message { get; set; }
        public int Status { get; set; }
        public int Code { get; set; }
        public T Data { get; set; }
    }
    public class UrlRequestRecognitionFR
    {
        public string BaseUrl { get; set; }
        public string EndPoint { get; set; }
        public bool Env { get; set; }
    }

    public class UrlRequestCrawlingDukcapil
    {
        public string BaseUrl { get; set; }
        public string EndPoint { get; set; }
        public string BaseUrlDEV { get; set; }
        public string EndPointDEV { get; set; }
        public string HeaderName { get; set; }
        public string HeaderValue { get; set; }
        public string HeaderValueDev { get; set; }
        public bool Env { get; set; }
    }

    public class MatchingFingerReq {
        [Required]
        public string BaseImg64 { get; set; }
        [Required]
        public string BaseImg642 { get; set; }
        [Required]
        public bool isIso { get; set; }
    }

    public class InboxEnrollNoMatchingData
    {
        public Int64 Number { get; set; }
        public int Id { get; set; }
        public string NIK { get; set; }
        public string Nama { get; set; }
        public string TempatLahir { get; set; }
        public string TanggalLahir { get; set; }
        public string JenisKelamin { get; set; }
        public string AlamatLengkap { get; set; }
        public string PathFile { get; set; }
        public string File { get; set; }
        public string CreatedTime { get; set; }
        public string EnrollBy { get; set; }
        public string StatusData { get; set; }
        public string CIF { get; set; }
    }



    public class TblEnrollNoMatchingLogVM
    {

        public string Notes { get; set; }
        public string SubmitByNPP { get; set; }
        public string CSNpp { get; set; }
        public string CreatedTimeString { get; set; }

        public string CreatedByName { get; set; }

        public string StatusName { get; set; }
    }
}


