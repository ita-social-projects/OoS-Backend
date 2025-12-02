using OutOfSchool.BusinessLogic.Models.Workshops;
using OutOfSchool.Services.Models.WorkshopDrafts;

namespace OutOfSchool.BusinessLogic.Services.SportsRegistry;

/// <summary>
/// Defines operations for synchronizing workshops and drafts with the external Sports Registry system.
/// Provides a unified abstraction for handling registry synchronization
/// independently of the source entity type (WorkshopDraft or WorkshopV2Dto).
/// </summary>
public interface IRegistrySyncService
{
    /// <summary>
    /// Synchronizes the specified workshop draft with the external Sports Registry.
    /// Determines whether the draft should be created or updated in the registry,
    /// builds and normalizes the request, sends it to the registry API,
    /// and updates the draft with the registry SectionId if successful.
    /// </summary>
    /// <param name="draft">The workshop draft to be synchronized.</param>
    /// <returns>
    /// A <see cref="Task"/> representing the asynchronous operation.
    /// Throws <see cref="InvalidOperationException"/> if the draft is invalid
    /// or if synchronization with the registry fails.
    /// </returns>
    Task SyncDraftAsync(WorkshopDraft draft);

    /// <summary>
    /// Synchronizes the specified workshop with the external Sports Registry.
    /// Intended for direct updates when moderated fields are not changed
    /// and approval flow is not required.
    /// </summary>
    /// <param name="workshopDto">The workshop to be synchronized.</param>
    /// <returns>
    /// A <see cref="Task"/> representing the asynchronous operation.
    /// Throws <see cref="InvalidOperationException"/> if the workshop data is invalid
    /// or if synchronization with the registry fails.
    /// </returns>
    Task SyncWorkshopAsync(WorkshopV2Dto workshopDto);
}
