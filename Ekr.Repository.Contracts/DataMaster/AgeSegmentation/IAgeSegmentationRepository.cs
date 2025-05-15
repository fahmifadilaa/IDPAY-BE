using Ekr.Core.Entities;
using Ekr.Core.Entities.DataMaster.AgeSegmentation.Entity;
using Ekr.Core.Entities.DataMaster.AgeSegmentation.ViewModel;
using System.Threading.Tasks;

namespace Ekr.Repository.Contracts.DataMaster.AgeSegmentation
{
    public interface IAgeSegmentationRepository
    {
        Task<GridResponse<AgeSegmentationVM>> LoadData(AgeSegmentationFilterVM req);
        Task<TblMasterSegmentasiUsia> GetAgeSegmentation(AgeSegmentationViewFilterVM req);
        Task<TblMasterSegmentasiUsia> InsertAgeSegmentation(TblMasterSegmentasiUsia req);
        Task<TblMasterSegmentasiUsia> UpdateAgeSegmentation(TblMasterSegmentasiUsia req);
        Task DeleteAgeSegmentation(AgeSegmentationViewFilterVM req, int PegawaiId);
    }
}
