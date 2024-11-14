using System.Net;
using OutOfSchool.Common.Responses;

namespace OutOfSchool.Common.Models;

public class ErrorResponse : IErrorResponse
{
    public HttpStatusCode HttpStatusCode { get; init; }

    public string Message { get; init; }

    public string Content { get; init; }

    public ApiErrorResponse ApiErrorResponse { get; set; }

    public static ErrorResponse BadRequest(ApiErrorResponse apiErrorResponse)
    {
        return new ErrorResponse
        {
            HttpStatusCode = HttpStatusCode.BadRequest,
            ApiErrorResponse = apiErrorResponse,
        };
    }
}