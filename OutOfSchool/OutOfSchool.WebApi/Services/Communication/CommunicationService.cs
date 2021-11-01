using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OutOfSchool.Common;
using OutOfSchool.WebApi.Services.Communication.ICommunication;

namespace OutOfSchool.WebApi.Services.Communication
{
    // TODO:
    // Add config section with:
    // httpRequest timeout
    // httpRequest retries
    public class CommunicationService : ICommunicationService
    {
        private readonly IHttpClientFactory httpClientFactory;

        public CommunicationService(IHttpClientFactory httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory;
        }

        public async Task<T> SendRequest<T>(Request request)
        {
            try
            {
                // TODO:
                // Setup number of parallel requests
                using var httpClient = httpClientFactory.CreateClient();

                httpClient.DefaultRequestHeaders.Authorization
                                = new AuthenticationHeaderValue("Bearer", request.Token);

                using var requestMessage = new HttpRequestMessage();

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

                var response = await httpClient.SendAsync(requestMessage).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();

                // TODO:
                // We can have isuues with duplicates when request finished with:
                // RequestTimeout
                // GatewayTimeout
                // ServiceUnavailable ?
                // Error handling with additional request to the identity server and check if such user was created.

                var responseApi = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                return JsonConvert.DeserializeObject<T>(responseApi);
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
