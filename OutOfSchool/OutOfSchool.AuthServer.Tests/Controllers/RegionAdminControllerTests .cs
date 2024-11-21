using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.Common;
using OutOfSchool.Common.Models;
using OutOfSchool.AuthCommon.Controllers;
using OutOfSchool.AuthCommon.Services.Interfaces;
using OutOfSchool.Services.Enums;
using Microsoft.EntityFrameworkCore;
using OutOfSchool.Services.Models;
using OutOfSchool.Services;
using System;
using OutOfSchool.AuthCommon.Services;
using AutoMapper;
using OutOfSchool.Services.Repository;
using Microsoft.AspNetCore.Identity;
using OutOfSchool.RazorTemplatesData.Services;
using Microsoft.Extensions.Localization;
using OutOfSchool.AuthCommon;
using System.Linq;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using OutOfSchool.EmailSender.Services;
using Microsoft.Extensions.Options;
using OutOfSchool.AuthCommon.Config;
using OutOfSchool.Tests.Common.DbContextTests;

namespace OutOfSchool.AuthServer.Tests.Controllers;

[TestFixture]
public class RegionAdminControllerTests
{
    private readonly RegionAdminController regionAdminController;
    private readonly Mock<ILogger<RegionAdminController>> fakeLogger;
    private readonly Mock<ICommonMinistryAdminService<RegionAdminBaseDto>> fakeRegionAdminService;
    private readonly Mock<HttpContext> fakeHttpContext;
    private RegionAdminController regionAdminControllerWithRealService;
    private ICommonMinistryAdminService<RegionAdminBaseDto> regionAdminService;
    private RegionAdminRepository regionAdminRepository;
    private Mock<IOptions<HostsConfig>> fakeHostsConfig;

    public RegionAdminControllerTests()
    {
        fakeLogger = new Mock<ILogger<RegionAdminController>>();
        fakeRegionAdminService = new Mock<ICommonMinistryAdminService<RegionAdminBaseDto>>();
    
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
            .CreateMinistryAdminAsync(
                It.IsAny<RegionAdminBaseDto>(),
                It.IsAny<Role>(),
                It.IsAny<IUrlHelper>(),
                It.IsAny<string>()))
            .ReturnsAsync(fakeResponseDto);

        fakeRegionAdminService.Setup(s => s
            .UpdateMinistryAdminAsync(
                It.IsAny<RegionAdminBaseDto>(),
                It.IsAny<string>()))
            .ReturnsAsync(fakeResponseDto);

        fakeRegionAdminService.Setup(s => s
            .DeleteMinistryAdminAsync(
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync(fakeResponseDto);
        
        fakeRegionAdminService.Setup(s => s
            .BlockMinistryAdminAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool>()))
            .ReturnsAsync(fakeResponseDto);

        fakeRegionAdminService.Setup(s => s
            .ReinviteMinistryAdminAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<IUrlHelper>()))
            .ReturnsAsync(fakeResponseDto);

        var fakeHttpContext = new Mock<HttpContext>();
        fakeHttpContext.Setup(s => s.Request.Headers[It.IsAny<string>()]).Returns("Ok");
        
        regionAdminController.ControllerContext.HttpContext = fakeHttpContext.Object;

        var context = GetContext();

        regionAdminRepository = new RegionAdminRepository(context);
        var userManager = new UserManager<User>(
            new UserStore<User>(context), null, null, null, null, null, null, null, null);

        fakeHostsConfig = new Mock<IOptions<HostsConfig>>();
        var config = new HostsConfig()
        {
            BackendUrl = "http://localhost:5443"
        };
        fakeHostsConfig.Setup(x => x.Value).Returns(config);

        regionAdminService = new CommonMinistryAdminService<long, RegionAdmin, RegionAdminBaseDto, RegionAdminRepository>(
            new Mock<IMapper>().Object,
            regionAdminRepository,
            new Mock<ILogger<CommonMinistryAdminService<long, RegionAdmin, RegionAdminBaseDto, RegionAdminRepository>>>().Object,
            new Mock<IEmailSenderService>().Object,
            userManager,
            context,
            new Mock<IRazorViewToStringRenderer>().Object,
            new Mock<IStringLocalizer<SharedResource>>().Object,
            fakeHostsConfig.Object);

        regionAdminControllerWithRealService = new RegionAdminController(fakeLogger.Object, regionAdminService);
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
    public async Task Update_WhenModelWithChangedCATOTTGIdAndInstitutionId_ShouldNotChangeCATOTTGIdAndInstitutionId()
    {
        // Arrange        
        var oldInstitutionId = Guid.NewGuid();
        long oldCAOTTGId = long.MinValue;
        var userId = string.Empty;
        RegionAdmin regionAdmin = new RegionAdmin { UserId = userId, InstitutionId = oldInstitutionId, CATOTTGId = oldCAOTTGId };
        await SeedRegionAdmin(regionAdmin);
        var regionAdminToUpdate = new RegionAdminBaseDto 
        { 
            UserId = userId,
            FirstName = string.Empty,
            LastName = string.Empty,
            InstitutionId = Guid.NewGuid(),
            CATOTTGId = long.MaxValue 
        };

        // Act
        await regionAdminControllerWithRealService.Update(userId, regionAdminToUpdate);

        // Assert
        var updatedRegionAdmin = regionAdminRepository.GetAll().Result.First();
        Assert.AreEqual(oldInstitutionId, updatedRegionAdmin.InstitutionId);
        Assert.AreEqual(oldCAOTTGId, updatedRegionAdmin.CATOTTGId);
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
    public async Task Delete_WithNullId_ReturnsException()
    {
        Assert.That(() => regionAdminController.Delete(null), Throws.ArgumentNullException);
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

    [Test]
    public async Task OnActionExecuting_WithNullContext_ReturnsException()
    {
        Assert.That(() => regionAdminController.OnActionExecuting(null), Throws.ArgumentNullException);
    }

    [Test]
    public async Task Reinvite_WithValidModel_ReturnsSuccessResponseDto()
    {
        // Arrange

        // Act
        var result = await regionAdminController.Reinvite("fakeAdminId");

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.AreEqual(true, result.IsSuccess);
        Assert.AreEqual("fakeFirstName", ((RegionAdminBaseDto)result.Result).FirstName);
    }
    private async Task SeedRegionAdmin(RegionAdmin regionAdmin)
    {
        OutOfSchoolDbContext context = GetContext();

        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
        context.Add(regionAdmin);
        context.Add(new User { Id = regionAdmin.UserId, FirstName = string.Empty, LastName = string.Empty });
        await context.SaveChangesAsync();
    }

    private static TestOutOfSchoolDbContext GetContext()
    {
        return new TestOutOfSchoolDbContext(
            new DbContextOptionsBuilder<OutOfSchoolDbContext>()
            .UseInMemoryDatabase(databaseName: "OutOfSchoolTestDB")
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options);
    }
}