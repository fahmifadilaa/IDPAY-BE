using Ekr.Core.Entities.Recognition;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ekr.Repository.Contracts.Recognition
{
    public interface IFingerRepository
    {
        Task<IEnumerable<FingerByNik>> GetFingersEnrolled(string nik);
        Task<IEnumerable<FingerByNik>> GetFingersEnrolledByNikIsoDB(string nik);
        Task<IEnumerable<FingerByNik>> GetFingersEnrolledByNikIsoFile(string nik);
        Task<IEnumerable<FingerByNik>> GetFingersEnrolledNpp(string npp);
        Task<IEnumerable<FingerByNik>> GetFingersEnrolledByNppIsoFile(string npp);
        Task<IEnumerable<FingerByNik>> GetFingersEnrolledByNppIsoDB(string npp);
        Task<IEnumerable<FingerByNik>> GetFingersEmpEnrolled(string nik);
        Task<FingerByType> GetFingerByType(string nik, string type);
        Task<IEnumerable<FingerByNik>> GetFingersEnrolledByCIF(string cif);
        Task<IEnumerable<FingerByNik>> GetFingersEnrolledByCIFIsoFile(string cif);
        Task<IEnumerable<FingerByNik>> GetFingersEnrolledByCIFIsoDB(string cif);
        Task<FingerByType> GetFingerByTypeEmp(string nik, string type);
        Task<IEnumerable<FingerISOByNik>> GetFingersISOEnrolledNik(string nik);
        Task<FingerByTypeISO> GetFingerByTypeEmpISO(string nik, string type);
        Task<FingerByType> GetFingerByTypeEmpIso(string nik, string type);
    }
}
