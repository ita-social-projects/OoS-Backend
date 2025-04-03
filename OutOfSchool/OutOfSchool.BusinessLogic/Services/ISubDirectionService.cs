using OutOfSchool.BusinessLogic.Common;
using OutOfSchool.BusinessLogic.Models;

namespace OutOfSchool.BusinessLogic.Services;

/// <summary>
/// Defines interface for CRUD functionality for Direction entity.
/// </summary>
public interface ISubDirectionService
{
    /// <summary>
    /// Get SubDirection objects from DB by filter.
    /// </summary>
    /// <param name="directionId">Id of the related direction.</param>
    /// <param name="filter">Filter for dtos.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    /// The task result contains a List of <see cref="SubDirectionDto"/> that were found.</returns>
    Task<SearchResult<SubDirectionDto>> GetByFilter(long directionId, SearchStringFilter filter);

    /// <summary>
    /// To recieve the SubDirection object with define id.
    /// </summary>
    /// <param name="id">Key in the table.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    /// The task result contains a <see cref="SubDirectionDto"/> that was found.</returns>
    Task<SubDirectionDto> GetById(long id);

    /// <summary>
    /// Add new Direction to the DB.
    /// </summary>
    /// <param name="directionId">Id of the related direction.</param>
    /// <param name="dto">DirectionDto element.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.
    /// The task result contains a <see cref="SubDirectionDto"/> that was created.</returns>
    Task<Result<SubDirectionDto>> Create(long directionId, SubDirectionDto dto);
}
