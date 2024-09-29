using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Changes;

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
}