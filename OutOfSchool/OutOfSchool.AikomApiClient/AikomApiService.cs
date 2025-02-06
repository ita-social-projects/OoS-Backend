using Microsoft.Extensions.Options;
using OpenIddict.Client;
using OutOfSchool.AikomApiClient.Config;
using OutOfSchool.AikomApiClient.Extensions;
using OutOfSchool.AikomApiClient.Models.Data;
using OutOfSchool.AikomApiClient.Models.Requests;
using OutOfSchool.AikomApiClient.Models.Responses;
using OutOfSchool.Common.Communication;
using OutOfSchool.Common.Communication.ICommunication;
using OutOfSchool.Common.Extensions;
using OutOfSchool.Common.Models;

namespace OutOfSchool.AikomApiClient;

public class AikomApiService : IAikomApiService
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

    public Task<Either<ErrorResponse, SearchUniversityDto>> SearchUniversity(string edrpou)
    {
        var request = new SearchUniversityRequest(edrpou);
        var endpoint = apiUrl + request.BusinessProcessDefinitionKey;
        return PostAsync<SearchUniversityRequest, SearchUniversityResponse>(
            endpoint, request)
            .FlatMapAsync(result => result.ToResponseDto(data => data.ToDto()));
    }

    public Task<Either<ErrorResponse, GetUniversityDto>> GetUniversity(int id)
    {
        var request = new GetUniversityRequest(id);
        var endpoint = apiUrl + request.BusinessProcessDefinitionKey;

        return PostAsync<GetUniversityRequest, GetUniversityResponse>(
                endpoint, request)
            .FlatMapAsync(result => result.ToResponseDto(data => data.ToDto()));
    }

    private async Task<Either<ErrorResponse,TResponse>> PostAsync<TRequest, TResponse>(string endpoint, TRequest request)
    where TResponse : IResponse
    {

            var token = await GetAccessTokenAsync().ConfigureAwait(false);
            var req = new Request
            {
                Url = new Uri(endpoint),
                Data = request,
                Token = token,
                HttpMethodType = HttpMethodType.Post,
            };
            return await communicationService.SendRequest<TResponse, ErrorResponse>(req).ConfigureAwait(false);
    }

    private async Task<string> GetAccessTokenAsync()
    {
        var result = await service.AuthenticateWithClientCredentialsAsync(new());

        return result.AccessToken;
    }
}