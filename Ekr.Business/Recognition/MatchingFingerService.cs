using Ekr.Business.Contracts.Recognition;
using Ekr.Core.Configuration;
using Ekr.Core.Constant;
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
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Ekr.Business.Recognition
{
    public class MatchingFingerService : IMatchingFingerService
    {
        private readonly IImageRecognitionService _imageRecognitionService;
        private readonly IAuthRepository _authRepository;
        private readonly IFingerRepository _fingerRepository;
        private readonly ErrorMessageConfig _ErrorMessageConfig;
        private readonly successMessageConfig _SuccessMessageConfig;

        public MatchingFingerService(IImageRecognitionService imageRecognitionService,
            IAuthRepository authRepository,
            IProfileRepository profileRepository,
            IOptions<ErrorMessageConfig> options2,
            IOptions<successMessageConfig> options3,
            IFingerRepository fingerRepository)
        {
            _imageRecognitionService = imageRecognitionService;
            _authRepository = authRepository;
            _fingerRepository = fingerRepository;
            _ErrorMessageConfig = options2.Value;
            _SuccessMessageConfig = options3.Value;
        }

        public async Task<bool> IsMatchFinger(ProfileLoopReq req, string BaseUrl, string EndPoint, DateTime RequestTime, string AuthNpp, string AuthUnitCode)
        {
            var dataFinger = await _authRepository.GetDataFingerGeneral(req.Nik)
                .ConfigureAwait(false);

            if (!dataFinger.Any()) return false;

            int? match = 0;

            using WebClient webClient = new();

            foreach (var i in dataFinger)
            {
                if (match == 0)
                {
                    try
                    {
                        var b64 = webClient.DownloadData(i.file ?? "");
                        var b64String = "";

                        using (var r = new StreamReader(new MemoryStream(b64)))
                        {
                            var encryptedKey = r.ReadToEnd();
                            b64String = encryptedKey.Decrypt(Phrase.FileEncryption);
                        }

                        var matchingRes = await _imageRecognitionService.MatchImageBase64ToBase64(
                            new Base64ToBase64Req
                            {
                                Base64Images1 = req.Base64Img,
                                Base64Images2 = b64String
                            }, new UrlRequestRecognition
                            {
                                BaseUrl = BaseUrl,
                                EndPoint = EndPoint
                            })
                            .ConfigureAwait(false);

                        

                        if (matchingRes?.Data?.IsFoundOrSuccess == true) match = 1;
                    }
                    catch (Exception ex)
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
                            ErrorMessage = ex.Message.ToString(),
                            CreatedByNpp = AuthNpp,
                            CreatedByUnitCode = AuthUnitCode
                        };

                        _ = _authRepository.InsertLogClientApps(_logs);
                    }
                }
            }

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
                ErrorMessage = "",
                CreatedByNpp = AuthNpp,
                CreatedByUnitCode = AuthUnitCode
            };

            _ = _authRepository.InsertLogClientApps(_log);

            return match == 1;
        }

        public async Task<bool> IsMatchFingerEmp(ProfileLoopReq req, string BaseUrl, string EndPoint, DateTime RequestTime, string AuthNpp, string AuthUnitCode)
        {
            var dataFinger = await _authRepository.GetDataFingerGeneralEmp(req.Nik)
                .ConfigureAwait(false);

            if (!dataFinger.Any()) return false;

            int? match = 0;

            using WebClient webClient = new();

            foreach (var i in dataFinger)
            {
                if (match == 0)
                {
                    try
                    {
                        var b64 = webClient.DownloadData(i.file ?? "");
                        var b64String = "";

                        using (var r = new StreamReader(new MemoryStream(b64)))
                        {
                            var encryptedKey = r.ReadToEnd();
                            b64String = encryptedKey.Decrypt(Phrase.FileEncryption);
                        }

                        var matchingRes = await _imageRecognitionService.MatchImageBase64ToBase64(
                            new Base64ToBase64Req
                            {
                                Base64Images1 = req.Base64Img,
                                Base64Images2 = b64String
                            }, new UrlRequestRecognition
                            {
                                BaseUrl = BaseUrl,
                                EndPoint = EndPoint
                            })
                            .ConfigureAwait(false);

                        if (matchingRes?.Data?.IsFoundOrSuccess == true) match = 1;
                    }
                    catch (Exception ex)
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
                            ErrorMessage = ex.Message.ToString(),
                            CreatedByNpp = AuthNpp,
                            CreatedByUnitCode = AuthUnitCode
                        };

                        _ = _authRepository.InsertLogClientApps(_logs);
                    }
                }
            }

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
                ErrorMessage = "",
                CreatedByNpp = AuthNpp,
                CreatedByUnitCode = AuthUnitCode
            };

            _ = _authRepository.InsertLogClientApps(_log);

            return match == 1;
        }

        public async Task<(string msg, string status)> MatchFinger(ProfileLoopReq req, string BaseUrl, string EndPoint, DateTime RequestTime, string AuthNpp, string AuthUnitCode)
        {
            var dataFinger = await _authRepository.GetDataFingerGeneral(req.Nik)
                .ConfigureAwait(false);

            if (!dataFinger.Any()) return new(_ErrorMessageConfig.MatchingFingerNotFound, "error");

            int? match = 0;

            using WebClient webClient = new();

            foreach (var i in dataFinger)
            {
                if (match == 0)
                {
                    try
                    {
                        var b64 = webClient.DownloadData(i.file ?? "");
                        var b64String = "";

                        using (var r = new StreamReader(new MemoryStream(b64)))
                        {
                            var encryptedKey = r.ReadToEnd();
                            b64String = encryptedKey.Decrypt(Phrase.FileEncryption);
                        }

                        var matchingRes = await _imageRecognitionService.MatchImageBase64ToBase64(
                            new Base64ToBase64Req
                            {
                                Base64Images1 = req.Base64Img,
                                Base64Images2 = b64String
                            }, new UrlRequestRecognition
                            {
                                BaseUrl = BaseUrl,
                                EndPoint = EndPoint
                            })
                            .ConfigureAwait(false);

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
                            ErrorMessage = "",
                            CreatedByNpp = AuthNpp,
                            CreatedByUnitCode = AuthUnitCode
                        };

                        _ = _authRepository.InsertLogClientApps(_log);

                        if (matchingRes?.Data?.IsFoundOrSuccess == true) match = 1;
                    }
                    catch (Exception ex)
                    {
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
                            ErrorMessage = ex.Message.ToString(),
                            CreatedByNpp = AuthNpp,
                            CreatedByUnitCode = AuthUnitCode
                        };

                        _ = _authRepository.InsertLogClientApps(_log);
                    }
                }
            }

            return match == 1 ? (_SuccessMessageConfig.MatchingFingerSuccess, "sukses") : (_ErrorMessageConfig.MatchingFingerFailed, "error");
        }

        public async Task<(string msg, string status)> MatchFingerEmp(ProfileLoopNppReq req, string nik, string BaseUrl, string EndPoint, DateTime RequestTime, string AuthNpp, string AuthUnitCode)
        {
            var dataFinger = await _authRepository.GetDataFingerGeneralEmp(nik)
                .ConfigureAwait(false);

            if (!dataFinger.Any()) return new(_ErrorMessageConfig.MatchingFingerNotFound, "error");

            int? match = 0;

            using WebClient webClient = new();

            foreach (var i in dataFinger)
            {
                if (match == 0)
                {
                    try
                    {
                        var b64 = webClient.DownloadData(i.file ?? "");
                        var b64String = "";

                        using (var r = new StreamReader(new MemoryStream(b64)))
                        {
                            var encryptedKey = r.ReadToEnd();
                            b64String = encryptedKey.Decrypt(Phrase.FileEncryption);
                        }

                        var matchingRes = await _imageRecognitionService.MatchImageBase64ToBase64(
                            new Base64ToBase64Req
                            {
                                Base64Images1 = req.Base64Img,
                                Base64Images2 = b64String
                            }, new UrlRequestRecognition
                            {
                                BaseUrl = BaseUrl,
                                EndPoint = EndPoint
                            })
                            .ConfigureAwait(false);

                        var _log = new Tbl_LogClientApps
                        {
                            Param = "nik: " + nik + ",base64Img: " + req.Base64Img,
                            LvTeller = req.LvTeller,
                            Branch = req.Branch,
                            SubBranch = req.SubBranch,
                            ClientApps = req.ClientApps,
                            RequestTime = RequestTime,
                            ResponseTime = DateTime.Now,
                            CreatedTime = DateTime.Now,
                            EndPoint = req.EndPoint,
                            ErrorMessage = "",
                            CreatedByNpp = AuthNpp,
                            CreatedByUnitCode = AuthUnitCode
                        };

                        _ = _authRepository.InsertLogClientApps(_log);

                        if (matchingRes?.Data?.IsFoundOrSuccess == true) match = 1;
                    }
                    catch(Exception ex)
                    {
                        var _log = new Tbl_LogClientApps
                        {
                            Param = "nik: " + nik + ",base64Img: " + req.Base64Img,
                            LvTeller = req.LvTeller,
                            Branch = req.Branch,
                            SubBranch = req.SubBranch,
                            ClientApps = req.ClientApps,
                            RequestTime = RequestTime,
                            ResponseTime = DateTime.Now,
                            CreatedTime = DateTime.Now,
                            EndPoint = req.EndPoint,
                            ErrorMessage = ex.Message.ToString(),
                            CreatedByNpp = AuthNpp,
                            CreatedByUnitCode = AuthUnitCode
                        };

                        _ = _authRepository.InsertLogClientApps(_log);
                    }
                }
            }

            return match == 1 ? (_SuccessMessageConfig.MatchingFingerSuccess, "sukses") : (_ErrorMessageConfig.MatchingFingerFailed, "error");
        }

        public async Task<(string msg, string status)> MatchFingerType(string base64Img, string nik, string fingerType, string BaseUrl, string Endpoint)
        {
            var finger = await _fingerRepository.GetFingerByType(nik, fingerType)
                .ConfigureAwait(false);

            if (string.IsNullOrEmpty(finger?.Url)) return (_ErrorMessageConfig.NIKNotFound, "error");

            string b64 = "";

            try
            {
                using WebClient webClient = new();

                byte[] data = webClient.DownloadData(finger.Url);

                using var r = new StreamReader(new MemoryStream(data));
                var encryptedText = r.ReadToEnd();
                b64 = encryptedText.Decrypt(Phrase.FileEncryption);
            }
            catch
            {
            }

            var matchRes = await _imageRecognitionService.MatchImageBase64ToBase64(new Base64ToBase64Req
            {
                Base64Images1 = base64Img,
                Base64Images2 = b64
            }, new UrlRequestRecognition
            {
                BaseUrl = BaseUrl,
                EndPoint = Endpoint
            })
                .ConfigureAwait(false);

            if (matchRes?.Data?.IsFoundOrSuccess == false || matchRes?.Status.Equals("error") == true)
            {
                return (_ErrorMessageConfig.MatchingFingerFailed, "error");
            }
            else
            {
                return (_SuccessMessageConfig.MatchingFingerSuccess, "sukses");
            }
        }

        public async Task<bool> IsMatchFingerType(string base64Img, string nik, string fingerType, string BaseUrl, string Endpoint)
        {
            var finger = await _fingerRepository.GetFingerByType(nik, fingerType)
                .ConfigureAwait(false);

            if (string.IsNullOrEmpty(finger?.Url)) return false;

            string b64 = "";

            try
            {
                using WebClient webClient = new();

                byte[] data = webClient.DownloadData(finger.Url);

                using var r = new StreamReader(new MemoryStream(data));
                var encryptedText = r.ReadToEnd();
                b64 = encryptedText.Decrypt(Phrase.FileEncryption);
            }
            catch
            {
            }

            var matchRes = await _imageRecognitionService.MatchImageBase64ToBase64(new Base64ToBase64Req
            {
                Base64Images1 = base64Img,
                Base64Images2 = b64
            }, new UrlRequestRecognition
            {
                BaseUrl = BaseUrl,
                EndPoint = Endpoint
            })
                .ConfigureAwait(false);

            if (matchRes?.Data?.IsFoundOrSuccess == false || matchRes?.Status.Equals("error") == true)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public async Task<(string msg, string status)> MatchFingerTypeEmp(string base64Img, string nik, string fingerType, string BaseUrl, string Endpoint)
        {
            var finger = await _fingerRepository.GetFingerByTypeEmp(nik, fingerType)
                .ConfigureAwait(false);

            if (string.IsNullOrEmpty(finger?.Url)) return (_ErrorMessageConfig.NIKNotFound, "error");

            string b64 = "";

            try
            {
                using WebClient webClient = new();

                byte[] data = webClient.DownloadData(finger.Url);

                using var r = new StreamReader(new MemoryStream(data));
                var encryptedText = r.ReadToEnd();
                b64 = encryptedText.Decrypt(Phrase.FileEncryption);
            }
            catch
            {
            }

            var matchRes = await _imageRecognitionService.MatchImageBase64ToBase64(new Base64ToBase64Req
            {
                Base64Images1 = base64Img,
                Base64Images2 = b64
            }, new UrlRequestRecognition
            {
                BaseUrl = BaseUrl,
                EndPoint = Endpoint
            })
                .ConfigureAwait(false);

            if (matchRes?.Data?.IsFoundOrSuccess == false || matchRes?.Status.Equals("error") == true)
            {
                return (_ErrorMessageConfig.MatchingFingerFailed, "error");
            }
            else
            {
                return (_SuccessMessageConfig.MatchingFingerSuccess, "sukses");
            }
        }

        public async Task<bool> MatchFingerTypeEmpBool(string base64Img, string nik, string fingerType, string BaseUrl, string Endpoint)
        {
            var finger = await _fingerRepository.GetFingerByTypeEmp(nik, fingerType)
                .ConfigureAwait(false);

            if (string.IsNullOrEmpty(finger?.Url)) return false;

            string b64 = "";

            try
            {
                using WebClient webClient = new();

                byte[] data = webClient.DownloadData(finger.Url);

                using var r = new StreamReader(new MemoryStream(data));
                var encryptedText = r.ReadToEnd();
                b64 = encryptedText.Decrypt(Phrase.FileEncryption);
            }
            catch
            {
            }

            var matchRes = await _imageRecognitionService.MatchImageBase64ToBase64(new Base64ToBase64Req
            {
                Base64Images1 = base64Img,
                Base64Images2 = b64
            }, new UrlRequestRecognition
            {
                BaseUrl = BaseUrl,
                EndPoint = Endpoint
            })
                .ConfigureAwait(false);

            if (matchRes?.Data?.IsFoundOrSuccess == false || matchRes?.Status.Equals("error") == true)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
