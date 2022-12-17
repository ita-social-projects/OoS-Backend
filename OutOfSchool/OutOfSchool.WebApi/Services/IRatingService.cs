﻿using OutOfSchool.Services.Enums;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services;

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
    /// Get rating entity by it's key.
    /// </summary>
    /// <param name="id">Key in the table.</param>
    /// <returns>Rating entity.</returns>
    Task<RatingDto> GetById(long id);

    /// <summary>
    /// Get all rating entities with specified Id and type.
    /// </summary>
    /// <param name="entityId">Entity key.</param>
    /// <param name="type">Entity type.</param>
    /// <param name="filter">Skip & Take number.</param>
    /// <returns>The result is a <see cref="SearchResult{RatingDto}"/> that contains the count of all found ratings and a list of ratings that were received.</returns>
    Task<SearchResult<RatingDto>> GetAllByEntityId(Guid entityId, RatingType type, OffsetFilter filter);

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
    /// <param name="type">Entity type.</param>
    /// <returns>Parent rating for the specified entity.</returns>
    Task<RatingDto> GetParentRating(Guid parentId, Guid entityId, RatingType type);

    /// <summary>
    /// Get average entity rating.
    /// </summary>
    /// <param name="entityId">Entity key.</param>
    /// <param name="type">Entity type.</param>
    /// <returns>Average rating of entity.</returns>
    Task<Tuple<float, int>> GetAverageRatingAsync(Guid entityId, RatingType type);

    /// <summary>
    /// Get average rating for entities range.
    /// </summary>
    /// <param name="entities">Entities keys.</param>
    /// <param name="type">Entity type.</param>
    /// <returns>Average rating of entities range.</returns>
    Task<Dictionary<Guid, Tuple<float, int>>> GetAverageRatingForRangeAsync(IEnumerable<Guid> entities, RatingType type);

    /// <summary>
    /// Get average rating for provider.
    /// </summary>
    /// <param name="providerId">Provider's key.</param>
    /// <returns>Average rating for provider.</returns>
    Task<Tuple<float, int>> GetAverageRatingForProviderAsync(Guid providerId);

    /// <summary>
    /// Get average rating for each provider in the range.
    /// </summary>
    /// <param name="providerIds">Providers' keys.</param>
    /// <returns>Average rating for each provider in the range.</returns>
    Task<Dictionary<Guid, Tuple<float, int>>> GetAverageRatingForProvidersAsync(IEnumerable<Guid> providerIds);

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