using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Config.SearchString;
using OutOfSchool.BusinessLogic.Services.SearchString;

namespace OutOfSchool.WebApi.SearchStringServiceTest;

[TestFixture]
public class SearchStringServiceTests
{
    private Mock<IOptions<SearchStringOptions>> mockSettings;
    private Mock<ILogger<SearchStringService>> mockLogger;
    private ISearchStringService searchStringService;

    [SetUp]
    public void SetUp()
    {
        mockLogger = new Mock<ILogger<SearchStringService>>();
        mockSettings = new Mock<IOptions<SearchStringOptions>>();

        searchStringService = new SearchStringService(
            mockSettings.Object,
            mockLogger.Object);
    }

    [Test]
    public void SplitSearchString_WhenConfigurationIsNull_ShouldApplyDefaultSeparators()
    {
        // Arrange
        var input = "test search string";
        var expectedResult = new[] { "test", "search", "string" };

        mockSettings.Setup(s => s.Value)
          .Returns(new SearchStringOptions() { Separators = null});

        // Act
        var result = searchStringService.SplitSearchString(input);

        // Assert
        result.Should()
            .BeEquivalentTo(expectedResult);

        mockSettings.VerifyAll();
    }

    [Test]
    public void SplitSearchString_WhenInputIsWhiteSpace_ShouldReturnEmptyArray()
    {
        // Act
        var result = searchStringService.SplitSearchString(" ");

        // Assert
        result.Should()
            .BeEmpty();

        mockSettings.VerifyAll();
    }

    [Test]
    public void SplitSearchString_WhenInputIsWhiteSpaceAndCommaCharacters_ShouldReturnEmptyArray()
    {
        // Arrange
        mockSettings.Setup(s => s.Value)
           .Returns(new SearchStringOptions { Separators = [" ", ", "] });

        // Act
        var result = searchStringService.SplitSearchString(" ,   ,   , ");

        // Assert
        result.Should()
            .BeEmpty();

        mockSettings.VerifyAll();
    }

    [Test]
    public void SplitSearchString_WhenInputIsValid_ShouldReturnArrayOfStrings()
    {
        // Arrange
        var input = " test, search string ";
        var expectedResult = new[] { "test", "search", "string" };
        mockSettings.Setup(s => s.Value)
            .Returns(new SearchStringOptions { Separators = [" ", ","] });

        // Act
        var result = searchStringService.SplitSearchString(input);

        // Assert
        result.Should()
            .BeEquivalentTo(expectedResult);

        mockSettings.VerifyAll();
    }
}
