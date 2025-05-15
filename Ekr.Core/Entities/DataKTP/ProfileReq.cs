namespace Ekr.Core.Entities.DataKTP
{
    public class ProfileReq
    {
        public string Base64Img { get; set; }
        public string Nik { get; set; }
        public string FingerType { get; set; }
        public string Npp { get; set; }
        public string UnitCode { get; set; }
        public string LvTeller { get; set; }
        public string Branch { get; set; }
        public string SubBranch { get; set; }
        public string ClientApps { get; set; }
        public string EndPoint { get; set; }
    }

    public class ProfileByNppReq
    {
        public string Base64Img { get; set; }
        public string Npp { get; set; }
        public string FingerType { get; set; }
        public string NppRequester { get; set; }
        public string UnitCode { get; set; }
        public string LvTeller { get; set; }
        public string Branch { get; set; }
        public string SubBranch { get; set; }
        public string ClientApps { get; set; }
        public string EndPoint { get; set; }
    }

    public class ProfileLoopReq
    {
        public string Base64Img { get; set; }
        public string Nik { get; set; }
        public string Npp { get; set; }
        public string UnitCode { get; set; }
        public string LvTeller { get; set; }
        public string Branch { get; set; }
        public string SubBranch { get; set; }
        public string ClientApps { get; set; }
        public string EndPoint { get; set; }
    }

    public class ProfileLoopNIKThirdPartyReq
    {
        public string Base64Img { get; set; }
        public string Nik { get; set; }
        public string UnitCode { get; set; }
        public string LvTeller { get; set; }
        public string Branch { get; set; }
        public string SubBranch { get; set; }
        public string ClientApps { get; set; }
        public string EndPoint { get; set; }
    }
    public class ProfileLoopNppThirdPartyReq
    {
        public string Base64Img { get; set; }
        public string Npp { get; set; }
        public string UnitCode { get; set; }
        public string LvTeller { get; set; }
        public string Branch { get; set; }
        public string SubBranch { get; set; }
        public string ClientApps { get; set; }
        public string EndPoint { get; set; }
    }
    public class ProfileLoopCifThirdPartyReq
    {
        public string Base64Img { get; set; }
        public string Cif { get; set; }
        public string UnitCode { get; set; }
        public string LvTeller { get; set; }
        public string Branch { get; set; }
        public string SubBranch { get; set; }
        public string ClientApps { get; set; }
        public string EndPoint { get; set; }
    }

    public class ProfileFRReq
    {
        public string Nik { get; set; }
        public string Npp { get; set; }
        public string UnitCode { get; set; }
        public string LvTeller { get; set; }
        public string Branch { get; set; }
        public string SubBranch { get; set; }
        public string ClientApps { get; set; }
        public string EndPoint { get; set; }
    }

    public class ProfileLoopNppReq
    {
        public string Base64Img { get; set; }
        public string Npp { get; set; }
        public string NppRequester { get; set; }
        public string UnitCode { get; set; }
        public string LvTeller { get; set; }
        public string Branch { get; set; }
        public string SubBranch { get; set; }
        public string ClientApps { get; set; }
        public string EndPoint { get; set; }
    }

    public class ProfileLoopByCifReq
    {
        public string Base64Img { get; set; }
        public string Cif { get; set; }
        public string Npp { get; set; }
        public string UnitCode { get; set; }
        public string LvTeller { get; set; }
        public string Branch { get; set; }
        public string SubBranch { get; set; }
        public string ClientApps { get; set; }
        public string EndPoint { get; set; }
    }

    public class TestDecrypt
    {
        public string EncryptedText { get; set; }
        public string WantedResult { get; set; }
    }

    public class TestDecryptRes
    {
        public string TextResult { get; set; }
        public bool IsMatch { get; set; }
    }
}
