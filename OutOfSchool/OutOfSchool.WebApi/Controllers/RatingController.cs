using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class RatingController : ControllerBase
    {
        private readonly IRatingService service;
        private readonly IStringLocalizer<SharedResource> localizer;

        /// <summary>
        /// Initializes a new instance of the <see cref="RatingController"/> class.
        /// </summary>
        /// <param name="service">Service for Rating model.</param>
        /// <param name="localizer">Localizer.</param>
        public RatingController(IRatingService service, IStringLocalizer<SharedResource> localizer)
        {
            this.service = service;
            this.localizer = localizer;
        }

        /// <summary>
        /// Get all ratings from the database.
        /// </summary>
        /// <returns>List of all ratings.</returns>
        [Authorize(Roles = "admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var ratings = await service.GetAll().ConfigureAwait(false);

            if (!ratings.Any())
            {
                return NoContent();
            }

            return Ok(ratings);
        }

        /// <summary>
        /// Get rating by it's id.
        /// </summary>
        /// <param name="id">Rating's id.</param>
        /// <returns>Rating.</returns>
        [Authorize(Roles = "admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("{id}")]
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
        /// Add a new rating to the database.
        /// </summary>
        /// <param name="dto">Rating entity to add.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [Authorize(Roles = "parent,admin")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPost]
        public async Task<IActionResult> Create(RatingDTO dto)
        {
            var rating = await service.Create(dto).ConfigureAwait(false);

            if (rating == null)
            {
                return BadRequest("Can't create a Rating with such parameters!\n" +
                    "Please check that entity, parent, type information are valid and don't exist in the system yet.");
            }

            return CreatedAtAction(
                nameof(GetById),
                new { id = rating.Id, },
                rating);
        }

        /// <summary>
        /// Update info about a specific rating in the database.
        /// </summary>
        /// <param name="dto">Rating to update.</param>
        /// <returns>Rating.</returns>
        [Authorize(Roles = "parent,admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPut]
        public async Task<IActionResult> Update(RatingDTO dto)
        {
            var rating = await service.Update(dto).ConfigureAwait(false);

            if (rating == null)
            {
                return BadRequest("Can't change Rating with such parameters\n" +
                    "Please check that id, entity, parent, type information are valid and exist in the system.");
            }

            return Ok(rating);
        }

        /// <summary>
        /// Delete a specific Rating entity from the database.
        /// </summary>
        /// <param name="id">Rating's id.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [Authorize(Roles = "admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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
    }
}
