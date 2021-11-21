using System;
using System.Runtime.Serialization;

namespace OutOfSchool.Services.Common.Exceptions
{
    [Serializable]
    public class ImageNotFoundException : Exception
    {
        public ImageNotFoundException()
        {
        }

        public ImageNotFoundException(string message)
            : base(message)
        {
        }

        public ImageNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected ImageNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}