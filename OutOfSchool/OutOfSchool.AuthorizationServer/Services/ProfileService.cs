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
        var nameClaim = principal.Claims.FirstOrDefault(claim => claim.Type == "name");
        var roleClaim = principal.Claims.FirstOrDefault(claim => claim.Type == "role");

        if (nameClaim is not null && roleClaim is not null)
        {
            var userFromLogin = await userManager.FindByNameAsync(nameClaim.Value);

            var permissionsClaim = new Claim(IdentityResourceClaimsTypes.Permissions, await GetPermissionsForUser(userFromLogin, roleClaim.Value));

            var subrole = await GetSubroleByUserName(userFromLogin);
            var subRoleClaim = new Claim(IdentityResourceClaimsTypes.Subrole, subrole.ToString());

            var claims = new List<Claim>
            {
                permissionsClaim,
                subRoleClaim,
            };

            var identity = principal.Identity as ClaimsIdentity;
            identity?.AddClaims(claims);
        }
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