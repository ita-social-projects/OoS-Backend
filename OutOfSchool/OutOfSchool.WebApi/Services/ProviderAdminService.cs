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
        private readonly IProviderAdminRepository providerAdminRepository;

        public ProviderAdminService(
            IHttpClientFactory httpClientFactory,
            IOptions<IdentityServerConfig> identityServerConfig,
            IProviderAdminRepository providerAdminRepository)
            : base(httpClientFactory)
        {
            this.httpClientFactory = httpClientFactory;
            this.identityServerConfig = identityServerConfig.Value;
            this.providerAdminRepository = providerAdminRepository;
        }

        public async Task<ResponseDto> CreateProviderAdminAsync(string userId, ProviderAdminDto providerAdminDto, string token)
        {
            var checkAccess = await IsAllowed(providerAdminDto.ProviderId, userId)
                .ConfigureAwait(true);

            var numberProviderAdminsLessThanMax = await providerAdminRepository
                .GetNumberProviderAdminsAsync(providerAdminDto.ProviderId)
                .ConfigureAwait(false);

            if (checkAccess &&
                numberProviderAdminsLessThanMax < Constants.MaxNumberProviderAdmins)
            {
                var response = await SendRequest<ResponseDto>(new Request()
                {
                    HttpMethodType = HttpMethodType.Post,
                    Url = new Uri(identityServerConfig.Authority, CommunicationConstants.CreateProviderAdmin),
                    Token = token,
                    Data = providerAdminDto,
                }).ConfigureAwait(false);

                if (response.IsSuccess)
                {
                    var user = JsonConvert.DeserializeObject<ProviderAdminDto>(response.Result.ToString());

                    return new ResponseDto()
                    {
                        IsSuccess = true,
                        Result = providerAdminDto,
                    };
                }

                return response;
            }

            throw new UnauthorizedAccessException();
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
