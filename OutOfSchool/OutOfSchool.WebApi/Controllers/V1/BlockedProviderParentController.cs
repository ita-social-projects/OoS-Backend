using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OutOfSchool.Common.PermissionsModule;
using OutOfSchool.WebApi.Models.BlockedProviderParent;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Controllers.V1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]/[action]")]
    public class BlockedProviderParentController : ControllerBase
    {
        private readonly IBlockedProviderParentService blockedProviderParentService;

        /// <summary>
        /// Initializes a new instance of the <see cref="BlockedProviderParentController"/> class.
        /// </summary>
        /// <param name="blockedProviderParentService">Service for BlockedProviderParent model.</param>
        public BlockedProviderParentController(
            IBlockedProviderParentService blockedProviderParentService)
        {
            this.blockedProviderParentService = blockedProviderParentService;
        }

        /// <summary>
        /// Creating BlockedProviderParent in the database. Update dependent entities (IsBlocked = true).
        /// </summary>
        /// <param name="blockedProviderParentBlockDto">blockedProviderParentBlock entity to add.</param>
        /// <returns>A <see cref="Task{BlockedProviderParentDto}"/> representing the result of the asynchronous operation.</returns>
        /// <response code="201">BlockedProviderParent was successfully created.</response>
        /// <response code="400">BlockedProviderParentBlockDto was incorrect. Block exists with current ProviderId and ParentId.</response>
        /// <response code="401">If the user is not authorized.</response>
        /// <response code="500">If any server error occures.</response>
        [HasPermission(Permissions.ProviderAdmins)]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(BlockedProviderParentDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Block(BlockedProviderParentBlockDto blockedProviderParentBlockDto)
        {
            var result = await blockedProviderParentService.Block(blockedProviderParentBlockDto).ConfigureAwait(false);

            if (!result.Succeeded)
            {
                return BadRequest(result.OperationResult);
            }

            var blockedProviderParent = result.Value;

            return CreatedAtAction(
                nameof(GetBlock),
                new { parentId = blockedProviderParent.ParentId, providerId = blockedProviderParent.ProviderId, },
                blockedProviderParent);
        }

        /// <summary>
        /// Update existing BlockedProviderParent in the database. Update dependent entities (IsBlocked = false).
        /// </summary>
        /// <param name="blockedProviderParentUnblockDto">blockedProviderParentUnblockDto entity to update.</param>
        /// <returns>A <see cref="Task{BlockedProviderParentDto}"/> representing the result of the asynchronous operation.</returns>
        /// <response code="200">BlockedProviderParent was successfully updated.</response>
        /// <response code="400">BlockedProviderParentUnblockDto was incorrect. Block does not exist with current ProviderId and ParentId.</response>
        /// <response code="401">If the user is not authorized.</response>
        /// <response code="500">If any server error occures.</response>
        [HasPermission(Permissions.ProviderAdmins)]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BlockedProviderParentDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UnBlock(BlockedProviderParentUnblockDto blockedProviderParentUnblockDto)
        {
            var result = await blockedProviderParentService.Unblock(blockedProviderParentUnblockDto).ConfigureAwait(false);

            if (!result.Succeeded)
            {
                return BadRequest(result.OperationResult);
            }

            return Ok(result.Value);
        }

        /// <summary>
        /// Return blocked entity.
        /// </summary>
        /// <param name="parentId">Key of the Parent in table.</param>
        /// <param name="providerId">Key of the Provider in table.</param>
        /// <returns>A <see cref="Task{BlockedProviderParentDto}"/> that was found or null.</returns>
        /// <response code="200">Operation was executed successfully.</response>
        /// <response code="204">Blocked entity was not found.</response>
        /// <response code="400">Id was wrong.</response>
        /// <response code="401">If the user is not authorized.</response>
        /// <response code="500">If any server error occures.</response>
        [Authorize]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BlockedProviderParentDto))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetBlock(Guid parentId, Guid providerId)
        {
            return Ok(await blockedProviderParentService.GetBlock(parentId, providerId).ConfigureAwait(false));
        }
    }
}
