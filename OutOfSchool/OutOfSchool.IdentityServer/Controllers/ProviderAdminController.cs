using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

using OutOfSchool.Common;
using OutOfSchool.Common.Extensions;
using OutOfSchool.Common.Models;
using OutOfSchool.EmailSender;
using OutOfSchool.IdentityServer.Services;
using OutOfSchool.Services;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;

namespace OutOfSchool.IdentityServer.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class ProviderAdminController : Controller
    {
        private readonly UserManager<User> userManager;
        private readonly IProviderAdminRepository providerAdminRepository;
        private readonly IEmailSender emailSender;
        private readonly ILogger<ProviderAdminController> logger;
        private readonly OutOfSchoolDbContext db;
        private ResponseDto response;
        private string path;
        private string userId;

        public ProviderAdminController(
            UserManager<User> userManager,
            IProviderAdminRepository providerAdminRepository,
            IEmailSender emailSender,
            ILogger<ProviderAdminController> logger,
            OutOfSchoolDbContext db)
        {
            this.userManager = userManager;
            this.response = new ResponseDto();
            this.providerAdminRepository = providerAdminRepository;
            this.db = db;
            this.emailSender = emailSender;
            this.logger = logger;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            path = $"{context.HttpContext.Request.Path.Value}[{context.HttpContext.Request.Method}]";
            userId = User.GetUserPropertyByClaimType(IdentityResourceClaimsTypes.Sub);
        }

        [HttpPost]
        [Authorize(Roles = "provider, provideradmin")]
        public async Task<ResponseDto> Create(ProviderAdminDto providerAdminDto)
        {
            logger.LogDebug($"Received request " +
                $"{Request.Headers["X-Request-ID"]}. {path} started. User(id): {userId}");

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

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    IdentityResult result = await userManager.CreateAsync(user, password);

                    if (!result.Succeeded)
                    {
                        transaction.Rollback();

                        logger.LogError($"{path} Error happened while creation ProviderAdmin. Request(id): {Request.Headers["X-Request-ID"]}" +
                            $"User(id): {userId}" +
                            $"{string.Join(System.Environment.NewLine, result.Errors.Select(e => e.Description))}");

                        return new ResponseDto
                        {
                            IsSuccess = false,
                            HttpStatusCode = HttpStatusCode.InternalServerError,
                        };
                    }

                    var roleAssignResult = await userManager.AddToRoleAsync(user, user.Role);

                    if (!roleAssignResult.Succeeded)
                    {
                        transaction.Rollback();

                        logger.LogError($"{path} Error happened while adding role to user. Request(id): {Request.Headers["X-Request-ID"]}" +
                            $"User(id): {userId}" +
                            $"{string.Join(System.Environment.NewLine, result.Errors.Select(e => e.Description))}");

                        return new ResponseDto
                        {
                            IsSuccess = false,
                            HttpStatusCode = HttpStatusCode.InternalServerError,
                        };
                    }

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

                    transaction.Commit();

                    // TODO:
                    // Use template instead
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
                catch (Exception ex)
                {
                    transaction.Rollback();

                    logger.LogError($"{path} User(id): {userId}. {ex.Message}");

                    return new ResponseDto
                    {
                        IsSuccess = false,
                        HttpStatusCode = HttpStatusCode.InternalServerError,
                    };
                }
            }
        }
    }
}