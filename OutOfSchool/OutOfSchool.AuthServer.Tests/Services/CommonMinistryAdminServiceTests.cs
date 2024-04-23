using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using OutOfSchool.AuthCommon.Services.Interfaces;
using OutOfSchool.Common.Models;
using OutOfSchool.EmailSender.Services;
using OutOfSchool.RazorTemplatesData.Services;
using OutOfSchool.Services;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using System;
using System.Net;
using System.Threading.Tasks;

namespace OutOfSchool.AuthCommon.Services.Tests;

[TestFixture]
public class CommonMinistryAdminServiceTests
{
    private readonly Mock<IEmailSenderService> fakeEmailSender;
    private readonly Mock<IMapper> fakeMapper;
    private readonly Mock<ILogger<CommonMinistryAdminService<long, AreaAdmin, AreaAdminBaseDto, AreaAdminRepository>>> fakeLogger;
    private AreaAdminRepository areaAdminRepository;
    private readonly Mock<UserManager<User>> fakeUserManager;
    private OutOfSchoolDbContext context;
    private readonly Mock<IRazorViewToStringRenderer> fakeRenderer;
    private readonly Mock<IStringLocalizer<SharedResource>> fakeLocalizer;
    private readonly Mock<IUrlHelper> fakeUrlHelper;

    private ICommonMinistryAdminService<AreaAdminBaseDto> areaCommonMinistryAdminService;

    public CommonMinistryAdminServiceTests()
    {
        fakeEmailSender = new Mock<IEmailSenderService>();
        fakeMapper = new Mock<IMapper>();
        fakeLogger = new Mock<ILogger<CommonMinistryAdminService<long, AreaAdmin, AreaAdminBaseDto, AreaAdminRepository>>>();
        fakeRenderer = new Mock<IRazorViewToStringRenderer>();
        fakeLocalizer = new Mock<IStringLocalizer<SharedResource>>();
        fakeUrlHelper = new Mock<IUrlHelper>();
        fakeUserManager = new Mock<UserManager<User>>(
             new Mock<IUserStore<User>>().Object,
             new Mock<IOptions<IdentityOptions>>().Object,
             new Mock<IPasswordHasher<User>>().Object,
             new IUserValidator<User>[0],
             new IPasswordValidator<User>[0],
             new Mock<ILookupNormalizer>().Object,
             new Mock<IdentityErrorDescriber>().Object,
             new Mock<IServiceProvider>().Object,
             new Mock<ILogger<UserManager<User>>>().Object);
    }

    [SetUp]
    public async Task SetUp()
    {
        context = GetContext();

        areaAdminRepository = new AreaAdminRepository(context);

        areaCommonMinistryAdminService = new CommonMinistryAdminService<long, AreaAdmin, AreaAdminBaseDto, AreaAdminRepository>(
            fakeMapper.Object,
            areaAdminRepository,
            fakeLogger.Object,
            fakeEmailSender.Object,
            fakeUserManager.Object,
            context,
            fakeRenderer.Object,
            fakeLocalizer.Object);

        await Seed();
    }

    [Test]
    public async Task Create_WhenParametersIsValid_ReturnsOkResponse()
    {
        // Arrange
        var (user, areaAdminBaseDto, areaAdmin) = CreateTestData();
        var role = Role.AreaAdmin;
        var userId = string.Empty;
        var userRole = "areaadmin";
        IUrlHelper url = fakeUrlHelper.Object;
        fakeUrlHelper.Setup(x => x.Action(It.IsAny<UrlActionContext>())).Returns("url");
        fakeMapper.Setup(x => x.Map<User>(areaAdminBaseDto)).Returns(user);
        fakeUserManager.Setup(x => x.CreateAsync(user, It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
        fakeUserManager.Setup(x => x.AddToRoleAsync(user, userRole)).ReturnsAsync(IdentityResult.Success);
        fakeMapper.Setup(x => x.Map<AreaAdmin>(areaAdminBaseDto)).Returns(areaAdmin);

        // Act
        var result = await areaCommonMinistryAdminService.CreateMinistryAdminAsync(areaAdminBaseDto, role, url, userId);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(HttpStatusCode.OK, result.HttpStatusCode);
        areaAdminBaseDto.Should().BeEquivalentTo(result.Result);
    }


    [Test]
    public void Update_WhenNullModel_ReturnsException()
    {
        // Act and Assert
        areaCommonMinistryAdminService
            .Invoking(x => x
            .UpdateMinistryAdminAsync(It.IsAny<AreaAdminBaseDto>(), It.IsAny<string>()))
            .Should()
            .ThrowAsync<ArgumentNullException>();
    }

    [Test]
    public async Task Update_WhenAdminNotExist_ReturnsNotFoundResponse()
    {
        // Arrange
        var areaAdminBaseDto = new AreaAdminBaseDto { UserId = "invalidId" };

        // Act
        var result = await areaCommonMinistryAdminService.UpdateMinistryAdminAsync(areaAdminBaseDto, It.IsAny<string>());

        // Assert
        Assert.AreEqual(false, result.IsSuccess);
        Assert.AreEqual(HttpStatusCode.NotFound, result.HttpStatusCode);
    }

    [Test]
    public async Task Update_WhenDuplicatedEmail_ReturnsBadRequestResponse()
    {
        // Arrange
        var areaAdminBaseDto = new AreaAdminBaseDto { Email = string.Empty };

        // Act
        var result = await areaCommonMinistryAdminService.UpdateMinistryAdminAsync(areaAdminBaseDto, It.IsAny<string>());

        // Assert
        Assert.AreEqual(false, result.IsSuccess);
        Assert.AreEqual(HttpStatusCode.BadRequest, result.HttpStatusCode);
    }

    [Test]
    public async Task Update_WhenAllValid_ReturnsOkResponse()
    {
        // Arrange
        var (user, areaAdminBaseDto, _) = CreateTestData();
        fakeUserManager.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user);
        fakeUserManager.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);
        fakeUserManager.Setup(x => x.UpdateSecurityStampAsync(user)).ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await areaCommonMinistryAdminService.UpdateMinistryAdminAsync(areaAdminBaseDto, It.IsAny<string>());

        // Assert
        Assert.AreEqual(true, result.IsSuccess);
        Assert.AreEqual(HttpStatusCode.OK, result.HttpStatusCode);
    }

    private static OutOfSchoolDbContext GetContext()
    {
        return new OutOfSchoolDbContext(
            new DbContextOptionsBuilder<OutOfSchoolDbContext>()
            .UseInMemoryDatabase(databaseName: "OutOfSchoolTestDB")
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options);
    }

    private async Task Seed()
    {
        var context = GetContext();

        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
        context.Add(new AreaAdmin { UserId = string.Empty });
        context.Add(new User { Id = string.Empty, FirstName = string.Empty, LastName = string.Empty, Email = string.Empty });
        await context.SaveChangesAsync();
    }

    private (User, AreaAdminBaseDto, AreaAdmin) CreateTestData()
    {
        var user = new User
        {
            FirstName = "Іван",
            LastName = "Петренко",
        };

        var areaAdminBaseDto = new AreaAdminBaseDto
        {
            UserId = string.Empty,
            FirstName = "Іван",
            LastName = "Петренко",
            Email = "test@example.com",
            PhoneNumber = "985646549",
            CreatingTime = DateTimeOffset.Now,
            InstitutionId = Guid.NewGuid(),
            CATOTTGId = 28675,
        };

        var areaAdmin = new AreaAdmin
        {
            InstitutionId = areaAdminBaseDto.InstitutionId,
            User = user,
            UserId = string.Empty,
        };

        return (user, areaAdminBaseDto, areaAdmin);
    }
}