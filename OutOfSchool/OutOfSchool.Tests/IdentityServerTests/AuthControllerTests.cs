using System;
using System.Collections.Generic;
using System.Linq;
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
            mockRoleManager = MockRoleManager;
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

        [TestCaseSource(nameof(LoginViewModelsTestData))]
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

        [TestCaseSource(nameof(LoginViewModelsWithSignInResultTestData))]
        public async Task LoginWithLoginVMWithoutModelError(
            LoginViewModel loginViewModel, KeyValuePair<SignInResult, Type> expectedResult)
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

        [TestCase("")]
        [TestCase("Return url")]
        public async Task RegisterWithReturnUrl(string returnUrl)
        {
            // Arrange 
            var authController = CreateAuthController;
            mockRoleManager.Setup(manager => manager.Roles)
                .Returns(new List<IdentityRole>().AsQueryable());
            // Act
            var result = authController.Register(returnUrl);
            // Assert
            Assert.IsInstanceOf<ViewResult>(result);
        }

        [Test]
        public async Task RegisterWithVMWithModelError()
        {
            // Arrange 
            var authController = CreateAuthController;
            mockRoleManager.Setup(manager => manager.Roles)
                .Returns(new List<IdentityRole>().AsQueryable());
            authController.ModelState.AddModelError("", "Message");
            // Act
            var result = await authController.Register(new RegisterViewModel());
            // Assert
            Assert.IsInstanceOf<ViewResult>(result);
        }

        [TestCaseSource(nameof(RegisterViewModelsTestData))]
        public async Task RegisterWithVMWithoutModelError(RegisterViewModel viewModel)
        {
            // Arrange 
            var authController = CreateAuthController;
            mockRoleManager.Setup(manager => manager.Roles)
                .Returns(
                    new List<IdentityRole>() {new IdentityRole("testrole") {Id = viewModel.UserRoleId}}.AsQueryable());
            mockUserManager.Setup(manager => manager.CreateAsync(It.IsAny<User>(), viewModel.Password))
                .Returns(Task.FromResult<IdentityResult>(IdentityResult.Success));
            mockUserManager.Setup(manager => manager.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()))
                .Returns(Task.FromResult<IdentityResult>(IdentityResult.Success));
            mockSignInManager.Setup(manager => manager.SignInAsync(
                    It.IsAny<User>(), It.IsAny<bool>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);
            // Act
            var result = await authController.Register(viewModel);
            // Assert
            Assert.IsInstanceOf<RedirectResult>(result);
        }

        [TestCaseSource(nameof(RegisterViewModelsTestData))]
        public async Task RegisterWithVMAndIdentityError(RegisterViewModel viewModel)
        {
            // Arrange 
            var authController = CreateAuthController;
            var identityResult = IdentityResult.Failed(new List<IdentityError>()
            {
                new IdentityError() {Code = "User cant be created", Description = "The program failed to create user"},
            }.ToArray());
            mockRoleManager.Setup(manager => manager.Roles)
                .Returns(
                    new List<IdentityRole>() {new IdentityRole("testrole") {Id = viewModel.UserRoleId}}.AsQueryable());
            mockUserManager.Setup(manager => manager.CreateAsync(It.IsAny<User>(), viewModel.Password))
                .Returns(Task.FromResult<IdentityResult>(identityResult));
            mockUserManager.Setup(manager => manager.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()))
                .Returns(Task.FromResult<IdentityResult>(IdentityResult.Success));
            mockSignInManager.Setup(manager => manager.SignInAsync(
                    It.IsAny<User>(), It.IsAny<bool>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);
            // Act
            var result = await authController.Register(viewModel);
            // Assert
            Assert.IsInstanceOf<ViewResult>(result);
        }
        
        [TestCaseSource(nameof(RegisterViewModelsTestData))]
        public async Task RegisterWithVMAndAddToRoleError(RegisterViewModel viewModel)
        {
            // Arrange 
            var authController = CreateAuthController;
            var identityResultFailed = IdentityResult.Failed(new List<IdentityError>()
            {
                new IdentityError() {Code = "User cant be created", Description = "The program failed to create user"},
            }.ToArray());
            mockRoleManager.Setup(manager => manager.Roles)
                .Returns(
                    new List<IdentityRole>() {new IdentityRole("testrole") {Id = viewModel.UserRoleId}}.AsQueryable());
            mockUserManager.Setup(manager => manager.CreateAsync(It.IsAny<User>(), viewModel.Password))
                .Returns(Task.FromResult<IdentityResult>(IdentityResult.Success));
            mockUserManager.Setup(manager => manager.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()))
                .Returns(Task.FromResult<IdentityResult>(identityResultFailed));
            // Act
            var result = await authController.Register(viewModel);
            // Assert
            Assert.IsInstanceOf<ViewResult>(result);
        }

        [Test]
        public async Task ExternalLoginNotImplemented()
        {
            // Arrange
            var authController = CreateAuthController;

            // Assert & Act
            Assert.ThrowsAsync<NotImplementedException>(() =>
                authController.ExternalLogin("Provider", "return url"));
        }

        public static IEnumerable<TestCaseData> RegisterViewModelsTestData =>
            new List<TestCaseData>()
            {
                new TestCaseData(new RegisterViewModel()
                {
                    Username = "Baron", PhoneNumber = "0502391222", UserRoleId = "AdminRoleId", ReturnUrl = "Return url"
                }),
                new TestCaseData(new RegisterViewModel()
                {
                    Username = "Baron", PhoneNumber = "0502391222", UserRoleId = "AdminRoleId1",
                    ReturnUrl = "Return url2",
                }),
            };

        public static IEnumerable<LoginViewModel> LoginViewModels
        {
            get => new List<LoginViewModel>()
            {
                new LoginViewModel {ReturnUrl = "Return url", ExternalProviders = EmptySchemes},
                new LoginViewModel {ExternalProviders = EmptySchemes},
                new LoginViewModel {ReturnUrl = "Return url"},
                new LoginViewModel(),
            };
        }

        public static IEnumerable<TestCaseData> LoginViewModelsTestData
        {
            get
            {
                foreach (var loginViewModel in LoginViewModels)
                {
                    yield return new TestCaseData(loginViewModel);
                }
            }
        }

        public static IEnumerable<TestCaseData> LoginViewModelsWithSignInResultTestData
        {
            get
            {
                foreach (var signInResult in SignInAndActionResults)
                {
                    foreach (var loginViewModel in LoginViewModels)
                    {
                        yield return new TestCaseData(loginViewModel, signInResult);
                    }
                }
            }
        }

        public static IEnumerable<KeyValuePair<SignInResult, Type>> SignInAndActionResults
        {
            get
            {
                yield return new KeyValuePair<SignInResult, Type>(SignInResult.Success, typeof(RedirectResult));
                yield return new KeyValuePair<SignInResult, Type>(SignInResult.Failed, typeof(ViewResult));
                yield return new KeyValuePair<SignInResult, Type>(SignInResult.LockedOut, typeof(BadRequestResult));
            }
        }

        public static IEnumerable<AuthenticationScheme> EmptySchemes
        {
            get => new List<AuthenticationScheme>();
        }

        public static Mock<RoleManager<IdentityRole>> MockRoleManager =>
            new Mock<RoleManager<IdentityRole>>(
                new Mock<IRoleStore<IdentityRole>>().Object, null, null, null, null);
    }
}