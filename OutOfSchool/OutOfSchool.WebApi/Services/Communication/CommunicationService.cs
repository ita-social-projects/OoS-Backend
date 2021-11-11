using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

using OutOfSchool.Common;
using OutOfSchool.WebApi.Config;
using OutOfSchool.WebApi.Services.Communication.ICommunication;

namespace OutOfSchool.WebApi.Services.Communication
{
    // TODO:
    // Add config section with:
    // httpRequest timeout
    // httpRequest retries
    public class CommunicationService : ICommunicationService
    {
        // HttpClient is intended to be instantiated once and re-used throughout the life of an application.
        // Instantiating an HttpClient class for every request will exhaust the number of sockets available under heavy loads.
        private static HttpClient httpClient;
        private readonly IHttpClientFactory httpClientFactory;

        public CommunicationService(
            IHttpClientFactory httpClientFactory,
            CommunicationConfig communicationConfig)
        {
            this.httpClientFactory = httpClientFactory;

            httpClient = httpClientFactory.CreateClient(communicationConfig.ClientName);
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders
                .Accept.Add(new MediaTypeWithQualityHeaderValue(System.Net.Mime.MediaTypeNames.Application.Json));

            //httpClient.Timeout = TimeSpan.FromSeconds(communicationConfig.TimeoutInSeconds);
        }

        public async Task<T> SendRequest<T>(Request request)
        {
            try
            {
                // TODO:
                // Setup number of parallel requests
                httpClient.DefaultRequestHeaders.Authorization
                                = new AuthenticationHeaderValue("Bearer", request.Token);

                using var requestMessage = new HttpRequestMessage();

                requestMessage.Headers
                    .AcceptEncoding
                    .Add(new StringWithQualityHeaderValue("gzip"));

                requestMessage.RequestUri = new System.Uri(request.Url.ToString());

                if (request.Data != null)
                {
                    requestMessage.Content =
                            new StringContent(
                                JsonConvert.SerializeObject(request.Data),
                                Encoding.UTF8,
                                System.Net.Mime.MediaTypeNames.Application.Json);
                }

                requestMessage.Method = HttpMethodService.GetHttpMethodType(request);

                var response = await httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead)
                    .ConfigureAwait(false);

                // TODO:
                // We can have isuues with duplicates when request finished with:
                // RequestTimeout
                // GatewayTimeout
                // ServiceUnavailable ?
                // Error handling with additional request to the identity server and check if such user was created.

                using (var stream = await response.Content.ReadAsStreamAsync()
                    .ConfigureAwait(false))
                {
                    response.EnsureSuccessStatusCode();

                    return stream.ReadAndDeserializeFromJson<T>();
                }
            }
            catch (Exception ex)
            {
                var responseDto = new ResponseDto
                {
                    Message = "Error",
                    ErrorMessages = new List<string> { ex.Message },
                    IsSuccess = false,
                };

                // TODO:
                // I had better refactor it
                var result = JsonConvert.SerializeObject(responseDto);

                return JsonConvert.DeserializeObject<T>(result);
            }
        }
    }
}
