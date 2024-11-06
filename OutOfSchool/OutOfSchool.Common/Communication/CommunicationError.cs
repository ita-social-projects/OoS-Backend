#nullable enable

using System.Net;

namespace OutOfSchool.Common.Communication;

public class CommunicationError(HttpStatusCode? httpStatusCode = null)
{
    public HttpStatusCode HttpStatusCode { get; set; } = httpStatusCode ?? HttpStatusCode.InternalServerError;

    public string? Body { get; set; }
}