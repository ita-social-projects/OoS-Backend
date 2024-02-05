using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using OutOfSchool.WebApi.Controllers.V1;
using OutOfSchool.WebApi.Services;

namespace OutOfSchool.WebApi.Tests.Controllers;

[TestFixture]
public class NotificationControllerTests
{
    private NotificationController controller;
    private Mock<INotificationService> serviceMock;
    private Mock<HttpContext> httpContext;
    private string userId;

    [SetUp]
    public void SetUp()
    {
        serviceMock = new Mock<INotificationService>();
        userId = Guid.NewGuid().ToString();
        httpContext = new Mock<HttpContext>();
        httpContext.Setup(c => c.User.FindFirst("sub"))
            .Returns(new Claim(ClaimTypes.NameIdentifier, userId));

        controller = new NotificationController(serviceMock.Object)
        {
            ControllerContext = new ControllerContext() { HttpContext = httpContext.Object },
        };
    }

    [Test]
    public async Task ReadAll_WhenCalledByUser_ShouldReturnOk()
    {
        // Act
        var result = await controller.ReadAll();

        // Assert
        Assert.That(result, Is.InstanceOf<OkResult>());
    }
}
