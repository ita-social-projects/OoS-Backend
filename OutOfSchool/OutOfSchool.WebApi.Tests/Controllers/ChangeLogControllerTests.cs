using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Enums;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Changes;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.WebApi.Controllers.V1;

namespace OutOfSchool.WebApi.Tests.Controllers;

[TestFixture]
public class ChangeLogControllerTests
{
    private ChangesLogController controller;
    private Mock<IChangesLogService> changesLogServiceMock;
    private Mock<HttpContext> httpContextMock;

    [SetUp]
    public void Setup()
    {
        changesLogServiceMock = new Mock<IChangesLogService>();
        httpContextMock = new Mock<HttpContext>();

        controller = new ChangesLogController(changesLogServiceMock.Object)
        {
            ControllerContext = new ControllerContext() { HttpContext = httpContextMock.Object },
        };
    }

    [Test]
    public async Task Provider_WhenSearchResultIsNotNullOrTotalAmountIsZero_ReturnsOkObjectResult()
    {
        // Arrange
        var searchResult = new SearchResult<ProviderChangesLogDto>()
        {
            TotalAmount = 1,
        };

        var request = new ProviderChangesLogRequest();

        changesLogServiceMock.Setup(x => x.GetProviderChangesLogAsync(request)).ReturnsAsync(searchResult);

        // Act
        var result = await controller.Provider(request);

        // Assert
        result.Should().NotBeNull();
        result.Should()
              .BeOfType<OkObjectResult>()
              .Which.StatusCode
              .Should()
              .Be(StatusCodes.Status200OK);
    }

    [Test]
    public async Task Provider_WhenSearchResultIsNullOrTotalAmountIsZero_ReturnsNoContentObjectResult()
    {
        // Arrange
        var searchResult = new SearchResult<ProviderChangesLogDto>()
        {
            TotalAmount = 0,
        };

        var request = new ProviderChangesLogRequest();

        changesLogServiceMock.Setup(x => x.GetProviderChangesLogAsync(request)).ReturnsAsync(searchResult);

        // Act
        var result = await controller.Provider(request);

        // Assert
        result.Should().NotBeNull();
        result.Should()
              .BeOfType<NoContentResult>()
              .Which.StatusCode
              .Should()
              .Be(StatusCodes.Status204NoContent);
    }

    [Test]
    public async Task Application_WhenSearchResultIsNotNullOrTotalAmountIsZero_ReturnsOkObjectResult()
    {
        // Arrange
        var searchResult = new SearchResult<ApplicationChangesLogDto>()
        {
            TotalAmount = 1,
        };

        var request = new ApplicationChangesLogRequest();

        changesLogServiceMock.Setup(x => x.GetApplicationChangesLogAsync(request)).ReturnsAsync(searchResult);

        // Act
        var result = await controller.Application(request);

        // Assert
        result.Should().NotBeNull();
        result.Should()
              .BeOfType<OkObjectResult>()
              .Which.StatusCode
              .Should()
              .Be(StatusCodes.Status200OK);
    }

    [Test]
    public async Task Application_WhenSearchResultIsNullOrTotalAmountIsZero_ReturnsNoContentObjectResult()
    {
        // Arrange
        var searchResult = new SearchResult<ApplicationChangesLogDto>()
        {
            TotalAmount = 0,
        };

        var request = new ApplicationChangesLogRequest();

        changesLogServiceMock.Setup(x => x.GetApplicationChangesLogAsync(request)).ReturnsAsync(searchResult);

        // Act
        var result = await controller.Application(request);

        // Assert
        result.Should().NotBeNull();
        result.Should()
              .BeOfType<NoContentResult>()
              .Which.StatusCode
              .Should()
              .Be(StatusCodes.Status204NoContent);
    }

    [Test]
    public async Task ProviderAdmin_WhenSearchResultIsNotNullOrTotalAmountIsZero_ReturnsOkObjectResult()
    {
        // Arrange
        var searchResult = new SearchResult<EmployeeChangesLogDto>()
        {
            TotalAmount = 1,
        };

        var request = new EmployeeChangesLogRequest();

        changesLogServiceMock.Setup(x => x.GetEmployeeChangesLogAsync(request)).ReturnsAsync(searchResult);

        // Act
        var result = await controller.Employee(request);

        // Assert
        result.Should().NotBeNull();
        result.Should()
              .BeOfType<OkObjectResult>()
              .Which.StatusCode
              .Should()
              .Be(StatusCodes.Status200OK);
    }

    [Test]
    public async Task ProviderAdmin_WhenSearchResultIsNotNullOrTotalAmountIsZero_ReturnsNoContentObjectResult()
    {
        // Arrange
        var searchResult = new SearchResult<EmployeeChangesLogDto>()
        {
            TotalAmount = 0,
        };

        var request = new EmployeeChangesLogRequest();

        changesLogServiceMock.Setup(x => x.GetEmployeeChangesLogAsync(request)).ReturnsAsync(searchResult);

        // Act
        var result = await controller.Employee(request);

        // Assert
        result.Should().NotBeNull();
        result.Should()
              .BeOfType<NoContentResult>()
              .Which.StatusCode
              .Should()
              .Be(StatusCodes.Status204NoContent);
    }

    #region ParentBlockedByAdmin

    [Test]
    public async Task ParentBlockedByAdmin_WhenRequestIsValid_ReturnsOkResult()
    {
        // Arrange
        ParentBlockedByAdminChangesLogRequest request = new()
        {
            ShowParents = ShowParents.All,
            SearchString = string.Empty,
            DateFrom = new DateTime(2023, 9, 6),
        };
        SearchResult<ParentBlockedByAdminChangesLogDto> changesLog = new()
        {
            TotalAmount = 2,
            Entities = new List<ParentBlockedByAdminChangesLogDto>
            {
                new(),
                new(),
            },
        };
        changesLogServiceMock.Setup(x => x.GetParentBlockedByAdminChangesLogAsync(It.IsAny<ParentBlockedByAdminChangesLogRequest>()))
            .Returns(Task.FromResult(changesLog));

        // Act
        var result = await controller.ParentBlockedByAdmin(request);

        // Assert
        Assert.NotNull(result);
        Assert.IsInstanceOf<OkObjectResult>(result);
    }

    [Test]
    public async Task ParentBlockedByAdmin_WhenNoDataFound_ReturnsNoContentResult()
    {
        // Arrange
        ParentBlockedByAdminChangesLogRequest request = new()
        {
            ShowParents = ShowParents.All,
            SearchString = string.Empty,
            DateFrom = new DateTime(2023, 9, 6),
        };
        SearchResult<ParentBlockedByAdminChangesLogDto> changesLog = new()
        {
            TotalAmount = 0,
            Entities = new List<ParentBlockedByAdminChangesLogDto> { },
        };
        changesLogServiceMock.Setup(x => x.GetParentBlockedByAdminChangesLogAsync(It.IsAny<ParentBlockedByAdminChangesLogRequest>()))
            .Returns(Task.FromResult(changesLog));

        // Act
        var result = await controller.ParentBlockedByAdmin(request);

        // Assert
        Assert.NotNull(result);
        Assert.IsInstanceOf<NoContentResult>(result);
    }
    #endregion
}