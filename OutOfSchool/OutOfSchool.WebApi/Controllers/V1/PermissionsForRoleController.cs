using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using OutOfSchool.Common.PermissionsModule;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Controllers.V1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]/[action]")]
    [Route("[controller]/[action]")]
    [Authorize(AuthenticationSchemes = "Bearer")]
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
        [ProducesResponseType(StatusCodes.Status200OK)]
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("{roleName}")]
        public async Task<IActionResult> GetByRoleName(string roleName)
        {
            var permissionsForRole = await service.GetByRole(roleName).ConfigureAwait(false);
            return Ok(permissionsForRole);
        }
    }
}
