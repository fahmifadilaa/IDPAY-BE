using Ekr.Business.Contracts.BancslinkCifUpdate;
using Ekr.Core.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;

namespace Ekr.Api.BancslinkCifUpdate.Controllers
{
    [ApiController]
    [Route("bancslink")]   
    public class BancslinkCifUpdateController : ControllerBase
    {
        private readonly IBancslinkCifUpdateService _service;
        private readonly ILogger<BancslinkCifUpdateController> _logger;
        private readonly string _expectedApiKey;
        private readonly string _InvalidApiKeyMessage;
        private readonly string _SpvNullMessage;
        private readonly string _level;

        public BancslinkCifUpdateController(
            IBancslinkCifUpdateService service,
            ILogger<BancslinkCifUpdateController> logger,
            IConfiguration configuration)
        {
            _service = service;
            _logger = logger;
            _expectedApiKey = configuration["ApiSettings:ApiKey"];
            _InvalidApiKeyMessage = configuration["ApiSettings:InvalidApiKeyMessage"];
            _SpvNullMessage = configuration["ApiSettings:MissingSupervisorMessage"];
            _level = configuration["ApiSettings:Level"];
        }

        [HttpPost("update-cif-nik")]  
        public async Task<IActionResult> BancslinkCifNikUpdate(
            [FromBody] BancslinkCifNikUpdateRequest request)
        {
            //if (request == null)
            //    return BadRequest(new { success = false, message = "Invalid request payload" });
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid request payload",
                    errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                });
            }


            var apiKey = Request.Headers["ApiKey"].ToString();
            if (string.IsNullOrEmpty(apiKey) || apiKey != _expectedApiKey)
                return StatusCode(401, new { success = false, message = _InvalidApiKeyMessage, errorCode = 401 });

            if (string.IsNullOrWhiteSpace(request.SpvID) || string.IsNullOrWhiteSpace(request.IsAuthorized) || request.IsAuthorized != "Y" )
                return StatusCode(400, new { success = false, message = _SpvNullMessage + _level, errorCode = 400});

            var result = await _service.BancslinkCifNikUpdateAsync(request);
            if (!result.Success)
                return StatusCode(500, result);

            return Ok(result);
        }
    }
}
