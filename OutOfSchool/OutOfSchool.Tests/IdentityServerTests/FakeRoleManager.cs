using Microsoft.AspNetCore.Identity;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Tests.IdentityServerTests
{
    public class FakeRoleManager : RoleManager<IdentityRole>
    {
        public FakeRoleManager()
            : base(null, null, null, null, null)
        {
        }
    }
}   