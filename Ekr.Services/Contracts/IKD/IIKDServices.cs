using Ekr.Core.Entities.Account;
using Ekr.Core.Entities.ThirdParty;
using Ekr.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ekr.Core.Entities.Enrollment;

namespace Ekr.Services.Contracts.IKD
{
    public interface IIKDServices
    {
        Task<ScanResponse> ScanQRIKD(ScanQRIKDReq req, UrlRequestRecognitionFR url);
    }
}
