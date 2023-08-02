using System.Security.Authentication;
using System.Security.Claims;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.WebApi.Common;

public static class GettingUserProperties
{
    public static string GetUserId(HttpContext httpContext)
    {
        var userId = GetUserId(httpContext?.User);

        if (userId is null)
        {
            ThrowAuthenticationException(nameof(IdentityResourceClaimsTypes.Sub));
        }

        return userId;
    }

    public static string GetUserId(ClaimsPrincipal user)
    {
        return user?.GetUserPropertyByClaimType(IdentityResourceClaimsTypes.Sub);
    }

    public static Role GetUserRole(HttpContext httpContext)
    {
        var userRoleName = GetUserRole(httpContext?.User);

        if (userRoleName is null)
        {
            ThrowAuthenticationException(nameof(IdentityResourceClaimsTypes.Role));
        }

        Role userRole = (Role)Enum.Parse(typeof(Role), userRoleName, true);

        return userRole;
    }

    public static string GetUserRole(ClaimsPrincipal user)
    {
        return user?.GetUserPropertyByClaimType(IdentityResourceClaimsTypes.Role);
    }

    public static Subrole GetUserSubrole(HttpContext httpContext)
    {
        var userSubroleName = GetUserSubrole(httpContext?.User);

        if (userSubroleName is null)
        {
            ThrowAuthenticationException(nameof(IdentityResourceClaimsTypes.Subrole));
        }

        Subrole userSubrole = (Subrole)Enum.Parse(typeof(Subrole), userSubroleName, true);

        return userSubrole;
    }

    public static string GetUserSubrole(ClaimsPrincipal user)
    {
        return user?.GetUserPropertyByClaimType(IdentityResourceClaimsTypes.Subrole);
    }

    private static void ThrowAuthenticationException(string claimType)
        => throw new AuthenticationException($"Can not get user's claim {claimType} from Context.");
}