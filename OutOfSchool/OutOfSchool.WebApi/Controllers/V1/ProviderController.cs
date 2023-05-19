using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using OutOfSchool.WebApi.Common;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.Providers;

namespace OutOfSchool.WebApi.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]/[action]")]
public class ProviderController : ControllerBase
{
    private readonly IProviderService providerService;
    private readonly IStringLocalizer<SharedResource> localizer;
    private readonly ILogger<ProviderController> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProviderController"/> class.
    /// </summary>
    /// <param name="providerService">Service for Provider model.</param>
    /// <param name="localizer">Localizer.</param>
    /// <param name="logger"><see cref="Microsoft.Extensions.Logging.ILogger{T}"/> object.</param>
    public ProviderController(
        IProviderService providerService,
        IStringLocalizer<SharedResource> localizer,
        ILogger<ProviderController> logger)
    {
        this.localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
        this.providerService = providerService ?? throw new ArgumentNullException(nameof(providerService));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
    public async Task<IActionResult> Get([FromQuery] ProviderFilter filter)
    {
        var providers = await providerService.GetByFilter(filter).ConfigureAwait(false);

        if (providers.TotalAmount < 1)
        {
            return NoContent();
        }

        return Ok(providers);
    }

    /// <summary>
    /// Get all Providers from the database.
    /// </summary>
    /// <param name="filter">Filter to get a part of all providers that were found.</param>
    /// <returns>The result is a <see cref="SearchResult{ProviderDto}"/> that contains the count of all found providers and a list of providers that were received.</returns>
    [HasPermission(Permissions.ProviderRead)]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SearchResult<ProviderDto>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetByFilter([FromQuery] ProviderFilter filter)
    {
        var providers = await providerService.GetByFilter(filter).ConfigureAwait(false);

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
        var provider = await providerService.GetById(providerId).ConfigureAwait(false);
        if (provider == null)
        {
            return NotFound($"There is no Provider in DB with {nameof(provider.Id)} - {providerId}");
        }

        return Ok(provider);
    }

    /// <summary>
    /// To Get the Profile of authorized Provider.
    /// </summary>
    /// <returns>Authorized provider's profile.</returns>
    [HasPermission(Permissions.ProviderRead)]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ProviderDto))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetProfile()
    {
        // TODO: localize messages from the conrollers.
        var userId = GettingUserProperties.GetUserId(User);
        var isDeputyOrAdmin = !string.IsNullOrEmpty(GettingUserProperties.GetUserSubrole(User)) &&
                              GettingUserProperties.GetUserSubrole(User) != "None";
        if (userId == null)
        {
            BadRequest("Invalid user information.");
        }

        var provider = await providerService.GetByUserId(userId, isDeputyOrAdmin).ConfigureAwait(false);
        if (provider == null)
        {
            return NoContent();
        }

        return Ok(provider);
    }

    /// <summary>
    /// Method for creating new Provider.
    /// </summary>
    /// <param name="providerModel">Entity to add.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    [HasPermission(Permissions.ProviderAddNew)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [HttpPost]
    public async Task<IActionResult> Create(ProviderDto providerModel)
    {
        if (providerModel == null)
        {
            throw new ArgumentNullException(nameof(providerModel));
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        providerModel.Id = default;
        providerModel.LegalAddress.Id = default;

        if (providerModel.ActualAddress != null)
        {
            providerModel.ActualAddress.Id = default;
        }

        // TODO: find out if we need this field in the model
        providerModel.UserId = GettingUserProperties.GetUserId(User);

        try
        {
            var createdProvider = await providerService.Create(providerModel).ConfigureAwait(false);

            return CreatedAtAction(
                nameof(GetById),
                new { providerId = createdProvider.Id, },
                createdProvider);
        }
        catch (InvalidOperationException ex)
        {
            var errorMessage = $"Unable to create a new provider: {ex.Message}";
            logger.LogError(ex, errorMessage);

            // TODO: think about filtering of exception message.
            return BadRequest(errorMessage);
        }
    }

    /// <summary>
    /// Update info about the Provider.
    /// </summary>
    /// <param name="providerModel">Entity to update.</param>
    /// <returns>Updated Provider.</returns>
    [HasPermission(Permissions.ProviderEdit)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ProviderDto))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPut]
    public async Task<IActionResult> Update(ProviderUpdateDto providerModel)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var userId = GettingUserProperties.GetUserId(User);
            var provider = await providerService.Update(providerModel, userId).ConfigureAwait(false);

            if (provider == null)
            {
                return BadRequest("Can't change Provider with such parameters.\n" +
                                  "Please check that information are valid.");
            }

            return Ok(provider);
        }
        catch (DbUpdateConcurrencyException e)
        {
            return BadRequest(e);
        }
    }

    /// <summary>
    /// Block/unblock Provider.
    /// </summary>
    /// <param name="providerBlockDto">Entity to update.</param>
    /// <returns>Block Provider.</returns>
    [HasPermission(Permissions.ProviderEdit)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ProviderBlockDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPut]
    public async Task<ActionResult> Block([FromBody] ProviderBlockDto providerBlockDto)
    {
        var result = await providerService.Block(providerBlockDto);

        if (result is null)
        {
            return NotFound($"There is no Provider in DB with Id - {providerBlockDto.Id}");
        }

        return Ok(result);
    }

    /// <summary>
    /// Delete a specific Provider from the database.
    /// </summary>
    /// <param name="uid">Provider's key.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    [HasPermission(Permissions.ProviderRemove)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpDelete("{uid:guid}")]
    public async Task<IActionResult> Delete(Guid uid)
    {
        try
        {
            await providerService.Delete(uid).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            if (ex is ArgumentException || ex is ArgumentNullException)
            {
                return BadRequest(ex.Message);
            }

            throw;
        }

        return NoContent();
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
        var result = await providerService.UpdateStatus(request, GettingUserProperties.GetUserId(User))
            .ConfigureAwait(false);

        if (result is null)
        {
            return NotFound($"There is no Provider in DB with Id - {request.ProviderId}");
        }

        return Ok(result);
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
            var result = await providerService.UpdateLicenseStatus(request, GettingUserProperties.GetUserId(User))
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
    /// <summary>
    /// Get Providers that match filter's parameters.
    /// </summary>
    /// <param name="filter">Entity that represents searching parameters.</param>
    /// <returns><see cref="SearchResult{ProviderStatusDto}"/>, or no content.</returns>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SearchResult<ProviderStatusDto>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet]
    [HasPermission(Permissions.ProviderRead)]
    public async Task<IActionResult> GetProviderStatusById(Guid providerId)
    {
        var provider = await providerService.GetProviderStatusById(providerId).ConfigureAwait(false);
        if (provider == null)
        {
            return NotFound($"There is no Provider in DB with {nameof(provider.ProviderId)} - {providerId}");
        }

        return Ok(provider);
    }
}