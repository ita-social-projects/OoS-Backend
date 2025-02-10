using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.WorkshopDraft;
using OutOfSchool.BusinessLogic.Models.Workshops;

namespace OutOfSchool.BusinessLogic.Services.WorkshopDrafts;
/// <summary>
/// Provides methods to create and manipulate drafts of workshops 
/// before they are finalized or published.
/// </summary>
public interface IWorkshopDraftService
{
    /// <summary>
    /// Creates a new workshop draft.
    /// This method handles:
    /// - Mapping input data to the draft entity.
    /// - Saving the draft to the database.
    /// - Uploading associated images (workshop images, cover image, teachers images) in the external storage.
    /// - Associating teachers and processing their data.
    /// </summary>
    /// <param name="workshopV2Dto">
    /// Data transfer object containing information required to create the draft, 
    /// including workshop details, teacher details, and optional images.
    /// </param>
    /// <returns>
    /// A <see cref="WorkshopDraftResultDto"/> containing the details of the created draft, 
    /// including any results or status from image processing operations.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if the provided <paramref name="workshopV2Dto"/> is null.
    /// </exception>
    /// <exception cref="InvalidDataException">
    /// Thrown when the workshop does not contain a list of teachers or the list is empty.
    /// </exception>
    Task<WorkshopDraftResultDto> Create(WorkshopV2Dto workshopV2Dto);

    /// <summary>
    /// Update existing workshop draft.   
    /// </summary>
    /// <param name="workshopDraftUpdateDto">Dto containing information required to update the draft.</param>   
    /// <returns>
    /// A <see cref="WorkshopDraftResultDto"/> containing the details of the updated draft, 
    /// including any results or status from image processing operations.
    /// </returns>
    Task<WorkshopDraftResultDto> Update(WorkshopDraftUpdateDto workshopDraftUpdateDto);

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
    /// Get all workshop drafts by provider Id.
    /// </summary>
    /// <param name="id">Provider's key.</param>
    /// <param name="filter">Filter to get a certain portion of all entities Or/And exclude by Workshop id.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.
    /// The task result contains a <see cref="SearchResult{WorkshopDraftResponseDto}"/> that contains elements from the input sequence.</returns>
    Task<SearchResult<WorkshopDraftResponseDto>> GetByProviderId(Guid id, ExcludeIdFilter filter);
}
