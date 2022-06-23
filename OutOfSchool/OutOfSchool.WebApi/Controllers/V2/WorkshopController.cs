using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OutOfSchool.Common.PermissionsModule;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Models.Images;
using OutOfSchool.WebApi.Common;
using OutOfSchool.WebApi.Config;
using OutOfSchool.WebApi.Config.Images;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.Workshop;
using OutOfSchool.WebApi.Services;
using OutOfSchool.WebApi.Services.Images;
using OutOfSchool.WebApi.Util;
using OutOfSchool.WebApi.Util.ControllersResultsHelpers;

namespace OutOfSchool.WebApi.Controllers.V2
{
    /// <summary>
    /// Controller with CRUD operations for Workshop entity.
    /// </summary>
    [ApiController]
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/[controller]/[action]")]
    public class WorkshopController : ControllerBase
    {
        private readonly IWorkshopServicesCombinerV2 combinedWorkshopService;
        private readonly IProviderService providerService;
        private readonly IStringLocalizer<SharedResource> localizer;
        private readonly AppDefaultsConfig options;

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkshopController"/> class.
        /// </summary>
        /// <param name="combinedWorkshopService">Service for operations with Workshops.</param>
        /// <param name="providerService">Service for Provider model.</param>
        /// <param name="localizer">Localizer.</param>
        /// <param name="options">Application default values.</param>
        public WorkshopController(
            IWorkshopServicesCombinerV2 combinedWorkshopService,
            IProviderService providerService,
            IStringLocalizer<SharedResource> localizer,
            IOptions<AppDefaultsConfig> options)
        {
            this.localizer = localizer;
            this.combinedWorkshopService = combinedWorkshopService;
            this.providerService = providerService;
            this.options = options.Value;
        }

        /// <summary>
        /// Get workshop by it's id.
        /// </summary>
        /// <param name="id">Workshop's id.</param>
        /// <returns><see cref="WorkshopDTO"/>, or no content.</returns>
        /// <response code="200">The entity was found by given Id.</response>
        /// <response code="204">No entity with given Id was found.</response>
        /// <response code="500">If any server error occures. For example: Id was less than one.</response>
        [AllowAnonymous]
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(WorkshopDTO))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var workshop = await combinedWorkshopService.GetById(id).ConfigureAwait(false);

            if (workshop is null)
            {
                return NotFound();
            }

            return Ok(workshop);
        }

        /// <summary>
        /// Get workshop cards by Provider's Id.
        /// </summary>
        /// <param name="id">Provider's id.</param>
        /// <returns><see cref="IEnumerable{WorkshopCard}"/>, or no content.</returns>
        /// <response code="200">The list of found entities by given Id.</response>
        /// <response code="204">No entity with given Id was found.</response>
        /// <response code="500">If any server error occures. For example: Id was less than one.</response>
        [AllowAnonymous]
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<WorkshopCard>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetByProviderId(Guid id)
        {
            var workshopCards = await combinedWorkshopService.GetByProviderId(id).ConfigureAwait(false);

            if (!workshopCards.Any())
            {
                return NoContent();
            }

            return Ok(workshopCards);
        }

        /// <summary>
        /// Get workshops that matches filter's parameters.
        /// </summary>
        /// <param name="filter">Entity that represents searching parameters.</param>
        /// <returns><see cref="SearchResult{WorkshopCard}"/>, or no content.</returns>
        /// <response code="200">The list of found entities by given filter.</response>
        /// <response code="204">No entity with given filter was found.</response>
        /// <response code="500">If any server error occures. For example: Id was less than one.</response>
        [AllowAnonymous]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SearchResult<WorkshopCard>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetByFilter([FromQuery] WorkshopFilter filter)
        {
            if (string.IsNullOrWhiteSpace(filter.City))
            {
                filter.City = options.City;
            }

            var result = await combinedWorkshopService.GetByFilter(filter).ConfigureAwait(false);

            if (result.TotalAmount < 1)
            {
                return NoContent();
            }

            return Ok(result);
        }

        /// <summary>
        /// Add new workshop to the database.
        /// </summary>
        /// <param name="dto">Entity to add.</param>
        /// <returns>Created <see cref="WorkshopDTO"/>.</returns>
        /// <response code="201">Entity was created and returned with Id.</response>
        /// <response code="400">If the model is invalid, some properties are not set etc.</response>
        /// <response code="401">If the user is not authorized.</response>
        /// <response code="403">If the user has no rights to use this method, or sets some properties that are forbidden.</response>
        /// <response code="413">If the request break the limits, set in configs.</response>
        /// <response code="500">If any server error occures.</response>
        [HasPermission(Permissions.WorkshopAddNew)]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(WorkshopCreationResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status413PayloadTooLarge)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Create([FromForm] WorkshopDTO dto)
        {
            var userHasRights = await IsUserProvidersOwner(dto.ProviderId).ConfigureAwait(false);
            if (!userHasRights)
            {
                return StatusCode(403, "Forbidden to create workshops for another providers.");
            }

            dto.Id = default;
            dto.Address.Id = default;

            var creationResult = await combinedWorkshopService.Create(dto).ConfigureAwait(false);

            return CreatedAtAction(
                nameof(GetById),
                new { id = creationResult.Workshop.Id, },
                new WorkshopCreationResponse
                {
                    Workshop = creationResult.Workshop,
                    UploadingCoverImageResult = creationResult.UploadingCoverImageResult?.CreateSingleUploadingResult(),
                    UploadingImagesResults = creationResult.UploadingImagesResults?.CreateMultipleUploadingResult(),
                });
        }

        /// <summary>
        /// Update info about workshop entity.
        /// </summary>
        /// <param name="dto">Workshop to update.</param>
        /// <returns>Updated <see cref="WorkshopDTO"/>.</returns>
        /// <response code="200">Entity was updated and returned.</response>
        /// <response code="400">If the model is invalid, some properties are not set etc.</response>
        /// <response code="401">If the user is not authorized.</response>
        /// <response code="403">If the user has no rights to use this method, or sets some properties that are forbidden to change.</response>
        /// <response code="413">If the request break the limits, set in configs.</response>
        /// <response code="500">If any server error occures.</response>
        [HasPermission(Permissions.WorkshopEdit)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(WorkshopUpdateResponse))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status413PayloadTooLarge)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPut]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Update([FromForm] WorkshopDTO dto)
        {
            var userHasRights = await IsUserProvidersOwner(dto.ProviderId).ConfigureAwait(false);
            if (!userHasRights)
            {
                return StatusCode(StatusCodes.Status403Forbidden, "Forbidden to update workshops for another providers.");
            }

            try
            {
                var updatingResult = await combinedWorkshopService.Update(dto).ConfigureAwait(false);

                return Ok(CreateUpdateResponse(updatingResult));
            }
            catch (WorkshopUpdateStatusException e)
            {
                return BadRequest(e.Message);
            }
        }

        /// <summary>
        /// Delete a specific workshop from the database.
        /// </summary>
        /// <param name="id">Workshop's id.</param>
        /// <returns>StatusCode representing the task completion.</returns>
        /// <response code="204">If the entity was successfully deleted, or if the entity was not found by given Id.</response>
        /// <response code="401">If the user is not authorized.</response>
        /// <response code="403">If the user has no rights to use this method, or deletes not own workshop.</response>
        /// <response code="500">If any server error occures.</response>
        [HasPermission(Permissions.WorkshopRemove)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var workshop = await combinedWorkshopService.GetById(id).ConfigureAwait(false);

            if (workshop is null)
            {
                return NoContent();
            }

            var userHasRights = await this.IsUserProvidersOwner(workshop.ProviderId).ConfigureAwait(false);
            if (!userHasRights)
            {
                return new ForbidResult("Forbidden to delete workshops of another providers.");
            }

            await combinedWorkshopService.Delete(id).ConfigureAwait(false);
            return NoContent();
        }

        private async Task<bool> IsUserProvidersOwner(Guid providerId)
        {
            // Provider can create/update/delete a workshop only with it's own ProviderId.
            // Admin can create a work without checks.
            if (User.IsInRole("provider"))
            {
                var userId = User.FindFirst("sub")?.Value;
                var provider = await providerService.GetByUserId(userId).ConfigureAwait(false);

                if (providerId != provider.Id)
                {
                    return false;
                }
            }

            return true;
        }

        private WorkshopUpdateResponse CreateUpdateResponse(WorkshopUpdateResultDto updatingResult)
        {
            return new WorkshopUpdateResponse
            {
                Workshop = updatingResult.Workshop,
                UploadingCoverImageResult = updatingResult.UploadingCoverImageResult?.CreateSingleUploadingResult(),
                UploadingImagesResults = updatingResult.UploadingImagesResults?.CreateMultipleUploadingResult(),
            };
        }
    }
}
