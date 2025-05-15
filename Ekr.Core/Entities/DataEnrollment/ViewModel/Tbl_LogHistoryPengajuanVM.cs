using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ekr.Core.Entities.DataEnrollment.ViewModel
{
    public class Tbl_LogHistoryPengajuanVM
    {
        public int Id { get; set; }
        public bool IsVerified { get; set; }
        public string Verifikasi { get; set; }
        public int DataKTPId { get; set; }
        public string DataKTPNIK { get; set; }
        public string ConfirmedByNpp { get; set; }
        public string ConfirmedByName { get; set; }
        public string Comment { get; set; }
        public DateTime? CreatedTime { get; set; }
    }
}
