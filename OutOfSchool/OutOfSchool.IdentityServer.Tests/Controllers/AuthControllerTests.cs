using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using NUnit.Framework;
using OutOfSchool.IdentityServer.Controllers;
using OutOfSchool.IdentityServer.ViewModels;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OutOfSchool.IdentityServer.Config;

namespace OutOfSchool.IdentityServer.Tests.Controllers
{
    [TestFixture]
    public class AuthControllerTests
    {
        private readonly Mock<FakeUserManager> fakeUserManager;
        private readonly Mock<FakeSignInManager> fakeSignInManager;
        private readonly Mock<IIdentityServerInteractionService> fakeInteractionService;
        private readonly Mock<ILogger<AuthController>> fakeLogger;
        private readonly Mock<IParentRepository> fakeparentRepository;
        private AuthController authController;
        private readonly Mock<IStringLocalizer<SharedResource>> fakeLocalizer;
        private static Mock<IOptions<IdentityServerConfig>> fakeIdentityServerConfig;

        public AuthControllerTests()
        {
            fakeUserManager = new Mock<FakeUserManager>();
            fakeInteractionService = new Mock<IIdentityServerInteractionService>();
            fakeSignInManager = new Mock<FakeSignInManager>();
            fakeLogger = new Mock<ILogger<AuthController>>();
            fakeparentRepository = new Mock<IParentRepository>();
            fakeLocalizer = new Mock<IStringLocalizer<SharedResource>>();
        }

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            var config = new IdentityServerConfig();
            config.RedirectToStartPageUrl = "http://localhost:4200";

            fakeIdentityServerConfig = new Mock<IOptions<IdentityServerConfig>>();
            fakeIdentityServerConfig.Setup(m => m.Value).Returns(config);
        }

        [SetUp]
        public void Setup()
        {
            fakeLocalizer
                .Setup(localizer => localizer[It.IsAny<string>()])
                .Returns(new LocalizedString("mock", "error"));
            authController = new AuthController(
                fakeUserManager.Object,
                fakeSignInManager.Object,
                fakeInteractionService.Object, 
                fakeLogger.Object,
                fakeparentRepository.Object,
                fakeLocalizer.Object,
                fakeIdentityServerConfig.Object);
        }

        [Test]
        public async Task Logout_WithLogoutId_ReturnsRedirectResult()
        {
            // Arrange
            var logoutRequest = new LogoutRequest("iFrameUrl", new LogoutMessage())
            { 
                PostLogoutRedirectUri = "True logout id",
            };
            
            fakeSignInManager.Setup(manager => manager.SignOutAsync())
                .Returns(Task.CompletedTask);
            fakeInteractionService.Setup(service => service.GetLogoutContextAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(logoutRequest));
            // Act
            var result = await authController.Logout("Any logout ID");

            // Assert
            Assert.IsInstanceOf(typeof(RedirectResult), result);
        }

        [Test]
        public async Task Logout_WithoutLogoutId_ReturnsRedirectResult()
        {
            // Arrange
            var logoutRequest = new LogoutRequest("iFrameUrl", new LogoutMessage())
            {
                PostLogoutRedirectUri = "",
            };
            fakeSignInManager.Setup(manager => manager.SignOutAsync())
                .Returns(Task.CompletedTask);
            fakeInteractionService.Setup(service => service.GetLogoutContextAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(logoutRequest));

            // Act
            var result = await authController.Logout("Any logout ID");

            // Act & Assert
            Assert.IsInstanceOf(typeof(RedirectResult), result);
        }

        [TestCase(null)]
        [TestCase("Return url")]
        public async Task Login_WithAndWithoutReturnUrl_ReturnsViewResult(string returnUrl)
        {
            // Arrange

            // Act
            IActionResult viewResult;
            viewResult = returnUrl == null ? await authController.Login() : await authController.Login(returnUrl);

            // Assert
            Assert.IsInstanceOf(typeof(ViewResult), viewResult);
        }

        [TestCaseSource(nameof(LoginViewModelsTestData))]
        public async Task Login_WithLoginVMAndWithModelError_ReturnsViewWithModelErrors(LoginViewModel loginViewModel)
        {
            // Arrange
            var fakeErrorMessage = "Model is invalid";
            authController.ModelState.AddModelError(string.Empty, fakeErrorMessage);
            var errorMessage = authController.ModelState.Values.FirstOrDefault().Errors.FirstOrDefault().ErrorMessage;

            // Act
            var result = await authController.Login(loginViewModel);

            // Assert
            Assert.That(errorMessage, Is.EqualTo(fakeErrorMessage));
            Assert.IsInstanceOf(typeof(ViewResult), result);
        }

        [TestCaseSource(nameof(LoginViewModelsWithSignInResultTestData))]
        public async Task Login_WithLoginVMWithoutModelError_ReturnsActionResult(
            LoginViewModel loginViewModel, KeyValuePair<SignInResult, Type> expectedResult)
        {
            // Arrange
            var authController = this.authController;
            var user = new User();
            fakeSignInManager.Setup(manager =>
                    manager.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), false, false))
                .ReturnsAsync(expectedResult.Key);
            fakeUserManager.Setup(manager => manager.FindByEmailAsync(loginViewModel.Username))
                .Returns(Task.FromResult<User>(user));
            fakeUserManager.Setup(manager => manager.UpdateAsync(user))
                .Returns(Task.FromResult<IdentityResult>(IdentityResult.Success));

            // Act
            var result = await authController.Login(loginViewModel);

            // Assert
            Type type = expectedResult.Value;
            Assert.IsInstanceOf(type, result);
        }

        [Test]
        public void Register_WithoutReturnUrl_ReturnsViewResult()
        {
            // Arrange 

            // Act
            IActionResult result = authController.Register();
            RegisterViewModel viewResultModel = (RegisterViewModel) ((ViewResult) result).Model;

            // Assert
            Assert.That(viewResultModel.ReturnUrl, Is.EqualTo("Login"));
            Assert.IsInstanceOf<ViewResult>(result);
        }

        [Test]
        public void Register_WithReturnUrl_ReturnsViewResult()
        {
            // Arrange
            var returnUrl = "Return url";

            // Act
            IActionResult result = authController.Register(returnUrl);
            RegisterViewModel viewResultModel = (RegisterViewModel) ((ViewResult) result).Model;

            // Assert
            Assert.That(viewResultModel.ReturnUrl, Is.EqualTo(returnUrl));
            Assert.IsInstanceOf<ViewResult>(result);
        }

        [Test]
        public async Task Register_WithVMAndWithModelError_ReturnsViewResultWithModelErrors()
        {
            // Arrange 
            var fakeModelErrorMessage = "Model State is invalid";
            authController.ModelState.AddModelError(string.Empty, fakeModelErrorMessage);

            // Act
            var result = await authController.Register(new RegisterViewModel());
            var errorMessageFromController = authController.ModelState.Values.FirstOrDefault()
                .Errors.FirstOrDefault().ErrorMessage;

            // Assert
            Assert.That(errorMessageFromController, Is.EqualTo(fakeModelErrorMessage));
            Assert.IsInstanceOf<ViewResult>(result);
        }


        [TestCaseSource(nameof(RegisterViewModelsTestData))]
        public async Task Register_WithVMAndCreateUserError_ReturnsView(RegisterViewModel viewModel)
        {
            // Arrange 
            var error = new IdentityError()
                {Code = "User cant be created", Description = "The program failed to create user"};
            var identityResult = IdentityResult.Failed(new List<IdentityError> {error}.ToArray());

            fakeUserManager.Setup(manager => manager.CreateAsync(It.IsAny<User>(), viewModel.Password))
                .Returns(Task.FromResult<IdentityResult>(identityResult));
            fakeUserManager.Setup(manager => manager.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()))
                .Returns(Task.FromResult<IdentityResult>(IdentityResult.Success));
            fakeSignInManager.Setup(manager => manager.SignInAsync(
                    It.IsAny<User>(), It.IsAny<bool>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            DefaultHttpContext httpContext = new DefaultHttpContext();
            httpContext.Request.Scheme = "http";
            httpContext.Request.Host = new HostString("localhost");
            var formCol = new FormCollection(new Dictionary<string, StringValues>
            {
                  { "Provider", "Some value" },
            });

            httpContext.Request.ContentType = "application/x-www-form-urlencoded";
            httpContext.Request.Form = formCol;

            authController.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Act
            var result = await authController.Register(viewModel);
            var errorMessageFromController = authController.ModelState.Values.FirstOrDefault()
                .Errors.FirstOrDefault().ErrorMessage;

            // Assert
            Assert.That(errorMessageFromController, Is.EqualTo(error.Description));
            Assert.IsInstanceOf<ViewResult>(result);
        }

        [Test]
        public void ExternalLogin_ReturnsNotImplementedEx()
        {
            // Arrange
            var authController = this.authController;

            // Assert & Act
            Assert.ThrowsAsync<NotImplementedException>(() =>
                authController.ExternalLogin("Provider", "return url"));
        }
        
        public static IEnumerable<TestCaseData> RegisterViewModelsTestData =>
            new List<TestCaseData>()
            {
                new TestCaseData(new RegisterViewModel()
                {
                    Email = "test123@gmail.com",
                    PhoneNumber = "0502391222",
                    ReturnUrl = "Return url",
                }),
                new TestCaseData(new RegisterViewModel()
                {
                    Email = "test123@gmail.com",
                    PhoneNumber = "0502391222",
                    ReturnUrl = "Return url2",
                }),
            };

        public static IEnumerable<LoginViewModel> LoginViewModels
        {
            get => new List<LoginViewModel>()
            {
                new LoginViewModel
                {
                    ReturnUrl = "Return url",
                    ExternalProviders = EmptySchemes,
                },
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
    }
}