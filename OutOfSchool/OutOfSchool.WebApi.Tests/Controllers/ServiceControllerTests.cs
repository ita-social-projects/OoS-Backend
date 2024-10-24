using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using OutOfSchool.WebApi.Controllers.V1;
using System.Linq;

namespace OutOfSchool.WebApi.Tests.Controllers;

[TestFixture]
public class ServiceControllerTests
{
    private ServiceController controller;

    [SetUp]
    public void SetUp()
    {
        controller = new ServiceController(); // initialize controller object

        // create and and initialize context
        var httpContext = new DefaultHttpContext();
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext,
        };
    }

    [Test]
    public void GetHealth_ShouldReturnNoContent()
    {
        // Act
        var result = controller?.GetHealth();

        // Assert
        Assert.IsInstanceOf<NoContentResult>(result);
    }

    [Test]
    public void GetHealth_ShouldSetExpiresHeaderToMinusOne()
    {
        // Arrange
        string expectedExpiresValue = "-1";

        // Act
        controller.GetHealth();
        string headerValue = controller.Response.Headers["Expires"];

        // Assert
        Assert.AreEqual(expectedExpiresValue, headerValue);
    }

    [Test]
    public void GetHealth_ShouldHaveNoCachingAttributes()
    {
        // Arrange
        var methodInfo = typeof(ServiceController).GetMethod(nameof(ServiceController.GetHealth));

        // Act
        var responseCacheAttribute = methodInfo.GetCustomAttributes(typeof(ResponseCacheAttribute), false)
                                               .FirstOrDefault() as ResponseCacheAttribute;

        // Assert
        Assert.IsNotNull(responseCacheAttribute);
        Assert.IsTrue(responseCacheAttribute.NoStore);
        Assert.AreEqual(ResponseCacheLocation.None, responseCacheAttribute.Location);
        Assert.AreEqual(-1, responseCacheAttribute.Duration);
    }
}
