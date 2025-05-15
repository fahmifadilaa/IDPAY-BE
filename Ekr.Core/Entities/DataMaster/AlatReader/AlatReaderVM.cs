using System;
using System.ComponentModel.DataAnnotations;

namespace Ekr.Core.Entities.DataMaster.AlatReader
{
    public class TblMasterAlatReader
    {
        public int? Id { get; set; }
        public int? Unit_Id { get; set; }
        public string Kode { get; set; }
        public string Nama { get; set; }
        public string SN_Unit { get; set; }
        public string No_Perso_SAM { get; set; }
        public string No_Kartu { get; set; }
        public string Pcid { get; set; }
        public string Confiq { get; set; }
        public string Uid { get; set; }
        public string Status { get; set; }
        public string LastIp { get; set; }
        public DateTime? LastPingIp { get; set; }
        public DateTime? LastActive { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public int? LastUserId { get; set; }
        public int? LastPegawaiId { get; set; }
        public int? LastUnitId { get; set; }
        public DateTime? LastUsed { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public DateTime? CreatedTime { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public DateTime? DeletedTime { get; set; }
        public int? CreatedBy_Id { get; set; }
        public int? UpdatedBy_Id { get; set; }
        public int? DeletedBy_Id { get; set; }
    }

    public class TblMasterAlatReaderVM
    {
        public int? Id { get; set; }
        public int? Unit_Id { get; set; }
        public string Kode { get; set; }
        public string NamaUnit { get; set; }
        public int? TypeUnitId { get; set; }
        public string TypeNamaUnit { get; set; }
        public string Nama { get; set; }
        public string SN_Unit { get; set; }
        public string No_Perso_SAM { get; set; }
        public string No_Kartu { get; set; }
        public string Pcid { get; set; }
        public string Confiq { get; set; }
        public string Uid { get; set; }
        public string Status { get; set; }
        public string LastIp { get; set; }
        public DateTime? LastPingIp { get; set; }
        public DateTime? LastActive { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public int? LastUserId { get; set; }
        public int? LastPegawaiId { get; set; }
        public int? LastUnitId { get; set; }
        public DateTime? LastUsed { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public DateTime? CreatedTime { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public DateTime? DeletedTime { get; set; }
        public int? CreatedBy_Id { get; set; }
        public int? UpdatedBy_Id { get; set; }
        public int? DeletedBy_Id { get; set; }
    }

    public class TblMasterAlatReaderLogActvity
    {
        public int Id { get; set; }
        public string UID { get; set; }
        public string Type { get; set; }
        public string NIK { get; set; }
        public string LastIp { get; set; }
        public int? PegawaiId { get; set; }
        public int? UnitId { get; set; }
        public string SessionId { get; set; }
        public string ReqCode { get; set; }
        public string ResultSocket { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsDeleted { get; set; }
        public DateTime? CreatedTime { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public DateTime? DeletedTime { get; set; }
        public int? CreatedBy_Id { get; set; }
        public int? UpdatedBy_Id { get; set; }
        public int? DeletedBy_Id { get; set; }
    }

    public class TblMasterAlatReaderLogError
    {
        public int Id { get; set; }
        public string SerialNumber { get; set; }
        public string UID { get; set; }
        public string Type { get; set; }
        public string NIK { get; set; }
        public string LastIp { get; set; }
        public int? PegawaiId { get; set; }
        public int? UnitId { get; set; }
        public string NppPegawai { get; set; }
        public string KodeUnit { get; set; }
        public string SessionId { get; set; }
        public string ReqCode { get; set; }
        public string ResultSocket { get; set; }
        public DateTime? CreatedTime { get; set; }
        public int? CreatedBy_Id { get; set; }
    }

    public class TblMasterAlatReaderUser
    {
        public int Id { get; set; }
        public string UID { get; set; }
        public int? PegawaiId { get; set; }
        public int? UnitId { get; set; }
        public DateTime? LastActive { get; set; }
    }
    public class ReqMasterAlatReaderGetByUid
    {
        public string UID { get; set; }
        public int pegawaiId { get; set; }
        public int unitId { get; set; }
        public string NppPegawai { get; set; }
        public string KodeUnit { get; set; }
    }
    public class ReqCreateMasterAlatReaderLogError
    {
        public string UID { get; set; }
        public string LastIp { get; set; }
        public string NppPegawai { get; set; }
        public string KodeUnit { get; set; }
        public string SessionId { get; set; }
        public string ReqCode { get; set; }
        public string ResultSocket { get; set; }
    }
    public class ReqCreateLogActivity
    {
        public string UID { get; set; }
        public string Type { get; set; }
        public string NIK { get; set; }
        public string LastIP { get; set; }
        public string NppPegawai { get; set; }
        public string KodeUnit { get; set; }
        public int PegawaiId { get; set; }
        public string SessionId { get; set; }
        public int UnitId { get; set; }
        public string ReqCode { get; set; }
        public string ResultSocket { get; set; }
        public string kode { get; set; }
        public string nama { get; set; }
        public string snUnit { get; set; }
        public string noPersoSam { get; set; }
        public string noKartu { get; set; }
        public string pcid { get; set; }
        public string confiq { get; set; }
        public string status { get; set; }
        public string lastPingIp { get; set; }
        public string latitude { get; set; }
        public string longitude { get; set; }
        public bool isActive { get; set; }
        public bool isDelete { get; set; }
    }
    public class ReqCreateLogConnection
    {
        public string UID { get; set; }
        public string ip { get; set; }
        public string status { get; set; }
        public string rfid { get; set; }
        public DateTime? startTimePing { get; set; }
        public DateTime? EndTimePing { get; set; }
    }
    public class ReqCreateMasterAlatReader
    {
        [RegularExpression("^[a-zA-Z0-9 ./,'-]*$", ErrorMessage = "Bad Request")]
        public string kode { get; set; }
        [RegularExpression("^[a-zA-Z0-9 ./,'-]*$", ErrorMessage = "Bad Request")]
        public string nama { get; set; }
        [RegularExpression("^[0-9]*$", ErrorMessage = "Bad Request")]
        public int unitId { get; set; }
        [RegularExpression("^[0-9]*$", ErrorMessage = "Bad Request")]
        public int LastUnitId { get; set; }
        [RegularExpression("^[a-zA-Z0-9 ./,'-]*$", ErrorMessage = "Bad Request")]
        public string snUnit { get; set; }
        [RegularExpression("^[a-zA-Z0-9 ./,'-]*$", ErrorMessage = "Bad Request")]
        public string noPersoSam { get; set; }
        [RegularExpression("^[0-9]*$", ErrorMessage = "Bad Request")]
        public string noKartu { get; set; }
        [RegularExpression("^[a-zA-Z0-9]*$", ErrorMessage = "Bad Request")]
        public string pcid { get; set; }
        [RegularExpression("^[a-zA-Z0-9]*$", ErrorMessage = "Bad Request")]
        public string confiq { get; set; }
        [RegularExpression("^[a-zA-Z0-9]*$", ErrorMessage = "Bad Request")]
        public string uid { get; set; }
        [RegularExpression("^[a-zA-Z0-9 ./,'-]*$", ErrorMessage = "Bad Request")]
        public string status { get; set; }
        [RegularExpression("^[-a-zA-Z0-9]*$", ErrorMessage = "Bad Request")]
        public string lastIp { get; set; }
        [RegularExpression("^[-a-zA-Z0-9]*$", ErrorMessage = "Bad Request")]
        public string lastPingIp { get; set; }
        [RegularExpression("^[-a-zA-Z0-9./' ]*$", ErrorMessage = "Bad Request")]
        public string latitude { get; set; }
        [RegularExpression("^[-a-zA-Z0-9./' ]*$", ErrorMessage = "Bad Request")]
        public string longitude { get; set; }
        [RegularExpression("^[0-9]*$", ErrorMessage = "Bad Request")]
        public bool isActive { get; set; }
        [RegularExpression("^[0-9]*$", ErrorMessage = "Bad Request")]
        public bool isDelete { get; set; }
        [RegularExpression("^[0-9]*$", ErrorMessage = "Bad Request")]
        public int lastPegawaiId { get; set; }
        [RegularExpression("^[a-zA-Z0-9]*$", ErrorMessage = "Bad Request")]
        public string lastNpp { get; set; }
        [RegularExpression("^[a-zA-Z0-9]*$", ErrorMessage = "Bad Request")]
        public string lastUnitCode{ get; set; }
    }

    public class ReqUpdateMasterAlatReader
    {
        [RegularExpression("^[a-zA-Z0-9]*$", ErrorMessage = "Bad Request")]
        public string uid { get; set; }
        [RegularExpression("^[a-zA-Z0-9 ./,'-]*$", ErrorMessage = "Bad Request")]
        public string status { get; set; }
        [RegularExpression("^[0-9.]*$", ErrorMessage = "Bad Request")]
        public string lastIp { get; set; }
        public int UpdatedBy_Id { get; set; }
        [RegularExpression("^[0-9/-]*$", ErrorMessage = "Bad Request")]
        public string lastActive { get; set; }
        [RegularExpression("^[0-9/-]*$", ErrorMessage = "Bad Request")]
        public string lastUsed { get; set; }
    }

    public class UpdateStatusManifestAlatReader
    {
        public string uid { get; set; }
        public bool IsManifestError { get; set; }
        public string npp { get; set; }
    }

    public class CheckVersion
    {
        public string urlDownload { get; set; }
        public string filebase64 { get; set; }
    }
    public class ReqAppsVersionViewModels
    {
        public int Id { get; set; }
        public int UpdatedBy_Id { get; set; }
    }
}
