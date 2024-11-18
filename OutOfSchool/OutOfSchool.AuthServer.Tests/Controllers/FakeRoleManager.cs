using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;

namespace OutOfSchool.AuthServer.Tests.Controllers;

public class FakeRoleManager : RoleManager<IdentityRole>
{
    public FakeRoleManager()
        : base(
            new Mock<IRoleStore<IdentityRole>>().Object,
            [new Mock<IRoleValidator<IdentityRole>>().Object],
            new Mock<ILookupNormalizer>().Object,
            new Mock<IdentityErrorDescriber>().Object,
            new Mock<ILogger<RoleManager<IdentityRole>>>().Object)
    {
    }
}