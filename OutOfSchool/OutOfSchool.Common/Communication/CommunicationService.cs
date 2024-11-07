#nullable enable

using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OutOfSchool.Common.Communication.ICommunication;
using OutOfSchool.Common.Config;
using OutOfSchool.Common.Extensions;
using OutOfSchool.Common.Models;

namespace OutOfSchool.Common.Communication;

public class CommunicationService : ICommunicationService
{
    private readonly ILogger<CommunicationService> logger;
    private readonly HttpClient httpClient;

    public CommunicationService(
        IHttpClientFactory httpClientFactory,
        IOptions<CommunicationConfig> communicationConfig,
        ILogger<CommunicationService> logger)
    {
        ArgumentNullException.ThrowIfNull(communicationConfig);
        ArgumentNullException.ThrowIfNull(httpClientFactory);

        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

        httpClient = httpClientFactory.CreateClient(communicationConfig.Value.ClientName);
        httpClient.DefaultRequestHeaders.Clear();
        httpClient.DefaultRequestHeaders
            .Accept.Add(new MediaTypeWithQualityHeaderValue(MediaTypeNames.Application.Json));
        httpClient.Timeout = TimeSpan.FromSeconds(communicationConfig.Value.TimeoutInSeconds);
    }

    protected ILogger<CommunicationService> Logger => logger;

    public virtual async Task<Either<TError, T>> SendRequest<T, TError>(
        Request? request,
        IErrorHandler<TError>? errorHandler = null)
        where T : IResponse
        where TError : IErrorResponse
    {
        if (request is null)
        {
            return await HandleErrorAsync(
                new CommunicationError(HttpStatusCode.BadRequest),
                "Request is null",
                errorHandler);
        }

        try
        {
            // TODO:
            // Setup number of parallel requests
            if (!string.IsNullOrEmpty(request.Token))
            {
                httpClient.DefaultRequestHeaders.Authorization
                    = new AuthenticationHeaderValue("Bearer", request.Token);
            }

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

            if (!response.IsSuccessStatusCode)
            {
                // Error response should be small, but still no need to waste time as default handler is ignoring it
                var errorBody = errorHandler == null ? null : await response.Content.ReadAsStringAsync();
                var error = new CommunicationError(response.StatusCode)
                {
                    Body = errorBody,
                };
                logger.LogError("Remote service error: {StatusCode}", response.StatusCode);
                return await HandleErrorAsync(error, "Remote service error", errorHandler);
            }

            await using var stream = await response.EnsureSuccessStatusCode().Content.ReadAsStreamAsync()
                .ConfigureAwait(false);

            return stream.ReadAndDeserializeFromJson<T>();
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Networking error");
            return await HandleExceptionAsync(ex, errorHandler);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unknown error");
            return await HandleExceptionAsync(ex, errorHandler);
        }
    }

    private static async Task<TError> HandleErrorAsync<TError>(
        CommunicationError response,
        string? message,
        IErrorHandler<TError>? errorHandler)
    {
        if (errorHandler != null)
        {
            return await errorHandler.HandleErrorAsync(response, message);
        }

        // Use default error handling
        if (new DefaultErrorHandler() is IErrorHandler<TError> defaultHandler)
        {
            return await defaultHandler.HandleErrorAsync(response, message);
        }

        throw new InvalidOperationException("No error handler provided, and default error handler is not compatible.");
    }

    private static async Task<TError> HandleExceptionAsync<TError>(
        Exception ex,
        IErrorHandler<TError>? errorHandler)
    {
        var response = new CommunicationError();

        if (ex is HttpRequestException e)
        {
            response.HttpStatusCode = e.StatusCode ?? HttpStatusCode.InternalServerError;
        }

        return await HandleErrorAsync(response, ex.Message, errorHandler);
    }
}