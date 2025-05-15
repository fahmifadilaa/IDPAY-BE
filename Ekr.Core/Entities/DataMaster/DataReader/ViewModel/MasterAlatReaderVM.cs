using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ekr.Core.Entities.DataMaster.DataReader.ViewModel
{
    public class MasterAlatReaderVM
    {
        public Int64 Number { get; set; }
        public int Id { get; set; }
        public int? Unit_Id { get; set; }
        public string NamaUnit { get; set; }
        public int? TypeUnitId { get; set; }
        public string TypeUnitNama { get; set; }
        public string UID { get; set; }
        public string PCID { get; set; }
        public string Confiq { get; set; }
        public string Kode { get; set; }
        public string Nama { get; set; }
        public string SNUnit { get; set; }
        public string LastIp { get; set; }
        public DateTime? LastPingIp { get; set; }
        public string Status { get; set; }
        public DateTime? LastActive { get; set; }
        public string LastActiveString { get; set; }
        public int? LastUserId { get; set; }
        public DateTime? LastUsed { get; set; }
        public string LastUsedString { get; set; }
    }
}
