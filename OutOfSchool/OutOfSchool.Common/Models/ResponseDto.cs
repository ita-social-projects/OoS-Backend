using System.Net;

namespace OutOfSchool.Common
{
    public class ResponseDto
    {
        public object Result { get; set; }

        public string Message { get; set; }

        public HttpStatusCode HttpStatusCode { get; set; }

        public bool IsSuccess { get; set; }
    }
}