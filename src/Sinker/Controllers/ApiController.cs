using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sinker.Common;

namespace Sinker.Controllers
{
    [ApiController]
    [Route("api")]
    public class ApiController : ControllerBase
    {
        private readonly ILogger<ApiController> _logger;
        private readonly SinkerConfiguration _config;

        public ApiController(ILogger<ApiController> logger, IOptions<SinkerConfiguration> configOptions)
        {
            _logger = logger;
            _config = configOptions.Value;
        }

        [HttpGet("config")]
        public IActionResult GetConfig()
        {
            var responseData = new Dictionary<string, object>();
            responseData.Add("DotnetVersion", System.Environment.Version.ToString());
            responseData.Add("SinkerVersion", this.GetType().Assembly.GetName().Version.ToString());
            responseData.Add(nameof(SinkerConfiguration), GetNonSensitiveConfig());
            return Ok(responseData);
        }

        // Gets non-sensitive configuration details
        private object GetNonSensitiveConfig()
        {
            return new 
            {
                Labels = _config.Labels,
                RefreshInterval = _config.RefreshInterval
            };
        }
    }
}
