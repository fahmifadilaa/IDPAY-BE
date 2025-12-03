using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace  Ekr.Core.Entities
{
    public class BanchlinkCifNikUpdateRequest
    {
        public string IsAuthorized { get; set; }
        [Required]
        public string CIF { get; set; }
        [Required]
        public string NIK { get; set; }
        public string SpvID { get; set; }
        [Required]
        public string BranchID { get; set; }
        [Required]
        public string TellerID { get; set; }
    }
    public class BanchlinkCifNikUpdateResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string? ErrorCode { get; set; }
    }
}
