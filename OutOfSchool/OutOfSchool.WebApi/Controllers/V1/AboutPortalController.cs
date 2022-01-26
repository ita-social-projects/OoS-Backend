using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly IAboutPortalService informationAboutPortalService;
        private readonly IStringLocalizer<SharedResource> localizer;
        private readonly ILogger<AboutPortalController> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AboutPortalController"/> class.
        /// </summary>
        /// <param name="informationAboutPortalService">Service for InformationAboutPortal model.</param>
        /// <param name="localizer">Localizer.</param>
        /// <param name="logger"><see cref="Microsoft.Extensions.Logging.ILogger{T}"/> object.</param>
        public AboutPortalController(
            IAboutPortalService informationAboutPortalService,
            IStringLocalizer<SharedResource> localizer,
            ILogger<AboutPortalController> logger)
        {
            this.informationAboutPortalService = informationAboutPortalService ?? throw new ArgumentNullException(nameof(informationAboutPortalService));
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
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AboutPortalDto))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPut]
        public async Task<IActionResult> Update(AboutPortalDto informationAboutPortalModel)
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

        /// <summary>
        /// Get item in information about Portal by it's id.
        /// </summary>
        /// <param name="id">InformationAboutPortalItem's id.</param>
        /// <returns>InformationAboutPortalItem.</returns>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AboutPortalItemDto))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("{id}")]
        [Obsolete]
        public async Task<IActionResult> GetItemById(Guid id)
        {
            return Ok(await informationAboutPortalService.GetItemById(id).ConfigureAwait(false));
        }

        /// <summary>
        /// Add new item into information about Portal.
        /// </summary>
        /// <param name="informationAboutPortalItemModel">Entity to add.</param>
        /// <returns>Created item in information about Portal.</returns>
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPost]
        [Obsolete]
        public async Task<IActionResult> CreateItem(AboutPortalItemDto informationAboutPortalItemModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var informationAboutPortalItem = await informationAboutPortalService.CreateItem(informationAboutPortalItemModel).ConfigureAwait(false);

            if (informationAboutPortalItem == null)
            {
                return BadRequest("Cannot create new item in information about Portal.\n Please check if information is valid.");
            }

            return CreatedAtAction(
                nameof(GetItemById),
                new { id = informationAboutPortalItem.Id, },
                informationAboutPortalItem);
        }

        /// <summary>
        /// Update item in information about Portal.
        /// </summary>
        /// <param name="informationAboutPortalItemModel">Entity to update.</param>
        /// <returns>Updated information about Portal.</returns>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AboutPortalItemDto))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPut]
        [Obsolete]
        public async Task<IActionResult> UpdateItem(AboutPortalItemDto informationAboutPortalItemModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var informationAboutPortalItem = await informationAboutPortalService.UpdateItem(informationAboutPortalItemModel).ConfigureAwait(false);

            if (informationAboutPortalItem == null)
            {
                return BadRequest("Cannot change information about Portal.\n Please check information is valid.");
            }

            return Ok(informationAboutPortalItem);
        }

        /// <summary>
        /// Delete a specific Item in InformationAboutPortal from the database.
        /// </summary>
        /// <param name="id">Items's id.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpDelete("{id}")]
        [Obsolete]
        public async Task<IActionResult> DeleteItem(Guid id)
        {
            await informationAboutPortalService.DeleteItem(id).ConfigureAwait(false);

            return NoContent();
        }

        /// <summary>
        /// Get all items in InformationAboutPortal from the database.
        /// </summary>
        /// <returns>List of items.</returns>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<AboutPortalItemDto>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet]
        [Obsolete]
        public async Task<IActionResult> GetAllItems()
        {
            var items = await informationAboutPortalService.GetAllItems().ConfigureAwait(false);

            if (!items.Any())
            {
                return NoContent();
            }

            return Ok(items);
        }
    }
}
