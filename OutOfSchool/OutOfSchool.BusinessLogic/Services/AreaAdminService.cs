using System.Linq.Expressions;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Services.SearchString;
using OutOfSchool.Common.Communication;
using OutOfSchool.Common.Enums;
using OutOfSchool.Common.Models;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Services.Repository.Base.Api;

namespace OutOfSchool.BusinessLogic.Services;

public class AreaAdminService : CommunicationService, IAreaAdminService
{
    private readonly AuthorizationServerConfig authorizationServerConfig;
    private readonly IAreaAdminRepository areaAdminRepository;
    private readonly IEntityRepositorySoftDeleted<string, User> userRepository;
    private readonly IMapper mapper;
    private readonly ICurrentUserService currentUserService;
    private readonly IMinistryAdminService ministryAdminService;
    private readonly IRegionAdminService regionAdminService;
    private readonly ICodeficatorService codeficatorService;
    private readonly IApiErrorService apiErrorService;
    private readonly ISearchStringService searchStringService;
    private ICodeficatorRepository codeficatorRepository;

    public AreaAdminService(
        ICodeficatorRepository codeficatorRepository,
        ICodeficatorService codeficatorService,
        IHttpClientFactory httpClientFactory,
        IOptions<AuthorizationServerConfig> authorizationServerConfig,
        IOptions<CommunicationConfig> communicationConfig,
        IAreaAdminRepository areaAdminRepository,
        ILogger<AreaAdminService> logger,
        IEntityRepositorySoftDeleted<string, User> userRepository,
        IMapper mapper,
        ICurrentUserService currentUserService,
        IMinistryAdminService ministryAdminService,
        IRegionAdminService regionAdminService,
        IApiErrorService apiErrorService,
        ISearchStringService searchStringService)
        : base(httpClientFactory, communicationConfig, logger)
    {
        ArgumentNullException.ThrowIfNull(authorizationServerConfig);
        ArgumentNullException.ThrowIfNull(areaAdminRepository);
        ArgumentNullException.ThrowIfNull(userRepository);
        ArgumentNullException.ThrowIfNull(mapper);
        ArgumentNullException.ThrowIfNull(ministryAdminService);
        ArgumentNullException.ThrowIfNull(searchStringService);

        this.authorizationServerConfig = authorizationServerConfig.Value;
        this.areaAdminRepository = areaAdminRepository;
        this.userRepository = userRepository;
        this.codeficatorRepository = codeficatorRepository;
        this.mapper = mapper;
        this.currentUserService = currentUserService;
        this.ministryAdminService = ministryAdminService;
        this.regionAdminService = regionAdminService;
        this.codeficatorService = codeficatorService;
        this.apiErrorService = apiErrorService;
        this.searchStringService = searchStringService;
    }

    public async Task<AreaAdminDto> GetByIdAsync(string id)
    {
        Logger.LogInformation("Getting AreaAdmin by Id started. Looking Id = {Id}", id);

        var otgAdmin = await areaAdminRepository.GetByIdAsync(id).ConfigureAwait(false);

        if (otgAdmin is null)
        {
            return null;
        }

        Logger.LogInformation("Successfully got a AreaAdmin with Id = {Id}", id);

        return mapper.Map<AreaAdminDto>(otgAdmin);
    }

    public async Task<AreaAdminDto> GetByUserId(string id)
    {
        Logger.LogInformation("Getting AreaAdmin by UserId started. Looking UserId is {Id}", id);

        AreaAdmin areaAdmin = (await areaAdminRepository.GetByFilter(p => p.UserId == id).ConfigureAwait(false))
            .FirstOrDefault();

        if (areaAdmin == null)
        {
            Logger.LogError("There is no AreaAdmin in the Db with such User id {Id}", id);
            throw new ArgumentException($"There is no AreaAdmin in the Db with such User id {id}");
        }

        Logger.LogInformation("Successfully got a AreaAdmin with UserId = {Id}", id);

        return mapper.Map<AreaAdminDto>(areaAdmin);
    }

    public async Task<Either<ErrorResponse, AreaAdminBaseDto>> CreateAreaAdminAsync(
        string userId,
        AreaAdminBaseDto areaAdminBaseDto,
        string token)
    {
        Logger.LogDebug("RegionAdmin creating was started. User(id): {UserId}", userId);

        _ = areaAdminBaseDto ?? throw new ArgumentNullException(nameof(areaAdminBaseDto));

        var badRequestApiErrorResponse = await apiErrorService.AdminsCreatingIsBadRequestDataAttend(
            areaAdminBaseDto,
            $"{nameof(AreaAdmin)}");

        if (badRequestApiErrorResponse.ApiErrors.Count != 0)
        {
            return ErrorResponse.BadRequest(badRequestApiErrorResponse);
        }

        bool isValidCatottg = await IsValidCatottg(areaAdminBaseDto.CATOTTGId);
        if (!isValidCatottg)
        {
            Logger.LogDebug(
                "AreaAdmin creating is not possible. Catottg with Id {CatottgId} does not contain to area",
                areaAdminBaseDto.CATOTTGId);
            throw new InvalidOperationException(
                $"Catottg with Id {areaAdminBaseDto.CATOTTGId} does not contain to area.");
        }

        var request = new Request()
        {
            HttpMethodType = HttpMethodType.Post,
            Url = new Uri(authorizationServerConfig.Authority, CommunicationConstants.CreateAreaAdmin),
            Token = token,
            Data = areaAdminBaseDto,
        };

        Logger.LogDebug(
            "{HttpMethodType} Request was sent. User(id): {UserId}. Url: {Url}",
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
                    .DeserializeObject<AreaAdminBaseDto>(result.Result.ToString())
                : null);
    }

    /// <inheritdoc/>
    public async Task<SearchResult<AreaAdminDto>> GetByFilter(AreaAdminFilter filter)
    {
        Logger.LogInformation("Getting all AreaAdmins started (by filter)");

        filter ??= new AreaAdminFilter();
        ModelValidationHelper.ValidateOffsetFilter(filter);
        if (currentUserService.IsMinistryAdmin())
        {
            var ministryAdmin = await ministryAdminService.GetByUserId(currentUserService.UserId);
            filter.InstitutionId = ministryAdmin.InstitutionId;
        }

        var catottgs = new List<long>();
        if (currentUserService.IsRegionAdmin())
        {
            var regionAdminDto = await regionAdminService.GetByUserId(currentUserService.UserId).ConfigureAwait(false);
            filter.InstitutionId = regionAdminDto.InstitutionId;
            var childrenIds = await codeficatorService.GetAllChildrenIdsByParentIdAsync(regionAdminDto.CATOTTGId);
            if (filter.CATOTTGId != 0 && filter.CATOTTGId != regionAdminDto.CATOTTGId)
            {
                if (childrenIds.Contains(filter.CATOTTGId))
                {
                    childrenIds = await codeficatorService.GetAllChildrenIdsByParentIdAsync(filter.CATOTTGId);
                }
                else
                {
                    Logger.LogError($"Region admin with id = {currentUserService.UserId} tries to get list of AreaAdmins from other region");
                    throw new UnauthorizedAccessException();
                }
            }

            catottgs = childrenIds.ToList();
        }

        Expression<Func<AreaAdmin, bool>> filterPredicate = PredicateBuild(filter, catottgs);

        int count = await areaAdminRepository.Count(filterPredicate).ConfigureAwait(false);

        var sortExpression = new Dictionary<Expression<Func<AreaAdmin, object>>, SortDirection>
        {
            { x => x.User.IsBlocked, SortDirection.Ascending },
            { x => x.User.LastLogin == DateTimeOffset.MinValue, SortDirection.Descending },
            { x => x.User.LastName, SortDirection.Ascending },
        };

        var otgAdmins = await areaAdminRepository
            .Get(
                skip: filter.From,
                take: filter.Size,
                includeProperties: "Institution,User,CATOTTG.Parent.Parent",
                whereExpression: filterPredicate,
                orderBy: sortExpression,
                asNoTracking: true)
            .ToListAsync()
            .ConfigureAwait(false);

        if (otgAdmins.Any())
        {
            Logger.LogInformation(
                "All {Count} records were successfully received from the AreaAdmins table",
                otgAdmins.Count);
        }
        else
        {
            Logger.LogInformation("AreaAdmins table is empty");
        }

        var otgAdminsDto = otgAdmins.Select(admin => mapper.Map<AreaAdminDto>(admin)).ToList();

        var result = new SearchResult<AreaAdminDto>()
        {
            TotalAmount = count,
            Entities = otgAdminsDto,
        };

        return result;
    }

    /// <inheritdoc/>
    public async Task<Either<ErrorResponse, AreaAdminDto>> UpdateAreaAdminAsync(
        string userId,
        BaseUserDto updateAreaAdminDto,
        string token)
    {
        _ = updateAreaAdminDto ?? throw new ArgumentNullException(nameof(updateAreaAdminDto));

        Logger.LogDebug(
            "AreaAdmin(id): {AreaAdminDto} updating was started. User(id): {UserId}",
            updateAreaAdminDto.Id,
            userId);

        var regionAdmin = await areaAdminRepository.GetByIdAsync(updateAreaAdminDto.Id)
            .ConfigureAwait(false);

        if (regionAdmin is null)
        {
            Logger.LogError(
                "AreaAdmin(id) {AreaAdminDto} not found. User(id): {UserId}",
                updateAreaAdminDto.Id,
                userId);

            return new ErrorResponse
            {
                HttpStatusCode = HttpStatusCode.NotFound,
            };
        }

        var request = new Request()
        {
            HttpMethodType = HttpMethodType.Put,
            Url = new Uri(
                authorizationServerConfig.Authority,
                CommunicationConstants.UpdateAreaAdmin + updateAreaAdminDto.Id),
            Token = token,
            Data = mapper.Map<AreaAdminBaseDto>(updateAreaAdminDto),
        };

        Logger.LogDebug(
            "{HttpMethodType} Request was sent. User(id): {UserId}. Url: {Url}",
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
                ? mapper.Map<AreaAdminDto>(JsonConvert.DeserializeObject<AreaAdminBaseDto>(result.Result.ToString()))
                : null);
    }

    public async Task<Either<ErrorResponse, ActionResult>> DeleteAreaAdminAsync(
        string areaAdminId,
        string userId,
        string token)
    {
        Logger.LogDebug("AreaAdmin(id): {AreaAdminId} deleting was started. User(id): {UserId}", areaAdminId, userId);

        var otgAdmin = await areaAdminRepository.GetByIdAsync(areaAdminId)
            .ConfigureAwait(false);

        if (otgAdmin is null)
        {
            Logger.LogError("AreaAdmin(id) {AreaAdminId} not found. User(id): {UserId}", areaAdminId, userId);

            return new ErrorResponse
            {
                HttpStatusCode = HttpStatusCode.NotFound,
            };
        }

        var request = new Request()
        {
            HttpMethodType = HttpMethodType.Delete,
            Url = new Uri(authorizationServerConfig.Authority, CommunicationConstants.DeleteAreaAdmin + areaAdminId),
            Token = token,
        };

        Logger.LogDebug(
            "{HttpMethodType} Request was sent. User(id): {UserId}. Url: {Url}",
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

    public async Task<Either<ErrorResponse, ActionResult>> BlockAreaAdminAsync(
        string areaAdminId,
        string userId,
        string token,
        bool isBlocked)
    {
        Logger.LogDebug("AreaAdmin(id): {AreaAdminId} blocking was started. User(id): {UserId}", areaAdminId, userId);

        var otgAdmin = await areaAdminRepository.GetByIdAsync(areaAdminId)
            .ConfigureAwait(false);

        if (otgAdmin is null)
        {
            Logger.LogError("AreaAdmin(id) {AreaAdminId} not found. User(id): {UserId}", areaAdminId, userId);

            return new ErrorResponse
            {
                HttpStatusCode = HttpStatusCode.NotFound,
            };
        }

        var request = new Request()
        {
            HttpMethodType = HttpMethodType.Put,
            Url = new Uri(authorizationServerConfig.Authority, string.Concat(
                CommunicationConstants.BlockAreaAdmin,
                areaAdminId,
                "/",
                isBlocked)),
            Token = token,
        };

        Logger.LogDebug(
            "{HttpMethodType} Request was sent. User(id): {UserId}. Url: {Url}",
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
    public async Task<Either<ErrorResponse, ActionResult>> ReinviteAreaAdminAsync(
        string areaAdminId,
        string userId,
        string token)
    {
        Logger.LogDebug(
            "AreaAdmin(id): {AreaAdminId} reinvite was started. User(id): {UserId}",
            areaAdminId,
            userId);

        var areaAdmin = await areaAdminRepository.GetByIdAsync(areaAdminId).ConfigureAwait(false);
        if (areaAdmin == null)
        {
            return null;
        }

        var user = (await userRepository.GetByFilter(u => u.Id == areaAdmin.UserId).ConfigureAwait(false))
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
                CommunicationConstants.ReinviteAreaAdmin,
                areaAdminId,
                new PathString("/"))),
            Token = token,
        };

        Logger.LogDebug(
            "{HttpMethodType} Request was sent. User(id): {UserId}. Url: {Url}",
            request.HttpMethodType,
            userId,
            request.Url);

        var response = await SendRequest<ResponseDto, ErrorResponse>(request).ConfigureAwait(false);

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
    public async Task<bool> IsAreaAdminSubordinateMinistryAsync(string ministryAdminUserId, string areaAdminId)
    {
        _ = ministryAdminUserId ?? throw new ArgumentNullException(nameof(ministryAdminUserId));
        _ = areaAdminId ?? throw new ArgumentNullException(nameof(areaAdminId));

        var ministryAdmin = await ministryAdminService.GetByIdAsync(ministryAdminUserId).ConfigureAwait(false);
        var areaAdmin = await areaAdminRepository.GetByIdAsync(areaAdminId).ConfigureAwait(false);

        return ministryAdmin.InstitutionId == areaAdmin.InstitutionId;
    }

    public async Task<bool> IsAreaAdminSubordinateRegionAsync(string regionAdminUserId, string areaAdminId)
    {
        _ = regionAdminUserId ?? throw new ArgumentNullException(nameof(regionAdminUserId));
        _ = areaAdminId ?? throw new ArgumentNullException(nameof(areaAdminId));

        var regionAdmin = await regionAdminService.GetByIdAsync(regionAdminUserId).ConfigureAwait(false);
        var areaAdmin = await GetByIdAsync(areaAdminId).ConfigureAwait(false);

        return regionAdmin.InstitutionId == areaAdmin.InstitutionId && regionAdmin.CATOTTGId == areaAdmin.RegionId;
    }

    public async Task<bool> IsAreaAdminSubordinateMinistryCreateAsync(string ministryAdminUserId, Guid institutionId)
    {
        _ = ministryAdminUserId ?? throw new ArgumentNullException(nameof(ministryAdminUserId));
        var ministryAdmin = await ministryAdminService.GetByIdAsync(ministryAdminUserId).ConfigureAwait(false);

        return ministryAdmin.InstitutionId == institutionId;
    }

    public async Task<bool> IsAreaAdminSubordinateRegionCreateAsync(string regionAdminUserId, Guid institutionId, long catottgId)
    {
        _ = regionAdminUserId ?? throw new ArgumentNullException(nameof(regionAdminUserId));
        var regionAdmin = await regionAdminService.GetByIdAsync(regionAdminUserId).ConfigureAwait(false);

        var subSettlementsIds = await codeficatorService
            .GetAllChildrenIdsByParentIdAsync(regionAdmin.CATOTTGId)
            .ConfigureAwait(false);

        return regionAdmin.InstitutionId == institutionId && subSettlementsIds.Contains(catottgId);
    }

    private Expression<Func<AreaAdmin, bool>> PredicateBuild(AreaAdminFilter filter, List<long> catottgs)
    {
        Expression<Func<AreaAdmin, bool>> predicate = PredicateBuilder.True<AreaAdmin>();

        if (!string.IsNullOrWhiteSpace(filter.SearchString))
        {
            var tempPredicate = PredicateBuilder.False<AreaAdmin>();

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

        if (catottgs.Count > 0)
        {
            predicate = predicate.And(a => catottgs.Contains(a.CATOTTGId));
        }
        else if (filter.CATOTTGId > 0)
        {
            predicate = predicate.And(c => c.CATOTTG.Id == filter.CATOTTGId);
        }

        predicate = predicate.And(x => !x.Institution.IsDeleted);

        return predicate;
    }

    private async Task<bool> IsValidCatottg(long catottgId)
    {
        var catottg = await codeficatorRepository.GetById(catottgId);
        return catottg is not null && (catottg.Category == CodeficatorCategory.TerritorialCommunity.ToString() ||
                                       catottg.Category == CodeficatorCategory.SpecialStatusCity.ToString());
    }
}