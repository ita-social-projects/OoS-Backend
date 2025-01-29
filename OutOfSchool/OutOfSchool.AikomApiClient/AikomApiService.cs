using OpenIddict.Client;
using System.Text;
using System.Net.Http.Headers;
using OutOfSchool.AikomApiClient.Models;
using Microsoft.Extensions.Options;
using OutOfSchool.AikomApiClient.Config;
using OutOfSchool.AikomApiClient.Extensions;
using OutOfSchool.Common;
using ResponseDto = OutOfSchool.AikomApiClient.Models.ResponseDto;

namespace OutOfSchool.AikomApiClient;

public class AikomApiService : IAikomApiService
{
    private readonly HttpClient httpClient;
    private readonly OpenIddictClientService service;
    private readonly IOptions<AikomApiClientConfig> aikomOptions;
    private readonly string apiUrl;

    public AikomApiService(
        HttpClient httpClient,
        OpenIddictClientService service,
        IOptions<AikomApiClientConfig> aikomOptions)
    {
        this.httpClient = httpClient;
        this.service = service;
        this.aikomOptions = aikomOptions;
        var config = aikomOptions?.Value
            ?? throw new ArgumentNullException(nameof(aikomOptions));
        apiUrl = config.ApiUrl;
    }

    public async Task<ResponseDto> SearchUniversity(string edrpou)
    {
        var request = new SearchUniversityRequest(edrpou);
        var endpoint = apiUrl + request.BusinessProcessDefinitionKey;
        var (result, exception) = await PostAsync<SearchUniversityRequest, SearchUniversityResponse>(
            endpoint, request).ConfigureAwait(false);

        return result.ToResponseDto(exception, data => data.ToDto());
    }

    public async Task<ResponseDto> GetUniversity(int id)
    {
        var request = new GetUniversityRequest(id);
        var endpoint = apiUrl + request.BusinessProcessDefinitionKey;
        var (result, exception) = await PostAsync<GetUniversityRequest, GetUniversityResponse>(
            endpoint, request).ConfigureAwait(false);

        return result.ToResponseDto(exception, data => data.ToDto());
    }

    private async Task<(TResponse?, Exception?)> PostAsync<TRequest, TResponse>(string endpoint, TRequest request)
    {
        try 
        {
            var token = await GetAccessTokenAsync().ConfigureAwait(false);
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint);
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            httpRequest.Content = new StringContent(
                JsonSerializerHelper.Serialize(request), Encoding.UTF8, "application/json");
            using var response = await httpClient.SendAsync(httpRequest).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            return (JsonSerializerHelper.Deserialize<TResponse>(responseContent), null);
        }
        catch (Exception ex)
        {
            return (default(TResponse), ex);
        }
    }

    private async Task<string> GetAccessTokenAsync()
    {
        var result = await service.AuthenticateWithClientCredentialsAsync(new());

        return result.AccessToken;
    }
}