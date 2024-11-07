#nullable enable

using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Language.Flow;
using Moq.Protected;
using NUnit.Framework;
using OutOfSchool.Common.Communication;
using OutOfSchool.Common.Communication.ICommunication;
using OutOfSchool.Common.Config;
using OutOfSchool.Common.Models;
using OutOfSchool.Tests.Common;

namespace OutOfSchool.WebApi.Tests.Common;

[TestFixture]
public class CommunicationServiceTest
{
    private Mock<IOptions<CommunicationConfig>> communicationOptions;
    private Mock<IHttpClientFactory> httpClientFactory;
    private Mock<ILogger<CommunicationService>> logger;
    private Mock<HttpMessageHandler> handler;
    private HttpClient client;
    private readonly Uri uri = new("https://example.com");
    private CommunicationService communicationService;

    [SetUp]
    public void SetUp()
    {
        handler = new Mock<HttpMessageHandler>();
        client = new HttpClient(handler.Object);
        var communicationConfig = new CommunicationConfig
        {
            ClientName = "test",
            MaxNumberOfRetries = 1,
            TimeoutInSeconds = 1,
        };
        communicationOptions = new Mock<IOptions<CommunicationConfig>>();
        communicationOptions.Setup(x => x.Value).Returns(communicationConfig);
        httpClientFactory = new Mock<IHttpClientFactory>();
        httpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(client);
        logger = new Mock<ILogger<CommunicationService>>();
        communicationService =
            new CommunicationService(httpClientFactory.Object, communicationOptions.Object, logger.Object);
    }

    [Test]
    public async Task SendRequest_WithCorrectRequest_ReturnsOkResponse()
    {
        // Arrange
        var request = new Request
        {
            HttpMethodType = HttpMethodType.Post,
            Data = new TestRequestData("test"),
            Url = uri,
        };
        var response = new TestResponse("OK");
        var setup = SetupSendAsync(handler, HttpMethod.Post, uri.ToString());
        ReturnsHttpResponseAsync(setup, response, HttpStatusCode.OK);

        // Act
        var result = await communicationService.SendRequest<TestResponse, ErrorResponse>(request);

        result.AssertRight(r => { Assert.AreEqual("OK", r.Content); });
    }

    [Test]
    public async Task SendRequest_WithEmptyRequest_ReturnsErrorResponse()
    {
        // Arrange
        var setup = SetupSendAsync(handler, HttpMethod.Get, uri.ToString());
        ReturnsHttpResponseAsync(setup, null, HttpStatusCode.OK);

        // Act
        var result = await communicationService.SendRequest<TestResponse, ErrorResponse>(null);

        result.AssertLeft(error => Assert.AreEqual(HttpStatusCode.BadRequest, error.HttpStatusCode));
    }

    [Test]
    public async Task SendRequest_WithServerError_ReturnsErrorResponse()
    {
        // Arrange
        var request = new Request
        {
            HttpMethodType = HttpMethodType.Get,
            Url = uri,
        };
        var setup = SetupSendAsync(handler, HttpMethod.Get, uri.ToString());
        ReturnsHttpResponseAsync(setup, null, HttpStatusCode.Unauthorized);

        // Act
        var result = await communicationService.SendRequest<TestResponse, ErrorResponse>(request);

        result.AssertLeft(error => Assert.AreEqual(HttpStatusCode.Unauthorized, error.HttpStatusCode));
    }

    [Test]
    public void SendRequest_WithServerErrorAndWrongHandler_ThrowsException()
    {
        // Arrange
        var request = new Request
        {
            HttpMethodType = HttpMethodType.Get,
            Url = uri,
        };
        var setup = SetupSendAsync(handler, HttpMethod.Get, uri.ToString());
        ReturnsHttpResponseAsync(setup, null, HttpStatusCode.Unauthorized);

        // Act
        Assert.ThrowsAsync<InvalidOperationException>(() =>
            communicationService.SendRequest<TestResponse, TestError>(request));
    }

    [Test]
    public async Task SendRequest_WithServerErrorAndCustomHandler_ReturnsErrorResponse()
    {
        // Arrange
        var request = new Request
        {
            HttpMethodType = HttpMethodType.Get,
            Url = uri,
        };
        var setup = SetupSendAsync(handler, HttpMethod.Get, uri.ToString());
        ReturnsHttpResponseAsync(setup, null, HttpStatusCode.Unauthorized);

        // Act
        var result = await communicationService.SendRequest<TestResponse, TestError>(request, new TestErrorHandler());

        result.AssertLeft(error =>
        {
            Assert.IsInstanceOf<TestError>(error);
            Assert.AreEqual(HttpStatusCode.Unauthorized, error.HttpStatusCode);
        });
    }

    [Test]
    public async Task SendRequest_WithHttpException_ReturnsErrorResponse()
    {
        // Arrange
        var request = new Request
        {
            HttpMethodType = HttpMethodType.Get,
            Token = "secret",
            Url = uri,
        };
        var setup = SetupSendAsync(handler, HttpMethod.Get, uri.ToString());
        setup.Throws(new HttpRequestException(null, null, HttpStatusCode.InsufficientStorage));

        // Act
        var result = await communicationService.SendRequest<TestResponse, ErrorResponse>(request);

        result.AssertLeft(error =>
        {
            Assert.IsInstanceOf<ErrorResponse>(error);
            Assert.AreEqual(HttpStatusCode.InsufficientStorage, error.HttpStatusCode);
        });
    }

    private static ISetup<HttpMessageHandler, Task<HttpResponseMessage>> SetupSendAsync(
        Mock<HttpMessageHandler> handler, HttpMethod requestMethod, string requestUrl)
    {
        return handler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
            ItExpr.Is<HttpRequestMessage>(r =>
                r.Method == requestMethod &&
                r.RequestUri != null &&
                r.RequestUri.ToString() == requestUrl),
            ItExpr.IsAny<CancellationToken>());
    }

    private static IReturnsResult<HttpMessageHandler> ReturnsHttpResponseAsync(
        ISetup<HttpMessageHandler, Task<HttpResponseMessage>> moqSetup,
        object? responseBody,
        HttpStatusCode responseCode)
    {
        var serializedResponse = JsonSerializer.Serialize(responseBody);
        var stringContent = new StringContent(serializedResponse ?? string.Empty);

        var responseMessage = new HttpResponseMessage
        {
            StatusCode = responseCode,
            Content = stringContent,
        };

        return moqSetup.ReturnsAsync(responseMessage);
    }

    private record TestRequestData(string? Content);

    private record TestResponse(string? Content) : IResponse;

    private record TestError(HttpStatusCode HttpStatusCode, string? Message = null, string? Content = null)
        : IErrorResponse;

    private class TestErrorHandler : IErrorHandler<TestError>
    {
        public Task<TestError> HandleErrorAsync(CommunicationError errorResponse, string? message = null)
        {
            return Task.FromResult(new TestError(errorResponse.HttpStatusCode));
        }
    }
}