using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Microsoft.FeatureManagement;
using Microsoft.FeatureManagement.Mvc;
using OutOfSchool.AuthCommon.Config;
using OutOfSchool.AuthCommon.ViewModels;
using OutOfSchool.EmailSender.Services;
using OutOfSchool.RazorTemplatesData.Models.Emails;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.AuthCommon.Controllers;

/// <summary>
/// Handles authentication.
/// Contains methods for log in and sign up.
/// </summary>
public class AuthController : Controller
{
    private readonly SignInManager<User> signInManager;
    private readonly UserManager<User> userManager;
    private readonly IUserManagerAdditionalService userManagerAdditionalService;
    private readonly IInteractionService interactionService;
    private readonly ILogger<AuthController> logger;
    private readonly IStringLocalizer<SharedResource> localizer;
    private readonly AuthServerConfig identityServerConfig;
    private readonly IRazorViewToStringRenderer renderer;
    private readonly IEmailSenderService emailSender;
    private readonly IFeatureManager featureManager;
    private string userId;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthController"/> class.
    /// </summary>
    /// <param name="userManager"> ASP.Net Core Identity User Manager.</param>
    /// <param name="userManagerAdditionalService">Additional operations with user manager, including transactions.</param>
    /// <param name="signInManager"> ASP.Net Core Identity Sign in Manager.</param>
    /// <param name="interactionService"> Identity Server 4 interaction service.</param>
    /// <param name="logger"> ILogger class.</param>
    /// <param name="localizer"> Localizer.</param>
    /// <param name="identityServerConfig"> IdentityServer config.</param>
    /// <param name="renderer"> Renderer for Razor page.</param>
    /// <param name="emailSender">E-mail sender service.</param>
    /// <param name="featureManager">Feature management service.</param>
    public AuthController(
        UserManager<User> userManager,
        IUserManagerAdditionalService userManagerAdditionalService,
        SignInManager<User> signInManager,
        IInteractionService interactionService,
        ILogger<AuthController> logger,
        IStringLocalizer<SharedResource> localizer,
        IOptions<AuthServerConfig> identityServerConfig,
        IRazorViewToStringRenderer renderer,
        IEmailSenderService emailSender,
        IFeatureManager featureManager)
    {
        this.logger = logger;
        this.signInManager = signInManager;
        this.userManager = userManager;
        this.userManagerAdditionalService = userManagerAdditionalService;
        this.interactionService = interactionService;
        this.localizer = localizer;
        this.identityServerConfig = identityServerConfig.Value;
        this.renderer = renderer;
        this.emailSender = emailSender;
        this.featureManager = featureManager;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var rqf = Request.HttpContext.Features.Get<IRequestCultureFeature>();
        var culture = rqf.RequestCulture.Culture;

        HttpContext.Response.Cookies.Append("culture", culture.Name);

        userId = User.GetUserPropertyByClaimType(IdentityResourceClaimsTypes.Sub) ?? "unlogged";
    }

    /// <summary>
    /// Logging out a user who is authenticated.
    /// </summary>
    /// <param name="logoutId"> Identifier of cookie captured the current state needed for sign out.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpGet]
    public async Task<IActionResult> Logout(string logoutId)
    {
        logger.LogDebug("Logout started. User(id): {UserId}", userId);

        await signInManager.SignOutAsync();

        var logoutRedirectUri = await interactionService.GetPostLogoutRedirectUri(logoutId);

        // TODO: Check whether it is correct to return NotImplementedException here
        if (string.IsNullOrEmpty(logoutRedirectUri))
        {
            logger.LogError("PostLogoutRedirectUri was null. User(id): {UserId}", userId);

            return Redirect(identityServerConfig.RedirectToStartPageUrl);
        }

        logger.LogInformation("Successfully logged out. User(id): {UserId}", userId);

        return Redirect(logoutRedirectUri);
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

        logger.LogDebug("Login started");

        var externalProviders = await signInManager.GetExternalAuthenticationSchemesAsync();

        logger.LogDebug("External providers were obtained");

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
        logger.LogDebug("Login started");

        if (!ModelState.IsValid)
        {
            logger.LogError("Input data was not valid");

            return View(new LoginViewModel
            {
                ExternalProviders = await signInManager.GetExternalAuthenticationSchemesAsync(),
                ReturnUrl = model.ReturnUrl,
            });
        }

        var user = await userManager.FindByEmailAsync(model.Username);

        if (user != null)
        {
            if (user.IsBlocked && !user.Role.Equals(Role.Provider.ToString(), StringComparison.InvariantCultureIgnoreCase))
            {
                logger.LogInformation("User is blocked. Login was failed");

                ModelState.AddModelError(string.Empty, localizer["Your account is blocked"]);
                return View(new LoginViewModel
                {
                    ExternalProviders = await signInManager.GetExternalAuthenticationSchemesAsync(),
                    ReturnUrl = model.ReturnUrl,
                });
            }

            if (user.IsDeleted)
            {
                logger.LogInformation("User is deleted. Login was failed");

                ModelState.AddModelError(string.Empty, localizer["Your account is deleted"]);

                return View(new LoginViewModel
                {
                    ExternalProviders = await signInManager.GetExternalAuthenticationSchemesAsync(),
                    ReturnUrl = model.ReturnUrl,
                });
            }

            if (user.MustChangePassword)
            {
                var checkResult = await signInManager.CheckPasswordSignInAsync(user, model.Password, false);

                if (checkResult.Succeeded)
                {
                    logger.LogTrace("User is being redirected to ChangePasswordLogin");
                    return RedirectToAction(
                        nameof(ChangePasswordLogin),
                        new { email = user.Email, returnUrl = model.ReturnUrl });
                }
            }
            else
            {
                var result = await signInManager.PasswordSignInAsync(model.Username, model.Password, model.RememberMe, false);

                if (result.Succeeded)
                {
                    logger.LogInformation("Successfully logged. User(id): {UserId}", user.Id);

                    user.LastLogin = DateTimeOffset.UtcNow;
                    var lastLoginResult = await userManager.UpdateAsync(user);
                    if (!lastLoginResult.Succeeded)
                    {
                        throw new InvalidOperationException($"Unexpected error occurred setting the last login date" +
                                                            $" ({lastLoginResult}) for user with ID '{user.Id}'.");
                    }

                    return string.IsNullOrEmpty(model.ReturnUrl) ? Redirect(nameof(Login)) : Redirect(model.ReturnUrl);
                }

                if (result.IsLockedOut)
                {
                    logger.LogWarning("Attempting to sign-in is locked out");

                    return BadRequest();
                }
            }
        }

        logger.LogInformation("Login was failed");

        ModelState.AddModelError(string.Empty, localizer["Login or password is wrong"]);
        return View(new LoginViewModel
        {
            ExternalProviders = await signInManager.GetExternalAuthenticationSchemesAsync(),
            ReturnUrl = model.ReturnUrl,
        });
    }

    /// <summary>
    /// Generates a view for user to log in with changing password.
    /// </summary>
    /// <param name="email">User's email.</param>
    /// <param name="returnUrl">URL used to redirect user back to client.</param>
    /// <returns>An <see cref="IActionResult"/> representing the result.</returns>
    [HttpGet]
    [Obsolete("Change password API is no longer supported. Exists only for testing purposes.")]
    [FeatureGate(AuthServerConstants.FeatureManagement.PasswordLogin)]
    public IActionResult ChangePasswordLogin(string email, string returnUrl = "Login")
    {
        return View(new ChangePasswordLoginViewModel { Email = email, ReturnUrl = returnUrl });
    }

    /// <summary>
    /// Authenticate user with changing password based on model.
    /// </summary>
    /// <param name="model"> View model that contains credentials for logging in.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpPost]
    [Obsolete("Change password API is no longer supported. Exists only for testing purposes.")]
    [FeatureGate(AuthServerConstants.FeatureManagement.PasswordLogin)]
    public async Task<IActionResult> ChangePasswordLogin(ChangePasswordLoginViewModel model)
    {
        logger.LogDebug("ChangePasswordLogin started");

        if (!ModelState.IsValid)
        {
            logger.LogError("Input data was not valid");

            return View(new ChangePasswordLoginViewModel { Email = model.Email, ReturnUrl = model.ReturnUrl });
        }

        var user = await userManager.FindByEmailAsync(model.Email);

        if (user != null)
        {
            if (user.MustChangePassword)
            {
                var result = await signInManager.CheckPasswordSignInAsync(user, model.CurrentPassword, false);

                if (result.Succeeded)
                {
                    logger.LogDebug("Password with mustChangePassword indicator was started for user with email {Email}", user.Email);

                    var changeResult =
                        await userManagerAdditionalService.ChangePasswordWithRequiredMustChangePasswordAsync(
                            user,
                            model.CurrentPassword,
                            model.NewPassword);

                    if (changeResult.Succeeded)
                    {
                        logger.LogDebug("Password with mustChangePassword indicator was successfully changed for user with email {Email}", user.Email);

                        return await Login(new LoginViewModel
                        {
                            Username = user.Email,
                            Password = model.NewPassword,
                            ReturnUrl = model.ReturnUrl,
                            ExternalProviders = await signInManager.GetExternalAuthenticationSchemesAsync(),
                        });
                    }
                }
            }
            else
            {
                logger.LogWarning("User is not allowed to change password login action");

                ModelState.AddModelError(string.Empty, localizer["User is not allowed to change password login action"]);
                return View(new ChangePasswordLoginViewModel { Email = model.Email, ReturnUrl = model.ReturnUrl });
            }
        }

        logger.LogInformation("Login was failed");

        ModelState.AddModelError(string.Empty, localizer["Login or password is wrong"]);
        return View(new ChangePasswordLoginViewModel { Email = model.Email, ReturnUrl = model.ReturnUrl });
    }

    /// <summary>
    /// Generates a view for user to register.
    /// </summary>
    /// <param name="returnUrl"> URL used to redirect user back to client.</param>
    /// <param name="providerRegistration"> bool used to prepare page for provider registration.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpGet]
    [FeatureGate(AuthServerConstants.FeatureManagement.PasswordRegistration)]
    [Obsolete("Registration API is no longer supported. Exists only for testing purposes.")]
    public IActionResult Register(string returnUrl = "Login", bool? providerRegistration = null)
    {
        return View(new RegisterViewModel
        {
            ReturnUrl = returnUrl,
            ProviderRegistration = providerRegistration ?? GetProviderRegistrationFromUri(returnUrl),
        });
    }

    /// <summary>
    /// Creates user based on model.
    /// </summary>
    /// <param name="model"> View model that contains credentials for signing in.</param>
    /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
    [HttpPost]
    [FeatureGate(AuthServerConstants.FeatureManagement.PasswordRegistration)]
    [Obsolete("Registration API is no longer supported. Exists only for testing purposes.")]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        logger.LogDebug("Register started");

        if (!ModelState.IsValid)
        {
            logger.LogError("Input data was not valid");

            return View("Register", model);
        }

        if (Request.Form[nameof(Role.Provider)].Count == 1)
        {
            model.Role = nameof(Role.Provider).ToLower();
        }
        else if (Request.Form[nameof(Role.Parent)].Count == 1)
        {
            model.Role = nameof(Role.Parent).ToLower();
        }
        else
        {
            return View("Register", model);
        }

        var isEmailConfirmationEnabled = await featureManager.IsEnabledAsync(AuthServerConstants.FeatureManagement.EmailConfirmation);

        var user = new User()
        {
            UserName = model.Email,
            FirstName = model.FirstName,
            LastName = model.LastName,
            MiddleName = model.MiddleName,
            Email = model.Email,
            CreatingTime = DateTimeOffset.UtcNow,
            Role = model.Role,
            IsRegistered = false,
            IsBlocked = false,
            EmailConfirmed = !isEmailConfirmationEnabled,
        };

        try
        {
            var result = await userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                logger.LogDebug("User was created. User(id): {UserId}", user.Id);

                // TODO: Move sending email process to separated method
                var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
                var callBackUrl = Url.Action("EmailConfirmation", "Account", new { token, user.Email, model.ReturnUrl }, Request.Scheme);

                var email = model.Email;
                var subject = localizer["Confirm email"];
                var userActionViewModel = new UserActionViewModel
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    ActionUrl = callBackUrl,
                };
                var content = await renderer.GetHtmlPlainStringAsync(RazorTemplates.ConfirmEmail, userActionViewModel);
                await emailSender.SendAsync(email, subject, content);

                logger.LogInformation("Message to confirm email was sent. User(id): {UserId}", user.Id);

                IdentityResult roleAssignResult = IdentityResult.Failed();

                roleAssignResult = await userManager.AddToRoleAsync(user, user.Role);

                if (roleAssignResult.Succeeded)
                {
                    await signInManager.SignInAsync(user, false);

                    logger.LogInformation("User(id): {UserId} was successfully registered with Role: {UserRole}", user.Id, user.Role);

                    return View("ConfirmEmail", model);
                }

                var deletionResult = await userManager.DeleteAsync(user);

                if (!deletionResult.Succeeded)
                {
                    logger.LogWarning("User(id): {UserId} was created without role", user.Id);
                }

                foreach (var error in roleAssignResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            else
            {
                logger.LogError("Registration was failed. " +
                                $"{string.Join(Environment.NewLine, result.Errors.Select(e => e.Description))}");

                foreach (var error in result.Errors)
                {
                    if (error.Code == "DuplicateUserName")
                    {
                        error.Description = localizer["Email is already taken"];
                    }

                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View("Register", model);
        }
        catch (Exception ex)
        {
            await userManager.RemoveFromRoleAsync(user, user.Role);
            await userManager.DeleteAsync(user);

            ModelState.AddModelError(string.Empty, localizer["Error! Something happened on the server!"]);

            logger.LogError(ex, "Error happened while creating Parent entity");

            return View("Register", model);
        }
    }

    [Obsolete("Registration API is no longer supported. Exists only for testing purposes.")]
    private bool GetProviderRegistrationFromUri(string returnUrl)
    {
        var parsedQuery = QueryHelpers.ParseQuery(returnUrl);
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