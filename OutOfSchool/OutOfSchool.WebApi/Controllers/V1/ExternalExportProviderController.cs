using Microsoft.AspNetCore.Mvc;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.ProvidersInfo;

namespace OutOfSchool.WebApi.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
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
    /// <param name="sizeFilter">The size of the page for paginated results.</param>
    /// <returns><see cref="SearchResult{InfoProviderBaseDto}"/>, or no content.</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SearchResult<ProviderInfoBaseDto>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Route("export")]
    public async Task<ActionResult<SearchResult<ProviderInfoBaseDto>>> GetByFilter([FromQuery] DateTime updatedAfter, [FromQuery] SizeFilter sizeFilter)
    {
        try
        {
            var result = await externalProviderService.GetProvidersWithWorkshops(updatedAfter, sizeFilter);

            if (result.Entities.Any())
            {
                return Ok(result);
            }
            else
            {
                return NoContent();
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }
}