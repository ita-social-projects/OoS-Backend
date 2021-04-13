using System;
using System.Threading.Tasks;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OutOfSchool.IdentityServer.ViewModels;
using OutOfSchool.Services.Models;

namespace OutOfSchool.IdentityServer.Controllers
{
    /// <summary>
    /// Handles authentication.
    /// Contains methods for log in and sign up.
    /// </summary>
    public class AuthController : Controller
    {
        private readonly SignInManager<User> signInManager;
        private readonly UserManager<User> userManager;
        private readonly IIdentityServerInteractionService interactionService;
        private readonly ILogger<AuthController> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthController"/> class.
        /// </summary>
        /// <param name="userManager"> ASP.Net Core Identity User Manager.</param>
        /// <param name="signInManager"> ASP.Net Core Identity Sign in Manager.</param>
        /// <param name="interactionService"> Identity Server 4 interaction service.</param>
        /// <param name="logger"> ILogger class.</param>
        public AuthController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IIdentityServerInteractionService interactionService,
            ILogger<AuthController> logger)
        {
            this.logger = logger;
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.interactionService = interactionService;
        }

        /// <summary>
        /// Logging out a user who is authenticated.
        /// </summary>
        /// <param name="logoutId"> Identifier of cookie captured the current state needed for sign out.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [HttpGet]
        public async Task<IActionResult> Logout(string logoutId)
        {
            await signInManager.SignOutAsync();

            var logoutRequest = await interactionService.GetLogoutContextAsync(logoutId);

            if (string.IsNullOrEmpty(logoutRequest.PostLogoutRedirectUri))
            {
                throw new NotImplementedException();
            }

            return Redirect(logoutRequest.PostLogoutRedirectUri);
        }

        /// <summary>
        /// Generates a view for user to log in.
        /// </summary>
        /// <param name="returnUrl"> URL used to redirect user back to client.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [HttpGet]
        public async Task<IActionResult> Login(string returnUrl = "Login")
        {
            var externalProviders = await signInManager.GetExternalAuthenticationSchemesAsync();
            return View(new LoginViewModel
            {
                ReturnUrl = returnUrl,
                ExternalProviders = externalProviders,
            });
        }

        /// <summary>
        /// Authenticate user based on model.
        /// </summary>
        /// <param name="model"> View model that contains credentials for logging in.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(new LoginViewModel
                {
                    ExternalProviders = await signInManager.GetExternalAuthenticationSchemesAsync(),
                });
            }

            var result = await signInManager.PasswordSignInAsync(model.Username, model.Password, false, false);

            if (result.Succeeded)
            {
                return string.IsNullOrEmpty(model.ReturnUrl) ? Redirect(nameof(Login)) : Redirect(model.ReturnUrl);
            }

            if (result.IsLockedOut)
            {
                return BadRequest();
            }

            ModelState.AddModelError(string.Empty, "Login or password is wrong");
            return View(new LoginViewModel
            {
                ExternalProviders = await signInManager.GetExternalAuthenticationSchemesAsync(),
                ReturnUrl = model.ReturnUrl,
            });
        }

        /// <summary>
        /// Generates a view for user to register.
        /// </summary>
        /// <param name="returnUrl"> URL used to redirect user back to client.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [HttpGet]
        public IActionResult Register(string returnUrl = "Login")
        {
            return View(
                new RegisterViewModel { ReturnUrl = returnUrl });
        }

        /// <summary>
        /// Creates user based on model.
        /// </summary>
        /// <param name="model"> View model that contains credentials for signing in.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = new User()
            {
               UserName = model.Email,
               Name = model.Name,
               LastName = model.LastName,
               MiddleName = model.MiddleName,
               Email = model.Email,
               PhoneNumber = model.PhoneNumber,
               CreatingTime = DateTime.Now,
            };

            var result = await userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                IdentityResult roleAssignResult = IdentityResult.Failed();
                if (Request.Form["Provider"].Count == 1)
                {
                    roleAssignResult = await userManager.AddToRoleAsync(user, "provider");
                }
                else
                if (Request.Form["Parent"].Count == 1)
                {
                    roleAssignResult = await userManager.AddToRoleAsync(user, "parent");
                }

                if (roleAssignResult.Succeeded)
                {
                    await signInManager.SignInAsync(user, false);

                    return Redirect(model.ReturnUrl);
                }

                var deletionResult = await userManager.DeleteAsync(user);
                if (!deletionResult.Succeeded)
                {
                    logger.Log(LogLevel.Warning, "User was created without role");
                }

                foreach (var error in roleAssignResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        public Task<IActionResult> ExternalLogin(string provider, string returnUrl)
        {
            throw new NotImplementedException();
        }
    }
}