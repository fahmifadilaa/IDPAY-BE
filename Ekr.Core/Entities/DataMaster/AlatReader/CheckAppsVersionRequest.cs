using Ekr.Core.Entities.Base;
using System;

namespace Ekr.Core.Entities.DataMaster.AlatReader
{
    public class CheckAppsVersionRequest
    {
        public decimal Version { get; set; }
    }

    public class AppsVersionRequestFilter : BaseSqlGridFilter
    {
        public string Version { get; set; }
    }

    public class Tbl_VersionAgentVM
    {
        public int Number { get; set; }
        public int Id { get; set; }
        public string Version { get; set; }
        public string Keterangan { get; set; }
        public string FileName { get; set; }
        public string Path { get; set; }
    }
}
