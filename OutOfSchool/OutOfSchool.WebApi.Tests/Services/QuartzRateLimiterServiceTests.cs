using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OutOfSchool.WebApi.RateLimiting;
using System.IO;
using Microsoft.Extensions.Options;
using System.Net;
using System.Text.Json;
using System.Threading.RateLimiting;
using System.Threading;
using System.Threading.Tasks;
using RateLimiterOptions = OutOfSchool.WebApi.Config.RateLimiterOptions;

namespace OutOfSchool.WebApi.Tests.Services;
[TestFixture]
public class QuartzRateLimiterServiceTests
{
    private QuartzRateLimiterService service;
    private RateLimiterOptions options;
    private Mock<ILogger<QuartzRateLimiterService>> loggerMock;

    [SetUp]
    public void SetUp()
    {
        options = new RateLimiterOptions
        {
            PermitLimit = 2,
            QueueLimit = 5,
            Window = 10,
            RetryAfterSeconds = 30,
            EnableIpBasedRateLimiting = true
        };

        loggerMock = new Mock<ILogger<QuartzRateLimiterService>>();
        var optionsWrapper = Options.Create(options);

        service = new QuartzRateLimiterService(optionsWrapper, loggerMock.Object);
    }

    [Test]
    public void GetPolicy_WhenCalledWithIpBasedLimiting_ShouldReturnPolicyWithIpPartition()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = IPAddress.Parse("192.168.0.1");

        // Act
        var partition = service.GetPolicy(context);

        // Assert
        var limiter = partition.Factory("192.168.0.1");
        Assert.That(limiter, Is.TypeOf<FixedWindowRateLimiter>());
    }

    [Test]
    public void GetPolicy_WhenIpBasedLimitingDisabled_ShouldUseUserName()
    {
        // Arrange
        options.EnableIpBasedRateLimiting = false;
        var context = new DefaultHttpContext();
        context.User = new System.Security.Claims.ClaimsPrincipal(
            new System.Security.Claims.ClaimsIdentity(
                new[] { new System.Security.Claims.Claim("name", "testuser") }, "TestAuth"));

        // Act
        var partition = service.GetPolicy(context);

        // Assert
        var limiter = partition.Factory("testuser");
        Assert.That(limiter, Is.TypeOf<FixedWindowRateLimiter>());
    }

    [Test]
    public async Task HandleRejection_ShouldSet429AndRetryAfterHeaderAndReturnJson()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        var responseStream = new MemoryStream();
        httpContext.Response.Body = responseStream;

        var context = new OnRejectedContext
        {
            HttpContext = httpContext,
            Lease = null!
        };

        // Act
        await service.HandleRejection(context, CancellationToken.None);

        // Assert
        Assert.That(httpContext.Response.StatusCode, Is.EqualTo(StatusCodes.Status429TooManyRequests));
        Assert.That(httpContext.Response.Headers["Retry-After"], Is.Not.Empty);

        // check json response
        responseStream.Seek(0, SeekOrigin.Begin);

        var responseBody = await new StreamReader(responseStream).ReadToEndAsync();

        var json = JsonSerializer.Deserialize<JsonElement>(responseBody);

        // check if the JSON contains the expected properties
        Assert.That(json.TryGetProperty("error", out var errorProp), Is.True, "JSON does not 'Error'");
        Assert.That(errorProp.GetString(), Does.Contain("Rate limit exceeded"));

        Assert.That(json.GetProperty("retryAfter").GetInt32(), Is.EqualTo(options.RetryAfterSeconds));
    }
}