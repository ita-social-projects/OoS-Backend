using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using AutoMapper;
using Castle.Core.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using Newtonsoft.Json;
using OutOfSchool.Common.Models;
using OutOfSchool.Services.Enums;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Services;

public class MinistryAdminService : CommunicationService, IMinistryAdminService
{
    private readonly IdentityServerConfig identityServerConfig;
    private readonly IInstitutionAdminRepository institutionAdminRepository;
    private readonly IEntityRepository<string, User> userRepository;
    private readonly IMapper mapper;

    public MinistryAdminService(
        IHttpClientFactory httpClientFactory,
        IOptions<IdentityServerConfig> identityServerConfig,
        IOptions<CommunicationConfig> communicationConfig,
        IInstitutionAdminRepository institutionAdminRepository,
        ILogger<MinistryAdminService> logger,
        IEntityRepository<string, User> userRepository,
        IMapper mapper)
        : base(httpClientFactory, communicationConfig?.Value, logger)
    {
        this.identityServerConfig = (identityServerConfig ?? throw new ArgumentNullException(nameof(identityServerConfig))).Value;
        this.institutionAdminRepository = institutionAdminRepository ?? throw new ArgumentNullException(nameof(institutionAdminRepository));
        this.userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<MinistryAdminDto> GetById(long id)
    {
        Logger.LogInformation($"Getting InstitutionAdmin by Id started. Looking Id = {id}.");

        var institutionAdmin = await institutionAdminRepository.GetById(id).ConfigureAwait(false);

        if (institutionAdmin is null)
        {
            return null;
        }

        Logger.LogInformation($"Successfully got a InstitutionAdmin with Id = {id}.");

        return mapper.Map<MinistryAdminDto>(institutionAdmin);
    }

    public async Task<MinistryAdminDto> GetByUserId(string id)
    {
        Logger.LogInformation($"Getting MinistryAdmin by UserId started. Looking UserId is {id}.");

        InstitutionAdmin ministryAdmin = (await institutionAdminRepository.GetByFilter(p => p.UserId == id).ConfigureAwait(false)).FirstOrDefault();

        if (ministryAdmin == null)
        {
            var errMsg = $"There is no MinistryAdmin in the Db with such User id {id}.";

            Logger.LogError(errMsg);
            throw new ArgumentException(errMsg);
        }

        Logger.LogInformation($"Successfully got a MinistryAdmin with UserId = {id}.");

        return mapper.Map<MinistryAdminDto>(ministryAdmin);
    }

    public async Task<Either<ErrorResponse, CreateMinistryAdminDto>> CreateMinistryAdminAsync(string userId, CreateMinistryAdminDto ministryAdminDto, string token)
    {
        Logger.LogDebug($"ministryAdmin creating was started. User(id): {userId}");

        ArgumentNullException.ThrowIfNull(ministryAdminDto);

        if (await IsSuchEmailExisted(ministryAdminDto.Email))
        {
            logger.LogDebug("ministryAdmin creating is not possible. Username {Email} is already taken.", ministryAdminDto.Email);
            throw new InvalidOperationException($"Username {ministryAdminDto.Email} is already taken.");
        }

        var request = new Request()
        {
            HttpMethodType = HttpMethodType.Post,
            Url = new Uri(identityServerConfig.Authority, CommunicationConstants.CreateMinistryAdmin),
            Token = token,
            Data = ministryAdminDto,
            RequestId = Guid.NewGuid(),
        };

        Logger.LogDebug($"{request.HttpMethodType} Request(id): {request.RequestId} " +
                        $"was sent. User(id): {userId}. Url: {request.Url}");

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
                    .DeserializeObject<CreateMinistryAdminDto>(result.Result.ToString())
                : null);
    }

    /// <inheritdoc/>
    public async Task<SearchResult<MinistryAdminDto>> GetByFilter(MinistryAdminFilter filter)
    {
        Logger.LogInformation("Getting all Ministry admins started (by filter).");

        filter ??= new MinistryAdminFilter();
        ModelValidationHelper.ValidateOffsetFilter(filter);

        var filterPredicate = PredicateBuild(filter);

        int count = await institutionAdminRepository.Count(filterPredicate).ConfigureAwait(false);

        var sortExpression = new Dictionary<Expression<Func<InstitutionAdmin, object>>, SortDirection>
        {
            { x => x.User.LastName, SortDirection.Ascending },
        };
        var institutionAdmins = await institutionAdminRepository
            .Get(
                skip: filter.From,
                take: filter.Size,
                includeProperties: string.Empty,
                where: filterPredicate,
                orderBy: sortExpression,
                asNoTracking: false)
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
    public async Task<MinistryAdminDto> Update(MinistryAdminDto ministryAdminDto)
    {
        ArgumentNullException.ThrowIfNull(ministryAdminDto);

        Logger.LogInformation($"Updating MinistryAdmin with User Id = {ministryAdminDto.Id} started.");

        try
        {
            var user = await userRepository.GetById(ministryAdminDto.Id);

            var updatedUser = await userRepository.Update(mapper.Map((BaseUserDto)ministryAdminDto, user)).ConfigureAwait(false);

            Logger.LogInformation($"User with Id = {updatedUser.Id} updated succesfully.");

            return ministryAdminDto;
        }
        catch (DbUpdateConcurrencyException)
        {
            Logger.LogError($"Updating failed. User with Id = {ministryAdminDto.Id} doesn't exist in the system.");
            throw;
        }
    }

    public async Task<Either<ErrorResponse, ActionResult>> DeleteMinistryAdminAsync(string ministryAdminId, string userId, string token)
    {
        Logger.LogDebug($"MinistryAdmin(id): {ministryAdminId} deleting was started. User(id): {userId}");

        var ministryAdmin = await institutionAdminRepository.GetByIdAsync(ministryAdminId)
            .ConfigureAwait(false);

        if (ministryAdmin is null)
        {
            Logger.LogError($"MinistryAdmin(id) {ministryAdminId} not found. User(id): {userId}.");

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

        Logger.LogDebug($"{request.HttpMethodType} Request(id): {request.RequestId} " +
                        $"was sent. User(id): {userId}. Url: {request.Url}");

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

    public async Task<Either<ErrorResponse, ActionResult>> BlockMinistryAdminAsync(string ministryAdminId, string userId, string token)
    {
        Logger.LogDebug($"MinistryAdmin(id): {ministryAdminId} blocking was started. User(id): {userId}");

        var ministryAdmin = await institutionAdminRepository.GetByIdAsync(ministryAdminId)
            .ConfigureAwait(false);

        if (ministryAdmin is null)
        {
            Logger.LogError($"MinistryAdmin(id) {ministryAdminId} not found. User(id): {userId}.");

            return new ErrorResponse
            {
                HttpStatusCode = HttpStatusCode.NotFound,
            };
        }

        var request = new Request()
        {
            HttpMethodType = HttpMethodType.Put,
            Url = new Uri(identityServerConfig.Authority, CommunicationConstants.BlockMinistryAdmin + ministryAdminId),
            Token = token,
            RequestId = Guid.NewGuid(),
        };

        Logger.LogDebug($"{request.HttpMethodType} Request(id): {request.RequestId} " +
                        $"was sent. User(id): {userId}. Url: {request.Url}");

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