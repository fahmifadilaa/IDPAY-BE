using System;

namespace Ekr.Core.Entities.ThirdParty
{
    public class ApiSOA
    {
        public string systemId { get; set; }
        public string numId { get; set; }
        public string idType { get; set; }
        public string host { get; set; }
        public string createdById { get; set; }
        public string createdByUnitId { get; set; }
        public string branch { get; set; }
        public string teller { get; set; }
        public string baseUrlNonSoa { get; set; }
        public string UrlEndPointNonSoa { get; set; }
    }

    public class ApiSOAResponse
    {
        public string cif { get; set; }
        public string coreJournal { get; set; }
        public string errorNum { get; set; }
        public string errorDescription { get; set; }
    }

    public class Tbl_ThirdPartyLog
    {
        public int Id { get; set; }
        public string? FeatureName { get; set; }
        public string? HostUrl { get; set; }
        public string? Request { get; set; }
        public int? Status { get; set; }
        public string? Response { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string CreatedBy { get; set; }

    }
}
