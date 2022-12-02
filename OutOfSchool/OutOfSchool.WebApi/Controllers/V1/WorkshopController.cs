using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OutOfSchool.Services.Enums;
using OutOfSchool.WebApi.Common;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.Workshop;

namespace OutOfSchool.WebApi.Controllers.V1;

/// <summary>
/// Controller with CRUD operations for Workshop entity.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]/[action]")]
public class WorkshopController : ControllerBase
{
    private readonly IWorkshopServicesCombiner combinedWorkshopService;
    private readonly IProviderService providerService;
    private readonly IProviderAdminService providerAdminService;
    private readonly IStringLocalizer<SharedResource> localizer;
    private readonly AppDefaultsConfig options;

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkshopController"/> class.
    /// </summary>
    /// <param name="combinedWorkshopService">Service for operations with Workshops.</param>
    /// <param name="providerService">Service for Provider model.</param>
    /// <param name="providerAdminService">Service for ProviderAdmin model.</param>
    /// <param name="localizer">Localizer.</param>
    /// <param name="options">Application default values.</param>
    public WorkshopController(
        IWorkshopServicesCombiner combinedWorkshopService,
        IProviderService providerService,
        IProviderAdminService providerAdminService,
        IStringLocalizer<SharedResource> localizer,
        IOptions<AppDefaultsConfig> options)
    {
        this.localizer = localizer;
        this.combinedWorkshopService = combinedWorkshopService;
        this.providerAdminService = providerAdminService;
        this.providerService = providerService;
        this.options = options.Value;
    }

    /// <summary>
    /// Get workshop by it's id.
    /// </summary>
    /// <param name="id">Workshop's id.</param>
    /// <returns><see cref="WorkshopDTO"/>, or no content.</returns>
    /// <response code="200">The entity was found by given Id.</response>
    /// <response code="204">No entity with given Id was found.</response>
    /// <response code="500">If any server error occures. For example: Id was less than one.</response>
    [AllowAnonymous]
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(WorkshopDTO))]
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
    /// Get workshop cards by Provider's Id.
    /// </summary>
    /// <param name="id">Provider's id.</param>
    /// <param name="filter">Filter to get specified portion of workshop view cards for specified provider.
    /// Id of the excluded workshop could be specified.</param>
    /// <returns><see cref="SearchResult{WorkshopBaseCard}"/>, or no content.</returns>
    /// <response code="200">The list of found entities by given Id.</response>
    /// <response code="204">No entity with given Id was found.</response>
    /// <response code="400">Provider id is empty.</response>
    /// <response code="500">If any server error occures. For example: Id was less than one.</response>
    [AllowAnonymous]
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SearchResult<WorkshopBaseCard>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetByProviderId(Guid id, [FromQuery] ExcludeIdFilter filter)
    {
        if (id == Guid.Empty)
        {
            return BadRequest("Provider id is empty.");
        }

        var workshopCards = await combinedWorkshopService.GetByProviderId<WorkshopBaseCard>(id, filter)
            .ConfigureAwait(false);

        if (workshopCards.TotalAmount == 0)
        {
            return NoContent();
        }

        return Ok(workshopCards);
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

        var workshopProviderViewCards = await combinedWorkshopService.GetByProviderId<WorkshopProviderViewCard>(id, filter).ConfigureAwait(false);

        if (workshopProviderViewCards.TotalAmount == 0)
        {
            return NoContent();
        }

        return Ok(workshopProviderViewCards);
    }

    /// <summary>
    /// Get workshops that matches filter's parameters.
    /// </summary>
    /// <param name="filter">Entity that represents searching parameters.</param>
    /// <returns><see cref="SearchResult{WorkshopCard}"/>, or no content.</returns>
    /// <response code="200">The list of found entities by given filter.</response>
    /// <response code="204">No entity with given filter was found.</response>
    /// <response code="500">If any server error occures. For example: Id was less than one.</response>
    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SearchResult<WorkshopCard>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetByFilter([FromQuery] WorkshopFilter filter)
    {
        var result = await combinedWorkshopService.GetByFilter(filter).ConfigureAwait(false);

        if (result.TotalAmount < 1)
        {
            return NoContent();
        }

        return Ok(result);
    }

    /// <summary>
    /// Add new workshop to the database.
    /// </summary>
    /// <param name="dto">Entity to add.</param>
    /// <returns>Created <see cref="WorkshopDTO"/>.</returns>
    /// <response code="201">Entity was created and returned with Id.</response>
    /// <response code="400">If the model is invalid, some properties are not set etc.</response>
    /// <response code="401">If the user is not authorized.</response>
    /// <response code="403">If the user has no rights to use this method, or sets some properties that are forbidden.</response>
    /// <response code="500">If any server error occures.</response>
    [HasPermission(Permissions.WorkshopAddNew)]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(WorkshopDTO))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPost]
    public async Task<IActionResult> Create(WorkshopDTO dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userHasRights = await this.IsUserProvidersOwnerOrAdmin(dto.ProviderId).ConfigureAwait(false);
        if (!userHasRights)
        {
            return StatusCode(403, "Forbidden to create workshops for another providers.");
        }

        dto.Id = default;
        dto.Address.Id = default;

        if (dto.Teachers != null)
        {
            foreach (var teacher in dto.Teachers)
            {
                teacher.Id = default;
            }
        }

        foreach (var dateTimeRangeDto in dto.DateTimeRanges)
        {
            dateTimeRangeDto.Id = default;
        }

        var workshop = await combinedWorkshopService.Create(dto).ConfigureAwait(false);

        // here we will get "false" if workshop was created by assistant provider admin
        // because user is not currently associated with new workshop
        // so we can update information to allow assistant manage created workshop

        if (!(await IsUserProvidersOwnerOrAdmin(workshop.ProviderId, workshop.Id).ConfigureAwait(false)))
        {
            var userId = User.FindFirst("sub")?.Value;
            await providerAdminService.GiveAssistantAccessToWorkshop(userId, workshop.Id).ConfigureAwait(false);
        }

        return CreatedAtAction(
            nameof(GetById),
            new { id = workshop.Id, },
            workshop);
    }

    /// <summary>
    /// Update info about workshop entity.
    /// </summary>
    /// <param name="dto">Workshop to update.</param>
    /// <returns>Updated <see cref="WorkshopDTO"/>.</returns>
    /// <response code="200">Entity was updated and returned.</response>
    /// <response code="400">If the model is invalid, some properties are not set etc.</response>
    /// <response code="401">If the user is not authorized.</response>
    /// <response code="403">If the user has no rights to use this method, or sets some properties that are forbidden to change.</response>
    /// <response code="500">If any server error occures.</response>
    [HasPermission(Permissions.WorkshopEdit)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(WorkshopDTO))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPut]
    public async Task<IActionResult> Update(WorkshopDTO dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userHasRights = await this.IsUserProvidersOwnerOrAdmin(dto.ProviderId, dto.Id).ConfigureAwait(false);

        if (!userHasRights)
        {
            return StatusCode(403, "Forbidden to update workshops, which are not related to you");
        }

        return Ok(await combinedWorkshopService.Update(dto).ConfigureAwait(false));
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
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(WorkshopStatusDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPut]
    public async Task<IActionResult> UpdateStatus([FromBody] WorkshopStatusDto request)
    {
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
            try
            {
                var provider = await providerService.GetByUserId(userId).ConfigureAwait(false);
                if (providerId != provider.Id)
                {
                    return false;
                }
            }
            catch (ArgumentException)
            {
                var isUserRelatedAdmin = await providerAdminService.CheckUserIsRelatedProviderAdmin(userId, providerId, workshopId).ConfigureAwait(false);
                if (!isUserRelatedAdmin)
                {
                    return false;
                }
            }
        }

        return true;
    }
}