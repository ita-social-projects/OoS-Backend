using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement.Mvc;
using OutOfSchool.BusinessLogic.Common;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Workshops;
using OutOfSchool.BusinessLogic.Services.ProviderServices;
using OutOfSchool.Services.Enums;
using OutOfSchool.WebApi.Enums;
using OutOfSchool.WebApi.Util.ControllersResultsHelpers;

namespace OutOfSchool.WebApi.Controllers.V2;

/// <summary>
/// Controller with CRUD operations for Workshop entity.
/// </summary>
[ApiController]
[FeatureGate(nameof(Feature.Images))]
[AspApiVersion(2)]
[Route("api/v{version:apiVersion}/[controller]/[action]")]
public class WorkshopController : ControllerBase
{
    private readonly IWorkshopServicesCombinerV2 combinedWorkshopService;
    private readonly IProviderService providerService;
    private readonly IStringLocalizer<SharedResource> localizer;
    private readonly ILogger<WorkshopController> logger;
    private readonly IEmployeeService employeeService;
    private readonly IUserService userService;

    private readonly AppDefaultsConfig options;

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkshopController"/> class.
    /// </summary>
    /// <param name="combinedWorkshopService">Service for operations with Workshops.</param>
    /// <param name="providerService">Service for Provider model.</param>
    /// <param name="localizer">Localizer.</param>
    /// <param name="logger"><see cref="Microsoft.Extensions.Logging.ILogger{T}"/> object.</param>
    /// <param name="employeeService">Service for ProviderAdmin model.</param>
    /// <param name="userService">Service for operations with users.</param>
    /// <param name="options">Application default values.</param>
    public WorkshopController(
        IWorkshopServicesCombinerV2 combinedWorkshopService,
        IProviderService providerService,
        IStringLocalizer<SharedResource> localizer,
        ILogger<WorkshopController> logger,
        IEmployeeService employeeService,
        IUserService userService,
        IOptions<AppDefaultsConfig> options)
    {
        this.localizer = localizer;
        this.combinedWorkshopService = combinedWorkshopService;
        this.providerService = providerService;
        this.logger = logger;
        this.employeeService = employeeService;
        this.userService = userService;
        this.options = options.Value;
    }

    /// <summary>
    /// Get workshop by it's id.
    /// </summary>
    /// <param name="id">Workshop's id.</param>
    /// <returns><see cref="WorkshopV2Dto"/>, or no content.</returns>
    /// <response code="200">The entity was found by given Id.</response>
    /// <response code="204">No entity with given Id was found.</response>
    /// <response code="500">If any server error occures. For example: Id was less than one.</response>
    [AllowAnonymous]
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(WorkshopV2Dto))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var workshop = await combinedWorkshopService.GetById(id).ConfigureAwait(false);

        if (workshop is null)
        {
            return NotFound();
        }

        return Ok(workshop);
    }

    /// <summary>
    /// Get workshop cards by Provider's Id.
    /// </summary>
    /// <param name="id">Provider's id.</param>
    /// <param name="filter">Filter to get specified portion of workshops for specified provider. Ids of the excluded workshops could be specified.</param>
    /// <returns><see cref="SearchResult{WorkshopProviderViewCard}"/>, or no content.</returns>
    /// <response code="200">The list of found entities by given Id.</response>
    /// <response code="204">No entity with given Id was found.</response>
    /// <response code="500">If any server error occures. For example: Id was less than one.</response>
    [AllowAnonymous]
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SearchResult<WorkshopProviderViewCard>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetByProviderId(Guid id, [FromQuery] ExcludeIdFilter filter)
    {
        var workshopCards = await combinedWorkshopService.GetByProviderId(id, filter).ConfigureAwait(false);

        return this.SearchResultToOkOrNoContent(workshopCards);
    }

    /// <summary>
    /// Get workshops that matches filter's parameters.
    /// </summary>
    /// <param name="filter">Entity that represents searching parameters.</param>
    /// <param name="isAdmins">True, if needs to retrieve information from admin panel.</param>
    /// <returns><see cref="SearchResult{WorkshopCard}"/>, or no content.</returns>
    /// <response code="200">The list of found entities by given filter.</response>
    /// <response code="204">No entity with given filter was found.</response>
    /// <response code="500">If any server error occures. For example: Id was less than one.</response>
    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SearchResult<WorkshopCard>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetByFilter([FromQuery] WorkshopFilter filter, bool isAdmins = false)
    {
        if (string.IsNullOrWhiteSpace(filter.City))
        {
            filter.City = options.City;
        }

        SearchResult<WorkshopCard> result;

        if (isAdmins)
        {
            result = await combinedWorkshopService.GetByFilterForAdmins(filter).ConfigureAwait(false);
        }
        else
        {
            result = await combinedWorkshopService.GetByFilter(filter).ConfigureAwait(false);
        }

        return this.SearchResultToOkOrNoContent(result);
    }

    /// <summary>
    /// Add new workshop to the database.
    /// </summary>
    /// <param name="dto">Entity to add.</param>
    /// <returns>Created <see cref="WorkshopV2Dto"/>.</returns>
    /// <response code="201">Entity was created and returned with Id.</response>
    /// <response code="400">If the model is invalid, some properties are not set etc.</response>
    /// <response code="401">If the user is not authorized.</response>
    /// <response code="403">If the user has no rights to use this method, or sets some properties that are forbidden.</response>
    /// <response code="413">If the request break the limits, set in configs.</response>
    /// <response code="500">If any server error occures.</response>
    [HasPermission(Permissions.WorkshopAddNew)]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(WorkshopResponseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status413PayloadTooLarge)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Create([FromForm] WorkshopV2CreateRequestDto dto)
    {
        if (dto == null)
        {
            return BadRequest("Workshop is null.");
        }

        if (await IsProviderBlocked(dto.ProviderId).ConfigureAwait(false))
        {
            return StatusCode(403, "Forbidden to create workshops at blocked providers");
        }

        if (await IsCurrentUserBlocked())
        {
            return StatusCode(403, "Forbidden to create the workshop by the blocked provider.");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userHasRights = await this.IsUserProvidersOwnerOrAdmin(dto.ProviderId).ConfigureAwait(false);
        if (!userHasRights)
        {
            return StatusCode(403, "Forbidden to create workshops for another providers.");
        }

        try
        {
            var creationResult = await combinedWorkshopService.Create(dto).ConfigureAwait(false);

            // here we will get "false" if workshop was created by assistant provider admin
            // because user is not currently associated with new workshop
            // so we can update information to allow assistant manage created workshop
            if (!await IsUserProvidersOwnerOrAdmin(creationResult.Workshop.ProviderId, creationResult.Workshop.Id).ConfigureAwait(false))
            {
                var userId = User.FindFirst("sub")?.Value;
                await employeeService.GiveEmployeeAccessToWorkshop(userId, creationResult.Workshop.Id).ConfigureAwait(false);
            }

            return CreatedAtAction(
            nameof(GetById),
            new { id = creationResult.Workshop.Id, },
            new WorkshopResponseDto
            {
                Workshop = creationResult.Workshop,
                UploadingCoverImageResult = creationResult.UploadingCoverImageResult?.CreateSingleUploadingResult(),
                UploadingImagesResults = creationResult.UploadingImagesResults?.CreateMultipleUploadingResult(),
            });
        }
        catch (InvalidOperationException ex)
        {
            var errorMessage = $"Unable to create a new workshop: {ex.Message}";
            logger.LogError(ex, errorMessage);

            return BadRequest(errorMessage);
        }
    }

    /// <summary>
    /// Update info about workshop entity.
    /// </summary>
    /// <param name="dto">Workshop to update.</param>
    /// <returns>Updated <see cref="WorkshopV2Dto"/>.</returns>
    /// <response code="200">Entity was updated and returned.</response>
    /// <response code="400">If the model is invalid, some properties are not set etc.</response>
    /// <response code="401">If the user is not authorized.</response>
    /// <response code="403">If the user has no rights to use this method, or sets some properties that are forbidden to change.</response>
    /// <response code="413">If the request break the limits, set in configs.</response>
    /// <response code="500">If any server error occures.</response>
    [HasPermission(Permissions.WorkshopEdit)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(WorkshopResponseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status413PayloadTooLarge)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPut]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Update([FromForm] WorkshopV2Dto dto)
    {
        var userHasRights = await IsUserProvidersOwner(dto.ProviderId).ConfigureAwait(false);
        if (!userHasRights)
        {
            return StatusCode(StatusCodes.Status403Forbidden, "Forbidden to update workshops for another providers.");
        }

        try
        {
            var updatingResult = await combinedWorkshopService.Update(dto).ConfigureAwait(false);

            if (!updatingResult.Succeeded)
            {
                return BadRequest(updatingResult.OperationResult.Errors.FirstOrDefault()?.Description
                    ?? Constants.UnknownErrorDuringUpdateMessage);
            }

            return Ok(CreateUpdateResponse(updatingResult.Value));
        }
        catch (ArgumentException e)
        {
            return BadRequest(e.Message);
        }
    }

    /// <summary>
    /// Delete a specific workshop from the database.
    /// </summary>
    /// <param name="id">Workshop's id.</param>
    /// <returns>StatusCode representing the task completion.</returns>
    /// <response code="204">If the entity was successfully deleted, or if the entity was not found by given Id.</response>
    /// <response code="401">If the user is not authorized.</response>
    /// <response code="403">If the user has no rights to use this method, or deletes not own workshop.</response>
    /// <response code="500">If any server error occures.</response>
    [HasPermission(Permissions.WorkshopRemove)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var workshop = await combinedWorkshopService.GetById(id).ConfigureAwait(false);

        if (workshop is null)
        {
            return NoContent();
        }

        var userHasRights = await this.IsUserProvidersOwner(workshop.ProviderId).ConfigureAwait(false);
        if (!userHasRights)
        {
            return new ForbidResult("Forbidden to delete workshops of another providers.");
        }

        await combinedWorkshopService.Delete(id).ConfigureAwait(false);
        return NoContent();
    }

    private async Task<bool> IsUserProvidersOwner(Guid providerId)
    {
        // Provider can create/update/delete a workshop only with it's own ProviderId.
        // Admin can create a work without checks.
        if (User.IsInRole("provider"))
        {
            var userId = User.FindFirst("sub")?.Value;
            var provider = await providerService.GetByUserId(userId).ConfigureAwait(false);

            if (providerId != provider?.Id)
            {
                return false;
            }
        }

        return true;
    }

    private WorkshopResponseDto CreateUpdateResponse(WorkshopResultDto updatingResult)
    {
        return new WorkshopResponseDto
        {
            Workshop = updatingResult.Workshop,
            UploadingCoverImageResult = updatingResult.UploadingCoverImageResult?.CreateSingleUploadingResult(),
            UploadingImagesResults = updatingResult.UploadingImagesResults?.CreateMultipleUploadingResult(),
        };
    }

    private async Task<bool> IsUserProvidersOwnerOrAdmin(Guid providerId, Guid workshopId = default)
    {
        if (User.IsInRole(nameof(Role.Provider).ToLower()))
        {
            var userId = GettingUserProperties.GetUserId(User);
            var provider = await providerService.GetByUserId(userId).ConfigureAwait(false);

            if (provider != null)
            {
                if (provider.Id != providerId)
                {
                    return false;
                }
            }
            else
            {
                var isUserRelatedAdmin = await employeeService
                    .CheckUserIsRelatedEmployee(userId, providerId, workshopId)
                    .ConfigureAwait(false);

                if (!isUserRelatedAdmin)
                {
                    return false;
                }
            }

            return true;
        }

        return false;
    }

    private async Task<bool> IsCurrentUserBlocked()
    {
        var userId = GettingUserProperties.GetUserId(User);

        return await userService.IsBlocked(userId);
    }

    private async Task<bool> IsProviderBlocked(Guid providerId, Guid workshopId = default)
    {
        providerId = providerId == Guid.Empty ?
            await providerService.GetProviderIdForWorkshopById(workshopId).ConfigureAwait(false) :
            providerId;

        return await providerService.IsBlocked(providerId).ConfigureAwait(false) ?? false;
    }
}