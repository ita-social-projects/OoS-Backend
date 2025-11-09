using OutOfSchool.Common.Models;
using OutOfSchool.SportsRegistryApiClient.Models.External;
using OutOfSchool.SportsRegistryApiClient.Models.Requests;

namespace OutOfSchool.SportsRegistryApiClient.Interfaces;

public interface ISportsRegistryWorkshopProvider
{
    /// <summary>
    /// Retrieves sports sections from the Sports Registry.
    /// Optionally filters by updated date range.
    /// </summary>
    /// <param name="updatedAtFrom">Lower bound of update date filter (inclusive).</param>
    /// <param name="updatedAtTo">Upper bound of update date filter (inclusive).</param>
    /// <param name="pageSize">Page size (default 50).</param>
    /// <returns>
    /// An <see cref="Either{TLeft, TRight}"/> containing either an <see cref="ErrorResponse"/> 
    /// if the request failed, or a list of <see cref="ExternalSportsSectionDto"/> 
    /// representing the updated sections.
    /// </returns>
    Task<Either<ErrorResponse, List<ExternalSportsSectionDto>>> GetAllSportsSectionsAsync(
        DateTimeOffset? updatedAtFrom = null,
        DateTimeOffset? updatedAtTo = null,
        int pageSize = 50);
}
