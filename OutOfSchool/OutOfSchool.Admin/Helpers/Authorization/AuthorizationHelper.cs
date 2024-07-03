using OutOfSchool.Services.Enums;
using System.Security.Claims;

namespace OutOfSchool.Admin.Helpers.Authorization;
public class AuthorizationHelper
{
    public static bool IsTechAdmin(ClaimsPrincipal user) => user.IsInRole(nameof(Role.TechAdmin).ToLower());
}
