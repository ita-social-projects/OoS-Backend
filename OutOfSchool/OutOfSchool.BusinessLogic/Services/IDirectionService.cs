using OutOfSchool.BusinessLogic.Models;

namespace OutOfSchool.BusinessLogic.Services;

/// <summary>
/// Defines interface for CRUD functionality for Direction entity.
/// </summary>
public interface IDirectionService
{
    /// <summary>
    /// Get Direction objects from DB by filter.
    /// </summary>
    /// <param name="filter">Filter for DirectionDto.</param>
    /// <param name="isAdmins">True, if needs to retrieve information from admin panel.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    /// The task result contains a List of <see cref="DirectionDto"/> that were found.</returns>
    Task<SearchResult<DirectionDto>> GetByFilter(DirectionFilter filter, bool isAdmins);

    /// <summary>
    /// Add new Direction to the DB.
    /// </summary>
    /// <param name="dto">DirectionDto element.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    /// The task result contains a <see cref="DirectionDto"/> that was created.</returns>
    Task<DirectionDto> Create(DirectionDto dto);

    /// <summary>
    /// Get all Direction objects from DB.
    /// </summary>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    /// The task result contains a List of <see cref="DirectionDto"/> that were found.</returns>
    Task<IEnumerable<DirectionDto>> GetAll();

    /// <summary>
    /// To recieve the Direction object with define id.
    /// </summary>
    /// <param name="id">Key in the table.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    /// The task result contains a <see cref="DirectionDto"/> that was found.</returns>
    Task<DirectionDto> GetById(long id);

}