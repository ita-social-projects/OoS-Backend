using System.Linq.Expressions;
using OutOfSchool.BusinessLogic.Models;

namespace OutOfSchool.BusinessLogic.Services;

/// <summary>
/// Defines interface for CRUD functionality for Rating entity.
/// </summary>
public interface IRatingService
{
    /// <summary>
    /// Add entity.
    /// </summary>
    /// <param name="dto">Rating entity to add.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<RatingDto> Create(RatingDto dto);

    /// <summary>
    /// Check if exists an any rewiewed application in workshop for parent.
    /// </summary>
    /// <param name="parentId">Parent's key.</param>
    /// <param name="workshopId">Workshop's key.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<bool> IsReviewed(Guid parentId, Guid workshopId);

    /// <summary>
    /// Get all rating entities.
    /// </summary>
    /// <param name="filter">Filter's key.</param>
    /// <returns>List of all rating records.</returns>
    Task<IEnumerable<RatingDto>> GetAsync(OffsetFilter filter);

    /// <summary>
    /// Get all rating entities by filter expression.
    /// </summary>
    /// <param name="filter">Filter expression.</param>
    /// <returns>List of all rating records.</returns>
    Task<IEnumerable<RatingDto>> GetAllAsync(Expression<Func<Rating, bool>> filter);

    /// <summary>
    /// Get rating entity by it's key.
    /// </summary>
    /// <param name="id">Key in the table.</param>
    /// <returns>Rating entity.</returns>
    Task<RatingDto> GetById(long id);

    /// <summary>
    /// Get all rating entities with specified Id and type.
    /// </summary>
    /// <param name="type">Entity type.</param>
    /// <param name="filter">Skip & Take number.</param>
    /// <returns>The result is a <see cref="SearchResult{RatingDto}"/> that contains the count of all found ratings and a list of ratings that were received.</returns>
    Task<SearchResult<RatingDto>> GetAllByEntityId(Guid entityId, OffsetFilter filter);

    /// <summary>
    /// Get all workshop rating by provider.
    /// </summary>
    /// <param name="id">Entity key.</param>
    /// <returns>List of all rating records relatet to provider.</returns>
    Task<IEnumerable<RatingDto>> GetAllWorshopsRatingByProvider(Guid id);

    /// <summary>
    /// Get parent rating for the specified entity.
    /// </summary>
    /// <param name="parentId">Parent key.</param>
    /// <param name="entityId">Entity key.</param>
    /// <returns>Parent rating for the specified entity.</returns>
    Task<RatingDto> GetParentRating(Guid parentId, Guid entityId);

    /// <summary>
    /// Update rating entity.
    /// </summary>
    /// <param name="dto">Rating entity to update.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<RatingDto> Update(RatingDto dto);

    /// <summary>
    ///  Delete rating entity.
    /// </summary>
    /// <param name="id">Rating key.</param>
    /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
    Task Delete(long id);
}