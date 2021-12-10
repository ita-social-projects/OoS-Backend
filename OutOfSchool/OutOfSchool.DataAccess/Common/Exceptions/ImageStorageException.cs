using System;
using System.Runtime.Serialization;

namespace OutOfSchool.Services.Common.Exceptions
{
    /// <summary>
    /// The ImageStorageException is thrown when something has happened while trying
    /// to work with image storage.
    /// </summary>
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