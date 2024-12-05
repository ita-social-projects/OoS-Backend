using System.Security.Claims;
using OutOfSchool.Common.PermissionsModule;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Repository.Api;
using OutOfSchool.Services.Repository.Base.Api;

namespace OutOfSchool.AuthorizationServer.Services;

public class ProfileService : IProfileService
{
    private readonly UserManager<User> userManager;
    private readonly IEntityRepository<long, PermissionsForRole> permissionsForRolesRepository;
    private readonly IEmployeeRepository employeeRepository;

    public ProfileService(
        UserManager<User> userManager,
        IEntityRepository<long, PermissionsForRole> permissionsForRolesRepository,
        IEmployeeRepository employeeRepository)
    {
        this.userManager = userManager;
        this.permissionsForRolesRepository = permissionsForRolesRepository;
        this.employeeRepository = employeeRepository;
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

            additionalClaims[IdentityResourceClaimsTypes.Permissions] =
                await GetPermissionsForUser(userFromLogin, roleClaim.Value);
        }

        return additionalClaims;
    }

    // get's list of permissions for current user's role from db
    private async Task<string> GetPermissionsForUser(User userFromLogin, string roleName)
    {
        var permissionsForUser = (await permissionsForRolesRepository
                .GetByFilter(p => p.RoleName == roleName))
            .FirstOrDefault()?.PackedPermissions;

        // action when no permissions for user's role existes in DB
        return permissionsForUser ?? new List<Permissions>() { Permissions.NotSet }.PackPermissionsIntoString();
    }
}