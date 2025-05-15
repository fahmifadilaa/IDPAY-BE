using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ekr.Core.Entities.DataEnrollment.ViewModel
{
    public class ConfirmEnrollSubmissionVM
    {
        public string NIK { get; set; }
        public bool IsVerified { get; set; }
        public string VerifiedByNpp { get; set; }
        public string VerifyComment { get; set; }
        public int UpdatedById { get; set; }
    }
}
