using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ekr.Core.Entities.DataMaster.Utility.ViewModel
{
    public class SystemParameterVM
    {
        public int Id { get; set; }

        public string KataKunci { get; set; }

        public string Value { get; set; }

        public string Keterangan { get; set; }

        public DateTime? CreatedTime { get; set; }

        public DateTime? UpdatedTime { get; set; }

        public DateTime? DeletedTime { get; set; }

        public int? CreatedBy_Id { get; set; }

        public int? UpdatedBy_Id { get; set; }

        public int? DeletedBy_Id { get; set; }

        public bool? IsActive { get; set; }

        public bool? IsDelete { get; set; }
    }
}
