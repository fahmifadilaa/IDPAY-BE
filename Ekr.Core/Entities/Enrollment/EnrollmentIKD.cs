using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ekr.Core.Entities.Enrollment
{
    public class Tbl_Enrollment_IKD {
        [AutoIncrement]
        [Column(TypeName = "bigint")]
        public long Id { get; set; }
        [Column(TypeName = "nvarchar(50)")]
        public string NIK { get; set; }
        public int CreatedById { get; set; }
        [Column(TypeName = "nvarchar(50)")]
        public string CreatedByNpp { get; set; }
        public DateTime? CreatedTime { get; set; }
        public int CreatedByUnitId { get; set; }
    }
}
