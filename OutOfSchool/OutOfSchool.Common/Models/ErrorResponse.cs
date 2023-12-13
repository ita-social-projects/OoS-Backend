using System.Net;
using OutOfSchool.Common.Responces;

namespace OutOfSchool.Common.Models;

public class ErrorResponse
{
    public HttpStatusCode HttpStatusCode { get; set; }

    public string Message { get; set; }

    public ApiErrorResponse ApiErrorResponse { get; set; }
}