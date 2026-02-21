using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using OpenIddict.Abstractions;
using OutOfSchool.AuthCommon.Services.Interfaces;
using OutOfSchool.Services.Models;
using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace OutOfSchool.AuthCommon.Services.Tests;

[TestFixture()]
public class UserServiceTests
{
    private IUserService userService;
    private Mock<UserManager<User>> userManager;
    private Mock<ILogger<UserService>> logger;
    private Mock<IOpenIddictTokenManager> tokenManager;

    [SetUp]
    public void SetUp()
    {
        userManager = new Mock<UserManager<User>>(
            new Mock<IUserStore<User>>().Object,
            new Mock<IOptions<IdentityOptions>>().Object,
            new Mock<IPasswordHasher<User>>().Object,
            new IUserValidator<User>[0],
            new IPasswordValidator<User>[0],
            new Mock<ILookupNormalizer>().Object,
            new Mock<IdentityErrorDescriber>().Object,
            new Mock<IServiceProvider>().Object,
            new Mock<ILogger<UserManager<User>>>().Object);

        logger = new Mock<ILogger<UserService>>();
        tokenManager = new Mock<IOpenIddictTokenManager>();

        userService = new UserService(
            userManager.Object,
            logger.Object,
            tokenManager.Object
            );
    }

    [Test]
    public async Task LogOutUserById_WhenUserNotExist_ReturnsNotFound()
    {
        // Arrange
        userManager.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(null as User);

        // Act
        var result = await userService.LogOutUserById(string.Empty);

        // Assert
        Assert.AreEqual(HttpStatusCode.NotFound, result.HttpStatusCode);

    }

    [Test]
    public async Task LogOutUserById_WhenUserExist_ReturnsOkResponse()
    {
        // Arrange
        userManager.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(new User());
        tokenManager.Setup(x => x.FindBySubjectAsync(string.Empty, It.IsAny<CancellationToken>())).Returns(AsyncEnumerable.Empty<object>());
        tokenManager.Setup(x => x.TryRevokeAsync(It.IsAny<object>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        var result = await userService.LogOutUserById(string.Empty);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, result.HttpStatusCode);
        Assert.AreEqual(true, result.IsSuccess);

    }
}