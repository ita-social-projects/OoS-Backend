using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.Common.Models;
using OutOfSchool.Tests.Common;
using OutOfSchool.Tests.Common.TestDataGenerators;
using OutOfSchool.WebApi.Controllers;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;
using OutOfSchool.WebApi.Util;

namespace OutOfSchool.WebApi.Tests.Controllers;

[TestFixture]
public class MinistryAdminControllerTests
{
    private MinistryAdminController ministryAdminController;
    private Mock<IMinistryAdminService> ministryAdminServiceMock;
    private IMapper mapper;
    private MinistryAdminDto ministryAdminDto;
    private List<MinistryAdminDto> ministryAdminDtos;
    private HttpContext fakeHttpContext;

    [SetUp]
    public void Setup()
    {
        mapper = TestHelper.CreateMapperInstanceOfProfileType<MappingProfile>();
        ministryAdminServiceMock = new Mock<IMinistryAdminService>();
        ministryAdminController =
            new MinistryAdminController(ministryAdminServiceMock.Object, new Mock<ILogger<MinistryAdminController>>().Object);
        ministryAdminDto = AdminGenerator.GenerateMinistryAdminDto();
        ministryAdminDtos = AdminGenerator.GenerateMinistryAdminsDtos(10);
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
    public async Task GetMinistryAdmins_WhenCalled_ReturnsOkResultObject_WithExpectedCollectionDtos()
    {
        // Arrange
        var expected = new SearchResult<MinistryAdminDto>
        {
            TotalAmount = 10,
            Entities = ministryAdminDtos.Select(x => mapper.Map<MinistryAdminDto>(x)).ToList(),
        };

        ministryAdminServiceMock.Setup(x => x.GetByFilter(It.IsAny<MinistryAdminFilter>()))
            .ReturnsAsync(expected);

        // Act
        var result = await ministryAdminController.GetByFilter(new MinistryAdminFilter()).ConfigureAwait(false) as OkObjectResult;

        // Assert
        ministryAdminServiceMock.VerifyAll();
        result.AssertResponseOkResultAndValidateValue(expected);
    }

    [Test]
    public async Task GetByFilter_WhenNoRecordsInDB_ReturnsNoContentResult()
    {
        // Arrange
        ministryAdminServiceMock.Setup(x => x.GetByFilter(It.IsAny<MinistryAdminFilter>()))
            .ReturnsAsync(new SearchResult<MinistryAdminDto> { TotalAmount = 0, Entities = new List<MinistryAdminDto>() });

        // Act
        var result = await ministryAdminController.GetByFilter(new MinistryAdminFilter()).ConfigureAwait(false);

        // Assert
        ministryAdminServiceMock.VerifyAll();
        Assert.IsInstanceOf<NoContentResult>(result);
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

    [Test]
    public async Task Update_WithInvalidModel_ReturnsRequestObjectResult()
    {
        // Arrange
        var updateInstitutionAdminDto = new MinistryAdminDto();
        ministryAdminController.ModelState.AddModelError("fakeKey", "Model is invalid");

        // Act
        var result = await ministryAdminController.Update(updateInstitutionAdminDto);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task Update_WithValidModel_ReturnsOkResult()
    {
        // Arrange
        var updateMinistryAdminDto = new MinistryAdminDto();

        var token = await fakeHttpContext.GetTokenAsync("access_token").ConfigureAwait(false);

        ministryAdminServiceMock
            .Setup(x => x.UpdateMinistryAdminAsync(It.IsAny<string>(), updateMinistryAdminDto, token))
        .ReturnsAsync(updateMinistryAdminDto);

        ministryAdminController.ModelState.Clear();

        // Act
        var result = await ministryAdminController.Update(updateMinistryAdminDto);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<OkResult>());
    }

    [Test]
    public async Task Update_WithErrorResponse_ReturnsStatusCodeResult()
    {
        // Arrange
        var updateMinistryAdminDto = new MinistryAdminDto();
        var errorResponse = new ErrorResponse();

        var token = await fakeHttpContext.GetTokenAsync("access_token").ConfigureAwait(false);

        ministryAdminServiceMock
            .Setup(x => x.UpdateMinistryAdminAsync(It.IsAny<string>(), updateMinistryAdminDto, token))
        .ReturnsAsync(errorResponse);

        ministryAdminController.ModelState.Clear();

        // Act
        var result = await ministryAdminController.Update(updateMinistryAdminDto);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<StatusCodeResult>());
    }

    [Test]
    public async Task Update_WhenServiceThrowDbUpdateConcurrencyException_ReturnsBadRequest()
    {
        // Arrange
        var updateMinistryAdminDto = new MinistryAdminDto();

        ministryAdminServiceMock
            .Setup(x => x.UpdateMinistryAdminAsync(It.IsAny<string>(), updateMinistryAdminDto, It.IsAny<string>()))
            .Throws<DbUpdateConcurrencyException>();

        // Act
        var result = await ministryAdminController.Update(updateMinistryAdminDto);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
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

        var context = new DefaultHttpContext()
        {
            RequestServices = serviceProviderMock.Object,
        };

        return context;
    }
}
