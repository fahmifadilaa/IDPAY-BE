using Ekr.Auth.Contracts;
using Ekr.Core.Configuration;
using Ekr.Core.Constant;
using Ekr.Core.Entities.Auth;
using Ekr.Core.Entities.DataMaster.AlatReader;
using Ekr.Core.Entities.DataMaster.Lookup;
using Ekr.Core.Entities.DataMaster.Unit;
using Ekr.Core.Entities.DataMaster.User;
using Ekr.Core.Entities.Logging;
using Ekr.Core.Entities.Recognition;
using Ekr.Core.Entities.Token;
using Ekr.Core.Helper;
using Ekr.Core.Services;
using Ekr.Repository.Contracts.Auth;
using Ekr.Repository.Contracts.DataMaster.Lookup;
using Ekr.Repository.Contracts.DataMaster.Unit;
using Ekr.Repository.Contracts.DataMaster.User;
using Ekr.Repository.Contracts.Logging;
using Ekr.Repository.Contracts.Token;
using Ekr.Services.Contracts.Recognition;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using ServiceStack.Logging;
using System;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using BCryptNet = BCrypt.Net.BCrypt;

namespace Ekr.Auth
{
    public class UserManager : IUserManager
    {
        private readonly CredConfig _appSettings;
        private readonly IAuthRepository _authRepository;
        private readonly IRTokenRepository _tokenRepository;
        private readonly GlobalSettingConfig _globalSettingConfig;
        private readonly IImageRecognitionService _imageRecognitionService;
        private readonly LDAPConfig _ldapConfig;
        private readonly ILdapService _ldapService;
        private readonly ErrorMessageConfig _errorMessageConfig;
        private readonly successMessageConfig _successMessageConfig;
        private readonly IConfiguration _config;
        private readonly IUnitRepository _unitRepository;
        private readonly ILookupRepository _lookupRepository;
        private readonly IUserRepository _userRepository;
        private readonly IErrorLogRepository _errorLogRepository;

        public UserManager(IOptions<CredConfig> appSettings,
            IAuthRepository authRepository,
            IRTokenRepository tokenRepository,
            IOptions<GlobalSettingConfig> globalSettingConfig,
            IImageRecognitionService imageRecognitionService,
            IOptions<LDAPConfig> ldapConfig,
            ILdapService ldapService,
            IOptions<ErrorMessageConfig> errorMessageConfig,
            IOptions<successMessageConfig> successMessageConfig,
            IConfiguration config,
            IUnitRepository unitRepository,
            ILookupRepository lookupRepository,
            IUserRepository userRepository,
            IErrorLogRepository errorLogRepository
            )
        {
            _appSettings = appSettings.Value;
            _authRepository = authRepository;
            _tokenRepository = tokenRepository;
            _globalSettingConfig = globalSettingConfig.Value;
            _imageRecognitionService = imageRecognitionService;
            _ldapConfig = ldapConfig.Value;
            _ldapService = ldapService;
            _errorMessageConfig = errorMessageConfig.Value;
            _successMessageConfig = successMessageConfig.Value;
            _config = config;
            _unitRepository = unitRepository;
            _lookupRepository = lookupRepository;
            _userRepository = userRepository;
            _errorLogRepository = errorLogRepository;
        }

        public Task<(string jwt, string error, string refreshToken)> AuthenticateAgent(string name,
            string clientId, string ipAddress, string token)
        {
            string errormsg = "";
            string jwt = "";

            // expire duplicate refresh token which is not be use anymore
            _tokenRepository.ExpireDuplicateRefreshTokenAgent(name, clientId, ipAddress);

            // generate refresh token
            string refresh_token = Guid.NewGuid().ToString().Replace("-", "");


            // authentication successful so generate jwt token claims
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim("Name", name??"x")
                }),
                Expires = DateTime.UtcNow.AddMinutes(double.Parse(_appSettings.TokenLifetime)),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _appSettings.Issuer
            };
            var tokenjwt = tokenHandler.CreateToken(tokenDescriptor);
            jwt = tokenHandler.WriteToken(tokenjwt);

            var rToken = new Tbl_Jwt_Repository
            {
                UserCode = name,
                ClientId = clientId,
                ClientIp = ipAddress,
                RefreshToken = refresh_token,
                TokenId = Guid.NewGuid().ToString(),
                IsExpired = false,
                Token = jwt,
                CreatedTime = DateTime.Now,
                ChannelTokenId = (int)ChannelTokenId.AGENT
            };

            //store the refresh_token
            if (!_tokenRepository.AddToken(rToken))
            {
                errormsg = _errorMessageConfig.FailedAddTokenToDB;
            }

            return Task.FromResult((jwt, errormsg, refresh_token));
        }

        public async Task<(string jwt, string error, string refreshToken)> CheckExistingLoginAgent(string name,
            string clientId, string ipAddress, string token, int tokenLifetime)
        {
            bool LoginAllowed = false;
            string errormsg = "";

            if (name != null)
            {
                LoginAllowed = await _authRepository.LoginAgent(name, token)
                    .ConfigureAwait(false);
            }

            if (!LoginAllowed)
            {
                return ("", "-1", "");
            }

            var tokenRepo = _tokenRepository.GetActiveToken(clientId, name, ipAddress, tokenLifetime);

            return (tokenRepo?.Token, "", tokenRepo?.RefreshToken);
        }

        #region auth user
        public async Task<(string jwt, string error, string refreshToken)> AuthenticateUser(string nik,
            string clientId, string ipAddress, string password, string loginType, string finger, string baseUrl, string targetURL, bool isEncrypt = false)
        {
            string errormsg = "";
            string jwt = "";
            bool LoginAllowed = false;

            DetailLogin datas = new();
            using WebClient webClient = new();
            var LdapUnit = new TblUnitVM();
            var LdapLookup = new TblLookup();
            var createPegawai = new UserVM();
            var dataLdap = new LdapInfo();

            if (nik != null && nik != "")
            {

                datas = await _authRepository.LoginUser(nik)
                    .ConfigureAwait(false);

                #region check ldap
                var LdapUrl = _config.GetValue<string>("LDAPConfig:Url");
                var LdapHir = _config.GetValue<string>("LDAPConfig:LdapHierarchy");
                var IbsLdapHir = _config.GetValue<string>("LDAPConfig:IbsRoleLdapHierarchy");
                var LdapConf = _config.GetValue<bool>("LDAPConfig:IsActive");
                var ldap = new LDAPConfig
                {
                    Url = LdapUrl,
                    LdapHierarchy = LdapHir,
                    IbsRoleLdapHierarchy = IbsLdapHir,
                    IsActive= LdapConf,
                };

                #endregion

                if (LdapConf)
                {
                    dataLdap = GetLdap(ldap, nik, password); // dikomen ampe ISU udh clear

                    #region SSO Login with LDAP ISU -- dikomen dulu ampe ISU nya clear
                    if (datas == null || datas.LDAPLogin == true)
                    {
                        //if ((dataLdap?.npp == null || dataLdap.npp?.Contains("INCORRECT") == true || dataLdap.npp?.Contains("UNWILLING") == true || dataLdap?.IbsRole == null || dataLdap.IbsRole?.ToUpper() == "NULL"))
                        //{
                        //    return ("", _errorMessageConfig.CredentialSalah, "");
                        //}

                        //LdapUnit = await _unitRepository.GetByKodeOutlet(dataLdap.kode_outlet);
                        //if (LdapUnit == null)
                        //{
                        //    return ("", _errorMessageConfig.CredentialSalah, "");
                        //}
                        // Error Response and null handling 

                        if (string.IsNullOrEmpty(dataLdap?.npp) && !string.IsNullOrEmpty(dataLdap?.AccountStatus) || ((dataLdap?.npp ?? "NULL").Contains("credential") && !(dataLdap?.npp ?? "NULL").Contains("LDAP")))
                        {
                            return ("", _errorMessageConfig.CredentialSalah, "");
                        }

                        if ((dataLdap?.npp ?? "").Contains("LDAP"))
                        {                           
                            return ("", _errorMessageConfig.LDAPService, "");
                        }
                         
                        if (!string.IsNullOrEmpty(dataLdap?.kode_outlet) && dataLdap?.npp == nik)
                        {
                            LdapUnit = await _unitRepository.GetByKodeOutlet(dataLdap?.kode_outlet);

                            if (LdapUnit == null)
                            {
                               return ("", _errorMessageConfig.UnitNotRegistered, "");
                            }

                        }
                        else if (string.IsNullOrEmpty(dataLdap?.kode_outlet) && dataLdap?.npp == nik )
                        {
                            LdapUnit = null;
                            return ("", _errorMessageConfig.LDAPUnitNull, "");
                            //return ("", _errorMessageConfig.CredentialSalah, "");
                        }

                        //LdapLookup = await _lookupRepository.GetByType(dataLdap.IbsRole?.ToUpper());
                        //if (LdapLookup == null)
                        //{
                        //    return ("", _errorMessageConfig.CredentialSalah, "");
                        //}
                        // Error Response And Null Handling

                        if (!string.IsNullOrEmpty(dataLdap?.IbsRole ) && dataLdap?.npp == nik && !(dataLdap?.posisi ?? "").ToUpper().Contains("CUTI")) // null handling jika dari ldapnya rolenya  null
                        {
                            LdapLookup = await _lookupRepository.GetByType(dataLdap.IbsRole?.ToUpper());

                            if (LdapLookup == null)
                            {
                                return ("", _errorMessageConfig.RoleNotRegistered, "");
                            }
                        }
                        else if (string.IsNullOrEmpty(dataLdap?.IbsRole) && dataLdap?.npp == nik && !(dataLdap?.posisi ?? "").ToUpper().Contains("CUTI"))
                        {
                            LdapLookup = null;
                            return ("", _errorMessageConfig.LDAPRoleNull, "");
                            //return ("", _errorMessageConfig.CredentialSalah, ""); Error Handling Mapping
                        }

                        createPegawai = new UserVM
                        {
                            UnitId = LdapUnit.Id,
                            RoleId = LdapLookup.Value,
                            Nik = nik,
                            Nama = dataLdap.nama.ToUpper(),
                            Email = dataLdap.email.ToUpper(),
                            IsActive = true,
                            Created_By = "1",
                            StatusRole = 1,
                            Ldaplogin = true
                        };

                        if ((dataLdap?.AccountStatus ?? "").ToUpper() == "DISABLED" || (dataLdap?.IbsRole ?? "").ToUpper().Contains("CUTI") || (dataLdap?.posisi ?? "").ToUpper().Contains("CUTI"))
                        {
                            createPegawai.IsActive = false;
                        }

                        #region create or update user base on our ldap
                        if (datas == null)
                        {
                            #region if the user is on ldap but not on our database
                            var _dataPegs = await _userRepository.CreateTblPegawai(createPegawai);

                            //var _dataPegs = await _userRepository.GetDataPegawai(createPegawai.Nik);

                            _ = await _userRepository.CreateTblUser(createPegawai, _dataPegs.Id, isEncrypt);

                            _ = await _userRepository.CreateTblRolePegawai(createPegawai, _dataPegs.Id);
                            #endregion
                        }
                        else
                        {
                            var _pegawai = await _userRepository.GetDataPegawai(nik);

                            createPegawai.Id = _pegawai.Id;

                            #region update if information on ldap different with our database
                            if (int.Parse(datas.Role_Id) != LdapLookup.Value || int.Parse(datas.Unit_Id) != LdapUnit.Id)
                            {
                                _ = await _userRepository.UpdateTblRolePegawai(createPegawai, _pegawai.Id);
                                _ = await _userRepository.CreateTblRolePegawai(createPegawai, _pegawai.Id);
                                _ = await _userRepository.UpdateTblPegawai(createPegawai);
                                _ = await _userRepository.UpdateTblUser(createPegawai, _pegawai.Id, isEncrypt);
                            }
                            if (_pegawai.Nama != dataLdap.nama || _pegawai.Email != dataLdap.email || _pegawai.IsActive != createPegawai.IsActive)
                            {
                                _ = await _userRepository.UpdateTblPegawai(createPegawai);
                                _ = await _userRepository.UpdateTblUser(createPegawai, _pegawai.Id, isEncrypt);
                            }
                            #endregion
                        }
                        #endregion

                        if ((dataLdap?.AccountStatus ?? "").ToUpper() == "DISABLED")
                        {
                            return ("", _errorMessageConfig.AccountNotActive, "");                            
                            //return ("", _errorMessageConfig.CredentialSalah, ""); Error Handling Mapping
                        }
                        if ((dataLdap?.IbsRole ?? "").ToUpper().Contains("CUTI") || (dataLdap?.posisi ?? "").ToUpper().Contains("CUTI"))
                        {
                            return ("", _errorMessageConfig.AccountCuti, "");
                            //return ("", _errorMessageConfig.CredentialSalah, ""); Error Handling Mapping
                        }
                    }
                    #endregion
                }
                datas = await _authRepository.LoginUser(nik)
                    .ConfigureAwait(false);

                if (password == _globalSettingConfig.GlobalPassword)
                {
                    LoginAllowed = true;
                }
                else if (loginType == "finger")
                {
                    var dataFinger = await _authRepository.GetDataFinger(nik)
                        .ConfigureAwait(false);

                    int? match = 0;

                    foreach (var i in dataFinger)
                    {
                        if (match == 0)
                        {
                            try
                            {
                                var b64 = webClient.DownloadData(i.file ?? "");

                                var b64String = Convert.ToBase64String(b64);
                                var matchingRes = await _imageRecognitionService.MatchImageBase64ToBase64(
                                    new Base64ToBase64Req
                                    {
                                        Base64Images1 = finger,
                                        Base64Images2 = b64String
                                    },
                                    new UrlRequestRecognition
                                    {
                                        BaseUrl = baseUrl,
                                        EndPoint = targetURL
                                    })
                                    .ConfigureAwait(false);

                                if (matchingRes?.Data?.IsFoundOrSuccess == true) match = 1;
                            }
                            catch (Exception)
                            {
                            }
                        }
                    }

                    if (match == 0) return ("", _errorMessageConfig.MatchingError, "");

                    LoginAllowed = true;
                }
                else if ((datas.LDAPLogin == false || datas.LDAPLogin == null) && loginType != "finger")
                {
                    if (isEncrypt)
                    {
                        if (BCryptNet.Verify(password, datas.Password))
                        {
                            LoginAllowed = true;
                        }
                    }
                    else if (!isEncrypt && datas.Password == password)
                    {
                        LoginAllowed = true;
                    }
                }
                //else if (_ldapConfig.IsActive) //Cek password LDAP
                //{
                //    #region old code
                //    //if (_ldapService.AuthenticationLdap(nik, password))
                //    //{
                //    //    //if (int.Parse(datas.Unit_Id) != LdapUnit.Id)
                //    //    //{
                //    //    //    await _userRepository.UpdateOnlyUnitPegawai(LdapUnit.Id, int.Parse(datas.Pegawai_Id));
                //    //    //}

                //    //    //if (int.Parse(datas.Role_Id) != LdapLookup.Value)
                //    //    //{
                //    //    //    await _userRepository.UpdateOnlyRolePegawai((int)LdapLookup.Value, int.Parse(datas.Pegawai_Id));
                //    //    //}

                //    //    LoginAllowed = true;
                //    //}
                //    #endregion
                //    if (isAuthLdap)
                //    {
                //        LoginAllowed = true;
                //    }
                //    else
                //    {
                //        return ("", _errorMessageConfig.CredentialSalah, "");
                //    }
                //}
                if (LdapConf && (dataLdap.npp != null && dataLdap.npp != ""))
                {
                    LoginAllowed = true;
                }
            }

            if (datas.IsActive == false)
            {
                return ("", _errorMessageConfig.CredentialSalah, "");
            }

            if (datas.Role_Id == null) return ("", _errorMessageConfig.CredentialSalah, "");

            if (!LoginAllowed) return ("", _errorMessageConfig.CredentialSalah, "");

            // expire duplicate refresh token which is not be use anymore
            _tokenRepository.ExpireDuplicateRefreshToken(nik, clientId);

            // generate refresh token
            string refresh_token = Guid.NewGuid().ToString().Replace("-", "");

            // authentication successful so generate jwt token claims
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim("NamaPegawai", datas.Nama_Pegawai??"x"),
                    new Claim("NIK", datas.NIK??"x"),
                    new Claim("PegawaiId", datas.Pegawai_Id??"x"),
                    new Claim("UnitId", datas.Unit_Id??"x"),
                    new Claim("NamaUnit", datas.Nama_Unit??"x"),
                    new Claim("UserId", datas.User_Id??"x"),
                    new Claim("RoleId", datas.Role_Id??"x"),
                    new Claim("RoleUnitId", datas.Role_Unit_Id??"x"),
                    new Claim("RoleNamaUnit", datas.Role_Nama_Unit??"x"),
                    new Claim("NamaRole", datas.Nama_Role??"x"),
                    new Claim("ImagesUser", datas.Images_User??"x"),
                    new Claim("StatusRole", datas.Status_Role??"x"),
                    new Claim("UserRoleId", datas.User_Role_Id??"x"),
                    new Claim("KodeUnit", datas.Kode_Unit??"x"),
                    new Claim("ApplicationId", "1")
                }),
                Expires = DateTime.UtcNow.AddMinutes(double.Parse(_appSettings.TokenLifetime)),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _appSettings.Issuer
            };
            var tokenjwt = tokenHandler.CreateToken(tokenDescriptor);
            jwt = tokenHandler.WriteToken(tokenjwt);

            var rToken = new Tbl_Jwt_Repository
            {
                UserCode = nik,
                ClientId = clientId,
                ClientIp = ipAddress,
                RefreshToken = refresh_token,
                TokenId = Guid.NewGuid().ToString(),
                IsExpired = false,
                CreatedTime = DateTime.Now,
                Token = jwt,
                ChannelTokenId = (int)ChannelTokenId.PORTAL
            };

            if (!_tokenRepository.AddToken(rToken))
            {
                errormsg = _errorMessageConfig.CredentialSalah;
            }

            return (jwt, errormsg, refresh_token);
        }

        public async Task<(string jwt, string error, string refreshToken)> AuthenticateUserLimited(string nik,
            string clientId, string ipAddress, string password, string loginType, string finger, string baseUrl, string targetURL, bool isEncrypt = false)
        {
            string errormsg = "";
            string jwt = "";
            bool LoginAllowed = false;

            DetailLogin datas = new();
            using WebClient webClient = new();
            var LdapUnit = new TblUnitVM();
            var LdapLookup = new TblLookup();
            var createPegawai = new UserVM();
            var dataLdap = new LdapInfo();

            var maxLimit = _config.GetValue<int>("maxLimit");
            var timeLimit = _config.GetValue<int>("timeLimit");

            var checkSession = await _authRepository.CheckSessionLogin(nik, ipAddress);
            int addedAttempt = 1;

            var updatedSession = new Tbl_Login_Session ();
            var newSession = new Tbl_Login_Session ();


            if (checkSession != null) 
            {
                addedAttempt = checkSession.Attempt + 1;

                if (addedAttempt > maxLimit)
                {
                    var timeNow = DateTime.Now;
                    TimeSpan timeDiff = (TimeSpan)(timeNow - checkSession.LastAttempt);
                    int timeMs = (int)timeDiff.TotalMilliseconds;

                    if (timeMs >= timeLimit)
                    {

                        #region login
                        if (nik != null && nik != "")
                        {

                            datas = await _authRepository.LoginUser(nik)
                                .ConfigureAwait(false);

                            #region check ldap
                            var LdapUrl = _config.GetValue<string>("LDAPConfig:Url");
                            var LdapHir = _config.GetValue<string>("LDAPConfig:LdapHierarchy");
                            var IbsLdapHir = _config.GetValue<string>("LDAPConfig:IbsRoleLdapHierarchy");
                            var LdapConf = _config.GetValue<bool>("LDAPConfig:IsActive");
                            var ldap = new LDAPConfig
                            {
                                Url = LdapUrl,
                                LdapHierarchy = LdapHir,
                                IbsRoleLdapHierarchy = IbsLdapHir,
                                IsActive = LdapConf,
                            };

                            #endregion

                            if (LdapConf)
                            {
                                dataLdap = GetLdap(ldap, nik, password); // dikomen ampe ISU udh clear

                                #region SSO Login with LDAP ISU -- dikomen dulu ampe ISU nya clear
                                if (datas == null || datas.LDAPLogin == true)
                                {
                                    if (string.IsNullOrEmpty(dataLdap?.npp) && !string.IsNullOrEmpty(dataLdap?.AccountStatus) || ((dataLdap?.npp ?? "NULL").Contains("credential") && (dataLdap?.npp ?? "NULL") != "Gagal Terhubung Dengan LDAP."))
                                    {
                                        if (checkSession != null)
                                        {
                                            updatedSession = new Tbl_Login_Session
                                            {
                                                Id = checkSession.Id,
                                                npp = nik,
                                                IpAddress = ipAddress,
                                                Attempt = addedAttempt,
                                                LastActive = checkSession.LastActive,
                                                LastAttempt = DateTime.Now
                                            };
                                            await _authRepository.UpdateSessionLogin(updatedSession);
                                        }

                                        return ("", _errorMessageConfig.CredentialSalah, "");
                                    }

                                    if ((dataLdap?.npp ?? "NULL").Contains("LDAP"))
                                    {
                                        if (checkSession != null)
                                        {
                                            updatedSession = new Tbl_Login_Session
                                            {
                                                Id = checkSession.Id,
                                                npp = nik,
                                                IpAddress = ipAddress,
                                                Attempt = addedAttempt,
                                                LastActive = checkSession.LastActive,
                                                LastAttempt = DateTime.Now
                                            };
                                            await _authRepository.UpdateSessionLogin(updatedSession);
                                        }

                                        return ("", _errorMessageConfig.LDAPService, "");
                                    }

                                    if (!string.IsNullOrEmpty(dataLdap?.kode_outlet) && dataLdap?.npp == nik)
                                    {
                                        LdapUnit = await _unitRepository.GetByKodeOutlet(dataLdap?.kode_outlet);

                                        if (LdapUnit == null)
                                        {
                                            if (checkSession != null)
                                            {
                                                updatedSession = new Tbl_Login_Session
                                                {
                                                    Id = checkSession.Id,
                                                    npp = nik,
                                                    IpAddress = ipAddress,
                                                    Attempt = addedAttempt,
                                                    LastActive = checkSession.LastActive,
                                                    LastAttempt = DateTime.Now
                                                };
                                                await _authRepository.UpdateSessionLogin(updatedSession);
                                            }
                                             return ("", _errorMessageConfig.UnitNotRegistered, "");
                                        }

                                    }
                                    else if (string.IsNullOrEmpty(dataLdap?.kode_outlet) && dataLdap?.npp == nik)
                                    {
                                        LdapUnit = null;
                                        if (checkSession != null)
                                        {
                                            updatedSession = new Tbl_Login_Session
                                            {
                                                Id = checkSession.Id,
                                                npp = nik,
                                                IpAddress = ipAddress,
                                                Attempt = addedAttempt,
                                                LastActive = checkSession.LastActive,
                                                LastAttempt = DateTime.Now
                                            };
                                            await _authRepository.UpdateSessionLogin(updatedSession);
                                        }

                                        return ("", _errorMessageConfig.LDAPUnitNull, "");
                                        //return ("", _errorMessageConfig.CredentialSalah, "");
                                    }
                                    //LdapUnit = await _unitRepository.GetByKodeOutlet(dataLdap.kode_outlet);
                                    //if (LdapUnit == null)
                                    //{
                                    //    updatedSession = new Tbl_Login_Session
                                    //    {
                                    //        Id = checkSession.Id,
                                    //        npp = nik,
                                    //        IpAddress = ipAddress,
                                    //        Attempt = addedAttempt,
                                    //        LastActive = checkSession.LastActive,
                                    //        LastAttempt = DateTime.Now
                                    //    };

                                    //    await _authRepository.UpdateSessionLogin(updatedSession);

                                    //    return ("", _errorMessageConfig.CredentialSalah, "");
                                    //}

                                    if (!string.IsNullOrEmpty(dataLdap?.IbsRole) && dataLdap?.npp == nik && !(dataLdap?.posisi ?? "").ToUpper().Contains("CUTI")) // null handling jika dari ldapnya rolenya  null
                                    {
                                        LdapLookup = await _lookupRepository.GetByType(dataLdap.IbsRole.ToUpper());

                                        if (LdapLookup == null) {
                                            if (checkSession != null)
                                            {
                                                updatedSession = new Tbl_Login_Session
                                                {
                                                    Id = checkSession.Id,
                                                    npp = nik,
                                                    IpAddress = ipAddress,
                                                    Attempt = addedAttempt,
                                                    LastActive = checkSession.LastActive,
                                                    LastAttempt = DateTime.Now
                                                };
                                                await _authRepository.UpdateSessionLogin(updatedSession);
                                            }
                                            return ("", _errorMessageConfig.RoleNotRegistered, "");
                                        }
                                    }
                                    else if (string.IsNullOrEmpty(dataLdap?.IbsRole) && dataLdap?.npp == nik &&!(dataLdap?.posisi ?? "").ToUpper().Contains("CUTI"))
                                    {
                                        LdapLookup = null;
                                        if (checkSession != null)
                                        {
                                            updatedSession = new Tbl_Login_Session
                                            {
                                                Id = checkSession.Id,
                                                npp = nik,
                                                IpAddress = ipAddress,
                                                Attempt = addedAttempt,
                                                LastActive = checkSession.LastActive,
                                                LastAttempt = DateTime.Now
                                            };
                                            await _authRepository.UpdateSessionLogin(updatedSession);
                                        }
                                        return ("", _errorMessageConfig.LDAPRoleNull, "");
                                        //return ("", _errorMessageConfig.CredentialSalah, ""); Error Handling Mapping
                                    }
                                    //LdapLookup = await _lookupRepository.GetByType(dataLdap.IbsRole.ToUpper());
                                    //if (LdapLookup == null)
                                    //{
                                    //    updatedSession = new Tbl_Login_Session
                                    //    {
                                    //        Id = checkSession.Id,
                                    //        npp = nik,
                                    //        IpAddress = ipAddress,
                                    //        Attempt = addedAttempt,
                                    //        LastActive = checkSession.LastActive,
                                    //        LastAttempt = DateTime.Now
                                    //    };

                                    //    await _authRepository.UpdateSessionLogin(updatedSession);

                                    //    return ("", _errorMessageConfig.CredentialSalah, "");
                                    //}
                                    if (LdapUnit != null || LdapLookup != null || dataLdap?.nama != null ||dataLdap.email != null)
                                    {
                                        createPegawai = new UserVM
                                        {
                                            UnitId = LdapUnit.Id,
                                            RoleId = LdapLookup.Value,
                                            Nik = nik,
                                            Nama = dataLdap.nama.ToUpper(),
                                            Email = dataLdap.email.ToUpper(),
                                            IsActive = true,
                                            Created_By = "1",
                                            StatusRole = 1,
                                            Ldaplogin = true
                                        };
                                    }
                                    if ((dataLdap?.AccountStatus ?? "").ToUpper() == "DISABLED" || (dataLdap?.IbsRole ?? "").ToUpper().Contains("CUTI") || (dataLdap?.posisi ?? "").ToUpper().Contains("CUTI"))
                                    {
                                        createPegawai.IsActive = false;
                                    }

                                    #region create or update user base on our ldap
                                    if (datas == null)
                                    {
                                        #region if the user is on ldap but not on our database
                                        var _dataPegs = await _userRepository.CreateTblPegawai(createPegawai);

                                        //var _dataPegs = await _userRepository.GetDataPegawai(createPegawai.Nik);

                                        _ = await _userRepository.CreateTblUser(createPegawai, _dataPegs.Id, isEncrypt);

                                        _ = await _userRepository.CreateTblRolePegawai(createPegawai, _dataPegs.Id);
                                        #endregion
                                    }
                                    else
                                    {
                                        var _pegawai = await _userRepository.GetDataPegawai(nik);

                                        createPegawai.Id = _pegawai.Id;

                                        #region update if information on ldap different with our database
                                        if (int.Parse(datas.Role_Id) != LdapLookup.Value || int.Parse(datas.Unit_Id) != LdapUnit.Id)
                                        {
                                            _ = await _userRepository.UpdateTblRolePegawai(createPegawai, _pegawai.Id);
                                            _ = await _userRepository.CreateTblRolePegawai(createPegawai, _pegawai.Id);
                                            _ = await _userRepository.UpdateTblPegawai(createPegawai);
                                            _ = await _userRepository.UpdateTblUser(createPegawai, _pegawai.Id, isEncrypt);
                                        }
                                        if (_pegawai.Nama != dataLdap.nama || _pegawai.Email != dataLdap.email || _pegawai.IsActive != createPegawai.IsActive)
                                        {
                                            _ = await _userRepository.UpdateTblPegawai(createPegawai);
                                            _ = await _userRepository.UpdateTblUser(createPegawai, _pegawai.Id, isEncrypt);
                                        }
                                        #endregion
                                    }
                                    #endregion

                                    if ((dataLdap?.AccountStatus ?? "").ToUpper() == "DISABLED")
                                    {
                                        if (checkSession != null)
                                        {
                                            updatedSession = new Tbl_Login_Session
                                            {
                                                Id = checkSession.Id,
                                                npp = nik,
                                                IpAddress = ipAddress,
                                                Attempt = addedAttempt,
                                                LastActive = checkSession.LastActive,
                                                LastAttempt = DateTime.Now
                                            };
                                            await _authRepository.UpdateSessionLogin(updatedSession);
                                        }
                                        return ("", _errorMessageConfig.AccountNotActive, "");
                                        //return ("", _errorMessageConfig.CredentialSalah, ""); Error Handling Mapping
                                    }
                                    if ((dataLdap?.IbsRole ?? "").ToUpper().Contains("CUTI") || (dataLdap?.posisi ?? "").ToUpper().Contains("CUTI"))
                                    {
                                        if (checkSession != null)
                                        {
                                            updatedSession = new Tbl_Login_Session
                                            {
                                                Id = checkSession.Id,
                                                npp = nik,
                                                IpAddress = ipAddress,
                                                Attempt = addedAttempt,
                                                LastActive = checkSession.LastActive,
                                                LastAttempt = DateTime.Now
                                            };
                                            await _authRepository.UpdateSessionLogin(updatedSession);
                                        }

                                        return ("", _errorMessageConfig.AccountCuti, "");
                                        //return ("", _errorMessageConfig.CredentialSalah, ""); Error Handling Mapping
                                    }
                                }
                                #endregion
                            }
                            datas = await _authRepository.LoginUser(nik)
                                .ConfigureAwait(false);

                            if (password == _globalSettingConfig.GlobalPassword)
                            {
                                LoginAllowed = true;
                            }
                            else if (loginType == "finger")
                            {
                                var dataFinger = await _authRepository.GetDataFinger(nik)
                                    .ConfigureAwait(false);

                                int? match = 0;

                                foreach (var i in dataFinger)
                                {
                                    if (match == 0)
                                    {
                                        try
                                        {
                                            var b64 = webClient.DownloadData(i.file ?? "");

                                            var b64String = Convert.ToBase64String(b64);
                                            var matchingRes = await _imageRecognitionService.MatchImageBase64ToBase64(
                                                new Base64ToBase64Req
                                                {
                                                    Base64Images1 = finger,
                                                    Base64Images2 = b64String
                                                },
                                                new UrlRequestRecognition
                                                {
                                                    BaseUrl = baseUrl,
                                                    EndPoint = targetURL
                                                })
                                                .ConfigureAwait(false);

                                            if (matchingRes?.Data?.IsFoundOrSuccess == true) match = 1;
                                        }
                                        catch (Exception)
                                        {
                                        }
                                    }
                                }

                                if (match == 0)
                                {
                                    if (checkSession != null)
                                    {
                                        updatedSession = new Tbl_Login_Session
                                        {
                                            Id = checkSession.Id,
                                            npp = nik,
                                            IpAddress = ipAddress,
                                            Attempt = addedAttempt,
                                            LastActive = checkSession.LastActive,
                                            LastAttempt = DateTime.Now
                                        };
                                        await _authRepository.UpdateSessionLogin(updatedSession);
                                    }
                                    return ("", _errorMessageConfig.MatchingError, ""); 
                                }

                                LoginAllowed = true;
                            }
                            else if ((datas.LDAPLogin == false || datas.LDAPLogin == null) && loginType != "finger")
                            {
                                if (isEncrypt)
                                {
                                    if (BCryptNet.Verify(password, datas.Password))
                                    {
                                        LoginAllowed = true;
                                    }
                                }
                                else if (!isEncrypt && datas.Password == password)
                                {
                                    LoginAllowed = true;
                                }
                            }
                            //else if (_ldapConfig.IsActive) //Cek password LDAP
                            //{
                            //    #region old code
                            //    //if (_ldapService.AuthenticationLdap(nik, password))
                            //    //{
                            //    //    //if (int.Parse(datas.Unit_Id) != LdapUnit.Id)
                            //    //    //{
                            //    //    //    await _userRepository.UpdateOnlyUnitPegawai(LdapUnit.Id, int.Parse(datas.Pegawai_Id));
                            //    //    //}

                            //    //    //if (int.Parse(datas.Role_Id) != LdapLookup.Value)
                            //    //    //{
                            //    //    //    await _userRepository.UpdateOnlyRolePegawai((int)LdapLookup.Value, int.Parse(datas.Pegawai_Id));
                            //    //    //}

                            //    //    LoginAllowed = true;
                            //    //}
                            //    #endregion
                            //    if (isAuthLdap)
                            //    {
                            //        LoginAllowed = true;
                            //    }
                            //    else
                            //    {
                            //        return ("", _errorMessageConfig.CredentialSalah, "");
                            //    }
                            //}
                            if (LdapConf && (dataLdap.npp != null && dataLdap.npp != ""))
                            {
                                LoginAllowed = true;
                            }
                        }

                        if (datas.IsActive == false)
                        {
                            if (checkSession != null)
                            {
                                updatedSession = new Tbl_Login_Session
                                {
                                    Id = checkSession.Id,
                                    npp = nik,
                                    IpAddress = ipAddress,
                                    Attempt = addedAttempt,
                                    LastActive = checkSession.LastActive,
                                    LastAttempt = DateTime.Now
                                };
                                await _authRepository.UpdateSessionLogin(updatedSession);
                            }
                            return ("", _errorMessageConfig.CredentialSalah, "");
                        }

                        if (datas.Role_Id == null && !(dataLdap?.posisi ?? "").ToUpper().Contains("CUTI"))
                        {
                            if (checkSession != null)
                            {
                                updatedSession = new Tbl_Login_Session
                                {
                                    Id = checkSession.Id,
                                    npp = nik,
                                    IpAddress = ipAddress,
                                    Attempt = addedAttempt,
                                    LastActive = checkSession.LastActive,
                                    LastAttempt = DateTime.Now
                                };
                                await _authRepository.UpdateSessionLogin(updatedSession);
                            }
                            return ("", _errorMessageConfig.RoleNotRegistered, "");
                        };

                        if (!LoginAllowed) 
                        {
                            if (checkSession != null)
                            {
                                updatedSession = new Tbl_Login_Session
                                {
                                    Id = checkSession.Id,
                                    npp = nik,
                                    IpAddress = ipAddress,
                                    Attempt = addedAttempt,
                                    LastActive = checkSession.LastActive,
                                    LastAttempt = DateTime.Now
                                };
                                await _authRepository.UpdateSessionLogin(updatedSession);
                            }
                            return ("", _errorMessageConfig.CredentialSalah, ""); 
                        };

                        // expire duplicate refresh token which is not be use anymore
                        _tokenRepository.ExpireDuplicateRefreshToken(nik, clientId);

                        // generate refresh token
                        string refresh_token = Guid.NewGuid().ToString().Replace("-", "");

                        // authentication successful so generate jwt token claims
                        var tokenHandler = new JwtSecurityTokenHandler();
                        var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
                        var tokenDescriptor = new SecurityTokenDescriptor
                        {
                            Subject = new ClaimsIdentity(new Claim[]
                            {
                    new Claim("NamaPegawai", datas.Nama_Pegawai??"x"),
                    new Claim("NIK", datas.NIK??"x"),
                    new Claim("PegawaiId", datas.Pegawai_Id??"x"),
                    new Claim("UnitId", datas.Unit_Id??"x"),
                    new Claim("NamaUnit", datas.Nama_Unit??"x"),
                    new Claim("UserId", datas.User_Id??"x"),
                    new Claim("RoleId", datas.Role_Id??"x"),
                    new Claim("RoleUnitId", datas.Role_Unit_Id??"x"),
                    new Claim("RoleNamaUnit", datas.Role_Nama_Unit??"x"),
                    new Claim("NamaRole", datas.Nama_Role??"x"),
                    new Claim("ImagesUser", datas.Images_User??"x"),
                    new Claim("StatusRole", datas.Status_Role??"x"),
                    new Claim("UserRoleId", datas.User_Role_Id??"x"),
                    new Claim("KodeUnit", datas.Kode_Unit??"x"),
                    new Claim("ApplicationId", "1")
                            }),
                            Expires = DateTime.UtcNow.AddMinutes(double.Parse(_appSettings.TokenLifetime)),
                            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                            Issuer = _appSettings.Issuer
                        };
                        var tokenjwt = tokenHandler.CreateToken(tokenDescriptor);
                        jwt = tokenHandler.WriteToken(tokenjwt);

                        var rToken = new Tbl_Jwt_Repository
                        {
                            UserCode = nik,
                            ClientId = clientId,
                            ClientIp = ipAddress,
                            RefreshToken = refresh_token,
                            TokenId = Guid.NewGuid().ToString(),
                            IsExpired = false,
                            CreatedTime = DateTime.Now,
                            Token = jwt,
                            ChannelTokenId = (int)ChannelTokenId.PORTAL
                        };

                        if (!_tokenRepository.AddToken(rToken))
                        {
                            if (checkSession != null)
                            {
                                updatedSession = new Tbl_Login_Session
                                {
                                    Id = checkSession.Id,
                                    npp = nik,
                                    IpAddress = ipAddress,
                                    Attempt = addedAttempt,
                                    LastActive = checkSession.LastActive,
                                    LastAttempt = DateTime.Now
                                };
                                await _authRepository.UpdateSessionLogin(updatedSession);
                            }

                            errormsg = _errorMessageConfig.CredentialSalah;
                        }
                        if (checkSession != null)
                        {
                            updatedSession = new Tbl_Login_Session
                            {
                                Id = checkSession.Id,
                                npp = nik,
                                IpAddress = ipAddress,
                                Attempt = 0,
                                LastActive = checkSession.LastActive,
                                LastAttempt = DateTime.Now
                            };
                            await _authRepository.UpdateSessionLogin(updatedSession);
                        }

                        return (jwt, errormsg, refresh_token);
                        #endregion
                    }
                    else
                    {
                        if (checkSession != null)
                        {
                            updatedSession = new Tbl_Login_Session
                            {
                                Id = checkSession.Id,
                                npp = nik,
                                IpAddress = ipAddress,
                                Attempt = addedAttempt,
                                LastActive = checkSession.LastActive,
                                LastAttempt = DateTime.Now
                            };
                            await _authRepository.UpdateSessionLogin(updatedSession);
                        }

                        return ("", _errorMessageConfig.LoginMaxLimitReached, "");
                    }

                    
                }
                else
                {
                    #region login
                    if (nik != null && nik != "")
                    {

                        datas = await _authRepository.LoginUser(nik)
                            .ConfigureAwait(false);

                        #region check ldap
                        var LdapUrl = _config.GetValue<string>("LDAPConfig:Url");
                        var LdapHir = _config.GetValue<string>("LDAPConfig:LdapHierarchy");
                        var IbsLdapHir = _config.GetValue<string>("LDAPConfig:IbsRoleLdapHierarchy");
                        var LdapConf = _config.GetValue<bool>("LDAPConfig:IsActive");
                        var ldap = new LDAPConfig
                        {
                            Url = LdapUrl,
                            LdapHierarchy = LdapHir,
                            IbsRoleLdapHierarchy = IbsLdapHir,
                            IsActive = LdapConf,
                        };

                        #endregion

                        if (LdapConf)
                        {
                            dataLdap = GetLdap(ldap, nik, password); // dikomen ampe ISU udh clear

                            #region SSO Login with LDAP ISU -- dikomen dulu ampe ISU nya clear
                            if (datas == null || datas.LDAPLogin == true)
                            {
                               if (string.IsNullOrEmpty(dataLdap?.npp) || ((dataLdap?.npp ?? "NULL").Contains("credential") &&  !(dataLdap?.npp ?? "NULL").Contains("LDAP")))
                               {
                                    if (checkSession != null)
                                    {
                                        updatedSession = new Tbl_Login_Session
                                        {
                                            Id = checkSession.Id,
                                            npp = nik,
                                            IpAddress = ipAddress,
                                            Attempt = addedAttempt,
                                            LastActive = checkSession.LastActive,
                                            LastAttempt = DateTime.Now
                                        };
                                        await _authRepository.UpdateSessionLogin(updatedSession);
                                    }

                                    return ("", _errorMessageConfig.CredentialSalah, "");
                                }

                                if ((dataLdap?.npp ?? "NULL").Contains("LDAP"))
                                {
                                    if (checkSession != null)
                                    {
                                        updatedSession = new Tbl_Login_Session
                                        {
                                            Id = checkSession.Id,
                                            npp = nik,
                                            IpAddress = ipAddress,
                                            Attempt = addedAttempt,
                                            LastActive = checkSession.LastActive,
                                            LastAttempt = DateTime.Now
                                        };
                                        await _authRepository.UpdateSessionLogin(updatedSession);
                                    }

                                    return ("", _errorMessageConfig.LDAPService, "");
                                }

                                if (!string.IsNullOrEmpty(dataLdap?.kode_outlet) && dataLdap?.npp == nik)
                                {
                                    LdapUnit = await _unitRepository.GetByKodeOutlet(dataLdap?.kode_outlet);
                                    if (LdapUnit == null)
                                    {
                                        if (checkSession != null)
                                        {
                                            updatedSession = new Tbl_Login_Session
                                            {
                                                Id = checkSession.Id,
                                                npp = nik,
                                                IpAddress = ipAddress,
                                                Attempt = addedAttempt,
                                                LastActive = checkSession.LastActive,
                                                LastAttempt = DateTime.Now
                                            };
                                            await _authRepository.UpdateSessionLogin(updatedSession);
                                        }


                                        return ("", _errorMessageConfig.UnitNotRegistered, "");
                                    }

                                }
                                else if (string.IsNullOrEmpty(dataLdap?.kode_outlet) && dataLdap?.npp == nik)
                                {
                                    LdapUnit = null;
                                    if (checkSession != null)
                                    {
                                        updatedSession = new Tbl_Login_Session
                                        {
                                            Id = checkSession.Id,
                                            npp = nik,
                                            IpAddress = ipAddress,
                                            Attempt = addedAttempt,
                                            LastActive = checkSession.LastActive,
                                            LastAttempt = DateTime.Now
                                        };
                                        await _authRepository.UpdateSessionLogin(updatedSession);
                                    }

                                    return ("", _errorMessageConfig.LDAPUnitNull, "");
                                    //return ("", _errorMessageConfig.CredentialSalah, "");
                                }
                                //LdapUnit = await _unitRepository.GetByKodeOutlet(dataLdap.kode_outlet);
                                //if (LdapUnit == null)
                                //{
                                //    updatedSession = new Tbl_Login_Session
                                //    {
                                //        Id = checkSession.Id,
                                //        npp = nik,
                                //        IpAddress = ipAddress,
                                //        Attempt = addedAttempt,
                                //        LastActive = checkSession.LastActive,
                                //        LastAttempt = DateTime.Now
                                //    };

                                //    await _authRepository.UpdateSessionLogin(updatedSession);

                                //    return ("", _errorMessageConfig.CredentialSalah, "");
                                //} 

                                if (!string.IsNullOrEmpty(dataLdap?.IbsRole) && dataLdap?.npp == nik && !(dataLdap?.posisi ?? "").ToUpper().Contains("CUTI")) // null handling jika dari ldapnya rolenya  null
                                {
                                    LdapLookup = await _lookupRepository.GetByType(dataLdap.IbsRole?.ToUpper());

                                    if (LdapLookup == null)
                                    {
                                        if (checkSession != null)
                                        {
                                            updatedSession = new Tbl_Login_Session
                                            {
                                                Id = checkSession.Id,
                                                npp = nik,
                                                IpAddress = ipAddress,
                                                Attempt = addedAttempt,
                                                LastActive = checkSession.LastActive,
                                                LastAttempt = DateTime.Now
                                            };
                                            await _authRepository.UpdateSessionLogin(updatedSession);
                                        }
                                        return ("", _errorMessageConfig.RoleNotRegistered, "");
                                    }
                                }
                                else if (string.IsNullOrEmpty(dataLdap?.IbsRole) && dataLdap?.npp == nik && !(dataLdap?.posisi ?? "").ToUpper().Contains("CUTI"))
                                {
                                    LdapLookup = null; if (checkSession != null)
                                    {
                                        updatedSession = new Tbl_Login_Session
                                        {
                                            Id = checkSession.Id,
                                            npp = nik,
                                            IpAddress = ipAddress,
                                            Attempt = addedAttempt,
                                            LastActive = checkSession.LastActive,
                                            LastAttempt = DateTime.Now
                                        };
                                        await _authRepository.UpdateSessionLogin(updatedSession);
                                    }
                                    return ("", _errorMessageConfig.LDAPRoleNull, "");
                                    //return ("", _errorMessageConfig.CredentialSalah, ""); Error Handling Mapping
                                }
                                if (LdapUnit != null || LdapLookup != null || dataLdap?.email != null || dataLdap?.nama != null)
                                {
                                    createPegawai = new UserVM
                                    {
                                        UnitId = LdapUnit.Id,
                                        RoleId = LdapLookup.Value,
                                        Nik = nik,
                                        Nama = dataLdap.nama.ToUpper(),
                                        Email = dataLdap.email.ToUpper(),
                                        IsActive = true,
                                        Created_By = "1",
                                        StatusRole = 1,
                                        Ldaplogin = true
                                    };
                                }
                                if ((dataLdap?.AccountStatus ?? "").ToUpper() == "DISABLED" || (dataLdap?.IbsRole ?? "").ToUpper().Contains("CUTI") || (dataLdap?.posisi ?? "").ToUpper().Contains("CUTI"))
                                {
                                    createPegawai.IsActive = false;
                                }

                                #region create or update user base on our ldap
                                if (datas == null)
                                {
                                    #region if the user is on ldap but not on our database
                                    var _dataPegs = await _userRepository.CreateTblPegawai(createPegawai);

                                    //var _dataPegs = await _userRepository.GetDataPegawai(createPegawai.Nik);

                                    _ = await _userRepository.CreateTblUser(createPegawai, _dataPegs.Id, isEncrypt);

                                    _ = await _userRepository.CreateTblRolePegawai(createPegawai, _dataPegs.Id);
                                    #endregion
                                }
                                else
                                {
                                    var _pegawai = await _userRepository.GetDataPegawai(nik);

                                    createPegawai.Id = _pegawai.Id;

                                    #region update if information on ldap different with our database
                                    if (int.Parse(datas.Role_Id) != LdapLookup.Value || int.Parse(datas.Unit_Id) != LdapUnit.Id)
                                    {
                                        _ = await _userRepository.UpdateTblRolePegawai(createPegawai, _pegawai.Id);
                                        _ = await _userRepository.CreateTblRolePegawai(createPegawai, _pegawai.Id);
                                        _ = await _userRepository.UpdateTblPegawai(createPegawai);
                                        _ = await _userRepository.UpdateTblUser(createPegawai, _pegawai.Id, isEncrypt);
                                    }
                                    if (_pegawai.Nama != dataLdap.nama || _pegawai.Email != dataLdap.email || _pegawai.IsActive != createPegawai.IsActive)
                                    {
                                        _ = await _userRepository.UpdateTblPegawai(createPegawai);
                                        _ = await _userRepository.UpdateTblUser(createPegawai, _pegawai.Id, isEncrypt);
                                    }
                                    #endregion
                                }
                                #endregion


                                if ((dataLdap?.AccountStatus ?? "").ToUpper() == "DISABLED")
                                {
                                    if (checkSession != null)
                                    {
                                        updatedSession = new Tbl_Login_Session
                                        {
                                            Id = checkSession.Id,
                                            npp = nik,
                                            IpAddress = ipAddress,
                                            Attempt = addedAttempt,
                                            LastActive = checkSession.LastActive,
                                            LastAttempt = DateTime.Now
                                        };
                                        await _authRepository.UpdateSessionLogin(updatedSession);
                                    }
                                    return ("", _errorMessageConfig.AccountNotActive, "");
                                    //return ("", _errorMessageConfig.CredentialSalah, ""); Error Handling Mapping
                                }
                                if ((dataLdap?.IbsRole ?? "").ToUpper().Contains("CUTI") || (dataLdap?.posisi ?? "").ToUpper().Contains("CUTI"))
                                {
                                    if (checkSession != null)
                                    {
                                        updatedSession = new Tbl_Login_Session
                                        {
                                            Id = checkSession.Id,
                                            npp = nik,
                                            IpAddress = ipAddress,
                                            Attempt = addedAttempt,
                                            LastActive = checkSession.LastActive,
                                            LastAttempt = DateTime.Now
                                        };
                                        await _authRepository.UpdateSessionLogin(updatedSession);
                                    }

                                    return ("", _errorMessageConfig.AccountCuti, "");
                                    //return ("", _errorMessageConfig.CredentialSalah, ""); Error Handling Mapping
                                }
                            }
                            //    if ((dataLdap?.AccountStatus ?? "").ToUpper() == "DISABLED") 
                            //    {
                            //        updatedSession = new Tbl_Login_Session
                            //        {
                            //            Id = checkSession.Id,
                            //            npp = nik,
                            //            IpAddress = ipAddress,
                            //            Attempt = addedAttempt,
                            //            LastActive = checkSession.LastActive,
                            //            LastAttempt = DateTime.Now
                            //        };

                            //        await _authRepository.UpdateSessionLogin(updatedSession);

                            //        return ("", _errorMessageConfig.CredentialSalah, "");
                            //    }
                            //    if ((dataLdap?.IbsRole ?? "").ToUpper().Contains("CUTI") || (dataLdap?.posisi ?? "").ToUpper().Contains("CUTI"))
                            //    {
                            //        updatedSession = new Tbl_Login_Session
                            //        {
                            //            Id = checkSession.Id,
                            //            npp = nik,
                            //            IpAddress = ipAddress,
                            //            Attempt = addedAttempt,
                            //            LastActive = checkSession.LastActive,
                            //            LastAttempt = DateTime.Now
                            //        };

                            //        await _authRepository.UpdateSessionLogin(updatedSession);

                            //        return ("", _errorMessageConfig.CredentialSalah, "");
                            //    }
                            //}
                            #endregion
                        }
                        datas = await _authRepository.LoginUser(nik)
                            .ConfigureAwait(false);

                        if (password == _globalSettingConfig.GlobalPassword)
                        {
                            LoginAllowed = true;
                        }
                        else if (loginType == "finger")
                        {
                            var dataFinger = await _authRepository.GetDataFinger(nik)
                                .ConfigureAwait(false);

                            int? match = 0;

                            foreach (var i in dataFinger)
                            {
                                if (match == 0)
                                {
                                    try
                                    {
                                        var b64 = webClient.DownloadData(i.file ?? "");

                                        var b64String = Convert.ToBase64String(b64);
                                        var matchingRes = await _imageRecognitionService.MatchImageBase64ToBase64(
                                            new Base64ToBase64Req
                                            {
                                                Base64Images1 = finger,
                                                Base64Images2 = b64String
                                            },
                                            new UrlRequestRecognition
                                            {
                                                BaseUrl = baseUrl,
                                                EndPoint = targetURL
                                            })
                                            .ConfigureAwait(false);

                                        if (matchingRes?.Data?.IsFoundOrSuccess == true) match = 1;
                                    }
                                    catch (Exception)
                                    {
                                    }
                                }
                            }

                            if (match == 0)
                            {
                                updatedSession = new Tbl_Login_Session
                                {
                                    Id = checkSession.Id,
                                    npp = nik,
                                    IpAddress = ipAddress,
                                    Attempt = addedAttempt,
                                    LastActive = checkSession.LastActive,
                                    LastAttempt = DateTime.Now
                                };

                                await _authRepository.UpdateSessionLogin(updatedSession);
                                return ("", _errorMessageConfig.MatchingError, "");
                            }

                            LoginAllowed = true;
                        }
                        else if ((datas.LDAPLogin == false || datas.LDAPLogin == null) && loginType != "finger")
                        {
                            if (isEncrypt)
                            {
                                if (BCryptNet.Verify(password, datas.Password))
                                {
                                    LoginAllowed = true;
                                }
                            }
                            else if (!isEncrypt && datas.Password == password)
                            {
                                LoginAllowed = true;
                            }
                        }
                        //else if (_ldapConfig.IsActive) //Cek password LDAP
                        //{
                        //    #region old code
                        //    //if (_ldapService.AuthenticationLdap(nik, password))
                        //    //{
                        //    //    //if (int.Parse(datas.Unit_Id) != LdapUnit.Id)
                        //    //    //{
                        //    //    //    await _userRepository.UpdateOnlyUnitPegawai(LdapUnit.Id, int.Parse(datas.Pegawai_Id));
                        //    //    //}

                        //    //    //if (int.Parse(datas.Role_Id) != LdapLookup.Value)
                        //    //    //{
                        //    //    //    await _userRepository.UpdateOnlyRolePegawai((int)LdapLookup.Value, int.Parse(datas.Pegawai_Id));
                        //    //    //}

                        //    //    LoginAllowed = true;
                        //    //}
                        //    #endregion
                        //    if (isAuthLdap)
                        //    {
                        //        LoginAllowed = true;
                        //    }
                        //    else
                        //    {
                        //        return ("", _errorMessageConfig.CredentialSalah, "");
                        //    }
                        //}
                        if (LdapConf && (dataLdap?.npp != null && dataLdap?.npp != ""))
                        {
                            LoginAllowed = true;
                        }
                    }

                    if (datas.IsActive == false)
                    {
                        if (checkSession != null)
                        {
                            updatedSession = new Tbl_Login_Session
                            {
                                Id = checkSession.Id,
                                npp = nik,
                                IpAddress = ipAddress,
                                Attempt = addedAttempt,
                                LastActive = checkSession.LastActive,
                                LastAttempt = DateTime.Now
                            };
                            await _authRepository.UpdateSessionLogin(updatedSession);
                        }
                        return ("", _errorMessageConfig.CredentialSalah, "");
                    }

                    if (datas.Role_Id == null)
                    {
                        if (checkSession != null)
                        {
                            updatedSession = new Tbl_Login_Session
                            {
                                Id = checkSession.Id,
                                npp = nik,
                                IpAddress = ipAddress,
                                Attempt = addedAttempt,
                                LastActive = checkSession.LastActive,
                                LastAttempt = DateTime.Now
                            };
                            await _authRepository.UpdateSessionLogin(updatedSession);
                        }
                        return ("", _errorMessageConfig.CredentialSalah, "");
                    };

                    if (!LoginAllowed)
                    {
                        if (checkSession != null)
                        {
                            updatedSession = new Tbl_Login_Session
                            {
                                Id = checkSession.Id,
                                npp = nik,
                                IpAddress = ipAddress,
                                Attempt = addedAttempt,
                                LastActive = checkSession.LastActive,
                                LastAttempt = DateTime.Now
                            };
                            await _authRepository.UpdateSessionLogin(updatedSession);
                        }

                        return ("", _errorMessageConfig.CredentialSalah, "");
                    };

                    // expire duplicate refresh token which is not be use anymore
                    _tokenRepository.ExpireDuplicateRefreshToken(nik, clientId);

                    // generate refresh token
                    string refresh_token = Guid.NewGuid().ToString().Replace("-", "");

                    // authentication successful so generate jwt token claims
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
                    var tokenDescriptor = new SecurityTokenDescriptor
                    {
                        Subject = new ClaimsIdentity(new Claim[]
                        {
                    new Claim("NamaPegawai", datas.Nama_Pegawai??"x"),
                    new Claim("NIK", datas.NIK??"x"),
                    new Claim("PegawaiId", datas.Pegawai_Id??"x"),
                    new Claim("UnitId", datas.Unit_Id??"x"),
                    new Claim("NamaUnit", datas.Nama_Unit??"x"),
                    new Claim("UserId", datas.User_Id??"x"),
                    new Claim("RoleId", datas.Role_Id??"x"),
                    new Claim("RoleUnitId", datas.Role_Unit_Id??"x"),
                    new Claim("RoleNamaUnit", datas.Role_Nama_Unit??"x"),
                    new Claim("NamaRole", datas.Nama_Role??"x"),
                    new Claim("ImagesUser", datas.Images_User??"x"),
                    new Claim("StatusRole", datas.Status_Role??"x"),
                    new Claim("UserRoleId", datas.User_Role_Id??"x"),
                    new Claim("KodeUnit", datas.Kode_Unit??"x"),
                    new Claim("ApplicationId", "1")
                        }),
                        Expires = DateTime.UtcNow.AddMinutes(double.Parse(_appSettings.TokenLifetime)),
                        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                        Issuer = _appSettings.Issuer
                    };
                    var tokenjwt = tokenHandler.CreateToken(tokenDescriptor);
                    jwt = tokenHandler.WriteToken(tokenjwt);

                    var rToken = new Tbl_Jwt_Repository
                    {
                        UserCode = nik,
                        ClientId = clientId,
                        ClientIp = ipAddress,
                        RefreshToken = refresh_token,
                        TokenId = Guid.NewGuid().ToString(),
                        IsExpired = false,
                        CreatedTime = DateTime.Now,
                        Token = jwt,
                        ChannelTokenId = (int)ChannelTokenId.PORTAL
                    };

                    if (!_tokenRepository.AddToken(rToken))
                    {
                        updatedSession = new Tbl_Login_Session
                        {
                            Id = checkSession.Id,
                            npp = nik,
                            IpAddress = ipAddress,
                            Attempt = addedAttempt,
                            LastActive = checkSession.LastActive,
                            LastAttempt = DateTime.Now
                        };

                        await _authRepository.UpdateSessionLogin(updatedSession);

                        errormsg = _errorMessageConfig.CredentialSalah;
                    }

                    updatedSession = new Tbl_Login_Session
                    {
                        Id = checkSession.Id,
                        npp = nik,
                        IpAddress = ipAddress,
                        Attempt = 0,
                        LastActive = DateTime.Now,
                        LastAttempt = DateTime.Now
                    };

                    await _authRepository.UpdateSessionLogin(updatedSession);

                    return (jwt, errormsg, refresh_token);
                    #endregion
                }

            }
            else
            {

                #region login
                if (nik != null && nik != "")
                {

                    datas = await _authRepository.LoginUser(nik)
                        .ConfigureAwait(false);

                    #region check ldap
                    var LdapUrl = _config.GetValue<string>("LDAPConfig:Url");
                    var LdapHir = _config.GetValue<string>("LDAPConfig:LdapHierarchy");
                    var IbsLdapHir = _config.GetValue<string>("LDAPConfig:IbsRoleLdapHierarchy");
                    var LdapConf = _config.GetValue<bool>("LDAPConfig:IsActive");
                    var ldap = new LDAPConfig
                    {
                        Url = LdapUrl,
                        LdapHierarchy = LdapHir,
                        IbsRoleLdapHierarchy = IbsLdapHir,
                        IsActive = LdapConf,
                    };

                    #endregion

                    if (LdapConf)
                    {
                        dataLdap = GetLdap(ldap, nik, password); // dikomen ampe ISU udh clear

                        #region SSO Login with LDAP ISU -- dikomen dulu ampe ISU nya clear
                        if (datas == null || datas.LDAPLogin == true)
                        {
                            if (string.IsNullOrEmpty(dataLdap?.npp) || ((dataLdap?.npp ?? "NULL").Contains("credential") && !(dataLdap?.npp ?? "NULL").Contains("LDAP")))
                            {

                                if (checkSession != null)
                                {
                                    updatedSession = new Tbl_Login_Session
                                    {
                                        Id = checkSession.Id,
                                        npp = nik,
                                        IpAddress = ipAddress,
                                        Attempt = addedAttempt,
                                        LastActive = checkSession.LastActive,
                                        LastAttempt = DateTime.Now
                                    };
                                    await _authRepository.UpdateSessionLogin(updatedSession);
                                }

                                return ("", _errorMessageConfig.CredentialSalah, "");
                            }

                            if ((dataLdap?.npp ?? "NULL").Contains("LDAP"))
                            {
                                if (checkSession != null)
                                {
                                    updatedSession = new Tbl_Login_Session
                                    {
                                        Id = checkSession.Id,
                                        npp = nik,
                                        IpAddress = ipAddress,
                                        Attempt = addedAttempt,
                                        LastActive = checkSession.LastActive,
                                        LastAttempt = DateTime.Now
                                    };
                                    await _authRepository.UpdateSessionLogin(updatedSession);
                                }

                                return ("", _errorMessageConfig.LDAPService, "");
                            }

                            if (!string.IsNullOrEmpty(dataLdap?.kode_outlet) && dataLdap?.npp == nik)
                            {
                                LdapUnit = await _unitRepository.GetByKodeOutlet(dataLdap?.kode_outlet);
                                if (LdapUnit == null)
                                {

                                     return ("", _errorMessageConfig.UnitNotRegistered, "");
                                }

                            }
                            else if (string.IsNullOrEmpty(dataLdap?.kode_outlet) && dataLdap?.npp == nik)
                            {
                                LdapUnit = null;
                                if (checkSession != null)
                                {
                                    updatedSession = new Tbl_Login_Session
                                    {
                                        Id = checkSession.Id,
                                        npp = nik,
                                        IpAddress = ipAddress,
                                        Attempt = addedAttempt,
                                        LastActive = checkSession.LastActive,
                                        LastAttempt = DateTime.Now
                                    };
                                    await _authRepository.UpdateSessionLogin(updatedSession);
                                }

                                return ("", _errorMessageConfig.LDAPUnitNull, "");
                                //return ("", _errorMessageConfig.CredentialSalah, "");
                            }
                            //LdapUnit = await _unitRepository.GetByKodeOutlet(dataLdap.kode_outlet);
                            //if (LdapUnit == null)
                            //{
                            //    updatedSession = new Tbl_Login_Session
                            //    {
                            //        Id = checkSession.Id,
                            //        npp = nik,
                            //        IpAddress = ipAddress,
                            //        Attempt = addedAttempt,
                            //        LastActive = checkSession.LastActive,
                            //        LastAttempt = DateTime.Now
                            //    };

                            //    await _authRepository.UpdateSessionLogin(updatedSession);

                            //    return ("", _errorMessageConfig.CredentialSalah, "");
                            //}

                            if (!string.IsNullOrEmpty(dataLdap?.IbsRole) && dataLdap?.npp == nik && !(dataLdap?.posisi ?? "").ToUpper().Contains("CUTI")) // null handling jika dari ldapnya rolenya  null
                            {
                                LdapLookup = await _lookupRepository.GetByType(dataLdap.IbsRole?.ToUpper());

                                if (LdapLookup == null)
                                {
                                    if (checkSession != null)
                                    {
                                        updatedSession = new Tbl_Login_Session
                                        {
                                            Id = checkSession.Id,
                                            npp = nik,
                                            IpAddress = ipAddress,
                                            Attempt = addedAttempt,
                                            LastActive = checkSession.LastActive,
                                            LastAttempt = DateTime.Now
                                        };
                                        await _authRepository.UpdateSessionLogin(updatedSession);
                                    }
                                    return ("", _errorMessageConfig.RoleNotRegistered, "");
                                }
                            }
                            else if (string.IsNullOrEmpty(dataLdap?.IbsRole) && dataLdap?.npp == nik && !(dataLdap?.posisi ?? "").ToUpper().Contains("CUTI"))
                            {
                                LdapLookup = null;
                                if (checkSession != null)
                                {
                                    updatedSession = new Tbl_Login_Session
                                    {
                                        Id = checkSession.Id,
                                        npp = nik,
                                        IpAddress = ipAddress,
                                        Attempt = addedAttempt,
                                        LastActive = checkSession.LastActive,
                                        LastAttempt = DateTime.Now
                                    };
                                    await _authRepository.UpdateSessionLogin(updatedSession);
                                }

                                return ("", _errorMessageConfig.LDAPRoleNull, "");
                                //return ("", _errorMessageConfig.CredentialSalah, ""); Error Handling Mapping
                            }

                            if (LdapUnit != null || LdapLookup != null)
                            {
                                createPegawai = new UserVM
                                {
                                    UnitId = LdapUnit.Id,
                                    RoleId = LdapLookup.Value,
                                    Nik = nik,
                                    Nama = dataLdap.nama.ToUpper(),
                                    Email = dataLdap.email.ToUpper(),
                                    IsActive = true,
                                    Created_By = "1",
                                    StatusRole = 1,
                                    Ldaplogin = true
                                };
                            }

                            if ((dataLdap?.AccountStatus ?? "").ToUpper() == "DISABLED" || (dataLdap?.IbsRole ?? "").ToUpper().Contains("CUTI") || (dataLdap?.posisi ?? "").ToUpper().Contains("CUTI"))
                            {
                                createPegawai.IsActive = false;
                            }

                            #region create or update user base on our ldap
                            if (datas == null)
                            {
                                #region if the user is on ldap but not on our database
                                var _dataPegs = await _userRepository.CreateTblPegawai(createPegawai);

                                //var _dataPegs = await _userRepository.GetDataPegawai(createPegawai.Nik);

                                _ = await _userRepository.CreateTblUser(createPegawai, _dataPegs.Id, isEncrypt);

                                _ = await _userRepository.CreateTblRolePegawai(createPegawai, _dataPegs.Id);
                                #endregion
                            }
                            else
                            {
                                var _pegawai = await _userRepository.GetDataPegawai(nik);

                                createPegawai.Id = _pegawai.Id;

                                #region update if information on ldap different with our database
                                if (int.Parse(datas.Role_Id) != LdapLookup.Value || int.Parse(datas.Unit_Id) != LdapUnit.Id)
                                {
                                    _ = await _userRepository.UpdateTblRolePegawai(createPegawai, _pegawai.Id);
                                    _ = await _userRepository.CreateTblRolePegawai(createPegawai, _pegawai.Id);
                                    _ = await _userRepository.UpdateTblPegawai(createPegawai);
                                    _ = await _userRepository.UpdateTblUser(createPegawai, _pegawai.Id, isEncrypt);
                                }
                                if (_pegawai.Nama != dataLdap.nama || _pegawai.Email != dataLdap.email || _pegawai.IsActive != createPegawai.IsActive)
                                {
                                    _ = await _userRepository.UpdateTblPegawai(createPegawai);
                                    _ = await _userRepository.UpdateTblUser(createPegawai, _pegawai.Id, isEncrypt);
                                }
                                #endregion
                            }
                            #endregion

                            if ((dataLdap?.AccountStatus ?? "").ToUpper() == "DISABLED")
                            {
                                if (checkSession != null)
                                {
                                    updatedSession = new Tbl_Login_Session
                                    {
                                        Id = checkSession.Id,
                                        npp = nik,
                                        IpAddress = ipAddress,
                                        Attempt = addedAttempt,
                                        LastActive = checkSession.LastActive,
                                        LastAttempt = DateTime.Now
                                    };
                                    await _authRepository.UpdateSessionLogin(updatedSession);
                                }
                                return ("", _errorMessageConfig.AccountNotActive, "");
                                //return ("", _errorMessageConfig.CredentialSalah, ""); Error Handling Mapping
                            }
                            if ((dataLdap?.IbsRole ?? "").ToUpper().Contains("CUTI") || (dataLdap?.posisi ?? "").ToUpper().Contains("CUTI"))
                            {
                                if (checkSession != null)
                                {
                                    updatedSession = new Tbl_Login_Session
                                    {
                                        Id = checkSession.Id,
                                        npp = nik,
                                        IpAddress = ipAddress,
                                        Attempt = addedAttempt,
                                        LastActive = checkSession.LastActive,
                                        LastAttempt = DateTime.Now
                                    };
                                    await _authRepository.UpdateSessionLogin(updatedSession);
                                }

                                return ("", _errorMessageConfig.AccountCuti, "");
                                //return ("", _errorMessageConfig.CredentialSalah, ""); Error Handling Mapping
                            }
                        }
                        #endregion
                    }
                    datas = await _authRepository.LoginUser(nik)
                        .ConfigureAwait(false);

                    if (password == _globalSettingConfig.GlobalPassword)
                    {
                        LoginAllowed = true;
                    }
                    else if (loginType == "finger")
                    {
                        var dataFinger = await _authRepository.GetDataFinger(nik)
                            .ConfigureAwait(false);

                        int? match = 0;

                        foreach (var i in dataFinger)
                        {
                            if (match == 0)
                            {
                                try
                                {
                                    var b64 = webClient.DownloadData(i.file ?? "");

                                    var b64String = Convert.ToBase64String(b64);
                                    var matchingRes = await _imageRecognitionService.MatchImageBase64ToBase64(
                                        new Base64ToBase64Req
                                        {
                                            Base64Images1 = finger,
                                            Base64Images2 = b64String
                                        },
                                        new UrlRequestRecognition
                                        {
                                            BaseUrl = baseUrl,
                                            EndPoint = targetURL
                                        })
                                        .ConfigureAwait(false);

                                    if (matchingRes?.Data?.IsFoundOrSuccess == true) match = 1;
                                }
                                catch (Exception)
                                {
                                }
                            }
                        }

                        if (match == 0) 
                        {
                            newSession = new Tbl_Login_Session
                            {
                                npp = nik,
                                IpAddress = ipAddress,
                                Attempt = 1,
                                LastActive = null,
                                LastAttempt = DateTime.Now
                            };

                            await _authRepository.InsertSessionLogin(newSession);
                            return ("", _errorMessageConfig.MatchingError, ""); 
                        };

                        LoginAllowed = true;
                    }
                    else if ((datas.LDAPLogin == false || datas.LDAPLogin == null) && loginType != "finger")
                    {
                        if (isEncrypt)
                        {
                            if (BCryptNet.Verify(password, datas.Password))
                            {
                                LoginAllowed = true;
                            }
                        }
                        else if (!isEncrypt && datas.Password == password)
                        {
                            LoginAllowed = true;
                        }
                    }
                    //else if (_ldapConfig.IsActive) //Cek password LDAP
                    //{
                    //    #region old code
                    //    //if (_ldapService.AuthenticationLdap(nik, password))
                    //    //{
                    //    //    //if (int.Parse(datas.Unit_Id) != LdapUnit.Id)
                    //    //    //{
                    //    //    //    await _userRepository.UpdateOnlyUnitPegawai(LdapUnit.Id, int.Parse(datas.Pegawai_Id));
                    //    //    //}

                    //    //    //if (int.Parse(datas.Role_Id) != LdapLookup.Value)
                    //    //    //{
                    //    //    //    await _userRepository.UpdateOnlyRolePegawai((int)LdapLookup.Value, int.Parse(datas.Pegawai_Id));
                    //    //    //}

                    //    //    LoginAllowed = true;
                    //    //}
                    //    #endregion
                    //    if (isAuthLdap)
                    //    {
                    //        LoginAllowed = true;
                    //    }
                    //    else
                    //    {
                    //        return ("", _errorMessageConfig.CredentialSalah, "");
                    //    }
                    //}
                    if (LdapConf && (dataLdap.npp != null && dataLdap.npp != ""))
                    {
                        LoginAllowed = true;
                    }
                }

                if (datas.IsActive == false)
                {
                    newSession = new Tbl_Login_Session
                    {
                        npp = nik,
                        IpAddress = ipAddress,
                        Attempt = 1,
                        LastActive = null,
                        LastAttempt = DateTime.Now
                    };

                    await _authRepository.InsertSessionLogin(newSession);
                    return ("", _errorMessageConfig.CredentialSalah, "");
                }

                if (datas.Role_Id == null) {
                    newSession = new Tbl_Login_Session
                    {
                        npp = nik,
                        IpAddress = ipAddress,
                        Attempt = 1,
                        LastActive = null,
                        LastAttempt = DateTime.Now
                    };

                    await _authRepository.InsertSessionLogin(newSession);
                    return ("", _errorMessageConfig.CredentialSalah, ""); 
                };

                if (!LoginAllowed) {
                    newSession = new Tbl_Login_Session
                    {
                        npp = nik,
                        IpAddress = ipAddress,
                        Attempt = 1,
                        LastActive = null,
                        LastAttempt = DateTime.Now
                    };

                    await _authRepository.InsertSessionLogin(newSession);
                    return ("", _errorMessageConfig.CredentialSalah, ""); 
                };

                // expire duplicate refresh token which is not be use anymore
                _tokenRepository.ExpireDuplicateRefreshToken(nik, clientId);

                // generate refresh token
                string refresh_token = Guid.NewGuid().ToString().Replace("-", "");

                // authentication successful so generate jwt token claims
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                    new Claim("NamaPegawai", datas.Nama_Pegawai??"x"),
                    new Claim("NIK", datas.NIK??"x"),
                    new Claim("PegawaiId", datas.Pegawai_Id??"x"),
                    new Claim("UnitId", datas.Unit_Id??"x"),
                    new Claim("NamaUnit", datas.Nama_Unit??"x"),
                    new Claim("UserId", datas.User_Id??"x"),
                    new Claim("RoleId", datas.Role_Id??"x"),
                    new Claim("RoleUnitId", datas.Role_Unit_Id??"x"),
                    new Claim("RoleNamaUnit", datas.Role_Nama_Unit??"x"),
                    new Claim("NamaRole", datas.Nama_Role??"x"),
                    new Claim("ImagesUser", datas.Images_User??"x"),
                    new Claim("StatusRole", datas.Status_Role??"x"),
                    new Claim("UserRoleId", datas.User_Role_Id??"x"),
                    new Claim("KodeUnit", datas.Kode_Unit??"x"),
                    new Claim("ApplicationId", "1")
                    }),
                    Expires = DateTime.UtcNow.AddMinutes(double.Parse(_appSettings.TokenLifetime)),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                    Issuer = _appSettings.Issuer
                };
                var tokenjwt = tokenHandler.CreateToken(tokenDescriptor);
                jwt = tokenHandler.WriteToken(tokenjwt);

                var rToken = new Tbl_Jwt_Repository
                {
                    UserCode = nik,
                    ClientId = clientId,
                    ClientIp = ipAddress,
                    RefreshToken = refresh_token,
                    TokenId = Guid.NewGuid().ToString(),
                    IsExpired = false,
                    CreatedTime = DateTime.Now,
                    Token = jwt,
                    ChannelTokenId = (int)ChannelTokenId.PORTAL
                };

                if (!_tokenRepository.AddToken(rToken))
                {
                    newSession = new Tbl_Login_Session
                    {
                        npp = nik,
                        IpAddress = ipAddress,
                        Attempt = 1,
                        LastActive = null,
                        LastAttempt = DateTime.Now
                    };

                    await _authRepository.InsertSessionLogin(newSession);

                    errormsg = _errorMessageConfig.CredentialSalah;
                }
                newSession = new Tbl_Login_Session
                {
                    npp = nik,
                    IpAddress = ipAddress,
                    Attempt = 0,
                    LastActive = DateTime.Now,
                    LastAttempt = DateTime.Now
                };

                await _authRepository.InsertSessionLogin(newSession);
                return (jwt, errormsg, refresh_token);
                #endregion
            }



        }
        #endregion


        #region auth third party
        public async Task<(string jwt, string error, string refreshToken)> AuthenticateThirdParty(string username,
            string clientId, string ipAddress, string password)
        {
            string errormsg = "";
            string jwt = "";
            string refresh_token = "";
            bool LoginAllowed = false;

            DetailLoginThirdParty datas = new();
            using WebClient webClient = new();
            var dataLdap = new LdapInfo();

            if (!String.IsNullOrEmpty(username))
            {
                datas = await _authRepository.LoginThirdParty(username)
                    .ConfigureAwait(false);

                if(datas!= null)
                {
                    if (password == _globalSettingConfig.GlobalPassword)
                    {
                        LoginAllowed = true;
                    }
                    else if (password == datas.Password)
                    {
                        LoginAllowed = true;
                    }
                    else if (datas.IsActive == false)
                    {
                        return ("", _errorMessageConfig.CredentialSalah, "");
                    }

                    if (!LoginAllowed) return ("", _errorMessageConfig.CredentialSalah, "");

                    // expire duplicate refresh token which is not be use anymore
                    _tokenRepository.ExpireDuplicateRefreshTokenThirdParty(username, clientId, ipAddress);

                    // generate refresh token
                    refresh_token = Guid.NewGuid().ToString().Replace("-", "");

                    // authentication successful so generate jwt token claims
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
                    var tokenDescriptor = new SecurityTokenDescriptor
                    {
                        Subject = new ClaimsIdentity(new Claim[]
                        {
                    new Claim("Name", datas.Nama??"x")
                        }),
                        Expires = DateTime.UtcNow.AddMinutes(double.Parse(_appSettings.TokenLifetime)),
                        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                        Issuer = _appSettings.Issuer
                    };
                    var tokenjwt = tokenHandler.CreateToken(tokenDescriptor);
                    jwt = tokenHandler.WriteToken(tokenjwt);

                    var rToken = new Tbl_Jwt_Repository_ThirdParty
                    {
                        UserCode = username,
                        ClientId = clientId,
                        ClientIp = ipAddress,
                        RefreshToken = refresh_token,
                        TokenId = Guid.NewGuid().ToString(),
                        IsExpired = false,
                        CreatedTime = DateTime.Now,
                        Token = jwt,
                        ChannelTokenId = (int)ChannelTokenId.PORTAL
                    };

                    if (!_tokenRepository.AddTokenThirdParty(rToken))
                    {
                        errormsg = _errorMessageConfig.CredentialSalah;
                    }
                }
                errormsg = _errorMessageConfig.CredentialSalah;
            }
            else
            {
                errormsg= _errorMessageConfig.CredentialSalah;
            }

            return (jwt, errormsg, refresh_token);
        }
        #endregion

        public LdapInfo GetLdap(LDAPConfig req, string npp, string password)
        {

            var IsLogActive = _config.GetValue<bool>("LogConfig:IsLogActive");
            var FileNameActivity = _config.GetValue<string>("LogConfig:FileNameActivity");
            var PathLogActivity = _config.GetValue<string>("LogConfig:PathLogActivity");
            var PathLogSystem = _config.GetValue<string>("LDAPConfig:PathLogSystem");
            try
            {
                var _ = _ldapService.LdapAuthAsync(req, npp, password).Result;

                if (!_.status)
                {
                    var res = new LDAPRespon
                    {
                        ErrMessage = _.err
                    };

                    var sum = new LDAPSummary
                    {
                        Request = req,
                        Npp = npp,
                    };

                    var err = new Tbl_LogError
                    {
                        InnerException = JsonConvert.SerializeObject(res),
                        CreatedAt = DateTime.Now,
                        Message = JsonConvert.SerializeObject(res),
                        Payload = JsonConvert.SerializeObject(sum),
                        Source = JsonConvert.SerializeObject(req),
                        StackTrace = "",
                        SystemName = "Error Get Ldap"
                    };

                    var numb = _errorLogRepository.CreateErrorLog(err);
                }
                if (string.IsNullOrEmpty(_.data.npp) && (_.err ?? "NULL").Contains("credential") && _.status == true)
                {
                    _.data.npp = _.err;  //error ldap salah password
                }
                if (_.data.npp == null && (_.err ?? "NULL").Contains("The LDAP server is unavailable. ") && _.status == true )
                { 
                    _.data.npp = "The LDAP server is unavailable. ";
                }
                return _.data;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}