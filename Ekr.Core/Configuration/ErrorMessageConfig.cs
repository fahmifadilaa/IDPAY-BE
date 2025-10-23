namespace Ekr.Core.Configuration
{
    public class ErrorMessageConfig
    {
        public string MatchingError { get; set; }
        public string TimeoutException { get; set; }
        public string SQLException { get; set; }
        public string NasabahGagalEnroll { get; set; }
        public string DemografiTidakDitemukan { get; set; }
        public string ErrorParameterKosong { get; set; }
        public string NasabahPernahEnroll { get; set; }
        public string NasabahPernahEnrollProses { get; set; }
        public string InputTidakLengkap { get; set; }
        public string DemografiGagalEnroll { get; set; }
        public string NIKNotFound { get; set; }
        public string MatchingFingerNotFound { get; set; }
        public string ChangePasswordSalah { get; set; }
        public string ConformPasswordSalah { get; set; }
        public string ImageTidakSesuai { get; set; }
        public string InternalError { get; set; }
        public string MatchingFingerFailed { get; set; }
        public string FailedAddTokenToDB { get; set; }
        public string NoAccess { get; set; }
        public string UnitNotRegistered { get; set; }
        public string RoleNotRegistered { get; set; }
        public string AccountNotActive { get; set; }
        public string AccountCuti { get; set; }
        public string CredentialSalah { get; set; }
        public string RefreshTokenFailed { get; set; }
        public string RefreshTokenInvalid { get; set; }
        public string ErrorCreteTokenFromRefreshToken { get; set; }
        public string UserCSNotFound { get; set; }
        public string UserPenyeliaNotFound { get; set; }
        public string UserPemimpinNotFound { get; set; }
        public string OpsiJariKananInvalid { get; set; }
        public string OpsiJariKiriInvalid { get; set; }
        public string OpsiAgamaInvalid { get; set; }
        public string OpsiStatusPerkawinanInvalid { get; set; }
        public string OpsiGenderInvalid { get; set; }
        public string FileTypeFingerKananInvalid { get; set; }
        public string FileTypeFingerKiriInvalid { get; set; }
        public string FileTypeFingerKananISOInvalid { get; set; }
        public string FileTypeFingerKiriISOInvalid { get; set; }
        public string OpsiTipeFileMarker { get; set; }
        public string MasaBerlakuKTPInvalid { get; set; }
        public string FormatTglLahirInvalid { get; set; }
        public string OpsiGolDarahInvalid { get; set; }
        public string OpsiKewarganegaraanInvalid { get; set; }
        public string FormatRTRWInvalid { get; set; }
        public string FormatRTRW2Invalid { get; set; }
        public string FingerNotFound { get; set; }
        public string FingerParamsNotComplete { get; set; }
        public string MsgNumericOnly { get; set; }
        public string NIKLessOrMoreThan16 { get; set; }
        public string PostalCodeBetween5And10 { get; set; }
        public string LoginMaxLimitReached { get; set; }
        public string LimitScanReached { get; set; }
        public string InputTidakSesuai { get; set; }
        public string LDAPService { get; set; }
        public string LDAPUnitNull { get; set; }
        public string LDAPRoleNull { get; set; }
    }
}