using System.Net;

namespace OutOfSchool.Common.Models;

public interface IErrorResponse
{
    public HttpStatusCode HttpStatusCode { get; set; }

    public string Message { get; set; }

    /// <summary>
    /// Gets or sets the raw error content if required.
    /// </summary>
    public string Content { get; set; }
}