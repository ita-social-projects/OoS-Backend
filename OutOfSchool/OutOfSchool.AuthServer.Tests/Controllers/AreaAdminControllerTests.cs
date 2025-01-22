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
public class AreaAdminControllerTests
{
    private readonly AreaAdminController areaAdminController;
    private readonly Mock<ILogger<AreaAdminController>> fakeLogger;
    private readonly Mock<ICommonMinistryAdminService<AreaAdminBaseDto>> fakeAreaAdminService;
    private readonly Mock<HttpContext> fakeHttpContext;
    private AreaAdminController areaAdminControllerWithRealService;
    private ICommonMinistryAdminService<AreaAdminBaseDto> areaAdminService;
    private AreaAdminRepository areaAdminRepository;
    private Mock<IOptions<HostsConfig>> fakeHostsConfig;

    public AreaAdminControllerTests()
    {
        fakeLogger = new Mock<ILogger<AreaAdminController>>();
        fakeAreaAdminService = new Mock<ICommonMinistryAdminService<AreaAdminBaseDto>>();
    
        fakeHttpContext = new Mock<HttpContext>();
        
        areaAdminController = new AreaAdminController(
            fakeLogger.Object,
            fakeAreaAdminService.Object
        );
        areaAdminController.ControllerContext.HttpContext = fakeHttpContext.Object;
    }

    [SetUp]
    public void Setup()
    {
        var fakeAreaAdminDto = new AreaAdminBaseDto()
        {
            FirstName = "fakeFirstName",
            LastName = "fakeLastName",
            Email = "fake@email.com",
            PhoneNumber = "11-222-33-44",
        };

        var fakeResponseDto = new ResponseDto()
        {
            IsSuccess = true,
            Result = fakeAreaAdminDto
        };

        fakeAreaAdminService.Setup(s => s
            .CreateMinistryAdminAsync(
                It.IsAny<AreaAdminBaseDto>(),
                It.IsAny<Role>(),
                It.IsAny<IUrlHelper>(),
                It.IsAny<string>()))
            .ReturnsAsync(fakeResponseDto);

        fakeAreaAdminService.Setup(s => s
            .UpdateMinistryAdminAsync(
                It.IsAny<AreaAdminBaseDto>(),
                It.IsAny<string>()))
            .ReturnsAsync(fakeResponseDto);

        fakeAreaAdminService.Setup(s => s
            .DeleteMinistryAdminAsync(
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync(fakeResponseDto);
        
        fakeAreaAdminService.Setup(s => s
            .BlockMinistryAdminAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<bool>()))
            .ReturnsAsync(fakeResponseDto);

        fakeAreaAdminService.Setup(s => s
            .ReinviteMinistryAdminAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<IUrlHelper>()))
            .ReturnsAsync(fakeResponseDto);

        var fakeHttpContext = new Mock<HttpContext>();
        fakeHttpContext.Setup(s => s.Request.Headers[It.IsAny<string>()]).Returns("Ok");
        
        areaAdminController.ControllerContext.HttpContext = fakeHttpContext.Object;

        var context = GetContext();

        areaAdminRepository = new AreaAdminRepository(context);
        var userManager = new UserManager<User>(
            new UserStore<User>(context), null, null, null, null, null, null, null, null);

        fakeHostsConfig = new Mock<IOptions<HostsConfig>>();
        var config = new HostsConfig()
        {
            BackendUrl = "http://localhost:5443"
        };
        fakeHostsConfig.Setup(x => x.Value).Returns(config);

        areaAdminService = new CommonMinistryAdminService<long, AreaAdmin, AreaAdminBaseDto, AreaAdminRepository>(
            new Mock<IMapper>().Object,
            areaAdminRepository,
            new Mock<ILogger<CommonMinistryAdminService<long, AreaAdmin, AreaAdminBaseDto, AreaAdminRepository>>>().Object,
            new Mock<IEmailSenderService>().Object,
            userManager,
            context,
            new Mock<IRazorViewToStringRenderer>().Object,
            new Mock<IStringLocalizer<SharedResource>>().Object,
            fakeHostsConfig.Object);

        areaAdminControllerWithRealService = new AreaAdminController(fakeLogger.Object, areaAdminService);
        }

    [Test]
    public async Task Create_WithInvalidModel_ReturnsNotSuccessResponseDto()
    {
        // Arrange
        areaAdminController.ModelState.AddModelError("fakeKey", "Model is invalid");

        // Act
        var result = await areaAdminController.Create(new AreaAdminBaseDto());

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsSuccess, Is.False);
    }

    [Test]
    public async Task Create_WithValidModel_ReturnsSuccessResponseDto()
    {
        // Arrange
        areaAdminController.ModelState.Clear();
        
        // Act
        var result = await areaAdminController.Create(new AreaAdminBaseDto());

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(((AreaAdminBaseDto)result.Result).FirstName, Is.EqualTo("fakeFirstName"));
    }

    [Test]
    public async Task Update_WithValidModel_ReturnsSuccessResponseDto()
    {
        // Arrange

        // Act
        var result = await areaAdminController.Update("fakeAdminId", new AreaAdminBaseDto());

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(((AreaAdminBaseDto)result.Result).FirstName, Is.EqualTo("fakeFirstName"));
    }

    [Test]
    public async Task Update_WhenModelWithChangedCATOTTGIdAndInstitutionId_ShouldNotChangeCATOTTGIdAndInstitutionId()
    {
        // Arrange        
        var oldInstitutionId = Guid.NewGuid();
        long oldCAOTTGId = long.MinValue;
        var userId = string.Empty;
        AreaAdmin areaAdmin = new AreaAdmin { UserId = userId, InstitutionId = oldInstitutionId, CATOTTGId = oldCAOTTGId };
        await SeedAreaAdmin(areaAdmin);
        var areaAdminToUpdate = new AreaAdminBaseDto 
        { 
            UserId = userId,
            FirstName = string.Empty,
            LastName = string.Empty,
            InstitutionId = Guid.NewGuid(),
            CATOTTGId = long.MaxValue 
        };

        // Act
        await areaAdminControllerWithRealService.Update(userId, areaAdminToUpdate);

        // Assert
        var updatedAreaAdmin = areaAdminRepository.GetAll().Result.First();
        Assert.AreEqual(oldInstitutionId, updatedAreaAdmin.InstitutionId);
        Assert.AreEqual(oldCAOTTGId, updatedAreaAdmin.CATOTTGId);
    }

    [Test]
    public async Task Delete_WithValidModel_ReturnsSuccessResponseDto()
    {
        // Arrange
        
        // Act
        var result = await areaAdminController.Delete("fakeAdminId");

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(((AreaAdminBaseDto)result.Result).FirstName, Is.EqualTo("fakeFirstName"));
    }

    [Test]
    public async Task Delete_WithNullId_ReturnsException()
    {
        Assert.That(() => areaAdminController.Delete(null), Throws.ArgumentNullException);
    }

    [Test]
    public async Task Block_WithValidModel_ReturnsSuccessResponseDto()
    {
        // Arrange
        
        // Act
        var result = await areaAdminController.Block("fakeAdminId", It.IsAny<bool>());

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(((AreaAdminBaseDto)result.Result).FirstName, Is.EqualTo("fakeFirstName"));
    }

    [Test]
    public async Task OnActionExecuting_WithNullContext_ReturnsException()
    {
        Assert.That(() => areaAdminController.OnActionExecuting(null), Throws.ArgumentNullException);
    }

    [Test]
    public async Task Reinvite_WithValidModel_ReturnsSuccessResponseDto()
    {
        // Arrange

        // Act
        var result = await areaAdminController.Reinvite("fakeAdminId");

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.AreEqual(true, result.IsSuccess);
        Assert.AreEqual("fakeFirstName", ((AreaAdminBaseDto)result.Result).FirstName);
    }
    private async Task SeedAreaAdmin(AreaAdmin areaAdmin)
    {
        OutOfSchoolDbContext context = GetContext();

        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
        context.Add(areaAdmin);
        context.Add(new User { Id = areaAdmin.UserId, FirstName = string.Empty, LastName = string.Empty });
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