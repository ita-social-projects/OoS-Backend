using Microsoft.AspNetCore.Mvc;
using OutOfSchool.BusinessLogic.Models.Official;

namespace OutOfSchool.WebApi.Controllers.V1;

[ApiController]
[AspApiVersion(1)]
[Route("api/v{version:apiVersion}/director")]
public class DirectorManagementController : ControllerBase
{
    private readonly IDirectorManagementService directorService;
    private readonly ICurrentUserService currentUserService;
    private readonly ILogger<DirectorManagementController> logger;

    public DirectorManagementController(
        IDirectorManagementService directorService,
        ICurrentUserService currentUserService,
        ILogger<DirectorManagementController> logger)
    {
        this.directorService = directorService;
        this.currentUserService = currentUserService;
        this.logger = logger;
    }

    /// <summary>
    /// Promote an employee to the director position (only allowed if no director exists).
    /// </summary>
    /// <param name="providerId">ID of the provider.</param>
    /// <param name="request">Promotion request data.</param>
    /// <returns>Details of the new director position.</returns>
    [Authorize]
    [HttpPost("{providerId:guid}/promote")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PromoteToDirectorResponseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Promote(Guid providerId, [FromBody] PromoteToDirectorRequestDto request)
    {
        try
        {
            var result = await directorService.PromoteEmployeeToDirector(providerId, request);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogWarning(ex, "Unauthorized access during promotion.");
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Business rule violation during promotion.");
            return BadRequest(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogWarning(ex, "Official not found.");
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Transfer the director role to another employee.
    /// Only current director can perform this operation.
    /// </summary>
    /// <param name="providerId">Provider's ID.</param>
    /// <param name="request">Transfer request DTO.</param>
    /// <returns>Result status.</returns>
    [Authorize]
    [HttpPost("{providerId:guid}/transfer")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Transfer(Guid providerId, [FromBody] TransferDirectorRequestDto request)
    {
        try
        {
            var result = await directorService.TransferDirectorPosition(providerId, request);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogWarning(ex, "Unauthorized transfer attempt.");
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Invalid transfer operation.");
            return BadRequest(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogWarning(ex, "One or both officials not found.");
            return NotFound(ex.Message);
        }
    }
}
