using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using OutOfSchool.BusinessLogic.Services;
using OutOfSchool.WebApi.Filters;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace OutOfSchool.WebApi.Tests.Filters;
[TestFixture]
public class TechAdminAccessFilterTests
{
    private Mock<ICurrentUserService> currentUserServiceMock;
    private TechAdminAccessFilter filter;
    private EndpointFilterInvocationContext context;

    [SetUp]
    public void SetUp()
    {
        currentUserServiceMock = new Mock<ICurrentUserService>();
    }

    private static EndpointFilterInvocationContext CreateFakeContext()
    {
        return new DefaultEndpointFilterInvocationContext(new DefaultHttpContext(), Array.Empty<object>());
    }

    [Test]
    public async Task InvokeAsync_WhenUserIsTechAdmin_ShouldCallNext()
    {
        // Arrange
        currentUserServiceMock.Setup(s => s.IsTechAdmin()).Returns(true);
        filter = new TechAdminAccessFilter(currentUserServiceMock.Object);
        context = CreateFakeContext();

        var wasCalled = false;

        // Act
        var result = await filter.InvokeAsync(context, ctx =>
        {
            wasCalled = true;
            return ValueTask.FromResult<object?>(Results.Ok("Success"));
        });

        // Assert
        Assert.That(result, Is.InstanceOf<Ok<string>>());
        Assert.That(wasCalled, Is.True);
    }

    [Test]
    public async Task InvokeAsync_WhenUserIsNotTechAdmin_ShouldReturn403()
    {
        // Arrange
        currentUserServiceMock.Setup(s => s.IsTechAdmin()).Returns(false);
        filter = new TechAdminAccessFilter(currentUserServiceMock.Object);
        context = CreateFakeContext();

        // create a fake HttpContext and DI container
        var httpContext = new DefaultHttpContext();
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLogging();
        serviceCollection.AddMvc();
        httpContext.RequestServices = serviceCollection.BuildServiceProvider();

        var result = await filter.InvokeAsync(context, ctx => ValueTask.FromResult<object?>(null));

        // Act
        await (result as IResult)!.ExecuteAsync(httpContext);

        // Assert
        Assert.That(httpContext.Response.StatusCode, Is.EqualTo(StatusCodes.Status403Forbidden));
    }
}