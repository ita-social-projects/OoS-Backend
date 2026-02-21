using Microsoft.AspNetCore.Mvc;
using OutOfSchool.BusinessLogic.Enums;
using OutOfSchool.BusinessLogic.Models.CompetitiveEvent;

namespace OutOfSchool.WebApi.Controllers.V1;

/// <summary>
/// Controller with CRUD operations for CompetitiveEvent Accounting Type entity.
/// </summary>
[ApiController]
[AspApiVersion(1)]
[Route("api/v{version:apiVersion}/[controller]/[action]")]
public class CompetitiveEventAccountingTypeController : ControllerBase
{
    private readonly ICompetitiveEventAccountingTypeService accountingTypeService;

    /// <summary>
    /// Initializes a new instance of the <see cref="CompetitiveEventAccountingTypeController"/> class.
    /// </summary>
    /// <param name="service">Service for CompetitiveEvent Accounting Type entity.</param>
    public CompetitiveEventAccountingTypeController(ICompetitiveEventAccountingTypeService service)
    {
        this.accountingTypeService = service ?? throw new ArgumentNullException(nameof(service));
    }

    /// <summary>
    /// Get all CompetitiveEvent Accounting Types from the database.
    /// </summary>
    /// <param name="localization">Localization: Ua - 0, En - 1.</param>
    /// <returns>List of all CompetitiveEvent Accounting Types.</returns>
    /// <response code="400">Localization is invalid.</response>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<CompetitiveEventAccountingTypeDto>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAll(LocalizationType localization = LocalizationType.Ua)
    {
        var competitiveEventAccountingTypes = await accountingTypeService.GetAll(localization).ConfigureAwait(false);
        if (!competitiveEventAccountingTypes.Any())
        {
            return NoContent();
        }
        return Ok(competitiveEventAccountingTypes);
    }
}
