using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Controllers
{
    /// <summary>
    /// Controller with CRUD operations for a Teacher entity.
    /// </summary>
    [ApiController]
    [Route("[controller]/[action]")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class TeacherController : ControllerBase
    {
        private readonly ITeacherService teacherService;

        /// <summary>
        /// Initializes a new instance of the <see cref="TeacherController"/> class.
        /// </summary>
        /// <param name="teacherService">Service for Teacher model.</param>
        public TeacherController(ITeacherService teacherService)
        {
            this.teacherService = teacherService;
        }

        /// <summary>
        /// Get all teachers from the database.
        /// </summary>
        /// <returns>List of teachers.</returns>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [HttpGet]
        public async Task<IActionResult> GetTeachers()
        {
            return Ok(await teacherService.GetAll().ConfigureAwait(false));
        }

        /// <summary>
        /// Get teacher by it's id.
        /// </summary>
        /// <param name="id">Teacher's key.</param>
        /// <returns>Teacher.</returns>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTeacherById(long id)
        {
            if (id < 1 || teacherService.GetAll().Result.AsQueryable().Count() < id)
            {
                throw new ArgumentOutOfRangeException(id.ToString(), "The id is less than 1 or greater than number of table entities.");
            }

            return Ok(await teacherService.GetById(id).ConfigureAwait(false));
        }

        /// <summary>
        /// Add a new teacher to the database.
        /// </summary>
        /// <param name="teacherDto">Entity to add.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [Authorize(Roles = "organization,admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPost]
        public async Task<IActionResult> CreateTeacher(TeacherDTO teacherDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var teacher = await teacherService.Create(teacherDto).ConfigureAwait(false);

            return CreatedAtAction(
                nameof(GetTeacherById),
                new
                {
                    id = teacher.Id,
                });
        }

        /// <summary>
        /// Update info about a specific teacher in the database.
        /// </summary>
        /// <param name="dto">Teacher to update.</param>
        /// <returns>Teacher's key.</returns>
        [Authorize(Roles = "organization,admin")]
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

            return Ok(await teacherService.Update(dto).ConfigureAwait(false));
        }

        /// <summary>
        /// Delete a specific Teacher entity from the database.
        /// </summary>
        /// <param name="id">Teacher's key.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [Authorize(Roles = "organization,admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTeacher(long id)
        {
            if (id < 1 || teacherService.GetAll().Result.AsQueryable().Count() < id)
            {
                throw new ArgumentOutOfRangeException(id.ToString(),
                    "The id is less than 1 or greater than number of table entities.");
            }

            await teacherService.Delete(id).ConfigureAwait(false);

            return Ok();
        }
    }
}