using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OutOfSchool.Common;
using OutOfSchool.Common.Models;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Config;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Services.Communication;

namespace OutOfSchool.WebApi.Services
{
    public class ProviderAdminService : CommunicationService, IProviderAdminService
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IdentityServerConfig identityServerConfig;

        public ProviderAdminService(
            IHttpClientFactory httpClientFactory,
            IOptions<IdentityServerConfig> identityServerConfig)
            : base(httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory;
            this.identityServerConfig = identityServerConfig.Value;
        }

        public async Task<ResponseDto> CreateProviderAdminAsync(string userId, ProviderAdminDto providerAdminDto, string token)
        {
            // TODO:
            // Check if user entitled to work with this specific provider (vericifaction)

            var createUserDto = providerAdminDto.ToModel();

            var response = await SendRequest<ResponseDto>(new Request()
            {
                HttpMethodType = HttpMethodType.Post,
                Url = new Uri(identityServerConfig.Authority, Communication.Constants.CreateAssistant),
                Token = token,
                Data = createUserDto,
            }).ConfigureAwait(false);

            if (response.IsSuccess)
            {
                var user = JsonConvert.DeserializeObject<CreateUserDto>(response.Result.ToString());
                providerAdminDto.UserId = user.UserId;

                return new ResponseDto()
                {
                    IsSuccess = true,
                    Result = providerAdminDto,
                };
            }

            return response;
        }
    }
}
