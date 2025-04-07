using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement.Mvc;
using OutOfSchool.BusinessLogic.Common;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.WorkshopDraft;
using OutOfSchool.BusinessLogic.Models.Workshops;
using OutOfSchool.BusinessLogic.Services.ProviderServices;
using OutOfSchool.BusinessLogic.Services.WorkshopDrafts;
using OutOfSchool.Common.Models;
using OutOfSchool.WebApi.Enums;

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
    private readonly ILogger<WorkshopController> logger;
    private readonly ICurrentUserService currentUserService;
    private readonly IUserService userService;
    private readonly IWorkshopDraftService workshopDraftService;

    private readonly AppDefaultsConfig options;

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkshopController"/> class.
    /// </summary>
    /// <param name="combinedWorkshopService">Service for operations with Workshops.</param>
    /// <param name="providerService">Service for Provider model.</param>
    /// <param name="logger"><see cref="Microsoft.Extensions.Logging.ILogger{T}"/> object.</param>
    /// <param name="currentUserService">Service for getting current user info.</param>
    /// <param name="userService">Service for operations with users.</param>
    /// <param name="workshopDraftService">Service for operations with workshop drafts.</param>
    /// <param name="options">Application default values.</param>
    public WorkshopController(
        IWorkshopServicesCombinerV2 combinedWorkshopService,
        IProviderService providerService,
        ILogger<WorkshopController> logger,
        ICurrentUserService currentUserService,
        IUserService userService,
        IWorkshopDraftService workshopDraftService,
        IOptions<AppDefaultsConfig> options)
    {
        this.combinedWorkshopService = combinedWorkshopService;        
        this.providerService = providerService;
        this.logger = logger;
        this.currentUserService = currentUserService;
        this.userService = userService;
        this.workshopDraftService = workshopDraftService;
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
    public async Task<IActionResult> GetByProviderId(Guid id, [FromQuery] WorkshopFilterTitle filter)
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
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(WorkshopDraftResultDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status413PayloadTooLarge)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Create([FromForm] WorkshopV2Dto dto)
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
        
        var creationResult = await workshopDraftService.Create(dto).ConfigureAwait(false);

        // TODO: We don`t need it right now.
        // here we will get "false" if workshop was created by assistant provider admin
        // because user is not currently associated with new workshop
        // so we can update information to allow assistant manage created workshop
        //if (!await IsUserProvidersOwnerOrAdmin(creationResult.WorkshopDraft.WorkshopDetails.ProviderId, creationResult.WorkshopDraft.WorkshopDetails.Id)
        //        .ConfigureAwait(false))
        //{
        //    var userId = User.FindFirst("sub")?.Value;
        //    await employeeService.GiveEmployeeAccessToWorkshop(userId, creationResult.Workshop.Id).ConfigureAwait(false);
        //}

        return CreatedAtAction(
                nameof(WorkshopDraftController.Get),
                nameof(WorkshopDraftController).Replace("Controller", ""),
                new { id = creationResult.WorkshopDraft.WorkshopDraftId },
                creationResult);
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
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(WorkshopV2Dto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status413PayloadTooLarge)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPut]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Update([FromForm] WorkshopV2Dto dto)
    {
        try
        {
            var updatingResult = await workshopDraftService.UpdateWorkshop(dto).ConfigureAwait(false);

            return Ok(updatingResult);
        }
        catch (InvalidOperationException e)
        {
            return BadRequest(e.Message);
        }
    }

    /// <summary>
    /// Delete a specific workshop from the database.
    /// </summary>
    /// <param name="id">Workshop's id.</param>
    /// <returns>StatusCode representing the task completion.</returns>
    /// <response code="204">If the entity was successfully archived, or if the entity was not found by given Id.</response>
    /// <response code="401">If the user is not authorized.</response>
    /// <response code="403">If the user has no rights to use this method, or deletes not own workshop.</response>
    /// <response code="500">If any server error occures.</response>
    [HasPermission(Permissions.WorkshopRemove)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpDelete]
    [Route("~/api/v{version:apiVersion}/[controller]/{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var workshop = await combinedWorkshopService.GetById(id).ConfigureAwait(false);

        if (workshop is null)
        {
            return NoContent();
        }

        await currentUserService.UserHasRights(new ProviderRights(workshop.ProviderId), new EmployeeRights(workshop.ProviderId)).ConfigureAwait(false);

        await combinedWorkshopService.Delete(id).ConfigureAwait(false);
        return NoContent();
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