using System;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OutOfSchool.Common;
using OutOfSchool.Common.Extensions;
using OutOfSchool.EmailSender;
using OutOfSchool.IdentityServer.ViewModels;
using OutOfSchool.RazorTemplatesData.Models.Emails;
using OutOfSchool.RazorTemplatesData.Services;
using OutOfSchool.Services.Models;

namespace OutOfSchool.IdentityServer.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<User> signInManager;
    private readonly UserManager<User> userManager;
    private readonly IEmailSender emailSender;
    private readonly ILogger<AccountController> logger;
    private readonly IRazorViewToStringRenderer renderer;
    private readonly IStringLocalizer<SharedResource> localizer;
    private string userId;
    private string path;

    public AccountController(
        SignInManager<User> signInManager,
        UserManager<User> userManager,
        IEmailSender emailSender,
        ILogger<AccountController> logger,
        IRazorViewToStringRenderer renderer,
        IStringLocalizer<SharedResource> localizer)
    {
        this.signInManager = signInManager;
        this.userManager = userManager;
        this.emailSender = emailSender;
        this.logger = logger;
        this.localizer = localizer;
        this.renderer = renderer;
        this.localizer = localizer;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        userId = User.GetUserPropertyByClaimType(IdentityResourceClaimsTypes.Sub) ?? "unlogged";
        path = $"{context.HttpContext.Request.Path.Value}[{context.HttpContext.Request.Method}]";
    }

    [HttpGet]
    [Authorize]
    public IActionResult ChangeEmail(string returnUrl = "Login")
    {
        return View("Email/ChangeEmail", new ChangeEmailViewModel() { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> ChangeEmail(ChangeEmailViewModel model)
    {
        logger.LogDebug($"{path} started. User(id): {userId}.");

        if (model.Submit == localizer["Cancel"])
        {
            if (!string.IsNullOrWhiteSpace(model.Email))
            {
                logger.LogInformation($"{path} Cancel click, but user enter the new email, show confirmation.");

                return View("Email/CancelChangeEmail");
            }
            else
            {
                logger.LogInformation($"{path} Cancel click, close window.");

                return new ContentResult()
                {
                    ContentType = "text/html",
                    Content = "<script>window.close();</script>",
                };
            }
        }

        if (!ModelState.IsValid)
        {
            logger.LogError($"{path} Input data was not valid for User(id): {userId}. " +
                            $"Entered new Email: {model.Email}");

            return View("Email/ChangeEmail", new ChangeEmailViewModel());
        }

        if (model.CurrentEmail != User.Identity.Name.ToLower())
        {
            logger.LogError($"{path} Current Email mismatch. Entered current Email: {model.CurrentEmail}");

            model.Submit = "emailMismatch";
            return View("Email/ChangeEmail", model);
        }

        var userNewEmail = await userManager.FindByEmailAsync(model.Email);
        if (userNewEmail != null)
        {
            logger.LogError($"{path} Email already used. Entered new Email: {model.Email}");

            model.Submit = "emailUsed";
            return View("Email/ChangeEmail", model);
        }

        var user = await userManager.FindByEmailAsync(User.Identity.Name);
        var token = await userManager.GenerateChangeEmailTokenAsync(user, model.Email);
        var callBackUrl = Url.Action(nameof(ConfirmChangeEmail), "Account", new { userId = user.Id, email = model.Email, token }, Request.Scheme);

        var email = model.Email;
        var subject = "Confirm email.";

        var userActionViewModel = new UserActionViewModel
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            ActionUrl = HtmlEncoder.Default.Encode(callBackUrl),
        };
        var htmlMessage = await renderer.GetHtmlStringAsync(RazorTemplates.ChangeEmail, userActionViewModel);

        await emailSender.SendAsync(email, subject, htmlMessage);

        logger.LogInformation($"{path} Confirmation message was sent for User(id) + {userId}.");

        return View("Email/SendChangeEmail", model);
    }

    [HttpGet]
    public async Task<IActionResult> ConfirmChangeEmail(string userId, string email, string token)
    {
        logger.LogDebug($"{path} started. User(id): {userId}.");

        if (userId == null || email == null || token == null)
        {
            logger.LogError($"{path} Parameters were not valid. User(id): {userId}.");

            return BadRequest("One or more parameters are null.");
        }

        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            logger.LogError($"{path} User(id): {userId} was not found.");

            return NotFound($"Changing email for user with ID: '{userId}' was not allowed.");
        }

        var result = await userManager.ChangeEmailAsync(user, email, token);
        if (!result.Succeeded)
        {
            logger.LogError($"{path} Changing email was failed for User(id): {user.Id}. " +
                            $"{string.Join(System.Environment.NewLine, result.Errors.Select(e => e.Description))}");

            return BadRequest();
        }

        var setUserNameResult = await userManager.SetUserNameAsync(user, email);
        if (!setUserNameResult.Succeeded)
        {
            logger.LogError($"{path} Setting username was failed for User(id): {userId}. " +
                            $"{string.Join(System.Environment.NewLine, result.Errors.Select(e => e.Description))}");

            return BadRequest();
        }

        logger.LogInformation($"{path} Successfully logged. User(id): {userId}");

        await signInManager.RefreshSignInAsync(user);
        return View("Email/ConfirmChangeEmail");
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> ConfirmEmail()
    {
        logger.LogDebug($"{path} started. User(id): {userId}.");

        var user = await userManager.FindByEmailAsync(User.Identity.Name);
        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var callBackUrl = Url.Action(nameof(EmailConfirmation), "Account", new { userId = user.Id, token }, Request.Scheme);

        var email = user.Email;
        var subject = "Confirm email.";

        var userActionViewModel = new UserActionViewModel
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            ActionUrl = HtmlEncoder.Default.Encode(callBackUrl),
        };
        var htmlMessage = await renderer.GetHtmlStringAsync(RazorTemplates.ConfirmEmail, userActionViewModel);

        await emailSender.SendAsync(email, subject, htmlMessage);

        logger.LogInformation($"Confirmation message was sent. User(id): {userId}.");

        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> EmailConfirmation(string userId, string token, string redirectUrl = null)
    {
        logger.LogDebug($"{path} started. User(id): {userId}.");

        if (userId == null || token == null)
        {
            logger.LogError($"{path} Parameters were not valid. User(id): {userId}.");

            return BadRequest("One or more parameters are null.");
        }

        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            logger.LogError($"{path} User with UserId: {userId} was not found.");

            return NotFound($"Unable to load user with ID: '{userId}'.");
        }

        var result = await userManager.ConfirmEmailAsync(user, token);
        if (!result.Succeeded)
        {
            logger.LogError($"{path} Email сonfirmation  was failed for User(id): {userId} " +
                            $"{string.Join(System.Environment.NewLine, result.Errors.Select(e => e.Description))}");

            return BadRequest();
        }

        logger.LogInformation($"{path} Email was confirmed. User(id): {userId}.");

        return string.IsNullOrEmpty(redirectUrl) ? Ok() : Redirect(redirectUrl);
    }

    [HttpGet]
    public IActionResult ForgotPassword(string returnUrl = "Login")
    {
        return View("Password/ForgotPassword", new ForgotPasswordViewModel() { ReturnUrl = returnUrl });
    }

    [HttpPost]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        logger.LogDebug($"{path} started. User(id): {userId}");

        if (!ModelState.IsValid)
        {
            logger.LogError($"{path} Input data was not valid.");

            return View("Password/ForgotPassword", new ForgotPasswordViewModel());
        }

        var user = await userManager.FindByEmailAsync(model.Email);
        if (user == null || !(await userManager.IsEmailConfirmedAsync(user)))
        {
            logger.LogError($"{path} User with Email: {model.Email} was not found or Email was not confirmed.");

            return View("Password/ForgotPasswordConfirmation");
        }

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var callBackUrl = Url.Action("ResetPassword", "Account", new { token, user.Email }, Request.Scheme);

        var email = model.Email;
        var subject = "Reset Password";
        var userActionViewModel = new UserActionViewModel
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            ActionUrl = callBackUrl,
        };

        var htmlMessage = await renderer.GetHtmlStringAsync(RazorTemplates.ResetPassword, userActionViewModel);

        await emailSender.SendAsync(email, subject, htmlMessage);

        logger.LogInformation($"{path} Message to change password was sent. User(id): {user.Id}.");

        return View("Password/ForgotPasswordConfirmation");
    }

    [HttpGet]
    public async Task<IActionResult> ResetPassword(string token = null, string email = null)
    {
        logger.LogDebug($"{path} started. User(id): {userId}");

        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(email))
        {
            logger.LogError($"{path} Token or email was not supplied for reset password. User(id): {userId}");
            return BadRequest("A token and email must be supplied for password reset.");
        }

        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            logger.LogError($"{path} User not found. Email: {email}");

            // If message will be "user not found", someone can use this url to check registered emails. I decide to show "invalid token"
            return View("Password/ResetPasswordFailed", localizer["Change password invalid token"]);
        }

        var purpose = UserManager<User>.ResetPasswordTokenPurpose;
        var checkToken = await userManager.VerifyUserTokenAsync(user, userManager.Options.Tokens.PasswordResetTokenProvider, purpose, token);
        if (!checkToken)
        {
            logger.LogError($"{path} Token is not valid for user: {user.Id}");

            return View("Password/ResetPasswordFailed", localizer["Change password invalid token"]);
        }

        return View("Password/ResetPassword", new ResetPasswordViewModel() { Token = token, Email = email });
    }

    [HttpPost]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        logger.LogDebug($"{path} started. User(id): {userId}");

        if (!ModelState.IsValid)
        {
            logger.LogError($"{path} Input data was not valid. User(id): {userId}");

            return View("Password/ResetPassword", new ResetPasswordViewModel());
        }

        var user = await userManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            logger.LogError($"{path} User with Email:{model.Email} was not found. User(id): {userId}");

            return View("Password/ResetPasswordFailed", localizer["Change password failed"]);
        }

        var result = await userManager.ResetPasswordAsync(user, model.Token, model.Password);
        if (result.Succeeded)
        {
            logger.LogInformation($"{path} Password was successfully reseted. User(id): {user.Id}");

            return View("Password/ResetPasswordConfirmation");
        }

        logger.LogError($"{path} Reset password was failed. User(id): {userId}. " +
                        $"{string.Join(System.Environment.NewLine, result.Errors.Select(e => e.Description))}");

        return View("Password/ResetPasswordFailed", localizer["Change password failed"]);
    }

    [HttpGet]
    [Authorize]
    public IActionResult ChangePassword(string returnUrl = "Login")
    {
        return View("Password/ChangePassword", new ChangePasswordViewModel() { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        logger.LogDebug($"{path} started. User(id): {userId}.");

        if (!ModelState.IsValid)
        {
            logger.LogError($"{path} Input data was not valid. User(id): {userId}.");

            return BadRequest(ModelState);
        }

        var user = await userManager.GetUserAsync(User);
        var result = await userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
        if (result.Succeeded)
        {
            logger.LogInformation($"{path} Password was changed. User(id): {userId}.");

            return View("Password/ChangePasswordConfirmation");
        }

        logger.LogError($"{path} Changing password was failed for User(id): {userId}." +
                        $"{string.Join(System.Environment.NewLine, result.Errors.Select(e => e.Description))}");

        return Redirect(model.ReturnUrl);
    }
}