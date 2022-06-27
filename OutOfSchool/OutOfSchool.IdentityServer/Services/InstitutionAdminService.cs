using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OutOfSchool.Common;
using OutOfSchool.Common.Models;
using OutOfSchool.EmailSender;
using OutOfSchool.IdentityServer.Services.Intefaces;
using OutOfSchool.IdentityServer.Services.Interfaces;
using OutOfSchool.IdentityServer.Services.Password;
using OutOfSchool.RazorTemplatesData.Models.Emails;
using OutOfSchool.RazorTemplatesData.Services;
using OutOfSchool.Services;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;

namespace OutOfSchool.IdentityServer.Services
{
    public class InstitutionAdminService : IInstitutionAdminService
    {
        private readonly IEmailSender emailSender;
        private readonly IMapper mapper;
        private readonly ILogger<InstitutionAdminService> logger;
        private readonly IInstitutionAdminRepository institutionAdminRepository;

        private readonly UserManager<User> userManager;
        private readonly OutOfSchoolDbContext context;
        private readonly IRazorViewToStringRenderer renderer;
        private readonly IProviderAdminChangesLogService institutionAdminChangesLogService;

        public InstitutionAdminService(
            IMapper mapper,
            IInstitutionAdminRepository institutionAdminRepository,
            ILogger<InstitutionAdminService> logger,
            IEmailSender emailSender,
            UserManager<User> userManager,
            OutOfSchoolDbContext context,
            IRazorViewToStringRenderer renderer)
        {
            this.mapper = mapper;
            this.userManager = userManager;
            this.context = context;
            this.institutionAdminRepository = institutionAdminRepository;
            this.logger = logger;
            this.emailSender = emailSender;
            this.renderer = renderer;
        }

        public async Task<ResponseDto> CreateInstitutionAdminAsync(
            CreateInstitutionAdminDto institutionAdminDto,
            IUrlHelper url,
            string userId,
            string requestId)
        {
            var user = mapper.Map<User>(institutionAdminDto);

            var password = PasswordGenerator
                .GenerateRandomPassword(userManager.Options.Password);

            var executionStrategy = context.Database.CreateExecutionStrategy();
            var result = await executionStrategy.Execute(async () =>
            {
                var response = new ResponseDto();
                await using var transaction = await context.Database.BeginTransactionAsync().ConfigureAwait(false);
                try
                {
                    user.IsDerived = true;
                    user.IsRegistered = true;
                    user.IsBlocked = false;
                    user.Role = nameof(Role.InstitutionAdmin).ToLower();

                    var result = await userManager.CreateAsync(user, password);

                    if (!result.Succeeded)
                    {
                        await transaction.RollbackAsync();

                        logger.LogError(
                            $"Error happened while creation InstitutionAdmin. Request(id): {requestId}" +
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

                    institutionAdminDto.UserId = user.Id;

                    var institutionAdmin = mapper.Map<InstitutionAdmin>(institutionAdminDto);
                    await institutionAdminRepository.Create(institutionAdmin)
                        .ConfigureAwait(false);

                    logger.LogInformation(
                        $"InstitutionAdmin(id):{institutionAdminDto.UserId} was successfully created by " +
                        $"User(id): {userId}. Request(id): {requestId}");

                    // TODO:
                    // Endpoint with sending new password

                    // TODO:
                    // Use template instead
                    var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
                    var confirmationLink = url.Action(
                        "EmailConfirmation", "Account",
                        new { userId = user.Id, token },
                        "https");
                    var subject = "Запрошення!";
                    var adminInvitationViewModel = new AdminInvitationViewModel
                    {
                        ConfirmationUrl = confirmationLink,
                        Email = user.Email,
                        Password = password,
                    };
                    var htmlMessage = await renderer.GetHtmlStringAsync(RazorTemplates.NewAdminInvitation, adminInvitationViewModel);

                    await emailSender.SendAsync(user.Email, subject, htmlMessage);

                    // No sense to commit if the email was not sent, as user will not be able to login
                    // and needs to be re-created
                    // TODO: +1 need Endpoint with sending new password
                    await transaction.CommitAsync();
                    response.IsSuccess = true;
                    response.HttpStatusCode = HttpStatusCode.OK;
                    response.Result = institutionAdminDto;

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
            });
            return result;
        }

        public async Task<ResponseDto> DeleteInstitutionAdminAsync(
            string institutionAdminId,
            string userId,
            string requestId)
        {
            var executionStrategy = context.Database.CreateExecutionStrategy();
            var result = await executionStrategy.Execute(async () =>
            {
                var response = new ResponseDto();
                await using var transaction = await context.Database.BeginTransactionAsync().ConfigureAwait(false);
                try
                {
                    var institutionAdmin = GetInstitutionAdmin(institutionAdminId);

                    if (institutionAdmin is null)
                    {
                        response.IsSuccess = false;
                        response.HttpStatusCode = HttpStatusCode.NotFound;

                        logger.LogError($"InstitutionAdmin(id) {institutionAdminId} not found. " +
                                        $"Request(id): {requestId}" +
                                        $"User(id): {userId}");

                        return response;
                    }

                    context.InstitutionAdmins.Remove(institutionAdmin);

                    var user = await userManager.FindByIdAsync(institutionAdminId);
                    var result = await userManager.DeleteAsync(user);

                    if (!result.Succeeded)
                    {
                        await transaction.RollbackAsync();

                        logger.LogError($"Error happened while deleting InstitutionAdmin. Request(id): {requestId}" +
                                        $"User(id): {userId}" +
                                        $"{string.Join(Environment.NewLine, result.Errors.Select(e => e.Description))}");

                        response.IsSuccess = false;
                        response.HttpStatusCode = HttpStatusCode.InternalServerError;

                        return response;
                    }

                    await transaction.CommitAsync();
                    response.IsSuccess = true;
                    response.HttpStatusCode = HttpStatusCode.OK;

                    logger.LogInformation($"InstitutionAdmin(id):{institutionAdminId} was successfully deleted by " +
                                          $"User(id): {userId}. Request(id): {requestId}");

                    return response;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    logger.LogError($"Error happened while deleting InstitutionAdmin. Request(id): {requestId}" +
                                    $"User(id): {userId} {ex.Message}");

                    response.IsSuccess = false;
                    response.HttpStatusCode = HttpStatusCode.InternalServerError;

                    return response;
                }
            });
            return result;
        }

        public async Task<ResponseDto> BlockInstitutionAdminAsync(
            string institutionAdminId,
            string userId,
            string requestId)
        {
            var response = new ResponseDto();

            var providerAdmin = GetInstitutionAdmin(institutionAdminId);

            if (providerAdmin is null)
            {
                response.IsSuccess = false;
                response.HttpStatusCode = HttpStatusCode.NotFound;

                logger.LogError($"ProviderAdmin(id) {institutionAdminId} not found. " +
                                $"Request(id): {requestId}" +
                                $"User(id): {userId}");

                return response;
            }

            var user = await userManager.FindByIdAsync(institutionAdminId);

            var executionStrategy = context.Database.CreateExecutionStrategy();
            return await executionStrategy.Execute(BlockProviderAdminOperation).ConfigureAwait(false);

            async Task<ResponseDto> BlockProviderAdminOperation()
            {
                await using var transaction = await context.Database.BeginTransactionAsync().ConfigureAwait(false);
                try
                {
                    user.IsBlocked = true;
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

                    logger.LogInformation($"ProviderAdmin(id):{institutionAdminId} was successfully blocked by " +
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

        private InstitutionAdmin GetInstitutionAdmin(string institutionAdminId)
            => context.InstitutionAdmins.SingleOrDefault(pa => pa.UserId == institutionAdminId);
    }
}
