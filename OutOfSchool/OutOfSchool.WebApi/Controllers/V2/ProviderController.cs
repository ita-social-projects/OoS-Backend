using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;
using OutOfSchool.WebApi.Common;
using OutOfSchool.WebApi.Enums;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.Providers;

namespace OutOfSchool.WebApi.Controllers.V2;

[ApiController]
[FeatureGate(nameof(Feature.Images))]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/[controller]/[action]")]
public class ProviderController : ControllerBase
{
    private readonly IProviderServiceV2 providerService;
    private readonly ILogger<ProviderController> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProviderController"/> class.
    /// </summary>
    /// <param name="providerService">Service for Provider model.</param>
    /// <param name="logger"><see cref="Microsoft.Extensions.Logging.ILogger{T}"/> Logger.</param>
    public ProviderController(
        IProviderServiceV2 providerService,
        ILogger<ProviderController> logger)
    {
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
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Create([FromForm] ProviderCreateDto providerModel)
    {
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
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Update([FromForm] ProviderUpdateDto providerModel)
    {
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
}