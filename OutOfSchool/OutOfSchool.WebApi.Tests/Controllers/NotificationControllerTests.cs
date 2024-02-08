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

    [SetUp]
    public void SetUp()
    {
        var userId = Guid.NewGuid().ToString();
        var httpContext = new Mock<HttpContext>();
        httpContext.Setup(c => c.User.FindFirst("sub"))
            .Returns(new Claim(ClaimTypes.NameIdentifier, userId));

        controller = new NotificationController(new Mock<INotificationService>().Object)
        {
            ControllerContext = new ControllerContext() { HttpContext = httpContext.Object },
        };
    }

    [Test]
    public async Task ReadAll_WhenCalledByUser_ShouldReturnOk()
    {
        // Act
        var result = await controller.ReadAll().ConfigureAwait(false);

        // Assert
        Assert.That(result, Is.InstanceOf<OkResult>());
    }
}
