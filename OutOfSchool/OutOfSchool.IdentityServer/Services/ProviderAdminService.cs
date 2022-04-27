﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OutOfSchool.Common;
using OutOfSchool.Common.Models;
using OutOfSchool.EmailSender;
using OutOfSchool.IdentityServer.Config;
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
    public class ProviderAdminService : IProviderAdminService
    {
        private readonly IEmailSender emailSender;
        private readonly IMapper mapper;
        private readonly ILogger<ProviderAdminService> logger;
        private readonly IProviderAdminRepository providerAdminRepository;
        private readonly GRPCConfig gPRCConfig;

        private readonly UserManager<User> userManager;
        private readonly OutOfSchoolDbContext context;
        private readonly IRazorViewToStringRenderer renderer;
        private readonly IProviderAdminChangesLogService providerAdminChangesLogService;

        public ProviderAdminService(
            IMapper mapper,
            IProviderAdminRepository providerAdminRepository,
            ILogger<ProviderAdminService> logger,
            IEmailSender emailSender,
            UserManager<User> userManager,
            OutOfSchoolDbContext context,
            IRazorViewToStringRenderer renderer,
            IProviderAdminChangesLogService providerAdminChangesLogService,
            IOptions<GRPCConfig> gRPCConfig)
        {
            this.mapper = mapper;
            this.userManager = userManager;
            this.context = context;
            this.providerAdminRepository = providerAdminRepository;
            this.logger = logger;
            this.emailSender = emailSender;
            this.renderer = renderer;
            this.providerAdminChangesLogService = providerAdminChangesLogService;
            this.gPRCConfig = gRPCConfig.Value;
        }

        public async Task<ResponseDto> CreateProviderAdminAsync(
            CreateProviderAdminDto providerAdminDto,
            IUrlHelper url,
            string userId,
            string requestId)
        {
            var user = mapper.Map<User>(providerAdminDto);

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
                    user.Role = nameof(Role.Provider).ToLower();

                    var result = await userManager.CreateAsync(user, password);

                    if (!result.Succeeded)
                    {
                        await transaction.RollbackAsync();

                        logger.LogError(
                            $"Error happened while creation ProviderAdmin. Request(id): {requestId}" +
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

                    providerAdminDto.UserId = user.Id;

                    var providerAdmin = mapper.Map<ProviderAdmin>(providerAdminDto);
                    providerAdmin.ManagedWorkshops = !providerAdmin.IsDeputy
                        ? context.Workshops.Where(w => providerAdminDto.ManagedWorkshopIds.Contains(w.Id))
                            .ToList()
                        :

                        // we create empty list, because deputy are not connected with each workshop, but to all related to provider
                        new List<Workshop>();
                    if (!providerAdmin.IsDeputy && !providerAdmin.ManagedWorkshops.Any())
                    {
                        await transaction.RollbackAsync();
                        logger.LogError($"Cant create assistant provider admin without related workshops");
                        response.IsSuccess = false;
                        response.HttpStatusCode = HttpStatusCode.BadRequest;
                        response.Message =
                            "You have to specify related workshops to be able to create workshop admin";

                        return response;
                    }

                    await providerAdminRepository.Create(providerAdmin)
                        .ConfigureAwait(false);

                    await providerAdminChangesLogService.SaveChangesLogAsync(providerAdmin, userId, OperationType.Create)
                        .ConfigureAwait(false);

                    logger.LogInformation(
                        $"ProviderAdmin(id):{providerAdminDto.UserId} was successfully created by " +
                        $"User(id): {userId}. Request(id): {requestId}");

                    // TODO:
                    // Endpoint with sending new password

                    // TODO:
                    // Use template instead
                    var token = await userManager.GenerateEmailConfirmationTokenAsync(user);

                    string confirmationLink =
                        url is null
                        ? $"{gPRCConfig.ProviderAdminConfirmationLink}?userId={user.Id}&token={token}"
                        : url.Action(
                                "EmailConfirmation",
                                "Account",
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
                    response.Result = providerAdminDto;

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

        public async Task<ResponseDto> DeleteProviderAdminAsync(
            string providerAdminId,
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
                    var providerAdmin = GetProviderAdmin(providerAdminId);

                    if (providerAdmin is null)
                    {
                        response.IsSuccess = false;
                        response.HttpStatusCode = HttpStatusCode.NotFound;

                        logger.LogError($"ProviderAdmin(id) {providerAdminId} not found. " +
                                        $"Request(id): {requestId}" +
                                        $"User(id): {userId}");

                        return response;
                    }

                    context.ProviderAdmins.Remove(providerAdmin);

                    var user = await userManager.FindByIdAsync(providerAdminId);
                    var result = await userManager.DeleteAsync(user);

                    if (!result.Succeeded)
                    {
                        await transaction.RollbackAsync();

                        logger.LogError($"Error happened while deleting ProviderAdmin. Request(id): {requestId}" +
                                        $"User(id): {userId}" +
                                        $"{string.Join(Environment.NewLine, result.Errors.Select(e => e.Description))}");

                        response.IsSuccess = false;
                        response.HttpStatusCode = HttpStatusCode.InternalServerError;

                        return response;
                    }

                    // TODO: enable logging of changes when soft delete for Users is added
                    if (false)
                    {
                        await providerAdminChangesLogService.SaveChangesLogAsync(providerAdmin, userId, OperationType.Delete)
                            .ConfigureAwait(false);
                    }

                    await transaction.CommitAsync();
                    response.IsSuccess = true;
                    response.HttpStatusCode = HttpStatusCode.OK;

                    logger.LogInformation($"ProviderAdmin(id):{providerAdminId} was successfully deleted by " +
                                          $"User(id): {userId}. Request(id): {requestId}");

                    return response;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();

                    logger.LogError($"Error happened while deleting ProviderAdmin. Request(id): {requestId}" +
                                    $"User(id): {userId} {ex.Message}");

                    response.IsSuccess = false;
                    response.HttpStatusCode = HttpStatusCode.InternalServerError;

                    return response;
                }
            });
            return result;
        }

        public async Task<ResponseDto> BlockProviderAdminAsync(
            string providerAdminId,
            string userId,
            string requestId)
        {
            var response = new ResponseDto();

            var providerAdmin = GetProviderAdmin(providerAdminId);

            if (providerAdmin is null)
            {
                response.IsSuccess = false;
                response.HttpStatusCode = HttpStatusCode.NotFound;

                logger.LogError($"ProviderAdmin(id) {providerAdminId} not found. " +
                                $"Request(id): {requestId}" +
                                $"User(id): {userId}");

                return response;
            }

            var user = await userManager.FindByIdAsync(providerAdminId);

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

                    await providerAdminChangesLogService.SaveChangesLogAsync(providerAdmin, userId, OperationType.Block)
                        .ConfigureAwait(false);

                    await transaction.CommitAsync().ConfigureAwait(false);

                    logger.LogInformation($"ProviderAdmin(id):{providerAdminId} was successfully blocked by " +
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

        private ProviderAdmin GetProviderAdmin(string providerAdminId)
            => context.ProviderAdmins.SingleOrDefault(pa => pa.UserId == providerAdminId);
    }
}
