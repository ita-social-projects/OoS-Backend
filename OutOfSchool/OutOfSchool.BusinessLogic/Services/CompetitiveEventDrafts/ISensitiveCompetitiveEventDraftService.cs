using OutOfSchool.BusinessLogic.Common;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.CompetitiveEventDraft;

namespace OutOfSchool.BusinessLogic.Services.CompetitiveEventDrafts;

public interface ISensitiveCompetitiveEventDraftService
{
    /// <summary>
    /// Get entities from the database that match filter's parameters in admin panel.
    /// </summary>
    /// <param name="filter">Filter with specified searching parameters.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    /// The task result contains the <see cref="SearchResult{CompetitiveEventDraftResponseDto}"/> that contains found elements.</returns>    
    Task<SearchResult<CompetitiveEventDraftResponseDto>> FetchByFilterForAdmins(CompetitiveEventDraftFilterAdministration filter = null);

    /// <summary>
    /// Deletes the cover image of a competitive event draft on behalf of a moderator.
    /// Only allowed if the draft exists, is editable, and contains a cover image.
    /// </summary>
    /// <param name="draftId">The ID of the competitive event draft.</param>
    /// <returns>A <see cref="Result{CompetitiveEventDraftResponseDto}"/> with the updated draft or error information.</returns>
    Task<Result<CompetitiveEventDraftResponseDto>> DeleteCoverImageAsModeratorAsync(Guid draftId);

    /// <summary>
    /// Deletes multiple images from a competitive event draft on behalf of a moderator.
    /// Also updates the draft status and logs all deletions.
    /// </summary>
    /// <param name="draftId">The ID of the competitive event draft.</param>
    /// <param name="imageIds">A collection of externalStorageIds of images to delete.</param>
    /// <returns>A <see cref="Result{CompetitiveEventDraftResponseDto}"/> with the updated draft or error information.</returns>
    Task<Result<CompetitiveEventDraftResponseDto>> DeleteImagesAsModeratorAsync(Guid draftId, IEnumerable<string> imageIds);
} 