using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenIddict.Client;
using OutOfSchool.Common.Communication;
using OutOfSchool.Common.Communication.ICommunication;
using OutOfSchool.Common.Models;
using OutOfSchool.SportsRegistryApiClient.Config;
using OutOfSchool.SportsRegistryApiClient.Interfaces;
using OutOfSchool.SportsRegistryApiClient.Models;
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

    public Task<Either<ErrorResponse, SectionCreateUpdateResponse>> CreateSectionAsync(SportsSectionPostRequest request)
        => StartProcessAsync<SportsSectionPostRequest, SectionCreateUpdateResponse>(
            request,
            RegistryConstants.SectionCreateProcessKey);

    public Task<Either<ErrorResponse, SectionCreateUpdateResponse>> UpdateSectionAsync(SportsSectionUpdateRequest request)
        => StartProcessAsync<SportsSectionUpdateRequest, SectionCreateUpdateResponse>(
            request,
            RegistryConstants.SectionUpdateProcessKey);
    
    public async Task<Either<ErrorResponse, SportKindListResponse>> GetSportKindsAsync(int page = 0, int pageSize = 100)
    {
        var tokenResult = await GetAccessTokenAsync();

        var httpResult = await tokenResult
            .Map(accessToken => new Request
            {
                Url = new Uri($"{config.PlatformApiUrl}/api/public/data-factory/dict-sport-kinds?pageNo={page}&pageSize={pageSize}"),
                HttpMethodType = HttpMethodType.Get,
                Token = accessToken
            })
            .FlatMapAsync(request => communicationService.SendRequest<SportKindListResponse, ErrorResponse>(request))
            .ConfigureAwait(false);
        return httpResult;
    }
    public async Task<Either<ErrorResponse, SportsSectionListResponse>> GetSectionsAsync(int page = 0, int pageSize = 100)
    {
        var tokenResult = await GetAccessTokenAsync();

        var httpResult = await tokenResult
            .Map(accessToken => new Request
            {
                Url = new Uri($"{config.PlatformApiUrl}/api/public/data-factory/sections?pageNo={page}&pageSize={pageSize}"),
                HttpMethodType = HttpMethodType.Get,
                Token = accessToken
            })
            .FlatMapAsync(request => communicationService.SendRequest<SportsSectionListResponse, ErrorResponse>(request))
            .ConfigureAwait(false);
        return httpResult;
    }

    private async Task<Either<ErrorResponse, TResponse>> StartProcessAsync<TRequest, TResponse>(
        TRequest request,
        string processKey)
        where TResponse : ISectionResponse
    {
        var tokenResult = await GetAccessTokenAsync();

        var httpResult = await tokenResult
            .Map(accessToken => BuildRequest(request, accessToken, processKey))
            .FlatMapAsync(registryRequest =>
                communicationService.SendRequest<TResponse, ErrorResponse>(registryRequest))
            .ConfigureAwait(false);

        return httpResult.FlatMap(resp =>
            isRegistrySuccess(resp)
                ? (Either<ErrorResponse, TResponse>)resp
                : ToRegistryError(resp));
    }

    private Request BuildRequest<TRequest>(
        TRequest request,
        string accessToken,
        string processKey)
    {
        var payload = new
        {
            
            businessProcessDefinitionKey = processKey,
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
                Message = "Failed to get access token: ",
                Content = ex.Message,
            };
        }
    }
    #region ApiResponseInterpretation

    private static bool isRegistrySuccess<TResponse> (
        TResponse response)
        where TResponse : ISectionResponse
    {
        var codeStr = response?.ResultVariables?.Code;
        var errorRaw = response?.ResultVariables?.Errors;
        
        var hasErrors = !string.IsNullOrEmpty(errorRaw);
        
        return int.TryParse(codeStr, out var code)
        && code is >= 200 and < 300
        && !hasErrors;
    }

    private static ErrorResponse ToRegistryError<TResponse> (
        TResponse response) 
        where TResponse : ISectionResponse
    {
        var codeStr = response?.ResultVariables?.Code;
        var errorsRaw = response?.ResultVariables?.Errors;
        var details = NormalizeErrorsContent(errorsRaw);
        
        return new ErrorResponse
        {
            HttpStatusCode = MapToHttpStatusCode(codeStr),
            Message = "Sports Registry returned errors.",
            Content = details
        };
    }
    private static HttpStatusCode MapToHttpStatusCode(string? codeStr)
        => int.TryParse(codeStr, out var code) && Enum.IsDefined(typeof(HttpStatusCode), code)
            ? (HttpStatusCode)code
            : HttpStatusCode.BadRequest;
    
    private static string? NormalizeErrorsContent(
        string? errorsRaw)
    {
        if (string.IsNullOrWhiteSpace(errorsRaw))
            return null;

        try
        {
            using var doc = JsonDocument.Parse(errorsRaw);
            return JsonSerializer.Serialize(doc.RootElement, new JsonSerializerOptions
            {
                WriteIndented = true
            });
        }
        catch
        {
            return errorsRaw;
        }
    }

    #endregion
}
