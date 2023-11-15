using System.Text.Json;
using Microsoft.Extensions.Options;

namespace OutOfSchool.WebApi.Extensions;

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
        catch (ArgumentNullException ex)
        {
            logger.LogError(ex, "Request data is null");
            var messageForUser = "Request data is empty. Please check your input data and try again.";
            await HandleExceptionAsync(context, messageForUser, StatusCodes.Status400BadRequest).ConfigureAwait(false);
        }
        catch (ArgumentException ex)
        {
            logger.LogError($"Exception information: {ex}");

            var messageForUser = "Validation error. Please check your input data and try again. If you are sure of input data please contact support.";

            await HandleExceptionAsync(context, messageForUser, StatusCodes.Status400BadRequest).ConfigureAwait(false);
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogError($"Exception information: {ex}");

            var messageForUser = "Sorry, you have no rights to do this(or get, or change some properties). Check your input data and try again.";

            await HandleExceptionAsync(context, messageForUser, StatusCodes.Status403Forbidden).ConfigureAwait(false);
        }
        catch (DbUpdateException ex)
        {
            logger.LogError($"Exception information: {ex}");

            var messageForUser = "Server error, your data was not saved. Please try again later or contact support.";

            await HandleExceptionAsync(context, messageForUser, StatusCodes.Status500InternalServerError).ConfigureAwait(false);
        }
        catch (MySqlException ex)
        {
            logger.LogError($"Exception information: {ex}");

            var messageForUser = "Server error, invalid query. Please check your input data or contact support.";

            await HandleExceptionAsync(context, messageForUser, StatusCodes.Status500InternalServerError).ConfigureAwait(false);
        }
        catch (OptionsValidationException ex)
        {
            logger.LogError($"Exception information: {ex}");

            var messageForUser = "Server error, options validation error. Please contact support.";

            await HandleExceptionAsync(context, messageForUser, StatusCodes.Status500InternalServerError).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogError($"Exception information: {ex}");

            var messageForUser = "Internal Server Error. Please try again later or contact support.";

            await HandleExceptionAsync(context, messageForUser, StatusCodes.Status500InternalServerError).ConfigureAwait(false);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, string messageForUser, int statusCode)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        return context.Response.WriteAsync(JsonSerializer.Serialize(new
        {
            Message = messageForUser,
        }));
    }
}