using OutOfSchool.AikomApiClient.Models.Data;
using OutOfSchool.Common.Models;

namespace OutOfSchool.AikomApiClient;

/// <summary>
/// Service for verifying provider and director access against external registry.
/// </summary>
public interface IAikomProviderService
{
    /// <summary>
    /// Verifies provider and director access by searching the provider in external registry using EDRPOU
    /// and checking if the provided RNOKPP matches the director's RNOKPP.
    /// </summary>
    /// <param name="providerEdrpou">The EDRPOU code of the provider to verify.</param>
    /// <param name="directorRnokpp">The RNOKPP of the director to verify.</param>
    /// <param name="cacheKeyPrefix">Optional prefix for the cache key. If not provided, default prefix will be used.</param>
    /// <param name="cacheExpiration">Optional cache expiration time. If not provided, default expiration will be used.</param>
    /// <returns>
    /// A task that represents the asynchronous operation, containing <see cref="Either{TL, TR}"/>:
    /// <list type="bullet">
    /// <item>An error response if the verification fails</item>
    /// <item>Provider information with access status if verification succeeds</item>
    /// The result may be cached for subsequent requests with the same parameters.
    /// </list>
    /// </returns>
    Task<Either<ErrorResponse, AikomProviderResponse?>> VerifyProviderAndDirectorAccess(
        string providerEdrpou,
        string directorRnokpp,
        string? cacheKeyPrefix = null,
        TimeSpan? cacheExpiration = null);
    
    /// <summary>
    /// Verifies director access by checking if the provided RNOKPP matches the director's RNOKPP
    /// for a provider with the specified external registry ID.
    /// </summary>
    /// <param name="externalRegistryProviderId">The ID of the provider in the external registry.</param>
    /// <param name="directorRnokpp">The RNOKPP of the director to verify.</param>
    /// <param name="cacheKeyPrefix">Optional prefix for the cache key. If not provided, default prefix will be used.</param>
    /// <param name="cacheExpiration">Optional cache expiration time. If not provided, default expiration will be used.</param>
    /// <returns>
    /// A task that represents the asynchronous operation, containing <see cref="Either{TL, TR}"/>:
    /// <list type="bullet">
    /// <item>An error response if the verification fails</item>
    /// <item>Provider information with access status if verification succeeds</item>
    /// </list>
    /// The result may be cached for subsequent requests with the same parameters.
    /// </returns>
    Task<Either<ErrorResponse, AikomProviderResponse?>> VerifyDirectorAccess(
        long externalRegistryProviderId,
        string directorRnokpp,
        string? cacheKeyPrefix = null,
        TimeSpan? cacheExpiration = null);
}