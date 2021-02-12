namespace IdentityServer.Controllers
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using IdentityServer4.Services;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using OutOfSchool.Services.Models;

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
        /// Logging out a user who is authenticated
        /// </summary>
        /// <param name="logoutId"> Identifier of cookie captured the current state needed for sign out.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [HttpGet]
        public async Task<IActionResult> Logout(string logoutId)
        {
            await this.signInManager.SignOutAsync();

            var logoutRequest = await this.interactionService.GetLogoutContextAsync(logoutId);

            if (string.IsNullOrEmpty(logoutRequest.PostLogoutRedirectUri))
            {
                return this.RedirectToAction("Index", "Home");
            }

            return this.Redirect(logoutRequest.PostLogoutRedirectUri);
        }

        /// <summary>
        /// Generates a view for user to log in
        /// </summary>
        /// <param name="returnUrl"> URL used to redirect user back to client.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [HttpGet]
        public async Task<IActionResult> Login(string returnUrl = "Login")
        {
            var externalProviders = await this.signInManager.GetExternalAuthenticationSchemesAsync();
            return this.View(new LoginViewModel
            {
                ReturnUrl = returnUrl,
                ExternalProviders = externalProviders,
            });
        }

        /// <summary>
        /// Authenticate user based on model
        /// </summary>
        /// <param name="model"> View model that contains credentials for logging in.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return View(new LoginViewModel
                {
                    ExternalProviders = await this.signInManager.GetExternalAuthenticationSchemesAsync(),
                });
            }

            var result = await signInManager.PasswordSignInAsync(model.Username, model.Password, false, false);

            if (result.Succeeded)
            {
                return this.Redirect(model.ReturnUrl);
            }

            if (result.IsLockedOut)
            {
                return this.BadRequest();
            }

            this.ModelState.AddModelError(string.Empty, "Login or password is wrong");
            return this.View(new LoginViewModel
            {
                ExternalProviders = await this.signInManager.GetExternalAuthenticationSchemesAsync(),
            });
        }

        /// <summary>
        /// Generates a view for user to register
        /// </summary>
        /// <param name="returnUrl"> URL used to redirect user back to client.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [HttpGet]
        public IActionResult Register(string returnUrl = "Login")
        {
            return this.View(
                new RegisterViewModel { ReturnUrl = returnUrl, AllRoles = this.roleManager.Roles.ToList() });
        }

        /// <summary>
        /// Creates user based on model
        /// </summary>
        /// <param name="model"> View model that contains credentials for signing in.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!this.ModelState.IsValid)
            {
                model.AllRoles = this.roleManager.Roles.ToList();
                return this.View(model);
            }

            var user = new User()
            {
                UserName = model.Username,
                PhoneNumber = model.PhoneNumber,
                CreatingTime = DateTime.Now,
            };
            var result = await this.userManager.CreateAsync(user, model.Password);
            var selectedRole = this.roleManager.Roles.First(role => role.Id == model.UserRoleId).Name;
            if (result.Succeeded)
            {
                var resultRoleAssign = await this.userManager.AddToRoleAsync(user, selectedRole);
                if (resultRoleAssign.Succeeded)
                {
                    await this.signInManager.SignInAsync(user, false);

                    return this.Redirect(model.ReturnUrl);
                }
            }

            foreach (var error in result.Errors)
            {
                this.ModelState.AddModelError(string.Empty, error.Description);
            }

            return this.View(model);
        }
    }
}