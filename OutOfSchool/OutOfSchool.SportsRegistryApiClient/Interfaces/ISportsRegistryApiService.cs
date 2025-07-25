namespace OutOfSchool.SportsRegistryApiClient.Interfaces;
using OutOfSchool.SportsRegistryApiClient.Models.Requests;
using OutOfSchool.SportsRegistryApiClient.Models.Responses;

public interface ISportsRegistryApiService
{
    /// <summary>
    /// Sends a new sports section to the external Ministry of Sports registry.
    /// </summary>
    /// <param name="request">The data required to create a sports section.</param>
    /// <returns>A response containing section registration status and process ID, if available.</returns>
    Task<SectionCreateResponse> CreateSectionAsync(SportsSectionPostRequest request);
}