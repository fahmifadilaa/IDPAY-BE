using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ekr.Core.Entities.DataMaster.DataReader.ViewModel
{
    public class DashboardDetailReaderVM
    {
        public List<DataDashboard1VM> DataDashboard1 { get; set; }
        public List<DataDashboard2VM> DataDashboard2 { get; set; }
        public List<DataDashboard3VM> DataDashboard3 { get; set; }
    }

    public class DataDashboard1VM
    {
        public string JumlahEnroll { get; set; }
        public string JumlahActivity { get; set; }
        public string JumlahIP { get; set; }
        public string JumlahUser { get; set; }
    }

    public class DataDashboard2VM
    {
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
    public class DataDashboard3VM
    {
        public string Type { get; set; }
        public string label { get; set; }
        public int y { get; set; }
    }
}
