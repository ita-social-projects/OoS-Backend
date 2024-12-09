using OutOfSchool.BusinessLogic.Models.CompetitiveEvent;

namespace OutOfSchool.BusinessLogic.Services;

/// <summary>
/// Defines interface for CRUD functionality for CompetitiveEvent entity.
/// </summary>
public interface ICompetitiveEventService
{
    /// <summary>
    /// Get entity by it's key.
    /// </summary>
    /// <param name="id">Key in the table.</param>
    /// <returns>CompetitiveEvent or null if event not found.</returns>
    Task<CompetitiveEventDto?> GetById(Guid id);

    /// <summary>
    /// Add entity.
    /// </summary>
    /// <param name="dto">CompetitiveEvent entity to add.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<CompetitiveEventDto> Create(CompetitiveEventCreateDto dto);

    /// <summary>
    /// Update entity.
    /// </summary>
    /// <param name="dto">CompetitiveEvent entity to add.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<CompetitiveEventDto> Update(CompetitiveEventUpdateDto dto);

    /// <summary>
    ///  Delete entity.
    /// </summary>
    /// <param name="id">CompetitiveEvent key.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    Task Delete(Guid id);
}
