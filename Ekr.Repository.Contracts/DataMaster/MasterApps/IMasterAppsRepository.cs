using Ekr.Core.Entities.DataMaster.Lookup;
using Ekr.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ekr.Core.Entities.DataMaster.MasterApps;

namespace Ekr.Repository.Contracts.DataMaster.MasterApps
{
    public interface IMasterAppsRepository
    {
        Task<GridResponse<MasterAppsVM>> LoadData(MasterAppsFilter req);
        Task<Tbl_Master_Apps> GetById(MasterAppsByIdVM req);
        Task<Tbl_Master_Apps> InsertLookup(Tbl_Master_Apps req);
        Task<Tbl_Master_Apps> UpdateLookup(Tbl_Master_Apps req);
        Task DeleteLookup(MasterAppsByIdVM req, int PegawaiId);
    }
}
