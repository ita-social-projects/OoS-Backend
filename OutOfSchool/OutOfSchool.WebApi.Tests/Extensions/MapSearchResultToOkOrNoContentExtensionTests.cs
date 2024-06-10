using System.Collections.Generic;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Application;
using OutOfSchool.Tests.Common.TestDataGenerators;
using OutOfSchool.WebApi.Extensions;

namespace OutOfSchool.WebApi.Tests.Extensions;

[TestFixture]
public class MapSearchResultToOkOrNoContentExtensionTests
{
    [Test]
    public void MapSearchResultToOkOrNoContentExtension_SearchResultIsNotNullAndNotEmpty_ReturnOk()
    {
        // Arrange
        var applications = ApplicationDTOsGenerator.Generate(3);
        var searchResult = new SearchResult<ApplicationDto>() { Entities = applications };
        var expectedResult = new OkObjectResult(new { });

        var mockedController = new Mock<ControllerBase>();
        mockedController.Setup(c => c.Ok(searchResult)).Returns(expectedResult);

        // Act
        var result = mockedController.Object.MapSearchResultToOkOrNoContent(searchResult);

        // Assert
        mockedController.VerifyAll();
        mockedController.Verify(c => c.Ok(searchResult), Times.Once);
        mockedController.Verify(c => c.NoContent(), Times.Never);

        result.Should().NotBeNull();
        result.Should()
              .BeOfType<OkObjectResult>()
              .Which.StatusCode
              .Should()
              .Be(StatusCodes.Status200OK);
    }

    [Test]
    public void MapSearchResultToOkOrNoContentExtension_SearchResultIsNull_ReturnNoContent()
    {
        // Arrange
        SearchResult<ApplicationDto> searchResult = null;
        var expectedResult = new NoContentResult();

        var mockedController = new Mock<ControllerBase>();
        mockedController.Setup(c => c.NoContent()).Returns(new NoContentResult());

        // Act
        var result = mockedController.Object.MapSearchResultToOkOrNoContent(searchResult);

        // Assert
        mockedController.VerifyAll();
        mockedController.Verify(c => c.NoContent(), Times.Once);
        mockedController.Verify(c => c.Ok(), Times.Never);

        result.Should().NotBeNull();
        result.Should()
              .BeOfType<NoContentResult>()
              .Which.StatusCode
              .Should()
              .Be(StatusCodes.Status204NoContent);
    }

    [Test]
    public void MapSearchResultToOkOrNoContentExtension_SearchResultIsNotNullButEmpty_ReturnNoContent()
    {
        // Arrange
        var applications = new List<ApplicationDto>()
        {
        };

        var searchResult = new SearchResult<ApplicationDto>() { Entities = applications };
        var expectedResult = new NoContentResult();

        var mockedController = new Mock<ControllerBase>();
        mockedController.Setup(c => c.NoContent()).Returns(new NoContentResult());

        // Act
        var result = mockedController.Object.MapSearchResultToOkOrNoContent(searchResult);

        // Assert
        mockedController.VerifyAll();
        mockedController.Verify(c => c.NoContent(), Times.Once);
        mockedController.Verify(c => c.Ok(), Times.Never);

        result.Should().NotBeNull();
        result.Should()
              .BeOfType<NoContentResult>()
              .Which.StatusCode
              .Should()
              .Be(StatusCodes.Status204NoContent);
    }
}