using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.Tests.Common.TestDataGenerators;
using OutOfSchool.WebApi.Controllers;

namespace OutOfSchool.WebApi.Tests.Controllers;

[TestFixture]
public class MinistryAdminControllerTests
{
    private MinistryAdminController ministryAdminController;
    private Mock<IMinistryAdminService> ministryAdminServiceMock;
    private MinistryAdminDto ministryAdminDto;
    private HttpContext fakeHttpContext;

    [SetUp]
    public void Setup()
    {
        ministryAdminServiceMock = new Mock<IMinistryAdminService>();
        ministryAdminController =
            new MinistryAdminController(ministryAdminServiceMock.Object, new Mock<ILogger<MinistryAdminController>>().Object);
        ministryAdminDto = AdminGenerator.GenerateMinistryAdminDto();
        fakeHttpContext = GetFakeHttpContext();
        ministryAdminController.ControllerContext.HttpContext = fakeHttpContext;
    }

    [Test]
    public async Task GetById_WhenIdIsValid_ShouldReturnOkResultObject()
    {
        // Arrange
        ministryAdminServiceMock.Setup(x => x.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(ministryAdminDto);

        // Act
        var result = await ministryAdminController.GetById(It.IsAny<string>()).ConfigureAwait(false) as OkObjectResult;

        // Assert
        ministryAdminServiceMock.VerifyAll();
        Assert.That(result, Is.Not.Null);
        Assert.AreSame(ministryAdminDto, result.Value);
        Assert.IsInstanceOf<OkObjectResult>(result);
    }

    [Test]
    public async Task GetById_WhenNoMinistryAdminWithSuchId_ReturnsNotFoundResult()
    {
        // Arrange
        ministryAdminServiceMock.Setup(x => x.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(null as MinistryAdminDto);

        // Act
        var result = await ministryAdminController.GetById(It.IsAny<string>()).ConfigureAwait(false);

        // Assert
        ministryAdminServiceMock.VerifyAll();
        Assert.IsInstanceOf<NotFoundObjectResult>(result);
    }

    [Test]
    public async Task Update_WithNullModel_ReturnsBadRequestObjectResult()
    {
        // Arrange
        ministryAdminController.ModelState.Clear();

        // Act
        var result = await ministryAdminController.Update(null);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }

    private HttpContext GetFakeHttpContext()
    {
        var authProps = new AuthenticationProperties();

        authProps.StoreTokens(new List<AuthenticationToken>
        {
            new() { Name = "access_token", Value = "accessTokenValue"},
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

        var context = new DefaultHttpContext()
        {
            RequestServices = serviceProviderMock.Object,
        };

        return context;
    }
}
