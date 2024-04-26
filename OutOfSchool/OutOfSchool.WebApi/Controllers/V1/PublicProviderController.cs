using Microsoft.AspNetCore.Mvc;
using OutOfSchool.WebApi.Common;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.Providers;
using OutOfSchool.WebApi.Services.ProviderServices;

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
    /// Get Providers that match filter's parameters.
    /// </summary>
    /// <param name="filter">Entity that represents searching parameters.</param>
    /// <returns><see cref="SearchResult{ProviderDto}"/>, or no content.</returns>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SearchResult<ProviderDto>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Get([FromQuery] PublicProviderFilter filter)
    {
        var providers = await publicProviderService.GetByFilter(filter).ConfigureAwait(false);

        if (providers.TotalAmount < 1)
        {
            return NoContent();
        }

        return Ok(providers);
    }

    /// <summary>
    /// Get Provider by it's Id.
    /// </summary>
    /// <param name="providerId">Provider's id.</param>
    /// <returns>Provider.</returns>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ProviderDto))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet("{providerId:Guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid providerId)
    {
        var provider = await publicProviderService.GetById(providerId).ConfigureAwait(false);
        if (provider == null)
        {
            return NotFound($"There is no Provider in DB with {nameof(provider.Id)} - {providerId}");
        }

        return Ok(provider);
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
