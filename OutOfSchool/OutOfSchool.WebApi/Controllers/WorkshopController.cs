using System;
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
    public class WorkshopController : ControllerBase
    {
        private readonly IWorkshopService service;
        private readonly IStringLocalizer<SharedResource> localizer;

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkshopController"/> class.
        /// </summary>
        /// <param name="service">Service for Workshop model.</param>
        /// <param name="localizer">Localizer.</param>
        public WorkshopController(IWorkshopService service, IStringLocalizer<SharedResource> localizer)
        {
            this.localizer = localizer;
            this.service = service;
        }

        /// <summary>
        /// Get all workshops from the database.
        /// </summary>
        /// <returns>List of all workshops.</returns>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var workshops = await service.GetAll().ConfigureAwait(false);

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
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(long id)
        {
            this.ValidateId(id, localizer);

            return Ok(await service.GetById(id).ConfigureAwait(false));
        }

        /// <summary>
        /// Add new workshop to the database.
        /// </summary>
        /// <param name="dto">Entity to add.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [Authorize(Roles = "provider,admin")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPost]
        public async Task<IActionResult> Create(WorkshopDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            dto.Id = default;
            dto.Address.Id = default;

            var workshop = await service.Create(dto).ConfigureAwait(false);

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
        [Authorize(AuthenticationSchemes = "Bearer")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPut]
        public async Task<IActionResult> Update(WorkshopDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok(await service.Update(dto).ConfigureAwait(false));
        }

        /// <summary>
        /// Delete a specific workshop from the database.
        /// </summary>
        /// <param name="id">Workshop's id.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>       
        [Authorize(AuthenticationSchemes = "Bearer")]
        [Authorize(Roles = "provider,admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            this.ValidateId(id, localizer);

            await service.Delete(id).ConfigureAwait(false);

            return NoContent();
        }

        /// <summary>
        /// Get count of pages of filtered workshop records.
        /// </summary>
        /// <param name="filter">Workshop filter.</param>
        /// <param name="pageSize">Count of records on one page.</param>
        /// <returns>COunt of pages.</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPagesCount(WorkshopFilter filter, int pageSize)
        {
            PageSizeValidation(pageSize);
            
            int count = await service.GetPagesCount(filter, pageSize).ConfigureAwait(false);

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
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPage(WorkshopFilter filter, int pageNumber, int pageSize)
        {
            PageSizeValidation(pageSize);
            PageNumberValidation(pageNumber);

            var workshops = await service.GetPage(filter, pageSize, pageNumber).ConfigureAwait(false);

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
    }
}