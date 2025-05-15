using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ekr.Core.Entities.DataEnrollment.ViewModel
{
    public class TypeEnrollmentVM
    {
        public string enroll_with { get; set; }
        public decimal? prosentase { get; set; }
        public int jumlah { get; set; }
    }

    public class ChannelEnrollmentVM
    {
        public int jumlah { get; set; }
        public decimal? prosentase { get; set; }
        public string channel { get; set; }
    }

    public class StatusEnrollmentVM
    {
        public int jumlah { get; set; }
        public decimal? prosentase { get; set; }
        public string Status { get; set; }
    }
}
