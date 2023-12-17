using System.Linq.Expressions;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OutOfSchool.Common.Enums;
using OutOfSchool.Common.Models;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.Admins;

namespace OutOfSchool.WebApi.Services.Admins;

public abstract class BaseAdminService : CommunicationService
{
    private readonly AuthorizationServerConfig authorizationServerConfig;
    private readonly ILogger<BaseAdminService> logger;
    private readonly IMapper mapper;
    private readonly IUserService userService;

    public BaseAdminService(
        IHttpClientFactory httpClientFactory,
        IOptions<AuthorizationServerConfig> authorizationServerConfig,
        IOptions<CommunicationConfig> communicationConfig,
        ILogger<BaseAdminService> logger,
        IMapper mapper,
        IUserService userService)
        : base(httpClientFactory, communicationConfig, logger)
    {
        ArgumentNullException.ThrowIfNull(authorizationServerConfig);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(mapper);
        ArgumentNullException.ThrowIfNull(userService);

        this.authorizationServerConfig = authorizationServerConfig.Value;
        this.logger = logger;
        this.mapper = mapper;
        this.userService = userService;
    }

    public async Task<BaseAdminDto> GetByIdAsync(string id)
    {
        logger.LogInformation("Getting admin by id {id} started.", id);

        var admin = await GetById(id);

        if (admin is null)
        {
            logger.LogError("There is no admin in the Db with id {id}", id);
        }
        else
        {
            logger.LogInformation("Successfully got an admin with id {id}", id);
        }

        return admin;
    }

    public async Task<BaseAdminDto> GetByUserIdAsync(string userId)
    {
        logger.LogInformation("Getting admin by userId {id} started.", userId);

        var admin = await GetByUserId(userId);

        if (admin is null)
        {
            logger.LogError("There is no admin in the Db with userId {userId}", userId);

            throw new ArgumentException($"There is no admin in the Db with userId {userId}");
        }

        logger.LogInformation("Successfully got an admin with userId {userId}", userId);

        return admin;
    }

    public async Task<SearchResult<BaseAdminDto>> GetByFilter(BaseAdminFilter filter)
    {
        logger.LogInformation("Getting admins by filter started.");

        filter ??= CreateEmptyFilter();

        ModelValidationHelper.ValidateOffsetFilter(filter);

        if (!await IsUserHasRightsToGetAdminsByFilter(filter))
        {
            logger.LogInformation("Current user doesn't have rights to get admins.");

            return new SearchResult<BaseAdminDto>()
            {
                TotalAmount = 0,
                Entities = default,
            };
        }

        await UpdateTheFilterWithTheAdminRestrictions(filter);

        var filterPredicate = PredicateBuild(filter);

        var admins = Get(filter, filterPredicate);

        if (admins.Any())
        {
            logger.LogInformation("All records were successfully received from the Db.");
        }
        else
        {
            logger.LogInformation("There aren't any admins in the Db.");
        }

        return new SearchResult<BaseAdminDto>()
        {
            TotalAmount = Count(filterPredicate),
            Entities = (IReadOnlyCollection<BaseAdminDto>)admins,
        };
    }

    public async Task<Either<ErrorResponse, BaseAdminDto>> CreateAsync(string userId, BaseAdminDto adminDto, string token)
    {
        logger.LogDebug("Admin creating by userId {userId} started.", userId);

        ArgumentNullException.ThrowIfNull(adminDto);

        if (await IsUsernameIsTaken(adminDto.Email))
        {
            logger.LogDebug("Admin creating is not possible. Username {Email} is already taken/", adminDto.Email);
            throw new InvalidOperationException($"Username {adminDto.Email} is already taken.");
        }

        if (!await IsUserHasRightsToCreateAdmin(adminDto))
        {
            return new ErrorResponse
            {
                HttpStatusCode = HttpStatusCode.Forbidden,
            };
        }

        var request = MakeCreateRequest(adminDto, token);

        logger.LogDebug("{HttpMethodType} Request was sent. User(id): {UserId}. Url: {Url}", request.HttpMethodType, userId, request.Url);

        var response = await SendRequest<ResponseDto>(request);
        var t = response.Map(x => x.Result).Match(l => l.HttpStatusCode, r => r);

        return response
            .FlatMap<ResponseDto>(r => r.IsSuccess
                ? r
                : new ErrorResponse
                {
                    HttpStatusCode = r.HttpStatusCode,
                    Message = r.Message,
                })
            .Map(result => result.Result is not null
                ? mapper.Map(JsonConvert.DeserializeObject<AdminBaseDto>(result.Result.ToString()), adminDto)
                : null); // JsonConvert.DeserializeObject<BaseAdminDto>(result.Result.ToString())
    }

    public async Task<Either<ErrorResponse, BaseAdminDto>> UpdateAsync(string userId, BaseAdminDto adminDto, string token)
    {
        _ = adminDto ?? throw new ArgumentNullException(nameof(adminDto));

        logger.LogDebug("Admin with id {id} updating by userId {userId} started.", adminDto.Id, userId);

        // TODO Add checking if ministry Admin belongs to Institution and is exist MinistryAdmin with such UserId
        BaseAdminDto admin = await GetById(adminDto.Id);

        if (admin is null)
        {
            logger.LogError("Admin {id} not found. User(id): {UserId}", adminDto.Id, userId);

            return new ErrorResponse
            {
                HttpStatusCode = HttpStatusCode.NotFound,
            };
        }

        if (!await IsUserHasRightsToUpdateAdmin(adminDto.Id))
        {
            return new ErrorResponse
            {
                HttpStatusCode = HttpStatusCode.Forbidden,
            };
        }

        var request = MakeUpdateRequest(adminDto, token);

        logger.LogDebug(
            "{HttpMethodType} Request was sent. User(id): {UserId}. Url: {Url}",
            request.HttpMethodType,
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
                ? mapper.Map(JsonConvert.DeserializeObject<AdminBaseDto>(result.Result.ToString()), adminDto)
                : null); // JsonConvert.DeserializeObject<BaseAdminDto>(result.Result.ToString())
    }

    public async Task<Either<ErrorResponse, ActionResult>> DeleteAsync(string adminId, string userId, string token)
    {
        logger.LogDebug("Admin with id {id} deleting by userId {userId} started.", adminId, userId);

        BaseAdminDto admin = await GetById(adminId);

        if (admin is null)
        {
            logger.LogError("Admin {id} not found. User(id): {UserId}", adminId, userId);

            return new ErrorResponse
            {
                HttpStatusCode = HttpStatusCode.NotFound,
            };
        }

        if (!await IsUserHasRightsToDeleteAdmin(adminId))
        {
            return new ErrorResponse
            {
                HttpStatusCode = HttpStatusCode.Forbidden,
            };
        }

        var request = MakeDeleteRequest(adminId, token);

        logger.LogDebug(
            "{HttpMethodType} Request was sent. User(id): {UserId}. Url: {Url}",
            request.HttpMethodType,
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
                ? JsonConvert.DeserializeObject<ActionResult>(result.Result.ToString())
                : null);
    }

    public async Task<Either<ErrorResponse, ActionResult>> BlockAsync(string adminId, string userId, string token, bool isBlocked)
    {
        logger.LogDebug("Admin with id {id} blocking by userId {userId} started.", adminId, userId);

        BaseAdminDto admin = await GetById(adminId);

        if (admin is null)
        {
            logger.LogError("Admin {id} not found. User(id): {UserId}", adminId, userId);

            return new ErrorResponse
            {
                HttpStatusCode = HttpStatusCode.NotFound,
            };
        }

        if (!await IsUserHasRightsToBlockAdmin(adminId))
        {
            return new ErrorResponse
            {
                HttpStatusCode = HttpStatusCode.Forbidden,
            };
        }

        var request = MakeBlockRequest(adminId, token, isBlocked);

        logger.LogDebug(
            "{HttpMethodType} Request was sent. User(id): {UserId}. Url: {Url}",
            request.HttpMethodType,
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
                ? JsonConvert.DeserializeObject<ActionResult>(result.Result.ToString())
                : null);
    }

    public async Task<Either<ErrorResponse, ActionResult>> ReinviteAsync(string adminId, string userId, string token)
    {
        logger.LogDebug("Admin with id {id} reinviting by userId {userId} was started.", adminId, userId);

        BaseAdminDto admin = await GetById(adminId);

        if (admin is null)
        {
            return new ErrorResponse
            {
                HttpStatusCode = HttpStatusCode.NoContent,
                Message = "There is no admin in the Db with such an id.",
            };
        }

        try
        {
            if (await userService.IsNeverLogged(admin.Id))
            {
                return new ErrorResponse
                {
                    HttpStatusCode = HttpStatusCode.BadRequest,
                    Message = "Only neverlogged users can be reinvited.",
                };
            }
        }
        catch (ArgumentException ex)
        {
            return new ErrorResponse
            {
                HttpStatusCode = HttpStatusCode.NoContent,
                Message = ex.Message,
            };
        }

        var request = MakeReinviteRequest(adminId, token);

        logger.LogDebug(
            "{HttpMethodType} Request was sent. User(id): {UserId}. Url: {Url}",
            request.HttpMethodType,
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
                ? JsonConvert.DeserializeObject<ActionResult>(result.Result.ToString())
                : null);
    }

    protected virtual Expression<Func<InstitutionAdminBase, bool>> PredicateBuild(BaseAdminFilter filter)
    {
        var predicate = PredicateBuilder.True<InstitutionAdminBase>();

        if (!string.IsNullOrWhiteSpace(filter.SearchString))
        {
            var tempPredicate = PredicateBuilder.False<InstitutionAdminBase>();

            foreach (var word in filter.SearchString.Split(' ', ',', StringSplitOptions.RemoveEmptyEntries))
            {
                tempPredicate = tempPredicate.Or(
                    x => x.User.FirstName.Contains(word, StringComparison.InvariantCultureIgnoreCase)
                         || x.User.LastName.Contains(word, StringComparison.InvariantCultureIgnoreCase)
                         || x.User.Email.Contains(word, StringComparison.InvariantCultureIgnoreCase)
                         || x.User.PhoneNumber.Contains(word, StringComparison.InvariantCultureIgnoreCase));
            }

            predicate = predicate.And(tempPredicate);
        }

        return predicate;
    }

    protected abstract Task<BaseAdminDto> GetById(string id);

    protected abstract Task<BaseAdminDto> GetByUserId(string userId);

    protected abstract BaseAdminFilter CreateEmptyFilter();

    protected abstract Task<bool> IsUserHasRightsToGetAdminsByFilter(BaseAdminFilter filter);

    protected abstract Task<bool> IsUserHasRightsToCreateAdmin(BaseAdminDto adminDto);

    protected abstract Task<bool> IsUserHasRightsToUpdateAdmin(string adminId);

    protected abstract Task<bool> IsUserHasRightsToDeleteAdmin(string adminId);

    protected abstract Task<bool> IsUserHasRightsToBlockAdmin(string adminId);

    protected abstract Task UpdateTheFilterWithTheAdminRestrictions(BaseAdminFilter filter);

    protected abstract int Count(Expression<Func<InstitutionAdminBase, bool>> filterPredicate);

    protected abstract IEnumerable<BaseAdminDto> Get(BaseAdminFilter filter, Expression<Func<InstitutionAdminBase, bool>> filterPredicate);

    protected abstract string GetCommunicationString(RequestCommand command);

    private Request MakeCreateRequest(BaseAdminDto adminDto, string token) =>
        new Request()
        {
            HttpMethodType = HttpMethodType.Post,
            Url = new Uri(authorizationServerConfig.Authority, GetCommunicationString(RequestCommand.Create)),
            Token = token,
            Data = adminDto,
        };

    private Request MakeUpdateRequest(BaseAdminDto adminDto, string token) =>
        new Request()
        {
            HttpMethodType = HttpMethodType.Put,
            Url = new Uri(authorizationServerConfig.Authority, $"{GetCommunicationString(RequestCommand.Update)}{adminDto.Id}"),
            Token = token,
            Data = mapper.Map<AdminBaseDto>(adminDto),
        };

    private Request MakeDeleteRequest(string adminId, string token) =>
        new Request()
        {
            HttpMethodType = HttpMethodType.Delete,
            Url = new Uri(authorizationServerConfig.Authority, $"{GetCommunicationString(RequestCommand.Delete)}{adminId}"),
            Token = token,
        };

    private Request MakeBlockRequest(string adminId, string token, bool isBlocked) =>
        new Request()
        {
            HttpMethodType = HttpMethodType.Put,
            Url = new Uri(authorizationServerConfig.Authority, $"{GetCommunicationString(RequestCommand.Block)}{adminId}/{isBlocked}"),
            Token = token,
        };

    private Request MakeReinviteRequest(string adminId, string token) =>
        new Request()
        {
            HttpMethodType = HttpMethodType.Put,
            Url = new Uri(authorizationServerConfig.Authority, $"{GetCommunicationString(RequestCommand.Reinvite)}{adminId}"),
            Token = token,
        };

    private async Task<bool> IsUsernameIsTaken(string email)
    {
        try
        {
            return (await userService.GetByFilter(x => x.Email == email)).Any();
        }
        catch (ArgumentException)
        {
            return false;
        }
    }
}