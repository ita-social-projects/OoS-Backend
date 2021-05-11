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
    /// <summary>
    /// Controller with CRUD operations for a Teacher entity.
    /// </summary>
    [ApiController]
    [Route("[controller]/[action]")]
    public class TeacherController : ControllerBase
    {
        private readonly ITeacherService service;
        private readonly IStringLocalizer<SharedResource> localizer;

        /// <summary>
        /// Initializes a new instance of the <see cref="TeacherController"/> class.
        /// </summary>
        /// <param name="service">Service for Teacher model.</param>
        /// <param name="localizer">Localizer.</param>
        public TeacherController(ITeacherService service, IStringLocalizer<SharedResource> localizer)
        {
            this.localizer = localizer;
            this.service = service;
        }

        /// <summary>
        /// Get all teachers from the database.
        /// </summary>
        /// <returns>List of teachers.</returns>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var teachers = await service.GetAll().ConfigureAwait(false);

            if (!teachers.Any())
            {
                return NoContent();
            }

            return Ok(teachers);
        }

        /// <summary>
        /// Get teacher by it's id.
        /// </summary>
        /// <param name="id">Teacher's id.</param>
        /// <returns>Teacher.</returns>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
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
        /// Add a new teacher to the database.
        /// </summary>
        /// <param name="dto">Entity to add.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [Authorize(Roles = "provider,admin")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPost]
        public async Task<IActionResult> Create(TeacherDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var teacher = await service.Create(dto).ConfigureAwait(false);

            return CreatedAtAction(
                nameof(GetById),
                new { id = teacher.Id, },
                teacher);
        }

        /// <summary>
        /// Update info about a specific teacher in the database.
        /// </summary>
        /// <param name="dto">Teacher to update.</param>
        /// <returns>Teacher.</returns>
        [Authorize(Roles = "provider,admin")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPut]
        public async Task<IActionResult> Update(TeacherDTO dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok(await service.Update(dto).ConfigureAwait(false));
        }

        /// <summary>
        /// Delete a specific Teacher entity from the database.
        /// </summary>
        /// <param name="id">Teacher's id.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [Authorize(Roles = "provider,admin")]
        [Authorize(AuthenticationSchemes = "Bearer")]
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
    }
}