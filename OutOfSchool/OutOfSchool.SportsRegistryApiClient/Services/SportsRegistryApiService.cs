
using System.Net.Mime;
using Microsoft.Extensions.Options;
using OutOfSchool.Common.Communication;
using OutOfSchool.Common.Communication.ICommunication;
using OutOfSchool.Common.Models;
using OutOfSchool.SportsRegistryApiClient.Config;
using OutOfSchool.SportsRegistryApiClient.Interfaces;
using OutOfSchool.SportsRegistryApiClient.Models.Requests;
using OutOfSchool.SportsRegistryApiClient.Models.Responses;

namespace OutOfSchool.SportsRegistryApiClient.Services;

public class SportsRegistryApiService : ISportsRegistryApiService
{
    private readonly SportsRegistryApiClientConfig config;
    private readonly ICommunicationService communicationService;

    public SportsRegistryApiService(
        IOptions<SportsRegistryApiClientConfig> configOptions,
        ICommunicationService communicationService)
    {
        config = configOptions.Value ?? throw new ArgumentNullException(nameof(configOptions));
        this.communicationService = communicationService ?? throw new ArgumentNullException(nameof(communicationService));
    }


    public Task<SectionCreateResponse> CreateSectionAsync(SportsSectionPostRequest request)
    {
        throw new NotImplementedException();
    }
}
