using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Controllers.V1;

/// <summary>
/// Controller for data synchronization between databases.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]/[action]")]
[Authorize(AuthenticationSchemes = "Bearer")]
[Authorize(Roles = "admin")]
public class ElasticsearchSynchronizationController : ControllerBase
{
    private readonly IElasticsearchSynchronizationService elasticsearchSynchronizationService;

    public ElasticsearchSynchronizationController(IElasticsearchSynchronizationService elasticsearchSynchronizationService)
    {
        this.elasticsearchSynchronizationService = elasticsearchSynchronizationService;
    }

    /// <summary>
    /// Synchronize data.
    /// </summary>
    /// <returns>StatusCode representing the task completion.</returns>
    /// <response code="200">Synchronization complete successfully.</response>
    /// <response code="400">If the model is invalid, some properties are not set etc.</response>
    /// <response code="401">If the user is not authorized.</response>
    /// <response code="403">If the user has no rights to use this method, or sets some properties that are forbidden.</response>
    /// <response code="500">If any server error occures.</response>
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPost]
    public async Task<IActionResult> Synchronize()
    {
        var result = await elasticsearchSynchronizationService.Synchronize().ConfigureAwait(false);

        if (!result)
        {
            return StatusCode(500, "Synchronization failed.");
        }

        return Ok();
    }
}