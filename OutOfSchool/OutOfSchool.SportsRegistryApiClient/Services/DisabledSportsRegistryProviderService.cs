using System.Net;
using OutOfSchool.Common.Models;
using OutOfSchool.SportsRegistryApiClient.Interfaces;
using OutOfSchool.SportsRegistryApiClient.Models.Requests;
using OutOfSchool.SportsRegistryApiClient.Models.Responses;

sealed class DisabledSportsRegistryProviderService : ISportsRegistryProviderService
{
    static ErrorResponse Disabled(string op) => new()
    {
        HttpStatusCode = HttpStatusCode.ServiceUnavailable,
        Message = $"Sports Registry integration is disabled. Operation: {op}"
    };

    public Task<Either<ErrorResponse, SectionCreateResponse>> RegisterSectionAsync(SportsSectionPostRequest r)
        => Task.FromResult<Either<ErrorResponse, SectionCreateResponse>>(Disabled("CreateSection"));

    public Task<Either<ErrorResponse, SectionCreateResponse>> UpdateSectionAsync(SportsSectionPostRequest r)
        => Task.FromResult<Either<ErrorResponse, SectionCreateResponse>>(Disabled("UpdateSection"));
}