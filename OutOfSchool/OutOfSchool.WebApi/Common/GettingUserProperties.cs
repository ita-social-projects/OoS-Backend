using System;
using System.Security.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using OutOfSchool.Common;
using OutOfSchool.Common.Extensions;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.WebApi.Common
{
    public static class GettingUserProperties
    {
        public static string GetUserId(HttpContext httpContext)
        {
            var userId = GetUserId(httpContext?.User)
                ?? throw new AuthenticationException($"Can not get user's claim {nameof(IdentityResourceClaimsTypes.Sub)} from HttpContext.");

            return userId;
        }

        public static string GetUserId(ClaimsPrincipal user)
        {
            return user.GetUserPropertyByClaimType(IdentityResourceClaimsTypes.Sub);
        }

        public static Role GetUserRole(HttpContext httpContext)
        {
            var userRoleName = GetUserRole(httpContext?.User)
                ?? throw new AuthenticationException($"Can not get user's claim {nameof(IdentityResourceClaimsTypes.Role)} from HttpContext.");

            Role userRole = (Role)Enum.Parse(typeof(Role), userRoleName, true);

            return userRole;
        }

        public static string GetUserRole(ClaimsPrincipal user)
        {
            return user.GetUserPropertyByClaimType(IdentityResourceClaimsTypes.Role);
        }

        public static Subrole GetUserSubrole(HttpContext httpContext)
        {
            var userSubroleName = GetUserSubrole(httpContext?.User)
                ?? throw new AuthenticationException($"Can not get user's claim {nameof(IdentityResourceClaimsTypes.Subrole)} from HttpContext.");

            Subrole userSubrole = (Subrole)Enum.Parse(typeof(Subrole), userSubroleName, true);

            return userSubrole;
        }

        public static string GetUserSubrole(ClaimsPrincipal user)
        {
            return user.GetUserPropertyByClaimType(IdentityResourceClaimsTypes.Subrole);
        }
    }
}
