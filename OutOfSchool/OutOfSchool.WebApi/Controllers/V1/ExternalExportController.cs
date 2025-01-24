using Microsoft.AspNetCore.Mvc;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Exported.Directions;
using OutOfSchool.BusinessLogic.Models.Exported.Providers;
using OutOfSchool.BusinessLogic.Models.Exported.Workshops;

namespace OutOfSchool.WebApi.Controllers.V1;

[ApiController]
[AspApiVersion(1)]
[Route("api/v{version:apiVersion}/export")]
public class ExternalExportController : ControllerBase
{
    private readonly IExternalExportService externalProviderService;

    public ExternalExportController(IExternalExportService externalProviderService)
    {
        this.externalProviderService = externalProviderService;
    }

    /// <summary>
    /// Get Providers that match filter's parameters.
    /// </summary>
    /// <param name="updatedAfter">The date to filter providers based on their last update.</param>
    /// <param name="offsetFilter">Filter to get a part of all providers that were found.</param>
    /// <returns><see cref="SearchResult{InfoProviderBaseDto}"/>, or no content.</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SearchResult<ProviderInfoBaseDto>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Route("providers")]
    public async Task<IActionResult> GetProvidersByFilter([FromQuery] DateTime updatedAfter,
        [FromQuery] OffsetFilter offsetFilter) =>
        await externalProviderService.GetProviders(updatedAfter, offsetFilter)
            .ProtectAndMap(this.SearchResultToOkOrNoContent);

    /// <summary>
    /// Get Workshops that match filter's parameters.
    /// </summary>
    /// <param name="updatedAfter">The date to filter workshops based on their last update.</param>
    /// <param name="offsetFilter">Filter to get a part of all workshops that were found.</param>
    /// <returns><see cref="SearchResult{WorkshopInfoBaseDto}"/>, or no content.</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SearchResult<WorkshopInfoBaseDto>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Route("workshops")]
    public async Task<IActionResult> GetWorkshopsByFilter([FromQuery] DateTime updatedAfter,
        [FromQuery] OffsetFilter offsetFilter) =>
        await externalProviderService.GetWorkshops(updatedAfter, offsetFilter)
            .ProtectAndMap(this.SearchResultToOkOrNoContent);

    /// <summary>
    /// Get Directions that match filter's parameters.
    /// </summary>
    /// <param name="updatedAfter">The date to filter directions based on their last update.</param>
    /// <param name="offsetFilter">Filter to get a part of all directions that were found.</param>
    /// <returns><see cref="SearchResult{DirectionInfoBaseDto}"/>, or no content.</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SearchResult<DirectionInfoBaseDto>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Route("directions")]
    public async Task<IActionResult> GetDirectionsByFilter([FromQuery] DateTime updatedAfter,
        [FromQuery] OffsetFilter offsetFilter) =>
        await externalProviderService.GetDirections(updatedAfter, offsetFilter)
            .ProtectAndMap(this.SearchResultToOkOrNoContent);

    /// <summary>
    /// Get SubDirections that match filter's parameters.
    /// </summary>
    /// <param name="updatedAfter">The date to filter directions based on their last update.</param>
    /// <param name="offsetFilter">Filter to get a part of all sub directions that were found.</param>
    /// <returns><see cref="SearchResult{SubDirectionsInfoBaseDto}"/>, or no content.</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SearchResult<SubDirectionsInfoBaseDto>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Route("subdirections")]
    public async Task<IActionResult> GetSubDirectionsByFilter([FromQuery] DateTime updatedAfter,
        [FromQuery] OffsetFilter offsetFilter) =>
        await externalProviderService.GetSubDirections(updatedAfter, offsetFilter)
            .ProtectAndMap(this.SearchResultToOkOrNoContent);
}