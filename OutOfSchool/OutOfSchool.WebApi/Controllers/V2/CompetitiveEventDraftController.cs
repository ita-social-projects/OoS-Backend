using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.CompetitiveEvent.V2;
using OutOfSchool.BusinessLogic.Models.CompetitiveEventDraft;
using OutOfSchool.BusinessLogic.Services.CompetitiveEventDrafts;
using OutOfSchool.BusinessLogic.Services.ProviderServices;
using OutOfSchool.Services.Common.Exceptions;
using OutOfSchool.WebApi.Enums;

namespace OutOfSchool.WebApi.Controllers.V2;

/// <summary>
/// Controller with CRUD operations for CompetitiveEventDraft entity.
/// </summary>
[ApiController]
[FeatureGate(nameof(Feature.Images))]
[AspApiVersion(2)]
[Route("api/v{version:apiVersion}/[controller]/[action]")]
public class CompetitiveEventDraftController : ControllerBase
{
    private readonly ILogger<CompetitiveEventDraftController> logger;
    private readonly ICompetitiveEventDraftService competitiveEventDraftService;
    private readonly ISensitiveCompetitiveEventDraftService sensitiveCompetitiveEventDraftService;
    private readonly IProviderService providerService;

    public CompetitiveEventDraftController(
        ILogger<CompetitiveEventDraftController> logger,
        ICompetitiveEventDraftService competitiveEventDraftService,
        ISensitiveCompetitiveEventDraftService sensitiveCompetitiveEventDraftService,
        IProviderService providerService)
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.competitiveEventDraftService = competitiveEventDraftService ?? throw new ArgumentNullException(nameof(competitiveEventDraftService));
        this.sensitiveCompetitiveEventDraftService = sensitiveCompetitiveEventDraftService ?? throw new ArgumentNullException(nameof(sensitiveCompetitiveEventDraftService));
        this.providerService = providerService ?? throw new ArgumentNullException(nameof(providerService));
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
    [HttpPost("/api/v{version:apiVersion}/competitions-drafts")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Create([FromForm] CompetitiveEventV2Dto competitiveEventV2Dto)
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
            nameof(GetById),
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
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CompetitiveEventResponseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status413PayloadTooLarge)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPut("/api/v{version:apiVersion}/competitions-drafts/{id}")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Update(Guid id, [FromForm] CompetitiveEventDraftUpdateDto competitiveEventDraftUpdateDto)
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
                switch (result.OperationResult?.Errors?.FirstOrDefault()?.Code)
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
    [HttpDelete("/api/v{version:apiVersion}/competitions-drafts/{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var result = await competitiveEventDraftService.Delete(id).ConfigureAwait(false);

            if (!result.Succeeded)
            {
                switch (result.Errors?.FirstOrDefault()?.Code)
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
    [HttpPut("/api/v{version:apiVersion}/competitions-drafts/{id}/send-for-moderation")]
    public async Task<IActionResult> SendForModeration(Guid id)
    {
        try
        {
            var result = await competitiveEventDraftService.SendForModeration(id).ConfigureAwait(false);

            if (!result.Succeeded)
            {
                switch (result.Errors?.FirstOrDefault()?.Code)
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
    [HttpGet("/api/v{version:apiVersion}/provider/{providerId}/competitions-drafts")]
    public async Task<IActionResult> GetByProviderId(Guid providerId, [FromQuery] ExcludeIdFilter filter) =>
        await competitiveEventDraftService.GetByProviderId(providerId, filter).ProtectAndMap(this.SearchResultToOkOrNoContent);

    /// <summary>
    /// Gets competitive event draft by its id.
    /// </summary>
    /// <param name="id">Key in the table.</param>
    /// <returns>Competitive event draft by given id.</returns>
    /// <response code="200">The entity by given Id was found.</response>
    /// <response code="204">No entity with given Id was found.</response>
    /// <response code="400">If the model is invalid, some properties are not set etc.</response>
    /// <response code="500">If any server error occures. For example: Id was less than one.</response>
    [HasPermission(Permissions.CompetitiveEventRead)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CompetitiveEventDraftResponseDto))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet("/api/v{version:apiVersion}/competitions-drafts/{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var responseDto = await competitiveEventDraftService.GetCompetitiveEventDraftByIdMapped(id);
        return responseDto is not null ? Ok(responseDto) : NotFound();
    }

    /// <summary>
    /// Rejects the competitive event draft by a user with permission to moderate drafts.
    /// </summary>
    /// <param name="id">Key in the table.</param>
    /// <param name="competitiveEventDraftRejection">Dto that contains rejction message.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    [HasPermission(Permissions.CompetitiveEventApprove)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPut("/api/v{version:apiVersion}/competitions-drafts/{id}/reject")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] CompetitiveEventDraftRejectionDto competitiveEventDraftRejection)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            await competitiveEventDraftService.Reject(id, competitiveEventDraftRejection.RejectionMessage);
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
    /// Approves the competitive event draft by a user with permission to moderate drafts.
    /// </summary>
    /// <param name="id">The ID of the draft to approve.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    /// <response code="200">The draft was successfully approved.</response>
    /// <response code="400">The model is invalid.</response>
    /// <response code="401">The user is not authorized.</response>
    /// <response code="403">The user has no rights to use this method.</response>
    /// <response code="500">If any server error occurs.</response>
    [HasPermission(Permissions.CompetitiveEventApprove)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPut("/api/v{version:apiVersion}/competitions-drafts/{id}/approve")]
    public async Task<IActionResult> Approve(Guid id)
    {
        try
        {
            await competitiveEventDraftService.Approve(id);
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
    /// Deletes the cover image from the specified competitive event draft by a user with permission to moderate drafts.
    /// </summary>
    /// <param name="draftId">The ID of the draft to update.</param>
    /// <returns>Returns <see cref="CompetitiveEventDraftResponseDto"/> with the cover image removed if successful.</returns>
    /// <response code="200">Cover image deleted successfully.</response>
    /// <response code="400">The draft does not have a cover image.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user does not have permission to perform this action.</response>
    /// <response code="404">The specified draft was not found.</response>
    /// <response code="409">The draft is not editable.</response>
    /// <response code="500">An unexpected error occurred.</response>
    [HttpDelete("/api/v{version:apiVersion}/competitions-drafts/{draftId}/cover-image")]
    [HasPermission(Permissions.CompetitiveEventApprove)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CompetitiveEventDraftResponseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteCoverImageAsModerator(Guid draftId)
    {
        var result = await sensitiveCompetitiveEventDraftService.DeleteCoverImageAsModeratorAsync(draftId);

        return this.ToActionResult(result);
    }

    /// <summary>
    /// Deletes multiple images from a competitive events draft by a user with permission to moderate drafts.
    /// </summary>
    /// <param name="draftId">The ID of the draft.</param>
    /// <param name="imageIds">A list of image IDs (externalStorageId) to delete.</param>
    /// <returns>Returns <see cref="CompetitiveEventDraftResponseDto"/> with multiple images removed if successful.</returns>
    /// <response code="200">Images deleted successfully.</response>
    /// <response code="400">No image IDs were provided.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user does not have permission to perform this action.</response>
    /// <response code="404">Draft not found or images not present in draft.</response>
    /// <response code="409">The draft is not editable.</response>
    /// <response code="500">An unexpected error occurred.</response>
    [HttpDelete("/api/v{version:apiVersion}/competitions-drafts/{draftId}/images")]
    [HasPermission(Permissions.CompetitiveEventApprove)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CompetitiveEventDraftResponseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteImagesAsModerator(Guid draftId, [FromBody] List<string> imageIds)
    {
        var result = await sensitiveCompetitiveEventDraftService.DeleteImagesAsModeratorAsync(draftId, imageIds);

        return this.ToActionResult(result);
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
                new { Message = $"Provider with ID {providerId} is blocked and cannot create competitive event drafts." });
        }

        return null;
    }
} 