using System.Linq.Expressions;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Services.SearchString;
using OutOfSchool.Common.Communication;
using OutOfSchool.Common.Models;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Services.Repository.Base.Api;

namespace OutOfSchool.BusinessLogic.Services;

public class RegionAdminService : CommunicationService, IRegionAdminService
{
    private readonly AuthorizationServerConfig authorizationServerConfig;
    private readonly IRegionAdminRepository regionAdminRepository;
    private readonly IEntityRepositorySoftDeleted<string, User> userRepository;
    private readonly IMapper mapper;
    private readonly ICurrentUserService currentUserService;
    private readonly IMinistryAdminService ministryAdminService;
    private readonly IApiErrorService apiErrorService;
    private readonly ISearchStringService searchStringService;

    public RegionAdminService(
        IHttpClientFactory httpClientFactory,
        IOptions<AuthorizationServerConfig> authorizationServerConfig,
        IOptions<CommunicationConfig> communicationConfig,
        IRegionAdminRepository regionAdminRepository,
        ILogger<RegionAdminService> logger,
        IEntityRepositorySoftDeleted<string, User> userRepository,
        IMapper mapper,
        ICurrentUserService currentUserService,
        IMinistryAdminService ministryAdminService,
        IApiErrorService apiErrorService,
        ISearchStringService searchStringService)
        : base(httpClientFactory, communicationConfig, logger)
    {
        ArgumentNullException.ThrowIfNull(authorizationServerConfig);
        ArgumentNullException.ThrowIfNull(regionAdminRepository);
        ArgumentNullException.ThrowIfNull(userRepository);
        ArgumentNullException.ThrowIfNull(mapper);
        ArgumentNullException.ThrowIfNull(ministryAdminService);
        ArgumentNullException.ThrowIfNull(searchStringService);

        this.authorizationServerConfig = authorizationServerConfig.Value;
        this.regionAdminRepository = regionAdminRepository;
        this.userRepository = userRepository;
        this.mapper = mapper;
        this.currentUserService = currentUserService;
        this.ministryAdminService = ministryAdminService;
        this.apiErrorService = apiErrorService;
        this.searchStringService = searchStringService;
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

        var badRequestApiErrorResponse = await apiErrorService.AdminsCreatingIsBadRequestDataAttend(
            regionAdminBaseDto,
            $"{nameof(RegionAdmin)}");

        if (badRequestApiErrorResponse.ApiErrors.Count != 0)
        {
            return ErrorResponse.BadRequest(badRequestApiErrorResponse);
        }

        var request = new Request()
        {
            HttpMethodType = HttpMethodType.Post,
            Url = new Uri(authorizationServerConfig.Authority, CommunicationConstants.CreateRegionAdmin),
            Token = token,
            Data = regionAdminBaseDto,
        };

        Logger.LogDebug(
            "{request.HttpMethodType} Request was sent. User(id): {UserId}. Url: {request.Url}",
            request.HttpMethodType,
            userId,
            request.Url);

        var response = await SendRequest<ResponseDto, ErrorResponse>(request)
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

        if (currentUserService.IsMinistryAdmin())
        {
            var ministryAdmin = await ministryAdminService.GetByUserId(currentUserService.UserId);

            if (filter.InstitutionId == Guid.Empty)
            {
                filter.InstitutionId = ministryAdmin.InstitutionId;
            }

            if (filter.InstitutionId != ministryAdmin.InstitutionId)
            {
                Logger.LogInformation($"Filter institutionId {filter.InstitutionId} is not equals to logined Ministry admin institutionId {ministryAdmin.InstitutionId}");

                return new SearchResult<RegionAdminDto>()
                {
                    TotalAmount = 0,
                    Entities = default,
                };
            }
        }

        if (currentUserService.IsRegionAdmin())
        {
            var regionAdmin = await GetByUserId(currentUserService.UserId);

            if (filter.InstitutionId == Guid.Empty)
            {
                filter.InstitutionId = regionAdmin.InstitutionId;
            }

            if (filter.CATOTTGId <= 0)
            {
                filter.CATOTTGId = regionAdmin.CATOTTGId;
            }

            if (filter.InstitutionId != regionAdmin.InstitutionId || filter.CATOTTGId != regionAdmin.CATOTTGId)
            {
                Logger.LogInformation($"Filter institutionId {filter.InstitutionId} and CATOTTGId {filter.CATOTTGId} " +
                        $"is not equals to logined Region admin institutionId {regionAdmin.InstitutionId} and CATOTTGId {regionAdmin.CATOTTGId}");

                return new SearchResult<RegionAdminDto>()
                {
                    TotalAmount = 0,
                    Entities = default,
                };
            }
        }

        var filterPredicate = PredicateBuild(filter);

        int count = await regionAdminRepository.Count(filterPredicate).ConfigureAwait(false);

        var sortExpression = new Dictionary<Expression<Func<RegionAdmin, object>>, SortDirection>
        {
            { x => x.User.IsBlocked, SortDirection.Ascending },
            { x => x.User.LastLogin == DateTimeOffset.MinValue, SortDirection.Descending },
            { x => x.User.LastName, SortDirection.Ascending },
        };

        var regionAdmins = await regionAdminRepository
            .Get(
                skip: filter.From,
                take: filter.Size,
                includeProperties: "Institution,User,CATOTTG",
                whereExpression: filterPredicate,
                orderBy: sortExpression,
                asNoTracking: true)
            .ToListAsync()
            .ConfigureAwait(false);

        if (regionAdmins.Any())
        {
            Logger.LogInformation($"All {regionAdmins.Count} records were successfully received from the RegionAdmins table");
        }
        else
        {
            Logger.LogInformation("RegionAdmins table is empty.");
        }

        var regionAdminsDto = regionAdmins.Select(admin => mapper.Map<RegionAdminDto>(admin)).ToList();

        var result = new SearchResult<RegionAdminDto>()
        {
            TotalAmount = count,
            Entities = regionAdminsDto,
        };

        return result;
    }

    /// <inheritdoc/>
    public async Task<Either<ErrorResponse, RegionAdminDto>> UpdateRegionAdminAsync(
        string userId,
        BaseUserDto updateRegionAdminDto,
        string token)
    {
        _ = updateRegionAdminDto ?? throw new ArgumentNullException(nameof(updateRegionAdminDto));

        Logger.LogDebug("RegionAdmin(id): {RegionAdminId} updating was started. User(id): {UserId}", updateRegionAdminDto.Id, userId);

        var regionAdmin = await regionAdminRepository.GetByIdAsync(updateRegionAdminDto.Id)
            .ConfigureAwait(false);

        if (regionAdmin is null)
        {
            Logger.LogError("RegionAdmin(id) {RegionAdminId} not found. User(id): {UserId}", updateRegionAdminDto.Id, userId);

            return new ErrorResponse
            {
                HttpStatusCode = HttpStatusCode.NotFound,
            };
        }

        var request = new Request()
        {
            HttpMethodType = HttpMethodType.Put,
            Url = new Uri(authorizationServerConfig.Authority, CommunicationConstants.UpdateRegionAdmin + updateRegionAdminDto.Id),
            Token = token,
            Data = mapper.Map<RegionAdminBaseDto>(updateRegionAdminDto),
        };

        Logger.LogDebug(
            "{request.HttpMethodType} Request was sent. User(id): {UserId}. Url: {request.Url}",
            request.HttpMethodType,
            userId,
            request.Url);

        var response = await SendRequest<ResponseDto, ErrorResponse>(request)
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
                ? mapper.Map<RegionAdminDto>(JsonConvert.DeserializeObject<RegionAdminBaseDto>(result.Result.ToString()))
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
            Url = new Uri(authorizationServerConfig.Authority, CommunicationConstants.DeleteRegionAdmin + regionAdminId),
            Token = token,
        };

        Logger.LogDebug(
            "{request.HttpMethodType} Request was sent. User(id): {UserId}. Url: {request.Url}",
            request.HttpMethodType,
            userId,
            request.Url);

        var response = await SendRequest<ResponseDto, ErrorResponse>(request)
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
            Url = new Uri(authorizationServerConfig.Authority, string.Concat(
                CommunicationConstants.BlockRegionAdmin,
                regionAdminId,
                "/",
                isBlocked)),
            Token = token,
        };

        Logger.LogDebug(
            "{request.HttpMethodType} Request was sent. User(id): {UserId}. Url: {request.Url}",
            request.HttpMethodType,
            userId,
            request.Url);

        var response = await SendRequest<ResponseDto, ErrorResponse>(request)
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
    public async Task<Either<ErrorResponse, ActionResult>> ReinviteRegionAdminAsync(
        string regionAdminId,
        string userId,
        string token)
    {
        Logger.LogDebug(
            "RegionAdmin(id): {RegionAdminId} reinvite was started. User(id): {UserId}",
            regionAdminId,
            userId);

        var regionAdmin = await regionAdminRepository.GetByIdAsync(regionAdminId).ConfigureAwait(false);
        if (regionAdmin == null)
        {
            return null;
        }

        var user = (await userRepository.GetByFilter(u => u.Id == regionAdmin.UserId).ConfigureAwait(false))
            .SingleOrDefault();
        if (user == null)
        {
            return null;
        }
        else if (user.LastLogin != DateTimeOffset.MinValue)
        {
            return new ErrorResponse
            {
                HttpStatusCode = HttpStatusCode.BadRequest,
                Message = "Only neverlogged users can be reinvited.",
            };
        }

        var request = new Request()
        {
            HttpMethodType = HttpMethodType.Put,
            Url = new Uri(authorizationServerConfig.Authority, string.Concat(
                CommunicationConstants.ReinviteRegionAdmin,
                regionAdminId,
                new PathString("/"))),
            Token = token,
        };

        Logger.LogDebug(
            "{request.HttpMethodType} Request was sent. User(id): {UserId}. Url: {request.Url}",
            request.HttpMethodType,
            userId,
            request.Url);

        var response = await SendRequest<ResponseDto, ErrorResponse>(request).ConfigureAwait(false);

        return response
            .FlatMap<ResponseDto>(r => r.IsSuccess ? r : new ErrorResponse
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
        _ = ministryAdminUserId ?? throw new ArgumentNullException(nameof(ministryAdminUserId));
        _ = regionAdminId ?? throw new ArgumentNullException(nameof(regionAdminId));

        var ministryAdmin = await ministryAdminService.GetByIdAsync(ministryAdminUserId).ConfigureAwait(false);
        var regionAdmin = await regionAdminRepository.GetByIdAsync(regionAdminId).ConfigureAwait(false);

        return ministryAdmin.InstitutionId == regionAdmin.InstitutionId;
    }

    private Expression<Func<RegionAdmin, bool>> PredicateBuild(RegionAdminFilter filter)
    {
        var predicate = PredicateBuilder.True<RegionAdmin>();

        if (!string.IsNullOrWhiteSpace(filter.SearchString))
        {
            var tempPredicate = PredicateBuilder.False<RegionAdmin>();

            foreach (var word in searchStringService.SplitSearchString(filter.SearchString))
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

        predicate = predicate.And(x => !x.Institution.IsDeleted);

        return predicate;
    }
}