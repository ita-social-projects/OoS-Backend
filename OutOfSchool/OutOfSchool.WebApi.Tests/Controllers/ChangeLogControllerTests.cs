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

    #region WorkshopDraft

    [Test]
    public async Task WorkshopDraft_ValidRequestWithResults_ReturnsOkWithSearchResult()
    {
        // Arrange
        var request = new WorkshopDraftChangesLogRequest();
        var expectedResult = new SearchResult<WorkshopDraftChangesLogDto>
        {
            TotalAmount = 2,
            Entities = new List<WorkshopDraftChangesLogDto>
            {
                new WorkshopDraftChangesLogDto
                {
                    WorkshopDraftId = Guid.NewGuid(),
                    WorkshopTitle = "Test Workshop",
                    ProviderTitle = "Test Provider"
                },
                new WorkshopDraftChangesLogDto
                {
                    WorkshopDraftId = Guid.NewGuid(),
                    WorkshopTitle = "Another Workshop",
                    ProviderTitle = "Another Provider"
                }
            }
        };

        changesLogServiceMock
            .Setup(s => s.GetWorkshopDraftChangesLogAsync(request))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await controller.WorkshopDraft(request);

        // Assert
        Assert.IsInstanceOf<OkObjectResult>(result);
        var okResult = (OkObjectResult)result;
        Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);

        var searchResult = (SearchResult<WorkshopDraftChangesLogDto>)okResult.Value;
        Assert.AreEqual(expectedResult.TotalAmount, searchResult.TotalAmount);
        Assert.AreEqual(expectedResult.Entities.Count, searchResult.Entities.Count);
    }

    [Test]
    public async Task WorkshopDraft_NoResults_ReturnsNoContent()
    {
        // Arrange
        var request = new WorkshopDraftChangesLogRequest();
        var emptyResult = new SearchResult<WorkshopDraftChangesLogDto>
        {
            TotalAmount = 0,
            Entities = new List<WorkshopDraftChangesLogDto>()
        };

        changesLogServiceMock
            .Setup(s => s.GetWorkshopDraftChangesLogAsync(request))
            .ReturnsAsync(emptyResult);

        // Act
        var result = await controller.WorkshopDraft(request);

        // Assert
        Assert.IsInstanceOf<NoContentResult>(result);
        var noContentResult = (NoContentResult)result;
        Assert.AreEqual(StatusCodes.Status204NoContent, noContentResult.StatusCode);
    }

    [Test]
    public async Task WorkshopDraft_NullResult_ReturnsNoContent()
    {
        // Arrange
        var request = new WorkshopDraftChangesLogRequest();
        SearchResult<WorkshopDraftChangesLogDto> nullResult = null;

        changesLogServiceMock
            .Setup(s => s.GetWorkshopDraftChangesLogAsync(request))
            .ReturnsAsync(nullResult);

        // Act
        var result = await controller.WorkshopDraft(request);

        // Assert
        Assert.IsInstanceOf<NoContentResult>(result);
        var noContentResult = (NoContentResult)result;
        Assert.AreEqual(StatusCodes.Status204NoContent, noContentResult.StatusCode);
    }

    [Test]
    public async Task WorkshopDraft_InvalidResult_ReturnsNoContent()
    {
        // Arrange
        var request = new WorkshopDraftChangesLogRequest();
        var invalidResult = new SearchResult<WorkshopDraftChangesLogDto>
        {
            TotalAmount = 1, // Total amount less than the actual count (2)
            Entities = new List<WorkshopDraftChangesLogDto>
            {
                new WorkshopDraftChangesLogDto(),
                new WorkshopDraftChangesLogDto()
            }
        };

        changesLogServiceMock
            .Setup(s => s.GetWorkshopDraftChangesLogAsync(request))
            .ReturnsAsync(invalidResult);

        // Act
        var result = await controller.WorkshopDraft(request);

        // Assert
        Assert.IsInstanceOf<NoContentResult>(result);
        var noContentResult = (NoContentResult)result;
        Assert.AreEqual(StatusCodes.Status204NoContent, noContentResult.StatusCode);
    }

    [Test]
    public void WorkshopDraft_ExceptionThrown_ThrowsException()
    {
        // Arrange
        var request = new WorkshopDraftChangesLogRequest();
        var expectedException = new InvalidOperationException("Test exception");

        changesLogServiceMock
            .Setup(s => s.GetWorkshopDraftChangesLogAsync(request))
            .ThrowsAsync(expectedException);

        // Act & Assert
        var exception = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await controller.WorkshopDraft(request));

        Assert.AreEqual(expectedException.Message, exception.Message);
    } 

    #endregion
}