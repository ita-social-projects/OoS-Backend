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
    /// Block Parent entity.
    /// </summary>
    /// <param name="id">The key of the Parent entity in the table.</param>
    /// <param name="isBlocked">A boolean value indicating whether to block the Parent entity (true) or not (false).</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    [HasPermission(Permissions.SystemManagement)]
    [HttpPost("BlockParent")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> BlockParent(Guid id, bool isBlocked)
    {
        var result = await serviceParent.BlockParent(id, isBlocked).ConfigureAwait(false);

        if (!result.Succeeded)
        {
            if (result.OperationResult.Errors.Any(x => x.Code == "404"))
            {
                return NotFound(result.OperationResult);
            }

            return BadRequest(result.OperationResult);
        }

        return NoContent();
    }
 }