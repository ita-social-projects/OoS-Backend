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
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Controllers.V1
{
    /// <summary>
    /// Controller with CRUD operations for Class entity.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]/[action]")]
    [Route("[controller]/[action]")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class ClassController : ControllerBase
    {
        private readonly IClassService service;
        private readonly IStringLocalizer<SharedResource> localizer;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClassController"/> class.
        /// </summary>
        /// <param name="service">Service for Class entity.</param>
        /// <param name="localizer">Localizer.</param>
        public ClassController(IClassService service, IStringLocalizer<SharedResource> localizer)
        {
            this.localizer = localizer;
            this.service = service;
        }

        /// <summary>
        /// To get all Classes from DB.
        /// </summary>
        /// <returns>List of all classes, or no content.</returns>
        /// <response code="200">One or more classes were found.</response>
        /// <response code="204">No class was found.</response>
        /// <response code="500">If any server error occures.</response>
        [AllowAnonymous]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ClassDto>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Get()
        {
            var classes = await service.GetAll().ConfigureAwait(false);

            if (!classes.Any())
            {
                return NoContent();
            }

            return Ok(classes);
        }

        /// <summary>
        /// To recieve the class with the defined id.
        /// </summary>
        /// <param name="id">Key of the class in table.</param>
        /// <returns><see cref="ClassDto"/>.</returns>
        /// <response code="200">The entity was found by given Id.</response>
        /// <response code="500">If any server error occures. For example: Id was wrong.</response>
        [AllowAnonymous]
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ClassDto))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(long id)
        {
            this.ValidateId(id, localizer);

            return Ok(await service.GetById(id).ConfigureAwait(false));
        }

        /// <summary>
        /// To get Classes from DB with some Deprtment id.
        /// </summary>
        /// <param name="id">Department ID.</param>
        /// <returns>List of found classes, or no content.</returns>
        /// <response code="200">One or more classes were found.</response>
        /// <response code="204">No class was found.</response>
        /// <response code="400">Id was wrong.</response>
        /// <response code="500">If any server error occures.</response>
        [AllowAnonymous]
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ClassDto>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetByDepartmentId(long id)
        {
            try
            {
                var classes = await service.GetByDepartmentId(id).ConfigureAwait(false);

                if (!classes.Any())
                {
                    return NoContent();
                }

                return Ok(classes);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// To create a new Class and add it to the DB.
        /// </summary>
        /// <param name="classDto">ClassDto object that will be added.</param>
        /// <returns>Class that was created.</returns>
        /// <response code="201">Class was successfully created.</response>
        /// <response code="400">Model is invalid.</response>
        /// <response code="401">If the user is not authorized.</response>
        /// <response code="403">If the user has no rights to use this method.</response>
        /// <response code="500">If any server error occures.</response>
        // [Authorize(Roles = "admin")]
        [HasPermission(Permissions.SystemManagement)]
        [HttpPost]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create(ClassDto classDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                classDto.Id = default;

                var classEntity = await service.Create(classDto).ConfigureAwait(false);

                return CreatedAtAction(
                     nameof(GetById),
                     new { id = classEntity.Id, },
                     classEntity);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// To update Class entity that already exists.
        /// </summary>
        /// <param name="classDto">ClassDto object with new properties.</param>
        /// <returns>Class that was updated.</returns>
        /// <response code="200">Class was successfully updated.</response>
        /// <response code="400">Model is invalid.</response>
        /// <response code="401">If the user is not authorized.</response>
        /// <response code="403">If the user has no rights to use this method.</response>
        /// <response code="500">If any server error occures.</response>
        // [Authorize(Roles = "admin")]
        [HasPermission(Permissions.SystemManagement)]
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ClassDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Update(ClassDto classDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok(await service.Update(classDto).ConfigureAwait(false));
        }

        /// <summary>
        /// Delete the Class entity from DB.
        /// </summary>
        /// <param name="id">The key of the class in table.</param>
        /// <returns>Status Code.</returns>
        /// <response code="204">Class was successfully deleted.</response>
        /// <response code="401">If the user is not authorized.</response>
        /// <response code="403">If the user has no rights to use this method.</response>
        /// <response code="500">If any server error occures.</response>
        // [Authorize(Roles = "admin")]
        [HasPermission(Permissions.SystemManagement)]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Delete(long id)
        {
            this.ValidateId(id, localizer);

            await service.Delete(id).ConfigureAwait(false);

            return NoContent();
        }
    }
}
