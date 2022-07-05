using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OutOfSchool.Common.PermissionsModule;
using OutOfSchool.WebApi.Common;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Controllers.V1
{
    /// <summary>
    /// Controller with CRUD operations for a Child entity.
    /// </summary>
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]/[action]")]
    public class ChildController : ControllerBase
    {
        private readonly IChildService service;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChildController"/> class.
        /// </summary>
        /// <param name="service">Service for Child model.</param>
        public ChildController(IChildService service)
        {
            this.service = service ?? throw new ArgumentNullException(nameof(service));
        }

        /// <summary>
        /// Get all children from the database.
        /// </summary>
        /// <param name="filter">Filter to get a part of all children that were found.</param>
        /// <returns>The result is a <see cref="SearchResult{ChildDto}"/> that contains the count of all found children and a list of children that were received.</returns>
        [HasPermission(Permissions.SystemManagement)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SearchResult<ChildDto>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet]
        public async Task<IActionResult> GetAllForAdmin([FromQuery] SearchStringFilter filter)
        {
            return Ok(await service.GetByFilter(filter).ConfigureAwait(false));
        }

        /// <summary>
        /// Get all children from the database by parent's id.
        /// </summary>
        /// <param name="parentId">Id of the parent.</param>
        /// <param name="offsetFilter">Filter to get a part of all children that were found.</param>
        /// <returns>The result is a <see cref="SearchResult{ChildDto}"/> that contains the count of all found children and a list of children that were received.</returns>
        [HasPermission(Permissions.SystemManagement)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SearchResult<ChildDto>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("{parentId}")]
        public async Task<IActionResult> GetByParentIdForAdmin(Guid parentId, [FromQuery] OffsetFilter offsetFilter)
        {
            return Ok(await service.GetByParentIdOrderedByFirstName(parentId, offsetFilter).ConfigureAwait(false));
        }

        /// <summary>
        /// Get all user's children from the database.
        /// </summary>
        /// <param name="offsetFilter">Filter to get a part of all children that were found.</param>
        /// <returns>The result is a <see cref="SearchResult{ChildDto}"/> that contains the count of all found children and a list of children that were received.</returns>
        [HasPermission(Permissions.ChildRead)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SearchResult<ChildDto>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet]
        public async Task<IActionResult> GetUsersChildren([FromQuery] OffsetFilter offsetFilter)
        {
            string userId = GettingUserProperties.GetUserId(User);

            return Ok(await service.GetByUserId(userId, offsetFilter).ConfigureAwait(false));
        }

        /// <summary>
        /// Get the user's child by child's id.
        /// </summary>
        /// <param name="id">The child's id.</param>
        /// <returns>The <see cref="ChildDto"/> that was found or null.</returns>
        [HasPermission(Permissions.ChildRead)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ChildDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUsersChildById(Guid id)
        {
            string userId = GettingUserProperties.GetUserId(User);

            return Ok(await service.GetByIdAndUserId(id, userId).ConfigureAwait(false));
        }

        /// <summary>
        /// Method for creating a new user's child.
        /// </summary>
        /// <param name="childDto">Child entity to add.</param>
        /// <returns>The child that was created.</returns>
        [HasPermission(Permissions.ChildAddNew)]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ChildDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPost]
        public async Task<IActionResult> Create(ChildDto childDto)
        {
            string userId = GettingUserProperties.GetUserId(User);

            var child = await service.CreateChildForUser(childDto, userId).ConfigureAwait(false);

            return CreatedAtAction(
                nameof(GetUsersChildById),
                new { id = child.Id, },
                child);
        }

        /// <summary>
        /// Update info about the user's child in the database.
        /// </summary>
        /// <param name="dto">Child entity to update.</param>
        /// <returns>The child that was updated.</returns>
        [HasPermission(Permissions.ChildEdit)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ChildDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPut]
        public async Task<IActionResult> Update(ChildDto dto)
        {
            string userId = GettingUserProperties.GetUserId(User);

            return Ok(await service.UpdateChildCheckingItsUserIdProperty(dto, userId).ConfigureAwait(false));
        }

        /// <summary>
        /// Delete the user's child from the database.
        /// </summary>
        /// <param name="id">The child's id.</param>
        /// <returns>If deletion was successful, the result will be Status Code 204.</returns>
        [HasPermission(Permissions.ChildRemove)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            string userId = GettingUserProperties.GetUserId(User);

            await service.DeleteChildCheckingItsUserIdProperty(id, userId).ConfigureAwait(false);

            return NoContent();
        }
    }
}