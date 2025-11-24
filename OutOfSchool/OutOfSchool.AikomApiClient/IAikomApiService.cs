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
    /// Searches for a university/provider in the external registry using EDRPOU (tax identification number).
    /// </summary>
    /// <param name="edrpou">The EDRPOU (tax identification number) of the university/provider.</param>
    /// <returns>
    /// A task that represents the asynchronous operation, containing <see cref="Either{TL, TR}"/>:
    /// <list type="bullet">
    /// <item>An error response if the search fails</item>
    /// <item>University data including id, full name, and EDRPOU</item>
    /// </list>
    /// </returns>
    Task<Either<ErrorResponse, SearchUniversityResponseData>> SearchUniversity(string edrpou);

    /// <summary>
    /// Retrieves detailed university/provider information from the external registry using the university ID.
    /// </summary>
    /// <param name="id">The ID of the university/provider in the external registry.</param>
    /// <returns>
    /// A task that represents the asynchronous operation, containing <see cref="Either{TL, TR}"/>:
    /// <list type="bullet">
    /// <item>An error response if the retrieval fails</item>
    /// <item>University data including full name, address, contact information, and director details</item>
    /// </list>
    /// </returns>
    Task<Either<ErrorResponse, GetUniversityResponseData>> GetUniversity(long id);

    /// <summary>
    /// Retrieves user information from the external registry using RNOKPP (tax identification number).
    /// </summary>
    /// <param name="rnokpp">The RNOKPP (tax identification number) of the user.</param>
    /// <returns>
    /// A task that represents the asynchronous operation, containing <see cref="Either{TL, TR}"/>:
    /// <list type="bullet">
    /// <item>An error response if the retrieval fails</item>
    /// <item>User data including userCommonId, userStatus, organizationId, and userRole</item>
    /// </list>
    /// </returns>
    Task<Either<ErrorResponse, GetUserResponseData>> GetUser(string rnokpp);
}