using System.Security.Claims;

namespace OutOfSchool.AuthorizationServer.Services;

// TODO: Implement IsActive like in IdentityServer. Or don't :)
public interface IProfileService
{
    public Task GetProfileDataAsync(ClaimsPrincipal principal);

    public Task<IReadOnlyDictionary<string, string>> GetAdditionalClaimsAsync(ClaimsPrincipal principal);
}