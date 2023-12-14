using System.Net;
using OutOfSchool.Common.Responses;

namespace OutOfSchool.Common.Models;

public class ErrorResponse
{
    public HttpStatusCode HttpStatusCode { get; set; }

    public string Message { get; set; }

    public ApiErrorResponse ApiErrorResponse { get; set; }
}