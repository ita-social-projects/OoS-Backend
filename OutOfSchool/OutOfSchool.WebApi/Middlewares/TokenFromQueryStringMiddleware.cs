using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace OutOfSchool.WebApi.Middlewares
{
    public class TokenFromQueryStringMiddleware
    {
        private readonly RequestDelegate next;

        public TokenFromQueryStringMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            var request = httpContext.Request;

            // web sockets cannot pass headers so we must take the access token from query param and
            // add it to the header before authentication middleware runs

            // TODO Edit Origin comparation (hard-code) in TokenFromQueryStringMiddleware
            // if chat is from our origin and another origin is blocked then delete origin's comparation
            // else: implement extract allowed Origins from appsettings.json
            if (request.Path.StartsWithSegments("/chathub", StringComparison.OrdinalIgnoreCase) &&
                request.Query.TryGetValue("access_token", out var accessToken) &&
                (string.Equals(request.Headers["Origin"], "http://localhost:4200", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(request.Headers["Origin"], "http://oos.dmytrominochkin.cloud:80", StringComparison.OrdinalIgnoreCase)))
            {
                request.Headers.Add("Authorization", $"Bearer {accessToken}");
            }

            await next(httpContext).ConfigureAwait(false);
        }
    }
}
