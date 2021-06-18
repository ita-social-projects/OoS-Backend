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
        public IActionResult ChangeEmail()
        {
            return View("Email/Change");
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

            var message = new Message()
            {
                From = new EmailAddress()
                {
                    Name = "Oos-Backend",
                    Address = "OoS.Backend.Test.Server@gmail.com",
                },
                To = new EmailAddress()
                {
                    Name = model.Email,
                    Address = model.Email,
                },
                Content = $"Please confirm your email by <a href='{HtmlEncoder.Default.Encode(callBackUrl)}'>clicking here</a>.",
                Subject = "Confirm email.",
            };
            await emailSender.SendAsync(message);

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
            var callBackUrl = Url.Action(nameof(ConfirmationEmail), "Account", new { userId = user.Id, token }, Request.Scheme);

            var message = new Message()
            {
                From = new EmailAddress()
                {
                    Name = "Oos-Backend",
                    Address = "OoS.Backend.Test.Server@gmail.com",
                },
                To = new EmailAddress()
                {
                    Name = user.Email,
                    Address = user.Email,
                },
                Content = $"Please confirm your email by <a href='{HtmlEncoder.Default.Encode(callBackUrl)}'>clicking here</a>.",
                Subject = "Confirm email.",
            };
            await emailSender.SendAsync(message);
                        
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmationEmail(string userId, string token)
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
    }
}
