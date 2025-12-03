using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ekr.Business.Contracts.BanchlinkCifUpdate;
using Ekr.Repository.Contracts.BanchlinkCifUpdate;
using Ekr.Core.Entities;

namespace Ekr.Business.BanchlinkCifUpdate
{
    public class BanchlinkCifUpdateService : IBanchlinkCifUpdateService
    {
        private readonly IBanchlinkCifUpdateRepository _repository;
        private readonly ILogger<BanchlinkCifUpdateService> _logger;

        public BanchlinkCifUpdateService(IBanchlinkCifUpdateRepository repository, ILogger<BanchlinkCifUpdateService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<BanchlinkCifNikUpdateResult> BanchlinkCifNikUpdateAsync(BanchlinkCifNikUpdateRequest request)
        {
            try
            {
                _logger.LogInformation("Start BanchlinkCifNikUpdate for NIK {NIK}", request.NIK);

                var result = await _repository.ExecuteBanchlinkCifNikUpdateAsync(request);

                if (!result.Success)
                    _logger.LogWarning("BanchlinkCifNikUpdate failed for NIK {NIK}: {Message}", request.NIK, result.Message);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Service error on BanchlinkCifNikUpdate for NIK {NIK}", request.NIK);
                return new BanchlinkCifNikUpdateResult
                {
                    Success = false,
                    Message = "Internal service error",
                    ErrorCode = "ERR_SERVICE_EXCEPTION"
                };
            }
        }
    }
}
