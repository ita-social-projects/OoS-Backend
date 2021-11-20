using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using AutoMapper;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using OutOfSchool.Common;
using OutOfSchool.Common.Models;
using OutOfSchool.EmailSender;
using OutOfSchool.IdentityServer.Services.Intefaces;
using OutOfSchool.IdentityServer.Services.Password;
using OutOfSchool.Services;
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
            ProviderAdminDto providerAdminDto,
            HttpRequest request,
            IUrlHelper url,
            string path,
            string userId)
        {
            var user = mapper.Map<User>(providerAdminDto);

            var password = PasswordGenerator
                .GenerateRandomPassword(userManager.Options.Password);

            using (var transaction = context.Database.BeginTransaction())
            {
                try
                {
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
        }

        public async Task<ResponseDto> DeleteProviderAdminAsync(
            DeleteProviderAdminDto deleteProviderAdminDto,
            HttpRequest request,
            string path,
            string userId)
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
        }
    }
}
