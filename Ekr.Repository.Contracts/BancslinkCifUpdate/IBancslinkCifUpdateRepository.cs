using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ekr.Core.Entities;

namespace Ekr.Repository.Contracts.BancslinkCifUpdate
{
    public interface IBancslinkCifUpdateRepository
    {
        Task<BancslinkCifNikUpdateResult> ExecuteBancslinkCifNikUpdateAsync(BancslinkCifNikUpdateRequest request);
    }
}

