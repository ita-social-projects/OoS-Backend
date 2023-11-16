using Microsoft.AspNetCore.Mvc;
using OutOfSchool.WebApi.Common;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.Parent;

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

    /// <summary>
    /// Initializes a new instance of the <see cref="ParentController"/> class.
    /// Initialization of ParentController.
    /// </summary>
    /// <param name="serviceParent">Parent service for ParentController.</param>
    public ParentController(
        IParentService serviceParent)
    {
        this.serviceParent = serviceParent ?? throw new ArgumentNullException(nameof(serviceParent));
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
    /// Block or unblock Parent entity based on the provided information.
    /// </summary>
    /// <param name="parentBlockUnblock">A DTO containing the necessary information to block or unblock a parent,
    /// including the ParentId, the desired block status, and a reason.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    [HasPermission(Permissions.ParentBlock)]
    [HttpPost("BlockUnblockParent")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> BlockUnblockParent(BlockUnblockParentDto parentBlockUnblock)
    {
        var result = await serviceParent.BlockUnblockParent(parentBlockUnblock).ConfigureAwait(false);

        if (result.Succeeded)
        {
            return Ok();
        }

        if (result.OperationResult.Errors.Any(x => x.Code == StatusCodes.Status404NotFound.ToString()))
        {
            return NotFound(result.OperationResult);
        }

        return BadRequest(result.OperationResult);
    }
 }