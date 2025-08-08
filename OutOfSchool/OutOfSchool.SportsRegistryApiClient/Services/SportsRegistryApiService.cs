using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenIddict.Client;
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
    private readonly OpenIddictClientService openIddictClientService;
    private readonly ICommunicationService communicationService;
    private readonly ILogger<SportsRegistryApiService> logger;

    public SportsRegistryApiService(
        IOptions<SportsRegistryApiClientConfig> configOptions,
        OpenIddictClientService openIddictClientService,
        ICommunicationService communicationService,
        ILogger<SportsRegistryApiService> logger)
    {
        config = configOptions.Value ?? throw new ArgumentNullException(nameof(configOptions));
        this.openIddictClientService = openIddictClientService ?? throw new ArgumentNullException(nameof(openIddictClientService));
        this.communicationService = communicationService ?? throw new ArgumentNullException(nameof(communicationService));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Either<ErrorResponse, SectionCreateResponse>> CreateSectionAsync(SportsSectionPostRequest request)
    {
        if (request is null)
        {
            return new ErrorResponse
            {
                HttpStatusCode = HttpStatusCode.BadRequest,
                Message = "Request is null"
            };
        }

        var tokenResult = await GetAccessTokenAsync();

        return await tokenResult
            .Map(accessToken => BuildRequest(request, accessToken))
            .FlatMapAsync(registryRequest =>
                communicationService.SendRequest<SectionCreateResponse, ErrorResponse>(registryRequest)).ConfigureAwait(false);
    }

    private Request BuildRequest(SportsSectionPostRequest request, string accessToken)
    {
        var payload = new
        {
            businessProcessDefinitionKey = RegistryConstants.BusinessProcessDefinitionKey,
            startVariables = new
            {
                data = request
            }
        };

        return new Request
        {
            Url = new Uri($"{config.ApiUrl}/api/gateway/business-process/api/start-bp"),
            HttpMethodType = HttpMethodType.Post,
            Token = accessToken,
            Data = payload
        };
    }

    private async Task<Either<ErrorResponse, string>> GetAccessTokenAsync()
    {
        try
        {
            var registration = await openIddictClientService
                .GetClientRegistrationByProviderNameAsync("sportsregistry")
                .ConfigureAwait(false);

            var result = await openIddictClientService.AuthenticateWithClientCredentialsAsync(new()
            {
                RegistrationId = registration.RegistrationId
            }).ConfigureAwait(false);

            return result.AccessToken;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get access token from Sports Registry");
            return new ErrorResponse
            {
                HttpStatusCode = HttpStatusCode.InternalServerError,
                Message = "Failed to get access token",
                Content = ex.Message
            };
        }
    }
}
