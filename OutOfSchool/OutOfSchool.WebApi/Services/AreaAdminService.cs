using AutoMapper;
using Castle.Core.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OutOfSchool.Common.Models;
using OutOfSchool.Services.Enums;
using OutOfSchool.WebApi.Models;
using System.Linq.Expressions;
using OutOfSchool.Common.Enums;

namespace OutOfSchool.WebApi.Services;

public class AreaAdminService : CommunicationService, IAreaAdminService
{
    private readonly IdentityServerConfig identityServerConfig;
    private readonly IAreaAdminRepository areaAdminRepository;
    private readonly IEntityRepository<string, User> userRepository;
    private readonly IMapper mapper;
    private readonly ICurrentUserService currentUserService;
    private readonly IMinistryAdminService ministryAdminService;
    private readonly IRegionAdminService regionAdminService;
    private IAreaAdminService areaAdminServiceImplementation;
    private ICodeficatorRepository codeficatorRepository;
    private readonly ICodeficatorService codeficatorService;


    public AreaAdminService(
        ICodeficatorRepository codeficatorRepository,
        ICodeficatorService codeficatorService,
        IHttpClientFactory httpClientFactory,
        IOptions<IdentityServerConfig> identityServerConfig,
        IOptions<CommunicationConfig> communicationConfig,
        IAreaAdminRepository areaAdminRepository,
        ILogger<AreaAdminService> logger,
        IEntityRepository<string, User> userRepository,
        IMapper mapper,
        ICurrentUserService currentUserService,
        IMinistryAdminService ministryAdminService,
        IRegionAdminService regionAdminService)
        : base(httpClientFactory, communicationConfig?.Value, logger)
    {
        ArgumentNullException.ThrowIfNull(identityServerConfig);
        ArgumentNullException.ThrowIfNull(areaAdminRepository);
        ArgumentNullException.ThrowIfNull(userRepository);
        ArgumentNullException.ThrowIfNull(mapper);
        ArgumentNullException.ThrowIfNull(ministryAdminService);

        this.identityServerConfig = identityServerConfig.Value;
        this.areaAdminRepository = areaAdminRepository;
        this.userRepository = userRepository;
        this.codeficatorRepository = codeficatorRepository;
        this.mapper = mapper;
        this.currentUserService = currentUserService;
        this.ministryAdminService = ministryAdminService;
        this.regionAdminService = regionAdminService;
        this.codeficatorService = codeficatorService;
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

    public async Task<Either<ErrorResponse, AreaAdminBaseDto>> CreateAreaAdminAsync(string userId,
        AreaAdminBaseDto areaAdminBaseDto, string token)
    {
        Logger.LogDebug("RegionAdmin creating was started. User(id): {UserId}", userId);

        _ = areaAdminBaseDto ?? throw new ArgumentNullException(nameof(areaAdminBaseDto));

        if (await IsSuchEmailExisted(areaAdminBaseDto.Email))
        {
            Logger.LogDebug("AreaAdmin creating is not possible. Username {Email} is already taken",
                areaAdminBaseDto.Email);
            throw new InvalidOperationException($"Username {areaAdminBaseDto.Email} is already taken.");
        }

        bool isValidCatottg = await IsValidCatottg(areaAdminBaseDto.CATOTTGId);
        if (!isValidCatottg)
        {
            Logger.LogDebug("AreaAdmin creating is not possible. Catottg with Id {CatottgId} does not contain to area",
                areaAdminBaseDto.CATOTTGId);
            throw new InvalidOperationException(
                $"Catottg with Id {areaAdminBaseDto.CATOTTGId} does not contain to area.");
        }

        var request = new Request()
        {
            HttpMethodType = HttpMethodType.Post,
            Url = new Uri(identityServerConfig.Authority, CommunicationConstants.CreateAreaAdmin),
            Token = token,
            Data = areaAdminBaseDto,
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
            var childrenIds = await codeficatorService.GetAllChildrenIdsByParentIdAsync(filter.CATOTTGId);
            catottgs = childrenIds.ToList();
        }

        Expression<Func<AreaAdmin, bool>> filterPredicate = PredicateBuild(filter, catottgs);

        int count = await areaAdminRepository.Count(filterPredicate).ConfigureAwait(false);

        var sortExpression = new Dictionary<Expression<Func<AreaAdmin, object>>, SortDirection>
        {
            { x => x.User.LastName, SortDirection.Ascending },
        };

        var otgAdmins = await areaAdminRepository
            .Get(
                skip: filter.From,
                take: filter.Size,
                includeProperties: "Institution,User,CATOTTG",
                whereExpression: filterPredicate,
                orderBy: sortExpression,
                asNoTracking: true)
            .ToListAsync()
            .ConfigureAwait(false);

        if (otgAdmins.Any())
        {
            Logger.LogInformation(
                $"All {otgAdmins.Count} records were successfully received from the AreaAdmins table");
        }
        else
        {
            Logger.LogInformation("AreaAdmins table is empty.");
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
        AreaAdminDto updateAreaAdminDto,
        string token)
    {
        _ = updateAreaAdminDto ?? throw new ArgumentNullException(nameof(updateAreaAdminDto));

        Logger.LogDebug("AreaAdmin(id): {AreaAdminDto} updating was started. User(id): {UserId}", updateAreaAdminDto.Id,
            userId);

        var regionAdmin = await areaAdminRepository.GetByIdAsync(updateAreaAdminDto.Id)
            .ConfigureAwait(false);

        if (regionAdmin is null)
        {
            Logger.LogError("AreaAdmin(id) {AreaAdminDto} not found. User(id): {UserId}", updateAreaAdminDto.Id,
                userId);

            return new ErrorResponse
            {
                HttpStatusCode = HttpStatusCode.NotFound,
            };
        }

        var request = new Request()
        {
            HttpMethodType = HttpMethodType.Put,
            Url = new Uri(identityServerConfig.Authority,
                CommunicationConstants.UpdateAreaAdmin + updateAreaAdminDto.Id),
            Token = token,
            Data = mapper.Map<AreaAdminBaseDto>(updateAreaAdminDto),
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
                ? mapper.Map<AreaAdminDto>(JsonConvert.DeserializeObject<AreaAdminBaseDto>(result.Result.ToString()))
                : null);
    }

    public async Task<Either<ErrorResponse, ActionResult>> DeleteAreaAdminAsync(string areaAdminId, string userId,
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
            Url = new Uri(identityServerConfig.Authority, CommunicationConstants.DeleteAreaAdmin + areaAdminId),
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

    public async Task<Either<ErrorResponse, ActionResult>> BlockAreaAdminAsync(string areaAdminId, string userId,
        string token, bool isBlocked)
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
            Url = new Uri(identityServerConfig.Authority, string.Concat(
                CommunicationConstants.BlockAreaAdmin,
                areaAdminId,
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
            Url = new Uri(identityServerConfig.Authority, string.Concat(
                CommunicationConstants.ReinviteAreaAdmin,
                areaAdminId,
                new PathString("/"))),
            Token = token,
            RequestId = Guid.NewGuid(),
        };

        Logger.LogDebug(
            "{request.HttpMethodType} Request(id): {request.RequestId} was sent. User(id): {UserId}. Url: {request.Url}",
            request.HttpMethodType,
            request.RequestId,
            userId,
            request.Url);

        var response = await SendRequest<ResponseDto>(request).ConfigureAwait(false);

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
        var areaAdmin = await areaAdminRepository.GetByIdAsync(areaAdminId).ConfigureAwait(false);

        return regionAdmin.InstitutionId == areaAdmin.InstitutionId && regionAdmin.CATOTTGId == areaAdmin.CATOTTGId;
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

        return regionAdmin.InstitutionId == institutionId && regionAdmin.CATOTTGId == catottgId;
    }

    private static Expression<Func<AreaAdmin, bool>> PredicateBuild(AreaAdminFilter filter, List<long> catottgs)
    {
        Expression<Func<AreaAdmin, bool>> predicate = PredicateBuilder.True<AreaAdmin>();

        if (!string.IsNullOrWhiteSpace(filter.SearchString))
        {
            var tempPredicate = PredicateBuilder.False<AreaAdmin>();

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

    private async Task<bool> IsSuchEmailExisted(string email)
    {
        var result = await userRepository.GetByFilter(x => x.Email == email);
        return !result.IsNullOrEmpty();
    }

    private async Task<bool> IsValidCatottg(long catottgId)
    {
        var catottg = await codeficatorRepository.GetById(catottgId);
        return catottg is not null && (catottg.Category == CodeficatorCategory.TerritorialCommunity.ToString() ||
                                       catottg.Category == CodeficatorCategory.SpecialStatusCity.ToString());
    }
}