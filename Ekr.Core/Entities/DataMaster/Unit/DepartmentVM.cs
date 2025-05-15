using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ekr.Core.Entities.DataMaster.Unit
{
    public class DepartmentVM
    {
        public Int64 Number { get; set; }
        public int Id { get; set; }
        public string ParentName { get; set; }
        public int Type { get; set; }
        public string TypeName { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public string Code { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string Address { get; set; }
        public string Telepon { get; set; }
        public string FullCode { get; set; }
    }
}
