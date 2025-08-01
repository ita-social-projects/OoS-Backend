
using System.Net.Mime;
using System.Text.Json;
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
    
    public async Task<SectionCreateResponse> CreateSectionAsync(SportsSectionPostRequest request)
    {
        if (request is null)
        {
            logger.LogWarning("CreateSectionAsync called with null request.");
            throw new ArgumentNullException(nameof(request));
        }

        logger.LogInformation("Attempting to send workshop to Sports Registry...");

        var accessToken = await GetAccessTokenAsync().ConfigureAwait(false);

        var payload = new
        {
            businessProcessDefinitionKey = RegistryConstants.BusinessProcessDefinitionKey,
            startVariables = new
            {
                data = request
            }
        };

        var registryRequest = new Request
        {
            Url = new Uri($"{config.ApiUrl}/api/gateway/business-process/api/start-bp"),
            HttpMethodType = HttpMethodType.Post,
            Token = accessToken,
            Data = payload,
        };

        logger.LogDebug("Sending request to external registry: {Url}", registryRequest.Url);

        var response = await communicationService.SendRequest<SectionCreateResponse, ErrorResponse>(registryRequest);

        if (response.TryGetRight(out var result))
        {
            var code = result.ResultVariables.Code;
            var errors = result.ResultVariables.Errors;

            if (!IsSuccessfulStatusCode(code))
            {
                logger.LogError("Registry returned non-success code: {Code}. Errors: {Errors}",
                    code, JsonSerializer.Serialize(errors));
                throw new InvalidOperationException($"Registry response code {code}. Errors: {JsonSerializer.Serialize(errors)}");
            }

            logger.LogInformation("Section successfully pushed. SectionId: {SectionId}", result.ResultVariables.SectionId);
            return result;
        }

        response.TryGetLeft(out var error);
        logger.LogError("Request failed. Status: {Status}. Message: {Message}. Body: {Body}",
            error?.HttpStatusCode, error?.Message, error?.ApiErrorResponse);

        throw new InvalidOperationException($"Registry push failed: {error?.Message ?? "Unknown error"}");
    }
    
    private async Task<string> GetAccessTokenAsync()
    {
        var registration = await openIddictClientService
            .GetClientRegistrationByProviderNameAsync("sportsregistry")
            .ConfigureAwait(false);

        var result = await openIddictClientService.AuthenticateWithClientCredentialsAsync(new()
        {
            RegistrationId = registration.RegistrationId
        });

        return result.AccessToken;
    }
    
    private static bool IsSuccessfulStatusCode(string? codeRaw)
    {
        return int.TryParse(codeRaw, out var statusCode) && statusCode is >= 200 and < 300;
    }
}
