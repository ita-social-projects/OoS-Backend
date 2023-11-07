using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using OutOfSchool.WebApi.Common;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.BlockedProviderParent;
using OutOfSchool.WebApi.Models.Workshops;

namespace OutOfSchool.WebApi.Controllers.V1;

/// <summary>
/// Controller with CRUD operations for parent entity.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/parents")]
public class ParentController : ControllerBase
{
    private readonly IParentService serviceParent;
    private readonly IUserService userService;
    private readonly ICurrentUserService currentUserService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ParentController"/> class.
    /// Initialization of ParentController.
    /// </summary>
    /// <param name="serviceParent">Parent service for ParentController.</param>
    /// <param name="currentUserService">Service for operations with current user.</param>
    public ParentController(
        IParentService serviceParent,
        ICurrentUserService currentUserService)
    {
        this.serviceParent = serviceParent ?? throw new ArgumentNullException(nameof(serviceParent));
        this.currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
    }

    /// <summary>
    /// Delete Parent entity from DB.
    /// </summary>
    /// <param name="id">The key in table.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    [HasPermission(Permissions.ParentRemove)]
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Delete(Guid id)
    {
        await serviceParent.Delete(id).ConfigureAwait(false);

        return NoContent();
    }

    /// <summary>
    /// To Get the Profile of authorized Parent.
    /// </summary>
    /// <returns>Authorized parent's profile.</returns>
    [HasPermission(Permissions.ParentRead)]
    [HttpGet("profile")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ParentDTO))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ParentDTO>> GetProfile()
    {
        var userId = GettingUserProperties.GetUserId(HttpContext);
        var parentDto = await serviceParent.GetByUserId(userId).ConfigureAwait(false);
        return parentDto is not null ? Ok(parentDto) : NoContent();
    }

    /// <summary>
    /// Block Parent entity.
    /// </summary>
    /// <param name="id">The key in table.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    [HasPermission(Permissions.SystemManagement)]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Block(Guid id)
    {
        await serviceParent.Block(id, currentUserService.UserId).ConfigureAwait(false);

        return NoContent();
    }

    /// <summary>
    /// Block Parent entity.
    /// </summary>
    /// <param name="id">The key in table.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    [HasPermission(Permissions.SystemManagement)]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> UnBlock(Guid id)
    {
        await serviceParent.UnBlock(id, currentUserService.UserId).ConfigureAwait(false);

        return NoContent();
    }
}