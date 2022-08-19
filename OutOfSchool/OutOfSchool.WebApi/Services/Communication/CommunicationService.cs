using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using OutOfSchool.Common.Models;
using OutOfSchool.WebApi.Services.Communication.ICommunication;

namespace OutOfSchool.WebApi.Services.Communication;

public class CommunicationService : ICommunicationService
{
    private readonly IHttpClientFactory httpClientFactory;
    private readonly HttpClient httpClient;
    protected readonly ILogger<CommunicationService> logger;

    public CommunicationService(
        IHttpClientFactory httpClientFactory,
        CommunicationConfig communicationConfig,
        ILogger<CommunicationService> logger)
    {
        ArgumentNullException.ThrowIfNull(communicationConfig);

        this.httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

        httpClient = this.httpClientFactory.CreateClient(communicationConfig.ClientName);
        httpClient.DefaultRequestHeaders.Clear();
        httpClient.DefaultRequestHeaders
            .Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
        httpClient.Timeout = TimeSpan.FromSeconds(communicationConfig.TimeoutInSeconds);
    }

    public async Task<Either<ErrorResponse, T>> SendRequest<T>(Request request)
    where T : IResponse
    {
        try
        {
            // TODO:
            // Setup number of parallel requests
            if (!string.IsNullOrEmpty(request.Token))
            {
                httpClient.DefaultRequestHeaders.Authorization
                    = new AuthenticationHeaderValue("Bearer", request.Token);
            }

            httpClient.DefaultRequestHeaders
                .Add("X-Request-ID", request.RequestId.ToString());

            using var requestMessage = new HttpRequestMessage();

            requestMessage.Headers
                .AcceptEncoding
                .Add(new StringWithQualityHeaderValue("gzip"));

            requestMessage.RequestUri = request.Query != null
                ? new Uri(QueryHelpers.AddQueryString(request.Url.ToString(), request.Query))
                : request.Url;

            if (request.Data != null)
            {
                requestMessage.Content =
                    new StringContent(
                        JsonConvert.SerializeObject(request.Data),
                        Encoding.UTF8,
                        MediaTypeNames.Application.Json);
            }

            requestMessage.Method = HttpMethodService.GetHttpMethodType(request);

            var response = await httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead)
                .ConfigureAwait(false);

            await using var stream = await response.Content.ReadAsStreamAsync()
                .ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            return stream.ReadAndDeserializeFromJson<T>();
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, ex.Message);
            return new ErrorResponse
            {
                HttpStatusCode = ex.StatusCode ?? HttpStatusCode.BadRequest,
                Message = ex.Message,
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            return new ErrorResponse
            {
                HttpStatusCode = HttpStatusCode.InternalServerError,
            };
        }
    }
}