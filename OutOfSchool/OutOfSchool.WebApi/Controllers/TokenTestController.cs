using Microsoft.AspNetCore.Mvc;
using OutOfSchool.SportsRegistryApiClient.Interfaces;
using OutOfSchool.SportsRegistryApiClient.Models.Requests;

namespace OutOfSchool.WebApi.Controllers;

[ApiController]
[Route("api/test/token")]
public class TokenTestController : ControllerBase
{
    private readonly ISportsRegistryProviderService provider;

    public TokenTestController(ISportsRegistryProviderService provider)
    {
        this.provider = provider;
    }

    [HttpPost("create-section")]
    public async Task<IActionResult> CreateSection([FromBody] SportsSectionPostRequest request)
    {
        if (request == null)
        {
            return BadRequest("Request body is empty or malformed.");
        }

        var result = await provider.RegisterSectionAsync(request);

        return result.Match<IActionResult>(
            error => StatusCode((int)error.HttpStatusCode, new
            {
                error.Message,
                error.Content,
                error.ApiErrorResponse
            }),
            success => Ok(success));
    }
}