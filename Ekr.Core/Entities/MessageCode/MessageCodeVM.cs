using System;

namespace Ekr.Core.Entities.MessageCode
{
    public class MessageCodeVM
    {
        public Int64 Number { get; set; }
        public int Id { get; set; }
        public string Code { get; set; }
        public string Message { get; set; }
        public string Description { get; set; }
        public string Mitigation { get; set; }
        public bool IsActive { get; set; }
        public string Created_Time { get; set; }
        public string Created_By { get; set; }
        public string Updated_Time { get; set; }
        public string Updated_By { get; set; }
        public string Deleted_By { get; set; }
        public string Deleted_Time { get; set; }
        public MessageCodeVM()
        {
            //baru
            IsActive = true;
        }
    }

    public class Tbl_Master_MessageCode
    {
        public int Id { get; set; }

        public string Code { get; set; }

        public string Message { get; set; }
        public string Description { get; set; }

        public string Mitigation { get; set; }

        public DateTime? CreatedTime { get; set; }

        public DateTime? UpdatedTime { get; set; }

        public DateTime? DeletedTime { get; set; }

        public int? CreatedBy_Id { get; set; }

        public int? UpdatedBy_Id { get; set; }

        public int? DeletedBy_Id { get; set; }

        public bool? IsDeleted { get; set; }

        public bool? IsActive { get; set; }
    }
}
