using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.Tests.Common;

namespace OutOfSchool.AuthCommon.Services.Tests;

[TestFixture]
public class IdentityRolesInitializerHostedServiceTests
{
    private readonly Mock<ILogger<IdentityRolesInitializerHostedService>> mockedLogger;
    private readonly IEqualityComparer<IdentityRole> roleNameComparer;
    private readonly IdentityRole[] defaultRoles;

    public IdentityRolesInitializerHostedServiceTests()
    {
        this.mockedLogger = new Mock<ILogger<IdentityRolesInitializerHostedService>>();
        this.roleNameComparer = EqualityComparer<IdentityRole>.Create((a, b) => a.Name == b.Name);
        this.defaultRoles = IdentityRolesInitializerHostedService.GetDefaultRoleNames()
            .Select(roleName => new IdentityRole { Name = roleName })
            .ToArray();
    }

    [Test]
    public async Task Initialize_WhenContainsAllDefaultRoles_ShouldNotAddAnyDefaultRoles()
    {
        // Arrange
        var rolesInStore = defaultRoles;

        var expectedExecutionTimes = Times.Never();

        var mockedRoleManager = CreateMockedRoleManager(rolesInStore);

        var initializer = CreateInitializer(mockedRoleManager.Object);

        // Act
        await initializer.StartAsync(CancellationToken.None);

        // Arrange
        mockedRoleManager.Verify(rm => rm.CreateAsync(It.IsIn(defaultRoles, roleNameComparer)), expectedExecutionTimes);
    }

    [Test]
    public async Task Initialize_WhenContainsZeroDefaultRoles_ShouldAddAllDefaultRoles()
    {
        // Arrange
        var rolesInStore = Array.Empty<IdentityRole>();

        var expectedExecutionTimes = Times.Exactly(defaultRoles.Length);

        var mockedRoleManager = CreateMockedRoleManager(rolesInStore);

        var initializer = CreateInitializer(mockedRoleManager.Object);

        // Act
        await initializer.StartAsync(CancellationToken.None);

        // Arrange
        mockedRoleManager.Verify(rm => rm.CreateAsync(It.IsIn(defaultRoles, roleNameComparer)), expectedExecutionTimes);
    }

    [Test]
    public async Task Initialize_WhenContainsSomeDefaultRoles_ShouldAddRemainingDefaultRoles()
    {
        // Arrange
        var rolesInStore = defaultRoles.Take(3).ToArray();

        var expectedExecutionTimes = Times.Exactly(defaultRoles.Length - rolesInStore.Length);

        var mockedRoleManager = CreateMockedRoleManager(rolesInStore);

        var initializer = CreateInitializer(mockedRoleManager.Object);

        // Act
        await initializer.StartAsync(CancellationToken.None);

        // Arrange
        mockedRoleManager.Verify(rm => rm.CreateAsync(It.IsIn(defaultRoles, roleNameComparer)), expectedExecutionTimes);
    }

    private IdentityRolesInitializerHostedService CreateInitializer(RoleManager<IdentityRole> roleManager)
    {
        var serviceProvider = new ServiceCollection()
            .AddScoped(sp => roleManager)
            .BuildServiceProvider();

        return new IdentityRolesInitializerHostedService(serviceProvider, mockedLogger.Object);
    }

    private Mock<RoleManager<IdentityRole>> CreateMockedRoleManager(IEnumerable<IdentityRole> rolesInStore)
    {
        object[] roleManagerCtorArgs =
        [
            new Mock<IRoleStore<IdentityRole>>().Object,
            Array.Empty<IRoleValidator<IdentityRole>>(),
            new Mock<ILookupNormalizer>().Object,
            new Mock<IdentityErrorDescriber>().Object,
            new Mock<ILogger<RoleManager<IdentityRole>>>().Object,
        ];

        var mockedRoleManager = new Mock<RoleManager<IdentityRole>>(roleManagerCtorArgs);

        mockedRoleManager.Setup(rm => rm.CreateAsync(It.IsIn(defaultRoles, roleNameComparer)))
            .ReturnsAsync(IdentityResult.Success);

        mockedRoleManager.Setup(rm => rm.Roles)
            .Returns(rolesInStore.AsTestAsyncEnumerableQuery());

        return mockedRoleManager;
    }
}
