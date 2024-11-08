using OutOfSchool.Common.Models;

namespace OutOfSchool.AuthCommon.Models;

public class ExternalAuthError : IErrorResponse
{
    public HttpStatusCode HttpStatusCode { get; init; }

    public string? Message { get; init; }

    public string? Content { get; init; }

    public ExternalAuthErrorGroup ErrorGroup { get; set; }
}