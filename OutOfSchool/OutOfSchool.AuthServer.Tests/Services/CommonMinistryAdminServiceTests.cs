using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using OutOfSchool.AuthCommon.Config;
using OutOfSchool.AuthCommon.Services.Interfaces;
using OutOfSchool.Common.Models;
using OutOfSchool.EmailSender.Services;
using OutOfSchool.RazorTemplatesData.Services;
using OutOfSchool.Services;
using OutOfSchool.Services.Enums;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using OutOfSchool.Tests.Common.DbContextTests;
using OutOfSchool.Tests.Common.TestDataGenerators;
using System;
using System.Net;
using System.Threading.Tasks;

namespace OutOfSchool.AuthCommon.Services.Tests;

[TestFixture]
public class CommonMinistryAdminServiceTests
{
    private Mock<IMapper> fakeMapper;
    private AreaAdminRepository areaAdminRepository;
    private Mock<UserManager<User>> fakeUserManager;
    private OutOfSchoolDbContext context;
    private Mock<IUrlHelper> fakeUrlHelper;
    private Mock<IOptions<HostsConfig>> fakeHostsConfig;

    private ICommonMinistryAdminService<AreaAdminBaseDto> areaCommonMinistryAdminService;

    [SetUp]
    public async Task SetUp()
    {
        fakeMapper = new Mock<IMapper>();
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

        context = GetContext();

        areaAdminRepository = new AreaAdminRepository(context);

        fakeHostsConfig = new Mock<IOptions<HostsConfig>>();
        var config = new HostsConfig()
        {
            BackendUrl = "http://localhost:5443"
        };
        fakeHostsConfig.Setup(x => x.Value).Returns(config);

        areaCommonMinistryAdminService = new CommonMinistryAdminService<long, AreaAdmin, AreaAdminBaseDto, AreaAdminRepository>(
            fakeMapper.Object,
            areaAdminRepository,
            new Mock<ILogger<CommonMinistryAdminService<long, AreaAdmin, AreaAdminBaseDto, AreaAdminRepository>>>().Object,
            new Mock<IEmailSenderService>().Object,
            fakeUserManager.Object,
            context,
            new Mock<IRazorViewToStringRenderer>().Object,
            new Mock<IStringLocalizer<SharedResource>>().Object,
            fakeHostsConfig.Object);

        await Seed();
    }

    [Test]
    public async Task Create_WhenParametersIsValid_ReturnsOkResponse()
    {
        // Arrange
        var user = UserGenerator.Generate();
        var areaAdminBaseDto = AdminGenerator.GenerateAreaAdminBaseDto(); 
        var areaAdmin = AdminGenerator.GenerateAreaAdmin();
        var role = Role.AreaAdmin;
        var userId = string.Empty;
        var userRole = "areaadmin";
        IUrlHelper url = fakeUrlHelper.Object;
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
        var user = UserGenerator.Generate();
        var areaAdminBaseDto = AdminGenerator.GenerateAreaAdminBaseDto();
        areaAdminBaseDto.UserId = string.Empty;
        fakeUserManager.Setup(x => x.FindByIdAsync(It.IsAny<string>())).ReturnsAsync(user);
        fakeUserManager.Setup(x => x.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);
        fakeUserManager.Setup(x => x.UpdateSecurityStampAsync(user)).ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await areaCommonMinistryAdminService.UpdateMinistryAdminAsync(areaAdminBaseDto, It.IsAny<string>());

        // Assert
        Assert.AreEqual(true, result.IsSuccess);
        Assert.AreEqual(HttpStatusCode.OK, result.HttpStatusCode);
    }

    private static TestOutOfSchoolDbContext GetContext()
    {
        return new TestOutOfSchoolDbContext(
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
}