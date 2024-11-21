using OutOfSchool.BusinessLogic.Models.WorkshopDraft;

namespace OutOfSchool.BusinessLogic.Services.WorkshopDrafts;
/// <summary>
/// Provides methods to create and manipulate drafts of workshops 
/// before they are finalized or published.
/// </summary>
public interface IWorkshopDraftService
{
    /// <summary>
    /// Creates a new workshop draft based on the provided data.
    /// This method handles:
    /// - Mapping input data to the draft entity.
    /// - Saving the draft to the database.
    /// - Uploading associated images (workshop images, cover image).
    /// - Associating teachers and processing their data.
    /// </summary>
    /// <param name="workshopDraftDto">
    /// Data transfer object containing information required to create the draft, 
    /// including workshop details, teacher details, and optional images.
    /// </param>
    /// <returns>
    /// A <see cref="WorkshopDraftResultDto"/> containing the details of the created draft, 
    /// including any results or status from image processing operations.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if the provided <paramref name="workshopDraftDto"/> is null.
    /// </exception>
    Task<WorkshopDraftResultDto> Create(WorkshopDraftCreateDto workshopDraftDto);
}
