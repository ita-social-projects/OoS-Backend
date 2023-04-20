using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services.AverageRatings;

public interface IAverageRatingService
{
    /// <summary>
    /// Calculate the average rating for all of the workshops and the providers.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task CalculateAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get average rating by workshop's or provider's id.
    /// </summary>
    /// <param name="entityId">Provider's or workshop's id.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<AverageRatingDto> GetByEntityIdAsync(Guid entityId);

    /// <summary>
    /// Get average ratings by the list of the workshop's or provider's ids.
    /// </summary>
    /// <param name="entityIds">List of the provider's or workshop's ids.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    Task<IEnumerable<AverageRatingDto>> GetByEntityIdsAsync(IEnumerable<Guid> entityIds);
}
