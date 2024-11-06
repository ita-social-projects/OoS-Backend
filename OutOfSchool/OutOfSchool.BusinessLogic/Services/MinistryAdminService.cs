using System.Linq.Expressions;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OutOfSchool.Common.Models;
using OutOfSchool.Services.Enums;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.Common.Communication;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Services.Repository.Base.Api;
using OutOfSchool.BusinessLogic.Services.SearchString;


namespace OutOfSchool.BusinessLogic.Services;

public class MinistryAdminService : CommunicationService, IMinistryAdminService, ISensitiveMinistryAdminService
{
    private readonly AuthorizationServerConfig authorizationServerConfig;
    private readonly IInstitutionAdminRepository institutionAdminRepository;
    private readonly IEntityRepositorySoftDeleted<string, User> userRepository;
    private readonly IMapper mapper;
    private readonly ICurrentUserService currentUserService;
    private readonly IApiErrorService apiErrorService;
    private readonly ISearchStringService searchStringService;

    public MinistryAdminService(
        IHttpClientFactory httpClientFactory,
        IOptions<AuthorizationServerConfig> authorizationServerConfig,
        IOptions<CommunicationConfig> communicationConfig,
        IInstitutionAdminRepository institutionAdminRepository,
        ILogger<MinistryAdminService> logger,
        IEntityRepositorySoftDeleted<string, User> userRepository,
        IMapper mapper,
        ICurrentUserService currentUserService,
        IApiErrorService apiErrorService,
        ISearchStringService searchStringService)
        : base(httpClientFactory, communicationConfig, logger)
    {
        this.authorizationServerConfig = (authorizationServerConfig ?? throw new ArgumentNullException(nameof(authorizationServerConfig))).Value;
        this.institutionAdminRepository = institutionAdminRepository ?? throw new ArgumentNullException(nameof(institutionAdminRepository));
        this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        this.currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        this.apiErrorService = apiErrorService;
        this.searchStringService = searchStringService ?? throw new ArgumentNullException(nameof(searchStringService));
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

        var badRequestApiErrorResponse = await apiErrorService.AdminsCreatingIsBadRequestDataAttend(
            ministryAdminBaseDto,
            "MinistryAdmin");

        if (badRequestApiErrorResponse.ApiErrors.Count != 0)
        {
            return ErrorResponse.BadRequest(badRequestApiErrorResponse);
        }

        var request = new Request()
        {
            HttpMethodType = HttpMethodType.Post,
            Url = new Uri(authorizationServerConfig.Authority, CommunicationConstants.CreateMinistryAdmin),
            Token = token,
            Data = ministryAdminBaseDto,
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
                Logger.LogInformation(
                    "Filter institutionId {FilterInstitutionId} is not equals to logined Ministry admin institutionId {MinistryAdminInstitutionId}",
                    filter.InstitutionId,
                    ministryAdmin.InstitutionId);

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
            { x => x.User.IsBlocked, SortDirection.Ascending },
            { x => x.User.LastLogin == DateTimeOffset.MinValue, SortDirection.Descending },
            { x => x.User.LastName, SortDirection.Ascending },
        };

        var institutionAdmins = await institutionAdminRepository
            .Get(
                skip: filter.From,
                take: filter.Size,
                includeProperties: "Institution,User",
                whereExpression: filterPredicate,
                orderBy: sortExpression,
                asNoTracking: true)
            .ToListAsync()
            .ConfigureAwait(false);

        Logger.LogInformation(
            "All {Count} records were successfully received from the Parent table",
            institutionAdmins.Count);

        var ministryAdminsDto = institutionAdmins.Select(admin => mapper.Map<MinistryAdminDto>(admin)).ToList();

        var result = new SearchResult<MinistryAdminDto>()
        {
            TotalAmount = count,
            Entities = ministryAdminsDto,
        };

        return result;
    }

    /// <inheritdoc/>
    public async Task<Either<ErrorResponse, MinistryAdminDto>> UpdateMinistryAdminAsync(
        string userId,
        BaseUserDto updateMinistryAdminDto,
        string token)
    {
        _ = updateMinistryAdminDto ?? throw new ArgumentNullException(nameof(updateMinistryAdminDto));

        Logger.LogDebug("MinistryAdmin(id): {MinistryAdminId} updating was started. User(id): {UserId}", updateMinistryAdminDto.Id, userId);

        var ministryAdmin = await institutionAdminRepository.GetByIdAsync(updateMinistryAdminDto.Id)
            .ConfigureAwait(false);

        if (ministryAdmin is null)
        {
            Logger.LogError("MinistryAdmin(id) {MinistryAdminId} not found. User(id): {UserId}", updateMinistryAdminDto.Id, userId);

            return new ErrorResponse
            {
                HttpStatusCode = HttpStatusCode.NotFound,
            };
        }

        var request = new Request()
        {
            HttpMethodType = HttpMethodType.Put,
            Url = new Uri(authorizationServerConfig.Authority, CommunicationConstants.UpdateMinistryAdmin + updateMinistryAdminDto.Id),
            Token = token,
            Data = mapper.Map<MinistryAdminBaseDto>(updateMinistryAdminDto),
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
                ? mapper.Map<MinistryAdminDto>(JsonConvert.DeserializeObject<MinistryAdminBaseDto>(result.Result.ToString()))
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
            Url = new Uri(authorizationServerConfig.Authority, CommunicationConstants.DeleteMinistryAdmin + ministryAdminId),
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
            Url = new Uri(authorizationServerConfig.Authority, string.Concat(
                CommunicationConstants.BlockMinistryAdmin,
                ministryAdminId,
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
    public async Task<Either<ErrorResponse, ActionResult>> ReinviteMinistryAdminAsync(
        string ministryAdminId,
        string userId,
        string token)
    {
        Logger.LogDebug(
            "MinistryAdmin(id): {MinistryAdminId} reinvite was started. User(id): {UserId}",
            ministryAdminId,
            userId);

        var ministryAdmin = await institutionAdminRepository.GetByIdAsync(ministryAdminId).ConfigureAwait(false);
        if (ministryAdmin == null)
        {
            return null;
        }

        var user = (await userRepository.GetByFilter(u => u.Id == ministryAdmin.UserId).ConfigureAwait(false))
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
                CommunicationConstants.ReinviteMinistryAdmin,
                ministryAdminId,
                new PathString("/"))),
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
    public async Task<bool> IsProviderSubordinateAsync(string ministryAdminUserId, Guid providerId)
    {
        ArgumentNullException.ThrowIfNull(ministryAdminUserId);

        return await institutionAdminRepository
            .Any(x => x.UserId == ministryAdminUserId
                      && x.Institution.RelatedProviders.Any(rp => rp.Id == providerId)).ConfigureAwait(false);
    }

    private Expression<Func<InstitutionAdmin, bool>> PredicateBuild(MinistryAdminFilter filter)
    {
        var predicate = PredicateBuilder.True<InstitutionAdmin>();

        if (!string.IsNullOrWhiteSpace(filter.SearchString))
        {
            var tempPredicate = PredicateBuilder.False<InstitutionAdmin>();

            foreach (var word in searchStringService.SplitSearchString(filter.SearchString))
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

        predicate = predicate.And(p => !p.Institution.IsDeleted);

        return predicate;
    }
}