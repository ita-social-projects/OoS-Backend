using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using OutOfSchool.Common.PermissionsModule;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Repository;

namespace OutOfSchool.AuthorizationServer.Services;

public class CustomClaimsSingInManager : SignInManager<User>
{
    private readonly IEntityRepository<long, PermissionsForRole> permissionsForRolesRepository;
    private readonly IProviderAdminRepository providerAdminRepository;

    public CustomClaimsSingInManager(
        UserManager<User> userManager,
        IHttpContextAccessor contextAccessor,
        IUserClaimsPrincipalFactory<User> claimsFactory,
        IOptions<IdentityOptions> optionsAccessor,
        ILogger<SignInManager<User>> logger,
        IAuthenticationSchemeProvider schemes,
        IUserConfirmation<User> confirmation,
        IEntityRepository<long, PermissionsForRole> permissionsForRolesRepository,
        IProviderAdminRepository providerAdminRepository)
        : base(
        userManager,
        contextAccessor,
        claimsFactory,
        optionsAccessor,
        logger,
        schemes,
        confirmation)
    {
        this.permissionsForRolesRepository = permissionsForRolesRepository;
        this.providerAdminRepository = providerAdminRepository;
    }

    public override async Task<ClaimsPrincipal> CreateUserPrincipalAsync(User user)
    {
        var principal = await base.CreateUserPrincipalAsync(user);
        var roleClaim = principal.Claims.FirstOrDefault(claim => claim.Type == "role");

        var permissionsClaim = new Claim(IdentityResourceClaimsTypes.Permissions, await GetPermissionsForUser(user, roleClaim.Value));

        var subrole = await GetSubroleByUserName(user);
        var subRoleClaim = new Claim(IdentityResourceClaimsTypes.Subrole, subrole.ToString());
        principal.Claims.Append(permissionsClaim).Append(subRoleClaim);
        return principal;
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
            .FirstOrDefault().PackedPermissions;

        // action when no permissions for user's role existes in DB
        if (permissionsForUser == null)
        {
            return new List<Permissions>() { Permissions.NotSet }.PackPermissionsIntoString();
        }

        return permissionsForUser;
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