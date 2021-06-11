using System;
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
        private readonly IWorkshopService workshopService;
        private readonly IProviderService providerService;
        private readonly IStringLocalizer<SharedResource> localizer;

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkshopController"/> class.
        /// </summary>
        /// <param name="workshopService">Service for Workshop model.</param>
        /// <param name="providerService">Service for Provider model.</param>
        /// <param name="localizer">Localizer.</param>
        public WorkshopController(IWorkshopService workshopService, IProviderService providerService, IStringLocalizer<SharedResource> localizer)
        {
            this.localizer = localizer;
            this.workshopService = workshopService;
            this.providerService = providerService;
        }

        /// <summary>
        /// Get all workshops from the database.
        /// </summary>
        /// <returns>List of all workshops.</returns>
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<WorkshopDTO>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var workshops = await workshopService.GetAll().ConfigureAwait(false);

            if (!workshops.Any())
            {
                return NoContent();
            }

            return Ok(workshops);
        }

        /// <summary>
        /// Get workshop by it's id.
        /// </summary>
        /// <param name="id">Workshop's id.</param>
        /// <returns>Workshop.</returns>
        [AllowAnonymous]
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(WorkshopDTO))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> GetById(long id)
        {
            this.ValidateId(id, localizer);

            var workshop = await workshopService.GetById(id).ConfigureAwait(false);

            if (workshop is null)
            {
                return NoContent();
            }

            return Ok(workshop);
        }

        /// <summary>
        /// Add new workshop to the database.
        /// </summary>
        /// <param name="dto">Entity to add.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [Authorize(Roles = "provider,admin")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(WorkshopDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
                return Unauthorized("Forbidden to create workshops for another providers.");
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

            var workshop = await workshopService.Create(dto).ConfigureAwait(false);

            return CreatedAtAction(
                nameof(GetById),
                new { id = workshop.Id, },
                workshop);
        }

        /// <summary>
        /// Update info about workshop entity.
        /// </summary>
        /// <param name="dto">Workshop to update.</param>
        /// <returns>Workshop.</returns>
        [Authorize(Roles = "provider,admin")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(WorkshopDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
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
                return Unauthorized("Forbidden to update workshops for another providers.");
            }

            return Ok(await workshopService.Update(dto).ConfigureAwait(false));
        }

        /// <summary>
        /// Delete a specific workshop from the database.
        /// </summary>
        /// <param name="id">Workshop's id.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [Authorize(Roles = "provider,admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            this.ValidateId(id, localizer);

            var workshop = await workshopService.GetById(id).ConfigureAwait(false);

            if (workshop is null)
            {
                return BadRequest($"There is no workshop with id:{id}");
            }

            var userHasRights = await this.IsUserProvidersOwner(workshop.ProviderId).ConfigureAwait(false);
            if (!userHasRights)
            {
                return Unauthorized("Forbidden to delete workshops of another providers.");
            }

            await workshopService.Delete(id).ConfigureAwait(false);

            return NoContent();
        }

        /// <summary>
        /// Get count of pages of filtered workshop records.
        /// </summary>
        /// <param name="filter">Workshop filter.</param>
        /// <param name="pageSize">Count of records on one page.</param>
        /// <returns>Count of pages.</returns>
        [AllowAnonymous]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(int))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPagesCount(WorkshopFilter filter, int pageSize)
        {
            PageSizeValidation(pageSize);
            
            int count = await workshopService.GetPagesCount(filter, pageSize).ConfigureAwait(false);

            if (count == 0)
            {
                return NoContent();
            }

            return Ok(count);
        }

        /// <summary>
        /// Get page of filtered workshop records.
        /// </summary>
        /// <param name="filter">Workshop filter.</param>
        /// <param name="pageNumber">Number of page.</param>
        /// <param name="pageSize">Count of records on one page.</param>
        /// <returns>The list of workshops for this page.</returns>
        [AllowAnonymous]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<WorkshopDTO>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPage(WorkshopFilter filter, int pageNumber, int pageSize)
        {
            PageSizeValidation(pageSize);
            PageNumberValidation(pageNumber);

            var workshops = await workshopService.GetPage(filter, pageSize, pageNumber).ConfigureAwait(false);

            if (!workshops.Any())
            {
                return NoContent();
            }

            return Ok(workshops);
        }

        private void PageSizeValidation(int pageSize)
        {
            if (pageSize < 1)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(pageSize),
                    localizer["The pageSize cannot be less than 1."]);
            }
        }

        private void PageNumberValidation(int pageNumber)
        {
            if (pageNumber < 1)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(pageNumber),
                    localizer["The pageSize cannot be less than 1."]);
            }
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