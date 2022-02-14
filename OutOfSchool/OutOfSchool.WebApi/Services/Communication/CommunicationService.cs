using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

using OutOfSchool.Common;
using OutOfSchool.WebApi.Config;
using OutOfSchool.WebApi.Services.Communication.ICommunication;

namespace OutOfSchool.WebApi.Services.Communication
{
    public class CommunicationService : ICommunicationService
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly HttpClient httpClient;

        public CommunicationService(
            IHttpClientFactory httpClientFactory,
            CommunicationConfig communicationConfig)
        {
            this.httpClientFactory = httpClientFactory;
            httpClient = this.httpClientFactory.CreateClient(communicationConfig.ClientName);
            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders
                .Accept.Add(new MediaTypeWithQualityHeaderValue(System.Net.Mime.MediaTypeNames.Application.Json));
            httpClient.Timeout = TimeSpan.FromSeconds(communicationConfig.TimeoutInSeconds);
        }

        public async Task<ResponseDto> SendRequest(Request request)
        {
            try
            {
                // TODO:
                // Setup number of parallel requests
                httpClient.DefaultRequestHeaders.Authorization
                                = new AuthenticationHeaderValue("Bearer", request.Token);

                httpClient.DefaultRequestHeaders
                    .Add("X-Request-ID", request.RequestId.ToString());

                using var requestMessage = new HttpRequestMessage();

                requestMessage.Headers
                    .AcceptEncoding
                    .Add(new StringWithQualityHeaderValue("gzip"));

                requestMessage.RequestUri = new Uri(request.Url.ToString());

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

                using (var stream = await response.Content.ReadAsStreamAsync()
                    .ConfigureAwait(false))
                {
                    response.EnsureSuccessStatusCode();

                    return stream.ReadAndDeserializeFromJson<ResponseDto>();
                }
            }
            catch
            {
                var responseDto = new ResponseDto
                {
                    IsSuccess = false,
                    HttpStatusCode = HttpStatusCode.InternalServerError,
                };

                return responseDto;
            }
        }
    }
}
