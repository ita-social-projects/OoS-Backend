using Microsoft.AspNetCore.Mvc;
using OutOfSchool.WebApi.Common;
using OutOfSchool.WebApi.Common.Responses;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.Providers;

namespace OutOfSchool.WebApi.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]/[action]")]
public class ProviderController : ControllerBase
{
    private readonly IProviderService providerService;
    private readonly ILogger<ProviderController> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProviderController"/> class.
    /// </summary>
    /// <param name="providerService">Service for Provider model.</param>
    /// <param name="logger"><see cref="Microsoft.Extensions.Logging.ILogger{T}"/> object.</param>
    public ProviderController(
        IProviderService providerService,
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
    [ProducesResponseType(StatusCodes.Status204NoContent, Type = typeof(ApiErrorResponse))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiErrorResponse))]
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Get([FromQuery] ProviderFilter filter)
    {
        var providers = await providerService.GetByFilter(filter);

        return providers.TotalAmount < 1 ?
            NoContent() :
            Ok(providers);
    }

    /// <summary>
    /// Get all Providers from the database.
    /// </summary>
    /// <param name="filter">Filter to get a part of all providers that were found.</param>
    /// <returns>The result is a <see cref="SearchResult{ProviderDto}"/> that contains the count of all found providers and a list of providers that were received.</returns>
    [AllowAnonymous]
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
        var provider = await providerService.GetById(providerId);

        return provider is null ?
            NotFound(ProviderApiError.ProviderNotFound.CreateApiErrorResponse()) :
            Ok(provider);
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
        var userId = GettingUserProperties.GetUserId(User);
        var isDeputyOrAdmin = !string.IsNullOrEmpty(GettingUserProperties.GetUserSubrole(User)) &&
                              GettingUserProperties.GetUserSubrole(User) != "None";
        if (userId is null)
        {
            return BadRequest(UserApiError.InvalidUserInformation.CreateApiErrorResponse());
        }

        var provider = await providerService.GetByUserId(userId, isDeputyOrAdmin);

        return provider is null ?
            NoContent() :
            Ok(provider);
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
            var createdProvider = await providerService.Create(providerModel);

            return CreatedAtAction(
                nameof(GetById),
                new { providerId = createdProvider.Id, },
                createdProvider);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError(ex, "Unable to create a new provider");

            return BadRequest(ProviderApiError.ProviderNotCreated.CreateApiErrorResponse());
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
    public async Task<IActionResult> Update(ProviderDto providerModel)
    {
        try
        {
            var userId = GettingUserProperties.GetUserId(User);
            var provider = await providerService.Update(providerModel, userId);

            return provider is null ?
                BadRequest(ProviderApiError.ProviderNotUpdated.CreateApiErrorResponse()) :
                Ok(provider);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.LogError(ex, "Unable to update a provider");

            return BadRequest(ProviderApiError.ProviderNotUpdated.CreateApiErrorResponse());
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
        try
        {
            await providerService.Delete(uid);
        }
        catch (ArgumentException ex)
        {
            logger.LogError(ex, "Unable to delete a provider");
            return BadRequest(ProviderApiError.ProviderNotDeleted.CreateApiErrorResponse());
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
        var result = await providerService.UpdateStatus(request, GettingUserProperties.GetUserId(User));

        return result is null ?
            NotFound(ProviderApiError.ProviderNotFound.CreateApiErrorResponse()) :
            Ok(result);
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
            var result = await providerService.UpdateLicenseStatus(request, GettingUserProperties.GetUserId(User));

            return result is null ?
                NotFound(ProviderApiError.ProviderNotFound.CreateApiErrorResponse()) :
                Ok(result);
        }
        catch (ArgumentException ex)
        {
            logger.LogError(ex, "Update license failed");
            return BadRequest(ProviderApiError.ProviderNotFound.CreateApiErrorResponse());
        }
    }
}