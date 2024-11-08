using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using OutOfSchool.AuthorizationServer.Services;

namespace OutOfSchool.AuthorizationServer.Controllers;

public class UserinfoController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly IProfileService _profileService;

    public UserinfoController(UserManager<User> userManager, IProfileService profileService)
    {
        _userManager = userManager;
        _profileService = profileService;
    }

    // GET: /api/userinfo
    [Authorize(AuthenticationSchemes = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)]
    [HttpGet("~/connect/userinfo")]
    [HttpPost("~/connect/userinfo")]
    [Produces("application/json")]
    public async Task<IActionResult> Userinfo()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Challenge(
                authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                properties: new AuthenticationProperties(new Dictionary<string, string>
                {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = OpenIddictConstants.Errors.InvalidToken,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                        "The specified access token is bound to an account that no longer exists."
                }));
        }

        var claims = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            // Note: the "sub" claim is a mandatory claim and must be included in the JSON response.
            [OpenIddictConstants.Claims.Subject] = await _userManager.GetUserIdAsync(user),
        };

        // TODO: Stripping the prefix for now, as it does not work as expected
        if (User.HasScope(RemovePrefixInScope(OpenIddictConstants.Permissions.Scopes.Email)))
        {
            claims[OpenIddictConstants.Claims.Email] = await _userManager.GetEmailAsync(user);
            claims[OpenIddictConstants.Claims.EmailVerified] = await _userManager.IsEmailConfirmedAsync(user);
        }

        if (User.HasScope(RemovePrefixInScope(OpenIddictConstants.Permissions.Scopes.Profile)))
        {
            claims[OpenIddictConstants.Claims.Name] = await _userManager.GetEmailAsync(user);
        }

        if (User.HasScope(RemovePrefixInScope(OpenIddictConstants.Permissions.Scopes.Phone)))
        {
            claims[OpenIddictConstants.Claims.PhoneNumber] = await _userManager.GetPhoneNumberAsync(user);
            claims[OpenIddictConstants.Claims.PhoneNumberVerified] = await _userManager.IsPhoneNumberConfirmedAsync(user);
        }

        if (User.HasScope(RemovePrefixInScope(OpenIddictConstants.Permissions.Scopes.Roles)))
        {
            // User can have only one role
            var roles = await _userManager.GetRolesAsync(user);
            claims[OpenIddictConstants.Claims.Role] = roles?.FirstOrDefault() ?? string.Empty;
        }

        if (User.HasScope(RemovePrefixInScope(OpenIddictConstants.Permissions.Prefixes.Scope + "outofschoolapi")))
        {
            var additionalClaims = await _profileService.GetAdditionalClaimsAsync(User.Identities.First());

            foreach (var c in additionalClaims)
            {
                claims.Add(c.Key, c.Value);
            }
        }

        // Note: the complete list of standard claims supported by the OpenID Connect specification
        // can be found here: http://openid.net/specs/openid-connect-core-1_0.html#StandardClaims

        return Ok(claims);
    }

    private static string RemovePrefixInScope(string scope)
    {
        return Regex.Replace(scope, $"^{OpenIddictConstants.Permissions.Prefixes.Scope}", string.Empty);
    }
}