using System.Security.Claims;
using OutOfSchool.Common.PermissionsModule;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Repository;

namespace OutOfSchool.AuthorizationServer.Services;

public class ProfileService : IProfileService
{
    private readonly UserManager<User> userManager;
    private readonly IEntityRepository<long, PermissionsForRole> permissionsForRolesRepository;
    private readonly IProviderAdminRepository providerAdminRepository;

    public ProfileService(
        UserManager<User> userManager,
        IEntityRepository<long, PermissionsForRole> permissionsForRolesRepository,
        IProviderAdminRepository providerAdminRepository)
    {
        this.userManager = userManager;
        this.permissionsForRolesRepository = permissionsForRolesRepository;
        this.providerAdminRepository = providerAdminRepository;
    }

    public async Task GetProfileDataAsync(ClaimsPrincipal principal)
    {
        var claims = await GetAdditionalClaimsAsync(principal);
        var claimsList = claims.Select(kvp => new Claim(kvp.Key, kvp.Value)).ToList();

        if (claimsList.Any())
        {
            var identity = principal.Identity as ClaimsIdentity;
            identity?.AddClaims(claimsList);
        }
    }

    public async Task<IReadOnlyDictionary<string, string>> GetAdditionalClaimsAsync(ClaimsPrincipal principal)
    {
        var nameClaim = principal.Claims.FirstOrDefault(claim => claim.Type == "name");
        var roleClaim = principal.Claims.FirstOrDefault(claim => claim.Type == "role");

        var additionalClaims = new Dictionary<string, string>(StringComparer.Ordinal);

        if (nameClaim is not null && roleClaim is not null)
        {
            var userFromLogin = await userManager.FindByNameAsync(nameClaim.Value);

            var subrole = await GetSubroleByUserName(userFromLogin);

            additionalClaims[IdentityResourceClaimsTypes.Permissions] =
                await GetPermissionsForUser(userFromLogin, roleClaim.Value);
            additionalClaims[IdentityResourceClaimsTypes.Subrole] = subrole.ToString();
        }

        return additionalClaims;
    }

    // get's list of permissions for current user's role from db
    private async Task<string> GetPermissionsForUser(User userFromLogin, string roleName)
    {
        if (userFromLogin.Role == nameof(Role.Provider).ToLower() && userFromLogin.IsDerived)
        {
            // ProviderAdmin set of permissions in DB excludes not allowed actions
            roleName += Constants.AdminKeyword;
        }

        var permissionsForUser = (await permissionsForRolesRepository
                .GetByFilter(p => p.RoleName == roleName))
            .FirstOrDefault()?.PackedPermissions;

        // action when no permissions for user's role existes in DB
        return permissionsForUser ?? new List<Permissions>() { Permissions.NotSet }.PackPermissionsIntoString();
    }

    // Get subrole for user
    private async Task<Subrole> GetSubroleByUserName(User userFromLogin)
    {
        if (userFromLogin.Role == nameof(Role.Provider).ToLower() && userFromLogin.IsDerived)
        {
            var userDeputyOrAdmin = await providerAdminRepository
                .GetByFilter(p => p.UserId == userFromLogin.Id)
                .ConfigureAwait(false);

            if (userDeputyOrAdmin.Any(u => u.IsDeputy))
            {
                return Subrole.ProviderDeputy;
            }

            if (userDeputyOrAdmin.Any(u => !u.IsDeputy))
            {
                return Subrole.ProviderAdmin;
            }
        }

        return Subrole.None;
    }
}