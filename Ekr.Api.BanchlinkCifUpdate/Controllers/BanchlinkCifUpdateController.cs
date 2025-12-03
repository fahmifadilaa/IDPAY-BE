using Ekr.Business.Contracts.BanchlinkCifUpdate;
using Ekr.Core.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;

namespace Ekr.Api.BanchlinkCifUpdate.Controllers
{
    [ApiController]
    [Route("banchlink")]   
    public class BanchlinkCifUpdateController : ControllerBase
    {
        private readonly IBanchlinkCifUpdateService _service;
        private readonly ILogger<BanchlinkCifUpdateController> _logger;
        private readonly string _expectedApiKey;
        private readonly string _InvalidApiKeyMessage;
        private readonly string _SpvNullMessage;
        private readonly string _level;

        public BanchlinkCifUpdateController(
            IBanchlinkCifUpdateService service,
            ILogger<BanchlinkCifUpdateController> logger,
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
        public async Task<IActionResult> BanchlinkCifNikUpdate(
            [FromBody] BanchlinkCifNikUpdateRequest request)
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

            var result = await _service.BanchlinkCifNikUpdateAsync(request);
            if (!result.Success)
                return StatusCode(500, result);

            return Ok(result);
        }
    }
}
