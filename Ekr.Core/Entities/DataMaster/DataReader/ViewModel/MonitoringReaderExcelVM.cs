using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ekr.Core.Entities.DataMaster.DataReader.ViewModel
{
    public class MonitoringReaderExcelVM
    {
        public int? Id { get; set; }
        public string Kode { get; set; }
        public string Nama { get; set; }
        public string SN_Unit { get; set; }
        public string No_Perso_SAM { get; set; }
        public string No_Kartu { get; set; }
        public string PCID { get; set; }
        public string Confiq { get; set; }
        public string UID { get; set; }
        public string Status { get; set; }
        public string LastIP { get; set; }
        public string LastPingIP { get; set; }
        public string LastActive { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string LastPegawai { get; set; }
        public string LastUsed { get; set; }
    }
}
