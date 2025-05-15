using Ekr.Core.Entities.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ekr.Repository.Contracts.Logging
{
    public interface INIKInquiryLogRepository
    {
        long CreateNIKInquiryLog(Tbl_LogNIKInquiry log);
    }
}
