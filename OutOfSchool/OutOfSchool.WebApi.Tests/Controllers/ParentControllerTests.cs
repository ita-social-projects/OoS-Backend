using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Common;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Models.Parent;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.WebApi.Controllers.V1;

namespace OutOfSchool.WebApi.Tests.Controllers;

[TestFixture]
public class ParentControllerTests
{
    private ParentController controller;
    private Mock<IParentService> serviceParent;
    private Mock<HttpContext> httpContextMoq;

    [SetUp]
    public void Setup()
    {
        serviceParent = new Mock<IParentService>();

        httpContextMoq = new Mock<HttpContext>();
        httpContextMoq.Setup(x => x.User.FindFirst("sub"))
            .Returns(new Claim(ClaimTypes.NameIdentifier, "38776161-734b-4aec-96eb-4a1f87a2e5f3"));
        httpContextMoq.Setup(x => x.User.IsInRole("parent"))
            .Returns(true);

        controller = new ParentController(serviceParent.Object)
        {
            ControllerContext = new ControllerContext() { HttpContext = httpContextMoq.Object },
        };
    }

    #region Create

    [Test]
    public async Task Create_WhenServiceReturnsDto_ReturnsCreatedAtActionResult()
    {
        // Arrange
        var parentDto = new ParentDTO()
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid().ToString(),
        };

        serviceParent
            .Setup(x => x.Create(It.IsAny<ParentCreateDto>()))
            .ReturnsAsync(parentDto);

        // Act
        var result = await controller.Create(new ParentCreateDto()) as CreatedAtActionResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.AreEqual(StatusCodes.Status201Created, result.StatusCode);
    }

    [Test]
    public async Task Create_WhenServiceThrowsInvalidOperationException_ReturnsBadRequestObjectResult()
    {
        // Arrange
        serviceParent
            .Setup(x => x.Create(It.IsAny<ParentCreateDto>()))
            .ThrowsAsync(new InvalidOperationException());

        // Act
        var result = await controller.Create(new ParentCreateDto()) as BadRequestObjectResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.AreEqual(StatusCodes.Status400BadRequest, result.StatusCode);
    }

    #endregion

    #region DeleteParent

    [Test]
    public async Task DeleteParent_WhenIdIsValid_ReturnsNoContentResult()
    {
        // Arrange
        serviceParent.Setup(x => x.Delete(It.IsAny<Guid>())).Returns(Task.CompletedTask);

        // Act
        var result = await controller.Delete(Guid.NewGuid()) as NoContentResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.AreEqual(result.StatusCode, 204);
    }

    [Test]
    public void DeleteParent_WhenParentHasNoRights_ShouldReturn403ObjectResult()
    {
        // Arrange
        serviceParent.Setup(x => x.Delete(It.IsAny<Guid>())).ThrowsAsync(new UnauthorizedAccessException());

        // Act & Assert
        Assert.ThrowsAsync<UnauthorizedAccessException>(() => controller.Delete(Guid.NewGuid()));
    }
    #endregion

    #region BlockUnblockParent
    [Test]
    public async Task BlockUnblockParent_ValidDto_ReturnsOkResult()
    {
        // Arrange
        BlockUnblockParentDto blockUnblockParentDto = new()
        {
            ParentId = Guid.NewGuid(),
            IsBlocked = true,
            Reason = "Reason for block",
        };
        serviceParent.Setup(x => x.BlockUnblockParent(It.IsAny<BlockUnblockParentDto>())).ReturnsAsync(Result<bool>.Success(true));

        // Act
        var result = await controller.BlockUnblockParent(blockUnblockParentDto);

        // Assert
        Assert.IsInstanceOf<OkResult>(result);
    }

    [Test]
    public async Task BlockUnblockParent_InvalidDto_ReturnsBadRequestResult()
    {
        // Arrange
        var reasonInvalidLength = 501;
        BlockUnblockParentDto blockUnblockParentDto = new()
        {
            ParentId = Guid.NewGuid(),
            IsBlocked = true,
            Reason = new string('X', reasonInvalidLength),
        };
        serviceParent.Setup(x => x.BlockUnblockParent(It.IsAny<BlockUnblockParentDto>()))
            .ReturnsAsync(Result<bool>.Failed(new OperationError
            { Code = StatusCodes.Status400BadRequest.ToString() }));

        // Act
        var result = await controller.BlockUnblockParent(blockUnblockParentDto);

        // Assert
        Assert.IsInstanceOf<BadRequestObjectResult>(result);
    }
    #endregion
}