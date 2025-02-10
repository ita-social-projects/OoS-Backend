using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using OpenIddict.Client.AspNetCore;
using OutOfSchool.AuthCommon.Config;
using OutOfSchool.AuthCommon.Models;
using OutOfSchool.AuthCommon.ViewModels;
using OutOfSchool.Common.Models;
using OutOfSchool.Common.Models.ExternalAuth;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.AuthCommon.Controllers;

/// <summary>
/// Handles external authentication.
/// Contains methods for log in and sign up.
/// </summary>
public class ExternalAuthController : Controller
{
    private readonly SignInManager<User> signInManager;
    private readonly UserManager<User> userManager;
    private readonly RoleManager<IdentityRole> roleManager;
    private readonly ILogger<ExternalAuthController> logger;
    private readonly AuthorizationServerConfig authServerConfig;
    private readonly IStringLocalizer<SharedResource> localizer;
    private readonly IGovIdentityCommunicationService communicationService;
    private readonly OutOfSchoolDbContext dbContext;

    public ExternalAuthController(
        SignInManager<User> signInManager,
        UserManager<User> userManager,
        RoleManager<IdentityRole> roleManager,
        ILogger<ExternalAuthController> logger,
        IOptions<AuthorizationServerConfig> authServerConfig,
        IStringLocalizer<SharedResource> localizer,
        IGovIdentityCommunicationService communicationService,
        OutOfSchoolDbContext dbContext)
    {
        this.signInManager = signInManager;
        this.userManager = userManager;
        this.roleManager = roleManager;
        this.logger = logger;
        this.authServerConfig = authServerConfig.Value;
        this.localizer = localizer;
        this.communicationService = communicationService;
        this.dbContext = dbContext;
    }

    [Route("~/external-login")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> ExternalLogin(string provider, string role, string returnUrl)
    {
        var roleExists = await roleManager.RoleExistsAsync(role);
        if (!roleExists)
        {
            ModelState.AddModelError(string.Empty, localizer["LoginWithRoleNotSupported"]);
            return View("~/Views/Auth/Login.cshtml", new LoginViewModel
            {
                ExternalProviders = await signInManager.GetExternalAuthenticationSchemesAsync(),
                ReturnUrl = returnUrl,
            });
        }

        var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, returnUrl);
        properties.Items.Add(AuthServerConstants.ExternalAuthSelectedRoleKey, role);
        var allowedAuthTypes = role switch
        {
            _ when Role.Provider.ToString().Equals(role, StringComparison.OrdinalIgnoreCase) => authServerConfig
                .ExternalLogin.Parameters.AuthType.Business,
            _ => authServerConfig.ExternalLogin.Parameters.AuthType.Personal
        };
        properties.Parameters.Add(authServerConfig.ExternalLogin.Parameters.AuthType.Key, allowedAuthTypes);

        return Challenge(properties, OpenIddictClientAspNetCoreDefaults.AuthenticationScheme);
    }

    [HttpGet("~/callback/idgovua")]
    [HttpPost("~/callback/idgovua")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> ExternalLoginCallback()
    {
        var result = await HttpContext.AuthenticateAsync(OpenIddictClientAspNetCoreDefaults.AuthenticationScheme);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty,
                localizer["ExternalAuthorizationDataInvalid"]);

            return this.View("~/Views/Auth/Login.cshtml", new LoginViewModel
            {
                ExternalProviders = await signInManager.GetExternalAuthenticationSchemesAsync(),
                ReturnUrl = result.Properties?.RedirectUri ?? $"~/{AuthServerConstants.LoginPath}",
            });
        }

        var remoteUserId = result.Principal.GetClaim(AuthServerConstants.ExternalAuthUserIdKey);
        var backchannelToken =
            result.Properties.GetTokenValue(OpenIddictClientAspNetCoreConstants.Tokens.BackchannelAccessToken);

        var userInfoResult = await communicationService.GetUserInfo(remoteUserId, backchannelToken);

        return await userInfoResult.Match<Task<IActionResult>>(
            async error =>
            {
                logger.LogError("Unexpected error occurred: {Message} - {Content}", error.Message, error.Content);
                ModelState.AddModelError(string.Empty, localizer["ExternalAuthenticationError"]);

                return View("~/Views/Auth/Login.cshtml", new LoginViewModel
                {
                    ExternalProviders = await signInManager.GetExternalAuthenticationSchemesAsync(),
                    ReturnUrl = result.Properties?.RedirectUri ?? $"~/{AuthServerConstants.LoginPath}",
                });
            },
            async userInfo =>
            {
                try
                {
                    var strategy = dbContext.Database.CreateExecutionStrategy();
                    return await strategy.Execute(async () => await SignInUserAsync(userInfo, result));
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Unexpected error occurred while retrieving login information");
                    ModelState.AddModelError(string.Empty, "Unexpected error occurred while retrieving login information");

                    return View("~/Views/Auth/Login.cshtml", new LoginViewModel
                    {
                        ExternalProviders = await signInManager.GetExternalAuthenticationSchemesAsync(),
                        ReturnUrl = result.Properties?.RedirectUri ?? $"~/{AuthServerConstants.LoginPath}",
                    });
                }
            });
    }

    /// <summary>
    /// Signs in a user based on external authentication result and user info.
    /// </summary>
    /// <param name="userInfo">User information received from external auth provider.</param>
    /// <param name="result">Authentication result from external provider.</param>
    /// <returns><see cref="IActionResult"/> redirecting to appropriate page based on sign in result.</returns>
    private async Task<IActionResult> SignInUserAsync(
        UserInfoResponse userInfo,
        AuthenticateResult result)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync();
        try
        {
            var user = await GetOrCreateUserAsync(userInfo, result.Properties.Items[AuthServerConstants.ExternalAuthSelectedRoleKey]);
            var individual = await GetOrCreateIndividualAsync(userInfo, user);
            var claims = BuildClaims(individual, userInfo, result);
            var properties = await SignInWithClaimsAsync(result, claims);

            await dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            return Redirect(properties.RedirectUri);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error occurred while processing sign-in information");
            await transaction.RollbackAsync();

            ModelState.AddModelError(string.Empty, "Server error occurred while processing sign-in information");

            return View("~/Views/Auth/Login.cshtml", new LoginViewModel
            {
                ExternalProviders = await signInManager.GetExternalAuthenticationSchemesAsync(),
                ReturnUrl = result.Properties?.RedirectUri ?? $"~/{AuthServerConstants.LoginPath}",
            });
        }
    }

    /// <summary>
    /// Gets existing user or creates new one based on external auth info.
    /// </summary>
    /// <param name="userInfo">User information from external provider.</param>
    /// <param name="selectedRole">Selected role for the user.</param>
    /// <returns><see cref="User"/> entity.</returns>
    /// <exception cref="InvalidOperationException">Thrown when user creation fails.</exception>
    private async Task<User> GetOrCreateUserAsync(UserInfoResponse userInfo, string selectedRole)
    {
        var user = await userManager.FindByNameAsync(userInfo.DrfoCode);
        if (user != null)
        {
            return user;
        }

        user = new User
        {
            UserName = userInfo.DrfoCode,
            FirstName = userInfo.GivenName,
            LastName = userInfo.LastName,
            MiddleName = userInfo.MiddleName,
            Email = userInfo.Email,
            CreatingTime = DateTimeOffset.UtcNow,
            IsRegistered = false,
            IsBlocked = false,
            MustChangePassword = false,
        };

        var createResult = await userManager.CreateAsync(user);
        if (!createResult.Succeeded)
        {
            var error = string.Join("; ", createResult.Errors.Select(e => e.Description));
            throw new InvalidOperationException(error);
        }

        await userManager.AddToRoleAsync(user, selectedRole);
        return user;
    }

    /// <summary>
    /// Gets existing individual or creates new one based on user info.
    /// </summary>
    /// <param name="userInfo">User information from external provider.</param>
    /// <param name="user">Associated user entity.</param>
    /// <returns><see cref="Individual"/> entity.</returns>
    private async Task<Individual> GetOrCreateIndividualAsync(UserInfoResponse userInfo, User user)
    {
        var individual = await dbContext.Individuals
            .FirstOrDefaultAsync(i => i.Rnokpp == userInfo.DrfoCode);

        if (individual == null)
        {
            individual = new Individual
            {
                Id = Guid.NewGuid(),
                FirstName = userInfo.GivenName,
                LastName = userInfo.LastName,
                MiddleName = userInfo.MiddleName,
                Rnokpp = userInfo.DrfoCode,
            };
            dbContext.Individuals.Add(individual);
        }

        // Individual was created by admin or other user.
        // Linking it to User
        if (string.IsNullOrEmpty(individual.UserId))
        {
            individual.UserId = user.Id;
        }

        return individual;
    }

    /// <summary>
    /// Builds claims list for the user based on authentication result and user info.
    /// </summary>
    /// <param name="individual">Individual entity.</param>
    /// <param name="userInfo">User information from external provider.</param>
    /// <param name="result">Authentication result.</param>
    /// <returns>List of <see cref="Claim"/> for the user.</returns>
    private List<Claim> BuildClaims(Individual individual, UserInfoResponse userInfo, AuthenticateResult result)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Role, result.Properties.Items[AuthServerConstants.ExternalAuthSelectedRoleKey]),
            new(ClaimTypes.GivenName, individual.FirstName),
            new(ClaimTypes.Surname, individual.LastName),
            new(ClaimTypes.Email, userInfo.Email),
            new(AuthServerConstants.ClaimTypes.Rnokpp, individual.Rnokpp),
            new(
                OpenIddictConstants.Claims.Private.ProviderName,
                result.Principal.GetClaim(OpenIddictConstants.Claims.Private.ProviderName)),
        };
        if (!string.IsNullOrEmpty(result.Principal.GetClaim(OpenIddictConstants.Claims.Private.RegistrationId)))
        {
            claims.Add(new(
                OpenIddictConstants.Claims.Private.RegistrationId,
                result.Principal.GetClaim(OpenIddictConstants.Claims.Private.RegistrationId)));
        }

        if (Role.Provider.ToString()
            .Equals(result.Properties.Items[AuthServerConstants.ExternalAuthSelectedRoleKey],
                StringComparison.CurrentCultureIgnoreCase))
        {
            claims.Add(new Claim(AuthServerConstants.ClaimTypes.Edrpou, userInfo.EdrpouCode));
        }

        return claims;
    }

    /// <summary>
    /// Signs in the user with specified claims.
    /// </summary>
    /// <param name="result">Authentication result.</param>
    /// <param name="claims">Claims to associate with the sign in.</param>
    /// <returns><see cref="AuthenticationProperties"/> containing redirect URI.</returns>
    private async Task<AuthenticationProperties> SignInWithClaimsAsync(AuthenticateResult result, List<Claim> claims)
    {
        var properties = new AuthenticationProperties
        {
            RedirectUri = result.Properties?.RedirectUri ?? $"~/{AuthServerConstants.LoginPath}",
            IsPersistent = false,
        };

        var user = await userManager.FindByNameAsync(claims
            .First(c => c.Type == AuthServerConstants.ClaimTypes.Rnokpp).Value);
        await signInManager.SignInWithClaimsAsync(user, properties, claims);
        return properties;
    }
}