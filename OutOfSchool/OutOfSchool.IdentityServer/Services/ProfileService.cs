using System.Security.Claims;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using OutOfSchool.Services.Enums;

namespace OutOfSchool.IdentityServer.Services;

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

    public async Task GetProfileDataAsync(ProfileDataRequestContext context)
    {
        var nameClaim = context.Subject.Claims.FirstOrDefault(claim => claim.Type == "name");
        var roleClaim = context.Subject.Claims.FirstOrDefault(claim => claim.Type == "role");

        var userFromLogin = await userManager.FindByNameAsync(nameClaim?.Value);

        var permissionsClaim = new Claim(IdentityResourceClaimsTypes.Permissions, await GetPermissionsForUser(userFromLogin, roleClaim?.Value));

        var subrole = await GetProviderSubRoleByUserName(userFromLogin);
        var subRoleClaim = new Claim(IdentityResourceClaimsTypes.Subrole, subrole.ToString());

        var claims = new List<Claim>
    {
        nameClaim,
        roleClaim,
        permissionsClaim,
        subRoleClaim,
    };

        context.IssuedClaims.AddRange(claims);
    }

    public async Task IsActiveAsync(IsActiveContext context)
    {
        var sub = context.Subject.GetSubjectId();
        var user = await userManager.FindByIdAsync(sub);
        context.IsActive = user != null;
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
        if (permissionsForUser == null)
        {
            return new List<Permissions>() { Permissions.NotSet }.PackPermissionsIntoString();
        }

        return permissionsForUser;
    }

    // Get subrole for user
    private async Task<ProviderSubRole> GetProviderSubRoleByUserName(User userFromLogin)
    {
        if (userFromLogin.Role == nameof(Role.Provider).ToLower() && userFromLogin.IsDerived)
        {
            var userDeputyOrAdmin = (await providerAdminRepository
                .GetByFilter(p => p.UserId == userFromLogin.Id))
                .ToList();

            if (userDeputyOrAdmin.Any(u => u.IsDeputy))
            {
                return ProviderSubRole.Deputy;
            }

            if (userDeputyOrAdmin.Any(u => !u.IsDeputy))
            {
                return ProviderSubRole.Manager;
            }
        }

        return ProviderSubRole.Provider;
    }
}