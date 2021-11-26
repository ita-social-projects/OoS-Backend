using System;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OutOfSchool.Common;
using OutOfSchool.Common.Extensions;
using OutOfSchool.IdentityServer.ViewModels;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;

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
        private readonly IParentRepository parentRepository;
        private readonly IStringLocalizer<SharedResource> localizer;
        private string userId;
        private string path;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthController"/> class.
        /// </summary>
        /// <param name="userManager"> ASP.Net Core Identity User Manager.</param>
        /// <param name="signInManager"> ASP.Net Core Identity Sign in Manager.</param>
        /// <param name="interactionService"> Identity Server 4 interaction service.</param>
        /// <param name="parentRepository">Repository for Parent model.</param>
        /// <param name="logger"> ILogger class.</param>
        /// <param name="localizer"> Localizer.</param>
        public AuthController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IIdentityServerInteractionService interactionService,
            ILogger<AuthController> logger,
            IParentRepository parentRepository,
            IStringLocalizer<SharedResource> localizer)
        {
            this.logger = logger;
            this.parentRepository = parentRepository;
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.interactionService = interactionService;
            this.localizer = localizer;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            userId = User.GetUserPropertyByClaimType(IdentityResourceClaimsTypes.Sub) ?? "unlogged";
            path = $"{context.HttpContext.Request.Path.Value}[{context.HttpContext.Request.Method}]";
        }

        /// <summary>
        /// Logging out a user who is authenticated.
        /// </summary>
        /// <param name="logoutId"> Identifier of cookie captured the current state needed for sign out.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [HttpGet]
        public async Task<IActionResult> Logout(string logoutId)
        {
            logger.LogDebug($"{path} started. User(id): {userId}.");

            await signInManager.SignOutAsync();

            var logoutRequest = await interactionService.GetLogoutContextAsync(logoutId);

            // TODO: Check whether it is correct to return NotImplementedException here
            if (string.IsNullOrEmpty(logoutRequest.PostLogoutRedirectUri))
            {
                logger.LogError($"{path} PostLogoutRedirectUri was null. User(id): {userId}.");

                throw new NotImplementedException();
            }

            logger.LogInformation($"{path} Successfully logged out. User(id): {userId}");

            return Redirect(logoutRequest.PostLogoutRedirectUri);
        }

        /// <summary>
        /// Generates a view for user to log in.
        /// </summary>
        /// <param name="returnUrl"> URL used to redirect user back to client.</param>
        /// <param name="providerRegistration"> bool used to redirect on registration page and prepare page for provider registration.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [HttpGet]
        public async Task<IActionResult> Login(string returnUrl = "Login", bool? providerRegistration = null)
        {
            if (providerRegistration ?? GetProviderRegistrationFromUri(returnUrl))
            {
                return RedirectToAction("Register", new { returnUrl, providerRegistration });
            }

            logger.LogDebug($"{path} started.");

            var externalProviders = await signInManager.GetExternalAuthenticationSchemesAsync();

            logger.LogDebug($"{path} External providers were obtained.");

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
            logger.LogDebug($"{path} started.");

            if (!ModelState.IsValid)
            {
                logger.LogError($"{path} Input data was not valid.");

                return View(new LoginViewModel
                {
                    ExternalProviders = await signInManager.GetExternalAuthenticationSchemesAsync(),
                    ReturnUrl = model.ReturnUrl,
                });
            }

            var result = await signInManager.PasswordSignInAsync(model.Username, model.Password, false, false);

            if (result.Succeeded)
            {
                logger.LogInformation($"{path} Successfully logged. User(id): {userId}.");

                return string.IsNullOrEmpty(model.ReturnUrl) ? Redirect(nameof(Login)) : Redirect(model.ReturnUrl);
            }

            if (result.IsLockedOut)
            {
                logger.LogWarning($"{path} Attempting to sign-in is locked out.");

                return BadRequest();
            }

            logger.LogInformation($"{path} Login was failed.");

            ModelState.AddModelError(string.Empty, localizer["Login or password is wrong"]);
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
        /// <param name="providerRegistration"> bool used to prepare page for provider registration.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [HttpGet]
        public IActionResult Register(string returnUrl = "Login", bool? providerRegistration = null)
        {
            return View(new RegisterViewModel { ReturnUrl = returnUrl, ProviderRegistration = providerRegistration ?? GetProviderRegistrationFromUri(returnUrl) });
        }

        /// <summary>
        /// Creates user based on model.
        /// </summary>
        /// <param name="model"> View model that contains credentials for signing in.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            logger.LogDebug($"{path} started.");

            if (!ModelState.IsValid)
            {
                logger.LogError($"{path} Input data was not valid.");

                return View(model);
            }

            if (Request.Form[Role.Provider.ToString()].Count == 1)
            {
                model.Role = Role.Provider.ToString().ToLower();
            }
            else
            if (Request.Form[Role.Parent.ToString()].Count == 1)
            {
                model.Role = Role.Parent.ToString().ToLower();
            }
            else
            {
                return View(model);
            }

            var user = new User()
            {
                UserName = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                MiddleName = model.MiddleName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                CreatingTime = DateTimeOffset.UtcNow,
                Role = model.Role,
                IsRegistered = false,
            };

            try
            {
                var result = await userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    logger.LogDebug($"{path} User was created. User(id): {user.Id}");

                    IdentityResult roleAssignResult = IdentityResult.Failed();

                    roleAssignResult = await userManager.AddToRoleAsync(user, user.Role);

                    if (roleAssignResult.Succeeded)
                    {
                        await signInManager.SignInAsync(user, false);

                        if (user.Role == Role.Parent.ToString().ToLower())
                        {
                            var parent = new Parent()
                            {
                                UserId = user.Id,
                            };

                            Func<Task<Parent>> operation = async () => await parentRepository.Create(parent).ConfigureAwait(false);

                            await parentRepository.RunInTransaction(operation).ConfigureAwait(false);
                        }

                        logger.LogInformation($"{path} User(id): {user.Id} was successfully registered with Role: {user.Role}.");

                        return Redirect(model.ReturnUrl);
                    }

                    var deletionResult = await userManager.DeleteAsync(user);

                    if (!deletionResult.Succeeded)
                    {
                        logger.LogWarning($"{path} User(id): {user.Id} was created without role.");
                    }

                    foreach (var error in roleAssignResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
                else
                {
                    logger.LogError($"{path} Registration was failed. " +
                        $"{string.Join(System.Environment.NewLine, result.Errors.Select(e => e.Description))}");

                    foreach (var error in result.Errors)
                    {
                        if (error.Code == "DuplicateUserName")
                        {
                            error.Description = localizer["Email {0} is already taken", error.Description.Substring(10).Split('\'')[0]];
                        }

                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }

                return View(model);
            }
            catch (Exception ex)
            {
                await userManager.RemoveFromRoleAsync(user, user.Role);
                await userManager.DeleteAsync(user);

                ModelState.AddModelError(string.Empty, localizer["Error! Something happened on the server!"]);

                logger.LogError("Error happened while creating Parent entity. " + ex.Message);

                return View(model);
            }
        }

        public Task<IActionResult> ExternalLogin(string provider, string returnUrl)
        {
            throw new NotImplementedException();
        }

        private bool GetProviderRegistrationFromUri(string returnUrl)
        {
            var parsedQuery = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(returnUrl);
            if (parsedQuery.TryGetValue("providerregistration", out var providerRegistration))
            {
                if (bool.TryParse(providerRegistration.FirstOrDefault(), out bool result))
                {
                    return result;
                }
            }

            return false;
        }
    }
}
