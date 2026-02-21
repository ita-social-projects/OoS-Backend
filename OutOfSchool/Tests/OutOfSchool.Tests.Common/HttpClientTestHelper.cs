#nullable enable

using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Language.Flow;
using Moq.Protected;

namespace OutOfSchool.Tests.Common;

public static class HttpClientTestHelper
{
    public static ISetup<HttpMessageHandler, Task<HttpResponseMessage>> SetupSendAsync(
        this Mock<HttpMessageHandler> handler, HttpMethod requestMethod, string requestUrl, bool contains = false)
    {
        return handler.Protected().Setup<Task<HttpResponseMessage>>("SendAsync",
            ItExpr.Is<HttpRequestMessage>(r =>
                r.Method == requestMethod &&
                r.RequestUri != null &&
                contains ? r.RequestUri.ToString().Contains(requestUrl) : r.RequestUri.ToString() == requestUrl),
            ItExpr.IsAny<CancellationToken>());
    }

    public static IReturnsResult<HttpMessageHandler> ReturnsHttpResponseAsync(
        this ISetup<HttpMessageHandler, Task<HttpResponseMessage>> moqSetup,
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
}