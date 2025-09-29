using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using OutOfSchool.Common;
using OutOfSchool.Common.Models;
using System.Threading.Tasks;

namespace OutOfSchool.WebApi.Tests.Services;
[TestFixture]
public class ContextAwareUserServiceTests
{
    private Mock<IHttpContextAccessor> mockedHttpContextAccessor;
    private ContextAwareCurrentUser contextAwareUserService;

    [SetUp]
    public void SetUp()
    {
        mockedHttpContextAccessor = new Mock<IHttpContextAccessor>();
        contextAwareUserService = new ContextAwareCurrentUser(mockedHttpContextAccessor.Object);
    }

    [Test]
    public async Task UserId_WhenCalledWithoutHttpContext_ShouldReturnSystemUserId()
    {
        // Arrange
        mockedHttpContextAccessor.Setup(a => a.HttpContext).Returns((HttpContext?)null);

        // Act
        var userId = contextAwareUserService.UserId;

        // Assert
        Assert.That(userId, Is.EqualTo(Constants.SystemUserConstants.SystemUserId));
    }

    [Test]
    public async Task UserId_WhenCalledWithHttpContext_ShouldReturnWebUserId()
    {
        // Arrange
        var userId = "some-user-id";
        var mockedHttpContext = new DefaultHttpContext();
        mockedHttpContext.User = new System.Security.Claims.ClaimsPrincipal(
            new System.Security.Claims.ClaimsIdentity(new[]
            {
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, userId)
            }, "TestAuthentication"));
        mockedHttpContextAccessor.Setup(a => a.HttpContext).Returns(mockedHttpContext);

        // Act
        var resultUserId = contextAwareUserService.UserId;

        // Assert
        Assert.That(resultUserId, Is.EqualTo(userId));
    }
}
