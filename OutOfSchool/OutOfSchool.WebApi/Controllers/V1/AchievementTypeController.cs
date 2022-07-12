using Microsoft.AspNetCore.Mvc;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Controllers.V1;

/// <summary>
/// Controller with CRUD operations for Achievement Type entity.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]/[action]")]
public class AchievementTypeController : Controller
{
    private readonly IAchievementTypeService achievementTypeService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AchievementTypeController"/> class.
    /// </summary>
    /// <param name="service">Service for Achievement Type entity.</param>
    public AchievementTypeController(IAchievementTypeService service)
    {
        this.achievementTypeService = service;
    }

    /// <summary>
    /// Get all Achievement Types from the database.
    /// </summary>
    /// <returns>List of all Achievement Types.</returns>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<AchievementTypeDto>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await achievementTypeService.GetAll().ConfigureAwait(false));
    }
}
