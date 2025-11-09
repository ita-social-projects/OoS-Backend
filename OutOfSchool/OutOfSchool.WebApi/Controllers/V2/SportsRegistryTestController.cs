using global::OutOfSchool.SportsRegistryApiClient.Interfaces;
using Microsoft.AspNetCore.Mvc;
namespace OutOfSchool.WebApi.Controllers.V2;
/// <summary>
/// Temporary controller for testing connection with the Ministry of Sports registry.
/// </summary>
[ApiController]
[AspApiVersion(2)]
[Route("api/v{version:apiVersion}/[controller]/[action]")]
public class SportsRegistryTestController : ControllerBase
{
    private readonly ISportsRegistryWorkshopProvider workshopProvider;
    private readonly ILogger<SportsRegistryTestController> logger;

    public SportsRegistryTestController(
        ISportsRegistryWorkshopProvider workshopProvider,
        ILogger<SportsRegistryTestController> logger)
    {
        this.workshopProvider = workshopProvider;
        this.logger = logger;
    }


    /// <summary>
    /// Calls the external Sports Registry API and returns the list of sports sections fot the last numberDays days.
    /// Use only for testing connectivity and data format.
    /// </summary>
    /// <param name="pageSize">Optional page size (default 10)</param>
    /// <param name="numberDays"></param>
    /// <returns>List of sections from the external registry or error message</returns>
    [AllowAnonymous]
    [HttpGet("sections")]
    public async Task<IActionResult> GetSections(
        [FromQuery] int pageSize = 10,
        [FromQuery] int numberDays = 1)
    {
        logger.LogInformation("Testing fetch of sports sections from Sports Registry...");

        var updatedAtFrom = DateTimeOffset.UtcNow.AddDays(-numberDays);
        var updatedAtTo = DateTimeOffset.UtcNow;
        var result = await workshopProvider.GetAllSportsSectionsAsync(updatedAtFrom, updatedAtTo,pageSize);

        return result.Match<IActionResult>(
            error =>
            {
                logger.LogError("Failed to fetch sports sections: {Message}", error.Message);
                return StatusCode((int)error.HttpStatusCode, error);
            },
            success =>
            {
                logger.LogInformation("Fetched {Count} sports sections successfully", success.Count);
                return Ok(success);
            });
    }
}
