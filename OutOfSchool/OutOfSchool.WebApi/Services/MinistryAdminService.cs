using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using Newtonsoft.Json;
using OutOfSchool.Common.Models;
using OutOfSchool.WebApi.Enums;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services;

public class MinistryAdminService : CommunicationService, IMinistryAdminService
{
    private readonly IdentityServerConfig identityServerConfig;
    private readonly IInstitutionAdminRepository institutionAdminRepository;
    private readonly ILogger<MinistryAdminService> logger;
    private readonly ResponseDto responseDto;

    public MinistryAdminService(
        IHttpClientFactory httpClientFactory,
        IOptions<IdentityServerConfig> identityServerConfig,
        IOptions<CommunicationConfig> communicationConfig,
        IInstitutionAdminRepository institutionAdminRepository,
        ILogger<MinistryAdminService> logger)
        : base(httpClientFactory, communicationConfig.Value)
    {
        ArgumentNullException.ThrowIfNull(httpClientFactory);
        ArgumentNullException.ThrowIfNull(identityServerConfig);
        ArgumentNullException.ThrowIfNull(communicationConfig);
        ArgumentNullException.ThrowIfNull(institutionAdminRepository);
        ArgumentNullException.ThrowIfNull(logger);

        this.identityServerConfig = identityServerConfig.Value;
        this.institutionAdminRepository = institutionAdminRepository;
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
}