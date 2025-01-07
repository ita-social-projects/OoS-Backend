using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.WebApi.Controllers.V1;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OutOfSchool.WebApi.Tests.Controllers;
[TestFixture]
public class LanguageControllerTests
{
    private LanguageController controller;
    private Mock<ILanguageService> service;

    [SetUp]
    public void SetUp()
    {
        service = new Mock<ILanguageService>();

        controller = new LanguageController(service.Object);
    }

    [Test]
    public async Task Get_ReturnsNoContent_WhenListIsEmpty()
    {
        // Arrange
        var emptyList = new List<LanguageDto>();
        service.Setup(s => s.GetAll()).ReturnsAsync(emptyList);

        // Act
        var result = await controller.Get().ConfigureAwait(false) as NoContentResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(204));
    }

    [Test]
    public async Task Get_ReturnsOk_WhenListIsNotEmpty()
    {
        // Arrange
        var list = new List<LanguageDto>()
        {
            new LanguageDto()
            {
                Id = 1,
                Code = "en",
                Name = "English"
            }
        };
        service.Setup(s => s.GetAll()).ReturnsAsync(list);

        // Act
        var result = await controller.Get().ConfigureAwait(false) as OkObjectResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.StatusCode, Is.EqualTo(200));
    }
}
