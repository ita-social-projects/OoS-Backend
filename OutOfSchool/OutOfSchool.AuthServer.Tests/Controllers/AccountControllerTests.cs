using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.EmailSender;
using OutOfSchool.AuthCommon.Controllers;
using OutOfSchool.AuthCommon.ViewModels;
using Microsoft.AspNetCore.Identity;
using OutOfSchool.Services.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using OutOfSchool.AuthCommon;
using OutOfSchool.Common;
using OutOfSchool.AuthCommon.Config;
using OutOfSchool.RazorTemplatesData.Services;
using OutOfSchool.AuthCommon.Services.Interfaces;
using System.Net;

namespace OutOfSchool.AuthServer.Tests.Controllers;

public class AccountControllerTests
{
    private AccountController accountController;
    private readonly Mock<FakeSignInManager> fakeSignInManager;
    private readonly Mock<FakeUserManager> fakeUserManager;
    private readonly Mock<IEmailSender> fakeEmailSender;
    private readonly Mock<ILogger<AccountController>> fakeLogger;
    private readonly Mock<IStringLocalizer<SharedResource>> fakeLocalizer;
    private readonly Mock<IRazorViewToStringRenderer> fakeRazorViewToStringRenderer;
    private readonly Mock<IOptions<AuthServerConfig>> fakeIdentityServerConfig;
    private readonly Mock<IUserService> fakeUserService;

    public AccountControllerTests()
    {
        fakeSignInManager = new Mock<FakeSignInManager>();
        fakeUserManager = new Mock<FakeUserManager>();
        fakeEmailSender = new Mock<IEmailSender>();
        fakeLogger = new Mock<ILogger<AccountController>>();
        fakeLocalizer = new Mock<IStringLocalizer<SharedResource>>();
        fakeRazorViewToStringRenderer = new Mock<IRazorViewToStringRenderer>();
        fakeIdentityServerConfig = new Mock<IOptions<AuthServerConfig>>();
        fakeUserService = new Mock<IUserService>();
    }

    [SetUp]
    public void Setup()
    {
        fakeLocalizer
            .Setup(localizer => localizer[It.IsAny<string>()])
            .Returns(new LocalizedString("mock", "error"));

        fakeIdentityServerConfig
            .SetupGet(c => c.Value)
            .Returns(new Mock<AuthServerConfig>().Object);

        accountController = new AccountController(
            fakeSignInManager.Object,
            fakeUserManager.Object,
            fakeEmailSender.Object,
            fakeLogger.Object,
            fakeRazorViewToStringRenderer.Object,
            fakeLocalizer.Object,
            fakeIdentityServerConfig.Object,
            fakeUserService.Object
        );
        
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(IdentityResourceClaimsTypes.Sub, "example"),
        }, "mock"));
        
        accountController.ControllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    [Test]
    public void ForgotPassword_ReturnsViewResult()
    {
        // Arrange
        var returnUrl = "Return url";

        // Act
        IActionResult result = accountController.ForgotPassword(returnUrl);
        var forgotPasswordResultModel = (ForgotPasswordViewModel)((ViewResult)result).Model;

        // Assert
        Assert.That(forgotPasswordResultModel.ReturnUrl, Is.EqualTo(returnUrl));
        Assert.IsInstanceOf<ViewResult>(result);
    }

    #region ResetPasswordTests

    [Test]
    public async Task ResetPasswordGet_EmptyFields_ReturnsBadViewResult()
    {
        // Arrange

        // Act
        IActionResult result = await accountController.ResetPassword(null, null);

        // Assert
        Assert.IsInstanceOf<BadRequestObjectResult>(result);
    }

    [Test]
    public async Task ResetPasswordGet_EmailNotFound_ReturnsErrorMessage()
    {
        // Arrange
        fakeUserManager.Setup(userManager => userManager.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User)null);

        // Act
        IActionResult result = await accountController.ResetPassword("token", "email");
        string modelMessage = (LocalizedString)((ViewResult)result).Model;

        // Assert
        Assert.AreEqual("error", modelMessage);
    }

    [Test]
    public async Task ResetPasswordGet_TokenInvalid_ReturnsErrorMessage()
    {
        // Arrange
        fakeUserManager.Setup(userManager => userManager.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(new User());

        fakeUserManager.Setup(userManager => userManager.VerifyUserTokenAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(false);

        // Act
        IActionResult result = await accountController.ResetPassword("token", "email");
        string modelMessage = (LocalizedString)((ViewResult)result).Model;

        // Assert
        Assert.AreEqual("error", modelMessage);
    }

    [Test]
    public async Task ResetPasswordGet_TokenValid_ReturnsResetPasswordViewModel()
    {
        // Arrange
        var token = "token";
        var email = "test@email.com";
        fakeUserManager.Setup(userManager => userManager.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(new User());

        fakeUserManager.Setup(userManager => userManager.VerifyUserTokenAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act
        IActionResult result = await accountController.ResetPassword(token, email);
        ResetPasswordViewModel resetPasswordViewModel = (ResetPasswordViewModel)((ViewResult)result).Model;

        // Assert
        Assert.AreEqual(resetPasswordViewModel.Token, token);
        Assert.AreEqual(resetPasswordViewModel.Email, email);
    }

    [Test]
    public async Task ResetPasswordPost_InvalidModel_ReturnsResetPasswordViewModel()
    {
        // Arrange
        var fakeErrorMessage = "Model is invalid";
        accountController.ModelState.AddModelError(string.Empty, fakeErrorMessage);

        // Act
        IActionResult result = await accountController.ResetPassword(new ResetPasswordViewModel());
        ResetPasswordViewModel resetPasswordViewModelResult = (ResetPasswordViewModel)((ViewResult)result).Model;

        // Assert
        Assert.IsInstanceOf<ResetPasswordViewModel>(resetPasswordViewModelResult);
    }

    [Test]
    public async Task ResetPasswordPost_EmailNotFound_ReturnsResetPasswordViewModel()
    {
        // Arrange
        fakeUserManager.Setup(userManager => userManager.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User)null);
            
        // Act
        IActionResult result = await accountController.ResetPassword(new ResetPasswordViewModel());
        string modelMessage = (LocalizedString)((ViewResult)result).Model;

        // Assert
        Assert.AreEqual("error", modelMessage);
    }

    [Test]
    public async Task ResetPasswordPost_Success_ReturnsResetPasswordConfirmation()
    {
        // Arrange
        fakeUserManager.Setup(userManager => userManager.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(new User());
        fakeUserManager.Setup(userManager => userManager.ResetPasswordAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        IActionResult result = await accountController.ResetPassword(new ResetPasswordViewModel());
        ViewResult viewResult = (ViewResult)result;

        // Assert
        Assert.AreEqual("Password/ResetPasswordConfirmation", viewResult.ViewName);
    }

    [Test]
    public async Task ResetPasswordPost_Failed_ReturnsResetPasswordConfirmation()
    {
        // Arrange
        fakeUserManager.Setup(userManager => userManager.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(new User());
        fakeUserManager.Setup(userManager => userManager.ResetPasswordAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(null));

        // Act
        IActionResult result = await accountController.ResetPassword(new ResetPasswordViewModel());
        ViewResult viewResult = (ViewResult)result;

        // Assert
        Assert.AreEqual("Password/ResetPasswordFailed", viewResult.ViewName);
    }
    #endregion

    #region LogOutUserTests

    [Test]
    public async Task LogOutUser_WhenIdIsNull_ThrowsArgumentException()
    {
        // Arrange
        var invalidId = null as string;

        // Act
        var result = await accountController.LogOutUser(invalidId);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, result.HttpStatusCode);
    }

    [Test]
    public async Task LogOutUser_WhenIdIsEmpty_ThrowsArgumentException()
    {
        // Arrange
        var invalidId = string.Empty;

        // Act
        var result = await accountController.LogOutUser(invalidId);

        // Assert
        Assert.AreEqual(HttpStatusCode.BadRequest, result.HttpStatusCode);
    }

    [Test]
    public async Task LogOutUser_WhenIdIsValid_ReturnOkResponse()
    {
        // Arrange
        var validId = "fakeValidId";
        fakeUserService.Setup(x => x.LogOutUserById(It.IsAny<string>())).ReturnsAsync(new ResponseDto { IsSuccess = true, HttpStatusCode = HttpStatusCode.OK });

        // Act
        var result = await accountController.LogOutUser(validId);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, result.HttpStatusCode);
        Assert.That(result.IsSuccess);
    }

    #endregion
}