using Microsoft.AspNetCore.Mvc;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Exported;

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
    public async Task<IActionResult> GetProvidersByFilter([FromQuery] DateTime updatedAfter, [FromQuery] OffsetFilter offsetFilter)
    {
        try
        {
            var result = await externalProviderService.GetProviders(updatedAfter, offsetFilter);

            return this.SearchResultToOkOrNoContent(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }
    
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
    public async Task<IActionResult> GetWorkshopsByFilter([FromQuery] DateTime updatedAfter, [FromQuery] OffsetFilter offsetFilter)
    {
        try
        {
            var result = await externalProviderService.GetWorkshops(updatedAfter, offsetFilter);

            return this.SearchResultToOkOrNoContent(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }
}