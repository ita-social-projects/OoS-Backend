﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Moq;
using NUnit.Framework;
using OutOfSchool.WebApi.Middlewares;

namespace OutOfSchool.WebApi.Tests.Middlewares;

[TestFixture]
public class AuthorizationTokenMiddlewareTests
{
    private Mock<HttpContext> httpContextMoq;
    private HttpContext context;

    private Mock<RequestDelegate> requestDelegate;

    [SetUp]
    public void SetUp()
    {
        httpContextMoq = new Mock<HttpContext>();
        context = httpContextMoq.Object;

        requestDelegate = new Mock<RequestDelegate>();
        requestDelegate.Setup(x => x.Invoke(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);
    }

    [Test]
    public void ThrowsException_WhenParameterContextIsNull()
    {
        // Arrange
        var middleware = new AuthorizationTokenMiddleware(requestDelegate.Object);

        // Act and Assert
        Assert.ThrowsAsync<ArgumentNullException>(async () => await middleware.InvokeAsync(default));
    }

    [TestCase("")]
    [TestCase("chat")]
    [TestCase("provider")]
    public async Task DoesNotSetJWTtoHeader_WhenPathIsIncorrect(string incorectSegmentPath)
    {
        // Arrange
        var headers = new Dictionary<string, StringValues>();
        httpContextMoq.Setup(x => x.Request.Headers)
            .Returns(new HeaderDictionary(headers));

        var path = new PathString("/" + incorectSegmentPath);
        httpContextMoq.Setup(x => x.Request.Path)
            .Returns(new PathString(path));

        var store = new Dictionary<string, StringValues>();
        store.Add("access_token", "token");
        var query = new QueryCollection(store);
        httpContextMoq.Setup(x => x.Request.Query)
            .Returns(new QueryCollection(query));

        // Act
        var middleware = new AuthorizationTokenMiddleware(requestDelegate.Object);
        await middleware.InvokeAsync(context);

        // Assert
        Assert.IsFalse(context.Request.Headers.ContainsKey("Authorization"));
        requestDelegate.Verify(x => x.Invoke(context), Times.Once);
    }

    [TestCase("")]
    [TestCase("token")]
    [TestCase("access_tokan")]
    public async Task DoesNotSetJWTtoHeader_WhenQueryIsIncorrect(string wrongQueryParamName)
    {
        // Arrange
        var headers = new Dictionary<string, StringValues>();
        httpContextMoq.Setup(x => x.Request.Headers)
            .Returns(new HeaderDictionary(headers));

        var path = new PathString("/chathub");
        httpContextMoq.Setup(x => x.Request.Path)
            .Returns(new PathString(path));

        var store = new Dictionary<string, StringValues>();
        store.Add(wrongQueryParamName, "token");
        var query = new QueryCollection(store);
        httpContextMoq.Setup(x => x.Request.Query)
            .Returns(new QueryCollection(query));

        // Act
        var middleware = new AuthorizationTokenMiddleware(requestDelegate.Object);
        await middleware.InvokeAsync(context);

        // Assert
        Assert.IsFalse(context.Request.Headers.ContainsKey("Authorization"));
        requestDelegate.Verify(x => x.Invoke(context), Times.Once);
    }

    [TestCase("token")]
    [TestCase("enJdf34_56")]
    public async Task SetsJWTtoHeader_WhenQueryIsCorrect(string tokenValue)
    {
        // Arrange
        var headers = new Dictionary<string, StringValues>();
        httpContextMoq.Setup(x => x.Request.Headers)
            .Returns(new HeaderDictionary(headers));

        var path = new PathString("/chathub/workshop");
        httpContextMoq.Setup(x => x.Request.Path)
            .Returns(new PathString(path));

        var store = new Dictionary<string, StringValues>();
        store.Add("access_token", tokenValue);
        var query = new QueryCollection(store);
        httpContextMoq.Setup(x => x.Request.Query)
            .Returns(new QueryCollection(query));

        // Act
        var middleware = new AuthorizationTokenMiddleware(requestDelegate.Object);
        await middleware.InvokeAsync(context);

        // Assert
        Assert.IsTrue(context.Request.Headers.ContainsKey("Authorization"));
        context.Request.Headers.TryGetValue("Authorization", out StringValues value);
        Assert.AreEqual($"Bearer {tokenValue}", value.ToString());
        requestDelegate.Verify(x => x.Invoke(context), Times.Once);
    }
}