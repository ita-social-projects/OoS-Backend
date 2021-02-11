using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace OutOfSchool.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    [Authorize]
    public class OrganizationController : ControllerBase
    {
        private readonly ILogger<OrganizationController> _logger;
        private readonly IStringLocalizer<SharedResource> _sharedLocalizer;

        public OrganizationController(ILogger<OrganizationController> logger, IStringLocalizer<SharedResource> sharedLocalizer)
        {
            _sharedLocalizer = sharedLocalizer;
            _logger = logger;
        }

        public IActionResult TestOk()
        {
            return this.Ok(_sharedLocalizer["Hello"]);
        }
    }
}
