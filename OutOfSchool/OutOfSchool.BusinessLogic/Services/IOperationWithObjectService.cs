using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.BusinessLogic.Services;

/// <summary>
/// Defines interface for CRUD functionality for OperationWithObject entity.
/// </summary>
public interface IOperationWithObjectService
{
    /// <summary>
    /// Add entity.
    /// </summary>
    /// <param name="operationType">Operation type to add.</param>
    /// <param name="entityId">Id of entity to add.</param>
    /// <param name="entityType">Entity type.</param>
    /// <param name="eventDateTime">Date of the event.</param>
    /// <param name="rowSeparator">Separator if we need it.</param>
    /// <param name="comment">Comment.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    Task Create(
        OperationWithObjectOperationType operationType,
        Guid? entityId,
        OperationWithObjectEntityType? entityType,
        DateTimeOffset? eventDateTime = null,
        string rowSeparator = null,
        string comment = null);

    /// <summary>
    /// Delete the object from DB.
    /// </summary>
    /// <param name="id">Key of the OperationWithObject in table.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    Task Delete(Guid id);

    /// <summary>
    /// Get all operations with objects from the database.
    /// </summary>
    /// <param name="filter">Filter.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<IEnumerable<OperationWithObjectDto>> GetAll(OperationWithObjectFilter filter);

    /// <summary>
    /// Check if entity exists in DB.
    /// </summary>
    /// <param name="filter">Filter.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<bool> Exists(OperationWithObjectFilter filter);
}
