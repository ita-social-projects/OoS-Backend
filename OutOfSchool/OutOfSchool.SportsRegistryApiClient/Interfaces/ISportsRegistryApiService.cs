using OutOfSchool.Common.Models;
using OutOfSchool.SportsRegistryApiClient.Models.External;
using OutOfSchool.SportsRegistryApiClient.Models.Requests;
using OutOfSchool.SportsRegistryApiClient.Models.Responses;

namespace OutOfSchool.SportsRegistryApiClient.Interfaces;
public interface ISportsRegistryApiService
{
    /// <summary>
    /// Sends a new sports section to the external Ministry of Sports registry.
    /// </summary>
    /// <param name="request">The data required to create a sports section.</param>
    /// <returns>A response containing section registration status and process ID, if available.</returns>
    Task<Either<ErrorResponse, SectionCreateUpdateResponse>> CreateSectionAsync(SportsSectionPostRequest request);
    
    /// <summary>
    /// Update a sport section in the external Ministry of Sports registry
    /// </summary>
    /// <param name="request">The data required to create a sports section.</param>
    /// <returns>A response containing section update status and process ID, if available.</returns>
    Task<Either<ErrorResponse, SectionCreateUpdateResponse>> UpdateSectionAsync(SportsSectionUpdateRequest request);
    
    // <summary>
    /// Gets the list of sport kinds (dictionary) from the Sports Registry.
    /// </summary>
    /// <returns>A collection of sport kinds with their codes, names, and metadata.</returns>
    Task<Either<ErrorResponse, SportKindListResponse>> GetSportKindsAsync(int page = 0, int pageSize = 100);

    /// <summary>
    /// Retrieves a paged list of sports sections (workshops) from the Sports Registry.
    /// </summary>
    /// <returns>
    /// A paged response containing the sports sections 
    /// or an <see cref="ErrorResponse"/> if the request fails.
    /// </returns>
    Task<Either<ErrorResponse, SportsSectionListResponse>> GetSectionsAsync(int page = 0, int pageSize = 100, ExternalSportsSectionFilter? filter = null);
}