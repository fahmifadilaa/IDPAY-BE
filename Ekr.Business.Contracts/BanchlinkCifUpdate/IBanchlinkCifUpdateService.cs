using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ekr.Core.Entities;

namespace Ekr.Business.Contracts.BanchlinkCifUpdate
{

    public interface IBanchlinkCifUpdateService
    {
        Task<BanchlinkCifNikUpdateResult> BanchlinkCifNikUpdateAsync(BanchlinkCifNikUpdateRequest request);
    }
}
