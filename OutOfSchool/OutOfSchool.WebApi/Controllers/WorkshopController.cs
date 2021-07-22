using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Controllers
{
    /// <summary>
    /// Controller with CRUD operations for Workshop entity.
    /// </summary>
    [ApiController]
    [Route("[controller]/[action]")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class WorkshopController : ControllerBase
    {
        private readonly IWorkshopServicesCombiner combinedWorkshopService;
        private readonly IProviderService providerService;
        private readonly IStringLocalizer<SharedResource> localizer;

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkshopController"/> class.
        /// </summary>
        /// <param name="combinedWorkshopService">Service for operations with Workshops.</param>
        /// <param name="providerService">Service for Provider model.</param>
        /// <param name="localizer">Localizer.</param>
        public WorkshopController(IWorkshopServicesCombiner combinedWorkshopService, IProviderService providerService, IStringLocalizer<SharedResource> localizer)
        {
            this.localizer = localizer;
            this.combinedWorkshopService = combinedWorkshopService;
            this.providerService = providerService;
        }

        /// <summary>
        /// Get all workshops from the database.
        /// </summary>
        /// <returns>List of all workshops, or no content.</returns>
        /// <response code="200">One or more entites were found.</response>
        /// <response code="204">No entity was found.</response>
        /// <response code="500">If any server error occures.</response>
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SearchResult<WorkshopCard>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await combinedWorkshopService.GetAll().ConfigureAwait(false);

            if (result.TotalAmount < 1)
            {
                return NoContent();
            }

            return Ok(result);
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
        public async Task<IActionResult> GetById(long id)
        {
            this.ValidateId(id, localizer);

            var workshop = await combinedWorkshopService.GetById(id).ConfigureAwait(false);

            if (workshop is null)
            {
                return NoContent();
            }

            return Ok(workshop);
        }

        /// <summary>
        /// Get workshops by Provider's Id.
        /// </summary>
        /// <param name="id">Provider's id.</param>
        /// <returns><see cref="IEnumerable{WorkshopDTO}"/>, or no content.</returns>
        /// <response code="200">The list of found entities by given Id.</response>
        /// <response code="204">No entity with given Id was found.</response>
        /// <response code="500">If any server error occures. For example: Id was less than one.</response>
        [AllowAnonymous]
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<WorkshopDTO>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetByProviderId(long id)
        {
            this.ValidateId(id, localizer);

            var workshops = await combinedWorkshopService.GetByProviderId(id).ConfigureAwait(false);

            if (!workshops.Any())
            {
                return NoContent();
            }

            return Ok(workshops);
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
        public async Task<IActionResult> GetByFilter([FromQuery] WorkshopFilterDto filter)
        {
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
        /// <response code="403">If the user has no rights to use this method, or sets some properties that are forbidden to change.</response>
        /// <response code="500">If any server error occures.</response>
        [Authorize(Roles = "provider,admin")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(WorkshopDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPost]
        public async Task<IActionResult> Create(WorkshopDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userHasRights = await this.IsUserProvidersOwner(dto.ProviderId).ConfigureAwait(false);
            if (!userHasRights)
            {
                return StatusCode(403, "Forbidden to create workshops for another providers.");
            }

            dto.Id = default;
            dto.Address.Id = default;
            if (dto.Teachers.Any())
            {
                foreach (var teacher in dto.Teachers)
                {
                    teacher.Id = default;
                }
            }

            var workshop = await combinedWorkshopService.Create(dto).ConfigureAwait(false);

            return CreatedAtAction(
                nameof(GetById),
                new { id = workshop.Id, },
                workshop);
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
        /// <response code="500">If any server error occures.</response>
        [Authorize(Roles = "provider,admin")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(WorkshopDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPut]
        public async Task<IActionResult> Update(WorkshopDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userHasRights = await this.IsUserProvidersOwner(dto.ProviderId).ConfigureAwait(false);
            if (!userHasRights)
            {
                return StatusCode(403, "Forbidden to update workshops for another providers.");
            }

            return Ok(await combinedWorkshopService.Update(dto).ConfigureAwait(false));
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
        [Authorize(Roles = "provider,admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            this.ValidateId(id, localizer);

            var workshop = await combinedWorkshopService.GetById(id).ConfigureAwait(false);

            if (workshop is null)
            {
                return NoContent();
            }

            var userHasRights = await this.IsUserProvidersOwner(workshop.ProviderId).ConfigureAwait(false);
            if (!userHasRights)
            {
                return StatusCode(403, "Forbidden to delete workshops of another providers.");
            }

            await combinedWorkshopService.Delete(id).ConfigureAwait(false);

            return NoContent();
        }

        private async Task<bool> IsUserProvidersOwner(long providerId)
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
    }
}