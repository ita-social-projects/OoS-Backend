using Microsoft.AspNetCore.Mvc;
using OutOfSchool.AikomApiClient;
using OutOfSchool.BusinessLogic.Enums;
using OutOfSchool.BusinessLogic.Models;

namespace OutOfSchool.WebApi.Controllers.V1;

/// <summary>
/// Controller with CRUD operations for Achievement Type entity.
/// </summary>
[ApiController]
[AspApiVersion(1)]
[Route("api/v{version:apiVersion}/[controller]/[action]")]
public class AchievementTypeController : Controller
{
    private readonly IAchievementTypeService achievementTypeService;
    private readonly IAikomApiService aikomService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AchievementTypeController"/> class.
    /// </summary>
    /// <param name="service">Service for Achievement Type entity.</param>
    public AchievementTypeController(IAchievementTypeService service, IAikomApiService aikomService)
    {
        this.achievementTypeService = service ?? throw new ArgumentNullException(nameof(service));
        this.aikomService = aikomService ?? throw new ArgumentNullException(nameof(aikomService));
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
    public async Task<IActionResult> GetAll(LocalizationType localization = LocalizationType.Ua)
    {
        var result = await aikomService.SearchUniversity("87652321").ConfigureAwait(false);
        return Ok(await achievementTypeService.GetAll(localization).ConfigureAwait(false));
    }
}
