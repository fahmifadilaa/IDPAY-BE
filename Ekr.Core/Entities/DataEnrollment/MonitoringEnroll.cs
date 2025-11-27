using System;

namespace Ekr.Core.Entities.DataEnrollment
{
    public class MonitoringEnroll
    {
        public Int64 Number { get; set; }
        public int Id { get; set; }
        public string NIK { get; set; }
        public string Nama { get; set; }
        public string status { get; set; }
        public string enroll_with { get; set; }
        public string channel { get; set; }
        public string TempatLahir { get; set; }
        public string TanggalLahir { get; set; }
        public string JenisKelamin { get; set; }
        public string AlamatLengkap { get; set; }
        public string PathFile { get; set; }
        public string File { get; set; }
        public string CreatedTime { get; set; }
        public string EnrollBy { get; set; }
        public string StatusData { get; set; }
        public string CIF { get; set; }
        public string MakerName { get; set; }
        public string ReviewerName { get; set; }
        public string ApproverName { get; set; }
        //public string StatusEnrollment { get; set; }
    }

    public class MonitoringEnrollNew
    {
        public Int64 Number { get; set; }
        public string KantorWilayah { get; set; }
        public string NamaCabang { get; set; }
        public string NamaOutlet { get; set; }
        public Int64 JumlahEnrollmentKTP { get; set; }
        public Int64 JumlahEnrollmentFR { get; set; }
    }

    public class MonitoringEnrollCount
    {
        public int Total { get; set; }
    }

    public class ExportMonitoringEnroll
    {
        public Int64 Number { get; set; }
        public int Id { get; set; }
        public string NIK { get; set; }
        public string Nama { get; set; }
        public string Provinsi { get; set; }
        public string TempatLahir { get; set; }
        public string TanggalLahir { get; set; }
        public string JenisKelamin { get; set; }
        public string AlamatLengkap { get; set; }
        public string CreatedTime { get; set; }
        public string EnrollBy { get; set; }
        public string StatusData { get; set; }
        public string Usia { get; set; }
        public string SegmentasiUsia { get; set; }
        public string GenerasiLahir { get; set; }
        public string CIF { get; set; }
        public string EnrollFR { get; set; }
        public string Status { get; set; } = "";
    }

    public class ExportMonitoringEnrollNew
    {
        public Int64 Number { get; set; }
        public string KantorWilayah { get; set; }
        public string NamaCabang { get; set; }
        public string NamaOutlet { get; set; }
        public Int64 JumlahEnrollmentKTP { get; set; }
        public Int64 JumlahEnrollmentFR { get; set; }
    }

    public class ExportMonitoringEnrollTest
    {
        public string NIK { get; set; }
        public string Nama { get; set; }
    }
}
