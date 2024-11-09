using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OutOfSchool.AuthCommon.Config;
using OutOfSchool.AuthCommon.Config.ExternalUriModels;
using OutOfSchool.AuthCommon.Extensions;
using OutOfSchool.AuthCommon.Services.Password;
using OutOfSchool.Common.Enums;
using OutOfSchool.Common.Models;
using OutOfSchool.EmailSender.Services;
using OutOfSchool.RazorTemplatesData.Models.Emails;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Repository.Api;

namespace OutOfSchool.AuthCommon.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IEmailSenderService emailSender;
    private readonly IMapper mapper;
    private readonly ILogger<EmployeeService> logger;
    private readonly IEmployeeRepository employeeRepository;
    private readonly GrpcConfig grpcConfig;
    private readonly AngularClientScopeExternalUrisConfig externalUrisConfig;

    private readonly UserManager<User> userManager;
    private readonly OutOfSchoolDbContext context;
    private readonly IRazorViewToStringRenderer renderer;
    private readonly IEmployeeChangesLogService employeeChangesLogService;
    private readonly string[] trackedProperties;

    public EmployeeService(
        IMapper mapper,
        IEmployeeRepository employeeRepository,
        ILogger<EmployeeService> logger,
        IEmailSenderService emailSender,
        UserManager<User> userManager,
        OutOfSchoolDbContext context,
        IRazorViewToStringRenderer renderer,
        IEmployeeChangesLogService employeeChangesLogService,
        IOptions<GrpcConfig> grpcConfig,
        IOptions<AngularClientScopeExternalUrisConfig> externalUrisConfig,
        IOptions<ChangesLogConfig> changesLogConfig,
        IOptions<HostsConfig> hostsConfig)
    {
        this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        this.context = context ?? throw new ArgumentNullException(nameof(context));
        this.employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
        this.renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
        this.employeeChangesLogService = employeeChangesLogService ?? throw new ArgumentNullException(nameof(employeeChangesLogService));
        this.grpcConfig = grpcConfig?.Value ?? throw new ArgumentNullException(nameof(grpcConfig));
        this.externalUrisConfig =
            externalUrisConfig?.Value ?? throw new ArgumentNullException(nameof(externalUrisConfig));
        _ = changesLogConfig?.Value ?? throw new ArgumentNullException(nameof(changesLogConfig));
        this.trackedProperties =
            changesLogConfig.Value.TrackedProperties.TryGetValue("ProviderAdmin", out var properties)
            ? properties
            : Array.Empty<string>();
        _ = hostsConfig?.Value ?? throw new ArgumentNullException(nameof(hostsConfig));
    }

    public async Task<ResponseDto> CreateEmployeeAsync(
        CreateEmployeeDto employeeDto,
        IUrlHelper url,
        string userId)
    {
        var user = mapper.Map<User>(employeeDto);

        var password = PasswordGenerator
            .GenerateRandomPassword();

        var executionStrategy = context.Database.CreateExecutionStrategy();
        return await executionStrategy.Execute(employeeDto, CreateEmployeeOperation).ConfigureAwait(false);

        async Task<ResponseDto> CreateEmployeeOperation(CreateEmployeeDto createDto)
        {
            if (await context.Users.AnyAsync(x => x.Email == createDto.Email).ConfigureAwait(false))
            {
                logger.LogError("Cant create employee with duplicate email: {Email}", createDto.Email);
                return CreateResponseDto(
                    HttpStatusCode.BadRequest,
                    $"Cant create employee with duplicate email: {createDto.Email}");
            }

            await using var transaction = await context.Database.BeginTransactionAsync().ConfigureAwait(false);
            try
            {
                user.IsDerived = true;
                user.IsRegistered = true;
                user.IsBlocked = false;
                user.Role = nameof(Role.Employee).ToLower();

                var result = await userManager.CreateAsync(user, password);

                if (!result.Succeeded)
                {
                    await transaction.RollbackAsync();
                    var errorMessages = result.ErrorMessages();

                    logger.LogError(
                        "Error happened while creation Employee. User(id): {UserId}. {Errors}",
                        userId,
                        errorMessages);

                    // TODO: Don't leak all the errors eventually
                    return CreateResponseDto(
                        HttpStatusCode.BadRequest,
                        errorMessages);
                }

                var roleAssignResult = await userManager.AddToRoleAsync(user, user.Role);

                if (!roleAssignResult.Succeeded)
                {
                    await transaction.RollbackAsync();

                    logger.LogError(
                        "Error happened while adding role to user. User(id): {UserId}. {Errors}",
                        userId,
                        roleAssignResult.ErrorMessages());

                    return CreateResponseDto(HttpStatusCode.InternalServerError);
                }

                createDto.UserId = user.Id;

                var employee = mapper.Map<Employee>(createDto);
                employee.ManagedWorkshops =
                    context.Workshops.Where(w => createDto.ManagedWorkshopIds.Contains(w.Id)).ToList();
                if (!employee.ManagedWorkshops.Any())
                {
                    await transaction.RollbackAsync();
                    logger.LogError($"Cant create employee without related workshops");
                    return CreateResponseDto(
                        HttpStatusCode.BadRequest,
                        "You have to specify related workshops to be able to create workshop employee");
                }

                var newEmployee = await employeeRepository.Create(employee).ConfigureAwait(false);

                if (newEmployee is not null)
                {
                    await employeeChangesLogService.SaveChangesLogAsync(
                        employee,
                        userId,
                        OperationType.Create,
                        string.Empty,
                        string.Empty,
                        user.Email)
                    .ConfigureAwait(false);
                }

                logger.LogInformation(
                    "Employee(id):{Id} was successfully created by User(id): {UserId}",
                    createDto.UserId,
                    userId);

                await this.SendInvitationEmail(user, url, password);

                // No sense to commit if the email was not sent, as user will not be able to login
                // and needs to be re-created
                // TODO: +1 need Endpoint with sending new password
                await transaction.CommitAsync();

                return CreateResponseDto(HttpStatusCode.Created, null, createDto);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                logger.LogError(ex, "Operation failed for User(id): {UserId}", userId);

                return CreateResponseDto(HttpStatusCode.InternalServerError);
            }
        }
    }

    public async Task<ResponseDto> UpdateEmployeeAsync(
        UpdateEmployeeDto employeeUpdateDto,
        string userId)
    {
        _ = employeeUpdateDto ?? throw new ArgumentNullException(nameof(employeeUpdateDto));

        if (await context.Users.AnyAsync(x => x.Email == employeeUpdateDto.Email
            && x.Id != employeeUpdateDto.Id).ConfigureAwait(false))
        {
            logger.LogError("Cant update employee with duplicate email: {Email}", employeeUpdateDto.Email);
            return CreateResponseDto(
                HttpStatusCode.BadRequest,
                $"Cant update employee with duplicate email: {employeeUpdateDto.Email}");
        }

        var employee = this.GetEmployee(employeeUpdateDto.Id);

        if (employee is null)
        {
            logger.LogError(
                "Employee(id) {Id} not found. User(id): {UserId}",
                employeeUpdateDto.Id,
                userId);
            return CreateResponseDto(HttpStatusCode.NotFound);
        }

        if (!employeeUpdateDto.ManagedWorkshopIds.Any())
        {
            logger.LogError("Cant update employee without related workshops");
            return CreateResponseDto(HttpStatusCode.BadRequest);
        }

        var user = await userManager.FindByIdAsync(employeeUpdateDto.Id);

        var executionStrategy = context.Database.CreateExecutionStrategy();
        return await executionStrategy.Execute(employeeUpdateDto, UpdateEmployeeOperation).ConfigureAwait(false);

        async Task<ResponseDto> UpdateEmployeeOperation(UpdateEmployeeDto updateDto)
        {
            await using var transaction = await context.Database.BeginTransactionAsync().ConfigureAwait(false);
            try
            {
                var oldPropertiesValues = GetTrackedUserProperties(user);

                user.FirstName = updateDto.FirstName;
                user.LastName = updateDto.LastName;
                user.MiddleName = updateDto.MiddleName;
                user.Email = updateDto.Email;
                user.PhoneNumber = updateDto.PhoneNumber;

                var newPropertiesValues = GetTrackedUserProperties(user);

                var updateResult = await userManager.UpdateAsync(user);

                if (!updateResult.Succeeded)
                {
                    await transaction.RollbackAsync().ConfigureAwait(false);

                    logger.LogError(
                        "Error happened while updating Employee. User(id): {UserId}. {Errors}",
                        userId,
                        updateResult.ErrorMessages());

                    return CreateResponseDto(HttpStatusCode.InternalServerError);
                }

                var updateSecurityStamp = await userManager.UpdateSecurityStampAsync(user);

                if (!updateSecurityStamp.Succeeded)
                {
                    await transaction.RollbackAsync().ConfigureAwait(false);

                    logger.LogError(
                        "Error happened while updating security stamp. Employee. User(id): {UserId}. {Errors}",
                        userId,
                        updateSecurityStamp.ErrorMessages());
                    return CreateResponseDto(HttpStatusCode.InternalServerError);
                }

                var oldWorkshopsIds = employee.ManagedWorkshops.Select(x => x.Id).ToList();

                employee.ManagedWorkshops =
                    context.Workshops.Where(w => updateDto.ManagedWorkshopIds.Contains(w.Id)).ToList();

                await employeeRepository.Update(employee).ConfigureAwait(false);
                var newWorkshopsIds = employee.ManagedWorkshops.Select(x => x.Id).ToList();
                var addedWorkshopsIds = newWorkshopsIds.Except(oldWorkshopsIds).ToList();
                var removedWorkshopsIds = oldWorkshopsIds.Except(newWorkshopsIds).ToList();

                foreach (var addedId in addedWorkshopsIds)
                {
                    await employeeChangesLogService.SaveChangesLogAsync(
                        employee,
                        userId,
                        OperationType.Update,
                        "WorkshopId",
                        string.Empty,
                        addedId.ToString())
                        .ConfigureAwait(false);
                }

                foreach (var removedId in removedWorkshopsIds)
                {
                    await employeeChangesLogService.SaveChangesLogAsync(
                        employee,
                        userId,
                        OperationType.Update,
                        "WorkshopId",
                        removedId.ToString(),
                        string.Empty)
                        .ConfigureAwait(false);
                }

                foreach (var newProperty in newPropertiesValues)
                {
                    oldPropertiesValues.TryGetValue(newProperty.Key, out var oldValue);
                    if (newProperty.Value != oldValue)
                    {
                        await employeeChangesLogService.SaveChangesLogAsync(
                            employee,
                            userId,
                            OperationType.Update,
                            newProperty.Key,
                            oldValue,
                            newProperty.Value)
                            .ConfigureAwait(false);
                    }
                }

                await transaction.CommitAsync().ConfigureAwait(false);

                logger.LogInformation(
                    "Employee(id):{Id} was successfully updated by User(id): {UserId}",
                    updateDto.Id,
                    userId);

                return CreateResponseDto(HttpStatusCode.OK, null, updateDto);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync().ConfigureAwait(false);

                logger.LogError(
                    ex,
                    "Error happened while updating Employee. User(id): {UserId}",
                    userId);

                return CreateResponseDto(HttpStatusCode.InternalServerError);
            }
        }
    }

    public async Task<ResponseDto> DeleteEmployeeAsync(
        string employeeId,
        string userId)
    {
        var employee = this.GetEmployee(employeeId);

        if (employee is null)
        {
            logger.LogError(
                "Employee(id) {employeeId} not found. User(id): {UserId}",
                employeeId,
                userId);

            return CreateResponseDto(HttpStatusCode.NotFound);
        }

        var executionStrategy = context.Database.CreateExecutionStrategy();
        return await executionStrategy.Execute(DeleteEmployeeOperation).ConfigureAwait(false);

        async Task<ResponseDto> DeleteEmployeeOperation()
        {
            await using var transaction = await context.Database.BeginTransactionAsync().ConfigureAwait(false);
            try
            {
                context.Employees.Remove(employee);

                var user = await userManager.FindByIdAsync(employeeId);
                var result = await userManager.DeleteAsync(user);

                if (!result.Succeeded)
                {
                    await transaction.RollbackAsync();

                    logger.LogError(
                        "Error happened while deleting Employee. User(id): {UserId}. {Errors}",
                        userId,
                        result.ErrorMessages());

                    return CreateResponseDto(HttpStatusCode.InternalServerError);
                }

                await employeeChangesLogService.SaveChangesLogAsync(
                        employee,
                        userId,
                        OperationType.Delete,
                        string.Empty,
                        string.Empty,
                        string.Empty)
                    .ConfigureAwait(false);

                await transaction.CommitAsync();

                logger.LogInformation(
                    "Employee(id):{employeeId} was successfully deleted by User(id): {UserId}",
                    employeeId,
                    userId);

                return CreateResponseDto(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                logger.LogError(
                    ex,
                    "Error happened while deleting Employee. User(id): {UserId}",
                    userId);

                return CreateResponseDto(HttpStatusCode.InternalServerError);
            }
        }
    }

    public async Task<ResponseDto> BlockEmployeeAsync(
        string employeeId,
        string userId,
        bool isBlocked)
    {
        var employee = this.GetEmployee(employeeId);

        if (employee is null)
        {
            logger.LogError(
                "Employee(id) {employeeId} not found. User(id): {UserId}",
                employeeId,
                userId);

            return CreateResponseDto(HttpStatusCode.NotFound);
        }

        var user = await userManager.FindByIdAsync(employeeId);

        var oldUserIsBlockedValue = user.IsBlocked;

        var executionStrategy = context.Database.CreateExecutionStrategy();
        return await executionStrategy.Execute(BlockEmployeeOperation).ConfigureAwait(false);

        async Task<ResponseDto> BlockEmployeeOperation()
        {
            await using var transaction = await context.Database.BeginTransactionAsync().ConfigureAwait(false);
            try
            {
                user.IsBlocked = isBlocked;
                var newUserIsBlockedValue = user.IsBlocked;
                var updateResult = await userManager.UpdateAsync(user);

                if (!updateResult.Succeeded)
                {
                    await transaction.RollbackAsync().ConfigureAwait(false);

                    logger.LogError(
                        "Error happened while blocking Employee. User(id): {UserId}. {Errors}",
                        userId,
                        updateResult.ErrorMessages());

                    return CreateResponseDto(HttpStatusCode.InternalServerError);
                }

                var updateSecurityStamp = await userManager.UpdateSecurityStampAsync(user);

                if (!updateSecurityStamp.Succeeded)
                {
                    await transaction.RollbackAsync().ConfigureAwait(false);

                    logger.LogError(
                        "Error happened while updating security stamp. Employee. User(id): {UserId}. {Errors}",
                        userId,
                        updateSecurityStamp.ErrorMessages());

                    return CreateResponseDto(HttpStatusCode.InternalServerError);
                }

                await employeeChangesLogService.SaveChangesLogAsync(
                    employee,
                    userId,
                    OperationType.Block,
                    "IsBlocked",
                    oldUserIsBlockedValue ? "1" : "0",
                    newUserIsBlockedValue ? "1" : "0")
                .ConfigureAwait(false);

                await transaction.CommitAsync().ConfigureAwait(false);

                logger.LogInformation(
                    "Employee(id):{employeeId} was successfully blocked by User(id): {UserId}",
                    employeeId,
                    userId);

                return CreateResponseDto(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync().ConfigureAwait(false);

                logger.LogError(
                    ex,
                    "Error happened while blocking Employee. User(id): {UserId}",
                    userId);

                return CreateResponseDto(HttpStatusCode.InternalServerError);
            }
        }
    }

    public async Task<ResponseDto> BlockEmployeesByProviderAsync(
        Guid providerId,
        string userId,
        bool isBlocked)
    {
        var mainResponse = new ResponseDto() { IsSuccess = true, HttpStatusCode = HttpStatusCode.OK, Message = string.Empty };

        var employees = await employeeRepository
            .GetByFilter(x => x.ProviderId == providerId && x.BlockingType != BlockingType.Manually)
            .ConfigureAwait(false);

        foreach (var employeeUserId in employees.Select(employee => employee.UserId))
        {
            var response = await this.BlockEmployeeAsync(employeeUserId, userId, isBlocked);

            if (response.IsSuccess)
            {
                logger.LogInformation(
                    "Employee(id):{employeeUserId} was successfully blocked by User(id): {UserId}",
                    employeeUserId,
                    userId);
            }
            else
            {
                mainResponse.IsSuccess = false;
                mainResponse.Message = string.Concat(mainResponse.Message, employeeUserId, " ", response.HttpStatusCode.ToString(), " ");

                logger.LogInformation(
                    "Employee(id):{employeeUserId} wasn't blocked by User(id): {UserId}. Reason {ResponseHttpStatusCode}",
                    employeeUserId,
                    userId,
                    response.HttpStatusCode);
            }
        }

        return mainResponse;
    }

    public async Task<ResponseDto> ReinviteEmployeeAsync(
        string employeeId,
        string userId,
        IUrlHelper url)
    {
        var executionStrategy = context.Database.CreateExecutionStrategy();
        var result = await executionStrategy.Execute(async () =>
        {
            await using var transaction = await context.Database.BeginTransactionAsync().ConfigureAwait(false);
            try
            {
                var employee = this.GetEmployee(employeeId);

                if (employee is null)
                {
                    logger.LogError(
                        "Employee(id) {employeeId} not found.  User(id): {UserId}",
                        employeeId,
                        userId);
                    return CreateResponseDto(HttpStatusCode.NotFound);
                }

                var user = await userManager.FindByIdAsync(employeeId);
                var password = PasswordGenerator.GenerateRandomPassword();
                await userManager.RemovePasswordAsync(user);
                var result = await userManager.AddPasswordAsync(user, password);

                if (!result.Succeeded)
                {
                    await transaction.RollbackAsync();

                    logger.LogError(
                        "Error happened while updating Employee. User(id): {UserId}. {Errors}",
                        userId,
                        result.ErrorMessages());

                    return CreateResponseDto(HttpStatusCode.InternalServerError);
                }

                await this.SendInvitationEmail(user, url, password);

                await employeeChangesLogService.SaveChangesLogAsync(
                    employee,
                    userId,
                    OperationType.Reinvite,
                    string.Empty,
                    string.Empty,
                    string.Empty)
                .ConfigureAwait(false);

                await transaction.CommitAsync();

                logger.LogInformation(
                    "Employee(id):{employeeId} was successfully updated by User(id): {UserId}",
                    employeeId,
                    userId);

                return CreateResponseDto(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                logger.LogError(
                    ex,
                    "Error happened while updating Employee. User(id): {UserId}",
                    userId);

                return CreateResponseDto(HttpStatusCode.InternalServerError);
            }
        });
        return result;
    }

    private Dictionary<string, string?> GetTrackedUserProperties(User user)
    {
        if (user is null || !trackedProperties.Any())
        {
            return new Dictionary<string, string?> { };
        }

        Dictionary<string, string?> result = new();
        foreach (var propertyName in trackedProperties)
        {
            var property = user.GetType().GetProperty(propertyName);
            result.Add(propertyName, property?.GetValue(user)?.ToString());
        }

        return result;
    }

    private async Task SendInvitationEmail(User user, IUrlHelper url, string password)
    {
        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);

        string confirmationLink =
        url is null
                ? $"{grpcConfig.EmployeeConfirmationLink}?userId={user.Id}&token={token}?redirectUrl={externalUrisConfig.Login}"
                : url.Action(
                    "EmailConfirmation",
                    "Account",
                    new { email = user.Email, token, redirectUrl = externalUrisConfig.Login },
                    "https");

        var subject = "Запрошення!";
        var adminInvitationViewModel = new AdminInvitationViewModel
        {
            ConfirmationUrl = confirmationLink,
            Email = user.Email,
            Password = password,
        };
        var content = await renderer.GetHtmlPlainStringAsync(RazorTemplates.NewAdminInvitation, adminInvitationViewModel);

        await emailSender.SendAsync(user.Email, subject, content);
    }

    private Employee GetEmployee(string employeeId)
        => context.Employees.Include(x => x.ManagedWorkshops).SingleOrDefault(pa => pa.UserId == employeeId);

    private ResponseDto CreateResponseDto(HttpStatusCode statusCode, string? message = null, object? result = null)
    {
        return new ResponseDto()
        {
            IsSuccess = statusCode == HttpStatusCode.OK,
            HttpStatusCode = statusCode,
            Message = message,
            Result = result,
        };
    }
}
