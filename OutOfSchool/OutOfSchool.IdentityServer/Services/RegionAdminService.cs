using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using OutOfSchool.Common.Models;
using OutOfSchool.IdentityServer.Services.Password;
using OutOfSchool.RazorTemplatesData.Models.Emails;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.IdentityServer.Services;

public class RegionAdminService : IRegionAdminService
{
    private readonly IEmailSender emailSender;
    private readonly IMapper mapper;
    private readonly ILogger<RegionAdminService> logger;
    private readonly IRegionAdminRepository regionAdminRepository;
    private readonly UserManager<User> userManager;
    private readonly OutOfSchoolDbContext context;
    private readonly IRazorViewToStringRenderer renderer;
    private readonly IStringLocalizer<SharedResource> localizer;

    public RegionAdminService(
        IMapper mapper,
        IRegionAdminRepository regionAdminRepository,
        ILogger<RegionAdminService> logger,
        IEmailSender emailSender,
        UserManager<User> userManager,
        OutOfSchoolDbContext context,
        IRazorViewToStringRenderer renderer,
        IStringLocalizer<SharedResource> localizer)
    {
        ArgumentNullException.ThrowIfNull(mapper);
        ArgumentNullException.ThrowIfNull(regionAdminRepository);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(emailSender);
        ArgumentNullException.ThrowIfNull(userManager);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(renderer);
        ArgumentNullException.ThrowIfNull(localizer);

        this.mapper = mapper;
        this.regionAdminRepository = regionAdminRepository;
        this.logger = logger;
        this.emailSender = emailSender;
        this.userManager = userManager;
        this.context = context;
        this.renderer = renderer;
        this.localizer = localizer;
    }

    public async Task<ResponseDto> CreateRegionAdminAsync(
        RegionAdminBaseDto regionAdminBaseDto,
        IUrlHelper url,
        string userId,
        string requestId)
    {
        ArgumentNullException.ThrowIfNull(regionAdminBaseDto);
        ArgumentNullException.ThrowIfNull(url);
        ArgumentNullException.ThrowIfNull(userId);
        ArgumentNullException.ThrowIfNull(requestId);

        var user = mapper.Map<User>(regionAdminBaseDto);

        var password = PasswordGenerator
            .GenerateRandomPassword(userManager.Options.Password);

        var executionStrategy = context.Database.CreateExecutionStrategy();
        return await executionStrategy.Execute(CreateRegionAdminOperation).ConfigureAwait(false);

        async Task<ResponseDto> CreateRegionAdminOperation()
        {
            var response = new ResponseDto();
            await using var transaction = await context.Database.BeginTransactionAsync().ConfigureAwait(false);
            try
            {
                user.IsDerived = true;
                user.IsRegistered = true;
                user.IsBlocked = false;
                user.Role = nameof(Role.RegionAdmin).ToLower();

                var result = await userManager.CreateAsync(user, password);

                if (!result.Succeeded)
                {
                    await transaction.RollbackAsync();

                    logger.LogError(
                        $"Error happened while creation RegionAdmin. Request(id): {requestId}" +
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

                regionAdminBaseDto.UserId = user.Id;

                var regionAdmin = mapper.Map<RegionAdmin>(regionAdminBaseDto);
                await regionAdminRepository.Create(regionAdmin)
                    .ConfigureAwait(false);

                logger.LogInformation(
                    $"RegionAdmin(id):{regionAdminBaseDto.UserId} was successfully created by " +
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
                response.Result = regionAdminBaseDto;

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

    public async Task<ResponseDto> DeleteRegionAdminAsync(
        string regionAdminId,
        string userId,
        string requestId)
    {
        ArgumentNullException.ThrowIfNull(regionAdminId);
        ArgumentNullException.ThrowIfNull(userId);
        ArgumentNullException.ThrowIfNull(requestId);

        var executionStrategy = context.Database.CreateExecutionStrategy();
        return await executionStrategy.Execute(DeleteRegionAdminOperation).ConfigureAwait(false);

        async Task<ResponseDto> DeleteRegionAdminOperation()
        {
            var response = new ResponseDto();
            await using var transaction = await context.Database.BeginTransactionAsync().ConfigureAwait(false);
            try
            {
                var regionAdmin = GetRegionAdmin(regionAdminId);

                if (regionAdmin is null)
                {
                    response.IsSuccess = false;
                    response.HttpStatusCode = HttpStatusCode.NotFound;

                    logger.LogError($"RegionAdmin(id) {regionAdminId} not found. " +
                                    $"Request(id): {requestId}" +
                                    $"User(id): {userId}");

                    return response;
                }

                context.RegionAdmins.Remove(regionAdmin);

                var user = await userManager.FindByIdAsync(regionAdminId);
                var result = await userManager.DeleteAsync(user);

                if (!result.Succeeded)
                {
                    await transaction.RollbackAsync();

                    logger.LogError($"Error happened while deleting RegionAdmin. Request(id): {requestId}" +
                                    $"User(id): {userId}" +
                                    $"{string.Join(Environment.NewLine, result.Errors.Select(e => e.Description))}");

                    response.IsSuccess = false;
                    response.HttpStatusCode = HttpStatusCode.InternalServerError;

                    return response;
                }

                await transaction.CommitAsync();
                response.IsSuccess = true;
                response.HttpStatusCode = HttpStatusCode.OK;

                logger.LogInformation($"RegionAdmin(id):{regionAdminId} was successfully deleted by " +
                                      $"User(id): {userId}. Request(id): {requestId}");

                return response;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                logger.LogError($"Error happened while deleting RegionAdmin. Request(id): {requestId}" +
                                $"User(id): {userId} {ex.Message}");

                response.IsSuccess = false;
                response.HttpStatusCode = HttpStatusCode.InternalServerError;

                return response;
            }
        }
    }

    public async Task<ResponseDto> BlockRegionAdminAsync(
        string regionAdminId,
        string userId,
        string requestId,
        bool isBlocked)
    {
        ArgumentNullException.ThrowIfNull(regionAdminId);
        ArgumentNullException.ThrowIfNull(userId);
        ArgumentNullException.ThrowIfNull(requestId);

        var response = new ResponseDto();

        var regionAdmin = GetRegionAdmin(regionAdminId);

        if (regionAdmin is null)
        {
            response.IsSuccess = false;
            response.HttpStatusCode = HttpStatusCode.NotFound;

            logger.LogError($"RegionAdmin(id) {regionAdminId} not found. " +
                            $"Request(id): {requestId}" +
                            $"User(id): {userId}");

            return response;
        }

        var user = await userManager.FindByIdAsync(regionAdminId);

        var executionStrategy = context.Database.CreateExecutionStrategy();
        return await executionStrategy.Execute(BlockRegionAdminOperation).ConfigureAwait(false);

        async Task<ResponseDto> BlockRegionAdminOperation()
        {
            await using var transaction = await context.Database.BeginTransactionAsync().ConfigureAwait(false);
            try
            {
                user.IsBlocked = isBlocked;
                var updateResult = await userManager.UpdateAsync(user);

                if (!updateResult.Succeeded)
                {
                    await transaction.RollbackAsync().ConfigureAwait(false);

                    logger.LogError($"Error happened while blocking RegionAdmin. Request(id): {requestId}" +
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

                    logger.LogError($"Error happened while updating security stamp. RegionAdmin. Request(id): {requestId}" +
                                    $"User(id): {userId}" +
                                    $"{string.Join(Environment.NewLine, updateSecurityStamp.Errors.Select(e => e.Description))}");

                    response.IsSuccess = false;
                    response.HttpStatusCode = HttpStatusCode.InternalServerError;

                    return response;
                }

                await transaction.CommitAsync().ConfigureAwait(false);

                logger.LogInformation($"RegionAdmin(id):{regionAdminId} was successfully blocked by " +
                                      $"User(id): {userId}. Request(id): {requestId}");

                response.IsSuccess = true;
                response.HttpStatusCode = HttpStatusCode.OK;

                return response;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync().ConfigureAwait(false);

                logger.LogError($"Error happened while blocking RegionAdmin. Request(id): {requestId}" +
                                $"User(id): {userId} {ex.Message}");

                response.IsSuccess = false;
                response.HttpStatusCode = HttpStatusCode.InternalServerError;

                return response;
            }
        }
    }

    public async Task<ResponseDto> UpdateRegionAdminAsync(
        RegionAdminBaseDto updateRegionAdminDto,
        string userId,
        string requestId)
    {
        _ = updateRegionAdminDto ?? throw new ArgumentNullException(nameof(updateRegionAdminDto));

        var response = new ResponseDto();

        if (await context.Users.AnyAsync(x => x.Email == updateRegionAdminDto.Email
            && x.Id != updateRegionAdminDto.UserId).ConfigureAwait(false))
        {
            logger.LogError($"Cant update region admin with duplicate email: {updateRegionAdminDto.Email}");
            response.IsSuccess = false;
            response.HttpStatusCode = HttpStatusCode.BadRequest;
            response.Message = $"Cant update region admin with duplicate email: {updateRegionAdminDto.Email}";

            return response;
        }

        var regionAdmin = GetRegionAdmin(updateRegionAdminDto.UserId);

        if (regionAdmin is null)
        {
            response.IsSuccess = false;
            response.HttpStatusCode = HttpStatusCode.NotFound;

            logger.LogError($"RegionAdmin(id) {updateRegionAdminDto.UserId} not found. " +
                            $"Request(id): {requestId}" +
                            $"User(id): {userId}");

            return response;
        }

        var user = await userManager.FindByIdAsync(updateRegionAdminDto.UserId);

        var executionStrategy = context.Database.CreateExecutionStrategy();
        return await executionStrategy.Execute(updateRegionAdminDto, UpdateRegionAdminOperation).ConfigureAwait(false);

        async Task<ResponseDto> UpdateRegionAdminOperation(RegionAdminBaseDto regionAdminUpdateDto)
        {
            await using var transaction = await context.Database.BeginTransactionAsync().ConfigureAwait(false);
            try
            {
                user.FirstName = regionAdminUpdateDto.FirstName;
                user.LastName = regionAdminUpdateDto.LastName;
                user.MiddleName = regionAdminUpdateDto.MiddleName;

                // TODO Email is changed but UserName, NormalizedUserName - no
                user.Email = regionAdminUpdateDto.Email;
                user.PhoneNumber = regionAdminUpdateDto.PhoneNumber;

                var updateResult = await userManager.UpdateAsync(user);

                if (!updateResult.Succeeded)
                {
                    await transaction.RollbackAsync().ConfigureAwait(false);

                    logger.LogError($"Error happened while updating RegionAdmin. Request(id): {requestId}" +
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

                    logger.LogError($"Error happened while updating security stamp. RegionAdmin. Request(id): {requestId}" +
                                    $"User(id): {userId}" +
                                    $"{string.Join(Environment.NewLine, updateSecurityStamp.Errors.Select(e => e.Description))}");

                    response.IsSuccess = false;
                    response.HttpStatusCode = HttpStatusCode.InternalServerError;

                    return response;
                }

                // TODO How handle if such InstitutionId doesn't exist
                regionAdmin.InstitutionId = updateRegionAdminDto.InstitutionId;

                await regionAdminRepository.Update(regionAdmin).ConfigureAwait(false);

                // TODO Add write changeslog
                await transaction.CommitAsync().ConfigureAwait(false);

                logger.LogInformation($"RegionAdmin(id):{updateRegionAdminDto.UserId} was successfully updated by " +
                                      $"User(id): {userId}. Request(id): {requestId}");

                response.IsSuccess = true;
                response.HttpStatusCode = HttpStatusCode.OK;
                response.Result = updateRegionAdminDto;

                return response;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync().ConfigureAwait(false);

                logger.LogError($"Error happened while updating RegionAdmin. Request(id): {requestId}" +
                                $"User(id): {userId} {ex.Message}");

                response.IsSuccess = false;
                response.HttpStatusCode = HttpStatusCode.InternalServerError;

                return response;
            }
        }
    }

    private RegionAdmin GetRegionAdmin(string regionAdminId)
        => context.RegionAdmins.SingleOrDefault(pa => pa.UserId == regionAdminId);
}