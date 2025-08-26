using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement.Mvc;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.CompetitiveEvent;
using OutOfSchool.BusinessLogic.Models.WorkshopDraft;
using OutOfSchool.BusinessLogic.Models.Workshops;
using OutOfSchool.BusinessLogic.Services.ProviderServices;
using OutOfSchool.BusinessLogic.Services.WorkshopDrafts;
using OutOfSchool.Services.Common.Exceptions;
using OutOfSchool.WebApi.Enums;

namespace OutOfSchool.WebApi.Controllers.V2;

/// <summary>
/// Controller with CRUD operations for WorkshopDraft entity.
/// </summary>
[ApiController]
[FeatureGate(nameof(Feature.Images))]
[AspApiVersion(2)]
[Route("api/v{version:apiVersion}/[controller]/[action]")]
public class WorkshopDraftController : ControllerBase
{
    private readonly IProviderService providerService;
    private readonly IWorkshopDraftService workshopDraftService;
    private readonly ISensitiveWorkshopDraftService sensitiveWorkshopDraftService;

    /// <summary>
    /// Initializes a new instance of the <see cref="WorkshopDraftController"/> class.
    /// </summary>
    /// <param name="providerService">Service for Provider model</param>
    /// <param name="workshopDraftService">Service for WorkshopDraft model.</param>
    /// <param name="sensitiveWorkshopDraftService">Service for SensitiveWorkshopDraft model.</param>"

    public WorkshopDraftController(
        IProviderService providerService,
        IWorkshopDraftService workshopDraftService,
        ISensitiveWorkshopDraftService sensitiveWorkshopDraftService)
     
    {
        this.providerService = providerService;
        this.workshopDraftService = workshopDraftService;
        this.sensitiveWorkshopDraftService = sensitiveWorkshopDraftService;
    }


    /// <summary>
    /// Add new workshop draft to the database.
    /// </summary>
    /// <param name="workshopV2Dto">Entity to add.</param>
    /// <returns>Created <see cref="WorkshopDraftResultDto"/>.</returns>
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
    public async Task<IActionResult> Create([FromForm] WorkshopV2Dto workshopV2Dto)
    {
        var providerValidationResult = await ValidateProvider(workshopV2Dto.ProviderId);
        if (providerValidationResult != null)
        {
            return providerValidationResult;
        }

        // TODO: After implementing the new workshop model, add validation to check if the parent workshop is nested.
        // A workshop must have only one level of nesting.

        var result = await workshopDraftService.Create(workshopV2Dto);

        return CreatedAtAction(
            nameof(Create),
            new { id = result.WorkshopDraft.WorkshopDraftId },
            result);
    }

    [HasPermission(Permissions.WorkshopEdit)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(WorkshopDraftResultDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status413PayloadTooLarge)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPut]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Update([FromForm] WorkshopDraftUpdateDto workshopDraftUpdateDto)
    {
        var providerValidationResult = await ValidateProvider(workshopDraftUpdateDto.WorkshopV2Dto.ProviderId);
        if (providerValidationResult != null)
        {
            return providerValidationResult;
        }

        try
        {
            var result = await workshopDraftService.Update(workshopDraftUpdateDto);
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

    [HasPermission(Permissions.WorkshopRemove)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await workshopDraftService.Delete(id);
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

    [HasPermission(Permissions.WorkshopEdit)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPut("{id}")]
    public async Task<IActionResult> SendForModeration(Guid id)
    {
        try
        {
            await workshopDraftService.SendForModeration(id);
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

    [HasPermission(Permissions.WorkshopApprove)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPut("{id}")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] WorkshopDraftRejectionDto workshopDraftRejection)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            await workshopDraftService.Reject(id, workshopDraftRejection.RejectionMessage);
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

    [HasPermission(Permissions.WorkshopApprove)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpPut("{id}")]
    public async Task<IActionResult> Approve(Guid id)
    {
        try
        {
            await workshopDraftService.Approve(id);
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

    [HasPermission(Permissions.WorkshopEdit)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SearchResult<WorkshopDraftViewCardDto>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet("provider/{id}/drafts")]
    public async Task<IActionResult> GetByProviderId(Guid id, [FromQuery] CompetitiveEventFilterTitle filter) =>
        await workshopDraftService.GetByProviderId(id, filter).ProtectAndMap(this.SearchResultToOkOrNoContent);

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

    [HasPermission(Permissions.WorkshopEdit)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(SearchResult<WorkshopDraftResponseDto>))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [HttpGet("drafts/{id}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var responseDto = await workshopDraftService.GetWorkshopDraftByIdMapped(id);
        return responseDto is not null ? Ok(responseDto) : NotFound();
    }

    [HasPermission(Permissions.WorkshopEdit)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Guid?))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [HttpGet("{workshopId}")]
    public async Task<IActionResult> GetWorkshopDraftIdByWorkshopId(Guid workshopId)
    {
        var result = await workshopDraftService.GetWorkshopDraftIdByWorkshopId(workshopId);
        return result.HasValue ? Ok(result) : NoContent();
    }

    /// <summary>
    /// Updates a workshop draft by a user with permission to moderate drafts. Only allowed in specific statuses.
    /// </summary>
    /// <param name="draftId">The ID of the draft to update.</param>
    /// <param name="dto">The updated content provided by the moderator.</param>
    /// <returns>Returns <see cref="WorkshopDraftResponseDto"/> if successful.</returns>
    /// <response code="200">The draft was successfully updated.</response>
    /// <response code="400">Invalid input data.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user does not have permission to perform this action.</response>
    /// <response code="404">The specified draft was not found.</response>
    /// <response code="409">The draft cannot be edited in its current status.</response>
    /// <response code="500">An unexpected error occurred.</response>
    [HttpPut("/api/v{version:apiVersion}/workshop-drafts/{draftId}/moderator-edit")]
    [HasPermission(Permissions.WorkshopEdit)]
    [Consumes("application/json")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(WorkshopDraftResponseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateAsModerator(Guid draftId, [FromBody] ModeratorWorkshopDraftEditDto dto)
    {
        var result = await sensitiveWorkshopDraftService.UpdateDraftAsModeratorAsync(draftId, dto);

        return this.ToActionResult(result);
    }

    /// <summary>
    /// Deletes the cover image from the specified workshop draft by a user with permission to moderate drafts.
    /// </summary>
    /// <param name="draftId">The ID of the draft to update.</param>
    /// <returns>Returns <see cref="WorkshopDraftResponseDto"/> with the cover image removed if successful.</returns>
    /// <response code="200">Cover image deleted successfully.</response>
    /// <response code="400">The draft does not have a cover image.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user does not have permission to perform this action.</response>
    /// <response code="404">The specified draft was not found.</response>
    /// <response code="409">The draft is not editable.</response>
    /// <response code="500">An unexpected error occurred.</response>
    [HttpDelete("/api/v{version:apiVersion}/workshop-drafts/{draftId}/moderator/cover-image")]
    [HasPermission(Permissions.WorkshopEdit)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(WorkshopDraftResponseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteCoverImageAsModerator(Guid draftId)
    {
        var result = await sensitiveWorkshopDraftService.DeleteCoverImageAsModeratorAsync(draftId);

        return this.ToActionResult(result);
    }

    /// <summary>
    /// Deletes a specific image from a workshop draft by a user with permission to moderate drafts.
    /// </summary>
    /// <param name="draftId">The ID of the draft.</param>
    /// <param name="imageId">The ID of the image to delete (externalStorageId).</param>
    /// <returns>Returns <see cref="WorkshopDraftResponseDto"/> with the image removed if successful.</returns>
    /// <response code="200">Image was successfully deleted.</response>
    /// <response code="400">Image ID is missing or invalid.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user does not have permission to perform this action.</response>
    /// <response code="404">Draft or image not found.</response>
    /// <response code="409">The draft is not editable.</response>
    /// <response code="500">An unexpected error occurred.</response>
    [HttpDelete("/api/v{version:apiVersion}/workshop-drafts/{draftId}/moderator/image/{imageId}")]
    [HasPermission(Permissions.WorkshopEdit)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(WorkshopDraftResponseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteImageAsModerator(Guid draftId, string imageId)
    {

        var result = await sensitiveWorkshopDraftService.DeleteImageAsModeratorAsync(draftId, imageId);

        return this.ToActionResult(result);
    }

    /// <summary>
    /// Deletes multiple images from a workshop draft by a user with permission to moderate drafts.
    /// </summary>
    /// <param name="draftId">The ID of the draft.</param>
    /// <param name="imageIds">A list of image IDs (externalStorageId) to delete.</param>
    /// <returns>Returns <see cref="WorkshopDraftResponseDto"/> with multiple images removed if successful.</returns>
    /// <response code="200">Images deleted successfully.</response>
    /// <response code="400">No image IDs were provided.</response>
    /// <response code="401">The user is not authenticated.</response>
    /// <response code="403">The user does not have permission to perform this action.</response>
    /// <response code="404">Draft not found or images not present in draft.</response>
    /// <response code="409">The draft is not editable.</response>
    /// <response code="500">An unexpected error occurred.</response>
    [HttpDelete("/api/v{version:apiVersion}/workshop-drafts/{draftId}/moderator/images")]
    [HasPermission(Permissions.WorkshopEdit)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(WorkshopDraftResponseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteManyImagesAsModerator(Guid draftId, [FromBody] List<string> imageIds)
    {
        var result = await sensitiveWorkshopDraftService.DeleteManyImagesAsModeratorAsync(draftId, imageIds);

        return this.ToActionResult(result);
    }
}
