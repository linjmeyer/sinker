using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Geedunk.Controllers
{
    [ApiController]
    [Route("/")]
    public class HomeController : ControllerBase
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var responseData = new Dictionary<string,string>();
            responseData.Add("DOTNET_VERSION", System.Environment.Version.ToString());
            responseData.Add("APP_VERSION", this.GetType().Assembly.GetName().Version.ToString());
            return Ok(responseData);
        }
    }
}
