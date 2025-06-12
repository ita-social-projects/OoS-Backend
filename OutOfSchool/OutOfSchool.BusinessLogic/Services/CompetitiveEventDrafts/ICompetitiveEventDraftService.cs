using OutOfSchool.BusinessLogic.Models.CompetitiveEventDraft;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.CompetitiveEvent.V2;

namespace OutOfSchool.BusinessLogic.Services.CompetitiveEventDrafts;
public interface ICompetitiveEventDraftService
{
    /// <summary>
    /// Creates a new competitive event draft.
    /// This method handles:
    /// - Mapping input data to the draft entity.
    /// - Saving the draft to the database.
    /// - Uploading associated images (competitive event images, cover image) in the external storage.
    /// </summary>
    /// <param name="competitiveEventV2Dto">
    /// Data transfer object containing information required to create the draft, 
    /// including competitive event details, and optional images.
    /// </param>
    /// <returns>
    /// A <see cref="CompetitiveEventDraftResultDto"/> containing the details of the created draft, 
    /// including any results or status from image processing operations.
    /// </returns>
    Task<CompetitiveEventDraftResultDto> Create(CompetitiveEventV2Dto competitiveEventV2Dto);

    /// <summary>
    /// Update existing workshop draft.   
    /// </summary>
    /// <param name="workshopDraftUpdateDto">Dto containing information required to update the draft.</param>   
    /// <returns>
    /// A <see cref="WorkshopDraftResultDto"/> containing the details of the updated draft, 
    /// including any results or status from image processing operations.
    /// </returns>
    Task<CompetitiveEventDraftResultDto> Update(CompetitiveEventDraftUpdateDto workshopDraftUpdateDto);

    /// <summary>
    /// Delete entity.
    /// </summary>
    /// <param name="id">Key in the table.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    Task Delete(Guid id);

    /// <summary>
    /// Send draft for moderation.
    /// </summary>
    /// <param name="id">Key in the table.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    Task SendForModeration(Guid id);

    /// <summary>
    /// Approve draft after moderating.
    /// </summary>
    /// <param name="id">Key in the table.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    Task Approve(Guid id);

    /// <summary>
    /// Reject draft after moderating.
    /// </summary>
    /// <param name="id">Key in the table.</param>
    /// <param name="rejectionMessage">A message explaining the reason for rejection.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    Task Reject(Guid id, string rejectionMessage);

    /// <summary>
    /// Get all competitive event drafts by provider Id.
    /// </summary>
    /// <param name="id">Provider's key.</param>
    /// <param name="filter">Filter to get a certain portion of all entities Or/And exclude by CompetitiveEvent id.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.
    /// The task result contains a <see cref="SearchResult{CompetitiveEventDraftViewCardDto}"/> that contains elements from the input sequence.</returns>
    Task<SearchResult<CompetitiveEventDraftViewCardDto>> GetByProviderId(Guid id, ExcludeIdFilter filter);

    /// <summary>
    /// Get all competitive event drafts by provider Id.
    /// </summary>
    /// <param name="id">Provider's key.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.
    /// The task result contains a <see cref="{CompetitiveEventDraftResponseDto}"/> that contains mapped CompetitiveEventDraft.</returns>
    Task<CompetitiveEventDraftResponseDto> GetCompetitiveEventDraftByIdMapped(Guid id);

    /// <summary>
    /// Creates new draft for competitive event or updates competitive event directly.   
    /// </summary>
    /// <param name="competitiveEventV2Dto">Dto containing information required to update the draft.</param>   
    /// <returns>
    /// A <see cref="Task"/> representing the result of the asynchronous operation.
    /// The task result contains <see cref="CompetitiveEventV2Dto"/> containing the details of the updated workshop.
    /// </returns>
    Task<CompetitiveEventV2Dto> UpdateCompetitiveEvent(CompetitiveEventV2Dto competitiveEventV2Dto);

    /// <summary>
    /// Get CompetitiveEventDraft Id by CompetitiveEvent Id.   
    /// </summary>
    /// <param name="competitiveEventId">CompetitiveEvent Id.</param>   
    /// <returns>
    /// A <see cref="Guid"/> representing the CompetitiveEventDraft Id, or null if no matching CompetitiveEventDraft exists.
    /// </returns>
    Task<Guid?> GetCompetitiveEventDraftIdByCompetitiveEventId(Guid competitiveEventId);
}
