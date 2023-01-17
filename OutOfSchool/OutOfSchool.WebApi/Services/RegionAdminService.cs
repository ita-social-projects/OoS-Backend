using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using AutoMapper;
using Castle.Core.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using Newtonsoft.Json;
using OutOfSchool.Common.Models;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services;

public class RegionAdminService : CommunicationService, IRegionAdminService
{
    private readonly IdentityServerConfig identityServerConfig;
    private readonly IRegionAdminRepository regionAdminRepository;
    private readonly IEntityRepository<string, User> userRepository;
    private readonly IMapper mapper;
    private readonly ICurrentUserService currentUserService;
    private readonly IMinistryAdminService ministryAdminService;

    public RegionAdminService(
        IHttpClientFactory httpClientFactory,
        IOptions<IdentityServerConfig> identityServerConfig,
        IOptions<CommunicationConfig> communicationConfig,
        IRegionAdminRepository regionAdminRepository,
        ILogger<RegionAdminService> logger,
        IEntityRepository<string, User> userRepository,
        IMapper mapper,
        ICurrentUserService currentUserService,
        IMinistryAdminService ministryAdminService)
        : base(httpClientFactory, communicationConfig?.Value, logger)
    {
        ArgumentNullException.ThrowIfNull(identityServerConfig);
        ArgumentNullException.ThrowIfNull(regionAdminRepository);
        ArgumentNullException.ThrowIfNull(userRepository);
        ArgumentNullException.ThrowIfNull(mapper);
        ArgumentNullException.ThrowIfNull(ministryAdminService);

        this.identityServerConfig = identityServerConfig.Value;
        this.regionAdminRepository = regionAdminRepository;
        this.userRepository = userRepository;
        this.mapper = mapper;
        this.currentUserService = currentUserService;
        this.ministryAdminService = ministryAdminService;
    }

    public async Task<RegionAdminDto> GetByIdAsync(string id)
    {
        Logger.LogInformation("Getting RegionAdmin by Id started. Looking Id = {Id}", id);

        var regionAdmin = await regionAdminRepository.GetByIdAsync(id).ConfigureAwait(false);

        if (regionAdmin is null)
        {
            return null;
        }

        Logger.LogInformation("Successfully got a RegionAdmin with Id = {Id}", id);

        return mapper.Map<RegionAdminDto>(regionAdmin);
    }

    public async Task<RegionAdminDto> GetByUserId(string id)
    {
        Logger.LogInformation("Getting RegionAdmin by UserId started. Looking UserId is {Id}", id);

        RegionAdmin regionAdmin = (await regionAdminRepository.GetByFilter(p => p.UserId == id).ConfigureAwait(false)).FirstOrDefault();

        if (regionAdmin == null)
        {
            Logger.LogError("There is no RegionAdmin in the Db with such User id {Id}", id);
            throw new ArgumentException($"There is no RegionAdmin in the Db with such User id {id}");
        }

        Logger.LogInformation("Successfully got a RegionAdmin with UserId = {Id}", id);

        return mapper.Map<RegionAdminDto>(regionAdmin);
    }

    public async Task<Either<ErrorResponse, RegionAdminBaseDto>> CreateRegionAdminAsync(string userId, RegionAdminBaseDto regionAdminBaseDto, string token)
    {
        Logger.LogDebug("RegionAdmin creating was started. User(id): {UserId}", userId);

        _ = regionAdminBaseDto ?? throw new ArgumentNullException(nameof(regionAdminBaseDto));

        if (await IsSuchEmailExisted(regionAdminBaseDto.Email))
        {
            Logger.LogDebug("RegionAdmin creating is not possible. Username {Email} is already taken", regionAdminBaseDto.Email);
            throw new InvalidOperationException($"Username {regionAdminBaseDto.Email} is already taken.");
        }

        var request = new Request()
        {
            HttpMethodType = HttpMethodType.Post,
            Url = new Uri(identityServerConfig.Authority, CommunicationConstants.CreateRegionAdmin),
            Token = token,
            Data = regionAdminBaseDto,
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
                    .DeserializeObject<RegionAdminBaseDto>(result.Result.ToString())
                : null);
    }

    /// <inheritdoc/>
    public async Task<SearchResult<RegionAdminDto>> GetByFilter(RegionAdminFilter filter)
    {
        Logger.LogInformation("Getting all RegionAdmins started (by filter)");

        filter ??= new RegionAdminFilter();
        ModelValidationHelper.ValidateOffsetFilter(filter);

        var filterPredicate = PredicateBuild(filter);

        if (currentUserService.IsTechAdmin())
        {
            if (filter.InstitutionId != Guid.Empty)
            {
                filterPredicate = filterPredicate.And(p => p.InstitutionId == filter.InstitutionId);
            }

            if (filter.CATOTTGId != 0)
            {
                filterPredicate = filterPredicate.And(p => p.CATOTTGId == filter.CATOTTGId);
            }
        }

        if (currentUserService.IsMinistryAdmin())
        {
            var ministryAdmin = await GetByUserId(currentUserService.UserId);

            if (filter.InstitutionId == ministryAdmin.InstitutionId || filter.InstitutionId == Guid.Empty)
            {
                filterPredicate = filterPredicate.And(p => p.InstitutionId == ministryAdmin.InstitutionId);
            }
            else
            {
                Logger.LogInformation($"Filter institutionId {filter.InstitutionId} is not equals to logined Ministry admin institutionId {ministryAdmin.InstitutionId}");

                return new SearchResult<RegionAdminDto>()
                {
                    TotalAmount = 0,
                    Entities = default,
                };
            }

            if (filter.CATOTTGId != 0)
            {
                filterPredicate = filterPredicate.And(p => p.CATOTTGId == filter.CATOTTGId);
            }
        }

        if (currentUserService.IsRegionAdmin())
        {
            var regionAdmin = await GetByUserId(currentUserService.UserId);

            if ((filter.InstitutionId == regionAdmin.InstitutionId || filter.InstitutionId == Guid.Empty)
                && (filter.CATOTTGId == regionAdmin.CATOTTGId || filter.CATOTTGId == 0))
            {
                filterPredicate = filterPredicate.And(p => p.InstitutionId == regionAdmin.InstitutionId);
                filterPredicate = filterPredicate.And(p => p.CATOTTGId == regionAdmin.CATOTTGId);
            }
            else
            {
                Logger.LogInformation($"Filter institutionId {filter.InstitutionId} or CATOTTGId {filter.CATOTTGId} " +
                        $"is not equals to logined Region admin institutionId {regionAdmin.InstitutionId} or CATOTTGId {regionAdmin.CATOTTGId}");

                return new SearchResult<RegionAdminDto>()
                {
                    TotalAmount = 0,
                    Entities = default,
                };
            }
        }

        int count = await regionAdminRepository.Count(filterPredicate).ConfigureAwait(false);

        var sortExpression = new Dictionary<Expression<Func<RegionAdmin, object>>, SortDirection>
        {
            { x => x.User.LastName, SortDirection.Ascending },
        };
        var regionAdmins = await regionAdminRepository
            .Get(
                skip: filter.From,
                take: filter.Size,
                includeProperties: "Institution,User,CATOTTG",
                where: filterPredicate,
                orderBy: sortExpression,
                asNoTracking: true)
            .ToListAsync()
            .ConfigureAwait(false);

        Logger.LogInformation(!regionAdmins.Any()
            ? "RegionAdmins table is empty."
            : $"All {regionAdmins.Count} records were successfully received from the RegionAdmins table");

        var regionAdminsDto = regionAdmins.Select(admin => mapper.Map<RegionAdminDto>(admin)).ToList();

        var result = new SearchResult<RegionAdminDto>()
        {
            TotalAmount = count,
            Entities = regionAdminsDto,
        };

        return result;
    }

    /// <inheritdoc/>
    public async Task<Either<ErrorResponse, RegionAdminBaseDto>> UpdateRegionAdminAsync(
        string userId,
        RegionAdminBaseDto updateRegionAdminDto,
        string token)
    {
        _ = updateRegionAdminDto ?? throw new ArgumentNullException(nameof(updateRegionAdminDto));

        Logger.LogDebug("RegionAdmin(id): {RegionAdminId} updating was started. User(id): {UserId}", updateRegionAdminDto.UserId, userId);

        var regionAdmin = await regionAdminRepository.GetByIdAsync(updateRegionAdminDto.UserId)
            .ConfigureAwait(false);

        if (regionAdmin is null)
        {
            Logger.LogError("RegionAdmin(id) {RegionAdminId} not found. User(id): {UserId}", updateRegionAdminDto.UserId, userId);

            return new ErrorResponse
            {
                HttpStatusCode = HttpStatusCode.NotFound,
            };
        }

        var request = new Request()
        {
            HttpMethodType = HttpMethodType.Put,
            Url = new Uri(identityServerConfig.Authority, CommunicationConstants.UpdateRegionAdmin + updateRegionAdminDto.UserId),
            Token = token,
            Data = updateRegionAdminDto,
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
                    .DeserializeObject<RegionAdminBaseDto>(result.Result.ToString())
                : null);
    }

    public async Task<Either<ErrorResponse, ActionResult>> DeleteRegionAdminAsync(string regionAdminId, string userId, string token)
    {
        Logger.LogDebug("RegionAdmin(id): {RegionAdminId} deleting was started. User(id): {UserId}", regionAdminId, userId);

        var regionAdmin = await regionAdminRepository.GetByIdAsync(regionAdminId)
            .ConfigureAwait(false);

        if (regionAdmin is null)
        {
            Logger.LogError("RegionAdmin(id) {RegionAdminId} not found. User(id): {UserId}", regionAdminId, userId);

            return new ErrorResponse
            {
                HttpStatusCode = HttpStatusCode.NotFound,
            };
        }

        var request = new Request()
        {
            HttpMethodType = HttpMethodType.Delete,
            Url = new Uri(identityServerConfig.Authority, CommunicationConstants.DeleteRegionAdmin + regionAdminId),
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

    public async Task<Either<ErrorResponse, ActionResult>> BlockRegionAdminAsync(string regionAdminId, string userId, string token, bool isBlocked)
    {
        Logger.LogDebug("RegionAdmin(id): {RegionAdminId} blocking was started. User(id): {UserId}", regionAdminId, userId);

        var regionAdmin = await regionAdminRepository.GetByIdAsync(regionAdminId)
            .ConfigureAwait(false);

        if (regionAdmin is null)
        {
            Logger.LogError("RegionAdmin(id) {RegionAdminId} not found. User(id): {UserId}", regionAdminId, userId);

            return new ErrorResponse
            {
                HttpStatusCode = HttpStatusCode.NotFound,
            };
        }

        var request = new Request()
        {
            HttpMethodType = HttpMethodType.Put,
            Url = new Uri(identityServerConfig.Authority, string.Concat(
                CommunicationConstants.BlockRegionAdmin,
                regionAdminId,
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
    public async Task<bool> IsRegionAdminSubordinateAsync(string ministryAdminUserId, string regionAdminId)
    {
        _= ministryAdminUserId ?? throw new ArgumentNullException(nameof(ministryAdminUserId));
        _ = regionAdminId ?? throw new ArgumentNullException(nameof(regionAdminId));

        var ministryAdmin = await ministryAdminService.GetByIdAsync(ministryAdminUserId).ConfigureAwait(false);
        var regionAdmin = await regionAdminRepository.GetByIdAsync(regionAdminId).ConfigureAwait(false);

        return ministryAdmin.InstitutionId == regionAdmin.InstitutionId;
    }

    private static Expression<Func<RegionAdmin, bool>> PredicateBuild(RegionAdminFilter filter)
    {
        var predicate = PredicateBuilder.True<RegionAdmin>();

        if (!string.IsNullOrWhiteSpace(filter.SearchString))
        {
            var tempPredicate = PredicateBuilder.False<RegionAdmin>();

            foreach (var word in filter.SearchString.Split(' ', ',', StringSplitOptions.RemoveEmptyEntries))
            {
                tempPredicate = tempPredicate.Or(
                    x => x.User.FirstName.Contains(word, StringComparison.InvariantCultureIgnoreCase)
                         || x.User.LastName.Contains(word, StringComparison.InvariantCultureIgnoreCase)
                         || x.User.Email.Contains(word, StringComparison.InvariantCultureIgnoreCase)
                         || x.Institution.Title.Contains(word, StringComparison.InvariantCultureIgnoreCase)
                         || x.User.PhoneNumber.Contains(word, StringComparison.InvariantCultureIgnoreCase)
                         || x.CATOTTG.Name.Contains(word, StringComparison.InvariantCultureIgnoreCase));
            }

            predicate = predicate.And(tempPredicate);
        }

        if (filter.InstitutionId != Guid.Empty)
        {
            predicate = predicate.And(a => a.Institution.Id == filter.InstitutionId);
        }

        if (filter.CATOTTGId > 0)
        {
            predicate = predicate.And(c => c.CATOTTG.Id == filter.CATOTTGId);
        }

        return predicate;
    }

    private async Task<bool> IsSuchEmailExisted(string email)
    {
        var result = await userRepository.GetByFilter(x => x.Email == email);
        return !result.IsNullOrEmpty();
    }
}