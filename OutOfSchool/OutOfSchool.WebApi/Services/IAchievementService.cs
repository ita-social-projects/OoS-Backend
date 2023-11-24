using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.Achievement;

namespace OutOfSchool.WebApi.Services;

/// <summary>
/// Defines interface for CRUD functionality for Achievement entity.
/// </summary>
public interface IAchievementService
{
    /// <summary>
    /// To recieve the Achievement object with define id.
    /// </summary>
    /// <param name="id">Key in the table.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    /// The task result contains a <see cref="AchievementDto"/> that was found.</returns>
    Task<AchievementDto> GetById(Guid id);

    /// <summary>
    /// Get all Achievement objects  that match filter's parameters.
    /// </summary>
    /// <param name="filter">Filter with specified searching parametrs.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asybcronous operation.
    /// The task result contains a <see cref="SearchResult{AchievementDto}"/> that contains found elements.</returns>
    Task<SearchResult<AchievementDto>> GetByFilter(AchievementsFilter filter);

    /// <summary>
    /// Add new Achievement to the DB.
    /// </summary>
    /// <param name="dto">AchievementCreateDTO element.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    /// The task result contains a <see cref="AchievementCreateDTO"/> that was created.</returns>
    Task<AchievementDto> Create(AchievementCreateDTO dto);

    /// <summary>
    /// To Update our object in DB.
    /// </summary>
    /// <param name="dto">Achievement with new properties.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    /// The task result contains a <see cref="AchievementDto"/> that was updated.</returns>
    Task<AchievementDto> Update(AchievementCreateDTO dto);

    /// <summary>
    /// To delete the object from DB.
    /// </summary>
    /// <param name="id">Key of the Achievement in table.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task Delete(Guid id);
}