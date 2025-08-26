using OutOfSchool.Common.Models;
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
    Task<Either<ErrorResponse, SectionCreateResponse>> CreateSectionAsync(SportsSectionPostRequest request);
}