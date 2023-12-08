using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using OutOfSchool.WebApi.Controllers.V1;
using OutOfSchool.WebApi.Enums;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.Changes;
using OutOfSchool.WebApi.Services;

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