using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using AutoMapper;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using OutOfSchool.Common;
using OutOfSchool.Common.Models;
using OutOfSchool.EmailSender;
using OutOfSchool.IdentityServer.Services.Intefaces;
using OutOfSchool.IdentityServer.Services.Password;
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

        private readonly UserManager<User> userManager;
        private readonly OutOfSchoolDbContext context;
        private readonly ResponseDto response;

        public ProviderAdminService(
            IMapper mapper,
            IProviderAdminRepository providerAdminRepository,
            ILogger<ProviderAdminService> logger,
            IEmailSender emailSender,
            UserManager<User> userManager,
            OutOfSchoolDbContext context)
        {
            this.mapper = mapper;
            this.userManager = userManager;
            this.context = context;
            this.providerAdminRepository = providerAdminRepository;
            this.logger = logger;
            this.emailSender = emailSender;
            response = new ResponseDto();
        }

        public async Task<ResponseDto> CreateProviderAdminAsync(
            CreateProviderAdminDto providerAdminDto,
            HttpRequest request,
            IUrlHelper url,
            string path,
            string userId)
        {
            var user = mapper.Map<User>(providerAdminDto);

            var password = PasswordGenerator
                .GenerateRandomPassword(userManager.Options.Password);

            var executionStrategy = context.Database.CreateExecutionStrategy();
            await executionStrategy.Execute(async () =>
                {
                    using (var transaction = context.Database.BeginTransaction())
                    {
                        try
                        {
                            user.IsDerived = true;
                            user.IsEnabled = true;
                            user.Role = nameof(Role.Provider).ToLower();

                            IdentityResult result = await userManager.CreateAsync(user, password);

                            if (!result.Succeeded)
                            {
                                transaction.Rollback();

                                logger.LogError($"{path} Error happened while creation ProviderAdmin. Request(id): {request.Headers["X-Request-ID"]}" +
                                    $"User(id): {userId}" +
                                    $"{string.Join(System.Environment.NewLine, result.Errors.Select(e => e.Description))}");

                                response.IsSuccess = false;
                                response.HttpStatusCode = HttpStatusCode.InternalServerError;

                                return response;
                            }

                            var roleAssignResult = await userManager.AddToRoleAsync(user, user.Role);

                            if (!roleAssignResult.Succeeded)
                            {
                                transaction.Rollback();

                                logger.LogError($"{path} Error happened while adding role to user. Request(id): {request.Headers["X-Request-ID"]}" +
                                    $"User(id): {userId}" +
                                    $"{string.Join(System.Environment.NewLine, result.Errors.Select(e => e.Description))}");

                                response.IsSuccess = false;
                                response.HttpStatusCode = HttpStatusCode.InternalServerError;

                                return response;
                            }

                            providerAdminDto.UserId = user.Id;

                            var providerAdmin = mapper.Map<ProviderAdmin>(providerAdminDto);
                            providerAdmin.ManagedWorkshops = !providerAdmin.IsDeputy
                                 ?
                                 context.Workshops.Where(w => providerAdminDto.ManagedWorkshopIds.Contains(w.Id)).ToList()
                                 :

                                 // we create empty list, because deputy are not connected with each workshop, but to all related to provider
                                 new List<Workshop>();
                            if (!providerAdmin.IsDeputy && !providerAdmin.ManagedWorkshops.Any())
                            {
                                transaction.Rollback();
                                logger.LogError($"Cant create assistant provider admin without related workshops");
                                response.IsSuccess = false;
                                response.HttpStatusCode = HttpStatusCode.BadRequest;
                                response.Message = "You have to specify related workshops to be able to create workshop admin";

                                return response;
                            }

                            await providerAdminRepository.Create(providerAdmin)
                                    .ConfigureAwait(false);

                            response.IsSuccess = true;
                            response.Result = providerAdminDto;

                            transaction.Commit();

                            logger.LogInformation($"ProviderAdmin(id):{providerAdminDto.UserId} was successfully created by " +
                                $"User(id): {userId}. Request(id): {request.Headers["X-Request-ID"]}");

                            // TODO:
                            // Endpoint with sending new password

                            // TODO:
                            // Use template instead
                            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
                            var confirmationLink = url.Action("EmailConfirmation", "Account", new { userId = user.Id, token = token }, request.Scheme);
                            var subject = "Запрошення!";
                            var htmlMessage = $"Для реєстрації на платформі перейдіть " +
                                $"за посиланням та заповність ваші данні в особистому кабінеті.<br>" +
                                $"{confirmationLink}<br><br>" +
                                $"Логін: {user.Email}<br>" +
                                $"Пароль: {password}";

                            await emailSender.SendAsync(user.Email, subject, htmlMessage);

                            return response;
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();

                            logger.LogError($"{path} {ex.Message} User(id): {userId}.");

                            response.IsSuccess = false;
                            response.HttpStatusCode = HttpStatusCode.InternalServerError;
                            return response;
                        }
                    }
                });
            return response;
        }

        public async Task<ResponseDto> DeleteProviderAdminAsync(
            DeleteProviderAdminDto deleteProviderAdminDto,
            HttpRequest request,
            string path,
            string userId)
        {
            var executionStrategy = context.Database.CreateExecutionStrategy();
            await executionStrategy.Execute(async () =>
            {
                using (var transaction = context.Database.BeginTransaction())
                {
                    try
                    {
                        var providerAdmin = context.ProviderAdmins
                            .SingleOrDefault(pa => pa.UserId == deleteProviderAdminDto.ProviderAdminId);

                        if (providerAdmin is null)
                        {
                            response.IsSuccess = false;
                            response.HttpStatusCode = HttpStatusCode.NotFound;

                            logger.LogError($"{path} ProviderAdmin(id) {deleteProviderAdminDto.ProviderAdminId} not found. " +
                                $"Request(id): {request.Headers["X-Request-ID"]}" +
                                    $"User(id): {userId}");
                        }

                        context.ProviderAdmins.Remove(providerAdmin);

                        var user = await userManager.FindByIdAsync(deleteProviderAdminDto.ProviderAdminId);
                        var result = await userManager.DeleteAsync(user);

                        if (!result.Succeeded)
                        {
                            transaction.Rollback();

                            logger.LogError($"{path} Error happened while deleting ProviderAdmin. Request(id): {request.Headers["X-Request-ID"]}" +
                                $"User(id): {userId}" +
                                $"{string.Join(System.Environment.NewLine, result.Errors.Select(e => e.Description))}");

                            response.IsSuccess = false;
                            response.HttpStatusCode = HttpStatusCode.InternalServerError;

                            return response;
                        }

                        response.IsSuccess = true;
                        response.HttpStatusCode = HttpStatusCode.OK;

                        transaction.Commit();

                        logger.LogInformation($"ProviderAdmin(id):{deleteProviderAdminDto.ProviderAdminId} was successfully deleted by " +
                            $"User(id): {userId}. Request(id): {request.Headers["X-Request-ID"]}");

                        return response;
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();

                        logger.LogError($"{path} Error happened while deleting ProviderAdmin. Request(id): {request.Headers["X-Request-ID"]}" +
                                $"User(id): {userId} {ex.Message}");

                        response.IsSuccess = false;
                        response.HttpStatusCode = HttpStatusCode.InternalServerError;

                        return response;
                    }
                }
            });
            return response;
        }

        public async Task<ResponseDto> BlockProviderAdminAsync(
            BlockProviderAdminDto blockProviderAdminDto,
            HttpRequest request,
            string path,
            string userId)
        {
            var user = await userManager.FindByIdAsync(blockProviderAdminDto.ProviderAdminId);

            if (user is null)
            {
                response.IsSuccess = false;
                response.HttpStatusCode = HttpStatusCode.NotFound;

                logger.LogError($"{path} ProviderAdmin(id) {blockProviderAdminDto.ProviderAdminId} not found. " +
                            $"Request(id): {request.Headers["X-Request-ID"]}" +
                                $"User(id): {userId}");
            }

            user.IsEnabled = false;
            var updateResult = await userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                logger.LogError($"{path} Error happened while blocking ProviderAdmin. Request(id): {request.Headers["X-Request-ID"]}" +
                            $"User(id): {userId}" +
                            $"{string.Join(System.Environment.NewLine, updateResult.Errors.Select(e => e.Description))}");

                response.IsSuccess = false;
                response.HttpStatusCode = HttpStatusCode.InternalServerError;

                return response;
            }

            var updateSecurityStamp = await userManager.UpdateSecurityStampAsync(user);

            if (!updateResult.Succeeded)
            {
                logger.LogError($"{path} Error happened while updating security stamp. ProviderAdmin. Request(id): {request.Headers["X-Request-ID"]}" +
                            $"User(id): {userId}" +
                            $"{string.Join(System.Environment.NewLine, updateResult.Errors.Select(e => e.Description))}");

                response.IsSuccess = false;
                response.HttpStatusCode = HttpStatusCode.InternalServerError;

                return response;
            }

            logger.LogInformation($"ProviderAdmin(id):{blockProviderAdminDto.ProviderAdminId} was successfully blocked by " +
                        $"User(id): {userId}. Request(id): {request.Headers["X-Request-ID"]}");

            response.IsSuccess = true;
            response.HttpStatusCode = HttpStatusCode.OK;

            return response;
        }
    }
}
