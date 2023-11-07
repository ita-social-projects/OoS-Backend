using Microsoft.AspNetCore.Mvc;
using OutOfSchool.WebApi.Common;
using OutOfSchool.WebApi.Models.Providers;

namespace OutOfSchool.WebApi.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]/[action]")]
public class PrivateProviderController : Controller
{
    private readonly IPrivateProviderService privateProviderService;

    /// <summary>
    ///  Initializes a new instance of the <see cref="PrivateProviderController"/> class.
    /// </summary>
    /// <param name="privateProviderService"></param>
    public PrivateProviderController(IPrivateProviderService privateProviderService)
    {
        this.privateProviderService = privateProviderService;
    }

    /// <summary>
    /// Update Provider license status.
    /// </summary>
    /// <param name="request">Provider ID and license status to update.</param>
    /// <returns><see cref="ProviderLicenseStatusDto"/>.</returns>
    [HttpPut]
    [HasPermission(Permissions.ProviderApprove)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ProviderLicenseStatusDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> LicenseStatusUpdate([FromBody] ProviderLicenseStatusDto request)
    {
        try
        {
            var result = await privateProviderService.UpdateLicenseStatus(request, GettingUserProperties.GetUserId(User))
                .ConfigureAwait(false);

            if (result is null)
            {
                return NotFound($"There is no Provider in DB with Id - {request.ProviderId}");
            }

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
