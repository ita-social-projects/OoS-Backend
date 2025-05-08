using Microsoft.AspNetCore.Mvc;
using OutOfSchool.BusinessLogic.Models.Official;

namespace OutOfSchool.WebApi.Controllers.V1;

[ApiController]
[AspApiVersion(1)]
[Route("api/v{version:apiVersion}/director")]
public class DirectorManagementController : ControllerBase
{
    private readonly IDirectorManagementService directorService;
    private readonly ILogger<DirectorManagementController> logger;

    public DirectorManagementController(
        IDirectorManagementService directorService,
        ICurrentUserService currentUserService,
        ILogger<DirectorManagementController> logger)
    {
        this.directorService = directorService;
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
        var result = await directorService.PromoteEmployeeToDirector(providerId, request);

        if (result.Succeeded)
        {
            return Ok(result.Value);
        }
        var errorCodes = result.OperationResult.Errors.Select(e => e.Code).ToList();

        if (errorCodes.Contains("Unauthorized"))
        {
            return Forbid();
        }

        if (errorCodes.Contains("OfficialNotFound"))
        {
            return NotFound(result.OperationResult.Errors);
        }

        return BadRequest(result.OperationResult.Errors);
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
        var result = await directorService.TransferDirectorPosition(providerId, request);
        if (result.Succeeded)
        {
            return Ok(result.Value);
        }
        
        var errorsCodes = result.OperationResult.Errors.Select(x => x.Code).ToList();
        if (errorsCodes.Contains("Unauthorized"))
        {
            return Forbid();
        }

        if (errorsCodes.Contains("OfficialsNotFound"))
        {
            return NotFound(result.OperationResult.Errors);
        }

        return BadRequest(result.OperationResult.Errors);
    }
}
