using System;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Services;
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
            this._mockRoleManager = GetMockRoleManager();
            this._mockUserManager = new Mock<FakeUserManager>();
            this._mockInteractionService = new Mock<IIdentityServerInteractionService>();
            this._mockSignInManager = new Mock<FakeSignInManager>();
        }

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public async Task LogoutWithLogoutId()
        {
            // Arrange
            var authController = this.CreateAuthController;
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
            var authController = this.CreateAuthController;
            var logoutRequest = new LogoutRequest("iFrameUrl", new LogoutMessage());
            logoutRequest.PostLogoutRedirectUri = "";
            _mockSignInManager.Setup(manager => manager.SignOutAsync())
                .Returns(Task.CompletedTask);
            _mockInteractionService.Setup(service => service.GetLogoutContextAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(logoutRequest));
            // Act

            // Assert
            var ex = Assert.ThrowsAsync<NotImplementedException>(() => this.CreateAuthController.Logout("Any logout ID"));
        }

        public static Mock<RoleManager<IdentityRole>> GetMockRoleManager()
        {
            var roleStore = new Mock<IRoleStore<IdentityRole>>();
            return new Mock<RoleManager<IdentityRole>>(
                roleStore.Object, null, null, null, null);
        }
    }
}