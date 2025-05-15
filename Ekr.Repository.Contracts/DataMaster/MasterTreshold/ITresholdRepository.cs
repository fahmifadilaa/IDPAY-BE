using Ekr.Core.Entities.DataMaster.Lookup;
using Ekr.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ekr.Core.Entities.DataMaster.MasterTreshold;

namespace Ekr.Repository.Contracts.DataMaster.MasterTreshold
{
    public interface ITresholdRepository
    {
        Task<GridResponse<TresholdVM>> LoadData(TresholdFilter req);
        Task<TblMasterTreshold> GetById(TresholdByIdVM req);
        Task<TblMasterTreshold> InsertTreshold(TblMasterTreshold req);
        Task<TblMasterTreshold> UpdateTreshold(TblMasterTreshold req);
        Task DeleteTreshold(TresholdByIdVM req, int PegawaiId);
    }
}
