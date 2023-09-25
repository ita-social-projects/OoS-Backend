using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using OutOfSchool.Services.Enums;
using OutOfSchool.WebApi.Controllers.V1;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.Workshops;
using OutOfSchool.WebApi.Services;

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