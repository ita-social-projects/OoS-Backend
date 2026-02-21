using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.Services.Models;
using OutOfSchool.Services.Repository.Base.Api;
using NUnit.Framework;

namespace OutOfSchool.WebApi.Tests.Services;

[TestFixture]
public class LanguageServiceTests
{
    private readonly LanguageService service;
    private readonly Mock<IEntityRepository<long, Language>> repositoryMock;
    private readonly Mock<ILogger<LanguageService>> loggerMock;

    public LanguageServiceTests()
    {
        repositoryMock = new Mock<IEntityRepository<long, Language>>();
        loggerMock = new Mock<ILogger<LanguageService>>();
        service = new LanguageService(repositoryMock.Object, loggerMock.Object);
    }

    [Test]
    public async Task GetAll_ShouldReturnAllLanguages()
    {
        // Arrange
        var languages = new List<Language>
        {
            new() { Id = 1, Name = "English" },
            new() { Id = 2, Name = "Ukrainian" },
        };

        repositoryMock.Setup(r => r.GetAll()).ReturnsAsync(languages);

        // Act
        var result = await service.GetAll();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Select(x => x.Id).Should().BeEquivalentTo(new[] { 1, 2 });
        result.Should().AllBeOfType<LanguageDto>();

        repositoryMock.Verify(r => r.GetAll(), Times.Once);
    }

    [Test]
    public async Task GetById_WhenLanguageExists_ShouldReturnLanguageDto()
    {
        // Arrange
        var language = new Language { Id = 10, Name = "Spanish" };
        repositoryMock.Setup(r => r.GetById(10)).ReturnsAsync(language);

        // Act
        var result = await service.GetById(10);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(10);
        result.Name.Should().Be("Spanish");

        repositoryMock.Verify(r => r.GetById(10), Times.Once);
    }

    [Test]
    public async Task GetById_WhenLanguageDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        repositoryMock.Setup(r => r.GetById(It.IsAny<long>())).ReturnsAsync((Language)null);

        // Act
        var result = await service.GetById(999);

        // Assert
        result.Should().BeNull();
        repositoryMock.Verify(r => r.GetById(999), Times.Once);
    }
}
