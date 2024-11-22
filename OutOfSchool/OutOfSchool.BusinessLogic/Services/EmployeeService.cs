using System.Linq.Expressions;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Workshops;
using OutOfSchool.Common.Communication;
using OutOfSchool.Common.Enums;
using OutOfSchool.Common.Models;
using OutOfSchool.Common.Responses;
using OutOfSchool.BusinessLogic.Services.SearchString;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Services.Repository.Base.Api;

namespace OutOfSchool.BusinessLogic.Services;

public class EmployeeService : CommunicationService, IEmployeeService
{
    private readonly string includingPropertiesForMaping = $"{nameof(Employee.ManagedWorkshops)}";

    private readonly AuthorizationServerConfig authorizationServerConfig;
    private readonly EmployeeConfig employeeConfig;
    private readonly IEntityRepositorySoftDeleted<string, User> userRepository;
    private readonly IEmployeeRepository employeeRepository;
    private readonly IMapper mapper;
    private readonly IEmployeeOperationsService employeeOperationsService;
    private readonly IWorkshopService workshopService;
    private readonly ICurrentUserService currentUserService;
    private readonly IApiErrorService apiErrorService;
    private readonly ISearchStringService searchStringService;

    public EmployeeService(
        IHttpClientFactory httpClientFactory,
        IOptions<AuthorizationServerConfig> authorizationServerConfig,
        IOptions<EmployeeConfig> employeeConfig,
        IOptions<CommunicationConfig> communicationConfig,
        IEmployeeRepository employeeRepository,
        IEntityRepositorySoftDeleted<string, User> userRepository,
        IMapper mapper,
        ILogger<EmployeeService> logger,
        IEmployeeOperationsService employeeOperationsService,
        IWorkshopService workshopService,
        ICurrentUserService currentUserService,
        IApiErrorService apiErrorService,
        ISearchStringService searchStringService)
        : base(httpClientFactory, communicationConfig, logger)
    {
        this.authorizationServerConfig = authorizationServerConfig.Value;
        this.employeeConfig = employeeConfig.Value;
        this.employeeRepository = employeeRepository;
        this.userRepository = userRepository;
        this.mapper = mapper;
        this.employeeOperationsService = employeeOperationsService;
        this.workshopService = workshopService;
        this.currentUserService = currentUserService;
        this.apiErrorService = apiErrorService;
        this.searchStringService = searchStringService;
    }

    public async Task<Either<ErrorResponse, CreateEmployeeDto>> CreateEmployeeAsync(
        string userId,
        CreateEmployeeDto employeeDto,
        string token)
    {
        Logger.LogDebug("Employee creating was started. User(id): {UserId}", userId);

        var hasAccess = await IsAllowedCreateAsync(employeeDto.ProviderId, userId)
            .ConfigureAwait(true);

        if (!hasAccess)
        {
            Logger.LogError("User(id): {UserId} doesn't have permission to create employee", userId);
            return new ErrorResponse
            {
                HttpStatusCode = HttpStatusCode.Forbidden,
                ApiErrorResponse = ApiErrorsTypes.Employee.UserDontHavePermissionToCreate(userId).ToResponse(),
            };
        }

        var badRequestApiErrorResponse = await apiErrorService.AdminsCreatingIsBadRequestDataAttend(
            employeeDto,
            $"{nameof(Employee)}");

        if (badRequestApiErrorResponse.ApiErrors.Count != 0)
        {
            return ErrorResponse.BadRequest(badRequestApiErrorResponse);
        }

        var numberEmployeesLessThanMax = await employeeRepository
            .GetNumberEmployeesAsync(employeeDto.ProviderId)
            .ConfigureAwait(false);

        if (numberEmployeesLessThanMax >= employeeConfig.MaxNumberEmployees)
        {
            Logger.LogError(
                "Employee was not created by User(id): {UserId}. Limit on the number of employees has been exceeded for the Provider(id): {ProviderId}",
                userId,
                employeeDto.ProviderId);

            return new ErrorResponse
            {
                HttpStatusCode = HttpStatusCode.MethodNotAllowed,
            };
        }

        return await employeeOperationsService
            .CreateEmployeeAsync(userId, employeeDto, token)
            .ConfigureAwait(false);
    }

    public async Task<Either<ErrorResponse, UpdateEmployeeDto>> UpdateEmployeeAsync(
        UpdateEmployeeDto employeeModel,
        string userId,
        Guid providerId,
        string token)
    {
        _ = employeeModel ?? throw new ArgumentNullException(nameof(employeeModel));

        Logger.LogDebug("Employee(id): {employeeModel_Id} updating was started. User(id): {UserId}", employeeModel.Id, userId);

        var hasAccess = await IsAllowedAsync(providerId, userId)
            .ConfigureAwait(true);

        if (!hasAccess)
        {
            Logger.LogError("User(id): {UserId} doesn't have permission to update employee", userId);

            return new ErrorResponse
            {
                HttpStatusCode = HttpStatusCode.Forbidden,
            };
        }

        var employee = await employeeRepository.GetByIdAsync(employeeModel.Id, providerId)
            .ConfigureAwait(false);

        if (employee is null)
        {
            Logger.LogError("Employee(id) {employeeModel_Id} not found. User(id): {UserId}", employeeModel.Id, userId);

            return new ErrorResponse
            {
                HttpStatusCode = HttpStatusCode.NotFound,
            };
        }

        var request = new Request()
        {
            HttpMethodType = HttpMethodType.Put,
            Url = new Uri(authorizationServerConfig.Authority, CommunicationConstants.UpdateEmployee + employeeModel.Id),
            Token = token,
            Data = employeeModel,
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
                ? JsonSerializerHelper.Deserialize<UpdateEmployeeDto>(result.Result.ToString())
                : null);
    }

    public async Task<Either<ErrorResponse, ActionResult>> DeleteEmployeeAsync(
        string employeeId,
        string userId,
        Guid providerId,
        string token)
    {
        Logger.LogDebug("Employee(id): {employeeId} deleting was started. User(id): {UserId}", employeeId, userId);

        var hasAccess = await IsAllowedAsync(providerId, userId)
            .ConfigureAwait(true);

        if (!hasAccess)
        {
            Logger.LogError("User(id): {UserId} doesn't have permission to delete employee", userId);

            return new ErrorResponse
            {
                HttpStatusCode = HttpStatusCode.Forbidden,
            };
        }

        var employee = await employeeRepository.GetByIdAsync(employeeId, providerId)
            .ConfigureAwait(false);

        if (employee is null)
        {
            Logger.LogError("Employee(id) {employeeId} not found. User(id): {UserId}", employeeId, userId);

            return new ErrorResponse
            {
                HttpStatusCode = HttpStatusCode.NotFound,
            };
        }

        var request = new Request()
        {
            HttpMethodType = HttpMethodType.Delete,
            Url = new Uri(authorizationServerConfig.Authority, CommunicationConstants.DeleteEmployee + employeeId),
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
                ? JsonSerializerHelper.Deserialize<ActionResult>(result.Result.ToString())
                : null);
    }

    public async Task<Either<ErrorResponse, ActionResult>> BlockEmployeeAsync(
        string employeeId,
        string userId,
        Guid providerId,
        string token,
        bool isBlocked)
    {
        Logger.LogDebug("Employee(id): {employeeId} blocking was started. User(id): {UserId}", employeeId, userId);

        var hasAccess = await IsAllowedAsync(providerId, userId)
            .ConfigureAwait(true);

        if (!hasAccess)
        {
            Logger.LogError("User(id): {UserId} doesn't have permission to block employee", userId);

            return new ErrorResponse
            {
                HttpStatusCode = HttpStatusCode.Forbidden,
            };
        }

        var employee = await employeeRepository.GetByIdAsync(employeeId, providerId)
            .ConfigureAwait(false);

        if (employee is null)
        {
            Logger.LogError("Employee(id) {employeeId} not found. User(id): {UserId}", employeeId, userId);

            return new ErrorResponse
            {
                HttpStatusCode = HttpStatusCode.NotFound,
            };
        }

        employee.BlockingType = isBlocked ? BlockingType.Manually : BlockingType.None;
        _ = await employeeRepository.Update(employee).ConfigureAwait(false);

        var request = new Request()
        {
            HttpMethodType = HttpMethodType.Put,
            Url = new Uri(authorizationServerConfig.Authority, string.Concat(
                CommunicationConstants.BlockEmployee,
                employeeId,
                new PathString("/"),
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
                ? JsonSerializerHelper.Deserialize<ActionResult>(result.Result.ToString())
                : null);
    }

    public async Task<Either<ErrorResponse, ActionResult>> BlockEmployeeByProviderAsync(
        Guid providerId,
        string userId,
        string token,
        bool isBlocked)
    {
        var employees = await employeeRepository.GetByFilter(x => x.ProviderId == providerId).ConfigureAwait(false);

        var blockingType = isBlocked ? BlockingType.Automatically : BlockingType.None;

        foreach (var employee in employees)
        {
            if (employee.BlockingType != BlockingType.Manually)
            {
                employee.BlockingType = blockingType;
                _ = await employeeRepository.Update(employee).ConfigureAwait(false);
            }
        }

        var request = new Request()
        {
            HttpMethodType = HttpMethodType.Put,
            Url = new Uri(authorizationServerConfig.Authority, string.Concat(
                CommunicationConstants.BlockEmployeeByProvider,
                providerId,
                new PathString("/"),
                isBlocked)),
            Token = token,
        };

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
                ? JsonSerializerHelper.Deserialize<ActionResult>(result.Result.ToString())
                : null);
    }

    public async Task<bool> IsAllowedCreateAsync(Guid providerId, string userId)
    {
        bool provider = await employeeRepository.IsExistProviderWithUserIdAsync(userId)
            .ConfigureAwait(false);

        return provider;
    }

    public async Task<bool> IsAllowedAsync(Guid providerId, string userId)
    {
        bool provider = await employeeRepository.IsExistProviderWithUserIdAsync(userId)
            .ConfigureAwait(false);

        return provider;
    }

    public async Task GiveEmployeeAccessToWorkshop(string userId, Guid workshopId)
    {
        await employeeRepository.AddRelatedWorkshopForEmployee(userId, workshopId).ConfigureAwait(false);
        Logger.LogDebug("Employee(id): {UserId} now is related to workshop(id): {WorkshopId}", userId, workshopId);
    }

    public async Task<IEnumerable<Guid>> GetRelatedWorkshopIdsForEmployees(string userId)
    {
        var employees = await employeeRepository.GetByFilter(p => p.UserId == userId)
            .ConfigureAwait(false);
        return employees.SelectMany(admin => admin.ManagedWorkshops, (_, workshops) => new { workshops })
            .Select(x => x.workshops.Id);
    }

    /// <inheritdoc/>
    public async Task<SearchResult<WorkshopProviderViewCard>> GetWorkshopsThatEmployeeCanManage(string userId)
    {
        var employees = (await employeeRepository
            .GetByFilter(p => p.UserId == userId, includingPropertiesForMaping))
            .ToList();

        if (!employees.Any())
        {
            return new SearchResult<WorkshopProviderViewCard>()
            {
                Entities = new List<WorkshopProviderViewCard>(),
                TotalAmount = 0,
            };
        }

        var employee = employees.SingleOrDefault();
        if (employee is null)
        {
            return new SearchResult<WorkshopProviderViewCard>()
            {
                Entities = new List<WorkshopProviderViewCard>(),
                TotalAmount = 0,
            };
        }

        var filter = new ExcludeIdFilter() { From = 0, Size = int.MaxValue };
        return await workshopService.GetByProviderId(employee.ProviderId, filter).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<EmployeeProviderRelationDto> GetById(string userId)
    {
        var employee = (await employeeRepository.GetByFilter(p => p.UserId == userId).ConfigureAwait(false))
            .SingleOrDefault();

        if (employee is null)
        {
            return null;
        }

        return mapper.Map<EmployeeProviderRelationDto>(employee);
    }

    public async Task<bool> CheckUserIsRelatedEmployee(string userId, Guid providerId, Guid workshopId = default)
    {
        var employee = await employeeRepository.GetByIdAsync(userId, providerId).ConfigureAwait(false);

        if (workshopId != Guid.Empty)
        {
            return employee.ManagedWorkshops.Any(w => w.Id == workshopId);
        }

        return employee != null;
    }
    
    public async Task<SearchResult<EmployeeDto>> GetFilteredRelatedProviderAdmins(string userId, EmployeeSearchFilter filter)
    {
        filter ??= new EmployeeSearchFilter();
        ModelValidationHelper.ValidateOffsetFilter(filter);

        var relatedAdmins = await this.GetRelatedEmployees(userId).ConfigureAwait(false);

        int totalAmount;

        if (string.IsNullOrEmpty(filter.SearchString))
        {
            totalAmount = relatedAdmins.Count();
        }
        else
        {
            var filterPredicate = PredicateBuild(filter).Compile();
            totalAmount = relatedAdmins.Count(filterPredicate);
            relatedAdmins = relatedAdmins.Where(filterPredicate);
        }

        relatedAdmins = relatedAdmins.OrderBy(x => x.AccountStatus).ThenBy(x => x.LastName).ThenBy(x => x.FirstName).ThenBy(x => x.MiddleName);

        var providerAdmins = relatedAdmins.Skip(filter.From).Take(filter.Size).ToList();

        var searchResult = new SearchResult<EmployeeDto>()
        {
            TotalAmount = totalAmount,
            Entities = providerAdmins,
        };

        return searchResult;
    }

    public async Task<IEnumerable<EmployeeDto>> GetRelatedEmployees(string userId)
    {
        var provider = await employeeRepository.GetProviderWithUserIdAsync(userId).ConfigureAwait(false);
        List<Employee> employees = new List<Employee>();
        List<EmployeeDto> dtos = new List<EmployeeDto>();

        if (provider is not null)
        {
            employees = (await employeeRepository.GetByFilter(pa => pa.ProviderId == provider.Id)
                    .ConfigureAwait(false))
                .ToList();
        }
        else
        {
            var employee =
                (await employeeRepository.GetByFilter(p => p.UserId == userId).ConfigureAwait(false))
                .SingleOrDefault();
            employees = (await employeeRepository
                .GetByFilter(pa => pa.ProviderId == employee.ProviderId)
                .ConfigureAwait(false)).ToList();
        }

        if (employees.Any())
        {
            foreach (var pa in employees)
            {
                var user = (await userRepository.GetByFilter(u => u.Id == pa.UserId).ConfigureAwait(false)).Single();
                var dto = mapper.Map<EmployeeDto>(user);
                dto.AccountStatus = AccountStatusExtensions.Convert(user);

                dtos.Add(dto);
            }
        }

        return dtos;
    }

    public async Task<IEnumerable<string>> GetEmployeesIds(Guid workshopId)
    {
        var employees = await employeeRepository.GetByFilter(p =>
            p.ManagedWorkshops.Any(w => w.Id == workshopId)).ConfigureAwait(false);

        return employees.Select(a => a.UserId);
    }

    public async Task<IEnumerable<string>> GetProviderEmployeesIds(Guid providerId)
    {
        var providerEmplyees = await employeeRepository.GetByFilter(p => p.ProviderId == providerId).ConfigureAwait(false);

        return providerEmplyees.Select(d => d.UserId);
    }

    /// <inheritdoc/>
    public async Task<FullEmployeeDto> GetFullEmployee(string employeeId)
    {
        var employee = await GetById(employeeId).ConfigureAwait(false);
        if (employee == null)
        {
            return null;
        }

        await this.CheckProviderOrEmployeeRights(employee.ProviderId).ConfigureAwait(false);

        var user = (await userRepository.GetByFilter(u => u.Id == employee.UserId).ConfigureAwait(false))
            .SingleOrDefault();

        var result = mapper.Map<FullEmployeeDto>(user);

        result.WorkshopTitles = await workshopService.GetWorkshopListByEmployeeId(employeeId).ConfigureAwait(false);

        result.AccountStatus = AccountStatusExtensions.Convert(user);

        return result;
    }

    public async Task<Either<ErrorResponse, ActionResult>> ReinviteEmployeeAsync(
        string employeeId,
        string userId,
        string token)
    {
        Logger.LogDebug(
            "Employee(id): {employeeId} reinvite was started. User(id): {UserId}",
            employeeId,
            userId);

        var employee = await GetById(employeeId).ConfigureAwait(false);
        if (employee == null)
        {
            return null;
        }

        await this.CheckProviderOrEmployeeRights(employee.ProviderId)
            .ConfigureAwait(false);

        var user = (await userRepository.GetByFilter(u => u.Id == employee.UserId).ConfigureAwait(false))
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
                Message = "Only neverlogged users can be invited.",
            };
        }

        var request = new Request()
        {
            HttpMethodType = HttpMethodType.Put,
            Url = new Uri(authorizationServerConfig.Authority, string.Concat(
                CommunicationConstants.ReinviteEmployee,
                employeeId,
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
                ? JsonSerializerHelper.Deserialize<ActionResult>(result.Result.ToString())
                : null);
    }

    private async Task CheckProviderOrEmployeeRights(Guid providerId)
    {
        await currentUserService
            .UserHasRights(new ProviderRights(providerId))
            .ConfigureAwait(false);
    }

    private static Expression<Func<EmployeeDto, bool>> PredicateBuild(EmployeeSearchFilter filter)
    {
        var predicate = PredicateBuilder.True<EmployeeDto>();

        if (!string.IsNullOrWhiteSpace(filter.SearchString))
        {
            var tempPredicate = PredicateBuilder.False<EmployeeDto>();

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

        return predicate;
    }
}
