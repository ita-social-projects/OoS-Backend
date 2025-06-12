using System.Security.Claims;
using OpenIddict.Abstractions;
using OutOfSchool.Common.Enums;
using OutOfSchool.Common.PermissionsModule;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Repository.Base.Api;

namespace OutOfSchool.AuthorizationServer.Services;

public class ProfileService : IProfileService
{
    private readonly UserManager<User> userManager;
    private readonly IEntityRepository<long, PermissionsForRole> permissionsForRolesRepository;
    private readonly OutOfSchoolDbContext dbContext;

    public ProfileService(
        UserManager<User> userManager,
        IEntityRepository<long, PermissionsForRole> permissionsForRolesRepository,
        OutOfSchoolDbContext dbContext)
    {
        this.userManager = userManager;
        this.permissionsForRolesRepository = permissionsForRolesRepository;
        this.dbContext = dbContext;
    }

    public async Task<IReadOnlyDictionary<string, string>> GetAdditionalClaimsForRoleAsync(Claim? roleClaim)
    {
        var additionalClaims = new Dictionary<string, string>(StringComparer.Ordinal);

        if (roleClaim is not null)
        {
            // Always fetch fresh permissions
            var permissionsForUser = (await permissionsForRolesRepository
                    .GetByFilter(p => p.RoleName == roleClaim.Value))
                .FirstOrDefault()?.PackedPermissions;

            additionalClaims[IdentityResourceClaimsTypes.Permissions] =
                permissionsForUser ?? new List<Permissions> { Permissions.NotSet }.PackPermissionsIntoString();

            // If provider role, also fetch provider-specific claims
            if (roleClaim.Value.Equals(Role.Provider.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                var providerClaims = await GetProviderClaimsAsync(roleClaim.Subject);
                foreach (var claim in providerClaims)
                {
                    additionalClaims[claim.Key] = claim.Value;
                }
            }

            // If TechAdmin or Moderator role, add IndividualId claim if not already present
            else if (roleClaim.Value.Equals(Role.TechAdmin.ToString(), StringComparison.OrdinalIgnoreCase) ||
                     roleClaim.Value.Equals(Role.Moderator.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                var techStaffClaims = await GetTechStaffClaimsAsync(roleClaim.Subject);
                foreach (var claim in techStaffClaims)
                {
                    additionalClaims[claim.Key] = claim.Value;
                }
            }
        }

        return additionalClaims;
    }

    private async Task<IReadOnlyDictionary<string, string>> GetProviderClaimsAsync(ClaimsIdentity? identity)
    {
        var claims = new Dictionary<string, string>(StringComparer.Ordinal);

        if (identity?.Name is null)
        {
            return claims;
        }

        var user = await userManager.FindByNameAsync(identity.Name);
        if (user == null)
        {
            return claims;
        }

        var individual = await dbContext.Individuals
            .FirstOrDefaultAsync(i => !i.IsDeleted && i.UserId == user.Id);

        if (individual != null)
        {
            var positions = await dbContext.Positions
                .Include(p => p.Provider)
                .Where(x => !x.IsDeleted && 
                    !x.Provider.IsDeleted && 
                    x.Officials.Any(o => o.IndividualId == individual.Id && !o.IsDeleted))
                .Select(p => new 
                { 
                    p.Provider.Edrpou,
                    p.ProviderId,
                    p.PositionType 
                })
                .ToListAsync();

            if (positions.Any())
            {
                var providerId = positions.First().ProviderId;
                var isDeputy = positions.Any(p => p.PositionType == PositionType.DeputyDirector);
                var edrpou = positions.First().Edrpou;

                claims[Constants.ClaimTypes.Rnokpp] = individual.Rnokpp;
                claims[Constants.ClaimTypes.Edrpou] = edrpou;
                claims[Constants.ClaimTypes.ProviderId] = providerId.ToString();
                claims[Constants.ClaimTypes.IsDeputy] = isDeputy.ToString();
            }
        }

        return claims;
    }

    private async Task<IReadOnlyDictionary<string, string>> GetTechStaffClaimsAsync(ClaimsIdentity? identity)
    {
        var claims = new Dictionary<string, string>(StringComparer.Ordinal);

        if (identity?.Name is null)
        {
            return claims;
        }

        var user = await userManager.FindByNameAsync(identity.Name);
        if (user == null)
        {
            return claims;
        }

        var individual = await dbContext.Individuals
            .FirstOrDefaultAsync(i => !i.IsDeleted && i.UserId == user.Id);

        if (individual != null)
        {
            claims[Constants.ClaimTypes.IndividualId] = individual.Id.ToString();
        }

        return claims;
    }

    /// <inheritdoc />
    public async Task EnsureRequiredIdentityClaimsAsync(
        ClaimsIdentity identityToPopulate,
        ClaimsPrincipal existingPrincipal,
        User user)
    {
        // Copy existing claims
        var claimsToCopy = new[]
        {
            OpenIddictConstants.Claims.Role,
            OpenIddictConstants.Claims.FamilyName,
            OpenIddictConstants.Claims.GivenName,
            Constants.ClaimTypes.Rnokpp,
            Constants.ClaimTypes.IndividualId,
            Constants.ClaimTypes.Edrpou,
            Constants.ClaimTypes.ProviderId,
            Constants.ClaimTypes.IsDeputy,
            Constants.ClaimTypes.AikomProviderId,
        };

        foreach (var claimType in claimsToCopy)
        {
            var claim = existingPrincipal.FindFirst(claimType);
            if (claim != null)
            {
                identityToPopulate.SetClaim(claimType, claim.Value, claim.ValueType);
            }
        }

        // Ensure role claim exists
        if (!identityToPopulate.HasClaim(c => c.Type == OpenIddictConstants.Claims.Role))
        {
            identityToPopulate.SetClaims(OpenIddictConstants.Claims.Role,
                [..await userManager.GetRolesAsync(user)]);
        }

        // Get role claim for additional claims
        var roleClaim = identityToPopulate.Claims.FirstOrDefault(c => c.Type == OpenIddictConstants.Claims.Role);
        if (roleClaim != null)
        {
            // Get permissions and provider claims
            var additionalClaims = await GetAdditionalClaimsForRoleAsync(roleClaim);
            
            // Add all additional claims to identity
            foreach (var claim in additionalClaims)
            {
                if (!identityToPopulate.HasClaim(c => c.Type == claim.Key))
                {
                    identityToPopulate.AddClaim(new Claim(claim.Key, claim.Value));
                }
            }
        }
    }
}