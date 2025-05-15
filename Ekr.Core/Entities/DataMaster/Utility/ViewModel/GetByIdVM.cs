using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ekr.Core.Entities.DataMaster.Utility.ViewModel
{
    public class GetByIdVM
    {
        public int Id { get; set; }
    }
    public class GetByIdAppVM
    {
        public string RoleId { get; set; }
        public int AppId { get; set; }
    }
}
