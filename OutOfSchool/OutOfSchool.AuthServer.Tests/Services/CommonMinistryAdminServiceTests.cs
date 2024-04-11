using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.AuthCommon.Services.Interfaces;
using OutOfSchool.Common.Models;
using OutOfSchool.EmailSender.Services;
using OutOfSchool.RazorTemplatesData.Services;
using OutOfSchool.Services;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository;
using System;
using System.Net;
using System.Threading.Tasks;

namespace OutOfSchool.AuthCommon.Services.Tests;

[TestFixture()]
public class CommonMinistryAdminServiceTests
{
    private ICommonMinistryAdminService<AreaAdminBaseDto> areaCommonMinistryAdminService;
    private AreaAdminRepository areaAdminRepository;

    [SetUp]
    public async Task SetUp()
    {
        var context = GetContext();

        areaAdminRepository = new AreaAdminRepository(context);

        var userManager = new UserManager<User>(
            new UserStore<User>(context), null, null, null, null, null, null, null, null);

        areaCommonMinistryAdminService = new CommonMinistryAdminService<long, AreaAdmin, AreaAdminBaseDto, AreaAdminRepository>(
            new Mock<IMapper>().Object,
            areaAdminRepository,
            new Mock<ILogger<CommonMinistryAdminService<long, AreaAdmin, AreaAdminBaseDto, AreaAdminRepository>>>().Object,
            new Mock<IEmailSenderService>().Object,
            userManager,
            context,
            new Mock<IRazorViewToStringRenderer>().Object,
            new Mock<IStringLocalizer<SharedResource>>().Object);

         await Seed();
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
        var areaAdminBaseDto = new AreaAdminBaseDto { UserId = string.Empty, FirstName = string.Empty, LastName = string.Empty };

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
}