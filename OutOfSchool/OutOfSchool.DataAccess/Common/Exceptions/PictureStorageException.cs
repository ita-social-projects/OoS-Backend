using System;
using System.Runtime.Serialization;

namespace OutOfSchool.Services.Common.Exceptions
{
    [Serializable]
    public class PictureStorageException : Exception
    {
        private Exception ex;

        public PictureStorageException()
        {
        }

        public PictureStorageException(Exception ex)
        {
            this.ex = ex;
        }

        public PictureStorageException(string message)
            : base(message)
        {
        }

        public PictureStorageException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected PictureStorageException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}