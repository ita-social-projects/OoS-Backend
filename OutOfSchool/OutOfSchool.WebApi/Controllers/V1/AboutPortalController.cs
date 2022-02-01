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
    public class AboutPortalController : ControllerBase
    {
        private readonly IAboutPortalService aboutPortalService;
        private readonly IStringLocalizer<SharedResource> localizer;
        private readonly ILogger<AboutPortalController> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AboutPortalController"/> class.
        /// </summary>
        /// <param name="aboutPortalService">Service for AboutPortal model.</param>
        /// <param name="localizer">Localizer.</param>
        /// <param name="logger"><see cref="Microsoft.Extensions.Logging.ILogger{T}"/> object.</param>
        public AboutPortalController(
            IAboutPortalService aboutPortalService,
            IStringLocalizer<SharedResource> localizer,
            ILogger<AboutPortalController> logger)
        {
            this.aboutPortalService = aboutPortalService ?? throw new ArgumentNullException(nameof(aboutPortalService));
            this.localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get information about Portal from the database.
        /// </summary>
        /// <returns>Information about Portal.</returns>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AboutPortalDto))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Get()
        {
            var aboutPortal = await aboutPortalService.Get().ConfigureAwait(false);

            if (aboutPortal == null)
            {
                return NoContent();
            }

            return Ok(aboutPortal);
        }

        /// <summary>
        /// Update information about Portal.
        /// </summary>
        /// <param name="aboutPortalModel">Entity to update.</param>
        /// <returns>Updated information about Portal.</returns>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AboutPortalDto))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPut]
        public async Task<IActionResult> Update(AboutPortalDto aboutPortalModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var aboutPortal = await aboutPortalService.Update(aboutPortalModel).ConfigureAwait(false);

            if (aboutPortal == null)
            {
                return BadRequest("Cannot change information about Portal.\n Please check information is valid.");
            }

            return Ok(aboutPortal);
        }
    }
}
