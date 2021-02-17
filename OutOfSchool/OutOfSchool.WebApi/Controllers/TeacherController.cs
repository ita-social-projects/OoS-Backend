using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OutOfSchool.Services.Models;
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
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Teacher>>> GetTeachers()
        {
            return Ok(await teacherService.GetAllTeachers().ConfigureAwait(false));
        }

        /// <summary>
        /// Get teacher by it's id.
        /// </summary>
        /// <param name="id">Teacher's key.</param>
        /// <returns>Teacher.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<TeacherDTO>> GetTeacherById(long id)
        {
            if (id == 0)
            {
                return BadRequest("Id cannot be 0.");
            }

            var teacherDto = await teacherService.GetById(id).ConfigureAwait(false);

            return Ok(teacherDto);
        }

        /// <summary>
        /// Add a new teacher to the database.
        /// </summary>
        /// <param name="teacherDto">Entity to add.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [HttpPost]
        public async Task<ActionResult<Teacher>> CreateTeacher(TeacherDTO teacherDto)
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
        /// <param name="teacherDto">Teacher to update.</param>
        /// <returns>Teacher's key.</returns>
        [Authorize(Roles = "organization,admin")]
        [HttpPut]
        public async Task<ActionResult> Update(TeacherDTO teacherDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok(await teacherService.Update(teacherDto).ConfigureAwait(false));
        }

        /// <summary>
        /// Delete a specific Teacher entity from the database.
        /// </summary>
        /// <param name="id">Teacher's key.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [Authorize(Roles = "organization,admin")]
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(long id)
        {
            if (id == 0)
            {
                return BadRequest("Id cannot be 0.");
            }

            await teacherService.Delete(id).ConfigureAwait(false);

            return Ok();
        }
    }
}