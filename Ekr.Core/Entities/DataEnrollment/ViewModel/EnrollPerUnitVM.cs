using Ekr.Core.Entities.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ekr.Core.Entities.DataEnrollment.ViewModel
{
    public class EnrollPerUnitVM
    {
        public int Id { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public int jumlah { get; set; }
    }

    public class EnrollPerUnitFilterVM : BaseSqlGridFilter
    {
        public string Name { get; set; }
        public string Type { get; set; }
    }

    public class UnitIdsFilterVM
    {
        public string UnitIds { get; set; }
        public string Jenis { get; set; }
    }

    public class UnitIdsFilterVM2
    {
        public string UnitIds { get; set; }
        public string Jenis { get; set; }
        public string Tipe { get; set; }
    }
}
