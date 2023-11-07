using Microsoft.AspNetCore.Mvc;
using OutOfSchool.WebApi.Common;
using OutOfSchool.WebApi.Models.Providers;

namespace OutOfSchool.WebApi.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]/[action]")]
public class PublicProviderController : ControllerBase
{
    private readonly IPublicProviderService publicProviderService;

    /// <summary>
    /// Initializes a new instance of the <see cref="PublicProviderController"/> class.
    /// </summary>
    /// <param name="publicProviderService"></param>
    public PublicProviderController(IPublicProviderService publicProviderService)
    {
        this.publicProviderService = publicProviderService;
    }

    /// <summary>
    /// Update Provider status.
    /// </summary>
    /// <param name="request">Provider ID and status to update.</param>
    /// <returns><see cref="ProviderStatusDto"/>.</returns>
    [HttpPut]
    [HasPermission(Permissions.ProviderApprove)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ProviderStatusDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> StatusUpdate([FromBody] ProviderStatusDto request)
    {
        var result = await publicProviderService.UpdateStatus(request, GettingUserProperties.GetUserId(User))
            .ConfigureAwait(false);

        if (result is null)
        {
            return NotFound($"There is no Provider in DB with Id - {request.ProviderId}");
        }

        return Ok(result);
    }
}
