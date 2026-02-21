using Microsoft.AspNetCore.Mvc;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Official;

namespace OutOfSchool.WebApi.Controllers.V1;

/// <summary>
/// Controller with CRUD operations for Official entity
/// </summary>
[ApiController]
[AspApiVersion(1)]
[Route("api/v{version:apiVersion}/providers/{providerId}/officials/[action]")]
public class OfficialController : ControllerBase
{
    private readonly IOfficialService service;

    /// <summary>
    /// Initializes a new instance of the <see cref="OfficialController"/> class.
    /// </summary>
    /// <param name="service">Service for Official entity.</param>
    public OfficialController(
        IOfficialService service
        )
    {
        this.service = service;
    }

    /// <summary>
    /// Gets filtered list of officials from the database.
    /// </summary>
    /// <param name="providerId">Provider's id.</param>
    /// <param name="filter">Filter for list of officials.</param>
    /// <returns>SearchResult with list of filtered officials</returns>
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SearchResult<OfficialDto>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet]
    public async Task<IActionResult> Get([FromRoute] Guid providerId, [FromQuery] SearchStringFilter filter = null) =>
        await service.GetByFilter(providerId, filter)
            .ProtectAndMap(this.SearchResultToOkOrNoContent);
}
