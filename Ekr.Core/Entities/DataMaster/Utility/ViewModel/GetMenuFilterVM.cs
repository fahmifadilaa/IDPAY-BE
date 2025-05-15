using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ekr.Core.Entities.DataMaster.Utility.ViewModel
{
    public class GetMenuFilterVM
    {
        public string RoleId { get; set; }
        public int AppId { get; set; }
    }
    public class GetMenuByIdFilterVM
    {
        public int Id { get; set; }
    }
}
