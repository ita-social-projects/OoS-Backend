using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OutOfSchool.Services;
using OutOfSchool.Services.ViewModels;
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
            if (id < 1)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(id),
                    localizer["The id cannot be less than 1."]);
            }

            return Ok(await service.GetById(id).ConfigureAwait(false));
        }
        
        /// <summary>
        /// Get workshops by provider id.
        /// </summary>
        /// <param name="id">Provider's id.</param>
        /// <returns>Workshops.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAllByProvider(long id)
        {
            return Ok(await service.GetAllByProvider(id).ConfigureAwait(false));
        }

        /// <summary>
        /// Add new workshop to the database.
        /// </summary>
        /// <param name="dto">Entity to add.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [Authorize(Roles = "provider,admin")]
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

            var context = HttpContext.RequestServices.GetService<OutOfSchoolDbContext>();

            var userProvider = context.Providers.SingleOrDefaultAsync(u => u.UserId == User.FindFirst("sub").Value).Result;

            if (userProvider == null)
            {
                throw new Exception("Provider hasn't been created yet.");
            }

            dto.ProviderId = userProvider.Id;
            
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
        [Authorize(Roles = "parent,admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            if (id < 1)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(id),
                    localizer["The id cannot be less than 1."]);
            }

            await service.Delete(id).ConfigureAwait(false);

            return NoContent();
        }

        [HttpGet]
        public async Task<IActionResult> Search([FromQuery] SearchViewModel searchViewModel)
        {
            return Ok(await service.Search(searchViewModel).ConfigureAwait(false));
        }
    }
}