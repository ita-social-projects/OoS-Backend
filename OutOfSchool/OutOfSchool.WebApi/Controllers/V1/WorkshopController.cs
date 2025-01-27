using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OutOfSchool.BusinessLogic.Common;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Workshops;
using OutOfSchool.BusinessLogic.Services.ProviderServices;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.WebApi.Controllers.V1;

/// <summary>
/// Controller with CRUD operations for Workshop entity.
/// </summary>
[ApiController]
[AspApiVersion(1)]
[Route("api/v{version:apiVersion}/[controller]/[action]")]
public class WorkshopController : ControllerBase
{
    private readonly IWorkshopServicesCombiner combinedWorkshopService;
    private readonly IProviderService providerService;
    private readonly IEmployeeService employeeService;
    private readonly IUserService userService;
    private readonly IStringLocalizer<SharedResource> localizer;
    private readonly ILogger<WorkshopController> logger;
    private readonly AppDefaultsConfig options;

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkshopController"/> class.
    /// </summary>
    /// <param name="combinedWorkshopService">Service for operations with Workshops.</param>
    /// <param name="providerService">Service for Provider model.</param>
    /// <param name="employeeService">Service for Employee model.</param>
    /// <param name="userService">Service for operations with users.</param>
    /// <param name="localizer">Localizer.</param>
    /// <param name="logger"><see cref="Microsoft.Extensions.Logging.ILogger{T}"/> object.</param>
    /// <param name="options">Application default values.</param>
    public WorkshopController(
        IWorkshopServicesCombiner combinedWorkshopService,
        IProviderService providerService,
        IEmployeeService employeeService,
        IUserService userService,
        IStringLocalizer<SharedResource> localizer,
        ILogger<WorkshopController> logger,
        IOptions<AppDefaultsConfig> options)
    {
        this.localizer = localizer;
        this.combinedWorkshopService = combinedWorkshopService;
        this.employeeService = employeeService;
        this.providerService = providerService;
        this.userService = userService;
        this.logger = logger;
        this.options = options.Value;
    }

    /// <summary>
    /// Get workshop by it's id.
    /// </summary>
    /// <param name="id">Workshop's id.</param>
    /// <returns><see cref="WorkshopDto"/>, or no content.</returns>
    /// <response code="200">The entity was found by given Id.</response>
    /// <response code="204">No entity with given Id was found.</response>
    /// <response code="500">If any server error occures. For example: Id was less than one.</response>
    [AllowAnonymous]
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(WorkshopDto))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var workshop = await combinedWorkshopService.GetById(id).ConfigureAwait(false);

        if (workshop is null)
        {
            return NoContent();
        }

        return Ok(workshop);
    }

    /// <summary>
    /// Get all workshops (Id, Title) from the database by provider's id sorted by Title.
    /// </summary>
    /// <param name="providerId">Id of the provider.</param>
    /// <returns>The result is a <see cref="List{ShortEntityDto}"/> that contains a sorted by Title list of workshops that were received.</returns>
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<ShortEntityDto>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet("{providerId}")]
    public async Task<IActionResult> GetWorkshopListByProviderId(Guid providerId)
    {
        if (providerId == Guid.Empty)
        {
            return BadRequest("Provider id is empty.");
        }

        var workshops = await combinedWorkshopService.GetWorkshopListByProviderId(providerId).ConfigureAwait(false);

        if (!workshops.Any())
        {
            return NoContent();
        }

        return Ok(workshops);
    }

    /// <summary>
    /// Get all workshops (Id, Title) from the database by employee's id sorted by Title.
    /// </summary>
    /// <param name="employeeId">Id of the employee.</param>
    /// <returns>The result is a <see cref="List{ShortEntityDto}"/> that contains a sorted by Title list of workshops that were received.</returns>
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(List<ShortEntityDto>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet("{employeeId}")]
    public async Task<IActionResult> GetWorkshopListByEmployeeId(string employeeId)
    {
        if (employeeId == string.Empty)
        {
            return BadRequest("Emplyee id is empty.");
        }

        var workshops = await combinedWorkshopService.GetWorkshopListByEmployeeId(employeeId);

        if (!workshops.Any())
        {
            return NoContent();
        }

        return Ok(workshops);
    }

    // TODO: Check what these two methods do
    /// <summary>
    /// Get workshop cards by Provider's Id.
    /// </summary>
    /// <param name="id">Provider's id.</param>
    /// <param name="filter">Filter to get specified portion of workshop view cards for specified provider.
    /// Id of the excluded workshop could be specified.</param>
    /// <returns><see cref="SearchResult{WorkshopProviderViewCard}"/>, or no content.</returns>
    /// <response code="200">The list of found entities by given Id.</response>
    /// <response code="204">No entity with given Id was found.</response>
    /// <response code="400">Provider id is empty.</response>
    /// <response code="500">If any server error occures. For example: Id was less than one.</response>
    [AllowAnonymous]
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SearchResult<WorkshopProviderViewCard>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetByProviderId(Guid id, [FromQuery] ExcludeIdFilter filter)
    {
        if (id == Guid.Empty)
        {
            return BadRequest("Provider id is empty.");
        }

        var workshopCards = await combinedWorkshopService.GetByProviderId(id, filter)
            .ConfigureAwait(false);

        return this.SearchResultToOkOrNoContent(workshopCards);
    }

    /// <summary>
    /// Get a portion of workshop view cards for specified provider.
    /// </summary>
    /// <param name="id">Provider's Id.</param>
    /// <param name="filter">Filter to get specified portion of workshop view cards for specified provider, Id of the excluded workshop could be specified..</param>
    /// <returns><see cref="SearchResult{WorkshopProviderViewCard}"/>, or no content.</returns>
    [AllowAnonymous]
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SearchResult<WorkshopProviderViewCard>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetWorkshopProviderViewCardsByProviderId(Guid id, [FromQuery] ExcludeIdFilter filter)
    {
        if (id == Guid.Empty)
        {
            return BadRequest("Provider id is empty.");
        }

        var workshopProviderViewCards = await combinedWorkshopService.GetByProviderId(id, filter).ConfigureAwait(false);

        return this.SearchResultToOkOrNoContent(workshopProviderViewCards);
    }

    /// <summary>
    /// Gets the competitive selection description of the specified workshop.
    /// </summary>
    /// <param name="id">Id of the workshop to get the competitive selection description for.</param>
    /// <returns>A string description of the competitive selection for the workshop specified by id.</returns>
    [AllowAnonymous]
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(string))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]

    public async Task<IActionResult> GetCompetitiveSelectionDescription(Guid id)
    {
        if (id == Guid.Empty)
        {
            return BadRequest("Workshop id is empty.");
        }

        var workshop = await combinedWorkshopService.GetById(id).ConfigureAwait(false);

        if (workshop is null || string.IsNullOrEmpty(workshop.CompetitiveSelectionDescription))
        {
            return NoContent();
        }

        return Ok(workshop.CompetitiveSelectionDescription);
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
    /// <returns>Created <see cref="WorkshopDto"/>.</returns>
    /// <response code="201">Entity was created and returned with Id.</response>
    /// <response code="400">If the model is invalid, some properties are not set etc.</response>
    /// <response code="401">If the user is not authorized.</response>
    /// <response code="403">If the user has no rights to use this method, or sets some properties that are forbidden.</response>
    /// <response code="500">If any server error occures.</response>
    [HasPermission(Permissions.WorkshopAddNew)]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(WorkshopCreateUpdateDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] WorkshopCreateRequestDto dto)
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

        var userHasRights = await IsUserProvidersOwnerOrAdmin(dto.ProviderId).ConfigureAwait(false);
        if (!userHasRights)
        {
            return StatusCode(403, "Forbidden to create workshops for another providers.");
        }

        try
        {
            var workshop = await combinedWorkshopService.Create(dto).ConfigureAwait(false);


            // here we will get "false" if workshop was created by assistant provider admin
            // because user is not currently associated with new workshop
            // so we can update information to allow assistant manage created workshop
            if (!await IsUserProvidersOwnerOrAdmin(workshop.ProviderId, workshop.Id).ConfigureAwait(false))
            {
                var userId = User.FindFirst("sub")?.Value;
                await employeeService.GiveEmployeeAccessToWorkshop(userId, workshop.Id).ConfigureAwait(false);
            }

            return CreatedAtAction(
                nameof(GetById),
                new { id = workshop.Id, },
                workshop);
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
    /// <returns>Updated <see cref="WorkshopDto"/>.</returns>
    /// <response code="200">Entity was updated and returned.</response>
    /// <response code="400">If the model is invalid, some properties are not set etc.</response>
    /// <response code="401">If the user is not authorized.</response>
    /// <response code="403">If the user has no rights to use this method, or sets some properties that are forbidden to change.</response>
    /// <response code="500">If any server error occures.</response>
    [HasPermission(Permissions.WorkshopEdit)]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(WorkshopCreateUpdateDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPut]
    public async Task<IActionResult> Update([FromBody] WorkshopCreateUpdateDto dto)
    {
        if (dto == null)
        {
            return BadRequest("Workshop is null.");
        }

        if (await IsProviderBlocked(dto.ProviderId).ConfigureAwait(false))
        {
            return StatusCode(403, "Forbidden to update workshops at blocked providers");
        }

        if (await IsCurrentUserBlocked())
        {
            return StatusCode(403, "Forbidden to update the workshop by the blocked provider.");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userHasRights = await this.IsUserProvidersOwnerOrAdmin(dto.ProviderId, dto.Id).ConfigureAwait(false);

        if (!userHasRights)
        {
            return StatusCode(403, "Forbidden to update workshops, which are not related to you");
        }

        var result = await combinedWorkshopService.Update(dto).ConfigureAwait(false);

        if (!result.Succeeded)
        {
            return BadRequest(result.OperationResult.Errors.FirstOrDefault()?.Description
                ?? Constants.UnknownErrorDuringUpdateMessage);
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Update the Tags for Workshop entity.
    /// </summary>
    /// <param name="dto">Dto containing the Workshop Id and the Tag Ids to update.</param>
    /// <returns>Updated <see cref="Workshop"/>.</returns>
    /// <response code="200">Entity was updated and returned.</response>
    /// <response code="400">If the model is invalid, some properties are not set etc.</response>
    /// <response code="401">If the user is not authorized.</response>
    /// <response code="403">If the user has no rights to use this method, or sets some properties that are forbidden to change.</response>
    /// <response code="500">If any server error occures.</response>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Workshop))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPut]
    public async Task<IActionResult> UpdateTags([FromBody] WorkshopTagsUpdateDto dto)
    {
        if (dto == null)
        {
            return BadRequest("Invalid workshop data.");
        }

        var updatedWorkshop = await combinedWorkshopService.UpdateTags(dto);

        if (updatedWorkshop == null)
        {
            return NotFound($"Workshop with ID = {dto.WorkshopId} not found.");
        }

        return Ok(updatedWorkshop);
    }

    /// <summary>
    /// Update status field for workshop entity.
    /// </summary>
    /// <param name="request">Workshop id and status to update.</param>
    /// <returns>Updated <see cref="WorkshopStatusDto"/>.</returns>
    /// <response code="200">Entity was updated and returned.</response>
    /// <response code="400">If the model is invalid, some properties are not set etc.</response>
    /// <response code="401">If the user is not authorized.</response>
    /// <response code="403">If the user has no rights to use this method, or sets some properties that are forbidden to change.</response>
    /// <response code="500">If any server error occures.</response>
    [HasPermission(Permissions.WorkshopEdit)]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(WorkshopStatusDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPut]
    public async Task<IActionResult> UpdateStatus([FromBody] WorkshopStatusDto request)
    {
        if (request == null)
        {
            return BadRequest("WorkshopStatus is null.");
        }

        if (await IsProviderBlocked(Guid.Empty, request.WorkshopId).ConfigureAwait(false))
        {
            return StatusCode(403, "Forbidden to update workshops statuses at blocked providers");
        }

        if (await IsCurrentUserBlocked())
        {
            return StatusCode(403, "Forbidden to update the workshop by the blocked provider.");
        }

        var workshop = await combinedWorkshopService.GetById(request.WorkshopId).ConfigureAwait(false);

        if (workshop is null)
        {
            return NotFound($"There is no Workshop in DB with Id - {request.WorkshopId}");
        }

        var userHasRights = await this.IsUserProvidersOwnerOrAdmin(workshop.ProviderId, workshop.Id).ConfigureAwait(false);
        if (!userHasRights)
        {
            return StatusCode(403, "Forbidden to update workshops, which are not related to you");
        }

        try
        {
            return Ok(await combinedWorkshopService.UpdateStatus(request).ConfigureAwait(false));
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

        if (await IsProviderBlocked(workshop.ProviderId).ConfigureAwait(false))
        {
            return StatusCode(403, "Forbidden to delete workshops at blocked providers");
        }

        if (await IsCurrentUserBlocked())
        {
            return StatusCode(403, "Forbidden to delete the workshop by the blocked provider.");
        }

        var userHasRights = await this.IsUserProvidersOwnerOrAdmin(workshop.ProviderId).ConfigureAwait(false);
        if (!userHasRights)
        {
            return StatusCode(403, "Forbidden to delete workshops of another providers.");
        }

        await combinedWorkshopService.Delete(id).ConfigureAwait(false);

        return NoContent();
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