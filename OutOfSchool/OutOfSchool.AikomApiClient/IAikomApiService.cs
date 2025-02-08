using OutOfSchool.AikomApiClient.Models.Responses;
using OutOfSchool.Common.Models;

namespace OutOfSchool.AikomApiClient;

/// <summary>
/// Service for direct communication with the external Aikom API.
/// Provides low-level operations for searching and retrieving university/provider information.
/// </summary>
internal interface IAikomApiService
{
    /// <summary>
    /// Searches for a university/provider in the external registry using EDRPOU code.
    /// </summary>
    /// <param name="edrpou">The EDRPOU code to search for.</param>
    /// <returns>
    /// A task that represents the asynchronous operation, containing <see cref="Either{TL, TR}"/>:
    /// <list type="bullet">
    /// <item>An error response if the search fails</item>
    /// <item>University/provider data if found, including its ID and basic information</item>
    /// </list>
    /// </returns>
    Task<Either<ErrorResponse, SearchUniversityResponseData>> SearchUniversity(string edrpou);

    /// <summary>
    /// Retrieves detailed university/provider information from the external registry using its ID.
    /// </summary>
    /// <param name="id">The ID of the university/provider in the external registry.</param>
    /// <returns>
    /// A task that represents the asynchronous operation, containing <see cref="Either{TL, TR}"/>:
    /// <list type="bullet">
    /// <item>An error response if the retrieval fails</item>
    /// <item>Detailed university/provider data including management information</item>
    /// </list>
    /// </returns>
    Task<Either<ErrorResponse, GetUniversityResponseData>> GetUniversity(long id);
}