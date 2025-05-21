using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Common;
using OutOfSchool.WebApi.Extensions;

namespace OutOfSchool.WebApi.Tests.Extensions;

public class ToActionResultExtensionTests
{
    private TestController _controller;

    [SetUp]
    public void Setup()
    {
        _controller = new TestController();
    }

    [Test]
    public void ToActionResult_WhenResultSucceeded_ReturnsOkObjectResult()
    {
        // Arrange
        var expectedValue = "Test Value";
        var result = Result<string>.Success(expectedValue);

        // Act
        var actionResult = _controller.ToActionResult(result);

        // Assert
        Assert.IsInstanceOf<OkObjectResult>(actionResult);
        var okResult = (OkObjectResult)actionResult;
        Assert.AreEqual(200, okResult.StatusCode);
        Assert.AreEqual(expectedValue, okResult.Value);
    }

    [Test]
    public void ToActionResult_WhenResultFailedWithCode400_ReturnsBadRequestObjectResult()
    {
        // Arrange
        var errorDescription = "Bad Request Error";
        var error = new OperationError { Code = "400", Description = errorDescription };
        var result = Result<string>.Failed(error);

        // Act
        var actionResult = _controller.ToActionResult(result);

        // Assert
        Assert.IsInstanceOf<BadRequestObjectResult>(actionResult);
        var badRequestResult = (BadRequestObjectResult)actionResult;
        Assert.AreEqual(400, badRequestResult.StatusCode);
        Assert.AreEqual(errorDescription, badRequestResult.Value);
    }

    [Test]
    public void ToActionResult_WhenResultFailedWithCode403_ReturnsForbidResult()
    {
        // Arrange
        var error = new OperationError { Code = "403", Description = "Forbidden Error" };
        var result = Result<string>.Failed(error);

        // Act
        var actionResult = _controller.ToActionResult(result);

        // Assert
        Assert.IsInstanceOf<ForbidResult>(actionResult);
    }

    [Test]
    public void ToActionResult_WhenResultFailedWithCode404_ReturnsNotFoundObjectResult()
    {
        // Arrange
        var errorDescription = "Not Found Error";
        var error = new OperationError { Code = "404", Description = errorDescription };
        var result = Result<string>.Failed(error);

        // Act
        var actionResult = _controller.ToActionResult(result);

        // Assert
        Assert.IsInstanceOf<NotFoundObjectResult>(actionResult);
        var notFoundResult = (NotFoundObjectResult)actionResult;
        Assert.AreEqual(404, notFoundResult.StatusCode);
        Assert.AreEqual(errorDescription, notFoundResult.Value);
    }

    [Test]
    public void ToActionResult_WhenResultFailedWithCode409_ReturnsConflictObjectResult()
    {
        // Arrange
        var errorDescription = "Conflict Error";
        var error = new OperationError { Code = "409", Description = errorDescription };
        var result = Result<string>.Failed(error);

        // Act
        var actionResult = _controller.ToActionResult(result);

        // Assert
        Assert.IsInstanceOf<ConflictObjectResult>(actionResult);
        var conflictResult = (ConflictObjectResult)actionResult;
        Assert.AreEqual(409, conflictResult.StatusCode);
        Assert.AreEqual(errorDescription, conflictResult.Value);
    }

    [Test]
    public void ToActionResult_WhenResultFailedWithUnknownCode_ReturnsStatusCodeResult()
    {
        // Arrange
        var errorDescription = "Unknown Error";
        var error = new OperationError { Code = "999", Description = errorDescription };
        var result = Result<string>.Failed(error);

        // Act
        var actionResult = _controller.ToActionResult(result);

        // Assert
        Assert.IsInstanceOf<ObjectResult>(actionResult);
        var statusCodeResult = (ObjectResult)actionResult;
        Assert.AreEqual(500, statusCodeResult.StatusCode);
        Assert.AreEqual(errorDescription, statusCodeResult.Value);
    }

    [Test]
    public void ToActionResult_WhenResultFailedWithNoErrors_ReturnsStatusCodeResult()
    {
        // Arrange - Creating a result with an empty errors collection
        var operationResult = new OperationResult();
        operationResult.GetType().GetProperty("Succeeded").SetValue(operationResult, false);
        var result = new Result<string>();
        result.GetType().GetProperty("OperationResult").SetValue(result, operationResult);

        // Act
        var actionResult = _controller.ToActionResult(result);

        // Assert
        Assert.IsInstanceOf<ObjectResult>(actionResult);
        var statusCodeResult = (ObjectResult)actionResult;
        Assert.AreEqual(500, statusCodeResult.StatusCode);
        Assert.AreEqual("Unexpected error", statusCodeResult.Value);
    }

    [Test]
    public void ToActionResult_WhenResultFailedWithNullDescription_ReturnsStatusCodeResultWithDefaultMessage()
    {
        // Arrange
        var error = new OperationError { Code = "500", Description = null };
        var result = Result<string>.Failed(error);

        // Act
        var actionResult = _controller.ToActionResult(result);

        // Assert
        Assert.IsInstanceOf<ObjectResult>(actionResult);
        var statusCodeResult = (ObjectResult)actionResult;
        Assert.AreEqual(500, statusCodeResult.StatusCode);
        Assert.AreEqual("Unexpected error", statusCodeResult.Value);
    }

    [Test]
    public void ToActionResult_WithComplexObjectType_ReturnsOkObjectResultWithSameObject()
    {
        // Arrange
        var expectedValue = new TestModel { Id = 1, Name = "Test" };
        var result = Result<TestModel>.Success(expectedValue);

        // Act
        var actionResult = _controller.ToActionResult(result);

        // Assert
        Assert.IsInstanceOf<OkObjectResult>(actionResult);
        var okResult = (OkObjectResult)actionResult;
        Assert.AreEqual(expectedValue, okResult.Value);
    }

    [Test]
    public void ToActionResult_WithMultipleErrors_UsesFirstErrorForResponse()
    {
        // Arrange
        var firstError = new OperationError { Code = "404", Description = "First Error" };
        var secondError = new OperationError { Code = "400", Description = "Second Error" };
        var result = Result<string>.Failed(firstError, secondError);

        // Act
        var actionResult = _controller.ToActionResult(result);

        // Assert
        Assert.IsInstanceOf<NotFoundObjectResult>(actionResult);
        var notFoundResult = (NotFoundObjectResult)actionResult;
        Assert.AreEqual(404, notFoundResult.StatusCode);
        Assert.AreEqual("First Error", notFoundResult.Value);
    }

    private class TestController : ControllerBase { }

    private class TestModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
