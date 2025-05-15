using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ekr.Core.Entities.DataMaster.BornGeneration.ViewModel
{
    public class BornGenerationVM
    {
        public Int64 Number { get; set; }
        public int Id { get; set; }
        public string Nama { get; set; }
        public int TahunLahirAwal { get; set; }
        public int TahunLahirAkhir { get; set; }
    }
}
