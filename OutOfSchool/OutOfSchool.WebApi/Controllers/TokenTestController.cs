using Microsoft.AspNetCore.Mvc;
using OutOfSchool.SportsRegistryApiClient.Interfaces;
using System.Reflection;
using OutOfSchool.SportsRegistryApiClient.Models.Requests;
using OutOfSchool.SportsRegistryApiClient.Models.Responses;

namespace OutOfSchool.WebApi.Controllers;

[ApiController]
[Route("api/test/token")]
public class TokenTestController : ControllerBase
{
    private readonly ISportsRegistryApiService api;

    public TokenTestController(ISportsRegistryApiService api)
    {
        this.api = api;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        try
        {
            var method = api.GetType()
                .GetMethod("GetAccessTokenAsync", BindingFlags.Instance | BindingFlags.NonPublic);

            if (method == null)
            {
                return NotFound("GetAccessTokenAsync not found");
            }

            var resultTask = method.Invoke(api, null) as Task<string>;

            if (resultTask == null)
            {
                return BadRequest("Method did not return Task<string>");
            }

            var token = await resultTask;

            return Ok(token);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error: {ex.Message}");
        }
    }
    
    [HttpPost("create-section")]
    public async Task<ActionResult<SectionCreateResponse>> CreateSection([FromBody] SportsSectionPostRequest request)
    {
        if (request == null)
        {
            return BadRequest("Request body is empty or malformed.");
        }

        try
        {
            var result = await api.CreateSectionAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Failed to send section to Sports Registry: {ex.Message}");
        }
    }
}