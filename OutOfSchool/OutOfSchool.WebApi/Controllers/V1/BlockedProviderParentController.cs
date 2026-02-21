using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using OutOfSchool.BusinessLogic.Models.BlockedProviderParent;
using OutOfSchool.BusinessLogic.Services.ProviderServices;

namespace OutOfSchool.WebApi.Controllers.V1;

[ApiController]
[AspApiVersion(1)]
[Route("api/v{version:apiVersion}/[controller]/[action]")]
public class BlockedProviderParentController : ControllerBase
{
    private readonly IBlockedProviderParentService blockedProviderParentService;
    private readonly IUserService userService;
    private readonly IProviderService providerService;
    private readonly ICurrentUserService currentUserService;

    /// <summary>
    /// Initializes a new instance of the <see cref="BlockedProviderParentController"/> class.
    /// </summary>
    /// <param name="blockedProviderParentService">Service for BlockedProviderParent model.</param>
    /// <param name="userService">Service for operations with users.</param>
    /// <param name="providerService">Service for operations with providers.</param>
    /// <param name="currentUserService">Service for operations with current user.</param>
    public BlockedProviderParentController(
        IBlockedProviderParentService blockedProviderParentService,
        IUserService userService,
        IProviderService providerService,
        ICurrentUserService currentUserService)
    {
        this.blockedProviderParentService = blockedProviderParentService;
        this.userService = userService;
        this.providerService = providerService;
        this.currentUserService = currentUserService;
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
    [HasPermission(Permissions.Employees)]
    [HttpPost]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(BlockedProviderParentDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Block([FromBody] BlockedProviderParentBlockDto blockedProviderParentBlockDto)
    {
        if (blockedProviderParentBlockDto == null)
        {
            return BadRequest("ProviderParent is null.");
        }

        if (await IsProviderBlocked(blockedProviderParentBlockDto.ProviderId).ConfigureAwait(false))
        {
            return StatusCode(403, "Forbidden to block the parent at the blocked provider");
        }

        if (await IsCurrentUserBlocked()) 
        {
            return StatusCode(403, "Forbidden to block the parent by the blocked provider.");
        }

        var result = await blockedProviderParentService.Block(blockedProviderParentBlockDto, currentUserService.UserId).ConfigureAwait(false);

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
    [HasPermission(Permissions.Employees)]
    [HttpPost]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BlockedProviderParentDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UnBlock([FromBody] BlockedProviderParentUnblockDto blockedProviderParentUnblockDto)
    {
        if (blockedProviderParentUnblockDto == null)
        {
            return BadRequest("ProviderParent is null.");
        }

        if (await IsProviderBlocked(blockedProviderParentUnblockDto.ProviderId).ConfigureAwait(false))
        {
            return StatusCode(403, "Forbidden to unblock the parent at the blocked provider");
        }

        if (await IsCurrentUserBlocked())
        {
            return StatusCode(403, "Forbidden to unblock the parent by the blocked provider.");
        }

        var result = await blockedProviderParentService.Unblock(blockedProviderParentUnblockDto, currentUserService.UserId).ConfigureAwait(false);

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

    private async Task<bool> IsCurrentUserBlocked() =>
        await userService.IsBlocked(currentUserService.UserId);

    private async Task<bool> IsProviderBlocked(Guid providerId) =>
        await providerService.IsBlocked(providerId).ConfigureAwait(false) ?? false;
}