using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.IdentityServer.Controllers;
using OutOfSchool.Common;
using OutOfSchool.Common.Models;
using OutOfSchool.IdentityServer.Services.Interfaces;

namespace OutOfSchool.IdentityServer.Tests.Controllers;

[TestFixture]
public class RegionAdminControllerTests
{
    private readonly RegionAdminController regionAdminController;
    private readonly Mock<ILogger<RegionAdminController>> fakeLogger;
    private readonly Mock<IRegionAdminService> fakeRegionAdminService;
    private readonly Mock<HttpContext> fakeHttpContext;

    public RegionAdminControllerTests()
    {
        fakeLogger = new Mock<ILogger<RegionAdminController>>();
        fakeRegionAdminService = new Mock<IRegionAdminService>();
    
        fakeHttpContext = new Mock<HttpContext>();
        
        regionAdminController = new RegionAdminController(
            fakeLogger.Object,
            fakeRegionAdminService.Object
        );
        regionAdminController.ControllerContext.HttpContext = fakeHttpContext.Object;
    }

    [SetUp]
    public void Setup()
    {
        var fakeRegionAdminDto = new RegionAdminBaseDto()
        {
            FirstName = "fakeFirstName",
            LastName = "fakeLastName",
            Email = "fake@email.com",
            PhoneNumber = "11-222-33-44",
        };

        var fakeResponseDto = new ResponseDto()
        {
            IsSuccess = true,
            Result = fakeRegionAdminDto
        };

        fakeRegionAdminService.Setup(s => s
            .CreateRegionAdminAsync(
                It.IsAny<RegionAdminBaseDto>(),
                It.IsAny<IUrlHelper>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync(fakeResponseDto);

        fakeRegionAdminService.Setup(s => s
            .UpdateRegionAdminAsync(
                It.IsAny<RegionAdminBaseDto>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync(fakeResponseDto);

        fakeRegionAdminService.Setup(s => s
            .DeleteRegionAdminAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync(fakeResponseDto);
        
        fakeRegionAdminService.Setup(s => s
            .BlockRegionAdminAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool>()))
            .ReturnsAsync(fakeResponseDto);
        
        var fakeHttpContext = new Mock<HttpContext>();
        fakeHttpContext.Setup(s => s.Request.Headers[It.IsAny<string>()]).Returns("Ok");
        
        regionAdminController.ControllerContext.HttpContext = fakeHttpContext.Object;
    }

    [Test]
    public async Task Create_WithInvalidModel_ReturnsNotSuccessResponseDto()
    {
        // Arrange
        regionAdminController.ModelState.AddModelError("fakeKey", "Model is invalid");

        // Act
        var result = await regionAdminController.Create(new RegionAdminBaseDto());

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsSuccess, Is.False);
    }

    [Test]
    public async Task Create_WithValidModel_ReturnsSuccessResponseDto()
    {
        // Arrange
        regionAdminController.ModelState.Clear();
        
        // Act
        var result = await regionAdminController.Create(new RegionAdminBaseDto());

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(((RegionAdminBaseDto)result.Result).FirstName, Is.EqualTo("fakeFirstName"));
    }

    [Test]
    public async Task Update_WithValidModel_ReturnsSuccessResponseDto()
    {
        // Arrange

        // Act
        var result = await regionAdminController.Update("fakeAdminId", new RegionAdminBaseDto());

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(((RegionAdminBaseDto)result.Result).FirstName, Is.EqualTo("fakeFirstName"));
    }

    [Test]
    public async Task Delete_WithValidModel_ReturnsSuccessResponseDto()
    {
        // Arrange
        
        // Act
        var result = await regionAdminController.Delete("fakeAdminId");

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(((RegionAdminBaseDto)result.Result).FirstName, Is.EqualTo("fakeFirstName"));
    }
    
    [Test]
    public async Task Block_WithValidModel_ReturnsSuccessResponseDto()
    {
        // Arrange
        
        // Act
        var result = await regionAdminController.Block("fakeAdminId", It.IsAny<bool>());

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(((RegionAdminBaseDto)result.Result).FirstName, Is.EqualTo("fakeFirstName"));
    }
}