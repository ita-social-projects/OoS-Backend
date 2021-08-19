using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace OutOfSchool.WebApi.Middlewares
{
    public class AuthorizationTokenMiddleware
    {
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
            if (request.Path.StartsWithSegments("/chathub", StringComparison.OrdinalIgnoreCase) &&
                request.Query.TryGetValue("access_token", out var accessToken))
            {
                request.Headers.Add("Authorization", $"Bearer {accessToken}");
            }

            await next(httpContext).ConfigureAwait(false);
        }
    }
}
