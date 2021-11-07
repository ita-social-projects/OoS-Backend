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
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;

namespace OutOfSchool.IdentityServer
{
    public class ProfileService : IProfileService
    {
        private readonly UserManager<User> userManager;
        private readonly IEntityRepository<PermissionsForRole> repository;

        public ProfileService(UserManager<User> userManager, IEntityRepository<PermissionsForRole> repository)
        {
            this.userManager = userManager;
            this.repository = repository;
        }

        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var nameClaim = context.Subject.Claims.FirstOrDefault(claim => claim.Type == "name");
            var roleClaim = context.Subject.Claims.FirstOrDefault(claim => claim.Type == "role");
            var permissionsClaim = new Claim(IdentityResourceClaimsTypes.Permissions, await GetPermissionsForRole(roleClaim.Value));
            var claims = new List<Claim>
            {
                nameClaim,
                roleClaim,
                permissionsClaim,
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
        private async Task<string> GetPermissionsForRole(string roleName)
        {
            var permissionsForRole = (await repository.GetByFilter(p => p.RoleName == roleName)).FirstOrDefault();
            if (permissionsForRole == null)
            {
                return new List<Permissions>() { Permissions.NotSet }.PackPermissionsIntoString();
            }

            return permissionsForRole.PackedPermissions;
        }
    }
}