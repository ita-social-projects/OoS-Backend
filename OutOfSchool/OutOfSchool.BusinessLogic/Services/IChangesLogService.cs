using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Changes;
using OutOfSchool.Services.Models.WorkshopDrafts;

namespace OutOfSchool.BusinessLogic.Services;

/// <summary>
/// Defines interface for CRUD functionality for ChangesLog entity.
/// </summary>
public interface IChangesLogService
{
    /// <summary>
    /// Create and add ChangesLog records for the entity.
    /// </summary>
    /// <typeparam name="TEntity">Entity type that exists in the DB.</typeparam>
    /// <param name="entity">Modified entity.</param>
    /// <param name="userId">User ID.</param>
    /// <returns>Number of the added ChangesLog records.</returns>
    int AddEntityChangesToDbContext<TEntity>(TEntity entity, string userId)
        where TEntity : class, IKeyedEntity, new();

    /// <summary>
    /// Create and add ChangesLog records for the entity.
    /// </summary>
    /// <typeparam name="TEntity">Entity type that exists in the DB.</typeparam>
    /// <param name="entity">Modified entity.</param>
    /// <param name="userId">User ID.</param>
    /// <returns>True - if we did if.</returns>
    Task<bool> AddCreatingOfEntityToDbContext<TEntity>(TEntity entity, string userId)
        where TEntity : class, IKeyedEntity, new();

    /// <summary>
    /// Get Provider entities that match filter's parameters.
    /// </summary>
    /// <param name="request">Filter with specified searching parameters.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    /// The task result contains the <see cref="SearchResult{ProviderChangesLogDto}"/> that contains found elements.</returns>
    Task<SearchResult<ProviderChangesLogDto>> GetProviderChangesLogAsync(ProviderChangesLogRequest request);

    /// <summary>
    /// Get Application entities that match filter's parameters.
    /// </summary>
    /// <param name="request">Filter with specified searching parameters.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    /// The task result contains the <see cref="SearchResult{ApplicationChangesLogDto}"/> that contains found elements.</returns>
    Task<SearchResult<ApplicationChangesLogDto>> GetApplicationChangesLogAsync(ApplicationChangesLogRequest request);

    /// <summary>
    /// Get Employee entities that match filter's parameters.
    /// </summary>
    /// <param name="request">Filter with specified searching parameters.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    /// The task result contains the <see cref="SearchResult{ProviderAdminChangesLogDto}"/> that contains found elements.</returns>
    Task<SearchResult<EmployeeChangesLogDto>> GetEmployeeChangesLogAsync(EmployeeChangesLogRequest request);

    /// <summary>
    /// Get ParentBlockedByAdminLog entities that match filter's parameters.
    /// </summary>
    /// <param name="request">Filter with specified searching parameters.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    /// The task result contains the <see cref="SearchResult{ParentBlockedByAdminChangesLogDto}"/> that contains found elements.</returns>
    Task<SearchResult<ParentBlockedByAdminChangesLogDto>> GetParentBlockedByAdminChangesLogAsync(ParentBlockedByAdminChangesLogRequest request);

    /// <summary>
    /// Get Workshop logged entities that match filter's parameters.
    /// </summary>
    /// <param name="request">Filter with specified searching parameters.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    /// The task result contains the <see cref="SearchResult{WorkshopChangesLogDto}"/> that contains found elements.</returns>
    Task<SearchResult<WorkshopChangesLogDto>> GetWorkshopChangesLogAsync(WorkshopChangesLogRequest request);

    /// <summary>
    /// Retrieves a paginated list of changes for workshop drafts, filtered and scoped to the current user.
    /// </summary>
    /// <param name="request">Search and pagination parameters.</param>
    /// <returns>Search result containing change log entries for workshop drafts.</returns>
    Task<SearchResult<WorkshopDraftChangesLogDto>> GetWorkshopDraftChangesLogAsync(WorkshopDraftChangesLogRequest request);

    /// <summary>
    /// Logs changes between the old and new versions of a workshop draft content object.
    /// Handles both scalar properties and description item collections.
    /// </summary>
    /// <param name="oldContent">The original content before update.</param>
    /// <param name="newContent">The updated content after modification.</param>
    /// <param name="draftId">The ID of the modified workshop draft.</param>
    /// <param name="userId">The ID of the user who performed the change.</param>
    void LogWorkshopDraftChanges(
        WorkshopDraftContent oldContent,
        WorkshopDraftContent newContent,
        Guid draftId,
        string userId);

    /// <summary>
    /// Logs the deletion of one or more images from a workshop draft.
    /// </summary>
    /// <param name="oldImageIds">List of image IDs before deletion.</param>
    /// <param name="newImageIds">List of image IDs after deletion.</param>
    /// <param name="entityId">The ID of the entity (e.g., WorkshopDraft).</param>
    /// <param name="entityType">The type of the entity.</param>
    /// <param name="userId">The ID of the user who removed the images.</param>
    void LogImageDeletions(
        IEnumerable<string> oldImageIds,
        IEnumerable<string> newImageIds,
        Guid entityId,
        string entityType,
        string userId);
}