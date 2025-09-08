using OutOfSchool.Common.Models;
using OutOfSchool.SportsRegistryApiClient.Models.Requests;
using OutOfSchool.SportsRegistryApiClient.Models.Responses;

namespace OutOfSchool.SportsRegistryApiClient.Interfaces;

public interface ISportsRegistryProviderService
{
    Task<Either<ErrorResponse, SectionCreateUpdateResponse>> RegisterSectionAsync(SportsSectionPostRequest request);
    Task<Either<ErrorResponse, SectionCreateUpdateResponse>> UpdateSectionAsync(SportsSectionUpdateRequest request);
}
