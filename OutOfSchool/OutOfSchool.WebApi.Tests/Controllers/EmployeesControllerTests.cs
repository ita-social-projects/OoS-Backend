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
public class EmployeesControllerTests
{
    private string userId;

    private Mock<IEmployeeService> providerAdminService;
    private Mock<IUserService> userService;
    private Mock<IProviderService> providerService;
    private Mock<ILogger<EmployeesController>> logger;

    private EmployeesController employeesController;

    [SetUp]
    public void Setup()
    {
        userId = Guid.NewGuid().ToString();

        providerAdminService = new Mock<IEmployeeService>();
        userService = new Mock<IUserService>();
        providerService = new Mock<IProviderService>();
        logger = new Mock<ILogger<EmployeesController>>();

        employeesController = new EmployeesController(providerAdminService.Object, userService.Object, providerService.Object, logger.Object);

        employeesController.ControllerContext.HttpContext = GetFakeHttpContext();
    }

    [Test]
    public async Task CreateAdminProvider_WhenModelIsValid_ReturnsCreated()
    {
        // Arrange
        var providerAdminDto = new CreateEmployeeDto();

        providerAdminService.Setup(x => x.CreateEmployeeAsync(It.IsAny<string>(), It.IsAny<CreateEmployeeDto>(), It.IsAny<string>()))
            .ReturnsAsync(providerAdminDto);

        // Act
        var result = await employeesController.Create(providerAdminDto);

        // Assert
        Assert.IsInstanceOf<CreatedResult>(result);

        Assert.AreEqual(providerAdminDto, (result as CreatedResult).Value);
    }

    [Test]
    public async Task CreateAdminProvider_ProviderAdminIsNull_ReturnsBadRequest()
    {
        // Arrange
        CreateEmployeeDto employee = null;

        // Act
        var result = await employeesController.Create(employee);

        // Assert
        Assert.IsInstanceOf<BadRequestObjectResult>(result);
    }

    [Test]
    public async Task GetFilteredProviderAdmins_WhenSearchResultIsNotNullOrEmpty_ReturnsOkObjectResult()
    {
        // Arrange
        var searchResult = new SearchResult<EmployeeDto>()
        {
            TotalAmount = 1,
            Entities = new List<EmployeeDto>()
            {
                new EmployeeDto(),
            },
        };

        var filter = new EmployeeSearchFilter();

        providerAdminService.Setup(x => x.GetFilteredRelatedProviderAdmins(It.IsAny<string>(), filter)).ReturnsAsync(searchResult);

        // Act
        var result = await employeesController.GetFilteredProviderAdminsAsync(filter);

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
        var searchResult = new SearchResult<EmployeeDto>()
        { };

        var filter = new EmployeeSearchFilter();

        providerAdminService.Setup(x => x.GetFilteredRelatedProviderAdmins(userId, filter)).ReturnsAsync(searchResult);

        // Act
        var result = await employeesController.GetFilteredProviderAdminsAsync(filter);

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

        providerAdminService
            .Setup(x => x.GetWorkshopsThatEmployeeCanManage(It.IsAny<string>()))
            .ReturnsAsync(searchResult);

        // Act
        var result = await employeesController.ManagedWorkshops();

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

        providerAdminService.Setup(x => x.GetWorkshopsThatEmployeeCanManage(userId)).ReturnsAsync(searchResult);

        // Act
        var result = await employeesController.ManagedWorkshops();

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
                            new Claim(IdentityResourceClaimsTypes.Role, Role.Employee.ToString()),
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
