using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Identity;
using OutOfSchool.Services.Models;

namespace OutOfSchool.IdentityServer
{
    public class ProfileService : IProfileService
    {
        protected UserManager<User> userManager;

        public ProfileService(UserManager<User> userManager)
        {
            this.userManager = userManager;
        }

        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var user = await userManager.GetUserAsync(context.Subject);

            var claims = new List<Claim>
            {
                context.Subject.Claims.FirstOrDefault(claim => claim.Type == "name"),
                context.Subject.Claims.FirstOrDefault(claim => claim.Type == "role"),
            };

            context.IssuedClaims.AddRange(claims);
        }

        public async Task IsActiveAsync(IsActiveContext context)
        {
            var sub = context.Subject.GetSubjectId();
            var user = await userManager.FindByIdAsync(sub);
            context.IsActive = user != null;
        }
    }
}