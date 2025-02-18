using Microsoft.Extensions.Options;
using OpenIddict.Client;
using OutOfSchool.AikomApiClient.Config;
using OutOfSchool.AikomApiClient.Extensions;
using OutOfSchool.AikomApiClient.Models.Requests;
using OutOfSchool.AikomApiClient.Models.Responses;
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
    private readonly string apiUrl;

    public AikomApiService(
        ICommunicationService communicationService,
        OpenIddictClientService service,
        IOptions<AikomApiClientConfig> aikomOptions)
    {
        this.communicationService = communicationService;
        this.service = service;
        var config = aikomOptions?.Value
            ?? throw new ArgumentNullException(nameof(aikomOptions));
        apiUrl = config.ApiUrl;
    }

    /// <inheritdoc />
    public Task<Either<ErrorResponse, SearchUniversityResponseData>> SearchUniversity(string edrpou)
    {
        var request = new SearchUniversityRequest(edrpou);
        return PostAsync<SearchUniversityRequest, SearchUniversityResponse>(request)
            .FlatMapAsync(result => result.ToResponseData());
    }

    /// <inheritdoc />
    public Task<Either<ErrorResponse, GetUniversityResponseData>> GetUniversity(long id)
    {
        var request = new GetUniversityRequest(id);

        return PostAsync<GetUniversityRequest, GetUniversityResponse>(request)
            .FlatMapAsync(result => result.ToResponseData());
    }

    private async Task<Either<ErrorResponse,TResponse>> PostAsync<TRequest, TResponse>(TRequest request)
    where TResponse : IResponse
    {

            var token = await GetAccessTokenAsync().ConfigureAwait(false);
            var req = new Request
            {
                Url = new Uri(apiUrl),
                Data = request,
                Headers = [new KeyValuePair<string, string>("x-access-token", token)],
                HttpMethodType = HttpMethodType.Post,
            };
            return await communicationService.SendRequest<TResponse, ErrorResponse>(req).ConfigureAwait(false);
    }

    private async Task<string> GetAccessTokenAsync()
    {
        var registration = await service.GetClientRegistrationByProviderNameAsync("aikom").ConfigureAwait(false);
        var result = await service.AuthenticateWithClientCredentialsAsync(new()
        {
            RegistrationId = registration.RegistrationId
        });

        return result.AccessToken;
    }
}