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
            ModelState.AddModelError(string.Empty, "Login in with this role is not supported.");
            return View("~/Views/Auth/Login.cshtml", new LoginViewModel
            {
                ExternalProviders = await signInManager.GetExternalAuthenticationSchemesAsync(),
                ReturnUrl = returnUrl,
            });
        }

        var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, returnUrl);
        properties.Items.Add(AuthServerConstants.ExternalAuthSelectedRoleKey, role);
        properties.Parameters.Add(authServerConfig.ExternalLogin.Parameters.AuthType.Key,
            authServerConfig.ExternalLogin.Parameters.AuthType.Value);

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
            ModelState.AddModelError(string.Empty, "The external authorization data cannot be used for authentication.");

            return this.View("~/Views/Auth/Login.cshtml", new LoginViewModel
            {
                ExternalProviders = await signInManager.GetExternalAuthenticationSchemesAsync(),
                ReturnUrl = result.Properties?.RedirectUri ?? "/login",
            });
        }

        var remoteUserId = result.Principal.GetClaim(AuthServerConstants.ExternalAuthUserIdKey);
        var backchannelToken =
            result.Properties.GetTokenValue(OpenIddictClientAspNetCoreConstants.Tokens.BackchannelAccessToken);

        var externalAuth = await communicationService
            .GetUserInfo(remoteUserId, backchannelToken)
            .FlatMapAsync(
                async userInfo =>
                {
                    // TODO: maybe use this instead for future if we decide we will not store User in our Db Context
                    // using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
                    var strategy = dbContext.Database.CreateExecutionStrategy();
                    return await strategy.Execute(async () => await this.SignInUserAsync(userInfo, result));
                },
                error =>
                {
                    logger.LogError(error, "Unexpected error occurred while retrieving login information");
                    return new InternalAuthError
                    {
                        HttpStatusCode = HttpStatusCode.InternalServerError,
                        Message = "Unexpected error occurred while retrieving login information",
                        ErrorGroup = InternalAuthErrorGroup.Unknown,
                    };
                });

        return await externalAuth.Match(
            async error =>
            {
                logger.LogError("Unexpected error occurred: {Message} - {Content}", error.Message, error.Content);
                var frontMessage = error switch
                {
                    ExternalAuthError e => ProcessExternalError(e),
                    InternalAuthError e => ProcessInternalError(e),
                    _ => "Unknown error, please try again or contact local administrator",
                };

                ModelState.AddModelError(string.Empty, frontMessage);

                return View("~/Views/Auth/Login.cshtml", new LoginViewModel
                {
                    ExternalProviders = await signInManager.GetExternalAuthenticationSchemesAsync(),
                    ReturnUrl = result.Properties?.RedirectUri ?? "/login",
                }) as IActionResult;
            },
            Task.FromResult);
    }

    private static string ProcessExternalError(ExternalAuthError e)
    {
        return e.ErrorGroup switch
        {
            ExternalAuthErrorGroup.Unknown => "Unknown error, please try again.",
            ExternalAuthErrorGroup.Encryption => "Error processing your information, contact local administrator",
            ExternalAuthErrorGroup.IdGovUa => "External provider error, please contact external support",
            _ => "Unknown error, please try again or contact external support",
        };
    }

    private static string ProcessInternalError(InternalAuthError e)
    {
        return e.ErrorGroup switch
        {
            InternalAuthErrorGroup.Unknown => "Unknown error, please try again.",
            InternalAuthErrorGroup.Logic =>
                "Error processing your information. If error persists - contact local administrator",
            InternalAuthErrorGroup.Database => "Server error. If error persists - contact local administrator",
            _ => "Unknown error, please try again or contact local administrator",
        };
    }

    private async Task<Either<IErrorResponse, IActionResult>> SignInUserAsync(
        UserInfoResponse userInfo,
        AuthenticateResult result)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync();
        try
        {
            var signIn = await GetOrCreateUserAsync(userInfo)
                .FlatMapAsync(user => GetOrCreateIndividualAsync(userInfo, user))
                .FlatMapAsync(individual => BuildClaims(individual, userInfo, result))
                .FlatMapAsync(claims => SignInWithClaimsAsync(result, claims));

            var signInResult = await signIn.Match(
                async error =>
                {
                    if (error is not InternalAuthError e)
                    {
                        throw new InvalidOperationException(error.Message);
                    }

                    await transaction.RollbackAsync();

                    ModelState.AddModelError(string.Empty, ProcessInternalError(e));

                    return this.View("~/Views/Auth/Login.cshtml", new LoginViewModel
                    {
                        ExternalProviders = await signInManager.GetExternalAuthenticationSchemesAsync(),
                        ReturnUrl = result.Properties?.RedirectUri ?? "/login",
                    }) as ActionResult;
                },
                async properties =>
                {
                    await dbContext.SaveChangesAsync().ConfigureAwait(false);
                    await transaction.CommitAsync().ConfigureAwait(false);
                    return Redirect(properties.RedirectUri) as ActionResult;
                });

            return signInResult;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error occurred while processing sign-in information");
            await transaction.RollbackAsync();
            return new InternalAuthError
            {
                HttpStatusCode = HttpStatusCode.InternalServerError,
                Message = "Unexpected error occurred while processing sign-in information",
                ErrorGroup = InternalAuthErrorGroup.Database,
            };
        }
    }

    private async Task<Either<IErrorResponse, User>> GetOrCreateUserAsync(UserInfoResponse userInfo)
    {
        try
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
            if (createResult.Succeeded)
            {
                return user;
            }

            if (createResult.Errors.Any(e => e.Code == "DuplicateUserName"))
            {
                logger.LogWarning("Similar User already exists");

                // Retry: find the user again
                var retryUser = await userManager.FindByNameAsync(userInfo.DrfoCode);
                if (retryUser != null)
                {
                    return retryUser;
                }

                user.Id = Guid.NewGuid().ToString();

                // Retry creating the user once more
                createResult = await userManager.CreateAsync(user);
                if (createResult.Succeeded)
                {
                    return user;
                }
            }

            var error = string.Join("; ", createResult.Errors.Select(e => e.Description));
            return new InternalAuthError
            {
                HttpStatusCode = HttpStatusCode.InternalServerError,
                Message = error,
                ErrorGroup = InternalAuthErrorGroup.Logic,
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occured while creating user");
            return new InternalAuthError
            {
                HttpStatusCode = HttpStatusCode.InternalServerError,
                Message = "An error occured while creating user",
                ErrorGroup = InternalAuthErrorGroup.Database,
            };
        }
    }

    private async Task<Either<IErrorResponse, Individual>> GetOrCreateIndividualAsync(UserInfoResponse userInfo,
        User user)
    {
        try
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
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occured while creating individual");
            return new InternalAuthError
            {
                HttpStatusCode = HttpStatusCode.InternalServerError,
                Message = "An error occured while creating individual",
                ErrorGroup = InternalAuthErrorGroup.Database,
            };
        }
    }

    private Either<IErrorResponse, List<Claim>> BuildClaims(
        Individual individual, UserInfoResponse userInfo, AuthenticateResult result)
    {
        try
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.Role, result.Properties.Items[AuthServerConstants.ExternalAuthSelectedRoleKey]),
                new(ClaimTypes.GivenName, individual.FirstName),
                new(ClaimTypes.Surname, individual.LastName),
                new(ClaimTypes.Email, userInfo.Email),
                new(AuthServerConstants.ClaimTypes.Rnkopp, individual.Rnokpp),
                new(OpenIddictConstants.Claims.Private.ProviderName,
                    result.Principal.GetClaim(OpenIddictConstants.Claims.Private.ProviderName)),
                new(OpenIddictConstants.Claims.Private.RegistrationId,
                    result.Principal.GetClaim(OpenIddictConstants.Claims.Private.ProviderName)),
            };

            if (result.Properties.Items[AuthServerConstants.ExternalAuthSelectedRoleKey] == "provider")
            {
                claims.Add(new Claim(AuthServerConstants.ClaimTypes.Edrpou, userInfo.EdrpouCode));
            }

            return claims;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occured while building claims");
            return new InternalAuthError
            {
                HttpStatusCode = HttpStatusCode.InternalServerError,
                Message = "An error occured while building claims",
                ErrorGroup = InternalAuthErrorGroup.Logic,
            };
        }
    }

    private async Task<Either<IErrorResponse, AuthenticationProperties>> SignInWithClaimsAsync(
        AuthenticateResult result, List<Claim> claims)
    {
        var properties = new AuthenticationProperties
        {
            RedirectUri = result.Properties?.RedirectUri ?? "/login",
        };

        try
        {
            // user is still in memory, should be cheap and rnkopp claim should be present if we ever get here
            var user = await userManager.FindByNameAsync(claims
                .First(c => c.Type == AuthServerConstants.ClaimTypes.Rnkopp).Value);
            await signInManager.SignInWithClaimsAsync(user, properties, claims);
            return properties;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occured while signing in with claims");
            return new InternalAuthError
            {
                HttpStatusCode = HttpStatusCode.InternalServerError,
                Message = "An error occured while signing in with claims",
                ErrorGroup = InternalAuthErrorGroup.Logic,
            };
        }
    }
}