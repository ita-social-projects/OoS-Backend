using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Controllers.V1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]/[action]")]
    public class InformationAboutPortalController : ControllerBase
    {
        private readonly IInformationAboutPortalService informationAboutPortalService;
        private readonly IStringLocalizer<SharedResource> localizer;
        private readonly ILogger<InformationAboutPortalController> logger;

        public InformationAboutPortalController(
            IInformationAboutPortalService informationAboutPortalService,
            IStringLocalizer<SharedResource> localizer,
            ILogger<InformationAboutPortalController> logger)
        {
            this.informationAboutPortalService = informationAboutPortalService ?? throw new ArgumentNullException(nameof(informationAboutPortalService));
            this.localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Get()
        {
            var informationAboutPortal = await informationAboutPortalService.GetInformationAboutPortal().ConfigureAwait(false);

            if (informationAboutPortal == null)
            {
                return NoContent();
            }

            return Ok(informationAboutPortal);
        }

        [HttpPut]
        public async Task<IActionResult> Update(InformationAboutPortalDto informationAboutPortalModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var informationAboutPortal = await informationAboutPortalService.Update(informationAboutPortalModel).ConfigureAwait(false);

            if (informationAboutPortal == null)
            {
                return BadRequest("Cannot change information about Portal.\n Please check information is valid.");
            }

            return Ok(informationAboutPortal);
        }
    }
}
