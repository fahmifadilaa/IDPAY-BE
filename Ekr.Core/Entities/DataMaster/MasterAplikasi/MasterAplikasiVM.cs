using ServiceStack.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ekr.Core.Entities.DataMaster.MasterAplikasi
{
    public partial class TblMasterAplikasi
    {
        public int Id { get; set; }
        public string Kode { get; set; }
        public string Nama { get; set; }
        public string Deskripsi { get; set; }
        public string Images { get; set; }
        public string Url_Default { get; set; }
        public int? Order_By { get; set; }
        public DateTime? CreatedTime { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public DateTime? DeletedTime { get; set; }
        public int? CreatedById { get; set; }
        public int? UpdatedById { get; set; }
        public int? DeletedById { get; set; }
        public bool? IsDeleted { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsVisible { get; set; }
    }

    public class Tbl_Master_Aplikasi
    {
        [AutoIncrement]
        public int Id { get; set; }
        public string Kode { get; set; }
        public string Nama { get; set; }
        public string Deskripsi { get; set; }
        public string Images { get; set; }
        public string Url_Default { get; set; }
        public int? Order_By { get; set; }
        public DateTime? CreatedTime { get; set; }
        public DateTime? UpdatedTime { get; set; }
        public DateTime? DeletedTime { get; set; }
        public int? CreatedById { get; set; }
        public int? UpdatedById { get; set; }
        public int? DeletedById { get; set; }
        public bool? IsDeleted { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsVisible { get; set; }
    }
}
