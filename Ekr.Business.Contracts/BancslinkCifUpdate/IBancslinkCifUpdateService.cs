using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ekr.Core.Entities;

namespace Ekr.Business.Contracts.BancslinkCifUpdate
{

    public interface IBancslinkCifUpdateService
    {
        Task<BancslinkCifNikUpdateResult> BancslinkCifNikUpdateAsync(BancslinkCifNikUpdateRequest request);
    }
}
