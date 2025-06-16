using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;
using OutOfSchool.BusinessLogic.Common;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.CompetitiveEvent;
using OutOfSchool.BusinessLogic.Models.CompetitiveEvent.V2;
using OutOfSchool.BusinessLogic.Models.CompetitiveEventDraft;
using OutOfSchool.BusinessLogic.Services.CompetitiveEventDrafts;
using OutOfSchool.BusinessLogic.Services.ProviderServices;
using OutOfSchool.Services.Common.Exceptions;
using OutOfSchool.Services.Models.CompetitiveEvents;
using OutOfSchool.WebApi.Enums;
using OutOfSchool.WebApi.Util.ControllersResultsHelpers;

namespace OutOfSchool.WebApi.Controllers.V2;
/// <summary>
/// Controller with CRUD operations for CompetitiveEvent entity.
/// </summary>
[ApiController]
[FeatureGate(nameof(Feature.Images))]
[AspApiVersion(2)]
[Route("api/v{version:apiVersion}/[controller]/[action]")]
public class CompetitiveEventController : ControllerBase
{
    private readonly ILogger<CompetitiveEventController> logger;
    private readonly ICompetitiveEventServiceV2 competitiveEventService;
    private readonly IUserService userService;
    private readonly ICompetitiveEventDraftService competitiveEventDraftService;
    private readonly IProviderService providerService;

    public CompetitiveEventController(
        ICompetitiveEventServiceV2 competitiveEventService,
        IUserService userService,
        ILogger<CompetitiveEventController> logger,
        ICompetitiveEventDraftService competitiveEventDraftService,
        IProviderService providerService)
    {
        this.competitiveEventService = competitiveEventService ?? throw new ArgumentNullException(nameof(competitiveEventService));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.userService = userService ?? throw new ArgumentNullException(nameof(userService));
        this.competitiveEventDraftService = competitiveEventDraftService ?? throw new ArgumentNullException(nameof(competitiveEventDraftService));
        this.providerService = providerService ?? throw new ArgumentNullException(nameof(providerService));
    }

    /// <summary>
    /// Get competitive event by it's id.
    /// </summary>
    /// <param name="id">CompetitiveEvent's id.</param>
    /// <returns><see cref="CompetitiveEventV2Dto"/>, or no content.</returns>
    /// <response code="200">The entity was found by given Id.</response>
    /// <response code="204">No entity with given Id was found.</response>
    /// <response code="500">If any server error occures. For example: Id was less than one.</response>
    [AllowAnonymous]
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CompetitiveEventV2Dto))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var competitiveEvent = await competitiveEventService.GetById(id).ConfigureAwait(false);

        if (competitiveEvent is null)
        {
            return NotFound();
        }

        return Ok(competitiveEvent);
    }

    /// <summary>
    /// Get CompetitiveEvents cards by Provider's Id.
    /// </summary>
    /// <param name="id">CompetitiveEvent's id.</param>
    /// <param name="filter">Filter to get specified portion of competitive events for specified provider</param>
    /// <response code="200">The list of found entities by given Id.</response>
    /// <response code="204">No entity with given Id was found.</response>
    /// <response code="500">If any server error occures. For example: Id was less than one.</response>
    [AllowAnonymous]
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SearchResult<CompetitiveEventViewCardDto>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetByProviderId(Guid id, [FromQuery] ExcludeIdFilter filter)
    {
        var competitiveEventsCards = await competitiveEventService.GetByProviderId(id, filter).ConfigureAwait(false);

        return this.SearchResultToOkOrNoContent(competitiveEventsCards);
    }

    /// <summary>
    /// Get CompetitiveEvents cards by list of Ids.
    /// </summary>
    /// <param name="ids">CompetitiveEvent's ids list.</param>
    /// <response code="200">The list of found entities by given Ids.</response>
    /// <response code="204">No entity with any of given Ids was found.</response>
    /// <response code="500">If any server error occures. For example: Id was incorrect format.</response>
    [AllowAnonymous]
    [HttpGet("multipleIds")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SearchResult<CompetitiveEvent>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetByIds(IEnumerable<Guid> ids)
    {
        var competitiveEvents = await competitiveEventService.GetByIds(ids).ConfigureAwait(false);

        if (competitiveEvents.Any())
        {
            return NotFound();
        }
        return this.Ok(competitiveEvents);
    }


    /// <summary>
    /// Add new competitive image with image to the database.
    /// </summary>
    /// <param name="dto">Entity to add.</param>
    /// <returns>Created <see cref="CompetitiveEventV2Dto"/>.</returns>
    /// <response code="201">Entity was created and returned with Id.</response>
    /// <response code="400">If the model is invalid, some properties are not set etc.</response>
    /// <response code="401">If the user is not authorized.</response>
    /// <response code="403">If the user has no rights to use this method, or sets some properties that are forbidden.</response>
    /// <response code="413">If the request break the limits, set in configs.</response>
    /// <response code="500">If any server error occures.</response>
    [HasPermission(Permissions.CompetitiveEventAddNew)]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(CompetitiveEventResponseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status413PayloadTooLarge)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Create([FromForm] CompetitiveEventV2CreateRequestDto dto)
    {
        var error = dto switch
        {
            null => BadRequest("CompetitiveEvent is null"),
            _ when !ModelState.IsValid => BadRequest(ModelState),
            _ when await IsCurrentUserBlocked() => StatusCode(403, "User is blocked"),
            _ => null
        };

        if (error != null)
        {
            return error;
        }

        try
        {
            var creationResult = await competitiveEventService.CreateV2(dto).ConfigureAwait(false);

            return CreatedAtAction(
            nameof(GetById),
            new { id = creationResult.CompetitiveEventV2.Id, },
            new CompetitiveEventResponseDto
            {
                CompetitiveEventV2 = creationResult.CompetitiveEventV2,
                UploadingCoverImageResult = creationResult.UploadingCoverImageResult?.CreateSingleUploadingResult(),
                UploadingImagesResults = creationResult.UploadingImagesResults?.CreateMultipleUploadingResult(),
            });
        }
        catch (InvalidOperationException ex)
        {
            var errorMessage = $"Unable to create a new competitive event: {ex.Message}";
            logger.LogError(ex, errorMessage);

            return BadRequest(errorMessage);
        }
    }

    /// <summary>
    /// Update info about competitive event entity.
    /// </summary>
    /// <param name="dto">CompetitiveEvent to update.</param>
    /// <returns>Updated <see cref="CompetitiveEventV2Dto"/>.</returns>
    /// <response code="200">Entity was updated and returned.</response>
    /// <response code="400">If the model is invalid, some properties are not set etc.</response>
    /// <response code="401">If the user is not authorized.</response>
    /// <response code="403">If the user has no rights to use this method, or sets some properties that are forbidden to change.</response>
    /// <response code="413">If the request break the limits, set in configs.</response>
    /// <response code="500">If any server error occures.</response>
    [HasPermission(Permissions.CompetitiveEventEdit)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CompetitiveEventV2Dto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status413PayloadTooLarge)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPut]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Update([FromForm] CompetitiveEventV2CreateRequestDto dto)
    {
        try
        {
            var updatingResult = await competitiveEventService.UpdateV2(dto).ConfigureAwait(false);

            if (updatingResult.CompetitiveEventV2 is null)
            {
                return BadRequest();
            }

            return Ok(CreateUpdateResponse(updatingResult));
        }
        catch (InvalidOperationException ex)
        {
            var errorMessage = $"Unable to update a new competitive event: {ex.Message}";
            logger.LogError(ex, errorMessage);

            return BadRequest(errorMessage);
        }
    }

    /// <summary>
    /// Delete a specific competitive event from the database
    /// </summary>
    /// <param name="id">CompetitiveEvent's id</param>
    /// <returns>StatusCode representing the task completion</returns>
    /// <response code="204">If the entity was successfully deleted, or if the entity was not found by given Id</response>
    /// <response code="401">If the user is not authorized.</response>
    /// <response code="403">If the user has no rights to use this method, or deletes not own competitive event</response>
    /// <response code="500">If any server error occures.</response>
    [HasPermission(Permissions.CompetitiveEventRemove)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var competitiveEventToDelete = await competitiveEventService.GetById(id).ConfigureAwait(false);

            if (competitiveEventToDelete is null)
            {
                return NoContent();
            }

            await competitiveEventService.DeleteV2(id).ConfigureAwait(false);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            var errorMessage = $"Unable to delete a competitive event: {ex.Message}";
            logger.LogError(ex, errorMessage);

            return BadRequest(errorMessage);
        }
    }

    /// <summary>
    /// Create a draft of a competitive event.
    /// </summary>
    /// <param name="competitiveEventV2Dto">Entity to add.</param>
    /// <returns>Created <see cref="CompetitiveEventDraftResultDto"/>.</returns>
    /// <response code="201">Entity was created and returned with Id.</response>
    /// <response code="400">If the model is invalid, some properties are not set etc.</response>
    /// <response code="401">If the user is not authorized.</response>
    /// <response code="403">If the user has no rights to use this method, or sets some properties that are forbidden.</response>
    /// <response code="413">If the request break the limits, set in configs.</response>
    /// <response code="500">If any server error occures.</response>
    [HasPermission(Permissions.CompetitiveEventAddNew)]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(CompetitiveEventDraftResultDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status413PayloadTooLarge)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> CreateDraft([FromForm] CompetitiveEventV2Dto competitiveEventV2Dto)
    {
        if (competitiveEventV2Dto is null)
        {
            return BadRequest("Dto is null.");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var providerValidationResult = await ValidateProvider(competitiveEventV2Dto.OrganizerOfTheEventId).ConfigureAwait(false);
        if (providerValidationResult != null)
        {
            return providerValidationResult;
        }

        var result = await competitiveEventDraftService.Create(competitiveEventV2Dto).ConfigureAwait(false);

        if (result == null)
        {
            return BadRequest("Returned result is null.");
        }

        return CreatedAtAction(
            nameof(GetDraftById),
            new { id = result.CompetitiveEventDraft.CompetitiveEventDraftId },
            result);
    }

    /// <summary>
    /// Update existing competitive event draft.
    /// </summary>
    /// <param name="id">Key in the table.</param>
    /// <param name="competitiveEventDraftUpdateDto"></param>
    /// <returns>Returns <see cref="CompetitiveEventDraftResultDto"/>.</returns>
    /// <response code="200">Entity was updated and returned.</response>
    /// <response code="400">If the model is invalid, some properties are not set etc.</response>
    /// <response code="401">If the user is not authorized.</response>
    /// <response code="403">If the user has no rights to use this method, or sets some properties that are forbidden.</response>
    /// <response code="413">If the request break the limits, set in configs.</response>
    /// <response code="500">If any server error occures.</response>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CompetitiveEventResponseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status413PayloadTooLarge)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UpdateDraft(Guid id, [FromForm] CompetitiveEventDraftUpdateDto competitiveEventDraftUpdateDto)
    {
        if (competitiveEventDraftUpdateDto is null)
        {
            return BadRequest("Dto is null.");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var providerValidationResult = await ValidateProvider(competitiveEventDraftUpdateDto.CompetitiveEventV2Dto.OrganizerOfTheEventId).ConfigureAwait(false);
        if (providerValidationResult != null)
        {
            return providerValidationResult;
        }

        try
        {
            var result = await competitiveEventDraftService.Update(id, competitiveEventDraftUpdateDto).ConfigureAwait(false);

            if (!result.Succeeded)
            {
                switch (result.OperationResult.Errors.FirstOrDefault().Code)
                {
                    case "400":
                        return BadRequest(result.OperationResult.Errors.FirstOrDefault()?.Description ?? "Model is invalid.");
                    default:
                        return StatusCode(500, result.OperationResult.Errors.FirstOrDefault()?.Description ?? "Something gone wrong.");
                }
            }

            return Ok(result);
        }
        catch (EntityDeletedConflictException ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);

        }
        catch (EntityModifiedConflictException ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    /// <summary>
    /// Deletes existing competitive event draft.
    /// </summary>
    /// <param name="id">Key in the table.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    /// <response code="204">Entity was successfully deleted.</response>
    /// <response code="400">If the model is invalid, some properties are not set etc.</response>
    /// <response code="401">If the user is not authorized.</response>
    /// <response code="403">If the user has no rights to use this method, or sets some properties that are forbidden.</response>
    /// <response code="500">If any server error occures.</response>
    [HasPermission(Permissions.CompetitiveEventRemove)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDraft(Guid id)
    {
        try
        {
            var result = await competitiveEventDraftService.Delete(id).ConfigureAwait(false);

            if (!result.Succeeded)
            {
                switch (result.Errors.FirstOrDefault().Code)
                {
                    case "400":
                        return BadRequest(result.Errors.FirstOrDefault()?.Description ?? "Something gone wrong.");
                    case "404":
                        return NotFound(result.Errors.FirstOrDefault()?.Description ?? "Competitive event draft was not found by given id.");
                    default:
                        return StatusCode(500, result.Errors.FirstOrDefault()?.Description ?? "Something gone wrong.");
                }
            }

            return NoContent();
        }
        catch (EntityDeletedConflictException ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);

        }
        catch (EntityModifiedConflictException ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    /// <summary>
    /// Sends competitive event draft for moderation.
    /// </summary>
    /// <param name="id">Key in the table.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    /// <response code="200">Entity was successfully sent for moderation.</response>
    /// <response code="400">If the model is invalid, some properties are not set etc.</response>
    /// <response code="401">If the user is not authorized.</response>
    /// <response code="403">If the user has no rights to use this method, or sets some properties that are forbidden.</response>
    /// <response code="500">If any server error occures.</response>
    [HasPermission(Permissions.CompetitiveEventEdit)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPut("{id}/send-for-moderation")]
    public async Task<IActionResult> SendForModeration(Guid id)
    {
        try
        {
            var result = await competitiveEventDraftService.SendForModeration(id).ConfigureAwait(false);

            if (!result.Succeeded)
            {
                switch (result.Errors.FirstOrDefault().Code)
                {
                    case "400":
                        return BadRequest(result.Errors.FirstOrDefault()?.Description ?? "Something gone wrong.");
                    case "404":
                        return NotFound(result.Errors.FirstOrDefault()?.Description ?? "Competitive event draft was not found by given id.");
                    default:
                        return StatusCode(500, result.Errors.FirstOrDefault()?.Description ?? "Something gone wrong.");
                }
            }

            return Ok();
        }
        catch (EntityDeletedConflictException ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);

        }
        catch (EntityModifiedConflictException ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

    /// <summary>
    /// Gets competitive event drafts by provider's id. 
    /// </summary>
    /// <param name="providerId">Id of the provider.</param>
    /// <param name="filter">Filter to get specified portion of competitive events for specified provider</param>
    /// <returns>List of competitive event drafts.</returns>
    /// <response code="200">The list of found entities by given Id.</response>
    /// <response code="204">No entity with given Id was found.</response>
    /// <response code="400">If the model is invalid, some properties are not set etc.</response>
    /// <response code="500">If any server error occures. For example: Id was less than one.</response>
    [HasPermission(Permissions.CompetitiveEventRead)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SearchResult<CompetitiveEventDraftViewCardDto>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet("provider/{providerId}/drafts")]
    public async Task<IActionResult> GetDraftByProviderId(Guid providerId, [FromQuery] ExcludeIdFilter filter) =>
        await competitiveEventDraftService.GetByProviderId(providerId, filter).ProtectAndMap(this.SearchResultToOkOrNoContent);

    /// <summary>
    /// Gets competitive event darft by its id.
    /// </summary>
    /// <param name="id">Key in the table.</param>
    /// <returns>Competitive event draft by given id.</returns>
    /// <response code="200">The entoty by given Id.</response>
    /// <response code="204">No entity with given Id was found.</response>
    /// <response code="400">If the model is invalid, some properties are not set etc.</response>
    /// <response code="500">If any server error occures. For example: Id was less than one.</response>
    [HasPermission(Permissions.CompetitiveEventRead)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CompetitiveEventDraftResponseDto))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet("drafts/{id}")]
    public async Task<IActionResult> GetDraftById(Guid id)
    {
        var responseDto = await competitiveEventDraftService.GetCompetitiveEventDraftByIdMapped(id);
        return responseDto is not null ? Ok(responseDto) : NotFound();
    }

    private CompetitiveEventResponseDto CreateUpdateResponse(CompetitiveEventResultDto updatingResult)
    {
        return new CompetitiveEventResponseDto
        {
            CompetitiveEventV2 = updatingResult.CompetitiveEventV2,
            UploadingCoverImageResult = updatingResult.UploadingCoverImageResult?.CreateSingleUploadingResult(),
            UploadingImagesResults = updatingResult.UploadingImagesResults?.CreateMultipleUploadingResult(),
        };
    }

    private async Task<bool> IsCurrentUserBlocked()
    {
        var userId = GettingUserProperties.GetUserId(User);

        return await userService.IsBlocked(userId);
    }

    private async Task<IActionResult> ValidateProvider(Guid providerId)
    {
        var isBlocked = await providerService.IsBlocked(providerId).ConfigureAwait(false);

        // null means Provider does not exist
        if (!isBlocked.HasValue)
        {
            return StatusCode(
                StatusCodes.Status400BadRequest,
                new { Message = $"Provider with ID {providerId} not found." });
        }

        if (isBlocked.Value)
        {
            return StatusCode(
                StatusCodes.Status403Forbidden,
                new { Message = $"Provider with ID {providerId} is blocked and cannot create workshop drafts." });
        }

        return null;
    }
}
