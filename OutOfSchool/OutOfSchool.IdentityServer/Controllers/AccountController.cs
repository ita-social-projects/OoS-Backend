using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OutOfSchool.EmailSender;
using OutOfSchool.IdentityServer.ViewModels;
using OutOfSchool.Services.Models;

namespace OutOfSchool.IdentityServer.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<User> signInManager;
        private readonly UserManager<User> userManager;
        private readonly IEmailSender emailSender;
        private readonly ILogger<AccountController> logger;

        public AccountController(
            SignInManager<User> signInManager,
            UserManager<User> userManager,
            IEmailSender emailSender,
            ILogger<AccountController> logger)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.emailSender = emailSender;
            this.logger = logger;
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
            logger.LogInformation("ChangeEmail started.");

            if (!ModelState.IsValid)
            {
                logger.LogInformation("Model was not valid.");

                return View("Email/ChangeEmail", new ChangeEmailViewModel());
            }

            var user = await userManager.FindByEmailAsync(User.Identity.Name);
            var token = await userManager.GenerateChangeEmailTokenAsync(user, model.Email);
            var callBackUrl = Url.Action(nameof(ConfirmEmailChange), "Account", new { userId = user.Id, email = model.Email, token }, Request.Scheme);

            var email = model.Email;
            var subject = "Confirm email.";
            var htmlMessage = $"Please confirm your email by <a href='{HtmlEncoder.Default.Encode(callBackUrl)}'>clicking here</a>.";
            await emailSender.SendAsync(email, subject, htmlMessage);

            logger.LogInformation("Confirmation message was sent.");

            return View("Email/ChangeEmail");
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmailChange(string userId, string email, string token)
        {
            logger.LogInformation("ConfirmEmailChange started.");

            if (userId == null || email == null || token == null)
            {
                logger.LogWarning($"One or more parameters are null. " +
                    $"userId:{userId ?? "null"}, email:{email ?? "null"}, token:{token ?? "null"}");

                return BadRequest("One or more parameters are null.");
            }

            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                logger.LogWarning($"User with UserId: {userId} is not found.");

                return NotFound($"Unable to load user with ID: '{userId}'.");
            }

            var result = await userManager.ChangeEmailAsync(user, email, token);
            if (!result.Succeeded)
            {
                logger.LogWarning($"Email change failed.");

                return BadRequest();
            }

            var setUserNameResult = await userManager.SetUserNameAsync(user, email);
            if (!setUserNameResult.Succeeded)
            {
                logger.LogWarning($"Setting User Name failed.");

                return BadRequest();
            }

            logger.LogInformation("Email change was confirmed.");

            await signInManager.RefreshSignInAsync(user);
            return View("Email/ConfirmChangeEmail");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ConfirmEmail()
        {
            logger.LogInformation("ConfirmEmail started.");

            var user = await userManager.FindByEmailAsync(User.Identity.Name);
            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            var callBackUrl = Url.Action(nameof(EmailConfirmation), "Account", new { userId = user.Id, token }, Request.Scheme);

            var email = user.Email;
            var subject = "Confirm email.";
            var htmlMessage = $"Please confirm your email by <a href='{HtmlEncoder.Default.Encode(callBackUrl)}'>clicking here</a>.";
            await emailSender.SendAsync(email, subject, htmlMessage);

            logger.LogInformation("Confirmation message was sent.");

            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> EmailConfirmation(string userId, string token)
        {
            logger.LogInformation("EmailConfirmation started.");

            if (userId == null || token == null)
            {
                logger.LogWarning($"One or more parameters are null. userId:{userId ?? "null"}, token:{token ?? "null"}");

                return BadRequest("One or more parameters are null.");
            }

            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                logger.LogWarning($"User with UserId: {userId} was not found.");

                return NotFound($"Unable to load user with ID: '{userId}'.");
            }

            var result = await userManager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
            {
                logger.LogWarning($"Email сonfirmation  was failed.");

                return BadRequest();
            }

            logger.LogInformation("Email was confirmed.");

            return Ok();
        }

        [HttpGet]
        public IActionResult ForgotPassword(string returnUrl = "Login")
        {
            return View("Password/ForgotPassword", new ForgotPasswordViewModel() { ReturnUrl = returnUrl });
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            logger.LogInformation("ForgotPassword started.");

            if (!ModelState.IsValid)
            {
                logger.LogInformation("Model was not valid.");

                return View("Password/ForgotPassword", new ForgotPasswordViewModel());
            }

            var user = await userManager.FindByEmailAsync(model.Email);
            if (user == null || !(await userManager.IsEmailConfirmedAsync(user)))
            {
                logger.LogWarning($"User with Email: {model.Email} was not found or Email was not confirmed.");

                return View("Password/ForgotPasswordConfirmation");
            }

            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var callBackUrl = Url.Action("ResetPassword", "Account", new { token }, Request.Scheme);

            var email = model.Email;
            var subject = "Reset Password";
            var htmlMessage = $"Please reset your password by <a href='{HtmlEncoder.Default.Encode(callBackUrl)}'>clicking here</a>.";
            await emailSender.SendAsync(email, subject, htmlMessage);

            logger.LogInformation("Message to change password was sent.");

            return View("Password/ForgotPasswordConfirmation");
        }

        [HttpGet]
        public IActionResult ResetPassword(string token = null)
        {
            if (token == null)
            {
                return BadRequest("A token must be supplied for password reset.");
            }

            return View("Password/ResetPassword", new ResetPasswordViewModel() { Token = token });
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            logger.LogInformation("ResetPassword started.");

            if (!ModelState.IsValid)
            {
                logger.LogInformation("Model was not valid.");

                return BadRequest(ModelState);
            }

            var user = await userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                logger.LogWarning($"User with Email: {model.Email} was not found.");

                return View("Password/ResetPasswordConfirmation");
            }

            var result = await userManager.ResetPasswordAsync(user, model.Token, model.Password);
            if (result.Succeeded)
            {
                logger.LogInformation("Password was reset.");

                return View("Password/ResetPasswordConfirmation");
            }

            logger.LogWarning($"Reset password was failed");

            return Ok();
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
            logger.LogInformation("ChangePassword started.");

            if (!ModelState.IsValid)
            {
                logger.LogInformation("Model was not valid.");

                return BadRequest(ModelState);
            }

            var user = await userManager.GetUserAsync(User);
            var result = await userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (result.Succeeded)
            {
                logger.LogInformation("Password was changed.");

                return View("Password/ChangePasswordConfirmation");
            }

            logger.LogWarning($"Change password was failed.");

            return Redirect(model.ReturnUrl);
        }
    }
}
