using System;
using System.Threading.Tasks;
using IdentityServer.Controllers;
using System.Linq;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
        private readonly RoleManager<IdentityRole> roleManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthController"/> class.
        /// </summary>
        /// <param name="userManager"> ASP.Net Core Identity User Manager.</param>
        /// <param name="signInManager"> ASP.Net Core Identity Sign in Manager.</param>
        /// <param name="roleManager">ASP.Net Core Identity Role Manager.</param>
        /// <param name="interactionService"> Identity Server 4 interaction service.</param>
        public AuthController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            RoleManager<IdentityRole> roleManager,
            IIdentityServerInteractionService interactionService)
        {
            this.roleManager = roleManager;
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
                return RedirectToAction("Index", "Home");
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
                return Redirect(model.ReturnUrl);
            }

            if (result.IsLockedOut)
            {
                return BadRequest();
            }

            ModelState.AddModelError(string.Empty, "Login or password is wrong");
            return View(new LoginViewModel
            {
                ExternalProviders = await signInManager.GetExternalAuthenticationSchemesAsync(),
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
                new RegisterViewModel { ReturnUrl = returnUrl, AllRoles = roleManager.Roles.ToList() });
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
                model.AllRoles = roleManager.Roles.ToList();
                return View(model);
            }

            var user = new User()
            {
                UserName = model.Username,
                PhoneNumber = model.PhoneNumber,
                CreatingTime = DateTime.Now,
            };
            var result = await userManager.CreateAsync(user, model.Password);
            var selectedRole = roleManager.Roles.First(role => role.Id == model.UserRoleId).Name;
            if (result.Succeeded)
            {
                var resultRoleAssign = await userManager.AddToRoleAsync(user, selectedRole);
                if (resultRoleAssign.Succeeded)
                {
                    await signInManager.SignInAsync(user, false);

                    return Redirect(model.ReturnUrl);
                }
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }
    }
}