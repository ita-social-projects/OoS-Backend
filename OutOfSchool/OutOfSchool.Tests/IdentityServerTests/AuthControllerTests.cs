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
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

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

            // Act
            var viewResult = await authController.Login(returnUrl);

            // Assert
            Assert.IsInstanceOf(typeof(ViewResult), viewResult);
        }

        [TestCaseSource(nameof(GetLoginViewModels))]
        public async Task LoginWithLoginVMWithModelError(LoginViewModel loginViewModel)
        {
            // Arrange
            var authController = CreateAuthController;
            authController.ModelState.AddModelError(string.Empty, "Error");
            // Act
            var result = await authController.Login(loginViewModel);
            // Assert
            Assert.IsInstanceOf(typeof(ViewResult), result);
        }

        [TestCaseSource(nameof(GetLoginViewModels))]
        public async Task LoginWithLoginVMWithoutModelError(LoginViewModel loginViewModel, KeyValuePair<SignInResult, Type> expectedResult)
        {
            // Arrange
            var authController = CreateAuthController;
            mockSignInManager.Setup(manager =>
                    manager.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), false, false))
                .ReturnsAsync(expectedResult.Key);
            // Act
            var result = await authController.Login(loginViewModel);
            // Assert
            Type type = expectedResult.Value;
            Assert.IsInstanceOf(type, result);
        }

        public static IEnumerable<TestCaseData> GetLoginViewModels
        {
            get
            {
                foreach (var signInResult in GetSignInResults)
                {
                    yield return new TestCaseData(
                        new LoginViewModel {ReturnUrl = "Return url", ExternalProviders = GetSchemes }, signInResult);
                    yield return new TestCaseData(
                        new LoginViewModel {ExternalProviders = GetSchemes }, signInResult);
                    yield return new TestCaseData(
                        new LoginViewModel {ReturnUrl = "Return url" }, signInResult);
                    yield return new TestCaseData(
                        new LoginViewModel(), signInResult);
                }
            }
        }

        public static IEnumerable<KeyValuePair<SignInResult, Type>> GetSignInResults
        {
            get
            {
                yield return new KeyValuePair<SignInResult, Type>(SignInResult.Success, typeof(RedirectResult));
                yield return new KeyValuePair<SignInResult, Type>(SignInResult.Failed, typeof(ViewResult));
                yield return new KeyValuePair<SignInResult, Type>(SignInResult.LockedOut, typeof(BadRequestResult));
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