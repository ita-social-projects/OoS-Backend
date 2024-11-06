using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OutOfSchool.Common.Communication;
using OutOfSchool.Common.Models;

namespace OutOfSchool.BusinessLogic.Services.ProviderAdminOperations;

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
    /// <param name="authorizationServerConfig">Configuration for connection to OpenIdDict.</param>
    /// <param name="httpClientFactory">HttpClientFactory. For base class.</param>
    /// <param name="communicationConfig">CommunicationConfig. For base class.</param>
    public ProviderAdminOperationsRESTService(
        ILogger<ProviderAdminOperationsRESTService> logger,
        IOptions<AuthorizationServerConfig> authorizationServerConfig,
        IHttpClientFactory httpClientFactory,
        IOptions<CommunicationConfig> communicationConfig)
        : base(httpClientFactory, communicationConfig, logger)
    {
        this.authorizationServerConfig = authorizationServerConfig.Value;
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
        };

        Logger.LogDebug(
            "{HttpMethodType} Request was sent. User(id): {UserId}. Url: {Url}",
            request.HttpMethodType,
            userId,
            request.Url);

        var response = await SendRequest<ResponseDto, ErrorResponse>(request)
            .ConfigureAwait(false);

        return response
            .FlatMap<ResponseDto>(r => r.HttpStatusCode == HttpStatusCode.Created
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