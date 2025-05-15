namespace Ekr.Core.Entities.Recognition
{
    public class FingerByNik
    {
        public string NIK { get; set; }
        public string TypeFinger { get; set; }
    }

    public class FingerISOByNik
    {
        public string NIK { get; set; }
        public string TypeFinger { get; set; }
        public string PathFileISO { get; set; }
        public string FileNameISO { get; set; }
        public string FIleJariISO { get; set; }
    }

    public class FingerByType
    {
        public int Id { get; set; }
        public string NIK { get; set; }
        public string TypeFinger { get; set; }
        public string Url { get; set; }
        public string FileJari { get; set; }
    }

    public class FingerByTypeISO
    {
        public int Id { get; set; }
        public string NIK { get; set; }
        public string TypeFinger { get; set; }
        public string Url { get; set; }
        public string FileJari { get; set; }
        public string FileJariISO { get; set; }
    }
}
