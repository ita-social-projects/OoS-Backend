using Microsoft.AspNetCore.Http;
using OutOfSchool.Services.Enums;
using System.Security.Claims;
using System;

namespace OutOfSchool.Tests.Common;

public static class TestExtensions
{
    public static void SetContextUser(this HttpContext context, Role role = Role.Provider, string userId = null)
    {
        var userUserId = string.IsNullOrEmpty(userId) ? Guid.NewGuid().ToString() : userId;

        var user = new ClaimsPrincipal(new ClaimsIdentity(
            new Claim[]
            {
                new Claim(ClaimTypes.Role, role.ToString().ToLower()),
                new Claim("sub", userUserId),
            }));

        context.User = user;
    }
}
