using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using OutOfSchool.BusinessLogic.Common;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Parent;

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
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> BlockUnblockParent([FromBody] BlockUnblockParentDto parentBlockUnblock)
    {
        var result = await serviceParent.BlockUnblockParent(parentBlockUnblock).ConfigureAwait(false);

        if (!result.Succeeded)
        {
            return BadRequest(result.OperationResult);
        }

        return Ok();
    }
 }