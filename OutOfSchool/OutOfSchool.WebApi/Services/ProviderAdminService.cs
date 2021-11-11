using System;
using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;

using Newtonsoft.Json;

using OutOfSchool.Common;
using OutOfSchool.Common.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Config;
using OutOfSchool.WebApi.Services.Communication;

namespace OutOfSchool.WebApi.Services
{
    public class ProviderAdminService : CommunicationService, IProviderAdminService
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IdentityServerConfig identityServerConfig;
        private readonly ProviderAdminConfig providerAdminConfig;
        private readonly IProviderAdminRepository providerAdminRepository;
        private ResponseDto responseDto;

        public ProviderAdminService(
            IHttpClientFactory httpClientFactory,
            IOptions<IdentityServerConfig> identityServerConfig,
            IOptions<ProviderAdminConfig> providerAdminConfig,
            IOptions<CommunicationConfig> communicationConfig,
            IProviderAdminRepository providerAdminRepository)
            : base(httpClientFactory, communicationConfig.Value)
        {
            this.httpClientFactory = httpClientFactory;
            this.identityServerConfig = identityServerConfig.Value;
            this.providerAdminConfig = providerAdminConfig.Value;
            this.providerAdminRepository = providerAdminRepository;
            responseDto = new ResponseDto();
        }

        public async Task<ResponseDto> CreateProviderAdminAsync(string userId, ProviderAdminDto providerAdminDto, string token)
        {
            var checkAccess = await IsAllowed(providerAdminDto.ProviderId, userId)
                .ConfigureAwait(true);

            if (!checkAccess)
            {
                responseDto.IsSuccess = false;
                responseDto.Message = "You are not allowed to do that.";

                return responseDto;
            }

            var numberProviderAdminsLessThanMax = await providerAdminRepository
                .GetNumberProviderAdminsAsync(providerAdminDto.ProviderId)
                .ConfigureAwait(false);

            if (numberProviderAdminsLessThanMax >= providerAdminConfig.MaxNumberAdmins)
            {
                responseDto.IsSuccess = false;
                responseDto.Message = $"You can't have more than {providerAdminConfig.MaxNumberAdmins} provider assistants.";

                return responseDto;
            }

            var response = await SendRequest<ResponseDto>(new Request()
            {
                HttpMethodType = HttpMethodType.Post,
                Url = new Uri(identityServerConfig.Authority, CommunicationConstants.CreateProviderAdmin),
                Token = token,
                Data = providerAdminDto,
            }).ConfigureAwait(false);

            if (response.IsSuccess)
            {
                var createdProviderAdmin = JsonConvert.DeserializeObject<ProviderAdminDto>(response.Result.ToString());

                responseDto.IsSuccess = true;
                responseDto.Result = createdProviderAdmin;

                return responseDto;
            }

            return response;
        }

        public async Task<bool> IsAllowed(Guid providerId, string userId)
        {
            bool providerAdmin = await providerAdminRepository.IsExistProviderAdminWithUserIdAsync(providerId, userId)
                .ConfigureAwait(false);

            if (providerAdmin)
            {
                return true;
            }

            bool provider = await providerAdminRepository.IsExistProviderWithUserIdAsync(providerId, userId)
                .ConfigureAwait(false);

            if (provider)
            {
                return true;
            }

            return false;
        }
    }
}
