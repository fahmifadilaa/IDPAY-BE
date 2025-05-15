using Ekr.Core.Entities.Base;
using System.ComponentModel.DataAnnotations;

namespace Ekr.Core.Entities.DataEnrollment
{
    public class DataEnrollTempFilter : BaseSqlGridFilter
    {
        public string NIK { get; set; } = null;
        public string Jenis { get; set; } = null;
        public string CIF { get; set; } = null;
        public string Nama { get; set; } = null;
        public string Browser { get; set; } = null;
        public string IpAddress { get; set; } = null;
        public string Url { get; set; } = null;
        public string UnitIds { get; set; }
        public int LoginUnitId { get; set; }
        public int LoginPegawaiId { get; set; }
        public int LoginRoleId { get; set; }
    }
    public class ExportDataEnrollTempFilter
    {
        public string NIK { get; set; } = null;
        public string Nama { get; set; } = null;
        public string UnitIds { get; set; }
        public int LoginUnitId { get; set; }
        public int LoginPegawaiId { get; set; }
        public int LoginRoleId { get; set; }
        public string SortColumn { get; set; } = "Id";
        public string SortColumnDir { get; set; } = "desc";
        public string Jenis { get; set; } = "";
    }

    public class ExportDataEnrollTempFilterNew
    {
        public string UnitIds { get; set; }
        public string Jenis { get; set; } = "";
    }

    public class DataEnrollTempFilterV2 : BaseSqlGridFilter
    {
        [RegularExpression("^[0-9]*$", ErrorMessage = "Bad Request")]
        public string NIK { get; set; } = null;
        [RegularExpression("^[-a-zA-Z0-9/,'. ]*$", ErrorMessage = "Bad Request")]
        public string Nama { get; set; } = null;
        //[RegularExpression("^[0-9]*$", ErrorMessage = "Bad Request")]
        public string UnitIds { get; set; }
        [RegularExpression("^[0-9]*$", ErrorMessage = "Bad Request")]
        public int LoginUnitId { get; set; }
        [RegularExpression("^[0-9]*$", ErrorMessage = "Bad Request")]
        public int LoginPegawaiId { get; set; }
        [RegularExpression("^[0-9]*$", ErrorMessage = "Bad Request")]
        public int LoginRoleId { get; set; }
        [RegularExpression("^[-a-zA-Z0-9/,'. ]*$", ErrorMessage = "Bad Request")]
        public string Jenis { get; set; } = "";
    }

    public class DataEnrollTempFilterV3 : BaseSqlGridFilter
    {
        
        //[RegularExpression("^[0-9]*$", ErrorMessage = "Bad Request")]
        public string UnitIds { get; set; }
        [RegularExpression("^[-a-zA-Z0-9/,'. ]*$", ErrorMessage = "Bad Request")]
        public string Jenis { get; set; } = "";
    }

    public class DataEnrollTempFilterV4 : BaseSqlGridFilter
    {
        [RegularExpression("^[0-9]*$", ErrorMessage = "Bad Request")]
        public string NIK { get; set; } = null;
        [RegularExpression("^[-a-zA-Z0-9/,'. ]*$", ErrorMessage = "Bad Request")]
        public string Nama { get; set; } = null;
        //[RegularExpression("^[0-9]*$", ErrorMessage = "Bad Request")]
        public string UnitIds { get; set; }
        [RegularExpression("^[0-9]*$", ErrorMessage = "Bad Request")]
        public int LoginUnitId { get; set; }
        [RegularExpression("^[0-9]*$", ErrorMessage = "Bad Request")]
        public int LoginPegawaiId { get; set; }
        [RegularExpression("^[0-9]*$", ErrorMessage = "Bad Request")]
        public int LoginRoleId { get; set; }
        [RegularExpression("^[-a-zA-Z0-9/,'. ]*$", ErrorMessage = "Bad Request")]
        public string Jenis { get; set; } = "";
        [RegularExpression("^[-a-zA-Z0-9/,'. ]*$", ErrorMessage = "Bad Request")]
        public string Tipe { get; set; } = "";
    }

    public class ExportDataEnrollTempFilterV5
    {
        public string NIK { get; set; } = null;
        public string Nama { get; set; } = null;
        public string UnitIds { get; set; }
        public int LoginUnitId { get; set; }
        public int LoginPegawaiId { get; set; }
        public int LoginRoleId { get; set; }
        public string SortColumn { get; set; } = "Id";
        public string SortColumnDir { get; set; } = "desc";
        public string Jenis { get; set; } = "";
        public string Tipe { get; set; } = "";
    }

    public class DataEnrollTemp2Filter : BaseSqlGridFilter
    {
        public string NIK { get; set; } = null;
        public string CIF { get; set; } = null;
        public string Nama { get; set; } = null;
        public string Browser { get; set; } = null;
        public string IpAddress { get; set; } = null;
        public string Url { get; set; } = null;
        public string LoginUnitId { get; set; }
        public int LoginPegawaiId { get; set; }
        public int LoginRoleId { get; set; }
    }
    public class ExportDataEnrollTemp2Filter
    {
        public string NIK { get; set; } = null;
        public string Nama { get; set; } = null;
        public string LoginUnitId { get; set; }
        public int LoginPegawaiId { get; set; }
        public int LoginRoleId { get; set; }
        public string SortColumn { get; set; } = "Id";
        public string SortColumnDir { get; set; } = "desc";
    }
}
