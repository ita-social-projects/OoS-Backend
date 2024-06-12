using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Internal;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Workshops;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.BusinessLogic.Services.ProviderServices;
using OutOfSchool.Common;
using OutOfSchool.Common.Models;
using OutOfSchool.Services.Enums;
using OutOfSchool.WebApi.Controllers;

namespace OutOfSchool.WebApi.Tests.Controllers;

[TestFixture]
public class ProviderAdminControllerTests
{
    private string userId;

    private Mock<IProviderAdminService> providerAdminService;
    private Mock<IUserService> userService;
    private Mock<IProviderService> providerService;
    private Mock<ILogger<ProviderAdminController>> logger;

    private ProviderAdminController providerAdminController;

    [SetUp]
    public void Setup()
    {
        userId = Guid.NewGuid().ToString();

        providerAdminService = new Mock<IProviderAdminService>();
        userService = new Mock<IUserService>();
        providerService = new Mock<IProviderService>();
        logger = new Mock<ILogger<ProviderAdminController>>();

        providerAdminController = new ProviderAdminController(providerAdminService.Object, userService.Object, providerService.Object, logger.Object);

        providerAdminController.ControllerContext.HttpContext = GetFakeHttpContext();
    }

    [Test]
    public async Task CreateAdminProvider_WhenModelIsValid_ReturnsCreated()
    {
        // Arrange
        var providerAdminDto = new CreateProviderAdminDto();

        providerAdminService.Setup(x => x.CreateProviderAdminAsync(It.IsAny<string>(), It.IsAny<CreateProviderAdminDto>(), It.IsAny<string>()))
            .ReturnsAsync(providerAdminDto);

        // Act
        var result = await providerAdminController.Create(providerAdminDto);

        // Assert
        Assert.IsInstanceOf<CreatedResult>(result);

        Assert.AreEqual(providerAdminDto, (result as CreatedResult).Value);
    }

    [Test]
    public async Task CreateAdminProvider_ProviderAdminIsNull_ReturnsBadRequest()
    {
        // Arrange
        CreateProviderAdminDto providerAdmin = null;

        // Act
        var result = await providerAdminController.Create(providerAdmin);

        // Assert
        Assert.IsInstanceOf<BadRequestObjectResult>(result);
    }

    [Test]
    public async Task GetFilteredProviderAdmins_WhenSearchResultIsNotNullOrEmpty_ReturnsOkObjectResult()
    {
        // Arrange
        var searchResult = new SearchResult<ProviderAdminDto>()
        {
            TotalAmount = 1,
            Entities = new List<ProviderAdminDto>()
            {
                new ProviderAdminDto(),
            },
        };

        var filter = new ProviderAdminSearchFilter();

        providerAdminService.Setup(x => x.GetFilteredRelatedProviderAdmins(It.IsAny<string>(), filter)).ReturnsAsync(searchResult);

        // Act
        var result = await providerAdminController.GetFilteredProviderAdminsAsync(filter);

        // Assert
        result.Should().NotBeNull();
        result.Should()
              .BeOfType<OkObjectResult>()
              .Which.StatusCode
              .Should()
              .Be(StatusCodes.Status200OK);
    }

    [Test]
    public async Task GetFilteredProviderAdmins_WhenSearchResultIsNullOrEmpty_ReturnsNoContentObjectResult()
    {
        // Arrange
        var searchResult = new SearchResult<ProviderAdminDto>()
        { };

        var filter = new ProviderAdminSearchFilter();

        providerAdminService.Setup(x => x.GetFilteredRelatedProviderAdmins(userId, filter)).ReturnsAsync(searchResult);

        // Act
        var result = await providerAdminController.GetFilteredProviderAdminsAsync(filter);

        // Assert
        result.Should().NotBeNull();
        result.Should()
              .BeOfType<NoContentResult>()
              .Which.StatusCode
              .Should()
              .Be(StatusCodes.Status204NoContent);
    }

    [Test]
    public async Task ManagedWorkshops_WhenSearchResultIsNotNullOrEmpty_ReturnsOkObjectResult()
    {
        // Arrange
        var searchResult = new SearchResult<WorkshopProviderViewCard>()
        {
            TotalAmount = 1,
            Entities = new List<WorkshopProviderViewCard>()
            {
                new WorkshopProviderViewCard(),
            },
        };

        providerAdminService.Setup(x => x.GetWorkshopsThatProviderAdminCanManage(It.IsAny<string>(), true)).ReturnsAsync(searchResult);

        // Act
        var result = await providerAdminController.ManagedWorkshops();

        // Assert
        result.Should().NotBeNull();
        result.Should()
              .BeOfType<OkObjectResult>()
              .Which.StatusCode
              .Should()
              .Be(StatusCodes.Status200OK);
    }

    [Test]
    public async Task ManagedWorkshops_WhenSearchResultIsNullOrEmpty_ReturnsNoContentObjectResult()
    {
        // Arrange
        var searchResult = new SearchResult<WorkshopProviderViewCard>()
        { };

        providerAdminService.Setup(x => x.GetWorkshopsThatProviderAdminCanManage(userId, true)).ReturnsAsync(searchResult);

        // Act
        var result = await providerAdminController.ManagedWorkshops();

        // Assert
        result.Should().NotBeNull();
        result.Should()
              .BeOfType<NoContentResult>()
              .Which.StatusCode
              .Should()
              .Be(StatusCodes.Status204NoContent);
    }

    private HttpContext GetFakeHttpContext()
    {
        var authProps = new AuthenticationProperties();

        authProps.StoreTokens(new List<AuthenticationToken>
        {
            new AuthenticationToken{ Name = "access_token", Value = "accessTokenValue"},
        });

        var authResult = AuthenticateResult
            .Success(new AuthenticationTicket(new ClaimsPrincipal(), authProps, It.IsAny<string>()));

        var authenticationServiceMock = new Mock<IAuthenticationService>();

        authenticationServiceMock
            .Setup(x => x.AuthenticateAsync(It.IsAny<HttpContext>(), It.IsAny<string>()))
            .ReturnsAsync(authResult);

        var serviceProviderMock = new Mock<IServiceProvider>();

        serviceProviderMock
            .Setup(s => s.GetService(typeof(IAuthenticationService)))
        .Returns(authenticationServiceMock.Object);

        var user = new ClaimsPrincipal(
                    new ClaimsIdentity(
                        new Claim[]
                        {
                            new Claim(IdentityResourceClaimsTypes.Sub, userId),
                            new Claim("subrole", Subrole.ProviderDeputy.ToString()),
                            new Claim(IdentityResourceClaimsTypes.Role, Role.Provider.ToString()),
                        },
                        IdentityResourceClaimsTypes.Sub));

        var context = new DefaultHttpContext()
        {
            RequestServices = serviceProviderMock.Object,
            User = user,
        };

        return context;
    }
}
