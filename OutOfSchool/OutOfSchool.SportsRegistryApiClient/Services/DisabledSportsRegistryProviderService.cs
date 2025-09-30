using System.Net;
using OutOfSchool.Common.Models;
using OutOfSchool.SportsRegistryApiClient.Interfaces;
using OutOfSchool.SportsRegistryApiClient.Models.Requests;
using OutOfSchool.SportsRegistryApiClient.Models.Responses;

namespace OutOfSchool.SportsRegistryApiClient.Services;
sealed class DisabledSportsRegistryStub : ISportsRegistrySectionProvider, ISportsRegistryDictionaryProvider
{
    static ErrorResponse Disabled(string op) => new()
    {
        HttpStatusCode = HttpStatusCode.ServiceUnavailable,
        Message = $"Sports Registry integration is disabled. Operation: {op}"
    };

    public Task<Either<ErrorResponse, SectionCreateUpdateResponse>> RegisterSectionAsync(SportsSectionPostRequest r)
        => Task.FromResult<Either<ErrorResponse, SectionCreateUpdateResponse>>(Disabled("CreateSection"));

    public Task<Either<ErrorResponse, SectionCreateUpdateResponse>> UpdateSectionAsync(SportsSectionUpdateRequest r)
        => Task.FromResult<Either<ErrorResponse, SectionCreateUpdateResponse>>(Disabled("UpdateSection"));
    
    public Task<Either<ErrorResponse, List<SportKindDto>>> GetAllSportKindsAsync(int pageSize = 50)
        => Task.FromResult<Either<ErrorResponse,List<SportKindDto>>> (Disabled("GetSportKinds"));

}