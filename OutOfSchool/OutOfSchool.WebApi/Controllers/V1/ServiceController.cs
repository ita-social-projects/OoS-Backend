using Microsoft.AspNetCore.Mvc;

namespace OutOfSchool.WebApi.Controllers.V1;

/// <summary>
/// Controller for service health check.
/// </summary>

[ApiController]
[AspApiVersion(1)]
[Route("api/v{version:apiVersion}/[controller]/[action]")]
public class ServiceController : ControllerBase
{
    /// <summary>
    /// Performs a health check for the service.
    /// </summary>
    /// <returns>
    /// Returns 204 No Content if the service is healthy and running.
    /// </returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None, Duration = -1)]
    public IActionResult GetHealth()
    {
        Response.Headers["Expires"] = "-1";
        return NoContent();
    }
}