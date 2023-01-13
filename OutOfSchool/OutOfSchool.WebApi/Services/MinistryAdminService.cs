﻿using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using AutoMapper;
using Castle.Core.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using Newtonsoft.Json;
using OutOfSchool.Common.Models;
using OutOfSchool.Services.Enums;
using OutOfSchool.WebApi.Common;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services;

public class MinistryAdminService : CommunicationService, IMinistryAdminService
{
    private readonly IdentityServerConfig identityServerConfig;
    private readonly IInstitutionAdminRepository institutionAdminRepository;
    private readonly IEntityRepository<string, User> userRepository;
    private readonly IMapper mapper;
    private readonly ICurrentUserService currentUserService;

    public MinistryAdminService(
        IHttpClientFactory httpClientFactory,
        IOptions<IdentityServerConfig> identityServerConfig,
        IOptions<CommunicationConfig> communicationConfig,
        IInstitutionAdminRepository institutionAdminRepository,
        ILogger<MinistryAdminService> logger,
        IEntityRepository<string, User> userRepository,
        IMapper mapper,
        ICurrentUserService currentUserService)
        : base(httpClientFactory, communicationConfig?.Value, logger)
    {
        this.identityServerConfig = (identityServerConfig ?? throw new ArgumentNullException(nameof(identityServerConfig))).Value;
        this.institutionAdminRepository = institutionAdminRepository ?? throw new ArgumentNullException(nameof(institutionAdminRepository));
        this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        this.currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
    }

    public async Task<MinistryAdminDto> GetByIdAsync(string id)
    {
        Logger.LogInformation("Getting InstitutionAdmin by Id started. Looking Id = {Id}", id);

        var institutionAdmin = await institutionAdminRepository.GetByIdAsync(id).ConfigureAwait(false);

        if (institutionAdmin is null)
        {
            return null;
        }

        Logger.LogInformation("Successfully got a InstitutionAdmin with Id = {Id}", id);

        return mapper.Map<MinistryAdminDto>(institutionAdmin);
    }

    public async Task<MinistryAdminDto> GetByUserId(string id)
    {
        Logger.LogInformation("Getting MinistryAdmin by UserId started. Looking UserId is {Id}", id);

        InstitutionAdmin ministryAdmin = (await institutionAdminRepository.GetByFilter(p => p.UserId == id).ConfigureAwait(false)).FirstOrDefault();

        if (ministryAdmin == null)
        {
            Logger.LogError("There is no MinistryAdmin in the Db with such User id {Id}", id);
            throw new ArgumentException($"There is no MinistryAdmin in the Db with such User id {id}");
        }

        Logger.LogInformation("Successfully got a MinistryAdmin with UserId = {Id}", id);

        return mapper.Map<MinistryAdminDto>(ministryAdmin);
    }

    public async Task<Either<ErrorResponse, MinistryAdminBaseDto>> CreateMinistryAdminAsync(string userId, MinistryAdminBaseDto ministryAdminBaseDto, string token)
    {
        Logger.LogDebug("ministryAdmin creating was started. User(id): {UserId}", userId);

        ArgumentNullException.ThrowIfNull(ministryAdminBaseDto);

        if (await IsSuchEmailExisted(ministryAdminBaseDto.Email))
        {
            Logger.LogDebug("ministryAdmin creating is not possible. Username {Email} is already taken", ministryAdminBaseDto.Email);
            throw new InvalidOperationException($"Username {ministryAdminBaseDto.Email} is already taken.");
        }

        var request = new Request()
        {
            HttpMethodType = HttpMethodType.Post,
            Url = new Uri(identityServerConfig.Authority, CommunicationConstants.CreateMinistryAdmin),
            Token = token,
            Data = ministryAdminBaseDto,
            RequestId = Guid.NewGuid(),
        };

        Logger.LogDebug(
            "{request.HttpMethodType} Request(id): {request.RequestId} was sent. User(id): {UserId}. Url: {request.Url}",
            request.HttpMethodType,
            request.RequestId,
            userId,
            request.Url);

        var response = await SendRequest<ResponseDto>(request)
            .ConfigureAwait(false);

        return response
            .FlatMap<ResponseDto>(r => r.IsSuccess
                ? r
                : new ErrorResponse
                {
                    HttpStatusCode = r.HttpStatusCode,
                    Message = r.Message,
                })
            .Map(result => result.Result is not null
                ? JsonConvert
                    .DeserializeObject<MinistryAdminBaseDto>(result.Result.ToString())
                : null);
    }

    /// <inheritdoc/>
    public async Task<SearchResult<MinistryAdminDto>> GetByFilter(MinistryAdminFilter filter)
    {
        Logger.LogInformation("Getting all Ministry admins started (by filter)");

        filter ??= new MinistryAdminFilter();
        ModelValidationHelper.ValidateOffsetFilter(filter);

        var filterPredicate = PredicateBuild(filter);

        if (currentUserService.IsMinistryAdmin())
        {
            var ministryAdmin = await GetByUserId(currentUserService.UserId);

            if (filter.InstitutionId == Guid.Empty)
            {
                filterPredicate = filterPredicate.And(p => p.InstitutionId == ministryAdmin.InstitutionId);
            }
            else if (ministryAdmin.InstitutionId != filter.InstitutionId)
            {
                Logger.LogInformation($"Filter institutionId {filter.InstitutionId} is not equals to logined Ministry admin institutionId {ministryAdmin.InstitutionId}");

                return new SearchResult<MinistryAdminDto>()
                {
                    TotalAmount = 0,
                    Entities = default,
                };
            }
        }

        int count = await institutionAdminRepository.Count(filterPredicate).ConfigureAwait(false);

        var sortExpression = new Dictionary<Expression<Func<InstitutionAdmin, object>>, SortDirection>
        {
            { x => x.User.LastName, SortDirection.Ascending },
        };
        var institutionAdmins = await institutionAdminRepository
            .Get(
                skip: filter.From,
                take: filter.Size,
                includeProperties: "Institution,User",
                where: filterPredicate,
                orderBy: sortExpression,
                asNoTracking: true)
            .ToListAsync()
            .ConfigureAwait(false);

        Logger.LogInformation(!institutionAdmins.Any()
            ? "Parents table is empty."
            : $"All {institutionAdmins.Count} records were successfully received from the Parent table");

        var ministryAdminsDto = institutionAdmins.Select(admin => mapper.Map<MinistryAdminDto>(admin)).ToList();

        var result = new SearchResult<MinistryAdminDto>()
        {
            TotalAmount = count,
            Entities = ministryAdminsDto,
        };

        return result;
    }

    /// <inheritdoc/>
    public async Task<Either<ErrorResponse, MinistryAdminBaseDto>> UpdateMinistryAdminAsync(
        string userId,
        MinistryAdminBaseDto updateMinistryAdminDto,
        string token)
    {
        _ = updateMinistryAdminDto ?? throw new ArgumentNullException(nameof(updateMinistryAdminDto));

        Logger.LogDebug("ProviderAdmin(id): {MinistryAdminId} updating was started. User(id): {UserId}", updateMinistryAdminDto.UserId, userId);

        // TODO Add checking if ministry Admin belongs to Institution and is exist MinistryAdmin with such UserId
        var ministryAdmin = await institutionAdminRepository.GetByIdAsync(updateMinistryAdminDto.UserId)
            .ConfigureAwait(false);

        if (ministryAdmin is null)
        {
            Logger.LogError("MinistryAdmin(id) {MinistryAdminId} not found. User(id): {UserId}", updateMinistryAdminDto.UserId, userId);

            return new ErrorResponse
            {
                HttpStatusCode = HttpStatusCode.NotFound,
            };
        }

        var request = new Request()
        {
            HttpMethodType = HttpMethodType.Put,
            Url = new Uri(identityServerConfig.Authority, CommunicationConstants.UpdateMinistryAdmin + updateMinistryAdminDto.UserId),
            Token = token,
            Data = updateMinistryAdminDto,
            RequestId = Guid.NewGuid(),
        };

        Logger.LogDebug(
            "{request.HttpMethodType} Request(id): {request.RequestId} was sent. User(id): {UserId}. Url: {request.Url}",
            request.HttpMethodType,
            request.RequestId,
            userId,
            request.Url);

        var response = await SendRequest<ResponseDto>(request)
            .ConfigureAwait(false);

        return response
            .FlatMap<ResponseDto>(r => r.IsSuccess
                ? r
                : new ErrorResponse
                {
                    HttpStatusCode = r.HttpStatusCode,
                    Message = r.Message,
                })
            .Map(result => result.Result is not null
                ? JsonConvert
                    .DeserializeObject<MinistryAdminBaseDto>(result.Result.ToString())
                : null);
    }

    public async Task<Either<ErrorResponse, ActionResult>> DeleteMinistryAdminAsync(string ministryAdminId, string userId, string token)
    {
        Logger.LogDebug("MinistryAdmin(id): {MinistryAdminId} deleting was started. User(id): {UserId}", ministryAdminId, userId);

        var ministryAdmin = await institutionAdminRepository.GetByIdAsync(ministryAdminId)
            .ConfigureAwait(false);

        if (ministryAdmin is null)
        {
            Logger.LogError("MinistryAdmin(id) {MinistryAdminId} not found. User(id): {UserId}", ministryAdminId, userId);

            return new ErrorResponse
            {
                HttpStatusCode = HttpStatusCode.NotFound,
            };
        }

        var request = new Request()
        {
            HttpMethodType = HttpMethodType.Delete,
            Url = new Uri(identityServerConfig.Authority, CommunicationConstants.DeleteMinistryAdmin + ministryAdminId),
            Token = token,
            RequestId = Guid.NewGuid(),
        };

        Logger.LogDebug(
            "{request.HttpMethodType} Request(id): {request.RequestId} was sent. User(id): {UserId}. Url: {request.Url}",
            request.HttpMethodType,
            request.RequestId,
            userId,
            request.Url);

        var response = await SendRequest<ResponseDto>(request)
            .ConfigureAwait(false);

        return response
            .FlatMap<ResponseDto>(r => r.IsSuccess
                ? r
                : new ErrorResponse
                {
                    HttpStatusCode = r.HttpStatusCode,
                    Message = r.Message,
                })
            .Map(result => result.Result is not null
                ? JsonConvert
                    .DeserializeObject<ActionResult>(result.Result.ToString())
                : null);
    }

    public async Task<Either<ErrorResponse, ActionResult>> BlockMinistryAdminAsync(string ministryAdminId, string userId, string token, bool isBlocked)
    {
        Logger.LogDebug("MinistryAdmin(id): {MinistryAdminId} blocking was started. User(id): {UserId}", ministryAdminId, userId);

        var ministryAdmin = await institutionAdminRepository.GetByIdAsync(ministryAdminId)
            .ConfigureAwait(false);

        if (ministryAdmin is null)
        {
            Logger.LogError("MinistryAdmin(id) {MinistryAdminId} not found. User(id): {UserId}", ministryAdminId, userId);

            return new ErrorResponse
            {
                HttpStatusCode = HttpStatusCode.NotFound,
            };
        }

        var request = new Request()
        {
            HttpMethodType = HttpMethodType.Put,
            Url = new Uri(identityServerConfig.Authority, string.Concat(
                CommunicationConstants.BlockMinistryAdmin,
                ministryAdminId,
                "/",
                isBlocked)),
            Token = token,
            RequestId = Guid.NewGuid(),
        };

        Logger.LogDebug(
            "{request.HttpMethodType} Request(id): {request.RequestId} was sent. User(id): {UserId}. Url: {request.Url}",
            request.HttpMethodType,
            request.RequestId,
            userId,
            request.Url);

        var response = await SendRequest<ResponseDto>(request)
            .ConfigureAwait(false);

        return response
            .FlatMap<ResponseDto>(r => r.IsSuccess
                ? r
                : new ErrorResponse
                {
                    HttpStatusCode = r.HttpStatusCode,
                    Message = r.Message,
                })
            .Map(result => result.Result is not null
                ? JsonConvert
                    .DeserializeObject<ActionResult>(result.Result.ToString())
                : null);
    }

    /// <inheritdoc/>
    public async Task<bool> IsProviderSubordinateAsync(string ministryAdminUserId, Guid providerId)
    {
        ArgumentNullException.ThrowIfNull(ministryAdminUserId);

        return await institutionAdminRepository
            .Any(x => x.UserId == ministryAdminUserId
                      && x.Institution.RelatedProviders.Any(rp => rp.Id == providerId)).ConfigureAwait(false);
    }

    private static Expression<Func<InstitutionAdmin, bool>> PredicateBuild(MinistryAdminFilter filter)
    {
        var predicate = PredicateBuilder.True<InstitutionAdmin>();

        if (!string.IsNullOrWhiteSpace(filter.SearchString))
        {
            var tempPredicate = PredicateBuilder.False<InstitutionAdmin>();

            foreach (var word in filter.SearchString.Split(' ', ',', StringSplitOptions.RemoveEmptyEntries))
            {
                tempPredicate = tempPredicate.Or(
                    x => x.User.FirstName.Contains(word, StringComparison.InvariantCultureIgnoreCase)
                         || x.User.LastName.Contains(word, StringComparison.InvariantCultureIgnoreCase)
                         || x.User.Email.Contains(word, StringComparison.InvariantCultureIgnoreCase)
                         || x.Institution.Title.Contains(word, StringComparison.InvariantCultureIgnoreCase)
                         || x.User.PhoneNumber.Contains(word, StringComparison.InvariantCultureIgnoreCase));
            }

            predicate = predicate.And(tempPredicate);
        }

        if (filter.InstitutionId != Guid.Empty)
        {
            predicate = predicate.And(a => a.Institution.Id == filter.InstitutionId);
        }

        return predicate;
    }

    private async Task<bool> IsSuchEmailExisted(string email)
    {
        var result = await userRepository.GetByFilter(x => x.Email == email);
        return !result.IsNullOrEmpty();
    }
}