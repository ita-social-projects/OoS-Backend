using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Newtonsoft.Json;

using OutOfSchool.Common;
using OutOfSchool.Common.Models;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Config;
using OutOfSchool.WebApi.Enums;
using OutOfSchool.WebApi.Extensions;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services.Communication;

namespace OutOfSchool.WebApi.Services
{
    public class ProviderAdminService : CommunicationService, IProviderAdminService
    {
        private readonly IdentityServerConfig identityServerConfig;
        private readonly ProviderAdminConfig providerAdminConfig;
        private readonly IEntityRepository<User> userRepository;
        private readonly IProviderAdminRepository providerAdminRepository;
        private readonly ILogger<ProviderAdminService> logger;
        private readonly ResponseDto responseDto;

        public ProviderAdminService(
            IHttpClientFactory httpClientFactory,
            IOptions<IdentityServerConfig> identityServerConfig,
            IOptions<ProviderAdminConfig> providerAdminConfig,
            IOptions<CommunicationConfig> communicationConfig,
            IProviderAdminRepository providerAdminRepository,
            IEntityRepository<User> userRepository,
            ILogger<ProviderAdminService> logger)
            : base(httpClientFactory, communicationConfig.Value)
        {
            this.identityServerConfig = identityServerConfig.Value;
            this.providerAdminConfig = providerAdminConfig.Value;
            this.providerAdminRepository = providerAdminRepository;
            this.userRepository = userRepository;
            this.logger = logger;
            responseDto = new ResponseDto();
        }

        public async Task<ResponseDto> CreateProviderAdminAsync(string userId, CreateProviderAdminDto providerAdminDto, string token)
        {
            logger.LogDebug($"ProviderAdmin creating was started. User(id): {userId}");

            var hasAccess = await IsAllowedCreateAsync(providerAdminDto.ProviderId, userId, providerAdminDto.IsDeputy)
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
                    .DeserializeObject<CreateProviderAdminDto>(response.Result.ToString());

                return responseDto;
            }

            return response;
        }

        public async Task<ResponseDto> DeleteProviderAdminAsync(string providerAdminId, string userId, Guid providerId, string token)
        {
            logger.LogDebug($"ProviderAdmin(id): {providerAdminId} deleting was started. User(id): {userId}");

            var hasAccess = await IsAllowedAsync(providerId, userId)
                .ConfigureAwait(true);

            if (!hasAccess)
            {
                logger.LogError($"User(id): {userId} doesn't have permission to delete provider admin.");

                responseDto.IsSuccess = false;
                responseDto.HttpStatusCode = HttpStatusCode.Forbidden;

                return responseDto;
            }

            var provideradmin = await providerAdminRepository.GetByIdAsync(providerAdminId, providerId)
                .ConfigureAwait(false);

            if (provideradmin is null)
            {
                logger.LogError($"ProviderAdmin(id) {providerAdminId} not found. User(id): {userId}.");

                responseDto.IsSuccess = false;
                responseDto.HttpStatusCode = HttpStatusCode.NotFound;

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

        public async Task<ResponseDto> BlockProviderAdminAsync(string providerAdminId, string userId, Guid providerId, string token)
        {
            logger.LogDebug($"ProviderAdmin(id): {providerAdminId} blocking was started. User(id): {userId}");

            var hasAccess = await IsAllowedAsync(providerId, userId)
                .ConfigureAwait(true);

            if (!hasAccess)
            {
                logger.LogError($"User(id): {userId} doesn't have permission to block provider admin.");

                responseDto.IsSuccess = false;
                responseDto.HttpStatusCode = HttpStatusCode.Forbidden;

                return responseDto;
            }

            var provideradmin = await providerAdminRepository.GetByIdAsync(providerAdminId, providerId)
                .ConfigureAwait(false);

            if (provideradmin is null)
            {
                logger.LogError($"ProviderAdmin(id) {providerAdminId} not found. User(id): {userId}.");

                responseDto.IsSuccess = false;
                responseDto.HttpStatusCode = HttpStatusCode.NotFound;

                return responseDto;
            }

            var request = new Request()
            {
                HttpMethodType = HttpMethodType.Put,
                Url = new Uri(identityServerConfig.Authority, CommunicationConstants.BlockProviderAdmin),
                Token = token,
                Data = new BlockProviderAdminDto()
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

        public async Task<bool> IsAllowedCreateAsync(Guid providerId, string userId, bool isDeputy)
        {
            bool providerAdminDeputy = await providerAdminRepository.IsExistProviderAdminDeputyWithUserIdAsync(providerId, userId)
                .ConfigureAwait(false);

            bool provider = await providerAdminRepository.IsExistProviderWithUserIdAsync(providerId, userId)
                .ConfigureAwait(false);

            // provider admin deputy can create only assistants
            return (providerAdminDeputy && !isDeputy) || provider;
        }

        public async Task<bool> IsAllowedAsync(Guid providerId, string userId)
        {
            bool provider = await providerAdminRepository.IsExistProviderWithUserIdAsync(providerId, userId)
                .ConfigureAwait(false);

            return provider;
        }

        public async Task<bool> CheckUserIsRelatedProviderAdmin(string userId, Guid providerId, Guid workshopId)
        {
            var providerAdmin = await providerAdminRepository.GetByIdAsync(userId, providerId).ConfigureAwait(false);

            if (!providerAdmin.IsDeputy)
            {
                return providerAdmin.ManagedWorkshops.Any(w => w.Id == workshopId);
            }

            return providerAdmin != null;
        }

        public async Task<IEnumerable<ProviderAdminDto>> GetRelatedProviderAdmins(string userId, bool isDeputy = false)
        {
            var provider = await providerAdminRepository.GetProviderWithUserIdAsync(userId).ConfigureAwait(false);
            List<ProviderAdmin> providerAdmins = new List<ProviderAdmin>();
            List<ProviderAdminDto> dtos = new List<ProviderAdminDto>();

            if (provider != null)
            {
                providerAdmins = (await providerAdminRepository.GetByFilter(pa => pa.ProviderId == provider.Id).ConfigureAwait(false))
                    .ToList();
            }
            else
            {
                var providerAdmin = (await providerAdminRepository.GetByFilter(p => p.UserId == userId).ConfigureAwait(false)).SingleOrDefault();
                if (providerAdmin.IsDeputy)
                {
                    providerAdmins = (await providerAdminRepository
                        .GetByFilter(pa => pa.ProviderId == providerAdmin.ProviderId && !pa.IsDeputy)
                        .ConfigureAwait(false)).ToList();
                }
            }

            if (providerAdmins.Any())
            {
                foreach (var pa in providerAdmins)
                {
                    var user = (await userRepository.GetByFilter(u => u.Id == pa.UserId).ConfigureAwait(false)).Single();
                    var dto = user.ToModel(pa);

                    if (!user.IsEnabled)
                    {
                        dto.AccountStatus = AccountStatus.Blocked;
                    }
                    else
                    {
                        dto.AccountStatus = user.LastLogin == DateTimeOffset.MinValue ? AccountStatus.NeverLogged : AccountStatus.Accepted;
                    }

                    dtos.Add(dto);
                }

            }

            return dtos;
        }
    }
}
