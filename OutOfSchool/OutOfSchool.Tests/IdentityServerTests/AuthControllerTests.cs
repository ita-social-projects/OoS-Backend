using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using OutOfSchool.IdentityServer.Controllers;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Tests.IdentityServerTests
{
    [TestFixture]
    public class AuthControllerTests
    {
        private readonly Mock<FakeUserManager> _mockUserManager;
        private readonly Mock<FakeSignInManager> _mockSignInManager;
        private readonly Mock<IIdentityServerInteractionService> _mockInteractionService;
        private readonly Mock<RoleManager<IdentityRole>> _mockRoleManager;

        public AuthController CreateAuthController => new AuthController(_mockUserManager.Object,
            _mockSignInManager.Object, _mockRoleManager.Object, _mockInteractionService.Object);

        public AuthControllerTests()
        {
            _mockRoleManager = GetMockRoleManager();
            _mockUserManager = new Mock<FakeUserManager>();
            _mockInteractionService = new Mock<IIdentityServerInteractionService>();
            _mockSignInManager = new Mock<FakeSignInManager>();
        }

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public async Task LogoutWithLogoutId()
        {
            // Arrange
            var authController = CreateAuthController;
            var logoutRequest = new LogoutRequest("iFrameUrl", new LogoutMessage());
            logoutRequest.PostLogoutRedirectUri = "True logout id";
            _mockSignInManager.Setup(manager => manager.SignOutAsync())
                .Returns(Task.CompletedTask);
            _mockInteractionService.Setup(service => service.GetLogoutContextAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(logoutRequest));
            // Act
            var result = await CreateAuthController.Logout("Any logout ID");

            // Assert
            Assert.IsInstanceOf(typeof(RedirectResult), result);
        }

        [Test]
        public async Task LogoutWithoutLogoutId()
        {
            // Arrange
            var authController = CreateAuthController;
            var logoutRequest = new LogoutRequest("iFrameUrl", new LogoutMessage());
            logoutRequest.PostLogoutRedirectUri = "";
            _mockSignInManager.Setup(manager => manager.SignOutAsync())
                .Returns(Task.CompletedTask);
            _mockInteractionService.Setup(service => service.GetLogoutContextAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(logoutRequest));
            // Act

            // Assert
            var ex = Assert.ThrowsAsync<NotImplementedException>(
                () => CreateAuthController.Logout("Any logout ID"));
        }

        [Test]
        public async Task LoginWithReturnUrl()
        {
            // Arrange
            var authController = CreateAuthController;
            var logoutRequest = new LogoutRequest("iFrameUrl", new LogoutMessage());
            var authenticationSchemes = new List<AuthenticationScheme>();
            logoutRequest.PostLogoutRedirectUri = "";
            _mockSignInManager.Setup(manager => manager.GetExternalAuthenticationSchemesAsync())
                .Returns(Task.FromResult<IEnumerable<AuthenticationScheme>>(authenticationSchemes));
            // Act
            var viewResult = await authController.Login("Return url");
            // Assert
            Assert.IsInstanceOf(typeof(ViewResult), viewResult);
        }
        

        public static Mock<RoleManager<IdentityRole>> GetMockRoleManager()
        {
            var roleStore = new Mock<IRoleStore<IdentityRole>>();
            return new Mock<RoleManager<IdentityRole>>(
                roleStore.Object, null, null, null, null);
        }
    }
}