
using System.Net.Mime;
using Microsoft.Extensions.Options;
using OutOfSchool.Common.Communication;
using OutOfSchool.Common.Communication.ICommunication;
using OutOfSchool.Common.Models;
using OutOfSchool.SportsRegistryApiClient.Config;
using OutOfSchool.SportsRegistryApiClient.Interfaces;
using OutOfSchool.SportsRegistryApiClient.Models.Requests;
using OutOfSchool.SportsRegistryApiClient.Models.Responses;
using OutOfSchool.Common.Extensions;

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

    private async Task<string> GetAccessTokenAsync()
    {
        var tokenRequest = new Request()
        {
            Url = new Uri(config.TokenUrl),
            HttpMethodType = HttpMethodType.Post,
            Data = new Dictionary<string, string>
            {
                { "client_id", config.ClientId },
                { "client_secret", config.ClientSecret },
                { "grant_type", "client_credentials" }
            },
            Headers = new List<KeyValuePair<string, string>>
            {
                new("Content-Type", MediaTypeNames.Application.FormUrlEncoded)
            }
        };
        var response = await communicationService.SendRequest<TokenResponse, ErrorResponse>(tokenRequest);
        
        if (response.TryGetRight(out var token))
        {
            return token.AccessToken;
        }
        response.TryGetLeft(out var error);
        throw new InvalidOperationException($"Failed to obtain access token: {error?.Message ?? "Unknown error"}");
    }
}
