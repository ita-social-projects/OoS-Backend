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
    /// Controller with CRUD operations for Department entity.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]/[action]")]
    [Route("[controller]/[action]")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class DepartmentController : ControllerBase
    {
        private readonly IDepartmentService service;
        private readonly IStringLocalizer<SharedResource> localizer;

        /// <summary>
        /// Initializes a new instance of the <see cref="DepartmentController"/> class.
        /// </summary>
        /// <param name="service">Service for Department entity.</param>
        /// <param name="localizer">Localizer.</param>
        public DepartmentController(IDepartmentService service, IStringLocalizer<SharedResource> localizer)
        {
            this.localizer = localizer;
            this.service = service;
        }

        /// <summary>
        /// To get all Departments from DB.
        /// </summary>
        /// <returns>List of all departments, or no content.</returns>
        /// <response code="200">One or more deparments were found.</response>
        /// <response code="204">No department was found.</response>
        /// <response code="500">If any server error occures.</response>
        [AllowAnonymous]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<DepartmentDto>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Get()
        {
            var departments = await service.GetAll().ConfigureAwait(false);

            if (!departments.Any())
            {
                return NoContent();
            }

            return Ok(departments);
        }

        /// <summary>
        /// To recieve the department with the defined id.
        /// </summary>
        /// <param name="id">Key of the department in the table.</param>
        /// <returns><see cref="DepartmentDto"/>.</returns>
        /// <response code="200">The entity was found by given Id.</response>
        /// <response code="500">If any server error occures. For example: Id was wrong.</response>
        [AllowAnonymous]
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DepartmentDto))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(long id)
        {
            this.ValidateId(id, localizer);

            return Ok(await service.GetById(id).ConfigureAwait(false));
        }

        /// <summary>
        /// To get all Departments from DB with some DirectionId.
        /// </summary>
        /// <param name="id">The direction's Id.</param>
        /// <returns>List of found departments, or no content.</returns>
        /// <response code="200">One or more departments were found.</response>
        /// <response code="204">No department was found.</response>
        /// <response code="400">Id was wrong.</response>
        /// <response code="500">If any server error occures.</response>
        [AllowAnonymous]
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<DepartmentDto>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetByDirectionId(long id)
        {
            try
            {
                var departments = await service.GetByDirectionId(id).ConfigureAwait(false);

                if (!departments.Any())
                {
                    return NoContent();
                }

                return Ok(departments);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// To create a new Department and add it to the DB.
        /// </summary>
        /// <param name="departmentDto">DepartmentDto object that will be added.</param>
        /// <returns>Department that was created.</returns>
        /// <response code="201">Department was successfully created.</response>
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
        public async Task<IActionResult> Create(DepartmentDto departmentDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                departmentDto.Id = default;

                var department = await service.Create(departmentDto).ConfigureAwait(false);

                return CreatedAtAction(
                     nameof(GetById),
                     new { id = department.Id, },
                     department);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// To update Department entity that already exists.
        /// </summary>
        /// <param name="departmentDto">DepartmentDto object with new properties.</param>
        /// <returns>Department that was updated.</returns>
        /// <response code="200">Department was successfully updated.</response>
        /// <response code="400">Model is invalid.</response>
        /// <response code="401">If the user is not authorized.</response>
        /// <response code="403">If the user has no rights to use this method.</response>
        /// <response code="500">If any server error occures.</response>
        // [Authorize(Roles = "admin")]
        [HasPermission(Permissions.SystemManagement)]
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(DepartmentDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Update(DepartmentDto departmentDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return Ok(await service.Update(departmentDto).ConfigureAwait(false));
        }

        /// <summary>
        /// Delete the Department entity from DB.
        /// </summary>
        /// <param name="id">The key of the Department in table.</param>
        /// <returns>Status Code.</returns>
        /// <response code="204">Department was successfully deleted.</response>
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
