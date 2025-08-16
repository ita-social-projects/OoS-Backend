using Microsoft.AspNetCore.Mvc;
using OutOfSchool.SportsRegistryApiClient.Interfaces;
using OutOfSchool.SportsRegistryApiClient.Models.Requests;
using OutOfSchool.SportsRegistryApiClient.Models.Responses;

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
    [ProducesResponseType(typeof(SectionCreateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateSection([FromBody] SportsSectionPostRequest request, CancellationToken ct)
    {
        if (request is null)
        {
            return BadRequest(new { message = "Request body is empty or malformed." });
        }

        var result = await provider.RegisterSectionAsync(request).ConfigureAwait(false);

        return result.Match<IActionResult>(
            error => StatusCode(
                (int)error.HttpStatusCode,
                new
                {
                    message = string.IsNullOrWhiteSpace(error.Message) 
                        ? "Failed to register section in Sports Registry." 
                        : error.Message,
                }),
            success => Ok(success));
    }
}