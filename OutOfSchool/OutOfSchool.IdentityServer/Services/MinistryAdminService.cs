using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using OutOfSchool.Common.Models;
using OutOfSchool.IdentityServer.Services.Password;
using OutOfSchool.RazorTemplatesData.Models.Emails;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.IdentityServer.Services;

public class MinistryAdminService : IMinistryAdminService
{
    private readonly IEmailSender emailSender;
    private readonly IMapper mapper;
    private readonly ILogger<MinistryAdminService> logger;
    private readonly IInstitutionAdminRepository institutionAdminRepository;
    private readonly UserManager<User> userManager;
    private readonly OutOfSchoolDbContext context;
    private readonly IRazorViewToStringRenderer renderer;
    private readonly IStringLocalizer<SharedResource> localizer;

    public MinistryAdminService(
        IMapper mapper,
        IInstitutionAdminRepository institutionAdminRepository,
        ILogger<MinistryAdminService> logger,
        IEmailSender emailSender,
        UserManager<User> userManager,
        OutOfSchoolDbContext context,
        IRazorViewToStringRenderer renderer,
        IStringLocalizer<SharedResource> localizer)
    {
        ArgumentNullException.ThrowIfNull(mapper);
        ArgumentNullException.ThrowIfNull(institutionAdminRepository);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(emailSender);
        ArgumentNullException.ThrowIfNull(userManager);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(renderer);
        ArgumentNullException.ThrowIfNull(localizer);

        this.mapper = mapper;
        this.institutionAdminRepository = institutionAdminRepository;
        this.logger = logger;
        this.emailSender = emailSender;
        this.userManager = userManager;
        this.context = context;
        this.renderer = renderer;
        this.localizer = localizer;
    }

    public async Task<ResponseDto> CreateMinistryAdminAsync(
        MinistryAdminBaseDto ministryAdminBaseDto,
        IUrlHelper url,
        string userId,
        string requestId)
    {
        ArgumentNullException.ThrowIfNull(ministryAdminBaseDto);
        ArgumentNullException.ThrowIfNull(url);
        ArgumentNullException.ThrowIfNull(userId);
        ArgumentNullException.ThrowIfNull(requestId);

        var user = mapper.Map<User>(ministryAdminBaseDto);

        var password = PasswordGenerator
            .GenerateRandomPassword(userManager.Options.Password);

        var executionStrategy = context.Database.CreateExecutionStrategy();
        return await executionStrategy.Execute(CreateMinistryAdminOperation).ConfigureAwait(false);

        async Task<ResponseDto> CreateMinistryAdminOperation()
        {
            var response = new ResponseDto();
            await using var transaction = await context.Database.BeginTransactionAsync().ConfigureAwait(false);
            try
            {
                user.IsDerived = true;
                user.IsRegistered = true;
                user.IsBlocked = false;
                user.Role = nameof(Role.MinistryAdmin).ToLower();

                var result = await userManager.CreateAsync(user, password);

                if (!result.Succeeded)
                {
                    await transaction.RollbackAsync();

                    logger.LogError(
                        $"Error happened while creation MinistryAdmin. Request(id): {requestId}" +
                        $"User(id): {userId}" +
                        $"{string.Join(Environment.NewLine, result.Errors.Select(e => e.Description))}");

                    response.IsSuccess = false;
                    response.HttpStatusCode = HttpStatusCode.BadRequest;

                    // TODO: Don't leak all the errors eventually
                    response.Message = string.Join(
                        Environment.NewLine,
                        result.Errors.Select(e => e.Description));

                    return response;
                }

                var roleAssignResult = await userManager.AddToRoleAsync(user, user.Role);

                if (!roleAssignResult.Succeeded)
                {
                    await transaction.RollbackAsync();

                    logger.LogError(
                        $"Error happened while adding role to user. Request(id): {requestId}" +
                        $"User(id): {userId}" +
                        $"{string.Join(Environment.NewLine, result.Errors.Select(e => e.Description))}");

                    response.IsSuccess = false;
                    response.HttpStatusCode = HttpStatusCode.InternalServerError;

                    return response;
                }

                ministryAdminBaseDto.UserId = user.Id;

                var ministryAdmin = mapper.Map<InstitutionAdmin>(ministryAdminBaseDto);
                await institutionAdminRepository.Create(ministryAdmin)
                    .ConfigureAwait(false);

                logger.LogInformation(
                    $"MinistryAdmin(id):{ministryAdminBaseDto.UserId} was successfully created by " +
                    $"User(id): {userId}. Request(id): {requestId}");

                // TODO:
                // Endpoint with sending new password

                // TODO:
                // Use template instead
                var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
                var confirmationLink = url.Action(
                    "EmailConfirmation",
                    "Account",
                    new { userId = user.Id, token },
                    "https");
                var subject = localizer["Confirm email"];
                var adminInvitationViewModel = new AdminInvitationViewModel
                {
                    ConfirmationUrl = confirmationLink,
                    Email = user.Email,
                    Password = password,
                };
                var content = await renderer.GetHtmlPlainStringAsync(RazorTemplates.NewAdminInvitation, adminInvitationViewModel);

                await emailSender.SendAsync(user.Email, subject, content);

                // No sense to commit if the email was not sent, as user will not be able to login
                // and needs to be re-created
                // TODO: +1 need Endpoint with sending new password
                await transaction.CommitAsync();
                response.IsSuccess = true;
                response.HttpStatusCode = HttpStatusCode.OK;
                response.Result = ministryAdminBaseDto;

                return response;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                logger.LogError($"{ex.Message} User(id): {userId}.");

                response.IsSuccess = false;
                response.HttpStatusCode = HttpStatusCode.InternalServerError;

                return response;
            }
        }
    }

    public async Task<ResponseDto> DeleteMinistryAdminAsync(
        string ministryAdminId,
        string userId,
        string requestId)
    {
        ArgumentNullException.ThrowIfNull(ministryAdminId);
        ArgumentNullException.ThrowIfNull(userId);
        ArgumentNullException.ThrowIfNull(requestId);

        var executionStrategy = context.Database.CreateExecutionStrategy();
        return await executionStrategy.Execute(DeleteMinistryAdminOperation).ConfigureAwait(false);

        async Task<ResponseDto> DeleteMinistryAdminOperation()
        {
            var response = new ResponseDto();
            await using var transaction = await context.Database.BeginTransactionAsync().ConfigureAwait(false);
            try
            {
                var ministryAdmin = GetMinistryAdmin(ministryAdminId);

                if (ministryAdmin is null)
                {
                    response.IsSuccess = false;
                    response.HttpStatusCode = HttpStatusCode.NotFound;

                    logger.LogError($"MinistryAdmin(id) {ministryAdminId} not found. " +
                                    $"Request(id): {requestId}" +
                                    $"User(id): {userId}");

                    return response;
                }

                context.InstitutionAdmins.Remove(ministryAdmin);

                var user = await userManager.FindByIdAsync(ministryAdminId);
                var result = await userManager.DeleteAsync(user);

                if (!result.Succeeded)
                {
                    await transaction.RollbackAsync();

                    logger.LogError($"Error happened while deleting MinistryAdmin. Request(id): {requestId}" +
                                    $"User(id): {userId}" +
                                    $"{string.Join(Environment.NewLine, result.Errors.Select(e => e.Description))}");

                    response.IsSuccess = false;
                    response.HttpStatusCode = HttpStatusCode.InternalServerError;

                    return response;
                }

                await transaction.CommitAsync();
                response.IsSuccess = true;
                response.HttpStatusCode = HttpStatusCode.OK;

                logger.LogInformation($"MinistryAdmin(id):{ministryAdminId} was successfully deleted by " +
                                      $"User(id): {userId}. Request(id): {requestId}");

                return response;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                logger.LogError($"Error happened while deleting MinistryAdmin. Request(id): {requestId}" +
                                $"User(id): {userId} {ex.Message}");

                response.IsSuccess = false;
                response.HttpStatusCode = HttpStatusCode.InternalServerError;

                return response;
            }
        }
    }

    public async Task<ResponseDto> BlockMinistryAdminAsync(
        string ministryAdminId,
        string userId,
        string requestId,
        bool isBlocked)
    {
        ArgumentNullException.ThrowIfNull(ministryAdminId);
        ArgumentNullException.ThrowIfNull(userId);
        ArgumentNullException.ThrowIfNull(requestId);

        var response = new ResponseDto();

        var providerAdmin = GetMinistryAdmin(ministryAdminId);

        if (providerAdmin is null)
        {
            response.IsSuccess = false;
            response.HttpStatusCode = HttpStatusCode.NotFound;

            logger.LogError($"ProviderAdmin(id) {ministryAdminId} not found. " +
                            $"Request(id): {requestId}" +
                            $"User(id): {userId}");

            return response;
        }

        var user = await userManager.FindByIdAsync(ministryAdminId);

        var executionStrategy = context.Database.CreateExecutionStrategy();
        return await executionStrategy.Execute(BlockMinistryAdminOperation).ConfigureAwait(false);

        async Task<ResponseDto> BlockMinistryAdminOperation()
        {
            await using var transaction = await context.Database.BeginTransactionAsync().ConfigureAwait(false);
            try
            {
                user.IsBlocked = isBlocked;
                var updateResult = await userManager.UpdateAsync(user);

                if (!updateResult.Succeeded)
                {
                    await transaction.RollbackAsync().ConfigureAwait(false);

                    logger.LogError($"Error happened while blocking ProviderAdmin. Request(id): {requestId}" +
                                    $"User(id): {userId}" +
                                    $"{string.Join(Environment.NewLine, updateResult.Errors.Select(e => e.Description))}");

                    response.IsSuccess = false;
                    response.HttpStatusCode = HttpStatusCode.InternalServerError;

                    return response;
                }

                var updateSecurityStamp = await userManager.UpdateSecurityStampAsync(user);

                if (!updateSecurityStamp.Succeeded)
                {
                    await transaction.RollbackAsync().ConfigureAwait(false);

                    logger.LogError($"Error happened while updating security stamp. ProviderAdmin. Request(id): {requestId}" +
                                    $"User(id): {userId}" +
                                    $"{string.Join(Environment.NewLine, updateSecurityStamp.Errors.Select(e => e.Description))}");

                    response.IsSuccess = false;
                    response.HttpStatusCode = HttpStatusCode.InternalServerError;

                    return response;
                }

                await transaction.CommitAsync().ConfigureAwait(false);

                logger.LogInformation($"ProviderAdmin(id):{ministryAdminId} was successfully blocked by " +
                                      $"User(id): {userId}. Request(id): {requestId}");

                response.IsSuccess = true;
                response.HttpStatusCode = HttpStatusCode.OK;

                return response;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync().ConfigureAwait(false);

                logger.LogError($"Error happened while blocking ProviderAdmin. Request(id): {requestId}" +
                                $"User(id): {userId} {ex.Message}");

                response.IsSuccess = false;
                response.HttpStatusCode = HttpStatusCode.InternalServerError;

                return response;
            }
        }
    }

    public async Task<ResponseDto> UpdateMinistryAdminAsync(
        MinistryAdminBaseDto updateMinistryAdminDto,
        string userId,
        string requestId)
    {
        _ = updateMinistryAdminDto ?? throw new ArgumentNullException(nameof(updateMinistryAdminDto));

        var response = new ResponseDto();

        if (await context.Users.AnyAsync(x => x.Email == updateMinistryAdminDto.Email
            && x.Id != updateMinistryAdminDto.UserId).ConfigureAwait(false))
        {
            logger.LogError("Cant update ministry admin with duplicate email: {email}", updateMinistryAdminDto.Email);
            response.IsSuccess = false;
            response.HttpStatusCode = HttpStatusCode.BadRequest;
            response.Message = $"Cant update provider admin with duplicate email: {updateMinistryAdminDto.Email}";

            return response;
        }

        var ministryAdmin = GetMinistryAdmin(updateMinistryAdminDto.UserId);

        if (ministryAdmin is null)
        {
            response.IsSuccess = false;
            response.HttpStatusCode = HttpStatusCode.NotFound;

            logger.LogError(
                "ProviderAdmin(id) {providerAdminUpdateDto.Id} not found. " +
                "Request(id): {requestId}" +
                "User(id): {userId}",
                updateMinistryAdminDto.UserId,
                requestId,
                userId);

            return response;
        }

        var user = await userManager.FindByIdAsync(updateMinistryAdminDto.UserId);

        var executionStrategy = context.Database.CreateExecutionStrategy();
        return await executionStrategy.Execute(updateMinistryAdminDto, UpdateMinistryAdminOperation).ConfigureAwait(false);

        async Task<ResponseDto> UpdateMinistryAdminOperation(MinistryAdminBaseDto ministryAdminUpdateDto)
        {
            await using var transaction = await context.Database.BeginTransactionAsync().ConfigureAwait(false);
            try
            {
                user.FirstName = ministryAdminUpdateDto.FirstName;
                user.LastName = ministryAdminUpdateDto.LastName;
                user.MiddleName = ministryAdminUpdateDto.MiddleName;

                // TODO Email is changed but UserName, NormalizedUserName - no
                user.Email = ministryAdminUpdateDto.Email;
                user.PhoneNumber = ministryAdminUpdateDto.PhoneNumber;

                var updateResult = await userManager.UpdateAsync(user);

                if (!updateResult.Succeeded)
                {
                    await transaction.RollbackAsync().ConfigureAwait(false);

                    logger.LogError(
                        "Error happened while updating MinistryAdmin. Request(id): {requestId}" +
                        "User(id): {userId}" +
                        "{Errors}",
                        requestId,
                        userId,
                        string.Join(Environment.NewLine, updateResult.Errors.Select(e => e.Description)));

                    response.IsSuccess = false;
                    response.HttpStatusCode = HttpStatusCode.InternalServerError;

                    return response;
                }

                var updateSecurityStamp = await userManager.UpdateSecurityStampAsync(user);

                if (!updateSecurityStamp.Succeeded)
                {
                    await transaction.RollbackAsync().ConfigureAwait(false);

                    logger.LogError(
                        "Error happened while updating security stamp. MinistryAdmin. Request(id): {requestId}" +
                        "User(id): {userId}" +
                        "{Errors}",
                        requestId,
                        userId,
                        string.Join(Environment.NewLine, updateSecurityStamp.Errors.Select(e => e.Description)));

                    response.IsSuccess = false;
                    response.HttpStatusCode = HttpStatusCode.InternalServerError;

                    return response;
                }

                // TODO How handle if such InstitutionId doesn't exist
                ministryAdmin.InstitutionId = updateMinistryAdminDto.InstitutionId;

                await institutionAdminRepository.Update(ministryAdmin).ConfigureAwait(false);

                // TODO Add write changeslog
                await transaction.CommitAsync().ConfigureAwait(false);

                logger.LogInformation(
                    "MinistryAdmin(id):{ministryAdminUpdateDto.UserId} was successfully updated by " +
                    "User(id): {userId}. Request(id): {requestId}",
                    ministryAdminUpdateDto.UserId,
                    userId,
                    requestId);

                response.IsSuccess = true;
                response.HttpStatusCode = HttpStatusCode.OK;
                response.Result = ministryAdminUpdateDto;

                return response;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync().ConfigureAwait(false);

                logger.LogError(
                    "Error happened while updating ProviderAdmin. Request(id): {requestId}" +
                    "User(id): {userId} {ex.Message}",
                    requestId,
                    userId,
                    ex.Message);

                response.IsSuccess = false;
                response.HttpStatusCode = HttpStatusCode.InternalServerError;

                return response;
            }
        }
    }

    private InstitutionAdmin GetMinistryAdmin(string ministryAdminId)
        => context.InstitutionAdmins.SingleOrDefault(pa => pa.UserId == ministryAdminId);
}