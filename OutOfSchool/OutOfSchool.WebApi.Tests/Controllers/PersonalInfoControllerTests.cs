using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Models;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.Services.Enums;
using OutOfSchool.WebApi.Controllers.V1;

namespace OutOfSchool.WebApi.Tests.Controllers;

public class PersonalInfoControllerTests
{
    private Mock<IParentService> parentService;
    private Mock<IUserService> userService;
    private Mock<ICurrentUserService> currentUserService;
    private Mock<HttpContext> httpContext;

    [SetUp]
    public void Setup()
    {
        httpContext = new Mock<HttpContext>();
        parentService = new Mock<IParentService>();
        userService = new Mock<IUserService>();
        currentUserService = new Mock<ICurrentUserService>();
    }

    #region GetPersonalInfo

    [Test]
    public async Task GetPersonalInfo_WhenUserIsParent_ReturnsOkObjectResult()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();

        httpContext.Setup(x => x.User.FindFirst("sub"))
            .Returns(new Claim(ClaimTypes.NameIdentifier, userId));

        httpContext.Setup(x => x.User.IsInRole("parent"))
            .Returns(true);

        var controller = new PersonalInfoController(
            userService.Object,
            parentService.Object,
            currentUserService.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext.Object },
        };

        parentService.Setup(x => x.GetPersonalInfoByUserId(userId)).ReturnsAsync(new ShortUserDto());
        currentUserService.Setup(c => c.UserId).Returns(userId);
        currentUserService.Setup(c => c.IsInRole(Role.Parent)).Returns(true);

        // Act
        var result = await controller.GetPersonalInfo().ConfigureAwait(false) as OkObjectResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.AreEqual(StatusCodes.Status200OK, result.StatusCode);
    }

    [Test]
    public async Task GetPersonalInfo_WhenUserIsNotParent_ReturnsOkObjectResult()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();

        var controller = new PersonalInfoController(
            userService.Object,
            parentService.Object,
            currentUserService.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext.Object },
        };

        userService.Setup(x => x.GetById(userId)).ReturnsAsync(new ShortUserDto());
        currentUserService.Setup(c => c.UserId).Returns(userId);
        currentUserService.Setup(c => c.IsInRole(Role.Parent)).Returns(false);
        currentUserService.Setup(c => c.UserRole).Returns("provider");

        // Act
        var result = await controller.GetPersonalInfo().ConfigureAwait(false) as OkObjectResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.AreEqual(StatusCodes.Status200OK, result.StatusCode);
    }

    #endregion

    #region UpdateParent

    [Test]
    public async Task UpdateParent_WhenModelIsValid_ReturnsOkObjectResult()
    {
        // Arrange
        var controller = SetupParentTests();
        var changedParent = new ShortUserDto()
        {
            Id = "38776161-734b-4aec-96eb-4a1f87a2e5f3",
            PhoneNumber = "1160327456",
            LastName = "LastName",
            MiddleName = "MiddleName",
            FirstName = "FirstName",
            Gender = Gender.Male,
            DateOfBirth = DateTime.Today,
        };
        parentService.Setup(x => x.Update(changedParent)).ReturnsAsync(changedParent);
        currentUserService.Setup(c => c.UserId).Returns("38776161-734b-4aec-96eb-4a1f87a2e5f3");
        currentUserService.Setup(c => c.IsInRole(Role.Parent)).Returns(true);

        // Act
        var result = await controller.UpdatePersonalInfo(changedParent).ConfigureAwait(false) as OkObjectResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.AreEqual(result.StatusCode, 200);
    }

    [Test]
    public void UpdateParent_WhenIdUserHasNoRights_ShouldReturn403ObjectResult()
    {
        // Arrange
        var controller = SetupParentTests();
        parentService.Setup(x => x.Update(It.IsAny<ShortUserDto>()))
            .ThrowsAsync(new UnauthorizedAccessException());
        currentUserService.Setup(c => c.UserId).Returns("38776161-734b-4aec-96eb-4a1f87a2e5f3");
        currentUserService.Setup(c => c.IsInRole(Role.Parent)).Returns(true);

        // Act & Assert
        Assert.ThrowsAsync<UnauthorizedAccessException>(() => controller.UpdatePersonalInfo(new ShortUserDto()));
    }

    #endregion

    private PersonalInfoController SetupParentTests()
    {
        httpContext.Setup(x => x.User.FindFirst("sub"))
            .Returns(new Claim(ClaimTypes.NameIdentifier, "38776161-734b-4aec-96eb-4a1f87a2e5f3"));
        httpContext.Setup(x => x.User.IsInRole("parent"))
            .Returns(true);

        return new PersonalInfoController(
            userService.Object,
            parentService.Object,
            currentUserService.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext.Object },
        };
    }
}