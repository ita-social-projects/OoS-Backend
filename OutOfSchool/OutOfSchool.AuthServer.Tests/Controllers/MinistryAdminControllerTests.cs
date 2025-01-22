using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.AuthCommon.Controllers;
using OutOfSchool.AuthCommon.Services;
using OutOfSchool.AuthCommon.Services.Interfaces;
using OutOfSchool.Common;
using OutOfSchool.Common.Models;
using OutOfSchool.RazorTemplatesData.Services;
using OutOfSchool.Services;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using System;
using OutOfSchool.AuthCommon;
using System.Linq;
using OutOfSchool.EmailSender.Services;
using Microsoft.Extensions.Options;
using OutOfSchool.AuthCommon.Config;
using OutOfSchool.Tests.Common.DbContextTests;

namespace OutOfSchool.AuthServer.Tests.Controllers;

[TestFixture]
public class MinistryAdminControllerTests
{
    private readonly MinistryAdminController ministryAdminController;
    private readonly Mock<ILogger<MinistryAdminController>> fakeLogger;
    private readonly Mock<ICommonMinistryAdminService<MinistryAdminBaseDto>> fakeMinistryAdminService;
    private readonly Mock<HttpContext> fakehttpContext;
    private MinistryAdminController ministryAdminControllerWithRealService;
    private ICommonMinistryAdminService<MinistryAdminBaseDto> ministryAdminService;
    private InstitutionAdminRepository ministryAdminRepository;
    private Mock<IOptions<HostsConfig>> fakeHostsConfig;

    public MinistryAdminControllerTests()
    {
        fakeLogger = new Mock<ILogger<MinistryAdminController>>();
        fakeMinistryAdminService = new Mock<ICommonMinistryAdminService<MinistryAdminBaseDto>>();
    
        fakehttpContext = new Mock<HttpContext>();
        
        ministryAdminController = new MinistryAdminController(
            fakeLogger.Object,
            fakeMinistryAdminService.Object
        );
        ministryAdminController.ControllerContext.HttpContext = fakehttpContext.Object;
    }

    [SetUp]
    public void Setup()
    {
        var fakeMinistryAdminDto = new MinistryAdminBaseDto()
        {
            FirstName = "fakeFirstName",
            LastName = "fakeLastName",
            Email = "fake@email.com",
            PhoneNumber = "11-222-33-44",
        };

        var fakeResponseDto = new ResponseDto()
        {
            IsSuccess = true,
            Result = fakeMinistryAdminDto
        };

        fakeMinistryAdminService.Setup(s => s.CreateMinistryAdminAsync(It.IsAny<MinistryAdminBaseDto>(),
            It.IsAny<Role>(), It.IsAny<IUrlHelper>(), It.IsAny<string>())).ReturnsAsync(fakeResponseDto);

        fakeMinistryAdminService.Setup(s => s.DeleteMinistryAdminAsync(It.IsAny<string>(),
            It.IsAny<string>())).ReturnsAsync(fakeResponseDto);
        
        fakeMinistryAdminService.Setup(s => s.BlockMinistryAdminAsync(It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(fakeResponseDto);

        fakeMinistryAdminService.Setup(s => s.ReinviteMinistryAdminAsync(It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<IUrlHelper>())).ReturnsAsync(fakeResponseDto);

        var fakeHttpContext = new Mock<HttpContext>();
        fakeHttpContext.Setup(s => s.Request.Headers[It.IsAny<string>()]).Returns("Ok");
        
        ministryAdminController.ControllerContext.HttpContext = fakeHttpContext.Object;

        var context = GetContext();

        ministryAdminRepository = new InstitutionAdminRepository(context);
        var userManager = new UserManager<User>(
            new UserStore<User>(context), null, null, null, null, null, null, null, null);

        fakeHostsConfig = new Mock<IOptions<HostsConfig>>();
        var config = new HostsConfig()
        {
            BackendUrl = "http://localhost:5443"
        };
        fakeHostsConfig.Setup(x => x.Value).Returns(config);

        ministryAdminService = new CommonMinistryAdminService<Guid, InstitutionAdmin, MinistryAdminBaseDto, InstitutionAdminRepository>(
            new Mock<IMapper>().Object,
            ministryAdminRepository,
            new Mock<ILogger<CommonMinistryAdminService<Guid, InstitutionAdmin, MinistryAdminBaseDto, InstitutionAdminRepository>>>().Object,
        new Mock<IEmailSenderService>().Object,
            userManager,
            context,
            new Mock<IRazorViewToStringRenderer>().Object,
            new Mock<IStringLocalizer<SharedResource>>().Object,
            fakeHostsConfig.Object);

        ministryAdminControllerWithRealService = new MinistryAdminController(new Mock<ILogger<MinistryAdminController>>().Object, ministryAdminService);
    }

    [Test]
    public async Task Create_WithInvalidModel_ReturnsNotSuccessResponseDto()
    {
        // Arrange
        ministryAdminController.ModelState.AddModelError("fakeKey", "Model is invalid");

        // Act
        var result = await ministryAdminController.Create(new MinistryAdminBaseDto());

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.AreEqual(false, result.IsSuccess);
    }

    [Test]
    public async Task Create_WithValidModel_ReturnsSuccessResponseDto()
    {
        // Arrange
        ministryAdminController.ModelState.Clear();
        
        // Act
        var result = await ministryAdminController.Create(new MinistryAdminBaseDto());

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.AreEqual(true, result.IsSuccess);
    }
    
    [Test]
    public async Task Delete_WithValidModel_ReturnsSuccessResponseDto()
    {
        // Arrange
        
        // Act
        var result = await ministryAdminController.Delete("fakeAdminId");

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.AreEqual(true, result.IsSuccess);
        Assert.AreEqual("fakeFirstName", ((MinistryAdminBaseDto)result.Result).FirstName);
    }
    
    [Test]
    public async Task Block_WithValidModel_ReturnsSuccessResponseDto()
    {
        // Arrange
        
        // Act
        var result = await ministryAdminController.Block("fakeAdminId", It.IsAny<bool>());

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.AreEqual(true, result.IsSuccess);
        Assert.AreEqual("fakeFirstName", ((MinistryAdminBaseDto)result.Result).FirstName);
    }

    [Test]
    public async Task Reinvite_WithValidModel_ReturnsSuccessResponseDto()
    {
        // Arrange

        // Act
        var result = await ministryAdminController.Reinvite("fakeAdminId");

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.AreEqual(true, result.IsSuccess);
        Assert.AreEqual("fakeFirstName", ((MinistryAdminBaseDto)result.Result).FirstName);
    }

    [Test]
    public async Task Update_WhenModelWithChangedInstitutionId_ShouldNotChangeInstitutionId()
    {
        // Arrange        
        var oldInstitutionId = Guid.NewGuid();
        var userId = string.Empty;
        InstitutionAdmin ministryAdmin = new InstitutionAdmin { UserId = userId, InstitutionId = oldInstitutionId };
        await SeedMinistryAdmin(ministryAdmin);
        var ministryAdminToUpdate = new MinistryAdminBaseDto
        {
            UserId = userId,
            FirstName = string.Empty,
            LastName = string.Empty,
            InstitutionId = Guid.NewGuid()
        };

        // Act
        await ministryAdminControllerWithRealService.Update(userId, ministryAdminToUpdate);

        // Assert
        var updatedRegionAdmin = ministryAdminRepository.GetAll().Result.First();
        Assert.AreEqual(oldInstitutionId, updatedRegionAdmin.InstitutionId);
    }

    private async Task SeedMinistryAdmin(InstitutionAdmin ministryAdmin)
    {
        OutOfSchoolDbContext context = GetContext();

        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
        context.Add(ministryAdmin);
        context.Add(new User { Id = ministryAdmin.UserId, FirstName = string.Empty, LastName = string.Empty });
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