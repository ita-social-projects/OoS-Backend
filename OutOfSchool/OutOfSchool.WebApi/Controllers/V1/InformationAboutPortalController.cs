using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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

        /// <summary>
        /// Initializes a new instance of the <see cref="InformationAboutPortalController"/> class.
        /// </summary>
        /// <param name="informationAboutPortalService">Service for InformationAboutPortal model.</param>
        /// <param name="localizer">Localizer.</param>
        /// <param name="logger"><see cref="Microsoft.Extensions.Logging.ILogger{T}"/> object.</param>
        public InformationAboutPortalController(
            IInformationAboutPortalService informationAboutPortalService,
            IStringLocalizer<SharedResource> localizer,
            ILogger<InformationAboutPortalController> logger)
        {
            this.informationAboutPortalService = informationAboutPortalService ?? throw new ArgumentNullException(nameof(informationAboutPortalService));
            this.localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get information about Portal from the database.
        /// </summary>
        /// <returns>Information about Portal.</returns>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InformationAboutPortalDto))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

        /// <summary>
        /// Update information about Portal.
        /// </summary>
        /// <param name="informationAboutPortalModel">Entity to update.</param>
        /// <returns>Updated information about Portal.</returns>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(InformationAboutPortalDto))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
