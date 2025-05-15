using Ekr.Core.Entities;
using Ekr.Core.Entities.DataMaster.BornGeneration.Entity;
using Ekr.Core.Entities.DataMaster.BornGeneration.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ekr.Repository.Contracts.DataMaster.BornGeneration
{
    public interface IBornGenerationRepository
    {
        Task<GridResponse<BornGenerationVM>> LoadData(BornGenerationFilterVM req);
        Task<TblMasterGenerasiLahir>GetBornGeneration(BornGenerationViewFilterVM req);
        Task<TblMasterGenerasiLahir> InsertBornGeneration(TblMasterGenerasiLahir req);
        Task<TblMasterGenerasiLahir> UpdateBornGeneration(TblMasterGenerasiLahir req);
        Task DeleteBornGeneration(BornGenerationViewFilterVM req, int PegawaiId);
    }
}
