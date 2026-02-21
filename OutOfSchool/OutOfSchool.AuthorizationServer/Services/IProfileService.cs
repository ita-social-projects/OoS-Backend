using System.Security.Claims;

namespace OutOfSchool.AuthorizationServer.Services;

public interface IProfileService
{
    public Task<IReadOnlyDictionary<string, string>> GetAdditionalClaimsForRoleAsync(Claim? roleClaim);

    /// <summary>
    /// Sets essential identity claims required for proper token generation and user identification.
    /// This method MUST be called when creating or refreshing tokens to ensure proper claim propagation,
    /// especially for external authentication scenarios.
    /// </summary>
    /// <param name="identityToPopulate">The identity to populate with claims</param>
    /// <param name="existingPrincipal">The existing principal containing source claims</param>
    /// <param name="user">The user entity for retrieving roles when needed</param>
    /// <remarks>
    /// This method is critical for maintaining security context and MUST NOT be removed or modified without careful consideration.
    /// It handles both internal and external authentication scenarios, ensuring proper claim propagation.
    /// 
    /// Usage requirements:
    /// - Must be called after basic claims (Subject, Email, etc.) are set
    /// - Must be called before token generation
    /// - Must be called in all token generation/refresh scenarios
    /// </remarks>
    public Task EnsureRequiredIdentityClaimsAsync(ClaimsIdentity identityToPopulate, ClaimsPrincipal existingPrincipal, User user);
}