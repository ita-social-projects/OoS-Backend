using Microsoft.AspNetCore.Mvc;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.ProvidersInfo;

namespace OutOfSchool.WebApi.Controllers.V1;

[ApiController]
[AspApiVersion(1)]
[Route("api/v{version:apiVersion}/providers")]
public class ExternalExportProviderController : ControllerBase
{
    private readonly IExternalExportProviderService externalProviderService;

    public ExternalExportProviderController(IExternalExportProviderService externalProviderService)
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
    [Route("export")]
    public async Task<IActionResult> GetByFilter([FromQuery] DateTime updatedAfter, [FromQuery] OffsetFilter offsetFilter)
    {
        try
        {
            var result = await externalProviderService.GetProvidersWithWorkshops(updatedAfter, offsetFilter);

            return this.SearchResultToOkOrNoContent(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }
}