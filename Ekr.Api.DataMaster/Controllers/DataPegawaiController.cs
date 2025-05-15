using Ekr.Api.DataMaster.Filters;
using Ekr.Core.Constant;
using Ekr.Core.Entities;
using Ekr.Core.Entities.DataMaster.Pegawai;
using Ekr.Core.Entities.Enrollment;
using Ekr.Repository.Contracts.DataMaster.Pegawai;
using Ekr.Repository.Contracts.Enrollment;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace Ekr.Api.DataMaster.Controllers
{
	/// <summary>
	/// api Data Pegawai
	/// </summary>
	[Route("dm/pegawai")]
	[ApiController]
	public class DataPegawaiController : ControllerBase
	{
		private readonly IConfiguration _config;
		private readonly IPegawaiRepository _repo;
		private readonly IEnrollmentKTPRepository _enrollmentKTPRepository;
		public DataPegawaiController(IConfiguration config, IPegawaiRepository repo, IEnrollmentKTPRepository enrollmentKTPRepository)
		{
			_config = config;
			_repo = repo;
			_enrollmentKTPRepository = enrollmentKTPRepository;
		}

		/// <summary>
		/// Untuk Get All Data Nik Pegawai
		/// </summary>
		/// <param name="req"></param>
		/// <returns></returns>
		[HttpPost("getall")]
		[ProducesResponseType(typeof(GridResponse<Tbl_MappingNIK_Pegawai>), 200)]
		[ProducesResponseType(500)]
		[LogActivity(Keterangan = "Data Master")]
		public async Task<GridResponse<Tbl_MappingNIK_Pegawai>> GetAll([FromBody] PegawaiVM req)
		{
			_ = new GridResponse<Tbl_MappingNIK_Pegawai>();
			GridResponse<Tbl_MappingNIK_Pegawai> data;
			try
			{
				data = await _repo.LoadManageData(req);
			}
			catch (System.Exception)
			{
				data = new GridResponse<Tbl_MappingNIK_Pegawai>()
				{
					Data = null,
					Count = 0
				};
			}

			return data;
		}

		/// <summary>
		/// Untuk Get Data By Id
		/// </summary>
		/// <param name="Id"></param>
		/// <returns></returns>
		[HttpGet("getById")]
		[ProducesResponseType(typeof(ServiceResponse<Tbl_MappingNIK_Pegawai>), 201)]
		[ProducesResponseType(500)]
		[LogActivity(Keterangan = "Data Master")]
		public async Task<ServiceResponse<Tbl_MappingNIK_Pegawai>> Get([FromQuery] int Id)
		{
			var res = await _repo.Get(Id);

			return new ServiceResponse<Tbl_MappingNIK_Pegawai>
			{
				Status = (int)ServiceResponseStatus.SUKSES,
				Message = "Success",
				Data = res
			};
		}

		/// <summary>
		/// Untuk Create Data Nik Pegawai
		/// </summary>
		/// <param name="req"></param>
		/// <returns></returns>
		[HttpPost("create")]
		[ProducesResponseType(typeof(ServiceResponse<Tbl_MappingNIK_Pegawai>), 201)]
		[ProducesResponseType(500)]
		[LogActivity(Keterangan = "Data Master")]
		public async Task<ServiceResponse<Tbl_MappingNIK_Pegawai>> Create([FromBody] Tbl_MappingNIK_Pegawai req)
		{
			var Nik = await _repo.GetByNIk(req.NIK);
			if(Nik != null)
			{
				return new ServiceResponse<Tbl_MappingNIK_Pegawai>
				{
					Status = (int)ServiceResponseStatus.ERROR,
					Message = "Nik Sudah ada",
					Data = null
				};
			}
			var Npp = await _repo.GetByNpp(req.Npp);
			if (Npp != null)
			{
				return new ServiceResponse<Tbl_MappingNIK_Pegawai>
				{
					Status = (int)ServiceResponseStatus.ERROR,
					Message = "Npp Sudah ada",
					Data = null
				};
			}

			var res = await _repo.Create(req);

			return new ServiceResponse<Tbl_MappingNIK_Pegawai>
			{
				Status = (int)ServiceResponseStatus.SUKSES,
				Message = "Success",
				Data = res
			};
		}

		/// <summary>
		/// Untuk Update Data Nik Pegawai
		/// </summary>
		/// <param name="req"></param>
		/// <returns></returns>
		[HttpPost("update")]
		[ProducesResponseType(typeof(ServiceResponse<Tbl_MappingNIK_Pegawai>), 201)]
		[ProducesResponseType(500)]
		[LogActivity(Keterangan = "Data Master")]
		public async Task<ServiceResponse<Tbl_MappingNIK_Pegawai>> Update([FromBody] Tbl_MappingNIK_Pegawai req)
		{
			var res = await _repo.Update(req);

			if(res == null)
			{
				return new ServiceResponse<Tbl_MappingNIK_Pegawai>
				{
					Status = (int)ServiceResponseStatus.ERROR,
					Message = "Npp atau NIK sudah ada",
					Data = res
				};
			}

			return new ServiceResponse<Tbl_MappingNIK_Pegawai>
			{
				Status = (int)ServiceResponseStatus.SUKSES,
				Message = "Success",
				Data = res
			};
		}


		/// <summary>
		/// Untuk delete Nik Pegawai
		/// </summary>
		/// <param name="req"></param>
		/// <returns></returns>
		[HttpPost("delete")]
		[ProducesResponseType(typeof(ServiceResponse<Tbl_MappingNIK_Pegawai>), 201)]
		[ProducesResponseType(500)]
		[LogActivity(Keterangan = "Data Master")]
		public async Task<ServiceResponse<int>> Delete([FromQuery] int Id)
		{
			var data = await _repo.Get(Id);

			var _ktp = _enrollmentKTPRepository.GetDataDemografisByNik(data.NIK);
			if(_ktp != null)
			{
				return new ServiceResponse<int>
				{
					Status = (int)ServiceResponseStatus.ERROR,
					Message = "Nik sudah di enroll"
				};
			}

			var res = await _repo.Delete(Id);

			return new ServiceResponse<int>
			{
				Status = (int)ServiceResponseStatus.SUKSES,
				Message = "Success",
				Data = res
			};
		}
	}
}
