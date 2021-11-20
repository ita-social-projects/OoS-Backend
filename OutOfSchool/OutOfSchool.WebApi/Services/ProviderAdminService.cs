using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
        private readonly IdentityServerConfig identityServerConfig;
        private readonly ProviderAdminConfig providerAdminConfig;
        private readonly IProviderAdminRepository providerAdminRepository;
        private readonly ILogger<ProviderAdminService> logger;
        private readonly ResponseDto responseDto;

        public ProviderAdminService(
            IHttpClientFactory httpClientFactory,
            IOptions<IdentityServerConfig> identityServerConfig,
            IOptions<ProviderAdminConfig> providerAdminConfig,
            IOptions<CommunicationConfig> communicationConfig,
            IProviderAdminRepository providerAdminRepository,
            ILogger<ProviderAdminService> logger)
            : base(httpClientFactory, communicationConfig.Value)
        {
            this.identityServerConfig = identityServerConfig.Value;
            this.providerAdminConfig = providerAdminConfig.Value;
            this.providerAdminRepository = providerAdminRepository;
            this.logger = logger;
            responseDto = new ResponseDto();
        }

        public async Task<ResponseDto> CreateProviderAdminAsync(string userId, ProviderAdminDto providerAdminDto, string token)
        {
            logger.LogDebug($"ProviderAdmin creating was started. User(id): {userId}");

            var hasAccess = await IsAllowedCreateAsync(providerAdminDto.ProviderId, userId)
                .ConfigureAwait(true);

            if (!hasAccess)
            {
                logger.LogError($"User(id): {userId} doesn't have permission to create provider admin.");

                responseDto.IsSuccess = false;
                responseDto.HttpStatusCode = HttpStatusCode.Forbidden;

                return responseDto;
            }

            var numberProviderAdminsLessThanMax = await providerAdminRepository
                .GetNumberProviderAdminsAsync(providerAdminDto.ProviderId)
                .ConfigureAwait(false);

            if (numberProviderAdminsLessThanMax >= providerAdminConfig.MaxNumberAdmins)
            {
                logger.LogError($"Admin was not created by User(id): {userId}. " +
                    $"Limit on the number of admins has been exceeded for the Provider(id): {providerAdminDto.ProviderId}.");

                responseDto.IsSuccess = false;
                responseDto.HttpStatusCode = HttpStatusCode.MethodNotAllowed;

                return responseDto;
            }

            var request = new Request()
            {
                HttpMethodType = HttpMethodType.Post,
                Url = new Uri(identityServerConfig.Authority, CommunicationConstants.CreateProviderAdmin),
                Token = token,
                Data = providerAdminDto,
                RequestId = Guid.NewGuid(),
            };

            logger.LogDebug($"{request.HttpMethodType} Request(id): {request.RequestId} " +
                $"was sent. User(id): {userId}. Url: {request.Url}");

            var response = await SendRequest(request)
                    .ConfigureAwait(false);

            if (response.IsSuccess)
            {
                responseDto.IsSuccess = true;
                responseDto.Result = JsonConvert
                    .DeserializeObject<ProviderAdminDto>(response.Result.ToString());

                return responseDto;
            }

            return response;
        }

        public async Task<ResponseDto> DeleteProviderAdminAsync(string providerAdminId, string userId, Guid providerId, string token)
        {
            logger.LogDebug($"ProviderAdmin(id): {providerAdminId} deleting was started. User(id): {userId}");

            var hasAccess = await IsAllowedDeleteAsync(providerId, userId)
                .ConfigureAwait(true);

            if (!hasAccess)
            {
                logger.LogError($"User(id): {userId} doesn't have permission to delete provider admin.");

                responseDto.IsSuccess = false;
                responseDto.HttpStatusCode = HttpStatusCode.Forbidden;

                return responseDto;
            }

            var request = new Request()
            {
                HttpMethodType = HttpMethodType.Delete,
                Url = new Uri(identityServerConfig.Authority, CommunicationConstants.DeleteProviderAdmin),
                Token = token,
                Data = new DeleteProviderAdminDto()
                {
                    ProviderAdminId = providerAdminId,
                },
                RequestId = Guid.NewGuid(),
            };

            logger.LogDebug($"{request.HttpMethodType} Request(id): {request.RequestId} " +
                $"was sent. User(id): {userId}. Url: {request.Url}");

            var response = await SendRequest(request)
                    .ConfigureAwait(false);

            if (response.IsSuccess)
            {
                responseDto.IsSuccess = true;
                if (!(responseDto.Result is null))
                {
                    responseDto.Result = JsonConvert
                    .DeserializeObject<ActionResult>(response.Result.ToString());

                    return responseDto;
                }

                return responseDto;
            }

            return response;
        }

        public async Task<bool> IsAllowedCreateAsync(Guid providerId, string userId)
        {
            bool providerAdmin = await providerAdminRepository.IsExistProviderAdminWithUserIdAsync(providerId, userId)
                .ConfigureAwait(false);

            bool provider = await providerAdminRepository.IsExistProviderWithUserIdAsync(providerId, userId)
                .ConfigureAwait(false);

            return providerAdmin || provider;
        }

        public async Task<bool> IsAllowedDeleteAsync(Guid providerId, string userId)
        {
            bool provider = await providerAdminRepository.IsExistProviderWithUserIdAsync(providerId, userId)
                .ConfigureAwait(false);

            return provider;
        }
    }
}
