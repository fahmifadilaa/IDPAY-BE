using Ekr.Core.Entities.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ekr.Core.Entities.DataEnrollment.ViewModel
{
    public class InboxDataEnrollVM
    {
        public int Id { get; set; }
        public string NIK { get; set; }
        public string Nama { get; set; }
        public string TempatLahir { get; set; }
        public DateTime? TanggalLahir { get; set; }
        public string TanggalLahirString { get; set; }
        public string JenisKelamin { get; set; }
        public string GolonganDarah { get; set; }
        public string Npp { get; set; }
    }

    public class InboxDataEnrollFilterVM : BaseSqlGridFilter
    {
        public string UnitCode { get; set; }
        public string Npp { get; set; }
    }

    public class HistorySubmissionFilterVM
    {
        public int DataKTPId { get; set; }
    }
}
