using OpenIddict.Client;
using System.Text.Json;
using System.Text;
using System.Net.Http.Headers;
using OutOfSchool.AikomApiClient.Models;
using Microsoft.Extensions.Options;
using OutOfSchool.AikomApiClient.Config;

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
        
        if (exception != null)
        {
            return new ResponseDto
            {
                IsSuccess = false,
                ErrorMessage = exception.Message,
            };
        }

        if (result?.ResultVariables.Response.Error != null)
        {
            return new ResponseDto
            {
                IsSuccess = false,
                ErrorMessage = result.ResultVariables.Response.Error.Message,
                ErrorCode = result.ResultVariables.Response.Error.Code,
            };
        }
        
        if (result?.ResultVariables.Response.Data != null)
        {
            return new ResponseDto
            {
                IsSuccess = true,
                Result = new UniversityDto
                {
                    Id = result.ResultVariables.Response.Data.Id,
                    UniversityFullName = result.ResultVariables.Response.Data.UniversityFullName,
                    IsBranch = result.ResultVariables.Response.Data.IsBranch,
                    Edrpou = result.ResultVariables.Response.Data.Edrpou,
                    Branches = result.ResultVariables.Response.Data.Branches?
                    .Select(b => new BranchDto
                    {
                        BranchId = b.BranchId,
                        BranchName = b.BranchName,
                        Edrpou = b.Edrpou,
                    })
                    .ToList() ?? []
                }
            };
        }

        return new ResponseDto
        {
            IsSuccess = false,
            ErrorMessage = "Aikom API returned an empty result",
        };
    }

    private async Task<(TResponse?, Exception?)> PostAsync<TRequest, TResponse>(string endpoint, TRequest request)
    {
        try 
        {
            var token = await GetAccessTokenAsync().ConfigureAwait(false);
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint);
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true,
            };
            httpRequest.Content = new StringContent(
                JsonSerializer.Serialize(request, jsonOptions), Encoding.UTF8, "application/json");
            using var response = await httpClient.SendAsync(httpRequest).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return (JsonSerializer.Deserialize<TResponse>(responseContent, jsonOptions), null);
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