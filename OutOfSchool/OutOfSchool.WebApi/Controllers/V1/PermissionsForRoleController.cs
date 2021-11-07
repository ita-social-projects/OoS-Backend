using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using OutOfSchool.Common.PermissionsModule;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Controllers.V1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]/[action]")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [HasPermission(Permissions.SystemManagement)]
    public class PermissionsForRoleController : ControllerBase
    {
        private readonly IPermissionsForRoleService service;
        private readonly IStringLocalizer<SharedResource> localizer;

        /// <summary>
        /// Initializes a new instance of the <see cref="PermissionsForRoleController"/> class.
        /// </summary>
        /// <param name="service">Service for PermissionsForRole model.</param>
        /// <param name="localizer">Localizer.</param>
        public PermissionsForRoleController(IPermissionsForRoleService service, IStringLocalizer<SharedResource> localizer)
        {
            this.service = service;
            this.localizer = localizer;
        }

        /// <summary>
        /// Get all PermissionsForRole entities from the database.
        /// </summary>
        /// <returns>List of all PermissionsForRole entities in DB.</returns>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<PermissionsForRoleDTO>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var permissionsForRoles = await service.GetAll().ConfigureAwait(false);

            if (!permissionsForRoles.Any())
            {
                return NoContent();
            }

            return Ok(permissionsForRoles);
        }

        /// <summary>
        /// Get PermissionsForRole entity by it's roleName.
        /// </summary>
        /// <param name="roleName">PermissionsForRole entity roleName.</param>
        /// <returns>PermissionsForRole entity.</returns>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PermissionsForRoleDTO))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("{roleName}")]
        public async Task<IActionResult> GetByRoleName(string roleName)
        {
            var taskToDo = service.GetByRole(roleName);
            PermissionsForRoleDTO result;
            try
            {
                result = await taskToDo.ConfigureAwait(false);
            }
            catch (ArgumentNullException e)
            {
                return Ok(new { e.ParamName });
            }
            return Ok(result);
        }

        /// <summary>
        /// Add a new PermissionsForRole entity to the database.
        /// </summary>
        /// <param name="dto">PermissionsForRole entity to add.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPost]
        public async Task<IActionResult> Create(PermissionsForRoleDTO dto)
        {
            var permissionsForRole = await service.Create(dto).ConfigureAwait(false);

            return CreatedAtAction(
                nameof(GetByRoleName),
                new { id = permissionsForRole.Id, roleName = permissionsForRole.RoleName },
                permissionsForRole);
        }

        /// <summary>
        /// Update data in PermissionsForRole entity in the database.
        /// </summary>
        /// <param name="dto">PermissionsForRole to update.</param>
        /// <returns>PermissionsForRole</returns>
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPut]
        public async Task<IActionResult> Update(PermissionsForRoleDTO dto)
        {
            return Ok(await service.Update(dto).ConfigureAwait(false));
        }

        /// <summary>
        /// Gets all currently existing permissions from system and returns friendly response for admin person.
        /// </summary>
        /// <returns>List of all Permissions  DB.</returns>
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<string>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet]
        public IActionResult GetAllPermissions()
        {
            var resultQuery = Enum.GetValues(typeof(Permissions)).Cast<Permissions>();
            var resultValues = resultQuery.Select(p => p.ToString() + $". Use code - [{(int)p}] to add permission.");
            return Ok(resultValues);
        }

    }
}
