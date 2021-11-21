using System;
using System.Runtime.Serialization;

namespace OutOfSchool.Services.Common.Exceptions
{
    [Serializable]
    public class PictureNotFoundException : Exception
    {
        public PictureNotFoundException()
        {
        }

        public PictureNotFoundException(string message)
            : base(message)
        {
        }

        public PictureNotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected PictureNotFoundException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}