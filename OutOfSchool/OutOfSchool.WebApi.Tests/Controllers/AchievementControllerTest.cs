using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using OutOfSchool.WebApi.Controllers.V1;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.Achievement;
using OutOfSchool.WebApi.Services;

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

    private SearchResult<AchievementDto> SearchResult()
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
                new AchievementDto()
            },
        };
    }

}
