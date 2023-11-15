using Microsoft.AspNetCore.Mvc;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]/[action]")]
public class ProviderWorkshopController : ControllerBase
{
    private readonly IProviderWorkshopService externalProviderService;

    public ProviderWorkshopController(IProviderWorkshopService externalProviderService)
    {
        this.externalProviderService = externalProviderService;
    }

    [HttpGet]
    public async Task<ActionResult<SearchResult<ProviderWorkshopDto>>> GetByFilter([FromQuery] DateTime updatedAfter, [FromQuery] int pageSize = 100)
    {
        try
        {
            var result = await externalProviderService.GetProvidersWithWorkshops(updatedAfter, pageSize);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }

}
