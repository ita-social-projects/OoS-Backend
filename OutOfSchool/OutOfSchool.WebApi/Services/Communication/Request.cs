using System;

namespace OutOfSchool.WebApi.Services.Communication
{
    public class Request
    {
        public Guid RequestId { get; set; }

        public object Data { get; set; }

        public System.Uri Url { get; set; }

        public string Token { get; set; }

        public HttpMethodType HttpMethodType { get; set; }
    }
}