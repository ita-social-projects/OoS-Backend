using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AutoMapper;
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
    public class InstitutionAdminService : CommunicationService, IInstitutionAdminService
    {
        private readonly IdentityServerConfig identityServerConfig;
        private readonly IEntityRepository<User> userRepository;
        private readonly IInstitutionAdminRepository InstitutionAdminRepository;
        private readonly ILogger<InstitutionAdminService> logger;
        private readonly IMapper mapper;
        private readonly ResponseDto responseDto;

        public InstitutionAdminService(
            IHttpClientFactory httpClientFactory,
            IOptions<IdentityServerConfig> identityServerConfig,
            IOptions<CommunicationConfig> communicationConfig,
            IInstitutionAdminRepository InstitutionAdminRepository,
            IEntityRepository<User> userRepository,
            IMapper mapper,
            ILogger<InstitutionAdminService> logger)
            : base(httpClientFactory, communicationConfig.Value)
        {
            this.identityServerConfig = identityServerConfig.Value;
            this.InstitutionAdminRepository = InstitutionAdminRepository;
            this.userRepository = userRepository;
            this.mapper = mapper;
            this.logger = logger;
            responseDto = new ResponseDto();
        }

        public async Task<ResponseDto> CreateInstitutionAdminAsync(string userId, CreateInstitutionAdminDto InstitutionAdminDto, string token)
        {
            logger.LogDebug($"InstitutionAdmin creating was started. User(id): {userId}");

            var request = new Request()
            {
                HttpMethodType = HttpMethodType.Post,
                Url = new Uri(identityServerConfig.Authority, CommunicationConstants.CreateInstitutionAdmin),
                Token = token,
                Data = InstitutionAdminDto,
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
                    .DeserializeObject<CreateInstitutionAdminDto>(response.Result.ToString());

                return responseDto;
            }

            return response;
        }

        public async Task<ResponseDto> DeleteInstitutionAdminAsync(string InstitutionAdminId, string userId, Guid providerId, string token)
        {
            logger.LogDebug($"InstitutionAdmin(id): {InstitutionAdminId} deleting was started. User(id): {userId}");

            var hasAccess = await IsAllowedAsync(providerId, userId)
                .ConfigureAwait(true);

            if (!hasAccess)
            {
                logger.LogError($"User(id): {userId} doesn't have permission to delete provider admin.");

                responseDto.IsSuccess = false;
                responseDto.HttpStatusCode = HttpStatusCode.Forbidden;

                return responseDto;
            }

            var InstitutionAdmin = await InstitutionAdminRepository.GetByIdAsync(InstitutionAdminId, providerId)
                .ConfigureAwait(false);

            if (InstitutionAdmin is null)
            {
                logger.LogError($"InstitutionAdmin(id) {InstitutionAdminId} not found. User(id): {userId}.");

                responseDto.IsSuccess = false;
                responseDto.HttpStatusCode = HttpStatusCode.NotFound;

                return responseDto;
            }

            var request = new Request()
            {
                HttpMethodType = HttpMethodType.Delete,
                Url = new Uri(identityServerConfig.Authority, CommunicationConstants.DeleteInstitutionAdmin + InstitutionAdminId),
                Token = token,
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

        public async Task<ResponseDto> BlockInstitutionAdminAsync(string InstitutionAdminId, string userId, Guid providerId, string token)
        {
            logger.LogDebug($"InstitutionAdmin(id): {InstitutionAdminId} blocking was started. User(id): {userId}");

            var hasAccess = await IsAllowedAsync(providerId, userId)
                .ConfigureAwait(true);

            if (!hasAccess)
            {
                logger.LogError($"User(id): {userId} doesn't have permission to block provider admin.");

                responseDto.IsSuccess = false;
                responseDto.HttpStatusCode = HttpStatusCode.Forbidden;

                return responseDto;
            }

            var InstitutionAdmin = await InstitutionAdminRepository.GetByIdAsync(InstitutionAdminId, providerId)
                .ConfigureAwait(false);

            if (InstitutionAdmin is null)
            {
                logger.LogError($"InstitutionAdmin(id) {InstitutionAdminId} not found. User(id): {userId}.");

                responseDto.IsSuccess = false;
                responseDto.HttpStatusCode = HttpStatusCode.NotFound;

                return responseDto;
            }

            var request = new Request()
            {
                HttpMethodType = HttpMethodType.Put,
                Url = new Uri(identityServerConfig.Authority, CommunicationConstants.BlockInstitutionAdmin + InstitutionAdminId),
                Token = token,
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

        public async Task<bool> IsAllowedAsync(Guid providerId, string userId)
        {
            bool provider = await InstitutionAdminRepository.IsExistInstitutionWithUserIdAsync(userId)
                .ConfigureAwait(false);

            return provider;
        }

        public async Task GiveAssistantAccessToWorkshop(string userId, Guid workshopId)
        {
            await InstitutionAdminRepository.AddRelatedWorkshopForAssistant(userId, workshopId).ConfigureAwait(false);
            logger.LogDebug($"Assistant provider admin(id): {userId} now is related to workshop(id): {workshopId}");
        }

        public async Task<IEnumerable<InstitutionAdminDto>> GetRelatedInstitutionAdmins(string userId)
        {
            var provider = await InstitutionAdminRepository.GetInstitutionWithUserIdAsync(userId).ConfigureAwait(false);
            List<InstitutionAdmin> InstitutionAdmins = new List<InstitutionAdmin>();
            List<InstitutionAdminDto> dtos = new List<InstitutionAdminDto>();

            if (provider != null)
            {
                InstitutionAdmins = (await InstitutionAdminRepository.GetByFilter(pa => true).ConfigureAwait(false))
                // InstitutionAdmins = (await InstitutionAdminRepository.GetByFilter(pa => pa.Id == provider.Id).ConfigureAwait(false)) //FIXME
                    .ToList();
            }
            else
            {
                var InstitutionAdmin = (await InstitutionAdminRepository.GetByFilter(p => p.UserId == userId).ConfigureAwait(false)).SingleOrDefault();
                /*
                if (InstitutionAdmin.IsDeputy)
                {
                    InstitutionAdmins = (await InstitutionAdminRepository
                        .GetByFilter(pa => !pa.IsDeputy) // FIXME
                                                         // .GetByFilter(pa => pa.ProviderId == InstitutionAdmin.ProviderId && !pa.IsDeputy) // FIXME
                        .ConfigureAwait(false)).ToList();
                }*/
            }

            if (InstitutionAdmins.Any())
            {
                foreach (var pa in InstitutionAdmins)
                {
                    var user = (await userRepository.GetByFilter(u => u.Id == pa.UserId).ConfigureAwait(false)).Single();
                    var dto = mapper.Map<InstitutionAdminDto>(user);
                    // dto.IsDeputy = pa.IsDeputy;

                    if (user.IsBlocked)
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
