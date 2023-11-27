using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
using OutOfSchool.Services.Models;
using OutOfSchool.Tests.Common;
using OutOfSchool.Tests.Common.TestDataGenerators;
using OutOfSchool.WebApi.Controllers.V1;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;
using OutOfSchool.WebApi.Util;

namespace OutOfSchool.WebApi.Tests.Controllers;

[TestFixture]
public class AreaAdminControllerTests
{
    private AreaAdminController areaAdminController;
    private Mock<IAreaAdminService> areaAdminServiceMock;
    private IMapper mapper;
    private AreaAdmin areaAdmin;
    private List<AreaAdmin> areaAdmins;
    private AreaAdminDto areaAdminDto;
    private List<AreaAdminDto> areaAdminDtos;
    private HttpContext fakeHttpContext;

    [SetUp]
    public void Setup()
    {
        mapper = TestHelper.CreateMapperInstanceOfProfileType<MappingProfile>();
        areaAdminServiceMock = new Mock<IAreaAdminService>();
        areaAdminController =
            new AreaAdminController(areaAdminServiceMock.Object, new Mock<ILogger<AreaAdminController>>().Object);
        areaAdmin = AdminGenerator.GenerateAreaAdmin();
        areaAdmins = AdminGenerator.GenerateAreaAdmins(10);
        areaAdminDto = AdminGenerator.GenerateAreaAdminDto();
        areaAdminDtos = AdminGenerator.GenerateAreaAdminsDtos(10);
        fakeHttpContext = GetFakeHttpContext();
        areaAdminController.ControllerContext.HttpContext = fakeHttpContext;
    }

    [Test]
    public async Task GetById_WhenIdIsValid_ShouldReturnOkResultObject()
    {
        // Arrange
        areaAdminServiceMock.Setup(x => x.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(areaAdminDto);

        // Act
        var result = await areaAdminController.GetById(It.IsAny<string>()).ConfigureAwait(false) as OkObjectResult;

        // Assert
        areaAdminServiceMock.VerifyAll();
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Value, Is.SameAs(areaAdminDto));
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
    }

    [Test]
    public async Task GetById_WhenNoAreaAdminWithSuchId_ReturnsNotFoundResult()
    {
        // Arrange
        areaAdminServiceMock.Setup(x => x.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(null as AreaAdminDto);

        // Act
        var result = await areaAdminController.GetById(It.IsAny<string>()).ConfigureAwait(false);

        // Assert
        areaAdminServiceMock.VerifyAll();
        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task GetAreaAdmins_WhenCalled_ReturnsOkResultObject_WithExpectedCollectionDtos()
    {
        // Arrange
        var expected = new SearchResult<AreaAdminDto>
        {
            TotalAmount = 10,
            Entities = areaAdmins.Select(x => mapper.Map<AreaAdminDto>(x)).ToList(),
        };

        areaAdminServiceMock.Setup(x => x.GetByFilter(It.IsAny<AreaAdminFilter>()))
            .ReturnsAsync(expected);

        // Act
        var result = await areaAdminController.GetByFilter(new AreaAdminFilter()).ConfigureAwait(false) as OkObjectResult;

        // Assert
        areaAdminServiceMock.VerifyAll();
        result.AssertResponseOkResultAndValidateValue(expected);
    }

    [Test]
    public async Task GetByFilter_WhenNoRecordsInDB_ReturnsNoContentResult()
    {
        // Arrange
        areaAdminServiceMock.Setup(x => x.GetByFilter(It.IsAny<AreaAdminFilter>()))
            .ReturnsAsync(new SearchResult<AreaAdminDto> { TotalAmount = 0, Entities = new List<AreaAdminDto>() });

        // Act
        var result = await areaAdminController.GetByFilter(new AreaAdminFilter()).ConfigureAwait(false);

        // Assert
        areaAdminServiceMock.VerifyAll();
        Assert.That(result, Is.InstanceOf<NoContentResult>());
    }

    [Test]
    public async Task Create_WithInvalidModel_ReturnsStatusCode422()
    {
        // Arrange
        var areaAdminBaseDto = new AreaAdminBaseDto();
        areaAdminController.ModelState.AddModelError("fakeKey", "Model is invalid");

        // Act
        var result = await areaAdminController.Create(areaAdminBaseDto);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<StatusCodeResult>());
        Assert.That((result as StatusCodeResult).StatusCode, Is.EqualTo(StatusCodes.Status422UnprocessableEntity));
    }

    [Test]
    public async Task Create_WithValidModel_ReturnsSuccessResponseDto()
    {
        // Arrange
        var areaAdminBaseDto = new AreaAdminBaseDto();

        var token = await fakeHttpContext.GetTokenAsync("access_token").ConfigureAwait(false);

        areaAdminServiceMock
            .Setup(x => x.CreateAreaAdminAsync(It.IsAny<string>(), areaAdminBaseDto, token))
            .ReturnsAsync(areaAdminBaseDto);

        areaAdminController.ModelState.Clear();

        // Act
        var result = await areaAdminController.Create(areaAdminBaseDto);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        Assert.That((result as ObjectResult).Value, Is.InstanceOf<AreaAdminBaseDto>());
    }

    [Test]
    public async Task Create_WithErrorResponse_ReturnsObjectResult()
    {
        // Arrange
        var areaAdminBaseDto = new AreaAdminBaseDto();
        var errorResponse = new ErrorResponse();

        var token = await fakeHttpContext.GetTokenAsync("access_token").ConfigureAwait(false);

        areaAdminServiceMock
            .Setup(x => x.CreateAreaAdminAsync(It.IsAny<string>(), areaAdminBaseDto, token))
            .ReturnsAsync(errorResponse);

        areaAdminController.ModelState.Clear();

        // Act
        var result = await areaAdminController.Create(areaAdminBaseDto);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<ObjectResult>());
    }

    [Test]
    public async Task Update_WithNullModel_ReturnsBadRequestObjectResult()
    {
        // Arrange
        areaAdminController.ModelState.Clear();

        // Act
        var result = await areaAdminController.Update(null);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task Update_WithInvalidModel_ReturnsRequestObjectResult()
    {
        // Arrange
        var updateAreaAdminDto = new AreaAdminDto();
        areaAdminController.ModelState.AddModelError("fakeKey", "Model is invalid");

        // Act
        var result = await areaAdminController.Update(updateAreaAdminDto);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task Update_WithValidModel_ReturnsOkResult()
    {
        // Arrange
        var updateAreaAdminDto = new AreaAdminDto();

        var token = await fakeHttpContext.GetTokenAsync("access_token").ConfigureAwait(false);

        areaAdminServiceMock
            .Setup(x => x.UpdateAreaAdminAsync(It.IsAny<string>(), updateAreaAdminDto, token))
            .ReturnsAsync(updateAreaAdminDto);

        areaAdminController.ModelState.Clear();

        // Act
        var result = await areaAdminController.Update(updateAreaAdminDto);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<OkResult>());
    }

    [Test]
    public async Task Update_WithErrorResponse_ReturnsStatusCodeResult()
    {
        // Arrange
        var updateAreaAdminDto = new AreaAdminDto();
        var errorResponse = new ErrorResponse();

        var token = await fakeHttpContext.GetTokenAsync("access_token").ConfigureAwait(false);

        areaAdminServiceMock
            .Setup(x => x.UpdateAreaAdminAsync(It.IsAny<string>(), updateAreaAdminDto, token))
            .ReturnsAsync(errorResponse);

        areaAdminController.ModelState.Clear();

        // Act
        var result = await areaAdminController.Update(updateAreaAdminDto);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<StatusCodeResult>());
    }

    [Test]
    public async Task Update_WhenServiceThrowDbUpdateConcurrencyException_ReturnsBadRequest()
    {
        // Arrange
        var updateAreaAdminDto = new AreaAdminDto();

        areaAdminServiceMock
            .Setup(x => x.UpdateAreaAdminAsync(It.IsAny<string>(), updateAreaAdminDto, It.IsAny<string>()))
            .Throws<DbUpdateConcurrencyException>();

        // Act
        var result = await areaAdminController.Update(updateAreaAdminDto);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task Update_WhenUserIsNotAdmin_ReturnsForbidenResponse()
    {
        // Arrange
        var updateAreaAdminDto = new AreaAdminDto { Id = string.Empty };

        areaAdminServiceMock
            .Setup(x => x.UpdateAreaAdminAsync(It.IsAny<string>(), updateAreaAdminDto, It.IsAny<string>()))
            .Throws<DbUpdateConcurrencyException>();

        // Act
        var result = await areaAdminController.Update(updateAreaAdminDto);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<ObjectResult>());
        Assert.AreEqual((int)HttpStatusCode.Forbidden, (result as ObjectResult).StatusCode);
    }

    [Test]
    public async Task Delete_WithValidId_ReturnsOkResult()
    {
        // Arrange
        var token = await fakeHttpContext.GetTokenAsync("access_token").ConfigureAwait(false);

        areaAdminServiceMock
            .Setup(x => x.DeleteAreaAdminAsync(It.IsAny<string>(), It.IsAny<string>(), token))
            .ReturnsAsync(It.IsAny<ActionResult>());

        // Act
        var result = await areaAdminController.Delete(It.IsAny<string>());

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<OkResult>());
    }

    [Test]
    public async Task Delete_WithErrorResponse_ReturnsObjectResult()
    {
        // Arrange
        var token = await fakeHttpContext.GetTokenAsync("access_token").ConfigureAwait(false);

        areaAdminServiceMock
            .Setup(x => x.DeleteAreaAdminAsync(It.IsAny<string>(), It.IsAny<string>(), token))
            .ReturnsAsync(new ErrorResponse());

        // Act
        var result = await areaAdminController.Delete(It.IsAny<string>());

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<ObjectResult>());
    }

    [Test]
    public async Task Block_WithValidIdAndNullBlocked_ReturnsBadRequestObjectResult()
    {
        // Arrange
        var token = await fakeHttpContext.GetTokenAsync("access_token").ConfigureAwait(false);

        areaAdminServiceMock
            .Setup(x => x.BlockAreaAdminAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(It.IsAny<ActionResult>());

        // Act
        var result = await areaAdminController.Block(It.IsAny<string>(), null);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task Block_WithValidIdAndBlocked_ReturnsOkResult()
    {
        // Arrange
        var token = await fakeHttpContext.GetTokenAsync("access_token").ConfigureAwait(false);

        areaAdminServiceMock
            .Setup(x => x.BlockAreaAdminAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(It.IsAny<ActionResult>());

        // Act
        var result = await areaAdminController.Block(It.IsAny<string>(), It.IsAny<bool>());

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<OkResult>());
    }

    [Test]
    public async Task Block_WithErrorResponse_ReturnsObjectResult()
    {
        // Arrange
        var token = await fakeHttpContext.GetTokenAsync("access_token").ConfigureAwait(false);

        areaAdminServiceMock
            .Setup(x => x.BlockAreaAdminAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(new ErrorResponse());

        // Act
        var result = await areaAdminController.Block(It.IsAny<string>(), It.IsAny<bool>());

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<ObjectResult>());
    }

    [Test]
    public async Task Reinvite_WithValidId_ReturnsOkResult()
    {
        // Arrange
        var token = await fakeHttpContext.GetTokenAsync("access_token").ConfigureAwait(false);

        areaAdminServiceMock
            .Setup(x => x.ReinviteAreaAdminAsync(It.IsAny<string>(), It.IsAny<string>(), token))
            .ReturnsAsync(It.IsAny<ActionResult>());

        // Act
        var result = await areaAdminController.Reinvite(It.IsAny<string>());

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<OkResult>());
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
