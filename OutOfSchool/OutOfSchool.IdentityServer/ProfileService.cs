using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;
using OutOfSchool.Common;
using OutOfSchool.Common.Extensions;
using OutOfSchool.Common.PermissionsModule;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;

namespace OutOfSchool.IdentityServer
{
    public class ProfileService : IProfileService
    {
        private readonly UserManager<User> userManager;
        private readonly IEntityRepository<PermissionsForRole> permissionsForRolesRepository;
        private readonly IProviderAdminRepository providerAdminRepository;

        public ProfileService(
            UserManager<User> userManager,
            IEntityRepository<PermissionsForRole> permissionsForRolesRepository,
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
            var permissionsClaim = new Claim(IdentityResourceClaimsTypes.Permissions, await GetPermissionsForUser(nameClaim.Value, roleClaim.Value));

            var subrole = await GetSubroleByUserName(nameClaim.Value, roleClaim.Value);
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
        private async Task<string> GetPermissionsForUser(string userName, string roleName)
        {
            var userToLogin = await userManager.FindByNameAsync(userName);

            if (userToLogin.Role == nameof(Role.Provider).ToLower() && userToLogin.IsDerived)
            {
                // ProviderAdmin set of permissions in DB excludes not allowed actions
                roleName += nameof(Role.Admin);
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
        private async Task<Subrole> GetSubroleByUserName(string userName, string roleName)
        {
            var userToLogin = await userManager.FindByNameAsync(userName);

            if (userToLogin.Role == nameof(Role.Provider).ToLower() && userToLogin.IsDerived)
            {
                var userDeputyOrAdmin = await providerAdminRepository
                    .GetByFilter(p => p.UserId == userToLogin.Id)
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
}