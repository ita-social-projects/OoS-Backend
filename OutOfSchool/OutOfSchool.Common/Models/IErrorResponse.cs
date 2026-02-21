#nullable enable

using System.Net;

namespace OutOfSchool.Common.Models;

public interface IErrorResponse
{
    public HttpStatusCode HttpStatusCode { get; init; }

    public string? Message { get; init; }

    /// <summary>
    /// Gets the raw error content if required.
    /// </summary>
    public string? Content { get; init; }
}