using System.Threading.Tasks;

namespace Ekr.Business.Contracts.DataMigration
{
    public interface IEnrollment
    {
        Task<int> MigrateFingerJpgToEncTxt();
        Task<int> MigrateFingerJpgToEncTxtByNIK(string nik);
    }
}
