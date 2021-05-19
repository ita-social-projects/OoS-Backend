﻿using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace OutOfSchool.WebApi.Extensions
{
    public class ExceptionMiddlewareExtension
    {
        private readonly RequestDelegate next;
        private readonly ILogger<ExceptionMiddlewareExtension> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionMiddlewareExtension"/> class.
        /// </summary>
        /// <param name="next">Next delegate.</param>
        /// <param name="logger">Logger.</param>
        public ExceptionMiddlewareExtension(RequestDelegate next, ILogger<ExceptionMiddlewareExtension> logger)
        {
            this.next = next;
            this.logger = logger;
        }

        /// <summary>
        /// Exception Handler.
        /// </summary>
        /// <param name="context">HttpContext.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger.LogError($"Something went wrong: {ex.Message}");
                await HandleExceptionAsync(context, ex).ConfigureAwait(false);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = 500;

            return context.Response.WriteAsync(JsonSerializer.Serialize(new 
            {
                Message = "Internal Server Error. " + exception.Message,
            }));
        }
    } 
}