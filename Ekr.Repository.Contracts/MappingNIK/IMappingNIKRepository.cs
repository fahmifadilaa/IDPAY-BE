using Ekr.Core.Entities.DataMaster.Lookup;
using Ekr.Core.Entities.MappingNIKPegawai;
using Ekr.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ekr.Repository.Contracts.MappingNIK
{
    public interface IMappingNIKRepository
    {
        Task<Tbl_MappingNIK_Pegawai> InsertData(Tbl_MappingNIK_Pegawai req);
        Task<Tbl_MappingNIK_Pegawai> UpdateData(Tbl_MappingNIK_Pegawai req, Tbl_MappingNIK_Pegawai_log log);
        Task<Tbl_MappingNIK_Pegawai> GetById(LookupByIdVM req);
        Task<int> GetExistingDataInsert(requestExist req);
        Task<int> GetExistingDataUpdate(requestExist req);
        Task<GridResponse<MappingNIKVM>> LoadData(mappingGrid req);
        Task DeleteData(LookupByIdVM req, Tbl_MappingNIK_Pegawai_log log);
    }
}
