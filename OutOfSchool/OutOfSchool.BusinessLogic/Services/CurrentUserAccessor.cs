#nullable enable

using System.Security.Claims;
using OutOfSchool.BusinessLogic.Common;
using OutOfSchool.Common.Models;

namespace OutOfSchool.BusinessLogic.Services;

public class CurrentUserAccessor : ICurrentUser
{
    private readonly ClaimsPrincipal? user;

    public CurrentUserAccessor(ClaimsPrincipal? user)
    {
        this.user = user;
    }

    public string UserId => GettingUserProperties.GetUserId(user) ?? string.Empty;

    public string UserRole => GettingUserProperties.GetUserRole(user) ?? string.Empty;

    public bool IsInRole(string role) => user?.IsInRole(role) ?? false;

    public bool IsAuthenticated => user?.Identity?.IsAuthenticated ?? false;

    public bool HasClaim(string type, Func<string, bool>? valueComparer = null) =>
        user?.Identities
            .Any(identity =>
                identity.HasClaim(claim =>
                    string.Equals(
                        claim.Type,
                        type,
                        StringComparison.OrdinalIgnoreCase) && (valueComparer?.Invoke(claim.Value) ?? true))) ?? false;
}