﻿using System.Linq;
using System.Linq.Expressions;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Nest;
using Newtonsoft.Json;
using OutOfSchool.Common.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.WebApi.Enums;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services;

public class ProviderAdminService : CommunicationService, IProviderAdminService
{
    private readonly string includingPropertiesForMaping = $"{nameof(ProviderAdmin.ManagedWorkshops)}";

    private readonly IdentityServerConfig identityServerConfig;
    private readonly ProviderAdminConfig providerAdminConfig;
    private readonly IEntityRepository<string, User> userRepository;
    private readonly IProviderAdminRepository providerAdminRepository;
    private readonly IMapper mapper;
    private readonly IProviderAdminOperationsService providerAdminOperationsService;
    private readonly IWorkshopService workshopService;

    public ProviderAdminService(
        IHttpClientFactory httpClientFactory,
        IOptions<IdentityServerConfig> identityServerConfig,
        IOptions<ProviderAdminConfig> providerAdminConfig,
        IOptions<CommunicationConfig> communicationConfig,
        IProviderAdminRepository providerAdminRepository,
        IEntityRepository<string, User> userRepository,
        IMapper mapper,
        ILogger<ProviderAdminService> logger,
        IProviderAdminOperationsService providerAdminOperationsService,
        IWorkshopService workshopService)
        : base(httpClientFactory, communicationConfig.Value, logger)
    {
        this.identityServerConfig = identityServerConfig.Value;
        this.providerAdminConfig = providerAdminConfig.Value;
        this.providerAdminRepository = providerAdminRepository;
        this.userRepository = userRepository;
        this.mapper = mapper;
        this.providerAdminOperationsService = providerAdminOperationsService;
        this.workshopService = workshopService;
    }

    public async Task<Either<ErrorResponse, CreateProviderAdminDto>> CreateProviderAdminAsync(
        string userId,
        CreateProviderAdminDto providerAdminDto,
        string token)
    {
        Logger.LogDebug("ProviderAdmin creating was started. User(id): {UserId}", userId);

        var hasAccess = await IsAllowedCreateAsync(providerAdminDto.ProviderId, userId, providerAdminDto.IsDeputy)
            .ConfigureAwait(true);

        if (!hasAccess)
        {
            Logger.LogError("User(id): {UserId} doesn't have permission to create provider admin", userId);

            return new ErrorResponse
            {
                HttpStatusCode = HttpStatusCode.Forbidden,
            };
        }

        var numberProviderAdminsLessThanMax = await providerAdminRepository
            .GetNumberProviderAdminsAsync(providerAdminDto.ProviderId)
            .ConfigureAwait(false);

        if (numberProviderAdminsLessThanMax >= providerAdminConfig.MaxNumberAdmins)
        {
            Logger.LogError("Admin was not created by User(id): {UserId}. Limit on the number of admins has been exceeded for the Provider(id): {providerAdminDto.ProviderId}", userId, providerAdminDto.ProviderId);

            return new ErrorResponse
            {
                HttpStatusCode = HttpStatusCode.MethodNotAllowed,
            };
        }

        return await providerAdminOperationsService
            .CreateProviderAdminAsync(userId, providerAdminDto, token)
            .ConfigureAwait(false);
    }

    public async Task<Either<ErrorResponse, UpdateProviderAdminDto>> UpdateProviderAdminAsync(
        UpdateProviderAdminDto providerAdminModel,
        string userId,
        Guid providerId,
        string token)
    {
        _ = providerAdminModel ?? throw new ArgumentNullException(nameof(providerAdminModel));

        Logger.LogDebug("ProviderAdmin(id): {ProviderAdminId} updating was started. User(id): {UserId}", providerAdminModel.Id, userId);

        var hasAccess = await IsAllowedAsync(providerId, userId)
            .ConfigureAwait(true);

        if (!hasAccess)
        {
            Logger.LogError("User(id): {UserId} doesn't have permission to update provider admin", userId);

            return new ErrorResponse
            {
                HttpStatusCode = HttpStatusCode.Forbidden,
            };
        }

        var provideradmin = await providerAdminRepository.GetByIdAsync(providerAdminModel.Id, providerId)
            .ConfigureAwait(false);

        if (provideradmin is null)
        {
            Logger.LogError("ProviderAdmin(id) {ProviderAdminId} not found. User(id): {UserId}", providerAdminModel.Id, userId);

            return new ErrorResponse
            {
                HttpStatusCode = HttpStatusCode.NotFound,
            };
        }

        var request = new Request()
        {
            HttpMethodType = HttpMethodType.Put,
            Url = new Uri(identityServerConfig.Authority, CommunicationConstants.UpdateProviderAdmin + providerAdminModel.Id),
            Token = token,
            Data = providerAdminModel,
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
                    .DeserializeObject<UpdateProviderAdminDto>(result.Result.ToString())
                : null);
    }

    public async Task<Either<ErrorResponse, ActionResult>> DeleteProviderAdminAsync(
        string providerAdminId,
        string userId,
        Guid providerId,
        string token)
    {
        Logger.LogDebug("ProviderAdmin(id): {ProviderAdminId} deleting was started. User(id): {UserId}", providerAdminId, userId);

        var hasAccess = await IsAllowedAsync(providerId, userId)
            .ConfigureAwait(true);

        if (!hasAccess)
        {
            Logger.LogError("User(id): {UserId} doesn't have permission to delete provider admin", userId);

            return new ErrorResponse
            {
                HttpStatusCode = HttpStatusCode.Forbidden,
            };
        }

        var provideradmin = await providerAdminRepository.GetByIdAsync(providerAdminId, providerId)
            .ConfigureAwait(false);

        if (provideradmin is null)
        {
            Logger.LogError("ProviderAdmin(id) {ProviderAdminId} not found. User(id): {UserId}", providerAdminId, userId);

            return new ErrorResponse
            {
                HttpStatusCode = HttpStatusCode.NotFound,
            };
        }

        var request = new Request()
        {
            HttpMethodType = HttpMethodType.Delete,
            Url = new Uri(identityServerConfig.Authority, CommunicationConstants.DeleteProviderAdmin + providerAdminId),
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

    public async Task<Either<ErrorResponse, ActionResult>> BlockProviderAdminAsync(
        string providerAdminId,
        string userId,
        Guid providerId,
        string token,
        bool isBlocked)
    {
        Logger.LogDebug("ProviderAdmin(id): {ProviderAdminId} blocking was started. User(id): {UserId}", providerAdminId, userId);

        var hasAccess = await IsAllowedAsync(providerId, userId)
            .ConfigureAwait(true);

        if (!hasAccess)
        {
            Logger.LogError("User(id): {UserId} doesn't have permission to block provider admin", userId);

            return new ErrorResponse
            {
                HttpStatusCode = HttpStatusCode.Forbidden,
            };
        }

        var provideradmin = await providerAdminRepository.GetByIdAsync(providerAdminId, providerId)
            .ConfigureAwait(false);

        if (provideradmin is null)
        {
            Logger.LogError("ProviderAdmin(id) {ProviderAdminId} not found. User(id): {UserId}", providerAdminId, userId);

            return new ErrorResponse
            {
                HttpStatusCode = HttpStatusCode.NotFound,
            };
        }

        var request = new Request()
        {
            HttpMethodType = HttpMethodType.Put,
            Url = new Uri(identityServerConfig.Authority, string.Concat(
                CommunicationConstants.BlockProviderAdmin,
                providerAdminId,
                new PathString("/"),
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

    public async Task<bool> IsAllowedCreateAsync(Guid providerId, string userId, bool isDeputy)
    {
        bool providerAdminDeputy = await providerAdminRepository
            .IsExistProviderAdminDeputyWithUserIdAsync(providerId, userId)
            .ConfigureAwait(false);

        bool provider = await providerAdminRepository.IsExistProviderWithUserIdAsync(userId)
            .ConfigureAwait(false);

        // provider admin deputy can create only assistants
        return (providerAdminDeputy && !isDeputy) || provider;
    }

    public async Task<bool> IsAllowedAsync(Guid providerId, string userId)
    {
        bool provider = await providerAdminRepository.IsExistProviderWithUserIdAsync(userId)
            .ConfigureAwait(false);

        return provider;
    }

    public async Task GiveAssistantAccessToWorkshop(string userId, Guid workshopId)
    {
        await providerAdminRepository.AddRelatedWorkshopForAssistant(userId, workshopId).ConfigureAwait(false);
        Logger.LogDebug("Assistant provider admin(id): {UserId} now is related to workshop(id): {WorkshopId}", userId, workshopId);
    }

    public async Task<IEnumerable<Guid>> GetRelatedWorkshopIdsForProviderAdmins(string userId)
    {
        var providersAdmins = await providerAdminRepository.GetByFilter(p => p.UserId == userId && !p.IsDeputy)
            .ConfigureAwait(false);
        return providersAdmins.SelectMany(admin => admin.ManagedWorkshops, (admin, workshops) => new {workshops})
            .Select(x => x.workshops.Id);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<WorkshopProviderViewCard>> GetWorkshopsThatProviderAdminCanManage(
        string userId,
        bool isProviderDeputy)
    {
        var providersAdmins = await providerAdminRepository
            .GetByFilter(p => p.UserId == userId && p.IsDeputy == isProviderDeputy, includingPropertiesForMaping)
            .ConfigureAwait(false);

        if (!providersAdmins.Any())
        {
            return new List<WorkshopProviderViewCard>();
        }

        if (isProviderDeputy)
        {
            var providerAdmin = providersAdmins.SingleOrDefault(x => x.IsDeputy);
            if (providerAdmin == null) {
                return new List<WorkshopProviderViewCard>();
            }

            var offsetFilter = new OffsetFilter() { From = 0, Size = int.MaxValue };
            return await workshopService.GetByProviderId<WorkshopProviderViewCard>(providerAdmin.ProviderId, offsetFilter).ConfigureAwait(false);
        }

        return providersAdmins.SingleOrDefault().ManagedWorkshops
            .Select(workshop => mapper.Map<WorkshopProviderViewCard>(workshop));
    }

    /// <inheritdoc/>
    public async Task<ProviderAdminProviderRelationDto> GetById(string userId)
    {
        var providerAdmin = (await providerAdminRepository.GetByFilter(p => p.UserId == userId).ConfigureAwait(false))
            .SingleOrDefault();

        if (providerAdmin == null)
        {
            return null;
        }

        return mapper.Map<ProviderAdminProviderRelationDto>(providerAdmin);
    }

    public async Task<bool> CheckUserIsRelatedProviderAdmin(string userId, Guid providerId, Guid workshopId = default)
    {
        var providerAdmin = await providerAdminRepository.GetByIdAsync(userId, providerId).ConfigureAwait(false);

        if (!providerAdmin.IsDeputy && workshopId != Guid.Empty)
        {
            return providerAdmin.ManagedWorkshops.Any(w => w.Id == workshopId);
        }

        return providerAdmin != null;
    }

    public async Task<SearchResult<ProviderAdminDto>> GetFilteredRelatedProviderAdmins(string userId, ProviderAdminSearchFilter filter)
    {
        filter ??= new ProviderAdminSearchFilter();
        ModelValidationHelper.ValidateOffsetFilter(filter);

        var relatedAdmins = await GetRelatedProviderAdmins(userId).ConfigureAwait(false);

        int totalAmount;

        if (string.IsNullOrEmpty(filter.SearchString) && (filter.DeputyOnly == filter.AssistantsOnly))
        {
            totalAmount = relatedAdmins.Count();
        }
        else
        {
            var filterPredicate = PredicateBuild(filter).Compile();
            totalAmount = relatedAdmins.Count(filterPredicate);
            relatedAdmins = relatedAdmins.Where(filterPredicate);
        }

        var providerAdmins = relatedAdmins.Skip(filter.From).Take(filter.Size).ToList();

        var searchResult = new SearchResult<ProviderAdminDto>()
        {
            TotalAmount = totalAmount,
            Entities = providerAdmins,
        };

        return searchResult;
    }

    public async Task<IEnumerable<ProviderAdminDto>> GetRelatedProviderAdmins(string userId)
    {
        var provider = await providerAdminRepository.GetProviderWithUserIdAsync(userId).ConfigureAwait(false);
        List<ProviderAdmin> providerAdmins = new List<ProviderAdmin>();
        List<ProviderAdminDto> dtos = new List<ProviderAdminDto>();

        if (provider != null)
        {
            providerAdmins = (await providerAdminRepository.GetByFilter(pa => pa.ProviderId == provider.Id)
                    .ConfigureAwait(false))
                .ToList();
        }
        else
        {
            var providerAdmin =
                (await providerAdminRepository.GetByFilter(p => p.UserId == userId).ConfigureAwait(false))
                .SingleOrDefault();
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
                var dto = mapper.Map<ProviderAdminDto>(user);
                dto.IsDeputy = pa.IsDeputy;

                if (user.IsBlocked)
                {
                    dto.AccountStatus = AccountStatus.Blocked;
                }
                else
                {
                    dto.AccountStatus = user.LastLogin == DateTimeOffset.MinValue
                        ? AccountStatus.NeverLogged
                        : AccountStatus.Accepted;
                }

                dtos.Add(dto);
            }
        }

        return dtos;
    }

    public async Task<IEnumerable<string>> GetProviderAdminsIds(Guid workshopId)
    {
        var providerAdmins = await providerAdminRepository.GetByFilter(p =>
            p.ManagedWorkshops.Any(w => w.Id == workshopId)
            && !p.IsDeputy).ConfigureAwait(false);

        return providerAdmins.Select(a => a.UserId);
    }

    public async Task<IEnumerable<string>> GetProviderDeputiesIds(Guid providerId)
    {
        var providersDeputies = await providerAdminRepository.GetByFilter(p => p.ProviderId == providerId
                                                                               && p.IsDeputy).ConfigureAwait(false);

        return providersDeputies.Select(d => d.UserId);
    }

    private static Expression<Func<ProviderAdminDto, bool>> PredicateBuild(ProviderAdminSearchFilter filter)
    {
        var predicate = PredicateBuilder.True<ProviderAdminDto>();

        if (!string.IsNullOrWhiteSpace(filter.SearchString))
        {
            var tempPredicate = PredicateBuilder.False<ProviderAdminDto>();

            foreach (var word in filter.SearchString.Split(' ', ',', StringSplitOptions.RemoveEmptyEntries))
            {
                tempPredicate = tempPredicate.Or(
                    x => x.FirstName.StartsWith(word, StringComparison.InvariantCultureIgnoreCase)
                        || x.LastName.StartsWith(word, StringComparison.InvariantCultureIgnoreCase)
                        || x.MiddleName.StartsWith(word, StringComparison.InvariantCultureIgnoreCase)
                        || x.Email.StartsWith(word, StringComparison.InvariantCultureIgnoreCase)
                        || x.PhoneNumber.Contains(word, StringComparison.InvariantCulture));
            }

            predicate = predicate.And(tempPredicate);
        }

        if (filter.DeputyOnly != filter.AssistantsOnly)
        {
            predicate = predicate.And(p => p.IsDeputy == filter.DeputyOnly);
        }

        return predicate;
    }
}