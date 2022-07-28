﻿using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using Newtonsoft.Json;
using OutOfSchool.Common.Models;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services;

public class MinistryAdminService : CommunicationService, IMinistryAdminService
{
    private readonly IdentityServerConfig identityServerConfig;
    private readonly IInstitutionAdminRepository institutionAdminRepository;
    private readonly ILogger<MinistryAdminService> logger;
    private readonly ResponseDto responseDto;
    private readonly IEntityRepository<string, User> userRepository;
    private readonly IMapper mapper;

    public MinistryAdminService(
        IHttpClientFactory httpClientFactory,
        IOptions<IdentityServerConfig> identityServerConfig,
        IOptions<CommunicationConfig> communicationConfig,
        IInstitutionAdminRepository institutionAdminRepository,
        ILogger<MinistryAdminService> logger,
        IEntityRepository<string, User> userRepository,
        IMapper mapper)
        : base(httpClientFactory, communicationConfig?.Value)
    {
        this.identityServerConfig = (identityServerConfig ?? throw new ArgumentNullException(nameof(identityServerConfig))).Value;
        this.institutionAdminRepository = institutionAdminRepository ?? throw new ArgumentNullException(nameof(institutionAdminRepository));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));

        responseDto = new ResponseDto();
    }

    public async Task<MinistryAdminDto> GetByUserId(string id)
    {
        logger.LogInformation($"Getting MinistryAdmin by UserId started. Looking UserId is {id}.");

        InstitutionAdmin ministryAdmin = (await institutionAdminRepository.GetByFilter(p => p.UserId == id).ConfigureAwait(false)).FirstOrDefault();

        if (ministryAdmin == null)
        {
            var errMsg = $"There is no MinistryAdmin in the Db with such User id {id}.";

            logger.LogError(errMsg);
            throw new ArgumentException(errMsg);
        }

        logger.LogInformation($"Successfully got a MinistryAdmin with UserId = {id}.");

        return mapper.Map<MinistryAdminDto>(ministryAdmin);
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

    /// <inheritdoc/>
    public async Task<MinistryAdminDto> Update(MinistryAdminDto ministryAdminDto)
    {
        ArgumentNullException.ThrowIfNull(ministryAdminDto);

        logger.LogInformation($"Updating MinistryAdmin with User Id = {ministryAdminDto.Id} started.");

        try
        {
            var user = await userRepository.GetById(ministryAdminDto.Id);

            var updatedUser = await userRepository.Update(mapper.Map((BaseUserDto)ministryAdminDto, user)).ConfigureAwait(false);

            logger.LogInformation($"User with Id = {updatedUser.Id} updated succesfully.");

            return ministryAdminDto;
        }
        catch (DbUpdateConcurrencyException)
        {
            logger.LogError($"Updating failed. User with Id = {ministryAdminDto.Id} doesn't exist in the system.");
            throw;
        }
    }

    public async Task<ResponseDto> DeleteMinistryAdminAsync(string ministryAdminId, string userId, string token)
    {
        logger.LogDebug($"MinistryAdmin(id): {ministryAdminId} deleting was started. User(id): {userId}");

        var ministryAdmin = await institutionAdminRepository.GetByIdAsync(ministryAdminId)
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

    public async Task<ResponseDto> BlockMinistryAdminAsync(string ministryAdminId, string userId, string token)
    {
        logger.LogDebug($"MinistryAdmin(id): {ministryAdminId} blocking was started. User(id): {userId}");

        var ministryAdmin = await institutionAdminRepository.GetByIdAsync(ministryAdminId)
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

    /// <inheritdoc/>
    public async Task<bool> IsProviderSubordinateAsync(string ministryAdminUserId, Guid providerId)
    {
        ArgumentNullException.ThrowIfNull(ministryAdminUserId);

        return await institutionAdminRepository
            .Any(x => x.UserId == ministryAdminUserId
                      && x.Institution.RelatedProviders.Any(rp => rp.Id == providerId)).ConfigureAwait(false);
    }
}