using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using OutOfSchool.WebApi.Common;
using OutOfSchool.WebApi.Extensions;
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
    [Route("[controller]/[action]")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    [Authorize(Roles = "admin,parent")]
    public class ChildController : ControllerBase
    {
        private readonly IChildService service;
        private readonly IStringLocalizer<SharedResource> localizer;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChildController"/> class.
        /// </summary>
        /// <param name="service">Service for Child model.</param>
        /// <param name="localizer">Localizer.</param>
        public ChildController(IChildService service, IStringLocalizer<SharedResource> localizer)
        {
            this.localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
            this.service = service ?? throw new ArgumentNullException(nameof(service));
        }

        /// <summary>
        /// Get all children from the database.
        /// </summary>
        /// <param name="offsetFilter">Filter to get a part of all children that were found.</param>
        /// <returns>The result is a <see cref="SearchResult{ChildDto}"/> that contains the count of all found children and a list of children that were received.</returns>
        [Authorize(Roles = "admin")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SearchResult<ChildDto>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] OffsetFilter offsetFilter)
        {
            var children = await service.GetAllWithOffsetFilterOrderedById(offsetFilter).ConfigureAwait(false);

            if (children.TotalAmount < 1)
            {
                return NoContent();
            }

            return Ok(children);
        }

        /// <summary>
        /// Get all children from the database by parent id.
        /// </summary>
        /// <param name="parentId">Id of Parent.</param>
        /// <param name="offsetFilter">Filter to get a part of all children that were found.</param>
        /// <returns>The result is a <see cref="SearchResult{ChildDto}"/> that contains the count of all found children and a list of children that were received.</returns>
        [Authorize(Roles = "admin")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SearchResult<ChildDto>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("{parentId}")]
        public async Task<IActionResult> GetByParentId([Range(1, long.MaxValue)] long parentId, [FromQuery] OffsetFilter offsetFilter)
        {
            var children = await service.GetByParentIdOrderedByFirstName(parentId, offsetFilter).ConfigureAwait(false);

            if (children.TotalAmount < 1)
            {
                return NoContent();
            }

            return Ok(children);
        }

        /// <summary>
        /// Get all parent's children from the database.
        /// </summary>
        /// <param name="offsetFilter">Filter to get a part of all children that were found.</param>
        /// <returns>The result is a <see cref="SearchResult{ChildDto}"/> that contains the count of all found children and a list of children that were received.</returns>
        [Authorize(Roles = "parent")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SearchResult<ChildDto>))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet]
        public async Task<IActionResult> GetUsersChildren([FromQuery] OffsetFilter offsetFilter)
        {
            string userId = User.GetUserPropertyByClaimType(IdentityResourceClaimsTypes.Sub);

            var children = await service.GetByUserId(userId, offsetFilter).ConfigureAwait(false);

            if (children.TotalAmount < 1)
            {
                return NoContent();
            }

            return Ok(children);
        }

        /// <summary>
        /// Get child by it's id.
        /// </summary>
        /// <param name="id">The key in the database.</param>
        /// <returns>The child that was found.</returns>
        [Authorize(Roles = "parent")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ChildDto))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUsersChildById([Range(1, long.MaxValue)] long id)
        {
            string userId = User.GetUserPropertyByClaimType(IdentityResourceClaimsTypes.Sub);

            var child = await service.GetByIdAndUserId(id, userId).ConfigureAwait(false);

            if (child is null)
            {
                return NoContent();
            }

            return Ok(child);
        }

        /// <summary>
        /// Method for creating a new child.
        /// </summary>
        /// <param name="childDto">Child entity to add.</param>
        /// <returns>The created child.</returns>
        [Authorize(Roles = "parent")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ChildDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPost]
        public async Task<IActionResult> Create(ChildDto childDto)
        {
            string userId = User.GetUserPropertyByClaimType(IdentityResourceClaimsTypes.Sub);

            var child = await service.CreateChildForUser(childDto, userId).ConfigureAwait(false);

            return CreatedAtAction(
                nameof(GetUsersChildById),
                new { id = child.Id, },
                child);
        }

        /// <summary>
        /// Update info about a specific child in the database.
        /// </summary>
        /// <param name="dto">Child entity.</param>
        /// <returns>The updated child.</returns>
        [Authorize(Roles = "parent")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ChildDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPut]
        public async Task<IActionResult> Update(ChildDto dto)
        {
            string userId = User.GetUserPropertyByClaimType(IdentityResourceClaimsTypes.Sub);

            return Ok(await service.UpdateChildCheckingItsUserIdProperty(dto, userId).ConfigureAwait(false));
        }

        /// <summary>
        /// Delete a specific Child entity from the database.
        /// </summary>
        /// <param name="id">Child's key.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [Authorize(Roles = "parent")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([Range(1, long.MaxValue)] long id)
        {
            string userId = User.GetUserPropertyByClaimType(IdentityResourceClaimsTypes.Sub);

            await service.DeleteChildCheckingItsUserIdProperty(id, userId).ConfigureAwait(false);

            return NoContent();
        }
    }
}