using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ekr.Core.Entities.DataMaster.AgeSegmentation.ViewModel
{
    public class AgeSegmentationVM
    {
        public Int64 Number { get; set; }
        public int Id { get; set; }
        public string Nama { get; set; }
        public int UsiaAwal { get; set; }
        public int UsiaAkhir { get; set; }
    }
}
