using System.Collections.Immutable;
using System.Security.Claims;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Client;
using OpenIddict.Client.AspNetCore;
using OpenIddict.Server.AspNetCore;
using OutOfSchool.AuthCommon;
using OutOfSchool.AuthCommon.Config;
using OutOfSchool.AuthorizationServer.Services;
using OutOfSchool.AuthorizationServer.Util;
using OutOfSchool.AuthorizationServer.ViewModels;

namespace OutOfSchool.AuthorizationServer.Controllers;

public class TokenController : Controller
{
    private readonly IOpenIddictApplicationManager _applicationManager;
    private readonly IOpenIddictAuthorizationManager _authorizationManager;
    private readonly OpenIddictClientService _clientService;
    private readonly IOpenIddictScopeManager _scopeManager;
    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager;
    private readonly AuthServerConfig authorizationServerConfig;
    private readonly IProfileService _profileService;

    public TokenController(
        IOpenIddictApplicationManager applicationManager,
        IOpenIddictAuthorizationManager authorizationManager,
        OpenIddictClientService clientService,
        IOpenIddictScopeManager scopeManager,
        SignInManager<User> signInManager,
        UserManager<User> userManager,
        IOptions<AuthServerConfig> identityServerConfig,
        IProfileService profileService)
    {
        _applicationManager = applicationManager;
        _authorizationManager = authorizationManager;
        _clientService = clientService;
        _scopeManager = scopeManager;
        _signInManager = signInManager;
        _userManager = userManager;
        authorizationServerConfig = identityServerConfig.Value;
        _profileService = profileService;
    }

    #region Authorization code, implicit and hybrid flows

    [HttpGet("~/connect/authorize")]
    [HttpPost("~/connect/authorize")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Authorize()
    {
        var request = HttpContext.GetOpenIddictServerRequest() ??
                      throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        // Try to retrieve the user principal stored in the authentication cookie and redirect
        // the user agent to the login page (or to an external provider) in the following cases:
        //
        //  - If the user principal can't be extracted or the cookie is too old.
        //  - If prompt=login was specified by the client application.
        //  - If a max_age parameter was provided and the authentication cookie is not considered "fresh" enough.
        //
        // For scenarios where the default authentication handler configured in the ASP.NET Core
        // authentication options shouldn't be used, a specific scheme can be specified here.
        var result = await HttpContext.AuthenticateAsync();
        if (result == null || !result.Succeeded || request.HasPrompt(OpenIddictConstants.Prompts.Login) ||
            (request.MaxAge != null && result.Properties?.IssuedUtc != null &&
             DateTimeOffset.UtcNow - result.Properties.IssuedUtc > TimeSpan.FromSeconds(request.MaxAge.Value)))
        {
            // If the client application requested promptless authentication,
            // return an error indicating that the user is not logged in.
            if (request.HasPrompt(OpenIddictConstants.Prompts.None))
            {
                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] =
                            OpenIddictConstants.Errors.LoginRequired,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The user is not logged in."
                    }));
            }

            // To avoid endless login -> authorization redirects, the prompt=login flag
            // is removed from the authorization request payload before redirecting the user.
            var prompt = string.Join(" ", request.GetPrompts().Remove(OpenIddictConstants.Prompts.Login));

            var parameters = Request.HasFormContentType
                ? Request.Form.Where(parameter => parameter.Key != OpenIddictConstants.Parameters.Prompt).ToList()
                : Request.Query.Where(parameter => parameter.Key != OpenIddictConstants.Parameters.Prompt).ToList();

            parameters.Add(new(OpenIddictConstants.Parameters.Prompt, new StringValues(prompt)));

            // For applications that want to allow the client to select the external authentication provider
            // that will be used to authenticate the user, the identity_provider parameter can be used for that.
            if (!string.IsNullOrEmpty(request.IdentityProvider))
            {
                var registrations = await _clientService.GetClientRegistrationsAsync();
                if (!registrations.Any(registration => string.Equals(registration.ProviderName,
                        request.IdentityProvider, StringComparison.Ordinal)))
                {
                    return Forbid(
                        authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                        properties: new AuthenticationProperties(new Dictionary<string, string>
                        {
                            [OpenIddictServerAspNetCoreConstants.Properties.Error] =
                                OpenIddictConstants.Errors.InvalidRequest,
                            [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                                "The specified identity provider is not valid."
                        }));
                }

                var properties = _signInManager.ConfigureExternalAuthenticationProperties(
                    provider: request.IdentityProvider,
                    redirectUrl: Url.Action("ExternalLoginCallback", "Auth", new
                    {
                        ReturnUrl = Request.PathBase + Request.Path + QueryString.Create(parameters)
                    }));

                // Note: when only one client is registered in the client options,
                // specifying the issuer URI or the provider name is not required.
                properties.SetString(OpenIddictClientAspNetCoreConstants.Properties.ProviderName,
                    request.IdentityProvider);

                // Ask the OpenIddict client middleware to redirect the user agent to the identity provider.
                return Challenge(properties, OpenIddictClientAspNetCoreDefaults.AuthenticationScheme);
            }

            // For scenarios where the default challenge handler configured in the ASP.NET Core
            // authentication options shouldn't be used, a specific scheme can be specified here.
            return Challenge(new AuthenticationProperties
            {
                RedirectUri = Request.PathBase + Request.Path + QueryString.Create(parameters)
            });
        }

        // Retrieve the profile of the logged in user.
        var user = await _userManager.GetUserAsync(result.Principal) ??
                   throw new InvalidOperationException("The user details cannot be retrieved.");

        // Retrieve the application details from the database.
        var application = await _applicationManager.FindByClientIdAsync(request.ClientId) ??
                          throw new InvalidOperationException(
                              "Details concerning the calling client application cannot be found.");

        // Retrieve the permanent authorizations associated with the user and the calling client application.
        var authorizations = await _authorizationManager.FindAsync(
            subject: await _userManager.GetUserIdAsync(user),
            client: await _applicationManager.GetIdAsync(application),
            status: OpenIddictConstants.Statuses.Valid,
            type: OpenIddictConstants.AuthorizationTypes.Permanent,
            scopes: request.GetScopes()).ToListAsync();

        switch (await _applicationManager.GetConsentTypeAsync(application))
        {
            // If the consent is external (e.g when authorizations are granted by a sysadmin),
            // immediately return an error if no authorization can be found in the database.
            case OpenIddictConstants.ConsentTypes.External when authorizations.Count is 0:
                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] =
                            OpenIddictConstants.Errors.ConsentRequired,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                            "The logged in user is not allowed to access this client application."
                    }));

            // If the consent is implicit or if an authorization was found,
            // return an authorization response without displaying the consent form.
            case OpenIddictConstants.ConsentTypes.Implicit:
            case OpenIddictConstants.ConsentTypes.External when authorizations.Count is not 0:
            case OpenIddictConstants.ConsentTypes.Explicit when authorizations.Count is not 0 &&
                                                                !request.HasPrompt(OpenIddictConstants.Prompts.Consent):
                // Create the claims-based identity that will be used by OpenIddict to generate tokens.
                var identity = new ClaimsIdentity(
                    authenticationType: TokenValidationParameters.DefaultAuthenticationType,
                    nameType: OpenIddictConstants.Claims.Name,
                    roleType: OpenIddictConstants.Claims.Role);

                // Add the claims that will be persisted in the tokens.
                identity.SetClaim(OpenIddictConstants.Claims.Subject, await _userManager.GetUserIdAsync(user))
                    .SetClaim(OpenIddictConstants.Claims.Email, await _userManager.GetEmailAsync(user))
                    .SetClaim(OpenIddictConstants.Claims.Name, await _userManager.GetUserNameAsync(user))
                    .SetClaim(OpenIddictConstants.Claims.PreferredUsername, await _userManager.GetUserNameAsync(user));

                // TODO: for now only having this private claim is enough, maybe check value later.
                if (result.Principal.HasClaim(OpenIddictConstants.Claims.Private.ProviderName))
                {
                    // Take claims from external principal
                    identity.SetClaims(OpenIddictConstants.Claims.Role, [result.Principal.GetClaim(ClaimTypes.Role)]);
                    identity.SetClaim(OpenIddictConstants.Claims.FamilyName, result.Principal.GetClaim(ClaimTypes.Surname));
                    identity.SetClaim(OpenIddictConstants.Claims.GivenName, result.Principal.GetClaim(ClaimTypes.GivenName));
                    identity.SetClaim(AuthServerConstants.ClaimTypes.Rnokpp, result.Principal.GetClaim(AuthServerConstants.ClaimTypes.Rnokpp));
                    if (result.Principal.HasClaim(AuthServerConstants.ClaimTypes.Edrpou))
                    {
                        identity.SetClaim(AuthServerConstants.ClaimTypes.Edrpou, result.Principal.GetClaim(AuthServerConstants.ClaimTypes.Edrpou));
                    }
                }
                else
                {
                    identity.SetClaims(OpenIddictConstants.Claims.Role,
                            [..await _userManager.GetRolesAsync(user)]);
                }

                await _profileService.GetProfileDataAsync(identity);

                // Note: in this sample, the granted scopes match the requested scope
                // but you may want to allow the user to uncheck specific scopes.
                // For that, simply restrict the list of scopes before calling SetScopes.
                identity.SetScopes(request.GetScopes());
                identity.SetResources(await _scopeManager.ListResourcesAsync(identity.GetScopes()).ToListAsync());

                // Automatically create a permanent authorization to avoid requiring explicit consent
                // for future authorization or token requests containing the same scopes.
                var authorization = authorizations.LastOrDefault();
                authorization ??= await _authorizationManager.CreateAsync(
                    identity: identity,
                    subject: await _userManager.GetUserIdAsync(user),
                    client: await _applicationManager.GetIdAsync(application),
                    type: OpenIddictConstants.AuthorizationTypes.Permanent,
                    scopes: identity.GetScopes());

                identity.SetAuthorizationId(await _authorizationManager.GetIdAsync(authorization));
                identity.SetDestinations(GetDestinations);

                return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            // At this point, no authorization was found in the database and an error must be returned
            // if the client application specified prompt=none in the authorization request.
            case OpenIddictConstants.ConsentTypes.Explicit when request.HasPrompt(OpenIddictConstants.Prompts.None):
            case OpenIddictConstants.ConsentTypes.Systematic when request.HasPrompt(OpenIddictConstants.Prompts.None):
                return Forbid(
                    authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                    properties: new AuthenticationProperties(new Dictionary<string, string>
                    {
                        [OpenIddictServerAspNetCoreConstants.Properties.Error] =
                            OpenIddictConstants.Errors.ConsentRequired,
                        [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                            "Interactive user consent is required."
                    }));

            // In every other case, render the consent form.
            default:
                return View(new AuthorizeViewModel
                {
                    ApplicationName = await _applicationManager.GetLocalizedDisplayNameAsync(application),
                    Scope = request.Scope
                });
        }
    }

    [Authorize]
    [FormValueRequired("submit.Accept")]
    [HttpPost("~/connect/authorize")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Accept()
    {
        var request = HttpContext.GetOpenIddictServerRequest() ??
                      throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        // Retrieve the profile of the logged in user.
        var user = await _userManager.GetUserAsync(User) ??
                   throw new InvalidOperationException("The user details cannot be retrieved.");

        // Retrieve the application details from the database.
        var application = await _applicationManager.FindByClientIdAsync(request.ClientId) ??
                          throw new InvalidOperationException(
                              "Details concerning the calling client application cannot be found.");

        // Retrieve the permanent authorizations associated with the user and the calling client application.
        var authorizations = await _authorizationManager.FindAsync(
            subject: await _userManager.GetUserIdAsync(user),
            client: await _applicationManager.GetIdAsync(application),
            status: OpenIddictConstants.Statuses.Valid,
            type: OpenIddictConstants.AuthorizationTypes.Permanent,
            scopes: request.GetScopes()).ToListAsync();

        // Note: the same check is already made in the other action but is repeated
        // here to ensure a malicious user can't abuse this POST-only endpoint and
        // force it to return a valid response without the external authorization.
        if (authorizations.Count is 0 &&
            await _applicationManager.HasConsentTypeAsync(application, OpenIddictConstants.ConsentTypes.External))
        {
            return Forbid(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(new Dictionary<string, string>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.ConsentRequired,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                        "The logged in user is not allowed to access this client application."
                }));
        }

        // Create the claims-based identity that will be used by OpenIddict to generate tokens.
        var identity = new ClaimsIdentity(
            authenticationType: TokenValidationParameters.DefaultAuthenticationType,
            nameType: OpenIddictConstants.Claims.Name,
            roleType: OpenIddictConstants.Claims.Role);

        // Add the claims that will be persisted in the tokens.
        identity.SetClaim(OpenIddictConstants.Claims.Subject, await _userManager.GetUserIdAsync(user))
            .SetClaim(OpenIddictConstants.Claims.Email, await _userManager.GetEmailAsync(user))
            .SetClaim(OpenIddictConstants.Claims.Name, await _userManager.GetUserNameAsync(user))
            .SetClaim(OpenIddictConstants.Claims.PreferredUsername, await _userManager.GetUserNameAsync(user));

        // If flow reaches this point - user is already signed in with claims
        var signedInIdentity = User.Identity as ClaimsIdentity;
        // TODO: for now only having this private claim is enough, maybe check value later.
        if (signedInIdentity.HasClaim(OpenIddictConstants.Claims.Private.ProviderName))
        {
            // Take claims from external principal
            identity.SetClaims(OpenIddictConstants.Claims.Role, [signedInIdentity.GetClaim(ClaimTypes.Role)]);
            identity.SetClaim(OpenIddictConstants.Claims.FamilyName, signedInIdentity.GetClaim(ClaimTypes.Surname));
            identity.SetClaim(OpenIddictConstants.Claims.GivenName, signedInIdentity.GetClaim(ClaimTypes.GivenName));
            identity.SetClaim(AuthServerConstants.ClaimTypes.Rnokpp, signedInIdentity.GetClaim(AuthServerConstants.ClaimTypes.Rnokpp));
            if (signedInIdentity.HasClaim(AuthServerConstants.ClaimTypes.Edrpou))
            {
                identity.SetClaim(AuthServerConstants.ClaimTypes.Edrpou, signedInIdentity.GetClaim(AuthServerConstants.ClaimTypes.Edrpou));
            }
        }
        else
        {
            identity.SetClaims(OpenIddictConstants.Claims.Role,
                [..await _userManager.GetRolesAsync(user)]);
        }

        await _profileService.GetProfileDataAsync(identity);

        // Note: in this sample, the granted scopes match the requested scope
        // but you may want to allow the user to uncheck specific scopes.
        // For that, simply restrict the list of scopes before calling SetScopes.
        identity.SetScopes(request.GetScopes());
        identity.SetResources(await _scopeManager.ListResourcesAsync(identity.GetScopes()).ToListAsync());

        // Automatically create a permanent authorization to avoid requiring explicit consent
        // for future authorization or token requests containing the same scopes.
        var authorization = authorizations.LastOrDefault();
        authorization ??= await _authorizationManager.CreateAsync(
            identity: identity,
            subject: await _userManager.GetUserIdAsync(user),
            client: await _applicationManager.GetIdAsync(application),
            type: OpenIddictConstants.AuthorizationTypes.Permanent,
            scopes: identity.GetScopes());

        identity.SetAuthorizationId(await _authorizationManager.GetIdAsync(authorization));
        identity.SetDestinations(GetDestinations);

        // Returning a SignInResult will ask OpenIddict to issue the appropriate access/identity tokens.
        return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    [Authorize]
    [FormValueRequired("submit.Deny")]
    [HttpPost("~/connect/authorize")]
    [ValidateAntiForgeryToken]
    // Notify OpenIddict that the authorization grant has been denied by the resource owner
    // to redirect the user agent to the client application using the appropriate response_mode.
    public IActionResult Deny() => Forbid(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    #endregion

    #region Logout support for interactive flows like code and implicit

    [HttpGet("~/connect/logout")]
    public IActionResult Logout() => View();

    [ActionName(nameof(Logout))]
    [HttpPost("~/connect/logout")]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> DoLogout()
    {
        // logger.LogDebug($"{path} started. User(id): {userId}.");
        // Ask ASP.NET Core Identity to delete the local and external cookies created
        // when the user agent is redirected from the external identity provider
        // after a successful authentication flow (e.g Google or Facebook).
        await _signInManager.SignOutAsync();

        // Returning a SignOutResult will ask OpenIddict to redirect the user agent
        // to the post_logout_redirect_uri specified by the client application or to
        // the RedirectUri specified in the authentication properties if none was set.

        // logger.LogInformation($"{path} Successfully logged out. User(id): {userId}");
        return SignOut(
            authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
            properties: new AuthenticationProperties
            {
                RedirectUri = authorizationServerConfig.RedirectToStartPageUrl,
            });
    }
    #endregion

    #region Password, authorization code, device and refresh token flows
    [HttpPost("~/connect/token")]
    [IgnoreAntiforgeryToken]
    [Produces("application/json")]
    public async Task<IActionResult> Exchange()
    {
        var request = HttpContext.GetOpenIddictServerRequest() ??
            throw new InvalidOperationException("The OpenID Connect request cannot be retrieved.");

        if (request.IsPasswordGrantType())
        {
            return await this.HandleExchangePasswordGrantType(request);
        }

        if (request.IsAuthorizationCodeGrantType() || request.IsDeviceCodeGrantType() || request.IsRefreshTokenGrantType())
        {
            return await this.HandleExchangeCodeAndRefreshGrantTypes();
        }

        throw new InvalidOperationException("The specified grant type is not supported.");
    }

    private async Task<IActionResult> HandleExchangeCodeAndRefreshGrantTypes()
    {
        // Retrieve the claims principal stored in the authorization code/device code/refresh token.
        var result = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

        // Retrieve the user profile corresponding to the authorization code/refresh token.
        var user = await _userManager.FindByIdAsync(result.Principal.GetClaim(OpenIddictConstants.Claims.Subject));
        if (user is null)
        {
            return this.Forbid(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(new Dictionary<string, string>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidGrant,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The token is no longer valid."
                }));
        }

        // Ensure the user is still allowed to sign in.
        if (!await _signInManager.CanSignInAsync(user))
        {
            return this.Forbid(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(new Dictionary<string, string>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidGrant,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The user is no longer allowed to sign in."
                }));
        }

        var identity = new ClaimsIdentity(result.Principal.Claims,
            authenticationType: TokenValidationParameters.DefaultAuthenticationType,
            nameType: OpenIddictConstants.Claims.Name,
            roleType: OpenIddictConstants.Claims.Role);

        // Override the user claims present in the principal in case they
        // changed since the authorization code/refresh token was issued.
        identity.SetClaim(OpenIddictConstants.Claims.Subject, await _userManager.GetUserIdAsync(user))
            .SetClaim(OpenIddictConstants.Claims.Email, await _userManager.GetEmailAsync(user))
            .SetClaim(OpenIddictConstants.Claims.Name, await _userManager.GetUserNameAsync(user))
            .SetClaim(OpenIddictConstants.Claims.PreferredUsername, await _userManager.GetUserNameAsync(user))
            .SetClaims(OpenIddictConstants.Claims.Role, (await _userManager.GetRolesAsync(user)).ToImmutableArray());

        identity.SetDestinations(this.GetDestinations);

        // Returning a SignInResult will ask OpenIddict to issue the appropriate access/identity tokens.
        return this.SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    private async Task<IActionResult> HandleExchangePasswordGrantType(OpenIddictRequest request)
    {
        var user = await _userManager.FindByNameAsync(request.Username);
        if (user is null)
        {
            return this.Forbid(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(new Dictionary<string, string>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidGrant,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The username/password couple is invalid."
                }));
        }

        // Validate the username/password parameters and ensure the account is not locked out.
        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            return this.Forbid(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(new Dictionary<string, string>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidGrant,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] = "The username/password couple is invalid."
                }));
        }

        // Create the claims-based identity that will be used by OpenIddict to generate tokens.
        var identity = new ClaimsIdentity(
            authenticationType: TokenValidationParameters.DefaultAuthenticationType,
            nameType: OpenIddictConstants.Claims.Name,
            roleType: OpenIddictConstants.Claims.Role);

        // Add the claims that will be persisted in the tokens.
        identity.SetClaim(OpenIddictConstants.Claims.Subject, await _userManager.GetUserIdAsync(user))
            .SetClaim(OpenIddictConstants.Claims.Email, await _userManager.GetEmailAsync(user))
            .SetClaim(OpenIddictConstants.Claims.Name, await _userManager.GetUserNameAsync(user))
            .SetClaim(OpenIddictConstants.Claims.PreferredUsername, await _userManager.GetUserNameAsync(user))
            .SetClaims(OpenIddictConstants.Claims.Role, (await _userManager.GetRolesAsync(user)).ToImmutableArray());

        // Note: in this sample, the granted scopes match the requested scope
        // but you may want to allow the user to uncheck specific scopes.
        // For that, simply restrict the list of scopes before calling SetScopes.
        identity.SetScopes(request.GetScopes());
        identity.SetResources(await _scopeManager.ListResourcesAsync(identity.GetScopes()).ToListAsync());
        identity.SetDestinations(this.GetDestinations);

        // Returning a SignInResult will ask OpenIddict to issue the appropriate access/identity tokens.
        return this.SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    #endregion

    private IEnumerable<string> GetDestinations(Claim claim)
    {
        // Note: by default, claims are NOT automatically included in the access and identity tokens.
        // To allow OpenIddict to serialize them, you must attach them a destination, that specifies
        // whether they should be included in access tokens, in identity tokens or in both.

        switch (claim.Type)
        {
            case OpenIddictConstants.Claims.Name or OpenIddictConstants.Claims.PreferredUsername:
                yield return OpenIddictConstants.Destinations.AccessToken;

                if (claim.Subject.HasScope(OpenIddictConstants.Permissions.Scopes.Profile))
                {
                    yield return OpenIddictConstants.Destinations.IdentityToken;
                }

                yield break;

            case OpenIddictConstants.Claims.Email:
                yield return OpenIddictConstants.Destinations.AccessToken;

                if (claim.Subject.HasScope(OpenIddictConstants.Permissions.Scopes.Email))
                {
                    yield return OpenIddictConstants.Destinations.IdentityToken;
                }

                yield break;

            case OpenIddictConstants.Claims.Role:
                yield return OpenIddictConstants.Destinations.AccessToken;

                if (claim.Subject.HasScope(OpenIddictConstants.Permissions.Scopes.Roles))
                {
                    yield return OpenIddictConstants.Destinations.IdentityToken;
                }

                yield break;

            // Never include the security stamp in the access and identity tokens, as it's a secret value.
            case "AspNet.Identity.SecurityStamp": yield break;

            default:
                yield return OpenIddictConstants.Destinations.AccessToken;
                yield break;
        }
    }
}