using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OutOfSchool.AuthCommon.Config;
using OutOfSchool.AuthCommon.Config.ExternalUriModels;
using OutOfSchool.AuthCommon.Extensions;
using OutOfSchool.AuthCommon.Services.Password;
using OutOfSchool.Common.Enums;
using OutOfSchool.Common.Models;
using OutOfSchool.RazorTemplatesData.Models.Emails;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.AuthCommon.Services;

public class ProviderAdminService : IProviderAdminService
{
    private readonly IEmailSender emailSender;
    private readonly IMapper mapper;
    private readonly ILogger<ProviderAdminService> logger;
    private readonly IProviderAdminRepository providerAdminRepository;
    private readonly GRPCConfig gPRCConfig;
    private readonly AngularClientScopeExternalUrisConfig externalUrisConfig;
    private readonly ChangesLogConfig changesLogConfig;

    private readonly UserManager<User> userManager;
    private readonly OutOfSchoolDbContext context;
    private readonly IRazorViewToStringRenderer renderer;
    private readonly IProviderAdminChangesLogService providerAdminChangesLogService;
    private readonly string[] trackedProperties;

    public ProviderAdminService(
        IMapper mapper,
        IProviderAdminRepository providerAdminRepository,
        ILogger<ProviderAdminService> logger,
        IEmailSender emailSender,
        UserManager<User> userManager,
        OutOfSchoolDbContext context,
        IRazorViewToStringRenderer renderer,
        IProviderAdminChangesLogService providerAdminChangesLogService,
        IOptions<GRPCConfig> gRPCConfig,
        IOptions<AngularClientScopeExternalUrisConfig> externalUrisConfig,
        IOptions<ChangesLogConfig> changesLogConfig)
    {
        this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        this.context = context ?? throw new ArgumentNullException(nameof(context));
        this.providerAdminRepository = providerAdminRepository ?? throw new ArgumentNullException(nameof(providerAdminRepository));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
        this.renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
        this.providerAdminChangesLogService = providerAdminChangesLogService ?? throw new ArgumentNullException(nameof(providerAdminChangesLogService));
        this.gPRCConfig = gRPCConfig?.Value ?? throw new ArgumentNullException(nameof(gRPCConfig));
        this.externalUrisConfig =
            externalUrisConfig?.Value ?? throw new ArgumentNullException(nameof(externalUrisConfig));
        this.changesLogConfig = changesLogConfig?.Value ?? throw new ArgumentNullException(nameof(changesLogConfig));
        this.trackedProperties =
            this.changesLogConfig.TrackedProperties.TryGetValue("ProviderAdmin", out var trackedProperties)
            ? trackedProperties
            : Array.Empty<string>();
    }

    public async Task<ResponseDto> CreateProviderAdminAsync(
        CreateProviderAdminDto providerAdminDto,
        IUrlHelper url,
        string userId)
    {
        var user = mapper.Map<User>(providerAdminDto);

        var password = PasswordGenerator
            .GenerateRandomPassword(userManager.Options.Password);

        var executionStrategy = context.Database.CreateExecutionStrategy();
        return await executionStrategy.Execute(providerAdminDto, CreateProviderAdminOperation).ConfigureAwait(false);

        async Task<ResponseDto> CreateProviderAdminOperation(CreateProviderAdminDto createDto)
        {
            if (await context.Users.AnyAsync(x => x.Email == createDto.Email).ConfigureAwait(false))
            {
                logger.LogError("Cant create provider admin with duplicate email: {Email}", createDto.Email);
                return CreateResponseDto(
                    HttpStatusCode.BadRequest,
                    $"Cant create provider admin with duplicate email: {createDto.Email}");
            }

            await using var transaction = await context.Database.BeginTransactionAsync().ConfigureAwait(false);
            try
            {
                user.IsDerived = true;
                user.IsRegistered = true;
                user.IsBlocked = false;
                user.Role = nameof(Role.Provider).ToLower();

                var result = await userManager.CreateAsync(user, password);

                if (!result.Succeeded)
                {
                    await transaction.RollbackAsync();

                    logger.LogError(
                        "Error happened while creation ProviderAdmin. User(id): {UserId}. {Errors}",
                        userId,
                        result.ErrorMessages());

                    // TODO: Don't leak all the errors eventually
                    return CreateResponseDto(
                        HttpStatusCode.BadRequest,
                        string.Join(Environment.NewLine, result.Errors.Select(e => e.Description)));
                }

                var roleAssignResult = await userManager.AddToRoleAsync(user, user.Role);

                if (!roleAssignResult.Succeeded)
                {
                    await transaction.RollbackAsync();

                    logger.LogError(
                        "Error happened while adding role to user. User(id): {UserId}. {Errors}",
                        userId,
                        result.ErrorMessages());

                    return CreateResponseDto(HttpStatusCode.InternalServerError);
                }

                createDto.UserId = user.Id;

                var providerAdmin = mapper.Map<ProviderAdmin>(createDto);
                providerAdmin.ManagedWorkshops = !providerAdmin.IsDeputy
                    ? context.Workshops.Where(w => createDto.ManagedWorkshopIds.Contains(w.Id))
                        .ToList()
                    :

                    // we create empty list, because deputy are not connected with each workshop, but to all related to provider
                    new List<Workshop>();
                if (!providerAdmin.IsDeputy && !providerAdmin.ManagedWorkshops.Any())
                {
                    await transaction.RollbackAsync();
                    logger.LogError($"Cant create assistant provider admin without related workshops");
                    return CreateResponseDto(
                        HttpStatusCode.BadRequest,
                        "You have to specify related workshops to be able to create workshop admin");
                }

                await providerAdminRepository.Create(providerAdmin).ConfigureAwait(false);

                var newPropertiesValues = GetTrackedUserProperties(user);

                foreach (var newProperty in newPropertiesValues)
                {
                    await providerAdminChangesLogService.SaveChangesLogAsync(
                        providerAdmin,
                        userId,
                        OperationType.Create,
                        newProperty.Key,
                        string.Empty,
                        newProperty.Value)
                    .ConfigureAwait(false);
                }

                foreach (var workshop in providerAdmin.ManagedWorkshops)
                {
                    await providerAdminChangesLogService.SaveChangesLogAsync(
                        providerAdmin,
                        userId,
                        OperationType.Create,
                        "WorkshopId",
                        string.Empty,
                        workshop.Id.ToString())
                    .ConfigureAwait(false);
                }

                logger.LogInformation(
                    "ProviderAdmin(id):{Id} was successfully created by User(id): {UserId}",
                    createDto.UserId,
                    userId);

                await this.SendInvitationEmail(user, url, password);

                // No sense to commit if the email was not sent, as user will not be able to login
                // and needs to be re-created
                // TODO: +1 need Endpoint with sending new password
                await transaction.CommitAsync();

                return CreateResponseDto(HttpStatusCode.OK, null, createDto);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                logger.LogError(ex, "Operation failed for User(id): {UserId}", userId);

                return CreateResponseDto(HttpStatusCode.InternalServerError);
            }
        }
    }

    public async Task<ResponseDto> UpdateProviderAdminAsync(
        UpdateProviderAdminDto providerAdminUpdateDto,
        string userId)
    {
        _ = providerAdminUpdateDto ?? throw new ArgumentNullException(nameof(providerAdminUpdateDto));

        if (await context.Users.AnyAsync(x => x.Email == providerAdminUpdateDto.Email
            && x.Id != providerAdminUpdateDto.Id).ConfigureAwait(false))
        {
            logger.LogError("Cant update provider admin with duplicate email: {Email}", providerAdminUpdateDto.Email);
            return CreateResponseDto(
                HttpStatusCode.BadRequest,
                $"Cant update provider admin with duplicate email: {providerAdminUpdateDto.Email}");
        }

        var providerAdmin = this.GetProviderAdmin(providerAdminUpdateDto.Id);

        if (providerAdmin is null)
        {
            logger.LogError(
                "ProviderAdmin(id) {Id} not found. User(id): {UserId}",
                providerAdminUpdateDto.Id,
                userId);
            return CreateResponseDto(HttpStatusCode.NotFound);
        }

        if (!providerAdmin.IsDeputy && !providerAdminUpdateDto.ManagedWorkshopIds.Any())
        {
            logger.LogError("Cant update assistant provider admin without related workshops");
            return CreateResponseDto(HttpStatusCode.BadRequest);
        }

        var user = await userManager.FindByIdAsync(providerAdminUpdateDto.Id);

        var executionStrategy = context.Database.CreateExecutionStrategy();
        return await executionStrategy.Execute(providerAdminUpdateDto, UpdateProviderAdminOperation).ConfigureAwait(false);

        async Task<ResponseDto> UpdateProviderAdminOperation(UpdateProviderAdminDto updateDto)
        {
            await using var transaction = await context.Database.BeginTransactionAsync().ConfigureAwait(false);
            try
            {
                var oldPropertiesValues = GetTrackedUserProperties(user);

                user.FirstName = updateDto.FirstName;
                user.LastName = updateDto.LastName;
                user.MiddleName = updateDto.MiddleName;
                user.Email = updateDto.Email;
                user.PhoneNumber = Constants.PhonePrefix + updateDto.PhoneNumber;

                var newPropertiesValues = GetTrackedUserProperties(user);

                var updateResult = await userManager.UpdateAsync(user);

                if (!updateResult.Succeeded)
                {
                    await transaction.RollbackAsync().ConfigureAwait(false);

                    logger.LogError(
                        "Error happened while updating ProviderAdmin. User(id): {UserId}. {Errors}",
                        userId,
                        string.Join(Environment.NewLine, updateResult.Errors.Select(e => e.Description)));
                    return CreateResponseDto(HttpStatusCode.InternalServerError);
                }

                var updateSecurityStamp = await userManager.UpdateSecurityStampAsync(user);

                if (!updateSecurityStamp.Succeeded)
                {
                    await transaction.RollbackAsync().ConfigureAwait(false);

                    logger.LogError(
                        "Error happened while updating security stamp. ProviderAdmin. User(id): {UserId}. {Errors}",
                        userId,
                        string.Join(Environment.NewLine, updateSecurityStamp.Errors.Select(e => e.Description)));
                    return CreateResponseDto(HttpStatusCode.InternalServerError);
                }

                var oldWorkshopsIds = providerAdmin.ManagedWorkshops.Select(x => x.Id).ToList();

                providerAdmin.ManagedWorkshops = !providerAdmin.IsDeputy
                    ? context.Workshops.Where(w => updateDto.ManagedWorkshopIds.Contains(w.Id)).ToList()
                    : new List<Workshop>();

                await providerAdminRepository.Update(providerAdmin).ConfigureAwait(false);
                var newWorkshopsIds = providerAdmin.ManagedWorkshops.Select(x => x.Id).ToList();
                var addedWorkshopsIds = newWorkshopsIds.Except(oldWorkshopsIds).ToList();
                var removedWorkshopsIds = oldWorkshopsIds.Except(newWorkshopsIds).ToList();

                foreach (var addedId in addedWorkshopsIds)
                {
                    await providerAdminChangesLogService.SaveChangesLogAsync(
                        providerAdmin,
                        userId,
                        OperationType.Update,
                        "WorkshopId",
                        string.Empty,
                        addedId.ToString())
                        .ConfigureAwait(false);
                }

                foreach (var removedId in removedWorkshopsIds)
                {
                    await providerAdminChangesLogService.SaveChangesLogAsync(
                        providerAdmin,
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
                        await providerAdminChangesLogService.SaveChangesLogAsync(
                            providerAdmin,
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
                    "ProviderAdmin(id):{Id} was successfully updated by User(id): {UserId}",
                    updateDto.Id,
                    userId);

                return CreateResponseDto(HttpStatusCode.OK, null, updateDto);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync().ConfigureAwait(false);

                logger.LogError(
                    ex,
                    "Error happened while updating ProviderAdmin. User(id): {UserId}",
                    userId);

                return CreateResponseDto(HttpStatusCode.InternalServerError);
            }
        }
    }

    public async Task<ResponseDto> DeleteProviderAdminAsync(
        string providerAdminId,
        string userId)
    {
        var executionStrategy = context.Database.CreateExecutionStrategy();
        var result = await executionStrategy.Execute(async () =>
        {
            await using var transaction = await context.Database.BeginTransactionAsync().ConfigureAwait(false);
            try
            {
                var providerAdmin = this.GetProviderAdmin(providerAdminId);

                if (providerAdmin is null)
                {
                    logger.LogError(
                        "ProviderAdmin(id) {ProviderAdminId} not found. User(id): {UserId}",
                        providerAdminId,
                        userId);

                    return CreateResponseDto(HttpStatusCode.NotFound);
                }

                context.ProviderAdmins.Remove(providerAdmin);

                var user = await userManager.FindByIdAsync(providerAdminId);
                var result = await userManager.DeleteAsync(user);

                if (!result.Succeeded)
                {
                    await transaction.RollbackAsync();

                    logger.LogError(
                        "Error happened while deleting ProviderAdmin. User(id): {UserId}. {Errors}",
                        userId,
                        result.ErrorMessages());

                    return CreateResponseDto(HttpStatusCode.InternalServerError);
                }

                var oldPropertiesValues = GetTrackedUserProperties(user);

                foreach (var oldProperty in oldPropertiesValues)
                {
                    await providerAdminChangesLogService.SaveChangesLogAsync(
                        providerAdmin,
                        userId,
                        OperationType.Delete,
                        oldProperty.Key,
                        oldProperty.Value,
                        string.Empty)
                    .ConfigureAwait(false);
                }

                await transaction.CommitAsync();

                logger.LogInformation(
                    "ProviderAdmin(id):{ProviderAdminId} was successfully deleted by User(id): {UserId}",
                    providerAdminId,
                    userId);

                return CreateResponseDto(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                logger.LogError(
                    ex,
                    "Error happened while deleting ProviderAdmin. User(id): {UserId}",
                    userId);

                return CreateResponseDto(HttpStatusCode.InternalServerError);
            }
        });
        return result;
    }

    public async Task<ResponseDto> BlockProviderAdminAsync(
        string providerAdminId,
        string userId,
        bool isBlocked)
    {
        var providerAdmin = this.GetProviderAdmin(providerAdminId);

        if (providerAdmin is null)
        {
            logger.LogError(
                "ProviderAdmin(id) {ProviderAdminId} not found. User(id): {UserId}",
                providerAdminId,
                userId);

            return CreateResponseDto(HttpStatusCode.NotFound);
        }

        var user = await userManager.FindByIdAsync(providerAdminId);

        var oldUserIsBlockedValue = user.IsBlocked;

        var executionStrategy = context.Database.CreateExecutionStrategy();
        return await executionStrategy.Execute(BlockProviderAdminOperation).ConfigureAwait(false);

        async Task<ResponseDto> BlockProviderAdminOperation()
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
                        "Error happened while blocking ProviderAdmin. User(id): {UserId}. {Errors}",
                        userId,
                        string.Join(Environment.NewLine, updateResult.Errors.Select(e => e.Description)));

                    return CreateResponseDto(HttpStatusCode.InternalServerError);
                }

                var updateSecurityStamp = await userManager.UpdateSecurityStampAsync(user);

                if (!updateSecurityStamp.Succeeded)
                {
                    await transaction.RollbackAsync().ConfigureAwait(false);

                    logger.LogError(
                        "Error happened while updating security stamp. ProviderAdmin. User(id): {UserId}. {Errors}",
                        userId,
                        string.Join(Environment.NewLine, updateSecurityStamp.Errors.Select(e => e.Description)));

                    return CreateResponseDto(HttpStatusCode.InternalServerError);
                }

                await providerAdminChangesLogService.SaveChangesLogAsync(
                    providerAdmin,
                    userId,
                    OperationType.Block,
                    "IsBlocked",
                    oldUserIsBlockedValue ? "1" : "0",
                    newUserIsBlockedValue ? "1" : "0")
                .ConfigureAwait(false);

                await transaction.CommitAsync().ConfigureAwait(false);

                logger.LogInformation(
                    "ProviderAdmin(id):{ProviderAdminId} was successfully blocked by User(id): {UserId}",
                    providerAdminId,
                    userId);

                return CreateResponseDto(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync().ConfigureAwait(false);

                logger.LogError(
                    ex,
                    "Error happened while blocking ProviderAdmin. User(id): {UserId}",
                    userId);

                return CreateResponseDto(HttpStatusCode.InternalServerError);
            }
        }
    }

    public async Task<ResponseDto> BlockProviderAdminsAndDeputiesByProviderAsync(
        Guid providerId,
        string userId,
        bool isBlocked)
    {
        var mainResponse = new ResponseDto() { IsSuccess = true, HttpStatusCode = HttpStatusCode.OK, Message = string.Empty };

        var providerAdmins = await providerAdminRepository
            .GetByFilter(x => x.ProviderId == providerId && x.BlockingType != BlockingType.Manually)
            .ConfigureAwait(false);

        foreach (var providerAdmin in providerAdmins)
        {
            var response = await BlockProviderAdminAsync(providerAdmin.UserId, userId, isBlocked);

            if (response.IsSuccess)
            {
                logger.LogInformation(
                    "ProviderAdmin(id):{ProviderAdminId} was successfully blocked by User(id): {UserId}",
                    providerAdmin.UserId,
                    userId);
            }
            else
            {
                mainResponse.IsSuccess = false;
                mainResponse.Message = string.Concat(mainResponse.Message, providerAdmin.UserId, " ", response.HttpStatusCode.ToString(), " ");

                logger.LogInformation(
                    "ProviderAdmin(id):{ProviderAdminId} wasn't blocked by User(id): {UserId}. Reason {ResponseHttpStatusCode}",
                    providerAdmin.UserId,
                    userId,
                    response.HttpStatusCode);
            }
        }

        return mainResponse;
    }

    public async Task<ResponseDto> ReinviteProviderAdminAsync(
        string providerAdminId,
        string userId,
        IUrlHelper url)
    {
        var executionStrategy = context.Database.CreateExecutionStrategy();
        var result = await executionStrategy.Execute(async () =>
        {
            await using var transaction = await context.Database.BeginTransactionAsync().ConfigureAwait(false);
            try
            {
                var providerAdmin = this.GetProviderAdmin(providerAdminId);

                if (providerAdmin is null)
                {
                    logger.LogError(
                        "ProviderAdmin(id) {ProviderAdminId} not found.  User(id): {UserId}",
                        providerAdminId,
                        userId);
                    return CreateResponseDto(HttpStatusCode.NotFound);
                }

                var user = await userManager.FindByIdAsync(providerAdminId);
                var password = PasswordGenerator.GenerateRandomPassword(userManager.Options.Password);
                await userManager.RemovePasswordAsync(user);
                var result = await userManager.AddPasswordAsync(user, password);

                if (!result.Succeeded)
                {
                    await transaction.RollbackAsync();

                    logger.LogError(
                        "Error happened while updating ProviderAdmin. User(id): {UserId}. {Errors}",
                        userId,
                        result.ErrorMessages());

                    return CreateResponseDto(HttpStatusCode.InternalServerError);
                }

                await this.SendInvitationEmail(user, url, password);

                await providerAdminChangesLogService.SaveChangesLogAsync(
                    providerAdmin,
                    userId,
                    OperationType.Reinvite,
                    string.Empty,
                    string.Empty,
                    string.Empty)
                .ConfigureAwait(false);

                await transaction.CommitAsync();

                logger.LogInformation(
                    "ProviderAdmin(id):{ProviderAdminId} was successfully updated by User(id): {UserId}",
                    providerAdminId,
                    userId);

                return CreateResponseDto(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                logger.LogError(
                    ex,
                    "Error happened while updating ProviderAdmin. User(id): {UserId}",
                    userId);

                return CreateResponseDto(HttpStatusCode.InternalServerError);
            }
        });
        return result;
    }

    private Dictionary<string, string?> GetTrackedUserProperties(User user)
    {
        if (user == null || !trackedProperties.Any())
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
                ? $"{gPRCConfig.ProviderAdminConfirmationLink}?userId={user.Id}&token={token}?redirectUrl={externalUrisConfig.Login}"
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

    private ProviderAdmin GetProviderAdmin(string providerAdminId)
        => context.ProviderAdmins.Include(x => x.ManagedWorkshops).SingleOrDefault(pa => pa.UserId == providerAdminId);

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
