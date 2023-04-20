using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Moq;
using NUnit.Framework;
using OutOfSchool.Services.Enums;
using OutOfSchool.Tests.Common;
using OutOfSchool.Tests.Common.TestDataGenerators;
using OutOfSchool.WebApi.Controllers.V1;
using OutOfSchool.WebApi.Models;
using OutOfSchool.WebApi.Models.Application;
using OutOfSchool.WebApi.Models.Workshop;
using OutOfSchool.WebApi.Services;
using OutOfSchool.WebApi.Util;

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

        controller = new ParentController(
            serviceParent.Object)
        {
            ControllerContext = new ControllerContext() { HttpContext = httpContextMoq.Object },
        };
    }

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
}