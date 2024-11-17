using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Achievement;
using OutOfSchool.BusinessLogic.Models.Providers;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.BusinessLogic.Services.ProviderServices;
using OutOfSchool.Services.Enums;
using OutOfSchool.Tests.Common;
using OutOfSchool.WebApi.Controllers.V1;

namespace OutOfSchool.WebApi.Tests.Controllers;

[TestFixture]
internal class AchievementControllerTest
{
    private AchievementController controller;
    private Mock<IAchievementService> achievementService;
    private Mock<IProviderService> providerService;
    private Mock<IEmployeeService> employeeService;
    private Mock<IWorkshopService> workshopService;

    [SetUp]
    public void Setup()
    {
        achievementService = new Mock<IAchievementService>();
        providerService = new Mock<IProviderService>();
        employeeService = new Mock<IEmployeeService>();
        workshopService = new Mock<IWorkshopService>();

        controller = new AchievementController(achievementService.Object, providerService.Object, employeeService.Object, workshopService.Object);
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
        var dto = GetAchievementCreateDTO();
        workshopService.Setup(s => s.Exists(dto.WorkshopId)).ReturnsAsync(false);

        // Act
        var result = await controller.Create(dto).ConfigureAwait(false);

        // Assert
        Assert.IsInstanceOf<NotFoundObjectResult>(result);
    }

    [Test]
    public async Task CreateAchievement_WhenProviderIsBlocked_ShouldReturnForbidden()
    {
        // Arrange
        var dto = GetAchievementCreateDTO();
        var providerId = Guid.NewGuid();

        workshopService.Setup(s => s.Exists(dto.WorkshopId)).ReturnsAsync(true);
        providerService.Setup(s => s.GetProviderIdForWorkshopById(dto.WorkshopId)).ReturnsAsync(providerId);
        providerService.Setup(s => s.IsBlocked(providerId)).ReturnsAsync(true);

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
        var dto = GetAchievementCreateDTO();
        var providerId = Guid.NewGuid();

        workshopService.Setup(s => s.Exists(dto.WorkshopId)).ReturnsAsync(true);
        providerService.Setup(s => s.GetProviderIdForWorkshopById(dto.WorkshopId)).ReturnsAsync(providerId);
        providerService.Setup(s => s.IsBlocked(providerId)).ReturnsAsync(false);

        controller.ControllerContext.HttpContext = new DefaultHttpContext();
        controller.ControllerContext.HttpContext.SetContextUser(Role.Parent);

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
        var dto = GetAchievementCreateDTO();
        var providerId = Guid.NewGuid();
        var userId = Guid.NewGuid().ToString();

        workshopService.Setup(s => s.Exists(dto.WorkshopId)).ReturnsAsync(true);
        providerService.Setup(s => s.GetProviderIdForWorkshopById(dto.WorkshopId)).ReturnsAsync(providerId);
        providerService.Setup(s => s.IsBlocked(providerId)).ReturnsAsync(false);

        controller.ControllerContext.HttpContext = new DefaultHttpContext();
        controller.ControllerContext.HttpContext.SetContextUser(Role.Provider, userId);

        workshopService.Setup(s => s.GetWorkshopProviderOwnerIdAsync(dto.WorkshopId)).ReturnsAsync(providerId);
        employeeService.Setup(s => s.CheckUserIsRelatedEmployee(userId, providerId, dto.WorkshopId)).ReturnsAsync(true);
        achievementService.Setup(s => s.Create(dto)).ReturnsAsync(new AchievementDto());
        providerService.Setup(s => s.GetByUserId(userId, false)).ReturnsAsync(new ProviderDto { Id = providerId });

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
        var dto = GetAchievementCreateDTO();
        var providerId = Guid.NewGuid();
        var userId = Guid.NewGuid().ToString();

        workshopService.Setup(s => s.Exists(dto.WorkshopId)).ReturnsAsync(true);
        providerService.Setup(s => s.GetProviderIdForWorkshopById(dto.WorkshopId)).ReturnsAsync(providerId);
        providerService.Setup(s => s.IsBlocked(providerId)).ReturnsAsync(false);

        controller.ControllerContext.HttpContext = new DefaultHttpContext();
        controller.ControllerContext.HttpContext.SetContextUser(Role.Provider, userId);

        workshopService.Setup(s => s.GetWorkshopProviderOwnerIdAsync(dto.WorkshopId)).ReturnsAsync(providerId);
        employeeService
            .Setup(s => s.CheckUserIsRelatedEmployee(userId, providerId, dto.WorkshopId))
            .ReturnsAsync(true);
        achievementService.Setup(s => s.Create(dto)).ReturnsAsync(new AchievementDto());
        providerService.Setup(s => s.GetByUserId(userId, false)).ReturnsAsync(new ProviderDto() { Id = providerId });

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

    private AchievementCreateDTO GetAchievementCreateDTO()
    {
        return new AchievementCreateDTO()
        {
            Title = "Achievement_1",
            AchievementDate = DateTime.Now,
            AchievementTypeId = 1,
            WorkshopId = Guid.NewGuid(),
        };    }
}
