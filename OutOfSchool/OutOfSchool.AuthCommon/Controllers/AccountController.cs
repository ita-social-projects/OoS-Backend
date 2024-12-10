using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement.Mvc;
using OutOfSchool.AuthCommon.Config;
using OutOfSchool.AuthCommon.Extensions;
using OutOfSchool.AuthCommon.ViewModels;
using OutOfSchool.EmailSender.Services;
using OutOfSchool.RazorTemplatesData.Models.Emails;

namespace OutOfSchool.AuthCommon.Controllers;

public class AccountController : Controller
{
    private const string ReturnUrl = "Login";
    private readonly SignInManager<User> signInManager;
    private readonly UserManager<User> userManager;
    private readonly IEmailSenderService emailSender;
    private readonly ILogger<AccountController> logger;
    private readonly IRazorViewToStringRenderer renderer;
    private readonly IStringLocalizer<SharedResource> localizer;
    private readonly AuthServerConfig identityServerConfig;
    private readonly IUserService userService;

    public AccountController(
        SignInManager<User> signInManager,
        UserManager<User> userManager,
        IEmailSenderService emailSender,
        ILogger<AccountController> logger,
        IRazorViewToStringRenderer renderer,
        IStringLocalizer<SharedResource> localizer,
        IOptions<AuthServerConfig> identityServerConfig,
        IUserService userService)
    {
        this.signInManager = signInManager;
        this.userManager = userManager;
        this.emailSender = emailSender;
        this.logger = logger;
        this.renderer = renderer;
        this.localizer = localizer;
        this.identityServerConfig = identityServerConfig.Value;
        this.userService = userService;
    }

    [HttpGet]
    [Authorize]
    [FeatureGate(AuthServerConstants.FeatureManagement.EmailManagement)]
    [Obsolete("Change email API is no longer supported. Exists only for testing purposes.")]
    public IActionResult ChangeEmail(string returnUrl = "Login")
    {
        return View("Email/ChangeEmail", new ChangeEmailViewModel() { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [Authorize]
    [FeatureGate(AuthServerConstants.FeatureManagement.EmailManagement)]
    [Obsolete("Change email API is no longer supported. Exists only for testing purposes.")]
    public async Task<IActionResult> ChangeEmail(ChangeEmailViewModel model)
    {
        var userId = User.GetUserPropertyByClaimType(IdentityResourceClaimsTypes.Sub) ?? "unlogged";
        var path = $"{HttpContext.Request.Path.Value}[{HttpContext.Request.Method}]";
        logger.LogDebug("{Path} started. User(id): {UserId}", path, userId);

        if (model.Submit == localizer["Cancel"])
        {
            if (!string.IsNullOrWhiteSpace(model.Email))
            {
                logger.LogInformation("{Path} Cancel click, but user enter the new email, show confirmation", path);

                return View("Email/CancelChangeEmail");
            }
            else
            {
                logger.LogInformation("{Path} Cancel click, close window", path);

                return new ContentResult()
                {
                    ContentType = "text/html",
                    Content = "<script>window.close();</script>",
                };
            }
        }

        if (!ModelState.IsValid)
        {
            logger.LogError(
                "{Path} Input data was not valid for User(id): {UserId}. Entered new Email: {Email}",
                path,
                userId,
                model.Email);

            return View("Email/ChangeEmail", new ChangeEmailViewModel());
        }

        if (model.CurrentEmail != User.Identity.Name.ToLower())
        {
            logger.LogError("{Path} Current Email mismatch. Entered current Email: {CurrentEmail}", path, model.CurrentEmail);

            model.Submit = "emailMismatch";
            return View("Email/ChangeEmail", model);
        }

        var userNewEmail = await userManager.FindByEmailAsync(model.Email);
        if (userNewEmail != null)
        {
            logger.LogError("{Path} Email already used. Entered new Email: {Email}", path, model.Email);

            model.Submit = "emailUsed";
            return View("Email/ChangeEmail", model);
        }

        var user = await userManager.FindByEmailAsync(User.Identity.Name);
        var token = await userManager.GenerateChangeEmailTokenAsync(user, model.Email);

        await SendConfirmEmailProcess(nameof(ConfirmChangeEmail), user, RazorTemplates.ChangeEmail, new { userId = user.Id, token, email = model.Email });

        return View("Email/SendChangeEmail", model);
    }

    [HttpGet]
    [FeatureGate(AuthServerConstants.FeatureManagement.EmailManagement)]
    [Obsolete("Change email API is no longer supported. Exists only for testing purposes.")]
    public async Task<IActionResult> ConfirmChangeEmail(string userId, string email, string token)
    {
        var path = $"{HttpContext.Request.Path.Value}[{HttpContext.Request.Method}]";
        logger.LogDebug("{Path} started. User(id): {UserId}", path, userId);

        if (userId == null || email == null || token == null)
        {
            logger.LogError("{Path} Parameters were not valid. User(id): {UserId}", path, userId);

            return BadRequest("One or more parameters are null.");
        }

        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            logger.LogError("{Path} User(id): {UserId} was not found", path, userId);

            return BadRequest("Verify information and try again.");
        }

        if (!await userManager.VerifyUserTokenAsync(user, userManager.Options.Tokens.ChangeEmailTokenProvider, UserManager<User>.GetChangeEmailTokenPurpose(email), token))
        {
            logger.LogError("Token was not valid");

            return BadRequest("Verify information and try again.");
        }

        var userByEmail = await userManager.FindByEmailAsync(email);
        if (userByEmail is not null)
        {
            logger.LogError("{Path} User(email): {email} was found", path, email);

            return View("Email/ConfirmEmailFailed", localizer[@"Email '{0}' is already used", email]);
        }

        var result = await userManager.ChangeEmailAsync(user, email, token);
        if (!result.Succeeded)
        {
            logger.LogError(
                "{Path} Changing email was failed for User(id): {UserId}. {Errors}",
                path,
                user.Id,
                result.ErrorMessages());

            return BadRequest();
        }

        var setUserNameResult = await userManager.SetUserNameAsync(user, email);
        if (!setUserNameResult.Succeeded)
        {
            logger.LogError(
                "{Path} Setting username was failed for User(id): {UserId}. {Errors}",
                path,
                userId,
                result.ErrorMessages());

            return BadRequest();
        }

        logger.LogInformation("{Path} Successfully logged. User(id): {UserId}", path, userId);

        await signInManager.RefreshSignInAsync(user);
        return View("Email/ConfirmChangeEmail");
    }

    [HttpGet]
    [Authorize]
    [FeatureGate(AuthServerConstants.FeatureManagement.EmailConfirmation)]
    [Obsolete("Confirm email API is no longer supported. Exists only for testing purposes.")]
    public async Task<IActionResult> ReSendEmailConfirmation()
    {
        var user = await userManager.FindByEmailAsync(User.Identity.Name);
        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var email = user.Email;
        var passedData = new { token, email, ReturnUrl };

        await SendConfirmEmailProcess(nameof(EmailConfirmation), user, RazorTemplates.ConfirmEmail, passedData);

        return View("Email/ConfirmEmail", new RegisterViewModel { Email = email, });
    }

    [HttpGet]
    [FeatureGate(AuthServerConstants.FeatureManagement.EmailConfirmation)]
    [Obsolete("Confirm email API is no longer supported. Exists only for testing purposes.")]
    public async Task<IActionResult> EmailConfirmation(string email, string token)
    {
        var path = $"{HttpContext.Request.Path.Value}[{HttpContext.Request.Method}]";
        logger.LogDebug("{Path} started. User(email): {Email}", path, email);

        if (email == null || token == null)
        {
            logger.LogError("{Path} Parameters were not valid. User(email): {Email}", path, email);

            return BadRequest("One or more parameters are null.");
        }

        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            logger.LogError("{Path} User with Email: {Email} was not found", path, email);

            // TODO: add nice page with redirect to register
            // TODO: saying you need to register first :)
            return NotFound("Wrong email");
        }

        var purpose = UserManager<User>.ConfirmEmailTokenPurpose;
        var checkToken = await userManager.VerifyUserTokenAsync(user, userManager.Options.Tokens.EmailConfirmationTokenProvider, purpose, token);
        if (!checkToken)
        {
            logger.LogError("{Path} Token is not valid for user: {UserId}", path, user.Id);

            return View("Email/ConfirmEmailFailed", localizer["Invalid email confirmation token"]);
        }

        var result = await userManager.ConfirmEmailAsync(user, token);
        if (!result.Succeeded)
        {
            logger.LogError(
                "{Path} Email сonfirmation was failed for User(id): {UserId}. {Errors}",
                path,
                user.Id,
                result.ErrorMessages());

            return BadRequest();
        }

        logger.LogInformation("{Path} Email was confirmed. User(id): {UserId}", path, user.Id);

        var redirectUrl = identityServerConfig.RedirectFromEmailConfirmationUrl;

        return string.IsNullOrEmpty(redirectUrl) ? Ok("Email confirmed.") : Redirect(redirectUrl);
    }

    [HttpGet]
    [FeatureGate(AuthServerConstants.FeatureManagement.PasswordManagement)]
    [Obsolete("Forgot password API is no longer supported. Exists only for testing purposes.")]
    public IActionResult ForgotPassword(string returnUrl = "Login")
    {
        return View("Password/ForgotPassword", new ForgotPasswordViewModel() { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [FeatureGate(AuthServerConstants.FeatureManagement.PasswordManagement)]
    [Obsolete("Forgot password API is no longer supported. Exists only for testing purposes.")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        var userId = User.GetUserPropertyByClaimType(IdentityResourceClaimsTypes.Sub) ?? "unlogged";
        var path = $"{HttpContext.Request.Path.Value}[{HttpContext.Request.Method}]";
        logger.LogDebug("{Path} started. User(id): {UserId}", path, userId);

        if (!ModelState.IsValid)
        {
            logger.LogError("{Path} Input data was not valid", path);

            return View("Password/ForgotPassword", new ForgotPasswordViewModel());
        }

        var user = await userManager.FindByEmailAsync(model.Email);

        if (user == null)
        {
            logger.LogError(
                "{Path} User with Email: {Email} was not found or Email was not confirmed",
                path,
                model.Email);
            ModelState.AddModelError(string.Empty, "Користувача з такою адресою не знайдено.");
            return View("Password/ForgotPassword", model);
        }

        if (!await userManager.IsEmailConfirmedAsync(user))
        {
            logger.LogError(
                "{Path} User with Email: {Email} was not found or Email was not confirmed",
                path,
                model.Email);
            ModelState.AddModelError(string.Empty, "Ця електронна адреса не підтверджена");
            return View("Password/ForgotPassword", model);
        }

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var callBackUrl = Url.Action("ResetPassword", ControllerContext.ActionDescriptor.ControllerName, new { token, user.Email }, Request.Scheme);

        var email = model.Email;
        var subject = localizer["Reset password confirmation"];
        var userActionViewModel = new UserActionViewModel
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            ActionUrl = callBackUrl,
        };

        var content = await renderer.GetHtmlPlainStringAsync(RazorTemplates.ResetPassword, userActionViewModel);
        await emailSender.SendAsync(email, subject, content);

        logger.LogInformation("{Path} Message to change password was sent. User(id): {UserId}", path, user.Id);

        return View("Password/ForgotPasswordConfirmation");
    }

    [HttpGet]
    [FeatureGate(AuthServerConstants.FeatureManagement.PasswordManagement)]
    [Obsolete("Reset password API is no longer supported. Exists only for testing purposes.")]
    public async Task<IActionResult> ResetPassword(string token = null, string email = null)
    {
        var userId = User.GetUserPropertyByClaimType(IdentityResourceClaimsTypes.Sub) ?? "unlogged";
        var path = $"{HttpContext.Request.Path.Value}[{HttpContext.Request.Method}]";
        logger.LogDebug("{Path} started. User(id): {UserId}", path, userId);

        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(email))
        {
            logger.LogError(
                "{Path} Token or email was not supplied for reset password. User(id): {UserId}",
                path,
                userId);
            return BadRequest("A token and email must be supplied for password reset.");
        }

        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            logger.LogError("{Path} User not found. Email: {Email}", path, email);

            // If message will be "user not found", someone can use this url to check registered emails. I decide to show "invalid token"
            return View("Password/ResetPasswordFailed", localizer["Change password invalid token"]);
        }

        var purpose = UserManager<User>.ResetPasswordTokenPurpose;
        var checkToken = await userManager.VerifyUserTokenAsync(user, userManager.Options.Tokens.PasswordResetTokenProvider, purpose, token);
        if (!checkToken)
        {
            logger.LogError("{Path} Token is not valid for user: {UserId}", path, user.Id);

            return View("Password/ResetPasswordFailed", localizer["Change password invalid token"]);
        }

        return View("Password/ResetPassword", new ResetPasswordViewModel() { Token = token, Email = email });
    }

    [HttpPost]
    [FeatureGate(AuthServerConstants.FeatureManagement.PasswordManagement)]
    [Obsolete("Reset password API is no longer supported. Exists only for testing purposes.")]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        var userId = User.GetUserPropertyByClaimType(IdentityResourceClaimsTypes.Sub) ?? "unlogged";
        var path = $"{HttpContext.Request.Path.Value}[{HttpContext.Request.Method}]";
        logger.LogDebug("{Path} started. User(id): {UserId}", path, userId);

        if (!ModelState.IsValid)
        {
            logger.LogError("{Path} Input data was not valid. User(id): {UserId}", path, userId);

            return View("Password/ResetPassword", new ResetPasswordViewModel());
        }

        var user = await userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            logger.LogError(
                "{Path} User with Email:{Email} was not found. User(id): {UserId}",
                path,
                model.Email,
                userId);

            return View("Password/ResetPasswordFailed", localizer["Change password failed"]);
        }

        var result = await userManager.ResetPasswordAsync(user, model.Token, model.Password);
        if (result.Succeeded)
        {
            logger.LogInformation("{Path} Password was successfully reset. User(id): {UserId}", path, user.Id);

            return View(
                "Password/ResetPasswordConfirmation",
                identityServerConfig.RedirectFromEmailConfirmationUrl);
        }

        logger.LogError(
            "{Path} Reset password was failed. User(id): {UserId}. {Errors}",
            path,
            userId,
            result.ErrorMessages());

        return View("Password/ResetPasswordFailed", localizer["Change password failed"]);
    }

    [HttpGet]
    [Authorize]
    [FeatureGate(AuthServerConstants.FeatureManagement.PasswordManagement)]
    [Obsolete("Change password API is no longer supported. Exists only for testing purposes.")]
    public IActionResult ChangePassword(string returnUrl = "Login")
    {
        return View("Password/ChangePassword", new ChangePasswordViewModel() { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [Authorize]
    [FeatureGate(AuthServerConstants.FeatureManagement.PasswordManagement)]
    [Obsolete("Change password API is no longer supported. Exists only for testing purposes.")]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        var userId = User.GetUserPropertyByClaimType(IdentityResourceClaimsTypes.Sub) ?? "unlogged";
        var path = $"{HttpContext.Request.Path.Value}[{HttpContext.Request.Method}]";
        logger.LogDebug("{Path} started. User(id): {UserId}", path, userId);

        if (!ModelState.IsValid)
        {
            logger.LogError("{Path} Input data was not valid. User(id): {UserId}", path, userId);

            return View("Password/ChangePassword");
        }

        var user = await userManager.GetUserAsync(User);
        var result = await userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
        if (result.Succeeded)
        {
            logger.LogInformation("{Path} Password was changed. User(id): {UserId}", path, userId);

            return View("Password/ChangePasswordConfirmation");
        }

        logger.LogError(
            "{Path} Changing password was failed for User(id): {UserId}. {Errors}",
            path,
            userId,
            result.ErrorMessages());

        ModelState.AddModelError(string.Empty, localizer["Change password failed"]);
        return View("Password/ChangePassword");
    }

    [HttpDelete("{userId}")]
    [Route("account/deleteuser/{userId}")]
    [HasPermission(Permissions.ProviderRemove)]
    public async Task<ResponseDto> LogOutUser(string userId)
    {
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var id))
        {
            return new ResponseDto() { HttpStatusCode = HttpStatusCode.BadRequest };
        }

        logger.LogInformation($"LogOut of user with Id = {id} started");

        return await userService.LogOutUserById(userId);
    }

    [Obsolete("Confirm email API is no longer supported. Exists only for testing purposes.")]
    private async Task<IActionResult> SendConfirmEmailProcess(string action, User user, string razorTemplate, object passedData)
    {
        logger.LogDebug("{0} started. User(id): {1}", ControllerContext.ActionDescriptor.ActionName, user.Id);

        var callBackUrl = Url.Action(action, ControllerContext.ActionDescriptor.ControllerName, passedData, Request.Scheme);

        var email = user.Email;
        var subject = localizer["Confirm email"];

        var userActionViewModel = new UserActionViewModel
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            ActionUrl = callBackUrl,
        };

        var content = await renderer.GetHtmlPlainStringAsync(razorTemplate, userActionViewModel);
        await emailSender.SendAsync(email, subject, content);

        logger.LogInformation("Confirmation message was sent. User(id): {UserId}", user.Id);

        return Ok();
    }
}