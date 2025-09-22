using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.AuthCommon.Services;
using OutOfSchool.Common;
using OutOfSchool.Services.Models;
using System;
using System.Threading.Tasks;

namespace OutOfSchool.AuthServer.Tests.Services;
[TestFixture]
public class SystemUserInitializerTests
{
    private Mock<ILogger<SystemUserInitializer>> mockedLogger;
    private Mock<UserManager<User>> mockedUserManager;
    private Mock<RoleManager<IdentityRole>> mockedRoleManager;

    private SystemUserInitializer systemUserInitializer;

    [SetUp]
    public void SetUp()
    {
        mockedLogger = new Mock<ILogger<SystemUserInitializer>>();
        var mockedUserStore = new Mock<IUserStore<User>>();
        mockedUserManager = new Mock<UserManager<User>>(
            mockedUserStore.Object,
            null, null, null, null, null, null, null, null);
        var mockedRoleStore = new Mock<IRoleStore<IdentityRole>>();
        mockedRoleManager = new Mock<RoleManager<IdentityRole>>(
            mockedRoleStore.Object,
            null, null, null, null);
        systemUserInitializer = new SystemUserInitializer(
            mockedUserManager.Object,
            mockedRoleManager.Object,
            mockedLogger.Object);
    }

    [Test]
    public async Task EnsureExistsAsync_WhenSystemUserAndRoleDoNotExist_ShouldCreateBoth()
    {
        // Arrange
        var systemUser = new User
        {
            Id = Constants.SystemUserConstants.SystemUserId,
            UserName = Constants.SystemUserConstants.SystemUserName,
            FirstName = Constants.SystemUserConstants.SystemUserName,
            MiddleName = Constants.SystemUserConstants.SystemUserName,
            LastName = Constants.SystemUserConstants.SystemUserName,
            Email = Constants.SystemUserConstants.SystemUserEmail,
            EmailConfirmed = true,
            CreatingTime = DateTimeOffset.UtcNow,
            Role = Constants.SystemUserConstants.SystemUserRole,
            IsRegistered = false,
            IsBlocked = false,
            IsSystemProtected = true
        };

        mockedRoleManager.Setup(rm => rm.RoleExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(false);
        mockedRoleManager.Setup(rm => rm.CreateAsync(It.IsAny<IdentityRole>()))
            .ReturnsAsync(IdentityResult.Success);
        mockedUserManager.SetupSequence(um => um.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((User)null)
            .ReturnsAsync(systemUser);
        mockedUserManager.Setup(um => um.CreateAsync(It.IsAny<User>()))
            .ReturnsAsync(IdentityResult.Success);
        mockedUserManager.Setup(um => um.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        mockedUserManager.Setup(um => um.IsInRoleAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(false);

        // Act
        await systemUserInitializer.EnsureExistsAsync().ConfigureAwait(false);

        // Assert
        mockedRoleManager.Verify(rm => rm.CreateAsync(It.Is<IdentityRole>(r => r.Name == "system")), Times.Once);
        mockedUserManager.Verify(um => um.CreateAsync(It.Is<User>(u => u.UserName == "system")), Times.Once);
        mockedUserManager.Verify(um => um.AddToRoleAsync(It.Is<User>(u => u.UserName == "system"), "system"), Times.Once);
    }

    [Test]
    public async Task EnsureExistsAsync_WhenSystemUserExistsButNotInRole_ShouldAddToRole()
    {
        // Arrange
        var systemUser = new User
        {
            Id = Constants.SystemUserConstants.SystemUserId,
            UserName = Constants.SystemUserConstants.SystemUserName,
            FirstName = Constants.SystemUserConstants.SystemUserName,
            MiddleName = Constants.SystemUserConstants.SystemUserName,
            LastName = Constants.SystemUserConstants.SystemUserName,
            Email = Constants.SystemUserConstants.SystemUserEmail,
            EmailConfirmed = true,
            CreatingTime = DateTimeOffset.UtcNow,
            Role = Constants.SystemUserConstants.SystemUserRole,
            IsRegistered = false,
            IsBlocked = false,
            IsSystemProtected = true
        };

        mockedRoleManager.Setup(rm => rm.RoleExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(true);
        mockedUserManager.Setup(um => um.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync(systemUser);
        mockedUserManager.Setup(um => um.IsInRoleAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(false);
        mockedUserManager.Setup(um => um.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        await systemUserInitializer.EnsureExistsAsync().ConfigureAwait(false);

        // Assert
        mockedRoleManager.Verify(rm => rm.CreateAsync(It.IsAny<IdentityRole>()), Times.Never);
        mockedUserManager.Verify(um => um.CreateAsync(It.IsAny<User>()), Times.Never);
        mockedUserManager.Verify(um => um.AddToRoleAsync(It.Is<User>(u => u.UserName == "system"), "system"), Times.Once);
    }

    [Test]
    public async Task EnsureExistsAsync_WhenCreatedUserIsNull_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var systemUser = new User
        {
            Id = Constants.SystemUserConstants.SystemUserId,
            UserName = Constants.SystemUserConstants.SystemUserName,
            FirstName = Constants.SystemUserConstants.SystemUserName,
            MiddleName = Constants.SystemUserConstants.SystemUserName,
            LastName = Constants.SystemUserConstants.SystemUserName,
            Email = Constants.SystemUserConstants.SystemUserEmail,
            EmailConfirmed = true,
            CreatingTime = DateTimeOffset.UtcNow,
            Role = Constants.SystemUserConstants.SystemUserRole,
            IsRegistered = false,
            IsBlocked = false,
            IsSystemProtected = true
        };

        mockedRoleManager.Setup(rm => rm.RoleExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(true);
        mockedUserManager.Setup(um => um.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((User)null);
        mockedUserManager.Setup(um => um.CreateAsync(It.IsAny<User>()))
            .ReturnsAsync(IdentityResult.Success);

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(async () => await systemUserInitializer.EnsureExistsAsync().ConfigureAwait(false));
    }

    [Test]
    public async Task EnsureExistsAsync_WhenRoleCreationFails_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var identityError = new IdentityError
        {
            Code = "TestCode",
            Description = "TestDescription",
        };
        mockedRoleManager.Setup(rm => rm.RoleExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(false);
        mockedRoleManager.Setup(rm => rm.CreateAsync(It.IsAny<IdentityRole>()))
            .ReturnsAsync(IdentityResult.Failed(identityError));

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => await systemUserInitializer.EnsureExistsAsync().ConfigureAwait(false));
        Assert.That(ex.Message, Does.Contain("Failed to create system role"));
    }

    [Test]
    public async Task EnsureExistsAsync_WhenUserCreationFails_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var identityError = new IdentityError
        {
            Code = "TestCode",
            Description = "TestDescription",
        };
        mockedRoleManager.Setup(rm => rm.RoleExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(true);
        mockedUserManager.Setup(um => um.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((User)null);
        mockedUserManager.Setup(um => um.CreateAsync(It.IsAny<User>()))
            .ReturnsAsync(IdentityResult.Failed(identityError));

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => await systemUserInitializer.EnsureExistsAsync().ConfigureAwait(false));
        Assert.That(ex.Message, Does.Contain("Failed to create system user"));
    }

    [Test]
    public async Task EnsureExistsAsync_WhenAddingUserToRoleFails_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var systemUser = new User
        {
            Id = Constants.SystemUserConstants.SystemUserId,
            UserName = Constants.SystemUserConstants.SystemUserName,
            FirstName = Constants.SystemUserConstants.SystemUserName,
            MiddleName = Constants.SystemUserConstants.SystemUserName,
            LastName = Constants.SystemUserConstants.SystemUserName,
            Email = Constants.SystemUserConstants.SystemUserEmail,
            EmailConfirmed = true,
            CreatingTime = DateTimeOffset.UtcNow,
            IsRegistered = false,
            IsBlocked = false,
            IsSystemProtected = true
        };
        var identityError = new IdentityError
        {
            Code = "TestCode",
            Description = "TestDescription",
        };

        mockedRoleManager.Setup(rm => rm.RoleExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(true);
        mockedUserManager.Setup(um => um.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync(systemUser);
        mockedUserManager.Setup(um => um.IsInRoleAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(false);
        mockedUserManager.Setup(um => um.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(identityError));

        // Act & Assert
        var ex = Assert.ThrowsAsync<InvalidOperationException>(async () => await systemUserInitializer.EnsureExistsAsync().ConfigureAwait(false));
        Assert.That(ex.Message, Does.Contain($"Failed to assign user '{systemUser.UserName}' to role 'system'"));
    }

}
