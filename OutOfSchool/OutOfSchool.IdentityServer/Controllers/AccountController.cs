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
            return View("Email/Change", new ChangeEmailViewModel() { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ChangeEmail(ChangeEmailViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(new ChangeEmailViewModel());
            }

            var user = await userManager.FindByEmailAsync(User.Identity.Name);
            var token = await userManager.GenerateChangeEmailTokenAsync(user, model.Email);
            var callBackUrl = Url.Action(nameof(ConfirmEmailChange), "Account", new { userId = user.Id, email = model.Email, token }, Request.Scheme);

            var email = model.Email;
            var subject = "Confirm email.";
            var htmlMessage = $"Please confirm your email by <a href='{HtmlEncoder.Default.Encode(callBackUrl)}'>clicking here</a>.";
            await emailSender.SendAsync(email, subject, htmlMessage);

            return View("Email/Change");
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmailChange(string userId, string email, string token)
        {
            if (userId == null || email == null || token == null)
            {
                return View("Error");
            }

            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID: '{userId}'.");
            }

            var result = await userManager.ChangeEmailAsync(user, email, token);
            if (!result.Succeeded)
            {
                return View("Error");
            }

            var setUserNameResult = await userManager.SetUserNameAsync(user, email);
            if (!setUserNameResult.Succeeded)
            {
                return View("Error");
            }

            await signInManager.RefreshSignInAsync(user);
            return View("Email/ConfirmChange");
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> ConfirmEmail()
        {
            var user = await userManager.FindByEmailAsync(User.Identity.Name);
            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            var callBackUrl = Url.Action(nameof(EmailConfirmation), "Account", new { userId = user.Id, token }, Request.Scheme);

            var email = user.Email;
            var subject = "Confirm email.";
            var htmlMessage = $"Please confirm your email by <a href='{HtmlEncoder.Default.Encode(callBackUrl)}'>clicking here</a>.";
            await emailSender.SendAsync(email, subject, htmlMessage);

            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> EmailConfirmation(string userId, string token)
        {
            if (userId == null || token == null)
            {
                return View("Error");
            }

            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID: '{userId}'.");
            }

            var result = await userManager.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
            {
                return View("Error");
            }

            return Ok();
        }

        [HttpGet]
        public IActionResult ForgotPassword(string returnUrl = "Login")
        {
            return View("Password/Forgot", new ForgotPasswordViewModel() { ReturnUrl = returnUrl });
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(new ForgotPasswordViewModel());
            }

            var user = await userManager.FindByEmailAsync(model.Email);
            if (user == null || !(await userManager.IsEmailConfirmedAsync(user)))
            {
                return View("Password/ForgotPasswordConfirmation");
            }

            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var callBackUrl = Url.Action("ResetPassword", "Account", new { area = "Identity", token }, Request.Scheme);

            var email = model.Email;
            var subject = "Reset Password";
            var htmlMessage = $"Please reset your password by <a href='{HtmlEncoder.Default.Encode(callBackUrl)}'>clicking here</a>.";
            await emailSender.SendAsync(email, subject, htmlMessage);

            return View("Password/ForgotPasswordConfirmation");
        }

        [HttpGet]
        public IActionResult ResetPassword(string token = null)
        {
            if (token == null)
            {
                return BadRequest("A token must be supplied for password reset.");
            }

            return View("Password/Reset", new ResetPasswordViewModel() { Token = token });
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Error");
            }

            var user = await userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return View("Password/ResetPasswordConfirmation");
            }

            var result = await userManager.ResetPasswordAsync(user, model.Token, model.Password);
            if (result.Succeeded)
            {
                return View("Password/ResetPasswordConfirmation");
            }

            return Ok();
        }

        [HttpGet]
        [Authorize]
        public IActionResult ChangePassword(string returnUrl = "Login")
        {
            return View("Password/Change", new ChangePasswordViewModel() { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Error");
            }

            var user = await userManager.GetUserAsync(User);
            var result = await userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (result.Succeeded)
            {
                return View("Password/ChangePasswordConfirmation");
            }

            return Ok();
        }
    }
}
