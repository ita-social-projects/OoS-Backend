using System.Collections.Generic;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Application;
using OutOfSchool.WebApi.Extensions;

namespace OutOfSchool.WebApi.Tests.Extensions;

[TestFixture]
public class MapSearchResultToOkOrNoContentExtensionTests
{
    private class TestController : ControllerBase
    {
    }

    private TestController controller;

    [SetUp]
    public void SetUp()
    {
        controller = new TestController();
    }

    [Test]
    public void MapSearchResultToOkOrNoContentExtension_SearchResultIsNotNullAndNotEmpty_ReturnOk()
    {
        // Arrange
        var applications = new List<ApplicationDto>() {
            new ApplicationDto() { RejectionMessage = "Test 1" },
            new ApplicationDto() { RejectionMessage = "Test 2", },
            new ApplicationDto() { RejectionMessage = "Test 3" },
        };

        var searchResult = new SearchResult<ApplicationDto>() { TotalAmount = applications.Count, Entities = applications };

        // Act
        var result = controller.MapSearchResultToOkOrNoContent(searchResult);

        // Assert
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

        // Act
        var result = controller.MapSearchResultToOkOrNoContent(searchResult);

        // Assert
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

        var searchResult = new SearchResult<ApplicationDto>() { TotalAmount = applications.Count, Entities = applications };

        // Act
        var result = controller.MapSearchResultToOkOrNoContent(searchResult);

        // Assert
        result.Should().NotBeNull();
        result.Should()
              .BeOfType<NoContentResult>()
              .Which.StatusCode
              .Should()
              .Be(StatusCodes.Status204NoContent);
    }
}