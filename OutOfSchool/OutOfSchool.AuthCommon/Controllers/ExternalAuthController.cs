using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using OpenIddict.Client;
using OpenIddict.Client.AspNetCore;
using OutOfSchool.AikomApiClient;
using OutOfSchool.AuthCommon.Config;
using OutOfSchool.AuthCommon.ViewModels;
using OutOfSchool.Common.Enums;
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
    private readonly IAikomProviderService aikomProviderService;
    private readonly OpenIddictClientService openIddictClientService;

    public ExternalAuthController(
        SignInManager<User> signInManager,
        UserManager<User> userManager,
        RoleManager<IdentityRole> roleManager,
        ILogger<ExternalAuthController> logger,
        IOptions<AuthorizationServerConfig> authServerConfig,
        IStringLocalizer<SharedResource> localizer,
        IGovIdentityCommunicationService communicationService,
        OutOfSchoolDbContext dbContext,
        IAikomProviderService aikomProviderService,
        OpenIddictClientService openIddictClientService)
    {
        this.signInManager = signInManager;
        this.userManager = userManager;
        this.roleManager = roleManager;
        this.logger = logger;
        this.authServerConfig = authServerConfig.Value;
        this.localizer = localizer;
        this.communicationService = communicationService;
        this.dbContext = dbContext;
        this.aikomProviderService = aikomProviderService;
        this.openIddictClientService = openIddictClientService;
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
        
        var registration = await openIddictClientService.GetClientRegistrationByProviderNameAsync(provider).ConfigureAwait(true);
        
        properties.Items.Add(OpenIddictClientAspNetCoreConstants.Properties.RegistrationId, registration.RegistrationId);

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
            return await GetErrorMessageResult(result, localizer["ExternalAuthorizationDataInvalid"]);
        }

        var remoteUserId = result.Principal.GetClaim(AuthServerConstants.ExternalAuthUserIdKey);
        var backchannelToken =
            result.Properties.GetTokenValue(OpenIddictClientAspNetCoreConstants.Tokens.BackchannelAccessToken);

        var userInfoResult = await communicationService.GetUserInfo(remoteUserId, backchannelToken);

        return await userInfoResult.Match<Task<IActionResult>>(
            async error =>
            {
                logger.LogError("Unexpected error occurred: {Message} - {Content}", error.Message, error.Content);
                return await GetErrorMessageResult(result, localizer["ExternalAuthenticationError"]);
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
                    return await GetErrorMessageResult(result, "Unexpected error occurred while retrieving login information");
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
            var selectedRole = result.Properties.Items[AuthServerConstants.ExternalAuthSelectedRoleKey];

            // We can create User as it is tied to log in attempt
            var user = await GetOrCreateUserAsync(userInfo, selectedRole);
            
            // TODO: while AIKOM is not operational, we forbid new registrations.
            // Only people who are in DB are allowed to log in.
            var individual = await GetIndividualAsync(userInfo, user);

            if (individual == null)
            {
                return await GetErrorMessageResult(result, localizer["IndividualOrProviderNotFound"]);
            }

            List<Claim> claims = [];
            
            if (Role.Provider.ToString().Equals(selectedRole, StringComparison.OrdinalIgnoreCase) || 
                Role.Employee.ToString().Equals(selectedRole, StringComparison.OrdinalIgnoreCase))
            {
                // For provider role, verify director access before proceeding to database operations.
                long? externalProviderId = null;
                // TODO: while AIKOM is not operational, do not check anything.
                // usage is in VerifyProviderAccessAsync docs.

                var positions = await GetPositionsForProviderAndIndividual(userInfo, individual.Id);
                
                if (positions.Count == 0)
                {
                    return await GetErrorMessageResult(result, localizer["IndividualOrProviderNotFound"]);
                }
                
                // Process provider-specific logic
                if (Role.Provider.ToString().Equals(selectedRole, StringComparison.OrdinalIgnoreCase))
                {
                    var directorPosition = positions.FirstOrDefault(p => p.PositionType == PositionType.Director);
                    if (directorPosition == null)
                    {
                        return await GetErrorMessageResult(result, localizer["IndividualIsNotProviderDirector", positions[0].ProviderTitle]);
                    }
                }
                // Process employee-specific logic
                else if (Role.Employee.ToString().Equals(selectedRole, StringComparison.OrdinalIgnoreCase))
                {
                    
                    var employeePosition = positions.FirstOrDefault(p => p.PositionType is PositionType.Employee or PositionType.DeputyDirector);
                    if (employeePosition == null)
                    {
                        return await GetErrorMessageResult(result, localizer["IndividualIsNotProviderEmployee", positions[0].ProviderTitle]);
                    }
                }
                var providerId = positions.Select(p => p.ProviderId).FirstOrDefault();
                var isDeputy = positions.Any(p => p.PositionType == PositionType.DeputyDirector);
                
                claims = BuildProviderClaims(individual, userInfo, result, providerId, isDeputy, externalProviderId);
            }

            var properties = await SignInWithClaimsAsync(result, claims);

            await dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            return Redirect(properties.RedirectUri);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error occurred while processing sign-in information");
            await transaction.RollbackAsync();

            return await GetErrorMessageResult(result, "Server error occurred while processing sign-in information");
        }
    }

    /// <summary>
    /// Verifies if the user has access as a provider director. While AIKOM is not operational, do not check anything.
    /// <example>
    /// Usage in SignInUserAsync should be lke the following.
    /// <code>
    /// if (Role.Provider.ToString().Equals(selectedRole, StringComparison.OrdinalIgnoreCase))
    /// {
    ///     var verificationResult = await VerifyProviderAccessAsync(userInfo);
    ///     if (verificationResult.errorResult != null)
    ///     {
    ///         ModelState.AddModelError(string.Empty, verificationResult.errorResult);
    ///         return View("~/Views/Auth/Login.cshtml", new LoginViewModel
    ///             {
    ///                 ExternalProviders = await signInManager.GetExternalAuthenticationSchemesAsync(),
    ///                 ReturnUrl = $"~/{AuthServerConstants.LoginPath}",
    ///             });
    ///     }
    ///     externalProviderId = verificationResult.externalProviderId;
    /// }
    /// </code>
    /// </example>
    /// </summary>
    /// <param name="userInfo">User information from external provider.</param>
    /// <returns>A tuple containing error result (if any) and external provider ID (if verified).</returns>
    // ReSharper disable once UnusedMember.Local
    private async Task<(LocalizedString? errorResult, long? externalProviderId)> VerifyProviderAccessAsync(
        UserInfoResponse userInfo)
    {
        try
        {
            var verificationResult = await aikomProviderService
                .VerifyProviderAndDirectorAccess(userInfo.EdrpouCode, userInfo.DrfoCode)
                .ConfigureAwait(false);

            // TODO: this is a dirty either to non-either transition, but it works :)
            return verificationResult.Match<(LocalizedString? errorResult, long? externalProviderId)>(
                error =>
                {
                    logger.LogError(
                        "Failed to verify provider access. EDRPOU: {Edrpou}, DRFO: {Drfo}, Error: {Error}",
                        userInfo.EdrpouCode,
                        userInfo.DrfoCode,
                        error.Message);

                    return (localizer["ExternalProviderVerificationFailed"], null);
                },
                providerInfo =>
                {
                    if (providerInfo?.HasAccess ?? false)
                    {
                        return (null, providerInfo.ProviderInfo.Id);
                    }

                    logger.LogWarning(
                        "User is not a director of the provider. EDRPOU: {Edrpou}, DRFO: {Drfo}",
                        userInfo.EdrpouCode,
                        userInfo.DrfoCode);

                    return (localizer["NotAProviderDirector"], null);
                });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error verifying provider access for EDRPOU: {Edrpou}", userInfo.EdrpouCode);
            throw;
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
            if (!await userManager.IsInRoleAsync(user, selectedRole))
            {
                await userManager.AddToRoleAsync(user, selectedRole);
            }
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
    /// Gets existing individual based on user info.
    /// </summary>
    /// <param name="userInfo">User information from external provider.</param>
    /// <param name="user">Associated user entity.</param>
    /// <returns><see cref="Individual"/> entity.</returns>
    private async Task<Individual?> GetIndividualAsync(UserInfoResponse userInfo, User user)
    {
        var individual = await dbContext.Individuals
            .FirstOrDefaultAsync(i => !i.IsDeleted && i.Rnokpp == userInfo.DrfoCode);

        if (individual == null)
        {
            return null;
        }

        // It is first login attempt
        // Linking it to User
        if (string.IsNullOrEmpty(individual.UserId))
        {
            individual.UserId = user.Id;
        }

        return individual;
    }

    /// <summary>
    /// Gets existing Positions for given Individual DRFO and Provider EDRPOU.
    /// </summary>
    /// <param name="userInfo">User information from external provider.</param>
    /// <param name="individualId">Existing Individual id.</param>
    /// <returns><see cref="List{T}"/> of short Positions or empty list if combination was not found.</returns>
    private async Task<List<PositionProjection>> GetPositionsForProviderAndIndividual(UserInfoResponse userInfo, Guid individualId)
    {
        // Provider does not exist, no need to do big JOIN
        var providerExists = dbContext.Providers.Any(p => !p.IsDeleted && p.Edrpou == userInfo.EdrpouCode);

        if (!providerExists)
        {
            return [];
        }
        
        // Get the Positions for the given provider, that have officials linked to the individual
        var positions = await dbContext.Positions
            .Include(p => p.Provider)
            .Where(x => !x.IsDeleted && (!x.Provider.IsDeleted && x.Provider.Edrpou == userInfo.EdrpouCode) && x.Officials.Any(o => o.IndividualId == individualId && !o.IsDeleted))
            .Select(p => new PositionProjection(p.Provider.FullTitle, p.ProviderId, p.PositionType))
            .ToListAsync();
            
        return positions;
    }

    /// <summary>
    /// Builds claims list for the user based on authentication result and user info.
    /// </summary>
    /// <param name="individual">Individual entity.</param>
    /// <param name="userInfo">User information from external provider.</param>
    /// <param name="result">Authentication result.</param>
    /// <param name="providerId">Internal provider ID.</param>
    /// <param name="isDeputy">Boolean flag to show if individual has deputy director position</param>
    /// <param name="externalProviderId">Optional external provider ID for provider role.</param>
    /// <returns>List of <see cref="Claim"/> for the user.</returns>
    private List<Claim> BuildProviderClaims(
        Individual individual,
        UserInfoResponse userInfo,
        AuthenticateResult result,
        Guid providerId,
        bool isDeputy,
        long? externalProviderId = null)
    {
        var claims = new List<Claim>
        {
            new(OpenIddictConstants.Claims.Role, result.Properties.Items[AuthServerConstants.ExternalAuthSelectedRoleKey]),
            new(OpenIddictConstants.Claims.GivenName, individual.FirstName),
            new(OpenIddictConstants.Claims.FamilyName, individual.LastName),
            new(OpenIddictConstants.Claims.Email, userInfo.Email),
            new(Constants.ClaimTypes.Rnokpp, individual.Rnokpp),
            new(Constants.ClaimTypes.Edrpou, userInfo.EdrpouCode),
            new(Constants.ClaimTypes.ProviderId, providerId.ToString()),
            new(Constants.ClaimTypes.IsDeputy, isDeputy.ToString(), ClaimValueTypes.Boolean),
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
            
        if (externalProviderId.HasValue)
        {
            claims.Add(new Claim(Constants.ClaimTypes.AikomProviderId, 
                externalProviderId.Value.ToString()));
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
            .First(c => c.Type == Constants.ClaimTypes.Rnokpp).Value);
        await signInManager.SignInWithClaimsAsync(user, properties, claims.Where(c => c.Type != OpenIddictConstants.Claims.Role));
        User.SetClaim(OpenIddictConstants.Claims.Role, claims.First(c => c.Type == OpenIddictConstants.Claims.Role).Value);
        return properties;
    }
    
    /// <summary>
    /// Creates an error result with a custom message and returns the login view.
    /// </summary>
    /// <param name="result">The authentication result containing redirect URI information.</param>
    /// <param name="message">The error message to display to the user.</param>
    /// <returns>A view result with the login page and error message.</returns>
    private async Task<IActionResult> GetErrorMessageResult(AuthenticateResult result, string message)
    {
        ModelState.AddModelError(string.Empty, message);

        return this.View("~/Views/Auth/Login.cshtml", new LoginViewModel
        {
            ExternalProviders = await signInManager.GetExternalAuthenticationSchemesAsync(),
            ReturnUrl = result.Properties?.RedirectUri ?? $"~/{AuthServerConstants.LoginPath}",
        });
    }
    
    private record PositionProjection(string ProviderTitle, Guid ProviderId, PositionType PositionType);
}