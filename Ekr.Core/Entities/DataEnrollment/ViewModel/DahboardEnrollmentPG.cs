using Ekr.Core.Entities.Base;

namespace Ekr.Core.Entities.DataEnrollment.ViewModel
{
    public class DahboardEnrollmentPG
    {
        public int Id { get; set; }
        public string Nama { get; set; }
        public string NIK { get; set; }
        public string Provinsi { get; set; }
        public string Alamat { get; set; }
        public string TempatLahir { get; set; }
        public string TanggalLahir { get; set; }
        public string JenisKelamin { get; set; }
        public string PathFile { get; set; }
        public string StatusData { get; set; }
        public string EnrollBy { get; set; }
        public string CreatedTime { get; set; }
        public string File { get; set; }
    }

    public class DahboardEnrollmentPGFilterVM : BaseSqlGridFilter
    {
        public string Nama { get; set; }
        public string Provinsi { get; set; }
        public string UnitCode { get; set; }
        public string Role { get; set; }
    }
}
