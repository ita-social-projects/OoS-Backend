using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using Google.Apis.Util;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.Common.Models;
using OutOfSchool.Services.Models;
using OutOfSchool.Tests.Common;
using OutOfSchool.Tests.Common.TestDataGenerators;
using OutOfSchool.WebApi.Common;
using OutOfSchool.WebApi.Controllers;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Services;
using OutOfSchool.WebApi.Util;

namespace OutOfSchool.WebApi.Tests.Controllers;

[TestFixture]
public class RegionAdminControllerTests
{
    private RegionAdminController regionAdminController;
    private Mock<IRegionAdminService> regionAdminServiceMock;
    private IMapper mapper;
    private RegionAdmin regionAdmin;
    private List<RegionAdmin> regionAdmins;
    private RegionAdminDto regionAdminDto;
    private List<RegionAdminDto> regionAdminDtos;
    private HttpContext fakeHttpContext; 

    [SetUp]
    public void Setup()
    {
        mapper = TestHelper.CreateMapperInstanceOfProfileType<MappingProfile>();
        regionAdminServiceMock = new Mock<IRegionAdminService>();
        regionAdminController =
            new RegionAdminController(regionAdminServiceMock.Object, new Mock<ILogger<RegionAdminController>>().Object);
        regionAdmin = AdminGenerator.GenerateRegionAdmin();
        regionAdmins = AdminGenerator.GenerateRegionAdmins(10);
        regionAdminDto = AdminGenerator.GenerateRegionAdminDto();
        regionAdminDtos = AdminGenerator.GenerateRegionAdminsDtos(10);
        fakeHttpContext = GetFakeHttpContext();
        regionAdminController.ControllerContext.HttpContext = fakeHttpContext;
    }

    [Test]
    public async Task GetById_WhenIdIsValid_ShouldReturnOkResultObject()
    {
        // Arrange
        regionAdminServiceMock.Setup(x => x.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(regionAdminDto);

        // Act
        var result = await regionAdminController.GetById(It.IsAny<string>()).ConfigureAwait(false) as OkObjectResult;

        // Assert
        regionAdminServiceMock.VerifyAll();
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Value, Is.SameAs(regionAdminDto));
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
    }

    [Test]
    public async Task GetById_WhenNoRegionAdminWithSuchId_ReturnsNotFoundResult()
    {
        // Arrange
        regionAdminServiceMock.Setup(x => x.GetByIdAsync(It.IsAny<string>())).ReturnsAsync(null as RegionAdminDto);

        // Act
        var result = await regionAdminController.GetById(It.IsAny<string>()).ConfigureAwait(false);

        // Assert
        regionAdminServiceMock.VerifyAll();
        Assert.That(result, Is.InstanceOf<NotFoundObjectResult>());
    }

    [Test]
    public async Task GetRegionAdmins_WhenCalled_ReturnsOkResultObject_WithExpectedCollectionDtos()
    {
        // Arrange
        var expected = new SearchResult<RegionAdminDto>
        {
            TotalAmount = 10,
            Entities = regionAdmins.Select(x => mapper.Map<RegionAdminDto>(x)).ToList(),
        };

        regionAdminServiceMock.Setup(x => x.GetByFilter(It.IsAny<RegionAdminFilter>()))
            .ReturnsAsync(expected);

        // Act
        var result = await regionAdminController.GetByFilter(new RegionAdminFilter()).ConfigureAwait(false) as OkObjectResult;

        // Assert
        regionAdminServiceMock.VerifyAll();
        result.AssertResponseOkResultAndValidateValue(expected);
    }

    [Test]
    public async Task GetByFilter_WhenNoRecordsInDB_ReturnsNoContentResult()
    {
        // Arrange
        regionAdminServiceMock.Setup(x => x.GetByFilter(It.IsAny<RegionAdminFilter>()))
            .ReturnsAsync(new SearchResult<RegionAdminDto> { TotalAmount = 0, Entities = new List<RegionAdminDto>() });

        // Act
        var result = await regionAdminController.GetByFilter(new RegionAdminFilter()).ConfigureAwait(false);

        // Assert
        regionAdminServiceMock.VerifyAll();
        Assert.That(result, Is.InstanceOf<NoContentResult>());
    }

    [Test]
    public async Task Create_WithInvalidModel_ReturnsStatusCode422()
    {
        // Arrange
        var regionAdminBaseDto = new RegionAdminBaseDto();
        regionAdminController.ModelState.AddModelError("fakeKey", "Model is invalid");

        // Act
        var result = await regionAdminController.Create(regionAdminBaseDto);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<StatusCodeResult>());
        Assert.That((result as StatusCodeResult).StatusCode, Is.EqualTo(StatusCodes.Status422UnprocessableEntity));
    }

    [Test]
    public async Task Create_WithValidModel_ReturnsSuccessResponseDto()
    {
        // Arrange
        var regionAdminBaseDto = new RegionAdminBaseDto();

        var token = await fakeHttpContext.GetTokenAsync("access_token").ConfigureAwait(false);

        regionAdminServiceMock
            .Setup(x => x.CreateRegionAdminAsync(It.IsAny<string>(), regionAdminBaseDto, token))
            .ReturnsAsync(regionAdminBaseDto);

        regionAdminController.ModelState.Clear();

        // Act
        var result = await regionAdminController.Create(regionAdminBaseDto);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
        Assert.That((result as ObjectResult).Value, Is.InstanceOf<RegionAdminBaseDto>());
    }

    [Test]
    public async Task Create_WithErrorResponse_ReturnsObjectResult()
    {
        // Arrange
        var regionAdminBaseDto = new RegionAdminBaseDto();
        var errorResponse = new ErrorResponse();

        var token = await fakeHttpContext.GetTokenAsync("access_token").ConfigureAwait(false);

        regionAdminServiceMock
            .Setup(x => x.CreateRegionAdminAsync(It.IsAny<string>(), regionAdminBaseDto, token))
            .ReturnsAsync(errorResponse);

        regionAdminController.ModelState.Clear();

        // Act
        var result = await regionAdminController.Create(regionAdminBaseDto);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<ObjectResult>());
    }

    [Test]
    public async Task Update_WithNullModel_ReturnsBadRequestObjectResult()
    {
        // Arrange
        regionAdminController.ModelState.Clear();

        // Act
        var result = await regionAdminController.Update(null);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task Update_WithInvalidModel_ReturnsRequestObjectResult()
    {
        // Arrange
        var updateRegionAdminDto = new BaseUserDto();
        regionAdminController.ModelState.AddModelError("fakeKey", "Model is invalid");

        // Act
        var result = await regionAdminController.Update(updateRegionAdminDto);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task Update_WorkOnlyWithBaseUserDto()
    {
        // Arrange
        var updateRegionAdminDto = new BaseUserDto();

        regionAdminServiceMock.Setup(x => x.UpdateRegionAdminAsync(It.IsAny<string>(), updateRegionAdminDto, It.IsAny<string>())).ReturnsAsync(new RegionAdminDto());

        // Act
        var result = await regionAdminController.Update(updateRegionAdminDto);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<OkResult>());
    }

    [Test]
    public async Task Update_WithValidModel_ReturnsOkResult()
    {
        // Arrange
        var updateRegionAdminDto = new BaseUserDto();

        var token = await fakeHttpContext.GetTokenAsync("access_token").ConfigureAwait(false);

        regionAdminServiceMock
            .Setup(x => x.UpdateRegionAdminAsync(It.IsAny<string>(), updateRegionAdminDto, token))
            .ReturnsAsync(new RegionAdminDto());

        regionAdminController.ModelState.Clear();

        // Act
        var result = await regionAdminController.Update(updateRegionAdminDto);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<OkResult>());
    }

    [Test]
    public async Task Update_WithErrorResponse_ReturnsStatusCodeResult()
    {
        // Arrange
        var updateRegionAdminDto = new BaseUserDto();
        var errorResponse = new ErrorResponse();

        var token = await fakeHttpContext.GetTokenAsync("access_token").ConfigureAwait(false);

        regionAdminServiceMock
            .Setup(x => x.UpdateRegionAdminAsync(It.IsAny<string>(), updateRegionAdminDto, token))
            .ReturnsAsync(errorResponse);

        regionAdminController.ModelState.Clear();

        // Act
        var result = await regionAdminController.Update(updateRegionAdminDto);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<StatusCodeResult>());
    }

    [Test]
    public async Task Delete_WithValidId_ReturnsOkResult()
    {
        // Arrange
        var token = await fakeHttpContext.GetTokenAsync("access_token").ConfigureAwait(false);

        regionAdminServiceMock
            .Setup(x => x.DeleteRegionAdminAsync(It.IsAny<string>(), It.IsAny<string>(), token))
            .ReturnsAsync(It.IsAny<ActionResult>());

        // Act
        var result = await regionAdminController.Delete(It.IsAny<string>());

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<OkResult>());
    }

    [Test]
    public async Task Delete_WithErrorResponse_ReturnsObjectResult()
    {
        // Arrange
        var token = await fakeHttpContext.GetTokenAsync("access_token").ConfigureAwait(false);

        regionAdminServiceMock
            .Setup(x => x.DeleteRegionAdminAsync(It.IsAny<string>(), It.IsAny<string>(), token))
            .ReturnsAsync(new ErrorResponse());

        // Act
        var result = await regionAdminController.Delete(It.IsAny<string>());

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<ObjectResult>());
    }

    [Test]
    public async Task Block_WithValidIdAndNullBlocked_ReturnsBadRequestObjectResult()
    {
        // Arrange
        var token = await fakeHttpContext.GetTokenAsync("access_token").ConfigureAwait(false);

        regionAdminServiceMock
            .Setup(x => x.BlockRegionAdminAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(It.IsAny<ActionResult>());

        // Act
        var result = await regionAdminController.Block(It.IsAny<string>(), null);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public async Task Block_WithValidIdAndBlocked_ReturnsOkResult()
    {
        // Arrange
        var token = await fakeHttpContext.GetTokenAsync("access_token").ConfigureAwait(false);

        regionAdminServiceMock
            .Setup(x => x.BlockRegionAdminAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(It.IsAny<ActionResult>());

        // Act
        var result = await regionAdminController.Block(It.IsAny<string>(), It.IsAny<bool>());

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<OkResult>());
    }

    [Test]
    public async Task Block_WithErrorResponse_ReturnsObjectResult()
    {
        // Arrange
        var token = await fakeHttpContext.GetTokenAsync("access_token").ConfigureAwait(false);

        regionAdminServiceMock
            .Setup(x => x.BlockRegionAdminAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
            .ReturnsAsync(new ErrorResponse());

        // Act
        var result = await regionAdminController.Block(It.IsAny<string>(), It.IsAny<bool>());

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<ObjectResult>());
    }

    [Test]
    public async Task Reinvite_WithValidId_ReturnsOkResult()
    {
        // Arrange
        var token = await fakeHttpContext.GetTokenAsync("access_token").ConfigureAwait(false);

        regionAdminServiceMock
            .Setup(x => x.ReinviteRegionAdminAsync(It.IsAny<string>(), It.IsAny<string>(), token))
            .ReturnsAsync(It.IsAny<ActionResult>());

        // Act
        var result = await regionAdminController.Reinvite(It.IsAny<string>());

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
