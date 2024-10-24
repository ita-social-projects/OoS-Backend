﻿using System;
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

    public virtual async Task<Either<ErrorResponse, T>> SendRequest<T>(Request request)
    where T : IResponse
    {
        if (request is null)
        {
            return new ErrorResponse
            {
                HttpStatusCode = HttpStatusCode.BadRequest,
            };
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

            await using var stream = await response.Content.ReadAsStreamAsync()
                .ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            return stream.ReadAndDeserializeFromJson<T>();
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Networking error");
            return new ErrorResponse
            {
                HttpStatusCode = ex.StatusCode ?? HttpStatusCode.BadRequest,
                Message = ex.Message,
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unknown error");
            return new ErrorResponse
            {
                HttpStatusCode = HttpStatusCode.InternalServerError,
            };
        }
    }
}