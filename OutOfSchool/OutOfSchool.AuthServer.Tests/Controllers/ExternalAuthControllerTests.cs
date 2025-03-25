using System;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using OpenIddict.Abstractions;
using OpenIddict.Client;
using OpenIddict.Client.AspNetCore;
using OutOfSchool.AikomApiClient;
using OutOfSchool.AikomApiClient.Models.Data;
using OutOfSchool.AuthCommon;
using OutOfSchool.AuthCommon.Config;
using OutOfSchool.AuthCommon.Controllers;
using OutOfSchool.AuthCommon.Services.Interfaces;
using OutOfSchool.Common.Models;
using OutOfSchool.Common.Models.ExternalAuth;
using OutOfSchool.Services;
using OutOfSchool.Services.Models;
using OutOfSchool.Tests.Common.DbContextTests;

namespace OutOfSchool.AuthServer.Tests.Controllers;

[TestFixture]
public class ExternalAuthControllerTests
{
    private const string LoginViewCshtml = "~/Views/Auth/Login.cshtml";
    private const string RedirectUrl = "/test";
    private const string TestRnkopp = "1234567";
    private const string TestEdrpou = "0987654321";
    private const long TestExternalProviderId = 12345;
    private Mock<FakeSignInManager> signInManager;
    private Mock<FakeUserManager> userManager;
    private Mock<FakeRoleManager> roleManager;
    private Mock<ILogger<ExternalAuthController>> logger;
    private Mock<IOptions<AuthorizationServerConfig>> authServerOptions;
    private Mock<IStringLocalizer<SharedResource>> localizer;
    private Mock<IGovIdentityCommunicationService> communicationService;
    private Mock<IAikomProviderService> aikomProviderService;
    private Mock<OpenIddictClientService> openIddictClientService;
    private OutOfSchoolDbContext dbContext;
    private HttpContext httpContext;
    private Mock<IAuthenticationService> authenticationService;
    private AuthorizationServerConfig authServerConfig;
    private ExternalAuthController controller;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        authServerConfig = new AuthorizationServerConfig
        {
            ExternalLogin = new ExternalLogin
            {
                Parameters = new Parameters
                {
                    AuthType = new AuthType
                    {
                        Key = "key",
                        Business = "business",
                    }
                }
            }
        };
        dbContext = GetContext();
    }

    [SetUp]
    public void Setup()
    {
        userManager = new Mock<FakeUserManager>();
        signInManager = new Mock<FakeSignInManager>();
        roleManager = new Mock<FakeRoleManager>();
        logger = new Mock<ILogger<ExternalAuthController>>();
        localizer = new Mock<IStringLocalizer<SharedResource>>();
        communicationService = new Mock<IGovIdentityCommunicationService>();
        aikomProviderService = new Mock<IAikomProviderService>();
        authServerOptions = new Mock<IOptions<AuthorizationServerConfig>>();
        openIddictClientService = new Mock<OpenIddictClientService>(new Mock<IServiceProvider>().Object);

        authServerOptions.Setup(o => o.Value).Returns(authServerConfig);

        localizer
            .Setup(localizer => localizer[It.IsAny<string>()])
            .Returns(new LocalizedString("mock", "error"));

        authenticationService = new Mock<IAuthenticationService>();
        httpContext = new DefaultHttpContext();

        controller = new ExternalAuthController(
            signInManager.Object,
            userManager.Object,
            roleManager.Object,
            logger.Object,
            authServerOptions.Object,
            localizer.Object,
            communicationService.Object,
            dbContext,
            aikomProviderService.Object,
            openIddictClientService.Object);

        controller.ControllerContext.HttpContext = httpContext;
        controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());
        controller.HttpContext.RequestServices = new ServiceCollection()
            .AddSingleton(authenticationService.Object)
            .BuildServiceProvider();
        dbContext.Database.EnsureDeleted();
        dbContext.Database.EnsureCreated();
    }

    [Test]
    public async Task ExternalLogin_WithExistingRole_ReturnsChallengeResult()
    {
        // Arrange
        var expectedRole = "provider";
        var externalAuthProvider = "test";
        var returnUrl = "https://example.com";
        roleManager.Setup(r => r.RoleExistsAsync(expectedRole)).ReturnsAsync(true);
        signInManager.Setup(s => s.ConfigureExternalAuthenticationProperties(externalAuthProvider, returnUrl, null))
            .Returns(new AuthenticationProperties());
        openIddictClientService.Setup(o =>
                o.GetClientRegistrationByProviderNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OpenIddictClientRegistration()
            {
                RegistrationId = "test"
            });

        // Act
        var result = await controller.ExternalLogin(externalAuthProvider, expectedRole, returnUrl);

        // Assert
        Assert.IsInstanceOf<ChallengeResult>(result);
        var challengeResult = (ChallengeResult) result;
        Assert.AreEqual(OpenIddictClientAspNetCoreDefaults.AuthenticationScheme,
            challengeResult.AuthenticationSchemes.First());
        var role = challengeResult.Properties?.Items[AuthServerConstants.ExternalAuthSelectedRoleKey];
        Assert.AreEqual(expectedRole, role);
    }

    [Test]
    public async Task ExternalLogin_WithNonExistingRole_ReturnsViewResult()
    {
        // Arrange
        roleManager.Setup(r => r.RoleExistsAsync(It.IsAny<string>())).ReturnsAsync(false);

        // Act
        var result = await controller.ExternalLogin("test", "kfdjhg", "https://example.com");

        // Assert
        Assert.IsInstanceOf<ViewResult>(result);
        var viewResult = (ViewResult) result;
        Assert.AreEqual(LoginViewCshtml, viewResult.ViewName);
        Assert.NotNull(viewResult.Model);
    }

    [Test]
    public async Task ExternalLoginCallback_WithCorrectAuthFlow_ReturnsRedirectResult()
    {
        //Arrange
        var user = GetUser();
        SetupSuccessAuth();
        SetupSuccessProviderVerification();
        userManager.Setup(u => u.FindByNameAsync(TestRnkopp)).ReturnsAsync(user);

        // Act
        var result = await controller.ExternalLoginCallback();

        // Assert
        // TODO: while AIKOM is not operational, do not check anything.
        // signInManager.Verify(s =>
        //         s.SignInWithClaimsAsync(user, It.IsAny<AuthenticationProperties>(), It.Is<IEnumerable<Claim>>(claims =>
        //             claims.Any(c => c.Type == Constants.ClaimTypes.AikomProviderId && c.Value == TestExternalProviderId.ToString()))),
        //     Times.Once);

        Assert.IsInstanceOf<RedirectResult>(result);
        var viewResult = (RedirectResult)result;
        Assert.AreEqual(RedirectUrl, viewResult.Url);
    }

    [Test]
    public async Task ExternalLoginCallback_WhenUserNotExist_CreateUser()
    {
        //Arrange
        var user = GetUser();

        SetupSuccessAuth();
        SetupSuccessProviderVerification();

        userManager.SetupSequence(u => u.FindByNameAsync(TestRnkopp))
            .ReturnsAsync((User) null)
            .ReturnsAsync(user);
        userManager.Setup(u => u.CreateAsync(It.IsAny<User>())).ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await controller.ExternalLoginCallback();

        // Assert
        userManager.Verify(u => u.CreateAsync(It.IsAny<User>()), Times.Once);
        userManager.Verify(u => u.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Once);

        Assert.IsInstanceOf<RedirectResult>(result);
        var viewResult = (RedirectResult) result;
        Assert.AreEqual(RedirectUrl, viewResult.Url);
    }

    [Test]
    public async Task ExternalLoginCallback_WithFailedAuthFlow_ReturnsViewResult()
    {
        //Arrange
        var authResult = AuthenticateResult.Fail(new Exception(), new AuthenticationProperties
        {
            RedirectUri = RedirectUrl
        });
        authenticationService.Setup(a =>
                a.AuthenticateAsync(httpContext, OpenIddictClientAspNetCoreDefaults.AuthenticationScheme))
            .ReturnsAsync(authResult);
        signInManager.Setup(s => s.GetExternalAuthenticationSchemesAsync()).ReturnsAsync([
            new AuthenticationScheme("test", "test", typeof(IAuthenticationHandler))
        ]);

        // Act
        var result = await controller.ExternalLoginCallback();

        // Assert
        Assert.IsInstanceOf<ViewResult>(result);
        var viewResult = (ViewResult) result;
        Assert.AreEqual(LoginViewCshtml, viewResult.ViewName);
        Assert.NotNull(viewResult.Model);
    }
    
    [Test]
    public async Task ExternalLogin_WithNullRole_ReturnsViewResult()
    {
        // Act
        var result = await controller.ExternalLogin("test", null, "https://example.com");

        // Assert
        Assert.IsInstanceOf<ViewResult>(result);
    }

    [Test]
    public async Task ExternalLogin_WithEmptyProvider_ReturnsViewResult()
    {
        // Act
        var result = await controller.ExternalLogin(string.Empty, "provider", "https://example.com");

        // Assert
        Assert.IsInstanceOf<ViewResult>(result);
    }

    [Test]
    public async Task ExternalLogin_WithInvalidReturnUrl_ReturnsViewResult()
    {
        // Act
        var result = await controller.ExternalLogin("test", "provider", "invalid-url");

        // Assert
        Assert.IsInstanceOf<ViewResult>(result);
    }
    
    [Test]
    public async Task ExternalLoginCallback_WhenUserCreationFails_ReturnsViewResult()
    {
        // Arrange
        SetupSuccessAuth();
        userManager.Setup(u => u.FindByNameAsync(TestRnkopp))
            .ReturnsAsync((User)null);
        userManager.Setup(u => u.CreateAsync(It.IsAny<User>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Error" }));

        // Act
        var result = await controller.ExternalLoginCallback();

        // Assert
        Assert.IsInstanceOf<ViewResult>(result);
    }

    [Test]
    [Ignore("While AIKOM is not operational, do not check anything.")]
    public async Task ExternalLoginCallback_WithFailedProviderVerification_ReturnsViewResult()
    {
        //Arrange
        var user = GetUser();
        SetupSuccessAuth();
        SetupFailedProviderVerification();
        userManager.Setup(u => u.FindByNameAsync(TestRnkopp)).ReturnsAsync(user);

        // Act
        var result = await controller.ExternalLoginCallback();

        // Assert
        Assert.IsInstanceOf<ViewResult>(result);
        var viewResult = (ViewResult)result;
        Assert.AreEqual(LoginViewCshtml, viewResult.ViewName);
        Assert.NotNull(viewResult.Model);
    }

    [Test]
    [Ignore("While AIKOM is not operational, do not check anything.")]
    public async Task ExternalLoginCallback_WithErrorProviderVerification_ReturnsViewResult()
    {
        //Arrange
        var user = GetUser();
        SetupSuccessAuth();
        SetupErrorProviderVerification();
        userManager.Setup(u => u.FindByNameAsync(TestRnkopp)).ReturnsAsync(user);

        // Act
        var result = await controller.ExternalLoginCallback();

        // Assert
        Assert.IsInstanceOf<ViewResult>(result);
        var viewResult = (ViewResult)result;
        Assert.AreEqual(LoginViewCshtml, viewResult.ViewName);
        Assert.NotNull(viewResult.Model);
    }

    private static OutOfSchoolDbContext GetContext()
    {
        return new TestOutOfSchoolDbContext(
            new DbContextOptionsBuilder<OutOfSchoolDbContext>()
                .UseInMemoryDatabase(databaseName: "OutOfSchoolTestDB")
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options);
    }
    
    private void SetupSuccessAuth()
    {
        var principal = new ClaimsPrincipal();
        principal.AddIdentity(new ClaimsIdentity());
        principal.SetClaim(AuthServerConstants.ExternalAuthUserIdKey, "test");
        principal.SetClaim(OpenIddictConstants.Claims.Private.ProviderName, "external");
        principal.SetClaim(OpenIddictConstants.Claims.Private.RegistrationId, "12345");
        var properties = new AuthenticationProperties
        {
            RedirectUri = RedirectUrl,
            Items =
            {
                {".Token." + OpenIddictClientAspNetCoreConstants.Tokens.BackchannelAccessToken, "secret"},
                {AuthServerConstants.ExternalAuthSelectedRoleKey, "provider"}
            },
        };
        var ticket = new AuthenticationTicket(principal, properties,
            OpenIddictClientAspNetCoreDefaults.AuthenticationScheme);
        var authResult = AuthenticateResult.Success(ticket);
        authenticationService.Setup(a =>
                a.AuthenticateAsync(httpContext, OpenIddictClientAspNetCoreDefaults.AuthenticationScheme))
            .ReturnsAsync(authResult);
        communicationService.Setup(c => c.GetUserInfo("test", "secret"))
            .ReturnsAsync(GetUserInfoResponse());
    }

    private static User GetUser()
    {
        return new User
        {
            UserName = TestRnkopp,
            FirstName = "test",
            LastName = "test",
            MiddleName = "test",
            Email = "test@test.com",
            CreatingTime = DateTimeOffset.UtcNow,
            IsRegistered = false,
            IsBlocked = false,
            MustChangePassword = false,
        };
    }

    private static UserInfoResponse GetUserInfoResponse()
    {
        return new UserInfoResponse
        {
            DrfoCode = TestRnkopp,
            GivenName = "test",
            LastName = "test",
            MiddleName = "test",
            Email = "test@test.com",
            EdrpouCode = TestEdrpou
        };
    }

    private void SetupSuccessProviderVerification()
    {
        var response = new AikomProviderResponse
        {
            HasAccess = true,
            ProviderInfo = new AikomProviderInfo
            {
                Id = TestExternalProviderId,
                FullName = "Test Provider",
                ShortName = "Test",
                Address = "Test Address",
                Email = "test@test.com",
                Phone = "+380671234567",
            }
        };

        aikomProviderService
            .Setup(x => x.VerifyProviderAndDirectorAccess(TestEdrpou, TestRnkopp, null, null))
            .ReturnsAsync(response);
    }

    private void SetupFailedProviderVerification()
    {
        var response = new AikomProviderResponse
        {
            HasAccess = false,
            ProviderInfo = new AikomProviderInfo
            {
                Id = TestExternalProviderId,
                FullName = "Test Provider",
                ShortName = "Test",
                Address = "Test Address",
                Email = "test@test.com",
                Phone = "+380671234567",
            }
        };

        aikomProviderService
            .Setup(x => x.VerifyProviderAndDirectorAccess(TestEdrpou, TestRnkopp, null, null))
            .ReturnsAsync(response);
    }

    private void SetupErrorProviderVerification()
    {
        var error = new ErrorResponse { Message = "Test error" };
        
        aikomProviderService
            .Setup(x => x.VerifyProviderAndDirectorAccess(TestEdrpou, TestRnkopp, null, null))
            .ReturnsAsync(error);
    }
}