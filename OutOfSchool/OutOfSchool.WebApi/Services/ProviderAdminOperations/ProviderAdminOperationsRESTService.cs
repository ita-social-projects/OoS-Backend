using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OutOfSchool.Common;
using OutOfSchool.Common.Models;
using OutOfSchool.WebApi.Config;
using OutOfSchool.WebApi.Services.Communication;

namespace OutOfSchool.WebApi.Services.ProviderAdminOperations;

/// <summary>
/// Implements the interface for creating ProviderAdmin.
/// </summary>
public class ProviderAdminOperationsRESTService : CommunicationService, IProviderAdminOperationsService
{
    private readonly ILogger<ProviderAdminService> logger;
    private readonly IdentityServerConfig identityServerConfig;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProviderAdminOperationsRESTService"/> class.
    /// </summary>
    /// <param name="logger">Logger.</param>
    /// <param name="identityServerConfig">Configuration for connection to IdentityServer.</param>
    /// <param name="httpClientFactory">HttpClientFactory. For base class.</param>
    /// <param name="communicationConfig">CommunicationConfig. For base class.</param>
    public ProviderAdminOperationsRESTService(
        ILogger<ProviderAdminService> logger,
        IOptions<IdentityServerConfig> identityServerConfig,
        IHttpClientFactory httpClientFactory,
        IOptions<CommunicationConfig> communicationConfig)
        : base(httpClientFactory, communicationConfig.Value)
    {
        this.logger = logger;
        this.identityServerConfig = identityServerConfig.Value;
    }

    /// <inheritdoc/>
    public async Task<ResponseDto> CreateProviderAdminAsync(string userId, CreateProviderAdminDto providerAdminDto, string token)
    {
        var request = new Request()
        {
            HttpMethodType = HttpMethodType.Post,
            Url = new Uri(identityServerConfig.Authority, CommunicationConstants.CreateProviderAdmin),
            Token = token,
            Data = providerAdminDto,
            RequestId = Guid.NewGuid(),
        };

        logger.LogDebug($"{request.HttpMethodType} Request(id): {request.RequestId} " +
                        $"was sent. User(id): {userId}. Url: {request.Url}");

        var response = await SendRequest(request)
            .ConfigureAwait(false);

        if (response.IsSuccess)
        {
            response.IsSuccess = true;
            response.Result = JsonConvert
                .DeserializeObject<CreateProviderAdminDto>(response.Result.ToString());

            return response;
        }

        return response;
    }
}