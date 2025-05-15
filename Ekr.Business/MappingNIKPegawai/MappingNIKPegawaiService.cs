

using Ekr.Business.Contracts.MappingNIKPegawai;
using Ekr.Core.Entities.DataMaster.Lookup;
using Ekr.Core.Entities.MappingNIKPegawai;
using Ekr.Repository.Contracts.MappingNIK;
using System;
using System.Threading.Tasks;

namespace Ekr.Business.MappingNIKPegawai
{
    public class MappingNIKPegawaiService : IMappingNIKPegawaiService
    {
        private readonly IMappingNIKRepository _MappingdRepository;

        public MappingNIKPegawaiService(IMappingNIKRepository MappingRepository)
        {
            _MappingdRepository = MappingRepository;
        }

        public async Task<Tbl_MappingNIK_Pegawai> InsertMappingNIKAsync(Tbl_MappingNIK_Pegawai req)
        {
            var exist = await _MappingdRepository.GetExistingDataInsert(new requestExist() { NIK = req.NIK, Npp = req.Npp});

            if (exist > 0) {
                return new Tbl_MappingNIK_Pegawai();
            }

            var resp = await _MappingdRepository.InsertData(req);

            return resp;
        }

        public async Task<Tbl_MappingNIK_Pegawai> UpdateMappingNIKAsync(Tbl_MappingNIK_Pegawai req)
        {
            var exist = await _MappingdRepository.GetExistingDataUpdate(new requestExist() { NIK = req.NIK, Npp = req.Npp, Id = req.Id });

            var resp = new Tbl_MappingNIK_Pegawai();
            if (exist > 0)
            {
                return new Tbl_MappingNIK_Pegawai();
            }
            var dataexist = await _MappingdRepository.GetById(new LookupByIdVM() { Id = req.Id });

            var log = new Tbl_MappingNIK_Pegawai_log()
            {
                NIK = dataexist.NIK, Npp = dataexist.Npp, Nama = dataexist.Nama, CreatedDate = DateTime.Now, CreateById = req.UpdateById, Keterangan = "Update"
            };

            resp = await _MappingdRepository.UpdateData(req, log);

            return resp;
        }
    }
}
