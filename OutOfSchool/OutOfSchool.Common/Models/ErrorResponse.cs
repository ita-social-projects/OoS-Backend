using System.Net;
using OutOfSchool.Common.Responses;

namespace OutOfSchool.Common.Models;

public class ErrorResponse : IErrorResponse
{
    public HttpStatusCode HttpStatusCode { get; set; }

    public string Message { get; set; }

    public string Content { get; set; }

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