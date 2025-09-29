using Microsoft.AspNetCore.Http;
using OutOfSchool.Common.Extensions;
using System;
using System.Linq;
using System.Security.Claims;

namespace OutOfSchool.Common.Models;
/// <summary>
/// Returns id of current web user, or system user id if running in system context.
/// </summary>
public class ContextAwareCurrentUser : IContextAwareCurrentUser
{
    private readonly IHttpContextAccessor httpContextAccessor;

    public ContextAwareCurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        this.httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    public bool IsSystemContext => httpContextAccessor.HttpContext is null;

    public ClaimsPrincipal Principal => httpContextAccessor.HttpContext?.User;

    public string UserId
    {
        get
        {
            if (IsAuthenticated)
            {
                var id = Principal?.GetUserPropertyByClaimType(IdentityResourceClaimsTypes.Sub)
                    ?? Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (!string.IsNullOrWhiteSpace(id))
                    return id;
            }

            if (IsSystemContext)
                return Constants.SystemUserConstants.SystemUserId;

            return string.Empty;
        }
    }
    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;
    public string UserRole => Principal?.GetUserPropertyByClaimType(IdentityResourceClaimsTypes.Role) ?? string.Empty;
    public string GetClaimValue(string claimType) => Principal?.FindFirst(claimType)?.Value;
    public bool IsInRole(string role) => Principal?.IsInRole(role) ?? false;
    public bool HasClaim(string type, Func<string, bool> valueComparer = null) =>
        Principal?.Identities
            .Any(identity =>
                identity.HasClaim(claim =>
                    string.Equals(
                        claim.Type,
                        type,
                        StringComparison.OrdinalIgnoreCase) && (valueComparer?.Invoke(claim.Value) ?? true))) ?? false;
}