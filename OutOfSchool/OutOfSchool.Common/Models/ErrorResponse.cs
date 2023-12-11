using OutOfSchool.Common.Responces;
using System.Net;

namespace OutOfSchool.Common.Models;

public class ErrorResponse
{
    public HttpStatusCode HttpStatusCode { get; set; }

    public string Message { get; set; }

    public ApiErrorResponse ApiErrorResponse { get; set; }
}