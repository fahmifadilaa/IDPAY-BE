using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Ekr.Business.Contracts.BancslinkCifUpdate;
using Ekr.Repository.Contracts.BancslinkCifUpdate;
using Ekr.Core.Entities;

namespace Ekr.Business.BancslinkCifUpdate
{
    public class BancslinkCifUpdateService : IBancslinkCifUpdateService
    {
        private readonly IBancslinkCifUpdateRepository _repository;
        private readonly ILogger<BancslinkCifUpdateService> _logger;

        public BancslinkCifUpdateService(IBancslinkCifUpdateRepository repository, ILogger<BancslinkCifUpdateService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<BancslinkCifNikUpdateResult> BancslinkCifNikUpdateAsync(BancslinkCifNikUpdateRequest request)
        {
            try
            {
                _logger.LogInformation("Start BancslinkCifNikUpdate for NIK {NIK}", request.NIK);

                var result = await _repository.ExecuteBancslinkCifNikUpdateAsync(request);

                if (!result.Success)
                    _logger.LogWarning("BancslinkCifNikUpdate failed for NIK {NIK}: {Message}", request.NIK, result.Message);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Service error on BancslinkCifNikUpdate for NIK {NIK}", request.NIK);
                return new BancslinkCifNikUpdateResult
                {
                    Success = false,
                    Message = "Internal service error",
                    ErrorCode = "ERR_SERVICE_EXCEPTION"
                };
            }
        }
    }
}
