using System.Collections.Generic;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;
using Moq;
using NUnit.Framework;
using OutOfSchool.AuthorizationServer.Util;

namespace OutOfSchool.AuthServer.Tests.Util;

[TestFixture]
public class FormValueRequiredAttributeTests
{
    private Mock<HttpContext> routeContextMock;
    private FormValueRequiredAttribute formValueRequiredAttribute;

    [SetUp]
    public void SetUp()
    {
        routeContextMock = new Mock<HttpContext>();
        formValueRequiredAttribute = new FormValueRequiredAttribute("example");
    }

    [Test]
    public void IsValidForRequest_WhenUnsupportedRequestType_ShouldReturnFalse()
    {
        // Arrange
        routeContextMock.Setup(c => c.Request.Method).Returns("GET");

        // Act
        var result =
            formValueRequiredAttribute.IsValidForRequest(new RouteContext(routeContextMock.Object),
                new ActionDescriptor());

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public void IsValidForRequest_WhenCorrectRequestButNoForm_ShouldReturnFalse()
    {
        // Arrange
        routeContextMock.Setup(c => c.Request.Method).Returns("POST");
        routeContextMock.Setup(c => c.Request.ContentType).Returns("application/x-www-form-urlencoded");
        routeContextMock.Setup(c => c.Request.Form).Returns(new FormCollection(null));

        // Act
        var result =
            formValueRequiredAttribute.IsValidForRequest(new RouteContext(routeContextMock.Object),
                new ActionDescriptor());

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public void IsValidForRequest_WhenCorrectRequestAndForm_ShouldReturnTrue()
    {
        // Arrange
        routeContextMock.Setup(c => c.Request.Method).Returns("POST");
        routeContextMock.Setup(c => c.Request.ContentType).Returns("application/x-www-form-urlencoded");
        routeContextMock.Setup(c => c.Request.Form)
            .Returns(new FormCollection(
                new Dictionary<string, StringValues>
                {
                    {"example", new StringValues("example")}
                }));

        // Act
        var result =
            formValueRequiredAttribute.IsValidForRequest(new RouteContext(routeContextMock.Object),
                new ActionDescriptor());

        // Assert
        result.Should().BeTrue();
    }
}