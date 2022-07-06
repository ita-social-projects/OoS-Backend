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
    public class MinistryAdminService : CommunicationService, IMinistryAdminService
    {
        private readonly IdentityServerConfig identityServerConfig;
        private readonly IEntityRepository<User> userRepository;
        private readonly IMinistryAdminRepository ministryAdminRepository;
        private readonly ILogger<MinistryAdminService> logger;
        private readonly IMapper mapper;
        private readonly ResponseDto responseDto;

        public MinistryAdminService(
            IHttpClientFactory httpClientFactory,
            IOptions<IdentityServerConfig> identityServerConfig,
            IOptions<CommunicationConfig> communicationConfig,
            IMinistryAdminRepository ministryAdminRepository,
            IEntityRepository<User> userRepository,
            IMapper mapper,
            ILogger<MinistryAdminService> logger)
            : base(httpClientFactory, communicationConfig.Value)
        {
            this.identityServerConfig = identityServerConfig.Value;
            this.ministryAdminRepository = ministryAdminRepository;
            this.userRepository = userRepository;
            this.mapper = mapper;
            this.logger = logger;
            responseDto = new ResponseDto();
        }

        public async Task<ResponseDto> CreateMinistryAdminAsync(string userId, CreateMinistryAdminDto ministryAdminDto, string token)
        {
            logger.LogDebug($"ministryAdmin creating was started. User(id): {userId}");

            var request = new Request()
            {
                HttpMethodType = HttpMethodType.Post,
                Url = new Uri(identityServerConfig.Authority, CommunicationConstants.CreateMinistryAdmin),
                Token = token,
                Data = ministryAdminDto,
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
                    .DeserializeObject<CreateMinistryAdminDto>(response.Result.ToString());

                return responseDto;
            }

            return response;
        }

        public async Task<ResponseDto> DeleteMinistryAdminAsync(string ministryAdminId, string userId, string token)
        {
            logger.LogDebug($"MinistryAdmin(id): {ministryAdminId} deleting was started. User(id): {userId}");

            var ministryAdmin = await ministryAdminRepository.GetByIdAsync(ministryAdminId)
                .ConfigureAwait(false);

            if (ministryAdmin is null)
            {
                logger.LogError($"MinistryAdmin(id) {ministryAdminId} not found. User(id): {userId}.");

                responseDto.IsSuccess = false;
                responseDto.HttpStatusCode = HttpStatusCode.NotFound;

                return responseDto;
            }

            var request = new Request()
            {
                HttpMethodType = HttpMethodType.Delete,
                Url = new Uri(identityServerConfig.Authority, CommunicationConstants.DeleteMinistryAdmin + ministryAdminId),
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

        public async Task<ResponseDto> BlockMinistryAdminAsync(string ministryAdminId, string userId, Guid providerId, string token)
        {
            logger.LogDebug($"MinistryAdmin(id): {ministryAdminId} blocking was started. User(id): {userId}");

            var ministryAdmin = await ministryAdminRepository.GetByIdAsync(ministryAdminId)
                .ConfigureAwait(false);

            if (ministryAdmin is null)
            {
                logger.LogError($"MinistryAdmin(id) {ministryAdminId} not found. User(id): {userId}.");

                responseDto.IsSuccess = false;
                responseDto.HttpStatusCode = HttpStatusCode.NotFound;

                return responseDto;
            }

            var request = new Request()
            {
                HttpMethodType = HttpMethodType.Put,
                Url = new Uri(identityServerConfig.Authority, CommunicationConstants.BlockMinistryAdmin + ministryAdminId),
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

        public async Task GiveAssistantAccessToWorkshop(string userId, Guid workshopId)
        {
            await ministryAdminRepository.AddRelatedWorkshopForAssistant(userId, workshopId).ConfigureAwait(false);
            logger.LogDebug($"Assistant provider admin(id): {userId} now is related to workshop(id): {workshopId}");
        }

        public async Task<IEnumerable<MinistryAdminDto>> GetRelatedMinistryAdmins(string userId)
        {
            var provider = await ministryAdminRepository.GetInstitutionWithUserIdAsync(userId).ConfigureAwait(false);
            List<MinistryAdmin> ministryAdmins = new List<MinistryAdmin>();
            List<MinistryAdminDto> dtos = new List<MinistryAdminDto>();

            if (provider != null)
            {
                ministryAdmins = (await ministryAdminRepository.GetByFilter(pa => true).ConfigureAwait(false))
                // InstitutionAdmins = (await InstitutionAdminRepository.GetByFilter(pa => pa.Id == provider.Id).ConfigureAwait(false)) //FIXME
                    .ToList();
            }
            else
            {
                var InstitutionAdmin = (await ministryAdminRepository.GetByFilter(p => p.UserId == userId).ConfigureAwait(false)).SingleOrDefault();
                /*
                if (InstitutionAdmin.IsDeputy)
                {
                    InstitutionAdmins = (await InstitutionAdminRepository
                        .GetByFilter(pa => !pa.IsDeputy) // FIXME
                                                         // .GetByFilter(pa => pa.ProviderId == InstitutionAdmin.ProviderId && !pa.IsDeputy) // FIXME
                        .ConfigureAwait(false)).ToList();
                }*/
            }

            if (ministryAdmins.Any())
            {
                foreach (var pa in ministryAdmins)
                {
                    var user = (await userRepository.GetByFilter(u => u.Id == pa.UserId).ConfigureAwait(false)).Single();
                    var dto = mapper.Map<MinistryAdminDto>(user);
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
