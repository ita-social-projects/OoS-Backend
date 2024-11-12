using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.FeatureManagement;
using Moq;
using NUnit.Framework;
using OutOfSchool.AuthCommon;
using OutOfSchool.AuthCommon.Config;
using OutOfSchool.AuthCommon.Controllers;
using OutOfSchool.AuthCommon.Services.Interfaces;
using OutOfSchool.AuthCommon.ViewModels;
using OutOfSchool.EmailSender.Services;
using OutOfSchool.RazorTemplatesData.Services;
using OutOfSchool.Services.Models;
using OutOfSchool.Tests.Common.TestDataGenerators;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace OutOfSchool.AuthServer.Tests.Controllers;

[TestFixture]
public class AuthControllerTests
{
    private Mock<FakeUserManager> fakeUserManager;
    private Mock<IUserManagerAdditionalService> fakeUserManagerAdditionalService;
    private Mock<FakeSignInManager> fakeSignInManager;
    private Mock<IInteractionService> fakeInteractionService;
    private Mock<ILogger<AuthController>> fakeLogger;
    private AuthController authController;
    private Mock<IStringLocalizer<SharedResource>> fakeLocalizer;
    private static Mock<IOptions<AuthServerConfig>> fakeIdentityServerConfig;
    private Mock<IEmailSenderService> fakeEmailSender;
    private Mock<IRazorViewToStringRenderer> fakeRenderer;
    private Mock<IFeatureManager> fakeFeatureManager;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        var config = new AuthServerConfig();
        config.RedirectToStartPageUrl = "http://localhost:4200";

        fakeIdentityServerConfig = new Mock<IOptions<AuthServerConfig>>();
        fakeIdentityServerConfig.Setup(m => m.Value).Returns(config);
    }

    [SetUp]
    public void Setup()
    {
        fakeUserManager = new Mock<FakeUserManager>();
        fakeUserManagerAdditionalService = new Mock<IUserManagerAdditionalService>();
        fakeInteractionService = new Mock<IInteractionService>();
        fakeSignInManager = new Mock<FakeSignInManager>();
        fakeLogger = new Mock<ILogger<AuthController>>();
        fakeLocalizer = new Mock<IStringLocalizer<SharedResource>>();
        fakeEmailSender = new Mock<IEmailSenderService>();
        fakeRenderer = new Mock<IRazorViewToStringRenderer>();
        fakeFeatureManager = new Mock<IFeatureManager>();

        fakeLocalizer
            .Setup(localizer => localizer[It.IsAny<string>()])
            .Returns(new LocalizedString("mock", "error"));
        authController = new AuthController(
            fakeUserManager.Object,
            fakeUserManagerAdditionalService.Object,
            fakeSignInManager.Object,
            fakeInteractionService.Object, 
            fakeLogger.Object,
            fakeLocalizer.Object,
            fakeIdentityServerConfig.Object,
            fakeRenderer.Object,
            fakeEmailSender.Object,
            fakeFeatureManager.Object);
    }

    [Test]
    public async Task Logout_WithLogoutId_ReturnsRedirectResult()
    {
        // Arrange
        var PostLogoutRedirectUri = "True logout id";
            
        fakeSignInManager.Setup(manager => manager.SignOutAsync())
            .Returns(Task.CompletedTask);
        fakeInteractionService.Setup(service => service.GetPostLogoutRedirectUri(It.IsAny<string>()))
            .Returns(Task.FromResult(PostLogoutRedirectUri));
        // Act
        var result = await authController.Logout("Any logout ID");

        // Assert
        Assert.IsInstanceOf(typeof(RedirectResult), result);
    }

    [Test]
    public async Task Logout_WithoutLogoutId_ReturnsRedirectResult()
    {
        // Arrange
        var PostLogoutRedirectUri = "";

        fakeSignInManager.Setup(manager => manager.SignOutAsync())
            .Returns(Task.CompletedTask);
        fakeInteractionService.Setup(service => service.GetPostLogoutRedirectUri(It.IsAny<string>()))
            .Returns(Task.FromResult(PostLogoutRedirectUri));

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
    public async Task Login_WhenUserMustChangePasswordAndEmailWithPasswordAreCorrect_ReturnsRedirectActionToChangePasswordLogin()
    {
        // Arrange
        var user = UserGenerator.Generate();
        var loginViewModel = CreateLoginViewModelFromData();
        user.MustChangePassword = true;
        SetupDefaultUserManagerFindByEmailAsync(user);
        SetupDefaultSignInManagerCheckPasswordSignInAsync(SignInResult.Success);

        // Act
        var result = await authController.Login(loginViewModel);

        // Assert
        Assert.IsInstanceOf(typeof(RedirectToActionResult), result);
        Assert.AreEqual(0, authController.ModelState.Count);
        Assert.AreEqual(nameof(AuthController.ChangePasswordLogin), (result as RedirectToActionResult)?.ActionName);
    }

    [Test]
    public async Task Login_WhenUserMustChangePasswordAndEmailWithPasswordAreNotCorrect_ReturnsViewWithError()
    {
        // Arrange
        var user = UserGenerator.Generate();
        var loginViewModel = CreateLoginViewModelFromData();
        user.MustChangePassword = true;
        SetupDefaultUserManagerFindByEmailAsync(user);
        SetupDefaultSignInManagerCheckPasswordSignInAsync(SignInResult.Failed);

        // Act
        var result = await authController.Login(loginViewModel);

        // Assert
        Assert.IsInstanceOf(typeof(ViewResult), result);
        Assert.AreEqual(1, authController.ModelState.Count);
    }

    [Test]
    public async Task Login_WhenUserMustNotChangePasswordAndEmailWithPasswordAreCorrectAndReturnUrlIsNotEmpty_ReturnsRedirectToReturnUrl()
    {
        // Arrange
        var user = UserGenerator.Generate();
        var loginViewModel = CreateLoginViewModelFromData(user.UserName);
        user.MustChangePassword = false;
        SetupDefaultUserManagerFindByEmailAsync(user);
        SetupDefaultUserManagerUpdateAsync(IdentityResult.Success);
        SetupDefaultSignInManagerPasswordSignInAsync(SignInResult.Success);

        // Act
        var result = await authController.Login(loginViewModel);

        // Assert
        Assert.IsInstanceOf(typeof(RedirectResult), result);
        Assert.AreEqual(0, authController.ModelState.Count);
        Assert.AreEqual(loginViewModel.ReturnUrl, (result as RedirectResult)?.Url);
    }

    [Test]
    public async Task Login_WhenUserIsDeleted_ReturnsViewWithError()
    {
        // Arrange
        var user = UserGenerator.Generate();
        user.IsDeleted = true;
        var loginViewModel = CreateLoginViewModelFromData(user.UserName);
        SetupDefaultUserManagerFindByEmailAsync(user);

        // Act
        var result = await authController.Login(loginViewModel);

        // Assert
        Assert.IsInstanceOf(typeof(ViewResult), result);
        Assert.AreEqual(1, authController.ModelState.Count);
    }

    [Test]
    public async Task Login_WhenUserMustNotChangePasswordAndEmailWithPasswordAreCorrectAndReturnUrlIsEmpty_ReturnsRedirectToLogin()
    {
        // Arrange
        var user = UserGenerator.Generate();
        var loginViewModel = CreateLoginViewModelFromData(user.UserName, returnUrl: string.Empty);
        user.MustChangePassword = false;
        SetupDefaultUserManagerFindByEmailAsync(user);
        SetupDefaultUserManagerUpdateAsync(IdentityResult.Success);
        SetupDefaultSignInManagerPasswordSignInAsync(SignInResult.Success);

        // Act
        var result = await authController.Login(loginViewModel);

        // Assert
        Assert.IsInstanceOf(typeof(RedirectResult), result);
        Assert.AreEqual(0, authController.ModelState.Count);
        Assert.AreEqual(nameof(AuthController.Login), (result as RedirectResult)?.Url);
    }

    [Test]
    public async Task ChangePasswordLogin_WhenUserMustChangePasswordAndEmailWithPasswordsAreCorrectAndReturnUrlIsNotEmpty_ReturnsRedirectToReturnUrl()
    {
        // Arrange
        var user = UserGenerator.Generate();
        user.MustChangePassword = true;
        var changePasswordLoginViewModel = CreateChangePasswordLoginViewModelFromData();
        SetupDefaultUserManagerFindByEmailAsync(user);
        fakeUserManagerAdditionalService.Setup(s =>
            s.ChangePasswordWithRequiredMustChangePasswordAsync(user, It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(() => { user.MustChangePassword = false; return IdentityResult.Success;});
        SetupDefaultSignInManagerCheckPasswordSignInAsync(SignInResult.Success);
        fakeSignInManager.Setup(s => s.GetExternalAuthenticationSchemesAsync())
            .ReturnsAsync(new List<AuthenticationScheme>());
        SetupDefaultUserManagerUpdateAsync(IdentityResult.Success);
        SetupDefaultSignInManagerPasswordSignInAsync(SignInResult.Success);

        // Act
        var result = await authController.ChangePasswordLogin(changePasswordLoginViewModel);

        // Assert
        Assert.IsInstanceOf(typeof(RedirectResult), result);
        Assert.AreEqual(0, authController.ModelState.Count);
        Assert.AreEqual(changePasswordLoginViewModel.ReturnUrl, (result as RedirectResult)?.Url);
    }

    [Test]
    public async Task ChangePasswordLogin_WhenUserMustChangePasswordAndEmailWithPasswordsAreCorrectAndReturnUrlIsEmpty_ReturnsRedirectToLogin()
    {
        // Arrange
        var user = UserGenerator.Generate();
        user.MustChangePassword = true;
        var changePasswordLoginViewModel = CreateChangePasswordLoginViewModelFromData(returnUrl: string.Empty);
        SetupDefaultUserManagerFindByEmailAsync(user);
        fakeUserManagerAdditionalService.Setup(s =>
                s.ChangePasswordWithRequiredMustChangePasswordAsync(user, It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(() => { user.MustChangePassword = false; return IdentityResult.Success;});
        SetupDefaultSignInManagerCheckPasswordSignInAsync(SignInResult.Success);
        fakeSignInManager.Setup(s => s.GetExternalAuthenticationSchemesAsync())
            .ReturnsAsync(new List<AuthenticationScheme>());
        SetupDefaultUserManagerUpdateAsync(IdentityResult.Success);
        SetupDefaultSignInManagerPasswordSignInAsync(SignInResult.Success);

        // Act
        var result = await authController.ChangePasswordLogin(changePasswordLoginViewModel);

        // Assert
        Assert.IsInstanceOf(typeof(RedirectResult), result);
        Assert.AreEqual(0, authController.ModelState.Count);
        Assert.AreEqual(nameof(AuthController.Login), (result as RedirectResult)?.Url);
    }

    [Test]
    public async Task ChangePasswordLogin_WhenUserMustNotChangePassword_ReturnsViewWithModelError()
    {
        // Arrange
        var user = UserGenerator.Generate();
        user.MustChangePassword = false;
        var changePasswordLoginViewModel = CreateChangePasswordLoginViewModelFromData(returnUrl: string.Empty);
        SetupDefaultUserManagerFindByEmailAsync(user);

        // Act
        var result = await authController.ChangePasswordLogin(changePasswordLoginViewModel);

        // Assert
        Assert.IsInstanceOf(typeof(ViewResult), result);
        Assert.AreEqual(1, authController.ModelState.Count);
    }

    [Test] public async Task ChangePasswordLogin_WhenUserMustChangePasswordAndEmailWithPasswordAreNotCorrect_ReturnsViewWithModelError()
    {
        // Arrange
        var user = UserGenerator.Generate();
        user.MustChangePassword = true;
        var changePasswordLoginViewModel = CreateChangePasswordLoginViewModelFromData();
        SetupDefaultUserManagerFindByEmailAsync(user);
        SetupDefaultSignInManagerCheckPasswordSignInAsync(SignInResult.Failed);

        // Act
        var result = await authController.ChangePasswordLogin(changePasswordLoginViewModel);

        // Assert
        Assert.IsInstanceOf(typeof(ViewResult), result);
        Assert.AreEqual(1, authController.ModelState.Count);
    }

    [Test] public async Task ChangePasswordLogin_WhenUserNotFound_ReturnsViewWithModelError()
    {
        // Arrange
        var changePasswordLoginViewModel = CreateChangePasswordLoginViewModelFromData();
        SetupDefaultUserManagerFindByEmailAsync(null);

        // Act
        var result = await authController.ChangePasswordLogin(changePasswordLoginViewModel);

        // Assert
        Assert.IsInstanceOf(typeof(ViewResult), result);
        Assert.AreEqual(1, authController.ModelState.Count);
    }
    
    [Test]
    public void Register_WithoutReturnUrl_ReturnsViewResult()
    {
        // Arrange 

        // Act
        IActionResult result = authController.Register();
        RegisterViewModel viewResultModel = (RegisterViewModel) ((ViewResult) result).Model;

        // Assert
        Assert.That(viewResultModel.ReturnUrl, Is.EqualTo("login"));
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
        
    public static IEnumerable<TestCaseData> RegisterViewModelsTestData =>
        new List<TestCaseData>()
        {
            new TestCaseData(new RegisterViewModel()
            {
                Email = "test123@gmail.com",
                ReturnUrl = "Return url",
            }),
            new TestCaseData(new RegisterViewModel()
            {
                Email = "test123@gmail.com",
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
    
    private void SetupDefaultUserManagerFindByEmailAsync(User user)
        => fakeUserManager.Setup(s => s.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);

    private void SetupDefaultSignInManagerCheckPasswordSignInAsync(SignInResult signInResult)
        => fakeSignInManager.Setup(s => s
                .CheckPasswordSignInAsync(It.IsAny<User>(), It.IsAny<string>(), false))
                .ReturnsAsync(signInResult);

    private void SetupDefaultUserManagerUpdateAsync(IdentityResult identityResult)
        => fakeUserManager.Setup(s => s
            .UpdateAsync(It.IsAny<User>())).ReturnsAsync(identityResult);

    private void SetupDefaultSignInManagerPasswordSignInAsync(SignInResult signInResult)
        => fakeSignInManager.Setup(s => s.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), false, false))
            .ReturnsAsync(signInResult);
    private static LoginViewModel CreateLoginViewModelFromData(
        string email = "randomuser@test.com",
        string password = "RandomPassword3%",
        string returnUrl = "RandomReturnUrl",
        IEnumerable<AuthenticationScheme> authenticationSchemes = default)
    {
        return new LoginViewModel
        {
            Username = email,
            Password = password,
            ReturnUrl = returnUrl,
            ExternalProviders = authenticationSchemes
        };
    }

    private static ChangePasswordLoginViewModel CreateChangePasswordLoginViewModelFromData(
        string email = "randomuser@test.com",
        string password = "RandomPassword5%",
        string newPassword = "RandomPassword3%",
        string confirmNewPassword = "RandomPassword3%",
        string returnUrl = "RandomReturnUrl")
    {
        return new ChangePasswordLoginViewModel
        {
            Email = email,
            CurrentPassword = password,
            NewPassword = newPassword,
            ConfirmNewPassword = confirmNewPassword,
            ReturnUrl = returnUrl
        };
    }
}