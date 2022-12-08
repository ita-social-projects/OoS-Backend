using Microsoft.AspNetCore.Mvc;
using OutOfSchool.WebApi.Enums;
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
        this.achievementTypeService = service ?? throw new ArgumentNullException(nameof(service));
    }

    /// <summary>
    /// Get all Achievement Types from the database.
    /// </summary>
    /// <param name="localization">Localization: Ua - 0, En - 1.</param>
    /// <returns>List of all Achievement Types.</returns>
    /// <response code="400">Localization is invalid.</response>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<AchievementTypeDto>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll(AchievementTypeLocalization localization = AchievementTypeLocalization.Ua)
    {
        return Ok(await achievementTypeService.GetAll(localization).ConfigureAwait(false));
    }
}
