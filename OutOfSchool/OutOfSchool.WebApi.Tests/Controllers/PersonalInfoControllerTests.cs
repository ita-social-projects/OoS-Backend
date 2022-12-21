using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using OutOfSchool.Services.Enums;
using OutOfSchool.WebApi.Controllers.V1;
using OutOfSchool.WebApi.Models.Workshop;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Tests.Controllers;

public class PersonalInfoControllerTests
{
    private Mock<IParentService> parentService;
    private Mock<IUserService> userService;
    private Mock<HttpContext> httpContext;

    [SetUp]
    public void Setup()
    {
        httpContext = new Mock<HttpContext>();
        parentService = new Mock<IParentService>();
        userService = new Mock<IUserService>();
    }

    #region UpdateParent

    [Test]
    public async Task UpdateParent_WhenModelIsValid_ReturnsOkObjectResult()
    {
        // Arrange
        var controller = SetupParentTests();
        var changedParent = new ParentPersonalInfo()
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

        // Act
        var result = await controller.UpdateParentPersonalInfo(changedParent).ConfigureAwait(false) as OkObjectResult;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.AreEqual(result.StatusCode, 200);
    }

    [Test]
    public void UpdateParent_WhenIdUserHasNoRights_ShouldReturn403ObjectResult()
    {
        // Arrange
        var controller = SetupParentTests();
        parentService.Setup(x => x.Update(It.IsAny<ParentPersonalInfo>()))
            .ThrowsAsync(new UnauthorizedAccessException());

        // Act & Assert
        Assert.ThrowsAsync<UnauthorizedAccessException>(() => controller.UpdateParentPersonalInfo(new ParentPersonalInfo()));
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
            parentService.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = httpContext.Object },
        };
    }
}