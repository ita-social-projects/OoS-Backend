using System;
using System.Collections.Generic;

namespace OutOfSchool.Common.Communication;

public class Request
{
    public object Data { get; set; }

    public Uri Url { get; set; }

    public Dictionary<string, string> Query { get; set; }
    
    // Headers can occur multiple times and order matters
    public List<KeyValuePair<string, string>> Headers { get; set; }

    public string Token { get; set; }

    public HttpMethodType HttpMethodType { get; set; }
}