using Ekr.Business.Contracts.DataKTP;
using Ekr.Core.Configuration;
using Ekr.Core.Constant;
using Ekr.Core.Entities.Auth;
using Ekr.Core.Entities.DataKTP;
using Ekr.Core.Entities.Logging;
using Ekr.Core.Entities.Recognition;
using Ekr.Core.Securities.Symmetric;
using Ekr.Repository.Contracts.Auth;
using Ekr.Repository.Contracts.DataKTP;
using Ekr.Repository.Contracts.Recognition;
using Ekr.Services.Contracts.Recognition;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Ekr.Business.DataKTP
{
    public class ProfileService : IProfileService
    {
        private readonly IImageRecognitionService _imageRecognitionService;
        private readonly IProfileRepository _profileRepository;
        private readonly IFingerRepository _fingerRepository;
        private readonly IAuthRepository _authRepository;
        private readonly ErrorMessageConfig _ErrorMessageConfig;
        private readonly successMessageConfig _SuccessMessageConfig;

        public ProfileService(IImageRecognitionService imageRecognitionService,
            IProfileRepository profileRepository,
            IFingerRepository fingerRepository,
            IOptions<ErrorMessageConfig> options2,
            IOptions<successMessageConfig> options3,
            IAuthRepository authRepository)
        {
            _imageRecognitionService = imageRecognitionService;
            _profileRepository = profileRepository;
            _fingerRepository = fingerRepository;
            _authRepository = authRepository;
            _ErrorMessageConfig = options2.Value;
            _SuccessMessageConfig = options3.Value;
        }

        public async Task<ServiceResponse<ProfileByNik>> GetAuthKTPData(string base64Img, string nik, string fingerType, string baseUrl, string endPoint)
        {
            var finger = await _fingerRepository.GetFingerByType(nik, fingerType)
                .ConfigureAwait(false);

            if (string.IsNullOrEmpty(finger?.Url)) return new ServiceResponse<ProfileByNik> { Message = _ErrorMessageConfig.NIKNotFound, Status = "error" };

            string b64 = "";

            try
            {
                using WebClient webClient = new();

                byte[] data = webClient.DownloadData(finger.Url);

                using var r = new StreamReader(new MemoryStream(data));
                var encryptedText = r.ReadToEnd();
                b64 = encryptedText.Decrypt(Phrase.FileEncryption);
            }
            catch (Exception)
            {
            }

            var matchRes = await _imageRecognitionService.MatchImageBase64ToBase64(new Base64ToBase64Req
            {
                Base64Images1 = base64Img,
                Base64Images2 = b64
            },
            new UrlRequestRecognition
            {
                //BaseUrl = "http://147.139.171.13/ServiceReader",
                //EndPoint = "/api/v1/Fingerprint/Base64ToBase64"
                BaseUrl = baseUrl,
                EndPoint = endPoint
            })
                .ConfigureAwait(false);

            if (matchRes?.Data?.IsFoundOrSuccess == false || matchRes?.Status.Equals("error") == true) return new ServiceResponse<ProfileByNik> { Status = matchRes.Status, Message = matchRes.Message };

            var profile = await _profileRepository.GetProfileByNik(nik).ConfigureAwait(false);

            return new ServiceResponse<ProfileByNik>
            {
                Status = matchRes.Status,
                Message = matchRes.Message,
                Data = ConvertUrlToB64(profile)
            };
        }

        public async Task<ServiceResponse<ProfileByNik>> GetAuthKTPDataEmp(string base64Img, string nik, string fingerType, string baseUrl, string endPoint)
        {
            var finger = await _fingerRepository.GetFingerByTypeEmp(nik, fingerType)
                .ConfigureAwait(false);

            if (string.IsNullOrEmpty(finger?.Url)) return new ServiceResponse<ProfileByNik> { Message = "NIK tidak terdaftar", Status = "error" };

            string b64 = "";

            try
            {
                using WebClient webClient = new();

                byte[] data = webClient.DownloadData(finger.Url);

                using var r = new StreamReader(new MemoryStream(data));
                var encryptedText = r.ReadToEnd();
                b64 = encryptedText.Decrypt(Phrase.FileEncryption);
            }
            catch (Exception)
            {
            }

            var matchRes = await _imageRecognitionService.MatchImageBase64ToBase64(new Base64ToBase64Req
            {
                Base64Images1 = base64Img,
                Base64Images2 = b64
            },
            new UrlRequestRecognition
            {
                //BaseUrl = "http://147.139.171.13/ServiceReader",
                //EndPoint = "/api/v1/Fingerprint/Base64ToBase64"
                BaseUrl = baseUrl,
                EndPoint = endPoint
            })
                .ConfigureAwait(false);

            if (matchRes?.Data?.IsFoundOrSuccess == false || matchRes?.Status.Equals("error") == true) return new ServiceResponse<ProfileByNik> { Status = matchRes.Status, Message = matchRes.Message };

            var profile = await _profileRepository.GetProfileByNikEmp(nik).ConfigureAwait(false);

            return new ServiceResponse<ProfileByNik>
            {
                Status = matchRes.Status,
                Message = matchRes.Message,
                Data = ConvertUrlToB64(profile)
            };
        }

        public async Task<ServiceResponse<ProfileByNik>> GetKTPData(string nik)
        {
            var profile = await _profileRepository.GetProfileByNik(nik).ConfigureAwait(false);

            return new ServiceResponse<ProfileByNik>
            {
                Status = _SuccessMessageConfig.Sukses,
                Message = _SuccessMessageConfig.Sukses,
                Data = ConvertUrlToB64(profile)
            };
        }

        public async Task<ServiceResponse<ProfileByNik>> GetAuthKTPData(string base64Img, string nik, string baseUrl, string endPoint)
        {
            var dataFinger = await _authRepository.GetDataFingerGeneral(nik)
                .ConfigureAwait(false);

            if (dataFinger == null) return new ServiceResponse<ProfileByNik> { Message = _ErrorMessageConfig.NIKNotFound, Status = "error" };

            int? match = 0;

            using WebClient webClient = new();

            foreach (var i in dataFinger)
            {
                if (match == 0)
                {
                    try
                    {
                        var b64 = webClient.DownloadData(i.file ?? "");

                        string b64String = "";

                        using (var r = new StreamReader(new MemoryStream(b64)))
                        {
                            var encryptedText = r.ReadToEnd();
                            b64String = encryptedText.Decrypt(Phrase.FileEncryption);
                        }

                        var matchingRes = await _imageRecognitionService.MatchImageBase64ToBase64(
                            new Base64ToBase64Req
                            {
                                Base64Images1 = base64Img,
                                Base64Images2 = b64String
                            },
                            new UrlRequestRecognition
                            {
                                //BaseUrl = "http://147.139.171.13/ServiceReader",
                                //EndPoint = "/api/v1/Fingerprint/Base64ToBase64"
                                BaseUrl = baseUrl,
                                EndPoint = endPoint
                            })
                            .ConfigureAwait(false);

                        if (matchingRes?.Data?.IsFoundOrSuccess == true) match = 1;
                    }
                    catch
                    {
                    }
                }
            }

            if (match == 0) return new ServiceResponse<ProfileByNik> { Status = "error", Message = _ErrorMessageConfig.MatchingError };

            var profile = await _profileRepository.GetProfileByNik(nik).ConfigureAwait(false);

            return new ServiceResponse<ProfileByNik>
            {
                Status = _SuccessMessageConfig.Sukses,
                Message = _SuccessMessageConfig.MatchingSuccess,
                Data = ConvertUrlToB64(profile)
            };
        }

        public async Task<ServiceResponse<ProfileByNik>> GetKTPDataFingerEncOnly(string nik)
        {
            var profile = await _profileRepository.GetProfileByNik(nik).ConfigureAwait(false);

            return new ServiceResponse<ProfileByNik>
            {
                Status = "sukses",
                Message = _SuccessMessageConfig.Sukses,
                Data = profile == null ? null : ConvertUrlToB64FingerEncOnly(profile)
            };
        }

        public async Task<ServiceResponse<ProfileByNik>> GetKTPDataFingerEncOnlyNoMatching(string nik)
        {
            var profile = await _profileRepository.GetProfileByNikNoMatching(nik).ConfigureAwait(false);

            return new ServiceResponse<ProfileByNik>
            {
                Status = "sukses",
                Message = _SuccessMessageConfig.Sukses,
                Data = profile == null ? null : ConvertUrlToB64FingerEncOnly(profile)
            };
        }

        public async Task<ServiceResponse<ProfileByNik>> GetKTPDataFingerEncOnlyNoMatchingNew(string nik, int Id)
        {
            var profile = await _profileRepository.GetProfileByNikNoMatchingNew(nik, Id).ConfigureAwait(false);

            return new ServiceResponse<ProfileByNik>
            {
                Status = "sukses",
                Message = _SuccessMessageConfig.Sukses,
                Data = profile == null ? null : ConvertUrlToB64FingerEncOnly(profile)
            };
        }

        public async Task<ServiceResponse<ProfileByNik>> GetKTPDataByCifFingerEncOnly(string cif)
        {
            var profile = await _profileRepository.GetProfileByCIF(cif).ConfigureAwait(false);

            return new ServiceResponse<ProfileByNik>
            {
                Status = "sukses",
                Message = _SuccessMessageConfig.Sukses,
                Data = profile == null ? null : ConvertUrlToB64FingerEncOnly(profile)
            };
        }

        public async Task<ServiceResponse<ProfileByNik>> GetAuthKTPDataFingerEncOnly(string base64Img, string nik, string fingerType, string baseUrl, string endPoint)
        {
            var finger = await _fingerRepository.GetFingerByTypeEmp(nik, fingerType)
                .ConfigureAwait(false);

            if (string.IsNullOrEmpty(finger?.Url)) return new ServiceResponse<ProfileByNik> { Message = _ErrorMessageConfig.NIKNotFound, Status = "error" };

            string b64 = "";

            try
            {
                using WebClient webClient = new();

                byte[] data = webClient.DownloadData(finger.Url);

                using var r = new StreamReader(new MemoryStream(data));
                var encryptedText = r.ReadToEnd();
                b64 = encryptedText.Decrypt(Phrase.FileEncryption);
            }
            catch (Exception)
            {
            }

            var matchRes = await _imageRecognitionService.MatchImageBase64ToBase64(new Base64ToBase64Req
            {
                Base64Images1 = base64Img,
                Base64Images2 = b64
            },
            new UrlRequestRecognition
            {
                //BaseUrl = "http://147.139.171.13/ServiceReader",
                //EndPoint = "/api/v1/Fingerprint/Base64ToBase64"
                BaseUrl = baseUrl,
                EndPoint = endPoint
            })
                .ConfigureAwait(false);

            if (matchRes?.Data?.IsFoundOrSuccess == false || matchRes?.Status?.Equals("error") == true) return new ServiceResponse<ProfileByNik> { Status = matchRes?.Status, Message = matchRes?.Message };

            var profile = await _profileRepository.GetProfileByNikEmp(nik).ConfigureAwait(false);

            return new ServiceResponse<ProfileByNik>
            {
                Status = matchRes.Status,
                Message = matchRes.Message,
                Data = ConvertUrlToB64FingerEncOnly(profile)
            };
        }

        public async Task<ServiceResponse<ProfileByNik>> GetAuthKTPDataFingerEncOnlyIso(string base64Img, string nik, string fingerType, string baseUrl, string endPoint)
        {
            var finger = await _fingerRepository.GetFingerByTypeEmpIso(nik, fingerType)
                .ConfigureAwait(false);

            if (string.IsNullOrEmpty(finger?.Url)) return new ServiceResponse<ProfileByNik> { Message = _ErrorMessageConfig.NIKNotFound, Status = "error" };

            string b64 = "";

            try
            {
                using WebClient webClient = new();

                byte[] data = webClient.DownloadData(finger.Url);

                using var r = new StreamReader(new MemoryStream(data));
                var encryptedText = r.ReadToEnd();
                b64 = encryptedText.Decrypt(Phrase.FileEncryption);
            }
            catch (Exception)
            {
            }

            var matchRes = await _imageRecognitionService.MatchImageBase64ToBase64(new Base64ToBase64Req
            {
                Base64Images1 = base64Img,
                Base64Images2 = b64
            },
            new UrlRequestRecognition
            {
                //BaseUrl = "http://147.139.171.13/ServiceReader",
                //EndPoint = "/api/v1/Fingerprint/Base64ToBase64"
                BaseUrl = baseUrl,
                EndPoint = endPoint
            })
                .ConfigureAwait(false);

            if (matchRes?.Data?.IsFoundOrSuccess == false || matchRes?.Status?.Equals("error") == true) return new ServiceResponse<ProfileByNik> { Status = matchRes?.Status, Message = matchRes?.Message };

            var profile = await _profileRepository.GetProfileByNikEmp(nik).ConfigureAwait(false);

            return new ServiceResponse<ProfileByNik>
            {
                Status = matchRes.Status,
                Message = matchRes.Message,
                Data = ConvertUrlToB64FingerEncOnly(profile)
            };
        }


        public async Task<ServiceResponse<ProfileByNik>> GetAuthKTPDataFingerEncOnlyIsoDB(string base64Img, string nik, string fingerType, string baseUrl, string endPoint)
        {
            var finger = await _fingerRepository.GetFingerByTypeEmpIso(nik, fingerType)
                .ConfigureAwait(false);

            if (string.IsNullOrEmpty(finger?.Url)) return new ServiceResponse<ProfileByNik> { Message = _ErrorMessageConfig.NIKNotFound, Status = "error" };

            string b64 = "";

            try
            {
                var encryptedText = finger.FileJari;
                b64 = encryptedText.Decrypt(Phrase.FileEncryption);
            }
            catch (Exception)
            {
            }

            var matchRes = await _imageRecognitionService.MatchImageBase64ToBase64(new Base64ToBase64Req
            {
                Base64Images1 = base64Img,
                Base64Images2 = b64
            },
            new UrlRequestRecognition
            {
                //BaseUrl = "http://147.139.171.13/ServiceReader",
                //EndPoint = "/api/v1/Fingerprint/Base64ToBase64"
                BaseUrl = baseUrl,
                EndPoint = endPoint
            })
                .ConfigureAwait(false);

            if (matchRes?.Data?.IsFoundOrSuccess == false || matchRes?.Status?.Equals("error") == true) return new ServiceResponse<ProfileByNik> { Status = matchRes?.Status, Message = matchRes?.Message };

            var profile = await _profileRepository.GetProfileByNikEmp(nik).ConfigureAwait(false);

            return new ServiceResponse<ProfileByNik>
            {
                Status = matchRes.Status,
                Message = matchRes.Message,
                Data = ConvertUrlToB64FingerEncOnly(profile)
            };
        }

        public async Task<ServiceResponse<ProfileByNik>> GetAuthKTPDataFingerEncOnly(ProfileLoopReq req, string baseUrl, string endPoint, DateTime RequestTime, string AuthNpp, string AuthUnitCode)
        {
            var dataFinger = await _authRepository.GetDataFingerGeneral(req.Nik)
                .ConfigureAwait(false);

            if (!dataFinger.Any()) return new ServiceResponse<ProfileByNik> { Message = _ErrorMessageConfig.MatchingFingerNotFound, Status = "error" };

            int? match = 0;

            using WebClient webClient = new();

            var urlImg1 = "";
            var urlImg2 = "";
            var dec1 = "";
            var dec2 = "";
            var errorMsg = "";

            foreach (var i in dataFinger)
            {
                if (match == 0)
                {
                    var count = 0;
                    try
                    {
                        count++;
                        var b64 = webClient.DownloadData(i.file ?? "");

                        string b64String = "";

                        using (var r = new StreamReader(new MemoryStream(b64)))
                        {
                            var encryptedText = r.ReadToEnd();
                            b64String = encryptedText.Decrypt(Phrase.FileEncryption);
                        }

                        if (count == 1)
                        {
                            urlImg1 = i.file;
                            dec1 = b64String;
                        }
                        else
                        {
                            urlImg2 = i.file;
                            dec2 = b64String;
                        }

                        var matchingRes = await _imageRecognitionService.MatchImageBase64ToBase64(
                            new Base64ToBase64Req
                            {
                                Base64Images1 = req.Base64Img,
                                Base64Images2 = b64String
                            },
                            new UrlRequestRecognition
                            {
                                BaseUrl = baseUrl,
                                EndPoint = endPoint
                            })
                            .ConfigureAwait(false);

                        if (matchingRes?.Data?.IsFoundOrSuccess == true) match = 1;
                    }
                    catch (Exception ex)
                    {
                        errorMsg = ex.Message ?? (ex.InnerException.Message ?? "");
                    }
                }
            }

            if (match == 0)
            {
                var _logs = new Tbl_LogClientApps
                {
                    Param = "nik: " + req.Nik + ",base64Img: " + req.Base64Img,
                    LvTeller = req.LvTeller,
                    Branch = req.Branch,
                    SubBranch = req.SubBranch,
                    ClientApps = req.ClientApps,
                    RequestTime = RequestTime,
                    ResponseTime = DateTime.Now,
                    CreatedTime = DateTime.Now,
                    EndPoint = req.EndPoint,
                    ErrorMessage = errorMsg,
                    CreatedByNpp = AuthNpp,
                    CreatedByUnitCode = AuthUnitCode
                };

                _ = _authRepository.InsertLogClientApps(_logs);

                return new ServiceResponse<ProfileByNik>
                {
                    Status = "error",
                    Message = _ErrorMessageConfig.MatchingError,
                    Data = new ProfileByNik
                    {
                        RequestedImg = req.Base64Img,
                        ImgUrlPath1 = urlImg1,
                        ImgUrlPath2 = urlImg2,
                        ktp_FingerKanan = dec1,
                        ktp_FingerKiri = dec2,
                        ErrorMsg = errorMsg
                    }
                };
            }

            var profile = await _profileRepository.GetProfileByNik(req.Nik).ConfigureAwait(false);

            var _log = new Tbl_LogClientApps
            {
                Param = "nik: " + req.Nik + ",base64Img: " + req.Base64Img,
                LvTeller = req.LvTeller,
                Branch = req.Branch,
                SubBranch = req.SubBranch,
                ClientApps = req.ClientApps,
                RequestTime = RequestTime,
                ResponseTime = DateTime.Now,
                CreatedTime = DateTime.Now,
                EndPoint = req.EndPoint,
                ErrorMessage = errorMsg,
                CreatedByNpp = AuthNpp,
                CreatedByUnitCode = AuthUnitCode
            };

            _ = _authRepository.InsertLogClientApps(_log);

            return new ServiceResponse<ProfileByNik>
            {
                Status = "sukses",
                Message = _SuccessMessageConfig.MatchingSuccess,
                Data = ConvertUrlToB64FingerEncOnly(profile)
            };
        }

        public async Task<ServiceResponse<ProfileByNik>> GetAuthKTPDataFingerEncOnlyISO(ProfileLoopReq req, string baseUrl, string endPoint, DateTime RequestTime, string AuthNpp, string AuthUnitCode)
        {
            var dataFinger = await _authRepository.GetDataFingerGeneralISO(req.Nik)
                .ConfigureAwait(false);

            if (!dataFinger.Any()) return new ServiceResponse<ProfileByNik> { Message = _ErrorMessageConfig.MatchingFingerNotFound, Status = "error" };

            int? match = 0;

            using WebClient webClient = new();

            var urlImg1 = "";
            var urlImg2 = "";
            var dec1 = "";
            var dec2 = "";
            var errorMsg = "";

            foreach (var i in dataFinger)
            {
                if (match == 0)
                {
                    var count = 0;
                    try
                    {
                        count++;
                        var b64 = webClient.DownloadData(i.file ?? "");

                        string b64String = "";

                        using (var r = new StreamReader(new MemoryStream(b64)))
                        {
                            var encryptedText = r.ReadToEnd();
                            b64String = encryptedText.Decrypt(Phrase.FileEncryption);
                        }

                        if (count == 1)
                        {
                            urlImg1 = i.file;
                            dec1 = b64String;
                        }
                        else
                        {
                            urlImg2 = i.file;
                            dec2 = b64String;
                        }

                        var matchingRes = await _imageRecognitionService.MatchImageBase64ToBase64(
                            new Base64ToBase64Req
                            {
                                Base64Images1 = req.Base64Img,
                                Base64Images2 = b64String
                            },
                            new UrlRequestRecognition
                            {
                                BaseUrl = baseUrl,
                                EndPoint = endPoint
                            })
                            .ConfigureAwait(false);

                        if (matchingRes?.Data?.IsFoundOrSuccess == true) match = 1;
                    }
                    catch (Exception ex)
                    {
                        errorMsg = ex.Message ?? (ex.InnerException.Message ?? "");
                    }
                }
            }

            if (match == 0)
            {
                var _logs = new Tbl_LogClientApps
                {
                    Param = "nik: " + req.Nik + ",base64Img: " + req.Base64Img,
                    LvTeller = req.LvTeller,
                    Branch = req.Branch,
                    SubBranch = req.SubBranch,
                    ClientApps = req.ClientApps,
                    RequestTime = RequestTime,
                    ResponseTime = DateTime.Now,
                    CreatedTime = DateTime.Now,
                    EndPoint = req.EndPoint,
                    ErrorMessage = errorMsg,
                    CreatedByNpp = AuthNpp,
                    CreatedByUnitCode = AuthUnitCode
                };

                _ = _authRepository.InsertLogClientApps(_logs);

                return new ServiceResponse<ProfileByNik>
                {
                    Status = "error",
                    Message = _ErrorMessageConfig.MatchingError,
                    Data = new ProfileByNik
                    {
                        RequestedImg = req.Base64Img,
                        ImgUrlPath1 = urlImg1,
                        ImgUrlPath2 = urlImg2,
                        ktp_FingerKanan = dec1,
                        ktp_FingerKiri = dec2,
                        ErrorMsg = errorMsg
                    }
                };
            }

            var profile = await _profileRepository.GetProfileByNikISO(req.Nik).ConfigureAwait(false);

            var _log = new Tbl_LogClientApps
            {
                Param = "nik: " + req.Nik + ",base64Img: " + req.Base64Img,
                LvTeller = req.LvTeller,
                Branch = req.Branch,
                SubBranch = req.SubBranch,
                ClientApps = req.ClientApps,
                RequestTime = RequestTime,
                ResponseTime = DateTime.Now,
                CreatedTime = DateTime.Now,
                EndPoint = req.EndPoint,
                ErrorMessage = errorMsg,
                CreatedByNpp = AuthNpp,
                CreatedByUnitCode = AuthUnitCode
            };

            _ = _authRepository.InsertLogClientApps(_log);

            return new ServiceResponse<ProfileByNik>
            {
                Status = "sukses",
                Message = _SuccessMessageConfig.MatchingSuccess,
                Data = ConvertUrlToB64FingerEncOnly(profile)
            };
        }

        public async Task<ServiceResponse<ProfileByNik>> GetAuthKTPDataFingerEncOnlyISOThirdParty(ProfileLoopReq req, string baseUrl, string endPoint, DateTime RequestTime, string AuthUnitCode)
        {
            IEnumerable<MatchingLoginFinger> dataFinger = Enumerable.Empty<MatchingLoginFinger>();
            if (!string.IsNullOrEmpty(req.Npp))
            {
                dataFinger = await _authRepository.GetDataFingerGeneralEmpIso(req.Nik)
                .ConfigureAwait(false);
            }
            else
            {
                dataFinger = await _authRepository.GetDataFingerGeneralISO(req.Nik)
                .ConfigureAwait(false);
            }
            
            if (!dataFinger.Any()) return new ServiceResponse<ProfileByNik> { Message = _ErrorMessageConfig.MatchingFingerNotFound, Status = "error" };

            int? match = 0;

            using WebClient webClient = new();

            var urlImg1 = "";
            var urlImg2 = "";
            var dec1 = "";
            var dec2 = "";
            var errorMsg = "";

            foreach (var i in dataFinger)
            {
                if (match == 0)
                {
                    var count = 0;
                    try
                    {
                        count++;
                        var b64 = webClient.DownloadData(i.file ?? "");

                        string b64String = "";

                        using (var r = new StreamReader(new MemoryStream(b64)))
                        {
                            var encryptedText = r.ReadToEnd();
                            b64String = encryptedText.Decrypt(Phrase.FileEncryption);
                        }

                        if (count == 1)
                        {
                            urlImg1 = i.file;
                            dec1 = b64String;
                        }
                        else
                        {
                            urlImg2 = i.file;
                            dec2 = b64String;
                        }

                        var matchingRes = await _imageRecognitionService.MatchImageBase64ToBase64(
                            new Base64ToBase64Req
                            {
                                Base64Images1 = req.Base64Img,
                                Base64Images2 = b64String
                            },
                            new UrlRequestRecognition
                            {
                                BaseUrl = baseUrl,
                                EndPoint = endPoint
                            })
                            .ConfigureAwait(false);

                        if (matchingRes?.Data?.IsFoundOrSuccess == true) match = 1;
                    }
                    catch (Exception ex)
                    {
                        errorMsg = ex.Message ?? (ex.InnerException.Message ?? "");
                    }
                }
            }

            if (match == 0)
            {
                var _logs = new Tbl_LogClientApps
                {
                    Param = "nik: " + req.Nik + ",base64Img: " + req.Base64Img,
                    LvTeller = req.LvTeller,
                    Branch = req.Branch,
                    SubBranch = req.SubBranch,
                    ClientApps = req.ClientApps,
                    RequestTime = RequestTime,
                    ResponseTime = DateTime.Now,
                    CreatedTime = DateTime.Now,
                    //EndPoint = req.EndPoint,
                    EndPoint = endPoint,
                    ErrorMessage = errorMsg,
                    CreatedByNpp = string.IsNullOrEmpty(req.Npp) ? "" : req.Npp,
                    CreatedByUnitCode = AuthUnitCode
                };

                _ = _authRepository.InsertLogClientApps(_logs);

                return new ServiceResponse<ProfileByNik>
                {
                    Status = "error",
                    Message = _ErrorMessageConfig.MatchingError,
                    Data = new ProfileByNik
                    {
                        RequestedImg = req.Base64Img,
                        ImgUrlPath1 = urlImg1,
                        ImgUrlPath2 = urlImg2,
                        ktp_FingerKanan = dec1,
                        ktp_FingerKiri = dec2,
                        ErrorMsg = errorMsg
                    }
                };
            }

            var profile = await _profileRepository.GetProfileByNikISO(req.Nik).ConfigureAwait(false);

            var _log = new Tbl_LogClientApps
            {
                Param = "nik: " + req.Nik + ",base64Img: " + req.Base64Img,
                LvTeller = req.LvTeller,
                Branch = req.Branch,
                SubBranch = req.SubBranch,
                ClientApps = req.ClientApps,
                RequestTime = RequestTime,
                ResponseTime = DateTime.Now,
                CreatedTime = DateTime.Now,
                //EndPoint = req.EndPoint,
                EndPoint = endPoint,
                ErrorMessage = errorMsg,
                CreatedByNpp = string.IsNullOrEmpty(req.Npp) ? "" : req.Npp,
                CreatedByUnitCode = AuthUnitCode
            };

            _ = _authRepository.InsertLogClientApps(_log);

            return new ServiceResponse<ProfileByNik>
            {
                Status = "sukses",
                Message = _SuccessMessageConfig.MatchingSuccess,
                Data = ConvertUrlToB64FingerEncOnly(profile)
            };
        }

        public async Task<ServiceResponse<ProfileByNikOnlyImg>> GetAuthKTPDataFingerEncOnlyISOThirdPartyDemo(ProfileLoopReq req, string baseUrl, string endPoint, DateTime RequestTime, string AuthUnitCode)
        {
            IEnumerable<MatchingLoginFinger> dataFinger = Enumerable.Empty<MatchingLoginFinger>();
            if (!string.IsNullOrEmpty(req.Npp))
            {
                dataFinger = await _authRepository.GetDataFingerGeneralEmpIso(req.Nik)
                .ConfigureAwait(false);
            }
            else
            {
                dataFinger = await _authRepository.GetDataFingerGeneralISO(req.Nik)
                .ConfigureAwait(false);
            }

            if (!dataFinger.Any()) return new ServiceResponse<ProfileByNikOnlyImg> { Message = _ErrorMessageConfig.MatchingFingerNotFound, Status = "error" };

            int? match = 0;

            using WebClient webClient = new();

            var urlImg1 = "";
            var urlImg2 = "";
            var dec1 = "";
            var dec2 = "";
            var errorMsg = "";

            foreach (var i in dataFinger)
            {
                if (match == 0)
                {
                    var count = 0;
                    try
                    {
                        count++;
                        var b64 = webClient.DownloadData(i.file ?? "");

                        string b64String = "";

                        using (var r = new StreamReader(new MemoryStream(b64)))
                        {
                            var encryptedText = r.ReadToEnd();
                            b64String = encryptedText.Decrypt(Phrase.FileEncryption);
                        }

                        if (count == 1)
                        {
                            urlImg1 = i.file;
                            dec1 = b64String;
                        }
                        else
                        {
                            urlImg2 = i.file;
                            dec2 = b64String;
                        }

                        var matchingRes = await _imageRecognitionService.MatchImageBase64ToBase64(
                            new Base64ToBase64Req
                            {
                                Base64Images1 = req.Base64Img,
                                Base64Images2 = b64String
                            },
                            new UrlRequestRecognition
                            {
                                BaseUrl = baseUrl,
                                EndPoint = endPoint
                            })
                            .ConfigureAwait(false);

                        if (matchingRes?.Data?.IsFoundOrSuccess == true) match = 1;
                    }
                    catch (Exception ex)
                    {
                        errorMsg = ex.Message ?? (ex.InnerException.Message ?? "");
                    }
                }
            }

            if (match == 0)
            {
                var _logs = new Tbl_LogClientApps
                {
                    Param = "nik: " + req.Nik + ",base64Img: " + req.Base64Img,
                    LvTeller = req.LvTeller,
                    Branch = req.Branch,
                    SubBranch = req.SubBranch,
                    ClientApps = req.ClientApps,
                    RequestTime = RequestTime,
                    ResponseTime = DateTime.Now,
                    CreatedTime = DateTime.Now,
                    //EndPoint = req.EndPoint,
                    EndPoint = endPoint,
                    ErrorMessage = errorMsg,
                    CreatedByNpp = string.IsNullOrEmpty(req.Npp) ? "" : req.Npp,
                    CreatedByUnitCode = AuthUnitCode
                };

                _ = _authRepository.InsertLogClientApps(_logs);

                return new ServiceResponse<ProfileByNikOnlyImg>
                {
                    Status = "error",
                    Message = _ErrorMessageConfig.MatchingError,
                    //Data = new ProfileByNik
                    //{
                    //    RequestedImg = req.Base64Img,
                    //    ImgUrlPath1 = urlImg1,
                    //    ImgUrlPath2 = urlImg2,
                    //    ktp_FingerKanan = dec1,
                    //    ktp_FingerKiri = dec2,
                    //    ErrorMsg = errorMsg
                    //}

                    Data = new ProfileByNikOnlyImg
                    {
                        RequestedImg = req.Base64Img,
                        ErrorMsg = errorMsg
                    }
                };
            }

            var profile = await _profileRepository.GetProfileByNikISO(req.Nik).ConfigureAwait(false);
            //var profile = await _profileRepository.GetDataDemografis(req.Nik).ConfigureAwait(false);

            var _log = new Tbl_LogClientApps
            {
                Param = "nik: " + req.Nik + ",base64Img: " + req.Base64Img,
                LvTeller = req.LvTeller,
                Branch = req.Branch,
                SubBranch = req.SubBranch,
                ClientApps = req.ClientApps,
                RequestTime = RequestTime,
                ResponseTime = DateTime.Now,
                CreatedTime = DateTime.Now,
                //EndPoint = req.EndPoint,
                EndPoint = endPoint,
                ErrorMessage = errorMsg,
                CreatedByNpp = string.IsNullOrEmpty(req.Npp) ? "" : req.Npp,
                CreatedByUnitCode = AuthUnitCode
            };

            _ = _authRepository.InsertLogClientApps(_log);

            return new ServiceResponse<ProfileByNikOnlyImg>
            {
                Status = "sukses",
                Message = _SuccessMessageConfig.MatchingSuccess,
                //Data = ConvertUrlToB64FingerEncOnly(profile)
                Data = new ProfileByNikOnlyImg
                {
                    ktp_NIK = profile.ktp_NIK,
                    ktp_CIF = profile.ktp_CIF,
                    ktp_Nama = profile.ktp_Nama,
                    ktp_TempatLahir = profile.ktp_TempatLahir,
                    ktp_TanggalLahir = profile.ktp_TanggalLahir,
                    ktp_TTL = profile.ktp_TempatLahir + "," + profile.ktp_TanggalLahir,
                    ktp_JanisKelamin = profile.ktp_JanisKelamin,
                    ktp_GolonganDarah = profile.ktp_GolonganDarah,
                    ktp_Alamat = profile.ktp_Alamat,
                    ktp_RT = profile.ktp_RT,
                    ktp_RW = profile.ktp_RW,
                    ktp_Kelurahan = profile.ktp_Kelurahan,
                    Desa = profile.Desa,
                    ktp_Kecamatan = profile.ktp_Kecamatan,
                    ktp_Kota = profile.ktp_Kota,
                    ktp_Provinsi = profile.ktp_Provinsi,
                    ktp_Agama = profile.ktp_Agama,
                    ktp_KodePos = profile.ktp_KodePos,
                    ktp_Latitude = profile.ktp_Latitude,
                    ktp_Longitude = profile.ktp_Longitude,
                    ktp_StatusPerkawinan = profile.ktp_StatusPerkawinan,
                    ktp_Pekerjaan = profile.ktp_Pekerjaan,
                    ktp_Kewarganegaraan = profile.ktp_Kewarganegaraan,
                    ktp_MasaBerlaku = profile.ktp_MasaBerlaku,
                    ktp_PhotoCam = profile.ktp_PhotoCam,
                    ktp_PhotoKTP = profile.ktp_PhotoKTP,
                    ktp_Signature = profile.ktp_Signature,
                    ErrorMsg = profile.ErrorMsg
                }
            };


        }

        public async Task<ServiceResponse<ProfileByNikOnlyFinger>> GetAuthKTPDataFingerEncOnlyISOThirdPartyBio(ProfileLoopReq req, string baseUrl, string endPoint, DateTime RequestTime, string AuthUnitCode)
        {
            IEnumerable<MatchingLoginFinger> dataFinger = Enumerable.Empty<MatchingLoginFinger>();
            if (!string.IsNullOrEmpty(req.Npp))
            {
                dataFinger = await _authRepository.GetDataFingerGeneralEmpIso(req.Nik)
                .ConfigureAwait(false);
            }
            else
            {
                dataFinger = await _authRepository.GetDataFingerGeneralISO(req.Nik)
                .ConfigureAwait(false);
            }

            if (!dataFinger.Any()) return new ServiceResponse<ProfileByNikOnlyFinger> { Message = _ErrorMessageConfig.MatchingFingerNotFound, Status = "error" };

            int? match = 0;

            using WebClient webClient = new();

            var urlImg1 = "";
            var urlImg2 = "";
            var dec1 = "";
            var dec2 = "";
            var urlImgISO1 = "";
            var urlImgISO2 = "";
            var decISO1 = "";
            var decISO2 = "";
            var errorMsg = "";

            foreach (var i in dataFinger)
            {
                if (match == 0)
                {
                    var count = 0;
                    try
                    {
                        count++;
                        var b64 = webClient.DownloadData(i.file ?? "");
                        var b64ISO = webClient.DownloadData(i.file ?? "");

                        string b64String = "";

                        using (var r = new StreamReader(new MemoryStream(b64)))
                        {
                            var encryptedText = r.ReadToEnd();
                            b64String = encryptedText.Decrypt(Phrase.FileEncryption);
                        }

                        if (count == 1)
                        {
                            urlImg1 = i.file;
                            dec1 = b64String;
                        }
                        else
                        {
                            urlImg2 = i.file;
                            dec2 = b64String;
                        }

                        var matchingRes = await _imageRecognitionService.MatchImageBase64ToBase64(
                            new Base64ToBase64Req
                            {
                                Base64Images1 = req.Base64Img,
                                Base64Images2 = b64String
                            },
                            new UrlRequestRecognition
                            {
                                BaseUrl = baseUrl,
                                EndPoint = endPoint
                            })
                            .ConfigureAwait(false);

                        if (matchingRes?.Data?.IsFoundOrSuccess == true) match = 1;
                    }
                    catch (Exception ex)
                    {
                        errorMsg = ex.Message ?? (ex.InnerException.Message ?? "");
                    }
                }
            }

            if (match == 0)
            {
                var _logs = new Tbl_LogClientApps
                {
                    Param = "nik: " + req.Nik + ",base64Img: " + req.Base64Img,
                    LvTeller = req.LvTeller,
                    Branch = req.Branch,
                    SubBranch = req.SubBranch,
                    ClientApps = req.ClientApps,
                    RequestTime = RequestTime,
                    ResponseTime = DateTime.Now,
                    CreatedTime = DateTime.Now,
                    //EndPoint = req.EndPoint,
                    EndPoint = endPoint,
                    ErrorMessage = errorMsg,
                    CreatedByNpp = string.IsNullOrEmpty(req.Npp) ? "" : req.Npp,
                    CreatedByUnitCode = AuthUnitCode
                };

                _ = _authRepository.InsertLogClientApps(_logs);

                return new ServiceResponse<ProfileByNikOnlyFinger>
                {
                    Status = "error",
                    Message = _ErrorMessageConfig.MatchingError,
                    Data = new ProfileByNikOnlyFinger
                    {
                        RequestedImg = req.Base64Img,
                        ImgUrlPath1 = urlImg1,
                        ImgUrlPath2 = urlImg2,
                        ktp_FingerKanan = dec1,
                        ktp_FingerKiri = dec2,
                        ErrorMsg = errorMsg
                    }
                };
            }

            //var profile = await _profileRepository.GetProfileByNikISO(req.Nik).ConfigureAwait(false);
            var profile = await _profileRepository.GetProfileByNikISOBio(req.Nik).ConfigureAwait(false);

            var _log = new Tbl_LogClientApps
            {
                Param = "nik: " + req.Nik + ",base64Img: " + req.Base64Img,
                LvTeller = req.LvTeller,
                Branch = req.Branch,
                SubBranch = req.SubBranch,
                ClientApps = req.ClientApps,
                RequestTime = RequestTime,
                ResponseTime = DateTime.Now,
                CreatedTime = DateTime.Now,
                //EndPoint = req.EndPoint,
                EndPoint = endPoint,
                ErrorMessage = errorMsg,
                CreatedByNpp = string.IsNullOrEmpty(req.Npp) ? "" : req.Npp,
                CreatedByUnitCode = AuthUnitCode
            };

            _ = _authRepository.InsertLogClientApps(_log);

            return new ServiceResponse<ProfileByNikOnlyFinger>
            {
                Status = "sukses",
                Message = _SuccessMessageConfig.MatchingSuccess,
                Data = ConvertUrlToB64FingerEncAndISO(profile)
            };
        }

        public async Task<ServiceResponse<ProfileByNik>> GetAuthKTPDataFingerEncOnlyFR(ProfileFRReq req, DateTime RequestTime, string AuthNpp, string AuthUnitCode)
        {
            int? match = 0;
            var errorMsg = "";
            
            var profile = await _profileRepository.GetProfileByNikISO(req.Nik).ConfigureAwait(false);

            if (profile == null) return new ServiceResponse<ProfileByNik> { Message = _ErrorMessageConfig.DemografiTidakDitemukan, Status = "error" };

            var _log = new Tbl_LogClientApps
            {
                Param = "nik: " + req.Nik,
                LvTeller = req.LvTeller,
                Branch = req.Branch,
                SubBranch = req.SubBranch,
                ClientApps = req.ClientApps,
                RequestTime = RequestTime,
                ResponseTime = DateTime.Now,
                CreatedTime = DateTime.Now,
                EndPoint = req.EndPoint,
                ErrorMessage = errorMsg,
                CreatedByNpp = AuthNpp,
                CreatedByUnitCode = AuthUnitCode
            };

            _ = _authRepository.InsertLogClientApps(_log);

            return new ServiceResponse<ProfileByNik>
            {
                Status = "sukses",
                Message = _SuccessMessageConfig.MatchingSuccess,
                Data = ConvertUrlToB64FingerEncOnly(profile)
            };
        }

        public async Task<ServiceResponse<ProfileByNik>> GetAuthKTPDataFingerEncOnlyISODB(ProfileLoopReq req, string baseUrl, string endPoint, DateTime RequestTime, string AuthNpp, string AuthUnitCode)
        {
            var dataFinger = await _authRepository.GetDataFingerGeneralISO(req.Nik)
                .ConfigureAwait(false);

            if (!dataFinger.Any()) return new ServiceResponse<ProfileByNik> { Message = _ErrorMessageConfig.MatchingFingerNotFound, Status = "error" };

            int? match = 0;

            using WebClient webClient = new();

            var urlImg1 = "";
            var urlImg2 = "";
            var dec1 = "";
            var dec2 = "";
            var errorMsg = "";

            foreach (var i in dataFinger)
            {
                if (match == 0)
                {
                    var count = 0;
                    try
                    {
                        count++;
                        var b64 = i.iso;

                        string b64String = "";

                        if (!string.IsNullOrEmpty(b64))
                        {
                            b64String = b64.Decrypt(Phrase.FileEncryption);

                            if (count == 1)
                            {
                                urlImg1 = i.file;
                                dec1 = b64String;
                            }
                            else
                            {
                                urlImg2 = i.file;
                                dec2 = b64String;
                            }

                            var matchingRes = await _imageRecognitionService.MatchImageBase64ToBase64(
                                new Base64ToBase64Req
                                {
                                    Base64Images1 = req.Base64Img,
                                    Base64Images2 = b64String
                                },
                                new UrlRequestRecognition
                                {
                                    BaseUrl = baseUrl,
                                    EndPoint = endPoint
                                })
                                .ConfigureAwait(false);


                            if (matchingRes?.Data?.IsFoundOrSuccess == true) match = 1;
                        }
                    }
                    catch (Exception ex)
                    {
                        errorMsg = ex.Message ?? (ex.InnerException.Message ?? "");
                    }
                }
            }

            if (match == 0)
            {
                var _logs = new Tbl_LogClientApps
                {
                    Param = "nik: " + req.Nik + ",base64Img: " + req.Base64Img,
                    LvTeller = req.LvTeller,
                    Branch = req.Branch,
                    SubBranch = req.SubBranch,
                    ClientApps = req.ClientApps,
                    RequestTime = RequestTime,
                    ResponseTime = DateTime.Now,
                    CreatedTime = DateTime.Now,
                    EndPoint = req.EndPoint,
                    ErrorMessage = errorMsg,
                    CreatedByNpp = AuthNpp,
                    CreatedByUnitCode = AuthUnitCode
                };

                _ = _authRepository.InsertLogClientApps(_logs);

                return new ServiceResponse<ProfileByNik>
                {
                    Status = "error",
                    Message = _ErrorMessageConfig.MatchingError,
                    Data = new ProfileByNik
                    {
                        RequestedImg = req.Base64Img,
                        ImgUrlPath1 = urlImg1,
                        ImgUrlPath2 = urlImg2,
                        ktp_FingerKanan = dec1,
                        ktp_FingerKiri = dec2,
                        ErrorMsg = errorMsg
                    }
                };
            }

            var profile = await _profileRepository.GetProfileByNikISO(req.Nik).ConfigureAwait(false);

            var _log = new Tbl_LogClientApps
            {
                Param = "nik: " + req.Nik + ",base64Img: " + req.Base64Img,
                LvTeller = req.LvTeller,
                Branch = req.Branch,
                SubBranch = req.SubBranch,
                ClientApps = req.ClientApps,
                RequestTime = RequestTime,
                ResponseTime = DateTime.Now,
                CreatedTime = DateTime.Now,
                EndPoint = req.EndPoint,
                ErrorMessage = errorMsg,
                CreatedByNpp = AuthNpp,
                CreatedByUnitCode = AuthUnitCode
            };

            _ = _authRepository.InsertLogClientApps(_log);

            return new ServiceResponse<ProfileByNik>
            {
                Status = "sukses",
                Message = _SuccessMessageConfig.MatchingSuccess,
                Data = ConvertUrlToB64FingerEncOnly(profile)
            };
        }

        public async Task<ServiceResponse<ProfileByNik>> GetAuthKTPDataFingerEncOnlyNew(ProfileLoopReq req, string baseUrl, string endPoint, DateTime RequestTime, string AuthNpp, string AuthUnitCode)
        {
            var dataFinger = await _authRepository.GetDataFingerGeneralNew(req.Nik)
                .ConfigureAwait(false);

            if (!dataFinger.Any()) return new ServiceResponse<ProfileByNik> { Message = _ErrorMessageConfig.MatchingFingerNotFound, Status = "error" };

            int? match = 0;

            using WebClient webClient = new();

            var urlImg1 = "";
            var urlImg2 = "";
            var dec1 = "";
            var dec2 = "";
            var errorMsg = "";

            foreach (var i in dataFinger)
            {
                if (match == 0)
                {
                    var count = 0;
                    try
                    {
                        count++;
                        var b64 = i.iso;

                        string b64String = "";
                        if (!string.IsNullOrEmpty(b64))
                        {
                            b64String = b64.Decrypt(Phrase.FileEncryption);
                            if (count == 1)
                            {
                                urlImg1 = i.file;
                                dec1 = b64String;
                            }
                            else
                            {
                                urlImg2 = i.file;
                                dec2 = b64String;
                            }

                            var matchingRes = await _imageRecognitionService.MatchImageBase64ToBase64(
                                new Base64ToBase64Req
                                {
                                    Base64Images1 = req.Base64Img,
                                    Base64Images2 = b64String
                                },
                                new UrlRequestRecognition
                                {
                                    BaseUrl = baseUrl,
                                    EndPoint = endPoint
                                })
                                .ConfigureAwait(false);

                            if (matchingRes?.Data?.IsFoundOrSuccess == true) match = 1;
                        }

                    }
                    catch (Exception ex)
                    {
                        errorMsg = ex.Message ?? (ex.InnerException.Message ?? "");
                    }
                }
            }

            if (match == 0)
            {
                var _logs = new Tbl_LogClientApps
                {
                    Param = "nik: " + req.Nik,
                    LvTeller = req.LvTeller,
                    Branch = req.Branch,
                    SubBranch = req.SubBranch,
                    ClientApps = req.ClientApps,
                    RequestTime = RequestTime,
                    ResponseTime = DateTime.Now,
                    CreatedTime = DateTime.Now,
                    EndPoint = req.EndPoint,
                    ErrorMessage = errorMsg,
                    CreatedByNpp = AuthNpp,
                    CreatedByUnitCode = AuthUnitCode
                };

                _ = _authRepository.InsertLogClientApps(_logs);

                return new ServiceResponse<ProfileByNik>
                {
                    Status = "error",
                    Message = _ErrorMessageConfig.MatchingError,
                    Data = new ProfileByNik
                    {
                        RequestedImg = req.Base64Img,
                        ImgUrlPath1 = urlImg1,
                        ImgUrlPath2 = urlImg2,
                        ktp_FingerKanan = dec1,
                        ktp_FingerKiri = dec2,
                        ErrorMsg = errorMsg
                    }
                };
            }

            //var profile = await _profileRepository.GetProfileByNik(req.Nik).ConfigureAwait(false);

            var _log = new Tbl_LogClientApps
            {
                Param = "nik: " + req.Nik,
                LvTeller = req.LvTeller,
                Branch = req.Branch,
                SubBranch = req.SubBranch,
                ClientApps = req.ClientApps,
                RequestTime = RequestTime,
                ResponseTime = DateTime.Now,
                CreatedTime = DateTime.Now,
                EndPoint = req.EndPoint,
                ErrorMessage = errorMsg,
                CreatedByNpp = AuthNpp,
                CreatedByUnitCode = AuthUnitCode
            };

            _ = _authRepository.InsertLogClientApps(_log);

            return new ServiceResponse<ProfileByNik>
            {
                Status = "sukses",
                Message = _SuccessMessageConfig.MatchingSuccess,
            };
        }

		public async Task<ServiceResponse<ProfileByNik>> GetAuthKTPDataFingerEncOnlyISOWData(ProfileLoopReq req, string baseUrl, string endPoint, DateTime RequestTime, string AuthNpp, string AuthUnitCode)
		{
			var dataFinger = await _authRepository.GetDataFingerGeneralNew(req.Nik)
				.ConfigureAwait(false);

			if (!dataFinger.Any()) return new ServiceResponse<ProfileByNik> { Message = _ErrorMessageConfig.MatchingFingerNotFound, Status = "error" };

			int? match = 0;

			using WebClient webClient = new();

			var urlImg1 = "";
			var urlImg2 = "";
			var dec1 = "";
			var dec2 = "";
			var errorMsg = "";

			foreach (var i in dataFinger)
			{
				if (match == 0)
				{
					var count = 0;
					try
					{
						count++;
						var b64 = i.iso;

						string b64String = "";
						if (!string.IsNullOrEmpty(b64))
						{
							b64String = b64.Decrypt(Phrase.FileEncryption);
							if (count == 1)
							{
								urlImg1 = i.file;
								dec1 = b64String;
							}
							else
							{
								urlImg2 = i.file;
								dec2 = b64String;
							}

							var matchingRes = await _imageRecognitionService.MatchImageBase64ToBase64(
								new Base64ToBase64Req
								{
									Base64Images1 = req.Base64Img,
									Base64Images2 = b64String
								},
								new UrlRequestRecognition
								{
									BaseUrl = baseUrl,
									EndPoint = endPoint
								})
								.ConfigureAwait(false);

							if (matchingRes?.Data?.IsFoundOrSuccess == true) match = 1;
						}

					}
					catch (Exception ex)
					{
						errorMsg = ex.Message ?? (ex.InnerException.Message ?? "");
					}
				}
			}

			if (match == 0)
			{
				var _logs = new Tbl_LogClientApps
				{
					Param = "nik: " + req.Nik,
					LvTeller = req.LvTeller,
					Branch = req.Branch,
					SubBranch = req.SubBranch,
					ClientApps = req.ClientApps,
					RequestTime = RequestTime,
					ResponseTime = DateTime.Now,
					CreatedTime = DateTime.Now,
					EndPoint = req.EndPoint,
					ErrorMessage = errorMsg,
					CreatedByNpp = AuthNpp,
					CreatedByUnitCode = AuthUnitCode
				};

				_ = _authRepository.InsertLogClientApps(_logs);

				return new ServiceResponse<ProfileByNik>
				{
					Status = "error",
					Message = _ErrorMessageConfig.MatchingError,
					Data = new ProfileByNik
					{
						RequestedImg = req.Base64Img,
						ImgUrlPath1 = urlImg1,
						ImgUrlPath2 = urlImg2,
						ktp_FingerKanan = dec1,
						ktp_FingerKiri = dec2,
						ErrorMsg = errorMsg
					}
				};
			}

            var profile = await _profileRepository.GetProfileByNik(req.Nik).ConfigureAwait(false);

            var _log = new Tbl_LogClientApps
			{
				Param = "nik: " + req.Nik,
				LvTeller = req.LvTeller,
				Branch = req.Branch,
				SubBranch = req.SubBranch,
				ClientApps = req.ClientApps,
				RequestTime = RequestTime,
				ResponseTime = DateTime.Now,
				CreatedTime = DateTime.Now,
				EndPoint = req.EndPoint,
				ErrorMessage = errorMsg,
				CreatedByNpp = AuthNpp,
				CreatedByUnitCode = AuthUnitCode
			};

			_ = _authRepository.InsertLogClientApps(_log);

			return new ServiceResponse<ProfileByNik>
			{
				Status = "sukses",
				Message = _SuccessMessageConfig.MatchingSuccess,
				Data = ConvertUrlToB64FingerEncOnly(profile)
			};
		}

		public async Task<ServiceResponse<ProfileByNik>> GetAuthKTPDataFingerEncOnlyNewData(ProfileLoopReq req, string baseUrl, string endPoint, DateTime RequestTime, string AuthNpp, string AuthUnitCode)
        {
            var dataFinger = await _authRepository.GetDataFingerGeneralNew(req.Nik)
                .ConfigureAwait(false);

            if (!dataFinger.Any()) return new ServiceResponse<ProfileByNik> { Message = _ErrorMessageConfig.MatchingFingerNotFound, Status = "error" };

            int? match = 0;

            using WebClient webClient = new();

            var urlImg1 = "";
            var urlImg2 = "";
            var dec1 = "";
            var dec2 = "";
            var errorMsg = "";

            foreach (var i in dataFinger)
            {
                if (match == 0)
                {
                    var count = 0;
                    try
                    {
                        count++;
                        var b64 = i.iso;

                        string b64String = "";
                        if (!string.IsNullOrEmpty(b64))
                        {
                            b64String = b64.Decrypt(Phrase.FileEncryption);
                            if (count == 1)
                            {
                                urlImg1 = i.file;
                                dec1 = b64String;
                            }
                            else
                            {
                                urlImg2 = i.file;
                                dec2 = b64String;
                            }

                            var matchingRes = await _imageRecognitionService.MatchImageBase64ToBase64(
                                new Base64ToBase64Req
                                {
                                    Base64Images1 = req.Base64Img,
                                    Base64Images2 = b64String
                                },
                                new UrlRequestRecognition
                                {
                                    BaseUrl = baseUrl,
                                    EndPoint = endPoint
                                })
                                .ConfigureAwait(false);

                            if (matchingRes?.Data?.IsFoundOrSuccess == true) match = 1;
                        }

                    }
                    catch (Exception ex)
                    {
                        errorMsg = ex.Message ?? (ex.InnerException.Message ?? "");
                    }
                }
            }

            if (match == 0)
            {
                var _logs = new Tbl_LogClientApps
                {
                    Param = "nik: " + req.Nik,
                    LvTeller = req.LvTeller,
                    Branch = req.Branch,
                    SubBranch = req.SubBranch,
                    ClientApps = req.ClientApps,
                    RequestTime = RequestTime,
                    ResponseTime = DateTime.Now,
                    CreatedTime = DateTime.Now,
                    EndPoint = req.EndPoint,
                    ErrorMessage = errorMsg,
                    CreatedByNpp = AuthNpp,
                    CreatedByUnitCode = AuthUnitCode
                };

                _ = _authRepository.InsertLogClientApps(_logs);

                return new ServiceResponse<ProfileByNik>
                {
                    Status = "error",
                    Message = _ErrorMessageConfig.MatchingError,
                    Data = new ProfileByNik
                    {
                        RequestedImg = req.Base64Img,
                        ImgUrlPath1 = urlImg1,
                        ImgUrlPath2 = urlImg2,
                        ktp_FingerKanan = dec1,
                        ktp_FingerKiri = dec2,
                        ErrorMsg = errorMsg
                    }
                };
            }

            var profile = await _profileRepository.GetProfileByNik(req.Nik).ConfigureAwait(false);

            var _log = new Tbl_LogClientApps
            {
                Param = "nik: " + req.Nik,
                LvTeller = req.LvTeller,
                Branch = req.Branch,
                SubBranch = req.SubBranch,
                ClientApps = req.ClientApps,
                RequestTime = RequestTime,
                ResponseTime = DateTime.Now,
                CreatedTime = DateTime.Now,
                EndPoint = req.EndPoint,
                ErrorMessage = errorMsg,
                CreatedByNpp = AuthNpp,
                CreatedByUnitCode = AuthUnitCode
            };

            _ = _authRepository.InsertLogClientApps(_log);

            return new ServiceResponse<ProfileByNik>
            {
                Status = "sukses",
                Message = _SuccessMessageConfig.MatchingSuccess,
                Data = ConvertUrlToB64FingerEncOnly(profile)
            };
        }

        public async Task<ServiceResponse<ProfileByNik>> GetAuthKTPDataFingerEncOnlyNewFile(ProfileLoopReq req, string baseUrl, string endPoint, DateTime RequestTime, string AuthNpp, string AuthUnitCode)
        {
            var dataFinger = await _authRepository.GetDataFingerGeneralNewFile(req.Nik)
                .ConfigureAwait(false);

            if (!dataFinger.Any()) return new ServiceResponse<ProfileByNik> { Message = _ErrorMessageConfig.MatchingFingerNotFound, Status = "error" };

            int? match = 0;

            using WebClient webClient = new();

            var urlImg1 = "";
            var urlImg2 = "";
            var dec1 = "";
            var dec2 = "";
            var errorMsg = "";

            foreach (var i in dataFinger)
            {
                if (match == 0)
                {
                    var count = 0;
                    try
                    {
                        count++;
                        var b64 = webClient.DownloadData(i.file ?? "");

                        string b64String = "";

                        using (var r = new StreamReader(new MemoryStream(b64)))
                        {
                            var encryptedText = r.ReadToEnd();
                            b64String = encryptedText.Decrypt(Phrase.FileEncryption);
                        }

                        if (count == 1)
                        {
                            urlImg1 = i.file;
                            dec1 = b64String;
                        }
                        else
                        {
                            urlImg2 = i.file;
                            dec2 = b64String;
                        }

                        var matchingRes = await _imageRecognitionService.MatchImageBase64ToBase64(
                            new Base64ToBase64Req
                            {
                                Base64Images1 = req.Base64Img,
                                Base64Images2 = b64String
                            },
                            new UrlRequestRecognition
                            {
                                BaseUrl = baseUrl,
                                EndPoint = endPoint
                            })
                            .ConfigureAwait(false);

                        if (matchingRes?.Data?.IsFoundOrSuccess == true) match = 1;
                    }
                    catch (Exception ex)
                    {
                        errorMsg = ex.Message ?? (ex.InnerException.Message ?? "");
                    }
                }
            }

            if (match == 0)
            {
                var _logs = new Tbl_LogClientApps
                {
                    Param = "nik: " + req.Nik ,
                    LvTeller = req.LvTeller,
                    Branch = req.Branch,
                    SubBranch = req.SubBranch,
                    ClientApps = req.ClientApps,
                    RequestTime = RequestTime,
                    ResponseTime = DateTime.Now,
                    CreatedTime = DateTime.Now,
                    EndPoint = req.EndPoint,
                    ErrorMessage = errorMsg,
                    CreatedByNpp = AuthNpp,
                    CreatedByUnitCode = AuthUnitCode
                };

                _ = _authRepository.InsertLogClientApps(_logs);

                return new ServiceResponse<ProfileByNik>
                {
                    Status = "error",
                    Message = _ErrorMessageConfig.MatchingError,
                    Data = new ProfileByNik
                    {
                        RequestedImg = req.Base64Img,
                        ImgUrlPath1 = urlImg1,
                        ImgUrlPath2 = urlImg2,
                        ktp_FingerKanan = dec1,
                        ktp_FingerKiri = dec2,
                        ErrorMsg = errorMsg
                    }
                };
            }

            //var profile = await _profileRepository.GetProfileByNik(req.Nik).ConfigureAwait(false);

            var _log = new Tbl_LogClientApps
            {
                Param = "nik: " + req.Nik,
                LvTeller = req.LvTeller,
                Branch = req.Branch,
                SubBranch = req.SubBranch,
                ClientApps = req.ClientApps,
                RequestTime = RequestTime,
                ResponseTime = DateTime.Now,
                CreatedTime = DateTime.Now,
                EndPoint = req.EndPoint,
                ErrorMessage = errorMsg,
                CreatedByNpp = AuthNpp,
                CreatedByUnitCode = AuthUnitCode
            };

            _ = _authRepository.InsertLogClientApps(_log);

            return new ServiceResponse<ProfileByNik>
            {
                Status = "sukses",
                Message = _SuccessMessageConfig.MatchingSuccess,
                //Data = ConvertUrlToB64FingerEncOnly(profile)
            };
        }

		public async Task<ServiceResponse<ProfileByNik>> GetAuthKTPDataFingerEncOnlyISOFileWData(ProfileLoopReq req, string baseUrl, string endPoint, DateTime RequestTime, string AuthNpp, string AuthUnitCode)
		{
			var dataFinger = await _authRepository.GetDataFingerGeneralNewFile(req.Nik)
				.ConfigureAwait(false);

			if (!dataFinger.Any()) return new ServiceResponse<ProfileByNik> { Message = _ErrorMessageConfig.MatchingFingerNotFound, Status = "error" };

			int? match = 0;

			using WebClient webClient = new();

			var urlImg1 = "";
			var urlImg2 = "";
			var dec1 = "";
			var dec2 = "";
			var errorMsg = "";

			foreach (var i in dataFinger)
			{
				if (match == 0)
				{
					var count = 0;
					try
					{
						count++;
						var b64 = webClient.DownloadData(i.file ?? "");

						string b64String = "";

						using (var r = new StreamReader(new MemoryStream(b64)))
						{
							var encryptedText = r.ReadToEnd();
							b64String = encryptedText.Decrypt(Phrase.FileEncryption);
						}

						if (count == 1)
						{
							urlImg1 = i.file;
							dec1 = b64String;
						}
						else
						{
							urlImg2 = i.file;
							dec2 = b64String;
						}

						var matchingRes = await _imageRecognitionService.MatchImageBase64ToBase64(
							new Base64ToBase64Req
							{
								Base64Images1 = req.Base64Img,
								Base64Images2 = b64String
							},
							new UrlRequestRecognition
							{
								BaseUrl = baseUrl,
								EndPoint = endPoint
							})
							.ConfigureAwait(false);

						if (matchingRes?.Data?.IsFoundOrSuccess == true) match = 1;
					}
					catch (Exception ex)
					{
						errorMsg = ex.Message ?? (ex.InnerException.Message ?? "");
					}
				}
			}

			if (match == 0)
			{
				var _logs = new Tbl_LogClientApps
				{
					Param = "nik: " + req.Nik,
					LvTeller = req.LvTeller,
					Branch = req.Branch,
					SubBranch = req.SubBranch,
					ClientApps = req.ClientApps,
					RequestTime = RequestTime,
					ResponseTime = DateTime.Now,
					CreatedTime = DateTime.Now,
					EndPoint = req.EndPoint,
					ErrorMessage = errorMsg,
					CreatedByNpp = AuthNpp,
					CreatedByUnitCode = AuthUnitCode
				};

				_ = _authRepository.InsertLogClientApps(_logs);

				return new ServiceResponse<ProfileByNik>
				{
					Status = "error",
					Message = _ErrorMessageConfig.MatchingError,
					Data = new ProfileByNik
					{
						RequestedImg = req.Base64Img,
						ImgUrlPath1 = urlImg1,
						ImgUrlPath2 = urlImg2,
						ktp_FingerKanan = dec1,
						ktp_FingerKiri = dec2,
						ErrorMsg = errorMsg
					}
				};
			}

            var profile = await _profileRepository.GetProfileByNik(req.Nik).ConfigureAwait(false);

            var _log = new Tbl_LogClientApps
			{
				Param = "nik: " + req.Nik,
				LvTeller = req.LvTeller,
				Branch = req.Branch,
				SubBranch = req.SubBranch,
				ClientApps = req.ClientApps,
				RequestTime = RequestTime,
				ResponseTime = DateTime.Now,
				CreatedTime = DateTime.Now,
				EndPoint = req.EndPoint,
				ErrorMessage = errorMsg,
				CreatedByNpp = AuthNpp,
				CreatedByUnitCode = AuthUnitCode
			};

			_ = _authRepository.InsertLogClientApps(_log);

			return new ServiceResponse<ProfileByNik>
			{
				Status = "sukses",
				Message = _SuccessMessageConfig.MatchingSuccess,
                Data = ConvertUrlToB64FingerEncOnly(profile)
            };
		}

		public async Task<ServiceResponse<ProfileByNik>> GetAuthKTPDataFingerEncOnlyCompressed(ProfileLoopReq req, string baseUrl, string endPoint, DateTime RequestTime, string AuthNpp, string AuthUnitCode)
        {
            var dataFinger = await _authRepository.GetDataFingerGeneral(req.Nik)
                .ConfigureAwait(false);

            if (!dataFinger.Any()) return new ServiceResponse<ProfileByNik> { Message = _ErrorMessageConfig.MatchingFingerNotFound, Status = "error" };

            int? match = 0;

            using WebClient webClient = new();

            var urlImg1 = "";
            var urlImg2 = "";
            var dec1 = "";
            var dec2 = "";
            var errorMsg = "";

            foreach (var i in dataFinger)
            {
                if (match == 0)
                {
                    var count = 0;
                    try
                    {
                        count++;
                        var b64 = webClient.DownloadData(i.file ?? "");

                        string b64String = "";

                        using (var r = new StreamReader(new MemoryStream(b64)))
                        {
                            var encryptedText = r.ReadToEnd();
                            b64String = encryptedText.Decrypt(Phrase.FileEncryption);
                        }

                        if (count == 1)
                        {
                            urlImg1 = i.file;
                            dec1 = b64String;
                        }
                        else
                        {
                            urlImg2 = i.file;
                            dec2 = b64String;
                        }

                        var matchingRes = await _imageRecognitionService.MatchImageBase64ToBase64(
                            new Base64ToBase64Req
                            {
                                Base64Images1 = req.Base64Img,
                                Base64Images2 = b64String
                            },
                            new UrlRequestRecognition
                            {
                                BaseUrl = baseUrl,
                                EndPoint = endPoint
                            })
                            .ConfigureAwait(false);

                        if (matchingRes?.Data?.IsFoundOrSuccess == true) match = 1;
                    }
                    catch (Exception ex)
                    {
                        errorMsg = ex.Message ?? (ex.InnerException.Message ?? "");
                    }
                }
            }

            if (match == 0)
            {
                var _logs = new Tbl_LogClientApps
                {
                    Param = "nik: " + req.Nik + ",base64Img: " + req.Base64Img,
                    LvTeller = req.LvTeller,
                    Branch = req.Branch,
                    SubBranch = req.SubBranch,
                    ClientApps = req.ClientApps,
                    RequestTime = RequestTime,
                    ResponseTime = DateTime.Now,
                    CreatedTime = DateTime.Now,
                    EndPoint = req.EndPoint,
                    ErrorMessage = errorMsg,
                    CreatedByNpp = AuthNpp,
                    CreatedByUnitCode = AuthUnitCode
                };

                _ = _authRepository.InsertLogClientApps(_logs);

                return new ServiceResponse<ProfileByNik>
                {
                    Status = "error",
                    Message = _ErrorMessageConfig.MatchingError,
                    Data = new ProfileByNik
                    {
                        RequestedImg = req.Base64Img,
                        ImgUrlPath1 = urlImg1,
                        ImgUrlPath2 = urlImg2,
                        ktp_FingerKanan = dec1,
                        ktp_FingerKiri = dec2,
                        ErrorMsg = errorMsg
                    }
                };
            }

            var profile = await _profileRepository.GetProfileByNik(req.Nik).ConfigureAwait(false);

            var _log = new Tbl_LogClientApps
            {
                Param = "nik: " + req.Nik + ",base64Img: " + req.Base64Img,
                LvTeller = req.LvTeller,
                Branch = req.Branch,
                SubBranch = req.SubBranch,
                ClientApps = req.ClientApps,
                RequestTime = RequestTime,
                ResponseTime = DateTime.Now,
                CreatedTime = DateTime.Now,
                EndPoint = req.EndPoint,
                ErrorMessage = errorMsg,
                CreatedByNpp = AuthNpp,
                CreatedByUnitCode = AuthUnitCode
            };

            _ = _authRepository.InsertLogClientApps(_log);

            return new ServiceResponse<ProfileByNik>
            {
                Status = "sukses",
                Message = _SuccessMessageConfig.MatchingSuccess,
                Data = ConvertUrlToB64FingerEncOnlyCompressed(profile)
            };
        }

        public async Task<ServiceResponse<ProfileByNik>> GetAuthKTPDataFingerEmpEncOnly(ProfileLoopReq req, string baseUrl, string endPoint, DateTime RequestTime, string AuthNpp, string AuthUnitCode)
        {
            var dataFinger = await _authRepository.GetDataFingerGeneralEmp(req.Nik)
                .ConfigureAwait(false);

            if (!dataFinger.Any()) return new ServiceResponse<ProfileByNik> { Message = _ErrorMessageConfig.MatchingFingerNotFound, Status = "error" };

            int? match = 0;

            using WebClient webClient = new();

            var urlImg1 = "";
            var urlImg2 = "";
            var dec1 = "";
            var dec2 = "";
            var errorMsg = "";

            foreach (var i in dataFinger)
            {
                if (match == 0)
                {
                    var count = 0;
                    try
                    {
                        count++;
                        var b64 = webClient.DownloadData(i.file ?? "");

                        string b64String = "";

                        using (var r = new StreamReader(new MemoryStream(b64)))
                        {
                            var encryptedText = r.ReadToEnd();
                            b64String = encryptedText.Decrypt(Phrase.FileEncryption);
                        }

                        if (count == 1)
                        {
                            urlImg1 = i.file;
                            dec1 = b64String;
                        }
                        else
                        {
                            urlImg2 = i.file;
                            dec2 = b64String;
                        }

                        var matchingRes = await _imageRecognitionService.MatchImageBase64ToBase64(
                            new Base64ToBase64Req
                            {
                                Base64Images1 = req.Base64Img,
                                Base64Images2 = b64String
                            },
                            new UrlRequestRecognition
                            {
                                BaseUrl = baseUrl,
                                EndPoint = endPoint
                            })
                            .ConfigureAwait(false);

                        if (matchingRes?.Data?.IsFoundOrSuccess == true) match = 1;
                    }
                    catch (Exception ex)
                    {
                        errorMsg = ex.Message ?? (ex.InnerException.Message ?? "");
                    }
                }
            }

            if (match == 0)
            {
                var _logs = new Tbl_LogClientApps
                {
                    Param = "nik: " + req.Nik + ",base64Img: " + req.Base64Img,
                    LvTeller = req.LvTeller,
                    Branch = req.Branch,
                    SubBranch = req.SubBranch,
                    ClientApps = req.ClientApps,
                    RequestTime = RequestTime,
                    ResponseTime = DateTime.Now,
                    CreatedTime = DateTime.Now,
                    EndPoint = req.EndPoint,
                    ErrorMessage = errorMsg,
                    CreatedByNpp = AuthNpp,
                    CreatedByUnitCode = AuthUnitCode
                };

                //_ = _authRepository.InsertLogClientApps(_logs);

                return new ServiceResponse<ProfileByNik>
                {
                    Status = "error",
                    Message = _ErrorMessageConfig.MatchingFingerFailed,
                    Data = new ProfileByNik
                    {
                        RequestedImg = req.Base64Img,
                        ImgUrlPath1 = urlImg1,
                        ImgUrlPath2 = urlImg2,
                        ktp_FingerKanan = dec1,
                        ktp_FingerKiri = dec2,
                        ErrorMsg = errorMsg
                    }
                };
            }

            var profile = await _profileRepository.GetProfileByNikEmp(req.Nik).ConfigureAwait(false);

            var _log = new Tbl_LogClientApps
            {
                Param = "nik: " + req.Nik + ",base64Img: " + req.Base64Img,
                LvTeller = req.LvTeller,
                Branch = req.Branch,
                SubBranch = req.SubBranch,
                ClientApps = req.ClientApps,
                RequestTime = RequestTime,
                ResponseTime = DateTime.Now,
                CreatedTime = DateTime.Now,
                EndPoint = req.EndPoint,
                ErrorMessage = errorMsg,
                CreatedByNpp = AuthNpp,
                CreatedByUnitCode = AuthUnitCode
            };

            //_ = _authRepository.InsertLogClientApps(_log);

            return new ServiceResponse<ProfileByNik>
            {
                Status = "sukses",
                Message = _SuccessMessageConfig.MatchingSuccess,
                Data = ConvertUrlToB64FingerEncOnly(profile)
            };
        }

        public async Task<ServiceResponse<ProfileByNik>> IsFingerLoopMatch(ProfileLoopReq req, string baseUrl, string endPoint, DateTime RequestTime, string AuthNpp, string AuthUnitCode)
        {
            var dataFinger = await _authRepository.GetDataFingerGeneralEmp(req.Nik)
                .ConfigureAwait(false);

            if (!dataFinger.Any()) return new ServiceResponse<ProfileByNik> { Message = _ErrorMessageConfig.MatchingFingerNotFound, Status = "error" };

            int? match = 0;

            using WebClient webClient = new();

            var urlImg1 = "";
            var urlImg2 = "";
            var dec1 = "";
            var dec2 = "";
            var errorMsg = "";

            foreach (var i in dataFinger)
            {
                if (match == 0)
                {
                    var count = 0;
                    try
                    {
                        count++;
                        var b64 = webClient.DownloadData(i.file ?? "");

                        string b64String = "";

                        using (var r = new StreamReader(new MemoryStream(b64)))
                        {
                            var encryptedText = r.ReadToEnd();
                            b64String = encryptedText.Decrypt(Phrase.FileEncryption);
                        }

                        if (count == 1)
                        {
                            urlImg1 = i.file;
                            dec1 = b64String;
                        }
                        else
                        {
                            urlImg2 = i.file;
                            dec2 = b64String;
                        }

                        var matchingRes = await _imageRecognitionService.MatchImageBase64ToBase64(
                            new Base64ToBase64Req
                            {
                                Base64Images1 = req.Base64Img,
                                Base64Images2 = b64String
                            },
                            new UrlRequestRecognition
                            {
                                BaseUrl = baseUrl,
                                EndPoint = endPoint
                            })
                            .ConfigureAwait(false);

                        if (matchingRes?.Data?.IsFoundOrSuccess == true) match = 1;
                    }
                    catch (Exception ex)
                    {
                        errorMsg = ex.Message ?? (ex.InnerException.Message ?? "");
                    }
                }
            }

            if (match == 0)
            {
                var _logs = new Tbl_LogClientApps
                {
                    //Param = "nik: " + req.Nik + ",base64Img: " + req.Base64Img,
                    Param = "nik: " + req.Nik,
                    LvTeller = req.LvTeller,
                    Branch = req.Branch,
                    SubBranch = req.SubBranch,
                    ClientApps = req.ClientApps,
                    RequestTime = RequestTime,
                    ResponseTime = DateTime.Now,
                    CreatedTime = DateTime.Now,
                    EndPoint = req.EndPoint,
                    ErrorMessage = errorMsg,
                    CreatedByNpp = AuthNpp,
                    CreatedByUnitCode = AuthUnitCode
                };

                //_ = _authRepository.InsertLogClientApps(_logs);

                return new ServiceResponse<ProfileByNik>
                {
                    Status = "error",
                    Message = _ErrorMessageConfig.MatchingFingerFailed,
                    Data = new ProfileByNik
                    {
                        RequestedImg = req.Base64Img,
                        ImgUrlPath1 = urlImg1,
                        ImgUrlPath2 = urlImg2,
                        ktp_FingerKanan = dec1,
                        ktp_FingerKiri = dec2,
                        ErrorMsg = errorMsg
                    }
                };
            }

            var _log = new Tbl_LogClientApps
            {
                //Param = "nik: " + req.Nik + ",base64Img: " + req.Base64Img,
                Param = "nik: " + req.Nik,
                LvTeller = req.LvTeller,
                Branch = req.Branch,
                SubBranch = req.SubBranch,
                ClientApps = req.ClientApps,
                RequestTime = RequestTime,
                ResponseTime = DateTime.Now,
                CreatedTime = DateTime.Now,
                EndPoint = req.EndPoint,
                ErrorMessage = errorMsg,
                CreatedByNpp = AuthNpp,
                CreatedByUnitCode = AuthUnitCode
            };

            //_ = _authRepository.InsertLogClientApps(_log);

            return new ServiceResponse<ProfileByNik>
            {
                Status = "sukses",
                Message = _SuccessMessageConfig.MatchingSuccess
            };
        }

        public async Task<ServiceResponse<ProfileByNik>> IsFingerLoopMatchNew(ProfileLoopReq req, string baseUrl, string endPoint, DateTime RequestTime, string AuthNpp, string AuthUnitCode)
        {
            var dataFinger = await _authRepository.GetDataFingerGeneralEmpNew(req.Nik)
                .ConfigureAwait(false);

            if (!dataFinger.Any()) return new ServiceResponse<ProfileByNik> { Message = _ErrorMessageConfig.MatchingFingerNotFound, Status = "error" };

            int? match = 0;

            using WebClient webClient = new();

            var urlImg1 = "";
            var urlImg2 = "";
            var dec1 = "";
            var dec2 = "";
            var errorMsg = "";

            foreach (var i in dataFinger)
            {
                if (match == 0)
                {
                    var count = 0;
                    try
                    {
                        count++;
                        var b64 = i.iso;

                        string b64String = "";
                        if (!string.IsNullOrEmpty(b64))
                        {
                            b64String = b64.Decrypt(Phrase.FileEncryption);
                            if (count == 1)
                            {
                                urlImg1 = i.file;
                                dec1 = b64String;
                            }
                            else
                            {
                                urlImg2 = i.file;
                                dec2 = b64String;
                            }

                            var matchingRes = await _imageRecognitionService.MatchImageBase64ToBase64(
                                new Base64ToBase64Req
                                {
                                    Base64Images1 = req.Base64Img,
                                    Base64Images2 = b64String
                                },
                                new UrlRequestRecognition
                                {
                                    BaseUrl = baseUrl,
                                    EndPoint = endPoint
                                })
                                .ConfigureAwait(false);

                            if (matchingRes?.Data?.IsFoundOrSuccess == true) match = 1;
                        }

                    }
                    catch (Exception ex)
                    {
                        errorMsg = ex.Message ?? (ex.InnerException.Message ?? "");
                    }
                }
            }

            if (match == 0)
            {
                var _logs = new Tbl_LogClientApps
                {
                    Param = "nik: " + req.Nik,
                    LvTeller = req.LvTeller,
                    Branch = req.Branch,
                    SubBranch = req.SubBranch,
                    ClientApps = req.ClientApps,
                    RequestTime = RequestTime,
                    ResponseTime = DateTime.Now,
                    CreatedTime = DateTime.Now,
                    EndPoint = req.EndPoint,
                    ErrorMessage = errorMsg,
                    CreatedByNpp = AuthNpp,
                    CreatedByUnitCode = AuthUnitCode
                };

                _ = _authRepository.InsertLogClientApps(_logs);

                return new ServiceResponse<ProfileByNik>
                {
                    Status = "error",
                    Message = _ErrorMessageConfig.MatchingFingerFailed,
                    Data = new ProfileByNik
                    {
                        RequestedImg = req.Base64Img,
                        ImgUrlPath1 = urlImg1,
                        ImgUrlPath2 = urlImg2,
                        ktp_FingerKanan = dec1,
                        ktp_FingerKiri = dec2,
                        ErrorMsg = errorMsg
                    }
                };
            }

            var _log = new Tbl_LogClientApps
            {
                Param = "nik: " + req.Nik,
                LvTeller = req.LvTeller,
                Branch = req.Branch,
                SubBranch = req.SubBranch,
                ClientApps = req.ClientApps,
                RequestTime = RequestTime,
                ResponseTime = DateTime.Now,
                CreatedTime = DateTime.Now,
                EndPoint = req.EndPoint,
                ErrorMessage = errorMsg,
                CreatedByNpp = AuthNpp,
                CreatedByUnitCode = AuthUnitCode
            };

            _ = _authRepository.InsertLogClientApps(_log);

            return new ServiceResponse<ProfileByNik>
            {
                Status = "sukses",
                Message = _SuccessMessageConfig.MatchingSuccess
            };
        }

        public async Task<ServiceResponse<ProfileByNik>> IsFingerLoopMatchNewData(ProfileLoopReq req, string baseUrl, string endPoint, DateTime RequestTime, string AuthNpp, string AuthUnitCode)
        {
            var dataFinger = await _authRepository.GetDataFingerGeneralEmpNew(req.Nik)
                .ConfigureAwait(false);

            if (!dataFinger.Any()) return new ServiceResponse<ProfileByNik> { Message = _ErrorMessageConfig.MatchingFingerNotFound, Status = "error" };

            int? match = 0;

            using WebClient webClient = new();

            var urlImg1 = "";
            var urlImg2 = "";
            var dec1 = "";
            var dec2 = "";
            var errorMsg = "";

            foreach (var i in dataFinger)
            {
                if (match == 0)
                {
                    var count = 0;
                    try
                    {
                        count++;
                        var b64 = i.iso;

                        string b64String = "";
                        if (!string.IsNullOrEmpty(b64))
                        {
                            b64String = b64.Decrypt(Phrase.FileEncryption);
                            if (count == 1)
                            {
                                urlImg1 = i.file;
                                dec1 = b64String;
                            }
                            else
                            {
                                urlImg2 = i.file;
                                dec2 = b64String;
                            }

                            var matchingRes = await _imageRecognitionService.MatchImageBase64ToBase64(
                                new Base64ToBase64Req
                                {
                                    Base64Images1 = req.Base64Img,
                                    Base64Images2 = b64String
                                },
                                new UrlRequestRecognition
                                {
                                    BaseUrl = baseUrl,
                                    EndPoint = endPoint
                                })
                                .ConfigureAwait(false);

                            if (matchingRes?.Data?.IsFoundOrSuccess == true) match = 1;
                        }

                    }
                    catch (Exception ex)
                    {
                        errorMsg = ex.Message ?? (ex.InnerException.Message ?? "");
                    }
                }
            }

            if (match == 0)
            {
                var _logs = new Tbl_LogClientApps
                {
                    Param = "nik: " + req.Nik,
                    LvTeller = req.LvTeller,
                    Branch = req.Branch,
                    SubBranch = req.SubBranch,
                    ClientApps = req.ClientApps,
                    RequestTime = RequestTime,
                    ResponseTime = DateTime.Now,
                    CreatedTime = DateTime.Now,
                    EndPoint = req.EndPoint,
                    ErrorMessage = errorMsg,
                    CreatedByNpp = AuthNpp,
                    CreatedByUnitCode = AuthUnitCode
                };

                _ = _authRepository.InsertLogClientApps(_logs);

                return new ServiceResponse<ProfileByNik>
                {
                    Status = "error",
                    Message = _ErrorMessageConfig.MatchingFingerFailed,
                    Data = new ProfileByNik
                    {
                        RequestedImg = req.Base64Img,
                        ImgUrlPath1 = urlImg1,
                        ImgUrlPath2 = urlImg2,
                        ktp_FingerKanan = dec1,
                        ktp_FingerKiri = dec2,
                        ErrorMsg = errorMsg
                    }
                };
            }

            var profile = await _profileRepository.GetProfileByNik(req.Nik).ConfigureAwait(false);

            var _log = new Tbl_LogClientApps
            {
                Param = "nik: " + req.Nik,
                LvTeller = req.LvTeller,
                Branch = req.Branch,
                SubBranch = req.SubBranch,
                ClientApps = req.ClientApps,
                RequestTime = RequestTime,
                ResponseTime = DateTime.Now,
                CreatedTime = DateTime.Now,
                EndPoint = req.EndPoint,
                ErrorMessage = errorMsg,
                CreatedByNpp = AuthNpp,
                CreatedByUnitCode = AuthUnitCode
            };

            _ = _authRepository.InsertLogClientApps(_log);

            return new ServiceResponse<ProfileByNik>
            {
                Status = "sukses",
                Message = _SuccessMessageConfig.MatchingSuccess,
                Data = ConvertUrlToB64FingerEncOnly(profile)
            };
        }

        public async Task<ServiceResponse<ProfileByNik>> IsFingerLoopMatchNewIsoFile(ProfileLoopReq req, string baseUrl, string endPoint, DateTime RequestTime, string AuthNpp, string AuthUnitCode)
        {
            var dataFinger = await _authRepository.GetDataFingerGeneralEmpNewFileIso(req.Nik)
                .ConfigureAwait(false);

            if (!dataFinger.Any()) return new ServiceResponse<ProfileByNik> { Message = _ErrorMessageConfig.MatchingFingerNotFound, Status = "error" };

            int? match = 0;

            using WebClient webClient = new();

            var urlImg1 = "";
            var urlImg2 = "";
            var dec1 = "";
            var dec2 = "";
            var errorMsg = "";

            foreach (var i in dataFinger)
            {
                if (match == 0)
                {
                    var count = 0;
                    try
                    {
                        count++;
                        var b64 = webClient.DownloadData(i.file ?? "");

                        string b64String = "";

                        using (var r = new StreamReader(new MemoryStream(b64)))
                        {
                            var encryptedText = r.ReadToEnd();
                            b64String = encryptedText.Decrypt(Phrase.FileEncryption);
                        }

                        if (count == 1)
                        {
                            urlImg1 = i.file;
                            dec1 = b64String;
                        }
                        else
                        {
                            urlImg2 = i.file;
                            dec2 = b64String;
                        }

                        var matchingRes = await _imageRecognitionService.MatchImageBase64ToBase64(
                            new Base64ToBase64Req
                            {
                                Base64Images1 = req.Base64Img,
                                Base64Images2 = b64String
                            },
                            new UrlRequestRecognition
                            {
                                BaseUrl = baseUrl,
                                EndPoint = endPoint
                            })
                            .ConfigureAwait(false);

                        if (matchingRes?.Data?.IsFoundOrSuccess == true) match = 1;
                    }
                    catch (Exception ex)
                    {
                        errorMsg = ex.Message ?? (ex.InnerException.Message ?? "");
                    }
                }
            }

            if (match == 0)
            {
                var _logs = new Tbl_LogClientApps
                {
                    Param = "nik: " + req.Nik,
                    LvTeller = req.LvTeller,
                    Branch = req.Branch,
                    SubBranch = req.SubBranch,
                    ClientApps = req.ClientApps,
                    RequestTime = RequestTime,
                    ResponseTime = DateTime.Now,
                    CreatedTime = DateTime.Now,
                    EndPoint = req.EndPoint,
                    ErrorMessage = errorMsg,
                    CreatedByNpp = AuthNpp,
                    CreatedByUnitCode = AuthUnitCode
                };

                _ = _authRepository.InsertLogClientApps(_logs);

                return new ServiceResponse<ProfileByNik>
                {
                    Status = "error",
                    Message = _ErrorMessageConfig.MatchingFingerFailed,
                    Data = new ProfileByNik
                    {
                        RequestedImg = req.Base64Img,
                        ImgUrlPath1 = urlImg1,
                        ImgUrlPath2 = urlImg2,
                        ktp_FingerKanan = dec1,
                        ktp_FingerKiri = dec2,
                        ErrorMsg = errorMsg
                    }
                };
            }

            var _log = new Tbl_LogClientApps
            {
                Param = "nik: " + req.Nik,
                LvTeller = req.LvTeller,
                Branch = req.Branch,
                SubBranch = req.SubBranch,
                ClientApps = req.ClientApps,
                RequestTime = RequestTime,
                ResponseTime = DateTime.Now,
                CreatedTime = DateTime.Now,
                EndPoint = req.EndPoint,
                ErrorMessage = errorMsg,
                CreatedByNpp = AuthNpp,
                CreatedByUnitCode = AuthUnitCode
            };

            _ = _authRepository.InsertLogClientApps(_log);

            return new ServiceResponse<ProfileByNik>
            {
                Status = "sukses",
                Message = _SuccessMessageConfig.MatchingSuccess
            };
        }

        public async Task<ServiceResponse<ProfileByNik>> GetAuthKTPDataFingerEmpEncOnlyIso(ProfileLoopReq req, string baseUrl, string endPoint, DateTime RequestTime, string AuthNpp, string AuthUnitCode)
        {
            var dataFinger = await _authRepository.GetDataFingerGeneralEmpIso(req.Nik)
                .ConfigureAwait(false);

            if (!dataFinger.Any()) return new ServiceResponse<ProfileByNik> { Message = _ErrorMessageConfig.MatchingFingerNotFound, Status = "error" };

            int? match = 0;

            using WebClient webClient = new();

            var urlImg1 = "";
            var urlImg2 = "";
            var dec1 = "";
            var dec2 = "";
            var errorMsg = "";

            foreach (var i in dataFinger)
            {
                if (match == 0)
                {
                    var count = 0;
                    try
                    {
                        count++;
                        var b64 = webClient.DownloadData(i.file ?? "");

                        string b64String = "";

                        using (var r = new StreamReader(new MemoryStream(b64)))
                        {
                            var encryptedText = r.ReadToEnd();
                            b64String = encryptedText.Decrypt(Phrase.FileEncryption);
                        }

                        if (count == 1)
                        {
                            urlImg1 = i.file;
                            dec1 = b64String;
                        }
                        else
                        {
                            urlImg2 = i.file;
                            dec2 = b64String;
                        }

                        var matchingRes = await _imageRecognitionService.MatchImageBase64ToBase64(
                            new Base64ToBase64Req
                            {
                                Base64Images1 = req.Base64Img,
                                Base64Images2 = b64String
                            },
                            new UrlRequestRecognition
                            {
                                BaseUrl = baseUrl,
                                EndPoint = endPoint
                            })
                            .ConfigureAwait(false);

                        if (matchingRes?.Data?.IsFoundOrSuccess == true) match = 1;
                    }
                    catch (Exception ex)
                    {
                        errorMsg = ex.Message ?? (ex.InnerException.Message ?? "");
                    }
                }
            }

            if (match == 0)
            {
                var _logs = new Tbl_LogClientApps
                {
                    Param = "nik: " + req.Nik + ",base64Img: " + req.Base64Img,
                    LvTeller = req.LvTeller,
                    Branch = req.Branch,
                    SubBranch = req.SubBranch,
                    ClientApps = req.ClientApps,
                    RequestTime = RequestTime,
                    ResponseTime = DateTime.Now,
                    CreatedTime = DateTime.Now,
                    EndPoint = req.EndPoint,
                    ErrorMessage = errorMsg,
                    CreatedByNpp = AuthNpp,
                    CreatedByUnitCode = AuthUnitCode
                };

                _ = _authRepository.InsertLogClientApps(_logs);

                return new ServiceResponse<ProfileByNik>
                {
                    Status = "error",
                    Message = _ErrorMessageConfig.MatchingFingerFailed,
                    Data = new ProfileByNik
                    {
                        RequestedImg = req.Base64Img,
                        ImgUrlPath1 = urlImg1,
                        ImgUrlPath2 = urlImg2,
                        ktp_FingerKanan = dec1,
                        ktp_FingerKiri = dec2,
                        ErrorMsg = errorMsg
                    }
                };
            }

            var profile = await _profileRepository.GetProfileByNikEmp(req.Nik).ConfigureAwait(false);

            var _log = new Tbl_LogClientApps
            {
                Param = "nik: " + req.Nik + ",base64Img: " + req.Base64Img,
                LvTeller = req.LvTeller,
                Branch = req.Branch,
                SubBranch = req.SubBranch,
                ClientApps = req.ClientApps,
                RequestTime = RequestTime,
                ResponseTime = DateTime.Now,
                CreatedTime = DateTime.Now,
                EndPoint = req.EndPoint,
                ErrorMessage = errorMsg,
                CreatedByNpp = AuthNpp,
                CreatedByUnitCode = AuthUnitCode
            };

            _ = _authRepository.InsertLogClientApps(_log);

            return new ServiceResponse<ProfileByNik>
            {
                Status = "sukses",
                Message = _SuccessMessageConfig.MatchingSuccess,
                Data = ConvertUrlToB64FingerEncOnly(profile)
            };
        }

        public async Task<ServiceResponse<ProfileByNik>> IsFingerNppIsoMatch(ProfileLoopReq req, string baseUrl, string endPoint, DateTime RequestTime, string AuthNpp, string AuthUnitCode)
        {
            var dataFinger = await _authRepository.GetDataFingerGeneralEmpIso(req.Nik)
                .ConfigureAwait(false);

            if (!dataFinger.Any()) return new ServiceResponse<ProfileByNik> { Message = _ErrorMessageConfig.MatchingFingerNotFound, Status = "error" };

            int? match = 0;

            using WebClient webClient = new();

            var urlImg1 = "";
            var urlImg2 = "";
            var dec1 = "";
            var dec2 = "";
            var errorMsg = "";

            foreach (var i in dataFinger)
            {
                if (match == 0)
                {
                    var count = 0;
                    try
                    {
                        count++;
                        var b64 = webClient.DownloadData(i.file ?? "");

                        string b64String = "";

                        using (var r = new StreamReader(new MemoryStream(b64)))
                        {
                            var encryptedText = r.ReadToEnd();
                            b64String = encryptedText.Decrypt(Phrase.FileEncryption);
                        }

                        if (count == 1)
                        {
                            urlImg1 = i.file;
                            dec1 = b64String;
                        }
                        else
                        {
                            urlImg2 = i.file;
                            dec2 = b64String;
                        }

                        var matchingRes = await _imageRecognitionService.MatchImageBase64ToBase64(
                            new Base64ToBase64Req
                            {
                                Base64Images1 = req.Base64Img,
                                Base64Images2 = b64String
                            },
                            new UrlRequestRecognition
                            {
                                BaseUrl = baseUrl,
                                EndPoint = endPoint
                            })
                            .ConfigureAwait(false);

                        if (matchingRes?.Data?.IsFoundOrSuccess == true) match = 1;
                    }
                    catch (Exception ex)
                    {
                        errorMsg = ex.Message ?? (ex.InnerException.Message ?? "");
                    }
                }
            }

            if (match == 0)
            {
                var _logs = new Tbl_LogClientApps
                {
                    Param = "nik: " + req.Nik,
                    LvTeller = req.LvTeller,
                    Branch = req.Branch,
                    SubBranch = req.SubBranch,
                    ClientApps = req.ClientApps,
                    RequestTime = RequestTime,
                    ResponseTime = DateTime.Now,
                    CreatedTime = DateTime.Now,
                    EndPoint = req.EndPoint,
                    ErrorMessage = errorMsg,
                    CreatedByNpp = AuthNpp,
                    CreatedByUnitCode = AuthUnitCode
                };

                _ = _authRepository.InsertLogClientApps(_logs);

                return new ServiceResponse<ProfileByNik>
                {
                    Status = "error",
                    Message = _ErrorMessageConfig.MatchingFingerFailed,
                    Data = new ProfileByNik
                    {
                        RequestedImg = req.Base64Img,
                        ImgUrlPath1 = urlImg1,
                        ImgUrlPath2 = urlImg2,
                        ktp_FingerKanan = dec1,
                        ktp_FingerKiri = dec2,
                        ErrorMsg = errorMsg
                    }
                };
            }

            var _log = new Tbl_LogClientApps
            {
                Param = "nik: " + req.Nik,
                LvTeller = req.LvTeller,
                Branch = req.Branch,
                SubBranch = req.SubBranch,
                ClientApps = req.ClientApps,
                RequestTime = RequestTime,
                ResponseTime = DateTime.Now,
                CreatedTime = DateTime.Now,
                EndPoint = req.EndPoint,
                ErrorMessage = errorMsg,
                CreatedByNpp = AuthNpp,
                CreatedByUnitCode = AuthUnitCode
            };

            _ = _authRepository.InsertLogClientApps(_log);

            return new ServiceResponse<ProfileByNik>
            {
                Status = "sukses",
                Message = _SuccessMessageConfig.MatchingSuccess
            };
        }

        public async Task<ServiceResponse<ProfileByNik>> IsFingerNppIsoMatchDB(ProfileLoopReq req, string baseUrl, string endPoint, DateTime RequestTime, string AuthNpp, string AuthUnitCode)
        {
            var dataFinger = await _authRepository.GetDataFingerGeneralEmpIso(req.Nik)
                .ConfigureAwait(false);

            if (!dataFinger.Any()) return new ServiceResponse<ProfileByNik> { Message = _ErrorMessageConfig.MatchingFingerNotFound, Status = "error" };

            int? match = 0;

            using WebClient webClient = new();

            var urlImg1 = "";
            var urlImg2 = "";
            var dec1 = "";
            var dec2 = "";
            var errorMsg = "";

            foreach (var i in dataFinger)
            {
                if (match == 0)
                {
                    var count = 0;
                    try
                    {
                        count++;
                        var b64 = i.iso;

                        string b64String = "";

                        if (!string.IsNullOrEmpty(b64))
                        {
                            b64String = b64.Decrypt(Phrase.FileEncryption);

                            if (count == 1)
                            {
                                urlImg1 = i.file;
                                dec1 = b64String;
                            }
                            else
                            {
                                urlImg2 = i.file;
                                dec2 = b64String;
                            }

                            var matchingRes = await _imageRecognitionService.MatchImageBase64ToBase64(
                                new Base64ToBase64Req
                                {
                                    Base64Images1 = req.Base64Img,
                                    Base64Images2 = b64String
                                },
                                new UrlRequestRecognition
                                {
                                    BaseUrl = baseUrl,
                                    EndPoint = endPoint
                                })
                                .ConfigureAwait(false);

                            if (matchingRes?.Data?.IsFoundOrSuccess == true) match = 1;
                        }
                    }
                    catch (Exception ex)
                    {
                        errorMsg = ex.Message ?? (ex.InnerException.Message ?? "");
                    }
                }
            }

            if (match == 0)
            {
                var _logs = new Tbl_LogClientApps
                {
                    Param = "nik: " + req.Nik,
                    LvTeller = req.LvTeller,
                    Branch = req.Branch,
                    SubBranch = req.SubBranch,
                    ClientApps = req.ClientApps,
                    RequestTime = RequestTime,
                    ResponseTime = DateTime.Now,
                    CreatedTime = DateTime.Now,
                    EndPoint = req.EndPoint,
                    ErrorMessage = errorMsg,
                    CreatedByNpp = AuthNpp,
                    CreatedByUnitCode = AuthUnitCode
                };

                _ = _authRepository.InsertLogClientApps(_logs);

                return new ServiceResponse<ProfileByNik>
                {
                    Status = "error",
                    Message = _ErrorMessageConfig.MatchingFingerFailed,
                    Data = new ProfileByNik
                    {
                        RequestedImg = req.Base64Img,
                        ImgUrlPath1 = urlImg1,
                        ImgUrlPath2 = urlImg2,
                        ktp_FingerKanan = dec1,
                        ktp_FingerKiri = dec2,
                        ErrorMsg = errorMsg
                    }
                };
            }

            var _log = new Tbl_LogClientApps
            {
                Param = "nik: " + req.Nik,
                LvTeller = req.LvTeller,
                Branch = req.Branch,
                SubBranch = req.SubBranch,
                ClientApps = req.ClientApps,
                RequestTime = RequestTime,
                ResponseTime = DateTime.Now,
                CreatedTime = DateTime.Now,
                EndPoint = req.EndPoint,
                ErrorMessage = errorMsg,
                CreatedByNpp = AuthNpp,
                CreatedByUnitCode = AuthUnitCode
            };

            _ = _authRepository.InsertLogClientApps(_log);

            return new ServiceResponse<ProfileByNik>
            {
                Status = "sukses",
                Message = _SuccessMessageConfig.MatchingSuccess
            };
        }

        public async Task<ServiceResponse<ProfileByNik>> GetAuthKTPDataByCifFingerEncOnly(ProfileLoopByCifReq req, string baseUrl, string endPoint, DateTime RequestTime)
        {
            var dataFinger = await _authRepository.GetDataFingerGeneralByCif(req.Cif)
                .ConfigureAwait(false);

            if (!dataFinger.Any()) return new ServiceResponse<ProfileByNik> { Message = _ErrorMessageConfig.MatchingFingerNotFound, Status = "Empty" };

            int? match = 0;

            using WebClient webClient = new();

            var urlImg1 = "";
            var urlImg2 = "";
            var dec1 = "";
            var dec2 = "";
            var errorMsg = "";

            foreach (var i in dataFinger)
            {
                if (match == 0)
                {
                    var count = 0;
                    try
                    {
                        count++;
                        var b64 = webClient.DownloadData(i.file ?? "");

                        string b64String = "";

                        using (var r = new StreamReader(new MemoryStream(b64)))
                        {
                            var encryptedText = r.ReadToEnd();
                            b64String = encryptedText.Decrypt(Phrase.FileEncryption);
                        }

                        if (count == 1)
                        {
                            urlImg1 = i.file;
                            dec1 = b64String;
                        }
                        else
                        {
                            urlImg2 = i.file;
                            dec2 = b64String;
                        }

                        var matchingRes = await _imageRecognitionService.MatchImageBase64ToBase64(
                            new Base64ToBase64Req
                            {
                                Base64Images1 = req.Base64Img,
                                Base64Images2 = b64String
                            },
                            new UrlRequestRecognition
                            {
                                BaseUrl = baseUrl,
                                EndPoint = endPoint
                            })
                            .ConfigureAwait(false);

                        if (matchingRes?.Data?.IsFoundOrSuccess == true) match = 1;
                    }
                    catch (Exception ex)
                    {
                        errorMsg = ex.Message ?? (ex.InnerException.Message ?? "");
                    }
                }
            }

            if (match == 0)
            {
                var log = new Tbl_LogClientApps
                {
                    Param = "cif: " + req.Cif + ",base64Img: " + req.Base64Img,
                    LvTeller = req.LvTeller,
                    Branch = req.Branch,
                    SubBranch = req.SubBranch,
                    ClientApps = req.ClientApps,
                    RequestTime = RequestTime,
                    ResponseTime = DateTime.Now,
                    CreatedTime = DateTime.Now,
                    EndPoint = req.EndPoint,
                    ErrorMessage = errorMsg,
                    CreatedByNpp = req.Npp,
                    CreatedByUnitCode = req.UnitCode
                };

                _ = _authRepository.InsertLogClientApps(log);

                return new ServiceResponse<ProfileByNik>
                {
                    Status = "error",
                    Message = _ErrorMessageConfig.MatchingError,
                    Data = new ProfileByNik
                    {
                        RequestedImg = req.Base64Img,
                        ImgUrlPath1 = urlImg1,
                        ImgUrlPath2 = urlImg2,
                        ktp_FingerKanan = dec1,
                        ktp_FingerKiri = dec2,
                        ErrorMsg = errorMsg
                    }
                };
            }

            var profile = await _profileRepository.GetProfileByCIF(req.Cif).ConfigureAwait(false);

            var _log = new Tbl_LogClientApps
            {
                Param = "cif: " + req.Cif + ",base64Img: " + req.Base64Img,
                LvTeller = req.LvTeller,
                Branch = req.Branch,
                SubBranch = req.SubBranch,
                ClientApps = req.ClientApps,
                RequestTime = RequestTime,
                ResponseTime = DateTime.Now,
                CreatedTime = DateTime.Now,
                EndPoint = req.EndPoint,
                ErrorMessage = "",
                CreatedByNpp = req.Npp,
                CreatedByUnitCode = req.UnitCode
            };

            _ = _authRepository.InsertLogClientApps(_log);

            return new ServiceResponse<ProfileByNik>
            {
                Status = "sukses",
                Message = _SuccessMessageConfig.MatchingSuccess,
                Data = ConvertUrlToB64FingerEncOnly(profile)
            };
        }

        public async Task<ServiceResponse<ProfileByNik>> GetAuthKTPDataByCifFingerEncOnlyIso(ProfileLoopByCifReq req, string baseUrl, string endPoint, DateTime RequestTime)
        {
            var dataFinger = await _authRepository.GetDataFingerGeneralByCifIso(req.Cif)
                .ConfigureAwait(false);

            if (!dataFinger.Any()) return new ServiceResponse<ProfileByNik> { Message = _ErrorMessageConfig.MatchingFingerNotFound, Status = "Empty" };

            int? match = 0;

            using WebClient webClient = new();

            var urlImg1 = "";
            var urlImg2 = "";
            var dec1 = "";
            var dec2 = "";
            var errorMsg = "";

            foreach (var i in dataFinger)
            {
                if (match == 0)
                {
                    var count = 0;
                    try
                    {
                        count++;
                        var b64 = webClient.DownloadData(i.file ?? "");

                        string b64String = "";

                        using (var r = new StreamReader(new MemoryStream(b64)))
                        {
                            var encryptedText = r.ReadToEnd();
                            b64String = encryptedText.Decrypt(Phrase.FileEncryption);
                        }

                        if (count == 1)
                        {
                            urlImg1 = i.file;
                            dec1 = b64String;
                        }
                        else
                        {
                            urlImg2 = i.file;
                            dec2 = b64String;
                        }

                        var matchingRes = await _imageRecognitionService.MatchImageBase64ToBase64(
                            new Base64ToBase64Req
                            {
                                Base64Images1 = req.Base64Img,
                                Base64Images2 = b64String
                            },
                            new UrlRequestRecognition
                            {
                                BaseUrl = baseUrl,
                                EndPoint = endPoint
                            })
                            .ConfigureAwait(false);

                        if (matchingRes?.Data?.IsFoundOrSuccess == true) match = 1;
                    }
                    catch (Exception ex)
                    {
                        errorMsg = ex.Message ?? (ex.InnerException.Message ?? "");
                    }
                }
            }

            if (match == 0)
            {
                var log = new Tbl_LogClientApps
                {
                    Param = "cif: " + req.Cif + ",base64Img: " + req.Base64Img,
                    LvTeller = req.LvTeller,
                    Branch = req.Branch,
                    SubBranch = req.SubBranch,
                    ClientApps = req.ClientApps,
                    RequestTime = RequestTime,
                    ResponseTime = DateTime.Now,
                    CreatedTime = DateTime.Now,
                    EndPoint = req.EndPoint,
                    ErrorMessage = errorMsg,
                    CreatedByNpp = req.Npp,
                    CreatedByUnitCode = req.UnitCode
                };

                _ = _authRepository.InsertLogClientApps(log);

                return new ServiceResponse<ProfileByNik>
                {
                    Status = "error",
                    Message = _ErrorMessageConfig.MatchingError,
                    Data = new ProfileByNik
                    {
                        RequestedImg = req.Base64Img,
                        ImgUrlPath1 = urlImg1,
                        ImgUrlPath2 = urlImg2,
                        ktp_FingerKanan = dec1,
                        ktp_FingerKiri = dec2,
                        ErrorMsg = errorMsg
                    }
                };
            }

            var profile = await _profileRepository.GetProfileByCIF(req.Cif).ConfigureAwait(false);

            var _log = new Tbl_LogClientApps
            {
                Param = "cif: " + req.Cif + ",base64Img: " + req.Base64Img,
                LvTeller = req.LvTeller,
                Branch = req.Branch,
                SubBranch = req.SubBranch,
                ClientApps = req.ClientApps,
                RequestTime = RequestTime,
                ResponseTime = DateTime.Now,
                CreatedTime = DateTime.Now,
                EndPoint = req.EndPoint,
                ErrorMessage = "",
                CreatedByNpp = req.Npp,
                CreatedByUnitCode = req.UnitCode
            };

            _ = _authRepository.InsertLogClientApps(_log);

            return new ServiceResponse<ProfileByNik>
            {
                Status = "sukses",
                Message = _SuccessMessageConfig.MatchingSuccess,
                Data = ConvertUrlToB64FingerEncOnly(profile)
            };
        }

        public async Task<ServiceResponse<ProfileByNik>> GetAuthKTPDataByCifFingerEncOnlyIsoDb(ProfileLoopByCifReq req, string baseUrl, string endPoint, DateTime RequestTime)
        {
            var dataFinger = await _authRepository.GetDataFingerGeneralByCifIso(req.Cif)
                .ConfigureAwait(false);

            if (!dataFinger.Any()) return new ServiceResponse<ProfileByNik> { Message = _ErrorMessageConfig.MatchingFingerNotFound, Status = "Empty" };

            int? match = 0;

            using WebClient webClient = new();

            var urlImg1 = "";
            var urlImg2 = "";
            var dec1 = "";
            var dec2 = "";
            var errorMsg = "";

            foreach (var i in dataFinger)
            {
                if (match == 0)
                {
                    var count = 0;
                    try
                    {
                        count++;
                        var b64 = i.iso;

                        string b64String = "";

                        b64String = b64.Decrypt(Phrase.FileEncryption);

                        if (count == 1)
                        {
                            urlImg1 = i.file;
                            dec1 = b64String;
                        }
                        else
                        {
                            urlImg2 = i.file;
                            dec2 = b64String;
                        }

                        var matchingRes = await _imageRecognitionService.MatchImageBase64ToBase64(
                            new Base64ToBase64Req
                            {
                                Base64Images1 = req.Base64Img,
                                Base64Images2 = b64String
                            },
                            new UrlRequestRecognition
                            {
                                BaseUrl = baseUrl,
                                EndPoint = endPoint
                            })
                            .ConfigureAwait(false);

                        if (matchingRes?.Data?.IsFoundOrSuccess == true) match = 1;
                    }
                    catch (Exception ex)
                    {
                        errorMsg = ex.Message ?? (ex.InnerException.Message ?? "");
                    }
                }
            }

            if (match == 0)
            {
                var log = new Tbl_LogClientApps
                {
                    Param = "cif: " + req.Cif + ",base64Img: " + req.Base64Img,
                    LvTeller = req.LvTeller,
                    Branch = req.Branch,
                    SubBranch = req.SubBranch,
                    ClientApps = req.ClientApps,
                    RequestTime = RequestTime,
                    ResponseTime = DateTime.Now,
                    CreatedTime = DateTime.Now,
                    EndPoint = req.EndPoint,
                    ErrorMessage = errorMsg,
                    CreatedByNpp = req.Npp,
                    CreatedByUnitCode = req.UnitCode
                };

                _ = _authRepository.InsertLogClientApps(log);

                return new ServiceResponse<ProfileByNik>
                {
                    Status = "error",
                    Message = _ErrorMessageConfig.MatchingError,
                    Data = new ProfileByNik
                    {
                        RequestedImg = req.Base64Img,
                        ImgUrlPath1 = urlImg1,
                        ImgUrlPath2 = urlImg2,
                        ktp_FingerKanan = dec1,
                        ktp_FingerKiri = dec2,
                        ErrorMsg = errorMsg
                    }
                };
            }

            var profile = await _profileRepository.GetProfileByCIF(req.Cif).ConfigureAwait(false);

            var _log = new Tbl_LogClientApps
            {
                Param = "cif: " + req.Cif + ",base64Img: " + req.Base64Img,
                LvTeller = req.LvTeller,
                Branch = req.Branch,
                SubBranch = req.SubBranch,
                ClientApps = req.ClientApps,
                RequestTime = RequestTime,
                ResponseTime = DateTime.Now,
                CreatedTime = DateTime.Now,
                EndPoint = req.EndPoint,
                ErrorMessage = "",
                CreatedByNpp = req.Npp,
                CreatedByUnitCode = req.UnitCode
            };

            _ = _authRepository.InsertLogClientApps(_log);

            return new ServiceResponse<ProfileByNik>
            {
                Status = "sukses",
                Message = _SuccessMessageConfig.MatchingSuccess,
                Data = ConvertUrlToB64FingerEncOnly(profile)
            };
        }

        public async Task<ServiceResponse<ProfileByNik>> GetAuthKTPDataByCifFingerEncOnlyNew(ProfileLoopByCifReq req, string baseUrl, string endPoint, DateTime RequestTime)
        {
            var dataFinger = await _authRepository.GetDataFingerGeneralByCifNew(req.Cif)
                .ConfigureAwait(false);

            if (!dataFinger.Any()) return new ServiceResponse<ProfileByNik> { Message = _ErrorMessageConfig.MatchingFingerNotFound, Status = "Empty" };

            int? match = 0;

            using WebClient webClient = new();

            var urlImg1 = "";
            var urlImg2 = "";
            var dec1 = "";
            var dec2 = "";
            var errorMsg = "";

            foreach (var i in dataFinger)
            {
                if (match == 0)
                {
                    var count = 0;
                    try
                    {
                        count++;
                        var b64 = i.iso;

                        if (!string.IsNullOrEmpty(b64))
                        {
                            string b64String = "";

                            b64String = b64.Decrypt(Phrase.FileEncryption);

                            if (count == 1)
                            {
                                urlImg1 = i.file;
                                dec1 = b64String;
                            }
                            else
                            {
                                urlImg2 = i.file;
                                dec2 = b64String;
                            }

                            var matchingRes = await _imageRecognitionService.MatchImageBase64ToBase64(
                                new Base64ToBase64Req
                                {
                                    Base64Images1 = req.Base64Img,
                                    Base64Images2 = b64String
                                },
                                new UrlRequestRecognition
                                {
                                    BaseUrl = baseUrl,
                                    EndPoint = endPoint
                                })
                                .ConfigureAwait(false);

                            if (matchingRes?.Data?.IsFoundOrSuccess == true) match = 1;
                        }
                    }
                    catch (Exception ex)
                    {
                        errorMsg = ex.Message ?? (ex.InnerException.Message ?? "");
                    }
                }
            }

            if (match == 0)
            {
                var log = new Tbl_LogClientApps
                {
                    Param = "cif: " + req.Cif,
                    LvTeller = req.LvTeller,
                    Branch = req.Branch,
                    SubBranch = req.SubBranch,
                    ClientApps = req.ClientApps,
                    RequestTime = RequestTime,
                    ResponseTime = DateTime.Now,
                    CreatedTime = DateTime.Now,
                    EndPoint = req.EndPoint,
                    ErrorMessage = errorMsg,
                    CreatedByNpp = req.Npp,
                    CreatedByUnitCode = req.UnitCode
                };

                _ = _authRepository.InsertLogClientApps(log);

                return new ServiceResponse<ProfileByNik>
                {
                    Status = "error",
                    Message = _ErrorMessageConfig.MatchingError,
                    Data = new ProfileByNik
                    {
                        RequestedImg = req.Base64Img,
                        ImgUrlPath1 = urlImg1,
                        ImgUrlPath2 = urlImg2,
                        ktp_FingerKanan = dec1,
                        ktp_FingerKiri = dec2,
                        ErrorMsg = errorMsg
                    }
                };
            }


            var _log = new Tbl_LogClientApps
            {
                Param = "cif: " + req.Cif,
                LvTeller = req.LvTeller,
                Branch = req.Branch,
                SubBranch = req.SubBranch,
                ClientApps = req.ClientApps,
                RequestTime = RequestTime,
                ResponseTime = DateTime.Now,
                CreatedTime = DateTime.Now,
                EndPoint = req.EndPoint,
                ErrorMessage = "",
                CreatedByNpp = req.Npp,
                CreatedByUnitCode = req.UnitCode
            };

            _ = _authRepository.InsertLogClientApps(_log);

            return new ServiceResponse<ProfileByNik>
            {
                Status = "sukses",
                Message = _SuccessMessageConfig.MatchingSuccess,
            };
        }

        public async Task<ServiceResponse<ProfileByNik>> GetAuthKTPDataByCifFingerEncOnlyNewData(ProfileLoopByCifReq req, string baseUrl, string endPoint, DateTime RequestTime)
        {
            var dataFinger = await _authRepository.GetDataFingerGeneralByCifNew(req.Cif)
                .ConfigureAwait(false);

            if (!dataFinger.Any()) return new ServiceResponse<ProfileByNik> { Message = _ErrorMessageConfig.MatchingFingerNotFound, Status = "Empty" };

            int? match = 0;

            using WebClient webClient = new();

            var urlImg1 = "";
            var urlImg2 = "";
            var dec1 = "";
            var dec2 = "";
            var errorMsg = "";

            foreach (var i in dataFinger)
            {
                if (match == 0)
                {
                    var count = 0;
                    try
                    {
                        count++;
                        var b64 = i.iso;

                        if (!string.IsNullOrEmpty(b64))
                        {
                            string b64String = "";

                            b64String = b64.Decrypt(Phrase.FileEncryption);

                            if (count == 1)
                            {
                                urlImg1 = i.file;
                                dec1 = b64String;
                            }
                            else
                            {
                                urlImg2 = i.file;
                                dec2 = b64String;
                            }

                            var matchingRes = await _imageRecognitionService.MatchImageBase64ToBase64(
                                new Base64ToBase64Req
                                {
                                    Base64Images1 = req.Base64Img,
                                    Base64Images2 = b64String
                                },
                                new UrlRequestRecognition
                                {
                                    BaseUrl = baseUrl,
                                    EndPoint = endPoint
                                })
                                .ConfigureAwait(false);

                            if (matchingRes?.Data?.IsFoundOrSuccess == true) match = 1;
                        }
                    }
                    catch (Exception ex)
                    {
                        errorMsg = ex.Message ?? (ex.InnerException.Message ?? "");
                    }
                }
            }

            if (match == 0)
            {
                var log = new Tbl_LogClientApps
                {
                    Param = "cif: " + req.Cif,
                    LvTeller = req.LvTeller,
                    Branch = req.Branch,
                    SubBranch = req.SubBranch,
                    ClientApps = req.ClientApps,
                    RequestTime = RequestTime,
                    ResponseTime = DateTime.Now,
                    CreatedTime = DateTime.Now,
                    EndPoint = req.EndPoint,
                    ErrorMessage = errorMsg,
                    CreatedByNpp = req.Npp,
                    CreatedByUnitCode = req.UnitCode
                };

                _ = _authRepository.InsertLogClientApps(log);

                return new ServiceResponse<ProfileByNik>
                {
                    Status = "error",
                    Message = _ErrorMessageConfig.MatchingError,
                    Data = new ProfileByNik
                    {
                        RequestedImg = req.Base64Img,
                        ImgUrlPath1 = urlImg1,
                        ImgUrlPath2 = urlImg2,
                        ktp_FingerKanan = dec1,
                        ktp_FingerKiri = dec2,
                        ErrorMsg = errorMsg
                    }
                };
            }

            var profile = await _profileRepository.GetProfileByCIF(req.Cif).ConfigureAwait(false);

            var _log = new Tbl_LogClientApps
            {
                Param = "cif: " + req.Cif,
                LvTeller = req.LvTeller,
                Branch = req.Branch,
                SubBranch = req.SubBranch,
                ClientApps = req.ClientApps,
                RequestTime = RequestTime,
                ResponseTime = DateTime.Now,
                CreatedTime = DateTime.Now,
                EndPoint = req.EndPoint,
                ErrorMessage = "",
                CreatedByNpp = req.Npp,
                CreatedByUnitCode = req.UnitCode
            };

            _ = _authRepository.InsertLogClientApps(_log);

            return new ServiceResponse<ProfileByNik>
            {
                Status = "sukses",
                Message = _SuccessMessageConfig.MatchingSuccess,
                Data = ConvertUrlToB64FingerEncOnly(profile)
            };
        }

        public async Task<ServiceResponse<ProfileByNik>> GetAuthKTPDataByCifFingerEncOnlyNewFile(ProfileLoopByCifReq req, string baseUrl, string endPoint, DateTime RequestTime)
        {
            var dataFinger = await _authRepository.GetDataFingerGeneralByCifNewFile(req.Cif)
                .ConfigureAwait(false);

            if (!dataFinger.Any()) return new ServiceResponse<ProfileByNik> { Message = _ErrorMessageConfig.MatchingFingerNotFound, Status = "Empty" };

            int? match = 0;

            using WebClient webClient = new();

            var urlImg1 = "";
            var urlImg2 = "";
            var dec1 = "";
            var dec2 = "";
            var errorMsg = "";

            foreach (var i in dataFinger)
            {
                if (match == 0)
                {
                    var count = 0;
                    try
                    {
                        count++;
                        var b64 = webClient.DownloadData(i.file ?? "");

                        string b64String = "";

                        using (var r = new StreamReader(new MemoryStream(b64)))
                        {
                            var encryptedText = r.ReadToEnd();
                            b64String = encryptedText.Decrypt(Phrase.FileEncryption);
                        }

                        if (count == 1)
                        {
                            urlImg1 = i.file;
                            dec1 = b64String;
                        }
                        else
                        {
                            urlImg2 = i.file;
                            dec2 = b64String;
                        }

                        var matchingRes = await _imageRecognitionService.MatchImageBase64ToBase64(
                            new Base64ToBase64Req
                            {
                                Base64Images1 = req.Base64Img,
                                Base64Images2 = b64String
                            },
                            new UrlRequestRecognition
                            {
                                BaseUrl = baseUrl,
                                EndPoint = endPoint
                            })
                            .ConfigureAwait(false);

                        if (matchingRes?.Data?.IsFoundOrSuccess == true) match = 1;
                    }
                    catch (Exception ex)
                    {
                        errorMsg = ex.Message ?? (ex.InnerException.Message ?? "");
                    }
                }
            }

            if (match == 0)
            {
                var log = new Tbl_LogClientApps
                {
                    Param = "cif: " + req.Cif,
                    LvTeller = req.LvTeller,
                    Branch = req.Branch,
                    SubBranch = req.SubBranch,
                    ClientApps = req.ClientApps,
                    RequestTime = RequestTime,
                    ResponseTime = DateTime.Now,
                    CreatedTime = DateTime.Now,
                    EndPoint = req.EndPoint,
                    ErrorMessage = errorMsg,
                    CreatedByNpp = req.Npp,
                    CreatedByUnitCode = req.UnitCode
                };

                _ = _authRepository.InsertLogClientApps(log);

                return new ServiceResponse<ProfileByNik>
                {
                    Status = "error",
                    Message = _ErrorMessageConfig.MatchingError,
                    Data = new ProfileByNik
                    {
                        RequestedImg = req.Base64Img,
                        ImgUrlPath1 = urlImg1,
                        ImgUrlPath2 = urlImg2,
                        ktp_FingerKanan = dec1,
                        ktp_FingerKiri = dec2,
                        ErrorMsg = errorMsg
                    }
                };
            }

            //var profile = await _profileRepository.GetProfileByCIF(req.Cif).ConfigureAwait(false);

            var _log = new Tbl_LogClientApps
            {
                Param = "cif: " + req.Cif,
                LvTeller = req.LvTeller,
                Branch = req.Branch,
                SubBranch = req.SubBranch,
                ClientApps = req.ClientApps,
                RequestTime = RequestTime,
                ResponseTime = DateTime.Now,
                CreatedTime = DateTime.Now,
                EndPoint = req.EndPoint,
                ErrorMessage = "",
                CreatedByNpp = req.Npp,
                CreatedByUnitCode = req.UnitCode
            };

            _ = _authRepository.InsertLogClientApps(_log);

            return new ServiceResponse<ProfileByNik>
            {
                Status = "sukses",
                Message = _SuccessMessageConfig.MatchingSuccess,
                //Data = ConvertUrlToB64FingerEncOnly(profile)
            };
        }

        public async Task<ServiceResponse<ProfileByNik>> IsFingerByCifMatch(ProfileLoopByCifReq req, string baseUrl, string endPoint, DateTime RequestTime)
        {
            var dataFinger = await _authRepository.GetDataFingerGeneralByCif(req.Cif)
                .ConfigureAwait(false);

            if (!dataFinger.Any()) return new ServiceResponse<ProfileByNik> { Message = _ErrorMessageConfig.MatchingFingerNotFound, Status = "Empty" };

            int? match = 0;

            using WebClient webClient = new();

            var urlImg1 = "";
            var urlImg2 = "";
            var dec1 = "";
            var dec2 = "";
            var errorMsg = "";

            foreach (var i in dataFinger)
            {
                if (match == 0)
                {
                    var count = 0;
                    try
                    {
                        count++;
                        var b64 = webClient.DownloadData(i.file ?? "");

                        string b64String = "";

                        using (var r = new StreamReader(new MemoryStream(b64)))
                        {
                            var encryptedText = r.ReadToEnd();
                            b64String = encryptedText.Decrypt(Phrase.FileEncryption);
                        }

                        if (count == 1)
                        {
                            urlImg1 = i.file;
                            dec1 = b64String;
                        }
                        else
                        {
                            urlImg2 = i.file;
                            dec2 = b64String;
                        }

                        var matchingRes = await _imageRecognitionService.MatchImageBase64ToBase64(
                            new Base64ToBase64Req
                            {
                                Base64Images1 = req.Base64Img,
                                Base64Images2 = b64String
                            },
                            new UrlRequestRecognition
                            {
                                BaseUrl = baseUrl,
                                EndPoint = endPoint
                            })
                            .ConfigureAwait(false);

                        if (matchingRes?.Data?.IsFoundOrSuccess == true) match = 1;
                    }
                    catch (Exception ex)
                    {
                        errorMsg = ex.Message ?? (ex.InnerException.Message ?? "");
                    }
                }
            }

            if (match == 0)
            {
                var log = new Tbl_LogClientApps
                {
                    Param = "cif: " + req.Cif + ",base64Img: " + req.Base64Img,
                    LvTeller = req.LvTeller,
                    Branch = req.Branch,
                    SubBranch = req.SubBranch,
                    ClientApps = req.ClientApps,
                    RequestTime = RequestTime,
                    ResponseTime = DateTime.Now,
                    CreatedTime = DateTime.Now,
                    EndPoint = req.EndPoint,
                    ErrorMessage = errorMsg,
                    CreatedByNpp = req.Npp,
                    CreatedByUnitCode = req.UnitCode
                };

                _ = _authRepository.InsertLogClientApps(log);

                return new ServiceResponse<ProfileByNik>
                {
                    Status = "error",
                    Message = _ErrorMessageConfig.MatchingFingerFailed,
                    Data = new ProfileByNik
                    {
                        RequestedImg = req.Base64Img,
                        ImgUrlPath1 = urlImg1,
                        ImgUrlPath2 = urlImg2,
                        ktp_FingerKanan = dec1,
                        ktp_FingerKiri = dec2,
                        ErrorMsg = errorMsg
                    }
                };
            }

            var _log = new Tbl_LogClientApps
            {
                Param = "cif: " + req.Cif + ",base64Img: " + req.Base64Img,
                LvTeller = req.LvTeller,
                Branch = req.Branch,
                SubBranch = req.SubBranch,
                ClientApps = req.ClientApps,
                RequestTime = RequestTime,
                ResponseTime = DateTime.Now,
                CreatedTime = DateTime.Now,
                EndPoint = req.EndPoint,
                ErrorMessage = "",
                CreatedByNpp = req.Npp,
                CreatedByUnitCode = req.UnitCode
            };

            _ = _authRepository.InsertLogClientApps(_log);

            return new ServiceResponse<ProfileByNik>
            {
                Status = "sukses",
                Message = _SuccessMessageConfig.MatchingSuccess
            };
        }

        public async Task<ServiceResponse<ProfileByNik>> GetEmpAuthKTPDataByCifFingerEncOnly(string base64Img, string cif, string baseUrl, string endPoint)
        {
            var dataFinger = await _authRepository.GetDataFingerEmpGeneralByCif(cif)
                .ConfigureAwait(false);

            if (!dataFinger.Any()) return new ServiceResponse<ProfileByNik> { Message = _ErrorMessageConfig.MatchingFingerNotFound, Status = "Empty" };

            int? match = 0;

            using WebClient webClient = new();

            var urlImg1 = "";
            var urlImg2 = "";
            var dec1 = "";
            var dec2 = "";
            var errorMsg = "";

            foreach (var i in dataFinger)
            {
                if (match == 0)
                {
                    var count = 0;
                    try
                    {
                        count++;
                        var b64 = webClient.DownloadData(i.file ?? "");

                        string b64String = "";

                        using (var r = new StreamReader(new MemoryStream(b64)))
                        {
                            var encryptedText = r.ReadToEnd();
                            b64String = encryptedText.Decrypt(Phrase.FileEncryption);
                        }

                        if (count == 1)
                        {
                            urlImg1 = i.file;
                            dec1 = b64String;
                        }
                        else
                        {
                            urlImg2 = i.file;
                            dec2 = b64String;
                        }

                        var matchingRes = await _imageRecognitionService.MatchImageBase64ToBase64(
                            new Base64ToBase64Req
                            {
                                Base64Images1 = base64Img,
                                Base64Images2 = b64String
                            },
                            new UrlRequestRecognition
                            {
                                BaseUrl = baseUrl,
                                EndPoint = endPoint
                            })
                            .ConfigureAwait(false);

                        if (matchingRes?.Data?.IsFoundOrSuccess == true) match = 1;
                    }
                    catch (Exception ex)
                    {
                        errorMsg = ex.Message ?? (ex.InnerException.Message ?? "");
                    }
                }
            }

            if (match == 0)
            {
                return new ServiceResponse<ProfileByNik>
                {
                    Status = "error",
                    Message = _ErrorMessageConfig.MatchingError,
                    Data = new ProfileByNik
                    {
                        RequestedImg = base64Img,
                        ImgUrlPath1 = urlImg1,
                        ImgUrlPath2 = urlImg2,
                        ktp_FingerKanan = dec1,
                        ktp_FingerKiri = dec2,
                        ErrorMsg = errorMsg
                    }
                };
            }

            var profile = await _profileRepository.GetProfileEmpByCIF(cif).ConfigureAwait(false);

            return new ServiceResponse<ProfileByNik>
            {
                Status = "sukses",
                Message = _SuccessMessageConfig.MatchingSuccess,
                Data = ConvertUrlToB64FingerEncOnly(profile)
            };
        }


        public async Task<Tbl_DataKTP_Demografis> UpdateCIF(string nik, string cif, string source, string npp, string uname, string unitCode)
        {
            var prof = await _profileRepository.GetDataDemografis(nik);

            if (prof == null)
            {
                return null;
            }

            //var log = new Tbl_DataKTP_CIF
            //{
            //    CIF = prof.CIF,
            //    CreatedTime = DateTime.Now,
            //    IsActive = true,
            //    IsDeleted = false,
            //    NIK = nik,
            //    Source = source
            //};

            prof.CIF = cif;
            prof.UpdatedCIFByBS_Time = DateTime.Now;
            prof.UpdatedCIFByBS_Username = uname;
            prof.UpdatedByNpp = npp;
            prof.UpdatedByUnitCode = unitCode;
            prof.UpdatedTime = DateTime.Now;
            prof.IsNasabahTemp = false;

            await _profileRepository.UpdateDataDemografis(prof);

            return prof;

            //_profileRepository.InsertCIFLog(log);
        }

        private static ProfileByNik ConvertUrlToB64(ProfileByNik profile)
        {
            using WebClient webClient = new();

            try
            {
                var ktp_PhotoCamB = string.IsNullOrWhiteSpace(profile.ktp_PhotoCam) ? Array.Empty<byte>() : webClient.DownloadData(profile.ktp_PhotoCam);

                if (ktp_PhotoCamB.Length < 1)
                {
                    profile.ktp_PhotoCam = "";
                }
                else
                {
                    using var r = new StreamReader(new MemoryStream(ktp_PhotoCamB));
                    var enc = r.ReadToEnd();
                    profile.ktp_PhotoCam = enc.Decrypt(Phrase.FileEncryption);
                }
            }
            catch (Exception)
            {
                profile.ktp_PhotoCam = "";
            }
            try
            {
                var ktp_PhotoKTPB = string.IsNullOrWhiteSpace(profile.ktp_PhotoKTP) ? Array.Empty<byte>() : webClient.DownloadData(profile.ktp_PhotoKTP);

                if (ktp_PhotoKTPB.Length < 1)
                {
                    profile.ktp_PhotoKTP = "";
                }
                else
                {
                    using var r = new StreamReader(new MemoryStream(ktp_PhotoKTPB));
                    var enc = r.ReadToEnd();
                    profile.ktp_PhotoKTP = enc.Decrypt(Phrase.FileEncryption);
                }
            }
            catch (Exception)
            {
                profile.ktp_PhotoKTP = "";
            }
            try
            {
                var ktp_FingerKananB = string.IsNullOrWhiteSpace(profile.ktp_FingerKanan) ? Array.Empty<byte>() : webClient.DownloadData(profile.ktp_FingerKanan);

                if (ktp_FingerKananB.Length < 1)
                {
                    profile.ktp_FingerKanan = "";
                }
                else
                {
                    using var r = new StreamReader(new MemoryStream(ktp_FingerKananB));
                    var enc = r.ReadToEnd();
                    profile.ktp_FingerKanan = enc.Decrypt(Phrase.FileEncryption);
                }
            }
            catch (Exception)
            {
                profile.ktp_FingerKanan = "";
            }
            try
            {
                var ktp_FingerKiriB = string.IsNullOrWhiteSpace(profile.ktp_FingerKiri) ? Array.Empty<byte>() : webClient.DownloadData(profile.ktp_FingerKiri);

                if (ktp_FingerKiriB.Length < 1)
                {
                    profile.ktp_FingerKiri = "";
                }
                else
                {
                    using var r = new StreamReader(new MemoryStream(ktp_FingerKiriB));
                    var enc = r.ReadToEnd();
                    profile.ktp_FingerKiri = enc.Decrypt(Phrase.FileEncryption);
                }
            }
            catch (Exception)
            {
                profile.ktp_FingerKiri = "";
            }
            try
            {
                var ktp_SignatureB = string.IsNullOrWhiteSpace(profile.ktp_Signature) ? Array.Empty<byte>() : webClient.DownloadData(profile.ktp_Signature);

                if (ktp_SignatureB.Length < 1)
                {
                    profile.ktp_Signature = "";
                }
                else
                {
                    using var r = new StreamReader(new MemoryStream(ktp_SignatureB));
                    var enc = r.ReadToEnd();
                    profile.ktp_Signature = enc.Decrypt(Phrase.FileEncryption);
                }
            }
            catch (Exception)
            {
                profile.ktp_Signature = "";
            }

            return profile;
        }

        private static ProfileByNik ConvertUrlToB64FingerEncOnly(ProfileByNik profile)
        {
            using WebClient webClient = new();

            if (profile == null) return new ProfileByNik();

            try
            {
                var ktp_PhotoCamB = string.IsNullOrWhiteSpace(profile?.ktp_PhotoCam) ? Array.Empty<byte>() : webClient.DownloadData(profile?.ktp_PhotoCam);

                if (ktp_PhotoCamB.Length < 1)
                {
                    profile.ktp_PhotoCam = "";
                }
                else
                {
                    profile.ktp_PhotoCam = Convert.ToBase64String(ktp_PhotoCamB);
                }
            }
            catch (Exception)
            {
                profile.ktp_PhotoCam = "";
            }
            try
            {
                var ktp_PhotoKTPB = string.IsNullOrWhiteSpace(profile.ktp_PhotoKTP) ? Array.Empty<byte>() : webClient.DownloadData(profile.ktp_PhotoKTP);

                if (ktp_PhotoKTPB.Length < 1)
                {
                    profile.ktp_PhotoKTP = "";
                }
                else
                {
                    profile.ktp_PhotoKTP = Convert.ToBase64String(ktp_PhotoKTPB);
                }
            }
            catch (Exception)
            {
                profile.ktp_PhotoKTP = "";
            }
            try
            {
                var ktp_FingerKananB = string.IsNullOrWhiteSpace(profile.ktp_FingerKanan) ? Array.Empty<byte>() : webClient.DownloadData(profile.ktp_FingerKanan);

                profile.ImgUrlPath1 = profile.ktp_FingerKanan;

                if (ktp_FingerKananB.Length < 1)
                {
                    profile.ktp_FingerKanan = "";
                }
                else
                {
                    using var r = new StreamReader(new MemoryStream(ktp_FingerKananB));
                    var enc = r.ReadToEnd();
                    profile.ktp_FingerKanan = enc.Decrypt(Phrase.FileEncryption);
                }
            }
            catch (Exception)
            {
                profile.ktp_FingerKanan = "";
            }
            try
            {
                var ktp_FingerKiriB = string.IsNullOrWhiteSpace(profile.ktp_FingerKiri) ? Array.Empty<byte>() : webClient.DownloadData(profile.ktp_FingerKiri);

                profile.ImgUrlPath2 = profile.ktp_FingerKiri;

                if (ktp_FingerKiriB.Length < 1)
                {
                    profile.ktp_FingerKiri = "";
                }
                else
                {
                    using var r = new StreamReader(new MemoryStream(ktp_FingerKiriB));
                    var enc = r.ReadToEnd();
                    profile.ktp_FingerKiri = enc.Decrypt(Phrase.FileEncryption);
                }
            }
            catch (Exception)
            {
                profile.ktp_FingerKiri = "";
            }
            try
            {
                var ktp_SignatureB = string.IsNullOrWhiteSpace(profile.ktp_Signature) ? Array.Empty<byte>() : webClient.DownloadData(profile.ktp_Signature);

                if (ktp_SignatureB.Length < 1)
                {
                    profile.ktp_Signature = "";
                }
                else
                {
                    profile.ktp_Signature = Convert.ToBase64String(ktp_SignatureB);
                }
            }
            catch (Exception)
            {
                profile.ktp_Signature = "";
            }

            return profile;
        }

        private static ProfileByNikOnlyFinger ConvertUrlToB64FingerEncAndISO(ProfileByNikOnlyFinger profile)
        {
            using WebClient webClient = new();

            if (profile == null) return new ProfileByNikOnlyFinger();

            try
            {
                var ktp_PhotoCamB = string.IsNullOrWhiteSpace(profile?.ktp_PhotoCam) ? Array.Empty<byte>() : webClient.DownloadData(profile?.ktp_PhotoCam);

                if (ktp_PhotoCamB.Length < 1)
                {
                    profile.ktp_PhotoCam = "";
                }
                else
                {
                    profile.ktp_PhotoCam = Convert.ToBase64String(ktp_PhotoCamB);
                }
            }
            catch (Exception)
            {
                profile.ktp_PhotoCam = "";
            }
            try
            {
                var ktp_PhotoKTPB = string.IsNullOrWhiteSpace(profile.ktp_PhotoKTP) ? Array.Empty<byte>() : webClient.DownloadData(profile.ktp_PhotoKTP);

                if (ktp_PhotoKTPB.Length < 1)
                {
                    profile.ktp_PhotoKTP = "";
                }
                else
                {
                    profile.ktp_PhotoKTP = Convert.ToBase64String(ktp_PhotoKTPB);
                }
            }
            catch (Exception)
            {
                profile.ktp_PhotoKTP = "";
            }

            #region IMG Finger
            try
            {
                var ktp_FingerKananB = string.IsNullOrWhiteSpace(profile.ktp_FingerKanan) ? Array.Empty<byte>() : webClient.DownloadData(profile.ktp_FingerKanan);

                profile.ImgUrlPath1 = profile.ktp_FingerKanan;

                if (ktp_FingerKananB.Length < 1)
                {
                    profile.ktp_FingerKanan = "";
                }
                else
                {
                    using var r = new StreamReader(new MemoryStream(ktp_FingerKananB));
                    var enc = r.ReadToEnd();
                    profile.ktp_FingerKanan = enc.Decrypt(Phrase.FileEncryption);
                }
            }
            catch (Exception)
            {
                profile.ktp_FingerKanan = "";
            }
            try
            {
                var ktp_FingerKiriB = string.IsNullOrWhiteSpace(profile.ktp_FingerKiri) ? Array.Empty<byte>() : webClient.DownloadData(profile.ktp_FingerKiri);

                profile.ImgUrlPath2 = profile.ktp_FingerKiri;

                if (ktp_FingerKiriB.Length < 1)
                {
                    profile.ktp_FingerKiri = "";
                }
                else
                {
                    using var r = new StreamReader(new MemoryStream(ktp_FingerKiriB));
                    var enc = r.ReadToEnd();
                    profile.ktp_FingerKiri = enc.Decrypt(Phrase.FileEncryption);
                }
            }
            catch (Exception)
            {
                profile.ktp_FingerKiri = "";
            }
            #endregion

            #region ISO Finger
            try
            {
                var ktp_FingerKananIsoB = string.IsNullOrWhiteSpace(profile.ktp_FingerKananISO) ? Array.Empty<byte>() : webClient.DownloadData(profile.ktp_FingerKananISO);

                profile.ImgUrlPathISO1 = profile.ktp_FingerKananISO;

                if (ktp_FingerKananIsoB.Length < 1)
                {
                    profile.ktp_FingerKananISO = "";
                }
                else
                {
                    using var r = new StreamReader(new MemoryStream(ktp_FingerKananIsoB));
                    var enc = r.ReadToEnd();
                    profile.ktp_FingerKananISO = enc.Decrypt(Phrase.FileEncryption);
                }
            }
            catch (Exception)
            {
                profile.ktp_FingerKananISO = "";
            }
            try
            {
                var ktp_FingerKiriIsoB = string.IsNullOrWhiteSpace(profile.ktp_FingerKiriISO) ? Array.Empty<byte>() : webClient.DownloadData(profile.ktp_FingerKiriISO);

                profile.ImgUrlPathISO2 = profile.ktp_FingerKiriISO;

                if (ktp_FingerKiriIsoB.Length < 1)
                {
                    profile.ktp_FingerKiriISO = "";
                }
                else
                {
                    using var r = new StreamReader(new MemoryStream(ktp_FingerKiriIsoB));
                    var enc = r.ReadToEnd();
                    profile.ktp_FingerKiriISO = enc.Decrypt(Phrase.FileEncryption);
                }
            }
            catch (Exception)
            {
                profile.ktp_FingerKiriISO = "";
            }
            #endregion

            try
            {
                var ktp_SignatureB = string.IsNullOrWhiteSpace(profile.ktp_Signature) ? Array.Empty<byte>() : webClient.DownloadData(profile.ktp_Signature);

                if (ktp_SignatureB.Length < 1)
                {
                    profile.ktp_Signature = "";
                }
                else
                {
                    profile.ktp_Signature = Convert.ToBase64String(ktp_SignatureB);
                }
            }
            catch (Exception)
            {
                profile.ktp_Signature = "";
            }

            return profile;
        }

        private static ProfileByNik ConvertUrlToB64FingerEncOnlyCompressed(ProfileByNik profile)
        {
            using WebClient webClient = new();

            try
            {
                var ktp_PhotoCamB = string.IsNullOrWhiteSpace(profile.ktp_PhotoCam) ? Array.Empty<byte>() : webClient.DownloadData(profile.ktp_PhotoCam);

                if (ktp_PhotoCamB.Length < 1)
                {
                    profile.ktp_PhotoCam = "";
                }
                else
                {
                    profile.ktp_PhotoCam = Convert.ToBase64String(ktp_PhotoCamB);
                }
            }
            catch (Exception)
            {
                profile.ktp_PhotoCam = "";
            }
            try
            {
                var ktp_PhotoKTPB = string.IsNullOrWhiteSpace(profile.ktp_PhotoKTP) ? Array.Empty<byte>() : webClient.DownloadData(profile.ktp_PhotoKTP);

                if (ktp_PhotoKTPB.Length < 1)
                {
                    profile.ktp_PhotoKTP = "";
                }
                else
                {
                    profile.ktp_PhotoKTP = Convert.ToBase64String(ktp_PhotoKTPB);
                }
            }
            catch (Exception)
            {
                profile.ktp_PhotoKTP = "";
            }
            try
            {
                var ktp_FingerKananB = string.IsNullOrWhiteSpace(profile.ktp_FingerKanan) ? Array.Empty<byte>() : webClient.DownloadData(profile.ktp_FingerKanan);

                profile.ImgUrlPath1 = profile.ktp_FingerKanan;

                if (ktp_FingerKananB.Length < 1)
                {
                    profile.ktp_FingerKanan = "";
                }
                else
                {
                    using var r = new StreamReader(new MemoryStream(ktp_FingerKananB));
                    var enc = r.ReadToEnd();
                    profile.ktp_FingerKanan = enc.Decrypt(Phrase.FileEncryption);

                    if (profile.ktp_FingerKanan != null && profile.ktp_FingerKanan != "")
                    {
                        var (status, Base64Compress) = CompressImage(profile.ktp_FingerKanan, 0);

                        if (!status)
                        {
                            profile.ktp_FingerKanan = "";
                        }
                        profile.ktp_FingerKanan = Base64Compress;
                    }
                }
            }
            catch (Exception)
            {
                profile.ktp_FingerKanan = "";
            }
            try
            {
                var ktp_FingerKiriB = string.IsNullOrWhiteSpace(profile.ktp_FingerKiri) ? Array.Empty<byte>() : webClient.DownloadData(profile.ktp_FingerKiri);

                profile.ImgUrlPath2 = profile.ktp_FingerKiri;

                if (ktp_FingerKiriB.Length < 1)
                {
                    profile.ktp_FingerKiri = "";
                }
                else
                {
                    using var r = new StreamReader(new MemoryStream(ktp_FingerKiriB));
                    var enc = r.ReadToEnd();
                    profile.ktp_FingerKiri = enc.Decrypt(Phrase.FileEncryption);

                    if (profile.ktp_FingerKiri != null && profile.ktp_FingerKiri != "")
                    {
                        var (status, Base64Compress) = CompressImage(profile.ktp_FingerKiri, 0);

                        if (!status)
                        {
                            profile.ktp_FingerKiri = "";
                        }
                        profile.ktp_FingerKiri = Base64Compress;
                    }
                }
            }
            catch (Exception)
            {
                profile.ktp_FingerKiri = "";
            }
            try
            {
                var ktp_SignatureB = string.IsNullOrWhiteSpace(profile.ktp_Signature) ? Array.Empty<byte>() : webClient.DownloadData(profile.ktp_Signature);

                if (ktp_SignatureB.Length < 1)
                {
                    profile.ktp_Signature = "";
                }
                else
                {
                    profile.ktp_Signature = Convert.ToBase64String(ktp_SignatureB);
                }
            }
            catch (Exception)
            {
                profile.ktp_Signature = "";
            }

            return profile;
        }

        public static (bool status, string msg) CompressImage(string base64Source, int quality)
        {
            string base64COmpress = "";
            try
            {
                Byte[] bitmapData = Convert.FromBase64String(FixBase64ForImage(base64Source));
                MemoryStream streamBitmap = new MemoryStream(bitmapData);

                using (Bitmap bmpl = new Bitmap((Bitmap)Image.FromStream(streamBitmap)))
                {
                    ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);
                    Encoder QualityEncoder = Encoder.Quality;
                    EncoderParameters myEncoderParameters = new EncoderParameters(1);
                    EncoderParameter myEncoderParameter = new EncoderParameter(QualityEncoder, quality);
                    myEncoderParameters.Param[0] = myEncoderParameter;

                    MemoryStream ms = new MemoryStream();
                    bmpl.Save(ms, jpgEncoder, myEncoderParameters);

                    byte[] byteImage = ms.ToArray();
                    base64COmpress = Convert.ToBase64String(byteImage);

                }
                return (true, base64COmpress);
            }
            catch (Exception Ex)
            {
                return (false, Ex.Message);
            }

        }
        public static string FixBase64ForImage(string Image)
        {
            System.Text.StringBuilder sbText = new System.Text.StringBuilder(Image, Image.Length);
            sbText.Replace("", String.Empty); sbText.Replace(" ", String.Empty);
            return sbText.ToString();
        }
        private static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }
    }
}
