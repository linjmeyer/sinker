using System.Collections.Generic;
using KubeClient;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sinker.Common;
using Sinker.Extensions;

namespace Sinker.Controllers
{
    [ApiController]
    [Route("api")]
    public class ApiController : ControllerBase
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<ApiController> _logger;
        private KubeClientOptions _kubeOptions;
        private readonly SinkerConfiguration _config;
        private readonly IEnumerable<ISecretsProvider> _providers;

        public ApiController(ILogger<ApiController> logger, IOptions<SinkerConfiguration> configOptions, ILoggerFactory loggerFactory,
            KubeClientOptions kubeOptions, IEnumerable<ISecretsProvider> providers)
        {
            _kubeOptions = kubeOptions;
            _loggerFactory = loggerFactory;
            _logger = logger;
            _config = configOptions.Value;
            _providers = providers;
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

        [HttpPost("sink-secrets")]
        public IActionResult PostSinkSecrets()
        {
            var sinkLogger = _loggerFactory.CreateLogger<SecretsSinker>();
            var sinker = new SecretsSinker(sinkLogger, _kubeOptions, _providers, _config);
            _logger.LogInformation("Scheduling immediate secrets sink");
            sinker.UpdateAllSecretsAsync().FireAndForget(sinkLogger);
            return Ok();
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
