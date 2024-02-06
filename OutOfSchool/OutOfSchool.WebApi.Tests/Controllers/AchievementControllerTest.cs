using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using OutOfSchool.Services.Enums;
using OutOfSchool.WebApi.Controllers.V1;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.Achievement;
using OutOfSchool.WebApi.Models.Providers;
using OutOfSchool.WebApi.Services;
using OutOfSchool.WebApi.Services.ProviderServices;

namespace OutOfSchool.WebApi.Tests.Controllers;

[TestFixture]
internal class AchievementControllerTest
{
    private AchievementController controller;
    private Mock<IAchievementService> achievementService;
    private Mock<IProviderService> providerService;
    private Mock<IProviderAdminService> providerAdminService;
    private Mock<IWorkshopService> workshopService;

    [SetUp]
    public void Setup()
    {
        achievementService = new Mock<IAchievementService>();
        providerService = new Mock<IProviderService>();
        providerAdminService = new Mock<IProviderAdminService>();
        workshopService = new Mock<IWorkshopService>();

        controller = new AchievementController(achievementService.Object, providerService.Object, providerAdminService.Object, workshopService.Object);
    }

    [Test]
    public async Task CreateAchievement_WhenAchievementDtoIsNull_ShouldReturnBadRequest()
    {
        // Act
        var result = await controller.Create(null).ConfigureAwait(false);

        // Assert
        Assert.IsInstanceOf<BadRequestObjectResult>(result);
    }

    [Test]
    public async Task CreateAchievement_WhenWorkshopIdIsNotValid_ShouldReturnNotFound()
    {
        // Arrange
        var dto = new AchievementCreateDTO();
        workshopService.Setup(s => s.Exists(It.IsAny<Guid>())).ReturnsAsync(false);

        // Act
        var result = await controller.Create(dto).ConfigureAwait(false);

        // Assert
        Assert.IsInstanceOf<NotFoundObjectResult>(result);
    }

    [Test]
    public async Task CreateAchievement_WhenProviderIsBlocked_ShouldReturnForbidden()
    {
        // Arrange
        var dto = new AchievementCreateDTO();
        workshopService.Setup(s => s.Exists(It.IsAny<Guid>())).ReturnsAsync(true);
        providerService.Setup(s => s.GetProviderIdForWorkshopById(It.IsAny<Guid>())).ReturnsAsync(Guid.NewGuid());
        providerService.Setup(s => s.IsBlocked(It.IsAny<Guid>())).ReturnsAsync(true);

        // Act
        var result = await controller.Create(dto).ConfigureAwait(false) as ObjectResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.AreEqual(403, result.StatusCode);
    }

    [Test]
    public async Task CreateAchievement_UserDontHaveRights_ShouldReturnForbidden()
    {
        // Arrange
        var dto = new AchievementCreateDTO();
        workshopService.Setup(s => s.Exists(It.IsAny<Guid>())).ReturnsAsync(true);
        providerService.Setup(s => s.GetProviderIdForWorkshopById(It.IsAny<Guid>())).ReturnsAsync(Guid.NewGuid());
        providerService.Setup(s => s.IsBlocked(It.IsAny<Guid>())).ReturnsAsync(false);

        var user = new ClaimsPrincipal(new ClaimsIdentity(
            new Claim[]
            {
                new Claim(ClaimTypes.Role, nameof(Role.Parent).ToLower()),
                new Claim("subrole", nameof(Subrole.None).ToLower()),
                new Claim("sub", Guid.NewGuid().ToString()),
            }));

        controller.ControllerContext.HttpContext = new DefaultHttpContext { User = user };

        // Act
        var result = await controller.Create(dto).ConfigureAwait(false) as ObjectResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.AreEqual(403, result.StatusCode);
    }

    [Test]
    public async Task CreateAchievement_UserProviderAdminHaveRights_ShouldReturnCreated()
    {
        // Arrange
        var dto = new AchievementCreateDTO();
        workshopService.Setup(s => s.Exists(It.IsAny<Guid>())).ReturnsAsync(true);
        providerService.Setup(s => s.GetProviderIdForWorkshopById(It.IsAny<Guid>())).ReturnsAsync(Guid.NewGuid());
        providerService.Setup(s => s.IsBlocked(It.IsAny<Guid>())).ReturnsAsync(false);

        var user = new ClaimsPrincipal(new ClaimsIdentity(
            new Claim[]
            {
                new Claim(ClaimTypes.Role, nameof(Role.Provider).ToLower()),
                new Claim("subrole", nameof(Subrole.ProviderAdmin).ToLower()),
                new Claim("sub", Guid.NewGuid().ToString()),
            }));

        controller.ControllerContext.HttpContext = new DefaultHttpContext { User = user };

        workshopService.Setup(s => s.GetWorkshopProviderOwnerIdAsync(It.IsAny<Guid>())).ReturnsAsync(Guid.NewGuid());
        providerAdminService.Setup(s => s.CheckUserIsRelatedProviderAdmin(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(true);
        achievementService.Setup(s => s.Create(It.IsAny<AchievementCreateDTO>())).ReturnsAsync(new AchievementDto());

        // Act
        var result = await controller.Create(dto).ConfigureAwait(false) as CreatedAtActionResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.AreEqual(201, result.StatusCode);
    }

    [Test]
    public async Task CreateAchievement_UserDeputyHaveRights_ShouldReturnForbidden()
    {
        // Arrange
        var providerId = Guid.NewGuid();

        var dto = new AchievementCreateDTO();
        workshopService.Setup(s => s.Exists(It.IsAny<Guid>())).ReturnsAsync(true);
        providerService.Setup(s => s.GetProviderIdForWorkshopById(It.IsAny<Guid>())).ReturnsAsync(Guid.NewGuid());
        providerService.Setup(s => s.IsBlocked(It.IsAny<Guid>())).ReturnsAsync(false);

        var user = new ClaimsPrincipal(new ClaimsIdentity(
            new Claim[]
            {
                new Claim(ClaimTypes.Role, nameof(Role.Provider).ToLower()),
                new Claim("subrole", nameof(Subrole.ProviderDeputy).ToLower()),
                new Claim("sub", Guid.NewGuid().ToString()),
            }));

        controller.ControllerContext.HttpContext = new DefaultHttpContext { User = user };

        workshopService.Setup(s => s.GetWorkshopProviderOwnerIdAsync(It.IsAny<Guid>())).ReturnsAsync(providerId);
        providerAdminService.Setup(s => s.CheckUserIsRelatedProviderAdmin(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>())).ReturnsAsync(true);
        achievementService.Setup(s => s.Create(It.IsAny<AchievementCreateDTO>())).ReturnsAsync(new AchievementDto());
        providerService.Setup(s => s.GetByUserId(It.IsAny<string>(), It.IsAny<bool>())).ReturnsAsync(new ProviderDto() { Id = providerId });

        // Act
        var result = await controller.Create(dto).ConfigureAwait(false) as CreatedAtActionResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.AreEqual(201, result.StatusCode);
    }

    [Test]
    public async Task GetByWorkshopId_Valid_ReturnsOkObject()
    {
        // Arrange
        achievementService.Setup(a => a.GetByFilter(It.IsAny<AchievementsFilter>())).ReturnsAsync(SearchResult());

        // Act
        var result = await controller.GetByWorkshopId(It.IsAny<AchievementsFilter>()).ConfigureAwait(false) as OkObjectResult;
        var resultValue = result.Value as SearchResult<AchievementDto>;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.AreEqual(200, result.StatusCode);
        Assert.AreEqual(10, resultValue.TotalAmount);
        Assert.AreEqual(6, resultValue.Entities.Count);
    }

    [Test]
    public async Task GetByWorkshopId_NotValid_RetunsNoCententResult()
    {
        // Arrange
        achievementService.Setup(a => a.GetByFilter(It.IsAny<AchievementsFilter>())).ReturnsAsync(new SearchResult<AchievementDto>());

        var result = await controller.GetByWorkshopId(It.IsAny<AchievementsFilter>()).ConfigureAwait(false) as NoContentResult;

        Assert.That(result, Is.Not.Null);
        Assert.AreEqual(204, result.StatusCode);
    }

    private static SearchResult<AchievementDto> SearchResult()
    {
        return new SearchResult<AchievementDto>
        {
            TotalAmount = 10,
            Entities = new List<AchievementDto>()
            {
                new AchievementDto(),
                new AchievementDto(),
                new AchievementDto(),
                new AchievementDto(),
                new AchievementDto(),
                new AchievementDto(),
            },
        };
    }
}
