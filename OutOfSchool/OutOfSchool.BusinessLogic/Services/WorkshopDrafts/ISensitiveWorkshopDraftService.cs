using OutOfSchool.BusinessLogic.Common;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.WorkshopDraft;

namespace OutOfSchool.BusinessLogic.Services.WorkshopDrafts;
public interface ISensitiveWorkshopDraftService
{
    /// <summary>
    /// Get entities from the database that match filter's parameters in admin panel.
    /// </summary>
    /// <param name="filter">Filter with specified searching parameters.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    /// The task result contains the <see cref="SearchResult{WorkshopDraftResponseDto}"/> that contains found elements.</returns>    
    Task<SearchResult<WorkshopDraftResponseDto>> FetchByFilterForAdmins(WorkshopDraftFilterAdministration filter = null);

    /// <summary>
    /// Updates the content of a workshop draft using data provided by a moderator.
    /// Allowed only if the draft is in a modifiable status and the user is a tech admin or the specified moderator.
    /// </summary>
    /// <param name="draftId">The ID of the workshop draft to update.</param>
    /// <param name="userId">The ID of the user performing the update.</param>
    /// <param name="dto">The data transfer object containing updated content fields.</param>
    /// <returns>A <see cref="Result{WorkshopDraftResponseDto}"/> with updated draft data or error information.</returns>
    Task<Result<WorkshopDraftResponseDto>> UpdateDraftAsModeratorAsync(Guid draftId, Guid userId, ModeratorWorkshopDraftEditDto dto);

    /// <summary>
    /// Deletes the cover image of a workshop draft on behalf of a moderator.
    /// Only allowed if the draft exists, is editable, and contains a cover image.
    /// </summary>
    /// <param name="draftId">The ID of the workshop draft.</param>
    /// <param name="userId">The ID of the user performing the deletion.</param>
    /// <returns>A <see cref="Result{WorkshopDraftResponseDto}"/> with the updated draft or error information.</returns>
    Task<Result<WorkshopDraftResponseDto>> DeleteCoverImageAsModeratorAsync(Guid draftId, Guid userId);

    /// <summary>
    /// Deletes a single image from a workshop draft on behalf of a moderator.
    /// </summary>
    /// <param name="draftId">The ID of the workshop draft.</param>
    /// <param name="userId">The ID of the user performing the deletion.</param>
    /// <param name="imageId">The externalStorageId of the image to delete.</param>
    /// <returns>A <see cref="Result{WorkshopDraftResponseDto}"/> with the updated draft or error information.</returns>
    Task<Result<WorkshopDraftResponseDto>> DeleteImageAsModeratorAsync(Guid draftId, Guid userId, string imageId);

    /// <summary>
    /// Deletes multiple images from a workshop draft on behalf of a moderator.
    /// Also updates the draft status and logs all deletions.
    /// </summary>
    /// <param name="draftId">The ID of the workshop draft.</param>
    /// <param name="userId">The ID of the user performing the deletion.</param>
    /// <param name="imageIds">A collection of externalStorageIds of images to delete.</param>
    /// <returns>A <see cref="Result{WorkshopDraftResponseDto}"/> with the updated draft or error information.</returns>
    Task<Result<WorkshopDraftResponseDto>> DeleteManyImagesAsModeratorAsync(Guid draftId, Guid userId, IEnumerable<string> imageIds);
}