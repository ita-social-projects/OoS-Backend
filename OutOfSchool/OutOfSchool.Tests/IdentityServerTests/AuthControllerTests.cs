using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Moq;
using NUnit.Framework;
using OutOfSchool.IdentityServer.Controllers;
using OutOfSchool.IdentityServer.ViewModels;
using OutOfSchool.Services.Models;

namespace OutOfSchool.Tests.IdentityServerTests
{
    [TestFixture]
    public class AuthControllerTests
    {
        private readonly Mock<FakeUserManager> mockUserManager;
        private readonly Mock<FakeSignInManager> mockSignInManager;
        private readonly Mock<IIdentityServerInteractionService> mockInteractionService;
        private readonly Mock<RoleManager<IdentityRole>> mockRoleManager;

        public AuthController CreateAuthController => new AuthController(mockUserManager.Object,
            mockSignInManager.Object, mockRoleManager.Object, mockInteractionService.Object);

        public AuthControllerTests()
        {
            mockRoleManager = GetMockRoleManager();
            mockUserManager = new Mock<FakeUserManager>();
            mockInteractionService = new Mock<IIdentityServerInteractionService>();
            mockSignInManager = new Mock<FakeSignInManager>();
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
            mockSignInManager.Setup(manager => manager.SignOutAsync())
                .Returns(Task.CompletedTask);
            mockInteractionService.Setup(service => service.GetLogoutContextAsync(It.IsAny<string>()))
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
            mockSignInManager.Setup(manager => manager.SignOutAsync())
                .Returns(Task.CompletedTask);
            mockInteractionService.Setup(service => service.GetLogoutContextAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(logoutRequest));
            // Act

            // Assert
            var ex = Assert.ThrowsAsync<NotImplementedException>(
                () => CreateAuthController.Logout("Any logout ID"));
        }

        [TestCase("")]
        [TestCase("Return url")]
        public async Task LoginWithReturnUrl(string returnUrl)
        {
            // Arrange
            var authController = CreateAuthController;
            var logoutRequest = new LogoutRequest("iFrameUrl", new LogoutMessage());
            logoutRequest.PostLogoutRedirectUri = "";
            // mockSignInManager.Setup(manager => manager.GetExternalAuthenticationSchemesAsync())
            //     .Returns(Task.FromResult<IEnumerable<AuthenticationScheme>>(new List<AuthenticationScheme>()
            //         {new AuthenticationScheme("Scheme", "Display type", typeof(HandleRequestContext<>))}));

            // Act
            var viewResult = await authController.Login(returnUrl);

            // Assert
            Assert.IsInstanceOf(typeof(ViewResult), viewResult);
        }

        [TestCaseSource(nameof(GetViewModels))]
        public async Task LoginWithLoginVMWithModelError(LoginViewModel loginViewModel)
        {
            // Arrange
            var authController = CreateAuthController;
            authController.ModelState.AddModelError(string.Empty, "Error");
            // mockSignInManager.Setup(manager => manager.GetExternalAuthenticationSchemesAsync())
            //     .Returns(Task.FromResult<IEnumerable<AuthenticationScheme>>(new List<AuthenticationScheme>()));
            // Act
            var externalAuthenticationSchemesAsync = await mockSignInManager.Object.GetExternalAuthenticationSchemesAsync();
            var result = await authController.Login(loginViewModel);
            // Assert
            Assert.IsInstanceOf(typeof(ViewResult), result);
        }

        public static IEnumerable<TestCaseData> GetViewModels
        {
            get
            {
                yield return new TestCaseData(
                    new LoginViewModel {ReturnUrl = "Return url", ExternalProviders = GetSchemes});
                yield return new TestCaseData(
                    new LoginViewModel {ExternalProviders = GetSchemes});
                yield return new TestCaseData(
                    new LoginViewModel {ReturnUrl = "Return url"});
                yield return new TestCaseData(
                    new LoginViewModel());
            }
        }

        public static IEnumerable<AuthenticationScheme> GetSchemes
        {
            get { return new List<AuthenticationScheme>(); }
        }

        public static Mock<RoleManager<IdentityRole>> GetMockRoleManager()
        {
            var roleStore = new Mock<IRoleStore<IdentityRole>>();
            return new Mock<RoleManager<IdentityRole>>(
                roleStore.Object, null, null, null, null);
        }
    }
}