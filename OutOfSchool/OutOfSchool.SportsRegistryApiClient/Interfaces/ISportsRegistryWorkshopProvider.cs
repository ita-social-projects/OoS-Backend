using OutOfSchool.Common.Models;
using OutOfSchool.SportsRegistryApiClient.Models.External;
using OutOfSchool.SportsRegistryApiClient.Models.Requests;

namespace OutOfSchool.SportsRegistryApiClient.Interfaces;

public interface ISportsRegistryWorkshopProvider
{
    /// <summary>
    /// Retrieves all workshops (sports sections).
    /// </summary>
    /// <param name="pageSize">
    ///     The number of records to fetch per page (default is 50).
    /// </param>
    /// <returns>
    /// An <see cref="Either{TLeft, TRight}"/> containing either an <see cref="ErrorResponse"/> 
    /// if the request failed, or a list of <see cref="SportsSectionUpdateRequest"/> 
    /// representing the updated workshops.
    /// </returns>
    Task<Either<ErrorResponse, List<ExternalSportsSectionDto>>> GetAllSportsSectionsAsync(int pageSize = 50);
}
