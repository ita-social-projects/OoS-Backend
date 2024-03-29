﻿namespace OutOfSchool.WebApi.Middlewares;

public class AuthorizationTokenMiddleware
{
    private static readonly string[] Hubs = new string[] { Constants.PathToChatHub, Constants.PathToNotificationHub };
    private readonly RequestDelegate next;

    public AuthorizationTokenMiddleware(RequestDelegate next)
    {
        this.next = next;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        if (httpContext is null)
        {
            throw new ArgumentNullException($"{nameof(httpContext)}");
        }

        var request = httpContext.Request;

        // web sockets cannot pass headers so we must take the access token from query param and
        // add it to the header before authentication middleware runs
        if (IsHubConnectionPath(request) && request.Query.TryGetValue("access_token", out var accessToken))
        {
            request.Headers.Add("Authorization", $"Bearer {accessToken}");
        }

        await next(httpContext).ConfigureAwait(false);
    }

    private static bool IsHubConnectionPath(HttpRequest request)
    {
        // We have an array, not list. Exists is not applicable
        #pragma warning disable S6605
        return Hubs.Any(h => request.Path.StartsWithSegments(h, StringComparison.OrdinalIgnoreCase));
        #pragma warning restore S6605
    }
}