using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OutOfSchool.WebApi.Models;

namespace OutOfSchool.WebApi.Extensions
{
    public class ExceptionMiddlewareExtension
    {
        private readonly RequestDelegate next;
        private readonly ILogger<ExceptionMiddlewareExtension> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionMiddlewareExtensions"/> class.
        /// </summary>
        public ExceptionMiddlewareExtension(RequestDelegate next, ILogger<ExceptionMiddlewareExtension> logger)
        {
            this.next = next;
            this.logger = logger;
        }

        /// <summary>
        /// 
        /// </summary>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                logger.LogError($"Something went wrong: {ex}");
                await HandleExceptionAsync(context, ex).ConfigureAwait(false);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = 500;

            return context.Response.WriteAsync(JsonSerializer.Serialize(new ErrorDetails()
            {
                StatusCode = context.Response.StatusCode,
                Message = "Internal Server Error. " + exception.Message,
            }));
        }
    }

    internal class ErrorDetails
    {
        public ErrorDetails()
        {
        }

        public int StatusCode { get; set; }
        public string Message { get; set; }
    }
}