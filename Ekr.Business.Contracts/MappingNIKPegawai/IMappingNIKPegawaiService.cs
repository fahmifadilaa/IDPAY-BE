using Ekr.Core.Entities.MappingNIKPegawai;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ekr.Business.Contracts.MappingNIKPegawai
{
    public interface IMappingNIKPegawaiService
    {
        Task<Tbl_MappingNIK_Pegawai> InsertMappingNIKAsync(Tbl_MappingNIK_Pegawai req);
        Task<Tbl_MappingNIK_Pegawai> UpdateMappingNIKAsync(Tbl_MappingNIK_Pegawai req);
    }
}
