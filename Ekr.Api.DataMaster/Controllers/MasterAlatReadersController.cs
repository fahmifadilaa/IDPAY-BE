using Ekr.Api.DataMaster.Filters;
using Ekr.Auth;
using Ekr.Business.Contracts.DataMaster;
using Ekr.Core.Constant;
using Ekr.Core.Entities;
using Ekr.Core.Entities.DataMaster.AlatReader;
using Ekr.Core.Entities.DataMaster.User;
using Ekr.Core.Entities.DataMaster.Utility.ViewModel;
using Ekr.Repository.Contracts.DataMaster.AlatReader;
using Ekr.Repository.Contracts.DataMaster.User;
using Ekr.Repository.Contracts.DataMaster.Utility;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace Ekr.Api.DataMaster.Controllers
{
    [Route("dm/AlatReader")]
    [ApiController]
    public class MasterAlatReadersController : ControllerBase
    {
        private readonly IAlatReaderRepository _alatReaderRepository;
        private readonly IAppVersionService _appVersionService;
        private readonly IConfiguration _configuration;
        private readonly IUtilityRepository _utilityRepository;
        private readonly IUserRepository _userRepository;

        public MasterAlatReadersController(IAlatReaderRepository alatReaderRepository,
            IAppVersionService appVersionService, IUtilityRepository utilityRepository,
            IUserRepository userRepository)
        {
            _alatReaderRepository = alatReaderRepository;
            _appVersionService = appVersionService;
            _utilityRepository = utilityRepository;
            _userRepository = userRepository;
        }

        /// <summary>
        /// Untuk load all Data Master Alat Reader
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        #region Get
        [HttpPost("GetAll")]
        [ProducesResponseType(typeof(GridResponse<TblMasterAlatReaderVM>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public Task<GridResponse<TblMasterAlatReaderVM>> GetAll([FromBody] MasterAlatReaderFilter req)
        {
            var data = _alatReaderRepository.GridGetAll(req);

            return data;
        }

        /// <summary>
        /// Untuk Get Master Alat Reader By UID
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        [HttpGet("GetDataByUID")]
        [ProducesResponseType(typeof(TblMasterAlatReaderVM), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public Task<TblMasterAlatReaderVM> GetDataByUID([FromQuery] string uid)
        {
            var result = _alatReaderRepository.GetDataByUID(uid);

            return result;
        }

        /// <summary>
        /// Untuk Get All Version Apps
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("GetAllVersion")]
        [ProducesResponseType(typeof(GridResponse<Tbl_VersionAgentVM>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public Task<GridResponse<Tbl_VersionAgentVM>> GetAllVersion([FromBody] AppsVersionRequestFilter req)
        {
            var result = _alatReaderRepository.GridGetAllVersionApps(req);

            return result;
        }

        /// <summary>
        /// Untuk Get Version By Id
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpGet("GetVersionById")]
        [ProducesResponseType(typeof(ServiceResponse<Tbl_VersionAgent>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<Tbl_VersionAgent>> GetVersionById([FromQuery] int Id)
        {
            var result = await _alatReaderRepository.GetVersionById(Id);

            return new ServiceResponse<Tbl_VersionAgent>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = result
            };
        }

        /// <summary>
        /// To Get Version Apps By Id
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpGet("GetVersionById_v2")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<CheckVersion>> GetVersionByIdV2([FromQuery] int Id)
        {
            if (Id == 0)
            {
                return new ServiceResponse<CheckVersion>
                {
                    Status = (int)ServiceResponseStatus.EMPTY_PARAMETER,
                    Message = "Param Can't be Empty"
                };
            }

            (var fileBase64, var urlPath) = await _appVersionService.GetVersionById(Id);
            if (fileBase64 == null && urlPath == null)
            {
                return new ServiceResponse<CheckVersion>
                {
                    Status = (int)ServiceResponseStatus.Data_Empty,
                    Message = "Data Not Found"
                };
            }

            return new ServiceResponse<CheckVersion>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = new CheckVersion
                {
                    filebase64 = fileBase64,
                    urlDownload = urlPath,
                }
            };
        }

        /// <summary>
        /// Untuk Get Master Alat Reader By UID
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        [HttpGet("GetDataByUID2")]
        [ProducesResponseType(typeof(ServiceResponse<TblMasterAlatReaderVM>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<TblMasterAlatReaderVM>> GetDataByUID2([FromQuery] string uid)
        {
            var result = await _alatReaderRepository.GetDataByUID(uid);
            if(result == null)
            {
                return new ServiceResponse<TblMasterAlatReaderVM>
                {
                    Status = (int)ServiceResponseStatus.Data_Empty,
                    Message = "Data Not Found",
                    Data = null
                };
            }

            return new ServiceResponse<TblMasterAlatReaderVM>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = result
            };
        }

        /// <summary>
        /// To check application version
        /// </summary>
        /// <param name="checkAppsVersionRequest"></param>
        /// <returns></returns>
        [HttpGet("check_version")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<IActionResult> CheckVersion([FromQuery] CheckAppsVersionRequest checkAppsVersionRequest)
        {
            if (checkAppsVersionRequest == null)
            {
                return NotFound();
            }

            (var fileByte, var appsVersion) = await _appVersionService.CheckVersion(checkAppsVersionRequest);
            if(fileByte == null && appsVersion == null)
            {
                return NotFound();
            }

            return File(fileByte, "application/octet-stream", appsVersion + ".exe");
        }


        /// <summary>
        /// To check application version
        /// </summary>
        /// <param name="checkAppsVersionRequest"></param>
        /// <returns></returns>
        [HttpGet("check_version_v2")]
        [ProducesResponseType(200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<CheckVersion>> CheckVersionV2([FromQuery] CheckAppsVersionRequest checkAppsVersionRequest)
        {
            if (checkAppsVersionRequest == null)
            {
                return null;
            }

            (var fileBase64, var urlPath) = await _appVersionService.CheckVersionV2(checkAppsVersionRequest);
            if (fileBase64 == null && urlPath == null)
            {
                return new ServiceResponse<CheckVersion>
                {
                    Status = (int)ServiceResponseStatus.Data_Empty,
                    Message = "Current version is up to date"
                };
            }

            return new ServiceResponse<CheckVersion>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = new CheckVersion
                {
                    filebase64 = fileBase64,
                    urlDownload = urlPath,
                }
            };
        }
        #endregion

        #region Create
        /// <summary>
        /// Untuk Create Master Alat Reader
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("CreateMasterAlatReader")]
        [ProducesResponseType(typeof(ServiceResponse<ReqCreateMasterAlatReader>), 201)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<ReqCreateMasterAlatReader>> CreateMasterAlatReader([FromBody] ReqCreateMasterAlatReader req)
        {
            var _dataAlat = await _alatReaderRepository.GetDataByUID(req.uid);
            if (_dataAlat == null)
            {
                _ = await _alatReaderRepository.CreateAlatMasterReader(req);
            }
            else
            {
                _ = await _alatReaderRepository.UpdateAlatMasterReader(req);
            }

            return new ServiceResponse<ReqCreateMasterAlatReader>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = req
            };
        }

        /// <summary>
        /// Untuk Create Master Alat Reader Log Activity
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("CreateLogActivity")]
        [ProducesResponseType(typeof(ServiceResponse<ReqCreateLogActivity>), 201)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<ReqCreateLogActivity>> CreateLogActivity([FromBody] ReqCreateLogActivity req)
        {
            var _dataAlat = await _alatReaderRepository.GetDataByUID(req.UID);
            var reqUser = new ReqMasterAlatReaderGetByUid
            {
                UID = req.UID,
                pegawaiId = req.PegawaiId,
                unitId = req.UnitId,
                NppPegawai = req.NppPegawai,
                KodeUnit = req.KodeUnit
            };
            var reqAlatReader = new ReqCreateMasterAlatReader
            {
                lastPegawaiId = req.PegawaiId,
                kode = req.kode,
                nama = req.nama,
                unitId = req.UnitId,
                snUnit = req.snUnit,
                noPersoSam = req.noPersoSam,
                noKartu = req.noKartu,
                pcid = req.pcid,
                confiq = req.confiq,
                uid = req.UID,
                status = req.status,
                lastIp = req.LastIP,
                lastPingIp = req.lastPingIp,
                latitude = req.latitude,
                longitude = req.longitude,
                isActive = req.isActive,
                isDelete = req.isDelete,
                lastNpp = req.NppPegawai,
                lastUnitCode = req.KodeUnit
            };

            if (_dataAlat == null)
            {
                return new ServiceResponse<ReqCreateLogActivity>
                {
                    Status = (int)ServiceResponseStatus.ERROR,
                    Message = "UID Not Found, Insert it to Master First!",
                    Data = req
                };
            }
            else
            {
                //var checkUser = await _alatReaderRepository.GetAlatMasterReaderUserByUIDPegawaiId(req.UID, req.NppPegawai);
                var checkUser = await _alatReaderRepository.GetAlatMasterReaderUserByUIDPegawaiIdMax(req.UID, req.NppPegawai);

                if (checkUser == null)
                {
                    _ = await _alatReaderRepository.CreateAlatMasterReaderUser(reqUser);
                }
                else
                {
                    _ = await _alatReaderRepository.UpdateAlatMasterReaderFromLogActivity(reqAlatReader);
                    //_ = await _alatReaderRepository.UpdateAlatMasterReaderUserByUID(req.UID);
                    //_ = await _alatReaderRepository.UpdateAlatMasterReaderUserByID(checkUser.Id);
                }

                _ = await _alatReaderRepository.CreateLogActivity(req);

                return new ServiceResponse<ReqCreateLogActivity>
                {
                    Status = (int)ServiceResponseStatus.SUKSES,
                    Message = "Success",
                    Data = req
                };
            }
        }

        /// <summary>
        /// Untuk Create Master Alat Reader User
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("CreateUser")]
        [ProducesResponseType(typeof(ServiceResponse<ReqMasterAlatReaderGetByUid>), 201)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<ReqMasterAlatReaderGetByUid>> CreateUser([FromBody] ReqMasterAlatReaderGetByUid req)
        {

            var _dataAlat = await _alatReaderRepository.GetDataByUID(req.UID);
            if (_dataAlat == null)
            {
                return new ServiceResponse<ReqMasterAlatReaderGetByUid>
                {
                    Status = (int)ServiceResponseStatus.ERROR,
                    Message = "UID Not Found, Insert it to Master First!",
                    Data = req
                };
            }
            else
            {
                var checkUser = await _alatReaderRepository.GetAlatMasterReaderUserByUID(req.UID);
                if (checkUser == null || (checkUser.UID != req.UID && checkUser.PegawaiId != req.pegawaiId))
                {
                    var reqUser = new ReqMasterAlatReaderGetByUid
                    {
                        UID = req.UID,
                        pegawaiId = req.pegawaiId,
                        unitId = req.unitId
                    };

                    _ = await _alatReaderRepository.CreateAlatMasterReaderUser(reqUser);
                }
                else
                {
                    _ = await _alatReaderRepository.UpdateAlatMasterReaderUserByUID(req.UID);
                }


                return new ServiceResponse<ReqMasterAlatReaderGetByUid>
                {
                    Status = (int)ServiceResponseStatus.SUKSES,
                    Message = "Success",
                    Data = req
                };
            }
        }

        /// <summary>
        /// Untuk Create Master Alat Reader Log Connection
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("CreateLogConnection")]
        [ProducesResponseType(typeof(ServiceResponse<ReqCreateLogConnection>), 201)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<ReqCreateLogConnection>> CreateLogConnection([FromBody] ReqCreateLogConnection req)
        {

            var _dataAlat = await _alatReaderRepository.GetDataByUID(req.UID);
            if (_dataAlat == null)
            {
                return new ServiceResponse<ReqCreateLogConnection>
                {
                    Status = (int)ServiceResponseStatus.ERROR,
                    Message = "UID Not Found, Insert it to Master First!",
                    Data = req
                };
            }
            else
            {
                _ = await _alatReaderRepository.CreateAlatMasterReaderLogConnection(req);
                return new ServiceResponse<ReqCreateLogConnection>
                {
                    Status = (int)ServiceResponseStatus.SUKSES,
                    Message = "Success",
                    Data = req
                };
            }
        }

        /// <summary>
        /// Untuk Create Master Alat Reader Log Error
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("CreateLogError")]
        [ProducesResponseType(typeof(ServiceResponse<ReqCreateMasterAlatReaderLogError>), 201)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<ReqCreateMasterAlatReaderLogError>> CreateLogError([FromBody] ReqCreateMasterAlatReaderLogError req)
        {

            var _dataAlat = await _alatReaderRepository.GetDataByUID(req.UID);
            if (_dataAlat == null)
            {
                return new ServiceResponse<ReqCreateMasterAlatReaderLogError>
                {
                    Status = (int)ServiceResponseStatus.ERROR,
                    Message = "UID Not Found, Insert it to Master First!",
                    Data = req
                };
            }
            else
            {
                _ = await _alatReaderRepository.CreateLogError(req);
                return new ServiceResponse<ReqCreateMasterAlatReaderLogError>
                {
                    Status = (int)ServiceResponseStatus.SUKSES,
                    Message = "Success",
                    Data = req
                };
            }
        }

        /// <summary>
        /// To upload new version of agent apps
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("upload-apps")]
        [ProducesResponseType(typeof(ServiceResponse<string>), 201)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<string>> UploadAppVersion([FromBody] UploadAppsReq req)
        {

            if (req == null)
            {
                return new ServiceResponse<string>
                {
                    Status = (int)ServiceResponseStatus.EMPTY_PARAMETER,
                    Message = nameof(ServiceResponseStatus.EMPTY_PARAMETER)
                };
            }
            // check version
            var apps = _utilityRepository.GetLatestAppsVersion();
            if (apps.Version >= req.Version)
            {
                return new ServiceResponse<string>
                {
                    Message = nameof(ServiceResponseStatus.ERROR),
                    Status = (int)ServiceResponseStatus.ERROR,
                    Data = "Versi yang Diinput Kurang dari atau Sama Dengan Versi: " + apps.Version
                };
            }

            string authorization = HttpContext.Request.Headers["Authorization"];

            if (string.IsNullOrWhiteSpace(authorization))
            {
                await _appVersionService.UploadApps(req, 24, "tes", "0001");

                return new ServiceResponse<string>
                {
                    Message = nameof(ServiceResponseStatus.SUKSES),
                    Status = (int)ServiceResponseStatus.SUKSES
                };
            }

            var token = authorization.Split(" ")[1];

            var claims = TokenManager.GetPrincipal(token);

            await _appVersionService.UploadApps(req, int.Parse(claims.PegawaiId), claims.NIK, claims.KodeUnit);

            return new ServiceResponse<string>
            {
                Message = nameof(ServiceResponseStatus.SUKSES),
                Status = (int)ServiceResponseStatus.SUKSES
            };
        }

        /// <summary>
        /// To upload new version of agent apps new
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("upload-apps-new")]
        [ProducesResponseType(typeof(ServiceResponse<string>), 201)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<string>> UploadAppVersionNew([FromForm] UploadAppsReq req)
        {

            if (req == null)
            {
                return new ServiceResponse<string>
                {
                    Status = (int)ServiceResponseStatus.EMPTY_PARAMETER,
                    Message = nameof(ServiceResponseStatus.EMPTY_PARAMETER)
                };
            }
            // check version
            var apps = _utilityRepository.GetLatestAppsVersion();
            if (apps.Version >= req.Version)
            {
                return new ServiceResponse<string>
                {
                    Message = nameof(ServiceResponseStatus.ERROR),
                    Status = (int)ServiceResponseStatus.ERROR,
                    Data = "Versi yang Diinput Kurang dari atau Sama Dengan Versi: " + apps.Version
                };
            }

            string authorization = HttpContext.Request.Headers["Authorization"];

            if (string.IsNullOrWhiteSpace(authorization))
            {
                await _appVersionService.UploadApps(req, 24, "tes", "0001");

                return new ServiceResponse<string>
                {
                    Message = nameof(ServiceResponseStatus.SUKSES),
                    Status = (int)ServiceResponseStatus.SUKSES
                };
            }

            var token = authorization.Split(" ")[1];

            var claims = TokenManager.GetPrincipal(token);

            await _appVersionService.UploadApps(req, int.Parse(claims.PegawaiId), claims.NIK, claims.KodeUnit);

            return new ServiceResponse<string>
            {
                Message = nameof(ServiceResponseStatus.SUKSES),
                Status = (int)ServiceResponseStatus.SUKSES
            };
        }
        #endregion

        #region Update
        /// <summary>
        /// Untuk Update Status Master Alat Reader
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("UpdateMasterAlatReader")]
        [ProducesResponseType(typeof(ServiceResponse<ReqUpdateMasterAlatReader>), 201)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<ReqUpdateMasterAlatReader>> UpdateMasterAlatReader([FromBody] ReqUpdateMasterAlatReader req)
        {
            req.UpdatedBy_Id = 24;

            if (req.uid == null || req.uid == "")
            {
                return new ServiceResponse<ReqUpdateMasterAlatReader>
                {
                    Status = (int)ServiceResponseStatus.EMPTY_PARAMETER,
                    Message = "False",
                    Data = req
                };
            }
            else
            {
                _ = await _alatReaderRepository.UpdateStatusAlatMasterReader(req);
            }

            return new ServiceResponse<ReqUpdateMasterAlatReader>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = req
            };
        }

        /// <summary>
        /// Untuk Update Status Manifest Master Alat Reader
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("UpdateStatusManifestAlatReader")]
        [ProducesResponseType(typeof(ServiceResponse<ReqUpdateMasterAlatReader>), 201)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<UpdateStatusManifestAlatReader>> UpdateStatusManifestAlatReader([FromBody] UpdateStatusManifestAlatReader req)
        {
            var _pegawai = new TblPegawai();

            if(req == null) return new ServiceResponse<UpdateStatusManifestAlatReader>
            {
                Status = (int)ServiceResponseStatus.EMPTY_PARAMETER,
                Message = "False",
                Data = req
            };

            if (string.IsNullOrEmpty(req.uid))
            {
                return new ServiceResponse<UpdateStatusManifestAlatReader>
                {
                    Status = (int)ServiceResponseStatus.EMPTY_PARAMETER,
                    Message = "False",
                    Data = req
                };
            }

            if(!string.IsNullOrEmpty(req.npp))
			{
                _pegawai = await _userRepository.GetDataPegawai(req.npp);
            }
			else
			{
                _pegawai.Id = 0;
			}

            _ = await _alatReaderRepository.UpdateStatusManifestAlatReader(req, _pegawai.Id);

            return new ServiceResponse<UpdateStatusManifestAlatReader>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = req
            };
        }
        #endregion

        #region delete
        /// <summary>
        /// Untuk Delete Apps Version
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("DeleteAppsVersion")]
        [ProducesResponseType(typeof(ServiceResponse<string>), 201)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<string>> DeleteAppsVersion([FromBody] ReqAppsVersionViewModels req)
        {
            string authorization = HttpContext.Request.Headers["Authorization"];
            string updateBy;
            if (string.IsNullOrWhiteSpace(authorization))
            {
                updateBy = "24";
            }
            else
            {
                var token = authorization.Split(" ")[1];
                var claims = TokenManager.GetPrincipal(token);
                updateBy = claims.PegawaiId;
            }

            var res = await _alatReaderRepository.DeleteApps(req.Id, updateBy);

            return new ServiceResponse<string>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = res
            };
        }
        #endregion
    }
}
