using System.Net.Http.Headers;
using System.Net.Mime;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenIddict.Client;
using OutOfSchool.AikomApiClient.Config;
using OutOfSchool.AikomApiClient.Extensions;
using OutOfSchool.AikomApiClient.Models;
using OutOfSchool.AikomApiClient.Models.Contract;
using OutOfSchool.AikomApiClient.Models.Requests;
using OutOfSchool.AikomApiClient.Models.Responses;
using OutOfSchool.Common;
using OutOfSchool.Common.Communication;
using OutOfSchool.Common.Communication.ICommunication;
using OutOfSchool.Common.Extensions;
using OutOfSchool.Common.Models;

namespace OutOfSchool.AikomApiClient;

/// <inheritdoc />
internal class AikomApiService : IAikomApiService
{
    private readonly ICommunicationService communicationService;
    private readonly OpenIddictClientService service;
    private readonly IHttpClientFactory httpClientFactory;
    private readonly ILogger<AikomApiService> logger;
    private readonly AikomApiClientConfig config;
    private readonly string apiUrl;

    public AikomApiService(
        ICommunicationService communicationService,
        OpenIddictClientService service,
        IHttpClientFactory httpClientFactory,
        ILogger<AikomApiService> logger,
        IOptions<AikomApiClientConfig> aikomOptions)
    {
        this.communicationService = communicationService;
        this.service = service;
        this.httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        config = aikomOptions?.Value
            ?? throw new ArgumentNullException(nameof(aikomOptions));
        apiUrl = config.ApiUrl;
    }

    /// <inheritdoc />
    public Task<Either<ErrorResponse, SearchUniversityResponseData>> SearchUniversity(string edrpou)
    {
        var request = new SearchUniversityRequest(edrpou);
        return PostAsyncWithBearerToken<SearchUniversityRequest, ApiResponse>(request)
            .FlatMapAsync(result => result.ToResponseData<SearchUniversityResponseData>());
    }

    /// <inheritdoc />
    public Task<Either<ErrorResponse, GetUniversityResponseData>> GetUniversity(long id)
    {
        var request = new GetUniversityRequest(id);
        return PostAsyncWithBearerToken<GetUniversityRequest, ApiResponse>(request)
            .FlatMapAsync(result => result.ToResponseData<GetUniversityResponseData>());
    }

    /// <inheritdoc />
    public Task<Either<ErrorResponse, GetUserResponseData>> GetUser(string rnokpp)
    {
        var request = new GetUserRequest(rnokpp);
        return PostAsyncWithBearerToken<GetUserRequest, ApiResponse>(request)
            .FlatMapAsync(result => result.ToResponseData<GetUserResponseData>());
    }

    private async Task<Either<ErrorResponse, TResponse>> PostAsyncWithBearerToken<TRequest, TResponse>(TRequest request)
        where TResponse : IResponse
    {
        var token = await GetAccessTokenAsync().ConfigureAwait(false);
        var req = new Request
        {
            Url = new Uri(apiUrl),
            Data = request,
            Token = token,
            HttpMethodType = HttpMethodType.Post,
        };
        return await communicationService.SendRequest<TResponse, ErrorResponse>(req).ConfigureAwait(false);
    }

    private async Task<string> GetAccessTokenAsync()
    {
        try
        {
            using var httpClient = httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));

            var requestBody = new List<KeyValuePair<string, string>>
            {
                new("client_id", config.ClientId),
                new("client_secret", config.ClientSecret),
                new("grant_type", config.GrantType)
            };

            using var content = new FormUrlEncodedContent(requestBody);
            content.Headers.ContentType = new MediaTypeHeaderValue(MediaTypeNames.Application.FormUrlEncoded);

            using var response = await httpClient.PostAsync(config.TokenEndpoint, content).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                logger.LogError(
                    "Failed to get access token from Keycloak. Status: {StatusCode}, Response: {Response}",
                    response.StatusCode,
                    errorBody);
                throw new HttpRequestException(
                    $"Failed to get access token from Keycloak. Status: {response.StatusCode}, Response: {errorBody}");
            }

            var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var tokenResponse = JsonSerializerHelper.Deserialize<TokenResponse>(responseBody);

            if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.AccessToken))
            {
                logger.LogError("Keycloak token response is null or access token is empty. Response: {Response}", responseBody);
                throw new InvalidOperationException("Keycloak token response is null or access token is empty.");
            }

            return tokenResponse.AccessToken;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting access token from Keycloak endpoint: {TokenEndpoint}", config.TokenEndpoint);
            throw;
        }
    }
}