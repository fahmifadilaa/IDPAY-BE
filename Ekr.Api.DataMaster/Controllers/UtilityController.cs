using Ekr.Core.Constant;
using Ekr.Core.Entities;
using Ekr.Core.Entities.DataMaster.Lookup;
using Ekr.Core.Entities.DataMaster.Utility.ViewModel;
using Ekr.Core.Entities.DataMaster.Utility;
using Ekr.Repository.Contracts.DataMaster.Utility;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using Ekr.Core.Entities.DataMaster.Menu.Entity;
using Ekr.Core.Entities.DataMaster.MasterAplikasi;
using Ekr.Core.Entities.DataMaster.Menu.ViewModel;
using Ekr.Auth;
using System.Linq;
using Newtonsoft.Json;
using Ekr.Business.Contracts.DataMaster;
using Ekr.Api.DataMaster.Filters;
using System;
using Microsoft.Extensions.Configuration;
using Ekr.Repository.Contracts.DataMaster.Unit;
using Ekr.Core.Entities.DataMaster.User;
using FluentFTP.Helpers;

namespace Ekr.Api.DataMaster.Controllers
{
    /// <summary>
    /// api Utility
    /// </summary>
    [Route("dm/utility")]
    [ApiController]
    public class UtilityController : ControllerBase
    {
        private readonly IUtilityRepository _utilityRepository;
        private readonly IUtility1Repository _utility1Repository;
        private readonly IUtilityService _utilityService;
        private readonly IUnitRepository _unitRepository;
        private readonly IConfiguration _config;

        public UtilityController(IUtilityRepository utilityRepository, IUtility1Repository utility1Repository, IUtilityService utilityService, IUnitRepository unitRepository, IConfiguration config)
        {
            _utilityRepository = utilityRepository;
            _utility1Repository = utility1Repository;
            _utilityService = utilityService;
            _config = config;
            _unitRepository = unitRepository;
        }


        /// <summary>
        /// Untuk load data menu user role
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("get_menu")]
        [ProducesResponseType(typeof(ServiceResponses<GetMenuVM>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponses<GetMenuVM>> GetMenu([FromBody] GetMenuFilterVM req)
        {
            var res = await _utilityRepository.GetMenu(req);

            return new ServiceResponses<GetMenuVM>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = res
            };
        }

        /// <summary>
        /// Untuk get data menu
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet("get_menu2")]
        [ProducesResponseType(typeof(ServiceResponse<TblMenu>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<TblMenu>> GetMenuById([FromQuery] GetMenuByIdFilterVM req)
        {
            var res = await _utilityRepository.GetMenuById(req);

            return new ServiceResponse<TblMenu>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = res
            };
        }

        /// <summary>
        /// Untuk get lookup by type
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("lookup_by_type")]
        [ProducesResponseType(typeof(ServiceResponses<TblLookup>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponses<TblLookup>> SelectLookup([FromBody] LookupFilterVM req)
        {
            var res = await _utilityRepository.SelectLookup(req);

            return new ServiceResponses<TblLookup>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = res
            };
        }

        /// <summary>
        /// Untuk get lookup type By RoleId
        /// </summary>
        /// <returns></returns>
        [HttpGet("DropdownUnitTypeByRoleId")]
        [ProducesResponseType(typeof(ServiceResponses<TblLookup>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponses<TblLookup>> SelectLookupTypeUnitByRoleId()
        {
            string authorization = HttpContext.Request.Headers["Authorization"];
            var UnitId = "";
            var RoleId = "1";
            if (authorization != null)
            {
                var token = authorization.Split(" ")[1];
                var claims = TokenManager.GetPrincipal(token);
                UnitId = claims.UnitId;
                RoleId = claims.RoleId;
            }

            var res = await _utilityRepository.SelectLookupTypeByUnitId(RoleId, UnitId);

            return new ServiceResponses<TblLookup>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = res
            };
        }

        /// <summary>
        /// Untuk get system parameter by kata kunci
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("sysparam_by_key")]
        [ProducesResponseType(typeof(ServiceResponse<SystemParameterVM>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<SystemParameterVM>> SelectSystemParameter([FromBody] SystemParameterFilterVM req)
        {
            var res = await _utilityRepository.SelectSystemParameter(req);

            return new ServiceResponse<SystemParameterVM>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = res
            };
        }

        /// <summary>
        /// Untuk get eligible apps
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("eligible_apps")]
        [ProducesResponseType(typeof(ServiceResponses<EligibleAppsVM>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponses<EligibleAppsVM>> GetEligibleApps([FromBody] EligibleAppsFilterVM req)
        {
            string authorization = HttpContext.Request.Headers["Authorization"];
            if (authorization != null)
            {
                var token = authorization.Split(" ")[1];
                var claims = TokenManager.GetPrincipal(token);
                req.PegawaiId = claims.PegawaiId;
            }

            var res = await _utilityRepository.GetEligibleApps(req);

            return new ServiceResponses<EligibleAppsVM>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = res
            };
        }

        /// <summary>
        /// Untuk get log acitivity
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("log_activity")]
        [ProducesResponseType(typeof(ServiceResponse<GridResponse<LogActivityVM>>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<GridResponse<LogActivityVM>>> GetLogActivity([FromBody] LogActivityFilterVM req)
        {
            var res = await _utilityRepository.GetLogActivity(req);

            return new ServiceResponse<GridResponse<LogActivityVM>>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = res
            };
        }

        /// <summary>
        /// Untuk get log NIK Inquiry
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("GetLogNIKInquiry")]
        [ProducesResponseType(typeof(ServiceResponse<GridResponse<LogNikInquiryVM>>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<GridResponse<LogNikInquiryVM>>> GetLogNikInquiry([FromBody] LogNikInquiryFilterVM req)
        {
            var res = await _utilityRepository.GetLogNikInquiry(req);

            return new ServiceResponse<GridResponse<LogNikInquiryVM>>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = res
            };
        }

        /// <summary>
        /// Dropdown menu
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet("dropdown_menu")]
        [ProducesResponseType(typeof(ServiceResponse<GridResponse<DataDropdownServerSide>>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<GridResponse<DataDropdownServerSide>>> DropdownMenu([FromQuery] DropdownMenuFilterVM req)
        {
            var res = await _utilityRepository.DropdownMenu(req);

            return new ServiceResponse<GridResponse<DataDropdownServerSide>>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = res
            };
        }

        /// <summary>
        /// Dropdown master data aplikasi
        /// </summary>
        /// <returns></returns>
        [HttpGet("dropdown_aplikasi")]
        [ProducesResponseType(typeof(ServiceResponses<DataDropdownServerSide>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponses<DataDropdownServerSide>> DropdownAplikasi()
        {
            var res = await _utilityRepository.DropdownAplikasi();

            return new ServiceResponses<DataDropdownServerSide>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = res
            };
        }

        /// <summary>
        /// Dropdown Pegawai Hirarki
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("dropdown_pegawai_hirarki")]
        [ProducesResponseType(typeof(ServiceResponse<GridResponse<DataDropdownServerSide>>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<GridResponse<DataDropdownServerSide>>> DropdownPegawaiHirarki([FromBody] DropdownMenuFilterVM req)
        {
            string authorization = HttpContext.Request.Headers["Authorization"];
            var UnitId = "";
            var RoleId = "1";
            if (authorization != null)
            {
                var token = authorization.Split(" ")[1];
                var claims = TokenManager.GetPrincipal(token);
                UnitId = claims.UnitId;
                RoleId = claims.RoleId;
            }
            var res = await _utilityRepository.DropdownPegawaiHirarki(req, UnitId, RoleId);

            return new ServiceResponse<GridResponse<DataDropdownServerSide>>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = res
            };
        }

        /// <summary>
        /// Untuk cek akses menu dari role
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("check_access_menu")]
        [ProducesResponseType(typeof(ServiceResponse<bool>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<bool>> IsGetAccess([FromBody] CheckAccessMenuFilterVM req)
        {
            string authorization = HttpContext.Request.Headers["Authorization"];
            if (authorization != null)
            {
                var token = authorization.Split(" ")[1];
                var claims = TokenManager.GetPrincipal(token);
                req.RoleId = claims.RoleId;
            }
            var res = await _utilityRepository.IsGetAccess(req);

            return new ServiceResponse<bool>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = res
            };
        }

        /// <summary>
        /// Get All Data Master Type Jari
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet("GetAllDataMasterTypeFinger")]
        [ProducesResponseType(typeof(ServiceResponse<GridResponse<DataDropdownServerSide>>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<GridResponse<DataDropdownServerSide>>> GetAllDataMasterTypeFinger([FromQuery] UtilityVM req)
        {
            var res = await _utilityRepository.GetAllDataMasterTypeFinger(req);

            return new ServiceResponse<GridResponse<DataDropdownServerSide>>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = res
            };
        }

        /// <summary>
        /// Get ALL Role Pegawai By Pegawai and App Id
        /// </summary>
        /// <param name="PegawaiId"></param>
        /// <param name="AppId"></param>
        /// <returns></returns>
        [HttpGet("GetAllRolePegawai")]
        [ProducesResponseType(typeof(ServiceResponse<GridResponse<DataDropdownServerSide>>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<GridResponse<DataDropdownServerSide>>> GetAllRolePegawai([FromQuery] int PegawaiId, int AppId)
        {
            var res = await _utilityRepository.GetAllRolePegawai(PegawaiId, AppId);

            return new ServiceResponse<GridResponse<DataDropdownServerSide>>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = res
            };
        }

        /// <summary>
        /// Get Dropdown Roles By Menu Id
        /// </summary>
        /// <param name="Roles"></param>
        /// <returns></returns>
        [HttpGet("GetDropdownRolesByMenuId")]
        [ProducesResponseType(typeof(ServiceResponse<GridResponse<DataDropdownServerSide>>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<GridResponse<DataDropdownServerSide>>> GetDropdownRolesByMenuId([FromQuery] int Roles)
        {
            var res = await _utilityRepository.GetDropdownRolesByMenuId(Roles);

            return new ServiceResponse<GridResponse<DataDropdownServerSide>>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = res
            };
        }

        /// <summary>
        /// Get Dropdown Apps By Id
        /// </summary>
        /// <param name="apps"></param>
        /// <returns></returns>
        [HttpGet("GetDropdownAppsById")]
        [ProducesResponseType(typeof(ServiceResponse<GridResponse<DataDropdownServerSide>>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<GridResponse<DataDropdownServerSide>>> GetDropdownAppsById([FromQuery] string apps)
        {
            var res = await _utilityRepository.GetDropdownAppsById(apps);

            return new ServiceResponse<GridResponse<DataDropdownServerSide>>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = res
            };
        }

        /// <summary>
        /// Get Count Data Enroll
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        [HttpGet("GetCountDataEnroll")]
        [ProducesResponseType(typeof(ServiceResponse<Jumlah_Inbox>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<Jumlah_Inbox>> GetCountDataEnroll([FromQuery] string uid)
        {
            string authorization = HttpContext.Request.Headers["Authorization"];
            var PegawaiId = "";
            var UnitId = "";
            if (authorization != null)
            {
                var token = authorization.Split(" ")[1];
                var claims = TokenManager.GetPrincipal(token);
                PegawaiId = claims.PegawaiId;
                UnitId = claims.UnitId;
            }

            var res = await _utilityRepository.GetCountDataEnroll(uid, PegawaiId, UnitId);
            var res2 = res.Data.FirstOrDefault();

            return new ServiceResponse<Jumlah_Inbox>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = res2
            };
        }

        /// <summary>
        /// Get Count Data Enroll By Unit Id
        /// </summary>
        /// <param name="UnitId"></param>
        /// <returns></returns>
        [HttpGet("GetCountDataEnrollByUnitId")]
        [ProducesResponseType(typeof(ServiceResponse<Jumlah_Inbox>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<Jumlah_Inbox>> GetCountDataEnrollByUnitId([FromQuery] string UnitId)
        {
            //string authorization = HttpContext.Request.Headers["Authorization"];
            //if (authorization != null)
            //{
            //    var token = authorization.Split(" ")[1];
            //    var claims = TokenManager.GetPrincipal(token);
            //    UnitId = claims.UnitId;
            //}

            var res = await _utilityRepository.GetCountDataEnrollByUnitId(UnitId);
            var res2 = res.Data.FirstOrDefault();

            return new ServiceResponse<Jumlah_Inbox>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = res2
            };
        }

        [HttpGet("GetCountDataEnrollByUnitIdJenis")]
        [ProducesResponseType(typeof(ServiceResponse<Jumlah_Inbox>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<Jumlah_Inbox>> GetCountDataEnrollByUnitIdJenis([FromQuery] string UnitId, string Jenis)
        {
            //string authorization = HttpContext.Request.Headers["Authorization"];
            //if (authorization != null)
            //{
            //    var token = authorization.Split(" ")[1];
            //    var claims = TokenManager.GetPrincipal(token);
            //    UnitId = claims.UnitId;
            //}

            var res = await _utilityRepository.GetCountDataEnrollByUnitIdJenis(UnitId, Jenis);
            var res2 = res.Data.FirstOrDefault();

            return new ServiceResponse<Jumlah_Inbox>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = res2
            };
        }

        /// <summary>
        /// Get Count Data Enroll Temp
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet("GetCountDataEnrollTemp")]
        [ProducesResponseType(typeof(ServiceResponse<Jumlah_Inbox>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<Jumlah_Inbox>> GetCountDataEnrollTemp([FromQuery] DataEnrollFilter req)
        {
            string authorization = HttpContext.Request.Headers["Authorization"];
            if (authorization != null)
            {
                var token = authorization.Split(" ")[1];
                var claims = TokenManager.GetPrincipal(token);
                req.PegawaiId = claims.PegawaiId;
                req.UnitId = claims.UnitId;
            }
            var res = await _utilityRepository.GetCountDataEnrollTemp(req);
            var res2 = res.Data.FirstOrDefault();

            return new ServiceResponse<Jumlah_Inbox>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = res2
            };
        }

        /// <summary>
        /// Get Data Maps Enroll
        /// </summary>
        /// <param name="uid"></param>
        /// <returns></returns>
        [HttpGet("GetDataMapsEnroll")]
        [ProducesResponseType(typeof(ServiceResponse<GridResponse<DataMaps_ViewModels>>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<GridResponse<DataMaps_ViewModels>>> GetDataMapsEnroll([FromQuery] string uid)
        {
            var res = await _utilityRepository.GetDataMapsEnroll(uid);

            return new ServiceResponse<GridResponse<DataMaps_ViewModels>>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = res
            };
        }

        /// <summary>
        /// Get Cek Usia
        /// </summary>
        /// <param name="tglLahir"></param>
        /// <returns></returns>
        
        [HttpGet("CekUsia")]
        [ProducesResponseType(typeof(ServiceResponse<GridResponse<CekUsia_ViewModels>>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]

        #region before 2024 pentest
        //public async Task<ServiceResponse<GridResponse<CekUsia_ViewModels>>> CekUsia([FromQuery] string tglLahir)
        //{
        //    if (string.IsNullOrEmpty(tglLahir)) return new ServiceResponse<GridResponse<CekUsia_ViewModels>>
        //    {
        //        Status = (int)ServiceResponseStatus.EMPTY_PARAMETER,
        //        Message = "Error"
        //    };



        //    tglLahir = tglLahir.Replace(".", "");
        //    if (tglLahir.Contains("/"))
        //    {
        //        tglLahir = tglLahir.Replace("/", "-");
        //    }

        //    if (!DateTime.TryParse(tglLahir, out _))
        //    {
        //        return new ServiceResponse<GridResponse<CekUsia_ViewModels>>
        //        {
        //            Status = (int)ServiceResponseStatus.ERROR_PARAMETER,
        //            Message = "Parameter Datetime Error"
        //        };
        //    }

        //    var res = await _utilityRepository.CekUsia(tglLahir);

        //    return new ServiceResponse<GridResponse<CekUsia_ViewModels>>
        //    {
        //        Status = (int)ServiceResponseStatus.SUKSES,
        //        Message = "Success",
        //        Data = res
        //    };
        //}
        #endregion

        public async Task<ServiceResponse<GridResponse<CekUsia_ViewModels>>> CekUsia([FromQuery] string tglLahir)
        {
            if (string.IsNullOrEmpty(tglLahir)) return new ServiceResponse<GridResponse<CekUsia_ViewModels>>
            {
                Status = (int)ServiceResponseStatus.EMPTY_PARAMETER,
                Message = "Error"
            };

            tglLahir = tglLahir.Replace(".", "");
            if (tglLahir.Contains("/"))
            {
                tglLahir = tglLahir.Replace("/", "-");
            }

            if (!DateTime.TryParse(tglLahir, out _) && DateTime.Parse(tglLahir) >= DateTime.Now)
            {
                return new ServiceResponse<GridResponse<CekUsia_ViewModels>>
                {
                    Status = (int)ServiceResponseStatus.ERROR_PARAMETER,
                    Message = "Parameter Datetime Error Parsing Or Datetime must be earlier than Today."
                };
            }

            var res = await _utilityRepository.CekUsia(tglLahir);

            return new ServiceResponse<GridResponse<CekUsia_ViewModels>>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = res
            };
        }
        

        /// <summary>
        /// Get Data FInger Login
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        [HttpGet("SelectDataTypeFingerLogin")]
        [ProducesResponseType(typeof(ServiceResponse<GridResponse<DataDropdownServerSide>>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<GridResponse<DataDropdownServerSide>>> SelectDataTypeFingerLogin([FromQuery] string username)
        {
            var res = await _utilityRepository.SelectDataTypeFingerLogin(username);

            return new ServiceResponse<GridResponse<DataDropdownServerSide>>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = res
            };
        }

        /// <summary>
        /// Get Data All Unit By Id
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpGet("GetUnitById")]
        [ProducesResponseType(typeof(ServiceResponse<GridResponse<DataDropdownServerSide>>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<GridResponse<DataDropdownServerSide>>> GetUnitById(string Id)
        {
            var res = await _utilityRepository.GetUnitById(Id);

            return new ServiceResponse<GridResponse<DataDropdownServerSide>>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = res
            };
        }

        /// <summary>
        /// Convert Alamat to Lat Long
        /// </summary>
        /// <param name="apikey"></param>
        /// <param name="alamatEncode"></param>
        /// <returns></returns>
        [HttpGet("ConvertAlamatToLatlong")]
        [ProducesResponseType(typeof(ServiceResponse<ConvertLatLong_ViewModels>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<ConvertLatLong_ViewModels>> ConvertAlamatToLatlong([FromQuery] string apikey, string alamatEncode)
        {
            string url = @"https://maps.googleapis.com/maps/api/geocode/json?address=" + alamatEncode + "&key=" + apikey;

            WebRequest request = WebRequest.Create(url);

            var isProxy = _config.GetValue<bool>("isProxy");
            if (isProxy)
            {
                WebProxy myProxy = new WebProxy();
                // Obtain the Proxy Prperty of the  Default browser.  
                //myProxy = (WebProxy)request.Proxy;

                var IpProxy = _config.GetValue<string>("IpProxy");
                //// Create a new Uri object.
                Uri newUri = new Uri(IpProxy);

                // Associate the new Uri object to the myProxy object.
                myProxy.Address = newUri;

                //request.Proxy = myProxy;

                request.Proxy = myProxy;
            }

            WebResponse response = request.GetResponse();

            Stream data = response.GetResponseStream();

            StreamReader reader = new StreamReader(data);

            // json-formatted string from maps api
            string responseFromServer = reader.ReadToEnd();

            var res = JsonConvert.DeserializeObject<ConvertLatLong_ViewModels>(responseFromServer);

            return new ServiceResponse<ConvertLatLong_ViewModels>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = res
            };
        }

        /// <summary>
        /// Get All Data Unit
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet("GetAllDataUnit")]
        [ProducesResponseType(typeof(ServiceResponse<GridResponse<DataDropdownServerSide>>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<GridResponse<DataDropdownServerSide>>> GetAllDataUnit([FromQuery] Utility2VM req)
        {
            var res = await _utilityRepository.GetUnit(req);

            return new ServiceResponse<GridResponse<DataDropdownServerSide>>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = res
            };
        }

        /// <summary>
        /// Get All Data Unit By Type and Unit Id
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost("GetAllDataUnitByTypeAndUnitId")]
        [ProducesResponseType(typeof(ServiceResponse<GridResponse<DataDropdownServerSide>>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<GridResponse<DataDropdownServerSide>>> GetDataUnitByTypeAndUnitId([FromBody] GetDataUnitByTypeAndUnitIdViewModel req)
        {
            string authorization = HttpContext.Request.Headers["Authorization"];
            if (authorization != null)
            {
                var token = authorization.Split(" ")[1];
                var claims = TokenManager.GetPrincipal(token);
                req.UnitId = claims.UnitId;
                req.RoleId = claims.RoleId;
            }
            else
            {
                req.UnitId = "1880";
                req.RoleId = "1";
            }

            var res = await _utilityRepository.GetUnitByTypeAndUnitId(req);

            return new ServiceResponse<GridResponse<DataDropdownServerSide>>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = res
            };
        }

        /// <summary>
        /// Get All Data Role
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet("GetAllDataRole")]
        [ProducesResponseType(typeof(ServiceResponse<GridResponse<DataDropdownServerSide>>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<GridResponse<DataDropdownServerSide>>> GetAllDataRole([FromQuery] Utility2VM req)
        {
            var res = await _utilityRepository.GetRole(req);

            return new ServiceResponse<GridResponse<DataDropdownServerSide>>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = res
            };
        }

        /// <summary>
        /// Get master data aplikasi by id
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet("master_aplikasi")]
        [ProducesResponseType(typeof(ServiceResponse<TblMasterAplikasi>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<TblMasterAplikasi>> GetMasterAplikasi([FromQuery] GetByIdVM req)
        {
            var res = await _utilityRepository.GetMasterAplikasi(req);

            return new ServiceResponse<TblMasterAplikasi>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = res
            };
        }

        /// <summary>
        /// To change application and get list menu
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpGet("change_apps")]
        [ProducesResponseType(typeof(ServiceResponse<MenuByChangeAppsVM>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<MenuByChangeAppsVM>> ChangeApplication([FromQuery] GetByIdAppVM req)
        {
            var res = await _utilityRepository.ChangeApplication(req);

            if (res != null) // rewrite the responds
            {
                var data = res.ListMenu.First();
                res.MasterApps = new TblMasterAplikasi
                {
                    Id = data.Id,
                    Nama = data.Name,
                    Url_Default = data.Route,
                    Kode = data.Name,
                    Deskripsi = data.Name,
                    Order_By = data.Order
                };
            }

            return new ServiceResponse<MenuByChangeAppsVM>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = res
            };
        }

        /// <summary>
        /// Get count maps data reader
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        [HttpGet("count_data_maps_reader")]
        [ProducesResponseType(typeof(ServiceResponse<int>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponse<int>> GetCountDataReader([FromQuery] string UnitIds)
        {
            var res = await _utilityRepository.GetCountDataReader(UnitIds);

            return new ServiceResponse<int>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = res
            };
        }

        /// <summary>
        /// Get maps data reader
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        [HttpGet("data_maps_reader")]
        [ProducesResponseType(typeof(ServiceResponses<DataMapsVM>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponses<DataMapsVM>> GetMapsDataReader()
        {
            var res = await _utilityRepository.GetMapsDataReader();

            return new ServiceResponses<DataMapsVM>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = res
            };
        }

        /// <summary>
        /// Update session log
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        [HttpPost("update_session_log")]
        [ProducesResponseType(typeof(ServiceResponse<bool>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public ServiceResponse<bool> UpdateSessionLog([FromBody] UserSessionLogFilterVM req)
        {
            if (req == null)
            {
                return new ServiceResponse<bool>
                {
                    Message = nameof(ServiceResponseStatus.EMPTY_PARAMETER),
                    Status = (int)ServiceResponseStatus.EMPTY_PARAMETER,
                    Data = false
                };
            }

            string authorization = HttpContext.Request.Headers["Authorization"];
            if (authorization == null)
            {
                return new ServiceResponse<bool>
                {
                    Message = "Authorization not found",
                    Status = (int)ServiceResponseStatus.ERROR,
                    Data = false
                };
            }

            UserSessionLogVM sessionLog = new UserSessionLogVM();
            var token = authorization.Split(" ")[1];
            var claims = TokenManager.GetPrincipal(token);
            sessionLog.UserId = int.Parse(claims.UserId);
            sessionLog.Npp = claims.NIK;
            sessionLog.UnitId = claims.UnitId;
            sessionLog.RoleId = claims.RoleId;

            sessionLog.IpAddress = req.IpAddress;
            sessionLog.SessionId = req.SessionId;
            sessionLog.Url = req.Url;
            sessionLog.Browser = req.Browser;
            sessionLog.Os = req.Os;
            sessionLog.ClientInfo = req.ClientInfo;

            var res = _utilityService.UpdateSessionLog(sessionLog);

            return new ServiceResponse<bool>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = res
            };
        }

        /// <summary>
        /// Untuk load monitoring reader
        /// </summary>
        /// <returns></returns>
        [HttpGet("monitoring_reader_excel")]
        [ProducesResponseType(typeof(ServiceResponses<MonitoringReaderVM>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public async Task<ServiceResponses<MonitoringReaderVM>> GetMonitoringReader([FromQuery] string UnitIds)
        {
            var res = await _utilityRepository.GetMonitoringReader(UnitIds);

            return new ServiceResponses<MonitoringReaderVM>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = res
            };
        }

        /// <summary>
        /// Untuk Log NIK Inquiry
        /// </summary>
        /// <returns></returns>
        [HttpPost("log_nik_inquiry")]
        [ProducesResponseType(typeof(ServiceResponses<bool>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public ServiceResponse<bool> InsertLogNIKInquiry([FromBody] Core.Entities.DataMaster.Utility.Entity.Tbl_LogNIKInquiry req)
        {
            if (req == null)
            {
                return new ServiceResponse<bool>
                {
                    Message = nameof(ServiceResponseStatus.EMPTY_PARAMETER),
                    Status = (int)ServiceResponseStatus.EMPTY_PARAMETER,
                    Data = false
                };
            }

            var res = _utilityRepository.InsertLogNIKInquiry(req);

            return new ServiceResponse<bool>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = res
            };
        }

        /// <summary>
        /// Insert Log Activity - use this if only needed
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        [HttpPost("insert_log_activity")]
        [ProducesResponseType(typeof(ServiceResponse<bool>), 200)]
        [ProducesResponseType(500)]
        [LogActivity(Keterangan = "Data Master")]
        public ServiceResponse<bool> InsertLogActivity([FromBody] Core.Entities.DataMaster.Utility.Entity.TblLogActivity req)
        {
            if (req == null)
            {
                return new ServiceResponse<bool>
                {
                    Message = nameof(ServiceResponseStatus.EMPTY_PARAMETER),
                    Status = (int)ServiceResponseStatus.EMPTY_PARAMETER,
                    Data = false
                };
            }

            string authorization = HttpContext.Request.Headers["Authorization"];
            if (!string.IsNullOrWhiteSpace(authorization))
            {
                var token = authorization.Split(" ")[1];
                var claims = TokenManager.GetPrincipal(token);
                req.UserId = int.Parse(claims.UserId);
                req.UnitId = int.Parse(claims.UnitId);
                req.Npp = claims.NIK;
            }


            req.ActionTime = DateTime.Now;

            var res = _utility1Repository.InsertLogActivity(req);

            return new ServiceResponse<bool>
            {
                Status = (int)ServiceResponseStatus.SUKSES,
                Message = "Success",
                Data = res
            };
        }
    }
}
