using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ekr.Core.Entities.Logging
{
    public class Tbl_Enrollment_ThirdParty_Log
    {
        [AutoIncrement]
        public int Id { get; set; }
        public string AppsChannel { get; set; }
        public string NIK { get; set; }
        public DateTime SubmitDate { get; set; }
        public string JournalID { get; set; }
    }
}
