using ServiceStack.DataAnnotations;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ekr.Core.Entities.DataEnrollment.Entity
{
    public class Tbl_LogHistoryPengajuan
    {
        [AutoIncrement]
        [Column(TypeName = "bigint")]
        public long Id { get; set; }
        public bool IsVerified { get; set; }
        [Column(TypeName = "bigint")]
        public long DataKTPId { get; set; }
        public string DataKTPNIK { get; set; }
        public string ConfirmedByNpp { get; set; }
        public string Comment { get; set; }
        public DateTime? CreatedTime { get; set; }
        public int? CreatedBy_Id { get; set; }

    }
}
