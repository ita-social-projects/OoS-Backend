using System;
using System.Runtime.Serialization;

namespace OutOfSchool.Services.Common.Exceptions
{
    [Serializable]
    public class ImageStorageException : Exception
    {

        public ImageStorageException()
        {
        }

        public ImageStorageException(Exception ex)
            : this("Unhandled exception", ex)
        {
        }

        public ImageStorageException(string message)
            : base(message)
        {
        }

        public ImageStorageException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected ImageStorageException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}