using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OutOfSchool.Common.Models;

namespace OutOfSchool.WebApi.Services.ProviderAdminOperations;

/// <summary>
/// Implements the interface for creating ProviderAdmin.
/// </summary>
public class ProviderAdminOperationsRESTService : CommunicationService, IProviderAdminOperationsService
{
    private readonly AuthorizationServerConfig authorizationServerConfig;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProviderAdminOperationsRESTService"/> class.
    /// </summary>
    /// <param name="logger">Logger.</param>
    /// <param name="identityServerConfig">Configuration for connection to IdentityServer.</param>
    /// <param name="httpClientFactory">HttpClientFactory. For base class.</param>
    /// <param name="communicationConfig">CommunicationConfig. For base class.</param>
    public ProviderAdminOperationsRESTService(
        ILogger<ProviderAdminOperationsRESTService> logger,
        IOptions<AuthorizationServerConfig> identityServerConfig,
        IHttpClientFactory httpClientFactory,
        IOptions<CommunicationConfig> communicationConfig)
        : base(httpClientFactory, communicationConfig.Value, logger)
    {
        this.authorizationServerConfig = identityServerConfig.Value;
    }

    /// <inheritdoc/>
    public async Task<Either<ErrorResponse, CreateProviderAdminDto>> CreateProviderAdminAsync(
        string userId,
        CreateProviderAdminDto providerAdminDto,
        string token)
    {
        var request = new Request()
        {
            HttpMethodType = HttpMethodType.Post,
            Url = new Uri(authorizationServerConfig.Authority, CommunicationConstants.CreateProviderAdmin),
            Token = token,
            Data = providerAdminDto,
            RequestId = Guid.NewGuid(),
        };

        Logger.LogDebug(
            "{request.HttpMethodType} Request(id): {request.RequestId} was sent. User(id): {UserId}. Url: {request.Url}",
            request.HttpMethodType,
            request.RequestId,
            userId,
            request.Url);

        var response = await SendRequest<ResponseDto>(request)
            .ConfigureAwait(false);

        return response
            .FlatMap<ResponseDto>(r => r.IsSuccess
                ? r
                : new ErrorResponse
                {
                    HttpStatusCode = r.HttpStatusCode,
                    Message = r.Message,
                })
            .Map(result => result.Result is not null
                ? JsonConvert
                    .DeserializeObject<CreateProviderAdminDto>(result.Result.ToString())
                : null);
    }
}