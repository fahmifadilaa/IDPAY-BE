using Ekr.Core.Entities;
using Ekr.Core.Entities.DataMaster.Pegawai;
using Ekr.Core.Entities.Enrollment;
using System.Threading.Tasks;

namespace Ekr.Repository.Contracts.DataMaster.Pegawai
{
	public interface IPegawaiRepository
	{
		Task<GridResponse<Tbl_MappingNIK_Pegawai>> LoadManageData(PegawaiVM req);
		Task<Tbl_MappingNIK_Pegawai> Get(int Id);
		Task<Tbl_MappingNIK_Pegawai> GetByNpp(string Npp);
		Task<Tbl_MappingNIK_Pegawai> GetByNIk(string Nik);
		Task<Tbl_MappingNIK_Pegawai> Create(Tbl_MappingNIK_Pegawai req);
		Task<Tbl_MappingNIK_Pegawai> Update(Tbl_MappingNIK_Pegawai req);
		Task<int> Delete(int Id);
	}
}
