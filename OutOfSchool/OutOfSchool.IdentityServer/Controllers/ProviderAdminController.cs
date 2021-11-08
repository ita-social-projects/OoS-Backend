using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using OutOfSchool.Common;
using OutOfSchool.Common.Models;
using OutOfSchool.EmailSender;
using OutOfSchool.IdentityServer.Services;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;

namespace OutOfSchool.IdentityServer.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class ProviderAdminController : ControllerBase
    {
        private readonly UserManager<User> userManager;
        private readonly IProviderAdminRepository providerAdminRepository;
        private readonly IEmailSender emailSender;
        private ResponseDto response;

        public ProviderAdminController(
            UserManager<User> userManager,
            IProviderAdminRepository providerAdminRepository,
            IEmailSender emailSender)
        {
            this.userManager = userManager;
            this.response = new ResponseDto();
            this.providerAdminRepository = providerAdminRepository;
            this.emailSender = emailSender;
        }

        [HttpPost]
        [Authorize(Roles = "provider, provideradmin")]
        public async Task<ResponseDto> Create(ProviderAdminDto providerAdminDto)
        {
            // TODO:
            // Let's think about expediency of using mapper here.
            var user = new User()
            {
                UserName = providerAdminDto.Email,
                Email = providerAdminDto.Email,
                PhoneNumber = providerAdminDto.PhoneNumber,
                Role = providerAdminDto.Role,
                FirstName = providerAdminDto.FirstName,
                LastName = providerAdminDto.LastName,
                MiddleName = providerAdminDto.MiddleName,
                CreatingTime = DateTimeOffset.UtcNow,
                IsRegistered = false,
            };

            var password = PasswordGenerator
                .GenerateRandomPassword(userManager.Options.Password);

            try
            {
                IdentityResult result = await userManager.CreateAsync(user, password);

                if (result.Succeeded)
                {
                    IdentityResult roleAssignResult = await userManager.AddToRoleAsync(user, user.Role);

                    if (roleAssignResult.Succeeded)
                    {
                        var adminprovider = new ProviderAdmin()
                        {
                            UserId = user.Id,
                            ProviderId = providerAdminDto.ProviderId,
                            CityId = providerAdminDto.CityId,
                        };

                        await providerAdminRepository.Create(adminprovider)
                            .ConfigureAwait(false);

                        providerAdminDto.UserId = user.Id;

                        response.IsSuccess = true;
                        response.Result = providerAdminDto;

                        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
                        var confirmationLink = Url.Action("EmailConfirmation", "Account", new { userId = user.Id, token = token }, Request.Scheme);
                        var subject = "Запрошення!";
                        var htmlMessage = $"Для реєстрації на платформі перейдіть " +
                            $"за посиланням та заповність ваші данні в особистому кабінеті.<br>" +
                            $"{confirmationLink}<br><br>" +
                            $"Логін: {user.Email}<br>" +
                            $"Пароль: {password}";

                        await emailSender.SendAsync(user.Email, subject, htmlMessage);

                        return response;
                    }

                    var deletionResult = await userManager.DeleteAsync(user);

                    if (!deletionResult.Succeeded)
                    {
                        // Log error here (user create without role) 
                    }

                    response.IsSuccess = false;
                    response.ErrorMessages =
                            roleAssignResult.Errors.Select(error => error.Description);

                    return response;
                }

                response.IsSuccess = false;
                response.ErrorMessages =
                        result.Errors.Select(error => error.Description);

                return response;
            }
            catch (Exception ex)
            {
                await userManager.RemoveFromRoleAsync(user, user.Role);
                await userManager.DeleteAsync(user);

                // await providerAdminRepository.DeleteAsync(provider);
                // Log errors

                response.IsSuccess = false;
                response.ErrorMessages =
                    new List<string> { ex.Message };

                return response;
            }
        }
    }
}