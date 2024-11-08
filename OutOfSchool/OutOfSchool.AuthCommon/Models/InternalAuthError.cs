using OutOfSchool.Common.Models;

namespace OutOfSchool.AuthCommon.Models;

public class InternalAuthError : IErrorResponse
{
    public HttpStatusCode HttpStatusCode { get; init; }

    public string? Message { get; init; }

    public string? Content { get; init; }

    public InternalAuthErrorGroup ErrorGroup { get; set; }
}