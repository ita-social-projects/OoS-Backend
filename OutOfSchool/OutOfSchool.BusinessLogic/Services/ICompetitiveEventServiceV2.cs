using OutOfSchool.BusinessLogic.Models.CompetitiveEvent.V2;

namespace OutOfSchool.BusinessLogic.Services;
public interface ICompetitiveEventServiceV2 : ICompetitiveEventService
{
    /// <summary>
    /// Creates a new version 2 competitive event, including handling of image and cover uploads
    /// All operations are executed within a transaction
    /// </summary>
    /// <param name="dto">The DTO containing data for the new competitive event with images</param>
    Task<CompetitiveEventResultDto> CreateV2(CompetitiveEventV2CreateRequestDto dto);

    /// <summary>
    /// Updates an existing version 2 competitive event, including description changes, image updates, and cover image replacement
    /// All operations are executed within a transaction
    /// </summary>
    /// /// <param name="dto">The DTO containing updated data for the competitive event</param>
    /// <param name="fromDraft">Flag to signal if the updated value is taken from draft.</param>
    /// <returns>A result DTO containing the updated competitive event and results of the image updates</returns>
    Task<CompetitiveEventResultDto> UpdateV2(CompetitiveEventV2Dto dto, bool fromDraft = false);

    /// <summary>
    /// Deletes a competitive event and removes any associated images and cover image if present
    /// The operation is executed within a transaction
    /// </summary>
    /// <param name="id">The Id of the competitive event to be deleted</param>
    Task DeleteV2(Guid id);
}
