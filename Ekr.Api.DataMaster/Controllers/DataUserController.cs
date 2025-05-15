using Ekr.Api.DataMaster.Filters;
using Ekr.Auth;
using Ekr.Business.Contracts.DataMaster;
using Ekr.Core.Constant;
using Ekr.Core.Entities;
using Ekr.Core.Entities.DataKTP;
using Ekr.Core.Entities.DataMaster.User;
using Ekr.Core.Entities.DataMaster.UserFinger;
using Ekr.Repository.Contracts.DataMaster.MasterAplikasi;
using Ekr.Repository.Contracts.DataMaster.MasterTypeJari;
using Ekr.Repository.Contracts.DataMaster.SystemParameters;
using Ekr.Repository.Contracts.DataMaster.User;
using Ekr.Repository.Contracts.DataMaster.UserFinger;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using System.Web;

namespace Ekr.Api.DataMaster.Controllers
{
    [Route("dm/user")]
    [ApiController]
    public class DataUserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly ISysParameterRepository _sysParameterRepository;
        private readonly IMasTypeJariRepository _MasTypeJariRepository;
        private readonly IUserFingerRepository _userFingerRepository;
        private readonly IMasterAplikasiRepository _masterAplikasiRepository;
        private readonly IUserDataService _userDataService;
        private readonly IConfiguration _config;

        public DataUserController(IUserRepository userRepository,
            ISysParameterRepository sysParameterRepository,
            IMasTypeJariRepository masTypeJariRepository,
            IUserFingerRepository userFingerRepository,
            IMasterAplikasiRepository masterAplikasiRepository,
            IUserDataService userDataService,
            IConfiguration config)
        {
            _userRepository = userRepository;
            _sysParameterRepository = sysParameterRepository;
            _MasTypeJariRepository = masTypeJariRepository;
            _userFingerRepository = userFingerRepository;
            _masterAplikasiRepository = masterAplikasiRepository;
            _userDataService = userDataService;
            _config = config;
        }


        #region Get
        /// <summary>
        /// Untuk Load ALL Data User
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("GetAll")]
        [ProducesResponseType(typeof(GridResponse<UserResponseVM>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<GridResponse<UserResponseVM>> GetAll([FromBody] UserFilter req)
        {
            string authorization = HttpContext.Request.Headers["Authorization"];

            if (string.IsNullOrWhiteSpace(authorization))
            {
                return await _userRepository.GridGetAll(req, 1, 1);
            }

            var token = authorization.Split(" ")[1];

            var claims = TokenManager.GetPrincipal(token);

            return await _userRepository.GridGetAll(req, int.Parse(claims.UnitId),
                int.Parse(claims.RoleUnitId == "x"? claims.RoleId : claims.RoleUnitId));
        }


        /// <summary>
        /// Untuk Load Data User Base on Id
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpGet("GetById")]
        [ProducesResponseType(typeof(UserResponseVM), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<UserResponseVM> GetById([FromQuery] int Id)
        {
            var result = await _userRepository.GetDataById(Id);
            var res = result.Data.FirstOrDefault();

            return res;
        }

        /// <summary>
        /// Untuk Get Data Enroll Finger
        /// </summary>
        /// <param name="UserId"></param>
        /// <returns></returns>
        [HttpGet("GetEnrollFinger")]
        [ProducesResponseType(typeof(GridResponse<UserFingerVM>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<GridResponse<UserFingerVM>> GetEnrollFinger([FromQuery] int UserId)
        {
            return await _userRepository.GetDataUserFinger(UserId);
        }

        /// <summary>
        /// Untuk Get Data Role
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet("GetDataRole")]
        [ProducesResponseType(typeof(ServiceResponse<List<RoleUserVM>>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<List<RoleUserVM>>> GetDataRole([FromQuery] RoleUserReqVM req)
        {
            var data = new List<RoleUserVM>();
            req.ListDataRole = HttpUtility.UrlDecode(req.ListDataRole);
            int no = 0;

            if (req.State == "create")
            {
                List<string> SplitAllData = req.ListDataRole.Split('~').ToList();
                if (SplitAllData.Count > 0 && req.ListDataRole != "")
                {
                    foreach (var item in SplitAllData)
                    {
                        no++;
                        string[] SplitDataRole = item.Split('|');
                        var SplitApp = SplitDataRole[6].Split(',');
                        foreach (var IdMA in SplitApp)
                        {
                            var Id = int.Parse(IdMA);
                            var _listMasterAplikasi = await _masterAplikasiRepository.GetMasterAplikasiById(Id);

                            RoleUserVM DataRole = new()
                            {
                                Id = no,
                                Role_Id = int.Parse(SplitDataRole[0].ToString()),
                                Role_Name = SplitDataRole[1].ToString(),
                                RoleDivisiId = int.Parse(SplitDataRole[2].ToString()),
                                Unit_Name = SplitDataRole[3].ToString(),
                                Tanggal = SplitDataRole[4].ToString(),
                                Status_Role_Name = SplitDataRole[5].ToString() == "1" ? "PJB" : "PGS",
                                AppName = _listMasterAplikasi.Nama
                            };
                            data.Add(DataRole);
                        }
                    }
                }
                else
                {
                    string Date = DateTime.Now.ToString("yyyy-MM-dd");
                    data = await _userRepository.GridGetAllRole(req.UserId, Date);
                }
            }

            return new ServiceResponse<List<RoleUserVM>>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Sukses!",
                Data = data
            };
        }

        /// <summary>
        /// Untuk Get Data Pegawai Demografi
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("GetDataPegawaiDemografi")]
        [ProducesResponseType(typeof(ServiceResponse<GridResponse<Tbl_DataKTP_Demografis>>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<GridResponse<Tbl_DataKTP_Demografis>>> GetDataPegawaiDemografi([FromBody] PegawaiDemografi req)
        {
            var res = await _userRepository.GetDataPegawaiDemografi(req);

            return new ServiceResponse<GridResponse<Tbl_DataKTP_Demografis>>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Sukses!",
                Data = res
            };
        }

        /// <summary>
        /// Get list model role
        /// </summary>
        /// <param name="listDataRoles"></param>
        /// <returns></returns>
        [HttpGet("GetModelRole")]
        [ProducesResponseType(typeof(List<RoleUserVM>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public List<RoleUserVM> GetModelRole([FromQuery] string listDataRoles)
        {
            return _userDataService.GetModelRole(listDataRoles);
        }
        #endregion

        /// <summary>
        /// Untuk Create Data User Pegawai
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        #region Create
        [HttpPost("create")]
        [ProducesResponseType(typeof(ServiceResponse<UserVM>), 201)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<UserVM>> Create([FromBody] UserVM req)
        {
            var isEncrypt = _config.GetValue<bool>("isEncrypt");
            string authorization = HttpContext.Request.Headers["Authorization"];
            int UnitId = 0;
            if (authorization != null)
            {
                var token = authorization.Split(" ")[1];
                var claims = TokenManager.GetPrincipal(token);
                req.Created_By = claims.PegawaiId;
                UnitId = int.Parse(claims.UnitId);
            }

            req.ListDataRole = HttpUtility.UrlDecode(req.ListDataRole);

            var _dataPeg = await _userRepository.GetDataPegawai(req.Nik);

            if(_dataPeg != null)
            {
                if(_dataPeg.IsActive == false)
                {
                    return new ServiceResponse<UserVM>
                    {
                        Status = (int)ServiceResponseStatus.ERROR,
                        Message = "Data pegawai sudah diinputkan sebelumnya, dengan status tidak aktif!",
                        Data = req
                    };
                }
                else if(UnitId == _dataPeg.Unit_Id)
                {
                    return new ServiceResponse<UserVM>
                    {
                        Status = (int)ServiceResponseStatus.ERROR,
                        Message = "Data pegawai sudah diinputkan sebelumnya Pada Unit Anda!",
                        Data = req
                    };
                }
                else if (UnitId != _dataPeg.Unit_Id)
                {
                    return new ServiceResponse<UserVM>
                    {
                        Status = (int)ServiceResponseStatus.ERROR,
                        Message = "Data pegawai sudah diinputkan sebelumnya Pada Unit Berbeda dengan Anda, Silahkan Lakukan Mutasi Pegawai atau Koordinasi Lebih Lanjut!",
                        Data = req
                    };
                }
                else
                {
                    return new ServiceResponse<UserVM>
                    {
                        Status = (int)ServiceResponseStatus.ERROR,
                        Message = "Data pegawai sudah diinputkan sebelumnya!",
                        Data = req
                    };
                }
            }
            _ = await _userRepository.CreateTblPegawai(req);

            var _dataPegs = await _userRepository.GetDataPegawai(req.Nik);

            _ = await _userRepository.CreateTblUser(req, _dataPegs.Id, isEncrypt);

            _ = await _userRepository.CreateTblRolePegawai(req, _dataPegs.Id);

            return new ServiceResponse<UserVM>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = req
            };
        }

        /// <summary>
        /// Untuk Create Data Enroll Finger
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("createEnrollFinger")]
        [ProducesResponseType(typeof(ServiceResponse<UserFingerReqVM>), 201)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<UserFingerReqVM>> CreateEnrollFinger([FromBody] UserFingerReqVM req)
        {
            string authorization = HttpContext.Request.Headers["Authorization"];
            if (authorization != null)
            {
                var token = authorization.Split(" ")[1];
                var claims = TokenManager.GetPrincipal(token);
                req.PegawaiId = int.Parse(claims.PegawaiId);
            }
            //Enroll Data Jari Kiri
            if (req.File != null)
            {
                byte[] imageBytes = Convert.FromBase64String(req.File);
                string JamServer = DateTime.Now.ToString("yyyyMMdd_HHmmss");

                var _pathFolder = await _sysParameterRepository.GetPathFolder("PathFingerUser");
                var _typeJari = await _MasTypeJariRepository.GetTypeJari(req.TypeFingerId);

                var dataPegawai = await _userRepository.GetDataPegawaiById((int)req.PegawaiId);

                if(dataPegawai!= null)
                {
                    var _pathFolderPhotoFinger = _sysParameterRepository.GetPathFolder("Finger");
                    var _subPathFolderPhotoFinger = _pathFolder + "/" + _pathFolderPhotoFinger + "/" + dataPegawai.Nik + "_" + dataPegawai.Nama + "/";
                    if (!Directory.Exists(_subPathFolderPhotoFinger))
                    {
                        Directory.CreateDirectory(_subPathFolderPhotoFinger);
                    }

                    string fileName = "PhotoFinger_" + _typeJari.Nama + "_" + JamServer + ".jpg";
                    string filePath = _subPathFolderPhotoFinger + fileName;
                    System.IO.File.WriteAllBytes(filePath, imageBytes);

                    var CekDataFinger = _userFingerRepository.GetDataFinger((int)req.PegawaiId, (int)req.TypeFingerId);

                    using var trx = new TransactionScope();
                    var DataInsert = new TblPegawaiFinger
                    {
                        PegawaiId = req.PegawaiId,
                        TypeFingerId = req.TypeFingerId,
                        FileName = fileName,
                        Path = filePath,
                        CreatedById = req.PegawaiId,
                        CreatedTime = DateTime.Now,
                        IsActive = true,
                        IsDeleted = false
                    };
                    if (CekDataFinger != null)
                    {
                        _ = _userFingerRepository.Update(CekDataFinger.Id);
                        _ = _userFingerRepository.Create(DataInsert);
                    }
                    else
                    {
                        _ = _userFingerRepository.Create(DataInsert);
                    }
                    trx.Complete();
                }
                else
                {
                    return new ServiceResponse<UserFingerReqVM>
                    {
                        Status = (int)ServiceResponseStatus.EMPTY_PARAMETER,
                        Message = "Pegawai Not Found!",
                        Data = req
                    };
                }
            }
            else
            {
                return new ServiceResponse<UserFingerReqVM>
                {
                    Status = (int)ServiceResponseStatus.EMPTY_PARAMETER,
                    Message = "File Cant be Null!",
                    Data = req
                };
            }

            return new ServiceResponse<UserFingerReqVM>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = req
            };
        }

        /// <summary>
        /// Untuk Create Data Mutasi Pegawai
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("CreateMutationUser")]
        [ProducesResponseType(typeof(ServiceResponse<UserMutateVM>), 201)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<TblPegawai>> CreateMutationUser([FromBody] UserMutateVM req)
        {
            string authorization = HttpContext.Request.Headers["Authorization"];
            int CreateBy = 0;
            if (authorization != null)
            {
                var token = authorization.Split(" ")[1];
                var claims = TokenManager.GetPrincipal(token);
                CreateBy = int.Parse(claims.PegawaiId);
            }

            var Pegawai = await _userRepository.GetDataPegawaiById(req.PegawaiId);
            if(Pegawai != null)
            {
                var dataLama = await _userRepository.GetDataPegawaiMutasi(req.PegawaiId);
                int UnitIdLama = (int)Pegawai.Unit_Id;
                if (dataLama != null)
                {
                    _ = await _userRepository.UpdateMutationUser(req, CreateBy);
                    _ = await _userRepository.CreateMutationUser(req, CreateBy);
                }
                else
                {
                    var pegawai = await _userRepository.CreateMutationUser(req, CreateBy);
                }

                var UpdateUser = new UserVM
                {
                    Id = Pegawai.Id,
                    UnitId = req.UnitId,
                    RoleId = Pegawai.Role_Id,
                    IdJenisKelamin = Pegawai.Id_JenisKelamin,
                    Nik = Pegawai.Nik,
                    Nama = Pegawai.Nama,
                    Alamat = Pegawai.Alamat,
                    Email = Pegawai.Email,
                    Lastlogin = Pegawai.Lastlogin,
                    Images = Pegawai.Images,
                    TanggalLahir = Pegawai.Tanggal_Lahir.ToString(),
                    NoHp = Pegawai.No_HP,
                    Updated_Time = DateTime.Now.ToString(),
                    Updated_By = CreateBy.ToString()
                };

                _ = await Update(UpdateUser);

                var _new = await _userRepository.GetDataPegawaiById(req.PegawaiId);

                return new ServiceResponse<TblPegawai>
                {
                    Status = (int)ServiceResponseStatus.SUKSES,
                    Message = "Success",
                    Data = _new
                };
            }
            else
            {
                return new ServiceResponse<TblPegawai>
                {
                    Status = (int)ServiceResponseStatus.Data_Empty,
                    Message = "Pegawai Tidak Ditemukan",
                    Data = Pegawai
                };
            }


        }
        #endregion

        #region Update
        /// <summary>
        /// Untuk Update Data User Pegawai
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPut("update")]
        [ProducesResponseType(typeof(ServiceResponse<UserVM>), 201)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<UserVM>> Update([FromBody] UserVM req)
        {
            var isEncrypt = _config.GetValue<bool>("isEncrypt");
            string authorization = HttpContext.Request.Headers["Authorization"];
            if (authorization != null)
            {
                var token = authorization.Split(" ")[1];
                var claims = TokenManager.GetPrincipal(token);
                req.Updated_By = claims.PegawaiId;
            }

            req.ListDataRole = HttpUtility.UrlDecode(req.ListDataRole);

            var _dataPeg = await _userRepository.GetDataPegawaiById(req.Id);

            if (_dataPeg != null)
            {
                if (_dataPeg.Role_Id != req.RoleId || _dataPeg.Unit_Id != req.UnitId)
                {
                    _ = await _userRepository.UpdateTblRolePegawai(req, req.Id);
                    _ = await _userRepository.CreateTblRolePegawai(req, req.Id);
                }
                _ = await _userRepository.UpdateTblPegawai(req);
                _ = await _userRepository.UpdateTblUser(req, req.Id, isEncrypt);

                return new ServiceResponse<UserVM>
                {
                    Status = (int)ServiceResponseStatus.SUKSES,
                    Message = "Success",
                    Data = req
                };
            }
            else
            {
                return new ServiceResponse<UserVM>
                {
                    Status = (int)ServiceResponseStatus.ERROR,
                    Message = "No Data Found!",
                    Data = req
                };
            }
        }

        /// <summary>
        /// Untuk Update Data Role Pegawai
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPut("updateDataRole")]
        [ProducesResponseType(typeof(ServiceResponse<RoleUserUpdateReqVM>), 201)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<RoleUserUpdateReqVM>> UpdateDataRole([FromBody] RoleUserUpdateReqVM req)
        {
            var appItems = req.Apps.Split(',');

            _ = _userRepository.UpdateRolePegawai(req, appItems);

            return new ServiceResponse<RoleUserUpdateReqVM>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = req
            };
        }
        #endregion

        #region Delete
        /// <summary>
        /// Untuk Delete Data User Pegawai
        /// </summary>
        /// <param name="Ids"></param>
        /// <returns></returns>
        [HttpDelete("delete")]
        [ProducesResponseType(typeof(ServiceResponse<bool>), 201)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<bool>> Delete([FromQuery] String Ids)
        {
            int[] confirmedDeleteId = Ids.Split(',').Select(int.Parse).ToArray();
            foreach (var item in confirmedDeleteId)
            {
                _ = _userRepository.Delete(item);
            }
            return new ServiceResponse<bool>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = true
            };
        }

        /// <summary>
        /// Untuk Delete Data Role Pegawai
        /// </summary>
        /// <param name="Ids"></param>
        /// <returns></returns>
        [HttpDelete("deleteRole")]
        [ProducesResponseType(typeof(ServiceResponse<bool>), 201)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<bool>> DeleteRole([FromQuery] String Ids)
        {
            int[] confirmedDeleteId = Ids.Split(',').Select(int.Parse).ToArray();
            foreach (var item in confirmedDeleteId)
            {
                _ = _userRepository.DeleteRole(item);
            }
            return new ServiceResponse<bool>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = true
            };
        }
        #endregion
    }
}
