using System;

namespace OutOfSchool.WebApi.Services.Communication;

public class Request
{
    public Guid RequestId { get; set; }

    public object? Data { get; set; }

    public Uri Url { get; set; }

    public Dictionary<string, string>? Query { get; set; }

    public string Token { get; set; }

    public HttpMethodType HttpMethodType { get; set; }
}