using System.Net.Mime;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using OutOfSchool.BusinessLogic.Common;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Providers;
using OutOfSchool.BusinessLogic.Services.ProviderServices;

namespace OutOfSchool.WebApi.Controllers.V1;

[ApiController]
[AspApiVersion(1)]
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
        var isEmployee = !string.IsNullOrEmpty(GettingUserProperties.GetUserRole(User)) &&
                              GettingUserProperties.GetUserRole(User) == "Employee";
        if (userId == null)
        {
            BadRequest("Invalid user information.");
        }

        var provider = await providerService.GetByUserId(userId, isEmployee).ConfigureAwait(false);
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
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ProviderCreateDto providerModel)
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
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ProviderDto))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPut]
    public async Task<IActionResult> Update([FromBody] ProviderUpdateDto providerModel)
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
        var result = await providerService.Delete(
            uid,
            await HttpContext.GetTokenAsync("access_token").ConfigureAwait(false))
            .ConfigureAwait(false);

        return result.Match<ActionResult>(
            error => StatusCode((int)error.HttpStatusCode, error.Message),
            result =>
            {
                logger.LogInformation("Successfully deleted Provider with id: {uid}", uid);
                return Ok();
            });
    }

    /// <summary>
    /// Get Provider status by providerId.
    /// </summary>
    /// <param name="providerId">Id of provider.</param>
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