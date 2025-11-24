using System.Net.Http.Headers;
using System.Net.Mime;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OutOfSchool.AikomApiClient.Config;
using OutOfSchool.AikomApiClient.Models;
using OutOfSchool.AikomApiClient.Models.Data;
using OutOfSchool.Common;
using OutOfSchool.Common.Extensions;
using OutOfSchool.Common.Models;
using OutOfSchool.Redis;

namespace OutOfSchool.AikomApiClient;

/// <inheritdoc />
internal class AikomProviderService : IAikomProviderService
{
    private readonly IAikomApiService aikomApiService;
    private readonly IReadWriteCacheService cacheService;
    private readonly ILogger<AikomProviderService> logger;
    private readonly IHttpClientFactory httpClientFactory;
    private readonly AikomApiClientConfig config;

    private const string DefaultCacheKeyPrefix = "aikom:provider:";
    private const string EdrpouKeyPart = "edrpou";
    private const string IdKeyPart = "id";
    private static readonly TimeSpan DefaultCacheExpiration = TimeSpan.FromMinutes(5);

    public AikomProviderService(
        IAikomApiService aikomApiService,
        IReadWriteCacheService cacheService,
        ILogger<AikomProviderService> logger,
        IHttpClientFactory httpClientFactory,
        IOptions<AikomApiClientConfig> configOptions)
    {
        this.aikomApiService = aikomApiService;
        this.cacheService = cacheService;
        this.logger = logger;
        this.httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        config = configOptions?.Value ?? throw new ArgumentNullException(nameof(configOptions));
    }

    /// <inheritdoc />
    public async Task<Either<ErrorResponse, AikomProviderResponse?>> VerifyProviderAndDirectorAccess(
        string providerEdrpou,
        string directorRnokpp,
        string? cacheKeyPrefix = null,
        TimeSpan? cacheExpiration = null)
    {
        var cacheKey = $"{cacheKeyPrefix ?? DefaultCacheKeyPrefix}{EdrpouKeyPart}:{providerEdrpou}:{directorRnokpp}";

        try
        {
            var cachedValue = await cacheService.ReadAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedValue))
            {
                return JsonSerializerHelper.Deserialize<AikomProviderResponse>(cachedValue);
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to read from cache for key: {CacheKey}", cacheKey);
        }

        var result = await VerifyProviderAndDirectorAccessInternal(providerEdrpou, directorRnokpp);

        try
        {
            await result.Match(
                _ => Task.CompletedTask,
                async success => await cacheService.WriteAsync(
                    cacheKey,
                    JsonSerializerHelper.Serialize(success),
                    cacheExpiration ?? DefaultCacheExpiration));
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to write to cache for key: {CacheKey}", cacheKey);
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<Either<ErrorResponse, AikomProviderResponse?>> VerifyDirectorAccess(
        long externalRegistryProviderId,
        string directorRnokpp,
        string? cacheKeyPrefix = null,
        TimeSpan? cacheExpiration = null)
    {
        var cacheKey = $"{cacheKeyPrefix ?? DefaultCacheKeyPrefix}{IdKeyPart}:{externalRegistryProviderId}:{directorRnokpp}";

        try
        {
            var cachedValue = await cacheService.ReadAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedValue))
            {
                return JsonSerializerHelper.Deserialize<AikomProviderResponse>(cachedValue);
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to read from cache for key: {CacheKey}", cacheKey);
        }

        var result = await VerifyDirectorAccessInternal(externalRegistryProviderId, directorRnokpp);

        try
        {
            await result.Match(
                _ => Task.CompletedTask,
                async success => await cacheService.WriteAsync(
                    cacheKey,
                    JsonSerializerHelper.Serialize(success),
                    cacheExpiration ?? DefaultCacheExpiration));
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to write to cache for key: {CacheKey}", cacheKey);
        }

        return result;
    }

    private async Task<Either<ErrorResponse, AikomProviderResponse?>> VerifyProviderAndDirectorAccessInternal(
        string providerEdrpou,
        string directorRnokpp)
    {
        try
        {
            var aikomResponse = await aikomApiService
                .SearchUniversity(providerEdrpou)
                .FlatMapAsync(searchResult => aikomApiService
                    .GetUniversity(searchResult.Id)
                    .MapAsync(university => (searchResult.Id, University: university)))
                .ConfigureAwait(false);

            return aikomResponse.Match<Either<ErrorResponse, AikomProviderResponse?>>(
                error =>
                {
                    var apiError = error.ApiErrorResponse?.ApiErrors?.Count > 0 ? error.ApiErrorResponse.ApiErrors[0] : null;
                    logger.LogError(
                        "Error while verifying provider in external registry: Code - {Code}, Message - {Message}",
                        apiError?.Code ?? error.HttpStatusCode.ToString(),
                        apiError?.Message ?? error.Message ?? string.Empty);
                    return error;
                },
                result =>
                {
                    var isDirector = result.University.UniversityBoss.BossRnokpp == directorRnokpp;
                    return new AikomProviderResponse
                    {
                        HasAccess = isDirector,
                        ProviderInfo = new AikomProviderInfo
                        {
                            Id = result.Id,
                            FullName = result.University.FullName,
                            ShortName = result.University.ShortName,
                            Address = result.University.Address,
                            Email = result.University.Email,
                            Phone = result.University.Phone,
                        }
                    };
                });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error verifying organization access for EDRPOU: {Edrpou}", providerEdrpou);
            throw;
        }
    }

    private async Task<Either<ErrorResponse, AikomProviderResponse?>> VerifyDirectorAccessInternal(
        long externalRegistryProviderId,
        string directorRnokpp)
    {
        try
        {
            var aikomResponse = await aikomApiService
                .GetUniversity(externalRegistryProviderId)
                .ConfigureAwait(false);

            return aikomResponse.Match<Either<ErrorResponse, AikomProviderResponse?>>(
                error =>
                {
                    var apiError = error.ApiErrorResponse.ApiErrors.Count > 0 ? error.ApiErrorResponse.ApiErrors[0] : null;
                    logger.LogError(
                        "Error while verifying provider in external registry: Code - {Code}, Message - {Message}",
                        apiError?.Code ?? "Unknown",
                        apiError?.Message ?? string.Empty);
                    return error;
                },
                university =>
                {
                    var isDirector = university.UniversityBoss.BossRnokpp == directorRnokpp;
                    return new AikomProviderResponse
                    {
                        HasAccess = isDirector,
                        ProviderInfo = new AikomProviderInfo
                        {
                            Id = externalRegistryProviderId,
                            FullName = university.FullName,
                            ShortName = university.ShortName,
                            Address = university.Address,
                            Email = university.Email,
                            Phone = university.Phone,
                        }
                    };
                });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error verifying director access for Provider: {ExternalProviderId}",
                externalRegistryProviderId);
            throw;
        }
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
                    "Failed to get access token. Status: {StatusCode}, Response: {Response}",
                    response.StatusCode,
                    errorBody);
                throw new HttpRequestException(
                    $"Failed to get access token. Status: {response.StatusCode}, Response: {errorBody}");
            }

            var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var tokenResponse = JsonSerializerHelper.Deserialize<TokenResponse>(responseBody);

            if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.AccessToken))
            {
                logger.LogError("Token response is null or access token is empty. Response: {Response}", responseBody);
                throw new InvalidOperationException("Token response is null or access token is empty.");
            }

            return tokenResponse.AccessToken;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting access token from endpoint: {TokenEndpoint}", config.TokenEndpoint);
            throw;
        }
    }
}