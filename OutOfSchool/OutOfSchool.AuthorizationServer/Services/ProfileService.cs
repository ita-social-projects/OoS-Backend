using System.Security.Claims;
using OpenIddict.Abstractions;
using OutOfSchool.Common.PermissionsModule;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Services.Repository.Base.Api;

namespace OutOfSchool.AuthorizationServer.Services;

public class ProfileService : IProfileService
{
    private readonly UserManager<User> userManager;
    private readonly IEntityRepository<long, PermissionsForRole> permissionsForRolesRepository;

    public ProfileService(
        UserManager<User> userManager,
        IEntityRepository<long, PermissionsForRole> permissionsForRolesRepository)
    {
        this.userManager = userManager;
        this.permissionsForRolesRepository = permissionsForRolesRepository;
    }

    public async Task GetProfileDataAsync(ClaimsIdentity identity)
    {
        var claims = await GetAdditionalClaimsAsync(identity);
        var claimsList = claims.Select(kvp => new Claim(kvp.Key, kvp.Value)).ToList();

        identity.AddClaims(claimsList);
    }

    public async Task<IReadOnlyDictionary<string, string>> GetAdditionalClaimsAsync(ClaimsIdentity identity)
    {
        var nameClaim = identity.Claims.FirstOrDefault(claim => claim.Type == OpenIddictConstants.Claims.Name);
        var roleClaim = identity.Claims.FirstOrDefault(claim => claim.Type == OpenIddictConstants.Claims.Role);

        var additionalClaims = new Dictionary<string, string>(StringComparer.Ordinal);

        if (nameClaim is not null && roleClaim is not null)
        {
            var userFromLogin = await userManager.FindByNameAsync(nameClaim.Value);

            additionalClaims[IdentityResourceClaimsTypes.Permissions] =
                await GetPermissionsForUser(userFromLogin, roleClaim.Value);
        }

        return additionalClaims;
    }

    // gets list of permissions for current user's role from db
    private async Task<string> GetPermissionsForUser(User userFromLogin, string roleName)
    {
        var permissionsForUser = (await permissionsForRolesRepository
                .GetByFilter(p => p.RoleName == roleName))
            .FirstOrDefault()?.PackedPermissions;

        // action when no permissions for user's role existes in DB
        return permissionsForUser ?? new List<Permissions>() { Permissions.NotSet }.PackPermissionsIntoString();
    }
}