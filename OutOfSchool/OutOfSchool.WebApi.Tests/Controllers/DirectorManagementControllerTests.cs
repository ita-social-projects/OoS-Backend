using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Common;
using OutOfSchool.BusinessLogic.Models.Official;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.Common.Enums;
using OutOfSchool.WebApi.Controllers.V1;

namespace OutOfSchool.WebApi.Tests.Controllers;

[TestFixture]
class DirectorManagementControllerTests
{
    private Mock<IDirectorManagementService> _directorServiceMock;
    private Mock<ICurrentUserService> _currentUserServiceMock;
    private Mock<ILogger<DirectorManagementController>> _loggerMock;
    private DirectorManagementController _controller;

    [SetUp]
    public void SetUp()
    {
        _directorServiceMock = new Mock<IDirectorManagementService>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _loggerMock = new Mock<ILogger<DirectorManagementController>>();

        _controller = new DirectorManagementController(
            _directorServiceMock.Object,
            _currentUserServiceMock.Object,
            _loggerMock.Object);
    }

    [Test]
    public async Task Promote_ReturnsOk_WithCorrectResponseDto()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var officialId = Guid.NewGuid();
        var positionId = Guid.NewGuid();
        var activeFrom = DateOnly.FromDateTime(DateTime.UtcNow);
        var positionType = PositionType.Director;
        var fullName = "Petro Petrov";

        var request = new PromoteToDirectorRequestDto { OfficialId = officialId };

        var response = new PromoteToDirectorResponseDto
        {
            OfficialId = officialId,
            PositionId = positionId,
            ActiveFrom = activeFrom,
            PositionType = positionType,
            FullName = fullName
        };

        _directorServiceMock
            .Setup(s => s.PromoteEmployeeToDirector(providerId, request))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.Promote(providerId, request);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult, "Expected OkObjectResult");
        var actualResponse = okResult.Value as PromoteToDirectorResponseDto;
        Assert.IsNotNull(actualResponse, "Expected PromoteToDirectorResponseDto");

        Assert.AreEqual(response.OfficialId, actualResponse.OfficialId);
        Assert.AreEqual(response.PositionId, actualResponse.PositionId);
        Assert.AreEqual(response.ActiveFrom, actualResponse.ActiveFrom);
        Assert.AreEqual(response.PositionType, actualResponse.PositionType);
        Assert.AreEqual(response.FullName, actualResponse.FullName);
    }

    [Test]
    public async Task Promote_ReturnsForbid_WhenUnauthorizedAccessExceptionThrown()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var request = new PromoteToDirectorRequestDto { OfficialId = Guid.NewGuid() };

        _directorServiceMock
            .Setup(s => s.PromoteEmployeeToDirector(providerId, request))
            .ThrowsAsync(new UnauthorizedAccessException());

        // Act
        var result = await _controller.Promote(providerId, request);

        // Assert
        Assert.IsInstanceOf<ForbidResult>(result);
    }

    [Test]
    public async Task Promote_ReturnsBadRequest_WhenInvalidOperationExceptionThrown()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var request = new PromoteToDirectorRequestDto { OfficialId = Guid.NewGuid() };
        var errorMessage = "Business rule violated.";

        _directorServiceMock
            .Setup(s => s.PromoteEmployeeToDirector(providerId, request))
            .ThrowsAsync(new InvalidOperationException(errorMessage));

        // Act
        var result = await _controller.Promote(providerId, request);

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual(errorMessage, badRequestResult.Value);
    }

    [Test]
    public async Task Promote_ReturnsNotFound_WhenKeyNotFoundExceptionThrown()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var request = new PromoteToDirectorRequestDto { OfficialId = Guid.NewGuid() };
        var errorMessage = "Official not found.";

        _directorServiceMock
            .Setup(s => s.PromoteEmployeeToDirector(providerId, request))
            .ThrowsAsync(new KeyNotFoundException(errorMessage));

        // Act
        var result = await _controller.Promote(providerId, request);

        // Assert
        var notFoundResult = result as NotFoundObjectResult;
        Assert.IsNotNull(notFoundResult);
        Assert.AreEqual(errorMessage, notFoundResult.Value);
    }

    [Test]
    public async Task Transfer_ReturnsOk_WhenTransferSucceeds()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var request = new TransferDirectorRequestDto
        {
            FromOfficialId = Guid.NewGuid(),
            ToOfficialId = Guid.NewGuid()
        };
        var responseDto = new TransferDirectorResponseDto { ProviderId = providerId };
        var result = Result<TransferDirectorResponseDto>.Success(responseDto);
        _directorServiceMock
            .Setup(s => s.TransferDirectorPosition(providerId, request))
            .ReturnsAsync(result);

        // Act
        var resultAction = await _controller.Transfer(providerId, request);

        // Assert
        Assert.IsInstanceOf<OkObjectResult>(resultAction);
        var okResult = resultAction as OkObjectResult;
        Assert.AreEqual(responseDto, okResult.Value);
    }

    [Test]
    public async Task Transfer_ReturnsForbid_WhenWhenResultIndicatesUnauthorized()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var request = new TransferDirectorRequestDto
        {
            FromOfficialId = Guid.NewGuid(),
            ToOfficialId = Guid.NewGuid()
        };

        var failedResult = Result<TransferDirectorResponseDto>.Failed(new OperationError
        {
            Code = "Unauthorized",
            Description = "Only the current director can initiate a transfer."
        });

        _directorServiceMock
            .Setup(s => s.TransferDirectorPosition(providerId, request))
            .ReturnsAsync(failedResult);

        // Act
        var result = await _controller.Transfer(providerId, request);

        // Assert
        Assert.IsInstanceOf<ForbidResult>(result);
    }

    [Test]
    public async Task Transfer_ReturnsBadRequest_WhenInvalidOperation()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var request = new TransferDirectorRequestDto
        {
            FromOfficialId = Guid.NewGuid(),
            ToOfficialId = Guid.NewGuid()
        };

        var error = new OperationError
        {
            Code = "InvalidOperation",
            Description = "Invalid transfer operation."
        };

        var failedResult = Result<TransferDirectorResponseDto>.Failed(error);

        _directorServiceMock
            .Setup(s => s.TransferDirectorPosition(providerId, request))
             .ReturnsAsync(failedResult);

        // Act
        var result = await _controller.Transfer(providerId, request);

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        Assert.IsNotNull(badRequestResult);
        var errors = badRequestResult.Value as IEnumerable<OperationError>;
        Assert.IsNotNull(errors);
        Assert.That(errors.First().Description, Is.EqualTo(error.Description));
    }

    [Test]
    public async Task Transfer_ReturnsNotFound_WhenOfficialsNotFound()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var request = new TransferDirectorRequestDto
        {
            FromOfficialId = Guid.NewGuid(),
            ToOfficialId = Guid.NewGuid()
        };

        var error = new OperationError
        {
            Code = "OfficialsNotFound",
            Description = "One or both officials were not found."
        };

        var result = Result<TransferDirectorResponseDto>.Failed(error);

        _directorServiceMock
            .Setup(s => s.TransferDirectorPosition(providerId, request))
            .ReturnsAsync(result);


        // Act
        var resultAction = await _controller.Transfer(providerId, request);

        // Assert
        var notFoundResult = resultAction as NotFoundObjectResult;
        Assert.IsNotNull(notFoundResult);
        Assert.AreEqual(new List<OperationError> { error }, notFoundResult.Value);
    }
}
