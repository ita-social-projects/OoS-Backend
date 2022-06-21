using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using OutOfSchool.Common.PermissionsModule;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.Achievement;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Controllers.V1
{
    /// <summary>
    /// Controller with CRUD operations for Achievement entity.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]/[action]")]
    public class AchievementController : ControllerBase
    {
        private readonly IAchievementService achievementService;
        private readonly IStringLocalizer<SharedResource> localizer;

        /// <summary>
        /// Initializes a new instance of the <see cref="AchievementController"/> class.
        /// </summary>
        /// <param name="service">Service for Achievement entity.</param>
        /// <param name="localizer">Localizer.</param>
        public AchievementController(IAchievementService service, IStringLocalizer<SharedResource> localizer)
        {
            this.localizer = localizer;
            this.achievementService = service;
        }

        /// <summary>
        /// To recieve the Achievement with the defined id.
        /// </summary>
        /// <param name="id">Key of the Achievement in the table.</param>
        /// <returns><see cref="AchievementDto"/>.</returns>
        /// <response code="200">The entity was found by given Id.</response>
        /// <response code="500">If any server error occures. For example: Id was wrong.</response>
        [AllowAnonymous]
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AchievementDto))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(Guid id)
        {
            return Ok(await achievementService.GetById(id).ConfigureAwait(false));
        }

        /// <summary>
        /// To recieve the Achievement list by Workshop id.
        /// </summary>
        /// <param name="workshopId">Key of the Workshop in the table.</param>
        /// <returns>List of achievements.</returns>
        /// <response code="200">The entity was found by given Id.</response>
        /// <response code="500">If any server error occures. For example: Id was wrong.</response>
        [AllowAnonymous]
        [HttpGet("{workshopId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<AchievementDto>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetByWorkshopId(Guid workshopId)
        {
            var achievements = await achievementService.GetByWorkshopId(workshopId).ConfigureAwait(false);

            if (!achievements.Any())
            {
                return NoContent();
            }

            return Ok(achievements);
        }

        /// <summary>
        /// To create a new Achievement and add it to the DB.
        /// </summary>
        /// <param name="AchievementCreateDto">AchievementCreateDto object that will be added.</param>
        /// <returns>Achievement that was created.</returns>
        /// <response code="201">Achievement was successfully created.</response>
        /// <response code="400">Model is invalid.</response>
        /// <response code="401">If the user is not authorized.</response>
        /// <response code="403">If the user has no rights to use this method.</response>
        /// <response code="500">If any server error occures.</response>
        [HttpPost]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create(AchievementCreateDTO achievementDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                achievementDto.Id = default;

                var achievement = await achievementService.Create(achievementDto).ConfigureAwait(false);

                return CreatedAtAction(
                     nameof(GetById),
                     new { id = achievement.Id, },
                     achievement);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// To update Achievement entity that already exists.
        /// </summary>
        /// <param name="AchievementCreateDTO">AchievementCreateDTO object with new properties.</param>
        /// <returns>Achievement that was updated.</returns>
        /// <response code="200">Achievement was successfully updated.</response>
        /// <response code="400">Model is invalid.</response>
        /// <response code="401">If the user is not authorized.</response>
        /// <response code="403">If the user has no rights to use this method.</response>
        /// <response code="500">If any server error occures.</response>
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(AchievementDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Update(AchievementCreateDTO achievementDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok(await achievementService.Update(achievementDto).ConfigureAwait(false));
        }

        /// <summary>
        /// Delete the Achievement entity from DB.
        /// </summary>
        /// <param name="id">The key of the Achievement in table.</param>
        /// <returns>Status Code.</returns>
        /// <response code="204">Achievement was successfully deleted.</response>
        /// <response code="400">If some workshops assosiated with this Achievement.</response>
        /// <response code="401">If the user is not authorized.</response>
        /// <response code="403">If the user has no rights to use this method.</response>
        /// <response code="500">If any server error occures.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await achievementService.Delete(id).ConfigureAwait(false);
                return NoContent();
            }

            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
