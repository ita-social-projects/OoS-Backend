using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using OutOfSchool.Common.PermissionsModule;
using OutOfSchool.WebApi.Common;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.Images;
using OutOfSchool.WebApi.Models.Responses;
using OutOfSchool.WebApi.Models.Teachers;
using OutOfSchool.WebApi.Models.Workshop;
using OutOfSchool.WebApi.Services;
using OutOfSchool.WebApi.Util.ControllersResultsHelpers;

namespace OutOfSchool.WebApi.Controllers.V2
{
    /// <summary>
    /// Controller with CRUD operations for a Teacher entity.
    /// </summary>
    [ApiController]
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/[controller]/[action]")]
    public class TeacherController : ControllerBase
    {
        private readonly ITeacherService teacherService;
        private readonly IStringLocalizer<SharedResource> localizer;

        /// <summary>
        /// Initializes a new instance of the <see cref="TeacherController"/> class.
        /// </summary>
        /// <param name="teacherService">Service for Teacher model.</param>
        /// <param name="localizer">Localizer.</param>
        public TeacherController(ITeacherService teacherService, IStringLocalizer<SharedResource> localizer)
        {
            this.localizer = localizer;
            this.teacherService = teacherService;
        }

        /// <summary>
        /// Get all teachers from the database.
        /// </summary>
        /// <returns>List of teachers.</returns>
        [HasPermission(Permissions.TeacherRead)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<TeacherDTO>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var teachers = await teacherService.GetAll().ConfigureAwait(false);

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
        [HasPermission(Permissions.TeacherRead)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TeacherDTO))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var teacher = await teacherService.GetById(id).ConfigureAwait(false);

            return teacher != null ? (IActionResult)Ok(teacher) : NoContent();
        }

        /// <summary>
        /// Add a new teacher to the database.
        /// </summary>
        /// <param name="dto">Entity to add.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [HasPermission(Permissions.TeacherAddNew)]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(CreationWithImagesResponse<Guid>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Consumes("multipart/form-data")]
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] TeacherCreationDto dto)
        {
            var creationResult = await teacherService.Create(dto).ConfigureAwait(false);

            return CreatedAtAction(
                nameof(GetById),
                new { id = creationResult.Dto.Id, },
                new CreationWithImagesResponse<Guid>
                {
                    Id = creationResult.Dto.Id,
                    UploadingImagesResults = creationResult.UploadingImageResult.CreateSingleUploadingResult(),
                });
        }

        /// <summary>
        /// Update info about a specific teacher in the database.
        /// </summary>
        /// <param name="dto">Teacher to update.</param>
        /// <returns>Teacher.</returns>
        [HasPermission(Permissions.TeacherEdit)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UpdateWithImagesResponse<TeacherDTO>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Consumes("multipart/form-data")]
        [HttpPut]
        public async Task<IActionResult> Update([FromForm] TeacherUpdateDto dto)
        {
            var updatedTeacher = await teacherService.Update(dto).ConfigureAwait(false);

            return Ok(new UpdateWithImagesResponse<TeacherDTO>
            {
                UpdatedEntity = updatedTeacher.Dto,
                UploadingImagesResults = updatedTeacher.UploadingImageResult?.CreateSingleUploadingResult(),
                RemovingImagesResults = updatedTeacher.RemovingImageResult?.CreateSingleRemovingResult(),
            });
        }

        /// <summary>
        /// Delete a specific Teacher entity from the database.
        /// </summary>
        /// <param name="id">Teacher's id.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [HasPermission(Permissions.TeacherRemove)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await teacherService.Delete(id).ConfigureAwait(false);

            return NoContent();
        }
    }
}