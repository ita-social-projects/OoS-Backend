using System.Linq.Expressions;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OutOfSchool.Common.Enums;
using OutOfSchool.Common.Models;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.Admins;
using OutOfSchool.WebApi.Services.Communication.ICommunication;

namespace OutOfSchool.WebApi.Services.Admins;

public abstract class BaseAdminService<TEntity, TDto, TFilter>
    where TEntity : InstitutionAdminBase
    where TDto : BaseAdminDto
    where TFilter : BaseAdminFilter
{
    private readonly AuthorizationServerConfig authorizationServerConfig;
    private readonly ICommunicationService communicationService;
    private readonly ILogger<BaseAdminService<TEntity, TDto, TFilter>> logger;
    private readonly IMapper mapper;
    private readonly IUserService userService;

    protected BaseAdminService(
        IOptions<AuthorizationServerConfig> authorizationServerConfig,
        ICommunicationService communicationService,
        ILogger<BaseAdminService<TEntity, TDto, TFilter>> logger,
        IMapper mapper,
        IUserService userService)
    {
        ArgumentNullException.ThrowIfNull(authorizationServerConfig);
        ArgumentNullException.ThrowIfNull(communicationService);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(mapper);
        ArgumentNullException.ThrowIfNull(userService);

        this.authorizationServerConfig = authorizationServerConfig.Value;
        this.communicationService = communicationService;
        this.logger = logger;
        this.mapper = mapper;
        this.userService = userService;
    }

    public async Task<TDto> GetByIdAsync(string id)
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

    public async Task<TDto> GetByUserIdAsync(string userId)
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

    public async Task<SearchResult<TDto>> GetByFilter(TFilter filter)
    {
        logger.LogInformation("Getting admins by filter started.");

        filter ??= CreateEmptyFilter();

        ModelValidationHelper.ValidateOffsetFilter(filter);

        if (!await UserHasRightsToGetAdminsByFilter(filter))
        {
            logger.LogInformation("Current user doesn't have rights to get admins.");

            return new SearchResult<TDto>()
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

        return new SearchResult<TDto>()
        {
            TotalAmount = Count(filterPredicate),
            Entities = (IReadOnlyCollection<TDto>)admins,
        };
    }

    public async Task<Either<ErrorResponse, TDto>> CreateAsync(string userId, TDto adminDto, string token)
    {
        logger.LogDebug("Admin creating by userId {userId} started.", userId);

        ArgumentNullException.ThrowIfNull(adminDto);

        if (await IsUsernameIsTaken(adminDto.Email))
        {
            logger.LogDebug("Admin creating is not possible. Username {Email} is already taken/", adminDto.Email);
            throw new InvalidOperationException($"Username {adminDto.Email} is already taken.");
        }

        if (!await UserHasRightsToCreateAdmin(adminDto))
        {
            return new ErrorResponse
            {
                HttpStatusCode = HttpStatusCode.Forbidden,
            };
        }

        var request = MakeCreateRequest(adminDto, token);

        logger.LogDebug("{HttpMethodType} Request was sent. User(id): {UserId}. Url: {Url}", request.HttpMethodType, userId, request.Url);

        var response = await communicationService.SendRequest<ResponseDto>(request);

        return response
            .FlatMap<ResponseDto>(r => r.IsSuccess
                ? r
                : new ErrorResponse
                {
                    HttpStatusCode = r.HttpStatusCode,
                    Message = r.Message,
                })
            .Map(result => result.Result is not null
                ? mapper.Map(JsonConvert.DeserializeObject<TDto>(result.Result.ToString()), adminDto)
                : null);
    }

    public async Task<Either<ErrorResponse, TDto>> UpdateAsync(string userId, TDto adminDto, string token)
    {
        _ = adminDto ?? throw new ArgumentNullException(nameof(adminDto));

        logger.LogDebug("Admin with id {id} updating by userId {userId} started.", adminDto.UserId, userId);

        var admin = await GetById(adminDto.UserId);

        if (admin is null)
        {
            logger.LogError("Admin {id} not found. User(id): {UserId}", adminDto.UserId, userId);

            return new ErrorResponse
            {
                HttpStatusCode = HttpStatusCode.NotFound,
            };
        }

        if (!await UserHasRightsToUpdateAdmin(adminDto.UserId))
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

        var response = await communicationService.SendRequest<ResponseDto>(request)
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
                ? mapper.Map(JsonConvert.DeserializeObject<TDto>(result.Result.ToString()), adminDto)
                : null);
    }

    public async Task<Either<ErrorResponse, ActionResult>> DeleteAsync(string adminId, string userId, string token)
    {
        logger.LogDebug("Admin with id {id} deleting by userId {userId} started.", adminId, userId);

        var admin = await GetById(adminId);

        if (admin is null)
        {
            logger.LogError("Admin {id} not found. User(id): {UserId}", adminId, userId);

            return new ErrorResponse
            {
                HttpStatusCode = HttpStatusCode.NotFound,
            };
        }

        if (!await UserHasRightsToDeleteAdmin(adminId))
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

        var response = await communicationService.SendRequest<ResponseDto>(request)
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

        var admin = await GetById(adminId);

        if (admin is null)
        {
            logger.LogError("Admin {id} not found. User(id): {UserId}", adminId, userId);

            return new ErrorResponse
            {
                HttpStatusCode = HttpStatusCode.NotFound,
            };
        }

        if (!await UserHasRightsToBlockAdmin(adminId))
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

        var response = await communicationService.SendRequest<ResponseDto>(request)
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

        var admin = await GetById(adminId);

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
            if (await userService.IsNeverLogged(admin.UserId))
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

        var response = await communicationService.SendRequest<ResponseDto>(request)
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

    protected abstract Expression<Func<TEntity, bool>> PredicateBuild(TFilter filter);

    protected abstract Task<TDto> GetById(string id);

    protected abstract Task<TDto> GetByUserId(string userId);

    protected abstract TFilter CreateEmptyFilter();

    protected abstract Task<bool> UserHasRightsToGetAdminsByFilter(TFilter filter);

    protected abstract Task<bool> UserHasRightsToCreateAdmin(TDto adminDto);

    protected abstract Task<bool> UserHasRightsToUpdateAdmin(string adminId);

    protected abstract Task<bool> UserHasRightsToDeleteAdmin(string adminId);

    protected abstract Task<bool> UserHasRightsToBlockAdmin(string adminId);

    protected abstract Task UpdateTheFilterWithTheAdminRestrictions(TFilter filter);

    protected abstract int Count(Expression<Func<TEntity, bool>> filterPredicate);

    protected abstract IEnumerable<TDto> Get(TFilter filter, Expression<Func<TEntity, bool>> filterPredicate);

    protected abstract string GetCommunicationString(RequestCommand command);

    private Request MakeCreateRequest(TDto adminDto, string token) =>
        new()
        {
            HttpMethodType = HttpMethodType.Post,
            Url = new Uri(authorizationServerConfig.Authority, GetCommunicationString(RequestCommand.Create)),
            Token = token,
            Data = adminDto,
        };

    private Request MakeUpdateRequest(TDto adminDto, string token) =>
        new()
        {
            HttpMethodType = HttpMethodType.Put,
            Url = new Uri(authorizationServerConfig.Authority, $"{GetCommunicationString(RequestCommand.Update)}{adminDto.UserId}"),
            Token = token,
            Data = mapper.Map<AdminBaseDto>(adminDto),
        };

    private Request MakeDeleteRequest(string adminId, string token) =>
        new()
        {
            HttpMethodType = HttpMethodType.Delete,
            Url = new Uri(authorizationServerConfig.Authority, $"{GetCommunicationString(RequestCommand.Delete)}{adminId}"),
            Token = token,
        };

    private Request MakeBlockRequest(string adminId, string token, bool isBlocked) =>
        new()
        {
            HttpMethodType = HttpMethodType.Put,
            Url = new Uri(authorizationServerConfig.Authority, $"{GetCommunicationString(RequestCommand.Block)}{adminId}/{isBlocked}"),
            Token = token,
        };

    private Request MakeReinviteRequest(string adminId, string token) =>
        new()
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