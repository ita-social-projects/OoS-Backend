using System;
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
    public void SplitSearchString_ConfigurationIsNull_ShouldApplyDefaultSeparators()
    {
        // Arrange
        string input = "test search string";
        SetUpOptionsAndExpectedResult(null, input, out var expectedRes);

        // Act
        var result = searchStringService.SplitSearchString(input);

        // Assert
        result.Should()
            .BeEquivalentTo(expectedRes);
    }

    [Test]
    public void SplitSearchString_InputIsWhiteSpace_ShouldReturnEmptyArray()
    {
        // Arrange
        string input = " ";

        // Act
        var result = searchStringService.SplitSearchString(input);

        // Assert
        result.Should()
            .BeEmpty();
    }

    [Test]
    public void SplitSearchString_InputIsWhiteSpaceAndCommaCharacters_ShouldReturnEmptyArray()
    {
        // Arrange
        string input = " ,   ,   , ";
        SetUpOptionsAndExpectedResult([" ", ", "], input, out var expectedRes);

        // Act
        var result = searchStringService.SplitSearchString(input);

        // Assert
        result.Should()
            .BeEmpty();
    }

    [Test]
    public void SplitSearchString_InputIsValid_ShouldReturnArrayOfStrings()
    {
        // Arrange
        string input = " test, search string ";
        SetUpOptionsAndExpectedResult([" ", ", "], input, out var expectedRes);

        // Act
        var result = searchStringService.SplitSearchString(input);

        // Assert
        result.Should()
            .BeEquivalentTo(expectedRes);
    }

    private void SetUpOptionsAndExpectedResult(string[] separators, string input, out string[] expectedResult)
    {
        var options = separators == null ? null : new SearchStringOptions { Separators = separators };

        mockSettings.Setup(s => s.Value).Returns(options);

        expectedResult = input.Split(
            separators ?? new string[] { " " },
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }
}
