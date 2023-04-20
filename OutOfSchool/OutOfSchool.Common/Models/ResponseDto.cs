using System.Net;
using OutOfSchool.Common.Models;

namespace OutOfSchool.Common;

public class ResponseDto : IResponse
{
    public object Result { get; set; }

    public string Message { get; set; }

    public HttpStatusCode HttpStatusCode { get; set; }

    public bool IsSuccess { get; set; }
}