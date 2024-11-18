using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using OpenIddict.Abstractions;
using OpenIddict.Client;
using OpenIddict.Server;
using OpenIddict.Server.AspNetCore;
using OutOfSchool.AuthCommon.Config;
using OutOfSchool.AuthorizationServer.Controllers;
using OutOfSchool.AuthorizationServer.Services;
using OutOfSchool.Services.Models;
using SignInResult = Microsoft.AspNetCore.Mvc.SignInResult;

namespace OutOfSchool.AuthServer.Tests.Controllers;

[TestFixture]
public class TokenControllerTests
{
    private Mock<IOpenIddictApplicationManager> applicationManager;
    private Mock<IOpenIddictAuthorizationManager> authorizationManager;
    private Mock<OpenIddictClientService> clientService;
    private Mock<IOpenIddictScopeManager> scopeManager;
    private Mock<FakeSignInManager> signInManager;
    private Mock<FakeUserManager> userManager;
    private Mock<IOptions<AuthServerConfig>> identityServerOptions;
    private Mock<IProfileService> profileService;
    private AuthServerConfig authServerConfig;
    private HttpContext httpContext;
    private Mock<IAuthenticationService> authenticationService;
    private TokenController controller;

    [SetUp]
    public void SetUp()
    {
        applicationManager = new Mock<IOpenIddictApplicationManager>();
        authorizationManager = new Mock<IOpenIddictAuthorizationManager>();
        clientService = new Mock<OpenIddictClientService>(new Mock<IServiceProvider>().Object);
        scopeManager = new Mock<IOpenIddictScopeManager>();
        userManager = new Mock<FakeUserManager>();
        signInManager = new Mock<FakeSignInManager>();
        identityServerOptions = new Mock<IOptions<AuthServerConfig>>();
        profileService = new Mock<IProfileService>();
        authenticationService = new Mock<IAuthenticationService>();

        authServerConfig = new AuthServerConfig
        {
            RedirectToStartPageUrl = "/"
        };
        identityServerOptions.Setup(x => x.Value).Returns(authServerConfig);

        controller = new TokenController(
            applicationManager.Object,
            authorizationManager.Object,
            clientService.Object,
            scopeManager.Object,
            signInManager.Object,
            userManager.Object,
            identityServerOptions.Object,
            profileService.Object
        );
        httpContext = new DefaultHttpContext();
        controller.ControllerContext.HttpContext = httpContext;
        controller.HttpContext.RequestServices = new ServiceCollection()
            .AddSingleton(authenticationService.Object)
            .BuildServiceProvider();
    }

    [Test]
    public async Task Authorize_UserNotAuthenticated_ShouldChallenge()
    {
        // Arrange
        var request = new OpenIddictRequest();
        SetOpenIddictServerRequest(httpContext, request);

        // Mock the authentication result to be failed (not authenticated)
        var authResult = AuthenticateResult.Fail("Not authenticated");
        authenticationService.Setup(a => a.AuthenticateAsync(controller.HttpContext, null))
            .ReturnsAsync(authResult);

        // Act
        var result = await controller.Authorize();

        // Assert
        Assert.IsInstanceOf<ChallengeResult>(result);
    }

    [Test]
    public async Task Authorize_UserAuthenticated_AuthorizationFound_ShouldSignIn()
    {
        // Arrange
        var request = new OpenIddictRequest();
        SetOpenIddictServerRequest(httpContext, request);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "123"),
            new(ClaimTypes.Name, "username")
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var principal = new ClaimsPrincipal(identity);
        var authResult = AuthenticateResult.Success(new AuthenticationTicket(principal, "TestScheme"));

        authenticationService.Setup(a => a.AuthenticateAsync(httpContext, null))
            .ReturnsAsync(authResult);

        var user = new User {Id = "123", UserName = "username"};
        userManager.Setup(u => u.GetUserAsync(principal)).ReturnsAsync(user);

        var application = new object();
        applicationManager.Setup(a => a.FindByClientIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(application);

        applicationManager.Setup(a => a.GetConsentTypeAsync(application, It.IsAny<CancellationToken>()))
            .ReturnsAsync(OpenIddictConstants.ConsentTypes.Implicit);

        userManager.Setup(u => u.GetUserIdAsync(user)).ReturnsAsync(user.Id);

        userManager.Setup(u => u.GetEmailAsync(user)).ReturnsAsync("user@example.com");
        userManager.Setup(u => u.GetUserNameAsync(user)).ReturnsAsync(user.UserName);
        userManager.Setup(u => u.GetRolesAsync(user)).ReturnsAsync(new List<string> {"Role1", "Role2"});

        authorizationManager.Setup(a => a.FindAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<ImmutableArray<string>>(),
                It.IsAny<CancellationToken>()))
            .Returns(new List<object>().ToAsyncEnumerable());

        scopeManager.Setup(s => s.ListResourcesAsync(It.IsAny<ImmutableArray<string>>(), It.IsAny<CancellationToken>()))
            .Returns(new List<string> {"resource1", "resource2"}.ToAsyncEnumerable());

        profileService.Setup(p => p.GetProfileDataAsync(It.IsAny<ClaimsIdentity>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await controller.Authorize();

        // Assert
        Assert.IsInstanceOf<SignInResult>(result);
    }

    [Test]
    public async Task Accept_UserNotAllowed_ShouldForbid()
    {
        // Arrange
        var request = new OpenIddictRequest();
        SetOpenIddictServerRequest(httpContext, request);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "123"),
            new(ClaimTypes.Name, "username")
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var principal = new ClaimsPrincipal(identity);
        controller.ControllerContext.HttpContext.User = principal;

        var user = new User {Id = "123", UserName = "username"};
        userManager.Setup(u => u.GetUserAsync(principal)).ReturnsAsync(user);

        var application = new object();
        applicationManager.Setup(a => a.FindByClientIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(application);

        applicationManager.Setup(a =>
                a.HasConsentTypeAsync(application, OpenIddictConstants.ConsentTypes.External,
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        authorizationManager.Setup(a => a.FindAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<ImmutableArray<string>>(),
                It.IsAny<CancellationToken>()))
            .Returns(new List<object>().ToAsyncEnumerable());

        // Act
        var result = await controller.Accept();

        // Assert
        Assert.IsInstanceOf<ForbidResult>(result);
    }

    [Test]
    public async Task DoLogout_ShouldSignOutAndRedirect()
    {
        // Arrange
        signInManager.Setup(s => s.SignOutAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await controller.DoLogout();

        // Assert
        var signOutResult = result as SignOutResult;
        Assert.IsNotNull(signOutResult);
        Assert.AreEqual(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
            signOutResult.AuthenticationSchemes.First());
        Assert.AreEqual(authServerConfig.RedirectToStartPageUrl, signOutResult.Properties.RedirectUri);
    }


    private void SetOpenIddictServerRequest(HttpContext context, OpenIddictRequest request)
    {
        context.Features.Set(new OpenIddictServerAspNetCoreFeature
        {
            Transaction = new OpenIddictServerTransaction
            {
                Request = request,
            }
        });
    }
}