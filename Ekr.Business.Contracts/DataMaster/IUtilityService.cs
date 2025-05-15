using Ekr.Core.Entities.DataMaster.Utility.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ekr.Business.Contracts.DataMaster
{
    public interface IUtilityService
    {
        bool UpdateSessionLog(UserSessionLogVM userSessionLogVM);
    }
}
